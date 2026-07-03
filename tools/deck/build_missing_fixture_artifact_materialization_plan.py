"""Build the M72-02 missing fixture artifact materialization plan."""

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
M72_01_AUDIT = OUTPUT_DIR / "m72_01_gated_fixture_artifact_materialization_audit.json"
JSON_OUTPUT = OUTPUT_DIR / "m72_02_missing_fixture_artifact_materialization_plan.json"
MD_OUTPUT = OUTPUT_DIR / "m72_02_missing_fixture_artifact_materialization_plan.md"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


MATERIALIZATION_STEPS: list[dict[str, Any]] = [
    {
        "artifact_id": "m58_01_sixth_schema",
        "milestone": "M58-01",
        "fixture_chain": "sixth_fixture_shadow_paladin",
        "role": "schema_validation",
        "command": "python tools\\deck\\validate_sixth_runtime_fixture_schema.py",
        "expected_output": "outputs/target_slice/m58_01_sixth_fixture_schema_validation.json",
        "requires": ["real M57-06/M57-closeout fixture evidence"],
    },
    {
        "artifact_id": "m58_02_sixth_deck_text",
        "milestone": "M58-02",
        "fixture_chain": "sixth_fixture_shadow_paladin",
        "role": "deck_text_export",
        "command": "python tools\\deck\\export_sixth_fixture_deck_text.py",
        "expected_output": "outputs/target_slice/m58_02_sixth_fixture_deck_text_export.json",
        "requires": ["M58-01 schema validation JSON"],
    },
    {
        "artifact_id": "m58_03_sixth_headless_smoke",
        "milestone": "M58-03",
        "fixture_chain": "sixth_fixture_shadow_paladin",
        "role": "headless_load_smoke",
        "command": "python tools\\deck\\build_sixth_headless_fixture_load_smoke.py",
        "expected_output": "outputs/target_slice/m58_03_sixth_fixture_load_smoke.json",
        "requires": ["M58-02 deck text JSON", "M58-02 count-line deck text", "Unity headless acceptance evidence for scale gate"],
    },
    {
        "artifact_id": "m58_04_sixth_scale_decision",
        "milestone": "M58-04",
        "fixture_chain": "sixth_fixture_shadow_paladin",
        "role": "scale_decision",
        "command": "python tools\\deck\\build_six_fixture_scale_decision.py",
        "expected_output": "outputs/target_slice/m58_04_six_fixture_scale_decision.json",
        "requires": ["M58-03 headless smoke JSON with Unity deck-code acceptance"],
    },
    {
        "artifact_id": "m62_01_seventh_schema",
        "milestone": "M62-01",
        "fixture_chain": "seventh_fixture_neo_nectar",
        "role": "schema_validation",
        "command": "python tools\\deck\\validate_seventh_runtime_fixture_schema.py",
        "expected_output": "outputs/target_slice/m62_01_seventh_fixture_schema_validation.json",
        "requires": ["real M61-06/M61-closeout fixture evidence", "M58-04 scale decision JSON"],
    },
    {
        "artifact_id": "m62_02_seventh_deck_text",
        "milestone": "M62-02",
        "fixture_chain": "seventh_fixture_neo_nectar",
        "role": "deck_text_export",
        "command": "python tools\\deck\\export_seventh_fixture_deck_text.py",
        "expected_output": "outputs/target_slice/m62_02_seventh_fixture_deck_text_export.json",
        "requires": ["M62-01 schema validation JSON"],
    },
    {
        "artifact_id": "m62_03_seventh_headless_smoke",
        "milestone": "M62-03",
        "fixture_chain": "seventh_fixture_neo_nectar",
        "role": "headless_load_smoke",
        "command": "python tools\\deck\\build_seventh_headless_fixture_load_smoke.py",
        "expected_output": "outputs/target_slice/m62_03_seventh_fixture_load_smoke.json",
        "requires": ["M62-02 deck text JSON", "M62-02 count-line deck text", "Unity headless acceptance evidence for scale gate"],
    },
    {
        "artifact_id": "m62_04_seventh_scale_decision",
        "milestone": "M62-04",
        "fixture_chain": "seventh_fixture_neo_nectar",
        "role": "scale_decision",
        "command": "python tools\\deck\\build_seven_fixture_scale_decision.py",
        "expected_output": "outputs/target_slice/m62_04_seven_fixture_scale_decision.json",
        "requires": ["M62-03 headless smoke JSON with Unity deck-code acceptance"],
    },
    {
        "artifact_id": "m66_01_eighth_schema",
        "milestone": "M66-01",
        "fixture_chain": "eighth_fixture_kagero",
        "role": "schema_validation",
        "command": "python tools\\deck\\validate_eighth_runtime_fixture_schema.py",
        "expected_output": "outputs/target_slice/m66_01_eighth_fixture_schema_validation.json",
        "requires": ["real M65-06/M65-closeout fixture evidence", "M62-04 scale decision JSON"],
    },
    {
        "artifact_id": "m66_02_eighth_deck_text",
        "milestone": "M66-02",
        "fixture_chain": "eighth_fixture_kagero",
        "role": "deck_text_export",
        "command": "python tools\\deck\\export_eighth_fixture_deck_text.py",
        "expected_output": "outputs/target_slice/m66_02_eighth_fixture_deck_text_export.json",
        "requires": ["M66-01 schema validation JSON"],
    },
    {
        "artifact_id": "m66_03_eighth_headless_smoke",
        "milestone": "M66-03",
        "fixture_chain": "eighth_fixture_kagero",
        "role": "headless_load_smoke",
        "command": "python tools\\deck\\build_eighth_headless_fixture_load_smoke.py",
        "expected_output": "outputs/target_slice/m66_03_eighth_fixture_load_smoke.json",
        "requires": ["M66-02 deck text JSON", "M66-02 count-line deck text", "Unity headless acceptance evidence for scale gate"],
    },
    {
        "artifact_id": "m66_04_eighth_scale_decision",
        "milestone": "M66-04",
        "fixture_chain": "eighth_fixture_kagero",
        "role": "scale_decision",
        "command": "python tools\\deck\\build_eight_fixture_scale_decision.py",
        "expected_output": "outputs/target_slice/m66_04_eight_fixture_scale_decision.json",
        "requires": ["M66-03 headless smoke JSON with Unity deck-code acceptance"],
    },
    {
        "artifact_id": "m70_01_ninth_schema",
        "milestone": "M70-01",
        "fixture_chain": "ninth_fixture_aqua_force",
        "role": "schema_validation",
        "command": "python tools\\deck\\validate_ninth_runtime_fixture_schema.py",
        "expected_output": "outputs/target_slice/m70_01_ninth_fixture_schema_validation.json",
        "requires": ["real M69-06/M69-closeout fixture evidence", "M66-04 scale decision JSON"],
    },
    {
        "artifact_id": "m70_02_ninth_deck_text",
        "milestone": "M70-02",
        "fixture_chain": "ninth_fixture_aqua_force",
        "role": "deck_text_export",
        "command": "python tools\\deck\\export_ninth_fixture_deck_text.py",
        "expected_output": "outputs/target_slice/m70_02_ninth_fixture_deck_text_export.json",
        "requires": ["M70-01 schema validation JSON"],
    },
    {
        "artifact_id": "m70_03_ninth_headless_smoke",
        "milestone": "M70-03",
        "fixture_chain": "ninth_fixture_aqua_force",
        "role": "headless_load_smoke",
        "command": "python tools\\deck\\build_ninth_headless_fixture_load_smoke.py",
        "expected_output": "outputs/target_slice/m70_03_ninth_fixture_load_smoke.json",
        "requires": ["M70-02 deck text JSON", "M70-02 count-line deck text", "Unity headless acceptance evidence for scale gate"],
    },
    {
        "artifact_id": "m70_04_ninth_scale_decision",
        "milestone": "M70-04",
        "fixture_chain": "ninth_fixture_aqua_force",
        "role": "scale_decision",
        "command": "python tools\\deck\\build_nine_fixture_scale_decision.py",
        "expected_output": "outputs/target_slice/m70_04_nine_fixture_scale_decision.json",
        "requires": ["M70-03 headless smoke JSON with Unity deck-code acceptance"],
    },
    {
        "artifact_id": "m71_01_post_nine_queue_plan",
        "milestone": "M71-01",
        "fixture_chain": "post_nine_queue",
        "role": "queue_plan",
        "command": "python tools\\deck\\build_post_nine_fixture_queue_plan.py",
        "expected_output": "outputs/target_slice/m71_01_post_nine_fixture_queue_plan.json",
        "requires": ["M70-04 nine-fixture scale decision JSON"],
    },
]


