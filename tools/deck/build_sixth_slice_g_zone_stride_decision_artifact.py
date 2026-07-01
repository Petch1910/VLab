"""Build the M57-04 sixth-slice G Zone / Stride decision artifact."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M57_03_ACCEPTANCE = OUTPUT_DIR / "m57_03_sixth_slice_human_accepted_repair_artifact.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


OPTION_POLICIES: dict[str, dict[str, Any]] = {
    "main_deck_only_review_no_runtime_promotion": {
        "label": "Main-deck-only review, no runtime promotion",
        "decision_status": "selected_for_main_deck_only_validation",
        "recommendation": "continue_to_m57_05_repaired_validation_rerun",
        "main_deck_only_validation_allowed": True,
        "boundary_resolves_g_zone_deferred_for_main_deck_validation": True,
        "ready_for_m57_05": True,
        "next_target": {
            "milestone": "M57-05",
            "task": "Sixth-slice repaired recipe validation rerun",
        },
        "rationale": [
            "Current Windows fixture scope validates main-deck recipes only.",
            "G Zone slots, Stride timing, G-unit visibility, and Generation Break runtime support are not implemented yet.",
            "The accepted main deck can be revalidated without pretending that G Zone runtime exists.",
        ],
    },
    "defer_until_g_zone_runtime_exists": {
        "label": "Defer until G Zone runtime exists",
        "decision_status": "deferred_until_g_zone_support",
        "recommendation": "keep_sixth_slice_advisory_until_g_zone_support",
        "main_deck_only_validation_allowed": False,
        "boundary_resolves_g_zone_deferred_for_main_deck_validation": False,
        "ready_for_m57_05": False,
        "next_target": {
            "milestone": "M57-closeout",
            "task": "Sixth-slice advisory closeout",
        },
        "rationale": [
            "The recipe uses G-era evidence and can wait for a complete G Zone / Stride implementation.",
            "No repaired validation or runtime fixture should be created while the G Zone boundary is unresolved.",
        ],
    },
}


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _available_option_ids(accepted_artifact: dict[str, Any]) -> set[str]:
    return {
        str(option.get("option_id"))
        for option in accepted_artifact.get("accepted_repair", {}).get("g_zone_decision_options", [])
        if option.get("option_id")
    }


def _require_selected_option_available(accepted_artifact: dict[str, Any], selected_option: str) -> None:
    if selected_option not in OPTION_POLICIES:
        raise ValueError(f"Unsupported M57-04 G Zone decision option: {selected_option}")
    available = _available_option_ids(accepted_artifact)
    if selected_option not in available:
        raise ValueError(f"Selected M57-04 option is not present in the accepted artifact: {selected_option}")


def build_sixth_slice_g_zone_stride_decision_artifact(
    accepted_artifact: dict[str, Any] | None = None,
    *,
    selected_option: str,
) -> dict[str, Any]:
    accepted_artifact = accepted_artifact or load_json(M57_03_ACCEPTANCE)
    _require_selected_option_available(accepted_artifact, selected_option)
    policy = OPTION_POLICIES[selected_option]
    summary = accepted_artifact.get("summary", {})
    repair = accepted_artifact.get("accepted_repair", {})
    ready_for_m57_05 = (
        bool(policy["ready_for_m57_05"])
        and bool(summary.get("ready_for_m57_04"))
        and bool(summary.get("human_acceptance_recorded"))
        and bool(repair.get("g_zone_deferred"))
        and not bool(summary.get("runtime_promotion_allowed"))
    )

    return {
        "version": "M57-04",
        "description": "Sixth-slice G Zone / Stride decision artifact",
        "selected_target": accepted_artifact.get("selected_target", {}),
        "source_inputs": {
            "m57_03_human_accepted_repair_artifact": str(M57_03_ACCEPTANCE.relative_to(ROOT)),
        },
        "scope": {
            "offline_boundary_decision": True,
            "records_human_selection": False,
            "records_human_acceptance": False,
            "records_g_zone_decision": True,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
            "declares_recipe_valid": False,
        },
        "decision": {
            "selected_option_id": selected_option,
            "label": policy["label"],
            "decision_status": policy["decision_status"],
            "recommendation": policy["recommendation"],
            "rationale": list(policy["rationale"]),
        },
        "boundary_policy": {
            "main_deck_only_validation_allowed": bool(policy["main_deck_only_validation_allowed"]),
            "boundary_resolves_g_zone_deferred_for_main_deck_validation": bool(
                policy["boundary_resolves_g_zone_deferred_for_main_deck_validation"]
            ),
            "runtime_promotion_allowed": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "g_zone_slot_model_enabled": False,
            "stride_deck_building_validation_enabled": False,
            "generation_break_runtime_enabled": False,
            "grade4_main_deck_allowed": False,
            "g_units_allowed_in_main_deck": False,
            "g_zone_cards_allowed_in_current_windows_fixture": False,
            "m57_05_must_rerun_main_deck_validation": bool(policy["ready_for_m57_05"]),
            "runtime_gate_must_remain_blocked_until_validation": True,
        },
        "m57_05_validation_policy": {
            "validation_scope": "main_deck_only" if policy["main_deck_only_validation_allowed"] else "advisory_only",
            "may_suppress_issue_codes_after_boundary_decision": (
                ["g_zone_support_deferred"] if policy["main_deck_only_validation_allowed"] else []
            ),
            "must_still_enforce_issue_codes": [
                "main_deck_count",
                "missing_card",
                "copy_limit",
                "trigger_count",
                "grade_profile_outside_target",
                "manual_review_overlap",
                "clan_or_format_mismatch",
            ],
            "must_keep_grade4_cards_out_of_main_deck": True,
            "must_keep_g_units_out_of_runtime_fixture": True,
        },
        "decision_item": {
            "decision_item_id": f"m57_04_{summary.get('accepted_recipe_id', '')}_g_zone_stride_decision",
            "accepted_review_item_id": summary.get("accepted_review_item_id", ""),
            "accepted_recipe_id": summary.get("accepted_recipe_id", ""),
            "accepted_g_zone_package_id": summary.get("accepted_g_zone_package_id", ""),
            "selected_option_id": selected_option,
            "decision_status": policy["decision_status"],
            "main_deck_only_validation_allowed": bool(policy["main_deck_only_validation_allowed"]),
            "boundary_resolves_g_zone_deferred_for_main_deck_validation": bool(
                policy["boundary_resolves_g_zone_deferred_for_main_deck_validation"]
            ),
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "grade4_main_deck_allowed": False,
            "g_units_allowed_in_main_deck": False,
            "runtime_promotion_allowed": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "requires_validation_rerun": bool(policy["ready_for_m57_05"]),
            "g_zone_future_system_work": list(repair.get("g_zone_future_system_work", [])),
        },
        "summary": {
            "accepted_review_item_id": summary.get("accepted_review_item_id", ""),
            "accepted_recipe_id": summary.get("accepted_recipe_id", ""),
            "accepted_g_zone_package_id": summary.get("accepted_g_zone_package_id", ""),
            "selected_option_id": selected_option,
            "main_deck_only_validation_allowed": bool(policy["main_deck_only_validation_allowed"]),
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "runtime_promotion_allowed": False,
            "human_selection_recorded": bool(summary.get("human_selection_recorded")),
            "human_acceptance_recorded": bool(summary.get("human_acceptance_recorded")),
            "g_zone_decision_recorded": True,
            "accepted_artifact_ready_for_m57_04": bool(summary.get("ready_for_m57_04")),
            "ready_for_m57_05": ready_for_m57_05,
        },
        "next_target": policy["next_target"],
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    boundary = report["boundary_policy"]
    item = report["decision_item"]
    lines = [
        "# M57-04 Sixth-Slice G Zone / Stride Decision Artifact",
        "",
        "## Summary",
        "",
        f"- Accepted review item: `{summary['accepted_review_item_id']}`",
        f"- Accepted recipe: `{summary['accepted_recipe_id']}`",
        f"- Accepted G Zone package: `{summary['accepted_g_zone_package_id']}`",
        f"- Selected option: `{summary['selected_option_id']}`",
        f"- Main-deck-only validation allowed: `{summary['main_deck_only_validation_allowed']}`",
        f"- G Zone runtime enabled: `{summary['g_zone_runtime_enabled']}`",
        f"- Stride runtime enabled: `{summary['stride_runtime_enabled']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M57-05: `{summary['ready_for_m57_05']}`",
        "",
        "## Decision",
        "",
        f"- Status: `{decision['decision_status']}`",
        f"- Recommendation: `{decision['recommendation']}`",
        "",
    ]
    for reason in decision["rationale"]:
        lines.append(f"- {reason}")
    lines.extend(
        [
            "",
            "## Boundary Policy",
            "",
            f"- Validation scope: `{report['m57_05_validation_policy']['validation_scope']}`",
            f"- G Zone slot model enabled: `{boundary['g_zone_slot_model_enabled']}`",
            f"- Stride deck-building validation enabled: `{boundary['stride_deck_building_validation_enabled']}`",
            f"- Generation Break runtime enabled: `{boundary['generation_break_runtime_enabled']}`",
            f"- Grade 4 main-deck allowed: `{boundary['grade4_main_deck_allowed']}`",
            f"- G units allowed in main deck: `{boundary['g_units_allowed_in_main_deck']}`",
            "",
            "## Decision Item",
            "",
            f"- Decision item: `{item['decision_item_id']}`",
            f"- Selected option: `{item['selected_option_id']}`",
            f"- Future G Zone work: `{item['g_zone_future_system_work']}`",
            "",
            "## Next",
            "",
            f"`{report['next_target']['milestone']}`: {report['next_target']['task']}.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M57-04 sixth-slice G Zone / Stride decision artifact.")
    parser.add_argument("--selected-option", required=True, choices=sorted(OPTION_POLICIES))
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_sixth_slice_g_zone_stride_decision_artifact(selected_option=args.selected_option)
    json_path = args.output_dir / "m57_04_sixth_slice_g_zone_stride_decision_artifact.json"
    md_path = args.output_dir / "m57_04_sixth_slice_g_zone_stride_decision_artifact.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M57-04 sixth-slice G Zone / Stride decision artifact wrote {json_path}")
    print(f"M57-04 sixth-slice G Zone / Stride decision artifact summary wrote {md_path}")
    print(
        "ready_for_m57_05={ready} accepted_recipe={recipe}".format(
            ready=report["summary"]["ready_for_m57_05"],
            recipe=report["summary"]["accepted_recipe_id"],
        )
    )
    return 0 if report["summary"]["ready_for_m57_05"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
