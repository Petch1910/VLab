"""Build selected-slice compatibility output.

M35-C5 of the Hybrid Vertical-Slice Strategy.

This combines C1 pair graph data with C2 resource, C3 timing, and C4 zone
detectors into one advisory edge-level compatibility output. It does not choose
a deck skeleton or publish playbook/bot data.
"""

from __future__ import annotations

import argparse
import json
import sys
from collections import Counter
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
PAIR_GRAPH_REPORT = ROOT / "outputs" / "target_slice" / "m35_c1_first_slice_pair_compatibility_graph.json"
RESOURCE_REPORT = ROOT / "outputs" / "target_slice" / "m35_c2_first_slice_resource_conflict_detector.json"
TIMING_REPORT = ROOT / "outputs" / "target_slice" / "m35_c3_first_slice_timing_compatibility_detector.json"
ZONE_REPORT = ROOT / "outputs" / "target_slice" / "m35_c4_first_slice_zone_target_detector.json"
OUTPUT_DIR = ROOT / "outputs" / "target_slice"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


RESOURCE_SYNERGY = {"resource_support"}
RESOURCE_CONFLICT = {"shared_resource_pressure", "mixed_support_and_shared_pressure"}
RESOURCE_MISSING = {"missing_slice_recovery"}

TIMING_SYNERGY = {"timing_can_precede", "target_timing_not_constrained"}
TIMING_CONFLICT = {"provider_after_consumer_window", "same_window_requires_ordering"}
TIMING_MISSING = {"source_timing_unknown_or_static"}

ZONE_SYNERGY = {"zone_support"}
ZONE_CONFLICT = {"vanguard_role_conflict", "rear_guard_slot_pressure", "mixed_zone_support_and_slot_pressure"}
ZONE_MISSING = {"missing_zone_support_in_slice"}


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _finding_map(report: dict[str, Any], key: str) -> dict[str, dict[str, Any]]:
    return {finding["edge"]: finding for finding in report.get(key, [])}


def _append_reason(target: list[str], reason: str) -> None:
    if reason not in target:
        target.append(reason)


def _classify_status(labels: set[str]) -> str:
    if "manual_review_required" in labels:
        return "manual_review_required"
    if "missing_data" in labels:
        return "missing_data"
    if "conflict" in labels and "synergy" in labels:
        return "mixed"
    if "conflict" in labels:
        return "conflict"
    if "synergy" in labels:
        return "synergy"
    return "neutral"


