"""Build M64-closeout eighth-slice runtime readiness decision.

This is a decision artifact only. It closes the M64 scaffold evidence and
selects the next queue without mutating recipe drafts, recording human
selection, creating runtime fixtures, publishing saved decks/UI lists, enabling
Lock/Legion runtime, or enabling bot/playbook use.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M64_01_SCAFFOLD = OUTPUT_DIR / "m64_01_eighth_slice_fixture_scaffold.json"
M64_02_PACKET = OUTPUT_DIR / "m64_02_eighth_slice_review_packet.json"
M64_03_DRAFTS = OUTPUT_DIR / "m64_03_eighth_slice_recipe_draft_model.json"
M64_04_VALIDATION = OUTPUT_DIR / "m64_04_eighth_slice_recipe_validation_report.json"
M64_05_CONSISTENCY = OUTPUT_DIR / "m64_05_eighth_slice_combo_recipe_consistency_report.json"
M64_06_REPAIRS = OUTPUT_DIR / "m64_06_eighth_slice_blocker_repair_candidates.json"


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
        "m64_01_eighth_slice_fixture_scaffold": str(M64_01_SCAFFOLD.relative_to(ROOT)),
        "m64_02_eighth_slice_review_packet": str(M64_02_PACKET.relative_to(ROOT)),
        "m64_03_eighth_slice_recipe_draft_model": str(M64_03_DRAFTS.relative_to(ROOT)),
        "m64_04_eighth_slice_recipe_validation_report": str(M64_04_VALIDATION.relative_to(ROOT)),
        "m64_05_eighth_slice_combo_recipe_consistency_report": str(M64_05_CONSISTENCY.relative_to(ROOT)),
        "m64_06_eighth_slice_blocker_repair_candidates": str(M64_06_REPAIRS.relative_to(ROOT)),
    }


def _artifact_checks() -> list[dict[str, Any]]:
    paths = [
        ("M64-01", "Eighth-slice fixture scaffold", M64_01_SCAFFOLD),
        ("M64-02", "Eighth-slice review packet", M64_02_PACKET),
        ("M64-03", "Eighth-slice recipe draft model", M64_03_DRAFTS),
        ("M64-04", "Eighth-slice recipe validator", M64_04_VALIDATION),
        ("M64-05", "Eighth-slice combo-to-recipe consistency", M64_05_CONSISTENCY),
        ("M64-06", "Eighth-slice blocker repair candidates", M64_06_REPAIRS),
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


def _next_queue_selection(human_selection_review_allowed: bool) -> dict[str, Any]:
    if not human_selection_review_allowed:
        return {
            "id": "M64-repair",
            "title": "Eighth-slice Repair Evidence Completion",
            "goal": "Repair missing M64 evidence before selecting a human/Lock/Legion decision queue.",
            "tasks": [],
        }
    return {
        "id": "M65",
        "title": "Eighth-slice Human Selection, Grade Repair, and Lock/Legion Decision Gate",
        "goal": (
            "Review M64-06 repair previews, select exactly one eighth-slice recipe, "
            "record explicit grade-profile repair acceptance or rejection, decide Lock/Unlock "
            "and Legion/Mate support scope, and rerun validation before any runtime fixture gate."
        ),
        "tasks": [
            {
                "id": "M65-01",
                "title": "Eighth-slice human repair review packet",
                "goal": "Export a concise review packet for M64-06 repair packages and candidate recipes.",
            },
            {
                "id": "M65-02",
                "title": "Eighth-slice human-selected recipe artifact",
                "goal": "Record exactly one selected eighth-slice recipe id without mutating M64 drafts.",
            },
            {
                "id": "M65-03",
                "title": "Eighth-slice human-accepted grade repair artifact",
                "goal": "Record explicit acceptance or rejection of the selected grade-profile repair package.",
            },
            {
                "id": "M65-04",
                "title": "Eighth-slice Lock and Legion decision artifact",
                "goal": "Record whether runtime promotion waits for Lock/Unlock and Legion/Mate rules support.",
            },
            {
                "id": "M65-05",
                "title": "Eighth-slice repaired recipe validation rerun",
                "goal": (
                    "Apply accepted repair in memory and rerun count, trigger, grade, copy-limit, clan, "
                    "human-selection, Lock, and Legion validation."
                ),
            },
            {
                "id": "M65-06",
                "title": "Eighth-slice runtime fixture promotion gate",
                "goal": "Promote only if repaired validation, consistency, human selection, and system decisions all pass.",
            },
            {
                "id": "M65-closeout",
                "title": "Eighth-slice fixture closeout",
                "goal": "Decide whether the eighth slice enters offline fixture scope or remains advisory.",
            },
        ],
    }


def build_eighth_slice_runtime_readiness_closeout(
    scaffold_report: dict[str, Any] | None = None,
    packet_report: dict[str, Any] | None = None,
    draft_report: dict[str, Any] | None = None,
    validation_report: dict[str, Any] | None = None,
    consistency_report: dict[str, Any] | None = None,
    repair_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    in_memory_reports = all(
        report is not None
        for report in (
            scaffold_report,
            packet_report,
            draft_report,
            validation_report,
            consistency_report,
            repair_report,
        )
    )
    scaffold_report = scaffold_report or load_json(M64_01_SCAFFOLD)
    packet_report = packet_report or load_json(M64_02_PACKET)
    draft_report = draft_report or load_json(M64_03_DRAFTS)
    validation_report = validation_report or load_json(M64_04_VALIDATION)
    consistency_report = consistency_report or load_json(M64_05_CONSISTENCY)
    repair_report = repair_report or load_json(M64_06_REPAIRS)

    artifact_checks = _artifact_checks()
    validation_summary = validation_report.get("summary", {})
    consistency_summary = consistency_report.get("summary", {})
    repair_summary = repair_report.get("summary", {})
    issue_counts = validation_summary.get("issue_counts", {})
    m64_scaffold_complete = (
        bool(scaffold_report.get("summary", {}).get("ready_for_m64_02"))
        and bool(packet_report.get("summary", {}).get("ready_for_m64_03"))
        and bool(draft_report.get("summary", {}).get("ready_for_m64_04"))
        and bool(validation_summary.get("ready_for_m64_05"))
        and bool(consistency_summary.get("ready_for_m64_06"))
        and bool(repair_summary.get("ready_for_m64_closeout"))
    )
    real_artifacts_available = False if in_memory_reports else all(item["exists"] for item in artifact_checks)
    runtime_ready_count = int(validation_summary.get("runtime_ready_recipe_count", 0))
    promotion_allowed_count = int(consistency_summary.get("promotion_allowed_count", 0))
    manual_review_overlap_count = int(validation_summary.get("manual_review_overlap_recipe_count", 0))
    grade_profile_review_count = int(validation_summary.get("grade_profile_review_recipe_count", 0))
    lock_deferred_count = int(validation_summary.get("lock_deferred_recipe_count", 0))
    legion_deferred_count = int(validation_summary.get("legion_deferred_recipe_count", 0))
    human_selection_pending_count = int(issue_counts.get("human_recipe_selection_pending", 0))
    human_repair_ready_count = int(repair_summary.get("ready_for_human_repair_review_count", 0))
    runtime_ready = (
        m64_scaffold_complete
        and runtime_ready_count > 0
        and promotion_allowed_count > 0
        and manual_review_overlap_count == 0
        and grade_profile_review_count == 0
        and lock_deferred_count == 0
        and legion_deferred_count == 0
        and human_selection_pending_count == 0
    )
    human_selection_review_allowed = m64_scaffold_complete and not runtime_ready and human_repair_ready_count > 0

    blockers: list[str] = []
    if runtime_ready_count == 0:
        blockers.append("no_runtime_ready_recipe")
    if promotion_allowed_count == 0:
        blockers.append("no_promotion_allowed_consistency_check")
    if manual_review_overlap_count > 0:
        blockers.append("manual_review_overlap_requires_acceptance")
    if grade_profile_review_count > 0:
        blockers.append("grade_profile_review_requires_acceptance")
    if lock_deferred_count > 0:
        blockers.append("lock_runtime_support_deferred")
    if legion_deferred_count > 0:
        blockers.append("legion_runtime_support_deferred")
    if human_selection_pending_count > 0:
        blockers.append("human_recipe_selection_pending")
    if human_repair_ready_count > 0:
        blockers.append("repair_candidates_require_human_selection")
    blockers.append("runtime_fixture_gate_not_run")

    next_queue = _next_queue_selection(human_selection_review_allowed)
    return {
        "version": "M64-closeout",
        "description": "Eighth-slice runtime readiness decision",
        "source_inputs": _source_inputs(),
        "input_mode": "in_memory_reports" if in_memory_reports else "file_artifacts",
        "scope": {
            "closeout_report": True,
            "changes_recipe_draft_file": False,
            "records_human_selection": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "lock_runtime": False,
            "legion_runtime": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
        },
        "artifact_checks": artifact_checks,
        "key_results": {
            "selected_target": repair_report.get("selected_target", {}),
            "fixture_scaffold_ready": scaffold_report.get("summary", {}).get("ready_for_m64_02"),
            "review_items": packet_report.get("summary", {}).get("total_review_item_count"),
            "recipe_drafts": draft_report.get("summary", {}).get("recipe_draft_count"),
            "runtime_ready_recipe_count": runtime_ready_count,
            "promotion_allowed_count": promotion_allowed_count,
            "blocked_by_manual_review_count": validation_summary.get("blocked_by_manual_review_count"),
            "manual_review_overlap_recipe_count": manual_review_overlap_count,
            "grade_profile_review_recipe_count": grade_profile_review_count,
            "lock_deferred_recipe_count": lock_deferred_count,
            "legion_deferred_recipe_count": legion_deferred_count,
            "repair_candidates_ready_for_human_review": human_repair_ready_count,
            "human_selection_candidate_count": repair_summary.get("human_selection_candidate_count"),
            "grade_profile_complete_candidate_count": repair_summary.get("grade_profile_complete_candidate_count"),
            "unexpected_structural_blocker_recipe_count": repair_summary.get(
                "unexpected_structural_blocker_recipe_count"
            ),
        },
        "decision": {
            "m64_scaffold_complete": m64_scaffold_complete,
            "real_artifacts_available": real_artifacts_available,
            "eighth_slice_runtime_ready_recipe_available": runtime_ready,
            "eighth_slice_can_enter_runtime_fixture_gate_now": runtime_ready,
            "eighth_slice_remains_advisory": not runtime_ready,
            "human_selection_review_allowed": human_selection_review_allowed,
            "grade_profile_acceptance_required_before_promotion": grade_profile_review_count > 0,
            "lock_runtime_support_required_before_promotion": lock_deferred_count > 0,
            "legion_runtime_support_required_before_promotion": legion_deferred_count > 0,
            "live_runtime_deck_enabled": False,
            "saved_deck_or_ui_publication_allowed": False,
            "bot_playbook_enabled": False,
            "decision_blockers": blockers,
            "recommendation": (
                "open_m65_eighth_slice_human_selection_grade_repair_and_lock_legion_decision_gate"
                if human_selection_review_allowed
                else "repair_m64_evidence_before_runtime_decision"
            ),
        },
        "next_queue": next_queue,
        "summary": {
            "m64_scaffold_complete": m64_scaffold_complete,
            "real_artifacts_available": real_artifacts_available,
            "runtime_ready_recipe_available": runtime_ready,
            "human_selection_review_allowed": human_selection_review_allowed,
            "next_queue_id": next_queue["id"],
            "ready_for_next_queue": human_selection_review_allowed,
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
        "# M64 Eighth-Slice Runtime Readiness Closeout",
        "",
        "## Summary",
        "",
        f"- M64 scaffold complete: `{summary['m64_scaffold_complete']}`",
        f"- Real artifacts available: `{summary['real_artifacts_available']}`",
        f"- Runtime-ready recipe available: `{summary['runtime_ready_recipe_available']}`",
        f"- Human selection review allowed: `{summary['human_selection_review_allowed']}`",
        f"- Next queue: `{summary['next_queue_id']}`",
        f"- Ready for next queue: `{summary['ready_for_next_queue']}`",
        "",
        "## Key Results",
        "",
        f"- Fixture scaffold ready: `{results['fixture_scaffold_ready']}`",
        f"- Review items: `{results['review_items']}`",
        f"- Recipe drafts: `{results['recipe_drafts']}`",
        f"- Runtime-ready recipes: `{results['runtime_ready_recipe_count']}`",
        f"- Promotion-allowed checks: `{results['promotion_allowed_count']}`",
        f"- Blocked by manual review: `{results['blocked_by_manual_review_count']}`",
        f"- Manual-review overlap recipes: `{results['manual_review_overlap_recipe_count']}`",
        f"- Grade-profile review recipes: `{results['grade_profile_review_recipe_count']}`",
        f"- Lock deferred recipes: `{results['lock_deferred_recipe_count']}`",
        f"- Legion deferred recipes: `{results['legion_deferred_recipe_count']}`",
        f"- Repair candidates ready for human review: `{results['repair_candidates_ready_for_human_review']}`",
        f"- Human selection candidates: `{results['human_selection_candidate_count']}`",
        f"- Grade-profile complete candidates: `{results['grade_profile_complete_candidate_count']}`",
        f"- Unexpected structural blockers: `{results['unexpected_structural_blocker_recipe_count']}`",
        "",
        "## Decision",
        "",
        f"- Runtime fixture gate now: `{decision['eighth_slice_can_enter_runtime_fixture_gate_now']}`",
        f"- Eighth slice remains advisory: `{decision['eighth_slice_remains_advisory']}`",
        f"- Grade-profile acceptance required before promotion: `{decision['grade_profile_acceptance_required_before_promotion']}`",
        f"- Lock runtime support required before promotion: `{decision['lock_runtime_support_required_before_promotion']}`",
        f"- Legion runtime support required before promotion: `{decision['legion_runtime_support_required_before_promotion']}`",
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
            "- Closeout does not record human selection.",
            "- Closeout does not inject saved decks or publish UI deck lists.",
            "- Closeout does not create runtime fixtures, enable Lock/Legion runtime, or enable bot playbooks.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M64 eighth-slice runtime readiness closeout.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_eighth_slice_runtime_readiness_closeout()
    json_path = args.output_dir / "m64_closeout_eighth_slice_runtime_readiness.json"
    md_path = args.output_dir / "m64_closeout_eighth_slice_runtime_readiness.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M64 closeout eighth-slice runtime readiness wrote {json_path}")
    print(f"M64 closeout eighth-slice runtime readiness summary wrote {md_path}")
    print(
        "m64_scaffold_complete={complete} runtime_ready={runtime_ready} next_queue={queue}".format(
            complete=report["summary"]["m64_scaffold_complete"],
            runtime_ready=report["summary"]["runtime_ready_recipe_available"],
            queue=report["summary"]["next_queue_id"],
        )
    )
    return 0 if report["summary"]["ready_for_next_queue"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
