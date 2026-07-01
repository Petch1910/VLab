"""Build pair compatibility graph for the selected first slice.

M35-C1 of the Hybrid Vertical-Slice Strategy.

This graph connects provider cards to consumer cards when the M35-B3
requirement/provider model finds an advisory match. It is not a deck skeleton
and does not promote any pair into a bot playbook.
"""

from __future__ import annotations

import argparse
import json
import sys
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any, Iterable, Sequence


ROOT = Path(__file__).resolve().parents[2]
REQUIREMENT_PROVIDER_REPORT = (
    ROOT / "outputs" / "target_slice" / "m35_b3_first_slice_requirement_provider_model.json"
)
MANUAL_REVIEW_REPORT = ROOT / "outputs" / "target_slice" / "m35_b4_first_slice_manual_review_queue.json"
OUTPUT_DIR = ROOT / "outputs" / "target_slice"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


COMPATIBILITY_RULES: list[dict[str, Any]] = [
    {
        "id": "resource_counter_blast_recovery",
        "category": "resource_support",
        "provider": "provides_counter_charge",
        "supports_requirements": ["requires_counter_blast"],
        "weight": 3,
        "reason": "CounterCharge providers help cards that need CounterBlast.",
    },
    {
        "id": "resource_counter_blast_sustain",
        "category": "resource_support",
        "provider": "provides_counter_blast_sustain",
        "supports_requirements": ["requires_counter_blast"],
        "weight": 3,
        "reason": "Self-sustain style CounterBlast support can reduce deck-level CB pressure.",
    },
    {
        "id": "resource_soul_charge_support",
        "category": "resource_support",
        "provider": "provides_soul_charge",
        "supports_requirements": ["requires_soul_blast", "requires_soul_reference"],
        "weight": 3,
        "reason": "SoulCharge providers help cards that spend or reference Soul.",
    },
    {
        "id": "resource_soul_resource_support",
        "category": "resource_support",
        "provider": "provides_soul_resource",
        "supports_requirements": ["requires_soul_blast", "requires_soul_reference"],
        "weight": 3,
        "reason": "Move-to-Soul style providers help cards that spend or reference Soul.",
    },
    {
        "id": "resource_soul_blast_sustain",
        "category": "resource_support",
        "provider": "provides_soul_blast_sustain",
        "supports_requirements": ["requires_soul_blast", "requires_soul_reference"],
        "weight": 3,
        "reason": "SoulBlast sustain providers reduce deck-level Soul pressure.",
    },
    {
        "id": "board_extension_for_rear_guard_needs",
        "category": "board_support",
        "provider": "provides_board_extension",
        "supports_requirements": ["requires_rear_guard_circle", "requires_boosted"],
        "weight": 2,
        "reason": "Superior-call or board-extension effects help RC and boosted-condition cards.",
    },
    {
        "id": "search_for_grade_or_setup_needs",
        "category": "consistency_support",
        "provider": "provides_search",
        "supports_requirements": ["requires_grade_requirement", "requires_on_ride_timing"],
        "weight": 2,
        "reason": "Search providers improve access to setup pieces and ride-timing pieces.",
    },
    {
        "id": "card_advantage_for_cost_pressure",
        "category": "consistency_support",
        "provider": "provides_card_advantage",
        "supports_requirements": ["requires_discard", "requires_rear_guard_circle"],
        "weight": 1,
        "reason": "Card advantage can help pay hand/card-count pressure in a deck package.",
    },
    {
        "id": "power_pressure_for_hit_conditions",
        "category": "battle_pressure_support",
        "provider": "provides_power_pressure",
        "supports_requirements": ["requires_attack_hit", "requires_hit_timing", "requires_boosted"],
        "weight": 2,
        "reason": "Power pressure helps hit-based and boosted battle requirements matter.",
    },
    {
        "id": "critical_pressure_for_hit_conditions",
        "category": "battle_pressure_support",
        "provider": "provides_critical_pressure",
        "supports_requirements": ["requires_attack_hit", "requires_hit_timing"],
        "weight": 2,
        "reason": "Critical pressure forces guard decisions around hit-based requirements.",
    },
    {
        "id": "multi_attack_for_attack_timing",
        "category": "battle_pressure_support",
        "provider": "provides_restand_or_multi_attack",
        "supports_requirements": ["requires_on_attack_timing", "requires_attack_hit", "requires_hit_timing"],
        "weight": 2,
        "reason": "Restand or multi-attack providers create more attack windows.",
    },
    {
        "id": "guard_restrict_for_hit_conditions",
        "category": "battle_pressure_support",
        "provider": "provides_guard_restrict",
        "supports_requirements": ["requires_attack_hit", "requires_hit_timing"],
        "weight": 2,
        "reason": "Guard restriction improves odds for hit-based consumers.",
    },
    {
        "id": "trigger_value_for_trigger_windows",
        "category": "trigger_support",
        "provider": "provides_trigger_check_value",
        "supports_requirements": ["requires_trigger_check_timing"],
        "weight": 1,
        "reason": "Trigger-value providers participate in trigger-check timing packages.",
    },
    {
        "id": "stand_trigger_for_attack_windows",
        "category": "trigger_support",
        "provider": "provides_stand_trigger",
        "supports_requirements": ["requires_on_attack_timing", "requires_attack_hit", "requires_hit_timing"],
        "weight": 1,
        "reason": "Stand triggers can create additional battle pressure for attack-window consumers.",
    },
    {
        "id": "critical_trigger_for_damage_pressure",
        "category": "trigger_support",
        "provider": "provides_critical_trigger",
        "supports_requirements": ["requires_attack_hit", "requires_hit_timing"],
        "weight": 1,
        "reason": "Critical triggers increase pressure around hit-based consumers.",
    },
    {
        "id": "draw_trigger_for_card_advantage",
        "category": "trigger_support",
        "provider": "provides_draw_trigger",
        "supports_requirements": ["requires_rear_guard_circle"],
        "weight": 1,
        "reason": "Draw triggers can help maintain board-card availability.",
    },
]


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _sorted_unique(values: Iterable[str]) -> list[str]:
    return sorted({value for value in values if value})


