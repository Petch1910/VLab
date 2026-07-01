"""Build M37-01 accepted-seed slot-gap completion candidates.

The report proposes source-backed trigger packages for the one M36 accepted
seed recipe. It is advisory only: no recipe is changed, no runtime deck is
created, and no bot/playbook path is promoted.
"""

from __future__ import annotations

import argparse
import json
import sqlite3
import sys
from collections import Counter
from contextlib import closing
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
RECIPE_DRAFTS = OUTPUT_DIR / "m36_02_deck_recipe_draft_model.json"
VALIDATION_REPORT = OUTPUT_DIR / "m36_03_deck_recipe_validation_report.json"
M36_CLOSEOUT = OUTPUT_DIR / "m36_closeout_deck_recipe_validation.json"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"

CLASSIC_PART1_SERIES = (
    "TD01",
    "TD02",
    "TD03",
    "TD04",
    "TD05",
    "TD06",
    "BT01",
    "BT02",
    "BT03",
    "BT04",
    "BT05",
    "BT06",
    "BT07",
    "BT08",
    "BT09",
    "EB01",
    "EB02",
    "EB03",
    "EB04",
    "EB05",
)

SERIES_ORDER = {code: index for index, code in enumerate(CLASSIC_PART1_SERIES)}


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _get(data: dict[str, Any], path: Sequence[str], default: Any = None) -> Any:
    cursor: Any = data
    for key in path:
        if not isinstance(cursor, dict) or key not in cursor:
            return default
        cursor = cursor[key]
    return cursor


def _accepted_seed_recipe(recipe_report: dict[str, Any]) -> dict[str, Any]:
    recipes = [
        recipe
        for recipe in recipe_report.get("recipe_drafts", [])
        if recipe.get("review_status", "").startswith("accepted_seed")
    ]
    if len(recipes) != 1:
        raise ValueError(f"Expected exactly one accepted seed recipe, found {len(recipes)}")
    return recipes[0]


def _validation_for_recipe(validation_report: dict[str, Any], recipe_id: str) -> dict[str, Any]:
    for validation in validation_report.get("recipe_validations", []):
        if validation.get("recipe_id") == recipe_id:
            return validation
    raise ValueError(f"Validation not found for recipe: {recipe_id}")


def _current_quantities(recipe: dict[str, Any]) -> dict[str, int]:
    return {row["card_id"]: int(row.get("quantity", 0)) for row in recipe.get("quantities", [])}


def _load_cards(card_ids: Sequence[str], sqlite_path: Path = CARDS_SQLITE) -> dict[str, dict[str, Any]]:
    if not card_ids:
        return {}
    placeholders = ",".join("?" for _ in card_ids)
    query = (
        "select card_id, name_th, series, series_code, clan, grade, trigger, "
        "deck_limit, type_1, shield, power from cards where card_id in "
        f"({placeholders})"
    )
    with closing(sqlite3.connect(sqlite_path)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, list(card_ids)).fetchall()
    return {row["card_id"]: dict(row) for row in rows}


def _load_trigger_pool(clan: str, sqlite_path: Path = CARDS_SQLITE) -> list[dict[str, Any]]:
    placeholders = ",".join("?" for _ in CLASSIC_PART1_SERIES)
    query = (
        "select card_id, name_th, series, series_code, clan, grade, trigger, "
        "deck_limit, type_1, shield, power from cards "
        "where clan = ? and trigger is not null and trigger <> '' "
        f"and series_code in ({placeholders}) "
        "order by series_code, trigger, card_id"
    )
    with closing(sqlite3.connect(sqlite_path)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, [clan, *CLASSIC_PART1_SERIES]).fetchall()
    return [dict(row) for row in rows]


def _candidate_rows(trigger_pool: list[dict[str, Any]], current_quantities: dict[str, int]) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    for card in trigger_pool:
        current = current_quantities.get(card["card_id"], 0)
        deck_limit = int(card.get("deck_limit") or 4)
        available = max(0, deck_limit - current)
        if available <= 0:
            continue
        rows.append(
            {
                "card_id": card["card_id"],
                "name_th": card.get("name_th", ""),
                "series_code": card.get("series_code", ""),
                "clan": card.get("clan", ""),
                "grade": card.get("grade"),
                "trigger": card.get("trigger", ""),
                "deck_limit": deck_limit,
                "current_quantity": current,
                "available_quantity": available,
                "shield": card.get("shield"),
                "power": card.get("power"),
                "type_1": card.get("type_1", ""),
                "source": "data/packs/vanguard_th/cards.sqlite:cards",
            }
        )
    return rows


