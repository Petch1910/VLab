"""Build the M49-03 fourth-slice human-accepted repair artifact."""

from __future__ import annotations

import argparse
import json
import sys
from collections import Counter
from datetime import date
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M49_01_REVIEW = OUTPUT_DIR / "m49_01_fourth_slice_human_repair_review_packet.json"
M49_02_G_ZONE_DECISION = OUTPUT_DIR / "m49_02_fourth_slice_g_zone_support_decision.json"
M48_DRAFTS = OUTPUT_DIR / "m48_03_fourth_slice_recipe_draft_model.json"

DEFAULT_ACCEPTED_REVIEW_ITEM_ID = "m49_01_m48_recipe_001_repair_review"
DEFAULT_ACCEPTANCE_TEXT = "\u0e07\u0e31\u0e49\u0e19\u0e08\u0e31\u0e14\u0e44\u0e1b"
DEFAULT_G_ZONE_OPTION = "main_deck_only_for_current_windows_fixture"

if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_fourth_slice_blocker_repair_candidates import (  # noqa: E402
    CLASSIC_GRADE_TARGET,
    M48_SCAFFOLD,
    _grade_counts,
    _load_cards,
    _pick_grade_additions,
    _pick_grade_removals,
    _series_order,
    _series_scope,
)


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _find_review_item(review_packet: dict[str, Any], review_item_id: str) -> dict[str, Any]:
    for item in review_packet.get("review_items", []):
        if item.get("review_item_id") == review_item_id:
            return item
    raise ValueError(f"Review item not found: {review_item_id}")


def _find_decision_item(g_zone_decision: dict[str, Any], recipe_id: str) -> dict[str, Any]:
    for item in g_zone_decision.get("decision_items", []):
        if item.get("recipe_id") == recipe_id:
            return item
    raise ValueError(f"G Zone decision item not found for recipe: {recipe_id}")


def _find_recipe(drafts: dict[str, Any], recipe_id: str) -> dict[str, Any]:
    for recipe in drafts.get("recipe_drafts", []):
        if recipe.get("recipe_id") == recipe_id:
            return recipe
    raise ValueError(f"Recipe not found: {recipe_id}")


def _quantity_map(recipe: dict[str, Any]) -> dict[str, int]:
    return {row["card_id"]: int(row.get("quantity", 0)) for row in recipe.get("quantities", [])}


def _apply_manual_substitutions(
    quantities: dict[str, int],
    review_item: dict[str, Any],
) -> tuple[dict[str, int], list[dict[str, Any]]]:
    repaired = dict(quantities)
    issues: list[dict[str, Any]] = []
    for substitution in review_item.get("manual_substitutions", []):
        remove_card_id = substitution["remove_card_id"]
        remove_quantity = int(substitution.get("remove_quantity", 0))
        before = repaired.get(remove_card_id, 0)
        if before < remove_quantity:
            issues.append(
                {
                    "code": "manual_substitution_negative_quantity",
                    "severity": "blocker",
                    "card_id": remove_card_id,
                    "before": before,
                    "remove": remove_quantity,
                }
            )
            continue
        after = before - remove_quantity
        if after:
            repaired[remove_card_id] = after
        else:
            repaired.pop(remove_card_id, None)
        replacement_card_id = substitution["replacement_card_id"]
        repaired[replacement_card_id] = repaired.get(replacement_card_id, 0) + int(
            substitution.get("replacement_quantity", 0)
        )
    return repaired, issues


def _source_grade_package_conflicts(
    quantities_after_manual: dict[str, int],
    review_item: dict[str, Any],
) -> list[dict[str, Any]]:
    conflicts: list[dict[str, Any]] = []
    for removal in review_item.get("grade_removals", []):
        card_id = removal["card_id"]
        remove_quantity = int(removal.get("quantity", 0))
        available = quantities_after_manual.get(card_id, 0)
        if available < remove_quantity:
            conflicts.append(
                {
                    "code": "source_grade_package_remove_conflicts_after_manual_substitution",
                    "severity": "review",
                    "card_id": card_id,
                    "available_after_manual": available,
                    "source_grade_remove_quantity": remove_quantity,
                    "source_grade_package_id": review_item.get("grade_profile_package_id", ""),
                }
            )
    return conflicts


def _normalize_removal_sources(
    removals: list[dict[str, Any]],
    manual_replacement_ids: set[str],
) -> list[dict[str, Any]]:
    normalized: list[dict[str, Any]] = []
    for removal in removals:
        row = dict(removal)
        if row.get("card_id") in manual_replacement_ids:
            row["source"] = "m49_03_manual_substitution_preview"
        normalized.append(row)
    return normalized


