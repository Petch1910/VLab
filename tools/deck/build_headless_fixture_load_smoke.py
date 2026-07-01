"""Build and verify the M39-03 headless fixture load smoke artifacts."""

from __future__ import annotations

import argparse
import base64
import gzip
import hashlib
import json
import sqlite3
import sys
from contextlib import closing
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
DEFAULT_FIXTURE = OUTPUT_DIR / "runtime_fixtures" / "recipe_003_classic_core_nova_grappler_m38_04.json"
DEFAULT_M39_02_REPORT = OUTPUT_DIR / "m39_02_fixture_deck_text_export.json"
DEFAULT_DECK_TEXT = OUTPUT_DIR / "m39_02_fixture_deck_text_export.txt"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"

DECK_CODE_OUTPUT = OUTPUT_DIR / "m39_03_headless_fixture_deck_code.txt"
JSON_OUTPUT = OUTPUT_DIR / "m39_03_headless_fixture_load_smoke.json"
MD_OUTPUT = OUTPUT_DIR / "m39_03_headless_fixture_load_smoke.md"
DEFAULT_UNITY_RESULT = OUTPUT_DIR / "m39_03_headless_fixture_unity_result.json"
DEFAULT_UNITY_REPLAY = OUTPUT_DIR / "m39_03_headless_fixture_unity_replay.json"

HEADLESS_SEED = 3903
HEADLESS_RULESET = "Premium"


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


def parse_count_line_deck_text(text: str) -> dict[str, Any]:
    metadata = {
        "Name": "Imported Deck",
        "Format": "D",
        "PackId": "local",
        "PackVersion": "unknown",
        "PackDefinitionHash": "",
    }
    zones: dict[str, list[dict[str, Any]]] = {"Main": [], "Ride": [], "G": []}
    current_zone: str | None = None
    issues: list[dict[str, Any]] = []

    for line_number, raw in enumerate(text.replace("\r\n", "\n").replace("\r", "\n").split("\n"), start=1):
        line = raw.strip()
        if not line or line.startswith("#"):
            continue
        for key in list(metadata):
            prefix = key + ":"
            if line.lower().startswith(prefix.lower()):
                value = line[len(prefix) :].strip()
                if not value:
                    issues.append(_issue("empty_metadata", "blocker", "Deck text metadata is empty.", {"line": line_number, "field": key}))
                metadata[key] = value
                break
        else:
            if line.startswith("[") and line.endswith("]"):
                zone = line[1:-1].strip()
                if zone not in zones:
                    issues.append(_issue("unknown_zone", "blocker", "Deck text has an unknown zone.", {"line": line_number, "zone": zone}))
                    current_zone = None
                else:
                    current_zone = zone
                continue

            if current_zone is None:
                issues.append(_issue("card_before_zone", "blocker", "Deck text has a card line before a zone header.", {"line": line_number}))
                continue

            parts = line.split(None, 1)
            if len(parts) != 2:
                issues.append(_issue("invalid_card_line", "blocker", "Card line must be '<quantity> <card_id>'.", {"line": line_number}))
                continue
            try:
                quantity = int(parts[0])
            except ValueError:
                issues.append(_issue("invalid_quantity", "blocker", "Card quantity is not an integer.", {"line": line_number}))
                continue
            card_id = parts[1].strip()
            if quantity <= 0 or not card_id:
                issues.append(_issue("invalid_card_line", "blocker", "Card line has invalid quantity or card id.", {"line": line_number}))
                continue
            zones[current_zone].append({"card_id": card_id, "quantity": quantity})

    return {
        "metadata": metadata,
        "zones": zones,
        "issues": issues,
    }


def _load_known_card_ids(card_ids: Sequence[str]) -> set[str]:
    if not card_ids:
        return set()
    placeholders = ",".join("?" for _ in card_ids)
    query = f"select card_id from cards where card_id in ({placeholders})"
    with closing(sqlite3.connect(CARDS_SQLITE)) as connection:
        rows = connection.execute(query, list(card_ids)).fetchall()
    return {row[0] for row in rows}


