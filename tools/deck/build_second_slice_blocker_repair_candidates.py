"""Build M40-05 second-slice blocker repair candidates.

This creates advisory, source-backed repair candidates for blockers reported by
M40-03 and carried through M40-04. It does not mutate M40-02 drafts and does
not create saved decks, runtime fixtures, UI entries, or bot playbooks.
"""

from __future__ import annotations

import argparse
import json
import sqlite3
import sys
from collections import Counter
from contextlib import closing
from copy import deepcopy
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M40_RECIPE_DRAFTS = OUTPUT_DIR / "m40_02_second_slice_recipe_draft_model.json"
M40_VALIDATION = OUTPUT_DIR / "m40_03_second_slice_recipe_validation_report.json"
M40_CONSISTENCY = OUTPUT_DIR / "m40_04_second_slice_combo_recipe_consistency_report.json"
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


def _by_recipe_id(items: Sequence[dict[str, Any]]) -> dict[str, dict[str, Any]]:
    return {item.get("recipe_id", ""): item for item in items if item.get("recipe_id")}


def _quantity_map(recipe: dict[str, Any]) -> dict[str, int]:
    return {row["card_id"]: int(row.get("quantity", 0)) for row in recipe.get("quantities", [])}


def _all_recipe_card_ids(recipe_report: dict[str, Any]) -> list[str]:
    return sorted(
        {
            row["card_id"]
            for recipe in recipe_report.get("recipe_drafts", [])
            for row in recipe.get("quantities", [])
        }
    )


def _load_cards(card_ids: Sequence[str]) -> dict[str, dict[str, Any]]:
    ids = sorted(set(card_ids))
    if not ids:
        return {}
    placeholders = ",".join("?" for _ in ids)
    query = (
        "select card_id, name_th, series_code, clan, grade, trigger, deck_limit, type_1, shield, power "
        f"from cards where card_id in ({placeholders})"
    )
    with closing(sqlite3.connect(CARDS_SQLITE)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, ids).fetchall()
    return {row["card_id"]: dict(row) for row in rows}


def _load_candidate_pool(
    clan: str,
    grade: int,
    trigger: str,
    type_1: str,
    excluded_ids: set[str],
) -> list[dict[str, Any]]:
    placeholders = ",".join("?" for _ in CLASSIC_PART1_SERIES)
    query = (
        "select card_id, name_th, series_code, clan, grade, trigger, deck_limit, type_1, shield, power "
        "from cards where clan = ? and grade = ? and coalesce(trigger, '') = ? and type_1 = ? "
        f"and series_code in ({placeholders}) order by series_code, card_id"
    )
    with closing(sqlite3.connect(CARDS_SQLITE)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, [clan, grade, trigger, type_1, *CLASSIC_PART1_SERIES]).fetchall()
    return [dict(row) for row in rows if row["card_id"] not in excluded_ids]


def _sort_candidates(rows: Sequence[dict[str, Any]]) -> list[dict[str, Any]]:
    return sorted(
        rows,
        key=lambda row: (
            SERIES_ORDER.get(row.get("series_code", ""), 999),
            -int(row.get("shield") or 0),
            -int(row.get("power") or 0),
            row["card_id"],
        ),
    )


def _manual_ids(validation: dict[str, Any]) -> list[str]:
    for issue in validation.get("issues", []):
        if issue.get("code") == "manual_review_card_overlap":
            return sorted(issue.get("details", {}).get("card_ids", []))
    return []


def _grade_issue(validation: dict[str, Any]) -> dict[str, dict[str, int]]:
    for issue in validation.get("issues", []):
        if issue.get("code") == "grade_profile_review":
            return issue.get("details", {})
    return {}


def _manual_repair_options(
    manual_card_id: str,
    quantity: int,
    quantities: dict[str, int],
    cards: dict[str, dict[str, Any]],
    manual_ids: set[str],
) -> list[dict[str, Any]]:
    manual_card = cards[manual_card_id]
    pool = _sort_candidates(
        _load_candidate_pool(
            manual_card["clan"],
            int(manual_card["grade"]),
            manual_card.get("trigger") or "",
            manual_card["type_1"],
            manual_ids,
        )
    )
    options: list[dict[str, Any]] = []
    for row in pool:
        current = quantities.get(row["card_id"], 0)
        deck_limit = int(row.get("deck_limit") or 4)
        available = max(0, deck_limit - current)
        if available <= 0:
            continue
        proposed = min(quantity, available)
        options.append(
            {
                "replacement_card_id": row["card_id"],
                "replacement_name_th": row.get("name_th", ""),
                "series_code": row.get("series_code", ""),
                "grade": str(row.get("grade")),
                "trigger": row.get("trigger") or "",
                "type_1": row.get("type_1", ""),
                "current_quantity": current,
                "deck_limit": deck_limit,
                "available_quantity": available,
                "proposed_quantity": proposed,
                "covers_full_quantity": proposed == quantity,
                "source": "data/packs/vanguard_th/cards.sqlite:cards",
            }
        )
        if len(options) >= 5:
            break
    return options


