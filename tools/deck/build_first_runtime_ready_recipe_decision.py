"""Build the M37 closeout first runtime-ready recipe decision report."""

from __future__ import annotations

import argparse
import json
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


@dataclass(frozen=True)
class MilestoneInput:
    milestone: str
    title: str
    filename: str

    @property
    def path(self) -> Path:
        return OUTPUT_DIR / self.filename


MILESTONE_INPUTS: tuple[MilestoneInput, ...] = (
    MilestoneInput("M37-01", "Accepted seed slot-gap completion candidates", "m37_01_accepted_seed_slot_gap_candidates.json"),
    MilestoneInput("M37-02", "Trigger package repair proposal", "m37_02_trigger_package_repair_proposal.json"),
    MilestoneInput("M37-03", "Rejected-line support-gap triage", "m37_03_rejected_line_support_gap_triage.json"),
    MilestoneInput("M37-04", "Manual semantic mapping candidates", "m37_04_manual_semantic_mapping_candidates.json"),
    MilestoneInput("M37-05", "Revised recipe validation rerun", "m37_05_revised_recipe_validation_rerun.json"),
)


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _load_inputs() -> dict[str, dict[str, Any]]:
    return {item.milestone: load_json(item.path) for item in MILESTONE_INPUTS}


def _input_checks() -> list[dict[str, Any]]:
    return [
        {
            "milestone": item.milestone,
            "title": item.title,
            "path": str(item.path.relative_to(ROOT)),
            "exists": item.path.exists(),
        }
        for item in MILESTONE_INPUTS
    ]


def _key_results(reports: dict[str, dict[str, Any]]) -> dict[str, Any]:
    m37_01 = reports["M37-01"]["summary"]
    m37_02 = reports["M37-02"]["summary"]
    m37_03 = reports["M37-03"]["summary"]
    m37_04 = reports["M37-04"]["summary"]
    m37_05 = reports["M37-05"]
    after = m37_05["accepted_seed_after"]
    return {
        "slot_gap_candidates": {
            "trigger_candidate_card_count": m37_01["trigger_candidate_card_count"],
            "complete_package_count": m37_01["complete_package_count"],
        },
        "trigger_repair": {
            "recommended_package_id": m37_02["recommended_package_id"],
            "recommended_profile_id": m37_02["recommended_profile_id"],
            "resolved_blockers": m37_02["resolved_blockers"],
            "remaining_review_issue_count": m37_02["remaining_review_issue_count"],
        },
        "support_gap_triage": {
            "rejected_line_count": m37_03["rejected_line_count"],
            "gap_group_count": m37_03["gap_group_count"],
            "manual_mapping_backlog_count": m37_03["manual_mapping_backlog_count"],
        },
        "manual_mapping_candidates": {
            "mapping_candidate_count": m37_04["mapping_candidate_count"],
            "line_mapping_link_count": m37_04["line_mapping_link_count"],
            "executable_schema_change": False,
        },
        "revised_validation": {
            "recipe_id": m37_05["summary"]["recipe_id"],
            "validation_status_after": after["validation_status"],
            "consistency_status_after": after["consistency_status"],
            "remaining_blocker_count": m37_05["summary"]["remaining_blocker_count"],
            "remaining_review_issue_count": m37_05["summary"]["remaining_review_issue_count"],
            "runtime_ready": after["runtime_ready"],
            "promotion_allowed": after["promotion_allowed"],
            "trigger_counts": after["count_summary"]["trigger_counts"],
            "grade_counts": after["count_summary"]["grade_counts"],
            "review_codes": after["review_codes"],
        },
    }


def _next_queue_selection() -> dict[str, Any]:
    return {
        "milestone": "M38",
        "name": "Human acceptance and grade-profile repair gate",
        "why_this_next": [
            "M37 cleared accepted-seed trigger/deck-size blockers in memory.",
            "The recipe is still not runtime-ready because human acceptance and grade-profile review remain.",
            "A runtime fixture should not be created until those review gates are explicit artifacts.",
        ],
        "first_tasks": [
            {
                "id": "M38-01",
                "title": "Accepted seed human review packet",
                "goal": "Export a concise review packet for recipe_003 and the recommended trigger repair.",
            },
            {
                "id": "M38-02",
                "title": "Grade profile repair candidates",
                "goal": "Propose reviewable grade-profile adjustments without mutating runtime decks.",
            },
            {
                "id": "M38-03",
                "title": "Human-accepted recipe artifact",
                "goal": "Record explicit acceptance or rejection of the repaired recipe.",
            },
            {
                "id": "M38-04",
                "title": "Runtime fixture promotion gate",
                "goal": "Promote only if validation, consistency, grade review, and human acceptance all pass.",
            },
            {
                "id": "M38-closeout",
                "title": "First runtime fixture closeout",
                "goal": "Decide whether the first recipe enters runtime/test-fixture scope or remains advisory.",
            },
        ],
        "hard_gates": [
            "no runtime deck promotion without explicit human acceptance",
            "no runtime deck promotion while grade_profile_review remains open",
            "no bot/playbook promotion until runtime fixture gate passes",
            "no automatic mutation of m36 recipe draft artifacts",
            "no live card text parsing",
        ],
    }


