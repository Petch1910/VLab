"""Build the M68-01 ninth-slice fixture scaffold artifact."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M67_04_GATE = OUTPUT_DIR / "m67_04_ninth_slice_recipe_pipeline_entry_gate.json"
M67_02_READINESS = OUTPUT_DIR / "m67_02_ninth_slice_fixture_readiness.json"
M67_03_PROBE = OUTPUT_DIR / "m67_03_ninth_slice_semantic_compatibility_probe.json"
JSON_OUTPUT = OUTPUT_DIR / "m68_01_ninth_slice_fixture_scaffold.json"
MD_OUTPUT = OUTPUT_DIR / "m68_01_ninth_slice_fixture_scaffold.md"


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


def build_ninth_slice_fixture_scaffold(
    gate_report: dict[str, Any] | None = None,
    readiness_report: dict[str, Any] | None = None,
    semantic_probe: dict[str, Any] | None = None,
) -> dict[str, Any]:
    gate_report = gate_report or load_json(M67_04_GATE)
    readiness_report = readiness_report or load_json(M67_02_READINESS)
    semantic_probe = semantic_probe or load_json(M67_03_PROBE)
    selected = gate_report.get("selected_target", readiness_report.get("selected_target", {}))
    gate_summary = gate_report.get("summary", {})
    gate_evidence = gate_report.get("evidence_summary", {})
    card_pool = readiness_report.get("card_pool_summary", {})
    format_scope = readiness_report.get("format_scope", {})
    probe_summary = semantic_probe.get("summary", {})
    issues: list[dict[str, Any]] = []

    if not gate_summary.get("ready_for_m68"):
        issues.append(
            _issue(
                "entry_gate_not_ready",
                "blocker",
                "M67-04 must allow the offline M68 pipeline before scaffolding fixtures.",
                {"summary": gate_summary},
            )
        )
    if not gate_evidence.get("new_format_or_mechanic_fixtures_required"):
        issues.append(
            _issue(
                "unexpected_policy_reuse",
                "warning",
                "M68-01 is intended for a ninth slice that requires a new fixture scaffold.",
                {"evidence": gate_evidence},
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
                "The scaffold requires all four G-era trigger families to be present.",
                {"missing_triggers": missing_triggers},
            )
        )

    grade_counts = card_pool.get("grade_counts", {})
    g_zone_boundary_required = bool(format_scope.get("g_zone_fixture_boundary_required"))
    if g_zone_boundary_required and int(grade_counts.get("4", 0)) <= 0:
        issues.append(
            _issue(
                "missing_g_unit_pool",
                "blocker",
                "The G-series fixture scaffold requires a source-backed Grade 4/G-unit pool for review.",
                {"grade_counts": grade_counts},
            )
        )

    blocker_count = sum(1 for issue in issues if issue["severity"] == "blocker")
    scaffold_ready = blocker_count == 0
    series_counts = card_pool.get("series_counts", {})

    return {
        "version": "M68-01",
        "description": "Ninth-slice fixture scaffold",
        "source_inputs": {
            "ninth_slice_recipe_pipeline_entry_gate": str(M67_04_GATE.relative_to(ROOT)),
            "ninth_slice_fixture_readiness": str(M67_02_READINESS.relative_to(ROOT)),
            "ninth_slice_semantic_compatibility_probe": str(M67_03_PROBE.relative_to(ROOT)),
        },
        "selected_target": selected,
        "fixture_scaffold": {
            "policy_level": "ninth_slice_aquaforce_g_series_fixture_scaffold_not_full_official_legality",
            "identity_field": selected.get("group_field", "clan"),
            "selected_identity": selected.get("group", ""),
            "era_preset": selected.get("era_preset", ""),
            "set_specs": format_scope.get("set_specs", []),
            "set_scope": format_scope.get("series_scope", []),
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
            "g_zone_fixture_boundary_required": g_zone_boundary_required,
            "grade4_source_cards_reserved_for_g_zone_review": True,
            "deferred_source_backed_special_limits": [
                "full official heal-trigger maximum beyond runtime deck_limit",
                "card-specific special limits not represented in runtime SQLite",
                "format-wide exceptions not represented in local source data",
                "G Zone size and face-up/face-down state constraints",
                "Stride cost, heart card, generation break, and G guardian constraints",
                "Aqua Force attack-count and battle-order timing constraints not represented in local source data",
            ],
        },
        "mechanic_scope": {
            "format_family": "G-series first / Stride era",
            "runtime_g_zone_enabled": False,
            "runtime_stride_enabled": False,
            "g_zone_fixture_boundary_required": g_zone_boundary_required,
            "stride_or_g_unit_text_requires_manual_review": True,
            "generation_break_text_requires_manual_review": True,
            "aqua_force_attack_count_text_requires_manual_review": True,
            "grade4_cards_present": int(grade_counts.get("4", 0)) > 0,
            "grade4_cards_g_zone_advisory_only_until_format_support": True,
            "imaginary_gift_enabled": False,
            "ride_deck_enabled": False,
            "over_trigger_enabled": False,
            "front_trigger_enabled": False,
            "order_cards_enabled": False,
            "bloom_token_runtime_enabled": False,
            "lock_runtime_enabled": False,
            "unlock_runtime_enabled": False,
            "legion_runtime_enabled": False,
            "mate_identity_check_enabled": False,
            "notes": [
                "This scaffold is a validator policy for offline recipes, not a full official ruleset.",
                "Grade 4/G-unit cards stay out of main-deck recipes until dedicated G Zone and Stride rules modules exist.",
                "Aqua Force battle-count text stays manual-review until battle-order and timing modules can validate it.",
            ],
        },
        "validator_contract_for_m68_04": {
            "must_validate_main_deck_count": True,
            "must_validate_trigger_count": True,
            "must_validate_required_trigger_types": True,
            "must_validate_required_grade_coverage": True,
            "must_validate_selected_identity": True,
            "must_validate_set_scope": True,
            "must_validate_runtime_deck_limit": True,
            "must_block_missing_cards": True,
            "must_keep_grade4_cards_out_of_main_deck_until_format_support": True,
            "must_keep_g_zone_cards_in_review_boundary": True,
            "must_not_accept_manual_review_dependencies_as_runtime_ready": True,
        },
        "evidence_summary": {
            "source_card_count": card_pool.get("source_card_count", 0),
            "series_counts": series_counts,
            "grade_counts": grade_counts,
            "trigger_counts": trigger_counts,
            "trigger_capacity": card_pool.get("trigger_capacity", 0),
            "non_trigger_capacity": card_pool.get("non_trigger_capacity", 0),
            "candidate_edge_count": probe_summary.get("candidate_edge_count", 0),
            "manual_review_card_count": probe_summary.get("manual_review_card_count", 0),
            "g_zone_fixture_boundary_required": g_zone_boundary_required,
        },
        "runtime_boundary": {
            "scaffold_artifact_only": True,
            "does_not_create_deck": True,
            "does_not_create_recipe_draft": True,
            "does_not_create_runtime_fixture": True,
            "does_not_mutate_runtime_pack": True,
            "does_not_publish_to_ui": True,
            "does_not_publish_to_bot": True,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "bloom_token_runtime_enabled": False,
            "lock_runtime_enabled": False,
            "unlock_runtime_enabled": False,
            "legion_runtime_enabled": False,
            "mate_identity_check_enabled": False,
            "GameState_mutation": False,
        },
        "issues": issues,
        "summary": {
            "scaffold_ready": scaffold_ready,
            "blocking_issue_count": blocker_count,
            "issue_count": len(issues),
            "ready_for_m68_02": scaffold_ready,
        },
        "next_target": {
            "milestone": "M68-02" if scaffold_ready else "M68-repair",
            "task": "Ninth-slice review packet"
            if scaffold_ready
            else "Repair ninth-slice fixture scaffold blockers",
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
        "# M68-01 Ninth-Slice Fixture Scaffold",
        "",
        "## Selected Target",
        "",
        f"- Group: `{selected.get('group', '')}`",
        f"- Era preset: `{selected.get('era_preset', '')}`",
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
        f"- G Zone fixture boundary required: `{scaffold['g_zone_fixture_boundary_required']}`",
        f"- Copy limit source: `{scaffold['copy_limit_source']}`",
        "",
        "## Mechanic Scope",
        "",
    ]
    for key, value in report["mechanic_scope"].items():
        if key == "notes":
            continue
        lines.append(f"- `{key}`: `{value}`")
    lines.extend(
        [
            "",
            "## Evidence",
            "",
            f"- Source cards: `{evidence['source_card_count']}`",
            f"- Series counts: `{evidence['series_counts']}`",
            f"- Grade counts: `{evidence['grade_counts']}`",
            f"- Trigger counts: `{evidence['trigger_counts']}`",
            f"- Candidate edges: `{evidence['candidate_edge_count']}`",
            f"- Manual-review cards: `{evidence['manual_review_card_count']}`",
            f"- G Zone boundary required: `{evidence['g_zone_fixture_boundary_required']}`",
            "",
            "## Summary",
            "",
            f"- Scaffold ready: `{summary['scaffold_ready']}`",
            f"- Blocking issues: `{summary['blocking_issue_count']}`",
            f"- Ready for M68-02: `{summary['ready_for_m68_02']}`",
            "",
            "## Boundary",
            "",
        ]
    )
    for key, value in report["runtime_boundary"].items():
        lines.append(f"- `{key}`: `{value}`")
    lines.extend(["", "## Next", "", f"`{report['next_target']['milestone']}`: {report['next_target']['task']}.", ""])
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M68-01 ninth-slice fixture scaffold.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_ninth_slice_fixture_scaffold()
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M68-01 ninth-slice fixture scaffold wrote {json_path}")
    print(f"M68-01 ninth-slice fixture scaffold summary wrote {md_path}")
    print(
        "ready={ready} blockers={blockers} next={next_milestone}".format(
            ready=report["summary"]["ready_for_m68_02"],
            blockers=report["summary"]["blocking_issue_count"],
            next_milestone=report["next_target"]["milestone"],
        )
    )
    return 0 if report["summary"]["ready_for_m68_02"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