def _build_manual_package(
    recipe_id: str,
    manual_ids: list[str],
    quantities: dict[str, int],
    cards: dict[str, dict[str, Any]],
) -> dict[str, Any]:
    package_quantities = deepcopy(quantities)
    substitutions: list[dict[str, Any]] = []
    complete = True
    manual_set = set(manual_ids)
    for manual_id in manual_ids:
        qty = package_quantities.get(manual_id, 0)
        options = _manual_repair_options(manual_id, qty, package_quantities, cards, manual_set)
        full = next((option for option in options if option["covers_full_quantity"]), None)
        if not full:
            complete = False
            substitutions.append(
                {
                    "remove_card_id": manual_id,
                    "remove_quantity": qty,
                    "replacement_options": options,
                    "selected_replacement": None,
                }
            )
            continue
        package_quantities[manual_id] = max(0, package_quantities.get(manual_id, 0) - qty)
        package_quantities[full["replacement_card_id"]] = package_quantities.get(full["replacement_card_id"], 0) + qty
        substitutions.append(
            {
                "remove_card_id": manual_id,
                "remove_quantity": qty,
                "replacement_options": options,
                "selected_replacement": {
                    "card_id": full["replacement_card_id"],
                    "quantity": qty,
                    "source": full["source"],
                },
            }
        )
    return {
        "package_id": f"{recipe_id}_manual_overlap_pkg_001",
        "repair_type": "manual_review_overlap_substitution",
        "advisory_only": True,
        "substitutions": substitutions,
        "complete_candidate": complete,
        "runtime_promotion_allowed": False,
    }


def _grade_counts(quantities: dict[str, int], cards: dict[str, dict[str, Any]]) -> Counter[str]:
    counts: Counter[str] = Counter()
    for card_id, quantity in quantities.items():
        if quantity <= 0:
            continue
        counts[str(cards[card_id]["grade"])] += quantity
    return counts


def _pick_grade_additions(
    clan: str,
    grade: str,
    needed: int,
    quantities: dict[str, int],
    manual_ids: set[str],
) -> list[dict[str, Any]]:
    additions: list[dict[str, Any]] = []
    remaining = needed
    pool = _sort_candidates(_load_candidate_pool(clan, int(grade), "", "Normal Unit", manual_ids))
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
                "series_code": card.get("series_code", ""),
                "current_quantity": current,
                "final_quantity_if_chosen": current + qty,
                "deck_limit": deck_limit,
                "source": "data/packs/vanguard_th/cards.sqlite:cards",
            }
        )
        remaining -= qty
    return additions


def _pick_grade_removals(
    grade: str,
    count: int,
    quantities: dict[str, int],
    cards: dict[str, dict[str, Any]],
    protected_ids: set[str],
    manual_ids: set[str],
) -> list[dict[str, Any]]:
    rows = [
        {
            "card_id": card_id,
            "name_th": cards[card_id].get("name_th", ""),
            "quantity": quantity,
            "grade": str(cards[card_id]["grade"]),
            "series_code": cards[card_id].get("series_code", ""),
            "source": "m40_02_second_slice_recipe_draft_model",
        }
        for card_id, quantity in quantities.items()
        if quantity > 0 and str(cards[card_id]["grade"]) == grade and card_id not in protected_ids
    ]
    rows.sort(
        key=lambda row: (
            row["card_id"] not in manual_ids,
            -row["quantity"],
            SERIES_ORDER.get(row["series_code"], 999),
            row["card_id"],
        )
    )
    removals: list[dict[str, Any]] = []
    remaining = count
    for row in rows:
        if remaining <= 0:
            break
        qty = min(row["quantity"], remaining)
        removals.append({**row, "quantity": qty})
        remaining -= qty
    return removals