def _manual_review_ids(report: dict[str, Any] | None, cards: Sequence[dict[str, Any]]) -> set[str]:
    if report:
        return {item["card_id"] for item in report.get("manual_review_queue", [])}
    return {card["card_id"] for card in cards if card.get("manual_review_required")}


def _match_rule(provider: str, requirement: str) -> dict[str, Any] | None:
    for rule in COMPATIBILITY_RULES:
        if provider == rule["provider"] and requirement in rule["supports_requirements"]:
            return rule
    return None


def _edge_policy(source: dict[str, Any], target: dict[str, Any], manual_review_ids: set[str]) -> dict[str, Any]:
    reasons: list[str] = []
    if source["card_id"] in manual_review_ids or source.get("manual_review_required"):
        reasons.append("source_manual_review_required")
    if target["card_id"] in manual_review_ids or target.get("manual_review_required"):
        reasons.append("target_manual_review_required")
    return {
        "manual_review_required": bool(reasons),
        "manual_review_reasons": reasons,
        "confidence": "review_required" if reasons else "advisory",
        "allowed_next_use": (
            "manual_review_required_before_high_confidence_compatibility"
            if reasons
            else "candidate_for_m35_c2_resource_check"
        ),
    }


def build_edges(cards: Sequence[dict[str, Any]], manual_review_ids: set[str]) -> list[dict[str, Any]]:
    edge_map: dict[tuple[str, str], dict[str, Any]] = {}
    for source in cards:
        providers = source.get("providers", [])
        if not providers:
            continue
        for target in cards:
            if source["card_id"] == target["card_id"]:
                continue
            requirements = target.get("requirements", [])
            if not requirements:
                continue
            matches: list[dict[str, Any]] = []
            for provider in providers:
                for requirement in requirements:
                    rule = _match_rule(provider, requirement)
                    if rule:
                        matches.append(
                            {
                                "rule_id": rule["id"],
                                "category": rule["category"],
                                "source_provider": provider,
                                "target_requirement": requirement,
                                "weight": rule["weight"],
                                "reason": rule["reason"],
                            }
                        )
            if not matches:
                continue
            key = (source["card_id"], target["card_id"])
            policy = _edge_policy(source, target, manual_review_ids)
            edge_map[key] = {
                "source_card_id": source["card_id"],
                "source_name_th": source["name_th"],
                "target_card_id": target["card_id"],
                "target_name_th": target["name_th"],
                "direction": "provider_to_consumer",
                "relation": "supports",
                "score": sum(match["weight"] for match in matches),
                "categories": _sorted_unique(match["category"] for match in matches),
                "rule_ids": _sorted_unique(match["rule_id"] for match in matches),
                "source_providers": _sorted_unique(match["source_provider"] for match in matches),
                "target_requirements": _sorted_unique(match["target_requirement"] for match in matches),
                "matches": sorted(matches, key=lambda match: (match["category"], match["rule_id"], match["target_requirement"])),
                **policy,
            }
    return sorted(
        edge_map.values(),
        key=lambda edge: (
            edge["manual_review_required"],
            -edge["score"],
            edge["source_card_id"],
            edge["target_card_id"],
        ),
    )


