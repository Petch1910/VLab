"""Build the M65-04 eighth-slice Lock / Legion decision artifact."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M65_03_ACCEPTANCE = OUTPUT_DIR / "m65_03_eighth_slice_human_accepted_grade_repair_artifact.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


LOCK_OPTION_POLICIES: dict[str, dict[str, Any]] = {
    "main_deck_only_review_no_runtime_promotion": {
        "label": "Main-deck-only review, no runtime promotion",
        "decision_status": "selected_for_main_deck_only_validation",
        "main_deck_only_validation_allowed": True,
        "boundary_resolves_lock_deferred_for_main_deck_validation": True,
        "ready_for_m65_05": True,
        "rationale": [
            "Current Windows fixture scope validates the 50-card main deck only.",
            "Lock/Unlock circle state, locked-card visibility, unlock timing, and related runtime rules are not implemented yet.",
            "The accepted main deck can be revalidated without pretending that Lock/Unlock runtime exists.",
        ],
    },
    "defer_until_lock_unlock_runtime_exists": {
        "label": "Defer until Lock/Unlock runtime exists",
        "decision_status": "deferred_until_lock_unlock_support",
        "main_deck_only_validation_allowed": False,
        "boundary_resolves_lock_deferred_for_main_deck_validation": False,
        "ready_for_m65_05": False,
        "rationale": [
            "The recipe can remain advisory until a complete Lock/Unlock runtime implementation exists.",
            "No repaired validation or runtime fixture should be created while the Lock/Unlock boundary is unresolved.",
        ],
    },
}


LEGION_OPTION_POLICIES: dict[str, dict[str, Any]] = {
    "main_deck_only_review_no_runtime_promotion": {
        "label": "Main-deck-only review, no runtime promotion",
        "decision_status": "selected_for_main_deck_only_validation",
        "main_deck_only_validation_allowed": True,
        "boundary_resolves_legion_deferred_for_main_deck_validation": True,
        "ready_for_m65_05": True,
        "rationale": [
            "Current Windows fixture scope validates the 50-card main deck only.",
            "Legion declaration, Mate identity checks, Legion state, and Legion attack/runtime timing are not implemented yet.",
            "The accepted main deck can be revalidated without pretending that Legion/Mate runtime exists.",
        ],
    },
    "defer_until_legion_mate_runtime_exists": {
        "label": "Defer until Legion/Mate runtime exists",
        "decision_status": "deferred_until_legion_mate_support",
        "main_deck_only_validation_allowed": False,
        "boundary_resolves_legion_deferred_for_main_deck_validation": False,
        "ready_for_m65_05": False,
        "rationale": [
            "The recipe can remain advisory until a complete Legion/Mate runtime implementation exists.",
            "No repaired validation or runtime fixture should be created while the Legion/Mate boundary is unresolved.",
        ],
    },
}


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _available_option_ids(accepted_artifact: dict[str, Any], key: str) -> set[str]:
    return {
        str(option.get("option_id"))
        for option in accepted_artifact.get("accepted_repair", {}).get(key, [])
        if option.get("option_id")
    }


def _require_lock_option_available(accepted_artifact: dict[str, Any], selected_option: str) -> None:
    if selected_option not in LOCK_OPTION_POLICIES:
        raise ValueError(f"Unsupported M65-04 Lock/Unlock decision option: {selected_option}")
    available = _available_option_ids(accepted_artifact, "lock_decision_options")
    if selected_option not in available:
        raise ValueError(f"Selected M65-04 Lock option is not present in the accepted artifact: {selected_option}")


def _require_legion_option_available(accepted_artifact: dict[str, Any], selected_option: str) -> None:
    if selected_option not in LEGION_OPTION_POLICIES:
        raise ValueError(f"Unsupported M65-04 Legion/Mate decision option: {selected_option}")
    available = _available_option_ids(accepted_artifact, "legion_decision_options")
    if selected_option not in available:
        raise ValueError(f"Selected M65-04 Legion option is not present in the accepted artifact: {selected_option}")


def _next_target(ready_for_m65_05: bool) -> dict[str, str]:
    if ready_for_m65_05:
        return {
            "milestone": "M65-05",
            "task": "Eighth-slice repaired recipe validation rerun",
        }
    return {
        "milestone": "M65-closeout",
        "task": "Eighth-slice advisory closeout",
    }


def build_eighth_slice_lock_legion_decision_artifact(
    accepted_artifact: dict[str, Any] | None = None,
    *,
    selected_lock_option: str,
    selected_legion_option: str,
) -> dict[str, Any]:
    accepted_artifact = accepted_artifact or load_json(M65_03_ACCEPTANCE)
    _require_lock_option_available(accepted_artifact, selected_lock_option)
    _require_legion_option_available(accepted_artifact, selected_legion_option)
    lock_policy = LOCK_OPTION_POLICIES[selected_lock_option]
    legion_policy = LEGION_OPTION_POLICIES[selected_legion_option]
    summary = accepted_artifact.get("summary", {})
    repair = accepted_artifact.get("accepted_repair", {})

    main_deck_only_validation_allowed = bool(lock_policy["ready_for_m65_05"]) and bool(
        legion_policy["ready_for_m65_05"]
    )
    ready_for_m65_05 = (
        main_deck_only_validation_allowed
        and bool(summary.get("ready_for_m65_04"))
        and bool(summary.get("human_selection_recorded"))
        and bool(summary.get("human_acceptance_recorded"))
        and bool(summary.get("grade_repair_accepted"))
        and bool(repair.get("lock_deferred"))
        and bool(repair.get("legion_deferred"))
        and not bool(summary.get("runtime_promotion_allowed"))
    )
    suppress_issue_codes = ["lock_runtime_support_deferred", "legion_runtime_support_deferred"] if ready_for_m65_05 else []

    return {
        "version": "M65-04",
        "description": "Eighth-slice Lock / Legion decision artifact",
        "selected_target": accepted_artifact.get("selected_target", {}),
        "source_inputs": {
            "m65_03_human_accepted_grade_repair_artifact": str(M65_03_ACCEPTANCE.relative_to(ROOT)),
        },
        "scope": {
            "offline_boundary_decision": True,
            "records_human_selection": False,
            "records_human_acceptance": False,
            "records_grade_repair_acceptance": False,
            "records_lock_decision": True,
            "records_legion_decision": True,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
            "declares_recipe_valid": False,
        },
        "decision": {
            "lock": {
                "selected_option_id": selected_lock_option,
                "label": lock_policy["label"],
                "decision_status": lock_policy["decision_status"],
                "rationale": list(lock_policy["rationale"]),
            },
            "legion": {
                "selected_option_id": selected_legion_option,
                "label": legion_policy["label"],
                "decision_status": legion_policy["decision_status"],
                "rationale": list(legion_policy["rationale"]),
            },
            "recommendation": (
                "continue_to_m65_05_repaired_validation_rerun"
                if ready_for_m65_05
                else "keep_eighth_slice_advisory_until_deferred_runtime_support"
            ),
        },
        "boundary_policy": {
            "main_deck_only_validation_allowed": main_deck_only_validation_allowed,
            "boundary_resolves_lock_deferred_for_main_deck_validation": bool(
                lock_policy["boundary_resolves_lock_deferred_for_main_deck_validation"]
            ),
            "boundary_resolves_legion_deferred_for_main_deck_validation": bool(
                legion_policy["boundary_resolves_legion_deferred_for_main_deck_validation"]
            ),
            "runtime_promotion_allowed": False,
            "lock_runtime_enabled": False,
            "unlock_runtime_enabled": False,
            "locked_card_visibility_enabled": False,
            "lock_circle_state_enabled": False,
            "unlock_timing_runtime_enabled": False,
            "legion_runtime_enabled": False,
            "mate_identity_check_enabled": False,
            "legion_state_enabled": False,
            "legion_attack_timing_enabled": False,
            "legion_deck_building_validation_enabled": False,
            "m65_05_must_rerun_main_deck_validation": ready_for_m65_05,
            "runtime_gate_must_remain_blocked_until_validation": True,
        },
        "m65_05_validation_policy": {
            "validation_scope": "main_deck_only" if ready_for_m65_05 else "advisory_only",
            "may_suppress_issue_codes_after_boundary_decision": suppress_issue_codes,
            "must_still_enforce_issue_codes": [
                "main_deck_count",
                "missing_card",
                "copy_limit",
                "trigger_count",
                "heal_trigger_limit",
                "grade_profile_outside_target",
                "human_selection_missing",
                "clan_or_format_mismatch",
            ],
            "must_not_enable_lock_or_unlock_effect_resolution": True,
            "must_not_enable_legion_or_mate_effect_resolution": True,
            "must_not_create_runtime_fixture_before_validation": True,
        },
        "decision_item": {
            "decision_item_id": f"m65_04_{summary.get('selected_recipe_id', '')}_lock_legion_boundary_decision",
            "selected_review_item_id": summary.get("selected_review_item_id", ""),
            "selected_recipe_id": summary.get("selected_recipe_id", ""),
            "accepted_grade_repair_package_id": summary.get("accepted_grade_repair_package_id", ""),
            "selected_lock_package_id": summary.get("selected_lock_package_id", ""),
            "selected_legion_package_id": summary.get("selected_legion_package_id", ""),
            "selected_lock_option_id": selected_lock_option,
            "selected_legion_option_id": selected_legion_option,
            "lock_decision_status": lock_policy["decision_status"],
            "legion_decision_status": legion_policy["decision_status"],
            "main_deck_only_validation_allowed": main_deck_only_validation_allowed,
            "boundary_resolves_lock_deferred_for_main_deck_validation": bool(
                lock_policy["boundary_resolves_lock_deferred_for_main_deck_validation"]
            ),
            "boundary_resolves_legion_deferred_for_main_deck_validation": bool(
                legion_policy["boundary_resolves_legion_deferred_for_main_deck_validation"]
            ),
            "lock_runtime_enabled": False,
            "unlock_runtime_enabled": False,
            "legion_runtime_enabled": False,
            "mate_identity_check_enabled": False,
            "runtime_promotion_allowed": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "requires_validation_rerun": ready_for_m65_05,
            "lock_future_system_work": list(repair.get("lock_future_system_work", [])),
            "legion_future_system_work": list(repair.get("legion_future_system_work", [])),
        },
        "summary": {
            "selected_review_item_id": summary.get("selected_review_item_id", ""),
            "selected_recipe_id": summary.get("selected_recipe_id", ""),
            "accepted_grade_repair_package_id": summary.get("accepted_grade_repair_package_id", ""),
            "selected_lock_package_id": summary.get("selected_lock_package_id", ""),
            "selected_legion_package_id": summary.get("selected_legion_package_id", ""),
            "selected_lock_option_id": selected_lock_option,
            "selected_legion_option_id": selected_legion_option,
            "main_deck_only_validation_allowed": main_deck_only_validation_allowed,
            "lock_runtime_enabled": False,
            "unlock_runtime_enabled": False,
            "legion_runtime_enabled": False,
            "mate_identity_check_enabled": False,
            "runtime_promotion_allowed": False,
            "human_selection_recorded": bool(summary.get("human_selection_recorded")),
            "human_acceptance_recorded": bool(summary.get("human_acceptance_recorded")),
            "grade_repair_accepted": bool(summary.get("grade_repair_accepted")),
            "lock_decision_recorded": True,
            "legion_decision_recorded": True,
            "accepted_artifact_ready_for_m65_04": bool(summary.get("ready_for_m65_04")),
            "ready_for_m65_05": ready_for_m65_05,
        },
        "next_target": _next_target(ready_for_m65_05),
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    boundary = report["boundary_policy"]
    validation = report["m65_05_validation_policy"]
    item = report["decision_item"]
    lines = [
        "# M65-04 Eighth-Slice Lock / Legion Decision Artifact",
        "",
        "## Summary",
        "",
        f"- Selected review item: `{summary['selected_review_item_id']}`",
        f"- Selected recipe: `{summary['selected_recipe_id']}`",
        f"- Accepted grade package: `{summary['accepted_grade_repair_package_id']}`",
        f"- Selected Lock package: `{summary['selected_lock_package_id']}`",
        f"- Selected Legion package: `{summary['selected_legion_package_id']}`",
        f"- Selected Lock option: `{summary['selected_lock_option_id']}`",
        f"- Selected Legion option: `{summary['selected_legion_option_id']}`",
        f"- Main-deck-only validation allowed: `{summary['main_deck_only_validation_allowed']}`",
        f"- Lock runtime enabled: `{summary['lock_runtime_enabled']}`",
        f"- Unlock runtime enabled: `{summary['unlock_runtime_enabled']}`",
        f"- Legion runtime enabled: `{summary['legion_runtime_enabled']}`",
        f"- Mate identity check enabled: `{summary['mate_identity_check_enabled']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M65-05: `{summary['ready_for_m65_05']}`",
        "",
        "## Decision",
        "",
        f"- Lock status: `{decision['lock']['decision_status']}`",
        f"- Legion status: `{decision['legion']['decision_status']}`",
        f"- Recommendation: `{decision['recommendation']}`",
        "",
        "## Boundary Policy",
        "",
        f"- Validation scope: `{validation['validation_scope']}`",
        f"- Suppressed issue codes after decision: `{validation['may_suppress_issue_codes_after_boundary_decision']}`",
        f"- Lock circle state enabled: `{boundary['lock_circle_state_enabled']}`",
        f"- Unlock timing runtime enabled: `{boundary['unlock_timing_runtime_enabled']}`",
        f"- Legion state enabled: `{boundary['legion_state_enabled']}`",
        f"- Legion attack timing enabled: `{boundary['legion_attack_timing_enabled']}`",
        f"- Legion deck-building validation enabled: `{boundary['legion_deck_building_validation_enabled']}`",
        "",
        "## Decision Item",
        "",
        f"- Decision item: `{item['decision_item_id']}`",
        f"- Future Lock work: `{item['lock_future_system_work']}`",
        f"- Future Legion work: `{item['legion_future_system_work']}`",
        "",
        "## Next",
        "",
        f"`{report['next_target']['milestone']}`: {report['next_target']['task']}.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M65-04 eighth-slice Lock / Legion decision artifact.")
    parser.add_argument("--lock-option", required=True, choices=sorted(LOCK_OPTION_POLICIES))
    parser.add_argument("--legion-option", required=True, choices=sorted(LEGION_OPTION_POLICIES))
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_eighth_slice_lock_legion_decision_artifact(
        selected_lock_option=args.lock_option,
        selected_legion_option=args.legion_option,
    )
    json_path = args.output_dir / "m65_04_eighth_slice_lock_legion_decision_artifact.json"
    md_path = args.output_dir / "m65_04_eighth_slice_lock_legion_decision_artifact.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M65-04 eighth-slice Lock / Legion decision artifact wrote {json_path}")
    print(f"M65-04 eighth-slice Lock / Legion decision artifact summary wrote {md_path}")
    print(
        "ready_for_m65_05={ready} selected_recipe={recipe}".format(
            ready=report["summary"]["ready_for_m65_05"],
            recipe=report["summary"]["selected_recipe_id"],
        )
    )
    return 0 if report["summary"]["ready_for_m65_05"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