def build_edge_output(
    edge: dict[str, Any],
    resource_by_edge: dict[str, dict[str, Any]],
    timing_by_edge: dict[str, dict[str, Any]],
    zone_by_edge: dict[str, dict[str, Any]],
) -> dict[str, Any]:
    edge_key = f"{edge['source_card_id']}->{edge['target_card_id']}"
    resource = resource_by_edge.get(edge_key)
    timing = timing_by_edge.get(edge_key)
    zone = zone_by_edge.get(edge_key)

    labels: set[str] = set()
    synergy_reasons: list[str] = []
    conflict_reasons: list[str] = []
    missing_data_reasons: list[str] = []
    manual_review_reasons: list[str] = []

    if edge.get("categories"):
        labels.add("synergy")
        _append_reason(synergy_reasons, "pair_graph_provider_requirement_match")

    if edge.get("manual_review_required"):
        labels.add("manual_review_required")
        manual_review_reasons.extend(edge.get("manual_review_reasons", []))

    resource_verdict = resource["verdict"] if resource else "not_resource_relevant"
    if resource_verdict in RESOURCE_SYNERGY:
        labels.add("synergy")
        _append_reason(synergy_reasons, f"resource:{resource_verdict}")
    if resource_verdict in RESOURCE_CONFLICT:
        labels.add("conflict")
        _append_reason(conflict_reasons, f"resource:{resource_verdict}")
    if resource_verdict in RESOURCE_MISSING:
        labels.add("missing_data")
        _append_reason(missing_data_reasons, f"resource:{resource_verdict}")
    if resource and resource.get("manual_review_required"):
        labels.add("manual_review_required")
        _append_reason(manual_review_reasons, "resource_manual_review_required")

    timing_verdict = timing["verdict"] if timing else "not_timing_relevant"
    if timing_verdict in TIMING_SYNERGY:
        labels.add("synergy")
        _append_reason(synergy_reasons, f"timing:{timing_verdict}")
    if timing_verdict in TIMING_CONFLICT:
        labels.add("conflict")
        _append_reason(conflict_reasons, f"timing:{timing_verdict}")
    if timing_verdict in TIMING_MISSING:
        labels.add("missing_data")
        _append_reason(missing_data_reasons, f"timing:{timing_verdict}")
    if timing and timing.get("manual_review_required"):
        labels.add("manual_review_required")
        _append_reason(manual_review_reasons, "timing_manual_review_required")

    zone_verdict = zone["verdict"] if zone else "not_zone_relevant"
    if zone_verdict in ZONE_SYNERGY:
        labels.add("synergy")
        _append_reason(synergy_reasons, f"zone:{zone_verdict}")
    if zone_verdict in ZONE_CONFLICT:
        labels.add("conflict")
        _append_reason(conflict_reasons, f"zone:{zone_verdict}")
    if zone_verdict in ZONE_MISSING:
        labels.add("missing_data")
        _append_reason(missing_data_reasons, f"zone:{zone_verdict}")
    if zone and zone.get("manual_review_required"):
        labels.add("manual_review_required")
        _append_reason(manual_review_reasons, "zone_manual_review_required")

    status = _classify_status(labels)
    conflict_penalty = len(conflict_reasons) * 3 + len(missing_data_reasons) * 4
    manual_penalty = 5 if "manual_review_required" in labels else 0
    synergy_bonus = len(synergy_reasons) * 2
    net_score = edge["score"] + synergy_bonus - conflict_penalty - manual_penalty
    candidate_for_d1 = status == "synergy" and net_score > 0

    return {
        "edge": edge_key,
        "source_card_id": edge["source_card_id"],
        "source_name_th": edge["source_name_th"],
        "target_card_id": edge["target_card_id"],
        "target_name_th": edge["target_name_th"],
        "status": status,
        "labels": sorted(labels),
        "net_score": net_score,
        "pair_graph_score": edge["score"],
        "resource_verdict": resource_verdict,
        "timing_verdict": timing_verdict,
        "zone_verdict": zone_verdict,
        "synergy_reasons": synergy_reasons,
        "conflict_reasons": conflict_reasons,
        "missing_data_reasons": missing_data_reasons,
        "manual_review_reasons": sorted(set(manual_review_reasons)),
        "candidate_for_m35_d1": candidate_for_d1,
        "allowed_next_use": "candidate_package_selection" if candidate_for_d1 else "review_or_filter_before_d1",
    }