def build_indexes(edges: Sequence[dict[str, Any]]) -> dict[str, Any]:
    by_source: dict[str, list[str]] = defaultdict(list)
    by_target: dict[str, list[str]] = defaultdict(list)
    by_category: dict[str, list[str]] = defaultdict(list)
    by_rule: dict[str, list[str]] = defaultdict(list)
    category_counts: Counter[str] = Counter()
    rule_counts: Counter[str] = Counter()
    for edge in edges:
        edge_key = f"{edge['source_card_id']}->{edge['target_card_id']}"
        by_source[edge["source_card_id"]].append(edge_key)
        by_target[edge["target_card_id"]].append(edge_key)
        for category in edge["categories"]:
            by_category[category].append(edge_key)
            category_counts[category] += 1
        for rule_id in edge["rule_ids"]:
            by_rule[rule_id].append(edge_key)
            rule_counts[rule_id] += 1
    return {
        "by_source": {key: sorted(value) for key, value in sorted(by_source.items())},
        "by_target": {key: sorted(value) for key, value in sorted(by_target.items())},
        "by_category": {key: sorted(value) for key, value in sorted(by_category.items())},
        "by_rule": {key: sorted(value) for key, value in sorted(by_rule.items())},
        "category_counts": dict(sorted(category_counts.items())),
        "rule_counts": dict(sorted(rule_counts.items())),
    }


