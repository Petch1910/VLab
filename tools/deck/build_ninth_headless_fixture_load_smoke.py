"""Build and verify the M70-03 ninth fixture headless load smoke artifacts."""

from __future__ import annotations

import argparse
import hashlib
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_fourth_headless_fixture_load_smoke import (  # noqa: E402
    _deck_code_from_deck,
    _deck_from_deck_code,
    _issue,
    _load_known_card_ids,
    _load_optional_unity_result,
    _zone_quantity_map,
    load_json,
    parse_count_line_deck_text,
    write_deck_code,
    write_json,
)


OUTPUT_DIR = ROOT / "outputs" / "target_slice"
DEFAULT_FIXTURE = OUTPUT_DIR / "runtime_fixtures" / "m68_recipe_001_aqua_force_m69_06.json"
DEFAULT_M70_02_REPORT = OUTPUT_DIR / "m70_02_ninth_fixture_deck_text_export.json"
DEFAULT_DECK_TEXT = OUTPUT_DIR / "m70_02_ninth_fixture_deck_text_export.txt"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"

DECK_CODE_OUTPUT = OUTPUT_DIR / "m70_03_ninth_fixture_deck_code.txt"
JSON_OUTPUT = OUTPUT_DIR / "m70_03_ninth_fixture_load_smoke.json"
MD_OUTPUT = OUTPUT_DIR / "m70_03_ninth_fixture_load_smoke.md"
DEFAULT_UNITY_RESULT = OUTPUT_DIR / "m70_03_ninth_fixture_unity_result.json"
DEFAULT_UNITY_REPLAY = OUTPUT_DIR / "m70_03_ninth_fixture_unity_replay.json"

HEADLESS_SEED = 7003
HEADLESS_RULESET = "Premium"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def _build_deck_object(parsed: dict[str, Any]) -> dict[str, Any]:
    metadata = parsed["metadata"]
    zones = parsed["zones"]
    digest_source = json.dumps(zones, ensure_ascii=False, sort_keys=True)
    deck_id = "m7003" + hashlib.sha256(digest_source.encode("utf-8")).hexdigest()[:26]
    return {
        "deck_id": deck_id,
        "name": metadata["Name"],
        "format": metadata["Format"],
        "card_pack_id": metadata["PackId"],
        "card_pack_version": metadata["PackVersion"],
        "main": zones["Main"],
        "ride": zones["Ride"],
        "g": zones["G"],
    }


