"""Build the M61-06 seventh-slice runtime fixture promotion gate."""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
FIXTURE_DIR = OUTPUT_DIR / "runtime_fixtures"
M61_03_ACCEPTED_REPAIR = OUTPUT_DIR / "m61_03_seventh_slice_human_accepted_repair_artifact.json"
M61_05_VALIDATION = OUTPUT_DIR / "m61_05_seventh_slice_repaired_recipe_validation_report.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _check(check_id: str, passed: bool, evidence: dict[str, Any]) -> dict[str, Any]:
    return {
        "check_id": check_id,
        "passed": bool(passed),
        "evidence": evidence,
    }


def _all_checks_pass(checks: Sequence[dict[str, Any]]) -> bool:
    return all(bool(item.get("passed")) for item in checks)


def _group_slug(group: str) -> str:
    known = {
        "เนโอ เนคต้า": "neo_nectar",
    }
    if group in known:
        return known[group]
    slug = re.sub(r"[^a-z0-9]+", "_", group.lower()).strip("_")
    return slug or "group"


def _fixture_filename(recipe_id: str, selected_target: dict[str, Any]) -> str:
    return f"{recipe_id}_{_group_slug(str(selected_target.get('group', '')))}_m61_06.json"


def _fixture_id(recipe_id: str, selected_target: dict[str, Any]) -> str:
    return f"runtime_fixture_{recipe_id}_{_group_slug(str(selected_target.get('group', '')))}_m61_06"


def _repaired_rows(accepted_artifact: dict[str, Any]) -> list[dict[str, Any]]:
    return [
        {
            "card_id": row["card_id"],
            "quantity": int(row.get("quantity", 0)),
        }
        for row in accepted_artifact.get("accepted_repair", {}).get("repaired_quantities", [])
    ]


def build_runtime_fixture(
    accepted_artifact: dict[str, Any],
    validation_report: dict[str, Any],
) -> dict[str, Any]:
    validation = validation_report["repaired_recipe_validation"]
    accepted_context = validation_report["accepted_context"]
    system_context = validation_report["system_decision_context"]
    recipe_id = validation["recipe_id"]
    target = validation_report.get("selected_target", {})
    count_summary = validation["count_summary"]

    return {
        "schema_version": "deck_recipe_runtime_fixture_v1",
        "fixture_id": _fixture_id(recipe_id, target),
        "fixture_scope": "offline_runtime_test_fixture",
        "recipe_id": recipe_id,
        "source_artifacts": {
            "m61_03_human_accepted_repair_artifact": str(M61_03_ACCEPTED_REPAIR.relative_to(ROOT)),
            "m61_05_repaired_recipe_validation_report": str(M61_05_VALIDATION.relative_to(ROOT)),
        },
        "selected_target": target,
        "format_policy": {
            "slice": target.get("slice", ""),
            "era_preset": target.get("era_preset", ""),
            "group": target.get("group", ""),
            "group_field": target.get("group_field", ""),
        },
        "main_deck": _repaired_rows(accepted_artifact),
        "count_summary": {
            "main_deck_count": int(count_summary["explicit_card_count"]),
            "trigger_count": int(count_summary["trigger_count"]),
            "trigger_counts": dict(count_summary["trigger_counts"]),
            "grade_counts": dict(count_summary["grade_counts"]),
            "grade4_main_deck_count": int(count_summary.get("grade4_main_deck_count", 0)),
            "clan_counts": dict(count_summary["clan_counts"]),
        },
        "source_packages": {
            "selected_review_item_id": accepted_context.get("accepted_review_item_id", ""),
            "combined_repair_package_id": accepted_context.get("accepted_combined_repair_package_id", ""),
            "g_zone_package_id": accepted_context.get("accepted_g_zone_package_id", ""),
            "bloom_token_package_id": accepted_context.get("accepted_bloom_token_package_id", ""),
            "system_decision_item_id": system_context.get("source_decision_item_id", ""),
            "selected_g_zone_option_id": system_context.get("selected_g_zone_option_id", ""),
            "selected_bloom_token_option_id": system_context.get("selected_bloom_token_option_id", ""),
        },
        "system_boundaries": {
            "selected_g_zone_option_id": system_context.get("selected_g_zone_option_id", ""),
            "selected_bloom_token_option_id": system_context.get("selected_bloom_token_option_id", ""),
            "main_deck_only_validation_allowed": bool(system_context.get("main_deck_only_boundary_applied")),
            "g_zone_boundary_applied": bool(system_context.get("g_zone_boundary_applied")),
            "bloom_token_boundary_applied": bool(system_context.get("bloom_token_boundary_applied")),
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "bloom_token_runtime_enabled": False,
            "grade4_main_deck_allowed": False,
            "g_units_allowed_in_main_deck": False,
            "tokens_allowed_in_main_deck": False,
            "bloom_template_runtime_enabled": False,
            "token_lifecycle_runtime_enabled": False,
            "same_name_runtime_tracking_enabled": False,
            "duration_cleanup_runtime_enabled": False,
        },
        "runtime_boundaries": {
            "test_fixture_only": True,
            "auto_injected_into_player_decks": False,
            "bot_playbook_enabled": False,
            "ui_deck_library_mutated": False,
            "game_state_mutated": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "bloom_token_runtime_enabled": False,
        },
    }


