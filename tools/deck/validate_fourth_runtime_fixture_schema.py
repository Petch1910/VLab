"""Validate M50-01 fourth runtime fixture schema and G Zone boundary."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.validate_runtime_fixture_schema import (  # noqa: E402
    CARDS_SQLITE,
    validate_fixture_schema,
)


OUTPUT_DIR = ROOT / "outputs" / "target_slice"
DEFAULT_FIXTURE = OUTPUT_DIR / "runtime_fixtures" / "m48_recipe_001_g_series_first_royal_paladin_m49_05.json"
EXPECTED_G_ZONE_OPTION = "main_deck_only_for_current_windows_fixture"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _display_path(path: Path) -> str:
    resolved = path.resolve()
    try:
        return str(resolved.relative_to(ROOT))
    except ValueError:
        return str(path)


def _issue(code: str, message: str, details: dict[str, Any] | None = None) -> dict[str, Any]:
    return {
        "code": code,
        "severity": "blocker",
        "message": message,
        "details": details or {},
    }


def _append_g_zone_boundary_issues(report: dict[str, Any], fixture: dict[str, Any]) -> None:
    issues = report["issues"]
    format_policy = fixture.get("format_policy", {})
    g_zone_boundary = fixture.get("g_zone_boundary", {})
    runtime_boundaries = fixture.get("runtime_boundaries", {})
    count_summary = fixture.get("count_summary", {})

    if format_policy.get("g_zone_boundary") != EXPECTED_G_ZONE_OPTION:
        issues.append(
            _issue(
                "g_zone_policy_mismatch",
                "Fourth fixture must stay on the accepted main-deck-only G Zone policy.",
                {
                    "expected": EXPECTED_G_ZONE_OPTION,
                    "actual": format_policy.get("g_zone_boundary"),
                },
            )
        )

    expected_flags = {
        "selected_option_id": EXPECTED_G_ZONE_OPTION,
        "main_deck_only_validation_allowed": True,
        "g_zone_runtime_enabled": False,
        "stride_runtime_enabled": False,
        "grade4_main_deck_allowed": False,
        "g_units_allowed_in_main_deck": False,
    }
    for field, expected in expected_flags.items():
        if g_zone_boundary.get(field) != expected:
            issues.append(
                _issue(
                    "g_zone_boundary_violation",
                    "Fourth fixture G Zone boundary has an unsafe value.",
                    {"field": field, "expected": expected, "actual": g_zone_boundary.get(field)},
                )
            )

    runtime_expected = {
        "g_zone_runtime_enabled": False,
        "stride_runtime_enabled": False,
    }
    for field, expected in runtime_expected.items():
        if runtime_boundaries.get(field) != expected:
            issues.append(
                _issue(
                    "g_zone_runtime_boundary_violation",
                    "Fourth fixture runtime boundary must keep G Zone and Stride disabled.",
                    {"field": field, "expected": expected, "actual": runtime_boundaries.get(field)},
                )
            )

    if count_summary.get("grade4_main_deck_count") != 0:
        issues.append(
            _issue(
                "grade4_main_deck_count_violation",
                "Fourth fixture main deck must not include G Units or grade 4 cards.",
                {"expected": 0, "actual": count_summary.get("grade4_main_deck_count")},
            )
        )


def _refresh_summary(report: dict[str, Any]) -> None:
    blocker_count = sum(1 for issue in report["issues"] if issue["severity"] == "blocker")
    report["summary"]["blocking_issue_count"] = blocker_count
    report["summary"]["issue_count"] = len(report["issues"])
    report["summary"]["schema_valid"] = blocker_count == 0


def build_fourth_runtime_fixture_schema_validation_report(
    fixture: dict[str, Any] | None = None,
    fixture_path: Path = DEFAULT_FIXTURE,
) -> dict[str, Any]:
    fixture = fixture or load_json(fixture_path)
    report = validate_fixture_schema(fixture)
    report["version"] = "M50-01"
    report["description"] = "Fourth runtime fixture schema validation"
    report["source_inputs"] = {
        "runtime_fixture": _display_path(fixture_path),
        "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
    }
    report["scope"]["fourth_fixture_schema_validator"] = True
    report["scope"]["g_zone_boundary_enforced"] = True
    report["validation_policy"]["g_zone_boundary"] = {
        "selected_option_id": EXPECTED_G_ZONE_OPTION,
        "main_deck_only_validation_allowed": True,
        "g_zone_runtime_enabled": False,
        "stride_runtime_enabled": False,
        "grade4_main_deck_allowed": False,
        "g_units_allowed_in_main_deck": False,
        "grade4_main_deck_count": 0,
    }
    _append_g_zone_boundary_issues(report, fixture)
    _refresh_summary(report)
    report["summary"].pop("ready_for_m39_02", None)
    report["summary"]["ready_for_m50_02"] = report["summary"]["schema_valid"]
    report["next_target"] = {
        "milestone": "M50-02",
        "task": "Fourth fixture deck text exporter",
    }
    return report


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    fixture = report["fixture_summary"]
    policy = report["validation_policy"]["g_zone_boundary"]
    lines = [
        "# M50-01 Fourth Runtime Fixture Schema Validation",
        "",
        "## Summary",
        "",
        f"- Fixture: `{fixture['fixture_id']}`",
        f"- Recipe: `{fixture['recipe_id']}`",
        f"- Schema valid: `{summary['schema_valid']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        f"- Main deck count: `{fixture['main_deck_count']}`",
        f"- Unique card count: `{fixture['unique_card_count']}`",
        f"- Trigger counts: `{fixture['trigger_counts']}`",
        f"- Grade counts: `{fixture['grade_counts']}`",
        f"- Ready for M50-02: `{summary['ready_for_m50_02']}`",
        "",
        "## G Zone Boundary",
        "",
        f"- Selected option: `{policy['selected_option_id']}`",
        f"- G Zone runtime enabled: `{policy['g_zone_runtime_enabled']}`",
        f"- Stride runtime enabled: `{policy['stride_runtime_enabled']}`",
        f"- Grade 4 main deck count: `{policy['grade4_main_deck_count']}`",
        "",
        "## Issues",
        "",
    ]
    if report["issues"]:
        for issue in report["issues"]:
            lines.append(f"- `{issue['code']}` severity=`{issue['severity']}` details=`{issue['details']}`")
    else:
        lines.append("- None")
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Validator is offline only.",
            "- It does not mutate the fixture artifact.",
            "- It does not inject saved decks.",
            "- It does not publish UI deck lists.",
            "- It does not enable bot playbooks.",
            "- It does not enable G Zone or Stride runtime.",
            "- It does not mutate GameState.",
            "",
            "## Next",
            "",
            "`M50-02`: Fourth fixture deck text exporter.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate M50-01 fourth runtime fixture schema.")
    parser.add_argument("--fixture", type=Path, default=DEFAULT_FIXTURE)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    fixture_path = args.fixture
    fixture = load_json(fixture_path)
    report = build_fourth_runtime_fixture_schema_validation_report(fixture, fixture_path)
    json_path = args.output_dir / "m50_01_fourth_fixture_schema_validation.json"
    md_path = args.output_dir / "m50_01_fourth_fixture_schema_validation.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M50-01 fourth fixture schema validation wrote {json_path}")
    print(f"M50-01 fourth fixture schema validation summary wrote {md_path}")
    print(
        "schema_valid={valid} blockers={blockers} next={next_ready}".format(
            valid=report["summary"]["schema_valid"],
            blockers=report["summary"]["blocking_issue_count"],
            next_ready=report["summary"]["ready_for_m50_02"],
        )
    )
    return 0 if report["summary"]["schema_valid"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
