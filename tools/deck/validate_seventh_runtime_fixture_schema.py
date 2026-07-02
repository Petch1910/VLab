"""Validate M62-01 seventh runtime fixture schema and system boundaries."""

from __future__ import annotations

import argparse
import copy
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
DEFAULT_FIXTURE = OUTPUT_DIR / "runtime_fixtures" / "m60_recipe_001_neo_nectar_m61_06.json"
EXPECTED_G_ZONE_OPTION = "main_deck_only_review_no_runtime_promotion"
EXPECTED_BLOOM_TOKEN_OPTION = "manual_semantic_review_only_no_runtime_promotion"


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


def _normalize_fixture_for_generic_validator(fixture: dict[str, Any]) -> dict[str, Any]:
    """Keep M61-06 plural source metadata compatible with the shared v1 validator."""

    normalized = copy.deepcopy(fixture)
    if "source_artifact" not in normalized and "source_artifacts" in normalized:
        normalized["source_artifact"] = normalized["source_artifacts"]
    return normalized


def _append_system_boundary_issues(report: dict[str, Any], fixture: dict[str, Any]) -> None:
    issues = report["issues"]
    source_packages = fixture.get("source_packages", {})
    system_boundaries = fixture.get("system_boundaries", {})
    runtime_boundaries = fixture.get("runtime_boundaries", {})
    count_summary = fixture.get("count_summary", {})

    expected_source_packages = {
        "selected_g_zone_option_id": EXPECTED_G_ZONE_OPTION,
        "selected_bloom_token_option_id": EXPECTED_BLOOM_TOKEN_OPTION,
    }
    for field, expected in expected_source_packages.items():
        if source_packages.get(field) != expected:
            issues.append(
                _issue(
                    "system_policy_mismatch",
                    "Seventh fixture must come from the accepted main-deck-only system boundary options.",
                    {"field": field, "expected": expected, "actual": source_packages.get(field)},
                )
            )

    expected_system_flags = {
        "selected_g_zone_option_id": EXPECTED_G_ZONE_OPTION,
        "selected_bloom_token_option_id": EXPECTED_BLOOM_TOKEN_OPTION,
        "main_deck_only_validation_allowed": True,
        "g_zone_boundary_applied": True,
        "bloom_token_boundary_applied": True,
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
    }
    for field, expected in expected_system_flags.items():
        if system_boundaries.get(field) != expected:
            issues.append(
                _issue(
                    "system_boundary_violation",
                    "Seventh fixture system boundary has an unsafe value.",
                    {"field": field, "expected": expected, "actual": system_boundaries.get(field)},
                )
            )

    runtime_expected = {
        "g_zone_runtime_enabled": False,
        "stride_runtime_enabled": False,
        "bloom_token_runtime_enabled": False,
    }
    for field, expected in runtime_expected.items():
        if runtime_boundaries.get(field) != expected:
            issues.append(
                _issue(
                    "system_runtime_boundary_violation",
                    "Seventh fixture runtime boundary must keep advanced systems disabled.",
                    {"field": field, "expected": expected, "actual": runtime_boundaries.get(field)},
                )
            )

    if count_summary.get("grade4_main_deck_count") != 0:
        issues.append(
            _issue(
                "grade4_main_deck_count_violation",
                "Seventh fixture main deck must not include G Units or grade 4 cards.",
                {"expected": 0, "actual": count_summary.get("grade4_main_deck_count")},
            )
        )


def _refresh_summary(report: dict[str, Any]) -> None:
    blocker_count = sum(1 for issue in report["issues"] if issue["severity"] == "blocker")
    report["summary"]["blocking_issue_count"] = blocker_count
    report["summary"]["issue_count"] = len(report["issues"])
    report["summary"]["schema_valid"] = blocker_count == 0


def build_seventh_runtime_fixture_schema_validation_report(
    fixture: dict[str, Any] | None = None,
    fixture_path: Path = DEFAULT_FIXTURE,
) -> dict[str, Any]:
    fixture = fixture or load_json(fixture_path)
    report = validate_fixture_schema(_normalize_fixture_for_generic_validator(fixture))
    report["version"] = "M62-01"
    report["description"] = "Seventh runtime fixture schema validation"
    report["source_inputs"] = {
        "runtime_fixture": _display_path(fixture_path),
        "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
    }
    report["scope"]["seventh_fixture_schema_validator"] = True
    report["scope"]["system_boundary_enforced"] = True
    report["scope"]["g_zone_stride_boundary_enforced"] = True
    report["scope"]["bloom_token_boundary_enforced"] = True
    report["validation_policy"]["system_boundary"] = {
        "selected_g_zone_option_id": EXPECTED_G_ZONE_OPTION,
        "selected_bloom_token_option_id": EXPECTED_BLOOM_TOKEN_OPTION,
        "main_deck_only_validation_allowed": True,
        "g_zone_boundary_applied": True,
        "bloom_token_boundary_applied": True,
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
        "grade4_main_deck_count": 0,
        "accepts_m61_06_source_artifacts_plural": True,
    }
    _append_system_boundary_issues(report, fixture)
    _refresh_summary(report)
    report["summary"].pop("ready_for_m39_02", None)
    report["summary"]["ready_for_m62_02"] = report["summary"]["schema_valid"]
    report["next_target"] = {
        "milestone": "M62-02",
        "task": "Seventh fixture deck text exporter",
    }
    return report


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    fixture = report["fixture_summary"]
    policy = report["validation_policy"]["system_boundary"]
    lines = [
        "# M62-01 Seventh Runtime Fixture Schema Validation",
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
        f"- Ready for M62-02: `{summary['ready_for_m62_02']}`",
        "",
        "## System Boundary",
        "",
        f"- Selected G Zone option: `{policy['selected_g_zone_option_id']}`",
        f"- Selected Bloom/token option: `{policy['selected_bloom_token_option_id']}`",
        f"- G Zone runtime enabled: `{policy['g_zone_runtime_enabled']}`",
        f"- Stride runtime enabled: `{policy['stride_runtime_enabled']}`",
        f"- Bloom/token runtime enabled: `{policy['bloom_token_runtime_enabled']}`",
        f"- Tokens allowed in main deck: `{policy['tokens_allowed_in_main_deck']}`",
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
            "- It does not enable G Zone, Stride, Bloom/token, or token runtime.",
            "- It does not mutate GameState.",
            "",
            "## Next",
            "",
            "`M62-02`: Seventh fixture deck text exporter.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate M62-01 seventh runtime fixture schema.")
    parser.add_argument("--fixture", type=Path, default=DEFAULT_FIXTURE)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    fixture_path = args.fixture
    fixture = load_json(fixture_path)
    report = build_seventh_runtime_fixture_schema_validation_report(fixture, fixture_path)
    json_path = args.output_dir / "m62_01_seventh_fixture_schema_validation.json"
    md_path = args.output_dir / "m62_01_seventh_fixture_schema_validation.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M62-01 seventh fixture schema validation wrote {json_path}")
    print(f"M62-01 seventh fixture schema validation summary wrote {md_path}")
    print(
        "schema_valid={valid} blockers={blockers} next={next_ready}".format(
            valid=report["summary"]["schema_valid"],
            blockers=report["summary"]["blocking_issue_count"],
            next_ready=report["summary"]["ready_for_m62_02"],
        )
    )
    return 0 if report["summary"]["schema_valid"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