def build_seventh_slice_runtime_fixture_promotion_gate(
    accepted_artifact: dict[str, Any] | None = None,
    validation_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    accepted_artifact = accepted_artifact or load_json(M61_03_ACCEPTED_REPAIR)
    validation_report = validation_report or load_json(M61_05_VALIDATION)

    accepted_summary = accepted_artifact.get("summary", {})
    accepted_context = validation_report["accepted_context"]
    system_context = validation_report["system_decision_context"]
    validation = validation_report["repaired_recipe_validation"]
    consistency = validation_report["repaired_recipe_consistency"]
    summary = validation_report["summary"]
    counts = validation["count_summary"]
    rows = _repaired_rows(accepted_artifact)
    recipe_id = validation["recipe_id"]
    target = validation_report.get("selected_target", {})
    target_grade_counts = {"0": 17, "1": 14, "2": 11, "3": 8}

    checks = [
        _check(
            "human_selection_and_acceptance",
            bool(accepted_context.get("human_selection_recorded"))
            and bool(accepted_context.get("human_acceptance_recorded"))
            and bool(accepted_summary.get("human_selection_recorded"))
            and bool(accepted_summary.get("human_acceptance_recorded")),
            {
                "validation_human_selection_recorded": accepted_context.get("human_selection_recorded"),
                "validation_human_acceptance_recorded": accepted_context.get("human_acceptance_recorded"),
                "accepted_human_selection_recorded": accepted_summary.get("human_selection_recorded"),
                "accepted_human_acceptance_recorded": accepted_summary.get("human_acceptance_recorded"),
                "accepted_review_item_id": accepted_context.get("accepted_review_item_id", ""),
            },
        ),
        _check(
            "system_boundary",
            system_context.get("selected_g_zone_option_id") == "main_deck_only_review_no_runtime_promotion"
            and system_context.get("selected_bloom_token_option_id")
            == "manual_semantic_review_only_no_runtime_promotion"
            and bool(system_context.get("g_zone_decision_recorded"))
            and bool(system_context.get("bloom_token_decision_recorded"))
            and bool(system_context.get("main_deck_only_boundary_applied"))
            and bool(system_context.get("g_zone_boundary_applied"))
            and bool(system_context.get("bloom_token_boundary_applied"))
            and not bool(system_context.get("g_zone_runtime_enabled"))
            and not bool(system_context.get("stride_runtime_enabled"))
            and not bool(system_context.get("bloom_token_runtime_enabled")),
            {
                "selected_g_zone_option_id": system_context.get("selected_g_zone_option_id", ""),
                "selected_bloom_token_option_id": system_context.get("selected_bloom_token_option_id", ""),
                "g_zone_decision_recorded": system_context.get("g_zone_decision_recorded"),
                "bloom_token_decision_recorded": system_context.get("bloom_token_decision_recorded"),
                "main_deck_only_boundary_applied": system_context.get("main_deck_only_boundary_applied"),
                "g_zone_boundary_applied": system_context.get("g_zone_boundary_applied"),
                "bloom_token_boundary_applied": system_context.get("bloom_token_boundary_applied"),
                "g_zone_runtime_enabled": system_context.get("g_zone_runtime_enabled"),
                "stride_runtime_enabled": system_context.get("stride_runtime_enabled"),
                "bloom_token_runtime_enabled": system_context.get("bloom_token_runtime_enabled"),
            },
        ),
        _check(
            "validation",
            validation.get("validation_status") == "validator_passed"
            and bool(validation.get("runtime_ready"))
            and int(validation.get("blocking_issue_count", 0)) == 0
            and counts.get("explicit_card_count") == 50
            and counts.get("trigger_count") == 16
            and counts.get("trigger_counts") == {"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}
            and counts.get("grade_counts") == target_grade_counts
            and int(counts.get("grade4_main_deck_count", 0)) == 0,
            {
                "validation_status": validation.get("validation_status"),
                "runtime_ready": validation.get("runtime_ready"),
                "blocking_issue_count": validation.get("blocking_issue_count"),
                "count_summary": counts,
            },
        ),
        _check(
            "combo_consistency",
            consistency.get("consistency_status") == "consistent_validator_passed"
            and bool(consistency.get("pair_cards_present"))
            and bool(consistency.get("promotion_allowed_by_validation_and_consistency"))
            and not bool(consistency.get("g_zone_support_deferred"))
            and not bool(consistency.get("bloom_token_support_deferred")),
            {
                "consistency_status": consistency.get("consistency_status"),
                "pair_cards_present": consistency.get("pair_cards_present"),
                "promotion_allowed_by_validation_and_consistency": consistency.get(
                    "promotion_allowed_by_validation_and_consistency"
                ),
                "g_zone_support_deferred": consistency.get("g_zone_support_deferred"),
                "bloom_token_support_deferred": consistency.get("bloom_token_support_deferred"),
            },
        ),
        _check(
            "accepted_repair_rows",
            accepted_summary.get("accepted_recipe_id") == recipe_id
            and int(sum(row["quantity"] for row in rows)) == 50
            and len(rows) > 0,
            {
                "accepted_recipe_id": accepted_summary.get("accepted_recipe_id", ""),
                "validation_recipe_id": recipe_id,
                "repaired_row_count": len(rows),
                "repaired_quantity_sum": int(sum(row["quantity"] for row in rows)),
            },
        ),
        _check(
            "rerun_ready",
            bool(summary.get("ready_for_m61_06")),
            {"ready_for_m61_06": summary.get("ready_for_m61_06")},
        ),
        _check(
            "runtime_boundary",
            not bool(summary.get("runtime_fixture_created"))
            and not bool(summary.get("runtime_promotion_allowed"))
            and not bool(validation_report["scope"].get("creates_runtime_fixture"))
            and not bool(validation_report["scope"].get("saved_deck_injection"))
            and not bool(validation_report["scope"].get("ui_deck_list_publication"))
            and not bool(validation_report["scope"].get("bot_playbook"))
            and not bool(validation_report["scope"].get("direct_GameState_mutation"))
            and not bool(validation_report["scope"].get("g_zone_runtime_enabled"))
            and not bool(validation_report["scope"].get("stride_runtime_enabled"))
            and not bool(validation_report["scope"].get("bloom_token_runtime_enabled")),
            {
                "m61_05_runtime_fixture_created": summary.get("runtime_fixture_created"),
                "m61_05_runtime_promotion_allowed": summary.get("runtime_promotion_allowed"),
                "m61_05_creates_runtime_fixture": validation_report["scope"].get("creates_runtime_fixture"),
                "m61_05_saved_deck_injection": validation_report["scope"].get("saved_deck_injection"),
                "m61_05_ui_deck_list_publication": validation_report["scope"].get("ui_deck_list_publication"),
                "m61_05_bot_playbook": validation_report["scope"].get("bot_playbook"),
                "m61_05_direct_GameState_mutation": validation_report["scope"].get("direct_GameState_mutation"),
                "m61_05_g_zone_runtime_enabled": validation_report["scope"].get("g_zone_runtime_enabled"),
                "m61_05_stride_runtime_enabled": validation_report["scope"].get("stride_runtime_enabled"),
                "m61_05_bloom_token_runtime_enabled": validation_report["scope"].get(
                    "bloom_token_runtime_enabled"
                ),
            },
        ),
    ]

    promotion_allowed = _all_checks_pass(checks)
    fixture = build_runtime_fixture(accepted_artifact, validation_report) if promotion_allowed else None
    fixture_filename = _fixture_filename(recipe_id, target)

    return {
        "version": "M61-06",
        "description": "Seventh-slice runtime fixture promotion gate",
        "source_inputs": {
            "m61_03_human_accepted_repair_artifact": str(M61_03_ACCEPTED_REPAIR.relative_to(ROOT)),
            "m61_05_repaired_recipe_validation_report": str(M61_05_VALIDATION.relative_to(ROOT)),
        },
        "scope": {
            "offline_promotion_gate": True,
            "creates_runtime_test_fixture_artifact": promotion_allowed,
            "mutates_runtime_deck_library": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_integration": False,
            "automatic_playbook_promotion": False,
            "live_card_text_parsing": False,
            "GameState_mutation": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "bloom_token_runtime_enabled": False,
        },
        "gate_policy": {
            "requires_human_selection": True,
            "requires_human_acceptance": True,
            "requires_main_deck_only_g_zone_boundary": True,
            "requires_manual_semantic_bloom_token_boundary": True,
            "requires_zero_blockers": True,
            "requires_combo_consistency": True,
            "requires_m61_05_ready": True,
            "fixture_only_not_live_deck": True,
        },
        "gate_checks": checks,
        "promotion_decision": {
            "promotion_allowed": promotion_allowed,
            "fixture_created": promotion_allowed,
            "fixture_path": f"outputs/target_slice/runtime_fixtures/{fixture_filename}" if promotion_allowed else "",
            "runtime_deck_library_mutated": False,
            "saved_deck_injected": False,
            "ui_deck_list_published": False,
            "bot_playbook_enabled": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "bloom_token_runtime_enabled": False,
            "next_action": "M61-closeout" if promotion_allowed else "repair_failed_gate_checks",
        },
        "runtime_fixture": fixture,
        "summary": {
            "recipe_id": recipe_id,
            "accepted_review_item_id": accepted_context.get("accepted_review_item_id", ""),
            "selected_g_zone_option_id": system_context.get("selected_g_zone_option_id", ""),
            "selected_bloom_token_option_id": system_context.get("selected_bloom_token_option_id", ""),
            "promotion_allowed": promotion_allowed,
            "passed_check_count": sum(1 for item in checks if item["passed"]),
            "failed_check_count": sum(1 for item in checks if not item["passed"]),
            "fixture_created": promotion_allowed,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "bloom_token_runtime_enabled": False,
            "ready_for_m61_closeout": promotion_allowed,
        },
        "next_target": {
            "milestone": "M61-closeout",
            "task": "Seventh-slice fixture closeout",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["promotion_decision"]
    lines = [
        "# M61-06 Seventh-Slice Runtime Fixture Promotion Gate",
        "",
        "## Summary",
        "",
        f"- Recipe: `{summary['recipe_id']}`",
        f"- Accepted review item: `{summary['accepted_review_item_id']}`",
        f"- Selected G Zone option: `{summary['selected_g_zone_option_id']}`",
        f"- Selected Bloom/token option: `{summary['selected_bloom_token_option_id']}`",
        f"- Promotion allowed: `{summary['promotion_allowed']}`",
        f"- Passed checks: `{summary['passed_check_count']}`",
        f"- Failed checks: `{summary['failed_check_count']}`",
        f"- Fixture created: `{summary['fixture_created']}`",
        f"- G Zone runtime enabled: `{summary['g_zone_runtime_enabled']}`",
        f"- Stride runtime enabled: `{summary['stride_runtime_enabled']}`",
        f"- Bloom/token runtime enabled: `{summary['bloom_token_runtime_enabled']}`",
        f"- Ready for M61-closeout: `{summary['ready_for_m61_closeout']}`",
        "",
        "## Gate Checks",
        "",
    ]
    for check in report["gate_checks"]:
        lines.append(f"- `{check['check_id']}` passed=`{check['passed']}` evidence=`{check['evidence']}`")
    lines.extend(
        [
            "",
            "## Fixture",
            "",
            f"- Fixture path: `{decision['fixture_path']}`",
            f"- Runtime deck library mutated: `{decision['runtime_deck_library_mutated']}`",
            f"- Saved deck injected: `{decision['saved_deck_injected']}`",
            f"- UI deck list published: `{decision['ui_deck_list_published']}`",
            f"- Bot playbook enabled: `{decision['bot_playbook_enabled']}`",
            f"- G Zone runtime enabled: `{decision['g_zone_runtime_enabled']}`",
            f"- Stride runtime enabled: `{decision['stride_runtime_enabled']}`",
            f"- Bloom/token runtime enabled: `{decision['bloom_token_runtime_enabled']}`",
            "",
            "## Policy",
            "",
            "- This gate may create an offline runtime/test fixture artifact only.",
            "- It does not inject a player deck.",
            "- It does not publish UI deck lists.",
            "- It does not enable bot playbooks.",
            "- It does not enable G Zone, Stride, Bloom/token, or token runtime.",
            "- It does not mutate GameState.",
            "",
            "## Next",
            "",
            "`M61-closeout`: Seventh-slice fixture closeout.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M61-06 seventh-slice runtime fixture promotion gate.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    parser.add_argument("--fixture-dir", type=Path, default=FIXTURE_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_seventh_slice_runtime_fixture_promotion_gate()
    json_path = args.output_dir / "m61_06_seventh_slice_runtime_fixture_promotion_gate.json"
    md_path = args.output_dir / "m61_06_seventh_slice_runtime_fixture_promotion_gate.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    fixture = report.get("runtime_fixture")
    if fixture:
        fixture_path = args.fixture_dir / _fixture_filename(report["summary"]["recipe_id"], fixture["selected_target"])
        write_json(fixture, fixture_path)
    print(f"M61-06 seventh-slice runtime fixture promotion gate wrote {json_path}")
    print(f"M61-06 seventh-slice runtime fixture promotion gate summary wrote {md_path}")
    if fixture:
        print(f"M61-06 seventh-slice runtime fixture wrote {fixture_path}")
    print(
        "promotion_allowed={allowed} passed={passed} failed={failed}".format(
            allowed=report["summary"]["promotion_allowed"],
            passed=report["summary"]["passed_check_count"],
            failed=report["summary"]["failed_check_count"],
        )
    )
    return 0 if report["summary"]["promotion_allowed"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
