"""Analyze theoretical deck-construction possibilities by clan or nation.

This is an offline math/reporting helper. It does not validate complete
official deck legality and does not run inside gameplay.
"""

from __future__ import annotations

import argparse
import csv
import json
import sqlite3
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Sequence

ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.combo.discover_clan_combos import (  # noqa: E402
    DEFAULT_DB,
    ERA_SET_SPECS,
    expand_set_specs,
    get_card_group,
)


OUTPUT_DIR = ROOT / "outputs" / "deck_possibility"
MAIN_DECK_SIZE = 50
TRIGGER_TARGET = 16
NON_TRIGGER_TARGET = MAIN_DECK_SIZE - TRIGGER_TARGET
G_ZONE_TARGET = 16


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


@dataclass(frozen=True)
class DeckCardRecord:
    card_id: str
    series_code: str
    clan: str
    nation: str
    grade: int | None
    trigger: str
    deck_limit: int
    type_1: str
    type_2: str


def load_cards(database_path: Path, set_codes: Sequence[str] | None = None) -> list[DeckCardRecord]:
    if not database_path.exists():
        raise FileNotFoundError(f"Card database not found: {database_path}")

    where = ""
    params: tuple[str, ...] = ()
    if set_codes is not None:
        if not set_codes:
            return []
        placeholders = ",".join("?" for _ in set_codes)
        where = f"WHERE series_code IN ({placeholders})"
        params = tuple(set_codes)

    query = f"""
        SELECT card_id, series_code, clan, nation, grade, trigger, deck_limit,
               type_1, type_2
        FROM cards
        {where}
        ORDER BY clan, nation, series_code, card_id
    """
    connection = sqlite3.connect(str(database_path))
    try:
        rows = connection.execute(query, params).fetchall()
    finally:
        connection.close()

    return [
        DeckCardRecord(
            card_id=row[0] or "",
            series_code=row[1] or "",
            clan=row[2] or "",
            nation=row[3] or "",
            grade=row[4],
            trigger=row[5] or "",
            deck_limit=max(0, int(row[6] or 0)),
            type_1=row[7] or "",
            type_2=row[8] or "",
        )
        for row in rows
    ]


def is_g_zone_card(card: DeckCardRecord) -> bool:
    return card.type_1 in {"G-Unit", "G-Guardian"}


def is_excluded_from_main_deck(card: DeckCardRecord) -> bool:
    return card.type_1 in {"G-Unit", "G-Guardian", "Token Unit", "Marker", "Crest"}


def is_trigger_card(card: DeckCardRecord) -> bool:
    return bool(card.trigger) or card.type_1 in {"Trigger Unit", "Trigger Order"}


def bounded_distribution_count(limits: Sequence[int], target: int) -> int:
    if target < 0:
        return 0
    dp = [0] * (target + 1)
    dp[0] = 1
    for raw_limit in limits:
        limit = max(0, min(int(raw_limit), target))
        if limit == 0:
            continue
        next_dp = [0] * (target + 1)
        window = 0
        for count in range(target + 1):
            window += dp[count]
            if count - limit - 1 >= 0:
                window -= dp[count - limit - 1]
            next_dp[count] = window
        dp = next_dp
    return dp[target]


def int_summary(value: int) -> dict[str, Any]:
    text = str(value)
    if value == 0:
        scientific = "0"
    elif len(text) <= 6:
        scientific = text
    else:
        scientific = f"{text[0]}.{text[1:4]}e+{len(text) - 1}"
    return {
        "exact": text,
        "digits": len(text),
        "scientific": scientific,
    }


def grade_capacity(cards: Sequence[DeckCardRecord]) -> dict[str, int]:
    result = {str(grade): 0 for grade in range(5)}
    result["unknown"] = 0
    for card in cards:
        key = str(card.grade) if card.grade is not None and 0 <= card.grade <= 4 else "unknown"
        result[key] += card.deck_limit
    return result


