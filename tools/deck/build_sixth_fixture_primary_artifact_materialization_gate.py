"""Gate M72-03 sixth fixture primary JSON artifact materialization."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_missing_fixture_artifact_materialization_plan import (  # noqa: E402
    MATERIALIZATION_STEPS,
    RUNTIME_BOUNDARY_FLAGS,
)
from tools.deck.build_six_fixture_scale_decision import (  # noqa: E402
    build_six_fixture_scale_decision,
)
from tools.deck.build_sixth_headless_fixture_load_smoke import (  # noqa: E402
    build_sixth_headless_fixture_load_smoke,
)
from tools.deck.export_sixth_fixture_deck_text import (  # noqa: E402
    build_sixth_fixture_deck_text_export,
)
from tools.deck.validate_sixth_runtime_fixture_schema import (  # noqa: E402
    DEFAULT_FIXTURE,
    build_sixth_runtime_fixture_schema_validation_report,
)


OUTPUT_DIR = ROOT / "outputs" / "target_slice"
JSON_OUTPUT = OUTPUT_DIR / "m72_03_sixth_fixture_primary_artifact_materialization_gate.json"
MD_OUTPUT = OUTPUT_DIR / "m72_03_sixth_fixture_primary_artifact_materialization_gate.md"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


SIXTH_CHAIN = "sixth_fixture_shadow_paladin"
SIXTH_MILESTONES = ["M58-01", "M58-02", "M58-03", "M58-04"]


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _display_path(path: Path) -> str:
    try:
        return str(path.resolve().relative_to(ROOT))
    except ValueError:
        return str(path)


def _issue(code: str, message: str, details: dict[str, Any] | None = None) -> dict[str, Any]:
    return {
        "code": code,
        "severity": "blocker",
        "message": message,
        "details": details or {},
    }


def _sixth_plan_steps() -> list[dict[str, Any]]:
    return [
        dict(step)
        for step in MATERIALIZATION_STEPS
        if step.get("fixture_chain") == SIXTH_CHAIN and step.get("milestone") in set(SIXTH_MILESTONES)
    ]


def _plan_ready_for_sixth(plan_report: dict[str, Any] | None) -> tuple[bool, list[dict[str, Any]]]:
    if plan_report is None:
        return True, []
    summary = plan_report.get("summary", {})
    decision = plan_report.get("decision", {})
    if plan_report.get("version") != "M72-02" or not summary.get("m72_02_ready"):
        return False, [
            _issue(
                "m72_02_plan_not_ready",
                "M72-03 requires a ready M72-02 materialization plan when one is supplied.",
                {
                    "version": plan_report.get("version"),
                    "summary": summary,
                    "decision": {
                        "recommended_milestone": decision.get("recommended_milestone"),
                        "ready_for_m72_03": decision.get("ready_for_m72_03"),
                    },
                },
            )
        ]
    planned = [
        step.get("milestone")
        for step in plan_report.get("materialization_steps", [])
        if step.get("fixture_chain") == SIXTH_CHAIN
    ]
    missing = [milestone for milestone in SIXTH_MILESTONES if milestone not in planned]
    if missing:
        return False, [
            _issue(
                "m72_02_plan_missing_sixth_chain_steps",
                "M72-02 plan does not include the complete M58 sixth fixture chain.",
                {"missing_milestones": missing, "planned_milestones": planned},
            )
        ]
    return True, []


def _artifact_status(milestone: str, status: str, ready: bool, details: dict[str, Any] | None = None) -> dict[str, Any]:
    step = next((item for item in _sixth_plan_steps() if item["milestone"] == milestone), {})
    return {
        "milestone": milestone,
        "artifact_id": step.get("artifact_id", ""),
        "role": step.get("role", ""),
        "expected_output": step.get("expected_output", ""),
        "status": status,
        "ready": bool(ready),
        "details": details or {},
    }


def build_sixth_fixture_primary_artifact_materialization_gate(
    plan_report: dict[str, Any] | None = None,
    fixture: dict[str, Any] | None = None,
    fixture_path: Path = DEFAULT_FIXTURE,
    unity_result_path: Path | None = None,
    unity_replay_path: Path | None = None,
) -> dict[str, Any]:
    """Return an M72-03 gate report without writing M58 artifacts."""

    issues: list[dict[str, Any]] = []
    materialization: list[dict[str, Any]] = []
    private_reports: dict[str, Any] = {}
    plan_ready, plan_issues = _plan_ready_for_sixth(plan_report)
    issues.extend(plan_issues)
    loaded_fixture = fixture

    if plan_ready and loaded_fixture is None:
        if fixture_path.exists():
            loaded_fixture = load_json(fixture_path)
        else:
            issues.append(
                _issue(
                    "m57_06_runtime_fixture_missing",
                    "M72-03 cannot materialize M58 artifacts until the real M57-06 runtime fixture exists.",
                    {
                        "required_fixture": _display_path(fixture_path),
                        "required_previous_milestone": "M57-06",
                        "safe_next_milestone": "M57-02",
                    },
                )
            )

    if issues:
        for milestone in SIXTH_MILESTONES:
            materialization.append(_artifact_status(milestone, "blocked_prerequisite_missing", False))
    else:
        m58_01 = build_sixth_runtime_fixture_schema_validation_report(loaded_fixture, fixture_path)
        private_reports["m58_01"] = m58_01
        m58_01_ready = bool(m58_01.get("summary", {}).get("ready_for_m58_02"))
        materialization.append(
            _artifact_status(
                "M58-01",
                "built_in_memory_ready" if m58_01_ready else "built_in_memory_blocked",
                m58_01_ready,
                {
                    "schema_valid": m58_01.get("summary", {}).get("schema_valid"),
                    "blocking_issue_count": m58_01.get("summary", {}).get("blocking_issue_count"),
                },
            )
        )

        if m58_01_ready:
            m58_02 = build_sixth_fixture_deck_text_export(loaded_fixture, m58_01)
            private_reports["m58_02"] = m58_02
            m58_02_ready = bool(m58_02.get("summary", {}).get("ready_for_m58_03"))
        else:
            m58_02 = {}
            m58_02_ready = False
        materialization.append(
            _artifact_status(
                "M58-02",
                "built_in_memory_ready" if m58_02_ready else "blocked_by_m58_01",
                m58_02_ready,
                {
                    "export_ready": m58_02.get("summary", {}).get("export_ready"),
                    "blocking_issue_count": m58_02.get("summary", {}).get("blocking_issue_count"),
                    "deck_text_sha256": m58_02.get("outputs", {}).get("deck_text_sha256", ""),
                },
            )
        )

        if m58_02_ready:
            m58_03 = build_sixth_headless_fixture_load_smoke(
                loaded_fixture,
                m58_02,
                m58_02.get("_deck_text", ""),
                unity_result_path,
                unity_replay_path,
            )
            private_reports["m58_03"] = m58_03
            m58_03_offline_ready = bool(m58_03.get("summary", {}).get("offline_load_ready"))
            m58_03_ready = bool(m58_03.get("summary", {}).get("ready_for_m58_04"))
        else:
            m58_03 = {}
            m58_03_offline_ready = False
            m58_03_ready = False
        materialization.append(
            _artifact_status(
                "M58-03",
                "built_in_memory_ready" if m58_03_ready else "needs_unity_headless_evidence",
                m58_03_ready,
                {
                    "offline_load_ready": m58_03_offline_ready,
                    "deck_code_created": m58_03.get("summary", {}).get("deck_code_created"),
                    "unity_headless_result_provided": m58_03.get("summary", {}).get("unity_headless_result_provided"),
                    "unity_headless_smoke_passed": m58_03.get("summary", {}).get("unity_headless_smoke_passed"),
                    "blocking_issue_count": m58_03.get("summary", {}).get("blocking_issue_count"),
                },
            )
        )

        if m58_03_ready:
            m58_04 = build_six_fixture_scale_decision(sixth_smoke=m58_03, sixth_fixture=loaded_fixture)
            private_reports["m58_04"] = m58_04
            m58_04_ready = bool(m58_04.get("summary", {}).get("ready_for_m59"))
            m58_04_status = "built_in_memory_ready" if m58_04_ready else "built_in_memory_blocked"
        else:
            m58_04 = {}
            m58_04_ready = False
            m58_04_status = "blocked_by_m58_03"
        materialization.append(
            _artifact_status(
                "M58-04",
                m58_04_status,
                m58_04_ready,
                {
                    "ready_for_m59": m58_04.get("summary", {}).get("ready_for_m59"),
                    "passed_fixture_count": m58_04.get("summary", {}).get("passed_fixture_count"),
                    "failed_fixture_count": m58_04.get("summary", {}).get("failed_fixture_count"),
                    "candidate_count": m58_04.get("summary", {}).get("candidate_count"),
                },
            )
        )

    ready_steps = sum(1 for item in materialization if item.get("ready"))
    chain_ready = ready_steps == len(SIXTH_MILESTONES) and not issues
    if issues:
        recommended = {
            "milestone": "M57-02",
            "task": "Provide explicit sixth-slice human recipe selection before M57-06/M58 materialization",
            "reason": "Real M57 human-gate artifacts and the M57-06 fixture are not materialized.",
        }
    elif chain_ready:
        recommended = {
            "milestone": "M72-04",
            "task": "Materialize seventh fixture primary JSON artifacts",
            "reason": "The sixth fixture M58 primary artifact chain is ready in-memory with Unity evidence.",
        }
    else:
        recommended = {
            "milestone": "M58-03-unity-headless-smoke",
            "task": "Run Unity headless with the sixth fixture deck code and rerun M72-03",
            "reason": "M58-03 needs accepted Unity deck-code evidence before M58-04 can be built.",
        }

    scope = {
        "sixth_fixture_primary_artifact_gate": True,
        "uses_m72_02_plan_table": True,
        "writes_m58_artifacts": False,
        "materializes_real_artifacts": False,
    }
    for flag in RUNTIME_BOUNDARY_FLAGS:
        scope[flag] = False

    return {
        "version": "M72-03",
        "description": "Sixth fixture primary JSON artifact materialization gate",
        "source_inputs": {
            "m72_02_materialization_plan": "tools/deck/build_missing_fixture_artifact_materialization_plan.py",
            "m57_06_runtime_fixture": _display_path(fixture_path),
            "unity_result_path": _display_path(unity_result_path) if unity_result_path else "",
            "unity_replay_path": _display_path(unity_replay_path) if unity_replay_path else "",
        },
        "scope": scope,
        "sixth_chain_plan": _sixth_plan_steps(),
        "materialization": materialization,
        "issues": issues,
        "decision": {
            "recommended_milestone": recommended["milestone"],
            "recommended_task": recommended["task"],
            "recommended_reason": recommended["reason"],
            "sixth_chain_ready": chain_ready,
            "m58_primary_json_artifacts_ready": chain_ready,
            "real_artifacts_written": False,
            "saved_deck_enabled": False,
            "ui_deck_list_enabled": False,
            "bot_playbook_enabled": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "GameState_mutation": False,
        },
        "summary": {
            "m72_03_ready": chain_ready or bool(issues),
            "sixth_chain_step_count": len(SIXTH_MILESTONES),
            "ready_step_count": ready_steps,
            "blocking_issue_count": len(issues),
            "issue_count": len(issues),
            "sixth_chain_ready": chain_ready,
            "ready_for_m72_04": chain_ready,
            "ready_for_m57_02_prerequisite": bool(issues),
        },
        "next_target": {
            "milestone": recommended["milestone"],
            "task": recommended["task"],
        },
        "_reports": private_reports,
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    serializable = {key: value for key, value in report.items() if key != "_reports"}
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(serializable, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    lines = [
        "# M72-03 Sixth Fixture Primary Artifact Materialization Gate",
        "",
        "## Summary",
        "",
        f"- Ready steps: `{summary['ready_step_count']}/{summary['sixth_chain_step_count']}`",
        f"- Sixth chain ready: `{summary['sixth_chain_ready']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        f"- Ready for M72-04: `{summary['ready_for_m72_04']}`",
        f"- Ready for M57-02 prerequisite: `{summary['ready_for_m57_02_prerequisite']}`",
        "",
        "## Materialization",
        "",
    ]
    for item in report["materialization"]:
        lines.append(
            "- `{milestone}` `{role}` status=`{status}` ready=`{ready}` output=`{output}`".format(
                milestone=item["milestone"],
                role=item["role"],
                status=item["status"],
                ready=item["ready"],
                output=item["expected_output"],
            )
        )
    lines.extend(["", "## Issues", ""])
    if report["issues"]:
        for issue in report["issues"]:
            lines.append(f"- `{issue['code']}` severity=`{issue['severity']}` details=`{issue['details']}`")
    else:
        lines.append("- None")
    lines.extend(
        [
            "",
            "## Decision",
            "",
            f"- Recommended milestone: `{decision['recommended_milestone']}`",
            f"- Recommended task: `{decision['recommended_task']}`",
            f"- Real artifacts written: `{decision['real_artifacts_written']}`",
            f"- Saved deck enabled: `{decision['saved_deck_enabled']}`",
            f"- UI deck list enabled: `{decision['ui_deck_list_enabled']}`",
            f"- Bot playbook enabled: `{decision['bot_playbook_enabled']}`",
            f"- G Zone runtime enabled: `{decision['g_zone_runtime_enabled']}`",
            f"- Stride runtime enabled: `{decision['stride_runtime_enabled']}`",
            f"- GameState mutation: `{decision['GameState_mutation']}`",
            "",
            "## Boundary",
            "",
            "- Gate/report only unless a later explicit materialization command writes artifacts.",
            "- Does not fake M57 human selection or acceptance.",
            "- Does not create saved decks or UI deck lists.",
            "- Does not enable bot playbooks, G Zone, or Stride runtime.",
            "- Does not mutate GameState.",
            "",
            "## Next",
            "",
            f"`{report['next_target']['milestone']}`: {report['next_target']['task']}.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M72-03 sixth fixture primary artifact materialization gate.")
    parser.add_argument("--fixture", type=Path, default=DEFAULT_FIXTURE)
    parser.add_argument("--unity-result", type=Path, default=None)
    parser.add_argument("--unity-replay", type=Path, default=None)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_sixth_fixture_primary_artifact_materialization_gate(
        fixture_path=args.fixture,
        unity_result_path=args.unity_result,
        unity_replay_path=args.unity_replay,
    )
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M72-03 sixth fixture primary artifact materialization gate wrote {json_path}")
    print(f"M72-03 sixth fixture primary artifact materialization gate summary wrote {md_path}")
    print(
        "ready_steps={ready}/{total} blockers={blockers} next={next_target}".format(
            ready=report["summary"]["ready_step_count"],
            total=report["summary"]["sixth_chain_step_count"],
            blockers=report["summary"]["blocking_issue_count"],
            next_target=report["next_target"]["milestone"],
        )
    )
    return 0 if report["summary"]["m72_03_ready"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
