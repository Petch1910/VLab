"""Build M60-closeout seventh-slice runtime readiness decision.

This is a decision artifact only. It closes the M60 scaffold evidence and
selects the next queue without mutating recipe drafts, recording human
acceptance, creating runtime fixtures, publishing saved decks/UI lists, enabling
G Zone/Stride runtime, or enabling bot/playbook use.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M60_01_SCAFFOLD = OUTPUT_DIR / "m60_01_seventh_slice_fixture_scaffold.json"
M60_02_PACKET = OUTPUT_DIR / "m60_02_seventh_slice_review_packet.json"
M60_03_DRAFTS = OUTPUT_DIR / "m60_03_seventh_slice_recipe_draft_model.json"
M60_04_VALIDATION = OUTPUT_DIR / "m60_04_seventh_slice_recipe_validation_report.json"
M60_05_CONSISTENCY = OUTPUT_DIR / "m60_05_seventh_slice_combo_recipe_consistency_report.json"
M60_06_REPAIRS = OUTPUT_DIR / "m60_06_seventh_slice_blocker_repair_candidates.json"


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
        "m60_01_seventh_slice_fixture_scaffold": str(M60_01_SCAFFOLD.relative_to(ROOT)),
        "m60_02_seventh_slice_review_packet": str(M60_02_PACKET.relative_to(ROOT)),
        "m60_03_seventh_slice_recipe_draft_model": str(M60_03_DRAFTS.relative_to(ROOT)),
        "m60_04_seventh_slice_recipe_validation_report": str(M60_04_VALIDATION.relative_to(ROOT)),
        "m60_05_seventh_slice_combo_recipe_consistency_report": str(M60_05_CONSISTENCY.relative_to(ROOT)),
        "m60_06_seventh_slice_blocker_repair_candidates": str(M60_06_REPAIRS.relative_to(ROOT)),
    }


def _artifact_checks() -> list[dict[str, Any]]:
    paths = [
        ("M60-01", "Seventh-slice fixture scaffold", M60_01_SCAFFOLD),
        ("M60-02", "Seventh-slice review packet", M60_02_PACKET),
        ("M60-03", "Seventh-slice recipe draft model", M60_03_DRAFTS),
        ("M60-04", "Seventh-slice recipe validator", M60_04_VALIDATION),
        ("M60-05", "Seventh-slice combo-to-recipe consistency", M60_05_CONSISTENCY),
        ("M60-06", "Seventh-slice blocker repair candidates", M60_06_REPAIRS),
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
            "id": "M60-repair",
            "title": "Seventh-slice Repair Evidence Completion",
            "goal": "Repair missing M60 evidence before selecting a human/G Zone/Bloom decision queue.",
            "tasks": [],
        }
    return {
        "id": "M61",
        "title": "Seventh-slice Human Selection, Repair, and G Zone/Bloom Decision Gate",
        "goal": (
            "Review M60-06 repair previews, select exactly one seventh-slice recipe, "
            "record explicit manual-card acceptance or replacement, decide G Zone/Stride "
            "and Bloom/token support scope, and rerun validation before any runtime fixture gate."
        ),
        "tasks": [
            {
                "id": "M61-01",
                "title": "Seventh-slice human repair review packet",
                "goal": "Export a concise review packet for M60-06 repair packages and candidate recipes.",
            },
            {
                "id": "M61-02",
                "title": "Seventh-slice human-selected recipe artifact",
                "goal": "Record exactly one selected seventh-slice recipe id without mutating M60 drafts.",
            },
            {
                "id": "M61-03",
                "title": "Seventh-slice human-accepted repair artifact",
                "goal": "Record explicit acceptance or rejection of selected manual/grade repair packages.",
            },
            {
                "id": "M61-04",
                "title": "Seventh-slice G Zone, Stride, and Bloom/token decision artifact",
                "goal": "Record whether runtime promotion waits for G Zone/Stride and Bloom/token rules support.",
            },
            {
                "id": "M61-05",
                "title": "Seventh-slice repaired recipe validation rerun",
                "goal": (
                    "Apply accepted repair in memory and rerun count, trigger, grade, copy-limit, clan, "
                    "manual-overlap, G Zone, and Bloom/token validation."
                ),
            },
            {
                "id": "M61-06",
                "title": "Seventh-slice runtime fixture promotion gate",
                "goal": "Promote only if repaired validation, consistency, human acceptance, and system decisions all pass.",
            },
            {
                "id": "M61-closeout",
                "title": "Seventh-slice fixture closeout",
                "goal": "Decide whether the seventh slice enters offline fixture scope or remains advisory.",
            },
        ],
    }


def build_seventh_slice_runtime_readiness_closeout(
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
    scaffold_report = scaffold_report or load_json(M60_01_SCAFFOLD)
    packet_report = packet_report or load_json(M60_02_PACKET)
    draft_report = draft_report or load_json(M60_03_DRAFTS)
    validation_report = validation_report or load_json(M60_04_VALIDATION)
    consistency_report = consistency_report or load_json(M60_05_CONSISTENCY)
    repair_report = repair_report or load_json(M60_06_REPAIRS)

    artifact_checks = _artifact_checks()
    validation_summary = validation_report.get("summary", {})
    consistency_summary = consistency_report.get("summary", {})
    repair_summary = repair_report.get("summary", {})
    issue_counts = validation_summary.get("issue_counts", {})
    m60_scaffold_complete = (
        bool(scaffold_report.get("summary", {}).get("ready_for_m60_02"))
        and bool(packet_report.get("summary", {}).get("ready_for_m60_03"))
        and bool(draft_report.get("summary", {}).get("ready_for_m60_04"))
        and bool(validation_summary.get("ready_for_m60_05"))
        and bool(consistency_summary.get("ready_for_m60_06"))
        and bool(repair_summary.get("ready_for_m60_closeout"))
    )
    real_artifacts_available = False if in_memory_reports else all(item["exists"] for item in artifact_checks)
    runtime_ready_count = int(validation_summary.get("runtime_ready_recipe_count", 0))
    promotion_allowed_count = int(consistency_summary.get("promotion_allowed_count", 0))
    manual_review_overlap_count = int(validation_summary.get("manual_review_overlap_recipe_count", 0))
    g_zone_deferred_count = int(validation_summary.get("g_zone_deferred_recipe_count", 0))
    bloom_token_deferred_count = int(validation_summary.get("bloom_token_deferred_recipe_count", 0))
    human_repair_ready_count = int(repair_summary.get("ready_for_human_repair_review_count", 0))
    runtime_ready = (
        m60_scaffold_complete
        and runtime_ready_count > 0
        and promotion_allowed_count > 0
        and manual_review_overlap_count == 0
        and g_zone_deferred_count == 0
        and bloom_token_deferred_count == 0
    )
    human_selection_review_allowed = m60_scaffold_complete and not runtime_ready and human_repair_ready_count > 0

    blockers: list[str] = []
    if runtime_ready_count == 0:
        blockers.append("no_runtime_ready_recipe")
    if promotion_allowed_count == 0:
        blockers.append("no_promotion_allowed_consistency_check")
    if manual_review_overlap_count > 0:
        blockers.append("manual_review_overlap_requires_acceptance")
    if int(validation_summary.get("grade_profile_review_recipe_count", 0)) > 0:
        blockers.append("grade_profile_review_requires_acceptance")
    if g_zone_deferred_count > 0:
        blockers.append("g_zone_support_deferred")
    if bloom_token_deferred_count > 0:
        blockers.append("bloom_token_support_deferred")
    if int(issue_counts.get("human_recipe_selection_pending", 0)) > 0:
        blockers.append("human_recipe_selection_pending")
    if human_repair_ready_count > 0:
        blockers.append("repair_candidates_require_human_acceptance")
    blockers.append("runtime_fixture_gate_not_run")

    next_queue = _next_queue_selection(human_selection_review_allowed)
    return {
        "version": "M60-closeout",
        "description": "Seventh-slice runtime readiness decision",
        "source_inputs": _source_inputs(),
        "input_mode": "in_memory_reports" if in_memory_reports else "file_artifacts",
        "scope": {
            "closeout_report": True,
            "changes_recipe_draft_file": False,
            "records_human_acceptance": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "g_zone_runtime": False,
            "stride_runtime": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
        },
        "artifact_checks": artifact_checks,
        "key_results": {
            "selected_target": repair_report.get("selected_target", {}),
            "fixture_scaffold_ready": scaffold_report.get("summary", {}).get("ready_for_m60_02"),
            "review_items": packet_report.get("summary", {}).get("total_review_item_count"),
            "recipe_drafts": draft_report.get("summary", {}).get("recipe_draft_count"),
            "runtime_ready_recipe_count": runtime_ready_count,
            "promotion_allowed_count": promotion_allowed_count,
            "blocked_by_manual_review_count": validation_summary.get("blocked_by_manual_review_count"),
            "manual_review_overlap_recipe_count": manual_review_overlap_count,
            "grade_profile_review_recipe_count": validation_summary.get("grade_profile_review_recipe_count"),
            "g_zone_deferred_recipe_count": g_zone_deferred_count,
            "bloom_token_deferred_recipe_count": bloom_token_deferred_count,
            "repair_candidates_ready_for_human_review": human_repair_ready_count,
            "manual_repair_complete_count": repair_summary.get("manual_repair_complete_count"),
            "grade_profile_complete_candidate_count": repair_summary.get("grade_profile_complete_candidate_count"),
            "unexpected_structural_blocker_recipe_count": repair_summary.get(
                "unexpected_structural_blocker_recipe_count"
            ),
        },
        "decision": {
            "m60_scaffold_complete": m60_scaffold_complete,
            "real_artifacts_available": real_artifacts_available,
            "seventh_slice_runtime_ready_recipe_available": runtime_ready,
            "seventh_slice_can_enter_runtime_fixture_gate_now": runtime_ready,
            "seventh_slice_remains_advisory": not runtime_ready,
            "human_selection_review_allowed": human_selection_review_allowed,
            "g_zone_runtime_support_required_before_promotion": g_zone_deferred_count > 0,
            "bloom_token_runtime_support_required_before_promotion": bloom_token_deferred_count > 0,
            "live_runtime_deck_enabled": False,
            "saved_deck_or_ui_publication_allowed": False,
            "bot_playbook_enabled": False,
            "decision_blockers": blockers,
            "recommendation": (
                "open_m61_seventh_slice_human_selection_repair_and_g_zone_bloom_decision_gate"
                if human_selection_review_allowed
                else "repair_m60_evidence_before_runtime_decision"
            ),
        },
        "next_queue": next_queue,
        "summary": {
            "m60_scaffold_complete": m60_scaffold_complete,
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
        "# M60 Seventh-Slice Runtime Readiness Closeout",
        "",
        "## Summary",
        "",
        f"- M60 scaffold complete: `{summary['m60_scaffold_complete']}`",
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
        f"- G Zone deferred recipes: `{results['g_zone_deferred_recipe_count']}`",
        f"- Bloom/token deferred recipes: `{results['bloom_token_deferred_recipe_count']}`",
        f"- Repair candidates ready for human review: `{results['repair_candidates_ready_for_human_review']}`",
        f"- Manual repair complete candidates: `{results['manual_repair_complete_count']}`",
        f"- Grade-profile complete candidates: `{results['grade_profile_complete_candidate_count']}`",
        f"- Unexpected structural blockers: `{results['unexpected_structural_blocker_recipe_count']}`",
        "",
        "## Decision",
        "",
        f"- Runtime fixture gate now: `{decision['seventh_slice_can_enter_runtime_fixture_gate_now']}`",
        f"- Seventh slice remains advisory: `{decision['seventh_slice_remains_advisory']}`",
        f"- G Zone runtime support required before promotion: `{decision['g_zone_runtime_support_required_before_promotion']}`",
        f"- Bloom/token runtime support required before promotion: `{decision['bloom_token_runtime_support_required_before_promotion']}`",
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
            "- Closeout does not create runtime fixtures, enable G Zone/Stride runtime, or enable bot playbooks.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M60 seventh-slice runtime readiness closeout.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_seventh_slice_runtime_readiness_closeout()
    json_path = args.output_dir / "m60_closeout_seventh_slice_runtime_readiness.json"
    md_path = args.output_dir / "m60_closeout_seventh_slice_runtime_readiness.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M60 closeout seventh-slice runtime readiness wrote {json_path}")
    print(f"M60 closeout seventh-slice runtime readiness summary wrote {md_path}")
    print(
        "m60_scaffold_complete={complete} runtime_ready={runtime_ready} next_queue={queue}".format(
            complete=report["summary"]["m60_scaffold_complete"],
            runtime_ready=report["summary"]["runtime_ready_recipe_available"],
            queue=report["summary"]["next_queue_id"],
        )
    )
    return 0 if report["summary"]["ready_for_next_queue"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