def _build_grade_package(
    recipe: dict[str, Any],
    validation: dict[str, Any],
    quantities: dict[str, int],
    cards: dict[str, dict[str, Any]],
    manual_ids: set[str],
) -> dict[str, Any]:
    issue = _grade_issue(validation)
    if not issue:
        return {
            "package_id": f"{recipe['recipe_id']}_grade_profile_pkg_001",
            "repair_type": "grade_profile_substitution",
            "advisory_only": True,
            "complete_candidate": False,
            "reason": "no_grade_profile_issue",
            "runtime_promotion_allowed": False,
        }
    protected = {recipe.get("pair", {}).get("source_card_id", ""), recipe.get("pair", {}).get("target_card_id", "")} - {""}
    clan = next(iter(cards.values()))["clan"]
    additions: list[dict[str, Any]] = []
    removals: list[dict[str, Any]] = []
    deficits: dict[str, int] = {}
    surpluses: dict[str, int] = {}
    for grade, values in issue.items():
        expected = int(values["expected"])
        actual = int(values["actual"])
        if actual < expected:
            deficits[grade] = expected - actual
            additions.extend(_pick_grade_additions(clan, grade, expected - actual, quantities, manual_ids))
        elif actual > expected:
            surpluses[grade] = actual - expected
            removals.extend(_pick_grade_removals(grade, actual - expected, quantities, cards, protected, manual_ids))

    after = dict(_grade_counts(quantities, cards))
    for item in additions:
        after[item["grade"]] = after.get(item["grade"], 0) + int(item["quantity"])
    for item in removals:
        after[item["grade"]] = after.get(item["grade"], 0) - int(item["quantity"])
    normalized_after = {grade: after.get(grade, 0) for grade in sorted(CLASSIC_GRADE_TARGET)}
    complete = (
        normalized_after == CLASSIC_GRADE_TARGET
        and sum(item["quantity"] for item in additions) == sum(item["quantity"] for item in removals)
    )
    return {
        "package_id": f"{recipe['recipe_id']}_grade_profile_pkg_001",
        "repair_type": "grade_profile_substitution",
        "advisory_only": True,
        "target_grade_counts": CLASSIC_GRADE_TARGET,
        "deficits": deficits,
        "surpluses": surpluses,
        "additions": additions,
        "removals": removals,
        "grade_counts_after": normalized_after,
        "added_card_count": sum(item["quantity"] for item in additions),
        "removed_card_count": sum(item["quantity"] for item in removals),
        "complete_candidate": complete,
        "runtime_promotion_allowed": False,
    }


