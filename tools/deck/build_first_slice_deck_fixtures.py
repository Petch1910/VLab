"""Build and validate minimal deck legality fixtures for the selected slice.

M35-A3 of the Hybrid Vertical-Slice Strategy.

The fixture rules are intentionally narrow:
  - selected M35-A2 slice only
  - local runtime SQLite card data only
  - count, trigger, grade setup, selected identity, and per-card deck_limit

This does not claim full official deck legality.
"""

from __future__ import annotations

import argparse
import json
import sys
from copy import deepcopy
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.combo.discover_clan_combos import get_card_group  # noqa: E402
from tools.deck.analyze_deck_possibilities import (  # noqa: E402
    DEFAULT_DB,
    DeckCardRecord,
    is_excluded_from_main_deck,
    is_trigger_card,
    load_cards,
)


SELECTED_SLICE_REPORT = ROOT / "outputs" / "target_slice" / "m35_a2_first_target_slice_report.json"
OUTPUT_DIR = ROOT / "outputs" / "target_slice"

MAIN_DECK_SIZE = 50
TRIGGER_TARGET = 16
NON_TRIGGER_TARGET = MAIN_DECK_SIZE - TRIGGER_TARGET
REQUIRED_SETUP_GRADES = {0, 1, 2, 3}

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_selected_report(path: Path = SELECTED_SLICE_REPORT) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"M35-A2 report not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def load_slice_cards(report: dict[str, Any]) -> tuple[list[DeckCardRecord], list[DeckCardRecord]]:
    set_codes = report["format_policy"]["set_codes"]
    selected_group = report["selected_target"]["group"]
    cards = load_cards(DEFAULT_DB, set_codes)
    selected = [
        card
        for card in cards
        if get_card_group(card, "auto")[0] == selected_group
        and not is_excluded_from_main_deck(card)
        and card.deck_limit > 0
    ]
    if not selected:
        raise ValueError(f"No cards found for selected group: {selected_group}")
    return cards, selected


def card_sort_key(card: DeckCardRecord) -> tuple[int, str, str]:
    grade = card.grade if card.grade is not None else 99
    return grade, card.series_code, card.card_id


def card_entry(card: DeckCardRecord, count: int) -> dict[str, Any]:
    return {
        "card_id": card.card_id,
        "count": count,
        "zone": "main",
        "name_th": getattr(card, "name_th", ""),
        "series_code": card.series_code,
        "clan": card.clan,
        "nation": card.nation,
        "grade": card.grade,
        "trigger": card.trigger,
        "deck_limit": card.deck_limit,
        "type_1": card.type_1,
    }


def add_count(entries: dict[str, dict[str, Any]], card: DeckCardRecord, count: int) -> int:
    if count <= 0:
        return 0
    existing = entries.get(card.card_id)
    used = int(existing["count"]) if existing else 0
    available = max(0, card.deck_limit - used)
    add = min(count, available)
    if add <= 0:
        return 0
    if existing:
        existing["count"] = used + add
    else:
        entries[card.card_id] = card_entry(card, add)
    return add


def build_valid_entries(selected_cards: Sequence[DeckCardRecord]) -> list[dict[str, Any]]:
    non_triggers = sorted([card for card in selected_cards if not is_trigger_card(card)], key=card_sort_key)
    triggers = sorted([card for card in selected_cards if is_trigger_card(card)], key=card_sort_key)

    entries: dict[str, dict[str, Any]] = {}
    for grade in sorted(REQUIRED_SETUP_GRADES):
        grade_card = next((card for card in non_triggers if card.grade == grade), None)
        if grade_card is None:
            raise ValueError(f"Selected slice lacks non-trigger grade {grade} setup card")
        add_count(entries, grade_card, 1)

    non_trigger_count = sum(
        entry["count"]
        for entry in entries.values()
        if not entry.get("trigger")
    )
    for card in non_triggers:
        if non_trigger_count >= NON_TRIGGER_TARGET:
            break
        non_trigger_count += add_count(entries, card, NON_TRIGGER_TARGET - non_trigger_count)

    trigger_count = 0
    for card in triggers:
        if trigger_count >= TRIGGER_TARGET:
            break
        added = add_count(entries, card, TRIGGER_TARGET - trigger_count)
        trigger_count += added

    if non_trigger_count != NON_TRIGGER_TARGET or trigger_count != TRIGGER_TARGET:
        raise ValueError(
            f"Could not build valid fixture: non_trigger={non_trigger_count}, trigger={trigger_count}"
        )

    return sorted(entries.values(), key=lambda entry: (entry.get("grade") if entry.get("grade") is not None else 99, entry["card_id"]))


