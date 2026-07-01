"""Build requirement/provider model for selected first-slice semantic tags.

M35-B3 of the Hybrid Vertical-Slice Strategy.

This transforms M35-B2 advisory semantic tags into card-level requirements and
providers. It is input for later compatibility graph work, not a playbook.
"""

from __future__ import annotations

import argparse
import json
import sys
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
SEMANTIC_TAGS_REPORT = ROOT / "outputs" / "target_slice" / "m35_b2_first_slice_semantic_tags.json"
OUTPUT_DIR = ROOT / "outputs" / "target_slice"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


COST_REQUIREMENTS = {
    "counter_blast": "requires_counter_blast",
    "soul_blast": "requires_soul_blast",
    "discard": "requires_discard",
    "rest_this_unit": "requires_rest_this_unit",
    "retire_this_unit": "requires_retire_this_unit",
    "put_to_soul": "requires_put_to_soul",
}

CONDITION_REQUIREMENTS = {
    "if_vanguard": "requires_vanguard_circle",
    "if_rear_guard": "requires_rear_guard_circle",
    "if_boosted": "requires_boosted",
    "if_attack_hits": "requires_attack_hit",
    "if_limit_break_4": "requires_limit_break_4",
    "if_counter_blast_available": "requires_counter_blast_available",
    "if_soul_available": "requires_soul_available",
    "if_same_clan": "requires_same_clan",
    "if_grade_requirement": "requires_grade_requirement",
}

TIMING_REQUIREMENTS = {
    "on_place": "requires_on_place_timing",
    "on_ride": "requires_on_ride_timing",
    "on_call": "requires_on_call_timing",
    "on_attack": "requires_on_attack_timing",
    "when_attack_hits": "requires_hit_timing",
    "when_boosts": "requires_boost_timing",
    "when_revealed_as_trigger": "requires_trigger_check_timing",
    "end_phase": "requires_end_phase_timing",
}

ZONE_REQUIREMENTS = {
    "vanguard_circle": "requires_vanguard_circle",
    "rear_guard_circle": "requires_rear_guard_circle",
    "guardian_circle": "requires_guardian_circle",
    "damage": "requires_damage_zone_reference",
    "drop": "requires_drop_zone_reference",
    "soul": "requires_soul_reference",
}

EFFECT_PROVIDERS = {
    "counter_charge": "provides_counter_charge",
    "soul_charge": "provides_soul_charge",
    "move_to_soul": "provides_soul_resource",
    "power_plus": "provides_power_pressure",
    "critical_plus": "provides_critical_pressure",
    "draw": "provides_card_advantage",
    "search_deck": "provides_search",
    "superior_call": "provides_board_extension",
    "stand_unit": "provides_restand_or_multi_attack",
    "retire": "provides_retire_pressure",
    "shuffle_deck": "provides_deck_recycle",
    "guard_restrict": "provides_guard_restrict",
    "sentinel_guard": "provides_defensive_guard",
}

TRIGGER_PROVIDERS = {
    "critical": "provides_critical_trigger",
    "draw": "provides_draw_trigger",
    "stand": "provides_stand_trigger",
    "heal": "provides_heal_trigger",
}


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def map_values(values: Sequence[str], mapping: dict[str, str]) -> list[str]:
    return sorted({mapping[value] for value in values if value in mapping})


def build_card_model(card: dict[str, Any]) -> dict[str, Any]:
    tags = card["semantic_tags"]
    requirements = set()
    providers = set()

    requirements.update(map_values(tags.get("costs", []), COST_REQUIREMENTS))
    requirements.update(map_values(tags.get("conditions", []), CONDITION_REQUIREMENTS))
    requirements.update(map_values(tags.get("timing", []), TIMING_REQUIREMENTS))
    requirements.update(map_values(tags.get("zones", []), ZONE_REQUIREMENTS))

    providers.update(map_values(tags.get("effects", []), EFFECT_PROVIDERS))
    providers.update(map_values(tags.get("trigger_icons", []), TRIGGER_PROVIDERS))

    if "limit_break" in tags.get("mechanic_groups", []):
        requirements.add("requires_limit_break_4")
    if "trigger_icon" in tags.get("mechanic_groups", []):
        providers.add("provides_trigger_check_value")

    if "requires_counter_blast" in requirements and "provides_counter_charge" in providers:
        providers.add("provides_counter_blast_sustain")
    if "requires_soul_blast" in requirements and (
        "provides_soul_charge" in providers or "provides_soul_resource" in providers
    ):
        providers.add("provides_soul_blast_sustain")

    review_reasons = list(card.get("manual_review_reasons", []))
    if not requirements and not providers and card.get("type_1") != "Normal Unit":
        review_reasons.append("no_requirement_or_provider_tags")

    return {
        "card_id": card["card_id"],
        "name_th": card["name_th"],
        "series_code": card["series_code"],
        "grade": card["grade"],
        "trigger": card["trigger"],
        "type_1": card["type_1"],
        "requirements": sorted(requirements),
        "providers": sorted(providers),
        "manual_review_required": bool(review_reasons or card.get("manual_review_required")),
        "manual_review_reasons": sorted(set(review_reasons)),
    }


