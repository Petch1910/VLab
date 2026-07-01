"""Build the M48-01 fourth-slice fixture scaffold artifact."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M47_04_GATE = OUTPUT_DIR / "m47_04_fourth_slice_recipe_pipeline_entry_gate.json"
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


def build_fourth_slice_fixture_scaffold(
    gate_report: dict[str, Any] | None = None,
    applied_scope_report: dict[str, Any] | None = None,
    semantic_probe: dict[str, Any] | None = None,
) -> dict[str, Any]:
    gate_report = gate_report or load_json(M47_04_GATE)
    applied_scope_report = applied_scope_report or load_json(M47_APPLIED_SCOPE)
    semantic_probe = semantic_probe or load_json(M47_03_PROBE)
    selected = gate_report.get("selected_target", applied_scope_report.get("selected_target", {}))
    evidence = gate_report.get("evidence_summary", {})
    card_pool = applied_scope_report.get("card_pool_summary", {})
    applied_scope = applied_scope_report.get("applied_scope", {})
    probe_summary = semantic_probe.get("summary", {})
    issues: list[dict[str, Any]] = []

    if not gate_report.get("summary", {}).get("ready_for_m48"):
        issues.append(
            _issue(
                "entry_gate_not_ready",
                "blocker",
                "M47-04 must allow the offline M48 pipeline before scaffolding fixtures.",
                {"summary": gate_report.get("summary", {})},
            )
        )
    if not evidence.get("new_format_or_mechanic_fixtures_required"):
        issues.append(
            _issue(
                "unexpected_classic_policy_reuse",
                "warning",
                "M48-01 is intended for a fourth slice that requires a new fixture scaffold.",
                {"evidence": evidence},
            )
        )
    if int(card_pool.get("source_card_count", 0)) < 50:
        issues.append(
            _issue(
                "insufficient_source_cards",
                "blocker",
                "The selected slice must have at least 50 source-backed cards for fixture scaffolding.",
                {"card_pool_summary": card_pool},
            )
        )
    trigger_counts = card_pool.get("trigger_counts", {})
    missing_triggers = [
        trigger
        for trigger in ["Critical", "Draw", "Heal", "Stand"]
        if int(trigger_counts.get(trigger, 0)) <= 0
    ]
    if missing_triggers:
        issues.append(
            _issue(
                "missing_required_trigger_types",
                "blocker",
                "The scaffold requires all four classic trigger types to be present.",
                {"missing_triggers": missing_triggers},
            )
        )

    blocker_count = sum(1 for issue in issues if issue["severity"] == "blocker")
    scaffold_ready = blocker_count == 0
    series_counts = card_pool.get("series_counts", {})

    return {
        "version": "M48-01",
        "description": "Fourth-slice fixture scaffold",
        "source_inputs": {
            "fourth_slice_recipe_pipeline_entry_gate": str(M47_04_GATE.relative_to(ROOT)),
            "fourth_slice_applied_scope": str(M47_APPLIED_SCOPE.relative_to(ROOT)),
            "fourth_slice_semantic_compatibility_probe": str(M47_03_PROBE.relative_to(ROOT)),
        },
        "selected_target": selected,
        "fixture_scaffold": {
            "policy_level": "fourth_slice_g_era_expanded_scope_fixture_scaffold_not_full_official_legality",
            "identity_field": selected.get("group_field", "clan"),
            "selected_identity": selected.get("group", ""),
            "era_preset": selected.get("era_preset", ""),
            "applied_expansion_id": applied_scope.get("expansion_id", ""),
            "set_scope": applied_scope.get("effective_series_scope", []),
            "base_series_scope": applied_scope.get("base_series_scope", []),
            "added_series": applied_scope.get("added_series", []),
            "source_series_present": sorted(series_counts),
            "main_deck_exact": 50,
            "trigger_target": 16,
            "required_trigger_types": ["Critical", "Draw", "Heal", "Stand"],
            "recommended_trigger_profile": {
                "Critical": 4,
                "Draw": 4,
                "Heal": 4,
                "Stand": 4,
            },
            "required_non_trigger_setup_grades": [0, 1, 2, 3],
            "preferred_grade_profile": {
                "0": 17,
                "1": 14,
                "2": 11,
                "3": 8,
            },
            "copy_limit_source": "runtime SQLite cards.deck_limit",
            "deferred_source_backed_special_limits": [
                "full official heal-trigger maximum beyond runtime deck_limit",
                "card-specific special limits not represented in runtime SQLite",
                "format-wide exceptions not represented in local source data",
                "G Zone and stride deck-building limits",
            ],
        },
        "mechanic_scope": {
            "format_family": "G Series / Stride era",
            "runtime_stride_enabled": False,
            "g_zone_recipe_validation_deferred": True,
            "grade4_cards_present": int(card_pool.get("grade_counts", {}).get("4", 0)) > 0,
            "grade4_cards_advisory_only_until_g_zone_support": True,
            "stride_or_generation_break_text_requires_manual_review": True,
            "imaginary_gift_enabled": False,
            "ride_deck_enabled": False,
            "over_trigger_enabled": False,
            "front_trigger_enabled": False,
            "order_cards_enabled": False,
            "notes": [
                "This scaffold is a validator policy for offline recipes, not a full official ruleset.",
                "G units and Stride/Generation Break text stay manual-review until dedicated G-zone rules exist.",
            ],
        },
        "validator_contract_for_m48_04": {
            "must_validate_main_deck_count": True,
            "must_validate_trigger_count": True,
            "must_validate_required_trigger_types": True,
            "must_validate_required_grade_coverage": True,
            "must_validate_selected_identity": True,
            "must_validate_set_scope": True,
            "must_validate_runtime_deck_limit": True,
            "must_block_missing_cards": True,
            "must_keep_grade4_cards_out_of_main_deck_until_g_zone_support": True,
            "must_not_accept_manual_review_dependencies_as_runtime_ready": True,
        },
        "evidence_summary": {
            "source_card_count": card_pool.get("source_card_count", 0),
            "series_counts": series_counts,
            "grade_counts": card_pool.get("grade_counts", {}),
            "trigger_counts": trigger_counts,
            "trigger_capacity": card_pool.get("trigger_capacity", 0),
            "non_trigger_capacity": card_pool.get("non_trigger_capacity", 0),
            "candidate_edge_count": probe_summary.get("candidate_edge_count", 0),
            "manual_review_card_count": probe_summary.get("manual_review_card_count", 0),
        },
        "runtime_boundary": {
            "scaffold_artifact_only": True,
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
            "scaffold_ready": scaffold_ready,
            "blocking_issue_count": blocker_count,
            "issue_count": len(issues),
            "ready_for_m48_02": scaffold_ready,
        },
        "next_target": {
            "milestone": "M48-02" if scaffold_ready else "M48-repair",
            "task": "Fourth-slice review packet"
            if scaffold_ready
            else "Repair fourth-slice fixture scaffold blockers",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    selected = report["selected_target"]
    scaffold = report["fixture_scaffold"]
    summary = report["summary"]
    evidence = report["evidence_summary"]
    lines = [
        "# M48-01 Fourth-Slice Fixture Scaffold",
        "",
        "## Selected Target",
        "",
        f"- Group: `{selected.get('group', '')}`",
        f"- Era preset: `{selected.get('era_preset', '')}`",
        f"- Applied expansion: `{scaffold['applied_expansion_id']}`",
        "",
        "## Scaffold",
        "",
        f"- Policy level: `{scaffold['policy_level']}`",
        f"- Main deck exact: `{scaffold['main_deck_exact']}`",
        f"- Trigger target: `{scaffold['trigger_target']}`",
        f"- Required trigger types: `{scaffold['required_trigger_types']}`",
        f"- Recommended trigger profile: `{scaffold['recommended_trigger_profile']}`",
        f"- Required setup grades: `{scaffold['required_non_trigger_setup_grades']}`",
        f"- Preferred grade profile: `{scaffold['preferred_grade_profile']}`",
        f"- Copy limit source: `{scaffold['copy_limit_source']}`",
        "",
        "## Evidence",
        "",
        f"- Source cards: `{evidence['source_card_count']}`",
        f"- Series counts: `{evidence['series_counts']}`",
        f"- Grade counts: `{evidence['grade_counts']}`",
        f"- Trigger counts: `{evidence['trigger_counts']}`",
        f"- Candidate edges: `{evidence['candidate_edge_count']}`",
        f"- Manual-review cards: `{evidence['manual_review_card_count']}`",
        "",
        "## Summary",
        "",
        f"- Scaffold ready: `{summary['scaffold_ready']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        f"- Ready for M48-02: `{summary['ready_for_m48_02']}`",
        "",
        "## Boundary",
        "",
    ]
    for key, value in report["runtime_boundary"].items():
        lines.append(f"- `{key}`: `{value}`")
    lines.extend(["", "## Next", "", f"`{report['next_target']['milestone']}`: {report['next_target']['task']}.", ""])
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M48-01 fourth-slice fixture scaffold.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_fourth_slice_fixture_scaffold()
    json_path = args.output_dir / "m48_01_fourth_slice_fixture_scaffold.json"
    md_path = args.output_dir / "m48_01_fourth_slice_fixture_scaffold.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M48-01 fourth-slice fixture scaffold wrote {json_path}")
    print(f"M48-01 fourth-slice fixture scaffold summary wrote {md_path}")
    print(
        "ready={ready} blockers={blockers} next={next_milestone}".format(
            ready=report["summary"]["ready_for_m48_02"],
            blockers=report["summary"]["blocking_issue_count"],
            next_milestone=report["next_target"]["milestone"],
        )
    )
    return 0 if report["summary"]["ready_for_m48_02"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
