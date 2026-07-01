"""Build zone/target compatibility detector output for the selected first slice.

M35-C4 of the Hybrid Vertical-Slice Strategy.

This consumes the selected-slice pair graph plus resource/timing detector
context and emits advisory zone/target findings. It does not produce final
compatibility or deck skeletons.
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
TIMING_REPORT = ROOT / "outputs" / "target_slice" / "m35_c3_first_slice_timing_compatibility_detector.json"
OUTPUT_DIR = ROOT / "outputs" / "target_slice"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


ZONE_REQUIREMENTS: dict[str, dict[str, Any]] = {
    "requires_vanguard_circle": {
        "zone": "vanguard_circle",
        "kind": "exclusive_role",
        "pressure": 3,
        "label": "Needs Vanguard circle",
    },
    "requires_rear_guard_circle": {
        "zone": "rear_guard_circle",
        "kind": "board_slot",
        "pressure": 2,
        "label": "Needs Rear-guard circle",
    },
    "requires_guardian_circle": {
        "zone": "guardian_circle",
        "kind": "guard_slot",
        "pressure": 1,
        "label": "Needs Guardian circle",
    },
    "requires_damage_zone_reference": {
        "zone": "damage_zone",
        "kind": "resource_zone",
        "pressure": 1,
        "label": "References Damage zone",
    },
    "requires_drop_zone_reference": {
        "zone": "drop_zone",
        "kind": "resource_zone",
        "pressure": 1,
        "label": "References Drop zone",
    },
    "requires_soul_reference": {
        "zone": "soul",
        "kind": "resource_zone",
        "pressure": 1,
        "label": "References Soul",
    },
}

ZONE_PROVIDERS: dict[str, dict[str, Any]] = {
    "provides_board_extension": {
        "zone": "rear_guard_circle",
        "kind": "board_presence",
        "support": 2,
        "label": "Provides board extension",
    },
    "provides_soul_charge": {
        "zone": "soul",
        "kind": "resource_zone_setup",
        "support": 2,
        "label": "Provides Soul setup",
    },
    "provides_soul_resource": {
        "zone": "soul",
        "kind": "resource_zone_setup",
        "support": 2,
        "label": "Provides Soul resource",
    },
    "provides_soul_blast_sustain": {
        "zone": "soul",
        "kind": "resource_zone_setup",
        "support": 2,
        "label": "Sustains Soul use",
    },
    "provides_counter_charge": {
        "zone": "damage_zone",
        "kind": "resource_zone_setup",
        "support": 2,
        "label": "Supports Damage zone resource use",
    },
    "provides_counter_blast_sustain": {
        "zone": "damage_zone",
        "kind": "resource_zone_setup",
        "support": 2,
        "label": "Sustains CounterBlast use",
    },
    "provides_search": {
        "zone": "deck",
        "kind": "access",
        "support": 1,
        "label": "Searches deck",
    },
    "provides_deck_recycle": {
        "zone": "deck",
        "kind": "access",
        "support": 1,
        "label": "Recycles deck",
    },
    "provides_card_advantage": {
        "zone": "hand",
        "kind": "access",
        "support": 1,
        "label": "Adds hand access",
    },
    "provides_draw_trigger": {
        "zone": "hand",
        "kind": "access",
        "support": 1,
        "label": "Draw trigger adds hand access",
    },
    "provides_defensive_guard": {
        "zone": "guardian_circle",
        "kind": "guard_support",
        "support": 2,
        "label": "Provides guard support",
    },
}


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _sorted_unique(values: Iterable[str]) -> list[str]:
    return sorted({value for value in values if value})


def build_zone_profile(node: dict[str, Any]) -> dict[str, Any]:
    requirement_entries = []
    provider_entries = []
    for requirement in node.get("requirements", []):
        zone = ZONE_REQUIREMENTS.get(requirement)
        if zone:
            requirement_entries.append({"requirement": requirement, **zone})
    for provider in node.get("providers", []):
        zone = ZONE_PROVIDERS.get(provider)
        if zone:
            provider_entries.append({"provider": provider, **zone})
    return {
        "card_id": node["card_id"],
        "name_th": node["name_th"],
        "zone_requirements": requirement_entries,
        "zone_providers": provider_entries,
        "zones_required": _sorted_unique(entry["zone"] for entry in requirement_entries),
        "zones_provided": _sorted_unique(entry["zone"] for entry in provider_entries),
        "zone_pressure_score": sum(entry["pressure"] for entry in requirement_entries),
        "zone_support_score": sum(entry["support"] for entry in provider_entries),
        "manual_review_required": node.get("manual_review_required", False),
    }


def build_profiles(nodes: Sequence[dict[str, Any]]) -> dict[str, dict[str, Any]]:
    return {node["card_id"]: build_zone_profile(node) for node in nodes}


def build_zone_indexes(profiles: dict[str, dict[str, Any]]) -> dict[str, Any]:
    requirement_index: dict[str, list[str]] = defaultdict(list)
    provider_index: dict[str, list[str]] = defaultdict(list)
    requirement_counts: Counter[str] = Counter()
    provider_counts: Counter[str] = Counter()
    for card_id, profile in profiles.items():
        for zone in profile["zones_required"]:
            requirement_index[zone].append(card_id)
            requirement_counts[zone] += 1
        for zone in profile["zones_provided"]:
            provider_index[zone].append(card_id)
            provider_counts[zone] += 1
    no_provider_expected = {"vanguard_circle"}
    unsupported_required_zones = sorted(
        zone for zone in requirement_index if zone not in provider_index and zone not in no_provider_expected
    )
    return {
        "requirement_index": {key: sorted(value) for key, value in sorted(requirement_index.items())},
        "provider_index": {key: sorted(value) for key, value in sorted(provider_index.items())},
        "requirement_counts": dict(sorted(requirement_counts.items())),
        "provider_counts": dict(sorted(provider_counts.items())),
        "unsupported_required_zones": unsupported_required_zones,
    }


def _resource_verdicts(resource_report: dict[str, Any] | None) -> dict[str, str]:
    if not resource_report:
        return {}
    return {
        finding["edge"]: finding["verdict"]
        for finding in resource_report.get("edge_resource_findings", [])
    }


def _timing_verdicts(timing_report: dict[str, Any] | None) -> dict[str, str]:
    if not timing_report:
        return {}
    return {
        finding["edge"]: finding["verdict"]
        for finding in timing_report.get("edge_timing_findings", [])
    }


def classify_edge(
    edge: dict[str, Any],
    source_profile: dict[str, Any],
    target_profile: dict[str, Any],
    resource_verdict_by_edge: dict[str, str],
    timing_verdict_by_edge: dict[str, str],
    unsupported_required_zones: set[str],
) -> dict[str, Any] | None:
    edge_key = f"{edge['source_card_id']}->{edge['target_card_id']}"
    source_required = set(source_profile["zones_required"])
    source_provided = set(source_profile["zones_provided"])
    target_required = set(target_profile["zones_required"])
    if not source_required and not source_provided and not target_required:
        return None

    supported_zones = sorted(target_required & source_provided)
    unsupported_by_source = sorted(target_required - source_provided)
    unsupported_in_slice = sorted(target_required & unsupported_required_zones)
    shared_required_zones = sorted(source_required & target_required)

    vanguard_conflict = "vanguard_circle" in shared_required_zones
    rear_guard_pressure = "rear_guard_circle" in shared_required_zones

    if unsupported_in_slice:
        verdict = "missing_zone_support_in_slice"
    elif vanguard_conflict:
        verdict = "vanguard_role_conflict"
    elif supported_zones and rear_guard_pressure:
        verdict = "mixed_zone_support_and_slot_pressure"
    elif supported_zones:
        verdict = "zone_support"
    elif rear_guard_pressure:
        verdict = "rear_guard_slot_pressure"
    elif target_required:
        verdict = "target_zone_need_not_supported_by_source"
    else:
        verdict = "source_zone_profile_only"

    reasons: list[str] = []
    if supported_zones:
        reasons.append("source_provides_target_zone_or_access")
    if vanguard_conflict:
        reasons.append("both_cards_require_vanguard_circle")
    if rear_guard_pressure:
        reasons.append("both_cards_require_rear_guard_circle")
    if unsupported_by_source:
        reasons.append("target_zone_need_not_met_by_source")
    if unsupported_in_slice:
        reasons.append("selected_slice_missing_zone_provider")
    if edge.get("manual_review_required"):
        reasons.append("manual_review_edge_kept_low_confidence")

    return {
        "edge": edge_key,
        "source_card_id": edge["source_card_id"],
        "target_card_id": edge["target_card_id"],
        "verdict": verdict,
        "supported_zones": supported_zones,
        "shared_required_zones": shared_required_zones,
        "target_unsupported_by_source": unsupported_by_source,
        "unsupported_required_zones_in_slice": unsupported_in_slice,
        "source_zones_required": source_profile["zones_required"],
        "source_zones_provided": source_profile["zones_provided"],
        "target_zones_required": target_profile["zones_required"],
        "zone_pressure_score": target_profile["zone_pressure_score"] + source_profile["zone_pressure_score"],
        "resource_verdict": resource_verdict_by_edge.get(edge_key, "not_resource_relevant"),
        "timing_verdict": timing_verdict_by_edge.get(edge_key, "not_timing_relevant"),
        "manual_review_required": edge.get("manual_review_required", False),
        "confidence": "review_required" if edge.get("manual_review_required") else "advisory",
        "reasons": reasons,
    }


def build_edge_findings(
    edges: Sequence[dict[str, Any]],
    profiles: dict[str, dict[str, Any]],
    resource_verdict_by_edge: dict[str, str],
    timing_verdict_by_edge: dict[str, str],
    unsupported_required_zones: Sequence[str],
) -> list[dict[str, Any]]:
    unsupported = set(unsupported_required_zones)
    findings = []
    for edge in edges:
        finding = classify_edge(
            edge,
            profiles[edge["source_card_id"]],
            profiles[edge["target_card_id"]],
            resource_verdict_by_edge,
            timing_verdict_by_edge,
            unsupported,
        )
        if finding:
            findings.append(finding)
    return sorted(
        findings,
        key=lambda finding: (
            finding["manual_review_required"],
            finding["verdict"],
            -finding["zone_pressure_score"],
            finding["source_card_id"],
            finding["target_card_id"],
        ),
    )


def build_report(
    pair_graph_report: dict[str, Any] | None = None,
    resource_report: dict[str, Any] | None = None,
    timing_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    pair_graph_report = pair_graph_report or load_json(PAIR_GRAPH_REPORT)
    if resource_report is None and RESOURCE_REPORT.exists():
        resource_report = load_json(RESOURCE_REPORT)
    if timing_report is None and TIMING_REPORT.exists():
        timing_report = load_json(TIMING_REPORT)
    profiles = build_profiles(pair_graph_report["nodes"])
    indexes = build_zone_indexes(profiles)
    findings = build_edge_findings(
        pair_graph_report["edges"],
        profiles,
        _resource_verdicts(resource_report),
        _timing_verdicts(timing_report),
        indexes["unsupported_required_zones"],
    )
    verdict_counts = Counter(finding["verdict"] for finding in findings)
    manual_review_findings = sum(1 for finding in findings if finding["manual_review_required"])
    return {
        "version": "M35-C4",
        "description": "Zone/target compatibility detector for selected first-slice pair graph",
        "selected_target": pair_graph_report["selected_target"],
        "source_inputs": {
            "pair_compatibility_graph": str(PAIR_GRAPH_REPORT.relative_to(ROOT)),
            "resource_conflict_detector": str(RESOURCE_REPORT.relative_to(ROOT)),
            "timing_compatibility_detector": str(TIMING_REPORT.relative_to(ROOT)),
        },
        "scope": {
            "advisory_only": True,
            "resource_conflict_detector": False,
            "timing_compatibility_detector": False,
            "zone_target_compatibility_detector": True,
            "deck_skeleton": False,
            "bot_playbook": False,
            "runtime_effect_execution": False,
        },
        "policy": {
            "vanguard_circle_conflict_is_advisory_until_archetype_review": True,
            "rear_guard_slot_pressure_is_not_exact_board_capacity": True,
            "manual_review_blocks_high_confidence": True,
            "does_not_mutate_card_data": True,
            "does_not_publish_to_runtime_or_bot": True,
        },
        "zone_requirement_map": ZONE_REQUIREMENTS,
        "zone_provider_map": ZONE_PROVIDERS,
        "summary": {
            "node_count": len(profiles),
            "source_edge_count": len(pair_graph_report["edges"]),
            "zone_relevant_edge_count": len(findings),
            "manual_review_zone_edge_count": manual_review_findings,
            "unsupported_required_zone_count": len(indexes["unsupported_required_zones"]),
            "verdict_counts": dict(sorted(verdict_counts.items())),
            "ready_for_m35_c5": True,
        },
        "zone_indexes": indexes,
        "card_zone_profiles": [profiles[card_id] for card_id in sorted(profiles)],
        "edge_zone_findings": findings,
        "next_target": "M35-C5",
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def _top_findings(findings: Sequence[dict[str, Any]], limit: int = 12) -> list[dict[str, Any]]:
    return sorted(
        findings,
        key=lambda item: (
            item["verdict"] != "vanguard_role_conflict",
            item["verdict"] != "rear_guard_slot_pressure",
            -item["zone_pressure_score"],
            item["source_card_id"],
            item["target_card_id"],
        ),
    )[:limit]


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    summary = report["summary"]
    lines = [
        "# M35-C4 Zone / Target Compatibility Detector",
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
        f"- Zone-relevant edges: `{summary['zone_relevant_edge_count']}`",
        f"- Manual-review zone edges: `{summary['manual_review_zone_edge_count']}`",
        f"- Unsupported required zone types: `{summary['unsupported_required_zone_count']}`",
        f"- Ready for M35-C5: `{summary['ready_for_m35_c5']}`",
        "",
        "## Verdict Counts",
        "",
    ]
    for verdict, count in summary["verdict_counts"].items():
        lines.append(f"- `{verdict}`: `{count}`")
    lines.extend(["", "## Zone Requirement Counts", ""])
    for zone, count in report["zone_indexes"]["requirement_counts"].items():
        lines.append(f"- `{zone}` requirement: `{count}`")
    lines.extend(["", "## Zone Provider Counts", ""])
    for zone, count in report["zone_indexes"]["provider_counts"].items():
        lines.append(f"- `{zone}` provider: `{count}`")
    lines.extend(["", "## Zone Findings To Review First", ""])
    for finding in _top_findings(report["edge_zone_findings"]):
        review = " review-required" if finding["manual_review_required"] else ""
        lines.append(
            f"- `{finding['edge']}` verdict=`{finding['verdict']}` "
            f"shared=`{', '.join(finding['shared_required_zones']) or 'none'}` "
            f"supported=`{', '.join(finding['supported_zones']) or 'none'}`{review}"
        )
    lines.extend(
        [
            "",
            "## Scope",
            "",
            "- Advisory zone/target detector only.",
            "- Vanguard-circle conflicts are archetype-review signals, not final rejection.",
            "- Rear-guard slot pressure is coarse, not exact board-capacity calculation.",
            "- No deck skeleton or bot playbook promotion.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35-C4 selected-slice zone/target detector.")
    parser.add_argument("--pair-graph", type=Path, default=PAIR_GRAPH_REPORT)
    parser.add_argument("--resource-report", type=Path, default=RESOURCE_REPORT)
    parser.add_argument("--timing-report", type=Path, default=TIMING_REPORT)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    pair_graph_report = load_json(args.pair_graph)
    resource_report = load_json(args.resource_report) if args.resource_report.exists() else None
    timing_report = load_json(args.timing_report) if args.timing_report.exists() else None
    report = build_report(pair_graph_report, resource_report, timing_report)
    json_path = args.output_dir / "m35_c4_first_slice_zone_target_detector.json"
    md_path = args.output_dir / "m35_c4_first_slice_zone_target_detector.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35-C4 zone/target detector wrote {json_path}")
    print(f"M35-C4 zone/target summary wrote {md_path}")
    print(
        "zone_edges={edges} manual_review_edges={manual} unsupported_zones={unsupported} ready_for_m35_c5={ready}".format(
            edges=report["summary"]["zone_relevant_edge_count"],
            manual=report["summary"]["manual_review_zone_edge_count"],
            unsupported=report["summary"]["unsupported_required_zone_count"],
            ready=report["summary"]["ready_for_m35_c5"],
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
