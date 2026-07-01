"""Build the M36 deck recipe validation closeout report.

The closeout is an offline coordination artifact. It summarizes the M36 recipe
draft, validation, consistency, and second-slice comparison outputs, then
selects the next blocker-focused queue without promoting any runtime deck or
bot behavior.
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
    title: str
    filename: str

    @property
    def path(self) -> Path:
        return OUTPUT_DIR / self.filename


MILESTONE_INPUTS: tuple[MilestoneInput, ...] = (
    MilestoneInput("M36-01", "First-slice review packet", "m36_01_first_slice_review_packet.json"),
    MilestoneInput("M36-02", "Deck recipe draft model", "m36_02_deck_recipe_draft_model.json"),
    MilestoneInput("M36-03", "Deck recipe validator", "m36_03_deck_recipe_validation_report.json"),
    MilestoneInput("M36-04", "Combo-line to recipe consistency check", "m36_04_combo_recipe_consistency_report.json"),
    MilestoneInput("M36-05", "Second-slice readiness comparison", "m36_05_second_slice_readiness_comparison.json"),
)


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
            "title": item.title,
            "path": str(item.path.relative_to(ROOT)),
            "exists": item.path.exists(),
        }
        for item in MILESTONE_INPUTS
    ]


def _task_summaries(reports: dict[str, dict[str, Any]]) -> list[dict[str, Any]]:
    summaries: list[dict[str, Any]] = []
    for item in MILESTONE_INPUTS:
        report = reports[item.milestone]
        summaries.append(
            {
                "milestone": item.milestone,
                "title": item.title,
                "version": report.get("version"),
                "summary": report.get("summary", {}),
                "readiness": report.get("readiness", {}),
                "next_target": report.get("next_target", {}),
            }
        )
    return summaries


def _key_results(reports: dict[str, dict[str, Any]]) -> dict[str, Any]:
    review = reports["M36-01"]["summary"]
    drafts = reports["M36-02"]["summary"]
    validation = reports["M36-03"]["summary"]
    consistency = reports["M36-04"]["summary"]
    second = reports["M36-05"]

    return {
        "review_packet": {
            "accepted_seed_item_count": int(review.get("accepted_seed_item_count", 0)),
            "rejected_line_item_count": int(review.get("rejected_line_item_count", 0)),
            "manual_card_item_count": int(review.get("manual_card_item_count", 0)),
            "total_review_item_count": int(review.get("total_review_item_count", 0)),
        },
        "recipe_drafts": {
            "recipe_draft_count": int(drafts.get("recipe_draft_count", 0)),
            "accepted_seed_recipe_count": int(drafts.get("accepted_seed_recipe_count", 0)),
            "rejected_line_recipe_count": int(drafts.get("rejected_line_recipe_count", 0)),
            "manual_overlap_recipe_count": int(drafts.get("manual_overlap_recipe_count", 0)),
            "recipes_with_slot_gaps": int(drafts.get("recipes_with_slot_gaps", 0)),
        },
        "validation": {
            "recipe_count": int(validation.get("recipe_count", 0)),
            "runtime_ready_recipe_count": int(validation.get("runtime_ready_recipe_count", 0)),
            "validator_passed_count": int(validation.get("validator_passed_count", 0)),
            "invalid_draft_count": int(validation.get("invalid_draft_count", 0)),
            "blocked_by_review_count": int(validation.get("blocked_by_review_count", 0)),
            "missing_card_recipe_count": int(validation.get("missing_card_recipe_count", 0)),
            "copy_limit_violation_recipe_count": int(validation.get("copy_limit_violation_recipe_count", 0)),
            "slot_gap_recipe_count": int(validation.get("slot_gap_recipe_count", 0)),
            "trigger_count_mismatch_recipe_count": int(
                validation.get("trigger_count_mismatch_recipe_count", 0)
            ),
            "issue_counts": validation.get("issue_counts", {}),
        },
        "consistency": {
            "combo_line_count": int(consistency.get("combo_line_count", 0)),
            "combo_cards_present_count": int(consistency.get("combo_cards_present_count", 0)),
            "missing_combo_card_check_count": int(consistency.get("missing_combo_card_check_count", 0)),
            "manual_review_dependency_check_count": int(
                consistency.get("manual_review_dependency_check_count", 0)
            ),
            "promotion_allowed_count": int(consistency.get("promotion_allowed_count", 0)),
            "runtime_ready_consistent_count": int(consistency.get("runtime_ready_consistent_count", 0)),
            "status_counts": consistency.get("status_counts", {}),
        },
        "second_slice": {
            "future_recipe_pipeline_ready": bool(
                _get(second, ["readiness", "second_slice_ready_for_future_recipe_pipeline"], False)
            ),
            "broader_scaleout_runtime_allowed": bool(
                _get(second, ["readiness", "broader_scaleout_runtime_allowed"], True)
            ),
            "recommendation": _get(second, ["comparison", "recommendation"], ""),
            "probe_candidate_edges": int(_get(second, ["second_slice_status", "probe_candidate_edges"], 0)),
        },
    }


def _blocker_summary(results: dict[str, Any]) -> dict[str, Any]:
    validation = results["validation"]
    consistency = results["consistency"]
    blockers: list[dict[str, Any]] = []

    if validation["blocked_by_review_count"]:
        blockers.append(
            {
                "id": "human_review_blocked_lines",
                "count": validation["blocked_by_review_count"],
                "next_action": "Triage rejected combo lines by unsupported semantic gap before re-drafting.",
            }
        )
    if validation["slot_gap_recipe_count"]:
        blockers.append(
            {
                "id": "unfilled_recipe_slots",
                "count": validation["slot_gap_recipe_count"],
                "next_action": "Generate source-backed slot-fill candidates, starting with the accepted seed recipe.",
            }
        )
    if validation["trigger_count_mismatch_recipe_count"]:
        blockers.append(
            {
                "id": "trigger_count_mismatch",
                "count": validation["trigger_count_mismatch_recipe_count"],
                "next_action": "Repair trigger package candidates before any recipe can be runtime-ready.",
            }
        )
    if validation["invalid_draft_count"]:
        blockers.append(
            {
                "id": "invalid_draft",
                "count": validation["invalid_draft_count"],
                "next_action": "Keep invalid drafts advisory until validator pass and human acceptance are both present.",
            }
        )
    if not consistency["promotion_allowed_count"]:
        blockers.append(
            {
                "id": "no_promotable_combo_line",
                "count": 1,
                "next_action": "Do not export runtime playbooks until a validated recipe contains a promotable line.",
            }
        )

    return {
        "blocker_count": len(blockers),
        "blockers": blockers,
        "runtime_promotion_blocked": True,
    }


def _next_queue_selection() -> dict[str, Any]:
    return {
        "milestone": "M37",
        "name": "First-slice blocker resolution and recipe repair",
        "why_this_next": [
            "M36 found zero runtime-ready recipes and zero promotable combo lines.",
            "The accepted seed is the safest repair target because it has static review acceptance but still has slot and trigger blockers.",
            "Rejected lines need support-gap triage before broader slice expansion or bot/runtime use.",
        ],
        "first_tasks": [
            {
                "id": "M37-01",
                "title": "Accepted seed slot-gap completion candidates",
                "goal": "Suggest source-backed candidates for missing recipe slots without auto-promoting them.",
            },
            {
                "id": "M37-02",
                "title": "Trigger package repair proposal",
                "goal": "Repair trigger-count mismatch candidates for the accepted seed recipe.",
            },
            {
                "id": "M37-03",
                "title": "Rejected-line support-gap triage",
                "goal": "Group blocked combo lines by unsupported semantic or review reason.",
            },
            {
                "id": "M37-04",
                "title": "Manual semantic mapping candidates",
                "goal": "Create reviewable mappings for unsupported effects such as bounce-to-hand style interactions.",
            },
            {
                "id": "M37-05",
                "title": "Revised recipe validation rerun",
                "goal": "Re-run recipe validator and consistency checks after accepted repairs are documented.",
            },
            {
                "id": "M37-closeout",
                "title": "First runtime-ready recipe decision",
                "goal": "Decide whether a recipe can become a runtime/test fixture or remains advisory only.",
            },
        ],
        "hard_gates": [
            "no runtime deck promotion until validator_passed and human_acceptance are both true",
            "no bot/playbook promotion until combo consistency promotion_allowed is true",
            "no automatic fill from raw card text without reviewable source evidence",
            "no direct GameState mutation",
            "no hidden-state or private deck leakage",
        ],
    }


def build_closeout(reports: dict[str, dict[str, Any]] | None = None) -> dict[str, Any]:
    reports = reports or _load_inputs()
    checks = _input_checks()
    results = _key_results(reports)
    blockers = _blocker_summary(results)
    all_inputs_present = all(item["exists"] for item in checks)
    m36_outputs_ready = all(
        bool(summary.get("summary", {}).get("ready_for_m36_02", True))
        and bool(summary.get("summary", {}).get("ready_for_m36_03", True))
        and bool(summary.get("summary", {}).get("ready_for_m36_04", True))
        and bool(summary.get("summary", {}).get("ready_for_m36_05", True))
        for summary in _task_summaries(reports)
    ) and bool(_get(reports["M36-05"], ["readiness", "ready_for_m36_closeout"], False))
    runtime_promotion_allowed = (
        results["validation"]["runtime_ready_recipe_count"] > 0
        and results["consistency"]["promotion_allowed_count"] > 0
        and not results["second_slice"]["broader_scaleout_runtime_allowed"]
    )

    return {
        "version": "M36-closeout",
        "description": "Deck recipe validation closeout and blocker-focused next queue selection",
        "scope": {
            "offline_coordination_artifact": True,
            "changes_runtime_gameplay": False,
            "changes_unity_ui": False,
            "creates_runtime_deck": False,
            "enables_bot_runtime": False,
        },
        "source_inputs": {item.milestone: str(item.path.relative_to(ROOT)) for item in MILESTONE_INPUTS},
        "input_checks": checks,
        "task_summaries": _task_summaries(reports),
        "key_results": results,
        "blocker_summary": blockers,
        "decision": {
            "m36_deck_recipe_validation_closed": all_inputs_present and m36_outputs_ready,
            "runtime_recipe_promotion_allowed": runtime_promotion_allowed,
            "broader_slice_scaleout_allowed": False,
            "recommended_next_milestone": "M37",
            "recommendation": "repair_first_slice_recipe_blockers_before_runtime_or_broader_scaleout",
        },
        "next_queue_selection": _next_queue_selection(),
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    results = report["key_results"]
    review = results["review_packet"]
    drafts = results["recipe_drafts"]
    validation = results["validation"]
    consistency = results["consistency"]
    second = results["second_slice"]
    decision = report["decision"]
    next_queue = report["next_queue_selection"]
    lines = [
        "# M36 Deck Recipe Validation Closeout",
        "",
        "## Summary",
        "",
        f"- M36 closed: `{decision['m36_deck_recipe_validation_closed']}`",
        f"- Runtime recipe promotion allowed: `{decision['runtime_recipe_promotion_allowed']}`",
        f"- Broader slice scale-out allowed: `{decision['broader_slice_scaleout_allowed']}`",
        f"- Recommendation: `{decision['recommendation']}`",
        "",
        "## M36 Results",
        "",
        f"- Review items: `{review['total_review_item_count']}` "
        f"(`{review['accepted_seed_item_count']}` accepted seed, "
        f"`{review['rejected_line_item_count']}` rejected lines, "
        f"`{review['manual_card_item_count']}` manual cards)",
        f"- Recipe drafts: `{drafts['recipe_draft_count']}`",
        f"- Accepted seed recipes: `{drafts['accepted_seed_recipe_count']}`",
        f"- Runtime-ready recipes: `{validation['runtime_ready_recipe_count']}`",
        f"- Invalid drafts: `{validation['invalid_draft_count']}`",
        f"- Blocked-by-review recipes: `{validation['blocked_by_review_count']}`",
        f"- Missing-card recipes: `{validation['missing_card_recipe_count']}`",
        f"- Copy-limit violations: `{validation['copy_limit_violation_recipe_count']}`",
        f"- Slot-gap recipes: `{validation['slot_gap_recipe_count']}`",
        f"- Trigger-count mismatch recipes: `{validation['trigger_count_mismatch_recipe_count']}`",
        f"- Combo cards present: `{consistency['combo_cards_present_count']}`",
        f"- Promotable combo lines: `{consistency['promotion_allowed_count']}`",
        f"- Second slice future recipe-ready: `{second['future_recipe_pipeline_ready']}`",
        f"- Second slice probe candidate edges: `{second['probe_candidate_edges']}`",
        "",
        "## Blockers",
        "",
    ]
    for blocker in report["blocker_summary"]["blockers"]:
        lines.append(f"- `{blocker['id']}`: `{blocker['count']}` - {blocker['next_action']}")
    lines.extend(
        [
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
    parser = argparse.ArgumentParser(description="Build M36 deck recipe validation closeout report.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_closeout()
    json_path = args.output_dir / "m36_closeout_deck_recipe_validation.json"
    md_path = args.output_dir / "m36_closeout_deck_recipe_validation.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M36 closeout wrote {json_path}")
    print(f"M36 closeout summary wrote {md_path}")
    print(
        "closed={closed} runtime_promotion_allowed={runtime} next_queue={queue}".format(
            closed=report["decision"]["m36_deck_recipe_validation_closed"],
            runtime=report["decision"]["runtime_recipe_promotion_allowed"],
            queue=report["next_queue_selection"]["milestone"],
        )
    )
    return 0 if report["decision"]["m36_deck_recipe_validation_closed"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
