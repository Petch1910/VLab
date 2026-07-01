"""Build the M37-05 revised recipe validation rerun.

This applies the M37-02 recommended trigger package to the accepted seed recipe
in memory only, reruns the existing offline validator and consistency check, and
reports the delta. No source recipe file is modified.
"""

from __future__ import annotations

import argparse
import copy
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
RECIPE_DRAFTS = OUTPUT_DIR / "m36_02_deck_recipe_draft_model.json"
ORIGINAL_VALIDATION = OUTPUT_DIR / "m36_03_deck_recipe_validation_report.json"
COMBO_LINES = OUTPUT_DIR / "m35_d3_first_slice_combo_line_explainer.json"
REVIEW_PACKET = OUTPUT_DIR / "m36_01_first_slice_review_packet.json"
M37_02_REPAIR = OUTPUT_DIR / "m37_02_trigger_package_repair_proposal.json"
M37_04_MAPPINGS = OUTPUT_DIR / "m37_04_manual_semantic_mapping_candidates.json"


if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.check_combo_recipe_consistency import build_consistency_report  # noqa: E402
from tools.deck.validate_deck_recipe_drafts import build_validation_report  # noqa: E402


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
        if recipe.get("recipe_id") == recipe_id:
            return recipe
    raise ValueError(f"Recipe not found: {recipe_id}")


def _find_validation(validation_report: dict[str, Any], recipe_id: str) -> dict[str, Any]:
    for validation in validation_report.get("recipe_validations", []):
        if validation.get("recipe_id") == recipe_id:
            return validation
    raise ValueError(f"Validation not found: {recipe_id}")


def _apply_quantity_delta(recipe: dict[str, Any], quantity_delta: Sequence[dict[str, Any]]) -> dict[str, Any]:
    revised = copy.deepcopy(recipe)
    rows_by_id = {row["card_id"]: row for row in revised.get("quantities", [])}
    added_count = 0
    for item in quantity_delta:
        card_id = item["card_id"]
        quantity = int(item["quantity"])
        added_count += quantity
        if card_id in rows_by_id:
            rows_by_id[card_id]["quantity"] = int(rows_by_id[card_id].get("quantity", 0)) + quantity
            rows_by_id[card_id]["quantity_source"] = "m37_05_in_memory_trigger_repair_delta"
        else:
            row = {
                "card_id": card_id,
                "quantity": quantity,
                "roles": ["trigger_repair_candidate"],
                "quantity_source": "m37_05_in_memory_trigger_repair_delta",
            }
            revised.setdefault("quantities", []).append(row)
            rows_by_id[card_id] = row

    slots = revised.setdefault("slot_summary", {})
    slots["explicit_card_count"] = int(slots.get("explicit_card_count", 0)) + added_count
    slots["trigger_slots_assigned"] = int(slots.get("trigger_slots_assigned", 0)) + added_count
    slots["trigger_slots_unfilled"] = max(0, int(slots.get("trigger_slots_unfilled", 0)) - added_count)
    slots["total_unfilled_slots"] = max(0, int(slots.get("main_deck_target", 50)) - int(slots["explicit_card_count"]))
    revised["recipe_status"] = (
        "draft_quantity_complete_pending_review"
        if slots["total_unfilled_slots"] == 0
        else "draft_with_gaps"
    )
    revised["review_blockers"] = ["human_acceptance"]
    metadata = revised.setdefault("validation_metadata", {})
    metadata["m37_05_in_memory_repair_preview"] = True
    metadata["not_runtime_deck"] = True
    metadata["not_auto_injected"] = True
    return revised


def _replace_recipe(recipe_report: dict[str, Any], revised_recipe: dict[str, Any]) -> dict[str, Any]:
    revised_report = copy.deepcopy(recipe_report)
    for index, recipe in enumerate(revised_report.get("recipe_drafts", [])):
        if recipe.get("recipe_id") == revised_recipe["recipe_id"]:
            revised_report["recipe_drafts"][index] = revised_recipe
            return revised_report
    raise ValueError(f"Recipe not found: {revised_recipe['recipe_id']}")