def _zone_quantity_map(entries: Sequence[dict[str, Any]]) -> dict[str, int]:
    quantities: dict[str, int] = {}
    for entry in entries:
        card_id = str(entry.get("card_id") or "").strip()
        quantity = int(entry.get("quantity") or 0)
        if card_id and quantity > 0:
            quantities[card_id] = quantities.get(card_id, 0) + quantity
    return quantities


def _deck_code_from_deck(deck: dict[str, Any]) -> str:
    payload = json.dumps(deck, ensure_ascii=False, separators=(",", ":")).encode("utf-8")
    compressed = gzip.compress(payload, mtime=0)
    encoded = base64.urlsafe_b64encode(compressed).decode("ascii").rstrip("=")
    return "VGTH1." + encoded


def _deck_from_deck_code(deck_code: str) -> dict[str, Any]:
    if not deck_code.startswith("VGTH1."):
        raise ValueError("Deck code must start with VGTH1.")
    payload = deck_code[len("VGTH1.") :]
    padding = "=" * ((4 - len(payload) % 4) % 4)
    compressed = base64.urlsafe_b64decode(payload + padding)
    raw = gzip.decompress(compressed)
    return json.loads(raw.decode("utf-8"))


def _build_deck_object(parsed: dict[str, Any]) -> dict[str, Any]:
    metadata = parsed["metadata"]
    zones = parsed["zones"]
    digest_source = json.dumps(zones, ensure_ascii=False, sort_keys=True)
    deck_id = "m3903" + hashlib.sha256(digest_source.encode("utf-8")).hexdigest()[:26]
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


def _load_optional_unity_result(result_path: Path | None, replay_path: Path | None) -> dict[str, Any]:
    if result_path is None or not result_path.exists():
        return {
            "provided": False,
            "accepted": None,
            "result_path": "" if result_path is None else str(result_path),
            "replay_path": "" if replay_path is None else str(replay_path),
        }
    result = load_json(result_path)
    replay_exists = replay_path.exists() if replay_path is not None else False
    return {
        "provided": True,
        "accepted": bool(result.get("accepted")),
        "result_path": str(result_path),
        "replay_path": "" if replay_path is None else str(replay_path),
        "replay_exists": replay_exists,
        "seed": result.get("seed"),
        "ruleset": result.get("ruleset"),
        "deck_source": result.get("deck_source"),
        "actions_executed": result.get("actions_executed"),
        "event_count": result.get("event_count"),
        "failure_reason": result.get("failure_reason", ""),
    }


