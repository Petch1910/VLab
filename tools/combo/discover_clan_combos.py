"""Discover advisory clan combo pairs from the local Vanguard TH card pack.

This is an offline data-mining helper. It may inspect card text because it
does not run during a live match and does not mutate game state.
"""

from __future__ import annotations

import argparse
import json
import re
import sqlite3
import sys
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any, Iterable, Sequence


ROOT = Path(__file__).resolve().parents[2]
DEFAULT_DB = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"
DEFAULT_OUTPUT = ROOT / "outputs" / "combo_discovery" / "td01_td06_bt01_bt09_eb01_eb05_clan_combos.json"
DEFAULT_SET_SPECS = ("TD01-TD06", "BT01-BT09", "EB01-EB05")
DEFAULT_MIN_SCORE = 60
OUTPUT_DIR = ROOT / "outputs" / "combo_discovery"

ERA_SET_SPECS: dict[str, tuple[str, ...]] = {
    "classic_part1": ("TD01-TD06", "BT01-BT09", "EB01-EB05"),
    "link_joker_legion_mate": ("TD07-TD17", "BT10-BT17", "EB06-EB12"),
    "g_series_first": (
        "G-TD01-G-TD09",
        "G-SD01-G-SD02",
        "G-BT01-G-BT08",
        "G-CB01-G-CB04",
        "G-TCB01-G-TCB02",
    ),
    "g_next_z": (
        "G-TD10-G-TD15",
        "G-BT09-G-BT14",
        "G-CB05-G-CB07",
        "G-CHB01-G-CHB03",
    ),
    "v_reboot": ("V-TD01-V-TD09", "V-BT01-V-BT06", "V-EB01-V-EB09"),
    "v_shinemon_if": ("V-TD10-V-TD12", "V-BT07-V-BT14", "V-EB10-V-EB15", "V-SS01-V-SS10"),
    "d_overdress": ("D-SD01-D-SD05", "D-BT01-D-BT05", "D-LBT01-D-LBT02"),
    "d_willdress": ("D-SD06", "D-TD01-D-TD03", "D-BT06-D-BT13", "D-LBT03-D-LBT04", "D-SS01-D-SS11"),
    "dz_divinez": ("DZ-SD01-DZ-SD06", "DZ-BT01-DZ-BT05", "DZ-SS01-DZ-SS03"),
}


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


@dataclass(frozen=True)
class CardRecord:
    card_id: str
    name_th: str
    text_th: str
    series_code: str
    clan: str
    grade: int | None
    power: int | None
    shield: int | None
    trigger: str
    type_1: str
    type_2: str = ""
    nation: str = ""


@dataclass(frozen=True)
class CardFeatures:
    tags: frozenset[str]
    clan_refs: frozenset[str]
    grade_refs: frozenset[int]
    named_refs: frozenset[str]
    power_bonuses: tuple[int, ...]


@dataclass(frozen=True)
class ScoreReason:
    points: int
    tag: str
    text: str


@dataclass(frozen=True)
class ComboPair:
    pair_id: str
    clan: str
    score: int
    card_a: CardRecord
    card_b: CardRecord
    reasons: tuple[ScoreReason, ...]
    shared_tags: tuple[str, ...]
    synergy_tags: tuple[str, ...]


def expand_set_specs(specs: Sequence[str]) -> list[str]:
    expanded: list[str] = []
    seen: set[str] = set()
    for raw_spec in specs:
        spec = raw_spec.strip().upper().replace("–", "-").replace("—", "-")
        if not spec:
            continue

        match = re.fullmatch(r"([A-Z]+(?:-[A-Z]+)*)(\d{2})-([A-Z]+(?:-[A-Z]+)*)?(\d{2})", spec)
        if match:
            start_prefix, start_number, end_prefix, end_number = match.groups()
            end_prefix = end_prefix or start_prefix
            if start_prefix != end_prefix:
                raise ValueError(f"Mixed-prefix set range is not supported: {raw_spec}")
            start = int(start_number)
            end = int(end_number)
            if end < start:
                raise ValueError(f"Descending set range is not supported: {raw_spec}")
            for number in range(start, end + 1):
                code = f"{start_prefix}{number:02d}"
                if code not in seen:
                    expanded.append(code)
                    seen.add(code)
            continue

        if not re.fullmatch(r"[A-Z]+(?:-[A-Z]+)*\d{2}", spec):
            raise ValueError(f"Invalid set spec: {raw_spec}")
        if spec not in seen:
            expanded.append(spec)
            seen.add(spec)
    return expanded


