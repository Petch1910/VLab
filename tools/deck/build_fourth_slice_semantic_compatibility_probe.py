"""Run the fourth-slice semantic/compatibility probe for M47-03.

This reuses the generalized M35 B/C probe in memory after normalizing the
M47 applied-scope artifact. The probe remains advisory and offline.
"""

from __future__ import annotations

import argparse
import copy
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_selected_slice_semantic_compatibility_probe import build_probe  # noqa: E402


OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M47_01_SELECTION = OUTPUT_DIR / "m47_01_fourth_target_slice_selection.json"
M47_APPLIED_SCOPE = OUTPUT_DIR / "m47_repair_apply_scope.json"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _normalize_selection(selection_report: dict[str, Any], applied_scope_report: dict[str, Any]) -> dict[str, Any]:
    normalized = copy.deepcopy(selection_report)
    selected = normalized.get("selected_target", {})
    applied_scope = applied_scope_report.get("applied_scope", {})
    normalized["format_policy"] = {
        "era_preset": selected.get("era_preset", ""),
        "group_field": selected.get("group_field", ""),
        "selected_group": selected.get("group", ""),
        "series_scope": applied_scope.get("effective_series_scope", []),
        "base_series_scope": applied_scope.get("base_series_scope", []),
        "added_series": applied_scope.get("added_series", []),
        "applied_expansion_id": applied_scope.get("expansion_id", ""),
        "policy_reuse_decision": "requires_fourth_slice_fixture_scaffold",
        "new_format_or_mechanic_fixtures_required": True,
        "offline_probe_only": True,
    }
    return normalized


def _normalize_readiness(applied_scope_report: dict[str, Any]) -> dict[str, Any]:
    normalized = copy.deepcopy(applied_scope_report)
    selected = normalized.get("selected_target", {})
    applied_scope = normalized.get("applied_scope", {})
    readiness = normalized.setdefault("readiness", {})
    readiness["semantic_scaleout_ready"] = bool(
        readiness.get("ready_for_m47_03")
        or readiness.get("semantic_probe_ready")
        or readiness.get("all_fixture_expectations_met")
    )
    readiness["classic_core_policy_reusable"] = False
    normalized["format_scope"] = {
        "era_preset": selected.get("era_preset", ""),
        "series_scope": applied_scope.get("effective_series_scope", []),
        "base_series_scope": applied_scope.get("base_series_scope", []),
        "added_series": applied_scope.get("added_series", []),
        "policy_reuse_decision": "requires_fourth_slice_fixture_scaffold",
        "new_format_or_mechanic_fixtures_required": True,
    }
    normalized["fixture_policy"] = {
        "policy_level": "fourth_slice_applied_source_scope_advisory_only",
        "identity_field": selected.get("group_field", ""),
        "selected_identity": selected.get("group", ""),
        "set_scope": applied_scope.get("effective_series_scope", []),
        "applied_expansion_id": applied_scope.get("expansion_id", ""),
        "policy_reuse_decision": "requires_fourth_slice_fixture_scaffold",
        "new_format_or_mechanic_fixtures_required": True,
        "requires_new_fixture_scaffold_before_runtime_promotion": True,
    }
    return normalized


def _stage_summary(report: dict[str, Any], stage: str, key: str, default: Any = 0) -> Any:
    return report.get("stage_summaries", {}).get(stage, {}).get(key, default)


