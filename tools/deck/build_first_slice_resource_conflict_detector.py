"""Build resource conflict detector output for the selected first slice.

M35-C2 of the Hybrid Vertical-Slice Strategy.

This consumes the M35-C1 pair graph and adds advisory resource-pressure
findings. It does not decide final compatibility; timing and zone checks remain
separate M35-C3/M35-C4 work.
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
OUTPUT_DIR = ROOT / "outputs" / "target_slice"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


RESOURCE_REQUIREMENTS: dict[str, dict[str, Any]] = {
    "requires_counter_blast": {
        "resource": "counter_blast",
        "kind": "cost",
        "pressure": 3,
        "label": "CounterBlast cost pressure",
    },
    "requires_soul_blast": {
        "resource": "soul",
        "kind": "cost",
        "pressure": 3,
        "label": "SoulBlast cost pressure",
    },
    "requires_discard": {
        "resource": "hand_cards",
        "kind": "cost",
        "pressure": 2,
        "label": "Discard/hand cost pressure",
    },
    "requires_rest_this_unit": {
        "resource": "self_rest",
        "kind": "activation_lock",
        "pressure": 1,
        "label": "Rest-this-unit activation pressure",
    },
    "requires_retire_this_unit": {
        "resource": "self_retire",
        "kind": "activation_loss",
        "pressure": 2,
        "label": "Retire-this-unit activation pressure",
    },
}

RESOURCE_PROVIDERS: dict[str, dict[str, Any]] = {
    "provides_counter_charge": {
        "resource": "counter_blast",
        "kind": "recovery",
        "relief": 3,
        "label": "CounterCharge recovery",
    },
    "provides_counter_blast_sustain": {
        "resource": "counter_blast",
        "kind": "sustain",
        "relief": 3,
        "label": "CounterBlast sustain",
    },
    "provides_soul_charge": {
        "resource": "soul",
        "kind": "recovery",
        "relief": 3,
        "label": "SoulCharge recovery",
    },
    "provides_soul_resource": {
        "resource": "soul",
        "kind": "recovery",
        "relief": 3,
        "label": "Soul resource generation",
    },
    "provides_soul_blast_sustain": {
        "resource": "soul",
        "kind": "sustain",
        "relief": 3,
        "label": "SoulBlast sustain",
    },
    "provides_card_advantage": {
        "resource": "hand_cards",
        "kind": "offset",
        "relief": 2,
        "label": "Card advantage offsets hand pressure",
    },
    "provides_draw_trigger": {
        "resource": "hand_cards",
        "kind": "offset",
        "relief": 1,
        "label": "Draw trigger offsets hand pressure",
    },
}


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _sorted_unique(values: Iterable[str]) -> list[str]:
    return sorted({value for value in values if value})


def build_card_profile(node: dict[str, Any]) -> dict[str, Any]:
    demand_entries = []
    provider_entries = []
    for requirement in node.get("requirements", []):
        resource = RESOURCE_REQUIREMENTS.get(requirement)
        if resource:
            demand_entries.append({"requirement": requirement, **resource})
    for provider in node.get("providers", []):
        resource = RESOURCE_PROVIDERS.get(provider)
        if resource:
            provider_entries.append({"provider": provider, **resource})

    demand_score = sum(entry["pressure"] for entry in demand_entries)
    recovery_score = sum(entry["relief"] for entry in provider_entries)
    return {
        "card_id": node["card_id"],
        "name_th": node["name_th"],
        "resource_demands": demand_entries,
        "resource_providers": provider_entries,
        "resources_demanded": _sorted_unique(entry["resource"] for entry in demand_entries),
        "resources_provided": _sorted_unique(entry["resource"] for entry in provider_entries),
        "demand_score": demand_score,
        "recovery_score": recovery_score,
        "net_pressure_score": demand_score - recovery_score,
        "manual_review_required": node.get("manual_review_required", False),
    }


def build_profiles(nodes: Sequence[dict[str, Any]]) -> dict[str, dict[str, Any]]:
    return {node["card_id"]: build_card_profile(node) for node in nodes}


def build_resource_indexes(profiles: dict[str, dict[str, Any]]) -> dict[str, Any]:
    demand_index: dict[str, list[str]] = defaultdict(list)
    provider_index: dict[str, list[str]] = defaultdict(list)
    demand_counts: Counter[str] = Counter()
    provider_counts: Counter[str] = Counter()
    for card_id, profile in profiles.items():
        for resource in profile["resources_demanded"]:
            demand_index[resource].append(card_id)
            demand_counts[resource] += 1
        for resource in profile["resources_provided"]:
            provider_index[resource].append(card_id)
            provider_counts[resource] += 1
    missing_recovery = sorted(resource for resource in demand_index if resource not in provider_index)
    return {
        "demand_index": {key: sorted(value) for key, value in sorted(demand_index.items())},
        "provider_index": {key: sorted(value) for key, value in sorted(provider_index.items())},
        "demand_counts": dict(sorted(demand_counts.items())),
        "provider_counts": dict(sorted(provider_counts.items())),
        "missing_recovery_resources": missing_recovery,
    }


def classify_edge(
    edge: dict[str, Any],
    source_profile: dict[str, Any],
    target_profile: dict[str, Any],
    missing_recovery_resources: set[str],
) -> dict[str, Any] | None:
    source_demands = set(source_profile["resources_demanded"])
    source_provides = set(source_profile["resources_provided"])
    target_demands = set(target_profile["resources_demanded"])
    if not source_demands and not source_provides and not target_demands:
        return None

    supported_resources = sorted(target_demands & source_provides)
    shared_pressure_resources = sorted((source_demands & target_demands) - set(supported_resources))
    target_unsupported_by_source = sorted(target_demands - source_provides)
    missing_slice_recovery = sorted(target_demands & missing_recovery_resources)

    if missing_slice_recovery:
        verdict = "missing_slice_recovery"
    elif supported_resources and shared_pressure_resources:
        verdict = "mixed_support_and_shared_pressure"
    elif supported_resources:
        verdict = "resource_support"
    elif shared_pressure_resources:
        verdict = "shared_resource_pressure"
    elif target_demands:
        verdict = "target_resource_need_not_supported_by_source"
    else:
        verdict = "source_resource_profile_only"

    risk_score = (
        target_profile["demand_score"]
        + source_profile["demand_score"]
        - source_profile["recovery_score"]
        - len(supported_resources)
    )
    reasons: list[str] = []
    if supported_resources:
        reasons.append("source_provides_target_resource")
    if shared_pressure_resources:
        reasons.append("source_and_target_pressure_same_resource")
    if target_unsupported_by_source:
        reasons.append("target_resource_need_not_met_by_source")
    if missing_slice_recovery:
        reasons.append("selected_slice_missing_recovery_for_resource")
    if edge.get("manual_review_required"):
        reasons.append("manual_review_edge_kept_low_confidence")

    return {
        "edge": f"{edge['source_card_id']}->{edge['target_card_id']}",
        "source_card_id": edge["source_card_id"],
        "target_card_id": edge["target_card_id"],
        "verdict": verdict,
        "risk_score": risk_score,
        "supported_resources": supported_resources,
        "shared_pressure_resources": shared_pressure_resources,
        "target_unsupported_by_source": target_unsupported_by_source,
        "missing_slice_recovery_resources": missing_slice_recovery,
        "source_resources_demanded": source_profile["resources_demanded"],
        "source_resources_provided": source_profile["resources_provided"],
        "target_resources_demanded": target_profile["resources_demanded"],
        "manual_review_required": edge.get("manual_review_required", False),
        "confidence": "review_required" if edge.get("manual_review_required") else "advisory",
        "reasons": reasons,
    }


def build_edge_findings(
    edges: Sequence[dict[str, Any]],
    profiles: dict[str, dict[str, Any]],
    missing_recovery_resources: Sequence[str],
) -> list[dict[str, Any]]:
    missing = set(missing_recovery_resources)
    findings = []
    for edge in edges:
        finding = classify_edge(edge, profiles[edge["source_card_id"]], profiles[edge["target_card_id"]], missing)
        if finding:
            findings.append(finding)
    return sorted(
        findings,
        key=lambda finding: (
            finding["manual_review_required"],
            finding["verdict"],
            -finding["risk_score"],
            finding["source_card_id"],
            finding["target_card_id"],
        ),
    )


def build_report(pair_graph_report: dict[str, Any] | None = None) -> dict[str, Any]:
    pair_graph_report = pair_graph_report or load_json(PAIR_GRAPH_REPORT)
    profiles = build_profiles(pair_graph_report["nodes"])
    indexes = build_resource_indexes(profiles)
    findings = build_edge_findings(
        pair_graph_report["edges"],
        profiles,
        indexes["missing_recovery_resources"],
    )
    verdict_counts = Counter(finding["verdict"] for finding in findings)
    manual_review_findings = sum(1 for finding in findings if finding["manual_review_required"])
    return {
        "version": "M35-C2",
        "description": "Resource conflict detector for selected first-slice pair graph",
        "selected_target": pair_graph_report["selected_target"],
        "source_inputs": {
            "pair_compatibility_graph": str(PAIR_GRAPH_REPORT.relative_to(ROOT)),
        },
        "scope": {
            "advisory_only": True,
            "resource_conflict_detector": True,
            "timing_compatibility_detector": False,
            "zone_target_compatibility_detector": False,
            "deck_skeleton": False,
            "bot_playbook": False,
            "runtime_effect_execution": False,
        },
        "policy": {
            "no_resource_amount_claim_without_structured_cost_amounts": True,
            "manual_review_blocks_high_confidence": True,
            "does_not_mutate_card_data": True,
            "does_not_publish_to_runtime_or_bot": True,
        },
        "resource_requirement_map": RESOURCE_REQUIREMENTS,
        "resource_provider_map": RESOURCE_PROVIDERS,
        "summary": {
            "node_count": len(profiles),
            "source_edge_count": len(pair_graph_report["edges"]),
            "resource_relevant_edge_count": len(findings),
            "manual_review_resource_edge_count": manual_review_findings,
            "missing_recovery_resource_count": len(indexes["missing_recovery_resources"]),
            "verdict_counts": dict(sorted(verdict_counts.items())),
            "ready_for_m35_c3": True,
        },
        "resource_indexes": indexes,
        "card_resource_profiles": [profiles[card_id] for card_id in sorted(profiles)],
        "edge_resource_findings": findings,
        "next_target": "M35-C3",
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def _top_findings(findings: Sequence[dict[str, Any]], limit: int = 12) -> list[dict[str, Any]]:
    return sorted(findings, key=lambda item: (-item["risk_score"], item["source_card_id"], item["target_card_id"]))[
        :limit
    ]


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    summary = report["summary"]
    lines = [
        "# M35-C2 Resource Conflict Detector",
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
        f"- Resource-relevant edges: `{summary['resource_relevant_edge_count']}`",
        f"- Manual-review resource edges: `{summary['manual_review_resource_edge_count']}`",
        f"- Missing recovery resource types: `{summary['missing_recovery_resource_count']}`",
        f"- Ready for M35-C3: `{summary['ready_for_m35_c3']}`",
        "",
        "## Verdict Counts",
        "",
    ]
    for verdict, count in summary["verdict_counts"].items():
        lines.append(f"- `{verdict}`: `{count}`")
    lines.extend(["", "## Resource Demand Counts", ""])
    for resource, count in report["resource_indexes"]["demand_counts"].items():
        lines.append(f"- `{resource}` demand: `{count}`")
    lines.extend(["", "## Resource Provider Counts", ""])
    for resource, count in report["resource_indexes"]["provider_counts"].items():
        lines.append(f"- `{resource}` provider: `{count}`")
    lines.extend(["", "## Top Resource Findings", ""])
    for finding in _top_findings(report["edge_resource_findings"]):
        review = " review-required" if finding["manual_review_required"] else ""
        lines.append(
            f"- `{finding['edge']}` verdict=`{finding['verdict']}` "
            f"risk=`{finding['risk_score']}` supported=`{', '.join(finding['supported_resources']) or 'none'}`{review}"
        )
    lines.extend(
        [
            "",
            "## Scope",
            "",
            "- Advisory resource detector only.",
            "- No exact resource amount claim until structured cost amounts exist.",
            "- No timing compatibility verdict yet.",
            "- No zone/target compatibility verdict yet.",
            "- No deck skeleton or bot playbook promotion.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35-C2 selected-slice resource conflict detector.")
    parser.add_argument("--pair-graph", type=Path, default=PAIR_GRAPH_REPORT)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    pair_graph_report = load_json(args.pair_graph)
    report = build_report(pair_graph_report)
    json_path = args.output_dir / "m35_c2_first_slice_resource_conflict_detector.json"
    md_path = args.output_dir / "m35_c2_first_slice_resource_conflict_detector.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35-C2 resource conflict detector wrote {json_path}")
    print(f"M35-C2 resource conflict summary wrote {md_path}")
    print(
        "resource_edges={edges} manual_review_edges={manual} missing_recovery={missing} ready_for_m35_c3={ready}".format(
            edges=report["summary"]["resource_relevant_edge_count"],
            manual=report["summary"]["manual_review_resource_edge_count"],
            missing=report["summary"]["missing_recovery_resource_count"],
            ready=report["summary"]["ready_for_m35_c3"],
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
