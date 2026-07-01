"""Validate M39-01 offline runtime fixture schema and card counts."""

from __future__ import annotations

import argparse
import json
import sqlite3
import sys
from collections import Counter
from contextlib import closing
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
DEFAULT_FIXTURE = OUTPUT_DIR / "runtime_fixtures" / "recipe_003_classic_core_nova_grappler_m38_04.json"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"

EXPECTED_SCHEMA_VERSION = "deck_recipe_runtime_fixture_v1"
EXPECTED_FIXTURE_SCOPE = "offline_runtime_test_fixture"
EXPECTED_GRADES = {"0": 17, "1": 14, "2": 11, "3": 8}
EXPECTED_TRIGGERS = {"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _issue(code: str, severity: str, message: str, details: dict[str, Any] | None = None) -> dict[str, Any]:
    return {
        "code": code,
        "severity": severity,
        "message": message,
        "details": details or {},
    }


def _load_cards(card_ids: Sequence[str]) -> dict[str, dict[str, Any]]:
    if not card_ids:
        return {}
    placeholders = ",".join("?" for _ in card_ids)
    query = (
        "select card_id, name_th, series_code, clan, grade, trigger, deck_limit "
        "from cards where card_id in "
        f"({placeholders})"
    )
    with closing(sqlite3.connect(CARDS_SQLITE)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, list(card_ids)).fetchall()
    return {row["card_id"]: dict(row) for row in rows}


def _validate_required_fields(fixture: dict[str, Any]) -> list[dict[str, Any]]:
    required = [
        "schema_version",
        "fixture_id",
        "fixture_scope",
        "recipe_id",
        "source_artifact",
        "selected_target",
        "format_policy",
        "main_deck",
        "count_summary",
        "source_packages",
        "runtime_boundaries",
    ]
    return [
        _issue("missing_required_field", "blocker", "Fixture is missing a required top-level field.", {"field": field})
        for field in required
        if field not in fixture
    ]


def _validate_main_deck_rows(fixture: dict[str, Any]) -> tuple[dict[str, int], list[dict[str, Any]]]:
    quantities: dict[str, int] = {}
    issues: list[dict[str, Any]] = []
    rows = fixture.get("main_deck")
    if not isinstance(rows, list):
        return quantities, [_issue("main_deck_not_list", "blocker", "main_deck must be a list.")]
    for index, row in enumerate(rows):
        if not isinstance(row, dict):
            issues.append(_issue("main_deck_row_not_object", "blocker", "main_deck row must be an object.", {"index": index}))
            continue
        card_id = row.get("card_id")
        quantity = row.get("quantity")
        if not isinstance(card_id, str) or not card_id:
            issues.append(_issue("invalid_card_id", "blocker", "main_deck row has invalid card_id.", {"index": index}))
            continue
        if not isinstance(quantity, int) or quantity <= 0:
            issues.append(
                _issue(
                    "invalid_quantity",
                    "blocker",
                    "main_deck row has invalid quantity.",
                    {"index": index, "card_id": card_id, "quantity": quantity},
                )
            )
            continue
        quantities[card_id] = quantities.get(card_id, 0) + quantity
    return quantities, issues


def _validate_runtime_boundaries(fixture: dict[str, Any]) -> list[dict[str, Any]]:
    boundaries = fixture.get("runtime_boundaries", {})
    expected = {
        "test_fixture_only": True,
        "auto_injected_into_player_decks": False,
        "bot_playbook_enabled": False,
        "ui_deck_library_mutated": False,
        "game_state_mutated": False,
    }
    issues: list[dict[str, Any]] = []
    for key, expected_value in expected.items():
        if boundaries.get(key) is not expected_value:
            issues.append(
                _issue(
                    "runtime_boundary_violation",
                    "blocker",
                    "Runtime boundary flag has an unsafe value.",
                    {"field": key, "expected": expected_value, "actual": boundaries.get(key)},
                )
            )
    return issues


def validate_fixture_schema(fixture: dict[str, Any] | None = None) -> dict[str, Any]:
    fixture = fixture or load_json(DEFAULT_FIXTURE)
    issues: list[dict[str, Any]] = []
    issues.extend(_validate_required_fields(fixture))

    if fixture.get("schema_version") != EXPECTED_SCHEMA_VERSION:
        issues.append(
            _issue(
                "schema_version_mismatch",
                "blocker",
                "Fixture schema_version is not supported.",
                {"expected": EXPECTED_SCHEMA_VERSION, "actual": fixture.get("schema_version")},
            )
        )
    if fixture.get("fixture_scope") != EXPECTED_FIXTURE_SCOPE:
        issues.append(
            _issue(
                "fixture_scope_mismatch",
                "blocker",
                "Fixture scope is not the offline runtime/test scope.",
                {"expected": EXPECTED_FIXTURE_SCOPE, "actual": fixture.get("fixture_scope")},
            )
        )

    quantities, row_issues = _validate_main_deck_rows(fixture)
    issues.extend(row_issues)
    cards = _load_cards(sorted(quantities))
    missing = sorted(card_id for card_id in quantities if card_id not in cards)
    if missing:
        issues.append(_issue("missing_cards", "blocker", "Fixture card ids are missing from SQLite.", {"card_ids": missing}))

    trigger_counts: Counter[str] = Counter()
    grade_counts: Counter[str] = Counter()
    clan_counts: Counter[str] = Counter()
    for card_id, quantity in quantities.items():
        card = cards.get(card_id)
        if not card:
            continue
        deck_limit = int(card.get("deck_limit") or 4)
        if quantity > deck_limit:
            issues.append(
                _issue(
                    "copy_limit_exceeded",
                    "blocker",
                    "Fixture card quantity exceeds deck_limit.",
                    {"card_id": card_id, "quantity": quantity, "deck_limit": deck_limit},
                )
            )
        trigger = card.get("trigger") or ""
        if trigger:
            trigger_counts[trigger] += quantity
        grade_counts[str(card.get("grade"))] += quantity
        clan_counts[card.get("clan") or ""] += quantity

    main_deck_count = sum(quantities.values())
    if main_deck_count != 50:
        issues.append(_issue("main_deck_count_mismatch", "blocker", "Fixture main deck count must be 50.", {"actual": main_deck_count}))
    if dict(sorted(trigger_counts.items())) != EXPECTED_TRIGGERS:
        issues.append(
            _issue(
                "trigger_profile_mismatch",
                "blocker",
                "Fixture trigger profile must match the accepted classic package.",
                {"expected": EXPECTED_TRIGGERS, "actual": dict(sorted(trigger_counts.items()))},
            )
        )
    actual_grades = {grade: grade_counts.get(grade, 0) for grade in sorted(EXPECTED_GRADES)}
    if actual_grades != EXPECTED_GRADES:
        issues.append(
            _issue(
                "grade_profile_mismatch",
                "blocker",
                "Fixture grade profile must match the accepted classic target.",
                {"expected": EXPECTED_GRADES, "actual": actual_grades},
            )
        )

    expected_group = fixture.get("selected_target", {}).get("group", "")
    off_group = {clan: count for clan, count in clan_counts.items() if clan and clan != expected_group}
    if off_group:
        issues.append(
            _issue(
                "group_mismatch",
                "blocker",
                "Fixture contains cards outside the selected group.",
                {"expected_group": expected_group, "off_group_counts": off_group},
            )
        )

    issues.extend(_validate_runtime_boundaries(fixture))
    blocker_count = sum(1 for issue in issues if issue["severity"] == "blocker")
    schema_valid = blocker_count == 0
    return {
        "version": "M39-01",
        "description": "Offline runtime fixture schema validation",
        "source_inputs": {
            "runtime_fixture": str(DEFAULT_FIXTURE.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "scope": {
            "offline_fixture_schema_validator": True,
            "mutates_fixture_artifact": False,
            "mutates_runtime_deck_library": False,
            "bot_integration": False,
            "GameState_mutation": False,
        },
        "validation_policy": {
            "schema_version": EXPECTED_SCHEMA_VERSION,
            "fixture_scope": EXPECTED_FIXTURE_SCOPE,
            "main_deck_count": 50,
            "trigger_profile": EXPECTED_TRIGGERS,
            "grade_profile": EXPECTED_GRADES,
            "copy_limit_from_sqlite": True,
            "selected_group_only": True,
            "runtime_boundaries_enforced": True,
        },
        "fixture_summary": {
            "fixture_id": fixture.get("fixture_id", ""),
            "recipe_id": fixture.get("recipe_id", ""),
            "main_deck_count": main_deck_count,
            "trigger_counts": dict(sorted(trigger_counts.items())),
            "grade_counts": actual_grades,
            "clan_counts": dict(sorted(clan_counts.items())),
            "unique_card_count": len(quantities),
        },
        "issues": issues,
        "summary": {
            "schema_valid": schema_valid,
            "blocking_issue_count": blocker_count,
            "issue_count": len(issues),
            "ready_for_m39_02": schema_valid,
        },
        "next_target": {
            "milestone": "M39-02",
            "task": "Fixture-to-deck text exporter",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    fixture = report["fixture_summary"]
    lines = [
        "# M39-01 Offline Fixture Schema Validation",
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
        f"- Ready for M39-02: `{summary['ready_for_m39_02']}`",
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
            "- It does not enable bot playbooks.",
            "- It does not mutate GameState.",
            "",
            "## Next",
            "",
            "`M39-02`: Fixture-to-deck text exporter.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate M39-01 offline runtime fixture schema.")
    parser.add_argument("--fixture", type=Path, default=DEFAULT_FIXTURE)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    fixture = load_json(args.fixture)
    report = validate_fixture_schema(fixture)
    report["source_inputs"]["runtime_fixture"] = str(args.fixture)
    json_path = args.output_dir / "m39_01_offline_fixture_schema_validation.json"
    md_path = args.output_dir / "m39_01_offline_fixture_schema_validation.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M39-01 offline fixture schema validation wrote {json_path}")
    print(f"M39-01 offline fixture schema validation summary wrote {md_path}")
    print(
        "schema_valid={valid} blockers={blockers} next={next_ready}".format(
            valid=report["summary"]["schema_valid"],
            blockers=report["summary"]["blocking_issue_count"],
            next_ready=report["summary"]["ready_for_m39_02"],
        )
    )
    return 0 if report["summary"]["schema_valid"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
