"""Build M38-02 grade profile repair candidates for recipe_003."""

from __future__ import annotations

import argparse
import copy
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
M37_02_REPAIR = OUTPUT_DIR / "m37_02_trigger_package_repair_proposal.json"
M37_05_RERUN = OUTPUT_DIR / "m37_05_revised_recipe_validation_rerun.json"
M38_01_REVIEW = OUTPUT_DIR / "m38_01_accepted_seed_human_review_packet.json"
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
CLASSIC_GRADE_TARGET = {"0": 17, "1": 14, "2": 11, "3": 8}


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _find_recipe(recipe_report: dict[str, Any], recipe_id: str) -> dict[str, Any]:
    for recipe in recipe_report.get("recipe_drafts", []):
        if recipe["recipe_id"] == recipe_id:
            return recipe
    raise ValueError(f"Recipe not found: {recipe_id}")


def _apply_delta_quantities(recipe: dict[str, Any], delta: Sequence[dict[str, Any]]) -> dict[str, int]:
    quantities = {row["card_id"]: int(row.get("quantity", 0)) for row in recipe.get("quantities", [])}
    for item in delta:
        quantities[item["card_id"]] = quantities.get(item["card_id"], 0) + int(item["quantity"])
    return quantities


def _load_cards(card_ids: Sequence[str]) -> dict[str, dict[str, Any]]:
    if not card_ids:
        return {}
    placeholders = ",".join("?" for _ in card_ids)
    query = (
        "select card_id, name_th, series_code, clan, grade, trigger, deck_limit, "
        "type_1, shield, power from cards where card_id in "
        f"({placeholders})"
    )
    with closing(sqlite3.connect(CARDS_SQLITE)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, list(card_ids)).fetchall()
    return {row["card_id"]: dict(row) for row in rows}


def _load_grade_pool(clan: str, grade: int) -> list[dict[str, Any]]:
    placeholders = ",".join("?" for _ in CLASSIC_PART1_SERIES)
    query = (
        "select card_id, name_th, series_code, clan, grade, trigger, deck_limit, "
        "type_1, shield, power from cards where clan = ? and grade = ? "
        f"and series_code in ({placeholders}) order by series_code, card_id"
    )
    with closing(sqlite3.connect(CARDS_SQLITE)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, [clan, grade, *CLASSIC_PART1_SERIES]).fetchall()
    return [dict(row) for row in rows]


def _grade_counts(quantities: dict[str, int], cards: dict[str, dict[str, Any]]) -> Counter[str]:
    counts: Counter[str] = Counter()
    for card_id, quantity in quantities.items():
        card = cards[card_id]
        counts[str(card["grade"])] += quantity
    return counts


def _sort_pool(pool: Sequence[dict[str, Any]], prefer_series: Sequence[str]) -> list[dict[str, Any]]:
    preferred = {series: index for index, series in enumerate(prefer_series)}
    return sorted(
        pool,
        key=lambda row: (
            0 if not row.get("trigger") else 1,
            preferred.get(row["series_code"], 999),
            -int(row.get("shield") or 0),
            -int(row.get("power") or 0),
            SERIES_ORDER.get(row["series_code"], 999),
            row["card_id"],
        ),
    )


def _pick_additions(
    clan: str,
    grade: str,
    needed: int,
    quantities: dict[str, int],
    prefer_series: Sequence[str],
) -> list[dict[str, Any]]:
    additions: list[dict[str, Any]] = []
    remaining = needed
    pool = _sort_pool(_load_grade_pool(clan, int(grade)), prefer_series)
    for card in pool:
        if remaining <= 0:
            break
        current = quantities.get(card["card_id"], 0)
        deck_limit = int(card.get("deck_limit") or 4)
        available = max(0, deck_limit - current)
        if available <= 0:
            continue
        qty = min(available, remaining)
        additions.append(
            {
                "card_id": card["card_id"],
                "name_th": card.get("name_th", ""),
                "quantity": qty,
                "grade": str(card["grade"]),
                "series_code": card["series_code"],
                "current_quantity": current,
                "final_quantity_if_chosen": current + qty,
                "deck_limit": deck_limit,
                "source": "data/packs/vanguard_th/cards.sqlite:cards",
            }
        )
        remaining -= qty
    return additions