def build_headless_fixture_load_smoke(
    fixture: dict[str, Any] | None = None,
    m39_02_report: dict[str, Any] | None = None,
    deck_text: str | None = None,
    unity_result_path: Path | None = None,
    unity_replay_path: Path | None = None,
) -> dict[str, Any]:
    fixture = fixture or load_json(DEFAULT_FIXTURE)
    m39_02_report = m39_02_report or load_json(DEFAULT_M39_02_REPORT)
    deck_text = deck_text if deck_text is not None else DEFAULT_DECK_TEXT.read_text(encoding="utf-8")
    issues: list[dict[str, Any]] = []

    if not m39_02_report.get("summary", {}).get("export_ready"):
        issues.append(
            _issue(
                "m39_02_export_not_ready",
                "blocker",
                "M39-02 deck text export must be ready before headless load smoke.",
                {"summary": m39_02_report.get("summary", {})},
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

    deck = _build_deck_object(parsed)
    deck_code = "" if issues else _deck_code_from_deck(deck)
    deck_round_trip = _deck_from_deck_code(deck_code) if deck_code else {}
    if deck_code and deck_round_trip.get("main") != deck["main"]:
        issues.append(_issue("deck_code_round_trip_mismatch", "blocker", "Generated deck code does not round-trip.", {}))

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
    ready_for_m39_04 = offline_load_ready and unity_smoke_passed

    return {
        "version": "M39-03",
        "description": "Headless fixture load smoke",
        "source_inputs": {
            "runtime_fixture": str(DEFAULT_FIXTURE.relative_to(ROOT)),
            "m39_02_report": str(DEFAULT_M39_02_REPORT.relative_to(ROOT)),
            "deck_text": str(DEFAULT_DECK_TEXT.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "scope": {
            "offline_headless_fixture_load_smoke": True,
            "creates_deck_code_artifact": True,
            "mutates_saved_decks": False,
            "mutates_ui_deck_library": False,
            "bot_integration": False,
            "GameState_mutation_from_python": False,
        },
        "fixture_summary": {
            "fixture_id": fixture.get("fixture_id", ""),
            "recipe_id": fixture.get("recipe_id", ""),
            "main_deck_count": sum(main_quantities.values()),
            "unique_card_count": len(main_quantities),
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
            "ready_for_m39_04": ready_for_m39_04,
        },
        "next_target": {
            "milestone": "M39-04",
            "task": "Second-slice recipe scale decision",
        },
        "_deck_code": deck_code,
    }


def write_deck_code(report: dict[str, Any], path: Path) -> None:
    deck_code = report.get("_deck_code", "")
    if not deck_code:
        raise ValueError("Deck code artifact is not ready.")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(deck_code + "\n", encoding="utf-8")


def write_json(report: dict[str, Any], path: Path) -> None:
    serializable = {key: value for key, value in report.items() if key != "_deck_code"}
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(serializable, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    fixture = report["fixture_summary"]
    request = report["headless_request"]
    unity = report["unity_headless_result"]
    lines = [
        "# M39-03 Headless Fixture Load Smoke",
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
        f"- Deck code path: `{request['deck_code_path']}`",
        f"- Deck code SHA-256: `{request['deck_code_sha256']}`",
        f"- Ready for M39-04: `{summary['ready_for_m39_04']}`",
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
            "`M39-04`: Second-slice recipe scale decision.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M39-03 headless fixture load smoke artifacts.")
    parser.add_argument("--fixture", type=Path, default=DEFAULT_FIXTURE)
    parser.add_argument("--m39-02-report", type=Path, default=DEFAULT_M39_02_REPORT)
    parser.add_argument("--deck-text", type=Path, default=DEFAULT_DECK_TEXT)
    parser.add_argument("--unity-result", type=Path, default=None)
    parser.add_argument("--unity-replay", type=Path, default=None)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    fixture = load_json(args.fixture)
    m39_02_report = load_json(args.m39_02_report)
    deck_text = args.deck_text.read_text(encoding="utf-8")
    report = build_headless_fixture_load_smoke(
        fixture,
        m39_02_report,
        deck_text,
        args.unity_result,
        args.unity_replay,
    )
    report["source_inputs"]["runtime_fixture"] = str(args.fixture)
    report["source_inputs"]["m39_02_report"] = str(args.m39_02_report)
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

    print(f"M39-03 headless fixture load smoke wrote {json_path}")
    print(f"M39-03 headless fixture load smoke summary wrote {md_path}")
    if report["summary"]["deck_code_created"]:
        print(f"M39-03 headless fixture deck code wrote {deck_code_path}")
    print(
        "offline_ready={offline} unity_provided={provided} unity_passed={passed} blockers={blockers} next={next_ready}".format(
            offline=report["summary"]["offline_load_ready"],
            provided=report["summary"]["unity_headless_result_provided"],
            passed=report["summary"]["unity_headless_smoke_passed"],
            blockers=report["summary"]["blocking_issue_count"],
            next_ready=report["summary"]["ready_for_m39_04"],
        )
    )
    return 0 if report["summary"]["offline_load_ready"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
