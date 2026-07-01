"""Run the third-slice semantic/compatibility probe for M43-03.

This reuses the proven M35-E3 selected-slice probe in memory after normalizing
the M43-01/M43-02 report shapes. The output stays advisory and offline: it
does not create decks, runtime fixtures, UI deck entries, or bot playbooks.
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
M43_01_SELECTION = OUTPUT_DIR / "m43_01_third_target_slice_selection.json"
M43_02_READINESS = OUTPUT_DIR / "m43_02_third_slice_fixture_readiness.json"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _normalize_selection(selection_report: dict[str, Any], readiness_report: dict[str, Any]) -> dict[str, Any]:
    normalized = copy.deepcopy(selection_report)
    selected = normalized.get("selected_target", {})
    format_scope = readiness_report.get("format_scope", {})
    normalized["format_policy"] = {
        "era_preset": selected.get("era_preset", ""),
        "group_field": selected.get("group_field", ""),
        "selected_group": selected.get("group", ""),
        "series_scope": format_scope.get("series_scope", []),
        "policy_reuse_decision": format_scope.get("policy_reuse_decision", ""),
        "new_format_or_mechanic_fixtures_required": bool(
            format_scope.get("new_format_or_mechanic_fixtures_required", False)
        ),
        "offline_probe_only": True,
    }
    return normalized


def _normalize_readiness(readiness_report: dict[str, Any]) -> dict[str, Any]:
    normalized = copy.deepcopy(readiness_report)
    selected = normalized.get("selected_target", {})
    format_scope = normalized.get("format_scope", {})
    readiness = normalized.setdefault("readiness", {})
    readiness["semantic_scaleout_ready"] = bool(
        readiness.get("semantic_probe_ready")
        or readiness.get("all_fixture_expectations_met")
    )
    readiness["classic_core_policy_reusable"] = False
    normalized["fixture_policy"] = {
        "policy_level": "third_slice_source_backed_fixture_readiness_only",
        "identity_field": selected.get("group_field", ""),
        "selected_identity": selected.get("group", ""),
        "set_scope": format_scope.get("series_scope", []),
        "policy_reuse_decision": format_scope.get("policy_reuse_decision", ""),
        "new_format_or_mechanic_fixtures_required": bool(
            format_scope.get("new_format_or_mechanic_fixtures_required", False)
        ),
        "requires_new_fixture_scaffold_before_runtime_promotion": True,
    }
    return normalized


def _stage_summary(report: dict[str, Any], stage: str, key: str, default: Any = 0) -> Any:
    return report.get("stage_summaries", {}).get(stage, {}).get(key, default)


def build_third_slice_semantic_compatibility_probe(
    selection_report: dict[str, Any] | None = None,
    readiness_report: dict[str, Any] | None = None,
    selection_path: Path = M43_01_SELECTION,
    readiness_path: Path = M43_02_READINESS,
) -> dict[str, Any]:
    selection_report = selection_report or load_json(selection_path)
    readiness_report = readiness_report or load_json(readiness_path)
    normalized_selection = _normalize_selection(selection_report, readiness_report)
    normalized_readiness = _normalize_readiness(readiness_report)
    legacy_probe = build_probe(
        normalized_selection,
        normalized_readiness,
        selected_report_path=selection_path,
        readiness_report_path=readiness_path,
    )
    stage_readiness = dict(legacy_probe["stage_readiness"])
    all_ready = all(stage_readiness.values())

    return {
        "version": "M43-03",
        "description": "Third-slice semantic/compatibility probe",
        "selected_target": normalized_selection["selected_target"],
        "source_inputs": {
            "third_target_slice_selection": str(selection_path.relative_to(ROOT)),
            "third_slice_fixture_readiness": str(readiness_path.relative_to(ROOT)),
        },
        "normalization": {
            "uses_m35_b_c_probe_in_memory": True,
            "adds_format_policy_for_contract": True,
            "adds_fixture_policy_for_contract": True,
            "maps_m43_readiness_to_semantic_scaleout_ready": True,
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
            "third_slice_semantic_compatibility_probe_passed": all_ready,
            "ready_for_m43_04": all_ready,
            "runtime_or_bot_promotion_allowed": False,
        },
        "summary": {
            "selected_group": normalized_selection["selected_target"].get("group", ""),
            "era_preset": normalized_selection["selected_target"].get("era_preset", ""),
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
            "milestone": "M43-04" if all_ready else "M43-repair",
            "task": "Third-slice recipe pipeline entry gate"
            if all_ready
            else "Repair third-slice semantic/compatibility blockers",
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
        "# M43-03 Third-Slice Semantic / Compatibility Probe",
        "",
        "## Selected Target",
        "",
        f"- Group: `{target.get('group', '')}`",
        f"- Era preset: `{target.get('era_preset', '')}`",
        f"- Rank: `{target.get('rank', '')}`",
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
    parser = argparse.ArgumentParser(description="Build M43-03 third-slice semantic/compatibility probe.")
    parser.add_argument("--selection-report", type=Path, default=M43_01_SELECTION)
    parser.add_argument("--readiness-report", type=Path, default=M43_02_READINESS)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    selection_report = load_json(args.selection_report)
    readiness_report = load_json(args.readiness_report)
    report = build_third_slice_semantic_compatibility_probe(
        selection_report,
        readiness_report,
        args.selection_report,
        args.readiness_report,
    )
    json_path = args.output_dir / "m43_03_third_slice_semantic_compatibility_probe.json"
    md_path = args.output_dir / "m43_03_third_slice_semantic_compatibility_probe.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M43-03 third-slice semantic probe wrote {json_path}")
    print(f"M43-03 third-slice semantic probe summary wrote {md_path}")
    print(
        "ready={ready} cards={cards} edges={edges} candidates={candidates}".format(
            ready=report["readiness"]["ready_for_m43_04"],
            cards=report["summary"]["semantic_card_count"],
            edges=report["summary"]["pair_graph_edge_count"],
            candidates=report["summary"]["candidate_edge_count"],
        )
    )
    return 0 if report["readiness"]["ready_for_m43_04"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
