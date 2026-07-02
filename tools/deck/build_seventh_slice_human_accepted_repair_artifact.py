"""Build the M61-03 seventh-slice human-accepted repair artifact.

This tool consumes the explicit M61-02 selected recipe artifact. It records
acceptance only when a non-empty acceptance text is provided and does not
declare the repaired recipe valid.
"""

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
M61_02_SELECTION = OUTPUT_DIR / "m61_02_seventh_slice_human_selected_recipe_artifact.json"
M60_DRAFTS = OUTPUT_DIR / "m60_03_seventh_slice_recipe_draft_model.json"

if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_seventh_slice_blocker_repair_candidates import (  # noqa: E402
    CLASSIC_GRADE_TARGET,
    M60_SCAFFOLD,
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


def _find_recipe(drafts: dict[str, Any], recipe_id: str) -> dict[str, Any]:
    for recipe in drafts.get("recipe_drafts", []):
        if recipe.get("recipe_id") == recipe_id:
            return recipe
    raise ValueError(f"Recipe not found: {recipe_id}")


def _quantity_map(recipe: dict[str, Any]) -> dict[str, int]:
    return {row["card_id"]: int(row.get("quantity", 0)) for row in recipe.get("quantities", [])}


def _require_acceptance_text(acceptance_text: str) -> str:
    normalized = acceptance_text.strip()
    if not normalized:
        raise ValueError("M61-03 requires non-empty acceptance_text.")
    return normalized


def _apply_manual_substitutions(
    quantities: dict[str, int],
    selection: dict[str, Any],
) -> tuple[dict[str, int], list[dict[str, Any]]]:
    repaired = dict(quantities)
    issues: list[dict[str, Any]] = []
    for substitution in selection.get("manual_substitutions", []):
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
        replacement_card_id = substitution.get("selected_replacement_card_id") or substitution.get(
            "replacement_card_id", ""
        )
        replacement_quantity = int(
            substitution.get("selected_replacement_quantity", substitution.get("replacement_quantity", 0))
        )
        if not replacement_card_id or replacement_quantity <= 0:
            issues.append(
                {
                    "code": "manual_substitution_missing_replacement",
                    "severity": "blocker",
                    "remove_card_id": remove_card_id,
                }
            )
            continue
        repaired[replacement_card_id] = repaired.get(replacement_card_id, 0) + replacement_quantity
    return repaired, issues


def _source_grade_package_conflicts(
    quantities_after_manual: dict[str, int],
    selection: dict[str, Any],
) -> list[dict[str, Any]]:
    conflicts: list[dict[str, Any]] = []
    for removal in selection.get("grade_removals", []):
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
                    "source_grade_package_id": selection.get("grade_profile_package_id", ""),
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
            row["source"] = "m61_03_manual_substitution_preview"
        normalized.append(row)
    return normalized


def _grade_target(selection: dict[str, Any]) -> dict[str, int]:
    target = selection.get("target_grade_counts") or CLASSIC_GRADE_TARGET
    return {str(grade): int(count) for grade, count in target.items()}


def _build_recomputed_grade_package(
    recipe: dict[str, Any],
    selection: dict[str, Any],
    quantities_after_manual: dict[str, int],
    scaffold: dict[str, Any],
) -> dict[str, Any]:
    cards = _load_cards(quantities_after_manual.keys())
    counts = _grade_counts(quantities_after_manual, cards)
    series_scope = _series_scope(scaffold)
    series_order = _series_order(scaffold)
    protected = {
        selection.get("pair", {}).get("source_card_id", ""),
        selection.get("pair", {}).get("target_card_id", ""),
    } - {""}
    manual_ids = set(selection.get("manual_review_card_ids", []))
    manual_replacement_ids = {
        row.get("selected_replacement_card_id") or row.get("replacement_card_id", "")
        for row in selection.get("manual_substitutions", [])
        if row.get("selected_replacement_card_id") or row.get("replacement_card_id")
    }
    clan = next(iter(cards.values()))["clan"] if cards else ""
    target = _grade_target(selection)
    additions: list[dict[str, Any]] = []
    removals: list[dict[str, Any]] = []
    deficits: dict[str, int] = {}
    surpluses: dict[str, int] = {}

    for grade, expected in target.items():
        actual = int(counts.get(grade, 0))
        if actual < expected:
            deficits[grade] = expected - actual
            additions.extend(
                _pick_grade_additions(
                    clan,
                    grade,
                    expected - actual,
                    quantities_after_manual,
                    manual_ids,
                    series_scope,
                    series_order,
                )
            )
        elif actual > expected:
            surpluses[grade] = actual - expected
            removals.extend(
                _pick_grade_removals(
                    grade,
                    actual - expected,
                    quantities_after_manual,
                    cards,
                    protected,
                    manual_ids,
                    series_order,
                )
            )

    removals = _normalize_removal_sources(removals, manual_replacement_ids)
    after: Counter[str] = Counter({grade: int(counts.get(grade, 0)) for grade in target})
    for item in additions:
        after[str(item["grade"])] += int(item["quantity"])
    for item in removals:
        after[str(item["grade"])] -= int(item["quantity"])
    normalized_after = {grade: int(after.get(grade, 0)) for grade in sorted(target)}
    added_count = sum(int(item["quantity"]) for item in additions)
    removed_count = sum(int(item["quantity"]) for item in removals)

    return {
        "package_id": f"{recipe['recipe_id']}_combined_manual_grade_pkg_001",
        "repair_type": "manual_then_recomputed_grade_profile_substitution",
        "advisory_only": True,
        "source_manual_overlap_package_id": selection.get("manual_overlap_package_id", ""),
        "source_grade_profile_package_id": selection.get("grade_profile_package_id", ""),
        "source_grade_package_directly_applied": False,
        "source_grade_package_recomputed_after_manual_substitution": True,
        "target_grade_counts": target,
        "grade_counts_after_manual": {grade: int(counts.get(grade, 0)) for grade in sorted(target)},
        "deficits_after_manual": deficits,
        "surpluses_after_manual": surpluses,
        "additions": additions,
        "removals": removals,
        "grade_counts_after": normalized_after,
        "added_card_count": added_count,
        "removed_card_count": removed_count,
        "complete_candidate": normalized_after == target and added_count == removed_count,
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
                "source": "m61_03_combined_repair_preview",
            }
        )
    return rows


