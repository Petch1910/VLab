"""Build the M38 closeout for the first runtime fixture gate."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M38_04_GATE = OUTPUT_DIR / "m38_04_runtime_fixture_promotion_gate.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def build_first_runtime_fixture_closeout(gate_report: dict[str, Any] | None = None) -> dict[str, Any]:
    gate_report = gate_report or load_json(M38_04_GATE)
    decision = gate_report["promotion_decision"]
    summary = gate_report["summary"]
    fixture = gate_report.get("runtime_fixture") or {}
    m38_complete = bool(summary.get("promotion_allowed")) and bool(decision.get("fixture_created"))
    next_queue_id = "M39" if m38_complete else "M38-repair"
    return {
        "version": "M38-closeout",
        "description": "First runtime fixture closeout and next queue selection",
        "source_inputs": {
            "runtime_fixture_promotion_gate": str(M38_04_GATE.relative_to(ROOT)),
        },
        "scope": {
            "closeout_report": True,
            "changes_fixture_artifact": False,
            "mutates_runtime_deck_library": False,
            "bot_integration": False,
            "automatic_playbook_promotion": False,
            "GameState_mutation": False,
        },
        "key_results": {
            "recipe_id": summary.get("recipe_id", ""),
            "m38_complete": m38_complete,
            "first_runtime_fixture_available": bool(decision.get("fixture_created")),
            "fixture_path": decision.get("fixture_path", ""),
            "fixture_scope": fixture.get("fixture_scope", ""),
            "promotion_allowed": bool(summary.get("promotion_allowed")),
            "gate_passed_check_count": int(summary.get("passed_check_count", 0)),
            "gate_failed_check_count": int(summary.get("failed_check_count", 0)),
            "runtime_deck_library_mutated": bool(decision.get("runtime_deck_library_mutated")),
            "bot_playbook_enabled": bool(decision.get("bot_playbook_enabled")),
        },
        "decision": {
            "first_recipe_enters_fixture_scope": m38_complete,
            "first_recipe_remains_advisory_only": not m38_complete,
            "live_runtime_deck_enabled": False,
            "bot_playbook_enabled": False,
            "needs_user_team_review_before_live_deck_ui": True,
            "decision_notes": [
                "The first recipe can be used as an offline runtime/test fixture.",
                "The fixture is not a saved player deck and is not automatically visible in UI.",
                "Bot/playbook runtime use still needs a separate gate.",
            ]
            if m38_complete
            else [
                "The first recipe failed the runtime fixture gate.",
                "Do not create fixture consumers until gate blockers are repaired.",
            ],
        },
        "next_queue": {
            "id": next_queue_id,
            "title": "Fixture Consumption and Second-Slice Scale Gate"
            if m38_complete
            else "Runtime Fixture Repair",
            "goal": "Validate how the first fixture can be consumed safely, then decide whether to scale recipe work to the second slice."
            if m38_complete
            else "Repair failed fixture-gate checks before any consumption work.",
            "tasks": [
                {
                    "id": "M39-01",
                    "title": "Offline fixture schema validator",
                    "goal": "Validate runtime fixture artifacts independently from the M38 generator.",
                },
                {
                    "id": "M39-02",
                    "title": "Fixture-to-deck text exporter",
                    "goal": "Export the fixture as reviewable count-line deck text without adding it to saved decks.",
                },
                {
                    "id": "M39-03",
                    "title": "Headless fixture load smoke",
                    "goal": "Load the fixture through offline tooling/headless paths without UI or bot mutation.",
                },
                {
                    "id": "M39-04",
                    "title": "Second-slice recipe scale decision",
                    "goal": "Decide whether Oracle Think Tank moves into the same recipe repair pipeline.",
                },
            ]
            if m38_complete
            else [],
        },
        "summary": {
            "m38_complete": m38_complete,
            "first_runtime_fixture_available": bool(decision.get("fixture_created")),
            "next_queue_id": next_queue_id,
            "ready_for_next_queue": m38_complete,
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    results = report["key_results"]
    decision = report["decision"]
    next_queue = report["next_queue"]
    lines = [
        "# M38 First Runtime Fixture Closeout",
        "",
        "## Summary",
        "",
        f"- M38 complete: `{report['summary']['m38_complete']}`",
        f"- Recipe: `{results['recipe_id']}`",
        f"- First runtime fixture available: `{results['first_runtime_fixture_available']}`",
        f"- Fixture path: `{results['fixture_path']}`",
        f"- Fixture scope: `{results['fixture_scope']}`",
        f"- Gate passed checks: `{results['gate_passed_check_count']}`",
        f"- Gate failed checks: `{results['gate_failed_check_count']}`",
        f"- Runtime deck library mutated: `{results['runtime_deck_library_mutated']}`",
        f"- Bot playbook enabled: `{results['bot_playbook_enabled']}`",
        "",
        "## Decision",
        "",
        f"- First recipe enters fixture scope: `{decision['first_recipe_enters_fixture_scope']}`",
        f"- First recipe remains advisory only: `{decision['first_recipe_remains_advisory_only']}`",
        f"- Live runtime deck enabled: `{decision['live_runtime_deck_enabled']}`",
        f"- Needs user/team review before live deck UI: `{decision['needs_user_team_review_before_live_deck_ui']}`",
        "",
        "## Next Queue",
        "",
        f"- `{next_queue['id']}`: {next_queue['title']}",
        f"- Goal: {next_queue['goal']}",
        "",
    ]
    for task in next_queue.get("tasks", []):
        lines.append(f"- `{task['id']}`: {task['title']} - {task['goal']}")
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Closeout does not mutate fixture artifacts.",
            "- Closeout does not inject saved decks.",
            "- Closeout does not enable bot playbooks.",
            "- Closeout does not mutate GameState.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M38 first runtime fixture closeout.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_first_runtime_fixture_closeout()
    json_path = args.output_dir / "m38_closeout_first_runtime_fixture.json"
    md_path = args.output_dir / "m38_closeout_first_runtime_fixture.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M38 closeout first runtime fixture wrote {json_path}")
    print(f"M38 closeout first runtime fixture summary wrote {md_path}")
    print(
        "m38_complete={complete} fixture={fixture} next_queue={queue}".format(
            complete=report["summary"]["m38_complete"],
            fixture=report["summary"]["first_runtime_fixture_available"],
            queue=report["summary"]["next_queue_id"],
        )
    )
    return 0 if report["summary"]["ready_for_next_queue"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