def build_second_slice_blocker_repair_candidates(
    recipe_report: dict[str, Any] | None = None,
    validation_report: dict[str, Any] | None = None,
    consistency_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    recipe_report = recipe_report or load_json(M40_RECIPE_DRAFTS)
    validation_report = validation_report or load_json(M40_VALIDATION)
    consistency_report = consistency_report or load_json(M40_CONSISTENCY)
    cards = _load_cards(_all_recipe_card_ids(recipe_report))
    validations = _by_recipe_id(validation_report.get("recipe_validations", []))
    consistency = _by_recipe_id(consistency_report.get("consistency_checks", []))

    repair_items: list[dict[str, Any]] = []
    for recipe in recipe_report.get("recipe_drafts", []):
        recipe_id = recipe["recipe_id"]
        quantities = _quantity_map(recipe)
        validation = validations.get(recipe_id, {})
        manual_ids = _manual_ids(validation)
        manual_package = _build_manual_package(recipe_id, manual_ids, quantities, cards) if manual_ids else None
        grade_package = _build_grade_package(recipe, validation, quantities, cards, set(manual_ids))
        grade_removed_ids = {item["card_id"] for item in grade_package.get("removals", [])}
        grade_package_clears_manual = bool(manual_ids) and set(manual_ids).issubset(grade_removed_ids)
        ready_for_human_repair_review = bool(grade_package.get("complete_candidate")) and (
            not manual_ids
            or grade_package_clears_manual
            or bool(manual_package and manual_package["complete_candidate"])
        )
        repair_items.append(
            {
                "recipe_id": recipe_id,
                "source_candidate_edge": recipe.get("source_candidate_edge", ""),
                "validation_status": validation.get("validation_status", ""),
                "consistency_status": consistency.get(recipe_id, {}).get("consistency_status", ""),
                "manual_review_card_ids": manual_ids,
                "manual_overlap_repair_package": manual_package,
                "grade_profile_repair_package": grade_package,
                "grade_package_clears_manual_overlap": grade_package_clears_manual,
                "ready_for_human_repair_review": ready_for_human_repair_review,
                "runtime_promotion_allowed": False,
            }
        )

    return {
        "version": "M40-05",
        "description": "Second-slice blocker repair candidates",
        "selected_target": recipe_report.get("selected_target", {}),
        "source_inputs": {
            "second_slice_recipe_draft_model": str(M40_RECIPE_DRAFTS.relative_to(ROOT)),
            "second_slice_recipe_validation_report": str(M40_VALIDATION.relative_to(ROOT)),
            "second_slice_combo_recipe_consistency_report": str(M40_CONSISTENCY.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "scope": {
            "offline_repair_candidates": True,
            "changes_recipe_draft_file": False,
            "records_human_acceptance": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "runtime_deck": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
        },
        "repair_policy": {
            "source_backed_candidates": True,
            "substitution_only": True,
            "manual_review_cards_are_not_resolved_here": True,
            "grade_profile_candidates_are_previews": True,
            "runtime_promotion_allowed": False,
        },
        "repair_items": repair_items,
        "summary": {
            "recipe_count": len(repair_items),
            "manual_overlap_recipe_count": sum(1 for item in repair_items if item["manual_review_card_ids"]),
            "manual_repair_complete_count": sum(
                1
                for item in repair_items
                if item["manual_overlap_repair_package"]
                and item["manual_overlap_repair_package"]["complete_candidate"]
            ),
            "grade_profile_repair_candidate_count": sum(
                1 for item in repair_items if item["grade_profile_repair_package"]
            ),
            "grade_profile_complete_candidate_count": sum(
                1
                for item in repair_items
                if item["grade_profile_repair_package"].get("complete_candidate")
            ),
            "grade_package_clears_manual_overlap_count": sum(
                1 for item in repair_items if item["grade_package_clears_manual_overlap"]
            ),
            "ready_for_human_repair_review_count": sum(
                1 for item in repair_items if item["ready_for_human_repair_review"]
            ),
            "runtime_promotion_allowed": False,
            "ready_for_m40_closeout": bool(repair_items),
        },
        "next_target": {
            "milestone": "M40-closeout",
            "task": "Second-slice runtime readiness decision",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M40-05 Second-Slice Blocker Repair Candidates",
        "",
        "## Summary",
        "",
        f"- Recipes: `{summary['recipe_count']}`",
        f"- Manual-overlap recipes: `{summary['manual_overlap_recipe_count']}`",
        f"- Complete manual repair candidates: `{summary['manual_repair_complete_count']}`",
        f"- Grade profile repair candidates: `{summary['grade_profile_repair_candidate_count']}`",
        f"- Complete grade profile candidates: `{summary['grade_profile_complete_candidate_count']}`",
        f"- Grade packages clearing manual overlap: `{summary['grade_package_clears_manual_overlap_count']}`",
        f"- Ready for human repair review: `{summary['ready_for_human_repair_review_count']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M40-closeout: `{summary['ready_for_m40_closeout']}`",
        "",
        "## Repair Items",
        "",
    ]
    for item in report["repair_items"]:
        manual_package = item["manual_overlap_repair_package"] or {}
        grade_package = item["grade_profile_repair_package"] or {}
        lines.append(
            f"- `{item['recipe_id']}` edge=`{item['source_candidate_edge']}` "
            f"manual_complete=`{manual_package.get('complete_candidate')}` "
            f"grade_complete=`{grade_package.get('complete_candidate')}` "
            f"ready=`{item['ready_for_human_repair_review']}`"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Repair candidates are advisory substitution previews only.",
            "- M40-02 draft files are not modified.",
            "- Human/team review is still required before any future acceptance artifact.",
            "- Runtime/saved deck/UI/bot promotion remains disabled.",
            "",
            "## Next",
            "",
            "`M40-closeout`: Second-slice runtime readiness decision.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M40-05 second-slice blocker repair candidates.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_second_slice_blocker_repair_candidates()
    json_path = args.output_dir / "m40_05_second_slice_blocker_repair_candidates.json"
    md_path = args.output_dir / "m40_05_second_slice_blocker_repair_candidates.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M40-05 second-slice blocker repair candidates wrote {json_path}")
    print(f"M40-05 second-slice blocker repair candidates summary wrote {md_path}")
    print(
        "ready_for_m40_closeout={ready} recipes={recipes} human_review_ready={ready_count}".format(
            ready=report["summary"]["ready_for_m40_closeout"],
            recipes=report["summary"]["recipe_count"],
            ready_count=report["summary"]["ready_for_human_repair_review_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m40_closeout"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