def _build_recomputed_grade_package(
    recipe: dict[str, Any],
    review_item: dict[str, Any],
    quantities_after_manual: dict[str, int],
    scaffold: dict[str, Any],
) -> dict[str, Any]:
    cards = _load_cards(quantities_after_manual.keys())
    counts = _grade_counts(quantities_after_manual, cards)
    series_scope = _series_scope(scaffold)
    series_order = _series_order(scaffold)
    protected = {
        review_item.get("pair", {}).get("source_card_id", ""),
        review_item.get("pair", {}).get("target_card_id", ""),
    } - {""}
    manual_ids = set(review_item.get("manual_review_card_ids", []))
    manual_replacement_ids = {
        row["replacement_card_id"]
        for row in review_item.get("manual_substitutions", [])
        if row.get("replacement_card_id")
    }
    clan = next(iter(cards.values()))["clan"] if cards else ""
    additions: list[dict[str, Any]] = []
    removals: list[dict[str, Any]] = []
    deficits: dict[str, int] = {}
    surpluses: dict[str, int] = {}

    for grade, target in CLASSIC_GRADE_TARGET.items():
        actual = int(counts.get(grade, 0))
        if actual < target:
            deficits[grade] = target - actual
            additions.extend(
                _pick_grade_additions(
                    clan,
                    grade,
                    target - actual,
                    quantities_after_manual,
                    manual_ids,
                    series_scope,
                    series_order,
                )
            )
        elif actual > target:
            surpluses[grade] = actual - target
            removals.extend(
                _pick_grade_removals(
                    grade,
                    actual - target,
                    quantities_after_manual,
                    cards,
                    protected,
                    manual_ids,
                    series_order,
                )
            )

    removals = _normalize_removal_sources(removals, manual_replacement_ids)
    after: Counter[str] = Counter({grade: int(counts.get(grade, 0)) for grade in CLASSIC_GRADE_TARGET})
    for item in additions:
        after[item["grade"]] += int(item["quantity"])
    for item in removals:
        after[item["grade"]] -= int(item["quantity"])
    normalized_after = {grade: int(after.get(grade, 0)) for grade in sorted(CLASSIC_GRADE_TARGET)}
    added_count = sum(int(item["quantity"]) for item in additions)
    removed_count = sum(int(item["quantity"]) for item in removals)

    return {
        "package_id": f"{recipe['recipe_id']}_combined_manual_grade_pkg_001",
        "repair_type": "manual_then_recomputed_grade_profile_substitution",
        "advisory_only": True,
        "source_manual_repair_package_id": review_item.get("manual_repair_package_id", ""),
        "source_grade_profile_package_id": review_item.get("grade_profile_package_id", ""),
        "source_grade_package_directly_applied": False,
        "source_grade_package_recomputed_after_manual_substitution": True,
        "target_grade_counts": CLASSIC_GRADE_TARGET,
        "grade_counts_after_manual": {grade: int(counts.get(grade, 0)) for grade in sorted(CLASSIC_GRADE_TARGET)},
        "deficits_after_manual": deficits,
        "surpluses_after_manual": surpluses,
        "additions": additions,
        "removals": removals,
        "grade_counts_after": normalized_after,
        "added_card_count": added_count,
        "removed_card_count": removed_count,
        "complete_candidate": normalized_after == CLASSIC_GRADE_TARGET and added_count == removed_count,
        "runtime_promotion_allowed": False,
    }


def _apply_grade_package(
    quantities_after_manual: dict[str, int],
    grade_package: dict[str, Any],
) -> tuple[dict[str, int], list[dict[str, Any]]]:
    repaired = dict(quantities_after_manual)
    issues: list[dict[str, Any]] = []
    for removal in grade_package.get("removals", []):
        card_id = removal["card_id"]
        remove_quantity = int(removal.get("quantity", 0))
        before = repaired.get(card_id, 0)
        if before < remove_quantity:
            issues.append(
                {
                    "code": "combined_grade_substitution_negative_quantity",
                    "severity": "blocker",
                    "card_id": card_id,
                    "before": before,
                    "remove": remove_quantity,
                }
            )
            continue
        after = before - remove_quantity
        if after:
            repaired[card_id] = after
        else:
            repaired.pop(card_id, None)
    for addition in grade_package.get("additions", []):
        card_id = addition["card_id"]
        repaired[card_id] = repaired.get(card_id, 0) + int(addition.get("quantity", 0))
    return repaired, issues


