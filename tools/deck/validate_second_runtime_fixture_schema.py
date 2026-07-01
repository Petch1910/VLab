"""Validate M42-01 second runtime fixture schema and card counts."""

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
DEFAULT_FIXTURE = OUTPUT_DIR / "runtime_fixtures" / "m40_recipe_001_classic_core_oracle_think_tank_m41_04.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def build_second_runtime_fixture_schema_validation_report(
    fixture: dict[str, Any] | None = None,
    fixture_path: Path = DEFAULT_FIXTURE,
) -> dict[str, Any]:
    fixture = fixture or load_json(fixture_path)
    report = validate_fixture_schema(fixture)
    report["version"] = "M42-01"
    report["description"] = "Second runtime fixture schema validation"
    report["source_inputs"] = {
        "runtime_fixture": str(fixture_path.relative_to(ROOT) if fixture_path.is_absolute() else fixture_path),
        "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
    }
    report["scope"]["second_fixture_schema_validator"] = True
    report["summary"]["ready_for_m42_02"] = report["summary"].pop("ready_for_m39_02")
    report["next_target"] = {
        "milestone": "M42-02",
        "task": "Second fixture deck text exporter",
    }
    return report


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    fixture = report["fixture_summary"]
    lines = [
        "# M42-01 Second Runtime Fixture Schema Validation",
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
        f"- Ready for M42-02: `{summary['ready_for_m42_02']}`",
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
            "- It does not mutate GameState.",
            "",
            "## Next",
            "",
            "`M42-02`: Second fixture deck text exporter.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate M42-01 second runtime fixture schema.")
    parser.add_argument("--fixture", type=Path, default=DEFAULT_FIXTURE)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    fixture_path = args.fixture
    fixture = load_json(fixture_path)
    report = build_second_runtime_fixture_schema_validation_report(fixture, fixture_path)
    json_path = args.output_dir / "m42_01_second_fixture_schema_validation.json"
    md_path = args.output_dir / "m42_01_second_fixture_schema_validation.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M42-01 second fixture schema validation wrote {json_path}")
    print(f"M42-01 second fixture schema validation summary wrote {md_path}")
    print(
        "schema_valid={valid} blockers={blockers} next={next_ready}".format(
            valid=report["summary"]["schema_valid"],
            blockers=report["summary"]["blocking_issue_count"],
            next_ready=report["summary"]["ready_for_m42_02"],
        )
    )
    return 0 if report["summary"]["schema_valid"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
