"""Select the first deck/combo vertical slice and emit a taxonomy gap report.

M35-A2 of the Hybrid Vertical-Slice Strategy.

This is an offline planning helper. It does not validate live deck legality,
does not mutate runtime card data, and does not run inside Unity gameplay.
"""

from __future__ import annotations

import argparse
import json
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.combo.discover_clan_combos import ERA_SET_SPECS, expand_set_specs  # noqa: E402

ARCHETYPE_PRIORITY_JSON = ROOT / "outputs" / "archetype_priority" / "archetype_priority_ranking.json"
DECK_POSSIBILITY_DIR = ROOT / "outputs" / "deck_possibility"
RULE_TAXONOMY_JSON = ROOT / "outputs" / "research_2026_06_29_new_chat" / "vanguard_development" / "rule_taxonomy.json"
MECHANIC_MATRIX_MD = (
    ROOT
    / "outputs"
    / "research_2026_06_29_new_chat"
    / "vanguard_rules_markdown"
    / "mechanic_presence_matrix.md"
)
OUTPUT_DIR = ROOT / "outputs" / "target_slice"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


@dataclass(frozen=True)
class SliceConfig:
    key: str
    display_name: str
    era_preset: str
    source_priority: str
    baseline_rules_source: str
    baseline_rules_note: str
    group_field: str


SLICE_CONFIGS: dict[str, SliceConfig] = {
    "classic_core": SliceConfig(
        key="classic_core",
        display_name="Classic Core",
        era_preset="classic_part1",
        source_priority="user_requested_td01_td06_bt01_bt09_eb01_eb05",
        baseline_rules_source="outputs/research_2026_06_29_new_chat/vanguard_rules_markdown/versions/01_og_link_joker_rules_1_19.md",
        baseline_rules_note=(
            "Use the historical 1.19 rules snapshot as the first fixture source "
            "for early clan-era play, then confirm any card-specific behavior "
            "against the source map before promoting fixtures."
        ),
        group_field="clan",
    ),
    "standard_dz": SliceConfig(
        key="standard_dz",
        display_name="Standard D/DZ",
        era_preset="dz_divinez",
        source_priority="current_nation_era_optional_slice",
        baseline_rules_source="outputs/research_2026_06_29_new_chat/vanguard_rules_markdown/versions/13_dz_rules_4_55.md",
        baseline_rules_note="Use Comprehensive Rules 4.55 for current Standard/DZ mechanics.",
        group_field="nation",
    ),
}


CLASSIC_REQUIRED_TERMS: dict[str, list[str]] = {
    "zones": [
        "Deck Zone",
        "Hand",
        "Drop Zone",
        "Field",
        "Circle",
        "Vanguard Circle",
        "Rear-guard Circle",
        "Guardian Circle",
        "Soul",
        "Damage Zone",
        "Trigger Zone",
    ],
    "placement_systems": [
        "Call",
        "Ride",
    ],
    "specific_actions": [
        "Stand/Rest",
        "Shuffle",
        "Move Cards from the Deck/Draw",
        "Discard",
        "Search",
        "Retire",
        "Heal",
        "(Perform) Drive Check",
        "(Perform) Damage Check",
        "Counter-blast",
        "Soul-blast",
        "Counter-charge",
        "Soul-charge",
        "Attack",
        "Battle",
        "Boost",
    ],
    "turn_structure": [
        "Stand Phase",
        "Draw Phase",
        "Ride Phase",
        "Main Phase",
        "Battle Phase",
        "End Phase",
    ],
    "battle_steps": [
        "Start Step",
        "Attack Step",
        "Guard Step",
        "Drive Step",
        "Damage Step",
        "Close Step",
    ],
    "ability_types": [
        "ACT",
        "AUTO",
        "CONT",
    ],
    "triggers": [
        "Critical",
        "Draw",
        "Stand",
        "Heal",
    ],
}

CLASSIC_KEYWORDS: dict[str, list[str]] = {
    "basic": [
        "Drive Abilities",
        "Intercept",
        "Boost",
        "Restraint",
        "Sentinel",
    ],
    "activation_condition": [
        "Limit Break",
    ],
    "procedure": [
        "Forerunner",
    ],
}

