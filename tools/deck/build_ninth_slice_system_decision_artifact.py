"""Build the M69-04 ninth-slice G Zone / Stride / Aqua Force decision artifact."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M69_03_ACCEPTANCE = OUTPUT_DIR / "m69_03_ninth_slice_human_accepted_repair_artifact.json"


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
        "ready_for_m69_05": True,
        "rationale": [
            "Current Windows fixture scope validates the 50-card main deck only.",
            "G Zone deck slots, G-unit visibility, and Generation Break runtime support are not implemented yet.",
            "The repaired main deck can be revalidated without pretending that G Zone runtime exists.",
        ],
    },
    "defer_until_g_zone_runtime_exists": {
        "label": "Defer until G Zone runtime exists",
        "decision_status": "deferred_until_g_zone_support",
        "main_deck_only_validation_allowed": False,
        "boundary_resolves_g_zone_deferred_for_main_deck_validation": False,
        "ready_for_m69_05": False,
        "rationale": [
            "The recipe can remain advisory until complete G Zone deck slots and G-unit visibility exist.",
            "No repaired validation or runtime fixture should be created while the G Zone boundary is unresolved.",
        ],
    },
}


STRIDE_OPTION_POLICIES: dict[str, dict[str, Any]] = {
    "main_deck_only_review_no_runtime_promotion": {
        "label": "Main-deck-only review, no runtime promotion",
        "decision_status": "selected_for_main_deck_only_validation",
        "main_deck_only_validation_allowed": True,
        "boundary_resolves_stride_deferred_for_main_deck_validation": True,
        "ready_for_m69_05": True,
        "rationale": [
            "Current Windows fixture scope validates the repaired main deck only.",
            "Stride declaration timing, cost validation, heart-card state, and end-phase return are not implemented yet.",
            "The repaired main deck can be revalidated without enabling Stride runtime.",
        ],
    },
    "defer_until_stride_runtime_exists": {
        "label": "Defer until Stride runtime exists",
        "decision_status": "deferred_until_stride_support",
        "main_deck_only_validation_allowed": False,
        "boundary_resolves_stride_deferred_for_main_deck_validation": False,
        "ready_for_m69_05": False,
        "rationale": [
            "The recipe can remain advisory until complete Stride timing and G-unit interaction support exists.",
            "No repaired validation or runtime fixture should be created while the Stride boundary is unresolved.",
        ],
    },
}


AQUA_FORCE_OPTION_POLICIES: dict[str, dict[str, Any]] = {
    "manual_semantic_review_only_no_runtime_promotion": {
        "label": "Manual semantic review only, no runtime promotion",
        "decision_status": "selected_for_manual_semantic_main_deck_validation",
        "main_deck_only_validation_allowed": True,
        "boundary_resolves_aqua_force_battle_order_deferred_for_main_deck_validation": True,
        "ready_for_m69_05": True,
        "rationale": [
            "Aqua Force battle-order text remains manual semantic evidence in the current Windows fixture scope.",
            "Battle-count tracking, attack-order predicates, and multi-attack labels are not implemented yet.",
            "The repaired main deck can be revalidated without enabling Aqua Force battle-order runtime.",
        ],
    },
    "defer_until_aqua_force_battle_order_runtime_exists": {
        "label": "Defer until Aqua Force battle-order runtime exists",
        "decision_status": "deferred_until_aqua_force_battle_order_support",
        "main_deck_only_validation_allowed": False,
        "boundary_resolves_aqua_force_battle_order_deferred_for_main_deck_validation": False,
        "ready_for_m69_05": False,
        "rationale": [
            "The recipe can remain advisory until battle-count tracking and attack-order predicates exist.",
            "No repaired validation or runtime fixture should be created while the Aqua Force battle-order boundary is unresolved.",
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


def _require_option_available(
    accepted_artifact: dict[str, Any],
    *,
    selected_option: str,
    option_key: str,
    policies: dict[str, dict[str, Any]],
    label: str,
) -> None:
    if selected_option not in policies:
        raise ValueError(f"Unsupported M69-04 {label} decision option: {selected_option}")
    available = _available_option_ids(accepted_artifact, option_key)
    if selected_option not in available:
        raise ValueError(f"Selected M69-04 {label} option is not present in the accepted artifact: {selected_option}")


def _next_target(ready_for_m69_05: bool) -> dict[str, str]:
    if ready_for_m69_05:
        return {
            "milestone": "M69-05",
            "task": "Ninth-slice repaired recipe validation rerun",
        }
    return {
        "milestone": "M69-closeout",
        "task": "Ninth-slice advisory closeout",
    }


def build_ninth_slice_system_decision_artifact(
    accepted_artifact: dict[str, Any] | None = None,
    *,
    selected_g_zone_option: str,
    selected_stride_option: str,
    selected_aqua_force_option: str,
) -> dict[str, Any]:
    accepted_artifact = accepted_artifact or load_json(M69_03_ACCEPTANCE)
    _require_option_available(
        accepted_artifact,
        selected_option=selected_g_zone_option,
        option_key="g_zone_decision_options",
        policies=G_ZONE_OPTION_POLICIES,
        label="G Zone",
    )
    _require_option_available(
        accepted_artifact,
        selected_option=selected_stride_option,
        option_key="stride_decision_options",
        policies=STRIDE_OPTION_POLICIES,
        label="Stride",
    )
    _require_option_available(
        accepted_artifact,
        selected_option=selected_aqua_force_option,
        option_key="aqua_force_decision_options",
        policies=AQUA_FORCE_OPTION_POLICIES,
        label="Aqua Force battle-order",
    )

    g_policy = G_ZONE_OPTION_POLICIES[selected_g_zone_option]
    stride_policy = STRIDE_OPTION_POLICIES[selected_stride_option]
    aqua_policy = AQUA_FORCE_OPTION_POLICIES[selected_aqua_force_option]
    summary = accepted_artifact.get("summary", {})
    repair = accepted_artifact.get("accepted_repair", {})

    main_deck_only_validation_allowed = (
        bool(g_policy["ready_for_m69_05"])
        and bool(stride_policy["ready_for_m69_05"])
        and bool(aqua_policy["ready_for_m69_05"])
    )
    ready_for_m69_05 = (
        main_deck_only_validation_allowed
        and bool(summary.get("ready_for_m69_04"))
        and bool(summary.get("human_selection_recorded"))
        and bool(summary.get("human_acceptance_recorded"))
        and bool(summary.get("repair_accepted"))
        and bool(repair.get("g_zone_deferred"))
        and bool(repair.get("stride_deferred"))
        and bool(repair.get("aqua_force_battle_order_deferred"))
        and int(summary.get("repair_application_issue_count", -1)) == 0
        and not bool(summary.get("runtime_promotion_allowed"))
    )
    suppress_issue_codes = (
        [
            "g_zone_support_deferred",
            "stride_support_deferred",
            "aqua_force_battle_order_support_deferred",
        ]
        if ready_for_m69_05
        else []
    )

    return {
        "version": "M69-04",
        "description": "Ninth-slice G Zone, Stride, and Aqua Force decision artifact",
        "selected_target": accepted_artifact.get("selected_target", {}),
        "source_inputs": {
            "m69_03_human_accepted_repair_artifact": str(M69_03_ACCEPTANCE.relative_to(ROOT)),
        },
        "scope": {
            "offline_boundary_decision": True,
            "records_human_selection": False,
            "records_human_acceptance": False,
            "records_repair_acceptance": False,
            "records_g_zone_decision": True,
            "records_stride_decision": True,
            "records_aqua_force_battle_order_decision": True,
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
            "stride": {
                "selected_option_id": selected_stride_option,
                "label": stride_policy["label"],
                "decision_status": stride_policy["decision_status"],
                "rationale": list(stride_policy["rationale"]),
            },
            "aqua_force_battle_order": {
                "selected_option_id": selected_aqua_force_option,
                "label": aqua_policy["label"],
                "decision_status": aqua_policy["decision_status"],
                "rationale": list(aqua_policy["rationale"]),
            },
            "recommendation": (
                "continue_to_m69_05_repaired_validation_rerun"
                if ready_for_m69_05
                else "keep_ninth_slice_advisory_until_deferred_runtime_support"
            ),
        },
        "boundary_policy": {
            "main_deck_only_validation_allowed": main_deck_only_validation_allowed,
            "boundary_resolves_g_zone_deferred_for_main_deck_validation": bool(
                g_policy["boundary_resolves_g_zone_deferred_for_main_deck_validation"]
            ),
            "boundary_resolves_stride_deferred_for_main_deck_validation": bool(
                stride_policy["boundary_resolves_stride_deferred_for_main_deck_validation"]
            ),
            "boundary_resolves_aqua_force_battle_order_deferred_for_main_deck_validation": bool(
                aqua_policy["boundary_resolves_aqua_force_battle_order_deferred_for_main_deck_validation"]
            ),
            "runtime_promotion_allowed": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "aqua_force_battle_order_runtime_enabled": False,
            "g_zone_slot_model_enabled": False,
            "stride_deck_building_validation_enabled": False,
            "generation_break_runtime_enabled": False,
            "battle_count_tracker_enabled": False,
            "attack_order_predicate_runtime_enabled": False,
            "multi_attack_label_runtime_enabled": False,
            "grade4_main_deck_allowed": False,
            "g_units_allowed_in_main_deck": False,
            "g_zone_cards_allowed_in_current_windows_fixture": False,
            "m69_05_must_rerun_main_deck_validation": ready_for_m69_05,
            "runtime_gate_must_remain_blocked_until_validation": True,
        },
        "m69_05_validation_policy": {
            "validation_scope": "main_deck_only" if ready_for_m69_05 else "advisory_only",
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
                "human_selection_missing",
                "human_acceptance_missing",
            ],
            "must_keep_grade4_cards_out_of_main_deck": True,
            "must_keep_g_units_out_of_runtime_fixture": True,
            "must_not_enable_g_zone_or_stride_runtime": True,
            "must_not_enable_aqua_force_battle_order_runtime": True,
            "must_not_create_runtime_fixture_before_validation": True,
        },
        "decision_item": {
            "decision_item_id": f"m69_04_{summary.get('selected_recipe_id', '')}_system_boundary_decision",
            "selected_review_item_id": summary.get("selected_review_item_id", ""),
            "selected_recipe_id": summary.get("selected_recipe_id", ""),
            "accepted_combined_repair_package_id": summary.get("accepted_combined_repair_package_id", ""),
            "selected_g_zone_package_id": summary.get("selected_g_zone_package_id", ""),
            "selected_stride_package_id": summary.get("selected_stride_package_id", ""),
            "selected_aqua_force_battle_order_package_id": summary.get(
                "selected_aqua_force_battle_order_package_id", ""
            ),
            "selected_g_zone_option_id": selected_g_zone_option,
            "selected_stride_option_id": selected_stride_option,
            "selected_aqua_force_option_id": selected_aqua_force_option,
            "g_zone_decision_status": g_policy["decision_status"],
            "stride_decision_status": stride_policy["decision_status"],
            "aqua_force_battle_order_decision_status": aqua_policy["decision_status"],
            "main_deck_only_validation_allowed": main_deck_only_validation_allowed,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "aqua_force_battle_order_runtime_enabled": False,
            "runtime_promotion_allowed": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "requires_validation_rerun": ready_for_m69_05,
            "g_zone_future_system_work": list(repair.get("g_zone_future_system_work", [])),
            "stride_future_system_work": list(repair.get("stride_future_system_work", [])),
            "aqua_force_battle_order_future_system_work": list(
                repair.get("aqua_force_battle_order_future_system_work", [])
            ),
        },
        "summary": {
            "selected_review_item_id": summary.get("selected_review_item_id", ""),
            "selected_recipe_id": summary.get("selected_recipe_id", ""),
            "accepted_combined_repair_package_id": summary.get("accepted_combined_repair_package_id", ""),
            "selected_g_zone_package_id": summary.get("selected_g_zone_package_id", ""),
            "selected_stride_package_id": summary.get("selected_stride_package_id", ""),
            "selected_aqua_force_battle_order_package_id": summary.get(
                "selected_aqua_force_battle_order_package_id", ""
            ),
            "selected_g_zone_option_id": selected_g_zone_option,
            "selected_stride_option_id": selected_stride_option,
            "selected_aqua_force_option_id": selected_aqua_force_option,
            "main_deck_only_validation_allowed": main_deck_only_validation_allowed,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "aqua_force_battle_order_runtime_enabled": False,
            "runtime_promotion_allowed": False,
            "human_selection_recorded": bool(summary.get("human_selection_recorded")),
            "human_acceptance_recorded": bool(summary.get("human_acceptance_recorded")),
            "repair_accepted": bool(summary.get("repair_accepted")),
            "g_zone_decision_recorded": True,
            "stride_decision_recorded": True,
            "aqua_force_battle_order_decision_recorded": True,
            "accepted_artifact_ready_for_m69_04": bool(summary.get("ready_for_m69_04")),
            "ready_for_m69_05": ready_for_m69_05,
        },
        "next_target": _next_target(ready_for_m69_05),
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    boundary = report["boundary_policy"]
    validation = report["m69_05_validation_policy"]
    item = report["decision_item"]
    lines = [
        "# M69-04 Ninth-Slice G Zone / Stride / Aqua Force Decision Artifact",
        "",
        "## Summary",
        "",
        f"- Selected review item: `{summary['selected_review_item_id']}`",
        f"- Selected recipe: `{summary['selected_recipe_id']}`",
        f"- Accepted repair package: `{summary['accepted_combined_repair_package_id']}`",
        f"- Selected G Zone package: `{summary['selected_g_zone_package_id']}`",
        f"- Selected Stride package: `{summary['selected_stride_package_id']}`",
        f"- Selected Aqua Force package: `{summary['selected_aqua_force_battle_order_package_id']}`",
        f"- Selected G Zone option: `{summary['selected_g_zone_option_id']}`",
        f"- Selected Stride option: `{summary['selected_stride_option_id']}`",
        f"- Selected Aqua Force option: `{summary['selected_aqua_force_option_id']}`",
        f"- Main-deck-only validation allowed: `{summary['main_deck_only_validation_allowed']}`",
        f"- G Zone runtime enabled: `{summary['g_zone_runtime_enabled']}`",
        f"- Stride runtime enabled: `{summary['stride_runtime_enabled']}`",
        f"- Aqua Force battle-order runtime enabled: `{summary['aqua_force_battle_order_runtime_enabled']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M69-05: `{summary['ready_for_m69_05']}`",
        "",
        "## Decision",
        "",
        f"- G Zone status: `{decision['g_zone']['decision_status']}`",
        f"- Stride status: `{decision['stride']['decision_status']}`",
        f"- Aqua Force status: `{decision['aqua_force_battle_order']['decision_status']}`",
        f"- Recommendation: `{decision['recommendation']}`",
        "",
        "## Boundary Policy",
        "",
        f"- Validation scope: `{validation['validation_scope']}`",
        f"- Suppressed issue codes after decision: `{validation['may_suppress_issue_codes_after_boundary_decision']}`",
        f"- G Zone slot model enabled: `{boundary['g_zone_slot_model_enabled']}`",
        f"- Stride deck-building validation enabled: `{boundary['stride_deck_building_validation_enabled']}`",
        f"- Generation Break runtime enabled: `{boundary['generation_break_runtime_enabled']}`",
        f"- Battle-count tracker enabled: `{boundary['battle_count_tracker_enabled']}`",
        f"- Attack-order predicate runtime enabled: `{boundary['attack_order_predicate_runtime_enabled']}`",
        f"- Multi-attack label runtime enabled: `{boundary['multi_attack_label_runtime_enabled']}`",
        f"- Grade 4 main-deck allowed: `{boundary['grade4_main_deck_allowed']}`",
        f"- G units allowed in main deck: `{boundary['g_units_allowed_in_main_deck']}`",
        "",
        "## Decision Item",
        "",
        f"- Decision item: `{item['decision_item_id']}`",
        f"- Future G Zone work: `{item['g_zone_future_system_work']}`",
        f"- Future Stride work: `{item['stride_future_system_work']}`",
        f"- Future Aqua Force work: `{item['aqua_force_battle_order_future_system_work']}`",
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
        description="Build M69-04 ninth-slice G Zone / Stride / Aqua Force decision artifact."
    )
    parser.add_argument("--g-zone-option", required=True, choices=sorted(G_ZONE_OPTION_POLICIES))
    parser.add_argument("--stride-option", required=True, choices=sorted(STRIDE_OPTION_POLICIES))
    parser.add_argument("--aqua-force-option", required=True, choices=sorted(AQUA_FORCE_OPTION_POLICIES))
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_ninth_slice_system_decision_artifact(
        selected_g_zone_option=args.g_zone_option,
        selected_stride_option=args.stride_option,
        selected_aqua_force_option=args.aqua_force_option,
    )
    json_path = args.output_dir / "m69_04_ninth_slice_system_decision_artifact.json"
    md_path = args.output_dir / "m69_04_ninth_slice_system_decision_artifact.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M69-04 ninth-slice system decision artifact wrote {json_path}")
    print(f"M69-04 ninth-slice system decision artifact summary wrote {md_path}")
    print(
        "ready_for_m69_05={ready} selected_recipe={recipe}".format(
            ready=report["summary"]["ready_for_m69_05"],
            recipe=report["summary"]["selected_recipe_id"],
        )
    )
    return 0 if report["summary"]["ready_for_m69_05"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
