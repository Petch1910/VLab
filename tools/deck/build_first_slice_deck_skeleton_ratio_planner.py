"""Build deck skeleton ratio plans for selected first-slice packages.

M35-D2 of the Hybrid Vertical-Slice Strategy.

This turns D1 candidate packages into advisory deck-curve skeleton plans. It
does not choose per-card quantities and does not create a final deck list.
"""

from __future__ import annotations

import argparse
import json
import sqlite3
import sys
from contextlib import closing
from collections import Counter
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
CANDIDATE_PACKAGES_REPORT = ROOT / "outputs" / "target_slice" / "m35_d1_first_slice_candidate_packages.json"
PAIR_GRAPH_REPORT = ROOT / "outputs" / "target_slice" / "m35_c1_first_slice_pair_compatibility_graph.json"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"
OUTPUT_DIR = ROOT / "outputs" / "target_slice"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


CLASSIC_RATIO_TARGET = {
    "main_deck_size": 50,
    "trigger_slots": 16,
    "normal_unit_slots": 34,
    "grade_targets": {
        "0": {"target": 17, "role": "starter_plus_triggers"},
        "1": {"target": 14, "role": "early_guard_and_boost"},
        "2": {"target": 11, "role": "midgame_attackers"},
        "3": {"target": 8, "role": "main_vanguard_and_finishers"},
    },
    "trigger_ratio_bands": {
        "Heal": {"min": 4, "max": 4, "note": "classic default guardrail"},
        "Stand": {"min": 4, "max": 8, "note": "Nova Grappler pressure candidate"},
        "Critical": {"min": 4, "max": 8, "note": "damage pressure candidate"},
        "Draw": {"min": 0, "max": 4, "note": "hand smoothing candidate"},
    },
}


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def load_card_rows(card_ids: Sequence[str], sqlite_path: Path = CARDS_SQLITE) -> dict[str, dict[str, Any]]:
    if not sqlite_path.exists():
        raise FileNotFoundError(f"Required SQLite pack not found: {sqlite_path}")
    placeholders = ",".join("?" for _ in card_ids)
    query = (
        "select card_id, name_th, grade, power, shield, trigger, deck_limit, "
        "type_1, clan, nation, series_code from cards where card_id in ({})"
    ).format(placeholders)
    with closing(sqlite3.connect(sqlite_path)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, list(card_ids)).fetchall() if card_ids else []
    return {row["card_id"]: dict(row) for row in rows}


def _node_index(pair_graph_report: dict[str, Any]) -> dict[str, dict[str, Any]]:
    return {node["card_id"]: node for node in pair_graph_report["nodes"]}


def _edge_degree(edge_keys: Sequence[str]) -> Counter[str]:
    degree: Counter[str] = Counter()
    for edge_key in edge_keys:
        source, target = edge_key.split("->", 1)
        degree[source] += 1
        degree[target] += 1
    return degree


def _shield_profile(cards: Sequence[dict[str, Any]]) -> dict[str, Any]:
    shield_values = [card["shield"] for card in cards if card.get("shield") is not None]
    high_shield = [card["card_id"] for card in cards if (card.get("shield") or 0) >= 10000]
    return {
        "cards_with_shield_value": len(shield_values),
        "average_shield": round(sum(shield_values) / len(shield_values), 2) if shield_values else None,
        "max_shield": max(shield_values) if shield_values else None,
        "high_shield_candidate_ids": sorted(high_shield),
        "limited_by_package_not_full_deck": True,
    }


def _card_roles(
    package: dict[str, Any],
    card_rows: dict[str, dict[str, Any]],
    node_index: dict[str, dict[str, Any]],
) -> dict[str, list[str]]:
    degree = _edge_degree(package["edge_keys"])
    key_cards = [package["anchor_card_id"]]
    key_cards.extend(card_id for card_id, count in degree.items() if count >= 3 and card_id != package["anchor_card_id"])
    trigger_cards = [
        card_id
        for card_id in package["card_ids"]
        if card_rows.get(card_id, {}).get("trigger")
    ]
    resource_recovery_cards = [
        card_id
        for card_id in package["card_ids"]
        if any(
            provider
            in {
                "provides_counter_charge",
                "provides_counter_blast_sustain",
                "provides_soul_charge",
                "provides_soul_resource",
                "provides_soul_blast_sustain",
                "provides_card_advantage",
            }
            for provider in node_index.get(card_id, {}).get("providers", [])
        )
    ]
    support_cards = [
        card_id
        for card_id in package["card_ids"]
        if card_id not in set(key_cards) and card_id not in set(trigger_cards)
    ]
    return {
        "key_cards": sorted(set(key_cards)),
        "support_cards": sorted(support_cards),
        "trigger_cards": sorted(trigger_cards),
        "resource_recovery_cards": sorted(set(resource_recovery_cards)),
    }