def _repaired_quantity_rows(quantities: dict[str, int]) -> list[dict[str, Any]]:
    cards = _load_cards(quantities.keys())
    rows: list[dict[str, Any]] = []
    for card_id in sorted(quantities):
        card = cards.get(card_id, {})
        rows.append(
            {
                "card_id": card_id,
                "name_th": card.get("name_th", ""),
                "quantity": int(quantities[card_id]),
                "grade": str(card.get("grade", "")),
                "trigger": card.get("trigger") or "",
                "type_1": card.get("type_1", ""),
                "series_code": card.get("series_code", ""),
                "source": "m49_03_combined_repair_preview",
            }
        )
    return rows


def _grade_counts_for_output(quantities: dict[str, int]) -> dict[str, int]:
    cards = _load_cards(quantities.keys())
    counts = _grade_counts(quantities, cards)
    return {grade: int(counts.get(grade, 0)) for grade in sorted(CLASSIC_GRADE_TARGET)}


def _g_zone_boundary_allows_acceptance(g_zone_item: dict[str, Any], g_zone_decision: dict[str, Any]) -> bool:
    return (
        g_zone_decision.get("summary", {}).get("ready_for_m49_03") is True
        and g_zone_item.get("selected_option_id") == DEFAULT_G_ZONE_OPTION
        and g_zone_item.get("main_deck_only_validation_allowed") is True
        and g_zone_item.get("g_zone_runtime_enabled") is False
        and g_zone_item.get("stride_runtime_enabled") is False
    )


