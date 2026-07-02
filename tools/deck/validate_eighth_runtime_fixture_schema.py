"""Validate M66-01 eighth runtime fixture schema and Lock / Legion boundary."""

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
DEFAULT_FIXTURE = OUTPUT_DIR / "runtime_fixtures" / "m64_recipe_001_kagero_m65_06.json"
EXPECTED_LOCK_OPTION = "main_deck_only_review_no_runtime_promotion"
EXPECTED_LEGION_OPTION = "main_deck_only_review_no_runtime_promotion"


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
    """Keep M65-06 plural source metadata compatible with the shared v1 validator."""

    normalized = copy.deepcopy(fixture)
    if "source_artifact" not in normalized and "source_artifacts" in normalized:
        normalized["source_artifact"] = normalized["source_artifacts"]
    return normalized


def _append_lock_legion_boundary_issues(report: dict[str, Any], fixture: dict[str, Any]) -> None:
    issues = report["issues"]
    source_packages = fixture.get("source_packages", {})
    system_boundaries = fixture.get("system_boundaries", {})
    runtime_boundaries = fixture.get("runtime_boundaries", {})
    count_summary = fixture.get("count_summary", {})

    expected_source_packages = {
        "selected_lock_option_id": EXPECTED_LOCK_OPTION,
        "selected_legion_option_id": EXPECTED_LEGION_OPTION,
    }
    for field, expected in expected_source_packages.items():
        if source_packages.get(field) != expected:
            issues.append(
                _issue(
                    "system_policy_mismatch",
                    "Eighth fixture must come from the accepted main-deck-only Lock and Legion options.",
                    {"field": field, "expected": expected, "actual": source_packages.get(field)},
                )
            )

    expected_system_flags = {
        "selected_lock_option_id": EXPECTED_LOCK_OPTION,
        "selected_legion_option_id": EXPECTED_LEGION_OPTION,
        "main_deck_only_validation_allowed": True,
        "lock_boundary_applied": True,
        "legion_boundary_applied": True,
        "lock_runtime_enabled": False,
        "unlock_runtime_enabled": False,
        "locked_card_visibility_enabled": False,
        "lock_circle_state_enabled": False,
        "unlock_timing_runtime_enabled": False,
        "legion_runtime_enabled": False,
        "mate_identity_check_enabled": False,
        "legion_state_enabled": False,
        "legion_attack_timing_enabled": False,
        "legion_deck_building_validation_enabled": False,
    }
    for field, expected in expected_system_flags.items():
        if system_boundaries.get(field) != expected:
            issues.append(
                _issue(
                    "system_boundary_violation",
                    "Eighth fixture Lock / Legion system boundary has an unsafe value.",
                    {"field": field, "expected": expected, "actual": system_boundaries.get(field)},
                )
            )

    runtime_expected = {
        "lock_runtime_enabled": False,
        "unlock_runtime_enabled": False,
        "legion_runtime_enabled": False,
        "mate_identity_check_enabled": False,
    }
    for field, expected in runtime_expected.items():
        if runtime_boundaries.get(field) != expected:
            issues.append(
                _issue(
                    "system_runtime_boundary_violation",
                    "Eighth fixture runtime boundary must keep Lock, Unlock, Legion, and Mate disabled.",
                    {"field": field, "expected": expected, "actual": runtime_boundaries.get(field)},
                )
            )

    if count_summary.get("grade4_main_deck_count") != 0:
        issues.append(
            _issue(
                "grade4_main_deck_count_violation",
                "Eighth fixture main deck must not include grade 4 cards.",
                {"expected": 0, "actual": count_summary.get("grade4_main_deck_count")},
            )
        )


def _refresh_summary(report: dict[str, Any]) -> None:
    blocker_count = sum(1 for issue in report["issues"] if issue["severity"] == "blocker")
    report["summary"]["blocking_issue_count"] = blocker_count
    report["summary"]["issue_count"] = len(report["issues"])
    report["summary"]["schema_valid"] = blocker_count == 0


