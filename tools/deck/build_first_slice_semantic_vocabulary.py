"""Build advisory semantic vocabulary for the selected first slice.

M35-B1 of the Hybrid Vertical-Slice Strategy.

The vocabulary is an allowed tag set for offline semantic extraction in M35-B2.
It is not a runtime effect schema and must not execute card text.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
FEASIBILITY_REFRESH = ROOT / "outputs" / "target_slice" / "m35_a4_first_slice_feasibility_refresh.json"
RULE_TAXONOMY_JSON = ROOT / "outputs" / "research_2026_06_29_new_chat" / "vanguard_development" / "rule_taxonomy.json"
OUTPUT_DIR = ROOT / "outputs" / "target_slice"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def taxonomy_contains(taxonomy: dict[str, Any], category: str, term: str) -> bool:
    value = taxonomy.get(category)
    if isinstance(value, list):
        return term in value
    if isinstance(value, dict):
        return any(term in values for values in value.values() if isinstance(values, list))
    return False


def keyword_contains(taxonomy: dict[str, Any], category: str, term: str) -> bool:
    keywords = taxonomy.get("keywords", {})
    values = keywords.get(category, []) if isinstance(keywords, dict) else []
    return term in values


def build_vocabulary(
    feasibility_report: dict[str, Any] | None = None,
    taxonomy: dict[str, Any] | None = None,
) -> dict[str, Any]:
    feasibility_report = feasibility_report or load_json(FEASIBILITY_REFRESH)
    taxonomy = taxonomy or load_json(RULE_TAXONOMY_JSON)
    target = feasibility_report["selected_target"]

    ability_types = ["ACT", "AUTO", "CONT"]
    zones = [
        "deck",
        "hand",
        "drop",
        "field",
        "vanguard_circle",
        "rear_guard_circle",
        "guardian_circle",
        "soul",
        "damage",
        "trigger_zone",
    ]
    timing = [
        "stand_phase",
        "draw_phase",
        "ride_phase",
        "main_phase",
        "battle_phase_start",
        "attack_step",
        "guard_step",
        "drive_step",
        "damage_step",
        "close_step",
        "end_phase",
        "on_place",
        "on_ride",
        "on_call",
        "on_attack",
        "when_attack_hits",
        "when_boosts",
        "when_revealed_as_trigger",
    ]
    conditions = [
        "if_vanguard",
        "if_rear_guard",
        "if_boosted",
        "if_attack_hits",
        "if_limit_break_4",
        "if_counter_blast_available",
        "if_soul_available",
        "if_same_clan",
        "if_opponent_vanguard",
        "if_unit_stand",
        "if_unit_rest",
        "if_grade_requirement",
    ]
    costs = [
        "counter_blast",
        "soul_blast",
        "discard",
        "rest_this_unit",
        "retire_this_unit",
        "put_to_soul",
        "no_cost",
    ]
    effects = [
        "power_plus",
        "critical_plus",
        "stand_unit",
        "draw",
        "search_deck",
        "superior_call",
        "retire",
        "counter_charge",
        "soul_charge",
        "move_to_soul",
        "guard_restrict",
        "sentinel_guard",
        "shuffle_deck",
    ]
    durations = [
        "one_shot",
        "until_end_of_turn",
        "during_battle",
        "continuous_while_condition",
        "until_end_of_that_battle",
    ]
    mechanic_groups = [
        "core",
        "classic_keyword",
        "limit_break",
        "forerunner",
        "sentinel",
        "trigger_icon",
    ]

    source_term_checks = {
        "ability_types": {
            term: taxonomy_contains(taxonomy, "ability_types", term) for term in ability_types
        },
        "core_zones": {
            "Deck Zone": taxonomy_contains(taxonomy, "zones", "Deck Zone"),
            "Vanguard Circle": taxonomy_contains(taxonomy, "zones", "Vanguard Circle"),
            "Rear-guard Circle": taxonomy_contains(taxonomy, "zones", "Rear-guard Circle"),
            "Guardian Circle": taxonomy_contains(taxonomy, "zones", "Guardian Circle"),
            "Soul": taxonomy_contains(taxonomy, "zones", "Soul"),
            "Trigger Zone": taxonomy_contains(taxonomy, "zones", "Trigger Zone"),
        },
        "classic_keywords": {
            "Limit Break": keyword_contains(taxonomy, "activation_condition", "Limit Break"),
            "Forerunner": keyword_contains(taxonomy, "procedure", "Forerunner"),
            "Sentinel": keyword_contains(taxonomy, "basic", "Sentinel"),
            "Boost": keyword_contains(taxonomy, "basic", "Boost"),
            "Intercept": keyword_contains(taxonomy, "basic", "Intercept"),
        },
        "classic_triggers": {
            term: taxonomy_contains(taxonomy, "triggers", term)
            for term in ["Critical", "Draw", "Stand", "Heal"]
        },
    }
    missing_source_terms = [
        term
        for group in source_term_checks.values()
        for term, present in group.items()
        if not present
    ]
    readiness = feasibility_report.get("readiness", {})
    selected_slice_ready = bool(
        feasibility_report.get("phase_a_ready_for_phase_b")
        or readiness.get("semantic_scaleout_ready")
        or readiness.get("classic_core_policy_reusable")
    )

    return {
        "version": "M35-B1",
        "description": "Semantic vocabulary for selected Classic Core first slice",
        "selected_target": target,
        "source_inputs": {
            "feasibility_refresh": str(FEASIBILITY_REFRESH.relative_to(ROOT)),
            "rule_taxonomy": str(RULE_TAXONOMY_JSON.relative_to(ROOT)),
        },
        "scope": {
            "advisory_only": True,
            "runtime_effect_execution": False,
            "live_card_text_parser": False,
            "selected_slice_only": True,
            "notes": [
                "Use these tags as the allowed vocabulary for M35-B2 offline extraction.",
                "Unsupported mechanics are excluded from the first slice instead of generalized.",
            ],
        },
        "vocabulary": {
            "ability_types": ability_types,
            "zones": zones,
            "timing": timing,
            "conditions": conditions,
            "costs": costs,
            "effects": effects,
            "durations": durations,
            "mechanic_groups": mechanic_groups,
            "trigger_icons": ["critical", "draw", "stand", "heal"],
        },
        "excluded_first_slice_tags": [
            "legion",
            "stride",
            "g_guardian",
            "imaginary_gift",
            "front_trigger",
            "over_trigger",
            "ride_deck",
            "order",
            "gauge",
            "crest",
            "energy",
            "overdress",
            "xoverdress",
            "divine_skill",
        ],
        "source_term_checks": source_term_checks,
        "missing_source_terms": missing_source_terms,
        "ready_for_m35_b2": not missing_source_terms and selected_slice_ready,
        "next_target": "M35-B2",
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    vocab = report["vocabulary"]
    lines = [
        "# M35-B1 First Slice Semantic Vocabulary",
        "",
        "## Selected Target",
        "",
        f"- Slice: `{target['slice']}`",
        f"- Era preset: `{target['era_preset']}`",
        f"- Group: `{target['group']}`",
        "",
        "## Scope",
        "",
        "- Advisory offline vocabulary only.",
        "- No runtime effect execution.",
        "- No live card text parser.",
        "",
        "## Vocabulary Counts",
        "",
    ]
    for key, values in vocab.items():
        lines.append(f"- `{key}`: `{len(values)}`")
    lines.extend(
        [
            "",
            "## Missing Source Terms",
            "",
            f"`{', '.join(report['missing_source_terms']) if report['missing_source_terms'] else 'none'}`",
            "",
            "## Ready For M35-B2",
            "",
            f"`{report['ready_for_m35_b2']}`",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35-B1 first-slice semantic vocabulary.")
    parser.add_argument("--feasibility-report", type=Path, default=FEASIBILITY_REFRESH)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    feasibility_report = load_json(args.feasibility_report)
    report = build_vocabulary(feasibility_report)
    json_path = args.output_dir / "m35_b1_first_slice_semantic_vocabulary.json"
    md_path = args.output_dir / "m35_b1_first_slice_semantic_vocabulary.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35-B1 semantic vocabulary wrote {json_path}")
    print(f"M35-B1 semantic vocabulary summary wrote {md_path}")
    print(f"ready_for_m35_b2={report['ready_for_m35_b2']}")
    return 0 if report["ready_for_m35_b2"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