def grade_unique_counts(cards: Sequence[DeckCardRecord]) -> dict[str, int]:
    result = {str(grade): 0 for grade in range(5)}
    result["unknown"] = 0
    for card in cards:
        key = str(card.grade) if card.grade is not None and 0 <= card.grade <= 4 else "unknown"
        result[key] += 1
    return result


def trigger_capacity_by_type(cards: Sequence[DeckCardRecord]) -> dict[str, int]:
    result: dict[str, int] = {}
    for card in cards:
        if not is_trigger_card(card):
            continue
        trigger = card.trigger or card.type_1 or "Unknown"
        result[trigger] = result.get(trigger, 0) + card.deck_limit
    return dict(sorted(result.items()))


def analyze_group(group: str, group_field: str, cards: Sequence[DeckCardRecord]) -> dict[str, Any]:
    main_cards = [card for card in cards if not is_excluded_from_main_deck(card) and card.deck_limit > 0]
    g_cards = [card for card in cards if is_g_zone_card(card) and card.deck_limit > 0]
    trigger_cards = [card for card in main_cards if is_trigger_card(card)]
    non_trigger_cards = [card for card in main_cards if not is_trigger_card(card)]
    ride_grade_cards = [card for card in non_trigger_cards if card.grade in {0, 1, 2, 3}]

    main_limits = [card.deck_limit for card in main_cards]
    trigger_limits = [card.deck_limit for card in trigger_cards]
    non_trigger_limits = [card.deck_limit for card in non_trigger_cards]
    g_limits = [card.deck_limit for card in g_cards]

    total_50 = bounded_distribution_count(main_limits, MAIN_DECK_SIZE)
    trigger_16 = bounded_distribution_count(trigger_limits, TRIGGER_TARGET)
    non_trigger_34 = bounded_distribution_count(non_trigger_limits, NON_TRIGGER_TARGET)
    exact_16_trigger = trigger_16 * non_trigger_34
    g_zone_16 = bounded_distribution_count(g_limits, G_ZONE_TARGET)

    ride_unique = grade_unique_counts(ride_grade_cards)
    ride_choice_configs = 1
    ride_deck_feasible = True
    for grade in range(4):
        count = ride_unique[str(grade)]
        ride_choice_configs *= count
        if count == 0:
            ride_deck_feasible = False

    main_capacity = sum(main_limits)
    trigger_capacity = sum(trigger_limits)
    non_trigger_capacity = sum(non_trigger_limits)
    g_capacity = sum(g_limits)
    issues: list[str] = []
    if main_capacity < MAIN_DECK_SIZE:
        issues.append("main_capacity_below_50")
    if trigger_capacity < TRIGGER_TARGET:
        issues.append("trigger_capacity_below_16")
    if non_trigger_capacity < NON_TRIGGER_TARGET:
        issues.append("non_trigger_capacity_below_34")
    if not ride_deck_feasible:
        issues.append("missing_ride_grade_0_1_2_3")

    return {
        "group": group,
        "group_field": group_field,
        "card_count": len(cards),
        "main_card_count": len(main_cards),
        "trigger_card_count": len(trigger_cards),
        "non_trigger_card_count": len(non_trigger_cards),
        "g_zone_card_count": len(g_cards),
        "main_capacity": main_capacity,
        "trigger_capacity": trigger_capacity,
        "non_trigger_capacity": non_trigger_capacity,
        "g_zone_capacity": g_capacity,
        "grade_capacity": grade_capacity(main_cards),
        "grade_unique_count": grade_unique_counts(main_cards),
        "trigger_capacity_by_type": trigger_capacity_by_type(main_cards),
        "feasible": {
            "basic_50_card_main": main_capacity >= MAIN_DECK_SIZE and total_50 > 0,
            "main_with_16_triggers_34_non_triggers": trigger_capacity >= TRIGGER_TARGET
            and non_trigger_capacity >= NON_TRIGGER_TARGET
            and exact_16_trigger > 0,
            "ride_deck_grade_0_1_2_3_choice": ride_deck_feasible,
            "g_zone_16": g_capacity >= G_ZONE_TARGET and g_zone_16 > 0,
        },
        "possibility_counts": {
            "main_50_any_mix": int_summary(total_50),
            "main_50_exact_16_triggers": int_summary(exact_16_trigger),
            "trigger_16_component": int_summary(trigger_16),
            "non_trigger_34_component": int_summary(non_trigger_34),
            "ride_deck_grade_0_1_2_3_choices": int_summary(ride_choice_configs if ride_deck_feasible else 0),
            "g_zone_16": int_summary(g_zone_16),
        },
        "issues": issues,
    }