CLASSIC_UNSUPPORTED_MODULES = [
    "Legion/D-Legion",
    "Stride and Becoming a Heart",
    "G Guardian",
    "Imaginary Gift",
    "Front Trigger",
    "Ride Deck Zone",
    "Persona Ride",
    "Over Trigger",
    "Order Area",
    "Order Zone",
    "Gauge Zone",
    "Crest Zone",
    "Energy-charge",
    "Energy-blast",
    "overDress",
    "XoverDress",
    "Divine Skill",
]


def load_json(path: Path) -> Any:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def load_deck_possibility(era_preset: str) -> dict[str, Any]:
    return load_json(DECK_POSSIBILITY_DIR / f"{era_preset}_deck_possibility.json")


def select_candidate(
    rankings: Sequence[dict[str, Any]],
    deck_report: dict[str, Any],
    config: SliceConfig,
) -> dict[str, Any]:
    deck_groups = {group["group"]: group for group in deck_report.get("groups", [])}
    candidates: list[dict[str, Any]] = []
    for row in rankings:
        if not row.get("feasible"):
            continue
        if row.get("best_era") != config.era_preset:
            continue
        if row.get("group") not in deck_groups:
            continue
        candidates.append(row)

    if not candidates:
        raise ValueError(f"No feasible ranking candidate found for slice {config.key}")

    candidates.sort(key=lambda row: (int(row.get("rank", 999999)), -float(row.get("priority_score", 0))))
    return candidates[0]


def selected_group_deck_report(deck_report: dict[str, Any], group_name: str) -> dict[str, Any]:
    for group in deck_report.get("groups", []):
        if group.get("group") == group_name:
            return group
    raise ValueError(f"Selected group not found in deck possibility report: {group_name}")


def build_format_policy(config: SliceConfig, selected: dict[str, Any], group_report: dict[str, Any]) -> dict[str, Any]:
    set_specs = list(ERA_SET_SPECS[config.era_preset])
    set_codes = expand_set_specs(set_specs)
    feasible = group_report.get("feasible", {})
    return {
        "policy_level": "m35_a2_minimal_source_backed_policy_not_full_official_legality",
        "slice": config.display_name,
        "era_preset": config.era_preset,
        "set_specs": set_specs,
        "set_codes": set_codes,
        "baseline_rules_source": config.baseline_rules_source,
        "baseline_rules_note": config.baseline_rules_note,
        "group_identity": {
            "field": config.group_field,
            "selected_group": selected["group"],
            "selected_group_field": selected.get("group_field", ""),
        },
        "deck_limits_for_next_fixtures": {
            "main_deck_exact": 50,
            "trigger_target": 16,
            "non_trigger_target": 34,
            "ride_grade_0_1_2_3_choices_required_for_setup": True,
            "ride_deck_required": False,
            "g_zone_required": False,
            "g_zone_max_for_first_slice": 0,
            "repository_backed_copy_limits": "defer_to_M35_A3_fixtures",
            "heal_trigger_limit": "defer_to_M35_A3_source_backed_fixture",
        },
        "current_count_evidence": {
            "card_count": group_report.get("card_count", 0),
            "main_card_count": group_report.get("main_card_count", 0),
            "trigger_card_count": group_report.get("trigger_card_count", 0),
            "non_trigger_card_count": group_report.get("non_trigger_card_count", 0),
            "main_capacity": group_report.get("main_capacity", 0),
            "trigger_capacity": group_report.get("trigger_capacity", 0),
            "non_trigger_capacity": group_report.get("non_trigger_capacity", 0),
            "grade_unique_count": group_report.get("grade_unique_count", {}),
            "trigger_capacity_by_type": group_report.get("trigger_capacity_by_type", {}),
            "basic_50_card_main": bool(feasible.get("basic_50_card_main")),
            "main_with_16_triggers_34_non_triggers": bool(
                feasible.get("main_with_16_triggers_34_non_triggers")
            ),
            "ride_deck_grade_0_1_2_3_choice": bool(feasible.get("ride_deck_grade_0_1_2_3_choice")),
            "issues": list(group_report.get("issues", [])),
        },
        "unsupported_in_first_slice": CLASSIC_UNSUPPORTED_MODULES if config.key == "classic_core" else [],
    }


