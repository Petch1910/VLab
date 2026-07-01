"""Build the M35 Hybrid Vertical-Slice Strategy closeout report.

The closeout is an offline coordination artifact. It summarizes the completed
M35 A-E pipeline and selects the next non-runtime implementation queue.
"""

from __future__ import annotations

import argparse
import json
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


@dataclass(frozen=True)
class MilestoneInput:
    milestone: str
    phase: str
    title: str
    filename: str

    @property
    def path(self) -> Path:
        return OUTPUT_DIR / self.filename


MILESTONE_INPUTS: tuple[MilestoneInput, ...] = (
    MilestoneInput("M35-A2", "A", "First target slice selection", "m35_a2_first_target_slice_report.json"),
    MilestoneInput("M35-A3", "A", "First-slice legality fixtures", "m35_a3_first_slice_deck_legality_fixtures.json"),
    MilestoneInput("M35-A4", "A", "First-slice feasibility refresh", "m35_a4_first_slice_feasibility_refresh.json"),
    MilestoneInput("M35-B1", "B", "Selected-slice semantic vocabulary", "m35_b1_first_slice_semantic_vocabulary.json"),
    MilestoneInput("M35-B2", "B", "Selected-slice semantic extractor", "m35_b2_first_slice_semantic_tags.json"),
    MilestoneInput("M35-B3", "B", "Requirement/provider model", "m35_b3_first_slice_requirement_provider_model.json"),
    MilestoneInput("M35-B4", "B", "Manual review queue", "m35_b4_first_slice_manual_review_queue.json"),
    MilestoneInput("M35-C1", "C", "Pair compatibility graph", "m35_c1_first_slice_pair_compatibility_graph.json"),
    MilestoneInput("M35-C2", "C", "Resource conflict detector", "m35_c2_first_slice_resource_conflict_detector.json"),
    MilestoneInput("M35-C3", "C", "Timing compatibility detector", "m35_c3_first_slice_timing_compatibility_detector.json"),
    MilestoneInput("M35-C4", "C", "Zone/target compatibility detector", "m35_c4_first_slice_zone_target_detector.json"),
    MilestoneInput("M35-C5", "C", "Selected-slice compatibility output", "m35_c5_first_slice_selected_compatibility_output.json"),
    MilestoneInput("M35-D1", "D", "Candidate package selection", "m35_d1_first_slice_candidate_packages.json"),
    MilestoneInput("M35-D2", "D", "Deck skeleton ratio planner", "m35_d2_first_slice_deck_skeleton_ratio_plans.json"),
    MilestoneInput("M35-D3", "D", "Combo line explainer", "m35_d3_first_slice_combo_line_explainer.json"),
    MilestoneInput("M35-D4", "D", "Reviewed playbook seed export", "m35_d4_first_slice_reviewed_playbook_seed.json"),
    MilestoneInput("M35-E1", "E", "Second slice selection", "m35_e1_second_target_slice_report.json"),
    MilestoneInput("M35-E2", "E", "Second-slice fixture readiness", "m35_e2_second_slice_fixture_readiness.json"),
    MilestoneInput("M35-E3", "E", "Generalized semantic/compatibility probe", "m35_e3_generalized_semantic_compatibility_probe.json"),
    MilestoneInput("M35-E4", "E", "Bot integration gate", "m35_e4_bot_integration_gate.json"),
)


PHASES = {
    "A": "Foundation Slice",
    "B": "Semantic Slice",
    "C": "Compatibility Slice",
    "D": "Deck Skeleton + Safe Playbook Seed",
    "E": "Scale Out + Bot Gate",
}


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _get(data: dict[str, Any], path: Sequence[str], default: Any = None) -> Any:
    cursor: Any = data
    for key in path:
        if not isinstance(cursor, dict) or key not in cursor:
            return default
        cursor = cursor[key]
    return cursor


def _load_inputs() -> dict[str, dict[str, Any]]:
    return {item.milestone: load_json(item.path) for item in MILESTONE_INPUTS}


def _input_checks() -> list[dict[str, Any]]:
    return [
        {
            "milestone": item.milestone,
            "phase": item.phase,
            "title": item.title,
            "path": str(item.path.relative_to(ROOT)),
            "exists": item.path.exists(),
        }
        for item in MILESTONE_INPUTS
    ]


def _phase_summaries(checks: list[dict[str, Any]]) -> list[dict[str, Any]]:
    summaries: list[dict[str, Any]] = []
    for phase, title in PHASES.items():
        phase_checks = [item for item in checks if item["phase"] == phase]
        summaries.append(
            {
                "phase": phase,
                "title": title,
                "status": "done" if all(item["exists"] for item in phase_checks) else "blocked",
                "milestones": [item["milestone"] for item in phase_checks],
                "output_count": len(phase_checks),
                "all_outputs_present": all(item["exists"] for item in phase_checks),
            }
        )
    return summaries