def build_fourth_slice_human_accepted_repair_artifact(
    review_packet: dict[str, Any] | None = None,
    g_zone_decision: dict[str, Any] | None = None,
    drafts: dict[str, Any] | None = None,
    scaffold: dict[str, Any] | None = None,
    *,
    accepted_review_item_id: str = DEFAULT_ACCEPTED_REVIEW_ITEM_ID,
    acceptance_text: str = DEFAULT_ACCEPTANCE_TEXT,
    accepted_at: str | None = None,
) -> dict[str, Any]:
    review_packet = review_packet or load_json(M49_01_REVIEW)
    g_zone_decision = g_zone_decision or load_json(M49_02_G_ZONE_DECISION)
    drafts = drafts or load_json(M48_DRAFTS)
    scaffold = scaffold or load_json(M48_SCAFFOLD)
    review_item = _find_review_item(review_packet, accepted_review_item_id)
    g_zone_item = _find_decision_item(g_zone_decision, review_item["recipe_id"])
    recipe = _find_recipe(drafts, review_item["recipe_id"])
    original_quantities = _quantity_map(recipe)
    quantities_after_manual, manual_issues = _apply_manual_substitutions(original_quantities, review_item)
    source_grade_conflicts = _source_grade_package_conflicts(quantities_after_manual, review_item)
    combined_grade_package = _build_recomputed_grade_package(recipe, review_item, quantities_after_manual, scaffold)
    repaired_quantities, grade_issues = _apply_grade_package(quantities_after_manual, combined_grade_package)
    repaired_rows = _repaired_quantity_rows(repaired_quantities)
    g_zone_boundary_allows_acceptance = _g_zone_boundary_allows_acceptance(g_zone_item, g_zone_decision)
    accepted = (
        bool(acceptance_text.strip())
        and bool(review_item.get("ready_for_human_repair_review"))
        and g_zone_boundary_allows_acceptance
    )
    accepted_at = accepted_at or date.today().isoformat()
    total_cards = sum(repaired_quantities.values())
    final_grade_counts = _grade_counts_for_output(repaired_quantities)
    issues = manual_issues + grade_issues
    ready_for_validation = (
        accepted
        and total_cards == 50
        and not issues
        and bool(combined_grade_package.get("complete_candidate"))
        and final_grade_counts == CLASSIC_GRADE_TARGET
        and g_zone_boundary_allows_acceptance
    )

    return {
        "version": "M49-03",
        "description": "Fourth-slice human-accepted repair artifact",
        "selected_target": review_packet.get("selected_target", {}),
        "source_inputs": {
            "human_repair_review_packet": str(M49_01_REVIEW.relative_to(ROOT)),
            "g_zone_support_decision": str(M49_02_G_ZONE_DECISION.relative_to(ROOT)),
            "m48_recipe_drafts": str(M48_DRAFTS.relative_to(ROOT)),
            "fourth_slice_fixture_scaffold": str(M48_SCAFFOLD.relative_to(ROOT)),
        },
        "scope": {
            "offline_human_accepted_artifact": True,
            "records_human_acceptance": True,
            "records_g_zone_decision": False,
            "changes_review_packet_file": False,
            "changes_g_zone_decision_file": False,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
            "declares_recipe_valid": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
        },
        "acceptance_record": {
            "decision": "accepted" if accepted else "pending",
            "accepted_by": "user",
            "accepted_at": accepted_at,
            "acceptance_text": acceptance_text,
            "interpreted_decision": "accept_first_ranked_main_deck_only_combined_manual_and_grade_repair_candidate",
            "accepted_review_item_id": accepted_review_item_id,
            "accepted_recipe_id": review_item["recipe_id"],
            "accepted_manual_repair_package_id": review_item.get("manual_repair_package_id", ""),
            "accepted_source_grade_profile_package_id": review_item.get("grade_profile_package_id", ""),
            "accepted_combined_repair_package_id": combined_grade_package["package_id"],
            "source_note": "Recorded from the user's go-ahead for continuing M49 after the M49-02 G Zone boundary decision.",
        },
        "g_zone_boundary_record": {
            "source_decision_item_id": g_zone_item["decision_item_id"],
            "source_g_zone_package_id": g_zone_item.get("g_zone_package_id", ""),
            "selected_option_id": g_zone_item.get("selected_option_id", ""),
            "main_deck_only_validation_allowed": bool(g_zone_item.get("main_deck_only_validation_allowed")),
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "grade4_main_deck_allowed": False,
            "g_units_allowed_in_main_deck": False,
            "boundary_allows_m49_03_acceptance": g_zone_boundary_allows_acceptance,
            "g_zone_requires_future_system_work": list(g_zone_item.get("g_zone_requires_future_system_work", [])),
        },
        "accepted_repair": {
            "recipe_id": review_item["recipe_id"],
            "source_candidate_edge": review_item["source_candidate_edge"],
            "pair": review_item["pair"],
            "manual_review_card_ids": review_item["manual_review_card_ids"],
            "manual_repair_package_id": review_item.get("manual_repair_package_id", ""),
            "manual_substitutions": review_item.get("manual_substitutions", []),
            "manual_application_issues": manual_issues,
            "source_grade_profile_package_id": review_item.get("grade_profile_package_id", ""),
            "source_grade_package_conflicts_after_manual": source_grade_conflicts,
            "combined_grade_repair_package": combined_grade_package,
            "repaired_quantities": repaired_rows,
            "repair_application_issues": issues,
            "main_deck_count_after_repair": total_cards,
            "grade_counts_after_repair": final_grade_counts,
            "runtime_promotion_allowed": False,
            "declares_recipe_valid": False,
            "requires_m49_04_validation": True,
            "ready_for_m49_04_validation_rerun": ready_for_validation,
        },
        "summary": {
            "accepted_review_item_id": accepted_review_item_id,
            "accepted_recipe_id": review_item["recipe_id"],
            "accepted_manual_repair_package_id": review_item.get("manual_repair_package_id", ""),
            "accepted_source_grade_profile_package_id": review_item.get("grade_profile_package_id", ""),
            "accepted_combined_repair_package_id": combined_grade_package["package_id"],
            "human_acceptance_recorded": accepted,
            "selected_g_zone_option_id": g_zone_item.get("selected_option_id", ""),
            "main_deck_only_boundary_applied": bool(g_zone_item.get("main_deck_only_validation_allowed")),
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "source_grade_package_conflict_count": len(source_grade_conflicts),
            "combined_grade_repair_recomputed": True,
            "main_deck_count_after_repair": total_cards,
            "grade_counts_after_repair": final_grade_counts,
            "repair_application_issue_count": len(issues),
            "runtime_promotion_allowed": False,
            "declares_recipe_valid": False,
            "ready_for_m49_04": ready_for_validation,
        },
        "next_target": {
            "milestone": "M49-04",
            "task": "Fourth-slice repaired recipe validation rerun",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    record = report["acceptance_record"]
    repair = report["accepted_repair"]
    combined = repair["combined_grade_repair_package"]
    boundary = report["g_zone_boundary_record"]
    lines = [
        "# M49-03 Fourth-Slice Human-Accepted Repair Artifact",
        "",
        "## Summary",
        "",
        f"- Accepted review item: `{summary['accepted_review_item_id']}`",
        f"- Accepted recipe: `{summary['accepted_recipe_id']}`",
        f"- Accepted manual repair package: `{summary['accepted_manual_repair_package_id']}`",
        f"- Source grade package: `{summary['accepted_source_grade_profile_package_id']}`",
        f"- Combined repair package: `{summary['accepted_combined_repair_package_id']}`",
        f"- Human acceptance recorded: `{summary['human_acceptance_recorded']}`",
        f"- Selected G Zone option: `{summary['selected_g_zone_option_id']}`",
        f"- Main-deck-only boundary applied: `{summary['main_deck_only_boundary_applied']}`",
        f"- G Zone runtime enabled: `{summary['g_zone_runtime_enabled']}`",
        f"- Stride runtime enabled: `{summary['stride_runtime_enabled']}`",
        f"- Source grade package conflicts after manual repair: `{summary['source_grade_package_conflict_count']}`",
        f"- Combined grade repair recomputed: `{summary['combined_grade_repair_recomputed']}`",
        f"- Main deck count after repair: `{summary['main_deck_count_after_repair']}`",
        f"- Grade counts after repair: `{summary['grade_counts_after_repair']}`",
        f"- Repair application issues: `{summary['repair_application_issue_count']}`",
        f"- Declares recipe valid: `{summary['declares_recipe_valid']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M49-04: `{summary['ready_for_m49_04']}`",
        "",
        "## Acceptance Record",
        "",
        f"- Decision: `{record['decision']}`",
        f"- Accepted by: `{record['accepted_by']}`",
        f"- Accepted at: `{record['accepted_at']}`",
        f"- Acceptance text: `{record['acceptance_text']}`",
        f"- Interpreted decision: `{record['interpreted_decision']}`",
        "",
        "## G Zone Boundary",
        "",
        f"- Source decision item: `{boundary['source_decision_item_id']}`",
        f"- Selected option: `{boundary['selected_option_id']}`",
        f"- Main-deck-only validation allowed: `{boundary['main_deck_only_validation_allowed']}`",
        f"- G Zone runtime enabled: `{boundary['g_zone_runtime_enabled']}`",
        f"- Stride runtime enabled: `{boundary['stride_runtime_enabled']}`",
        "",
        "## Accepted Repair",
        "",
        f"- Source edge: `{repair['source_candidate_edge']}`",
        f"- Pair: `{repair['pair']['source_card_id']}` -> `{repair['pair']['target_card_id']}`",
        f"- Manual substitutions: `{len(repair['manual_substitutions'])}` rows",
        f"- Recomputed grade additions: `{len(combined['additions'])}` rows",
        f"- Recomputed grade removals: `{len(combined['removals'])}` rows",
        f"- Source grade conflicts after manual repair: `{len(repair['source_grade_package_conflicts_after_manual'])}`",
        f"- Requires M49-04 validation: `{repair['requires_m49_04_validation']}`",
        "",
        "## Policy",
        "",
        "- This artifact records human acceptance of one main-deck-only repair candidate.",
        "- The source grade-profile package is recomputed after manual substitutions before validation.",
        "- It consumes the M49-02 G Zone boundary decision but does not change it.",
        "- It does not mutate M48-03 recipe drafts, M49-01 review packets, or M49-02 decision artifacts.",
        "- It does not declare the recipe valid.",
        "- It does not create runtime fixtures, saved decks, UI entries, or bot playbooks.",
        "- G Zone and Stride runtime remain disabled.",
        "",
        "## Next",
        "",
        "`M49-04`: Fourth-slice repaired recipe validation rerun.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M49-03 fourth-slice human-accepted repair artifact.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    parser.add_argument("--accepted-review-item-id", default=DEFAULT_ACCEPTED_REVIEW_ITEM_ID)
    parser.add_argument("--acceptance-text", default=DEFAULT_ACCEPTANCE_TEXT)
    parser.add_argument("--accepted-at", default=None)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_fourth_slice_human_accepted_repair_artifact(
        accepted_review_item_id=args.accepted_review_item_id,
        acceptance_text=args.acceptance_text,
        accepted_at=args.accepted_at,
    )
    json_path = args.output_dir / "m49_03_fourth_slice_human_accepted_repair_artifact.json"
    md_path = args.output_dir / "m49_03_fourth_slice_human_accepted_repair_artifact.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M49-03 fourth-slice human-accepted repair artifact wrote {json_path}")
    print(f"M49-03 fourth-slice human-accepted repair artifact summary wrote {md_path}")
    print(
        "ready_for_m49_04={ready} accepted={accepted} recipe={recipe} g_zone_option={option}".format(
            ready=report["summary"]["ready_for_m49_04"],
            accepted=report["summary"]["human_acceptance_recorded"],
            recipe=report["summary"]["accepted_recipe_id"],
            option=report["summary"]["selected_g_zone_option_id"],
        )
    )
    return 0 if report["summary"]["ready_for_m49_04"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