def build_fourth_slice_semantic_compatibility_probe(
    selection_report: dict[str, Any] | None = None,
    applied_scope_report: dict[str, Any] | None = None,
    selection_path: Path = M47_01_SELECTION,
    applied_scope_path: Path = M47_APPLIED_SCOPE,
) -> dict[str, Any]:
    selection_report = selection_report or load_json(selection_path)
    applied_scope_report = applied_scope_report or load_json(applied_scope_path)
    normalized_selection = _normalize_selection(selection_report, applied_scope_report)
    normalized_readiness = _normalize_readiness(applied_scope_report)
    legacy_probe = build_probe(
        normalized_selection,
        normalized_readiness,
        selected_report_path=selection_path,
        readiness_report_path=applied_scope_path,
    )
    stage_readiness = dict(legacy_probe["stage_readiness"])
    all_ready = bool(applied_scope_report.get("readiness", {}).get("ready_for_m47_03")) and all(
        stage_readiness.values()
    )

    return {
        "version": "M47-03",
        "description": "Fourth-slice semantic/compatibility probe",
        "selected_target": normalized_selection["selected_target"],
        "source_inputs": {
            "fourth_target_slice_selection": str(selection_path.relative_to(ROOT)),
            "fourth_slice_applied_scope": str(applied_scope_path.relative_to(ROOT)),
        },
        "normalization": {
            "uses_m35_b_c_probe_in_memory": True,
            "uses_m47_applied_scope_artifact": True,
            "adds_format_policy_for_contract": True,
            "adds_fixture_policy_for_contract": True,
            "maps_m47_applied_scope_to_semantic_scaleout_ready": True,
            "does_not_write_intermediate_m35_outputs": True,
        },
        "pipeline_contract": legacy_probe["pipeline_contract"],
        "stage_readiness": stage_readiness,
        "stage_summaries": legacy_probe["stage_summaries"],
        "runtime_boundary": {
            "advisory_probe_only": True,
            "does_not_create_deck": True,
            "does_not_create_runtime_fixture": True,
            "does_not_mutate_runtime_pack": True,
            "does_not_publish_to_ui": True,
            "does_not_publish_to_bot": True,
            "does_not_publish_playbook_seed": True,
            "GameState_mutation": False,
        },
        "readiness": {
            "fourth_slice_semantic_compatibility_probe_passed": all_ready,
            "ready_for_m47_04": all_ready,
            "runtime_or_bot_promotion_allowed": False,
        },
        "summary": {
            "selected_group": normalized_selection["selected_target"].get("group", ""),
            "era_preset": normalized_selection["selected_target"].get("era_preset", ""),
            "applied_expansion_id": applied_scope_report.get("applied_scope", {}).get("expansion_id", ""),
            "source_card_count": applied_scope_report.get("card_pool_summary", {}).get("source_card_count", 0),
            "effective_series_count": applied_scope_report.get("applied_scope", {}).get("effective_series_count", 0),
            "semantic_card_count": _stage_summary(legacy_probe, "b2_semantic_tags", "card_count"),
            "manual_review_card_count": _stage_summary(
                legacy_probe, "b4_manual_review_queue", "manual_review_count"
            ),
            "pair_graph_edge_count": _stage_summary(legacy_probe, "c1_pair_graph", "edge_count"),
            "candidate_edge_count": _stage_summary(
                legacy_probe, "c5_compatibility", "m35_d1_candidate_edge_count"
            ),
            "all_stage_readiness_passed": all_ready,
        },
        "next_target": {
            "milestone": "M47-04" if all_ready else "M47-repair-semantic",
            "task": "Fourth-slice recipe pipeline entry gate"
            if all_ready
            else "Repair fourth-slice semantic/compatibility blockers",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    summary = report["summary"]
    readiness = report["readiness"]
    lines = [
        "# M47-03 Fourth-Slice Semantic / Compatibility Probe",
        "",
        "## Selected Target",
        "",
        f"- Group: `{target.get('group', '')}`",
        f"- Era preset: `{target.get('era_preset', '')}`",
        f"- Applied expansion: `{summary['applied_expansion_id']}`",
        f"- Source cards: `{summary['source_card_count']}`",
        f"- Effective series count: `{summary['effective_series_count']}`",
        "",
        "## Summary",
        "",
        f"- Semantic cards: `{summary['semantic_card_count']}`",
        f"- Manual-review cards: `{summary['manual_review_card_count']}`",
        f"- Pair graph edges: `{summary['pair_graph_edge_count']}`",
        f"- Candidate edges: `{summary['candidate_edge_count']}`",
        f"- All stage readiness passed: `{summary['all_stage_readiness_passed']}`",
        f"- Runtime/bot promotion allowed: `{readiness['runtime_or_bot_promotion_allowed']}`",
        "",
        "## Stage Readiness",
        "",
    ]
    for key, value in report["stage_readiness"].items():
        lines.append(f"- `{key}`: `{value}`")
    lines.extend(
        [
            "",
            "## Boundary",
            "",
            "- Advisory offline probe only.",
            "- Does not create a deck, runtime fixture, UI deck entry, or bot playbook.",
            "- Does not mutate runtime pack or GameState.",
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
    parser = argparse.ArgumentParser(description="Build M47-03 fourth-slice semantic/compatibility probe.")
    parser.add_argument("--selection-report", type=Path, default=M47_01_SELECTION)
    parser.add_argument("--applied-scope-report", type=Path, default=M47_APPLIED_SCOPE)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    selection_report = load_json(args.selection_report)
    applied_scope_report = load_json(args.applied_scope_report)
    report = build_fourth_slice_semantic_compatibility_probe(
        selection_report,
        applied_scope_report,
        args.selection_report,
        args.applied_scope_report,
    )
    json_path = args.output_dir / "m47_03_fourth_slice_semantic_compatibility_probe.json"
    md_path = args.output_dir / "m47_03_fourth_slice_semantic_compatibility_probe.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M47-03 fourth-slice semantic probe wrote {json_path}")
    print(f"M47-03 fourth-slice semantic probe summary wrote {md_path}")
    print(
        "ready={ready} cards={cards} edges={edges} candidates={candidates} next={next_target}".format(
            ready=report["readiness"]["ready_for_m47_04"],
            cards=report["summary"]["semantic_card_count"],
            edges=report["summary"]["pair_graph_edge_count"],
            candidates=report["summary"]["candidate_edge_count"],
            next_target=report["next_target"]["milestone"],
        )
    )
    return 0 if report["readiness"]["ready_for_m47_04"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
