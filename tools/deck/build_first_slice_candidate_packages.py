"""Build candidate packages from selected-slice compatibility output.

M35-D1 of the Hybrid Vertical-Slice Strategy.

This selects human-reviewable candidate card packages from clean C5 synergy
edges. It does not assign deck quantities or create a playbook.
"""

from __future__ import annotations

import argparse
import json
import math
import sys
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
PAIR_GRAPH_REPORT = ROOT / "outputs" / "target_slice" / "m35_c1_first_slice_pair_compatibility_graph.json"
COMPATIBILITY_REPORT = (
    ROOT / "outputs" / "target_slice" / "m35_c5_first_slice_selected_compatibility_output.json"
)
OUTPUT_DIR = ROOT / "outputs" / "target_slice"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _node_index(pair_graph_report: dict[str, Any]) -> dict[str, dict[str, Any]]:
    return {node["card_id"]: node for node in pair_graph_report["nodes"]}


def _clean_edges(compatibility_report: dict[str, Any]) -> list[dict[str, Any]]:
    return [
        edge
        for edge in compatibility_report["compatibility_edges"]
        if edge.get("candidate_for_m35_d1")
        and edge["status"] == "synergy"
        and "manual_review_required" not in edge["labels"]
        and "missing_data" not in edge["labels"]
        and "conflict" not in edge["labels"]
    ]


def _grade_counts(card_ids: Sequence[str], nodes: dict[str, dict[str, Any]]) -> dict[str, int]:
    counts = Counter(str(nodes[card_id].get("grade", "")) for card_id in card_ids)
    return dict(sorted(counts.items()))


def _trigger_counts(card_ids: Sequence[str], nodes: dict[str, dict[str, Any]]) -> dict[str, int]:
    counts = Counter(nodes[card_id].get("trigger", "") or "none" for card_id in card_ids)
    return dict(sorted(counts.items()))


def _role_counts(edges: Sequence[dict[str, Any]], anchor: str) -> dict[str, int]:
    return {
        "as_provider_source": sum(1 for edge in edges if edge["source_card_id"] == anchor),
        "as_consumer_target": sum(1 for edge in edges if edge["target_card_id"] == anchor),
    }


def _package_score(edges: Sequence[dict[str, Any]], card_count: int) -> float:
    if not edges:
        return 0.0
    total = sum(edge["net_score"] for edge in edges)
    average = total / len(edges)
    density_bonus = len(edges) / max(1.0, math.sqrt(card_count))
    return round(average + density_bonus, 3)


def build_packages(
    pair_graph_report: dict[str, Any],
    compatibility_report: dict[str, Any],
    max_edges_per_anchor: int = 12,
    max_packages: int = 25,
) -> list[dict[str, Any]]:
    nodes = _node_index(pair_graph_report)
    clean_edges = _clean_edges(compatibility_report)
    incident_edges: dict[str, list[dict[str, Any]]] = defaultdict(list)
    for edge in clean_edges:
        incident_edges[edge["source_card_id"]].append(edge)
        incident_edges[edge["target_card_id"]].append(edge)

    packages = []
    seen_card_sets: set[tuple[str, ...]] = set()
    for anchor, edges in sorted(incident_edges.items()):
        selected_edges = sorted(edges, key=lambda edge: (-edge["net_score"], edge["edge"]))[:max_edges_per_anchor]
        card_ids = sorted(
            {
                anchor,
                *(edge["source_card_id"] for edge in selected_edges),
                *(edge["target_card_id"] for edge in selected_edges),
            }
        )
        if len(card_ids) < 2:
            continue
        card_set_key = tuple(card_ids)
        if card_set_key in seen_card_sets:
            continue
        seen_card_sets.add(card_set_key)
        edge_keys = [edge["edge"] for edge in selected_edges]
        packages.append(
            {
                "package_id": f"pkg_{len(packages) + 1:03d}",
                "anchor_card_id": anchor,
                "anchor_name_th": nodes[anchor]["name_th"],
                "card_ids": card_ids,
                "cards": [
                    {
                        "card_id": card_id,
                        "name_th": nodes[card_id]["name_th"],
                        "grade": nodes[card_id]["grade"],
                        "trigger": nodes[card_id]["trigger"],
                        "type_1": nodes[card_id]["type_1"],
                    }
                    for card_id in card_ids
                ],
                "edge_count": len(selected_edges),
                "edge_keys": edge_keys,
                "net_score": _package_score(selected_edges, len(card_ids)),
                "grade_counts": _grade_counts(card_ids, nodes),
                "trigger_counts": _trigger_counts(card_ids, nodes),
                "anchor_role_counts": _role_counts(selected_edges, anchor),
                "selection_reasons": [
                    "built_from_clean_m35_c5_synergy_edges",
                    "manual_review_missing_data_and_conflict_edges_excluded",
                    "candidate_package_only_no_deck_quantities",
                ],
                "candidate_for_m35_d2": True,
            }
        )
    selected = sorted(
        packages,
        key=lambda package: (-package["net_score"], -package["edge_count"], package["anchor_card_id"]),
    )[:max_packages]
    for index, package in enumerate(selected, start=1):
        package["package_id"] = f"pkg_{index:03d}"
    return selected


