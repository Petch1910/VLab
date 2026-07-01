"""Build the M47-04 fourth-slice recipe pipeline entry gate artifact."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M47_APPLIED_SCOPE = OUTPUT_DIR / "m47_repair_apply_scope.json"
M47_03_PROBE = OUTPUT_DIR / "m47_03_fourth_slice_semantic_compatibility_probe.json"


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


def build_fourth_slice_recipe_pipeline_entry_gate(
    applied_scope_report: dict[str, Any] | None = None,
    semantic_probe: dict[str, Any] | None = None,
) -> dict[str, Any]:
    applied_scope_report = applied_scope_report or load_json(M47_APPLIED_SCOPE)
    semantic_probe = semantic_probe or load_json(M47_03_PROBE)
    issues: list[dict[str, Any]] = []
    applied_readiness = applied_scope_report.get("readiness", {})
    probe_readiness = semantic_probe.get("readiness", {})
    probe_summary = semantic_probe.get("summary", {})
    applied_scope = applied_scope_report.get("applied_scope", {})

    if not applied_readiness.get("all_fixture_expectations_met"):
        issues.append(
            _issue(
                "applied_scope_fixture_expectations_not_met",
                "blocker",
                "M47-repair-apply-scope fixture expectations must pass before recipe pipeline entry.",
                {"readiness": applied_readiness},
            )
        )
    if not applied_readiness.get("ready_for_m47_03"):
        issues.append(
            _issue(
                "applied_scope_not_ready_for_semantic_probe",
                "blocker",
                "M47-repair-apply-scope did not mark the selected slice ready for semantic probe.",
                {"readiness": applied_readiness},
            )
        )
    if not probe_readiness.get("ready_for_m47_04"):
        issues.append(
            _issue(
                "semantic_compatibility_probe_not_ready",
                "blocker",
                "M47-03 semantic/compatibility probe must pass before recipe pipeline entry.",
                {"readiness": probe_readiness},
            )
        )
    if probe_summary.get("candidate_edge_count", 0) <= 0:
        issues.append(
            _issue(
                "no_candidate_edges",
                "blocker",
                "Fourth slice needs at least one candidate edge before recipe work.",
                {"summary": probe_summary},
            )
        )
    if applied_readiness.get("runtime_or_bot_promotion_allowed") or probe_readiness.get(
        "runtime_or_bot_promotion_allowed"
    ):
        issues.append(
            _issue(
                "unexpected_runtime_or_bot_promotion",
                "blocker",
                "Applied scope/probe artifacts must not allow runtime or bot promotion at this gate.",
                {"m47_apply_scope": applied_readiness, "m47_03": probe_readiness},
            )
        )

    blocker_count = sum(1 for issue in issues if issue["severity"] == "blocker")
    offline_recipe_pipeline_allowed = blocker_count == 0

    return {
        "version": "M47-04",
        "description": "Fourth-slice recipe pipeline entry gate",
        "source_inputs": {
            "fourth_slice_applied_scope": str(M47_APPLIED_SCOPE.relative_to(ROOT)),
            "fourth_slice_semantic_compatibility_probe": str(M47_03_PROBE.relative_to(ROOT)),
        },
        "selected_target": semantic_probe.get("selected_target", applied_scope_report.get("selected_target", {})),
        "evidence_summary": {
            "applied_scope_readiness_passed": bool(applied_readiness.get("all_fixture_expectations_met")),
            "semantic_probe_passed": bool(
                probe_readiness.get("fourth_slice_semantic_compatibility_probe_passed")
            ),
            "applied_expansion_id": applied_scope.get("expansion_id", ""),
            "source_card_count": applied_scope_report.get("card_pool_summary", {}).get("source_card_count", 0),
            "effective_series_count": applied_scope.get("effective_series_count", 0),
            "semantic_card_count": probe_summary.get("semantic_card_count", 0),
            "manual_review_card_count": probe_summary.get("manual_review_card_count", 0),
            "pair_graph_edge_count": probe_summary.get("pair_graph_edge_count", 0),
            "candidate_edge_count": probe_summary.get("candidate_edge_count", 0),
            "policy_reuse_decision": "requires_fourth_slice_fixture_scaffold",
            "new_format_or_mechanic_fixtures_required": True,
        },
        "decision": {
            "offline_recipe_pipeline_allowed": offline_recipe_pipeline_allowed,
            "fixture_scaffold_required_before_recipe_validation": True,
            "runtime_deck_promotion_allowed": False,
            "saved_deck_or_ui_publication_allowed": False,
            "bot_playbook_promotion_allowed": False,
            "recommended_next_queue": "M48 Fourth-slice offline recipe pipeline"
            if offline_recipe_pipeline_allowed
            else "Hold fourth-slice recipe pipeline",
            "reason": (
                "Fourth slice has an applied source scope plus a passed semantic/compatibility probe; offline recipe work may start with a new fixture scaffold."
                if offline_recipe_pipeline_allowed
                else "Fourth-slice recipe work is blocked until applied-scope/probe issues are repaired."
            ),
        },
        "proposed_m48_queue": [
            {
                "milestone": "M48-01",
                "task": "Fourth-slice fixture scaffold",
                "scope": "Define source-backed fixture policy for the Royal Paladin G-era expanded slice before validator work.",
            },
            {
                "milestone": "M48-02",
                "task": "Fourth-slice review packet",
                "scope": "Export candidate edges, manual-review cards, applied scope notes, and format notes for human review.",
            },
            {
                "milestone": "M48-03",
                "task": "Fourth-slice recipe draft model",
                "scope": "Create advisory recipe drafts only; no saved deck or UI injection.",
            },
            {
                "milestone": "M48-04",
                "task": "Fourth-slice recipe validator",
                "scope": "Validate count, trigger, grade, identity, copy limits, missing cards, and fixture scaffold constraints.",
            },
            {
                "milestone": "M48-05",
                "task": "Fourth-slice combo-to-recipe consistency",
                "scope": "Check candidate combo cards are present and not blocked by manual-review dependency.",
            },
            {
                "milestone": "M48-06",
                "task": "Fourth-slice blocker repair candidates",
                "scope": "Generate source-backed repair candidates for blocked recipes.",
            },
            {
                "milestone": "M48-closeout",
                "task": "Fourth-slice runtime readiness decision",
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
            "GameState_mutation": False,
        },
        "issues": issues,
        "summary": {
            "decision_ready": blocker_count == 0,
            "blocking_issue_count": blocker_count,
            "issue_count": len(issues),
            "offline_recipe_pipeline_allowed": offline_recipe_pipeline_allowed,
            "ready_for_m48": offline_recipe_pipeline_allowed,
        },
        "next_target": {
            "milestone": "M48-01" if offline_recipe_pipeline_allowed else "M47-repair-semantic",
            "task": "Fourth-slice fixture scaffold"
            if offline_recipe_pipeline_allowed
            else "Repair fourth-slice recipe pipeline entry blockers",
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
        "# M47-04 Fourth-Slice Recipe Pipeline Entry Gate",
        "",
        "## Selected Target",
        "",
        f"- Group: `{target.get('group', '')}`",
        f"- Era preset: `{target.get('era_preset', '')}`",
        f"- Applied expansion: `{evidence['applied_expansion_id']}`",
        "",
        "## Decision",
        "",
        f"- Decision ready: `{summary['decision_ready']}`",
        f"- Offline recipe pipeline allowed: `{decision['offline_recipe_pipeline_allowed']}`",
        f"- Fixture scaffold required before recipe validation: `{decision['fixture_scaffold_required_before_recipe_validation']}`",
        f"- Runtime deck promotion allowed: `{decision['runtime_deck_promotion_allowed']}`",
        f"- Saved deck/UI publication allowed: `{decision['saved_deck_or_ui_publication_allowed']}`",
        f"- Bot playbook promotion allowed: `{decision['bot_playbook_promotion_allowed']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        "",
        "## Evidence",
        "",
        f"- Source card count: `{evidence['source_card_count']}`",
        f"- Effective series count: `{evidence['effective_series_count']}`",
        f"- Semantic card count: `{evidence['semantic_card_count']}`",
        f"- Manual-review card count: `{evidence['manual_review_card_count']}`",
        f"- Pair graph edges: `{evidence['pair_graph_edge_count']}`",
        f"- Candidate edges: `{evidence['candidate_edge_count']}`",
        f"- Policy reuse decision: `{evidence['policy_reuse_decision']}`",
        "",
        "## Proposed M48 Queue",
        "",
    ]
    for item in report["proposed_m48_queue"]:
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
    parser = argparse.ArgumentParser(description="Build M47-04 fourth-slice recipe pipeline entry gate.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_fourth_slice_recipe_pipeline_entry_gate()
    json_path = args.output_dir / "m47_04_fourth_slice_recipe_pipeline_entry_gate.json"
    md_path = args.output_dir / "m47_04_fourth_slice_recipe_pipeline_entry_gate.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M47-04 fourth-slice recipe gate wrote {json_path}")
    print(f"M47-04 fourth-slice recipe gate summary wrote {md_path}")
    print(
        "ready={ready} blockers={blockers} next={next_milestone}".format(
            ready=report["summary"]["ready_for_m48"],
            blockers=report["summary"]["blocking_issue_count"],
            next_milestone=report["next_target"]["milestone"],
        )
    )
    return 0 if report["summary"]["ready_for_m48"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