def _key_results(reports: dict[str, dict[str, Any]]) -> dict[str, Any]:
    c5 = reports["M35-C5"]
    d4 = reports["M35-D4"]
    e3 = reports["M35-E3"]
    e4 = reports["M35-E4"]
    return {
        "first_slice": {
            "selected_target": reports["M35-A2"].get("selected_target", {}),
            "phase_a_ready_for_phase_b": _get(
                reports["M35-A4"], ["readiness", "phase_a_ready_for_phase_b"],
                reports["M35-A4"].get("phase_a_ready_for_phase_b", False),
            ),
            "semantic_tagged_cards": _get(
                reports["M35-B2"], ["summary", "tagged_card_count"],
                _get(reports["M35-B2"], ["summary", "card_count"], 0),
            ),
            "manual_review_cards": _get(
                reports["M35-B4"], ["summary", "manual_review_card_count"],
                _get(reports["M35-B4"], ["summary", "manual_review_count"], 0),
            ),
            "compatibility_edges": _get(c5, ["summary", "edge_count"], 0),
            "clean_candidate_edges": _get(c5, ["summary", "m35_d1_candidate_edge_count"], 0),
            "candidate_packages": _get(
                reports["M35-D1"], ["summary", "candidate_package_count"],
                _get(reports["M35-D1"], ["summary", "package_count"], 0),
            ),
            "deck_skeletons": _get(
                reports["M35-D2"], ["summary", "deck_skeleton_count"],
                _get(reports["M35-D2"], ["summary", "skeleton_count"], 0),
            ),
            "combo_lines": _get(reports["M35-D3"], ["summary", "combo_line_count"], 0),
            "reviewed_playbook_seed_entries": _get(d4, ["summary", "seed_entry_count"], 0),
            "rejected_playbook_lines": _get(d4, ["summary", "rejected_line_count"], 0),
        },
        "second_slice": {
            "selected_target": reports["M35-E1"].get("selected_target", {}),
            "fixture_expectations_met": _get(
                reports["M35-E2"], ["readiness", "expectations_met"],
                _get(reports["M35-E2"], ["readiness", "all_fixture_expectations_met"], False),
            ),
            "classic_core_policy_reusable": _get(
                reports["M35-E2"], ["readiness", "classic_core_policy_reusable"], False
            ),
            "probe_ready": _get(
                e3,
                ["readiness", "ready"],
                _get(e3, ["readiness", "second_slice_semantic_compatibility_probe_passed"], False),
            ),
            "probe_card_count": _get(
                e3,
                ["stage_summaries", "b2_semantic_tags", "tagged_card_count"],
                _get(e3, ["stage_summaries", "b2_semantic_tags", "card_count"], 0),
            ),
            "probe_edge_count": _get(e3, ["stage_summaries", "c5_compatibility", "edge_count"], 0),
            "probe_candidate_edges": _get(
                e3,
                ["stage_summaries", "c5_compatibility", "m35_d1_candidate_edge_count"],
                0,
            ),
        },
        "bot_gate": {
            "gate_passed": _get(e4, ["readiness", "gate_passed"], False),
            "future_hint_candidate_count": _get(
                e4, ["readiness", "reviewed_hint_candidate_count"], 0
            ),
            "blocked_source_count": _get(e4, ["readiness", "blocked_source_count"], 0),
            "runtime_bot_integration_enabled": _get(
                e4, ["readiness", "runtime_bot_integration_enabled"], True
            ),
        },
    }


def _next_queue_selection() -> dict[str, Any]:
    return {
        "milestone": "M36",
        "name": "Human-review-assisted deck recipe validation",
        "why_this_next": [
            "M35 produced advisory compatibility and one reviewed future bot hint, but it is not an executable deck or runtime playbook.",
            "M35-D4 rejected 24 combo lines that need explicit review or support-gap handling before expansion.",
            "Deck recipes need source-backed legality, card-quantity, trigger, grade, shield, and combo-line checks before runtime/bot work resumes.",
        ],
        "first_tasks": [
            {
                "id": "M36-01",
                "title": "First-slice review packet",
                "goal": "Create a human-review packet for rejected lines, manual-review cards, and the one accepted seed.",
            },
            {
                "id": "M36-02",
                "title": "Deck recipe draft model",
                "goal": "Convert advisory skeletons into explicit draft recipes with card quantities and validation metadata.",
            },
            {
                "id": "M36-03",
                "title": "Deck recipe validator",
                "goal": "Validate clan/format, main/trigger/grade counts, ride deck constraints where applicable, and missing-card paths.",
            },
            {
                "id": "M36-04",
                "title": "Combo-line to recipe consistency check",
                "goal": "Confirm selected combo lines are actually present in the draft recipe and do not rely on blocked/manual-review cards.",
            },
            {
                "id": "M36-05",
                "title": "Second-slice readiness comparison",
                "goal": "Compare Oracle Think Tank probe outputs against the first-slice pipeline before broader scale-out.",
            },
            {
                "id": "M36-closeout",
                "title": "Deck recipe validation closeout",
                "goal": "Decide whether to expand more clans, build source-backed ability fixtures, or open a separate runtime/bot milestone.",
            },
        ],
        "hard_gates": [
            "no runtime bot wiring",
            "no live card text parsing",
            "no direct GameState mutation",
            "no automatic deck injection",
            "human review required before playbook/runtime promotion",
        ],
    }