def build_report(
    requirement_provider_report: dict[str, Any] | None = None,
    manual_review_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    requirement_provider_report = requirement_provider_report or load_json(REQUIREMENT_PROVIDER_REPORT)
    if manual_review_report is None and MANUAL_REVIEW_REPORT.exists():
        manual_review_report = load_json(MANUAL_REVIEW_REPORT)
    cards = requirement_provider_report["cards"]
    manual_ids = _manual_review_ids(manual_review_report, cards)
    edges = build_edges(cards, manual_ids)
    indexes = build_indexes(edges)
    manual_review_edge_count = sum(1 for edge in edges if edge["manual_review_required"])
    return {
        "version": "M35-C1",
        "description": "Pair compatibility graph for selected first slice",
        "selected_target": requirement_provider_report["selected_target"],
        "source_inputs": {
            "requirement_provider_model": str(REQUIREMENT_PROVIDER_REPORT.relative_to(ROOT)),
            "manual_review_queue": str(MANUAL_REVIEW_REPORT.relative_to(ROOT)),
        },
        "scope": {
            "advisory_only": True,
            "pair_compatibility_graph": True,
            "resource_conflict_detector": False,
            "timing_compatibility_detector": False,
            "zone_target_compatibility_detector": False,
            "deck_skeleton": False,
            "bot_playbook": False,
            "runtime_effect_execution": False,
        },
        "policy": {
            "manual_review_blocks_high_confidence": True,
            "edges_are_candidates_not_deck_slots": True,
            "does_not_mutate_card_data": True,
            "does_not_publish_to_runtime_or_bot": True,
        },
        "compatibility_rules": COMPATIBILITY_RULES,
        "summary": {
            "node_count": len(cards),
            "edge_count": len(edges),
            "manual_review_card_count": len(manual_ids),
            "manual_review_edge_count": manual_review_edge_count,
            "advisory_edge_count": len(edges) - manual_review_edge_count,
            "category_count": len(indexes["by_category"]),
            "rule_count": len(indexes["by_rule"]),
            "ready_for_m35_c2": True,
        },
        "nodes": [
            {
                "card_id": card["card_id"],
                "name_th": card["name_th"],
                "series_code": card["series_code"],
                "grade": card["grade"],
                "trigger": card["trigger"],
                "type_1": card["type_1"],
                "requirements": card.get("requirements", []),
                "providers": card.get("providers", []),
                "manual_review_required": card["card_id"] in manual_ids or card.get("manual_review_required", False),
            }
            for card in cards
        ],
        "edges": edges,
        "indexes": indexes,
        "next_target": "M35-C2",
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def _top_edges(edges: Sequence[dict[str, Any]], limit: int = 12) -> list[dict[str, Any]]:
    return sorted(edges, key=lambda edge: (-edge["score"], edge["source_card_id"], edge["target_card_id"]))[:limit]


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    summary = report["summary"]
    lines = [
        "# M35-C1 Pair Compatibility Graph",
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
        f"- Edges: `{summary['edge_count']}`",
        f"- Advisory edges: `{summary['advisory_edge_count']}`",
        f"- Manual-review edges: `{summary['manual_review_edge_count']}`",
        f"- Categories: `{summary['category_count']}`",
        f"- Rules applied: `{summary['rule_count']}`",
        f"- Ready for M35-C2: `{summary['ready_for_m35_c2']}`",
        "",
        "## Category Counts",
        "",
    ]
    for category, count in report["indexes"]["category_counts"].items():
        lines.append(f"- `{category}`: `{count}`")
    lines.extend(["", "## Top Advisory Edges", ""])
    for edge in _top_edges(report["edges"]):
        review = " review-required" if edge["manual_review_required"] else ""
        lines.append(
            f"- `{edge['source_card_id']}` -> `{edge['target_card_id']}` "
            f"score=`{edge['score']}` categories=`{', '.join(edge['categories'])}`{review}"
        )
    lines.extend(
        [
            "",
            "## Scope",
            "",
            "- Advisory pair graph only.",
            "- No resource conflict verdict yet.",
            "- No timing compatibility verdict yet.",
            "- No zone/target compatibility verdict yet.",
            "- No deck skeleton or bot playbook promotion.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35-C1 selected-slice pair compatibility graph.")
    parser.add_argument("--requirement-provider", type=Path, default=REQUIREMENT_PROVIDER_REPORT)
    parser.add_argument("--manual-review", type=Path, default=MANUAL_REVIEW_REPORT)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    requirement_provider_report = load_json(args.requirement_provider)
    manual_review_report = load_json(args.manual_review) if args.manual_review.exists() else None
    report = build_report(requirement_provider_report, manual_review_report)
    json_path = args.output_dir / "m35_c1_first_slice_pair_compatibility_graph.json"
    md_path = args.output_dir / "m35_c1_first_slice_pair_compatibility_graph.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35-C1 pair compatibility graph wrote {json_path}")
    print(f"M35-C1 pair compatibility summary wrote {md_path}")
    print(
        "nodes={nodes} edges={edges} manual_review_edges={manual} ready_for_m35_c2={ready}".format(
            nodes=report["summary"]["node_count"],
            edges=report["summary"]["edge_count"],
            manual=report["summary"]["manual_review_edge_count"],
            ready=report["summary"]["ready_for_m35_c2"],
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