RUNTIME_BOUNDARY_FLAGS = [
    "selects_tenth_slice",
    "creates_runtime_fixture",
    "materializes_real_artifacts",
    "executes_materialization_commands",
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


def _m72_ready(audit_report: dict[str, Any]) -> bool:
    summary = audit_report.get("summary", {})
    decision = audit_report.get("decision", {})
    next_target = audit_report.get("next_target", {})
    return (
        audit_report.get("version") == "M72-01"
        and bool(summary.get("m72_01_ready"))
        and bool(decision.get("audit_complete"))
        and next_target.get("milestone") in {"M72-02", "M73-01"}
    )


def _missing_artifact_ids(audit_report: dict[str, Any]) -> set[str]:
    return {item.get("id", "") for item in audit_report.get("artifact_audit", []) if not item.get("present")}


def _plan_steps(missing_ids: set[str]) -> list[dict[str, Any]]:
    steps: list[dict[str, Any]] = []
    for index, step in enumerate(MATERIALIZATION_STEPS, start=1):
        if step["artifact_id"] in missing_ids:
            plan_step = dict(step)
            plan_step["order"] = len(steps) + 1
            plan_step["status"] = "planned_not_executed"
            plan_step["execution_allowed_in_this_slice"] = False
            plan_step["materialization_allowed_in_this_slice"] = False
            plan_step["source_order"] = index
            steps.append(plan_step)
    return steps


def _chain_plan(steps: list[dict[str, Any]]) -> list[dict[str, Any]]:
    chains: dict[str, list[dict[str, Any]]] = {}
    for step in steps:
        chains.setdefault(step["fixture_chain"], []).append(step)

    result: list[dict[str, Any]] = []
    for chain, chain_steps in chains.items():
        result.append(
            {
                "fixture_chain": chain,
                "step_count": len(chain_steps),
                "first_step": chain_steps[0]["milestone"],
                "last_step": chain_steps[-1]["milestone"],
                "milestones": [step["milestone"] for step in chain_steps],
                "roles": [step["role"] for step in chain_steps],
            }
        )
    return result


def build_missing_fixture_artifact_materialization_plan(audit_report: dict[str, Any] | None = None) -> dict[str, Any]:
    audit_report = audit_report or load_json(M72_01_AUDIT)
    audit_ready = _m72_ready(audit_report)
    missing_ids = _missing_artifact_ids(audit_report) if audit_ready else set()
    steps = _plan_steps(missing_ids)
    known_missing_ids = {step["artifact_id"] for step in steps}
    unknown_missing_ids = sorted(missing_ids - known_missing_ids)
    all_missing_are_planned = not unknown_missing_ids
    ready_for_m72_03 = audit_ready and bool(steps) and all_missing_are_planned
    ready_for_m73_01 = audit_ready and not steps and not unknown_missing_ids

    scope = {
        "materialization_plan_only": True,
        "uses_m72_01_audit": True,
        "primary_json_artifact_plan": True,
    }
    for flag in RUNTIME_BOUNDARY_FLAGS:
        scope[flag] = False

    if not audit_ready:
        recommended = {
            "milestone": "M72-01-repair",
            "task": "Repair gated fixture artifact materialization audit",
            "reason": "M72-01 audit evidence is not ready for materialization planning.",
        }
    elif unknown_missing_ids:
        recommended = {
            "milestone": "M72-02-repair",
            "task": "Map unknown missing artifact ids before materialization planning",
            "reason": "M72-01 reported missing artifact ids that are not in the known plan table.",
        }
    elif steps:
        recommended = {
            "milestone": "M72-03",
            "task": "Materialize sixth fixture primary JSON artifacts",
            "reason": "The first missing chain is M58, so materialization should start with sixth fixture artifacts.",
        }
    else:
        recommended = {
            "milestone": "M73-01",
            "task": "Tenth-slice selection gate",
            "reason": "No missing primary JSON artifacts remain.",
        }

    return {
        "version": "M72-02",
        "description": "Missing fixture artifact materialization plan",
        "source_inputs": {
            "gated_fixture_artifact_materialization_audit": str(M72_01_AUDIT.relative_to(ROOT)),
        },
        "scope": scope,
        "input_summary": {
            "m72_01_version": audit_report.get("version"),
            "m72_01_ready": audit_ready,
            "m72_01_missing_artifact_count": audit_report.get("summary", {}).get("missing_artifact_count"),
            "m72_01_next_target": audit_report.get("next_target", {}).get("milestone"),
        },
        "materialization_steps": steps,
        "chain_plan": _chain_plan(steps),
        "unknown_missing_artifact_ids": unknown_missing_ids,
        "decision": {
            "plan_complete": audit_ready and all_missing_are_planned,
            "recommended_milestone": recommended["milestone"],
            "recommended_task": recommended["task"],
            "recommended_reason": recommended["reason"],
            "ready_for_m72_03": ready_for_m72_03,
            "ready_for_m73_01": ready_for_m73_01,
            "tenth_slice_selected_now": False,
            "materialization_executed": False,
            "materialization_allowed_in_this_slice": False,
            "runtime_promotion_allowed": False,
            "requires_explicit_follow_up_gate": bool(steps),
        },
        "summary": {
            "m72_02_ready": audit_ready and all_missing_are_planned,
            "planned_step_count": len(steps),
            "affected_chain_count": len(_chain_plan(steps)),
            "unknown_missing_artifact_count": len(unknown_missing_ids),
            "ready_for_m72_03": ready_for_m72_03,
            "ready_for_m73_01": ready_for_m73_01,
            "blocking_issue_count": 0 if audit_ready and all_missing_are_planned else 1,
            "issue_count": 0 if audit_ready and all_missing_are_planned else 1,
        },
        "next_target": {
            "milestone": recommended["milestone"],
            "task": recommended["task"],
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    lines = [
        "# M72-02 Missing Fixture Artifact Materialization Plan",
        "",
        "## Summary",
        "",
        f"- Planned steps: `{summary['planned_step_count']}`",
        f"- Affected chains: `{summary['affected_chain_count']}`",
        f"- Unknown missing artifacts: `{summary['unknown_missing_artifact_count']}`",
        f"- Ready for M72-03: `{summary['ready_for_m72_03']}`",
        f"- Ready for M73-01: `{summary['ready_for_m73_01']}`",
        "",
        "## Chain Plan",
        "",
    ]
    for chain in report["chain_plan"]:
        lines.append(
            "- `{chain}` steps=`{count}` first=`{first}` last=`{last}`".format(
                chain=chain["fixture_chain"],
                count=chain["step_count"],
                first=chain["first_step"],
                last=chain["last_step"],
            )
        )
    lines.extend(["", "## Ordered Steps", ""])
    if report["materialization_steps"]:
        for step in report["materialization_steps"]:
            lines.append(
                "{order}. `{milestone}` `{role}` -> `{output}`".format(
                    order=step["order"],
                    milestone=step["milestone"],
                    role=step["role"],
                    output=step["expected_output"],
                )
            )
            lines.append(f"   - Command: `{step['command']}`")
    else:
        lines.append("- None.")
    lines.extend(
        [
            "",
            "## Decision",
            "",
            f"- Recommended milestone: `{decision['recommended_milestone']}`",
            f"- Recommended task: `{decision['recommended_task']}`",
            f"- Materialization executed: `{decision['materialization_executed']}`",
            f"- Materialization allowed in this slice: `{decision['materialization_allowed_in_this_slice']}`",
            f"- Tenth slice selected now: `{decision['tenth_slice_selected_now']}`",
            "",
            "## Boundary",
            "",
            "- Plan/checklist only.",
            "- Do not execute materialization commands in this slice.",
            "- Do not write missing artifacts in this slice.",
            "- Do not select a tenth slice.",
            "- Do not create runtime fixtures.",
            "- Do not publish saved decks or UI decks.",
            "- Do not enable bot/playbooks.",
            "- Do not enable G Zone, Stride, Aqua Force battle-order, Bloom/token, Lock, Unlock, Legion, or Mate runtime.",
            "- Do not parse live card text.",
            "- Do not mutate GameState.",
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
    parser = argparse.ArgumentParser(description="Build M72-02 missing fixture artifact materialization plan.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Any = None) -> int:
    args = parse_args(argv)
    report = build_missing_fixture_artifact_materialization_plan()
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M72-02 missing fixture artifact materialization plan wrote {json_path}")
    print(f"M72-02 missing fixture artifact materialization plan summary wrote {md_path}")
    print(
        "planned_steps={steps} next={next_target}".format(
            steps=report["summary"]["planned_step_count"],
            next_target=report["next_target"]["milestone"],
        )
    )
    return 0 if report["summary"]["m72_02_ready"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
