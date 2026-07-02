"""Build M60-06 seventh-slice blocker repair candidates.

This creates advisory repair candidates for blockers reported by M60-04 and
carried through M60-05. It does not mutate M60-03 drafts and does not create
saved decks, runtime fixtures, UI entries, G Zone runtime, Stride runtime, or
bot playbooks.
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
M60_RECIPE_DRAFTS = OUTPUT_DIR / "m60_03_seventh_slice_recipe_draft_model.json"
M60_VALIDATION = OUTPUT_DIR / "m60_04_seventh_slice_recipe_validation_report.json"
M60_CONSISTENCY = OUTPUT_DIR / "m60_05_seventh_slice_combo_recipe_consistency_report.json"
M60_SCAFFOLD = OUTPUT_DIR / "m60_01_seventh_slice_fixture_scaffold.json"
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


def _load_candidate_pool(
    clan: str,
    grade: int,
    trigger: str,
    type_1: str,
    excluded_ids: set[str],
    series_scope: Sequence[str],
) -> list[dict[str, Any]]:
    if not series_scope:
        return []
    placeholders = ",".join("?" for _ in series_scope)
    query = (
        "select card_id, name_th, series_code, clan, grade, trigger, deck_limit, type_1, shield, power "
        "from cards where clan = ? and grade = ? and coalesce(trigger, '') = ? and type_1 = ? "
        f"and series_code in ({placeholders}) order by series_code, card_id"
    )
    with closing(sqlite3.connect(CARDS_SQLITE)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, [clan, grade, trigger, type_1, *series_scope]).fetchall()
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


def _manual_repair_options(
    manual_card_id: str,
    quantity: int,
    quantities: dict[str, int],
    cards: dict[str, dict[str, Any]],
    manual_ids: set[str],
    series_scope: Sequence[str],
    series_order: dict[str, int],
) -> list[dict[str, Any]]:
    manual_card = cards[manual_card_id]
    pool = _sort_candidates(
        _load_candidate_pool(
            manual_card["clan"],
            int(manual_card["grade"]),
            manual_card.get("trigger") or "",
            manual_card["type_1"],
            manual_ids,
            series_scope,
        ),
        series_order,
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
    global_manual_ids: set[str],
    quantities: dict[str, int],
    cards: dict[str, dict[str, Any]],
    series_scope: Sequence[str],
    series_order: dict[str, int],
) -> dict[str, Any]:
    package_quantities = deepcopy(quantities)
    substitutions: list[dict[str, Any]] = []
    complete = True
    for manual_id in manual_ids:
        qty = package_quantities.get(manual_id, 0)
        options = _manual_repair_options(
            manual_id,
            qty,
            package_quantities,
            cards,
            global_manual_ids,
            series_scope,
            series_order,
        )
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
        "repair_type": "manual_review_overlap_substitution_preview",
        "advisory_only": True,
        "manual_review_may_accept_original_card": True,
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
    excluded_ids: set[str],
    series_scope: Sequence[str],
    series_order: dict[str, int],
) -> list[dict[str, Any]]:
    additions: list[dict[str, Any]] = []
    remaining = needed
    pool = _sort_candidates(
        _load_candidate_pool(clan, int(grade), "", "Normal Unit", excluded_ids, series_scope),
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
    series_order: dict[str, int],
) -> list[dict[str, Any]]:
    rows = [
        {
            "card_id": card_id,
            "name_th": cards[card_id].get("name_th", ""),
            "quantity": quantity,
            "grade": str(cards[card_id]["grade"]),
            "series_code": cards[card_id].get("series_code", ""),
            "source": "m60_03_seventh_slice_recipe_draft_model",
        }
        for card_id, quantity in quantities.items()
        if quantity > 0 and str(cards[card_id]["grade"]) == grade and card_id not in protected_ids
    ]
    rows.sort(
        key=lambda row: (
            row["card_id"] not in manual_ids,
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
    series_scope: Sequence[str],
    series_order: dict[str, int],
) -> dict[str, Any]:
    issue = _grade_issue(validation)
    if not issue:
        return {
            "package_id": f"{recipe['recipe_id']}_grade_profile_pkg_001",
            "repair_type": "grade_profile_substitution_preview",
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
    excluded_addition_ids = set(quantities) | set(manual_ids)
    for grade, values in issue.items():
        expected = int(values["expected"])
        actual = int(values["actual"])
        if actual < expected:
            deficits[grade] = expected - actual
            additions.extend(
                _pick_grade_additions(
                    clan,
                    grade,
                    expected - actual,
                    quantities,
                    excluded_addition_ids,
                    series_scope,
                    series_order,
                )
            )
        elif actual > expected:
            surpluses[grade] = actual - expected
            removals.extend(
                _pick_grade_removals(grade, actual - expected, quantities, cards, protected, manual_ids, series_order)
            )

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
        "repair_type": "grade_profile_substitution_preview",
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


def _build_g_zone_package(recipe_id: str, validation: dict[str, Any]) -> dict[str, Any] | None:
    if "g_zone_support_deferred" not in _issues_by_code(validation):
        return None
    return {
        "package_id": f"{recipe_id}_g_zone_deferred_pkg_001",
        "repair_type": "g_zone_support_deferred_decision",
        "advisory_only": True,
        "can_be_repaired_in_m60_06": False,
        "requires_future_system_work": [
            "G Zone deck slot model",
            "Stride deck-building validation",
            "G unit visibility and public-event policy",
            "Stride timing and Generation Break runtime support",
        ],
        "main_deck_validation_only": True,
        "runtime_promotion_allowed": False,
    }


def _build_bloom_token_package(recipe_id: str, validation: dict[str, Any]) -> dict[str, Any] | None:
    if "bloom_token_support_deferred" not in _issues_by_code(validation):
        return None
    return {
        "package_id": f"{recipe_id}_bloom_token_deferred_pkg_001",
        "repair_type": "bloom_token_support_deferred_decision",
        "advisory_only": True,
        "can_be_repaired_in_m60_06": False,
        "requires_future_system_work": [
            "Bloom keyword semantic template",
            "same-name unit detection across zones and circles",
            "token creation and token zone lifecycle support",
            "duration cleanup rules for Bloom/token-like modifiers",
        ],
        "manual_semantic_review_only": True,
        "runtime_promotion_allowed": False,
    }


def build_seventh_slice_blocker_repair_candidates(
    recipe_report: dict[str, Any] | None = None,
    validation_report: dict[str, Any] | None = None,
    consistency_report: dict[str, Any] | None = None,
    scaffold: dict[str, Any] | None = None,
) -> dict[str, Any]:
    recipe_report = recipe_report or load_json(M60_RECIPE_DRAFTS)
    validation_report = validation_report or load_json(M60_VALIDATION)
    consistency_report = consistency_report or load_json(M60_CONSISTENCY)
    scaffold = scaffold or load_json(M60_SCAFFOLD)
    series_scope = _series_scope(scaffold)
    series_order = _series_order(scaffold)
    cards = _load_cards(_all_recipe_card_ids(recipe_report))
    validations = _by_recipe_id(validation_report.get("recipe_validations", []))
    consistency = _by_recipe_id(consistency_report.get("consistency_checks", []))
    global_manual_ids = {
        card_id
        for validation in validations.values()
        for card_id in _manual_ids(validation)
    }

    repair_items: list[dict[str, Any]] = []
    for recipe in recipe_report.get("recipe_drafts", []):
        recipe_id = recipe["recipe_id"]
        quantities = _quantity_map(recipe)
        validation = validations.get(recipe_id, {})
        consistency_item = consistency.get(recipe_id, {})
        manual_ids = _manual_ids(validation)
        manual_package = (
            _build_manual_package(recipe_id, manual_ids, global_manual_ids, quantities, cards, series_scope, series_order)
            if manual_ids
            else None
        )
        grade_package = _build_grade_package(
            recipe,
            validation,
            quantities,
            cards,
            global_manual_ids,
            series_scope,
            series_order,
        )
        g_zone_package = _build_g_zone_package(recipe_id, validation)
        bloom_token_package = _build_bloom_token_package(recipe_id, validation)
        grade_removed_ids = {item["card_id"] for item in grade_package.get("removals", [])}
        grade_package_clears_manual = bool(manual_ids) and set(manual_ids).issubset(grade_removed_ids)
        structural_blockers = _structural_blockers(validation)
        grade_package_complete_or_not_needed = bool(grade_package.get("complete_candidate")) or (
            grade_package.get("reason") == "no_grade_profile_issue"
        )
        ready_for_human_repair_review = (
            not structural_blockers
            and bool(manual_package and manual_package["complete_candidate"])
            and grade_package_complete_or_not_needed
        )
        repair_items.append(
            {
                "recipe_id": recipe_id,
                "source_candidate_edge": recipe.get("source_candidate_edge", ""),
                "validation_status": validation.get("validation_status", ""),
                "consistency_status": consistency_item.get("consistency_status", ""),
                "manual_review_card_ids": manual_ids,
                "structural_blockers": structural_blockers,
                "manual_overlap_repair_package": manual_package,
                "grade_profile_repair_package": grade_package,
                "g_zone_deferred_package": g_zone_package,
                "bloom_token_deferred_package": bloom_token_package,
                "grade_package_clears_manual_overlap": grade_package_clears_manual,
                "ready_for_human_repair_review": ready_for_human_repair_review,
                "blocked_by_g_zone_deferred": bool(g_zone_package),
                "blocked_by_bloom_token_deferred": bool(bloom_token_package),
                "runtime_promotion_allowed": False,
            }
        )

    unexpected_structural_blocker_count = sum(1 for item in repair_items if item["structural_blockers"])
    return {
        "version": "M60-06",
        "description": "Seventh-slice blocker repair candidates",
        "selected_target": recipe_report.get("selected_target", {}),
        "source_inputs": {
            "seventh_slice_recipe_draft_model": str(M60_RECIPE_DRAFTS.relative_to(ROOT)),
            "seventh_slice_recipe_validation_report": str(M60_VALIDATION.relative_to(ROOT)),
            "seventh_slice_combo_recipe_consistency_report": str(M60_CONSISTENCY.relative_to(ROOT)),
            "seventh_slice_fixture_scaffold": str(M60_SCAFFOLD.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "scope": {
            "offline_repair_candidates": True,
            "changes_recipe_draft_file": False,
            "records_human_acceptance": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "runtime_deck": False,
            "g_zone_runtime": False,
            "stride_runtime": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
        },
        "repair_policy": {
            "source_backed_candidates": True,
            "substitution_only": True,
            "series_scope_from_m60_01_fixture_scaffold": series_scope,
            "manual_review_cards_are_not_resolved_here": True,
            "grade_profile_candidates_are_previews": True,
            "g_zone_support_is_deferred_system_work": True,
            "bloom_token_support_is_deferred_system_work": True,
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
            "grade_package_clears_manual_overlap_count": sum(
                1 for item in repair_items if item["grade_package_clears_manual_overlap"]
            ),
            "g_zone_deferred_recipe_count": sum(1 for item in repair_items if item["g_zone_deferred_package"]),
            "bloom_token_deferred_recipe_count": sum(
                1 for item in repair_items if item["bloom_token_deferred_package"]
            ),
            "unexpected_structural_blocker_recipe_count": unexpected_structural_blocker_count,
            "ready_for_human_repair_review_count": sum(
                1 for item in repair_items if item["ready_for_human_repair_review"]
            ),
            "runtime_promotion_allowed": False,
            "ready_for_m60_closeout": bool(repair_items) and unexpected_structural_blocker_count == 0,
        },
        "next_target": {
            "milestone": "M60-closeout",
            "task": "Seventh-slice runtime readiness decision",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M60-06 Seventh-Slice Blocker Repair Candidates",
        "",
        "## Summary",
        "",
        f"- Recipes: `{summary['recipe_count']}`",
        f"- Manual-overlap recipes: `{summary['manual_overlap_recipe_count']}`",
        f"- Complete manual repair candidates: `{summary['manual_repair_complete_count']}`",
        f"- Grade profile repair candidates: `{summary['grade_profile_repair_candidate_count']}`",
        f"- Complete grade profile candidates: `{summary['grade_profile_complete_candidate_count']}`",
        f"- Grade packages clearing manual overlap: `{summary['grade_package_clears_manual_overlap_count']}`",
        f"- G Zone deferred recipes: `{summary['g_zone_deferred_recipe_count']}`",
        f"- Bloom/token deferred recipes: `{summary['bloom_token_deferred_recipe_count']}`",
        f"- Unexpected structural blocker recipes: `{summary['unexpected_structural_blocker_recipe_count']}`",
        f"- Ready for human repair review: `{summary['ready_for_human_repair_review_count']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M60-closeout: `{summary['ready_for_m60_closeout']}`",
        "",
        "## Repair Items",
        "",
    ]
    for item in report["repair_items"]:
        manual_package = item["manual_overlap_repair_package"] or {}
        grade_package = item["grade_profile_repair_package"] or {}
        g_zone_package = item["g_zone_deferred_package"] or {}
        bloom_package = item["bloom_token_deferred_package"] or {}
        lines.append(
            f"- `{item['recipe_id']}` edge=`{item['source_candidate_edge']}` "
            f"manual_complete=`{manual_package.get('complete_candidate')}` "
            f"grade_complete=`{grade_package.get('complete_candidate')}` "
            f"g_zone_deferred=`{bool(g_zone_package)}` "
            f"bloom_token_deferred=`{bool(bloom_package)}` "
            f"ready=`{item['ready_for_human_repair_review']}`"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Repair candidates are advisory previews only.",
            "- M60-03 draft files are not modified.",
            "- Manual-review cards are not resolved by this milestone.",
            "- G Zone, Stride, Bloom, and token support remain future system work.",
            "- Runtime/saved deck/UI/bot promotion remains disabled.",
            "",
            "## Next",
            "",
            "`M60-closeout`: Seventh-slice runtime readiness decision.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M60-06 seventh-slice blocker repair candidates.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_seventh_slice_blocker_repair_candidates()
    json_path = args.output_dir / "m60_06_seventh_slice_blocker_repair_candidates.json"
    md_path = args.output_dir / "m60_06_seventh_slice_blocker_repair_candidates.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M60-06 seventh-slice blocker repair candidates wrote {json_path}")
    print(f"M60-06 seventh-slice blocker repair candidates summary wrote {md_path}")
    print(
        "ready_for_m60_closeout={ready} recipes={recipes} human_review_ready={ready_count} "
        "g_zone_deferred={g_zone} bloom_token_deferred={bloom_token}".format(
            ready=report["summary"]["ready_for_m60_closeout"],
            recipes=report["summary"]["recipe_count"],
            ready_count=report["summary"]["ready_for_human_repair_review_count"],
            g_zone=report["summary"]["g_zone_deferred_recipe_count"],
            bloom_token=report["summary"]["bloom_token_deferred_recipe_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m60_closeout"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
