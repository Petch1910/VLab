"""Build M40-closeout second-slice runtime readiness decision."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M40_01_PACKET = OUTPUT_DIR / "m40_01_second_slice_review_packet.json"
M40_02_DRAFTS = OUTPUT_DIR / "m40_02_second_slice_recipe_draft_model.json"
M40_03_VALIDATION = OUTPUT_DIR / "m40_03_second_slice_recipe_validation_report.json"
M40_04_CONSISTENCY = OUTPUT_DIR / "m40_04_second_slice_combo_recipe_consistency_report.json"
M40_05_REPAIRS = OUTPUT_DIR / "m40_05_second_slice_blocker_repair_candidates.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _source_inputs() -> dict[str, str]:
    return {
        "m40_01_second_slice_review_packet": str(M40_01_PACKET.relative_to(ROOT)),
        "m40_02_second_slice_recipe_draft_model": str(M40_02_DRAFTS.relative_to(ROOT)),
        "m40_03_second_slice_recipe_validation_report": str(M40_03_VALIDATION.relative_to(ROOT)),
        "m40_04_second_slice_combo_recipe_consistency_report": str(M40_04_CONSISTENCY.relative_to(ROOT)),
        "m40_05_second_slice_blocker_repair_candidates": str(M40_05_REPAIRS.relative_to(ROOT)),
    }


def _input_checks() -> list[dict[str, Any]]:
    paths = [
        ("M40-01", "Second-slice review packet", M40_01_PACKET),
        ("M40-02", "Second-slice recipe draft model", M40_02_DRAFTS),
        ("M40-03", "Second-slice recipe validator", M40_03_VALIDATION),
        ("M40-04", "Second-slice combo-to-recipe consistency", M40_04_CONSISTENCY),
        ("M40-05", "Second-slice blocker repair candidates", M40_05_REPAIRS),
    ]
    return [
        {
            "milestone": milestone,
            "title": title,
            "path": str(path.relative_to(ROOT)),
            "exists": path.exists(),
        }
        for milestone, title, path in paths
    ]


def _next_queue_selection(human_repair_review_allowed: bool) -> dict[str, Any]:
    if not human_repair_review_allowed:
        return {
            "id": "M40-repair",
            "title": "Second-slice Repair Evidence Completion",
            "goal": "Repair missing M40 evidence before selecting a human review queue.",
            "tasks": [],
        }
    return {
        "id": "M41",
        "title": "Second-slice Human Repair Review Gate",
        "goal": "Review M40-05 repair candidates, record explicit acceptance or rejection, then rerun validation before any runtime fixture gate.",
        "tasks": [
            {
                "id": "M41-01",
                "title": "Second-slice human repair review packet",
                "goal": "Export a concise review packet for the M40-05 repair packages.",
            },
            {
                "id": "M41-02",
                "title": "Second-slice human-accepted repair artifact",
                "goal": "Record explicit acceptance or rejection of one repaired Oracle Think Tank recipe.",
            },
            {
                "id": "M41-03",
                "title": "Second-slice repaired recipe validation rerun",
                "goal": "Apply accepted repair in memory and rerun count, trigger, grade, copy-limit, clan, and manual-overlap validation.",
            },
            {
                "id": "M41-04",
                "title": "Second-slice runtime fixture promotion gate",
                "goal": "Promote only if repaired validation, consistency, and human acceptance all pass.",
            },
            {
                "id": "M41-closeout",
                "title": "Second-slice fixture closeout",
                "goal": "Decide whether Oracle Think Tank enters offline fixture scope or remains advisory.",
            },
        ],
    }


def build_second_slice_runtime_readiness_closeout(
    packet_report: dict[str, Any] | None = None,
    draft_report: dict[str, Any] | None = None,
    validation_report: dict[str, Any] | None = None,
    consistency_report: dict[str, Any] | None = None,
    repair_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    packet_report = packet_report or load_json(M40_01_PACKET)
    draft_report = draft_report or load_json(M40_02_DRAFTS)
    validation_report = validation_report or load_json(M40_03_VALIDATION)
    consistency_report = consistency_report or load_json(M40_04_CONSISTENCY)
    repair_report = repair_report or load_json(M40_05_REPAIRS)

    checks = _input_checks()
    validation_summary = validation_report.get("summary", {})
    consistency_summary = consistency_report.get("summary", {})
    repair_summary = repair_report.get("summary", {})
    m40_complete = (
        all(item["exists"] for item in checks)
        and bool(packet_report.get("summary", {}).get("ready_for_m40_02"))
        and bool(draft_report.get("summary", {}).get("ready_for_m40_03"))
        and bool(validation_summary.get("ready_for_m40_04"))
        and bool(consistency_summary.get("ready_for_m40_05"))
        and bool(repair_summary.get("ready_for_m40_closeout"))
    )
    runtime_ready_count = int(validation_summary.get("runtime_ready_recipe_count", 0))
    promotion_allowed_count = int(consistency_summary.get("promotion_allowed_count", 0))
    human_repair_ready_count = int(repair_summary.get("ready_for_human_repair_review_count", 0))
    runtime_ready = m40_complete and runtime_ready_count > 0 and promotion_allowed_count > 0
    human_repair_review_allowed = m40_complete and not runtime_ready and human_repair_ready_count > 0
    blockers: list[str] = []
    if runtime_ready_count == 0:
        blockers.append("no_runtime_ready_recipe")
    if promotion_allowed_count == 0:
        blockers.append("no_promotion_allowed_consistency_check")
    if int(validation_summary.get("manual_review_overlap_recipe_count", 0)) > 0:
        blockers.append("manual_review_overlap_unresolved")
    if human_repair_ready_count > 0:
        blockers.append("repair_candidates_require_human_acceptance")
    blockers.append("runtime_fixture_gate_not_run")

    next_queue = _next_queue_selection(human_repair_review_allowed)
    return {
        "version": "M40-closeout",
        "description": "Second-slice runtime readiness decision",
        "source_inputs": _source_inputs(),
        "scope": {
            "closeout_report": True,
            "changes_recipe_draft_file": False,
            "records_human_acceptance": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
        },
        "input_checks": checks,
        "key_results": {
            "selected_target": repair_report.get("selected_target", {}),
            "review_items": packet_report.get("summary", {}).get("total_review_item_count"),
            "recipe_drafts": draft_report.get("summary", {}).get("recipe_draft_count"),
            "runtime_ready_recipe_count": runtime_ready_count,
            "promotion_allowed_count": promotion_allowed_count,
            "manual_review_overlap_recipe_count": validation_summary.get("manual_review_overlap_recipe_count"),
            "repair_candidates_ready_for_human_review": human_repair_ready_count,
            "grade_profile_complete_candidate_count": repair_summary.get("grade_profile_complete_candidate_count"),
        },
        "decision": {
            "m40_complete": m40_complete,
            "second_slice_runtime_ready_recipe_available": runtime_ready,
            "second_slice_can_enter_runtime_fixture_gate_now": runtime_ready,
            "second_slice_remains_advisory": not runtime_ready,
            "human_repair_review_allowed": human_repair_review_allowed,
            "live_runtime_deck_enabled": False,
            "saved_deck_or_ui_publication_allowed": False,
            "bot_playbook_enabled": False,
            "decision_blockers": blockers,
            "recommendation": (
                "open_m41_human_repair_review_gate"
                if human_repair_review_allowed
                else "repair_m40_evidence_before_runtime_decision"
            ),
        },
        "next_queue": next_queue,
        "summary": {
            "m40_complete": m40_complete,
            "runtime_ready_recipe_available": runtime_ready,
            "human_repair_review_allowed": human_repair_review_allowed,
            "next_queue_id": next_queue["id"],
            "ready_for_next_queue": human_repair_review_allowed,
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    results = report["key_results"]
    next_queue = report["next_queue"]
    lines = [
        "# M40 Second-Slice Runtime Readiness Closeout",
        "",
        "## Summary",
        "",
        f"- M40 complete: `{summary['m40_complete']}`",
        f"- Runtime-ready recipe available: `{summary['runtime_ready_recipe_available']}`",
        f"- Human repair review allowed: `{summary['human_repair_review_allowed']}`",
        f"- Next queue: `{summary['next_queue_id']}`",
        f"- Ready for next queue: `{summary['ready_for_next_queue']}`",
        "",
        "## Key Results",
        "",
        f"- Review items: `{results['review_items']}`",
        f"- Recipe drafts: `{results['recipe_drafts']}`",
        f"- Runtime-ready recipes: `{results['runtime_ready_recipe_count']}`",
        f"- Promotion-allowed checks: `{results['promotion_allowed_count']}`",
        f"- Manual-review overlap recipes: `{results['manual_review_overlap_recipe_count']}`",
        f"- Repair candidates ready for human review: `{results['repair_candidates_ready_for_human_review']}`",
        "",
        "## Decision",
        "",
        f"- Runtime fixture gate now: `{decision['second_slice_can_enter_runtime_fixture_gate_now']}`",
        f"- Second slice remains advisory: `{decision['second_slice_remains_advisory']}`",
        f"- Saved deck/UI publication allowed: `{decision['saved_deck_or_ui_publication_allowed']}`",
        f"- Bot playbook enabled: `{decision['bot_playbook_enabled']}`",
        f"- Decision blockers: `{decision['decision_blockers']}`",
        f"- Recommendation: `{decision['recommendation']}`",
        "",
        "## Next Queue",
        "",
        f"`{next_queue['id']}`: {next_queue['title']}",
        "",
        next_queue["goal"],
        "",
    ]
    for task in next_queue.get("tasks", []):
        lines.append(f"- `{task['id']}`: {task['title']} - {task['goal']}")
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Closeout does not mutate draft artifacts.",
            "- Closeout does not record human acceptance.",
            "- Closeout does not inject saved decks or publish UI deck lists.",
            "- Closeout does not create runtime fixtures or enable bot playbooks.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M40 second-slice runtime readiness closeout.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_second_slice_runtime_readiness_closeout()
    json_path = args.output_dir / "m40_closeout_second_slice_runtime_readiness.json"
    md_path = args.output_dir / "m40_closeout_second_slice_runtime_readiness.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M40 closeout second-slice runtime readiness wrote {json_path}")
    print(f"M40 closeout second-slice runtime readiness summary wrote {md_path}")
    print(
        "m40_complete={complete} runtime_ready={runtime_ready} next_queue={queue}".format(
            complete=report["summary"]["m40_complete"],
            runtime_ready=report["summary"]["runtime_ready_recipe_available"],
            queue=report["summary"]["next_queue_id"],
        )
    )
    return 0 if report["summary"]["ready_for_next_queue"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