def default_output_for_preset(preset: str | None) -> Path:
    if preset:
        return OUTPUT_DIR / f"{preset}_clan_combos.json"
    return DEFAULT_OUTPUT


def load_cards(database_path: Path, set_codes: Sequence[str]) -> list[CardRecord]:
    if not database_path.exists():
        raise FileNotFoundError(f"Card database not found: {database_path}")
    if not set_codes:
        raise ValueError("At least one set code is required.")

    placeholders = ",".join("?" for _ in set_codes)
    query = f"""
        SELECT card_id, name_th, text_th, series_code, clan, nation, grade,
               power, shield, trigger, type_1, type_2
        FROM cards
        WHERE series_code IN ({placeholders})
        ORDER BY clan, series_code, card_id
    """
    connection = sqlite3.connect(str(database_path))
    try:
        rows = connection.execute(query, tuple(set_codes)).fetchall()
    finally:
        connection.close()

    return [
        CardRecord(
            card_id=row[0] or "",
            name_th=row[1] or "",
            text_th=row[2] or "",
            series_code=row[3] or "",
            clan=row[4] or "",
            nation=row[5] or "",
            grade=row[6],
            power=row[7],
            shield=row[8],
            trigger=row[9] or "",
            type_1=row[10] or "",
            type_2=row[11] or "",
        )
        for row in rows
    ]


def extract_features(card: CardRecord) -> CardFeatures:
    text = card.text_th or ""
    lower = text.lower()
    tags: set[str] = set()

    def has(*needles: str) -> bool:
        return any(needle.lower() in lower for needle in needles)

    if "[act]" in lower:
        tags.add("act_ability")
    if "[auto]" in lower:
        tags.add("auto_ability")
    if "[cont]" in lower:
        tags.add("cont_ability")
    if "(vc)" in lower:
        tags.add("vc")
    if "(rc)" in lower:
        tags.add("rc")
    if "เมื่อยูนิทนี้เข้าสู่ช่อง" in text or "เมื่อการ์ดนี้เข้าสู่ช่อง" in text:
        tags.add("on_call")
    if "ไรด์ทับ" in text or "ไรด์" in text:
        tags.add("ride_related")
    if "เมื่อยูนิทนี้โจมตี" in text or "เมื่อยูนิทนี้ถูกบูสท์" in text or "โจมตี" in text:
        tags.add("attack_related")
    if "โจมตีฮิท" in text or "ฮิทแวนการ์ด" in text:
        tags.add("on_hit")
    if "บูสท์" in text or "boost" in lower:
        tags.add("boost_related")
    if "เอนด์เฟส" in text or "end phase" in lower:
        tags.add("end_phase")

    if has("counter blast", "counter-blast"):
        tags.add("counter_blast_cost")
    if has("soul blast", "soul-blast"):
        tags.add("soul_blast_cost")
    if "counter charge" in lower or "หงายการ์ด" in text:
        tags.add("counter_charge")
    if "soul charge" in lower or "ชาร์จโซล" in text:
        tags.add("soul_charge")
    if "โซล" in text and ("นำ" in text or "เข้าสู่" in text or "ใส่" in text):
        tags.add("soul_resource")

    power_bonus_matches = re.findall(r"\[Power\]\s*\+([0-9,]+)", text, flags=re.IGNORECASE)
    if power_bonus_matches:
        tags.add("power_plus")
    if re.search(r"\[Critical\]\s*\+", text, flags=re.IGNORECASE):
        tags.add("critical_plus")
    if "จั่วการ์ด" in text or "จั่ว" in text:
        tags.add("draw")
    if "ค้นหา" in text or "หาการ์ด" in text or "เปิดดู" in text:
        tags.add("search_deck")
    if "คอลลงช่อง" in text or "คอลการ์ด" in text or "superior call" in lower:
        tags.add("superior_call")
    if "สแตนด์" in text or "stand" in lower:
        tags.add("stand_unit")
    if "รีไทร์" in text or "retire" in lower:
        tags.add("retire")
    if "กลับขึ้นมือ" in text or "ขึ้นมือ" in text:
        tags.add("bounce_to_hand")
    if "กลับเข้ากอง" in text or "สลับกอง" in text:
        tags.add("deck_recycle")
    if "ดรอปโซน" in text:
        tags.add("drop_zone")
    if "ดาเมจโซน" in text:
        tags.add("damage_zone")

    if card.trigger:
        tags.add("trigger_unit")
    if card.grade is not None:
        tags.add(f"grade_{card.grade}")

    clan_refs = frozenset(clean_ref(ref) for ref in re.findall(r"<<([^>]+)>>", text) if clean_ref(ref))
    named_refs = frozenset(clean_ref(ref) for ref in re.findall(r'"([^"]+)"', text) if clean_ref(ref))
    grade_refs = frozenset(int(value) for value in re.findall(r"เกรด\s*([0-4])|grade\s*([0-4])", lower) for value in value if value)
    power_bonuses = tuple(sorted({int(value.replace(",", "")) for value in power_bonus_matches}))

    return CardFeatures(
        tags=frozenset(tags),
        clan_refs=clan_refs,
        grade_refs=grade_refs,
        named_refs=named_refs,
        power_bonuses=power_bonuses,
    )