def normalize_entries(entries: Sequence[dict[str, Any]]) -> list[dict[str, Any]]:
    return [
        entry
        for entry in sorted(deepcopy(list(entries)), key=lambda item: (item.get("grade") if item.get("grade") is not None else 99, item["card_id"]))
        if int(entry.get("count", 0)) > 0
    ]


def decrement_first(entries: list[dict[str, Any]], predicate, amount: int = 1) -> int:
    remaining = amount
    for entry in entries:
        if remaining <= 0:
            break
        if not predicate(entry):
            continue
        take = min(int(entry["count"]), remaining)
        entry["count"] = int(entry["count"]) - take
        remaining -= take
    return amount - remaining


def increment_or_add(entries: list[dict[str, Any]], card: DeckCardRecord, amount: int) -> int:
    for entry in entries:
        if entry["card_id"] == card.card_id:
            available = card.deck_limit - int(entry["count"])
            add = min(amount, max(0, available))
            entry["count"] = int(entry["count"]) + add
            return add
    entries.append(card_entry(card, min(amount, card.deck_limit)))
    return min(amount, card.deck_limit)


def build_short_main(entries: list[dict[str, Any]]) -> list[dict[str, Any]]:
    result = deepcopy(entries)
    decrement_first(result, lambda entry: not entry.get("trigger") and int(entry["count"]) > 0, 1)
    return normalize_entries(result)


def build_bad_trigger_count(entries: list[dict[str, Any]], selected_cards: Sequence[DeckCardRecord]) -> list[dict[str, Any]]:
    result = deepcopy(entries)
    removed = decrement_first(result, lambda entry: bool(entry.get("trigger")), 1)
    if removed != 1:
        raise ValueError("Could not remove trigger from valid fixture")
    non_trigger_ids = {entry["card_id"]: int(entry["count"]) for entry in result if not entry.get("trigger")}
    for card in sorted([card for card in selected_cards if not is_trigger_card(card)], key=card_sort_key):
        if non_trigger_ids.get(card.card_id, 0) < card.deck_limit:
            increment_or_add(result, card, 1)
            break
    return normalize_entries(result)


def build_missing_grade(entries: list[dict[str, Any]], selected_cards: Sequence[DeckCardRecord], missing_grade: int = 3) -> list[dict[str, Any]]:
    result = deepcopy(entries)
    removed = 0
    for entry in result:
        if entry.get("grade") == missing_grade:
            removed += int(entry["count"])
            entry["count"] = 0
    if removed == 0:
        raise ValueError(f"Valid fixture did not contain grade {missing_grade}")

    current_counts = {entry["card_id"]: int(entry["count"]) for entry in result}
    replacement_pool = [
        card
        for card in selected_cards
        if not is_trigger_card(card) and card.grade != missing_grade
    ]
    for card in sorted(replacement_pool, key=card_sort_key):
        if removed <= 0:
            break
        available = card.deck_limit - current_counts.get(card.card_id, 0)
        add = min(removed, max(0, available))
        if add <= 0:
            continue
        increment_or_add(result, card, add)
        current_counts[card.card_id] = current_counts.get(card.card_id, 0) + add
        removed -= add
    if removed:
        raise ValueError("Could not refill missing-grade fixture")
    return normalize_entries(result)


def build_copy_limit_exceeded(entries: list[dict[str, Any]]) -> list[dict[str, Any]]:
    result = deepcopy(entries)
    target = next(
        (
            entry
            for entry in result
            if not entry.get("trigger") and int(entry["count"]) >= int(entry["deck_limit"])
        ),
        None,
    )
    if target is None:
        raise ValueError("No saturated card found for copy-limit fixture")
    target["count"] = int(target["deck_limit"]) + 1
    decrement_first(
        result,
        lambda entry: (
            entry["card_id"] != target["card_id"]
            and not entry.get("trigger")
            and int(entry["count"]) > 0
        ),
        1,
    )
    return normalize_entries(result)