def _pick_removals(
    surplus_grade: str,
    remove_count: int,
    quantities: dict[str, int],
    cards: dict[str, dict[str, Any]],
) -> list[dict[str, Any]]:
    rows = [
        {
            "card_id": card_id,
            "name_th": cards[card_id].get("name_th", ""),
            "quantity": quantity,
            "grade": str(cards[card_id]["grade"]),
            "series_code": cards[card_id].get("series_code", ""),
            "source": "m36_02_deck_recipe_draft_model",
        }
        for card_id, quantity in quantities.items()
        if str(cards[card_id]["grade"]) == surplus_grade and quantity > 0
    ]
    rows.sort(key=lambda row: (-row["quantity"], SERIES_ORDER.get(row["series_code"], 999), row["card_id"]))
    removals: list[dict[str, Any]] = []
    remaining = remove_count
    for row in rows:
        if remaining <= 0:
            break
        qty = min(row["quantity"], remaining)
        removals.append({**row, "quantity": qty})
        remaining -= qty
    return removals


def _build_candidate(
    package_id: str,
    title: str,
    clan: str,
    current_quantities: dict[str, int],
    current_cards: dict[str, dict[str, Any]],
    current_counts: dict[str, int],
    prefer_series: Sequence[str],
) -> dict[str, Any]:
    deficits = {
        grade: max(0, target - int(current_counts.get(grade, 0)))
        for grade, target in CLASSIC_GRADE_TARGET.items()
    }
    surpluses = {
        grade: max(0, int(current_counts.get(grade, 0)) - target)
        for grade, target in CLASSIC_GRADE_TARGET.items()
    }
    additions: list[dict[str, Any]] = []
    for grade, needed in deficits.items():
        if needed:
            additions.extend(_pick_additions(clan, grade, needed, current_quantities, prefer_series))
    removals: list[dict[str, Any]] = []
    for grade, count in surpluses.items():
        if count:
            removals.extend(_pick_removals(grade, count, current_quantities, current_cards))

    after = dict(current_counts)
    for item in additions:
        after[item["grade"]] = after.get(item["grade"], 0) + int(item["quantity"])
    for item in removals:
        after[item["grade"]] = after.get(item["grade"], 0) - int(item["quantity"])
    after = {grade: after.get(grade, 0) for grade in sorted(CLASSIC_GRADE_TARGET)}
    complete = after == CLASSIC_GRADE_TARGET and sum(item["quantity"] for item in additions) == sum(
        item["quantity"] for item in removals
    )
    return {
        "package_id": package_id,
        "title": title,
        "advisory_only": True,
        "target_grade_counts": CLASSIC_GRADE_TARGET,
        "current_grade_counts": current_counts,
        "additions": additions,
        "removals": removals,
        "grade_counts_after": after,
        "added_card_count": sum(item["quantity"] for item in additions),
        "removed_card_count": sum(item["quantity"] for item in removals),
        "net_card_count_change": sum(item["quantity"] for item in additions) - sum(
            item["quantity"] for item in removals
        ),
        "completion_status": "complete_candidate" if complete else "incomplete_candidate",
        "runtime_promotion_allowed": False,
    }


