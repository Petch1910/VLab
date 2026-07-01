"""Refresh first-slice feasibility after target selection and deck fixtures.

M35-A4 of the Hybrid Vertical-Slice Strategy.

This rollup decides whether the selected slice can move from Phase A
foundation work into Phase B semantic tagging. It does not claim full official
deck legality.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
SELECTED_SLICE_REPORT = ROOT / "outputs" / "target_slice" / "m35_a2_first_target_slice_report.json"
FIXTURES_REPORT = ROOT / "outputs" / "target_slice" / "m35_a3_first_slice_deck_legality_fixtures.json"
OUTPUT_DIR = ROOT / "outputs" / "target_slice"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def reason_prefix_present(reasons: Sequence[str], prefix: str) -> bool:
    return any(reason.startswith(prefix) for reason in reasons)


def build_fixture_summary(fixtures_report: dict[str, Any]) -> dict[str, Any]:
    fixtures = fixtures_report.get("fixtures", [])
    expected_failures: dict[str, bool] = {
        "short_main": False,
        "trigger_count": False,
        "missing_setup_grade": False,
        "copy_limit": False,
        "identity_mismatch": False,
    }
    for fixture in fixtures:
        reasons = fixture.get("validation", {}).get("reasons", [])
        fixture_id = fixture.get("fixture_id", "")
        if "short_main" in fixture_id and reason_prefix_present(reasons, "main_count"):
            expected_failures["short_main"] = True
        if "bad_trigger_count" in fixture_id and reason_prefix_present(reasons, "trigger_count"):
            expected_failures["trigger_count"] = True
        if "missing_grade" in fixture_id and reason_prefix_present(reasons, "missing_setup_grade"):
            expected_failures["missing_setup_grade"] = True
        if "copy_limit" in fixture_id and reason_prefix_present(reasons, "copy_limit_exceeded"):
            expected_failures["copy_limit"] = True
        if "identity_mismatch" in fixture_id and reason_prefix_present(reasons, "identity_mismatch"):
            expected_failures["identity_mismatch"] = True

    valid_fixture = next(
        (
            fixture
            for fixture in fixtures
            if fixture.get("fixture_id") == "classic_core_selected_group_valid_minimal"
        ),
        None,
    )
    valid_accepts = bool(valid_fixture and valid_fixture.get("validation", {}).get("accepted"))
    return {
        "fixture_count": len(fixtures),
        "all_expectations_met": bool(fixtures_report.get("all_expectations_met")),
        "valid_fixture_accepts": valid_accepts,
        "expected_failure_coverage": expected_failures,
        "expected_failure_coverage_complete": all(expected_failures.values()),
    }


def build_report(
    selected_report: dict[str, Any] | None = None,
    fixtures_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    selected_report = selected_report or load_json(SELECTED_SLICE_REPORT)
    fixtures_report = fixtures_report or load_json(FIXTURES_REPORT)

    count_evidence = selected_report["format_policy"]["current_count_evidence"]
    fixture_summary = build_fixture_summary(fixtures_report)
    missing_set_codes = selected_report["input_evidence"].get("missing_set_codes", [])
    missing_taxonomy_terms = selected_report["taxonomy_gap_report"].get("missing_required_terms", [])

    capacity_ready = (
        not missing_set_codes
        and bool(count_evidence.get("basic_50_card_main"))
        and bool(count_evidence.get("main_with_16_triggers_34_non_triggers"))
        and bool(count_evidence.get("ride_deck_grade_0_1_2_3_choice"))
    )
    taxonomy_ready = not missing_taxonomy_terms
    legality_fixture_ready = (
        fixture_summary["all_expectations_met"]
        and fixture_summary["valid_fixture_accepts"]
        and fixture_summary["expected_failure_coverage_complete"]
    )

    blocking_gaps: list[str] = []
    if not capacity_ready:
        blocking_gaps.append("capacity_or_set_scope_not_ready")
    if not taxonomy_ready:
        blocking_gaps.append("required_taxonomy_terms_missing")
    if not legality_fixture_ready:
        blocking_gaps.append("minimal_legality_fixtures_not_ready")

    deferred_before_full_legality = [
        "official heal trigger maximum source fixture",
        "format-wide copy-limit exceptions beyond runtime deck_limit",
        "full official deck construction source citations",
    ]

    phase_a_ready_for_phase_b = not blocking_gaps
    return {
        "version": "M35-A4",
        "description": "First-slice feasibility refresh after target selection and minimal legality fixtures",
        "selected_target": selected_report["selected_target"],
        "input_evidence": {
            "m35_a2_report": str(SELECTED_SLICE_REPORT.relative_to(ROOT)),
            "m35_a3_fixtures": str(FIXTURES_REPORT.relative_to(ROOT)),
        },
        "capacity": {
            "ready": capacity_ready,
            "card_count": count_evidence.get("card_count", 0),
            "main_capacity": count_evidence.get("main_capacity", 0),
            "trigger_capacity": count_evidence.get("trigger_capacity", 0),
            "non_trigger_capacity": count_evidence.get("non_trigger_capacity", 0),
            "grade_unique_count": count_evidence.get("grade_unique_count", {}),
            "trigger_capacity_by_type": count_evidence.get("trigger_capacity_by_type", {}),
            "missing_set_codes": missing_set_codes,
        },
        "taxonomy": {
            "ready": taxonomy_ready,
            "missing_required_terms": missing_taxonomy_terms,
            "unsupported_modules_first_slice": selected_report["taxonomy_gap_report"].get(
                "unsupported_modules_for_first_slice", []
            ),
        },
        "legality_fixtures": {
            "ready": legality_fixture_ready,
            **fixture_summary,
        },
        "missing_rule_gate": {
            "blocking_gaps_for_phase_b": blocking_gaps,
            "deferred_before_full_legality": deferred_before_full_legality,
            "not_a_full_official_legality_claim": True,
        },
        "phase_a_ready_for_phase_b": phase_a_ready_for_phase_b,
        "recommended_next": "M35-B1" if phase_a_ready_for_phase_b else "fix_phase_a_gaps",
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    blockers = report["missing_rule_gate"]["blocking_gaps_for_phase_b"]
    lines = [
        "# M35-A4 First Slice Feasibility Refresh",
        "",
        "## Selected Target",
        "",
        f"- Slice: `{target['slice']}`",
        f"- Era preset: `{target['era_preset']}`",
        f"- Group: `{target['group']}`",
        "",
        "## Readiness",
        "",
        f"- Capacity ready: `{report['capacity']['ready']}`",
        f"- Taxonomy ready: `{report['taxonomy']['ready']}`",
        f"- Legality fixture ready: `{report['legality_fixtures']['ready']}`",
        f"- Phase A ready for Phase B: `{report['phase_a_ready_for_phase_b']}`",
        "",
        "## Blocking Gaps For Phase B",
        "",
        f"`{', '.join(blockers) if blockers else 'none'}`",
        "",
        "## Deferred Before Full Legality",
        "",
    ]
    for item in report["missing_rule_gate"]["deferred_before_full_legality"]:
        lines.append(f"- {item}")
    lines.extend(
        [
            "",
            "## Recommended Next",
            "",
            f"`{report['recommended_next']}`",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Refresh selected first-slice feasibility for M35-A4.")
    parser.add_argument("--selected-report", type=Path, default=SELECTED_SLICE_REPORT)
    parser.add_argument("--fixtures-report", type=Path, default=FIXTURES_REPORT)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    selected_report = load_json(args.selected_report)
    fixtures_report = load_json(args.fixtures_report)
    report = build_report(selected_report, fixtures_report)
    json_path = args.output_dir / "m35_a4_first_slice_feasibility_refresh.json"
    md_path = args.output_dir / "m35_a4_first_slice_feasibility_refresh.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35-A4 feasibility refresh wrote {json_path}")
    print(f"M35-A4 feasibility summary wrote {md_path}")
    print(f"phase_a_ready_for_phase_b={report['phase_a_ready_for_phase_b']}")
    return 0 if report["phase_a_ready_for_phase_b"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