def _sort_candidates(candidates: Sequence[dict[str, Any]], prefer_series: Sequence[str]) -> list[dict[str, Any]]:
    preferred = {series: index for index, series in enumerate(prefer_series)}
    return sorted(
        candidates,
        key=lambda item: (
            preferred.get(item["series_code"], 999),
            -int(item.get("shield") or 0),
            -int(item.get("power") or 0),
            SERIES_ORDER.get(item["series_code"], 999),
            item["card_id"],
        ),
    )


def _pick_trigger_cards(
    candidates: Sequence[dict[str, Any]],
    trigger: str,
    requested: int,
    prefer_series: Sequence[str],
) -> tuple[list[dict[str, Any]], int]:
    picked: list[dict[str, Any]] = []
    remaining = requested
    trigger_candidates = [candidate for candidate in candidates if candidate["trigger"] == trigger]
    for candidate in _sort_candidates(trigger_candidates, prefer_series):
        if remaining <= 0:
            break
        quantity = min(int(candidate["available_quantity"]), remaining)
        if quantity <= 0:
            continue
        picked.append(
            {
                "card_id": candidate["card_id"],
                "name_th": candidate["name_th"],
                "quantity": quantity,
                "trigger": candidate["trigger"],
                "series_code": candidate["series_code"],
                "deck_limit": candidate["deck_limit"],
                "current_quantity": candidate["current_quantity"],
                "final_quantity_if_chosen": candidate["current_quantity"] + quantity,
                "source": candidate["source"],
            }
        )
        remaining -= quantity
    return picked, remaining


def _package_score(package: dict[str, Any]) -> int:
    score = 0
    if package["completion_status"] == "complete_candidate":
        score += 100
    final_counts = package["final_trigger_counts"]
    if final_counts.get("Heal", 0) == 4:
        score += 15
    if final_counts.get("Critical", 0) >= 4:
        score += 10
    if final_counts.get("Draw", 0) >= 4:
        score += 5
    if final_counts.get("Stand", 0) > 8:
        score -= 5
    if package["profile_id"] == "balanced_classic":
        score += 5
    return score


def _build_package(
    index: int,
    profile_id: str,
    title: str,
    target_additions: dict[str, int],
    candidates: Sequence[dict[str, Any]],
    current_counts: Counter[str],
    current_quantities: dict[str, int],
    trigger_slots_unfilled: int,
    prefer_series: Sequence[str] = (),
) -> dict[str, Any]:
    additions: list[dict[str, Any]] = []
    missing_by_trigger: dict[str, int] = {}
    for trigger, requested in target_additions.items():
        picked, missing = _pick_trigger_cards(candidates, trigger, requested, prefer_series)
        additions.extend(picked)
        if missing:
            missing_by_trigger[trigger] = missing

    added_count = sum(item["quantity"] for item in additions)
    final_counts = Counter(current_counts)
    for item in additions:
        final_counts[item["trigger"]] += int(item["quantity"])

    final_quantities = dict(current_quantities)
    for item in additions:
        final_quantities[item["card_id"]] = final_quantities.get(item["card_id"], 0) + int(item["quantity"])

    copy_violations = [
        {
            "card_id": item["card_id"],
            "final_quantity": final_quantities[item["card_id"]],
            "deck_limit": item["deck_limit"],
        }
        for item in additions
        if final_quantities[item["card_id"]] > int(item["deck_limit"])
    ]
    heal_limit_ok = final_counts.get("Heal", 0) <= 4
    trigger_total = sum(final_counts.values())
    complete = (
        added_count == trigger_slots_unfilled
        and trigger_total == 16
        and not missing_by_trigger
        and not copy_violations
        and heal_limit_ok
    )
    package = {
        "package_id": f"m37_01_pkg_{index:03d}",
        "profile_id": profile_id,
        "title": title,
        "advisory_only": True,
        "target_additions": target_additions,
        "additions": additions,
        "added_trigger_count": added_count,
        "missing_by_trigger": missing_by_trigger,
        "final_trigger_counts": dict(sorted(final_counts.items())),
        "copy_limit_violations": copy_violations,
        "heal_limit_ok": heal_limit_ok,
        "completion_status": "complete_candidate" if complete else "incomplete_candidate",
        "runtime_promotion_allowed": False,
    }
    package["score"] = _package_score(package)
    return package