def find_terms(container: Any, terms: Sequence[str]) -> dict[str, Any]:
    if isinstance(container, dict):
        values = []
        for item in container.values():
            if isinstance(item, list):
                values.extend(item)
    elif isinstance(container, list):
        values = container
    else:
        values = []

    present = [term for term in terms if term in values]
    missing = [term for term in terms if term not in values]
    return {
        "required": list(terms),
        "present": present,
        "missing": missing,
    }


def build_taxonomy_gap_report(config: SliceConfig, taxonomy: dict[str, Any]) -> dict[str, Any]:
    if config.key != "classic_core":
        required_terms = CLASSIC_REQUIRED_TERMS
        required_keywords = CLASSIC_KEYWORDS
    else:
        required_terms = CLASSIC_REQUIRED_TERMS
        required_keywords = CLASSIC_KEYWORDS

    categories: dict[str, Any] = {}
    for category, terms in required_terms.items():
        categories[category] = find_terms(taxonomy.get(category, []), terms)

    keyword_categories: dict[str, Any] = {}
    taxonomy_keywords = taxonomy.get("keywords", {})
    for category, terms in required_keywords.items():
        keyword_categories[category] = find_terms(taxonomy_keywords.get(category, []), terms)

    missing_terms: list[str] = []
    for category in categories.values():
        missing_terms.extend(category["missing"])
    for category in keyword_categories.values():
        missing_terms.extend(category["missing"])

    return {
        "taxonomy_source": str(RULE_TAXONOMY_JSON.relative_to(ROOT)),
        "mechanic_presence_source": str(MECHANIC_MATRIX_MD.relative_to(ROOT)),
        "required_categories": categories,
        "required_keywords": keyword_categories,
        "missing_required_terms": missing_terms,
        "unsupported_modules_for_first_slice": CLASSIC_UNSUPPORTED_MODULES if config.key == "classic_core" else [],
        "next_fixture_gaps": [
            "source-backed copy-limit fixture",
            "source-backed heal-trigger limit fixture",
            "selected-group legal/illegal decklist fixtures",
            "classic setup fixture without ride deck",
            "classic trigger package fixture with Critical/Draw/Stand/Heal only",
        ],
    }