def build_deck_possibility_report(
    cards: Sequence[DeckCardRecord],
    set_codes: Sequence[str] | None,
    preset: str,
    group_mode: str = "auto",
) -> dict[str, Any]:
    grouped: dict[str, list[DeckCardRecord]] = {}
    group_fields: dict[str, str] = {}
    for card in cards:
        group, group_field = get_card_group(card, group_mode)
        grouped.setdefault(group, []).append(card)
        group_fields.setdefault(group, group_field)

    group_reports = [
        analyze_group(group, group_fields.get(group, "unknown"), grouped[group])
        for group in sorted(grouped)
    ]

    return {
        "schema_version": 1,
        "generator": "tools/deck/analyze_deck_possibilities.py",
        "scope": {
            "preset": preset,
            "set_codes": list(set_codes) if set_codes is not None else None,
            "available_set_codes": sorted({card.series_code for card in cards}),
            "missing_set_codes": [code for code in set_codes if code not in {card.series_code for card in cards}]
            if set_codes is not None
            else [],
            "group_mode": group_mode,
            "main_deck_size": MAIN_DECK_SIZE,
            "trigger_target": TRIGGER_TARGET,
            "non_trigger_target": NON_TRIGGER_TARGET,
            "note": "offline theoretical count-distribution analysis, not full official deck legality",
        },
        "card_count": len(cards),
        "group_count": len(group_reports),
        "groups": group_reports,
    }


def write_report(report: dict[str, Any], output_path: Path) -> None:
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_summary_csv(reports: Sequence[dict[str, Any]], output_path: Path) -> None:
    output_path.parent.mkdir(parents=True, exist_ok=True)
    fieldnames = [
        "preset",
        "group",
        "group_field",
        "card_count",
        "main_card_count",
        "trigger_card_count",
        "non_trigger_card_count",
        "g_zone_card_count",
        "main_capacity",
        "trigger_capacity",
        "non_trigger_capacity",
        "g_zone_capacity",
        "basic_50_card_main",
        "main_with_16_triggers_34_non_triggers",
        "ride_deck_grade_0_1_2_3_choice",
        "g_zone_16",
        "main_50_any_mix_digits",
        "main_50_any_mix_scientific",
        "main_50_exact_16_triggers_digits",
        "main_50_exact_16_triggers_scientific",
        "ride_deck_choices_exact",
        "issues",
    ]
    with output_path.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        for report in reports:
            preset = report["scope"]["preset"]
            for group in report["groups"]:
                counts = group["possibility_counts"]
                feasible = group["feasible"]
                writer.writerow(
                    {
                        "preset": preset,
                        "group": group["group"],
                        "group_field": group["group_field"],
                        "card_count": group["card_count"],
                        "main_card_count": group["main_card_count"],
                        "trigger_card_count": group["trigger_card_count"],
                        "non_trigger_card_count": group["non_trigger_card_count"],
                        "g_zone_card_count": group["g_zone_card_count"],
                        "main_capacity": group["main_capacity"],
                        "trigger_capacity": group["trigger_capacity"],
                        "non_trigger_capacity": group["non_trigger_capacity"],
                        "g_zone_capacity": group["g_zone_capacity"],
                        "basic_50_card_main": feasible["basic_50_card_main"],
                        "main_with_16_triggers_34_non_triggers": feasible["main_with_16_triggers_34_non_triggers"],
                        "ride_deck_grade_0_1_2_3_choice": feasible["ride_deck_grade_0_1_2_3_choice"],
                        "g_zone_16": feasible["g_zone_16"],
                        "main_50_any_mix_digits": counts["main_50_any_mix"]["digits"],
                        "main_50_any_mix_scientific": counts["main_50_any_mix"]["scientific"],
                        "main_50_exact_16_triggers_digits": counts["main_50_exact_16_triggers"]["digits"],
                        "main_50_exact_16_triggers_scientific": counts["main_50_exact_16_triggers"]["scientific"],
                        "ride_deck_choices_exact": counts["ride_deck_grade_0_1_2_3_choices"]["exact"],
                        "issues": ";".join(group["issues"]),
                    }
                )