def build_report(
    pair_graph_report: dict[str, Any] | None = None,
    resource_report: dict[str, Any] | None = None,
    timing_report: dict[str, Any] | None = None,
    zone_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    pair_graph_report = pair_graph_report or load_json(PAIR_GRAPH_REPORT)
    resource_report = resource_report or load_json(RESOURCE_REPORT)
    timing_report = timing_report or load_json(TIMING_REPORT)
    zone_report = zone_report or load_json(ZONE_REPORT)

    resource_by_edge = _finding_map(resource_report, "edge_resource_findings")
    timing_by_edge = _finding_map(timing_report, "edge_timing_findings")
    zone_by_edge = _finding_map(zone_report, "edge_zone_findings")

    outputs = [
        build_edge_output(edge, resource_by_edge, timing_by_edge, zone_by_edge)
        for edge in pair_graph_report["edges"]
    ]
    outputs.sort(
        key=lambda item: (
            item["status"] != "synergy",
            item["status"] != "mixed",
            -item["net_score"],
            item["source_card_id"],
            item["target_card_id"],
        )
    )

    status_counts = Counter(item["status"] for item in outputs)
    label_counts: Counter[str] = Counter()
    for item in outputs:
        for label in item["labels"]:
            label_counts[label] += 1
    d1_candidates = [item for item in outputs if item["candidate_for_m35_d1"]]
    return {
        "version": "M35-C5",
        "description": "Selected-slice compatibility output from C1-C4 detectors",
        "selected_target": pair_graph_report["selected_target"],
        "source_inputs": {
            "pair_compatibility_graph": str(PAIR_GRAPH_REPORT.relative_to(ROOT)),
            "resource_conflict_detector": str(RESOURCE_REPORT.relative_to(ROOT)),
            "timing_compatibility_detector": str(TIMING_REPORT.relative_to(ROOT)),
            "zone_target_detector": str(ZONE_REPORT.relative_to(ROOT)),
        },
        "scope": {
            "advisory_only": True,
            "selected_slice_compatibility_output": True,
            "deck_skeleton": False,
            "bot_playbook": False,
            "runtime_effect_execution": False,
        },
        "policy": {
            "does_not_choose_deck_slots": True,
            "does_not_promote_manual_review_edges": True,
            "does_not_publish_to_runtime_or_bot": True,
            "d1_candidates_are_inputs_not_final_deck_choices": True,
        },
        "summary": {
            "edge_count": len(outputs),
            "status_counts": dict(sorted(status_counts.items())),
            "label_counts": dict(sorted(label_counts.items())),
            "m35_d1_candidate_edge_count": len(d1_candidates),
            "ready_for_m35_d1": True,
        },
        "compatibility_edges": outputs,
        "m35_d1_candidate_edges": d1_candidates[:200],
        "next_target": "M35-D1",
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    summary = report["summary"]
    lines = [
        "# M35-C5 Selected-Slice Compatibility Output",
        "",
        "## Selected Target",
        "",
        f"- Slice: `{target['slice']}`",
        f"- Era preset: `{target['era_preset']}`",
        f"- Group: `{target['group']}`",
        "",
        "## Summary",
        "",
        f"- Edges: `{summary['edge_count']}`",
        f"- M35-D1 candidate edges: `{summary['m35_d1_candidate_edge_count']}`",
        f"- Ready for M35-D1: `{summary['ready_for_m35_d1']}`",
        "",
        "## Status Counts",
        "",
    ]
    for status, count in summary["status_counts"].items():
        lines.append(f"- `{status}`: `{count}`")
    lines.extend(["", "## Label Counts", ""])
    for label, count in summary["label_counts"].items():
        lines.append(f"- `{label}`: `{count}`")
    lines.extend(["", "## Top D1 Candidate Edges", ""])
    for item in report["m35_d1_candidate_edges"][:20]:
        lines.append(
            f"- `{item['edge']}` score=`{item['net_score']}` "
            f"resource=`{item['resource_verdict']}` timing=`{item['timing_verdict']}` zone=`{item['zone_verdict']}`"
        )
    lines.extend(
        [
            "",
            "## Scope",
            "",
            "- Advisory selected-slice compatibility output only.",
            "- D1 candidates are inputs for package selection, not final deck choices.",
            "- No deck skeleton or bot playbook promotion.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35-C5 selected-slice compatibility output.")
    parser.add_argument("--pair-graph", type=Path, default=PAIR_GRAPH_REPORT)
    parser.add_argument("--resource-report", type=Path, default=RESOURCE_REPORT)
    parser.add_argument("--timing-report", type=Path, default=TIMING_REPORT)
    parser.add_argument("--zone-report", type=Path, default=ZONE_REPORT)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_report(
        load_json(args.pair_graph),
        load_json(args.resource_report),
        load_json(args.timing_report),
        load_json(args.zone_report),
    )
    json_path = args.output_dir / "m35_c5_first_slice_selected_compatibility_output.json"
    md_path = args.output_dir / "m35_c5_first_slice_selected_compatibility_output.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35-C5 selected-slice compatibility output wrote {json_path}")
    print(f"M35-C5 selected-slice compatibility summary wrote {md_path}")
    print(
        "edges={edges} d1_candidates={candidates} ready_for_m35_d1={ready}".format(
            edges=report["summary"]["edge_count"],
            candidates=report["summary"]["m35_d1_candidate_edge_count"],
            ready=report["summary"]["ready_for_m35_d1"],
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
