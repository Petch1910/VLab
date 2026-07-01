"""Rank clan/nation groups by deck feasibility, combo density, and mechanic complexity.

M34-03: Phase A1 of Hybrid Vertical-Slice Strategy.

Combines:
  - deck_possibility_summary.csv  (can a group build a legal 50-card deck?)
  - combo_matrix_group_candidates.csv  (how many combo pair candidates?)
  - combo_matrix_synergy_tags.csv  (what synergy types are strong?)
  - mechanic_presence_matrix.md  (which mechanics are complex?)

Output:
  - outputs/archetype_priority/archetype_priority_ranking.csv
  - outputs/archetype_priority/archetype_priority_ranking.json
"""

from __future__ import annotations

import csv
import json
import sys
from dataclasses import asdict, dataclass, field
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]

# --- paths ---
DECK_POSS_CSV = ROOT / "outputs" / "deck_possibility" / "deck_possibility_summary.csv"
COMBO_GROUP_CSV = ROOT / "outputs" / "combo_discovery" / "combo_matrix_group_candidates.csv"
SYNERGY_TAGS_CSV = ROOT / "outputs" / "combo_discovery" / "combo_matrix_synergy_tags.csv"
OUTPUT_DIR = ROOT / "outputs" / "archetype_priority"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


# --- mechanic complexity tiers (from mechanic_presence_matrix.md) ---
# Lower tier = simpler.  Mechanics are classified by implementation risk.

MECHANIC_TIER: dict[str, int] = {
    # Tier 1: Core OG — no special module needed
    "core": 1,
    # Tier 2: OG keywords — simple condition checks
    "limit_break": 2,
    "forerunner": 2,
    "lord": 2,
    "sentinel": 2,
    # Tier 3: OG complex — zone state changes
    "lock": 3,
    "delete": 3,
    # Tier 4: Legion — dual-vanguard model
    "legion": 4,
    "seek_mate": 4,
    # Tier 5: G — G zone, stride, heart
    "stride": 5,
    "heart": 5,
    "generation_break": 5,
    "g_guardian": 5,
    "ultimate_stride": 5,
    # Tier 6: V — markers/pseudo-cards
    "imaginary_gift": 6,
    "front_trigger": 6,
    # Tier 7: D — ride deck, orders, new zones
    "ride_deck": 7,
    "persona_ride": 7,
    "over_trigger": 7,
    "order": 7,
    "overdress": 7,
    "xoverdress": 7,
    # Tier 8: D/DZ advanced — energy, divine skill
    "energy": 8,
    "divine_skill": 8,
    "regalis_piece": 8,
}

# Map era presets to their primary mechanic tier
ERA_PRIMARY_TIER: dict[str, int] = {
    "classic_part1": 2,       # OG/Limit Break
    "link_joker_legion_mate": 4,  # Legion
    "g_series_first": 5,      # Stride
    "g_next_z": 5,            # Stride extended
    "v_reboot": 6,            # Imaginary Gift
    "v_shinemon_if": 6,       # Imaginary Gift extended
    "d_overdress": 7,         # Ride Deck/overDress
    "d_willdress": 7,         # will+Dress
    "dz_divinez": 8,          # Energy/Divine Skill
    "full_runtime": 8,        # all mechanics
}


@dataclass
class GroupFeasibility:
    group: str
    group_field: str  # "clan" or "nation"
    card_count: int = 0
    has_50_card_main: bool = False
    has_16_triggers: bool = False
    has_ride_grade_choice: bool = False
    trigger_capacity: int = 0
    non_trigger_capacity: int = 0
    issues: str = ""


@dataclass
class GroupCombo:
    group: str
    group_field: str
    total_candidates: int = 0
    era_candidates: dict[str, int] = field(default_factory=dict)
    best_era: str = ""
    best_era_candidates: int = 0


@dataclass
class ArchetypePriority:
    rank: int = 0
    group: str = ""
    group_field: str = ""
    # feasibility
    card_count: int = 0
    feasible: bool = False
    trigger_capacity: int = 0
    non_trigger_capacity: int = 0
    has_ride_grade_choice: bool = False
    # combo
    total_combo_candidates: int = 0
    best_era: str = ""
    best_era_candidates: int = 0
    # complexity
    mechanic_tier: int = 0
    mechanic_tier_label: str = ""
    # composite score (higher = better priority)
    priority_score: float = 0.0
    # reasons
    priority_reasons: list[str] = field(default_factory=list)


TIER_LABELS = {
    1: "Core OG",
    2: "OG Keywords",
    3: "OG Complex (Lock/Delete)",
    4: "Legion",
    5: "G/Stride",
    6: "V/Gift",
    7: "D/Ride Deck/overDress",
    8: "D/DZ Advanced",
}