def _grade_counts_for_output(quantities: dict[str, int], target: dict[str, int]) -> dict[str, int]:
    cards = _load_cards(quantities.keys())
    counts = _grade_counts(quantities, cards)
    return {grade: int(counts.get(grade, 0)) for grade in sorted(target)}


def build_seventh_slice_human_accepted_repair_artifact(
    selected_artifact: dict[str, Any] | None = None,
    drafts: dict[str, Any] | None = None,
    scaffold: dict[str, Any] | None = None,
    *,
    acceptance_text: str,
    accepted_by: str = "user",
    accepted_at: str | None = None,
) -> dict[str, Any]:
    selected_artifact = selected_artifact or load_json(M61_02_SELECTION)
    drafts = drafts or load_json(M60_DRAFTS)
    scaffold = scaffold or load_json(M60_SCAFFOLD)
    acceptance_text = _require_acceptance_text(acceptance_text)
    accepted_at = accepted_at or date.today().isoformat()
    selection = selected_artifact["selection"]
    recipe = _find_recipe(drafts, selection["recipe_id"])
    original_quantities = _quantity_map(recipe)
    quantities_after_manual, manual_issues = _apply_manual_substitutions(original_quantities, selection)
    source_grade_conflicts = _source_grade_package_conflicts(quantities_after_manual, selection)
    combined_grade_package = _build_recomputed_grade_package(recipe, selection, quantities_after_manual, scaffold)
    repaired_quantities, grade_issues = _apply_grade_package(quantities_after_manual, combined_grade_package)
    repaired_rows = _repaired_quantity_rows(repaired_quantities)
    target = _grade_target(selection)
    final_grade_counts = _grade_counts_for_output(repaired_quantities, target)
    total_cards = sum(repaired_quantities.values())
    issues = manual_issues + grade_issues
    accepted = bool(selected_artifact.get("summary", {}).get("ready_for_m61_03")) and bool(
        selection.get("ready_for_human_repair_review")
    )
    ready_for_m61_04 = (
        accepted
        and total_cards == 50
        and not issues
        and bool(combined_grade_package.get("complete_candidate"))
        and final_grade_counts == target
    )

    return {
        "version": "M61-03",
        "description": "Seventh-slice human-accepted repair artifact",
        "selected_target": selected_artifact.get("selected_target", {}),
        "source_inputs": {
            "m61_02_human_selected_recipe_artifact": str(M61_02_SELECTION.relative_to(ROOT)),
            "m60_recipe_drafts": str(M60_DRAFTS.relative_to(ROOT)),
            "seventh_slice_fixture_scaffold": str(M60_SCAFFOLD.relative_to(ROOT)),
        },
        "scope": {
            "offline_human_accepted_artifact": True,
            "records_human_selection": True,
            "records_human_acceptance": True,
            "records_g_zone_decision": False,
            "records_bloom_token_decision": False,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "g_zone_runtime": False,
            "stride_runtime": False,
            "bloom_token_runtime": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
            "declares_recipe_valid": False,
        },
        "acceptance_policy": {
            "requires_m61_02_selection": True,
            "requires_non_empty_acceptance_text": True,
            "acceptance_is_not_validation": True,
            "acceptance_is_not_g_zone_decision": True,
            "acceptance_is_not_bloom_token_decision": True,
            "source_grade_package_recomputed_after_manual_substitution": True,
            "runtime_promotion_allowed": False,
            "m61_04_must_record_explicit_g_zone_stride_bloom_token_decision": True,
            "m61_05_must_rerun_validation": True,
        },
        "acceptance_record": {
            "decision": "accepted",
            "accepted_by": accepted_by,
            "accepted_at": accepted_at,
            "acceptance_text": acceptance_text,
            "accepted_review_item_id": selection["selected_review_item_id"],
            "accepted_recipe_id": selection["recipe_id"],
            "accepted_manual_overlap_package_id": selection.get("manual_overlap_package_id", ""),
            "accepted_source_grade_profile_package_id": selection.get("grade_profile_package_id", ""),
            "accepted_g_zone_package_id": selection.get("g_zone_package_id", ""),
            "accepted_bloom_token_package_id": selection.get("bloom_token_package_id", ""),
            "source_note": "Recorded from explicit acceptance of the M61-02 selected seventh-slice recipe.",
        },
        "accepted_repair": {
            "recipe_id": selection["recipe_id"],
            "source_candidate_edge": selection.get("source_candidate_edge", ""),
            "pair": selection.get("pair", {}),
            "manual_overlap_package_id": selection.get("manual_overlap_package_id", ""),
            "manual_substitutions": selection.get("manual_substitutions", []),
            "source_grade_profile_package_id": selection.get("grade_profile_package_id", ""),
            "source_grade_package_conflicts_after_manual": source_grade_conflicts,
            "combined_grade_repair_package": combined_grade_package,
            "repaired_quantities": repaired_rows,
            "repair_application_issues": issues,
            "main_deck_count_after_repair": total_cards,
            "grade_counts_after_repair": final_grade_counts,
            "g_zone_package_id": selection.get("g_zone_package_id", ""),
            "g_zone_deferred": bool(selection.get("g_zone_deferred")),
            "g_zone_future_system_work": selection.get("g_zone_future_system_work", []),
            "g_zone_decision_options": selection.get("g_zone_decision_options", []),
            "bloom_token_package_id": selection.get("bloom_token_package_id", ""),
            "bloom_token_deferred": bool(selection.get("bloom_token_deferred")),
            "bloom_token_future_system_work": selection.get("bloom_token_future_system_work", []),
            "bloom_token_decision_options": selection.get("bloom_token_decision_options", []),
            "runtime_promotion_allowed": False,
            "requires_m61_04_g_zone_stride_bloom_token_decision": True,
            "requires_m61_05_validation": True,
            "ready_for_m61_04_system_decision": ready_for_m61_04,
        },
        "summary": {
            "accepted_review_item_id": selection["selected_review_item_id"],
            "accepted_recipe_id": selection["recipe_id"],
            "accepted_manual_overlap_package_id": selection.get("manual_overlap_package_id", ""),
            "accepted_source_grade_profile_package_id": selection.get("grade_profile_package_id", ""),
            "accepted_combined_repair_package_id": combined_grade_package["package_id"],
            "accepted_g_zone_package_id": selection.get("g_zone_package_id", ""),
            "accepted_bloom_token_package_id": selection.get("bloom_token_package_id", ""),
            "human_selection_recorded": True,
            "human_acceptance_recorded": True,
            "g_zone_decision_recorded": False,
            "bloom_token_decision_recorded": False,
            "g_zone_deferred": bool(selection.get("g_zone_deferred")),
            "bloom_token_deferred": bool(selection.get("bloom_token_deferred")),
            "source_grade_package_conflict_count": len(source_grade_conflicts),
            "combined_grade_repair_recomputed": bool(
                combined_grade_package.get("source_grade_package_recomputed_after_manual_substitution")
            ),
            "main_deck_count_after_repair": total_cards,
            "grade_counts_after_repair": final_grade_counts,
            "repair_application_issue_count": len(issues),
            "runtime_promotion_allowed": False,
            "declares_recipe_valid": False,
            "ready_for_m61_04": ready_for_m61_04,
        },
        "next_target": {
            "milestone": "M61-04",
            "task": "Seventh-slice G Zone, Stride, and Bloom/token decision artifact",
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
    lines = [
        "# M61-03 Seventh-Slice Human-Accepted Repair Artifact",
        "",
        "## Summary",
        "",
        f"- Accepted review item: `{summary['accepted_review_item_id']}`",
        f"- Accepted recipe: `{summary['accepted_recipe_id']}`",
        f"- Accepted manual package: `{summary['accepted_manual_overlap_package_id']}`",
        f"- Accepted source grade package: `{summary['accepted_source_grade_profile_package_id']}`",
        f"- Accepted combined package: `{summary['accepted_combined_repair_package_id']}`",
        f"- Accepted G Zone package: `{summary['accepted_g_zone_package_id']}`",
        f"- Accepted Bloom/token package: `{summary['accepted_bloom_token_package_id']}`",
        f"- Human selection recorded: `{summary['human_selection_recorded']}`",
        f"- Human acceptance recorded: `{summary['human_acceptance_recorded']}`",
        f"- G Zone decision recorded: `{summary['g_zone_decision_recorded']}`",
        f"- Bloom/token decision recorded: `{summary['bloom_token_decision_recorded']}`",
        f"- G Zone deferred: `{summary['g_zone_deferred']}`",
        f"- Bloom/token deferred: `{summary['bloom_token_deferred']}`",
        f"- Source grade package conflicts: `{summary['source_grade_package_conflict_count']}`",
        f"- Combined grade repair recomputed: `{summary['combined_grade_repair_recomputed']}`",
        f"- Main deck count after repair: `{summary['main_deck_count_after_repair']}`",
        f"- Grade counts after repair: `{summary['grade_counts_after_repair']}`",
        f"- Repair application issues: `{summary['repair_application_issue_count']}`",
        f"- Declares recipe valid: `{summary['declares_recipe_valid']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M61-04: `{summary['ready_for_m61_04']}`",
        "",
        "## Acceptance Record",
        "",
        f"- Decision: `{record['decision']}`",
        f"- Accepted by: `{record['accepted_by']}`",
        f"- Accepted at: `{record['accepted_at']}`",
        f"- Acceptance text: `{record['acceptance_text']}`",
        "",
        "## Accepted Repair",
        "",
        f"- Source edge: `{repair['source_candidate_edge']}`",
        f"- Pair: `{repair['pair'].get('source_card_id', '')}` -> `{repair['pair'].get('target_card_id', '')}`",
        f"- Manual substitutions: `{len(repair['manual_substitutions'])}`",
        f"- Source grade conflicts after manual: `{len(repair['source_grade_package_conflicts_after_manual'])}`",
        f"- Combined added cards: `{combined['added_card_count']}`",
        f"- Combined removed cards: `{combined['removed_card_count']}`",
        f"- Repaired quantity rows: `{len(repair['repaired_quantities'])}`",
        f"- G Zone future system work: `{len(repair['g_zone_future_system_work'])}`",
        f"- Bloom/token future system work: `{len(repair['bloom_token_future_system_work'])}`",
        "",
        "## Policy",
        "",
        "- Acceptance does not declare the recipe valid.",
        "- This artifact does not record a G Zone / Stride decision.",
        "- This artifact does not record a Bloom/token decision.",
        "- Runtime promotion remains disabled.",
        "- M61-04 must record explicit G Zone / Stride and Bloom/token decisions.",
        "- M61-05 must rerun validation before any fixture gate.",
        "",
        "## Next",
        "",
        "`M61-04`: Seventh-slice G Zone, Stride, and Bloom/token decision artifact.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M61-03 seventh-slice human-accepted repair artifact.")
    parser.add_argument("--acceptance-text", required=True, help="Non-empty text proving explicit acceptance.")
    parser.add_argument("--accepted-by", default="user")
    parser.add_argument("--accepted-at", default=None)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_seventh_slice_human_accepted_repair_artifact(
        acceptance_text=args.acceptance_text,
        accepted_by=args.accepted_by,
        accepted_at=args.accepted_at,
    )
    json_path = args.output_dir / "m61_03_seventh_slice_human_accepted_repair_artifact.json"
    md_path = args.output_dir / "m61_03_seventh_slice_human_accepted_repair_artifact.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M61-03 seventh-slice human-accepted repair artifact wrote {json_path}")
    print(f"M61-03 seventh-slice human-accepted repair artifact summary wrote {md_path}")
    print(
        "ready_for_m61_04={ready} accepted_recipe={recipe}".format(
            ready=report["summary"]["ready_for_m61_04"],
            recipe=report["summary"]["accepted_recipe_id"],
        )
    )
    return 0 if report["summary"]["ready_for_m61_04"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
