"""Build M64-06 eighth-slice blocker repair candidates.

This creates advisory repair candidates for the M64-04/M64-05 review gates.
The selected eighth slice has no numeric deck blockers, no manual-overlap
cards, and no missing pair cards. It still needs human recipe selection,
grade-profile review, and deferred Lock/Legion runtime decisions before any
runtime promotion can happen.
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
M64_RECIPE_DRAFTS = OUTPUT_DIR / "m64_03_eighth_slice_recipe_draft_model.json"
M64_VALIDATION = OUTPUT_DIR / "m64_04_eighth_slice_recipe_validation_report.json"
M64_CONSISTENCY = OUTPUT_DIR / "m64_05_eighth_slice_combo_recipe_consistency_report.json"
M64_SCAFFOLD = OUTPUT_DIR / "m64_01_eighth_slice_fixture_scaffold.json"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"

CLASSIC_GRADE_TARGET = {"0": 17, "1": 14, "2": 11, "3": 8}
STRUCTURAL_BLOCKER_CODES = {
    "missing_cards",
    "non_positive_quantity",
    "copy_limit_exceeded",
    "main_deck_size_mismatch",
    "unfilled_slots",
    "trigger_count_mismatch",
    "heal_trigger_limit_exceeded",
    "grade4_main_deck_not_allowed",
    "clan_mismatch",
}


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


def _series_scope(scaffold: dict[str, Any]) -> list[str]:
    fixture = scaffold.get("fixture_scaffold", {})
    return list(fixture.get("set_scope") or fixture.get("source_series_present") or [])


def _series_order(scaffold: dict[str, Any]) -> dict[str, int]:
    return {code: index for index, code in enumerate(_series_scope(scaffold))}


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


def _load_grade_candidate_pool(
    clan: str,
    grade: str,
    excluded_ids: set[str],
    series_scope: Sequence[str],
) -> list[dict[str, Any]]:
    if not series_scope:
        return []
    placeholders = ",".join("?" for _ in series_scope)
    query = (
        "select card_id, name_th, series_code, clan, grade, trigger, deck_limit, type_1, shield, power "
        "from cards where clan = ? and grade = ? and coalesce(trigger, '') = '' "
        "and type_1 = 'Normal Unit' "
        f"and series_code in ({placeholders}) order by series_code, card_id"
    )
    with closing(sqlite3.connect(CARDS_SQLITE)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, [clan, int(grade), *series_scope]).fetchall()
    return [dict(row) for row in rows if row["card_id"] not in excluded_ids]


def _sort_candidates(rows: Sequence[dict[str, Any]], series_order: dict[str, int]) -> list[dict[str, Any]]:
    return sorted(
        rows,
        key=lambda row: (
            series_order.get(row.get("series_code", ""), 999),
            -int(row.get("shield") or 0),
            -int(row.get("power") or 0),
            row["card_id"],
        ),
    )


def _issues_by_code(validation: dict[str, Any]) -> dict[str, dict[str, Any]]:
    return {issue.get("code", ""): issue for issue in validation.get("issues", []) if issue.get("code")}


def _has_issue(validation: dict[str, Any], code: str) -> bool:
    return code in _issues_by_code(validation)


def _manual_ids(validation: dict[str, Any]) -> list[str]:
    issue = _issues_by_code(validation).get("manual_review_card_overlap")
    if not issue:
        return []
    return sorted(issue.get("details", {}).get("card_ids", []))


def _grade_issue(validation: dict[str, Any]) -> dict[str, dict[str, int]]:
    issue = _issues_by_code(validation).get("grade_profile_review")
    if not issue:
        return {}
    return issue.get("details", {})


def _structural_blockers(validation: dict[str, Any]) -> list[str]:
    return sorted(
        issue["code"]
        for issue in validation.get("issues", [])
        if issue.get("severity") == "blocker" and issue.get("code") in STRUCTURAL_BLOCKER_CODES
    )


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
    excluded_ids: set[str],
    series_scope: Sequence[str],
    series_order: dict[str, int],
) -> list[dict[str, Any]]:
    additions: list[dict[str, Any]] = []
    remaining = needed
    pool = _sort_candidates(
        _load_grade_candidate_pool(clan, grade, excluded_ids, series_scope),
        series_order,
    )
    for card in pool:
        if remaining <= 0:
            break
        current = quantities.get(card["card_id"], 0)
        deck_limit = int(card.get("deck_limit") or 4)
        available = max(0, deck_limit - current)
        if available <= 0:
            continue
        quantity = min(available, remaining)
        additions.append(
            {
                "card_id": card["card_id"],
                "name_th": card.get("name_th", ""),
                "quantity": quantity,
                "grade": str(card["grade"]),
                "series_code": card.get("series_code", ""),
                "current_quantity": current,
                "final_quantity_if_chosen": current + quantity,
                "deck_limit": deck_limit,
                "source": "data/packs/vanguard_th/cards.sqlite:cards",
            }
        )
        remaining -= quantity
    return additions


def _pick_grade_removals(
    grade: str,
    count: int,
    quantities: dict[str, int],
    cards: dict[str, dict[str, Any]],
    protected_ids: set[str],
    series_order: dict[str, int],
) -> list[dict[str, Any]]:
    rows = [
        {
            "card_id": card_id,
            "name_th": cards[card_id].get("name_th", ""),
            "quantity": quantity,
            "grade": str(cards[card_id]["grade"]),
            "series_code": cards[card_id].get("series_code", ""),
            "source": "m64_03_eighth_slice_recipe_draft_model",
        }
        for card_id, quantity in quantities.items()
        if quantity > 0
        and str(cards[card_id]["grade"]) == grade
        and card_id not in protected_ids
        and not (cards[card_id].get("trigger") or "")
    ]
    rows.sort(
        key=lambda row: (
            -row["quantity"],
            series_order.get(row["series_code"], 999),
            row["card_id"],
        )
    )
    removals: list[dict[str, Any]] = []
    remaining = count
    for row in rows:
        if remaining <= 0:
            break
        quantity = min(row["quantity"], remaining)
        removals.append({**row, "quantity": quantity})
        remaining -= quantity
    return removals


def _build_human_selection_package(recipe: dict[str, Any], consistency: dict[str, Any]) -> dict[str, Any]:
    pair = recipe.get("pair", {})
    return {
        "package_id": f"{recipe['recipe_id']}_human_selection_pkg_001",
        "repair_type": "human_recipe_selection_decision",
        "advisory_only": True,
        "records_human_selection": False,
        "requires_human_selection": True,
        "pair_cards_present": bool(consistency.get("pair_cards_present", False)),
        "candidate_pair": {
            "source_card_id": pair.get("source_card_id", ""),
            "source_name_th": pair.get("source_name_th", ""),
            "target_card_id": pair.get("target_card_id", ""),
            "target_name_th": pair.get("target_name_th", ""),
            "net_score": pair.get("net_score", 0),
        },
        "runtime_promotion_allowed": False,
    }


def _build_grade_package(
    recipe: dict[str, Any],
    validation: dict[str, Any],
    quantities: dict[str, int],
    cards: dict[str, dict[str, Any]],
    series_scope: Sequence[str],
    series_order: dict[str, int],
) -> dict[str, Any]:
    issue = _grade_issue(validation)
    current_counts = {grade: int(count) for grade, count in validation.get("count_summary", {}).get("grade_counts", {}).items()}
    package = {
        "package_id": f"{recipe['recipe_id']}_grade_profile_pkg_001",
        "repair_type": "grade_profile_substitution_preview",
        "advisory_only": True,
        "target_grade_counts": deepcopy(CLASSIC_GRADE_TARGET),
        "grade_counts_before": current_counts,
        "grade_profile_issue": issue,
        "additions": [],
        "removals": [],
        "added_card_count": 0,
        "removed_card_count": 0,
        "grade_counts_after": current_counts,
        "complete_candidate": False,
        "runtime_promotion_allowed": False,
    }
    if not issue:
        package["reason"] = "no_grade_profile_issue"
        return package

    simulated = deepcopy(quantities)
    protected_ids = {
        recipe.get("pair", {}).get("source_card_id", ""),
        recipe.get("pair", {}).get("target_card_id", ""),
    } - {""}
    excluded_addition_ids = set(simulated)
    clan = recipe.get("selected_target", {}).get("group") or ""
    if not clan:
        clan_counts = validation.get("count_summary", {}).get("clan_counts", {})
        if len(clan_counts) == 1:
            clan = next(iter(clan_counts))

    additions: list[dict[str, Any]] = []
    removals: list[dict[str, Any]] = []
    for grade, target in CLASSIC_GRADE_TARGET.items():
        actual = current_counts.get(grade, 0)
        if actual < target:
            additions.extend(
                _pick_grade_additions(
                    clan,
                    grade,
                    target - actual,
                    simulated,
                    excluded_addition_ids,
                    series_scope,
                    series_order,
                )
            )
        elif actual > target:
            removals.extend(
                _pick_grade_removals(
                    grade,
                    actual - target,
                    simulated,
                    cards,
                    protected_ids,
                    series_order,
                )
            )

    for removal in removals:
        card_id = removal["card_id"]
        simulated[card_id] = max(0, simulated.get(card_id, 0) - int(removal["quantity"]))
        if simulated[card_id] == 0:
            del simulated[card_id]
    for addition in additions:
        card_id = addition["card_id"]
        simulated[card_id] = simulated.get(card_id, 0) + int(addition["quantity"])

    after_counts = dict(sorted(_grade_counts(simulated, cards | _load_cards([item["card_id"] for item in additions])).items()))
    added_count = sum(int(item["quantity"]) for item in additions)
    removed_count = sum(int(item["quantity"]) for item in removals)
    package.update(
        {
            "additions": additions,
            "removals": removals,
            "added_card_count": added_count,
            "removed_card_count": removed_count,
            "grade_counts_after": after_counts,
            "complete_candidate": (
                added_count == removed_count
                and after_counts == CLASSIC_GRADE_TARGET
                and sum(simulated.values()) == 50
            ),
        }
    )
    return package


def _build_lock_package(recipe_id: str, validation: dict[str, Any]) -> dict[str, Any] | None:
    if not _has_issue(validation, "lock_runtime_support_deferred"):
        return None
    return {
        "package_id": f"{recipe_id}_lock_deferred_pkg_001",
        "repair_type": "lock_runtime_support_deferred_decision",
        "advisory_only": True,
        "can_be_repaired_in_m64_06": False,
        "requires_future_system_work": [
            "Lock/Unlock runtime rules module",
            "locked-circle state representation",
            "unlock timing cleanup",
        ],
        "main_deck_validation_only": True,
        "runtime_promotion_allowed": False,
    }


def _build_legion_package(recipe_id: str, validation: dict[str, Any]) -> dict[str, Any] | None:
    if not _has_issue(validation, "legion_runtime_support_deferred"):
        return None
    return {
        "package_id": f"{recipe_id}_legion_deferred_pkg_001",
        "repair_type": "legion_runtime_support_deferred_decision",
        "advisory_only": True,
        "can_be_repaired_in_m64_06": False,
        "requires_future_system_work": [
            "Legion/Mate declaration rules",
            "mate pair validation model",
            "Legion state and seek/drop-zone movement helpers",
        ],
        "main_deck_validation_only": True,
        "runtime_promotion_allowed": False,
    }


def build_eighth_slice_blocker_repair_candidates(
    recipe_report: dict[str, Any] | None = None,
    validation_report: dict[str, Any] | None = None,
    consistency_report: dict[str, Any] | None = None,
    scaffold: dict[str, Any] | None = None,
) -> dict[str, Any]:
    recipe_report = recipe_report or load_json(M64_RECIPE_DRAFTS)
    validation_report = validation_report or load_json(M64_VALIDATION)
    consistency_report = consistency_report or load_json(M64_CONSISTENCY)
    scaffold = scaffold or load_json(M64_SCAFFOLD)

    validations_by_recipe = _by_recipe_id(validation_report.get("recipe_validations", []))
    consistency_by_recipe = _by_recipe_id(consistency_report.get("consistency_checks", []))
    cards = _load_cards(_all_recipe_card_ids(recipe_report))
    series_scope = _series_scope(scaffold)
    series_order = _series_order(scaffold)
    repair_items: list[dict[str, Any]] = []

    for recipe in recipe_report.get("recipe_drafts", []):
        recipe_id = recipe["recipe_id"]
        validation = validations_by_recipe.get(recipe_id, {})
        consistency = consistency_by_recipe.get(recipe_id, {})
        quantities = _quantity_map(recipe)
        grade_package = _build_grade_package(
            recipe,
            validation,
            quantities,
            cards,
            series_scope,
            series_order,
        )
        lock_package = _build_lock_package(recipe_id, validation)
        legion_package = _build_legion_package(recipe_id, validation)
        structural_blockers = _structural_blockers(validation)
        manual_ids = _manual_ids(validation)
        ready_for_human_repair_review = (
            not structural_blockers
            and not manual_ids
            and consistency.get("pair_cards_present", False)
            and grade_package.get("complete_candidate", False)
        )
        repair_items.append(
            {
                "recipe_id": recipe_id,
                "source_candidate_edge": recipe.get("source_candidate_edge", ""),
                "source_edge_rank": recipe.get("source_edge_rank"),
                "source_card_id": recipe.get("pair", {}).get("source_card_id", ""),
                "target_card_id": recipe.get("pair", {}).get("target_card_id", ""),
                "validation_status": validation.get("validation_status", "missing_validation"),
                "consistency_status": consistency.get("consistency_status", "missing_consistency"),
                "manual_review_card_ids": manual_ids,
                "structural_blockers": structural_blockers,
                "human_selection_package": _build_human_selection_package(recipe, consistency),
                "grade_profile_repair_package": grade_package,
                "lock_deferred_package": lock_package,
                "legion_deferred_package": legion_package,
                "ready_for_human_repair_review": ready_for_human_repair_review,
                "blocked_by_lock_deferred": bool(lock_package),
                "blocked_by_legion_deferred": bool(legion_package),
                "runtime_promotion_allowed": False,
            }
        )

    unexpected_structural_blocker_count = sum(1 for item in repair_items if item["structural_blockers"])
    return {
        "version": "M64-06",
        "description": "Eighth-slice blocker repair candidates",
        "selected_target": recipe_report.get("selected_target", {}),
        "source_inputs": {
            "eighth_slice_recipe_draft_model": str(M64_RECIPE_DRAFTS.relative_to(ROOT)),
            "eighth_slice_recipe_validation_report": str(M64_VALIDATION.relative_to(ROOT)),
            "eighth_slice_combo_recipe_consistency_report": str(M64_CONSISTENCY.relative_to(ROOT)),
            "eighth_slice_fixture_scaffold": str(M64_SCAFFOLD.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "scope": {
            "offline_repair_candidates": True,
            "changes_recipe_draft_file": False,
            "records_human_selection": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "runtime_deck": False,
            "lock_runtime": False,
            "legion_runtime": False,
            "bot_playbook": False,
            "automatic_deck_injection": False,
            "direct_GameState_mutation": False,
        },
        "repair_policy": {
            "source_backed_candidates": True,
            "substitution_only": True,
            "manual_review_cards_are_not_resolved_here": True,
            "human_selection_is_not_recorded_here": True,
            "grade_profile_candidates_are_previews": True,
            "lock_support_is_deferred_system_work": True,
            "legion_support_is_deferred_system_work": True,
            "series_scope_from_m64_01_fixture_scaffold": series_scope,
            "runtime_promotion_allowed": False,
        },
        "repair_items": repair_items,
        "summary": {
            "recipe_count": len(repair_items),
            "manual_overlap_recipe_count": sum(1 for item in repair_items if item["manual_review_card_ids"]),
            "human_selection_candidate_count": sum(1 for item in repair_items if item["human_selection_package"]),
            "grade_profile_repair_candidate_count": sum(
                1
                for item in repair_items
                if item["grade_profile_repair_package"]
                and item["grade_profile_repair_package"].get("reason") != "no_grade_profile_issue"
            ),
            "grade_profile_complete_candidate_count": sum(
                1
                for item in repair_items
                if item["grade_profile_repair_package"].get("complete_candidate")
            ),
            "lock_deferred_recipe_count": sum(1 for item in repair_items if item["lock_deferred_package"]),
            "legion_deferred_recipe_count": sum(1 for item in repair_items if item["legion_deferred_package"]),
            "unexpected_structural_blocker_recipe_count": unexpected_structural_blocker_count,
            "ready_for_human_repair_review_count": sum(
                1 for item in repair_items if item["ready_for_human_repair_review"]
            ),
            "runtime_promotion_allowed": False,
            "ready_for_m64_closeout": bool(repair_items) and unexpected_structural_blocker_count == 0,
        },
        "next_target": {
            "milestone": "M64-closeout",
            "task": "Eighth-slice runtime readiness decision",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M64-06 Eighth-Slice Blocker Repair Candidates",
        "",
        "## Summary",
        "",
        f"- Recipes: `{summary['recipe_count']}`",
        f"- Manual-overlap recipes: `{summary['manual_overlap_recipe_count']}`",
        f"- Human selection candidates: `{summary['human_selection_candidate_count']}`",
        f"- Grade profile repair candidates: `{summary['grade_profile_repair_candidate_count']}`",
        f"- Complete grade profile candidates: `{summary['grade_profile_complete_candidate_count']}`",
        f"- Lock deferred recipes: `{summary['lock_deferred_recipe_count']}`",
        f"- Legion deferred recipes: `{summary['legion_deferred_recipe_count']}`",
        f"- Unexpected structural blocker recipes: `{summary['unexpected_structural_blocker_recipe_count']}`",
        f"- Ready for human repair review: `{summary['ready_for_human_repair_review_count']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M64-closeout: `{summary['ready_for_m64_closeout']}`",
        "",
        "## Repair Items",
        "",
    ]
    for item in report["repair_items"]:
        grade_package = item["grade_profile_repair_package"] or {}
        lines.append(
            f"- `{item['recipe_id']}` edge=`{item['source_candidate_edge']}` "
            f"grade_complete=`{grade_package.get('complete_candidate')}` "
            f"lock_deferred=`{bool(item['lock_deferred_package'])}` "
            f"legion_deferred=`{bool(item['legion_deferred_package'])}` "
            f"ready=`{item['ready_for_human_repair_review']}`"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Repair candidates are advisory previews only.",
            "- Human selection is surfaced but not recorded.",
            "- Grade-profile substitutions are source-backed previews.",
            "- Lock runtime support remains deferred system work.",
            "- Legion runtime support remains deferred system work.",
            "- No saved-deck injection, UI publication, runtime deck creation, bot integration, or GameState mutation.",
            "",
            "## Next",
            "",
            "`M64-closeout`: Eighth-slice runtime readiness decision.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M64-06 eighth-slice blocker repair candidates.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_eighth_slice_blocker_repair_candidates()
    json_path = args.output_dir / "m64_06_eighth_slice_blocker_repair_candidates.json"
    md_path = args.output_dir / "m64_06_eighth_slice_blocker_repair_candidates.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M64-06 eighth-slice blocker repair candidates wrote {json_path}")
    print(f"M64-06 eighth-slice blocker repair candidates summary wrote {md_path}")
    print(
        "ready_for_m64_closeout={ready} recipes={recipes} human_selection={human} "
        "lock_deferred={lock} legion_deferred={legion}".format(
            ready=report["summary"]["ready_for_m64_closeout"],
            recipes=report["summary"]["recipe_count"],
            human=report["summary"]["human_selection_candidate_count"],
            lock=report["summary"]["lock_deferred_recipe_count"],
            legion=report["summary"]["legion_deferred_recipe_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m64_closeout"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