def build_closeout(reports: dict[str, dict[str, Any]] | None = None) -> dict[str, Any]:
    reports = reports or _load_inputs()
    checks = _input_checks()
    phases = _phase_summaries(checks)
    results = _key_results(reports)
    all_inputs_present = all(item["exists"] for item in checks)
    m35_complete = (
        all_inputs_present
        and all(phase["status"] == "done" for phase in phases)
        and bool(results["bot_gate"]["gate_passed"])
        and not bool(results["bot_gate"]["runtime_bot_integration_enabled"])
    )
    return {
        "version": "M35-closeout",
        "description": "Hybrid Vertical-Slice Strategy closeout and next queue selection",
        "scope": {
            "offline_coordination_artifact": True,
            "changes_runtime_gameplay": False,
            "changes_unity_ui": False,
            "enables_bot_runtime": False,
        },
        "input_checks": checks,
        "phase_summaries": phases,
        "key_results": results,
        "readiness": {
            "all_inputs_present": all_inputs_present,
            "all_phases_closed": all(phase["status"] == "done" for phase in phases),
            "m35_hybrid_vertical_slice_complete": m35_complete,
            "safe_to_continue_to_next_queue": m35_complete,
            "runtime_bot_integration_enabled": False,
        },
        "next_queue_selection": _next_queue_selection(),
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    readiness = report["readiness"]
    results = report["key_results"]
    next_queue = report["next_queue_selection"]
    first = results["first_slice"]
    second = results["second_slice"]
    bot = results["bot_gate"]
    lines = [
        "# M35 Hybrid Vertical-Slice Closeout",
        "",
        "## Summary",
        "",
        f"- M35 complete: `{readiness['m35_hybrid_vertical_slice_complete']}`",
        f"- All inputs present: `{readiness['all_inputs_present']}`",
        f"- All phases closed: `{readiness['all_phases_closed']}`",
        f"- Runtime bot integration enabled: `{readiness['runtime_bot_integration_enabled']}`",
        "",
        "## Phase Status",
        "",
    ]
    for phase in report["phase_summaries"]:
        lines.append(
            f"- Phase {phase['phase']} / {phase['title']}: `{phase['status']}` "
            f"({phase['output_count']} outputs)"
        )
    lines.extend(
        [
            "",
            "## Key Results",
            "",
            f"- First slice clean candidate edges: `{first['clean_candidate_edges']}`",
            f"- First slice candidate packages: `{first['candidate_packages']}`",
            f"- First slice deck skeletons: `{first['deck_skeletons']}`",
            f"- First slice combo lines: `{first['combo_lines']}`",
            f"- Reviewed playbook seed entries: `{first['reviewed_playbook_seed_entries']}`",
            f"- Rejected playbook lines: `{first['rejected_playbook_lines']}`",
            f"- Second slice probe cards: `{second['probe_card_count']}`",
            f"- Second slice probe edges: `{second['probe_edge_count']}`",
            f"- Second slice probe candidate edges: `{second['probe_candidate_edges']}`",
            f"- Bot gate passed: `{bot['gate_passed']}`",
            f"- Future bot hint candidates: `{bot['future_hint_candidate_count']}`",
            f"- Blocked bot sources: `{bot['blocked_source_count']}`",
            "",
            "## Next Queue",
            "",
            f"`{next_queue['milestone']}`: {next_queue['name']}",
            "",
            "First tasks:",
            "",
        ]
    )
    for task in next_queue["first_tasks"]:
        lines.append(f"- `{task['id']}`: {task['title']}")
    lines.extend(["", "Hard gates:", ""])
    for gate in next_queue["hard_gates"]:
        lines.append(f"- {gate}")
    lines.append("")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35 Hybrid Vertical-Slice closeout report.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_closeout()
    json_path = args.output_dir / "m35_closeout_hybrid_vertical_slice.json"
    md_path = args.output_dir / "m35_closeout_hybrid_vertical_slice.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35 closeout wrote {json_path}")
    print(f"M35 closeout summary wrote {md_path}")
    print(
        "complete={complete} next_queue={queue}".format(
            complete=report["readiness"]["m35_hybrid_vertical_slice_complete"],
            queue=report["next_queue_selection"]["milestone"],
        )
    )
    return 0 if report["readiness"]["m35_hybrid_vertical_slice_complete"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