def build_identity_mismatch(
    entries: list[dict[str, Any]],
    all_cards: Sequence[DeckCardRecord],
    report: dict[str, Any],
) -> list[dict[str, Any]]:
    selected_group = report["selected_target"]["group"]
    result = deepcopy(entries)
    replace_index = next(
        i
        for i, entry in enumerate(result)
        if not entry.get("trigger") and int(entry["count"]) > 0
    )
    replaced = result[replace_index]
    external = next(
        card
        for card in sorted(all_cards, key=card_sort_key)
        if get_card_group(card, "auto")[0] != selected_group
        and not is_excluded_from_main_deck(card)
        and bool(card.trigger) == bool(replaced.get("trigger"))
        and card.grade == replaced.get("grade")
        and card.deck_limit > 0
    )
    result[replace_index]["count"] = int(result[replace_index]["count"]) - 1
    increment_or_add(result, external, 1)
    return normalize_entries(result)


def card_index(cards: Sequence[DeckCardRecord]) -> dict[str, DeckCardRecord]:
    return {card.card_id: card for card in cards}


def validate_fixture(
    fixture: dict[str, Any],
    all_cards: Sequence[DeckCardRecord],
    report: dict[str, Any],
) -> dict[str, Any]:
    selected_group = report["selected_target"]["group"]
    allowed_sets = set(report["format_policy"]["set_codes"])
    index = card_index(all_cards)
    reasons: list[str] = []
    main_count = 0
    trigger_count = 0
    grade_counts = {str(grade): 0 for grade in sorted(REQUIRED_SETUP_GRADES)}
    per_card_counts: dict[str, int] = {}

    for entry in fixture["cards"]:
        card_id = entry["card_id"]
        count = int(entry.get("count", 0))
        if count <= 0:
            reasons.append(f"non_positive_count:{card_id}")
            continue
        card = index.get(card_id)
        if card is None:
            reasons.append(f"unknown_card:{card_id}")
            continue
        per_card_counts[card_id] = per_card_counts.get(card_id, 0) + count
        main_count += count
        if is_excluded_from_main_deck(card):
            reasons.append(f"main_deck_excluded_type:{card_id}:{card.type_1}")
        if card.series_code not in allowed_sets:
            reasons.append(f"outside_set_scope:{card_id}:{card.series_code}")
        if get_card_group(card, "auto")[0] != selected_group:
            reasons.append(f"identity_mismatch:{card_id}")
        if is_trigger_card(card):
            trigger_count += count
        if card.grade in REQUIRED_SETUP_GRADES and not is_trigger_card(card):
            grade_counts[str(card.grade)] += count

    for card_id, count in sorted(per_card_counts.items()):
        card = index[card_id]
        if count > card.deck_limit:
            reasons.append(f"copy_limit_exceeded:{card_id}:{count}>{card.deck_limit}")

    if main_count != MAIN_DECK_SIZE:
        reasons.append(f"main_count:{main_count}!={MAIN_DECK_SIZE}")
    if trigger_count != TRIGGER_TARGET:
        reasons.append(f"trigger_count:{trigger_count}!={TRIGGER_TARGET}")
    for grade in sorted(REQUIRED_SETUP_GRADES):
        if grade_counts[str(grade)] <= 0:
            reasons.append(f"missing_setup_grade:{grade}")

    return {
        "accepted": not reasons,
        "reasons": reasons,
        "main_count": main_count,
        "trigger_count": trigger_count,
        "grade_counts": grade_counts,
    }