def _replace_validation(validation_report: dict[str, Any], revised_validation: dict[str, Any]) -> dict[str, Any]:
    revised_report = copy.deepcopy(validation_report)
    for index, validation in enumerate(revised_report.get("recipe_validations", [])):
        if validation.get("recipe_id") == revised_validation["recipe_id"]:
            revised_report["recipe_validations"][index] = revised_validation
            break
    else:
        raise ValueError(f"Validation not found: {revised_validation['recipe_id']}")

    status_counts: dict[str, int] = {}
    issue_counts: dict[str, int] = {}
    validations = revised_report.get("recipe_validations", [])
    for validation in validations:
        status = validation["validation_status"]
        status_counts[status] = status_counts.get(status, 0) + 1
        for issue in validation.get("issues", []):
            issue_counts[issue["code"]] = issue_counts.get(issue["code"], 0) + 1
    revised_report["summary"] = {
        "recipe_count": len(validations),
        "runtime_ready_recipe_count": sum(1 for item in validations if item.get("runtime_ready")),
        "validator_passed_count": status_counts.get("validator_passed", 0),
        "validator_passed_pending_human_acceptance_count": status_counts.get(
            "validator_passed_pending_human_acceptance", 0
        ),
        "invalid_draft_count": status_counts.get("invalid_draft", 0),
        "blocked_by_review_count": status_counts.get("blocked_by_review", 0),
        "missing_card_recipe_count": sum(
            1 for item in validations if any(issue["code"] == "missing_cards" for issue in item.get("issues", []))
        ),
        "copy_limit_violation_recipe_count": sum(
            1
            for item in validations
            if any(issue["code"] == "copy_limit_exceeded" for issue in item.get("issues", []))
        ),
        "slot_gap_recipe_count": sum(
            1 for item in validations if any(issue["code"] == "unfilled_slots" for issue in item.get("issues", []))
        ),
        "trigger_count_mismatch_recipe_count": sum(
            1
            for item in validations
            if any(issue["code"] == "trigger_count_mismatch" for issue in item.get("issues", []))
        ),
        "issue_counts": dict(sorted(issue_counts.items())),
        "ready_for_m37_05_consistency_check": True,
    }
    revised_report["version"] = "M37-05-preview-validation"
    return revised_report


def _issue_codes(validation: dict[str, Any], severity: str | None = None) -> list[str]:
    return [
        issue["code"]
        for issue in validation.get("issues", [])
        if severity is None or issue.get("severity") == severity
    ]