def clean_ref(value: str) -> str:
    return re.sub(r"\s+", " ", value or "").strip()


def is_unknown_identity(value: str) -> bool:
    normalized = clean_ref(value).upper()
    return normalized in {"", "N/A", "NONE", "UNKNOWN", "NULL"}


def normalize_identity(value: str) -> str:
    normalized = clean_ref(value)
    if normalized.endswith(" D"):
        normalized = normalized[:-2].strip()
    return normalized


def get_card_group(card: CardRecord, group_mode: str = "auto") -> tuple[str, str]:
    clan = normalize_identity(card.clan)
    nation = normalize_identity(card.nation)
    if group_mode == "clan":
        if not is_unknown_identity(clan):
            return clan, "clan"
        if not is_unknown_identity(nation):
            return nation, "nation_fallback"
        return "Unknown", "unknown"
    if group_mode == "nation":
        if not is_unknown_identity(nation):
            return nation, "nation"
        if not is_unknown_identity(clan):
            return clan, "clan_fallback"
        return "Unknown", "unknown"
    if group_mode != "auto":
        raise ValueError(f"Unsupported group mode: {group_mode}")
    if not is_unknown_identity(clan):
        return clan, "clan"
    if not is_unknown_identity(nation):
        return nation, "nation"
    return "Unknown", "unknown"