def build_grade_profile_candidates(
    recipe_report: dict[str, Any] | None = None,
    repair_report: dict[str, Any] | None = None,
    rerun_report: dict[str, Any] | None = None,
    review_packet: dict[str, Any] | None = None,
) -> dict[str, Any]:
    recipe_report = recipe_report or load_json(RECIPE_DRAFTS)
    repair_report = repair_report or load_json(M37_02_REPAIR)
    rerun_report = rerun_report or load_json(M37_05_RERUN)
    review_packet = review_packet or load_json(M38_01_REVIEW)

    recipe_id = rerun_report["summary"]["recipe_id"]
    recipe = _find_recipe(recipe_report, recipe_id)
    current_quantities = _apply_delta_quantities(recipe, repair_report["recommended_repair"]["quantity_delta"])
    current_cards = _load_cards(list(current_quantities))
    clan = current_cards[recipe["anchor_card_id"]]["clan"]
    current_counts = dict(_grade_counts(current_quantities, current_cards))
    deficits = {
        grade: max(0, target - int(current_counts.get(grade, 0)))
        for grade, target in CLASSIC_GRADE_TARGET.items()
    }
    surpluses = {
        grade: max(0, int(current_counts.get(grade, 0)) - target)
        for grade, target in CLASSIC_GRADE_TARGET.items()
    }
    candidates = [
        _build_candidate(
            "m38_02_grade_pkg_001",
            "Classic target profile repair",
            clan,
            current_quantities,
            current_cards,
            current_counts,
            (),
        ),
        _build_candidate(
            "m38_02_grade_pkg_002",
            "EB04-local target profile repair",
            clan,
            current_quantities,
            current_cards,
            current_counts,
            ("EB04", "BT04", "TD03"),
        ),
    ]
    complete = [item for item in candidates if item["completion_status"] == "complete_candidate"]
    return {
        "version": "M38-02",
        "description": "Grade profile repair candidates for accepted seed recipe",
        "selected_target": recipe_report.get("selected_target", {}),
        "source_inputs": {
            "deck_recipe_draft_model": str(RECIPE_DRAFTS.relative_to(ROOT)),
            "trigger_package_repair_proposal": str(M37_02_REPAIR.relative_to(ROOT)),
            "revised_recipe_validation_rerun": str(M37_05_RERUN.relative_to(ROOT)),
            "accepted_seed_human_review_packet": str(M38_01_REVIEW.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "scope": {
            "offline_grade_profile_candidates": True,
            "changes_recipe_draft_file": False,
            "creates_runtime_deck": False,
            "records_human_acceptance": False,
            "bot_integration": False,
            "live_card_text_parsing": False,
        },
        "grade_profile_policy": {
            "target_grade_counts": CLASSIC_GRADE_TARGET,
            "source_backed_candidates": True,
            "substitution_only": True,
            "runtime_promotion_allowed": False,
        },
        "current_profile": {
            "recipe_id": recipe_id,
            "grade_counts": current_counts,
            "deficits": deficits,
            "surpluses": surpluses,
            "review_codes": rerun_report["accepted_seed_after"]["review_codes"],
            "human_review_packet_ready": review_packet["summary"]["ready_for_m38_02"],
        },
        "repair_candidates": candidates,
        "summary": {
            "recipe_id": recipe_id,
            "repair_candidate_count": len(candidates),
            "complete_candidate_count": len(complete),
            "grade_deficit_total": sum(deficits.values()),
            "grade_surplus_total": sum(surpluses.values()),
            "runtime_promotion_allowed": False,
            "ready_for_m38_03": bool(complete),
        },
        "next_target": {
            "milestone": "M38-03",
            "task": "Human-accepted recipe artifact",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    profile = report["current_profile"]
    lines = [
        "# M38-02 Grade Profile Repair Candidates",
        "",
        "## Summary",
        "",
        f"- Recipe: `{summary['recipe_id']}`",
        f"- Repair candidates: `{summary['repair_candidate_count']}`",
        f"- Complete candidates: `{summary['complete_candidate_count']}`",
        f"- Grade deficit total: `{summary['grade_deficit_total']}`",
        f"- Grade surplus total: `{summary['grade_surplus_total']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M38-03: `{summary['ready_for_m38_03']}`",
        "",
        "## Current Profile",
        "",
        f"- Current: `{profile['grade_counts']}`",
        f"- Deficits: `{profile['deficits']}`",
        f"- Surpluses: `{profile['surpluses']}`",
        "",
        "## Candidate Packages",
        "",
    ]
    for candidate in report["repair_candidates"]:
        lines.append(
            f"- `{candidate['package_id']}` status=`{candidate['completion_status']}` "
            f"add=`{candidate['added_card_count']}` remove=`{candidate['removed_card_count']}` "
            f"after=`{candidate['grade_counts_after']}`"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Candidates are substitution previews only.",
            "- Source recipe draft is not modified.",
            "- Runtime promotion remains disabled.",
            "",
            "## Next",
            "",
            "`M38-03`: Human-accepted recipe artifact.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: list[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M38-02 grade profile repair candidates.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_grade_profile_candidates()
    json_path = args.output_dir / "m38_02_grade_profile_repair_candidates.json"
    md_path = args.output_dir / "m38_02_grade_profile_repair_candidates.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M38-02 grade profile repair candidates wrote {json_path}")
    print(f"M38-02 grade profile repair candidates summary wrote {md_path}")
    print(
        "ready_for_m38_03={ready} candidates={candidates} complete={complete}".format(
            ready=report["summary"]["ready_for_m38_03"],
            candidates=report["summary"]["repair_candidate_count"],
            complete=report["summary"]["complete_candidate_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m38_03"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
