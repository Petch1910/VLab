"""Build the M71-01 post-nine fixture queue planning artifact."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_four_fixture_scale_decision import load_json  # noqa: E402


OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M70_04_DECISION = OUTPUT_DIR / "m70_04_nine_fixture_scale_decision.json"
JSON_OUTPUT = OUTPUT_DIR / "m71_01_post_nine_fixture_queue_plan.json"
MD_OUTPUT = OUTPUT_DIR / "m71_01_post_nine_fixture_queue_plan.md"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


RUNTIME_BOUNDARY_FLAGS = [
    "selects_tenth_slice",
    "creates_runtime_fixture",
    "materializes_real_artifacts",
    "saved_deck_injection",
    "ui_deck_list_publication",
    "bot_playbook",
    "g_zone_runtime_enabled",
    "stride_runtime_enabled",
    "aqua_force_battle_order_runtime_enabled",
    "bloom_token_runtime_enabled",
    "lock_runtime_enabled",
    "unlock_runtime_enabled",
    "legion_runtime_enabled",
    "mate_identity_check_enabled",
    "live_card_text_parsing",
    "GameState_mutation",
]


def _m70_ready(scale_decision: dict[str, Any]) -> bool:
    summary = scale_decision.get("summary", {})
    decision = scale_decision.get("decision", {})
    return (
        scale_decision.get("version") == "M70-04"
        and summary.get("fixture_evidence_count") == 9
        and summary.get("passed_fixture_count") == 9
        and summary.get("failed_fixture_count") == 0
        and bool(summary.get("post_m70_queue_review_ready"))
        and bool(summary.get("ready_for_m71_planning"))
        and bool(decision.get("post_m70_queue_review_ready"))
    )


def _input_summary(scale_decision: dict[str, Any], ready: bool) -> dict[str, Any]:
    summary = scale_decision.get("summary", {})
    decision = scale_decision.get("decision", {})
    fixture_evidence = scale_decision.get("fixture_evidence", [])
    return {
        "m70_version": scale_decision.get("version"),
        "m70_ready_for_m71": ready,
        "fixture_evidence_count": int(summary.get("fixture_evidence_count", 0) or 0),
        "passed_fixture_count": int(summary.get("passed_fixture_count", 0) or 0),
        "failed_fixture_count": int(summary.get("failed_fixture_count", 0) or 0),
        "candidate_count": int(summary.get("candidate_count", 0) or 0),
        "m70_recommended_next_action": decision.get("recommended_next_action", ""),
        "fixture_labels": [item.get("label", "") for item in fixture_evidence],
    }


def _queue_options(ready: bool) -> list[dict[str, Any]]:
    if ready:
        return [
            {
                "id": "gated_real_artifact_materialization_audit",
                "milestone": "M72-01",
                "priority": 1,
                "status": "recommended",
                "enabled_now": True,
                "reason": (
                    "Nine fixture scaffolds pass, but many real CLI artifacts remain gated; "
                    "audit materialization prerequisites before selecting a tenth slice."
                ),
            },
            {
                "id": "tenth_slice_selection",
                "milestone": "M73-01",
                "priority": 2,
                "status": "deferred",
                "enabled_now": False,
                "reason": "Do not select a tenth slice until the M72 materialization audit closes.",
            },
            {
                "id": "runtime_ui_bot_promotion",
                "milestone": "future-explicit-gate",
                "priority": 3,
                "status": "blocked_by_explicit_gate",
                "enabled_now": False,
                "reason": (
                    "Saved decks, UI deck publication, bot playbooks, G Zone, Stride, "
                    "Aqua Force battle-order runtime, and live card text parsing need later gates."
                ),
            },
        ]
    return [
        {
            "id": "repair_failed_fixture_evidence",
            "milestone": "M70-repair",
            "priority": 1,
            "status": "required",
            "enabled_now": True,
            "reason": "M70-04 evidence is not ready for post-nine queue planning.",
        },
        {
            "id": "gated_real_artifact_materialization_audit",
            "milestone": "M72-01",
            "priority": 2,
            "status": "blocked",
            "enabled_now": False,
            "reason": "Run only after M70 evidence has nine passing fixture records.",
        },
    ]


def build_post_nine_fixture_queue_plan(scale_decision: dict[str, Any] | None = None) -> dict[str, Any]:
    scale_decision = scale_decision or load_json(M70_04_DECISION)
    ready = _m70_ready(scale_decision)
    options = _queue_options(ready)
    recommended = options[0]
    scope = {
        "post_nine_queue_planning": True,
        "artifact_materialization_audit_only": ready,
        "uses_m70_04_evidence": True,
    }
    for flag in RUNTIME_BOUNDARY_FLAGS:
        scope[flag] = False

    return {
        "version": "M71-01",
        "description": "Post-nine fixture queue planning",
        "source_inputs": {
            "nine_fixture_scale_decision": str(M70_04_DECISION.relative_to(ROOT)),
        },
        "scope": scope,
        "input_summary": _input_summary(scale_decision, ready),
        "queue_options": options,
        "decision": {
            "recommended_queue_id": recommended["id"],
            "recommended_milestone": recommended["milestone"],
            "opens_m72_01": ready,
            "routes_to_m70_repair": not ready,
            "tenth_slice_selected_now": False,
            "runtime_promotion_allowed": False,
            "real_artifact_materialization_allowed_now": False,
            "requires_new_gate_for_runtime_or_ui": True,
            "decision_notes": [
                "M70-04 proves nine fixture smoke records only at scaffold/planning level.",
                "The next queue should audit real artifact materialization before any tenth-slice selection.",
                "Runtime/UI/bot promotion remains disabled until a later explicit gate.",
            ]
            if ready
            else [
                "M70-04 evidence is incomplete or failed.",
                "Repair the fixture evidence before post-nine queue planning continues.",
            ],
        },
        "summary": {
            "m71_01_ready": True,
            "m70_ready_for_m71": ready,
            "ready_for_m72_01": ready,
            "blocking_issue_count": 0 if ready else 1,
            "issue_count": 0 if ready else 1,
            "queue_option_count": len(options),
        },
        "next_target": {
            "milestone": "M72-01" if ready else "M70-repair",
            "task": "Gated fixture artifact materialization audit"
            if ready
            else "Repair failed fixture evidence",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    input_summary = report["input_summary"]
    lines = [
        "# M71-01 Post-Nine Fixture Queue Plan",
        "",
        "## Summary",
        "",
        f"- M70 ready for M71: `{summary['m70_ready_for_m71']}`",
        f"- Ready for M72-01: `{summary['ready_for_m72_01']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        f"- Queue options: `{summary['queue_option_count']}`",
        "",
        "## M70 Evidence",
        "",
        f"- M70 version: `{input_summary['m70_version']}`",
        f"- Fixture evidence: `{input_summary['fixture_evidence_count']}`",
        f"- Passed fixtures: `{input_summary['passed_fixture_count']}`",
        f"- Failed fixtures: `{input_summary['failed_fixture_count']}`",
        f"- M70 recommendation: `{input_summary['m70_recommended_next_action']}`",
        "",
        "## Queue Options",
        "",
    ]
    for option in report["queue_options"]:
        lines.append(
            "- `{id}` -> `{milestone}` status=`{status}` enabled=`{enabled}` priority=`{priority}`".format(
                id=option["id"],
                milestone=option["milestone"],
                status=option["status"],
                enabled=option["enabled_now"],
                priority=option["priority"],
            )
        )
    lines.extend(
        [
            "",
            "## Decision",
            "",
            f"- Recommended queue: `{decision['recommended_queue_id']}`",
            f"- Recommended milestone: `{decision['recommended_milestone']}`",
            f"- Opens M72-01: `{decision['opens_m72_01']}`",
            f"- Routes to M70 repair: `{decision['routes_to_m70_repair']}`",
            f"- Tenth slice selected now: `{decision['tenth_slice_selected_now']}`",
            f"- Runtime promotion allowed: `{decision['runtime_promotion_allowed']}`",
            f"- Real artifact materialization allowed now: `{decision['real_artifact_materialization_allowed_now']}`",
            "",
            "## Boundary",
            "",
            "- No tenth slice selection.",
            "- No runtime fixture creation.",
            "- No real artifact materialization in this slice.",
            "- No saved-deck injection or UI deck publication.",
            "- No bot/playbook promotion.",
            "- No G Zone, Stride, Aqua Force battle-order, Bloom/token, Lock, Unlock, Legion, or Mate runtime.",
            "- No live card text parsing.",
            "- No GameState mutation.",
            "",
            "## Next",
            "",
            f"`{report['next_target']['milestone']}`: {report['next_target']['task']}.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Any = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M71-01 post-nine fixture queue plan.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Any = None) -> int:
    args = parse_args(argv)
    report = build_post_nine_fixture_queue_plan()
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M71-01 post-nine fixture queue plan wrote {json_path}")
    print(f"M71-01 post-nine fixture queue plan summary wrote {md_path}")
    print(
        "ready_for_m72_01={ready} recommended={recommended} issues={issues}".format(
            ready=report["summary"]["ready_for_m72_01"],
            recommended=report["decision"]["recommended_milestone"],
            issues=report["summary"]["issue_count"],
        )
    )
    return 0 if report["summary"]["m71_01_ready"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
