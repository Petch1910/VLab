"""Build timing compatibility detector output for the selected first slice.

M35-C3 of the Hybrid Vertical-Slice Strategy.

The selected slice does not yet have structured per-effect provider timing, so
this detector uses card-level timing requirement tags as an explicit proxy.
It stays advisory and prepares data for the later zone/target detector.
"""

from __future__ import annotations

import argparse
import json
import sys
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any, Iterable, Sequence


ROOT = Path(__file__).resolve().parents[2]
PAIR_GRAPH_REPORT = ROOT / "outputs" / "target_slice" / "m35_c1_first_slice_pair_compatibility_graph.json"
RESOURCE_REPORT = ROOT / "outputs" / "target_slice" / "m35_c2_first_slice_resource_conflict_detector.json"
OUTPUT_DIR = ROOT / "outputs" / "target_slice"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


TIMING_WINDOWS: dict[str, dict[str, Any]] = {
    "requires_on_ride_timing": {
        "window": "on_ride",
        "rank": 10,
        "label": "On ride",
    },
    "requires_on_call_timing": {
        "window": "on_call",
        "rank": 20,
        "label": "On call/place to RC",
    },
    "requires_boost_timing": {
        "window": "boost_step",
        "rank": 35,
        "label": "Boost step",
    },
    "requires_on_attack_timing": {
        "window": "attack_declaration",
        "rank": 40,
        "label": "Attack declaration",
    },
    "requires_trigger_check_timing": {
        "window": "trigger_check",
        "rank": 50,
        "label": "Drive/damage trigger check",
    },
    "requires_hit_timing": {
        "window": "attack_hit",
        "rank": 60,
        "label": "Attack hit",
    },
    "requires_end_phase_timing": {
        "window": "end_phase",
        "rank": 80,
        "label": "End phase",
    },
}


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _sorted_unique(values: Iterable[str]) -> list[str]:
    return sorted({value for value in values if value})


def build_timing_profile(node: dict[str, Any]) -> dict[str, Any]:
    entries = []
    for requirement in node.get("requirements", []):
        timing = TIMING_WINDOWS.get(requirement)
        if timing:
            entries.append({"requirement": requirement, **timing})
    entries.sort(key=lambda entry: (entry["rank"], entry["window"]))
    ranks = [entry["rank"] for entry in entries]
    return {
        "card_id": node["card_id"],
        "name_th": node["name_th"],
        "timing_requirements": entries,
        "timing_windows": _sorted_unique(entry["window"] for entry in entries),
        "earliest_rank": min(ranks) if ranks else None,
        "latest_rank": max(ranks) if ranks else None,
        "manual_review_required": node.get("manual_review_required", False),
    }


def build_profiles(nodes: Sequence[dict[str, Any]]) -> dict[str, dict[str, Any]]:
    return {node["card_id"]: build_timing_profile(node) for node in nodes}


def build_timing_indexes(profiles: dict[str, dict[str, Any]]) -> dict[str, Any]:
    card_index: dict[str, list[str]] = defaultdict(list)
    timing_counts: Counter[str] = Counter()
    for card_id, profile in profiles.items():
        for window in profile["timing_windows"]:
            card_index[window].append(card_id)
            timing_counts[window] += 1
    return {
        "card_index": {key: sorted(value) for key, value in sorted(card_index.items())},
        "timing_counts": dict(sorted(timing_counts.items())),
    }


def _resource_verdict_by_edge(resource_report: dict[str, Any] | None) -> dict[str, str]:
    if not resource_report:
        return {}
    return {
        finding["edge"]: finding["verdict"]
        for finding in resource_report.get("edge_resource_findings", [])
    }