def build_indexes(card_models: Sequence[dict[str, Any]]) -> dict[str, Any]:
    provider_index: dict[str, list[str]] = defaultdict(list)
    requirement_index: dict[str, list[str]] = defaultdict(list)
    provider_counts: Counter[str] = Counter()
    requirement_counts: Counter[str] = Counter()
    for card in card_models:
        for provider in card["providers"]:
            provider_index[provider].append(card["card_id"])
            provider_counts[provider] += 1
        for requirement in card["requirements"]:
            requirement_index[requirement].append(card["card_id"])
            requirement_counts[requirement] += 1
    return {
        "provider_index": {key: sorted(value) for key, value in sorted(provider_index.items())},
        "requirement_index": {key: sorted(value) for key, value in sorted(requirement_index.items())},
        "provider_counts": dict(sorted(provider_counts.items())),
        "requirement_counts": dict(sorted(requirement_counts.items())),
    }


def build_report(semantic_report: dict[str, Any] | None = None) -> dict[str, Any]:
    semantic_report = semantic_report or load_json(SEMANTIC_TAGS_REPORT)
    card_models = [build_card_model(card) for card in semantic_report["cards"]]
    indexes = build_indexes(card_models)
    manual_review_count = sum(1 for card in card_models if card["manual_review_required"])
    cards_with_requirements = sum(1 for card in card_models if card["requirements"])
    cards_with_providers = sum(1 for card in card_models if card["providers"])
    return {
        "version": "M35-B3",
        "description": "Requirement/provider model for selected first slice",
        "selected_target": semantic_report["selected_target"],
        "source_inputs": {
            "semantic_tags": str(SEMANTIC_TAGS_REPORT.relative_to(ROOT)),
        },
        "scope": {
            "advisory_only": True,
            "compatibility_graph": False,
            "deck_skeleton": False,
            "bot_playbook": False,
        },
        "summary": {
            "card_count": len(card_models),
            "cards_with_requirements": cards_with_requirements,
            "cards_with_providers": cards_with_providers,
            "manual_review_count": manual_review_count,
            "provider_type_count": len(indexes["provider_index"]),
            "requirement_type_count": len(indexes["requirement_index"]),
        },
        "indexes": indexes,
        "cards": card_models,
        "ready_for_m35_b4": True,
        "next_target": "M35-B4",
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    summary = report["summary"]
    lines = [
        "# M35-B3 Requirement / Provider Model",
        "",
        "## Selected Target",
        "",
        f"- Slice: `{target['slice']}`",
        f"- Era preset: `{target['era_preset']}`",
        f"- Group: `{target['group']}`",
        "",
        "## Summary",
        "",
        f"- Cards: `{summary['card_count']}`",
        f"- Cards with requirements: `{summary['cards_with_requirements']}`",
        f"- Cards with providers: `{summary['cards_with_providers']}`",
        f"- Manual review count: `{summary['manual_review_count']}`",
        f"- Provider types: `{summary['provider_type_count']}`",
        f"- Requirement types: `{summary['requirement_type_count']}`",
        f"- Ready for M35-B4: `{report['ready_for_m35_b4']}`",
        "",
        "## Scope",
        "",
        "- Advisory requirement/provider model only.",
        "- No compatibility graph yet.",
        "- No deck skeleton or bot playbook promotion.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35-B3 requirement/provider model.")
    parser.add_argument("--semantic-tags", type=Path, default=SEMANTIC_TAGS_REPORT)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    semantic_report = load_json(args.semantic_tags)
    report = build_report(semantic_report)
    json_path = args.output_dir / "m35_b3_first_slice_requirement_provider_model.json"
    md_path = args.output_dir / "m35_b3_first_slice_requirement_provider_model.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35-B3 requirement/provider model wrote {json_path}")
    print(f"M35-B3 requirement/provider summary wrote {md_path}")
    print(
        "cards={card_count} providers={providers} requirements={requirements} manual_review={manual}".format(
            card_count=report["summary"]["card_count"],
            providers=report["summary"]["provider_type_count"],
            requirements=report["summary"]["requirement_type_count"],
            manual=report["summary"]["manual_review_count"],
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
