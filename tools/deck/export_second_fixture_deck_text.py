"""Export the second offline runtime fixture as count-line deck text (M42-02)."""

from __future__ import annotations

import argparse
import hashlib
import json
import sqlite3
import sys
from contextlib import closing
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
DEFAULT_FIXTURE = OUTPUT_DIR / "runtime_fixtures" / "m40_recipe_001_classic_core_oracle_think_tank_m41_04.json"
DEFAULT_VALIDATION_REPORT = OUTPUT_DIR / "m42_01_second_fixture_schema_validation.json"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"
PACK_MANIFEST = ROOT / "data" / "packs" / "vanguard_th" / "manifest.json"

TEXT_OUTPUT = OUTPUT_DIR / "m42_02_second_fixture_deck_text_export.txt"
JSON_OUTPUT = OUTPUT_DIR / "m42_02_second_fixture_deck_text_export.json"
MD_OUTPUT = OUTPUT_DIR / "m42_02_second_fixture_deck_text_export.md"


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


def _load_card_rows(card_ids: Sequence[str]) -> dict[str, dict[str, Any]]:
    if not card_ids:
        return {}
    placeholders = ",".join("?" for _ in card_ids)
    query = (
        "select card_id, name_th, series_code, clan, grade, trigger, type_1 "
        "from cards where card_id in "
        f"({placeholders})"
    )
    with closing(sqlite3.connect(CARDS_SQLITE)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, list(card_ids)).fetchall()
    return {row["card_id"]: dict(row) for row in rows}


def _load_pack_metadata() -> dict[str, str]:
    manifest = load_json(PACK_MANIFEST)
    return {
        "pack_id": str(manifest.get("pack_id") or "vanguard_th"),
        "pack_version": str(manifest.get("source_version") or "unknown"),
        "definition_hash": str(manifest.get("definition_hash") or ""),
    }


def _is_validation_ready(validation_report: dict[str, Any]) -> bool:
    summary = validation_report.get("summary", {})
    return bool(summary.get("schema_valid")) and bool(summary.get("ready_for_m42_02"))


def _main_deck_quantities(fixture: dict[str, Any]) -> list[dict[str, Any]]:
    rows = fixture.get("main_deck")
    if not isinstance(rows, list):
        return []
    return [row for row in rows if isinstance(row, dict)]


def _build_deck_text(
    fixture: dict[str, Any],
    card_rows: dict[str, dict[str, Any]],
    pack_metadata: dict[str, str],
) -> str:
    selected_target = fixture.get("selected_target", {})
    format_policy = fixture.get("format_policy", {})
    recipe_id = str(fixture.get("recipe_id") or "unknown_recipe")
    fixture_id = str(fixture.get("fixture_id") or "unknown_fixture")
    era_preset = str(format_policy.get("era_preset") or "classic_part1")
    slice_name = str(format_policy.get("slice") or selected_target.get("slice") or "Classic Core")

    lines = [
        "# Vanguard Thai Sim Deck List",
        "# M42-02 review-only export from an offline runtime fixture.",
        f"# FixtureId: {fixture_id}",
        f"# RecipeId: {recipe_id}",
        f"# Slice: {slice_name}",
        "# Scope: review_only_offline_fixture",
        "Name: Classic Core Oracle Think Tank Fixture (" + recipe_id + ")",
        "Format: " + era_preset,
        "PackId: " + pack_metadata["pack_id"],
        "PackVersion: " + pack_metadata["pack_version"],
    ]
    if pack_metadata["definition_hash"]:
        lines.append("PackDefinitionHash: " + pack_metadata["definition_hash"])

    lines.extend(["", "[Main]"])
    for row in _main_deck_quantities(fixture):
        card_id = str(row.get("card_id") or "").strip()
        quantity = int(row.get("quantity") or 0)
        card = card_rows.get(card_id, {})
        comment_parts = [
            str(card.get("name_th") or card_id),
            "G" + str(card.get("grade") if card.get("grade") is not None else "?"),
            str(card.get("type_1") or "Unknown"),
        ]
        if card.get("trigger"):
            comment_parts.append(str(card["trigger"]))
        if card.get("series_code"):
            comment_parts.append(str(card["series_code"]))
        lines.append("# Card: " + " | ".join(comment_parts))
        lines.append(f"{quantity} {card_id}")

    lines.extend(["", "[Ride]", "", "[G]", ""])
    return "\n".join(lines).rstrip() + "\n"