def classify_edge(
    edge: dict[str, Any],
    source_profile: dict[str, Any],
    target_profile: dict[str, Any],
    resource_verdict_by_edge: dict[str, str],
) -> dict[str, Any] | None:
    edge_key = f"{edge['source_card_id']}->{edge['target_card_id']}"
    source_rank = source_profile["earliest_rank"]
    target_rank = target_profile["earliest_rank"]
    if source_rank is None and target_rank is None:
        return None

    if target_rank is None:
        verdict = "target_timing_not_constrained"
    elif source_rank is None:
        verdict = "source_timing_unknown_or_static"
    elif source_rank < target_rank:
        verdict = "timing_can_precede"
    elif source_rank == target_rank:
        verdict = "same_window_requires_ordering"
    else:
        verdict = "provider_after_consumer_window"

    timing_gap = None
    if source_rank is not None and target_rank is not None:
        timing_gap = target_rank - source_rank

    reasons: list[str] = []
    if verdict == "target_timing_not_constrained":
        reasons.append("target_has_no_explicit_timing_requirement")
    elif verdict == "source_timing_unknown_or_static":
        reasons.append("source_provider_timing_not_explicit_in_current_tags")
    elif verdict == "timing_can_precede":
        reasons.append("source_timing_proxy_can_occur_before_target_window")
    elif verdict == "same_window_requires_ordering":
        reasons.append("same_timing_window_needs_resolution_order_review")
    elif verdict == "provider_after_consumer_window":
        reasons.append("source_timing_proxy_occurs_after_target_window")
    if edge.get("manual_review_required"):
        reasons.append("manual_review_edge_kept_low_confidence")

    return {
        "edge": edge_key,
        "source_card_id": edge["source_card_id"],
        "target_card_id": edge["target_card_id"],
        "verdict": verdict,
        "timing_gap": timing_gap,
        "source_timing_windows": source_profile["timing_windows"],
        "target_timing_windows": target_profile["timing_windows"],
        "source_earliest_rank": source_rank,
        "target_earliest_rank": target_rank,
        "resource_verdict": resource_verdict_by_edge.get(edge_key, "not_resource_relevant"),
        "manual_review_required": edge.get("manual_review_required", False),
        "confidence": "review_required" if edge.get("manual_review_required") else "advisory",
        "reasons": reasons,
    }


def build_edge_findings(
    edges: Sequence[dict[str, Any]],
    profiles: dict[str, dict[str, Any]],
    resource_verdict_by_edge: dict[str, str],
) -> list[dict[str, Any]]:
    findings = []
    for edge in edges:
        finding = classify_edge(
            edge,
            profiles[edge["source_card_id"]],
            profiles[edge["target_card_id"]],
            resource_verdict_by_edge,
        )
        if finding:
            findings.append(finding)
    return sorted(
        findings,
        key=lambda finding: (
            finding["manual_review_required"],
            finding["verdict"],
            finding["source_card_id"],
            finding["target_card_id"],
        ),
    )


