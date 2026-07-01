"""Build the M51-04 fifth-slice recipe pipeline entry gate artifact."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M51_02_READINESS = OUTPUT_DIR / "m51_02_fifth_slice_fixture_readiness.json"
M51_03_PROBE = OUTPUT_DIR / "m51_03_fifth_slice_semantic_compatibility_probe.json"


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


def build_fifth_slice_recipe_pipeline_entry_gate(
    readiness_report: dict[str, Any] | None = None,
    semantic_probe: dict[str, Any] | None = None,
) -> dict[str, Any]:
    readiness_report = readiness_report or load_json(M51_02_READINESS)
    semantic_probe = semantic_probe or load_json(M51_03_PROBE)
    issues: list[dict[str, Any]] = []
    readiness = readiness_report.get("readiness", {})
    probe_readiness = semantic_probe.get("readiness", {})
    probe_summary = semantic_probe.get("summary", {})
    format_scope = readiness_report.get("format_scope", {})

    if not readiness.get("all_fixture_expectations_met"):
        issues.append(
            _issue(
                "fixture_expectations_not_met",
                "blocker",
                "M51-02 fixture/format readiness must pass before recipe pipeline entry.",
                {"readiness": readiness},
            )
        )
    if not readiness.get("semantic_probe_ready"):
        issues.append(
            _issue(
                "semantic_probe_not_ready_from_readiness_gate",
                "blocker",
                "M51-02 did not mark the selected slice ready for semantic probe.",
                {"readiness": readiness},
            )
        )
    if not probe_readiness.get("ready_for_m51_04"):
        issues.append(
            _issue(
                "semantic_compatibility_probe_not_ready",
                "blocker",
                "M51-03 semantic/compatibility probe must pass before recipe pipeline entry.",
                {"readiness": probe_readiness},
            )
        )
    if probe_summary.get("candidate_edge_count", 0) <= 0:
        issues.append(
            _issue(
                "no_candidate_edges",
                "blocker",
                "Fifth slice needs at least one candidate edge before recipe work.",
                {"summary": probe_summary},
            )
        )
    if readiness.get("runtime_or_bot_promotion_allowed") or probe_readiness.get("runtime_or_bot_promotion_allowed"):
        issues.append(
            _issue(
                "unexpected_runtime_or_bot_promotion",
                "blocker",
                "Readiness/probe artifacts must not allow runtime or bot promotion at this gate.",
                {"m51_02": readiness, "m51_03": probe_readiness},
            )
        )

    blocker_count = sum(1 for issue in issues if issue["severity"] == "blocker")
    offline_recipe_pipeline_allowed = blocker_count == 0
    fixture_scaffold_required = bool(format_scope.get("new_format_or_mechanic_fixtures_required"))

    return {
        "version": "M51-04",
        "description": "Fifth-slice recipe pipeline entry gate",
        "source_inputs": {
            "fifth_slice_fixture_readiness": str(M51_02_READINESS.relative_to(ROOT)),
            "fifth_slice_semantic_compatibility_probe": str(M51_03_PROBE.relative_to(ROOT)),
        },
        "selected_target": semantic_probe.get("selected_target", readiness_report.get("selected_target", {})),
        "evidence_summary": {
            "fixture_readiness_passed": bool(readiness.get("all_fixture_expectations_met")),
            "semantic_probe_passed": bool(probe_readiness.get("fifth_slice_semantic_compatibility_probe_passed")),
            "source_card_count": readiness_report.get("card_pool_summary", {}).get("source_card_count", 0),
            "semantic_card_count": probe_summary.get("semantic_card_count", 0),
            "manual_review_card_count": probe_summary.get("manual_review_card_count", 0),
            "pair_graph_edge_count": probe_summary.get("pair_graph_edge_count", 0),
            "candidate_edge_count": probe_summary.get("candidate_edge_count", 0),
            "policy_reuse_decision": format_scope.get("policy_reuse_decision", ""),
            "new_format_or_mechanic_fixtures_required": fixture_scaffold_required,
        },
        "decision": {
            "offline_recipe_pipeline_allowed": offline_recipe_pipeline_allowed,
            "fixture_scaffold_required_before_recipe_validation": fixture_scaffold_required,
            "runtime_deck_promotion_allowed": False,
            "saved_deck_or_ui_publication_allowed": False,
            "bot_playbook_promotion_allowed": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "recommended_next_queue": "M52 Fifth-slice offline recipe pipeline"
            if offline_recipe_pipeline_allowed
            else "Hold fifth-slice recipe pipeline",
            "reason": (
                "Fifth slice has source-backed readiness plus a passed semantic/compatibility probe; offline recipe work may start with a new fixture scaffold."
                if offline_recipe_pipeline_allowed
                else "Fifth-slice recipe work is blocked until readiness/probe issues are repaired."
            ),
        },
        "proposed_m52_queue": [
            {
                "milestone": "M52-01",
                "task": "Fifth-slice fixture scaffold",
                "scope": "Define source-backed fixture policy for the Gold Paladin Link Joker/Legion Mate slice before validator work.",
            },
            {
                "milestone": "M52-02",
                "task": "Fifth-slice review packet",
                "scope": "Export candidate edges, manual-review cards, and format notes for human review.",
            },
            {
                "milestone": "M52-03",
                "task": "Fifth-slice recipe draft model",
                "scope": "Create advisory recipe drafts only; no saved deck or UI injection.",
            },
            {
                "milestone": "M52-04",
                "task": "Fifth-slice recipe validator",
                "scope": "Validate count, trigger, grade, identity, copy limits, missing cards, and fixture scaffold constraints.",
            },
            {
                "milestone": "M52-05",
                "task": "Fifth-slice combo-to-recipe consistency",
                "scope": "Check candidate combo cards are present and not blocked by manual-review dependency.",
            },
            {
                "milestone": "M52-06",
                "task": "Fifth-slice blocker repair candidates",
                "scope": "Generate source-backed repair candidates for blocked recipes.",
            },
            {
                "milestone": "M52-closeout",
                "task": "Fifth-slice runtime readiness decision",
                "scope": "Decide whether any recipe can later enter human acceptance and runtime fixture gates.",
            },
        ],
        "runtime_boundary": {
            "decision_artifact_only": True,
            "does_not_create_deck": True,
            "does_not_create_recipe_draft": True,
            "does_not_create_runtime_fixture": True,
            "does_not_mutate_runtime_pack": True,
            "does_not_publish_to_ui": True,
            "does_not_publish_to_bot": True,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "GameState_mutation": False,
        },
        "issues": issues,
        "summary": {
            "decision_ready": blocker_count == 0,
            "blocking_issue_count": blocker_count,
            "issue_count": len(issues),
            "offline_recipe_pipeline_allowed": offline_recipe_pipeline_allowed,
            "ready_for_m52": offline_recipe_pipeline_allowed,
        },
        "next_target": {
            "milestone": "M52-01" if offline_recipe_pipeline_allowed else "M51-repair-semantic",
            "task": "Fifth-slice fixture scaffold"
            if offline_recipe_pipeline_allowed
            else "Repair fifth-slice recipe pipeline entry blockers",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    evidence = report["evidence_summary"]
    target = report["selected_target"]
    lines = [
        "# M51-04 Fifth-Slice Recipe Pipeline Entry Gate",
        "",
        "## Selected Target",
        "",
        f"- Group: `{target.get('group', '')}`",
        f"- Era preset: `{target.get('era_preset', '')}`",
        "",
        "## Decision",
        "",
        f"- Decision ready: `{summary['decision_ready']}`",
        f"- Offline recipe pipeline allowed: `{decision['offline_recipe_pipeline_allowed']}`",
        f"- Fixture scaffold required before recipe validation: `{decision['fixture_scaffold_required_before_recipe_validation']}`",
        f"- Runtime deck promotion allowed: `{decision['runtime_deck_promotion_allowed']}`",
        f"- Saved deck/UI publication allowed: `{decision['saved_deck_or_ui_publication_allowed']}`",
        f"- Bot playbook promotion allowed: `{decision['bot_playbook_promotion_allowed']}`",
        f"- G Zone runtime enabled: `{decision['g_zone_runtime_enabled']}`",
        f"- Stride runtime enabled: `{decision['stride_runtime_enabled']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        "",
        "## Evidence",
        "",
        f"- Source card count: `{evidence['source_card_count']}`",
        f"- Semantic card count: `{evidence['semantic_card_count']}`",
        f"- Manual-review card count: `{evidence['manual_review_card_count']}`",
        f"- Pair graph edges: `{evidence['pair_graph_edge_count']}`",
        f"- Candidate edges: `{evidence['candidate_edge_count']}`",
        f"- Policy reuse decision: `{evidence['policy_reuse_decision']}`",
        "",
        "## Proposed M52 Queue",
        "",
    ]
    for item in report["proposed_m52_queue"]:
        lines.append(f"- `{item['milestone']}`: {item['task']} - {item['scope']}")
    lines.extend(["", "## Boundary", ""])
    for key, value in report["runtime_boundary"].items():
        lines.append(f"- `{key}`: `{value}`")
    lines.extend(["", "## Issues", ""])
    if report["issues"]:
        for issue in report["issues"]:
            lines.append(f"- `{issue['code']}` severity=`{issue['severity']}` details=`{issue['details']}`")
    else:
        lines.append("- None")
    lines.extend(["", "## Next", "", f"`{report['next_target']['milestone']}`: {report['next_target']['task']}.", ""])
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M51-04 fifth-slice recipe pipeline entry gate.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_fifth_slice_recipe_pipeline_entry_gate()
    json_path = args.output_dir / "m51_04_fifth_slice_recipe_pipeline_entry_gate.json"
    md_path = args.output_dir / "m51_04_fifth_slice_recipe_pipeline_entry_gate.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M51-04 fifth-slice recipe gate wrote {json_path}")
    print(f"M51-04 fifth-slice recipe gate summary wrote {md_path}")
    print(
        "ready={ready} blockers={blockers} next={next_milestone}".format(
            ready=report["summary"]["ready_for_m52"],
            blockers=report["summary"]["blocking_issue_count"],
            next_milestone=report["next_target"]["milestone"],
        )
    )
    return 0 if report["summary"]["ready_for_m52"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
