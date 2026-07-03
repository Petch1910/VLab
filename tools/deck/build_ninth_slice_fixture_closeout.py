"""Build the M69 closeout for the ninth-slice runtime fixture gate."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M69_06_GATE = OUTPUT_DIR / "m69_06_ninth_slice_runtime_fixture_promotion_gate.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _fixture_available(summary: dict[str, Any], decision: dict[str, Any], fixture: dict[str, Any]) -> bool:
    return (
        bool(summary.get("promotion_allowed"))
        and bool(decision.get("fixture_created"))
        and fixture.get("fixture_scope") == "offline_runtime_test_fixture"
    )


def _m70_queue() -> dict[str, Any]:
    return {
        "id": "M70",
        "title": "Ninth Fixture Consumption and Nine-Fixture Scale Gate",
        "goal": (
            "Validate how the Aqua Force fixture can be consumed safely, export reviewable deck text, "
            "run a headless load smoke, then decide whether the nine-fixture set is ready for another scale step."
        ),
        "tasks": [
            {
                "id": "M70-01",
                "title": "Ninth fixture schema validator",
                "goal": "Validate the Aqua Force runtime fixture independently from the M69 generator.",
            },
            {
                "id": "M70-02",
                "title": "Ninth fixture deck text exporter",
                "goal": "Export the Aqua Force fixture as reviewable count-line deck text without adding it to saved decks.",
            },
            {
                "id": "M70-03",
                "title": "Ninth fixture headless load smoke",
                "goal": (
                    "Load the Aqua Force fixture through offline/headless paths without UI, bot, G Zone, "
                    "Stride, Aqua Force battle-order runtime, or GameState mutation."
                ),
            },
            {
                "id": "M70-04",
                "title": "Nine-fixture scale decision",
                "goal": "Review all nine fixture evidence records before selecting any further slice.",
            },
        ],
    }


def build_ninth_slice_fixture_closeout(gate_report: dict[str, Any] | None = None) -> dict[str, Any]:
    gate_report = gate_report or load_json(M69_06_GATE)
    decision = gate_report["promotion_decision"]
    summary = gate_report["summary"]
    fixture = gate_report.get("runtime_fixture") or {}
    runtime_boundaries = fixture.get("runtime_boundaries", {})
    system_boundaries = fixture.get("system_boundaries", {})
    count_summary = fixture.get("count_summary", {})
    m69_complete = _fixture_available(summary, decision, fixture)
    next_queue_id = "M70" if m69_complete else "M69-repair"

    return {
        "version": "M69-closeout",
        "description": "Ninth-slice fixture closeout and next queue selection",
        "source_inputs": {
            "ninth_slice_runtime_fixture_promotion_gate": str(M69_06_GATE.relative_to(ROOT)),
        },
        "scope": {
            "closeout_report": True,
            "changes_fixture_artifact": False,
            "mutates_runtime_deck_library": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_integration": False,
            "automatic_playbook_promotion": False,
            "live_card_text_parsing": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "aqua_force_battle_order_runtime_enabled": False,
            "GameState_mutation": False,
        },
        "key_results": {
            "recipe_id": summary.get("recipe_id", ""),
            "selected_review_item_id": summary.get("selected_review_item_id", ""),
            "selected_g_zone_option_id": summary.get("selected_g_zone_option_id", ""),
            "selected_stride_option_id": summary.get("selected_stride_option_id", ""),
            "selected_aqua_force_option_id": summary.get("selected_aqua_force_option_id", ""),
            "m69_complete": m69_complete,
            "ninth_runtime_fixture_available": bool(decision.get("fixture_created")) and bool(fixture),
            "fixture_path": decision.get("fixture_path", ""),
            "fixture_scope": fixture.get("fixture_scope", ""),
            "promotion_allowed": bool(summary.get("promotion_allowed")),
            "gate_passed_check_count": int(summary.get("passed_check_count", 0)),
            "gate_failed_check_count": int(summary.get("failed_check_count", 0)),
            "main_deck_count": int(count_summary.get("main_deck_count", 0)),
            "trigger_count": int(count_summary.get("trigger_count", 0)),
            "grade4_main_deck_count": int(count_summary.get("grade4_main_deck_count", 0)),
            "runtime_deck_library_mutated": bool(decision.get("runtime_deck_library_mutated")),
            "saved_deck_injected": bool(decision.get("saved_deck_injected")),
            "ui_deck_list_published": bool(decision.get("ui_deck_list_published")),
            "bot_playbook_enabled": bool(decision.get("bot_playbook_enabled")),
            "g_zone_runtime_enabled": bool(decision.get("g_zone_runtime_enabled")),
            "stride_runtime_enabled": bool(decision.get("stride_runtime_enabled")),
            "aqua_force_battle_order_runtime_enabled": bool(
                decision.get("aqua_force_battle_order_runtime_enabled")
            ),
        },
        "decision": {
            "ninth_recipe_enters_fixture_scope": m69_complete,
            "ninth_recipe_remains_advisory_only": not m69_complete,
            "live_runtime_deck_enabled": False,
            "saved_deck_enabled": False,
            "ui_deck_list_enabled": False,
            "bot_playbook_enabled": False,
            "live_card_text_parsing_enabled": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "aqua_force_battle_order_runtime_enabled": False,
            "needs_user_team_review_before_live_deck_ui": True,
            "decision_notes": [
                "The ninth-slice Aqua Force recipe can be used as an offline runtime/test fixture.",
                "The fixture is main-deck-only; G Zone, Stride, and Aqua Force battle-order runtime remain disabled.",
                "The fixture is not a saved player deck and is not automatically visible in UI.",
                "Bot/playbook runtime use still needs a separate gate.",
            ]
            if m69_complete
            else [
                "The ninth-slice recipe failed the runtime fixture closeout.",
                "Do not create fixture consumers until gate blockers are repaired.",
            ],
        },
        "next_queue": _m70_queue()
        if m69_complete
        else {
            "id": next_queue_id,
            "title": "Ninth Fixture Repair",
            "goal": "Repair failed fixture-gate checks before any consumption work.",
            "tasks": [],
        },
        "summary": {
            "m69_complete": m69_complete,
            "ninth_runtime_fixture_available": bool(decision.get("fixture_created")) and bool(fixture),
            "g_zone_runtime_enabled": bool(runtime_boundaries.get("g_zone_runtime_enabled")),
            "stride_runtime_enabled": bool(runtime_boundaries.get("stride_runtime_enabled")),
            "aqua_force_battle_order_runtime_enabled": bool(
                runtime_boundaries.get("aqua_force_battle_order_runtime_enabled")
            ),
            "g_zone_boundary_applied": bool(system_boundaries.get("g_zone_boundary_applied")),
            "stride_boundary_applied": bool(system_boundaries.get("stride_boundary_applied")),
            "aqua_force_boundary_applied": bool(system_boundaries.get("aqua_force_boundary_applied")),
            "next_queue_id": next_queue_id,
            "ready_for_next_queue": m69_complete,
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
        "# M69 Ninth-Slice Fixture Closeout",
        "",
        "## Summary",
        "",
        f"- M69 complete: `{report['summary']['m69_complete']}`",
        f"- Recipe: `{results['recipe_id']}`",
        f"- Selected review item: `{results['selected_review_item_id']}`",
        f"- Selected G Zone option: `{results['selected_g_zone_option_id']}`",
        f"- Selected Stride option: `{results['selected_stride_option_id']}`",
        f"- Selected Aqua Force option: `{results['selected_aqua_force_option_id']}`",
        f"- Ninth runtime fixture available: `{results['ninth_runtime_fixture_available']}`",
        f"- Fixture path: `{results['fixture_path']}`",
        f"- Fixture scope: `{results['fixture_scope']}`",
        f"- Gate passed checks: `{results['gate_passed_check_count']}`",
        f"- Gate failed checks: `{results['gate_failed_check_count']}`",
        f"- Main deck count: `{results['main_deck_count']}`",
        f"- Trigger count: `{results['trigger_count']}`",
        f"- Grade 4 main deck count: `{results['grade4_main_deck_count']}`",
        f"- Runtime deck library mutated: `{results['runtime_deck_library_mutated']}`",
        f"- Saved deck injected: `{results['saved_deck_injected']}`",
        f"- UI deck list published: `{results['ui_deck_list_published']}`",
        f"- Bot playbook enabled: `{results['bot_playbook_enabled']}`",
        f"- G Zone runtime enabled: `{results['g_zone_runtime_enabled']}`",
        f"- Stride runtime enabled: `{results['stride_runtime_enabled']}`",
        f"- Aqua Force battle-order runtime enabled: `{results['aqua_force_battle_order_runtime_enabled']}`",
        "",
        "## Decision",
        "",
        f"- Ninth recipe enters fixture scope: `{decision['ninth_recipe_enters_fixture_scope']}`",
        f"- Ninth recipe remains advisory only: `{decision['ninth_recipe_remains_advisory_only']}`",
        f"- Live runtime deck enabled: `{decision['live_runtime_deck_enabled']}`",
        f"- Saved deck enabled: `{decision['saved_deck_enabled']}`",
        f"- UI deck list enabled: `{decision['ui_deck_list_enabled']}`",
        f"- Bot playbook enabled: `{decision['bot_playbook_enabled']}`",
        f"- Live card text parsing enabled: `{decision['live_card_text_parsing_enabled']}`",
        f"- G Zone runtime enabled: `{decision['g_zone_runtime_enabled']}`",
        f"- Stride runtime enabled: `{decision['stride_runtime_enabled']}`",
        f"- Aqua Force battle-order runtime enabled: `{decision['aqua_force_battle_order_runtime_enabled']}`",
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
            "- Closeout does not enable G Zone, Stride, Aqua Force battle-order, or related runtime behavior.",
            "- Closeout does not parse live card text.",
            "- Closeout does not mutate GameState.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M69 ninth-slice fixture closeout.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_ninth_slice_fixture_closeout()
    json_path = args.output_dir / "m69_closeout_ninth_slice_fixture.json"
    md_path = args.output_dir / "m69_closeout_ninth_slice_fixture.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M69 closeout ninth-slice fixture wrote {json_path}")
    print(f"M69 closeout ninth-slice fixture summary wrote {md_path}")
    print(
        "m69_complete={complete} fixture={fixture} next_queue={queue}".format(
            complete=report["summary"]["m69_complete"],
            fixture=report["summary"]["ninth_runtime_fixture_available"],
            queue=report["summary"]["next_queue_id"],
        )
    )
    return 0 if report["summary"]["ready_for_next_queue"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