def build_report(preferred_slice: str = "classic_core") -> dict[str, Any]:
    if preferred_slice not in SLICE_CONFIGS:
        raise ValueError(f"Unknown preferred slice: {preferred_slice}")
    config = SLICE_CONFIGS[preferred_slice]
    priority = load_json(ARCHETYPE_PRIORITY_JSON)
    deck_report = load_deck_possibility(config.era_preset)
    taxonomy = load_json(RULE_TAXONOMY_JSON)

    rankings = priority.get("rankings", [])
    selected = select_candidate(rankings, deck_report, config)
    group_report = selected_group_deck_report(deck_report, selected["group"])

    candidate_summary = [
        {
            "rank": row.get("rank"),
            "group": row.get("group"),
            "priority_score": row.get("priority_score"),
            "best_era": row.get("best_era"),
            "best_era_candidates": row.get("best_era_candidates"),
            "mechanic_tier": row.get("mechanic_tier"),
            "mechanic_tier_label": row.get("mechanic_tier_label"),
        }
        for row in rankings
        if row.get("feasible") and row.get("best_era") == config.era_preset
    ][:10]

    return {
        "version": "M35-A2",
        "description": "First target slice selection + format policy + taxonomy gap report",
        "selection_policy": {
            "preferred_slice": config.key,
            "reason": (
                "User priority is Classic Core clan-era combo work "
                "(TD01-TD06 / BT01-BT09 / EB01-EB05). Within that slice, choose "
                "the highest-ranked feasible M34-03 group."
            ),
            "do_not_assume_standard_by_default": True,
        },
        "selected_target": {
            "slice": config.display_name,
            "era_preset": config.era_preset,
            "group": selected["group"],
            "group_field": selected.get("group_field", ""),
            "rank": selected.get("rank"),
            "priority_score": selected.get("priority_score"),
            "best_era_candidates": selected.get("best_era_candidates"),
            "mechanic_tier": selected.get("mechanic_tier"),
            "mechanic_tier_label": selected.get("mechanic_tier_label"),
            "priority_reasons": selected.get("priority_reasons", []),
        },
        "candidate_summary": candidate_summary,
        "input_evidence": {
            "archetype_priority": str(ARCHETYPE_PRIORITY_JSON.relative_to(ROOT)),
            "deck_possibility": str((DECK_POSSIBILITY_DIR / f"{config.era_preset}_deck_possibility.json").relative_to(ROOT)),
            "rule_taxonomy": str(RULE_TAXONOMY_JSON.relative_to(ROOT)),
            "mechanic_presence_matrix": str(MECHANIC_MATRIX_MD.relative_to(ROOT)),
            "missing_set_codes": deck_report.get("scope", {}).get("missing_set_codes", []),
        },
        "format_policy": build_format_policy(config, selected, group_report),
        "taxonomy_gap_report": build_taxonomy_gap_report(config, taxonomy),
        "next_target": {
            "milestone": "M35-A3",
            "task": "Minimal deck legality fixtures for selected slice",
            "must_create": [
                "passing Classic Core selected-group deck fixture",
                "failing short-main deck fixture",
                "failing trigger-count deck fixture",
                "failing missing-grade setup fixture",
                "source note for heal/copy limits before enforcing them broadly",
            ],
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    policy = report["format_policy"]
    gaps = report["taxonomy_gap_report"]
    missing_terms = gaps["missing_required_terms"]
    candidate_lines = [
        f"- #{row['rank']} {row['group']} score={row['priority_score']} "
        f"candidates={row['best_era_candidates']} tier={row['mechanic_tier_label']}"
        for row in report["candidate_summary"][:5]
    ]
    text = f"""# M35-A2 First Target Slice Report

## Selected Target

- Slice: `{target['slice']}`
- Era preset: `{target['era_preset']}`
- Group: `{target['group']}`
- M34-03 rank: `{target['rank']}`
- Priority score: `{target['priority_score']}`
- Mechanic tier: `{target['mechanic_tier_label']}`

## Why This Slice

User priority is Classic Core clan-era combo work:
`TD01-TD06 / BT01-BT09 / EB01-EB05`.
The selected group is the highest-ranked feasible group inside that slice.
Standard/DZ is not assumed by default.

## Top Classic Core Candidates

{chr(10).join(candidate_lines)}

## Format Policy For M35-A3

- Main deck: exactly `{policy['deck_limits_for_next_fixtures']['main_deck_exact']}`
- Trigger target: `{policy['deck_limits_for_next_fixtures']['trigger_target']}`
- Ride deck required: `{policy['deck_limits_for_next_fixtures']['ride_deck_required']}`
- G zone required: `{policy['deck_limits_for_next_fixtures']['g_zone_required']}`
- Baseline source: `{policy['baseline_rules_source']}`

## Current Count Evidence

- Card count: `{policy['current_count_evidence']['card_count']}`
- Main capacity: `{policy['current_count_evidence']['main_capacity']}`
- Trigger capacity: `{policy['current_count_evidence']['trigger_capacity']}`
- Non-trigger capacity: `{policy['current_count_evidence']['non_trigger_capacity']}`
- Missing set codes: `{', '.join(report['input_evidence']['missing_set_codes']) or 'none'}`

## Taxonomy Gap

- Missing required taxonomy terms: `{', '.join(missing_terms) if missing_terms else 'none'}`
- Unsupported in first slice:
  `{', '.join(gaps['unsupported_modules_for_first_slice'])}`

## Next

`M35-A3`: create minimal source-backed deck legality fixtures for this selected
slice before semantic tagging or combo compatibility work.
"""
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Select first target deck/combo slice for M35-A2.")
    parser.add_argument("--preferred-slice", choices=sorted(SLICE_CONFIGS), default="classic_core")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_report(args.preferred_slice)
    json_path = args.output_dir / "m35_a2_first_target_slice_report.json"
    md_path = args.output_dir / "m35_a2_first_target_slice_report.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    selected = report["selected_target"]
    print(f"M35-A2 selected {selected['slice']} / {selected['group']} (rank {selected['rank']})")
    print(f"JSON: {json_path}")
    print(f"MD:   {md_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