def build_report(
    pair_graph_report: dict[str, Any] | None = None,
    compatibility_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    pair_graph_report = pair_graph_report or load_json(PAIR_GRAPH_REPORT)
    compatibility_report = compatibility_report or load_json(COMPATIBILITY_REPORT)
    clean_edges = _clean_edges(compatibility_report)
    packages = build_packages(pair_graph_report, compatibility_report)
    return {
        "version": "M35-D1",
        "description": "Candidate package selection from selected-slice compatibility output",
        "selected_target": compatibility_report["selected_target"],
        "source_inputs": {
            "pair_compatibility_graph": str(PAIR_GRAPH_REPORT.relative_to(ROOT)),
            "selected_compatibility_output": str(COMPATIBILITY_REPORT.relative_to(ROOT)),
        },
        "scope": {
            "advisory_only": True,
            "candidate_package_selection": True,
            "deck_skeleton": False,
            "deck_quantities": False,
            "bot_playbook": False,
            "runtime_effect_execution": False,
        },
        "policy": {
            "only_clean_synergy_edges": True,
            "no_manual_review_edges": True,
            "no_missing_data_edges": True,
            "no_conflict_edges": True,
            "packages_are_inputs_not_final_deck_choices": True,
        },
        "summary": {
            "clean_edge_count": len(clean_edges),
            "package_count": len(packages),
            "ready_for_m35_d2": True,
        },
        "packages": packages,
        "next_target": "M35-D2",
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    summary = report["summary"]
    lines = [
        "# M35-D1 Candidate Package Selection",
        "",
        "## Selected Target",
        "",
        f"- Slice: `{target['slice']}`",
        f"- Era preset: `{target['era_preset']}`",
        f"- Group: `{target['group']}`",
        "",
        "## Summary",
        "",
        f"- Clean synergy edges: `{summary['clean_edge_count']}`",
        f"- Candidate packages: `{summary['package_count']}`",
        f"- Ready for M35-D2: `{summary['ready_for_m35_d2']}`",
        "",
        "## Top Packages",
        "",
    ]
    for package in report["packages"][:15]:
        lines.append(
            f"- `{package['package_id']}` anchor=`{package['anchor_card_id']}` "
            f"cards=`{len(package['card_ids'])}` edges=`{package['edge_count']}` "
            f"score=`{package['net_score']}` grades=`{package['grade_counts']}`"
        )
    lines.extend(
        [
            "",
            "## Scope",
            "",
            "- Candidate package selection only.",
            "- No deck quantities or skeleton yet.",
            "- No bot/playbook promotion.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35-D1 selected-slice candidate packages.")
    parser.add_argument("--pair-graph", type=Path, default=PAIR_GRAPH_REPORT)
    parser.add_argument("--compatibility-report", type=Path, default=COMPATIBILITY_REPORT)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_report(load_json(args.pair_graph), load_json(args.compatibility_report))
    json_path = args.output_dir / "m35_d1_first_slice_candidate_packages.json"
    md_path = args.output_dir / "m35_d1_first_slice_candidate_packages.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35-D1 candidate packages wrote {json_path}")
    print(f"M35-D1 candidate package summary wrote {md_path}")
    print(
        "clean_edges={edges} packages={packages} ready_for_m35_d2={ready}".format(
            edges=report["summary"]["clean_edge_count"],
            packages=report["summary"]["package_count"],
            ready=report["summary"]["ready_for_m35_d2"],
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
