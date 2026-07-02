"""Build the M61-04 seventh-slice G Zone / Stride / Bloom-token decision artifact."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M61_03_ACCEPTANCE = OUTPUT_DIR / "m61_03_seventh_slice_human_accepted_repair_artifact.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


G_ZONE_OPTION_POLICIES: dict[str, dict[str, Any]] = {
    "main_deck_only_review_no_runtime_promotion": {
        "label": "Main-deck-only review, no runtime promotion",
        "decision_status": "selected_for_main_deck_only_validation",
        "main_deck_only_validation_allowed": True,
        "boundary_resolves_g_zone_deferred_for_main_deck_validation": True,
        "ready_for_m61_05": True,
        "rationale": [
            "Current Windows fixture scope validates main-deck recipes only.",
            "G Zone slots, Stride timing, G-unit visibility, and Generation Break runtime support are not implemented yet.",
            "The accepted main deck can be revalidated without pretending that G Zone runtime exists.",
        ],
    },
    "defer_until_g_zone_stride_runtime_exists": {
        "label": "Defer until G Zone and Stride runtime exists",
        "decision_status": "deferred_until_g_zone_stride_support",
        "main_deck_only_validation_allowed": False,
        "boundary_resolves_g_zone_deferred_for_main_deck_validation": False,
        "ready_for_m61_05": False,
        "rationale": [
            "The recipe uses G-era evidence and can wait for a complete G Zone / Stride implementation.",
            "No repaired validation or runtime fixture should be created while the G Zone / Stride boundary is unresolved.",
        ],
    },
}


BLOOM_TOKEN_OPTION_POLICIES: dict[str, dict[str, Any]] = {
    "manual_semantic_review_only_no_runtime_promotion": {
        "label": "Manual semantic review only, no runtime promotion",
        "decision_status": "selected_for_manual_semantic_main_deck_validation",
        "main_deck_only_validation_allowed": True,
        "boundary_resolves_bloom_token_deferred_for_main_deck_validation": True,
        "ready_for_m61_05": True,
        "rationale": [
            "Bloom-like and token-like text remains advisory/manual semantic evidence in the current Windows fixture scope.",
            "The accepted main deck can be revalidated without enabling Bloom templates, same-name tracking, or token lifecycle runtime.",
        ],
    },
    "defer_until_bloom_token_runtime_exists": {
        "label": "Defer until Bloom/token runtime exists",
        "decision_status": "deferred_until_bloom_token_support",
        "main_deck_only_validation_allowed": False,
        "boundary_resolves_bloom_token_deferred_for_main_deck_validation": False,
        "ready_for_m61_05": False,
        "rationale": [
            "The recipe can remain advisory until Bloom templates, same-name checks, token lifecycle, and duration cleanup exist.",
            "No repaired validation or runtime fixture should be created while Bloom/token boundaries are unresolved.",
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


def _require_g_zone_option_available(accepted_artifact: dict[str, Any], selected_option: str) -> None:
    if selected_option not in G_ZONE_OPTION_POLICIES:
        raise ValueError(f"Unsupported M61-04 G Zone / Stride decision option: {selected_option}")
    available = _available_option_ids(accepted_artifact, "g_zone_decision_options")
    if selected_option not in available:
        raise ValueError(f"Selected M61-04 G Zone option is not present in the accepted artifact: {selected_option}")


def _require_bloom_token_option_available(accepted_artifact: dict[str, Any], selected_option: str) -> None:
    if selected_option not in BLOOM_TOKEN_OPTION_POLICIES:
        raise ValueError(f"Unsupported M61-04 Bloom/token decision option: {selected_option}")
    available = _available_option_ids(accepted_artifact, "bloom_token_decision_options")
    if selected_option not in available:
        raise ValueError(
            f"Selected M61-04 Bloom/token option is not present in the accepted artifact: {selected_option}"
        )


def _next_target(ready_for_m61_05: bool) -> dict[str, str]:
    if ready_for_m61_05:
        return {
            "milestone": "M61-05",
            "task": "Seventh-slice repaired recipe validation rerun",
        }
    return {
        "milestone": "M61-closeout",
        "task": "Seventh-slice advisory closeout",
    }


def build_seventh_slice_system_decision_artifact(
    accepted_artifact: dict[str, Any] | None = None,
    *,
    selected_g_zone_option: str,
    selected_bloom_token_option: str,
) -> dict[str, Any]:
    accepted_artifact = accepted_artifact or load_json(M61_03_ACCEPTANCE)
    _require_g_zone_option_available(accepted_artifact, selected_g_zone_option)
    _require_bloom_token_option_available(accepted_artifact, selected_bloom_token_option)
    g_policy = G_ZONE_OPTION_POLICIES[selected_g_zone_option]
    bloom_policy = BLOOM_TOKEN_OPTION_POLICIES[selected_bloom_token_option]
    summary = accepted_artifact.get("summary", {})
    repair = accepted_artifact.get("accepted_repair", {})
    main_deck_only_validation_allowed = bool(g_policy["ready_for_m61_05"]) and bool(
        bloom_policy["ready_for_m61_05"]
    )
    ready_for_m61_05 = (
        main_deck_only_validation_allowed
        and bool(summary.get("ready_for_m61_04"))
        and bool(summary.get("human_acceptance_recorded"))
        and bool(repair.get("g_zone_deferred"))
        and bool(repair.get("bloom_token_deferred"))
        and not bool(summary.get("runtime_promotion_allowed"))
    )
    suppress_issue_codes = (
        ["g_zone_support_deferred", "bloom_token_support_deferred"] if ready_for_m61_05 else []
    )

    return {
        "version": "M61-04",
        "description": "Seventh-slice G Zone / Stride / Bloom-token decision artifact",
        "selected_target": accepted_artifact.get("selected_target", {}),
        "source_inputs": {
            "m61_03_human_accepted_repair_artifact": str(M61_03_ACCEPTANCE.relative_to(ROOT)),
        },
        "scope": {
            "offline_boundary_decision": True,
            "records_human_selection": False,
            "records_human_acceptance": False,
            "records_g_zone_decision": True,
            "records_bloom_token_decision": True,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
            "declares_recipe_valid": False,
        },
        "decision": {
            "g_zone": {
                "selected_option_id": selected_g_zone_option,
                "label": g_policy["label"],
                "decision_status": g_policy["decision_status"],
                "rationale": list(g_policy["rationale"]),
            },
            "bloom_token": {
                "selected_option_id": selected_bloom_token_option,
                "label": bloom_policy["label"],
                "decision_status": bloom_policy["decision_status"],
                "rationale": list(bloom_policy["rationale"]),
            },
            "recommendation": (
                "continue_to_m61_05_repaired_validation_rerun"
                if ready_for_m61_05
                else "keep_seventh_slice_advisory_until_deferred_runtime_support"
            ),
        },
        "boundary_policy": {
            "main_deck_only_validation_allowed": main_deck_only_validation_allowed,
            "boundary_resolves_g_zone_deferred_for_main_deck_validation": bool(
                g_policy["boundary_resolves_g_zone_deferred_for_main_deck_validation"]
            ),
            "boundary_resolves_bloom_token_deferred_for_main_deck_validation": bool(
                bloom_policy["boundary_resolves_bloom_token_deferred_for_main_deck_validation"]
            ),
            "runtime_promotion_allowed": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "bloom_token_runtime_enabled": False,
            "g_zone_slot_model_enabled": False,
            "stride_deck_building_validation_enabled": False,
            "generation_break_runtime_enabled": False,
            "bloom_template_runtime_enabled": False,
            "token_lifecycle_runtime_enabled": False,
            "same_name_runtime_tracking_enabled": False,
            "duration_cleanup_runtime_enabled": False,
            "grade4_main_deck_allowed": False,
            "g_units_allowed_in_main_deck": False,
            "g_zone_cards_allowed_in_current_windows_fixture": False,
            "m61_05_must_rerun_main_deck_validation": ready_for_m61_05,
            "runtime_gate_must_remain_blocked_until_validation": True,
        },
        "m61_05_validation_policy": {
            "validation_scope": "main_deck_only" if ready_for_m61_05 else "advisory_only",
            "may_suppress_issue_codes_after_boundary_decision": suppress_issue_codes,
            "must_still_enforce_issue_codes": [
                "main_deck_count",
                "missing_card",
                "copy_limit",
                "trigger_count",
                "heal_trigger_limit",
                "grade_profile_outside_target",
                "manual_review_overlap",
                "clan_or_format_mismatch",
            ],
            "must_keep_grade4_cards_out_of_main_deck": True,
            "must_keep_g_units_out_of_runtime_fixture": True,
            "must_keep_tokens_out_of_main_deck": True,
            "must_not_enable_bloom_or_token_effect_resolution": True,
        },
        "decision_item": {
            "decision_item_id": f"m61_04_{summary.get('accepted_recipe_id', '')}_system_boundary_decision",
            "accepted_review_item_id": summary.get("accepted_review_item_id", ""),
            "accepted_recipe_id": summary.get("accepted_recipe_id", ""),
            "accepted_g_zone_package_id": summary.get("accepted_g_zone_package_id", ""),
            "accepted_bloom_token_package_id": summary.get("accepted_bloom_token_package_id", ""),
            "selected_g_zone_option_id": selected_g_zone_option,
            "selected_bloom_token_option_id": selected_bloom_token_option,
            "g_zone_decision_status": g_policy["decision_status"],
            "bloom_token_decision_status": bloom_policy["decision_status"],
            "main_deck_only_validation_allowed": main_deck_only_validation_allowed,
            "boundary_resolves_g_zone_deferred_for_main_deck_validation": bool(
                g_policy["boundary_resolves_g_zone_deferred_for_main_deck_validation"]
            ),
            "boundary_resolves_bloom_token_deferred_for_main_deck_validation": bool(
                bloom_policy["boundary_resolves_bloom_token_deferred_for_main_deck_validation"]
            ),
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "bloom_token_runtime_enabled": False,
            "grade4_main_deck_allowed": False,
            "g_units_allowed_in_main_deck": False,
            "runtime_promotion_allowed": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "requires_validation_rerun": ready_for_m61_05,
            "g_zone_future_system_work": list(repair.get("g_zone_future_system_work", [])),
            "bloom_token_future_system_work": list(repair.get("bloom_token_future_system_work", [])),
        },
        "summary": {
            "accepted_review_item_id": summary.get("accepted_review_item_id", ""),
            "accepted_recipe_id": summary.get("accepted_recipe_id", ""),
            "accepted_g_zone_package_id": summary.get("accepted_g_zone_package_id", ""),
            "accepted_bloom_token_package_id": summary.get("accepted_bloom_token_package_id", ""),
            "selected_g_zone_option_id": selected_g_zone_option,
            "selected_bloom_token_option_id": selected_bloom_token_option,
            "main_deck_only_validation_allowed": main_deck_only_validation_allowed,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "bloom_token_runtime_enabled": False,
            "runtime_promotion_allowed": False,
            "human_selection_recorded": bool(summary.get("human_selection_recorded")),
            "human_acceptance_recorded": bool(summary.get("human_acceptance_recorded")),
            "g_zone_decision_recorded": True,
            "bloom_token_decision_recorded": True,
            "accepted_artifact_ready_for_m61_04": bool(summary.get("ready_for_m61_04")),
            "ready_for_m61_05": ready_for_m61_05,
        },
        "next_target": _next_target(ready_for_m61_05),
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    boundary = report["boundary_policy"]
    validation = report["m61_05_validation_policy"]
    item = report["decision_item"]
    lines = [
        "# M61-04 Seventh-Slice G Zone / Stride / Bloom-Token Decision Artifact",
        "",
        "## Summary",
        "",
        f"- Accepted review item: `{summary['accepted_review_item_id']}`",
        f"- Accepted recipe: `{summary['accepted_recipe_id']}`",
        f"- Accepted G Zone package: `{summary['accepted_g_zone_package_id']}`",
        f"- Accepted Bloom/token package: `{summary['accepted_bloom_token_package_id']}`",
        f"- Selected G Zone option: `{summary['selected_g_zone_option_id']}`",
        f"- Selected Bloom/token option: `{summary['selected_bloom_token_option_id']}`",
        f"- Main-deck-only validation allowed: `{summary['main_deck_only_validation_allowed']}`",
        f"- G Zone runtime enabled: `{summary['g_zone_runtime_enabled']}`",
        f"- Stride runtime enabled: `{summary['stride_runtime_enabled']}`",
        f"- Bloom/token runtime enabled: `{summary['bloom_token_runtime_enabled']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M61-05: `{summary['ready_for_m61_05']}`",
        "",
        "## Decision",
        "",
        f"- G Zone status: `{decision['g_zone']['decision_status']}`",
        f"- Bloom/token status: `{decision['bloom_token']['decision_status']}`",
        f"- Recommendation: `{decision['recommendation']}`",
        "",
        "## Boundary Policy",
        "",
        f"- Validation scope: `{validation['validation_scope']}`",
        f"- Suppressed issue codes after decision: `{validation['may_suppress_issue_codes_after_boundary_decision']}`",
        f"- G Zone slot model enabled: `{boundary['g_zone_slot_model_enabled']}`",
        f"- Stride deck-building validation enabled: `{boundary['stride_deck_building_validation_enabled']}`",
        f"- Generation Break runtime enabled: `{boundary['generation_break_runtime_enabled']}`",
        f"- Bloom template runtime enabled: `{boundary['bloom_template_runtime_enabled']}`",
        f"- Token lifecycle runtime enabled: `{boundary['token_lifecycle_runtime_enabled']}`",
        f"- Same-name runtime tracking enabled: `{boundary['same_name_runtime_tracking_enabled']}`",
        f"- Duration cleanup runtime enabled: `{boundary['duration_cleanup_runtime_enabled']}`",
        f"- Grade 4 main-deck allowed: `{boundary['grade4_main_deck_allowed']}`",
        f"- G units allowed in main deck: `{boundary['g_units_allowed_in_main_deck']}`",
        "",
        "## Decision Item",
        "",
        f"- Decision item: `{item['decision_item_id']}`",
        f"- Future G Zone work: `{item['g_zone_future_system_work']}`",
        f"- Future Bloom/token work: `{item['bloom_token_future_system_work']}`",
        "",
        "## Next",
        "",
        f"`{report['next_target']['milestone']}`: {report['next_target']['task']}.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Build M61-04 seventh-slice G Zone / Stride / Bloom-token decision artifact."
    )
    parser.add_argument("--g-zone-option", required=True, choices=sorted(G_ZONE_OPTION_POLICIES))
    parser.add_argument("--bloom-token-option", required=True, choices=sorted(BLOOM_TOKEN_OPTION_POLICIES))
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_seventh_slice_system_decision_artifact(
        selected_g_zone_option=args.g_zone_option,
        selected_bloom_token_option=args.bloom_token_option,
    )
    json_path = args.output_dir / "m61_04_seventh_slice_system_decision_artifact.json"
    md_path = args.output_dir / "m61_04_seventh_slice_system_decision_artifact.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M61-04 seventh-slice system decision artifact wrote {json_path}")
    print(f"M61-04 seventh-slice system decision artifact summary wrote {md_path}")
    print(
        "ready_for_m61_05={ready} accepted_recipe={recipe}".format(
            ready=report["summary"]["ready_for_m61_05"],
            recipe=report["summary"]["accepted_recipe_id"],
        )
    )
    return 0 if report["summary"]["ready_for_m61_05"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