def build_report(
    pair_graph_report: dict[str, Any] | None = None,
    resource_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    pair_graph_report = pair_graph_report or load_json(PAIR_GRAPH_REPORT)
    if resource_report is None and RESOURCE_REPORT.exists():
        resource_report = load_json(RESOURCE_REPORT)
    profiles = build_profiles(pair_graph_report["nodes"])
    indexes = build_timing_indexes(profiles)
    resource_verdicts = _resource_verdict_by_edge(resource_report)
    findings = build_edge_findings(pair_graph_report["edges"], profiles, resource_verdicts)
    verdict_counts = Counter(finding["verdict"] for finding in findings)
    manual_review_findings = sum(1 for finding in findings if finding["manual_review_required"])
    return {
        "version": "M35-C3",
        "description": "Timing compatibility detector for selected first-slice pair graph",
        "selected_target": pair_graph_report["selected_target"],
        "source_inputs": {
            "pair_compatibility_graph": str(PAIR_GRAPH_REPORT.relative_to(ROOT)),
            "resource_conflict_detector": str(RESOURCE_REPORT.relative_to(ROOT)),
        },
        "scope": {
            "advisory_only": True,
            "uses_card_level_timing_proxy": True,
            "resource_conflict_detector": False,
            "timing_compatibility_detector": True,
            "zone_target_compatibility_detector": False,
            "deck_skeleton": False,
            "bot_playbook": False,
            "runtime_effect_execution": False,
        },
        "policy": {
            "no_final_timing_verdict_without_structured_effect_timing": True,
            "manual_review_blocks_high_confidence": True,
            "does_not_mutate_card_data": True,
            "does_not_publish_to_runtime_or_bot": True,
        },
        "timing_windows": TIMING_WINDOWS,
        "summary": {
            "node_count": len(profiles),
            "source_edge_count": len(pair_graph_report["edges"]),
            "timing_relevant_edge_count": len(findings),
            "manual_review_timing_edge_count": manual_review_findings,
            "timing_window_type_count": len(indexes["card_index"]),
            "verdict_counts": dict(sorted(verdict_counts.items())),
            "ready_for_m35_c4": True,
        },
        "timing_indexes": indexes,
        "card_timing_profiles": [profiles[card_id] for card_id in sorted(profiles)],
        "edge_timing_findings": findings,
        "next_target": "M35-C4",
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def _top_findings(findings: Sequence[dict[str, Any]], limit: int = 12) -> list[dict[str, Any]]:
    return sorted(
        findings,
        key=lambda item: (
            item["verdict"] != "provider_after_consumer_window",
            item["timing_gap"] if item["timing_gap"] is not None else 999,
            item["source_card_id"],
            item["target_card_id"],
        ),
    )[:limit]


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    summary = report["summary"]
    lines = [
        "# M35-C3 Timing Compatibility Detector",
        "",
        "## Selected Target",
        "",
        f"- Slice: `{target['slice']}`",
        f"- Era preset: `{target['era_preset']}`",
        f"- Group: `{target['group']}`",
        "",
        "## Summary",
        "",
        f"- Nodes: `{summary['node_count']}`",
        f"- Source graph edges: `{summary['source_edge_count']}`",
        f"- Timing-relevant edges: `{summary['timing_relevant_edge_count']}`",
        f"- Manual-review timing edges: `{summary['manual_review_timing_edge_count']}`",
        f"- Timing window types: `{summary['timing_window_type_count']}`",
        f"- Ready for M35-C4: `{summary['ready_for_m35_c4']}`",
        "",
        "## Verdict Counts",
        "",
    ]
    for verdict, count in summary["verdict_counts"].items():
        lines.append(f"- `{verdict}`: `{count}`")
    lines.extend(["", "## Timing Window Counts", ""])
    for window, count in report["timing_indexes"]["timing_counts"].items():
        lines.append(f"- `{window}`: `{count}`")
    lines.extend(["", "## Timing Findings To Review First", ""])
    for finding in _top_findings(report["edge_timing_findings"]):
        review = " review-required" if finding["manual_review_required"] else ""
        lines.append(
            f"- `{finding['edge']}` verdict=`{finding['verdict']}` "
            f"gap=`{finding['timing_gap']}` source=`{', '.join(finding['source_timing_windows']) or 'none'}` "
            f"target=`{', '.join(finding['target_timing_windows']) or 'none'}`{review}"
        )
    lines.extend(
        [
            "",
            "## Scope",
            "",
            "- Advisory timing detector only.",
            "- Uses card-level timing tags as proxy until structured effect timing exists.",
            "- No zone/target compatibility verdict yet.",
            "- No deck skeleton or bot playbook promotion.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35-C3 selected-slice timing compatibility detector.")
    parser.add_argument("--pair-graph", type=Path, default=PAIR_GRAPH_REPORT)
    parser.add_argument("--resource-report", type=Path, default=RESOURCE_REPORT)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    pair_graph_report = load_json(args.pair_graph)
    resource_report = load_json(args.resource_report) if args.resource_report.exists() else None
    report = build_report(pair_graph_report, resource_report)
    json_path = args.output_dir / "m35_c3_first_slice_timing_compatibility_detector.json"
    md_path = args.output_dir / "m35_c3_first_slice_timing_compatibility_detector.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35-C3 timing compatibility detector wrote {json_path}")
    print(f"M35-C3 timing compatibility summary wrote {md_path}")
    print(
        "timing_edges={edges} manual_review_edges={manual} ready_for_m35_c4={ready}".format(
            edges=report["summary"]["timing_relevant_edge_count"],
            manual=report["summary"]["manual_review_timing_edge_count"],
            ready=report["summary"]["ready_for_m35_c4"],
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