def score_directed(source: CardRecord, target: CardRecord, source_features: CardFeatures, target_features: CardFeatures) -> list[ScoreReason]:
    reasons: list[ScoreReason] = []
    source_tags = source_features.tags
    target_tags = target_features.tags

    target_name = clean_ref(target.name_th)
    if target_name and (target_name in source_features.named_refs or target_name in source.text_th):
        reasons.append(ScoreReason(90, "named_reference", f"{source.card_id} names or directly references {target.card_id}."))

    target_identities = [normalize_identity(target.clan), normalize_identity(target.nation)]
    matching_identities = [
        identity
        for identity in target_identities
        if not is_unknown_identity(identity) and identity in source_features.clan_refs
    ]
    if matching_identities:
        reasons.append(ScoreReason(8, "clan_reference", f"{source.card_id} explicitly supports {matching_identities[0]}."))

    if source_features.grade_refs and target.grade in source_features.grade_refs:
        reasons.append(ScoreReason(18, "grade_target", f"{source.card_id} can look for or affect grade {target.grade}."))

    if "search_deck" in source_tags and target.grade is not None:
        grade_bonus = 8 if not source_features.grade_refs or target.grade in source_features.grade_refs else 0
        reasons.append(ScoreReason(18 + grade_bonus, "search_target", f"{source.card_id} searches/reveals deck pieces that can find {target.card_id}."))

    if "superior_call" in source_tags and target.grade is not None and target.grade <= 2:
        reasons.append(ScoreReason(22, "call_target", f"{source.card_id} can call lower-grade rear-guard targets like {target.card_id}."))

    if "power_plus" in source_tags and ("attack_related" in target_tags or "on_hit" in target_tags or target.power):
        bonus = max(source_features.power_bonuses) if source_features.power_bonuses else 0
        points = 16 + min(bonus // 1000, 8)
        reasons.append(ScoreReason(points, "power_pressure", f"{source.card_id} gives power that improves {target.card_id}'s attack pressure."))

    if "boost_related" in source_tags and ("attack_related" in target_tags or (target.grade is not None and target.grade >= 2)):
        reasons.append(ScoreReason(14, "boost_attack", f"{source.card_id} boost text pairs with attacker {target.card_id}."))

    if "stand_unit" in source_tags and "attack_related" in target_tags:
        reasons.append(ScoreReason(24, "restand_attack", f"{source.card_id} stand effect can create another attack with {target.card_id}."))

    if ("soul_charge" in source_tags or "soul_resource" in source_tags) and "soul_blast_cost" in target_tags:
        reasons.append(ScoreReason(24, "soul_resource", f"{source.card_id} helps pay soul costs for {target.card_id}."))

    if "counter_charge" in source_tags and "counter_blast_cost" in target_tags:
        reasons.append(ScoreReason(24, "counter_blast_resource", f"{source.card_id} helps sustain Counter Blast costs for {target.card_id}."))

    if "draw" in source_tags and ("counter_blast_cost" in target_tags or "soul_blast_cost" in target_tags or "superior_call" in target_tags):
        reasons.append(ScoreReason(10, "card_advantage_support", f"{source.card_id} draws into pieces or costs for {target.card_id}."))

    if "bounce_to_hand" in source_tags and "on_call" in target_tags:
        reasons.append(ScoreReason(18, "reuse_on_call", f"{source.card_id} can reuse on-call value from {target.card_id}."))

    if "retire" in source_tags and "drop_zone" in target_tags and "draw" in target_tags:
        reasons.append(ScoreReason(24, "retire_for_drop_value", f"{source.card_id} can enable drop-zone value from {target.card_id}."))

    if "on_hit" in source_tags and "power_plus" in target_tags:
        reasons.append(ScoreReason(14, "hit_pressure_support", f"{target.card_id} power support helps {source.card_id} reach hit-triggered pressure."))

    return reasons


def score_pair(left: CardRecord, right: CardRecord, features_by_id: dict[str, CardFeatures]) -> ComboPair | None:
    left_features = features_by_id[left.card_id]
    right_features = features_by_id[right.card_id]
    reasons = score_directed(left, right, left_features, right_features)
    reasons.extend(score_directed(right, left, right_features, left_features))
    if not reasons:
        return None

    deduped: dict[tuple[str, str], ScoreReason] = {}
    for reason in reasons:
        key = (reason.tag, reason.text)
        if key not in deduped:
            deduped[key] = reason

    reason_values = tuple(sorted(deduped.values(), key=lambda item: (-item.points, item.tag, item.text)))
    score = sum(reason.points for reason in reason_values)
    shared_tags = tuple(sorted(left_features.tags.intersection(right_features.tags)))
    synergy_tags = tuple(sorted({reason.tag for reason in reason_values}))
    return ComboPair(
        pair_id=f"{left.card_id}+{right.card_id}",
        clan=left.clan,
        score=score,
        card_a=left,
        card_b=right,
        reasons=reason_values,
        shared_tags=shared_tags,
        synergy_tags=synergy_tags,
    )


def build_clan_combo_report(
    cards: Sequence[CardRecord],
    set_codes: Sequence[str],
    top_per_clan: int = 20,
    min_score: int = DEFAULT_MIN_SCORE,
    group_mode: str = "auto",
) -> dict[str, Any]:
    features_by_id = {card.card_id: extract_features(card) for card in cards}
    clans: dict[str, list[CardRecord]] = {}
    group_fields: dict[str, str] = {}
    for card in cards:
        group, group_field = get_card_group(card, group_mode)
        clans.setdefault(group, []).append(card)
        group_fields.setdefault(group, group_field)

    clan_reports: list[dict[str, Any]] = []
    for clan in sorted(clans):
        clan_cards = sorted(clans[clan], key=lambda card: card.card_id)
        pairs: list[ComboPair] = []
        for index, left in enumerate(clan_cards):
            for right in clan_cards[index + 1 :]:
                pair = score_pair(left, right, features_by_id)
                if pair is not None and pair.score >= min_score:
                    pairs.append(pair)

        pairs.sort(key=lambda pair: (-pair.score, pair.pair_id))
        selected_pairs = pairs[: max(0, top_per_clan)]
        clan_reports.append(
            {
                "clan": clan,
                "group": clan,
                "group_field": group_fields.get(clan, "unknown"),
                "card_count": len(clan_cards),
                "set_codes": sorted({card.series_code for card in clan_cards}),
                "combo_pair_count": len(pairs),
                "top_pairs": [serialize_pair(pair) for pair in selected_pairs],
            }
        )

    return {
        "schema_version": 1,
        "generator": "tools/combo/discover_clan_combos.py",
        "scope": {
            "set_codes": list(set_codes),
            "available_set_codes": sorted({card.series_code for card in cards}),
            "missing_set_codes": [code for code in set_codes if code not in {card.series_code for card in cards}],
            "top_per_clan": top_per_clan,
            "min_score": min_score,
            "group_mode": group_mode,
            "note": "offline advisory only; not live effect resolution",
        },
        "card_count": len(cards),
        "clan_count": len(clan_reports),
        "group_count": len(clan_reports),
        "clans": clan_reports,
    }


def serialize_pair(pair: ComboPair) -> dict[str, Any]:
    return {
        "pair_id": pair.pair_id,
        "score": pair.score,
        "card_a": serialize_card(pair.card_a),
        "card_b": serialize_card(pair.card_b),
        "synergy_tags": list(pair.synergy_tags),
        "shared_tags": list(pair.shared_tags),
        "reasons": [
            {
                "points": reason.points,
                "tag": reason.tag,
                "text": reason.text,
            }
            for reason in pair.reasons
        ],
    }


def serialize_card(card: CardRecord) -> dict[str, Any]:
    return {
        "card_id": card.card_id,
        "name_th": card.name_th,
        "series_code": card.series_code,
        "clan": card.clan,
        "nation": card.nation,
        "grade": card.grade,
        "power": card.power,
        "shield": card.shield,
        "trigger": card.trigger,
        "type_1": card.type_1,
    }


def write_report(report: dict[str, Any], output_path: Path) -> None:
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Discover advisory clan combo pairs from Vanguard TH cards.")
    parser.add_argument("--db", type=Path, default=DEFAULT_DB)
    parser.add_argument("--sets", nargs="+")
    parser.add_argument("--preset", choices=sorted(ERA_SET_SPECS))
    parser.add_argument("--list-presets", action="store_true")
    parser.add_argument("--group-mode", choices=("auto", "clan", "nation"), default="auto")
    parser.add_argument("--top-per-clan", type=int, default=20)
    parser.add_argument("--min-score", type=int, default=DEFAULT_MIN_SCORE)
    parser.add_argument("--output", type=Path)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    if args.list_presets:
        print(json.dumps(ERA_SET_SPECS, ensure_ascii=False, indent=2))
        return 0
    if args.preset and args.sets:
        raise ValueError("--preset and --sets cannot be used together.")
    set_specs = ERA_SET_SPECS[args.preset] if args.preset else (args.sets or list(DEFAULT_SET_SPECS))
    set_codes = expand_set_specs(set_specs)
    cards = load_cards(args.db, set_codes)
    report = build_clan_combo_report(
        cards,
        set_codes,
        top_per_clan=args.top_per_clan,
        min_score=args.min_score,
        group_mode=args.group_mode,
    )
    output_path = args.output or default_output_for_preset(args.preset)
    write_report(report, output_path)
    print(
        "Clan combo discovery wrote "
        + str(output_path)
        + f" | cards={report['card_count']} clans={report['clan_count']}"
    )
    missing = report["scope"]["missing_set_codes"]
    if missing:
        print("Missing set codes: " + ", ".join(missing))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
