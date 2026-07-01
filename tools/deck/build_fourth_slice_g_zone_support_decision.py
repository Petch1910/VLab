"""Build the M49-02 fourth-slice G Zone support decision artifact."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M49_REVIEW_PACKET = OUTPUT_DIR / "m49_01_fourth_slice_human_repair_review_packet.json"
M48_CLOSEOUT = OUTPUT_DIR / "m48_closeout_fourth_slice_runtime_readiness.json"

DEFAULT_SELECTED_OPTION = "main_deck_only_for_current_windows_fixture"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


OPTION_POLICIES: dict[str, dict[str, Any]] = {
    "main_deck_only_for_current_windows_fixture": {
        "label": "Use as main-deck-only fixture for current Windows scope",
        "decision_status": "selected_for_main_deck_only_validation",
        "recommendation": "continue_to_m49_03_human_acceptance_gate",
        "main_deck_only_validation_allowed": True,
        "boundary_resolves_g_zone_deferred_for_main_deck_validation": True,
        "recipe_remains_advisory_until_acceptance": True,
        "ready_for_m49_03": True,
        "next_target": {
            "milestone": "M49-03",
            "task": "Fourth-slice human-accepted repair artifact",
        },
        "rationale": [
            "Current Windows fixture scope validates main-deck recipes only.",
            "G Zone slots, Stride timing, G-unit visibility, and Generation Break runtime support are not implemented yet.",
            "Choosing a main-deck-only boundary lets the repaired recipe be reviewed and revalidated without pretending that G Zone runtime exists.",
        ],
    },
    "defer_recipe_until_g_zone_support": {
        "label": "Defer this recipe until G Zone support exists",
        "decision_status": "deferred_until_g_zone_support",
        "recommendation": "keep_fourth_slice_advisory_until_g_zone_support",
        "main_deck_only_validation_allowed": False,
        "boundary_resolves_g_zone_deferred_for_main_deck_validation": False,
        "recipe_remains_advisory_until_acceptance": True,
        "ready_for_m49_03": False,
        "next_target": {
            "milestone": "M49-closeout",
            "task": "Fourth-slice fixture closeout",
        },
        "rationale": [
            "The recipe uses G-era evidence and can wait for a complete G Zone/Stride implementation.",
            "No runtime fixture should be created while the G Zone boundary is unresolved.",
        ],
    },
    "open_g_zone_implementation_queue": {
        "label": "Open a separate G Zone implementation queue",
        "decision_status": "requires_new_g_zone_implementation_queue",
        "recommendation": "write_g_zone_stride_specs_before_any_fixture_promotion",
        "main_deck_only_validation_allowed": False,
        "boundary_resolves_g_zone_deferred_for_main_deck_validation": False,
        "recipe_remains_advisory_until_acceptance": True,
        "ready_for_m49_03": False,
        "next_target": {
            "milestone": "G-Zone-Queue",
            "task": "G Zone and Stride implementation spec",
        },
        "rationale": [
            "G Zone support requires new deck slot, visibility, validation, timing, and replay contracts.",
            "The fourth slice cannot use G Zone runtime until those contracts are implemented and tested.",
        ],
    },
}


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _available_g_zone_option_ids(review_packet: dict[str, Any]) -> set[str]:
    option_ids: set[str] = set()
    for item in review_packet.get("review_items", []):
        for option in item.get("g_zone_decision_options", []):
            option_id = option.get("option_id")
            if option_id:
                option_ids.add(str(option_id))
    return option_ids


def _require_selected_option_available(review_packet: dict[str, Any], selected_option: str) -> None:
    known_options = set(OPTION_POLICIES)
    if selected_option not in known_options:
        raise ValueError(f"Unsupported G Zone decision option: {selected_option}")

    available_options = _available_g_zone_option_ids(review_packet)
    if selected_option not in available_options:
        raise ValueError(
            f"Selected G Zone decision option is not present in the review packet: {selected_option}"
        )


def _decision_item(review_item: dict[str, Any], selected_option: str, policy: dict[str, Any]) -> dict[str, Any]:
    return {
        "decision_item_id": f"m49_02_{review_item['recipe_id']}_g_zone_boundary_decision",
        "source_review_item_id": review_item["review_item_id"],
        "recipe_id": review_item["recipe_id"],
        "source_candidate_edge": review_item.get("source_candidate_edge", ""),
        "g_zone_package_id": review_item.get("g_zone_package_id", ""),
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
        "requires_human_acceptance": bool(policy["ready_for_m49_03"]),
        "requires_validation_rerun": bool(policy["ready_for_m49_03"]),
        "g_zone_requires_future_system_work": list(review_item.get("g_zone_requires_future_system_work", [])),
    }


def build_fourth_slice_g_zone_support_decision(
    review_packet: dict[str, Any] | None = None,
    closeout: dict[str, Any] | None = None,
    selected_option: str = DEFAULT_SELECTED_OPTION,
) -> dict[str, Any]:
    review_packet = review_packet or load_json(M49_REVIEW_PACKET)
    closeout = closeout or load_json(M48_CLOSEOUT)
    _require_selected_option_available(review_packet, selected_option)

    policy = OPTION_POLICIES[selected_option]
    review_items = review_packet.get("review_items", [])
    decision_items = [_decision_item(item, selected_option, policy) for item in review_items]
    g_zone_decision_items = [item for item in review_items if item.get("g_zone_decision_required")]
    review_ready = bool(review_packet.get("summary", {}).get("ready_for_m49_02"))
    closeout_allows_review = bool(closeout.get("summary", {}).get("human_g_zone_review_allowed"))
    all_items_have_decisions = len(decision_items) == len(review_items) and len(review_items) > 0
    ready_for_m49_03 = (
        bool(policy["ready_for_m49_03"])
        and review_ready
        and closeout_allows_review
        and all_items_have_decisions
        and len(g_zone_decision_items) == len(review_items)
    )

    return {
        "version": "M49-02",
        "description": "Fourth-slice G Zone support boundary decision",
        "selected_target": review_packet.get("selected_target", {}),
        "source_inputs": {
            "m49_review_packet": str(M49_REVIEW_PACKET.relative_to(ROOT)),
            "m48_closeout": str(M48_CLOSEOUT.relative_to(ROOT)),
        },
        "scope": {
            "offline_boundary_decision": True,
            "records_g_zone_decision": True,
            "records_human_acceptance": False,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
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
            "m49_03_must_record_explicit_human_acceptance": bool(policy["ready_for_m49_03"]),
            "m49_04_must_rerun_main_deck_validation": bool(policy["ready_for_m49_03"]),
            "m49_05_runtime_gate_must_remain_blocked_until_acceptance_and_validation": True,
        },
        "m49_04_validation_policy": {
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
        "summary": {
            "review_item_count": len(review_items),
            "g_zone_decision_item_count": len(g_zone_decision_items),
            "decision_item_count": len(decision_items),
            "selected_option_id": selected_option,
            "main_deck_only_validation_allowed": bool(policy["main_deck_only_validation_allowed"]),
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "runtime_promotion_allowed": False,
            "human_acceptance_recorded": False,
            "review_packet_ready_for_m49_02": review_ready,
            "closeout_allows_human_g_zone_review": closeout_allows_review,
            "ready_for_m49_03": ready_for_m49_03,
        },
        "decision_items": decision_items,
        "next_target": policy["next_target"],
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    boundary = report["boundary_policy"]
    lines = [
        "# M49-02 Fourth-Slice G Zone Support Decision",
        "",
        "## Summary",
        "",
        f"- Review items: `{summary['review_item_count']}`",
        f"- G Zone decision items: `{summary['g_zone_decision_item_count']}`",
        f"- Decision items: `{summary['decision_item_count']}`",
        f"- Selected option: `{summary['selected_option_id']}`",
        f"- Main-deck-only validation allowed: `{summary['main_deck_only_validation_allowed']}`",
        f"- G Zone runtime enabled: `{summary['g_zone_runtime_enabled']}`",
        f"- Stride runtime enabled: `{summary['stride_runtime_enabled']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M49-03: `{summary['ready_for_m49_03']}`",
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
            f"- Validation scope: `{report['m49_04_validation_policy']['validation_scope']}`",
            f"- G Zone slot model enabled: `{boundary['g_zone_slot_model_enabled']}`",
            f"- Stride deck-building validation enabled: `{boundary['stride_deck_building_validation_enabled']}`",
            f"- Generation Break runtime enabled: `{boundary['generation_break_runtime_enabled']}`",
            f"- Grade 4 main-deck allowed: `{boundary['grade4_main_deck_allowed']}`",
            f"- G units allowed in main deck: `{boundary['g_units_allowed_in_main_deck']}`",
            "",
            "## Decision Items",
            "",
        ]
    )
    for item in report["decision_items"]:
        lines.extend(
            [
                f"### `{item['recipe_id']}`",
                "",
                f"- Source review item: `{item['source_review_item_id']}`",
                f"- G Zone package: `{item['g_zone_package_id']}`",
                f"- Selected option: `{item['selected_option_id']}`",
                f"- Main-deck-only validation allowed: `{item['main_deck_only_validation_allowed']}`",
                f"- Future G Zone work: `{item['g_zone_requires_future_system_work']}`",
                "",
            ]
        )
    lines.extend(
        [
            "## Next",
            "",
            f"`{report['next_target']['milestone']}`: {report['next_target']['task']}.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M49-02 fourth-slice G Zone support decision.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    parser.add_argument("--selected-option", default=DEFAULT_SELECTED_OPTION, choices=sorted(OPTION_POLICIES))
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_fourth_slice_g_zone_support_decision(selected_option=args.selected_option)
    json_path = args.output_dir / "m49_02_fourth_slice_g_zone_support_decision.json"
    md_path = args.output_dir / "m49_02_fourth_slice_g_zone_support_decision.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M49-02 fourth-slice G Zone support decision wrote {json_path}")
    print(f"M49-02 fourth-slice G Zone support decision summary wrote {md_path}")
    print(
        "ready_for_m49_03={ready} selected_option={option} decision_items={items}".format(
            ready=report["summary"]["ready_for_m49_03"],
            option=report["summary"]["selected_option_id"],
            items=report["summary"]["decision_item_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m49_03"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
