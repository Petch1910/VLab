"""Build the M39-04 second-slice recipe scale decision artifact."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M39_03_REPORT = OUTPUT_DIR / "m39_03_headless_fixture_load_smoke.json"
M36_05_REPORT = OUTPUT_DIR / "m36_05_second_slice_readiness_comparison.json"
M35_E2_REPORT = OUTPUT_DIR / "m35_e2_second_slice_fixture_readiness.json"
M35_E3_REPORT = OUTPUT_DIR / "m35_e3_generalized_semantic_compatibility_probe.json"
M35_E4_REPORT = OUTPUT_DIR / "m35_e4_bot_integration_gate.json"

JSON_OUTPUT = OUTPUT_DIR / "m39_04_second_slice_recipe_scale_decision.json"
MD_OUTPUT = OUTPUT_DIR / "m39_04_second_slice_recipe_scale_decision.md"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _issue(code: str, severity: str, message: str, details: dict[str, Any] | None = None) -> dict[str, Any]:
    return {
        "code": code,
        "severity": severity,
        "message": message,
        "details": details or {},
    }


def build_second_slice_recipe_scale_decision(
    m39_03: dict[str, Any] | None = None,
    m36_05: dict[str, Any] | None = None,
    m35_e2: dict[str, Any] | None = None,
    m35_e3: dict[str, Any] | None = None,
    m35_e4: dict[str, Any] | None = None,
) -> dict[str, Any]:
    m39_03 = m39_03 or load_json(M39_03_REPORT)
    m36_05 = m36_05 or load_json(M36_05_REPORT)
    m35_e2 = m35_e2 or load_json(M35_E2_REPORT)
    m35_e3 = m35_e3 or load_json(M35_E3_REPORT)
    m35_e4 = m35_e4 or load_json(M35_E4_REPORT)

    issues: list[dict[str, Any]] = []
    if not m39_03.get("summary", {}).get("ready_for_m39_04"):
        issues.append(
            _issue(
                "first_fixture_consumption_not_ready",
                "blocker",
                "M39-03 must pass before scaling recipe work to a second slice.",
                {"summary": m39_03.get("summary", {})},
            )
        )
    if not m35_e2.get("readiness", {}).get("selected_group_fixture_ready"):
        issues.append(
            _issue(
                "second_slice_fixture_not_ready",
                "blocker",
                "Second-slice fixture readiness is not ready.",
                {"readiness": m35_e2.get("readiness", {})},
            )
        )
    if not m35_e3.get("readiness", {}).get("second_slice_semantic_compatibility_probe_passed"):
        issues.append(
            _issue(
                "second_slice_probe_not_ready",
                "blocker",
                "Second-slice semantic/compatibility probe is not ready.",
                {"readiness": m35_e3.get("readiness", {})},
            )
        )
    if m35_e3.get("readiness", {}).get("runtime_or_bot_promotion_allowed"):
        issues.append(
            _issue(
                "unexpected_runtime_promotion_flag",
                "blocker",
                "Second-slice probe unexpectedly allows runtime or bot promotion.",
                {"readiness": m35_e3.get("readiness", {})},
            )
        )
    if m35_e4.get("readiness", {}).get("runtime_bot_integration_enabled"):
        issues.append(
            _issue(
                "unexpected_bot_integration_enabled",
                "blocker",
                "Bot integration must remain disabled for second-slice scale decision.",
                {"readiness": m35_e4.get("readiness", {})},
            )
        )

    second_status = m36_05.get("second_slice_status", {})
    comparison = m36_05.get("comparison", {})
    selected_target = second_status.get("selected_target") or m35_e3.get("selected_target", {})
    blocker_count = sum(1 for issue in issues if issue["severity"] == "blocker")
    offline_recipe_scale_allowed = blocker_count == 0

    return {
        "version": "M39-04",
        "description": "Second-slice recipe scale decision",
        "source_inputs": {
            "m39_03_headless_fixture_load_smoke": str(M39_03_REPORT.relative_to(ROOT)),
            "m36_05_second_slice_readiness_comparison": str(M36_05_REPORT.relative_to(ROOT)),
            "m35_e2_second_slice_fixture_readiness": str(M35_E2_REPORT.relative_to(ROOT)),
            "m35_e3_generalized_semantic_compatibility_probe": str(M35_E3_REPORT.relative_to(ROOT)),
            "m35_e4_bot_integration_gate": str(M35_E4_REPORT.relative_to(ROOT)),
        },
        "selected_target": selected_target,
        "evidence_summary": {
            "first_fixture_headless_consumed": bool(m39_03.get("summary", {}).get("ready_for_m39_04")),
            "second_slice_fixture_ready": bool(m35_e2.get("readiness", {}).get("selected_group_fixture_ready")),
            "classic_core_policy_reusable": bool(m35_e2.get("readiness", {}).get("classic_core_policy_reusable")),
            "second_slice_probe_ready": bool(m35_e3.get("readiness", {}).get("second_slice_semantic_compatibility_probe_passed")),
            "probe_card_count": second_status.get("probe_card_count"),
            "probe_edge_count": second_status.get("probe_edge_count"),
            "probe_candidate_edges": second_status.get("probe_candidate_edges"),
            "manual_review_count": second_status.get("manual_review_count"),
            "candidate_edge_ratio_vs_first": comparison.get("second_candidate_edge_ratio_vs_first"),
        },
        "decision": {
            "offline_recipe_pipeline_allowed": offline_recipe_scale_allowed,
            "runtime_deck_promotion_allowed": False,
            "saved_deck_or_ui_publication_allowed": False,
            "bot_playbook_promotion_allowed": False,
            "recommended_next_queue": "M40 Second-slice offline recipe pipeline" if offline_recipe_scale_allowed else "Hold second-slice recipe pipeline",
            "reason": (
                "First fixture consumption passed and Oracle Think Tank has reusable Classic Core fixtures plus a passed semantic/compatibility probe."
                if offline_recipe_scale_allowed
                else "Second-slice scale is blocked until all readiness evidence passes."
            ),
        },
        "proposed_m40_queue": [
            {
                "milestone": "M40-01",
                "task": "Second-slice review packet",
                "scope": "Export Oracle Think Tank candidate edges, manual-review cards, and fixture notes for review.",
            },
            {
                "milestone": "M40-02",
                "task": "Second-slice recipe draft model",
                "scope": "Create advisory recipe drafts only; no saved deck injection.",
            },
            {
                "milestone": "M40-03",
                "task": "Second-slice recipe validator",
                "scope": "Validate count, trigger, grade, clan identity, copy limits, and missing cards.",
            },
            {
                "milestone": "M40-04",
                "task": "Second-slice combo-to-recipe consistency",
                "scope": "Check selected combo lines are present and not blocked by manual review.",
            },
            {
                "milestone": "M40-05",
                "task": "Second-slice blocker repair candidates",
                "scope": "Generate source-backed repair candidates for blocked recipes.",
            },
            {
                "milestone": "M40-closeout",
                "task": "Second-slice runtime readiness decision",
                "scope": "Decide whether any recipe can later enter a human-acceptance/runtime fixture gate.",
            },
        ],
        "runtime_boundary": {
            "decision_artifact_only": True,
            "does_not_create_deck": True,
            "does_not_mutate_runtime_pack": True,
            "does_not_publish_to_bot": True,
            "does_not_mutate_GameState": True,
        },
        "issues": issues,
        "summary": {
            "decision_ready": blocker_count == 0,
            "blocking_issue_count": blocker_count,
            "issue_count": len(issues),
            "offline_recipe_pipeline_allowed": offline_recipe_scale_allowed,
            "ready_for_m40": offline_recipe_scale_allowed,
        },
        "next_target": {
            "milestone": "M40-01" if offline_recipe_scale_allowed else "M39-04",
            "task": "Second-slice review packet" if offline_recipe_scale_allowed else "Resolve second-slice scale blockers",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    target = report["selected_target"]
    evidence = report["evidence_summary"]
    lines = [
        "# M39-04 Second-Slice Recipe Scale Decision",
        "",
        "## Summary",
        "",
        f"- Target slice: `{target.get('slice', '')}` / `{target.get('group', '')}`",
        f"- Decision ready: `{summary['decision_ready']}`",
        f"- Offline recipe pipeline allowed: `{decision['offline_recipe_pipeline_allowed']}`",
        f"- Runtime deck promotion allowed: `{decision['runtime_deck_promotion_allowed']}`",
        f"- Bot playbook promotion allowed: `{decision['bot_playbook_promotion_allowed']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        f"- Ready for M40: `{summary['ready_for_m40']}`",
        "",
        "## Evidence",
        "",
        f"- First fixture headless consumed: `{evidence['first_fixture_headless_consumed']}`",
        f"- Second-slice fixture ready: `{evidence['second_slice_fixture_ready']}`",
        f"- Classic Core policy reusable: `{evidence['classic_core_policy_reusable']}`",
        f"- Second-slice probe ready: `{evidence['second_slice_probe_ready']}`",
        f"- Probe cards: `{evidence['probe_card_count']}`",
        f"- Probe edges: `{evidence['probe_edge_count']}`",
        f"- Candidate edges: `{evidence['probe_candidate_edges']}`",
        f"- Manual-review cards: `{evidence['manual_review_count']}`",
        "",
        "## Decision",
        "",
        decision["reason"],
        "",
        "Allowed next work: offline review packets, advisory recipe drafts, validators, and repair candidates.",
        "",
        "Still blocked: saved-deck injection, UI publication, runtime deck promotion, and bot/playbook promotion.",
        "",
        "## Proposed M40 Queue",
        "",
    ]
    for item in report["proposed_m40_queue"]:
        lines.append(f"- `{item['milestone']}`: {item['task']} - {item['scope']}")
    lines.extend(["", "## Issues", ""])
    if report["issues"]:
        for issue in report["issues"]:
            lines.append(f"- `{issue['code']}` severity=`{issue['severity']}` details=`{issue['details']}`")
    else:
        lines.append("- None")
    lines.extend(["", "## Next", "", f"`{report['next_target']['milestone']}`: {report['next_target']['task']}", ""])
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M39-04 second-slice recipe scale decision.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_second_slice_recipe_scale_decision()
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M39-04 second-slice recipe scale decision wrote {json_path}")
    print(f"M39-04 second-slice recipe scale decision summary wrote {md_path}")
    print(
        "decision_ready={ready} offline_recipe_allowed={allowed} blockers={blockers} next={next_target}".format(
            ready=report["summary"]["decision_ready"],
            allowed=report["summary"]["offline_recipe_pipeline_allowed"],
            blockers=report["summary"]["blocking_issue_count"],
            next_target=report["next_target"]["milestone"],
        )
    )
    return 0 if report["summary"]["decision_ready"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
