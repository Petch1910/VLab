"""Extract advisory semantic tags for the selected first slice.

M35-B2 of the Hybrid Vertical-Slice Strategy.

This extractor reads local card text from the runtime SQLite pack and maps
heuristic feature tags into the bounded M35-B1 vocabulary. It does not execute
effects, alter runtime card data, or parse live match text.
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from collections import Counter
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.combo.discover_clan_combos import (  # noqa: E402
    DEFAULT_DB,
    ERA_SET_SPECS,
    CardRecord,
    expand_set_specs,
    extract_features,
    get_card_group,
    load_cards,
)


TARGET_REPORT = ROOT / "outputs" / "target_slice" / "m35_a2_first_target_slice_report.json"
VOCABULARY_REPORT = ROOT / "outputs" / "target_slice" / "m35_b1_first_slice_semantic_vocabulary.json"
OUTPUT_DIR = ROOT / "outputs" / "target_slice"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


ABILITY_MAP = {
    "act_ability": "ACT",
    "auto_ability": "AUTO",
    "cont_ability": "CONT",
}

ZONE_MAP = {
    "vc": "vanguard_circle",
    "rc": "rear_guard_circle",
    "drop_zone": "drop",
    "damage_zone": "damage",
}

TIMING_MAP = {
    "on_call": "on_call",
    "ride_related": "on_ride",
    "attack_related": "on_attack",
    "on_hit": "when_attack_hits",
    "boost_related": "when_boosts",
    "end_phase": "end_phase",
}

COST_MAP = {
    "counter_blast_cost": "counter_blast",
    "soul_blast_cost": "soul_blast",
}

EFFECT_MAP = {
    "counter_charge": "counter_charge",
    "soul_charge": "soul_charge",
    "soul_resource": "move_to_soul",
    "power_plus": "power_plus",
    "critical_plus": "critical_plus",
    "draw": "draw",
    "search_deck": "search_deck",
    "superior_call": "superior_call",
    "stand_unit": "stand_unit",
    "retire": "retire",
    "deck_recycle": "shuffle_deck",
}

IGNORED_FEATURE_PREFIXES = ("grade_",)
IGNORED_FEATURE_TAGS = {"trigger_unit"}


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def load_selected_cards(target_report: dict[str, Any]) -> list[CardRecord]:
    selected_group = target_report["selected_target"]["group"]
    era_preset = target_report["selected_target"]["era_preset"]
    format_policy = target_report.get("format_policy", {})
    scoped_codes = format_policy.get("series_scope") or format_policy.get("set_codes")
    set_codes = list(scoped_codes) if scoped_codes else expand_set_specs(ERA_SET_SPECS[era_preset])
    cards = load_cards(DEFAULT_DB, set_codes)
    selected = [card for card in cards if get_card_group(card, "auto")[0] == selected_group]
    if not selected:
        raise ValueError(f"No selected-slice cards found for group: {selected_group}")
    return selected


def has_limit_break(text: str) -> bool:
    return bool(re.search(r"limit\s*break|\[lb\]|\[limit", text or "", flags=re.IGNORECASE))


def has_forerunner(text: str) -> bool:
    return "forerunner" in (text or "").lower()


def has_sentinel(card: CardRecord) -> bool:
    text = card.text_th or ""
    return "sentinel" in text.lower() or "เซนติเนล" in text


def normalize_trigger(trigger: str) -> str:
    return (trigger or "").strip().lower()


def allowed_sets(vocabulary: dict[str, Any]) -> dict[str, set[str]]:
    vocab = vocabulary["vocabulary"]
    return {key: set(values) for key, values in vocab.items()}


def filter_allowed(values: set[str], allowed: set[str]) -> list[str]:
    return sorted(value for value in values if value in allowed)


def extract_card_semantics(card: CardRecord, vocabulary: dict[str, Any]) -> dict[str, Any]:
    features = extract_features(card)
    feature_tags = set(features.tags)
    allowed = allowed_sets(vocabulary)

    ability_types = {ABILITY_MAP[tag] for tag in feature_tags if tag in ABILITY_MAP}
    zones = {ZONE_MAP[tag] for tag in feature_tags if tag in ZONE_MAP}
    timing = {TIMING_MAP[tag] for tag in feature_tags if tag in TIMING_MAP}
    costs = {COST_MAP[tag] for tag in feature_tags if tag in COST_MAP}
    effects = {EFFECT_MAP[tag] for tag in feature_tags if tag in EFFECT_MAP}
    conditions: set[str] = set()
    mechanic_groups: set[str] = {"core"}
    durations: set[str] = set()

    if "vc" in feature_tags:
        conditions.add("if_vanguard")
    if "rc" in feature_tags:
        conditions.add("if_rear_guard")
    if "boost_related" in feature_tags:
        conditions.add("if_boosted")
    if "on_hit" in feature_tags:
        conditions.add("if_attack_hits")
    if has_limit_break(card.text_th):
        conditions.add("if_limit_break_4")
        mechanic_groups.add("limit_break")
    if has_forerunner(card.text_th):
        mechanic_groups.add("forerunner")
    if has_sentinel(card):
        mechanic_groups.add("sentinel")
    if card.trigger:
        mechanic_groups.add("trigger_icon")
        timing.add("when_revealed_as_trigger")

    if effects or costs:
        durations.add("one_shot")
    if "power_plus" in feature_tags or "critical_plus" in feature_tags:
        durations.add("until_end_of_turn")
    if "attack_related" in feature_tags or "boost_related" in feature_tags:
        durations.add("during_battle")

    trigger_icons = {normalize_trigger(card.trigger)} if card.trigger else set()
    mapped_feature_tags = set(ABILITY_MAP) | set(ZONE_MAP) | set(TIMING_MAP) | set(COST_MAP) | set(EFFECT_MAP)
    unmapped = sorted(
        tag
        for tag in feature_tags - mapped_feature_tags
        if tag not in IGNORED_FEATURE_TAGS
        and not any(tag.startswith(prefix) for prefix in IGNORED_FEATURE_PREFIXES)
    )
    manual_review_reasons: list[str] = []
    if unmapped:
        manual_review_reasons.append("unmapped_feature_tags")
    if card.text_th and not ability_types and not card.trigger:
        manual_review_reasons.append("text_without_ability_type_tag")

    return {
        "card_id": card.card_id,
        "name_th": card.name_th,
        "series_code": card.series_code,
        "clan": card.clan,
        "nation": card.nation,
        "grade": card.grade,
        "trigger": card.trigger,
        "type_1": card.type_1,
        "raw_feature_tags": sorted(feature_tags),
        "semantic_tags": {
            "ability_types": filter_allowed(ability_types, allowed["ability_types"]),
            "zones": filter_allowed(zones, allowed["zones"]),
            "timing": filter_allowed(timing, allowed["timing"]),
            "conditions": filter_allowed(conditions, allowed["conditions"]),
            "costs": filter_allowed(costs, allowed["costs"]),
            "effects": filter_allowed(effects, allowed["effects"]),
            "durations": filter_allowed(durations, allowed["durations"]),
            "mechanic_groups": filter_allowed(mechanic_groups, allowed["mechanic_groups"]),
            "trigger_icons": filter_allowed(trigger_icons, allowed["trigger_icons"]),
        },
        "unmapped_feature_tags": unmapped,
        "manual_review_required": bool(manual_review_reasons),
        "manual_review_reasons": manual_review_reasons,
    }


def build_summary(cards: Sequence[dict[str, Any]]) -> dict[str, Any]:
    category_counts: dict[str, Counter[str]] = {}
    for card in cards:
        for category, tags in card["semantic_tags"].items():
            category_counts.setdefault(category, Counter()).update(tags)
    return {
        "card_count": len(cards),
        "cards_with_any_semantic_tag": sum(
            1 for card in cards if any(card["semantic_tags"].values())
        ),
        "manual_review_count": sum(1 for card in cards if card["manual_review_required"]),
        "tag_counts": {
            category: dict(sorted(counter.items()))
            for category, counter in sorted(category_counts.items())
        },
    }


def build_report(
    target_report: dict[str, Any] | None = None,
    vocabulary: dict[str, Any] | None = None,
) -> dict[str, Any]:
    target_report = target_report or load_json(TARGET_REPORT)
    vocabulary = vocabulary or load_json(VOCABULARY_REPORT)
    cards = load_selected_cards(target_report)
    semantic_cards = [
        extract_card_semantics(card, vocabulary)
        for card in sorted(cards, key=lambda item: (item.series_code, item.card_id))
    ]
    excluded = set(vocabulary.get("excluded_first_slice_tags", []))
    used_tags = {
        tag
        for card in semantic_cards
        for tags in card["semantic_tags"].values()
        for tag in tags
    }
    excluded_used = sorted(used_tags & excluded)
    return {
        "version": "M35-B2",
        "description": "Offline semantic extractor output for selected first slice",
        "selected_target": target_report["selected_target"],
        "source_inputs": {
            "target_report": str(TARGET_REPORT.relative_to(ROOT)),
            "semantic_vocabulary": str(VOCABULARY_REPORT.relative_to(ROOT)),
            "runtime_database": "data/packs/vanguard_th/cards.sqlite",
        },
        "scope": {
            "advisory_only": True,
            "runtime_effect_execution": False,
            "live_card_text_parser": False,
            "selected_slice_only": True,
        },
        "summary": build_summary(semantic_cards),
        "excluded_first_slice_tags_used": excluded_used,
        "ready_for_m35_b3": not excluded_used,
        "cards": semantic_cards,
        "next_target": "M35-B3",
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    summary = report["summary"]
    lines = [
        "# M35-B2 First Slice Semantic Tags",
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
        f"- Cards with tags: `{summary['cards_with_any_semantic_tag']}`",
        f"- Manual review count: `{summary['manual_review_count']}`",
        f"- Excluded first-slice tags used: `{', '.join(report['excluded_first_slice_tags_used']) or 'none'}`",
        f"- Ready for M35-B3: `{report['ready_for_m35_b3']}`",
        "",
        "## Scope",
        "",
        "- Advisory offline extraction only.",
        "- No runtime effect execution.",
        "- No live card text parser.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Extract M35-B2 selected first-slice semantic tags.")
    parser.add_argument("--target-report", type=Path, default=TARGET_REPORT)
    parser.add_argument("--vocabulary-report", type=Path, default=VOCABULARY_REPORT)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    target_report = load_json(args.target_report)
    vocabulary = load_json(args.vocabulary_report)
    report = build_report(target_report, vocabulary)
    json_path = args.output_dir / "m35_b2_first_slice_semantic_tags.json"
    md_path = args.output_dir / "m35_b2_first_slice_semantic_tags.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35-B2 semantic tags wrote {json_path}")
    print(f"M35-B2 semantic tags summary wrote {md_path}")
    print(f"cards={report['summary']['card_count']} manual_review={report['summary']['manual_review_count']}")
    print(f"ready_for_m35_b3={report['ready_for_m35_b3']}")
    return 0 if report["ready_for_m35_b3"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