def build_decision_report(reports: dict[str, dict[str, Any]] | None = None) -> dict[str, Any]:
    reports = reports or _load_inputs()
    checks = _input_checks()
    results = _key_results(reports)
    revised = results["revised_validation"]
    human_acceptance_pending = "human_acceptance_pending" in revised["review_codes"]
    grade_profile_pending = "grade_profile_review" in revised["review_codes"]
    runtime_ready = (
        revised["validation_status_after"] == "validator_passed"
        and revised["consistency_status_after"] == "consistent_validator_passed"
        and revised["runtime_ready"]
        and revised["promotion_allowed"]
        and not human_acceptance_pending
        and not grade_profile_pending
    )
    blockers = []
    if human_acceptance_pending:
        blockers.append("human_acceptance_pending")
    if grade_profile_pending:
        blockers.append("grade_profile_review")
    if not revised["promotion_allowed"]:
        blockers.append("promotion_not_allowed")

    return {
        "version": "M37-closeout",
        "description": "First runtime-ready recipe decision after M37 repair pipeline",
        "scope": {
            "offline_decision_artifact": True,
            "changes_recipe_draft_file": False,
            "creates_runtime_deck": False,
            "bot_integration": False,
            "automatic_playbook_promotion": False,
        },
        "input_checks": checks,
        "source_inputs": {item.milestone: str(item.path.relative_to(ROOT)) for item in MILESTONE_INPUTS},
        "key_results": results,
        "decision": {
            "m37_complete": all(item["exists"] for item in checks),
            "first_runtime_ready_recipe_available": runtime_ready,
            "accepted_seed_can_be_runtime_fixture": runtime_ready,
            "accepted_seed_remains_advisory": not runtime_ready,
            "decision_blockers": blockers,
            "recommendation": "keep_recipe_003_advisory_until_human_acceptance_and_grade_review_clear",
        },
        "next_queue_selection": _next_queue_selection(),
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    decision = report["decision"]
    revised = report["key_results"]["revised_validation"]
    next_queue = report["next_queue_selection"]
    lines = [
        "# M37 First Runtime-Ready Recipe Decision",
        "",
        "## Summary",
        "",
        f"- M37 complete: `{decision['m37_complete']}`",
        f"- First runtime-ready recipe available: `{decision['first_runtime_ready_recipe_available']}`",
        f"- Accepted seed can be runtime fixture: `{decision['accepted_seed_can_be_runtime_fixture']}`",
        f"- Accepted seed remains advisory: `{decision['accepted_seed_remains_advisory']}`",
        f"- Decision blockers: `{decision['decision_blockers']}`",
        f"- Recommendation: `{decision['recommendation']}`",
        "",
        "## Accepted Seed Status",
        "",
        f"- Recipe: `{revised['recipe_id']}`",
        f"- Validation status: `{revised['validation_status_after']}`",
        f"- Consistency status: `{revised['consistency_status_after']}`",
        f"- Trigger counts: `{revised['trigger_counts']}`",
        f"- Grade counts: `{revised['grade_counts']}`",
        f"- Review codes: `{revised['review_codes']}`",
        "",
        "## Next Queue",
        "",
        f"`{next_queue['milestone']}`: {next_queue['name']}",
        "",
        "First tasks:",
        "",
    ]
    for task in next_queue["first_tasks"]:
        lines.append(f"- `{task['id']}`: {task['title']}")
    lines.extend(["", "Hard gates:", ""])
    for gate in next_queue["hard_gates"]:
        lines.append(f"- {gate}")
    lines.append("")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M37 closeout runtime-ready recipe decision.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_decision_report()
    json_path = args.output_dir / "m37_closeout_first_runtime_ready_recipe_decision.json"
    md_path = args.output_dir / "m37_closeout_first_runtime_ready_recipe_decision.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M37 closeout decision wrote {json_path}")
    print(f"M37 closeout decision summary wrote {md_path}")
    print(
        "m37_complete={complete} runtime_ready={ready} next_queue={queue}".format(
            complete=report["decision"]["m37_complete"],
            ready=report["decision"]["first_runtime_ready_recipe_available"],
            queue=report["next_queue_selection"]["milestone"],
        )
    )
    return 0 if report["decision"]["m37_complete"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
