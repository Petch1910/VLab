"""Build the M41-04 second-slice runtime fixture promotion gate report."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
FIXTURE_DIR = OUTPUT_DIR / "runtime_fixtures"
M41_TRIGGER_REPAIR_ACCEPTED = OUTPUT_DIR / "m41_repair_accept_second_slice_trigger_repair_artifact.json"
M41_REPAIR_VALIDATE = OUTPUT_DIR / "m41_repair_validate_second_slice_repaired_recipe.json"
M40_COMBO_CONSISTENCY = OUTPUT_DIR / "m40_04_second_slice_combo_recipe_consistency_report.json"


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


def _find_consistency_check(consistency_report: dict[str, Any], recipe_id: str) -> dict[str, Any]:
    for check in consistency_report.get("consistency_checks", []):
        if check.get("recipe_id") == recipe_id:
            return check
    return {}


def build_second_slice_runtime_fixture(
    accepted_report: dict[str, Any],
    validation_report: dict[str, Any],
) -> dict[str, Any]:
    accepted = accepted_report["accepted_repair"]
    validation = validation_report["recipe_validation"]
    target = validation_report.get("selected_target", {})
    main_deck = [
        {
            "card_id": row["card_id"],
            "quantity": int(row["quantity"]),
        }
        for row in accepted.get("repaired_quantities", [])
    ]
    return {
        "schema_version": "deck_recipe_runtime_fixture_v1",
        "fixture_id": "runtime_fixture_m40_recipe_001_classic_core_oracle_think_tank_m41_04",
        "fixture_scope": "offline_runtime_test_fixture",
        "recipe_id": accepted["recipe_id"],
        "source_artifact": "outputs/target_slice/m41_repair_accept_second_slice_trigger_repair_artifact.json",
        "selected_target": target,
        "format_policy": {
            "slice": target.get("slice", ""),
            "era_preset": target.get("era_preset", ""),
            "group": target.get("group", ""),
            "group_field": target.get("group_field", ""),
        },
        "main_deck": main_deck,
        "count_summary": {
            "main_deck_count": int(validation["count_summary"]["main_deck_count"]),
            "trigger_count": int(validation["count_summary"]["trigger_count"]),
            "trigger_counts": dict(validation["count_summary"]["trigger_counts"]),
            "grade_counts": dict(validation["count_summary"]["grade_counts"]),
            "clan_counts": dict(validation["count_summary"]["clan_counts"]),
        },
        "source_packages": {
            "base_grade_package_id": accepted.get("base_grade_repair_package_id", ""),
            "trigger_repair_package_id": accepted.get("trigger_repair_package_id", ""),
        },
        "runtime_boundaries": {
            "test_fixture_only": True,
            "auto_injected_into_player_decks": False,
            "bot_playbook_enabled": False,
            "ui_deck_library_mutated": False,
            "game_state_mutated": False,
        },
    }


def build_second_slice_runtime_fixture_promotion_gate(
    accepted_report: dict[str, Any] | None = None,
    validation_report: dict[str, Any] | None = None,
    consistency_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    accepted_report = accepted_report or load_json(M41_TRIGGER_REPAIR_ACCEPTED)
    validation_report = validation_report or load_json(M41_REPAIR_VALIDATE)
    consistency_report = consistency_report or load_json(M40_COMBO_CONSISTENCY)
    accepted = accepted_report["accepted_repair"]
    validation_summary = validation_report["summary"]
    validation = validation_report["recipe_validation"]
    recipe_id = validation_summary.get("recipe_id", "")
    consistency = _find_consistency_check(consistency_report, recipe_id)
    expected_edge = validation.get("source_candidate_edge", "")

    checks = [
        _check(
            "human_acceptance",
            bool(accepted_report.get("summary", {}).get("human_acceptance_recorded"))
            and accepted_report.get("acceptance_record", {}).get("decision") == "accepted",
            {
                "decision": accepted_report.get("acceptance_record", {}).get("decision", ""),
                "accepted_package_id": accepted_report.get("summary", {}).get("accepted_package_id", ""),
            },
        ),
        _check(
            "validation",
            validation_summary.get("validation_status") == "validator_passed"
            and bool(validation_summary.get("runtime_ready"))
            and int(validation_summary.get("blocking_issue_count", 0)) == 0
            and int(validation_summary.get("main_deck_count", 0)) == 50
            and int(validation_summary.get("trigger_count", 0)) == 16,
            {
                "validation_status": validation_summary.get("validation_status", ""),
                "runtime_ready": validation_summary.get("runtime_ready"),
                "blocking_issue_count": validation_summary.get("blocking_issue_count"),
                "main_deck_count": validation_summary.get("main_deck_count"),
                "trigger_count": validation_summary.get("trigger_count"),
            },
        ),
        _check(
            "grade_profile_review",
            validation_summary.get("grade_counts") == {"0": 17, "1": 14, "2": 11, "3": 8},
            {"grade_counts": validation_summary.get("grade_counts", {})},
        ),
        _check(
            "combo_pair_consistency",
            bool(consistency)
            and consistency.get("source_candidate_edge") == expected_edge
            and bool(consistency.get("pair_cards_present"))
            and not consistency.get("missing_pair_card_ids")
            and not consistency.get("pair_manual_review_dependencies"),
            {
                "source_candidate_edge": consistency.get("source_candidate_edge", ""),
                "expected_edge": expected_edge,
                "pair_cards_present": consistency.get("pair_cards_present"),
                "missing_pair_card_ids": consistency.get("missing_pair_card_ids", []),
                "pair_manual_review_dependencies": consistency.get("pair_manual_review_dependencies", []),
                "previous_recipe_manual_dependencies": consistency.get("recipe_manual_review_dependencies", []),
            },
        ),
        _check(
            "manual_review_cleared_after_repair",
            bool(validation_summary.get("manual_review_card_overlap_cleared"))
            and not validation["count_summary"].get("manual_review_card_ids_present"),
            {
                "manual_review_card_overlap_cleared": validation_summary.get("manual_review_card_overlap_cleared"),
                "manual_review_card_ids_present": validation["count_summary"].get("manual_review_card_ids_present", []),
            },
        ),
        _check(
            "runtime_boundary",
            not bool(validation_summary.get("promotion_allowed"))
            and not bool(accepted_report["scope"].get("creates_runtime_fixture"))
            and not bool(accepted_report["scope"].get("saved_deck_injection"))
            and not bool(accepted_report["scope"].get("ui_deck_list_publication"))
            and not bool(accepted_report["scope"].get("bot_playbook"))
            and not bool(accepted_report["scope"].get("direct_GameState_mutation")),
            {
                "validation_promotion_allowed": validation_summary.get("promotion_allowed"),
                "accept_creates_runtime_fixture": accepted_report["scope"].get("creates_runtime_fixture"),
                "accept_saved_deck_injection": accepted_report["scope"].get("saved_deck_injection"),
                "accept_ui_deck_list_publication": accepted_report["scope"].get("ui_deck_list_publication"),
                "accept_bot_playbook": accepted_report["scope"].get("bot_playbook"),
                "accept_direct_GameState_mutation": accepted_report["scope"].get("direct_GameState_mutation"),
            },
        ),
    ]
    promotion_allowed = _all_checks_pass(checks)
    fixture = build_second_slice_runtime_fixture(accepted_report, validation_report) if promotion_allowed else None

    return {
        "version": "M41-04",
        "description": "Second-slice runtime fixture promotion gate",
        "source_inputs": {
            "trigger_repair_acceptance_artifact": str(M41_TRIGGER_REPAIR_ACCEPTED.relative_to(ROOT)),
            "trigger_repaired_recipe_validation": str(M41_REPAIR_VALIDATE.relative_to(ROOT)),
            "second_slice_combo_recipe_consistency": str(M40_COMBO_CONSISTENCY.relative_to(ROOT)),
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
        },
        "gate_policy": {
            "requires_human_acceptance": True,
            "requires_validation_pass": True,
            "requires_grade_profile_clear": True,
            "requires_combo_pair_consistency": True,
            "requires_manual_review_cleared_after_repair": True,
            "fixture_only_not_live_deck": True,
        },
        "gate_checks": checks,
        "promotion_decision": {
            "promotion_allowed": promotion_allowed,
            "fixture_created": promotion_allowed,
            "fixture_path": "outputs/target_slice/runtime_fixtures/m40_recipe_001_classic_core_oracle_think_tank_m41_04.json"
            if promotion_allowed
            else "",
            "runtime_deck_library_mutated": False,
            "saved_deck_injected": False,
            "ui_deck_list_published": False,
            "bot_playbook_enabled": False,
            "next_action": "M41-closeout" if promotion_allowed else "repair_failed_gate_checks",
        },
        "runtime_fixture": fixture,
        "summary": {
            "recipe_id": recipe_id,
            "promotion_allowed": promotion_allowed,
            "passed_check_count": sum(1 for item in checks if item["passed"]),
            "failed_check_count": sum(1 for item in checks if not item["passed"]),
            "fixture_created": promotion_allowed,
            "ready_for_m41_closeout": promotion_allowed,
        },
        "next_target": {
            "milestone": "M41-closeout",
            "task": "Second-slice fixture closeout",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["promotion_decision"]
    lines = [
        "# M41-04 Second-Slice Runtime Fixture Promotion Gate",
        "",
        "## Summary",
        "",
        f"- Recipe: `{summary['recipe_id']}`",
        f"- Promotion allowed: `{summary['promotion_allowed']}`",
        f"- Passed checks: `{summary['passed_check_count']}`",
        f"- Failed checks: `{summary['failed_check_count']}`",
        f"- Fixture created: `{summary['fixture_created']}`",
        f"- Ready for M41-closeout: `{summary['ready_for_m41_closeout']}`",
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
            "",
            "## Policy",
            "",
            "- This gate may create an offline runtime/test fixture artifact only.",
            "- It does not inject a player deck.",
            "- It does not publish a UI deck list.",
            "- It does not enable bot playbooks.",
            "- It does not mutate GameState.",
            "",
            "## Next",
            "",
            "`M41-closeout`: Second-slice fixture closeout.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M41-04 second-slice runtime fixture promotion gate.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    parser.add_argument("--fixture-dir", type=Path, default=FIXTURE_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_second_slice_runtime_fixture_promotion_gate()
    json_path = args.output_dir / "m41_04_second_slice_runtime_fixture_promotion_gate.json"
    md_path = args.output_dir / "m41_04_second_slice_runtime_fixture_promotion_gate.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    fixture = report.get("runtime_fixture")
    if fixture:
        fixture_path = args.fixture_dir / "m40_recipe_001_classic_core_oracle_think_tank_m41_04.json"
        write_json(fixture, fixture_path)
    print(f"M41-04 second-slice runtime fixture promotion gate wrote {json_path}")
    print(f"M41-04 second-slice runtime fixture promotion gate summary wrote {md_path}")
    if fixture:
        print(f"M41-04 second-slice runtime fixture wrote {fixture_path}")
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
