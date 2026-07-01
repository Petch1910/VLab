"""Build the M53-05 fifth-slice runtime fixture promotion gate."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
FIXTURE_DIR = OUTPUT_DIR / "runtime_fixtures"
M53_04_RERUN = OUTPUT_DIR / "m53_04_fifth_slice_repaired_recipe_validation_rerun.json"


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


def _fixture_id(recipe_id: str) -> str:
    return f"runtime_fixture_{recipe_id}_gold_paladin_m53_05"


def build_runtime_fixture(rerun_report: dict[str, Any]) -> dict[str, Any]:
    validation = rerun_report["repaired_recipe_validation"]
    recipe_id = validation["recipe_id"]
    target = rerun_report.get("selected_target", {})
    main_deck = [
        {
            "card_id": row["card_id"],
            "quantity": int(row["quantity"]),
        }
        for row in rerun_report.get("repaired_recipe_rows", [])
    ]
    count_summary = validation["count_summary"]
    return {
        "schema_version": "deck_recipe_runtime_fixture_v1",
        "fixture_id": _fixture_id(recipe_id),
        "fixture_scope": "offline_runtime_test_fixture",
        "recipe_id": recipe_id,
        "source_artifact": "outputs/target_slice/m53_04_fifth_slice_repaired_recipe_validation_rerun.json",
        "selected_target": target,
        "format_policy": {
            "slice": target.get("slice", ""),
            "era_preset": target.get("era_preset", ""),
            "group": target.get("group", ""),
            "group_field": target.get("group_field", ""),
        },
        "main_deck": main_deck,
        "count_summary": {
            "main_deck_count": int(count_summary["explicit_card_count"]),
            "trigger_count": int(count_summary["trigger_count"]),
            "trigger_counts": dict(count_summary["trigger_counts"]),
            "grade_counts": dict(count_summary["grade_counts"]),
            "clan_counts": dict(count_summary["clan_counts"]),
        },
        "source_packages": {
            "selected_review_item_id": rerun_report["accepted_context"]["accepted_review_item_id"],
            "grade_profile_package_id": rerun_report["accepted_context"]["accepted_grade_profile_package_id"],
        },
        "runtime_boundaries": {
            "test_fixture_only": True,
            "auto_injected_into_player_decks": False,
            "bot_playbook_enabled": False,
            "ui_deck_library_mutated": False,
            "game_state_mutated": False,
        },
    }


def build_fifth_slice_runtime_fixture_promotion_gate(
    rerun_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    rerun_report = rerun_report or load_json(M53_04_RERUN)
    context = rerun_report["accepted_context"]
    validation = rerun_report["repaired_recipe_validation"]
    consistency = rerun_report["repaired_recipe_consistency"]
    summary = rerun_report["summary"]
    counts = validation["count_summary"]
    target_grade_counts = {"0": 17, "1": 14, "2": 11, "3": 8}

    checks = [
        _check(
            "human_selection_and_acceptance",
            bool(context.get("human_selection_recorded")) and bool(context.get("human_acceptance_recorded")),
            {
                "human_selection_recorded": context.get("human_selection_recorded"),
                "human_acceptance_recorded": context.get("human_acceptance_recorded"),
                "accepted_review_item_id": context.get("accepted_review_item_id", ""),
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
            and counts.get("grade_counts") == target_grade_counts,
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
            and bool(consistency.get("promotion_allowed_by_validation_and_consistency")),
            {
                "consistency_status": consistency.get("consistency_status"),
                "pair_cards_present": consistency.get("pair_cards_present"),
                "promotion_allowed_by_validation_and_consistency": consistency.get(
                    "promotion_allowed_by_validation_and_consistency"
                ),
            },
        ),
        _check(
            "rerun_ready",
            bool(summary.get("ready_for_m53_05")),
            {"ready_for_m53_05": summary.get("ready_for_m53_05")},
        ),
        _check(
            "runtime_boundary",
            not bool(summary.get("runtime_fixture_promotion_allowed"))
            and not bool(rerun_report["scope"].get("creates_runtime_fixture"))
            and not bool(rerun_report["scope"].get("bot_playbook"))
            and not bool(rerun_report["scope"].get("direct_GameState_mutation")),
            {
                "m53_04_runtime_fixture_promotion_allowed": summary.get("runtime_fixture_promotion_allowed"),
                "m53_04_creates_runtime_fixture": rerun_report["scope"].get("creates_runtime_fixture"),
                "m53_04_bot_playbook": rerun_report["scope"].get("bot_playbook"),
                "m53_04_direct_GameState_mutation": rerun_report["scope"].get("direct_GameState_mutation"),
            },
        ),
    ]
    promotion_allowed = _all_checks_pass(checks)
    fixture = build_runtime_fixture(rerun_report) if promotion_allowed else None
    recipe_id = validation["recipe_id"]
    fixture_filename = f"{recipe_id}_gold_paladin_m53_05.json"
    return {
        "version": "M53-05",
        "description": "Fifth-slice runtime fixture promotion gate",
        "source_inputs": {
            "m53_04_repaired_recipe_validation_rerun": str(M53_04_RERUN.relative_to(ROOT)),
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
            "requires_human_selection": True,
            "requires_human_acceptance": True,
            "requires_zero_blockers": True,
            "requires_combo_consistency": True,
            "requires_m53_04_ready": True,
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
            "next_action": "M53-closeout" if promotion_allowed else "repair_failed_gate_checks",
        },
        "runtime_fixture": fixture,
        "summary": {
            "recipe_id": recipe_id,
            "promotion_allowed": promotion_allowed,
            "passed_check_count": sum(1 for item in checks if item["passed"]),
            "failed_check_count": sum(1 for item in checks if not item["passed"]),
            "fixture_created": promotion_allowed,
            "ready_for_m53_closeout": promotion_allowed,
        },
        "next_target": {
            "milestone": "M53-closeout",
            "task": "Fifth-slice fixture closeout",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["promotion_decision"]
    lines = [
        "# M53-05 Fifth-Slice Runtime Fixture Promotion Gate",
        "",
        "## Summary",
        "",
        f"- Recipe: `{summary['recipe_id']}`",
        f"- Promotion allowed: `{summary['promotion_allowed']}`",
        f"- Passed checks: `{summary['passed_check_count']}`",
        f"- Failed checks: `{summary['failed_check_count']}`",
        f"- Fixture created: `{summary['fixture_created']}`",
        f"- Ready for M53-closeout: `{summary['ready_for_m53_closeout']}`",
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
            "- It does not publish UI deck lists.",
            "- It does not enable bot playbooks.",
            "- It does not mutate GameState.",
            "",
            "## Next",
            "",
            "`M53-closeout`: Fifth-slice fixture closeout.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M53-05 fifth-slice runtime fixture promotion gate.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    parser.add_argument("--fixture-dir", type=Path, default=FIXTURE_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_fifth_slice_runtime_fixture_promotion_gate()
    json_path = args.output_dir / "m53_05_fifth_slice_runtime_fixture_promotion_gate.json"
    md_path = args.output_dir / "m53_05_fifth_slice_runtime_fixture_promotion_gate.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    fixture = report.get("runtime_fixture")
    if fixture:
        fixture_path = args.fixture_dir / f"{report['summary']['recipe_id']}_gold_paladin_m53_05.json"
        write_json(fixture, fixture_path)
    print(f"M53-05 fifth-slice runtime fixture promotion gate wrote {json_path}")
    print(f"M53-05 fifth-slice runtime fixture promotion gate summary wrote {md_path}")
    if fixture:
        print(f"M53-05 fifth-slice runtime fixture wrote {fixture_path}")
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