def build_eighth_runtime_fixture_schema_validation_report(
    fixture: dict[str, Any] | None = None,
    fixture_path: Path = DEFAULT_FIXTURE,
) -> dict[str, Any]:
    fixture = fixture or load_json(fixture_path)
    report = validate_fixture_schema(_normalize_fixture_for_generic_validator(fixture))
    report["version"] = "M66-01"
    report["description"] = "Eighth runtime fixture schema validation"
    report["source_inputs"] = {
        "runtime_fixture": _display_path(fixture_path),
        "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
    }
    report["scope"]["eighth_fixture_schema_validator"] = True
    report["scope"]["system_boundary_enforced"] = True
    report["scope"]["lock_unlock_boundary_enforced"] = True
    report["scope"]["legion_mate_boundary_enforced"] = True
    report["validation_policy"]["system_boundary"] = {
        "selected_lock_option_id": EXPECTED_LOCK_OPTION,
        "selected_legion_option_id": EXPECTED_LEGION_OPTION,
        "main_deck_only_validation_allowed": True,
        "lock_boundary_applied": True,
        "legion_boundary_applied": True,
        "lock_runtime_enabled": False,
        "unlock_runtime_enabled": False,
        "locked_card_visibility_enabled": False,
        "lock_circle_state_enabled": False,
        "unlock_timing_runtime_enabled": False,
        "legion_runtime_enabled": False,
        "mate_identity_check_enabled": False,
        "legion_state_enabled": False,
        "legion_attack_timing_enabled": False,
        "legion_deck_building_validation_enabled": False,
        "grade4_main_deck_count": 0,
        "accepts_m65_06_source_artifacts_plural": True,
    }
    _append_lock_legion_boundary_issues(report, fixture)
    _refresh_summary(report)
    report["summary"].pop("ready_for_m39_02", None)
    report["summary"]["ready_for_m66_02"] = report["summary"]["schema_valid"]
    report["next_target"] = {
        "milestone": "M66-02",
        "task": "Eighth fixture deck text exporter",
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
        "# M66-01 Eighth Runtime Fixture Schema Validation",
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
        f"- Ready for M66-02: `{summary['ready_for_m66_02']}`",
        "",
        "## Lock / Legion Boundary",
        "",
        f"- Selected Lock option: `{policy['selected_lock_option_id']}`",
        f"- Selected Legion option: `{policy['selected_legion_option_id']}`",
        f"- Lock runtime enabled: `{policy['lock_runtime_enabled']}`",
        f"- Unlock runtime enabled: `{policy['unlock_runtime_enabled']}`",
        f"- Legion runtime enabled: `{policy['legion_runtime_enabled']}`",
        f"- Mate identity check enabled: `{policy['mate_identity_check_enabled']}`",
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
            "- It does not enable Lock, Unlock, Legion, or Mate runtime.",
            "- It does not mutate GameState.",
            "",
            "## Next",
            "",
            "`M66-02`: Eighth fixture deck text exporter.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate M66-01 eighth runtime fixture schema.")
    parser.add_argument("--fixture", type=Path, default=DEFAULT_FIXTURE)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    fixture_path = args.fixture
    fixture = load_json(fixture_path)
    report = build_eighth_runtime_fixture_schema_validation_report(fixture, fixture_path)
    json_path = args.output_dir / "m66_01_eighth_fixture_schema_validation.json"
    md_path = args.output_dir / "m66_01_eighth_fixture_schema_validation.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M66-01 eighth fixture schema validation wrote {json_path}")
    print(f"M66-01 eighth fixture schema validation summary wrote {md_path}")
    print(
        "schema_valid={valid} blockers={blockers} next={next_ready}".format(
            valid=report["summary"]["schema_valid"],
            blockers=report["summary"]["blocking_issue_count"],
            next_ready=report["summary"]["ready_for_m66_02"],
        )
    )
    return 0 if report["summary"]["schema_valid"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