def build_skeleton(package: dict[str, Any], card_rows: dict[str, dict[str, Any]], node_index: dict[str, dict[str, Any]]) -> dict[str, Any]:
    cards = [card_rows[card_id] for card_id in package["card_ids"] if card_id in card_rows]
    grade_counts = Counter(str(card.get("grade", "")) for card in cards)
    trigger_counts = Counter(card.get("trigger") or "none" for card in cards)
    roles = _card_roles(package, card_rows, node_index)
    return {
        "skeleton_id": package["package_id"].replace("pkg_", "skel_"),
        "source_package_id": package["package_id"],
        "anchor_card_id": package["anchor_card_id"],
        "anchor_name_th": package["anchor_name_th"],
        "package_net_score": package["net_score"],
        "ratio_targets": CLASSIC_RATIO_TARGET,
        "package_grade_counts": dict(sorted(grade_counts.items())),
        "package_trigger_counts": dict(sorted(trigger_counts.items())),
        "roles": roles,
        "guard_shield_profile": _shield_profile(cards),
        "known_gaps": [
            "no_per_card_quantities_until_d2_review_is_accepted",
            "shield_profile_is_package_only_not_full_deck",
            "trigger_ratio_is_band_based_not_final_trigger_list",
        ],
        "candidate_for_m35_d3": True,
    }


def build_report(
    candidate_packages_report: dict[str, Any] | None = None,
    pair_graph_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    candidate_packages_report = candidate_packages_report or load_json(CANDIDATE_PACKAGES_REPORT)
    pair_graph_report = pair_graph_report or load_json(PAIR_GRAPH_REPORT)
    node_index = _node_index(pair_graph_report)
    all_card_ids = sorted({card_id for package in candidate_packages_report["packages"] for card_id in package["card_ids"]})
    card_rows = load_card_rows(all_card_ids)
    skeletons = [
        build_skeleton(package, card_rows, node_index)
        for package in candidate_packages_report["packages"]
    ]
    return {
        "version": "M35-D2",
        "description": "Deck skeleton ratio planner for selected first-slice candidate packages",
        "selected_target": candidate_packages_report["selected_target"],
        "source_inputs": {
            "candidate_packages": str(CANDIDATE_PACKAGES_REPORT.relative_to(ROOT)),
            "pair_compatibility_graph": str(PAIR_GRAPH_REPORT.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "scope": {
            "advisory_only": True,
            "deck_skeleton_ratio_planner": True,
            "final_deck_list": False,
            "per_card_quantities": False,
            "bot_playbook": False,
            "runtime_effect_execution": False,
        },
        "policy": {
            "ratio_targets_not_final_deck": True,
            "no_card_quantities": True,
            "uses_runtime_sqlite_for_power_shield": True,
            "does_not_publish_to_runtime_or_bot": True,
        },
        "summary": {
            "source_package_count": len(candidate_packages_report["packages"]),
            "skeleton_count": len(skeletons),
            "cards_loaded_from_sqlite": len(card_rows),
            "ready_for_m35_d3": True,
        },
        "skeletons": skeletons,
        "next_target": "M35-D3",
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    summary = report["summary"]
    lines = [
        "# M35-D2 Deck Skeleton Ratio Planner",
        "",
        "## Selected Target",
        "",
        f"- Slice: `{target['slice']}`",
        f"- Era preset: `{target['era_preset']}`",
        f"- Group: `{target['group']}`",
        "",
        "## Summary",
        "",
        f"- Source packages: `{summary['source_package_count']}`",
        f"- Skeletons: `{summary['skeleton_count']}`",
        f"- Cards loaded from SQLite: `{summary['cards_loaded_from_sqlite']}`",
        f"- Ready for M35-D3: `{summary['ready_for_m35_d3']}`",
        "",
        "## Ratio Target",
        "",
        f"- Main deck size: `{CLASSIC_RATIO_TARGET['main_deck_size']}`",
        f"- Trigger slots: `{CLASSIC_RATIO_TARGET['trigger_slots']}`",
        f"- Normal unit slots: `{CLASSIC_RATIO_TARGET['normal_unit_slots']}`",
        "",
        "## Top Skeletons",
        "",
    ]
    for skeleton in report["skeletons"][:10]:
        lines.append(
            f"- `{skeleton['skeleton_id']}` package=`{skeleton['source_package_id']}` "
            f"anchor=`{skeleton['anchor_card_id']}` score=`{skeleton['package_net_score']}` "
            f"grades=`{skeleton['package_grade_counts']}` shield_avg=`{skeleton['guard_shield_profile']['average_shield']}`"
        )
    lines.extend(
        [
            "",
            "## Scope",
            "",
            "- Ratio skeleton planner only.",
            "- No per-card quantities or final deck list.",
            "- No bot/playbook promotion.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35-D2 deck skeleton ratio plans.")
    parser.add_argument("--candidate-packages", type=Path, default=CANDIDATE_PACKAGES_REPORT)
    parser.add_argument("--pair-graph", type=Path, default=PAIR_GRAPH_REPORT)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_report(load_json(args.candidate_packages), load_json(args.pair_graph))
    json_path = args.output_dir / "m35_d2_first_slice_deck_skeleton_ratio_plans.json"
    md_path = args.output_dir / "m35_d2_first_slice_deck_skeleton_ratio_plans.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35-D2 deck skeleton ratio plans wrote {json_path}")
    print(f"M35-D2 deck skeleton ratio summary wrote {md_path}")
    print(
        "source_packages={packages} skeletons={skeletons} ready_for_m35_d3={ready}".format(
            packages=report["summary"]["source_package_count"],
            skeletons=report["summary"]["skeleton_count"],
            ready=report["summary"]["ready_for_m35_d3"],
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