def build_ninth_headless_fixture_load_smoke(
    fixture: dict[str, Any] | None = None,
    m70_02_report: dict[str, Any] | None = None,
    deck_text: str | None = None,
    unity_result_path: Path | None = None,
    unity_replay_path: Path | None = None,
) -> dict[str, Any]:
    fixture = fixture or load_json(DEFAULT_FIXTURE)
    m70_02_report = m70_02_report or load_json(DEFAULT_M70_02_REPORT)
    deck_text = deck_text if deck_text is not None else DEFAULT_DECK_TEXT.read_text(encoding="utf-8")
    issues: list[dict[str, Any]] = []

    if not m70_02_report.get("summary", {}).get("export_ready"):
        issues.append(
            _issue(
                "m70_02_export_not_ready",
                "blocker",
                "M70-02 deck text export must be ready before headless load smoke.",
                {"summary": m70_02_report.get("summary", {})},
            )
        )

    parsed = parse_count_line_deck_text(deck_text)
    issues.extend(parsed["issues"])

    main_quantities = _zone_quantity_map(parsed["zones"]["Main"])
    fixture_quantities = _zone_quantity_map(fixture.get("main_deck", []))
    if main_quantities != fixture_quantities:
        issues.append(
            _issue(
                "fixture_deck_text_mismatch",
                "blocker",
                "Deck text main entries do not match the runtime fixture.",
                {
                    "fixture_count": sum(fixture_quantities.values()),
                    "deck_text_count": sum(main_quantities.values()),
                },
            )
        )

    known = _load_known_card_ids(sorted(main_quantities))
    missing = sorted(card_id for card_id in main_quantities if card_id not in known)
    if missing:
        issues.append(_issue("missing_cards", "blocker", "Deck text card ids are missing from SQLite.", {"card_ids": missing}))

    if parsed["zones"]["G"]:
        issues.append(_issue("g_zone_not_empty", "blocker", "Ninth fixture G section must stay empty.", {}))

    deck = _build_deck_object(parsed)
    deck_code = "" if issues else _deck_code_from_deck(deck)
    deck_round_trip = _deck_from_deck_code(deck_code) if deck_code else {}
    if deck_code and deck_round_trip.get("main") != deck["main"]:
        issues.append(_issue("deck_code_round_trip_mismatch", "blocker", "Generated deck code does not round-trip.", {}))
    if deck_code and deck_round_trip.get("g") != []:
        issues.append(_issue("deck_code_g_zone_not_empty", "blocker", "Generated deck code must keep G Zone empty.", {}))

    unity_result = _load_optional_unity_result(unity_result_path, unity_replay_path)
    if unity_result["provided"]:
        if not unity_result["accepted"]:
            issues.append(
                _issue(
                    "unity_headless_rejected",
                    "blocker",
                    "Unity headless runner rejected the generated fixture deck code.",
                    unity_result,
                )
            )
        if unity_result.get("deck_source") != "deck_code":
            issues.append(
                _issue(
                    "unity_headless_not_deck_code_source",
                    "blocker",
                    "Unity headless runner did not load from deck_code source.",
                    unity_result,
                )
            )

    blocker_count = sum(1 for issue in issues if issue["severity"] == "blocker")
    offline_load_ready = blocker_count == 0
    unity_smoke_passed = bool(unity_result["provided"] and unity_result["accepted"] and unity_result.get("deck_source") == "deck_code")
    ready_for_m70_04 = offline_load_ready and unity_smoke_passed

    return {
        "version": "M70-03",
        "description": "Ninth fixture headless load smoke",
        "source_inputs": {
            "runtime_fixture": str(DEFAULT_FIXTURE.relative_to(ROOT)),
            "m70_02_report": str(DEFAULT_M70_02_REPORT.relative_to(ROOT)),
            "deck_text": str(DEFAULT_DECK_TEXT.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "scope": {
            "offline_headless_fixture_load_smoke": True,
            "ninth_fixture_headless_load_smoke": True,
            "creates_deck_code_artifact": True,
            "mutates_saved_decks": False,
            "mutates_ui_deck_library": False,
            "bot_integration": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "aqua_force_battle_order_runtime_enabled": False,
            "battle_count_tracker_enabled": False,
            "attack_order_predicate_runtime_enabled": False,
            "multi_attack_label_runtime_enabled": False,
            "live_card_text_parsing": False,
            "GameState_mutation_from_python": False,
        },
        "fixture_summary": {
            "fixture_id": fixture.get("fixture_id", ""),
            "recipe_id": fixture.get("recipe_id", ""),
            "main_deck_count": sum(main_quantities.values()),
            "unique_card_count": len(main_quantities),
            "g_zone_count": sum(entry.get("quantity", 0) for entry in parsed["zones"]["G"]),
        },
        "headless_request": {
            "seed": HEADLESS_SEED,
            "ruleset": HEADLESS_RULESET,
            "deck_code_path": str(DECK_CODE_OUTPUT.relative_to(ROOT)),
            "deck_code_sha256": hashlib.sha256(deck_code.encode("utf-8")).hexdigest() if deck_code else "",
            "deck_code_prefix": deck_code[:16] if deck_code else "",
            "deck_code_length": len(deck_code),
            "unity_result_path": str(DEFAULT_UNITY_RESULT.relative_to(ROOT)),
            "unity_replay_path": str(DEFAULT_UNITY_REPLAY.relative_to(ROOT)),
        },
        "unity_headless_result": unity_result,
        "issues": issues,
        "summary": {
            "offline_load_ready": offline_load_ready,
            "deck_code_created": bool(deck_code),
            "unity_headless_result_provided": bool(unity_result["provided"]),
            "unity_headless_smoke_passed": unity_smoke_passed,
            "blocking_issue_count": blocker_count,
            "issue_count": len(issues),
            "ready_for_m70_04": ready_for_m70_04,
        },
        "next_target": {
            "milestone": "M70-04",
            "task": "Nine-fixture scale decision",
        },
        "_deck_code": deck_code,
    }


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    fixture = report["fixture_summary"]
    request = report["headless_request"]
    unity = report["unity_headless_result"]
    lines = [
        "# M70-03 Ninth Fixture Headless Load Smoke",
        "",
        "## Summary",
        "",
        f"- Fixture: `{fixture['fixture_id']}`",
        f"- Recipe: `{fixture['recipe_id']}`",
        f"- Offline load ready: `{summary['offline_load_ready']}`",
        f"- Deck code created: `{summary['deck_code_created']}`",
        f"- Unity headless result provided: `{summary['unity_headless_result_provided']}`",
        f"- Unity headless smoke passed: `{summary['unity_headless_smoke_passed']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        f"- Main deck count: `{fixture['main_deck_count']}`",
        f"- Unique cards: `{fixture['unique_card_count']}`",
        f"- G Zone count: `{fixture['g_zone_count']}`",
        f"- Deck code path: `{request['deck_code_path']}`",
        f"- Deck code SHA-256: `{request['deck_code_sha256']}`",
        f"- Ready for M70-04: `{summary['ready_for_m70_04']}`",
        "",
        "## Unity Headless",
        "",
        f"- Result path: `{unity.get('result_path', '')}`",
        f"- Replay path: `{unity.get('replay_path', '')}`",
        f"- Accepted: `{unity.get('accepted')}`",
        f"- Deck source: `{unity.get('deck_source', '')}`",
        f"- Actions: `{unity.get('actions_executed', '')}`",
        f"- Events: `{unity.get('event_count', '')}`",
        "",
        "## Runtime Boundary",
        "",
        "- No saved deck mutation.",
        "- No UI deck library mutation.",
        "- No bot playbook enablement.",
        "- No G Zone runtime enablement.",
        "- No Stride runtime enablement.",
        "- No Aqua Force battle-order runtime enablement.",
        "- No live card text parsing.",
        "- Python smoke does not mutate GameState.",
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
            "`M70-04`: Nine-fixture scale decision.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M70-03 ninth fixture headless load smoke artifacts.")
    parser.add_argument("--fixture", type=Path, default=DEFAULT_FIXTURE)
    parser.add_argument("--m70-02-report", type=Path, default=DEFAULT_M70_02_REPORT)
    parser.add_argument("--deck-text", type=Path, default=DEFAULT_DECK_TEXT)
    parser.add_argument("--unity-result", type=Path, default=None)
    parser.add_argument("--unity-replay", type=Path, default=None)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    fixture = load_json(args.fixture)
    m70_02_report = load_json(args.m70_02_report)
    deck_text = args.deck_text.read_text(encoding="utf-8")
    report = build_ninth_headless_fixture_load_smoke(
        fixture,
        m70_02_report,
        deck_text,
        args.unity_result,
        args.unity_replay,
    )
    report["source_inputs"]["runtime_fixture"] = str(args.fixture)
    report["source_inputs"]["m70_02_report"] = str(args.m70_02_report)
    report["source_inputs"]["deck_text"] = str(args.deck_text)

    deck_code_path = args.output_dir / DECK_CODE_OUTPUT.name
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    report["headless_request"]["deck_code_path"] = str(deck_code_path)
    report["headless_request"]["unity_result_path"] = str(args.output_dir / DEFAULT_UNITY_RESULT.name)
    report["headless_request"]["unity_replay_path"] = str(args.output_dir / DEFAULT_UNITY_REPLAY.name)

    if report["summary"]["deck_code_created"]:
        write_deck_code(report, deck_code_path)
    write_json(report, json_path)
    write_markdown(report, md_path)

    print(f"M70-03 ninth fixture headless load smoke wrote {json_path}")
    print(f"M70-03 ninth fixture headless load smoke summary wrote {md_path}")
    if report["summary"]["deck_code_created"]:
        print(f"M70-03 ninth fixture deck code wrote {deck_code_path}")
    print(
        "offline_ready={offline} unity_provided={provided} unity_passed={passed} blockers={blockers} next={next_ready}".format(
            offline=report["summary"]["offline_load_ready"],
            provided=report["summary"]["unity_headless_result_provided"],
            passed=report["summary"]["unity_headless_smoke_passed"],
            blockers=report["summary"]["blocking_issue_count"],
            next_ready=report["summary"]["ready_for_m70_04"],
        )
    )
    return 0 if report["summary"]["offline_load_ready"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
