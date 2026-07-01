"""Build the M45-04 third-slice runtime fixture promotion gate report."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
FIXTURE_DIR = OUTPUT_DIR / "runtime_fixtures"
M45_02_ACCEPTED_REPAIR = OUTPUT_DIR / "m45_02_third_slice_human_accepted_repair_artifact.json"
M45_03_VALIDATION = OUTPUT_DIR / "m45_03_third_slice_repaired_recipe_validation_report.json"

FIXTURE_FILENAME = "m44_recipe_001_link_joker_legion_bermuda_triangle_m45_04.json"


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


def _main_deck_rows(accepted_report: dict[str, Any]) -> list[dict[str, Any]]:
    return [
        {
            "card_id": row["card_id"],
            "quantity": int(row["quantity"]),
        }
        for row in accepted_report["accepted_repair"].get("repaired_quantities", [])
    ]


def _pair_presence(accepted_report: dict[str, Any]) -> dict[str, Any]:
    accepted = accepted_report["accepted_repair"]
    pair = accepted.get("pair", {})
    quantity_by_id = {row["card_id"]: int(row["quantity"]) for row in accepted.get("repaired_quantities", [])}
    required = [pair.get("source_card_id", ""), pair.get("target_card_id", "")]
    missing = [card_id for card_id in required if card_id and quantity_by_id.get(card_id, 0) <= 0]
    return {
        "required_pair_card_ids": [card_id for card_id in required if card_id],
        "missing_pair_card_ids": missing,
        "pair_cards_present": not missing and all(required),
    }


def build_third_slice_runtime_fixture(
    accepted_report: dict[str, Any],
    validation_report: dict[str, Any],
) -> dict[str, Any]:
    accepted = accepted_report["accepted_repair"]
    validation = validation_report["recipe_validations"][0]
    target = validation_report.get("selected_target", {})
    count_summary = validation["count_summary"]
    return {
        "schema_version": "deck_recipe_runtime_fixture_v1",
        "fixture_id": "runtime_fixture_m44_recipe_001_link_joker_legion_bermuda_triangle_m45_04",
        "fixture_scope": "offline_runtime_test_fixture",
        "recipe_id": accepted["recipe_id"],
        "source_artifact": "outputs/target_slice/m45_02_third_slice_human_accepted_repair_artifact.json",
        "selected_target": target,
        "format_policy": {
            "slice": target.get("slice", ""),
            "era_preset": target.get("era_preset", ""),
            "group": target.get("group", ""),
            "group_field": target.get("group_field", ""),
        },
        "main_deck": _main_deck_rows(accepted_report),
        "count_summary": {
            "main_deck_count": int(count_summary["explicit_card_count"]),
            "trigger_count": int(count_summary["trigger_count"]),
            "trigger_counts": dict(count_summary["trigger_counts"]),
            "grade_counts": dict(count_summary["grade_counts"]),
            "clan_counts": dict(count_summary["clan_counts"]),
        },
        "source_packages": {
            "manual_repair_package_id": accepted.get("manual_repair_package_id", ""),
            "source_grade_profile_package_id": accepted.get("source_grade_profile_package_id", ""),
            "combined_repair_package_id": accepted_report.get("summary", {}).get(
                "accepted_combined_repair_package_id", ""
            ),
        },
        "runtime_boundaries": {
            "test_fixture_only": True,
            "auto_injected_into_player_decks": False,
            "bot_playbook_enabled": False,
            "ui_deck_library_mutated": False,
            "game_state_mutated": False,
        },
    }


def build_third_slice_runtime_fixture_promotion_gate(
    accepted_report: dict[str, Any] | None = None,
    validation_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    accepted_report = accepted_report or load_json(M45_02_ACCEPTED_REPAIR)
    validation_report = validation_report or load_json(M45_03_VALIDATION)
    accepted = accepted_report["accepted_repair"]
    validation_summary = validation_report["summary"]
    validation = validation_report["recipe_validations"][0] if validation_report.get("recipe_validations") else {}
    pair_presence = _pair_presence(accepted_report)

    checks = [
        _check(
            "human_acceptance",
            bool(accepted_report.get("summary", {}).get("human_acceptance_recorded"))
            and accepted_report.get("acceptance_record", {}).get("decision") == "accepted",
            {
                "decision": accepted_report.get("acceptance_record", {}).get("decision", ""),
                "accepted_review_item_id": accepted_report.get("summary", {}).get("accepted_review_item_id", ""),
                "accepted_combined_repair_package_id": accepted_report.get("summary", {}).get(
                    "accepted_combined_repair_package_id", ""
                ),
            },
        ),
        _check(
            "validation",
            validation.get("validation_status") == "validator_passed"
            and bool(validation.get("runtime_ready"))
            and int(validation.get("blocking_issue_count", 0)) == 0
            and int(validation.get("count_summary", {}).get("explicit_card_count", 0)) == 50
            and int(validation.get("count_summary", {}).get("trigger_count", 0)) == 16
            and bool(validation_summary.get("ready_for_m45_04")),
            {
                "validation_status": validation.get("validation_status", ""),
                "runtime_ready": validation.get("runtime_ready"),
                "blocking_issue_count": validation.get("blocking_issue_count"),
                "main_deck_count": validation.get("count_summary", {}).get("explicit_card_count"),
                "trigger_count": validation.get("count_summary", {}).get("trigger_count"),
                "ready_for_m45_04": validation_summary.get("ready_for_m45_04"),
            },
        ),
        _check(
            "grade_profile_review",
            validation.get("count_summary", {}).get("grade_counts") == {"0": 17, "1": 14, "2": 11, "3": 8},
            {"grade_counts": validation.get("count_summary", {}).get("grade_counts", {})},
        ),
        _check(
            "combo_pair_consistency",
            bool(pair_presence["pair_cards_present"]),
            pair_presence,
        ),
        _check(
            "manual_review_cleared_after_repair",
            int(validation_summary.get("manual_review_overlap_recipe_count", 0)) == 0
            and not any(issue.get("code") == "manual_review_card_overlap" for issue in validation.get("issues", [])),
            {
                "manual_review_overlap_recipe_count": validation_summary.get("manual_review_overlap_recipe_count"),
                "issue_codes": [issue.get("code", "") for issue in validation.get("issues", [])],
            },
        ),
        _check(
            "combined_repair_integrity",
            bool(accepted_report.get("summary", {}).get("ready_for_m45_03"))
            and int(accepted_report.get("summary", {}).get("repair_application_issue_count", 0)) == 0
            and bool(accepted_report.get("summary", {}).get("combined_grade_repair_recomputed"))
            and accepted.get("grade_counts_after_repair") == {"0": 17, "1": 14, "2": 11, "3": 8},
            {
                "ready_for_m45_03": accepted_report.get("summary", {}).get("ready_for_m45_03"),
                "repair_application_issue_count": accepted_report.get("summary", {}).get(
                    "repair_application_issue_count"
                ),
                "combined_grade_repair_recomputed": accepted_report.get("summary", {}).get(
                    "combined_grade_repair_recomputed"
                ),
                "source_grade_package_conflict_count": accepted_report.get("summary", {}).get(
                    "source_grade_package_conflict_count"
                ),
                "grade_counts_after_repair": accepted.get("grade_counts_after_repair", {}),
            },
        ),
        _check(
            "runtime_boundary",
            not bool(accepted_report["scope"].get("creates_runtime_fixture"))
            and not bool(accepted_report["scope"].get("saved_deck_injection"))
            and not bool(accepted_report["scope"].get("ui_deck_list_publication"))
            and not bool(accepted_report["scope"].get("bot_playbook"))
            and not bool(accepted_report["scope"].get("direct_GameState_mutation"))
            and not bool(validation_report["scope"].get("creates_runtime_fixture"))
            and not bool(validation_report["scope"].get("saved_deck_injection"))
            and not bool(validation_report["scope"].get("ui_deck_list_publication"))
            and not bool(validation_report["scope"].get("bot_playbook"))
            and not bool(validation_report["scope"].get("direct_GameState_mutation")),
            {
                "accepted_creates_runtime_fixture": accepted_report["scope"].get("creates_runtime_fixture"),
                "accepted_saved_deck_injection": accepted_report["scope"].get("saved_deck_injection"),
                "accepted_ui_deck_list_publication": accepted_report["scope"].get("ui_deck_list_publication"),
                "accepted_bot_playbook": accepted_report["scope"].get("bot_playbook"),
                "accepted_direct_GameState_mutation": accepted_report["scope"].get("direct_GameState_mutation"),
                "validation_creates_runtime_fixture": validation_report["scope"].get("creates_runtime_fixture"),
                "validation_saved_deck_injection": validation_report["scope"].get("saved_deck_injection"),
                "validation_ui_deck_list_publication": validation_report["scope"].get("ui_deck_list_publication"),
                "validation_bot_playbook": validation_report["scope"].get("bot_playbook"),
                "validation_direct_GameState_mutation": validation_report["scope"].get("direct_GameState_mutation"),
            },
        ),
    ]
    promotion_allowed = _all_checks_pass(checks)
    fixture = build_third_slice_runtime_fixture(accepted_report, validation_report) if promotion_allowed else None
    fixture_path = f"outputs/target_slice/runtime_fixtures/{FIXTURE_FILENAME}" if promotion_allowed else ""

    return {
        "version": "M45-04",
        "description": "Third-slice runtime fixture promotion gate",
        "source_inputs": {
            "human_accepted_repair_artifact": str(M45_02_ACCEPTED_REPAIR.relative_to(ROOT)),
            "repaired_recipe_validation": str(M45_03_VALIDATION.relative_to(ROOT)),
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
            "requires_combined_repair_integrity": True,
            "fixture_only_not_live_deck": True,
        },
        "gate_checks": checks,
        "promotion_decision": {
            "promotion_allowed": promotion_allowed,
            "fixture_created": promotion_allowed,
            "fixture_path": fixture_path,
            "runtime_deck_library_mutated": False,
            "saved_deck_injected": False,
            "ui_deck_list_published": False,
            "bot_playbook_enabled": False,
            "next_action": "M45-closeout" if promotion_allowed else "repair_failed_gate_checks",
        },
        "runtime_fixture": fixture,
        "summary": {
            "recipe_id": accepted.get("recipe_id", ""),
            "promotion_allowed": promotion_allowed,
            "passed_check_count": sum(1 for item in checks if item["passed"]),
            "failed_check_count": sum(1 for item in checks if not item["passed"]),
            "fixture_created": promotion_allowed,
            "ready_for_m45_closeout": promotion_allowed,
        },
        "next_target": {
            "milestone": "M45-closeout",
            "task": "Third-slice fixture closeout",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["promotion_decision"]
    lines = [
        "# M45-04 Third-Slice Runtime Fixture Promotion Gate",
        "",
        "## Summary",
        "",
        f"- Recipe: `{summary['recipe_id']}`",
        f"- Promotion allowed: `{summary['promotion_allowed']}`",
        f"- Passed checks: `{summary['passed_check_count']}`",
        f"- Failed checks: `{summary['failed_check_count']}`",
        f"- Fixture created: `{summary['fixture_created']}`",
        f"- Ready for M45-closeout: `{summary['ready_for_m45_closeout']}`",
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
            "`M45-closeout`: Third-slice fixture closeout.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M45-04 third-slice runtime fixture promotion gate.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    parser.add_argument("--fixture-dir", type=Path, default=FIXTURE_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_third_slice_runtime_fixture_promotion_gate()
    json_path = args.output_dir / "m45_04_third_slice_runtime_fixture_promotion_gate.json"
    md_path = args.output_dir / "m45_04_third_slice_runtime_fixture_promotion_gate.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    fixture = report.get("runtime_fixture")
    if fixture:
        fixture_path = args.fixture_dir / FIXTURE_FILENAME
        write_json(fixture, fixture_path)
    print(f"M45-04 third-slice runtime fixture promotion gate wrote {json_path}")
    print(f"M45-04 third-slice runtime fixture promotion gate summary wrote {md_path}")
    if fixture:
        print(f"M45-04 third-slice runtime fixture wrote {fixture_path}")
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