def load_feasibility() -> dict[str, GroupFeasibility]:
    """Load deck possibility summary, using full_runtime preset for total capacity."""
    result: dict[str, GroupFeasibility] = {}
    with open(DECK_POSS_CSV, encoding="utf-8-sig") as f:
        reader = csv.DictReader(f)
        for row in reader:
            preset = row["preset"].strip()
            if preset != "full_runtime":
                continue
            group = row["group"].strip()
            gf = GroupFeasibility(
                group=group,
                group_field=row["group_field"].strip(),
                card_count=int(row["card_count"]),
                has_50_card_main=row["basic_50_card_main"].strip() == "True",
                has_16_triggers=row["main_with_16_triggers_34_non_triggers"].strip() == "True",
                has_ride_grade_choice=row["ride_deck_grade_0_1_2_3_choice"].strip() == "True",
                trigger_capacity=int(row["trigger_capacity"]),
                non_trigger_capacity=int(row["non_trigger_capacity"]),
                issues=row.get("issues", "").strip(),
            )
            result[group] = gf
    return result


def load_combo_candidates() -> dict[str, GroupCombo]:
    """Load combo candidate counts per group across all era presets."""
    result: dict[str, GroupCombo] = {}
    with open(COMBO_GROUP_CSV, encoding="utf-8-sig") as f:
        reader = csv.DictReader(f)
        eras = [
            c.strip() for c in reader.fieldnames or []
            if c.strip() not in ("group", "group_field")
        ]
        for row in reader:
            group = row["group"].strip()
            gf = row["group_field"].strip()
            era_cands: dict[str, int] = {}
            total = 0
            best_era = ""
            best_count = 0
            for era in eras:
                val = int(row[era].strip() or "0")
                era_cands[era] = val
                total += val
                if val > best_count:
                    best_count = val
                    best_era = era
            result[group] = GroupCombo(
                group=group,
                group_field=gf,
                total_candidates=total,
                era_candidates=era_cands,
                best_era=best_era,
                best_era_candidates=best_count,
            )
    return result


def determine_mechanic_tier(group: str, combo: GroupCombo | None) -> tuple[int, str]:
    """Determine the lowest mechanic tier where this group has cards/combos."""
    if combo is None:
        return 8, TIER_LABELS[8]

    # Find the simplest era that has significant candidates for this group
    for era in [
        "classic_part1",
        "link_joker_legion_mate",
        "g_series_first",
        "g_next_z",
        "v_reboot",
        "v_shinemon_if",
        "d_overdress",
        "d_willdress",
        "dz_divinez",
    ]:
        cands = combo.era_candidates.get(era, 0)
        if cands >= 10:  # need meaningful presence
            tier = ERA_PRIMARY_TIER[era]
            return tier, TIER_LABELS.get(tier, f"Tier {tier}")

    # Fallback: check if any era has candidates at all
    for era in [
        "classic_part1",
        "link_joker_legion_mate",
        "g_series_first",
        "g_next_z",
        "v_reboot",
        "v_shinemon_if",
        "d_overdress",
        "d_willdress",
    ]:
        cands = combo.era_candidates.get(era, 0)
        if cands > 0:
            tier = ERA_PRIMARY_TIER[era]
            return tier, TIER_LABELS.get(tier, f"Tier {tier}")

    return 8, TIER_LABELS[8]


def compute_priority_score(
    feasible: bool,
    card_count: int,
    total_candidates: int,
    mechanic_tier: int,
) -> float:
    """Higher score = higher priority for development focus.

    Formula weighs:
      - Feasibility (must-have gate)
      - Card count (more data = more coverage)
      - Combo candidates (more synergy = richer gameplay)
      - Mechanic simplicity (lower tier = easier to implement first)
    """
    if not feasible:
        return 0.0

    # Normalize components to 0-100 range
    card_score = min(card_count / 600.0, 1.0) * 25  # max ~600 cards
    combo_score = min(total_candidates / 5000.0, 1.0) * 35  # max ~5000 candidates
    simplicity_score = max(0, (9 - mechanic_tier)) / 8.0 * 40  # tier 1=max, tier 8=0

    return round(card_score + combo_score + simplicity_score, 2)


