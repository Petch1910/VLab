"""Validate the M69-03 ninth-slice repaired recipe preview for M69-05."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M69_03_ACCEPTED_REPAIR = OUTPUT_DIR / "m69_03_ninth_slice_human_accepted_repair_artifact.json"
M69_04_SYSTEM_DECISION = OUTPUT_DIR / "m69_04_ninth_slice_system_decision_artifact.json"

if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.check_ninth_slice_combo_recipe_consistency import (  # noqa: E402
    build_ninth_slice_consistency_report,
)
from tools.deck.validate_ninth_slice_recipe_drafts import (  # noqa: E402
    _all_card_ids,
    build_ninth_slice_validation_report,
    load_card_rows,
)


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _g_zone_boundary_applied(system_decision: dict[str, Any]) -> bool:
    summary = system_decision.get("summary", {})
    return (
        summary.get("selected_g_zone_option_id") == "main_deck_only_review_no_runtime_promotion"
        and summary.get("g_zone_runtime_enabled") is False
        and summary.get("stride_runtime_enabled") is False
        and summary.get("g_zone_decision_recorded") is True
    )


def _stride_boundary_applied(system_decision: dict[str, Any]) -> bool:
    summary = system_decision.get("summary", {})
    return (
        summary.get("selected_stride_option_id") == "main_deck_only_review_no_runtime_promotion"
        and summary.get("stride_runtime_enabled") is False
        and summary.get("stride_decision_recorded") is True
    )


def _aqua_force_boundary_applied(system_decision: dict[str, Any]) -> bool:
    summary = system_decision.get("summary", {})
    return (
        summary.get("selected_aqua_force_option_id") == "manual_semantic_review_only_no_runtime_promotion"
        and summary.get("aqua_force_battle_order_runtime_enabled") is False
        and summary.get("aqua_force_battle_order_decision_recorded") is True
    )


def _main_deck_only_boundary_applied(system_decision: dict[str, Any]) -> bool:
    summary = system_decision.get("summary", {})
    return (
        _g_zone_boundary_applied(system_decision)
        and _stride_boundary_applied(system_decision)
        and _aqua_force_boundary_applied(system_decision)
        and summary.get("main_deck_only_validation_allowed") is True
        and summary.get("ready_for_m69_05") is True
    )


def _recipe_report_from_artifacts(
    accepted_artifact: dict[str, Any],
    system_decision: dict[str, Any],
) -> dict[str, Any]:
    repair = accepted_artifact["accepted_repair"]
    accepted_summary = accepted_artifact.get("summary", {})
    decision_summary = system_decision.get("summary", {})
    g_zone_boundary = _g_zone_boundary_applied(system_decision)
    stride_boundary = _stride_boundary_applied(system_decision)
    aqua_boundary = _aqua_force_boundary_applied(system_decision)
    return {
        "selected_target": accepted_artifact.get("selected_target", {}),
        "recipe_drafts": [
            {
                "recipe_id": repair["recipe_id"],
                "source_candidate_edge": repair.get("source_candidate_edge", ""),
                "source_edge_rank": 1,
                "anchor_card_id": repair.get("pair", {}).get("source_card_id", ""),
                "review_status": "human_accepted_main_deck_only_repair_preview",
                "quantities": [
                    {
                        "card_id": row["card_id"],
                        "quantity": int(row.get("quantity", 0)),
                        "roles": ["m69_05_repaired_recipe_candidate"],
                        "quantity_source": "m69_03_accepted_repair_preview",
                    }
                    for row in repair.get("repaired_quantities", [])
                ],
                "slot_summary": {
                    "main_deck_target": 50,
                    "explicit_card_count": int(repair.get("main_deck_count_after_repair", 0)),
                    "total_unfilled_slots": max(0, 50 - int(repair.get("main_deck_count_after_repair", 0))),
                },
                "validation_metadata": {
                    "manual_review_card_ids": [],
                    "selected_review_item_id": accepted_summary.get("selected_review_item_id", ""),
                    "accepted_combined_repair_package_id": accepted_summary.get(
                        "accepted_combined_repair_package_id", ""
                    ),
                    "m69_05_in_memory_repaired_validation": True,
                    "system_decision_item_id": system_decision.get("decision_item", {}).get(
                        "decision_item_id", ""
                    ),
                    "g_zone_support_deferred_resolved_by_m69_04": g_zone_boundary,
                    "stride_support_deferred_resolved_by_m69_04": stride_boundary,
                    "aqua_force_battle_order_support_deferred_resolved_by_m69_04": aqua_boundary,
                    "g_zone_support_deferred": not g_zone_boundary,
                    "stride_support_deferred": not stride_boundary,
                    "aqua_force_battle_order_support_deferred": not aqua_boundary,
                    "selected_g_zone_option_id": decision_summary.get("selected_g_zone_option_id", ""),
                    "selected_stride_option_id": decision_summary.get("selected_stride_option_id", ""),
                    "selected_aqua_force_option_id": decision_summary.get("selected_aqua_force_option_id", ""),
                    "not_runtime_deck": True,
                    "not_saved_deck": True,
                    "not_ui_published": True,
                    "not_bot_playbook": True,
                },
                "review_blockers": [],
                "pair": repair.get("pair", {}),
            }
        ],
    }


def _issue_codes(validation: dict[str, Any], severity: str | None = None) -> list[str]:
    return [
        issue["code"]
        for issue in validation.get("issues", [])
        if severity is None or issue.get("severity") == severity
    ]


def build_ninth_slice_repaired_validation_report(
    accepted_artifact: dict[str, Any] | None = None,
    system_decision: dict[str, Any] | None = None,
) -> dict[str, Any]:
    accepted_artifact = accepted_artifact or load_json(M69_03_ACCEPTED_REPAIR)
    system_decision = system_decision or load_json(M69_04_SYSTEM_DECISION)
    accepted_summary = accepted_artifact.get("summary", {})
    decision_summary = system_decision.get("summary", {})
    g_zone_boundary = _g_zone_boundary_applied(system_decision)
    stride_boundary = _stride_boundary_applied(system_decision)
    aqua_boundary = _aqua_force_boundary_applied(system_decision)
    main_deck_only_boundary = _main_deck_only_boundary_applied(system_decision)
    repaired_recipe_report = _recipe_report_from_artifacts(accepted_artifact, system_decision)
    card_rows = load_card_rows(_all_card_ids(repaired_recipe_report))
    validation_report = build_ninth_slice_validation_report(repaired_recipe_report, card_rows)
    validation = validation_report["recipe_validations"][0]
    consistency_report = build_ninth_slice_consistency_report(repaired_recipe_report, validation_report)
    consistency = consistency_report["consistency_checks"][0]
    ready_for_m69_06 = (
        bool(accepted_summary.get("human_selection_recorded"))
        and bool(accepted_summary.get("human_acceptance_recorded"))
        and bool(accepted_summary.get("repair_accepted"))
        and bool(accepted_summary.get("ready_for_m69_04"))
        and bool(decision_summary.get("g_zone_decision_recorded"))
        and bool(decision_summary.get("stride_decision_recorded"))
        and bool(decision_summary.get("aqua_force_battle_order_decision_recorded"))
        and main_deck_only_boundary
        and validation.get("validation_status") == "validator_passed"
        and bool(validation.get("runtime_ready"))
        and bool(consistency.get("promotion_allowed"))
    )

    return {
        "version": "M69-05",
        "description": "Ninth-slice repaired recipe validation rerun",
        "selected_target": accepted_artifact.get("selected_target", {}),
        "source_inputs": {
            "m69_03_human_accepted_repair_artifact": str(M69_03_ACCEPTED_REPAIR.relative_to(ROOT)),
            "m69_04_system_decision_artifact": str(M69_04_SYSTEM_DECISION.relative_to(ROOT)),
            "runtime_cards_sqlite": "data/packs/vanguard_th/cards.sqlite",
        },
        "scope": {
            "offline_validation_rerun": True,
            "records_human_selection": False,
            "records_human_acceptance": False,
            "records_repair_acceptance": False,
            "records_g_zone_decision": False,
            "records_stride_decision": False,
            "records_aqua_force_battle_order_decision": False,
            "changes_accepted_repair_artifact": False,
            "changes_system_decision_artifact": False,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "automatic_deck_injection": False,
            "direct_GameState_mutation": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "aqua_force_battle_order_runtime_enabled": False,
        },
        "validation_policy": {
            **validation_report.get("validation_policy", {}),
            "validated_input": "m69_03_repaired_quantity_preview",
            "human_selection_required": True,
            "human_acceptance_required": True,
            "repair_acceptance_required": True,
            "system_boundary_required": True,
            "suppressed_by_m69_04_boundary_issue_codes": [
                code
                for code, resolved in [
                    ("g_zone_support_deferred", g_zone_boundary),
                    ("stride_support_deferred", stride_boundary),
                    ("aqua_force_battle_order_support_deferred", aqua_boundary),
                ]
                if resolved
            ],
            "main_deck_only_validation": main_deck_only_boundary,
            "runtime_fixture_gate_still_required": True,
            "must_not_enable_g_zone_or_stride_runtime": True,
            "must_not_enable_aqua_force_battle_order_runtime": True,
        },
        "accepted_context": {
            "selected_review_item_id": accepted_summary.get("selected_review_item_id", ""),
            "selected_recipe_id": accepted_summary.get("selected_recipe_id", ""),
            "accepted_combined_repair_package_id": accepted_summary.get(
                "accepted_combined_repair_package_id", ""
            ),
            "selected_g_zone_package_id": accepted_summary.get("selected_g_zone_package_id", ""),
            "selected_stride_package_id": accepted_summary.get("selected_stride_package_id", ""),
            "selected_aqua_force_battle_order_package_id": accepted_summary.get(
                "selected_aqua_force_battle_order_package_id", ""
            ),
            "human_selection_recorded": bool(accepted_summary.get("human_selection_recorded")),
            "human_acceptance_recorded": bool(accepted_summary.get("human_acceptance_recorded")),
            "repair_accepted": bool(accepted_summary.get("repair_accepted")),
            "accepted_artifact_ready_for_system_decision": bool(accepted_summary.get("ready_for_m69_04")),
        },
        "system_decision_context": {
            "selected_g_zone_option_id": decision_summary.get("selected_g_zone_option_id", ""),
            "selected_stride_option_id": decision_summary.get("selected_stride_option_id", ""),
            "selected_aqua_force_option_id": decision_summary.get("selected_aqua_force_option_id", ""),
            "g_zone_decision_recorded": bool(decision_summary.get("g_zone_decision_recorded")),
            "stride_decision_recorded": bool(decision_summary.get("stride_decision_recorded")),
            "aqua_force_battle_order_decision_recorded": bool(
                decision_summary.get("aqua_force_battle_order_decision_recorded")
            ),
            "main_deck_only_boundary_applied": main_deck_only_boundary,
            "g_zone_boundary_applied": g_zone_boundary,
            "stride_boundary_applied": stride_boundary,
            "aqua_force_boundary_applied": aqua_boundary,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "aqua_force_battle_order_runtime_enabled": False,
            "source_decision_item_id": system_decision.get("decision_item", {}).get("decision_item_id", ""),
        },
        "repaired_recipe_validation": {
            "recipe_id": validation["recipe_id"],
            "validation_status": validation["validation_status"],
            "runtime_ready": validation["runtime_ready"],
            "blocking_issue_count": validation["blocking_issue_count"],
            "blocker_codes": _issue_codes(validation, "blocker"),
            "review_codes": _issue_codes(validation, "review"),
            "count_summary": validation["count_summary"],
        },
        "repaired_recipe_consistency": {
            "recipe_id": consistency["recipe_id"],
            "consistency_status": consistency["consistency_status"],
            "pair_cards_present": consistency["pair_cards_present"],
            "promotion_allowed_by_validation_and_consistency": consistency["promotion_allowed"],
            "source_card_id": consistency["source_card_id"],
            "target_card_id": consistency["target_card_id"],
            "g_zone_support_deferred": consistency["g_zone_support_deferred"],
            "stride_support_deferred": consistency["stride_support_deferred"],
            "aqua_force_battle_order_support_deferred": consistency[
                "aqua_force_battle_order_support_deferred"
            ],
        },
        "validation_report_preview": validation_report,
        "consistency_report_preview": consistency_report,
        "summary": {
            **validation_report["summary"],
            "selected_recipe_id": accepted_summary.get("selected_recipe_id", ""),
            "selected_review_item_id": accepted_summary.get("selected_review_item_id", ""),
            "accepted_combined_repair_package_id": accepted_summary.get(
                "accepted_combined_repair_package_id", ""
            ),
            "selected_g_zone_package_id": accepted_summary.get("selected_g_zone_package_id", ""),
            "selected_stride_package_id": accepted_summary.get("selected_stride_package_id", ""),
            "selected_aqua_force_battle_order_package_id": accepted_summary.get(
                "selected_aqua_force_battle_order_package_id", ""
            ),
            "human_selection_recorded": bool(accepted_summary.get("human_selection_recorded")),
            "human_acceptance_recorded": bool(accepted_summary.get("human_acceptance_recorded")),
            "repair_accepted": bool(accepted_summary.get("repair_accepted")),
            "selected_g_zone_option_id": decision_summary.get("selected_g_zone_option_id", ""),
            "selected_stride_option_id": decision_summary.get("selected_stride_option_id", ""),
            "selected_aqua_force_option_id": decision_summary.get("selected_aqua_force_option_id", ""),
            "g_zone_decision_recorded": bool(decision_summary.get("g_zone_decision_recorded")),
            "stride_decision_recorded": bool(decision_summary.get("stride_decision_recorded")),
            "aqua_force_battle_order_decision_recorded": bool(
                decision_summary.get("aqua_force_battle_order_decision_recorded")
            ),
            "main_deck_only_boundary_applied": main_deck_only_boundary,
            "g_zone_boundary_applied": g_zone_boundary,
            "stride_boundary_applied": stride_boundary,
            "aqua_force_boundary_applied": aqua_boundary,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "aqua_force_battle_order_runtime_enabled": False,
            "source_system_decision_item_id": system_decision.get("decision_item", {}).get(
                "decision_item_id", ""
            ),
            "consistency_status": consistency["consistency_status"],
            "promotion_allowed_count": consistency_report["summary"]["promotion_allowed_count"],
            "runtime_fixture_created": False,
            "runtime_promotion_allowed": False,
            "ready_for_m69_06": ready_for_m69_06,
        },
        "recipe_validations": validation_report["recipe_validations"],
        "consistency_checks": consistency_report["consistency_checks"],
        "next_target": {
            "milestone": "M69-06",
            "task": "Ninth-slice runtime fixture promotion gate",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    validation = report["repaired_recipe_validation"]
    consistency = report["repaired_recipe_consistency"]
    lines = [
        "# M69-05 Ninth-Slice Repaired Recipe Validation Rerun",
        "",
        "## Summary",
        "",
        f"- Selected recipe: `{summary['selected_recipe_id']}`",
        f"- Selected review item: `{summary['selected_review_item_id']}`",
        f"- Accepted repair package: `{summary['accepted_combined_repair_package_id']}`",
        f"- Selected G Zone package: `{summary['selected_g_zone_package_id']}`",
        f"- Selected Stride package: `{summary['selected_stride_package_id']}`",
        f"- Selected Aqua Force package: `{summary['selected_aqua_force_battle_order_package_id']}`",
        f"- Human selection recorded: `{summary['human_selection_recorded']}`",
        f"- Human acceptance recorded: `{summary['human_acceptance_recorded']}`",
        f"- Repair accepted: `{summary['repair_accepted']}`",
        f"- Selected G Zone option: `{summary['selected_g_zone_option_id']}`",
        f"- Selected Stride option: `{summary['selected_stride_option_id']}`",
        f"- Selected Aqua Force option: `{summary['selected_aqua_force_option_id']}`",
        f"- G Zone decision recorded: `{summary['g_zone_decision_recorded']}`",
        f"- Stride decision recorded: `{summary['stride_decision_recorded']}`",
        f"- Aqua Force decision recorded: `{summary['aqua_force_battle_order_decision_recorded']}`",
        f"- Main-deck-only boundary applied: `{summary['main_deck_only_boundary_applied']}`",
        f"- G Zone runtime enabled: `{summary['g_zone_runtime_enabled']}`",
        f"- Stride runtime enabled: `{summary['stride_runtime_enabled']}`",
        f"- Aqua Force battle-order runtime enabled: `{summary['aqua_force_battle_order_runtime_enabled']}`",
        f"- Recipes validated: `{summary['recipe_count']}`",
        f"- Runtime-ready recipes: `{summary['runtime_ready_recipe_count']}`",
        f"- Validator passed: `{summary['validator_passed_count']}`",
        f"- Consistency status: `{summary['consistency_status']}`",
        f"- Promotion-allowed checks: `{summary['promotion_allowed_count']}`",
        f"- Blocking issues: `{validation['blocking_issue_count']}`",
        f"- Runtime fixture created: `{summary['runtime_fixture_created']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M69-06: `{summary['ready_for_m69_06']}`",
        "",
        "## Validation",
        "",
        f"- Validation status: `{validation['validation_status']}`",
        f"- Runtime ready: `{validation['runtime_ready']}`",
        f"- Blocker codes: `{validation['blocker_codes']}`",
        f"- Review codes: `{validation['review_codes']}`",
        f"- Count summary: `{validation['count_summary']}`",
        "",
        "## Consistency",
        "",
        f"- Pair cards present: `{consistency['pair_cards_present']}`",
        f"- Source -> target: `{consistency['source_card_id']}` -> `{consistency['target_card_id']}`",
        f"- Promotion allowed by validation/consistency: `{consistency['promotion_allowed_by_validation_and_consistency']}`",
        f"- G Zone support deferred in consistency check: `{consistency['g_zone_support_deferred']}`",
        f"- Stride support deferred in consistency check: `{consistency['stride_support_deferred']}`",
        f"- Aqua Force battle-order support deferred in consistency check: `{consistency['aqua_force_battle_order_support_deferred']}`",
        "",
        "## Policy",
        "",
        "- Rerun is in-memory only.",
        "- Source recipe artifacts are not modified.",
        "- G Zone, Stride, and Aqua Force battle-order runtime remain disabled.",
        "- Runtime fixture promotion remains disabled until M69-06.",
        "",
        "## Next",
        "",
        "`M69-06`: Ninth-slice runtime fixture promotion gate.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate M69-03 ninth-slice repaired recipe preview.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_ninth_slice_repaired_validation_report()
    json_path = args.output_dir / "m69_05_ninth_slice_repaired_recipe_validation_report.json"
    md_path = args.output_dir / "m69_05_ninth_slice_repaired_recipe_validation_report.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M69-05 ninth-slice repaired recipe validation report wrote {json_path}")
    print(f"M69-05 ninth-slice repaired recipe validation summary wrote {md_path}")
    print(
        "ready_for_m69_06={ready} validation={validation} consistency={consistency}".format(
            ready=report["summary"]["ready_for_m69_06"],
            validation=report["summary"]["validator_passed_count"],
            consistency=report["summary"]["consistency_status"],
        )
    )
    return 0 if report["summary"]["ready_for_m69_06"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