def build_second_fixture_deck_text_export(
    fixture: dict[str, Any] | None = None,
    validation_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    fixture = fixture or load_json(DEFAULT_FIXTURE)
    validation_report = validation_report or load_json(DEFAULT_VALIDATION_REPORT)
    issues: list[dict[str, Any]] = []

    if not _is_validation_ready(validation_report):
        issues.append(
            _issue(
                "m42_01_validation_not_ready",
                "blocker",
                "M42-01 validation report must be schema-valid and ready before deck text export.",
                {"summary": validation_report.get("summary", {})},
            )
        )

    rows = _main_deck_quantities(fixture)
    quantities: dict[str, int] = {}
    for index, row in enumerate(rows):
        card_id = str(row.get("card_id") or "").strip()
        quantity = row.get("quantity")
        if not card_id or not isinstance(quantity, int) or quantity <= 0:
            issues.append(
                _issue(
                    "invalid_fixture_main_deck_row",
                    "blocker",
                    "Fixture main deck row is not exportable.",
                    {"index": index, "row": row},
                )
            )
            continue
        quantities[card_id] = quantities.get(card_id, 0) + quantity

    card_rows = _load_card_rows(sorted(quantities))
    missing = sorted(card_id for card_id in quantities if card_id not in card_rows)
    if missing:
        issues.append(_issue("missing_cards", "blocker", "Fixture card ids are missing from SQLite.", {"card_ids": missing}))

    main_deck_count = sum(quantities.values())
    card_line_count = len(quantities)
    pack_metadata = _load_pack_metadata()
    deck_text = "" if issues else _build_deck_text(fixture, card_rows, pack_metadata)
    blocking_count = sum(1 for issue in issues if issue["severity"] == "blocker")
    export_ready = blocking_count == 0

    return {
        "version": "M42-02",
        "description": "Second fixture deck text exporter",
        "source_inputs": {
            "runtime_fixture": str(DEFAULT_FIXTURE.relative_to(ROOT)),
            "m42_01_validation_report": str(DEFAULT_VALIDATION_REPORT.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
            "pack_manifest": str(PACK_MANIFEST.relative_to(ROOT)),
        },
        "scope": {
            "offline_fixture_deck_text_export": True,
            "review_text_only": True,
            "mutates_fixture_artifact": False,
            "mutates_runtime_deck_library": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_integration": False,
            "GameState_mutation": False,
        },
        "deck_text_policy": {
            "format": "count_line_deck_text",
            "compatible_with_count_line_deck_codec": True,
            "comment_lines_are_review_only": True,
            "card_lines_shape": "<quantity> <card_id>",
            "empty_ride_and_g_sections_preserved": True,
        },
        "fixture_summary": {
            "fixture_id": fixture.get("fixture_id", ""),
            "recipe_id": fixture.get("recipe_id", ""),
            "main_deck_count": main_deck_count,
            "unique_card_count": card_line_count,
        },
        "pack_metadata": pack_metadata,
        "outputs": {
            "deck_text": str(TEXT_OUTPUT.relative_to(ROOT)),
            "json_report": str(JSON_OUTPUT.relative_to(ROOT)),
            "markdown_report": str(MD_OUTPUT.relative_to(ROOT)),
            "deck_text_sha256": hashlib.sha256(deck_text.encode("utf-8")).hexdigest() if deck_text else "",
        },
        "issues": issues,
        "summary": {
            "export_ready": export_ready,
            "blocking_issue_count": blocking_count,
            "issue_count": len(issues),
            "main_deck_count": main_deck_count,
            "exported_card_line_count": card_line_count if export_ready else 0,
            "ready_for_m42_03": export_ready,
        },
        "next_target": {
            "milestone": "M42-03",
            "task": "Second fixture headless load smoke",
        },
        "_deck_text": deck_text,
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    serializable = {key: value for key, value in report.items() if key != "_deck_text"}
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(serializable, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_deck_text(report: dict[str, Any], path: Path) -> None:
    deck_text = report.get("_deck_text", "")
    if not deck_text:
        raise ValueError("Deck text export is not ready; refusing to write an empty deck text artifact.")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(deck_text, encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    fixture = report["fixture_summary"]
    outputs = report["outputs"]
    lines = [
        "# M42-02 Second Fixture Deck Text Export",
        "",
        "## Summary",
        "",
        f"- Fixture: `{fixture['fixture_id']}`",
        f"- Recipe: `{fixture['recipe_id']}`",
        f"- Export ready: `{summary['export_ready']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        f"- Main deck count: `{summary['main_deck_count']}`",
        f"- Exported card lines: `{summary['exported_card_line_count']}`",
        f"- Deck text: `{outputs['deck_text']}`",
        f"- Deck text SHA-256: `{outputs['deck_text_sha256']}`",
        f"- Ready for M42-03: `{summary['ready_for_m42_03']}`",
        "",
        "## Runtime Boundary",
        "",
        "- Review text only.",
        "- Does not mutate the fixture artifact.",
        "- Does not add a saved deck.",
        "- Does not publish a UI deck list.",
        "- Does not enable bot playbooks.",
        "- Does not mutate GameState.",
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
            "## Next",
            "",
            "`M42-03`: Second fixture headless load smoke.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Export M42-02 second fixture count-line deck text.")
    parser.add_argument("--fixture", type=Path, default=DEFAULT_FIXTURE)
    parser.add_argument("--validation-report", type=Path, default=DEFAULT_VALIDATION_REPORT)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    fixture = load_json(args.fixture)
    validation_report = load_json(args.validation_report)
    report = build_second_fixture_deck_text_export(fixture, validation_report)
    report["source_inputs"]["runtime_fixture"] = str(args.fixture)
    report["source_inputs"]["m42_01_validation_report"] = str(args.validation_report)

    text_path = args.output_dir / TEXT_OUTPUT.name
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    report["outputs"]["deck_text"] = str(text_path)
    report["outputs"]["json_report"] = str(json_path)
    report["outputs"]["markdown_report"] = str(md_path)

    if report["summary"]["export_ready"]:
        write_deck_text(report, text_path)
    write_json(report, json_path)
    write_markdown(report, md_path)

    print(f"M42-02 second fixture deck text export wrote {json_path}")
    print(f"M42-02 second fixture deck text summary wrote {md_path}")
    if report["summary"]["export_ready"]:
        print(f"M42-02 second fixture deck text wrote {text_path}")
    print(
        "export_ready={ready} blockers={blockers} card_lines={lines} next={next_ready}".format(
            ready=report["summary"]["export_ready"],
            blockers=report["summary"]["blocking_issue_count"],
            lines=report["summary"]["exported_card_line_count"],
            next_ready=report["summary"]["ready_for_m42_03"],
        )
    )
    return 0 if report["summary"]["export_ready"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