def build_rankings() -> list[ArchetypePriority]:
    feasibility = load_feasibility()
    combos = load_combo_candidates()

    all_groups = sorted(set(feasibility.keys()) | set(combos.keys()))

    rankings: list[ArchetypePriority] = []
    for group in all_groups:
        feas = feasibility.get(group)
        combo = combos.get(group)

        card_count = feas.card_count if feas else 0
        is_feasible = (
            feas is not None
            and feas.has_50_card_main
            and feas.has_16_triggers
            and feas.has_ride_grade_choice
        )
        trigger_cap = feas.trigger_capacity if feas else 0
        non_trigger_cap = feas.non_trigger_capacity if feas else 0
        has_ride = feas.has_ride_grade_choice if feas else False

        total_cands = combo.total_candidates if combo else 0
        best_era = combo.best_era if combo else ""
        best_era_cands = combo.best_era_candidates if combo else 0

        mech_tier, mech_label = determine_mechanic_tier(group, combo)

        score = compute_priority_score(is_feasible, card_count, total_cands, mech_tier)

        reasons: list[str] = []
        if not is_feasible:
            reasons.append("NOT_FEASIBLE")
            if feas and not feas.has_50_card_main:
                reasons.append("insufficient_cards")
            if feas and not feas.has_16_triggers:
                reasons.append("insufficient_triggers")
            if feas and not feas.has_ride_grade_choice:
                reasons.append("missing_ride_grades")
        else:
            if score >= 70:
                reasons.append("HIGH_PRIORITY")
            elif score >= 40:
                reasons.append("MEDIUM_PRIORITY")
            else:
                reasons.append("LOW_PRIORITY")

            if mech_tier <= 2:
                reasons.append("simple_mechanics")
            elif mech_tier <= 4:
                reasons.append("moderate_mechanics")
            else:
                reasons.append("complex_mechanics")

            if total_cands >= 3000:
                reasons.append("rich_combo_pool")
            elif total_cands >= 500:
                reasons.append("decent_combo_pool")

            if card_count >= 400:
                reasons.append("large_card_pool")

        rankings.append(ArchetypePriority(
            group=group,
            group_field=feas.group_field if feas else (combo.group_field if combo else "unknown"),
            card_count=card_count,
            feasible=is_feasible,
            trigger_capacity=trigger_cap,
            non_trigger_capacity=non_trigger_cap,
            has_ride_grade_choice=has_ride,
            total_combo_candidates=total_cands,
            best_era=best_era,
            best_era_candidates=best_era_cands,
            mechanic_tier=mech_tier,
            mechanic_tier_label=mech_label,
            priority_score=score,
            priority_reasons=reasons,
        ))

    # Sort by score descending, then group name
    rankings.sort(key=lambda r: (-r.priority_score, r.group))

    # Assign ranks
    for i, r in enumerate(rankings):
        r.rank = i + 1

    return rankings


def export_csv(rankings: list[ArchetypePriority], path: Path) -> None:
    headers = [
        "rank", "group", "group_field", "priority_score",
        "feasible", "card_count", "trigger_capacity", "non_trigger_capacity",
        "has_ride_grade_choice",
        "total_combo_candidates", "best_era", "best_era_candidates",
        "mechanic_tier", "mechanic_tier_label",
        "priority_reasons",
    ]
    with open(path, "w", encoding="utf-8", newline="") as f:
        writer = csv.writer(f)
        writer.writerow(headers)
        for r in rankings:
            writer.writerow([
                r.rank, r.group, r.group_field, r.priority_score,
                r.feasible, r.card_count, r.trigger_capacity, r.non_trigger_capacity,
                r.has_ride_grade_choice,
                r.total_combo_candidates, r.best_era, r.best_era_candidates,
                r.mechanic_tier, r.mechanic_tier_label,
                "; ".join(r.priority_reasons),
            ])


def export_json(rankings: list[ArchetypePriority], path: Path) -> None:
    data = {
        "version": "M34-03",
        "description": "Deck-feasible archetype priority ranking v2",
        "scoring": {
            "card_pool_weight": 25,
            "combo_density_weight": 35,
            "mechanic_simplicity_weight": 40,
            "note": "Higher score = higher priority for development focus",
        },
        "total_groups": len(rankings),
        "feasible_groups": sum(1 for r in rankings if r.feasible),
        "rankings": [asdict(r) for r in rankings],
    }
    with open(path, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)


def main() -> None:
    rankings = build_rankings()

    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    csv_path = OUTPUT_DIR / "archetype_priority_ranking.csv"
    json_path = OUTPUT_DIR / "archetype_priority_ranking.json"

    export_csv(rankings, csv_path)
    export_json(rankings, json_path)

    print(f"Exported {len(rankings)} groups to:")
    print(f"  CSV:  {csv_path}")
    print(f"  JSON: {json_path}")

    print()
    print("=== Top 10 Priority Archetypes ===")
    print(f"{'Rank':>4}  {'Score':>6}  {'Tier':>5}  {'Cards':>5}  {'Combos':>6}  {'Group'}")
    print("-" * 70)
    for r in rankings[:10]:
        print(
            f"{r.rank:>4}  {r.priority_score:>6.1f}  "
            f"T{r.mechanic_tier:<4}  {r.card_count:>5}  "
            f"{r.total_combo_candidates:>6}  {r.group}"
        )

    print()
    feasible_count = sum(1 for r in rankings if r.feasible)
    infeasible = [r for r in rankings if not r.feasible]
    print(f"Feasible: {feasible_count} / {len(rankings)}")
    if infeasible:
        print(f"Infeasible ({len(infeasible)}): "
              + ", ".join(r.group for r in infeasible))


if __name__ == "__main__":
    main()
