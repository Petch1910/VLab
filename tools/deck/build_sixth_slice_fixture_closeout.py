"""Build the M57 closeout for the sixth-slice runtime fixture gate."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M57_06_GATE = OUTPUT_DIR / "m57_06_sixth_slice_runtime_fixture_promotion_gate.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def build_sixth_slice_fixture_closeout(gate_report: dict[str, Any] | None = None) -> dict[str, Any]:
    gate_report = gate_report or load_json(M57_06_GATE)
    decision = gate_report["promotion_decision"]
    summary = gate_report["summary"]
    fixture = gate_report.get("runtime_fixture") or {}
    runtime_boundaries = fixture.get("runtime_boundaries", {})
    m57_complete = bool(summary.get("promotion_allowed")) and bool(decision.get("fixture_created"))
    next_queue_id = "M58" if m57_complete else "M57-repair"
    return {
        "version": "M57-closeout",
        "description": "Sixth-slice fixture closeout and next queue selection",
        "source_inputs": {
            "sixth_slice_runtime_fixture_promotion_gate": str(M57_06_GATE.relative_to(ROOT)),
        },
        "scope": {
            "closeout_report": True,
            "changes_fixture_artifact": False,
            "mutates_runtime_deck_library": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_integration": False,
            "automatic_playbook_promotion": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "GameState_mutation": False,
        },
        "key_results": {
            "recipe_id": summary.get("recipe_id", ""),
            "accepted_review_item_id": summary.get("accepted_review_item_id", ""),
            "selected_g_zone_option_id": summary.get("selected_g_zone_option_id", ""),
            "m57_complete": m57_complete,
            "sixth_runtime_fixture_available": bool(decision.get("fixture_created")),
            "fixture_path": decision.get("fixture_path", ""),
            "fixture_scope": fixture.get("fixture_scope", ""),
            "promotion_allowed": bool(summary.get("promotion_allowed")),
            "gate_passed_check_count": int(summary.get("passed_check_count", 0)),
            "gate_failed_check_count": int(summary.get("failed_check_count", 0)),
            "runtime_deck_library_mutated": bool(decision.get("runtime_deck_library_mutated")),
            "saved_deck_injected": bool(decision.get("saved_deck_injected")),
            "ui_deck_list_published": bool(decision.get("ui_deck_list_published")),
            "bot_playbook_enabled": bool(decision.get("bot_playbook_enabled")),
            "g_zone_runtime_enabled": bool(decision.get("g_zone_runtime_enabled")),
            "stride_runtime_enabled": bool(decision.get("stride_runtime_enabled")),
        },
        "decision": {
            "sixth_recipe_enters_fixture_scope": m57_complete,
            "sixth_recipe_remains_advisory_only": not m57_complete,
            "live_runtime_deck_enabled": False,
            "saved_deck_enabled": False,
            "ui_deck_list_enabled": False,
            "bot_playbook_enabled": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "needs_user_team_review_before_live_deck_ui": True,
            "decision_notes": [
                "The sixth-slice Shadow Paladin recipe can be used as an offline runtime/test fixture.",
                "The fixture is main-deck-only; G Zone and Stride runtime remain disabled.",
                "The fixture is not a saved player deck and is not automatically visible in UI.",
                "Bot/playbook runtime use still needs a separate gate.",
            ]
            if m57_complete
            else [
                "The sixth-slice recipe failed the runtime fixture gate.",
                "Do not create fixture consumers until gate blockers are repaired.",
            ],
        },
        "next_queue": {
            "id": next_queue_id,
            "title": "Sixth Fixture Consumption and Six-Fixture Scale Gate"
            if m57_complete
            else "Sixth Fixture Repair",
            "goal": "Validate how the Shadow Paladin fixture can be consumed safely, export reviewable deck text, run a headless load smoke, then decide whether the fixture set is ready for another scale step."
            if m57_complete
            else "Repair failed fixture-gate checks before any consumption work.",
            "tasks": [
                {
                    "id": "M58-01",
                    "title": "Sixth fixture schema validator",
                    "goal": "Validate the Shadow Paladin runtime fixture independently from the M57 generator.",
                },
                {
                    "id": "M58-02",
                    "title": "Sixth fixture deck text exporter",
                    "goal": "Export the Shadow Paladin fixture as reviewable count-line deck text without adding it to saved decks.",
                },
                {
                    "id": "M58-03",
                    "title": "Sixth fixture headless load smoke",
                    "goal": "Load the Shadow Paladin fixture through offline/headless paths without UI, bot, G Zone, or Stride mutation.",
                },
                {
                    "id": "M58-04",
                    "title": "Six-fixture scale decision",
                    "goal": "Review all six fixture evidence before selecting any further slice.",
                },
            ]
            if m57_complete
            else [],
        },
        "summary": {
            "m57_complete": m57_complete,
            "sixth_runtime_fixture_available": bool(decision.get("fixture_created")),
            "g_zone_runtime_enabled": bool(runtime_boundaries.get("g_zone_runtime_enabled")),
            "stride_runtime_enabled": bool(runtime_boundaries.get("stride_runtime_enabled")),
            "next_queue_id": next_queue_id,
            "ready_for_next_queue": m57_complete,
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
        "# M57 Sixth-Slice Fixture Closeout",
        "",
        "## Summary",
        "",
        f"- M57 complete: `{report['summary']['m57_complete']}`",
        f"- Recipe: `{results['recipe_id']}`",
        f"- Accepted review item: `{results['accepted_review_item_id']}`",
        f"- Selected G Zone option: `{results['selected_g_zone_option_id']}`",
        f"- Sixth runtime fixture available: `{results['sixth_runtime_fixture_available']}`",
        f"- Fixture path: `{results['fixture_path']}`",
        f"- Fixture scope: `{results['fixture_scope']}`",
        f"- Gate passed checks: `{results['gate_passed_check_count']}`",
        f"- Gate failed checks: `{results['gate_failed_check_count']}`",
        f"- Runtime deck library mutated: `{results['runtime_deck_library_mutated']}`",
        f"- Saved deck injected: `{results['saved_deck_injected']}`",
        f"- UI deck list published: `{results['ui_deck_list_published']}`",
        f"- Bot playbook enabled: `{results['bot_playbook_enabled']}`",
        f"- G Zone runtime enabled: `{results['g_zone_runtime_enabled']}`",
        f"- Stride runtime enabled: `{results['stride_runtime_enabled']}`",
        "",
        "## Decision",
        "",
        f"- Sixth recipe enters fixture scope: `{decision['sixth_recipe_enters_fixture_scope']}`",
        f"- Sixth recipe remains advisory only: `{decision['sixth_recipe_remains_advisory_only']}`",
        f"- Live runtime deck enabled: `{decision['live_runtime_deck_enabled']}`",
        f"- Saved deck enabled: `{decision['saved_deck_enabled']}`",
        f"- UI deck list enabled: `{decision['ui_deck_list_enabled']}`",
        f"- Bot playbook enabled: `{decision['bot_playbook_enabled']}`",
        f"- G Zone runtime enabled: `{decision['g_zone_runtime_enabled']}`",
        f"- Stride runtime enabled: `{decision['stride_runtime_enabled']}`",
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
            "- Closeout does not publish UI deck lists.",
            "- Closeout does not enable bot playbooks.",
            "- Closeout does not enable G Zone or Stride runtime.",
            "- Closeout does not mutate GameState.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M57 sixth-slice fixture closeout.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_sixth_slice_fixture_closeout()
    json_path = args.output_dir / "m57_closeout_sixth_slice_fixture.json"
    md_path = args.output_dir / "m57_closeout_sixth_slice_fixture.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M57 closeout sixth-slice fixture wrote {json_path}")
    print(f"M57 closeout sixth-slice fixture summary wrote {md_path}")
    print(
        "m57_complete={complete} fixture={fixture} next_queue={queue}".format(
            complete=report["summary"]["m57_complete"],
            fixture=report["summary"]["sixth_runtime_fixture_available"],
            queue=report["summary"]["next_queue_id"],
        )
    )
    return 0 if report["summary"]["ready_for_next_queue"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