def _current_trigger_counts(recipe: dict[str, Any], card_rows: dict[str, dict[str, Any]]) -> Counter[str]:
    counts: Counter[str] = Counter()
    for row in recipe.get("quantities", []):
        trigger = card_rows.get(row["card_id"], {}).get("trigger") or ""
        if trigger:
            counts[trigger] += int(row.get("quantity", 0))
    return counts


def build_slot_gap_candidates(
    recipe_report: dict[str, Any] | None = None,
    validation_report: dict[str, Any] | None = None,
    closeout_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    recipe_report = recipe_report or load_json(RECIPE_DRAFTS)
    validation_report = validation_report or load_json(VALIDATION_REPORT)
    closeout_report = closeout_report or load_json(M36_CLOSEOUT)

    recipe = _accepted_seed_recipe(recipe_report)
    validation = _validation_for_recipe(validation_report, recipe["recipe_id"])
    current_quantities = _current_quantities(recipe)
    card_rows = _load_cards(list(current_quantities))
    anchor_card = card_rows[recipe["anchor_card_id"]]
    clan = anchor_card["clan"]
    current_counts = _current_trigger_counts(recipe, card_rows)
    trigger_pool = _load_trigger_pool(clan)
    candidates = _candidate_rows(trigger_pool, current_quantities)
    slot_summary = recipe["slot_summary"]
    trigger_slots_unfilled = int(slot_summary.get("trigger_slots_unfilled", 0))

    profiles = [
        (
            "balanced_classic",
            "Balanced 4 Critical / 4 Draw / 4 Heal completion",
            {"Critical": 4, "Draw": 4, "Heal": 4},
            (),
        ),
        (
            "eb04_local_balanced",
            "EB04-local balanced completion",
            {"Critical": 4, "Draw": 4, "Heal": 4},
            ("EB04", "BT04", "TD03"),
        ),
        (
            "critical_pressure",
            "Critical pressure completion",
            {"Critical": 8, "Heal": 4},
            (),
        ),
        (
            "stand_pressure",
            "Stand pressure completion",
            {"Stand": 4, "Critical": 4, "Heal": 4},
            (),
        ),
        (
            "draw_stand_guarded",
            "Draw and Stand guarded completion",
            {"Draw": 4, "Stand": 4, "Heal": 4},
            (),
        ),
    ]
    packages = [
        _build_package(
            index=index,
            profile_id=profile_id,
            title=title,
            target_additions=target_additions,
            candidates=candidates,
            current_counts=current_counts,
            current_quantities=current_quantities,
            trigger_slots_unfilled=trigger_slots_unfilled,
            prefer_series=prefer_series,
        )
        for index, (profile_id, title, target_additions, prefer_series) in enumerate(profiles, start=1)
    ]
    packages.sort(key=lambda item: (-item["score"], item["package_id"]))
    complete_packages = [package for package in packages if package["completion_status"] == "complete_candidate"]

    return {
        "version": "M37-01",
        "description": "Accepted seed slot-gap completion candidates",
        "selected_target": recipe_report.get("selected_target", {}),
        "source_inputs": {
            "deck_recipe_draft_model": str(RECIPE_DRAFTS.relative_to(ROOT)),
            "deck_recipe_validation_report": str(VALIDATION_REPORT.relative_to(ROOT)),
            "m36_closeout": str(M36_CLOSEOUT.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "scope": {
            "offline_candidate_generation": True,
            "changes_recipe_draft": False,
            "creates_runtime_deck": False,
            "bot_integration": False,
            "automatic_deck_injection": False,
            "live_card_text_parsing": False,
        },
        "accepted_seed": {
            "recipe_id": recipe["recipe_id"],
            "source_skeleton_id": recipe["source_skeleton_id"],
            "source_line_id": recipe["source_line_id"],
            "anchor_card_id": recipe["anchor_card_id"],
            "anchor_name_th": recipe.get("anchor_name_th", ""),
            "validation_status": validation.get("validation_status", ""),
            "current_trigger_counts": dict(sorted(current_counts.items())),
            "slot_summary": slot_summary,
        },
        "candidate_policy": {
            "candidate_source": "SQLite cards table",
            "series_scope": list(CLASSIC_PART1_SERIES),
            "same_clan_required": True,
            "trigger_unit_required": True,
            "copy_limit_respected": True,
            "heal_limit_max": 4,
            "human_review_required": True,
            "runtime_promotion_allowed": False,
        },
        "candidate_cards": candidates,
        "completion_packages": packages,
        "blocker_context": {
            "m36_runtime_ready_recipe_count": _get(
                closeout_report, ["key_results", "validation", "runtime_ready_recipe_count"], 0
            ),
            "m36_promotable_combo_lines": _get(
                closeout_report, ["key_results", "consistency", "promotion_allowed_count"], 0
            ),
            "reason_for_m37": _get(closeout_report, ["decision", "recommendation"], ""),
        },
        "summary": {
            "accepted_seed_recipe_id": recipe["recipe_id"],
            "trigger_candidate_card_count": len(candidates),
            "completion_package_count": len(packages),
            "complete_package_count": len(complete_packages),
            "trigger_slots_unfilled": trigger_slots_unfilled,
            "normal_slots_unfilled": int(slot_summary.get("normal_slots_unfilled", 0)),
            "runtime_promotion_allowed": False,
            "ready_for_m37_02": bool(complete_packages),
        },
        "next_target": {
            "milestone": "M37-02",
            "task": "Trigger package repair proposal",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    seed = report["accepted_seed"]
    lines = [
        "# M37-01 Accepted Seed Slot-Gap Completion Candidates",
        "",
        "## Summary",
        "",
        f"- Accepted seed recipe: `{summary['accepted_seed_recipe_id']}`",
        f"- Source line: `{seed['source_line_id']}`",
        f"- Trigger slots unfilled: `{summary['trigger_slots_unfilled']}`",
        f"- Normal slots unfilled: `{summary['normal_slots_unfilled']}`",
        f"- Trigger candidate cards: `{summary['trigger_candidate_card_count']}`",
        f"- Completion packages: `{summary['completion_package_count']}`",
        f"- Complete packages: `{summary['complete_package_count']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M37-02: `{summary['ready_for_m37_02']}`",
        "",
        "## Current Trigger Counts",
        "",
    ]
    for trigger, count in seed["current_trigger_counts"].items():
        lines.append(f"- `{trigger}`: `{count}`")
    lines.extend(["", "## Package Candidates", ""])
    for package in report["completion_packages"]:
        additions = ", ".join(
            f"{item['quantity']}x {item['card_id']} ({item['trigger']})"
            for item in package["additions"]
        )
        lines.append(
            f"- `{package['package_id']}` `{package['profile_id']}` "
            f"status=`{package['completion_status']}` score=`{package['score']}` "
            f"final={package['final_trigger_counts']} additions={additions}"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Candidate packages are advisory only.",
            "- The recipe draft is not modified by this report.",
            "- Runtime deck promotion remains disabled.",
            "- Human review is still required before any recipe repair is accepted.",
            "",
            "## Next",
            "",
            "`M37-02`: Trigger package repair proposal.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M37-01 accepted seed slot-gap candidates.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_slot_gap_candidates()
    json_path = args.output_dir / "m37_01_accepted_seed_slot_gap_candidates.json"
    md_path = args.output_dir / "m37_01_accepted_seed_slot_gap_candidates.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M37-01 accepted seed slot-gap candidates wrote {json_path}")
    print(f"M37-01 accepted seed slot-gap candidates summary wrote {md_path}")
    print(
        "ready_for_m37_02={ready} candidates={candidates} complete_packages={packages}".format(
            ready=report["summary"]["ready_for_m37_02"],
            candidates=report["summary"]["trigger_candidate_card_count"],
            packages=report["summary"]["complete_package_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m37_02"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
