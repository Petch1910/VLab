"""Build the M38-04 runtime fixture promotion gate report."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
FIXTURE_DIR = OUTPUT_DIR / "runtime_fixtures"
M37_05_RERUN = OUTPUT_DIR / "m37_05_revised_recipe_validation_rerun.json"
M38_03_ACCEPTED = OUTPUT_DIR / "m38_03_human_accepted_recipe_artifact.json"


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


def build_runtime_fixture(accepted_report: dict[str, Any]) -> dict[str, Any]:
    accepted = accepted_report["accepted_recipe"]
    summary = accepted["count_summary"]
    target = accepted_report.get("selected_target", {})
    main_deck = [
        {
            "card_id": row["card_id"],
            "quantity": int(row["quantity"]),
        }
        for row in accepted.get("quantities", [])
    ]
    return {
        "schema_version": "deck_recipe_runtime_fixture_v1",
        "fixture_id": "runtime_fixture_recipe_003_classic_core_nova_grappler_m38_04",
        "fixture_scope": "offline_runtime_test_fixture",
        "recipe_id": accepted["recipe_id"],
        "source_artifact": "outputs/target_slice/m38_03_human_accepted_recipe_artifact.json",
        "selected_target": target,
        "format_policy": {
            "slice": target.get("slice", ""),
            "era_preset": target.get("era_preset", ""),
            "group": target.get("group", ""),
            "group_field": target.get("group_field", ""),
        },
        "main_deck": main_deck,
        "count_summary": {
            "main_deck_count": int(summary["main_deck_count"]),
            "trigger_count": int(summary["trigger_count"]),
            "trigger_counts": dict(summary["trigger_counts"]),
            "grade_counts": dict(summary["grade_counts"]),
            "clan_counts": dict(summary["clan_counts"]),
        },
        "source_packages": {
            "grade_package_id": accepted_report["summary"]["accepted_grade_package_id"],
            "trigger_package_id": accepted_report["summary"]["accepted_trigger_package_id"],
        },
        "runtime_boundaries": {
            "test_fixture_only": True,
            "auto_injected_into_player_decks": False,
            "bot_playbook_enabled": False,
            "ui_deck_library_mutated": False,
            "game_state_mutated": False,
        },
    }


def build_runtime_fixture_promotion_gate(
    accepted_report: dict[str, Any] | None = None,
    rerun_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    accepted_report = accepted_report or load_json(M38_03_ACCEPTED)
    rerun_report = rerun_report or load_json(M37_05_RERUN)
    accepted = accepted_report["accepted_recipe"]
    summary = accepted_report["summary"]
    consistency_status = rerun_report["accepted_seed_after"].get("consistency_status", "")

    checks = [
        _check(
            "human_acceptance",
            bool(summary.get("human_acceptance_cleared")),
            {"accepted_by": accepted_report["acceptance_record"].get("accepted_by", ""), "decision": accepted_report["acceptance_record"].get("decision", "")},
        ),
        _check(
            "grade_profile_review",
            bool(summary.get("grade_profile_review_cleared"))
            and summary.get("grade_counts") == {"0": 17, "1": 14, "2": 11, "3": 8},
            {"grade_counts": summary.get("grade_counts", {})},
        ),
        _check(
            "validation",
            int(summary.get("blocking_issue_count", 0)) == 0
            and int(summary.get("main_deck_count", 0)) == 50
            and int(summary.get("trigger_count", 0)) == 16
            and accepted.get("validation_status") == "accepted_review_artifact_ready_for_runtime_gate",
            {
                "blocking_issue_count": summary.get("blocking_issue_count"),
                "main_deck_count": summary.get("main_deck_count"),
                "trigger_count": summary.get("trigger_count"),
                "validation_status": accepted.get("validation_status", ""),
            },
        ),
        _check(
            "combo_consistency",
            consistency_status in {"consistent_pending_human_acceptance", "consistent_runtime_candidate"},
            {"source_consistency_status": consistency_status},
        ),
        _check(
            "runtime_boundary",
            not bool(summary.get("runtime_promotion_allowed"))
            and not bool(accepted_report["scope"].get("creates_runtime_deck"))
            and not bool(accepted_report["scope"].get("bot_integration")),
            {
                "m38_03_runtime_promotion_allowed": summary.get("runtime_promotion_allowed"),
                "m38_03_creates_runtime_deck": accepted_report["scope"].get("creates_runtime_deck"),
                "m38_03_bot_integration": accepted_report["scope"].get("bot_integration"),
            },
        ),
    ]
    promotion_allowed = _all_checks_pass(checks)
    fixture = build_runtime_fixture(accepted_report) if promotion_allowed else None

    return {
        "version": "M38-04",
        "description": "Runtime fixture promotion gate for the first accepted recipe",
        "source_inputs": {
            "human_accepted_recipe_artifact": str(M38_03_ACCEPTED.relative_to(ROOT)),
            "revised_recipe_validation_rerun": str(M37_05_RERUN.relative_to(ROOT)),
        },
        "scope": {
            "offline_promotion_gate": True,
            "creates_runtime_test_fixture_artifact": promotion_allowed,
            "mutates_runtime_deck_library": False,
            "bot_integration": False,
            "automatic_playbook_promotion": False,
            "live_card_text_parsing": False,
            "GameState_mutation": False,
        },
        "gate_policy": {
            "requires_human_acceptance": True,
            "requires_grade_profile_clear": True,
            "requires_zero_blockers": True,
            "requires_combo_consistency": True,
            "fixture_only_not_live_deck": True,
        },
        "gate_checks": checks,
        "promotion_decision": {
            "promotion_allowed": promotion_allowed,
            "fixture_created": promotion_allowed,
            "fixture_path": "outputs/target_slice/runtime_fixtures/recipe_003_classic_core_nova_grappler_m38_04.json"
            if promotion_allowed
            else "",
            "runtime_deck_library_mutated": False,
            "bot_playbook_enabled": False,
            "next_action": "M38-closeout" if promotion_allowed else "repair_failed_gate_checks",
        },
        "runtime_fixture": fixture,
        "summary": {
            "recipe_id": summary.get("recipe_id", ""),
            "promotion_allowed": promotion_allowed,
            "passed_check_count": sum(1 for item in checks if item["passed"]),
            "failed_check_count": sum(1 for item in checks if not item["passed"]),
            "fixture_created": promotion_allowed,
            "ready_for_m38_closeout": promotion_allowed,
        },
        "next_target": {
            "milestone": "M38-closeout",
            "task": "First runtime fixture closeout",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["promotion_decision"]
    lines = [
        "# M38-04 Runtime Fixture Promotion Gate",
        "",
        "## Summary",
        "",
        f"- Recipe: `{summary['recipe_id']}`",
        f"- Promotion allowed: `{summary['promotion_allowed']}`",
        f"- Passed checks: `{summary['passed_check_count']}`",
        f"- Failed checks: `{summary['failed_check_count']}`",
        f"- Fixture created: `{summary['fixture_created']}`",
        f"- Ready for M38-closeout: `{summary['ready_for_m38_closeout']}`",
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
            f"- Bot playbook enabled: `{decision['bot_playbook_enabled']}`",
            "",
            "## Policy",
            "",
            "- This gate may create an offline runtime/test fixture artifact only.",
            "- It does not inject a player deck.",
            "- It does not enable bot playbooks.",
            "- It does not mutate GameState.",
            "",
            "## Next",
            "",
            "`M38-closeout`: First runtime fixture closeout.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M38-04 runtime fixture promotion gate.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    parser.add_argument("--fixture-dir", type=Path, default=FIXTURE_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_runtime_fixture_promotion_gate()
    json_path = args.output_dir / "m38_04_runtime_fixture_promotion_gate.json"
    md_path = args.output_dir / "m38_04_runtime_fixture_promotion_gate.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    fixture = report.get("runtime_fixture")
    if fixture:
        fixture_path = args.fixture_dir / "recipe_003_classic_core_nova_grappler_m38_04.json"
        write_json(fixture, fixture_path)
    print(f"M38-04 runtime fixture promotion gate wrote {json_path}")
    print(f"M38-04 runtime fixture promotion gate summary wrote {md_path}")
    if fixture:
        print(f"M38-04 runtime fixture wrote {fixture_path}")
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