def build_revised_validation_rerun(
    recipe_report: dict[str, Any] | None = None,
    original_validation: dict[str, Any] | None = None,
    combo_report: dict[str, Any] | None = None,
    review_packet: dict[str, Any] | None = None,
    repair_report: dict[str, Any] | None = None,
    mapping_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    recipe_report = recipe_report or load_json(RECIPE_DRAFTS)
    original_validation = original_validation or load_json(ORIGINAL_VALIDATION)
    combo_report = combo_report or load_json(COMBO_LINES)
    review_packet = review_packet or load_json(REVIEW_PACKET)
    repair_report = repair_report or load_json(M37_02_REPAIR)
    mapping_report = mapping_report or load_json(M37_04_MAPPINGS)

    recipe_id = repair_report["summary"]["recipe_id"]
    original_recipe = _find_recipe(recipe_report, recipe_id)
    original_seed_validation = _find_validation(original_validation, recipe_id)
    revised_recipe = _apply_quantity_delta(original_recipe, repair_report["recommended_repair"]["quantity_delta"])
    revised_recipe_report = _replace_recipe(recipe_report, revised_recipe)
    accepted_seed_validation_only = build_validation_report(
        {"selected_target": recipe_report.get("selected_target", {}), "recipe_drafts": [revised_recipe]}
    )
    revised_seed_validation = accepted_seed_validation_only["recipe_validations"][0]
    revised_validation_report = _replace_validation(original_validation, revised_seed_validation)
    revised_consistency_report = build_consistency_report(
        combo_report=combo_report,
        review_packet=review_packet,
        recipe_report=revised_recipe_report,
        validation_report=revised_validation_report,
    )
    line_id = revised_seed_validation["source_line_id"]
    revised_line_consistency = next(
        item for item in revised_consistency_report["consistency_checks"] if item["line_id"] == line_id
    )

    original_blockers = set(_issue_codes(original_seed_validation, "blocker"))
    revised_blockers = set(_issue_codes(revised_seed_validation, "blocker"))
    resolved_blockers = sorted(original_blockers - revised_blockers)

    return {
        "version": "M37-05",
        "description": "In-memory revised recipe validation rerun for accepted seed trigger repair",
        "selected_target": recipe_report.get("selected_target", {}),
        "source_inputs": {
            "deck_recipe_draft_model": str(RECIPE_DRAFTS.relative_to(ROOT)),
            "original_deck_recipe_validation_report": str(ORIGINAL_VALIDATION.relative_to(ROOT)),
            "combo_line_explainer": str(COMBO_LINES.relative_to(ROOT)),
            "first_slice_review_packet": str(REVIEW_PACKET.relative_to(ROOT)),
            "trigger_package_repair_proposal": str(M37_02_REPAIR.relative_to(ROOT)),
            "manual_semantic_mapping_candidates": str(M37_04_MAPPINGS.relative_to(ROOT)),
        },
        "scope": {
            "offline_in_memory_rerun": True,
            "changes_recipe_draft_file": False,
            "creates_runtime_deck": False,
            "bot_integration": False,
            "automatic_playbook_promotion": False,
            "live_card_text_parsing": False,
        },
        "repair_applied_in_memory": {
            "recipe_id": recipe_id,
            "package_id": repair_report["summary"]["recommended_package_id"],
            "profile_id": repair_report["summary"]["recommended_profile_id"],
            "quantity_delta": repair_report["recommended_repair"]["quantity_delta"],
            "mapping_candidate_count": mapping_report["summary"]["mapping_candidate_count"],
            "human_acceptance_present": False,
        },
        "accepted_seed_before": {
            "validation_status": original_seed_validation["validation_status"],
            "blocking_issue_count": original_seed_validation["blocking_issue_count"],
            "blocker_codes": sorted(original_blockers),
            "review_codes": _issue_codes(original_seed_validation, "review"),
            "count_summary": original_seed_validation["count_summary"],
        },
        "accepted_seed_after": {
            "validation_status": revised_seed_validation["validation_status"],
            "blocking_issue_count": revised_seed_validation["blocking_issue_count"],
            "blocker_codes": sorted(revised_blockers),
            "review_codes": _issue_codes(revised_seed_validation, "review"),
            "count_summary": revised_seed_validation["count_summary"],
            "runtime_ready": revised_seed_validation["runtime_ready"],
            "consistency_status": revised_line_consistency["consistency_status"],
            "promotion_allowed": revised_line_consistency["promotion_allowed"],
        },
        "validation_summary_after": revised_validation_report["summary"],
        "consistency_summary_after": revised_consistency_report["summary"],
        "summary": {
            "recipe_id": recipe_id,
            "resolved_blocker_count": len(resolved_blockers),
            "resolved_blockers": resolved_blockers,
            "remaining_blocker_count": len(revised_blockers),
            "remaining_review_issue_count": len(_issue_codes(revised_seed_validation, "review")),
            "accepted_seed_validation_status_after": revised_seed_validation["validation_status"],
            "accepted_seed_consistency_status_after": revised_line_consistency["consistency_status"],
            "runtime_promotion_allowed": False,
            "ready_for_m37_closeout": True,
        },
        "next_target": {
            "milestone": "M37-closeout",
            "task": "First runtime-ready recipe decision",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    after = report["accepted_seed_after"]
    lines = [
        "# M37-05 Revised Recipe Validation Rerun",
        "",
        "## Summary",
        "",
        f"- Recipe: `{summary['recipe_id']}`",
        f"- Resolved blockers: `{summary['resolved_blockers']}`",
        f"- Remaining blockers: `{summary['remaining_blocker_count']}`",
        f"- Remaining review issues: `{summary['remaining_review_issue_count']}`",
        f"- Validation status after: `{summary['accepted_seed_validation_status_after']}`",
        f"- Consistency status after: `{summary['accepted_seed_consistency_status_after']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M37-closeout: `{summary['ready_for_m37_closeout']}`",
        "",
        "## Accepted Seed After",
        "",
        f"- Trigger counts: `{after['count_summary']['trigger_counts']}`",
        f"- Grade counts: `{after['count_summary']['grade_counts']}`",
        f"- Review codes: `{after['review_codes']}`",
        "",
        "## Policy",
        "",
        "- Rerun is in-memory only.",
        "- Source recipe draft is not modified.",
        "- Runtime promotion remains disabled without human acceptance.",
        "",
        "## Next",
        "",
        "`M37-closeout`: First runtime-ready recipe decision.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M37-05 revised recipe validation rerun.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_revised_validation_rerun()
    json_path = args.output_dir / "m37_05_revised_recipe_validation_rerun.json"
    md_path = args.output_dir / "m37_05_revised_recipe_validation_rerun.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M37-05 revised recipe validation rerun wrote {json_path}")
    print(f"M37-05 revised recipe validation rerun summary wrote {md_path}")
    print(
        "ready_for_m37_closeout={ready} status_after={status} resolved_blockers={resolved}".format(
            ready=report["summary"]["ready_for_m37_closeout"],
            status=report["summary"]["accepted_seed_validation_status_after"],
            resolved=report["summary"]["resolved_blocker_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m37_closeout"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