def build_fixtures(report: dict[str, Any]) -> dict[str, Any]:
    all_cards, selected_cards = load_slice_cards(report)
    valid_entries = build_valid_entries(selected_cards)

    fixture_specs = [
        {
            "fixture_id": "classic_core_selected_group_valid_minimal",
            "expected": "pass",
            "description": "Valid minimal selected-slice deck by count, trigger, identity, grade setup, and runtime deck_limit.",
            "cards": valid_entries,
        },
        {
            "fixture_id": "classic_core_selected_group_short_main",
            "expected": "fail",
            "expected_reasons": ["main_count"],
            "description": "Rejects a 49-card main deck.",
            "cards": build_short_main(valid_entries),
        },
        {
            "fixture_id": "classic_core_selected_group_bad_trigger_count",
            "expected": "fail",
            "expected_reasons": ["trigger_count"],
            "description": "Rejects a 50-card deck with 15 triggers.",
            "cards": build_bad_trigger_count(valid_entries, selected_cards),
        },
        {
            "fixture_id": "classic_core_selected_group_missing_grade_3_setup",
            "expected": "fail",
            "expected_reasons": ["missing_setup_grade:3"],
            "description": "Rejects a deck without a non-trigger grade 3 setup choice.",
            "cards": build_missing_grade(valid_entries, selected_cards, 3),
        },
        {
            "fixture_id": "classic_core_selected_group_copy_limit_exceeded",
            "expected": "fail",
            "expected_reasons": ["copy_limit_exceeded"],
            "description": "Rejects a deck exceeding runtime per-card deck_limit.",
            "cards": build_copy_limit_exceeded(valid_entries),
        },
        {
            "fixture_id": "classic_core_selected_group_identity_mismatch",
            "expected": "fail",
            "expected_reasons": ["identity_mismatch"],
            "description": "Rejects a deck containing a card outside the selected clan identity.",
            "cards": build_identity_mismatch(valid_entries, all_cards, report),
        },
    ]

    for fixture in fixture_specs:
        validation = validate_fixture(fixture, all_cards, report)
        fixture["validation"] = validation
        fixture["expectation_met"] = (
            (fixture["expected"] == "pass" and validation["accepted"])
            or (fixture["expected"] == "fail" and not validation["accepted"])
        )

    return {
        "version": "M35-A3",
        "description": "Minimal deck legality fixtures for selected first slice",
        "based_on": str(SELECTED_SLICE_REPORT.relative_to(ROOT)),
        "selected_target": report["selected_target"],
        "fixture_policy": {
            "policy_level": "minimal_selected_slice_only_not_full_official_legality",
            "main_deck_exact": MAIN_DECK_SIZE,
            "trigger_target": TRIGGER_TARGET,
            "required_non_trigger_setup_grades": sorted(REQUIRED_SETUP_GRADES),
            "identity_field": report["selected_target"]["group_field"],
            "selected_identity": report["selected_target"]["group"],
            "set_scope": report["format_policy"]["set_specs"],
            "copy_limit_source": "runtime SQLite cards.deck_limit",
            "deferred_source_backed_special_limits": [
                "official heal trigger maximum",
                "format-wide copy-limit exceptions beyond runtime deck_limit",
            ],
        },
        "fixtures": fixture_specs,
        "all_expectations_met": all(fixture["expectation_met"] for fixture in fixture_specs),
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    lines = [
        "# M35-A3 First Slice Deck Legality Fixtures",
        "",
        "## Selected Target",
        "",
        f"- Slice: `{target['slice']}`",
        f"- Era preset: `{target['era_preset']}`",
        f"- Group: `{target['group']}`",
        "",
        "## Fixtures",
        "",
    ]
    for fixture in report["fixtures"]:
        validation = fixture["validation"]
        status = "accepted" if validation["accepted"] else "rejected"
        lines.append(
            f"- `{fixture['fixture_id']}` expected `{fixture['expected']}` -> "
            f"`{status}`; reasons: `{', '.join(validation['reasons']) or 'none'}`"
        )
    lines.extend(
        [
            "",
            "## Deferred Limits",
            "",
            "- Official heal trigger maximum is noted but not broadly enforced in this fixture slice.",
            "- Runtime per-card `deck_limit` is enforced for the selected fixtures.",
            "",
            "## Next",
            "",
            "`M35-A4`: Refresh first-slice feasibility with legality-readiness and missing-rule gates.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35-A3 first-slice deck legality fixtures.")
    parser.add_argument("--selected-report", type=Path, default=SELECTED_SLICE_REPORT)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    selected_report = load_selected_report(args.selected_report)
    fixtures = build_fixtures(selected_report)
    json_path = args.output_dir / "m35_a3_first_slice_deck_legality_fixtures.json"
    md_path = args.output_dir / "m35_a3_first_slice_deck_legality_fixtures.md"
    write_json(fixtures, json_path)
    write_markdown(fixtures, md_path)
    print(f"M35-A3 fixtures wrote {json_path}")
    print(f"M35-A3 fixture summary wrote {md_path}")
    print(f"expectations_met={fixtures['all_expectations_met']}")
    return 0 if fixtures["all_expectations_met"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