def build_summary_json(reports: Sequence[dict[str, Any]]) -> dict[str, Any]:
    rows: list[dict[str, Any]] = []
    for report in reports:
        groups = report["groups"]
        rows.append(
            {
                "preset": report["scope"]["preset"],
                "card_count": report["card_count"],
                "group_count": report["group_count"],
                "basic_50_feasible_groups": sum(1 for group in groups if group["feasible"]["basic_50_card_main"]),
                "exact_16_trigger_feasible_groups": sum(
                    1 for group in groups if group["feasible"]["main_with_16_triggers_34_non_triggers"]
                ),
                "ride_deck_feasible_groups": sum(1 for group in groups if group["feasible"]["ride_deck_grade_0_1_2_3_choice"]),
                "g_zone_16_feasible_groups": sum(1 for group in groups if group["feasible"]["g_zone_16"]),
                "missing_set_codes": report["scope"]["missing_set_codes"],
            }
        )
    return {
        "schema_version": 1,
        "generator": "tools/deck/analyze_deck_possibilities.py",
        "note": "offline theoretical deck possibility summary, not full official deck legality",
        "reports": rows,
    }


def write_summary_json(reports: Sequence[dict[str, Any]], output_path: Path) -> None:
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps(build_summary_json(reports), ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def preset_to_set_codes(preset: str) -> list[str] | None:
    if preset == "full_runtime":
        return None
    return expand_set_specs(ERA_SET_SPECS[preset])


def output_path_for_preset(preset: str) -> Path:
    return OUTPUT_DIR / f"{preset}_deck_possibility.json"


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Analyze theoretical deck construction possibilities by clan/nation.")
    parser.add_argument("--db", type=Path, default=DEFAULT_DB)
    parser.add_argument("--preset", choices=["full_runtime", *sorted(ERA_SET_SPECS)], default="full_runtime")
    parser.add_argument("--all-presets", action="store_true")
    parser.add_argument("--group-mode", choices=("auto", "clan", "nation"), default="auto")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    presets = ["full_runtime", *ERA_SET_SPECS.keys()] if args.all_presets else [args.preset]
    reports: list[dict[str, Any]] = []
    for preset in presets:
        set_codes = preset_to_set_codes(preset)
        cards = load_cards(args.db, set_codes)
        report = build_deck_possibility_report(cards, set_codes, preset, group_mode=args.group_mode)
        output_path = args.output_dir / f"{preset}_deck_possibility.json"
        write_report(report, output_path)
        reports.append(report)
        print(f"Deck possibility wrote {output_path} | cards={report['card_count']} groups={report['group_count']}")
        missing = report["scope"]["missing_set_codes"]
        if missing:
            print("Missing set codes: " + ", ".join(missing))

    if args.all_presets:
        summary_path = args.output_dir / "deck_possibility_summary.csv"
        summary_json_path = args.output_dir / "deck_possibility_summary.json"
        write_summary_csv(reports, summary_path)
        write_summary_json(reports, summary_json_path)
        print(f"Deck possibility summary wrote {summary_path}")
        print(f"Deck possibility summary JSON wrote {summary_json_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
