"""Run a generalized selected-slice semantic/compatibility probe for M35-E3.

This validates that the M35 B/C tooling can be driven from an injected
selected-slice report instead of the original first-slice file path. It keeps
the run offline and advisory: no runtime card data mutation, no player deck
creation, and no bot/playbook publication.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_first_slice_manual_review_queue import build_queue  # noqa: E402
from tools.deck.build_first_slice_pair_compatibility_graph import build_report as build_pair_graph  # noqa: E402
from tools.deck.build_first_slice_requirement_provider_model import build_report as build_requirement_provider  # noqa: E402
from tools.deck.build_first_slice_resource_conflict_detector import build_report as build_resource_detector  # noqa: E402
from tools.deck.build_first_slice_selected_compatibility_output import build_report as build_compatibility_output  # noqa: E402
from tools.deck.build_first_slice_semantic_vocabulary import build_vocabulary  # noqa: E402
from tools.deck.build_first_slice_timing_compatibility_detector import build_report as build_timing_detector  # noqa: E402
from tools.deck.build_first_slice_zone_target_detector import build_report as build_zone_detector  # noqa: E402
from tools.deck.extract_first_slice_semantics import build_report as build_semantic_tags  # noqa: E402


OUTPUT_DIR = ROOT / "outputs" / "target_slice"
SECOND_SLICE_REPORT = OUTPUT_DIR / "m35_e1_second_target_slice_report.json"
SECOND_SLICE_READINESS = OUTPUT_DIR / "m35_e2_second_slice_fixture_readiness.json"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _stage_summary(report: dict[str, Any], keys: Sequence[str]) -> dict[str, Any]:
    summary = report.get("summary", {})
    return {key: summary.get(key) for key in keys}


def build_probe(
    selected_report: dict[str, Any] | None = None,
    readiness_report: dict[str, Any] | None = None,
    selected_report_path: Path = SECOND_SLICE_REPORT,
    readiness_report_path: Path = SECOND_SLICE_READINESS,
) -> dict[str, Any]:
    selected_report = selected_report or load_json(selected_report_path)
    readiness_report = readiness_report or load_json(readiness_report_path)

    vocabulary = build_vocabulary(readiness_report)
    semantic_report = build_semantic_tags(selected_report, vocabulary)
    provider_report = build_requirement_provider(semantic_report)
    manual_review_report = build_queue(semantic_report, provider_report)
    pair_graph_report = build_pair_graph(provider_report, manual_review_report)
    resource_report = build_resource_detector(pair_graph_report)
    timing_report = build_timing_detector(pair_graph_report, resource_report)
    zone_report = build_zone_detector(pair_graph_report, resource_report, timing_report)
    compatibility_report = build_compatibility_output(
        pair_graph_report,
        resource_report,
        timing_report,
        zone_report,
    )

    stage_readiness = {
        "b1_vocabulary_ready": bool(vocabulary.get("ready_for_m35_b2")),
        "b2_semantics_ready": bool(semantic_report.get("ready_for_m35_b3")),
        "b3_provider_ready": bool(provider_report.get("ready_for_m35_b4")),
        "b4_phase_c_ready": bool(manual_review_report.get("summary", {}).get("ready_for_phase_c")),
        "c1_resource_ready": bool(pair_graph_report.get("summary", {}).get("ready_for_m35_c2")),
        "c2_timing_ready": bool(resource_report.get("summary", {}).get("ready_for_m35_c3")),
        "c3_zone_ready": bool(timing_report.get("summary", {}).get("ready_for_m35_c4")),
        "c4_output_ready": bool(zone_report.get("summary", {}).get("ready_for_m35_c5")),
        "c5_package_selection_ready": bool(compatibility_report.get("summary", {}).get("ready_for_m35_d1")),
    }
    all_ready = all(stage_readiness.values())

    return {
        "version": "M35-E3",
        "description": "Generalized selected-slice semantic/compatibility probe",
        "selected_target": selected_report["selected_target"],
        "source_inputs": {
            "selected_report": str(selected_report_path.relative_to(ROOT)),
            "readiness_report": str(readiness_report_path.relative_to(ROOT)),
        },
        "pipeline_contract": {
            "selected_report_required_keys": [
                "selected_target",
                "format_policy",
            ],
            "readiness_report_required_keys": [
                "selected_target",
                "fixture_policy",
                "readiness",
            ],
            "stage_order": [
                "B1 semantic vocabulary",
                "B2 semantic tags",
                "B3 requirement/provider model",
                "B4 manual review queue",
                "C1 pair compatibility graph",
                "C2 resource detector",
                "C3 timing detector",
                "C4 zone/target detector",
                "C5 selected compatibility output",
            ],
            "uses_injected_reports_in_memory": True,
            "does_not_require_first_slice_file_paths_for_execution": True,
        },
        "stage_readiness": stage_readiness,
        "stage_summaries": {
            "b1_vocabulary": {
                "ability_types": len(vocabulary["vocabulary"]["ability_types"]),
                "effects": len(vocabulary["vocabulary"]["effects"]),
                "missing_source_terms": len(vocabulary["missing_source_terms"]),
            },
            "b2_semantic_tags": _stage_summary(
                semantic_report,
                ["card_count", "cards_with_any_semantic_tag", "manual_review_count"],
            ),
            "b3_requirement_provider": _stage_summary(
                provider_report,
                [
                    "card_count",
                    "cards_with_requirements",
                    "cards_with_providers",
                    "manual_review_count",
                    "provider_type_count",
                    "requirement_type_count",
                ],
            ),
            "b4_manual_review_queue": _stage_summary(
                manual_review_report,
                ["card_count", "manual_review_count", "ready_for_phase_c"],
            ),
            "c1_pair_graph": _stage_summary(
                pair_graph_report,
                [
                    "node_count",
                    "edge_count",
                    "manual_review_card_count",
                    "manual_review_edge_count",
                    "advisory_edge_count",
                    "ready_for_m35_c2",
                ],
            ),
            "c2_resource": _stage_summary(
                resource_report,
                [
                    "node_count",
                    "source_edge_count",
                    "resource_relevant_edge_count",
                    "manual_review_resource_edge_count",
                    "missing_recovery_resource_count",
                    "ready_for_m35_c3",
                ],
            ),
            "c3_timing": _stage_summary(
                timing_report,
                [
                    "node_count",
                    "source_edge_count",
                    "timing_relevant_edge_count",
                    "manual_review_timing_edge_count",
                    "ready_for_m35_c4",
                ],
            ),
            "c4_zone": _stage_summary(
                zone_report,
                [
                    "node_count",
                    "source_edge_count",
                    "zone_relevant_edge_count",
                    "manual_review_zone_edge_count",
                    "unsupported_required_zone_count",
                    "ready_for_m35_c5",
                ],
            ),
            "c5_compatibility": _stage_summary(
                compatibility_report,
                ["edge_count", "status_counts", "m35_d1_candidate_edge_count", "ready_for_m35_d1"],
            ),
        },
        "runtime_boundary": {
            "advisory_probe_only": True,
            "does_not_create_deck": True,
            "does_not_mutate_runtime_pack": True,
            "does_not_publish_to_bot": True,
            "does_not_publish_playbook_seed": True,
        },
        "readiness": {
            "generalized_selected_slice_contract_ready": all_ready,
            "second_slice_semantic_compatibility_probe_passed": all_ready,
            "ready_for_m35_e4_bot_gate_review": all_ready,
            "runtime_or_bot_promotion_allowed": False,
        },
        "next_target": {
            "milestone": "M35-E4",
            "task": "Bot integration gate for reviewed playbook hints only",
            "must_create": [
                "explicit gate that bot can only consume reviewed hints through legal actions",
                "masked-state requirement audit",
                "no runtime use of unreviewed E3 probe edges",
            ],
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    readiness = report["readiness"]
    lines = [
        "# M35-E3 Generalized Semantic / Compatibility Probe",
        "",
        "## Selected Target",
        "",
        f"- Slice: `{target['slice']}`",
        f"- Era preset: `{target['era_preset']}`",
        f"- Group: `{target['group']}`",
        f"- M34-03 rank: `{target['rank']}`",
        "",
        "## Readiness",
        "",
        f"- Generalized selected-slice contract ready: `{readiness['generalized_selected_slice_contract_ready']}`",
        f"- Second-slice semantic/compatibility probe passed: `{readiness['second_slice_semantic_compatibility_probe_passed']}`",
        f"- Runtime/bot promotion allowed: `{readiness['runtime_or_bot_promotion_allowed']}`",
        "",
        "## Stage Readiness",
        "",
    ]
    for key, value in report["stage_readiness"].items():
        lines.append(f"- `{key}`: `{value}`")
    lines.extend(["", "## Stage Summaries", ""])
    for key, summary in report["stage_summaries"].items():
        lines.append(f"- `{key}`: `{summary}`")
    lines.extend(
        [
            "",
            "## Boundary",
            "",
            "- Advisory probe only.",
            "- Does not create or edit player decks.",
            "- Does not mutate runtime card packs.",
            "- Does not publish bot/runtime playbook data.",
            "",
            "## Next",
            "",
            "`M35-E4`: bot integration gate for reviewed playbook hints only.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35-E3 generalized selected-slice semantic/compatibility probe.")
    parser.add_argument("--selected-report", type=Path, default=SECOND_SLICE_REPORT)
    parser.add_argument("--readiness-report", type=Path, default=SECOND_SLICE_READINESS)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    selected_report = load_json(args.selected_report)
    readiness_report = load_json(args.readiness_report)
    report = build_probe(selected_report, readiness_report, args.selected_report, args.readiness_report)
    json_path = args.output_dir / "m35_e3_generalized_semantic_compatibility_probe.json"
    md_path = args.output_dir / "m35_e3_generalized_semantic_compatibility_probe.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35-E3 generalized probe wrote {json_path}")
    print(f"M35-E3 generalized probe summary wrote {md_path}")
    print(
        "ready={ready} cards={cards} edges={edges} candidates={candidates}".format(
            ready=report["readiness"]["generalized_selected_slice_contract_ready"],
            cards=report["stage_summaries"]["b2_semantic_tags"]["card_count"],
            edges=report["stage_summaries"]["c5_compatibility"]["edge_count"],
            candidates=report["stage_summaries"]["c5_compatibility"]["m35_d1_candidate_edge_count"],
        )
    )
    return 0 if report["readiness"]["generalized_selected_slice_contract_ready"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
