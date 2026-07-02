"""Build seventh-slice fixture/format readiness evidence for M59-02."""

from __future__ import annotations

import argparse
import json
import sqlite3
import sys
from collections import Counter
from contextlib import closing
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.combo.discover_clan_combos import ERA_SET_SPECS, expand_set_specs  # noqa: E402


OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M59_01_SELECTION = OUTPUT_DIR / "m59_01_seventh_target_slice_selection.json"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"
JSON_OUTPUT = OUTPUT_DIR / "m59_02_seventh_slice_fixture_readiness.json"
MD_OUTPUT = OUTPUT_DIR / "m59_02_seventh_slice_fixture_readiness.md"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _series_scope(era_preset: str) -> list[str]:
    specs = ERA_SET_SPECS.get(era_preset)
    return expand_set_specs(specs) if specs else []


def _trigger_requirements(era_preset: str) -> list[tuple[str, ...]]:
    if era_preset.startswith("d_") or era_preset.startswith("dz_"):
        return [("Critical",), ("Draw",), ("Front",), ("Heal",), ("Over", "Over Trigger")]
    if era_preset.startswith("v_"):
        return [("Critical",), ("Draw",), ("Front", "Stand"), ("Heal",)]
    return [("Critical",), ("Draw",), ("Heal",), ("Stand",)]


def _trigger_type_gaps(era_preset: str, trigger_counts: Counter[str]) -> list[str]:
    gaps: list[str] = []
    for family in _trigger_requirements(era_preset):
        if not any(trigger_counts.get(trigger, 0) > 0 for trigger in family):
            gaps.append("/".join(family))
    return gaps


def _load_cards(group: str, group_field: str, series_scope: Sequence[str]) -> list[dict[str, Any]]:
    if group_field not in {"clan", "nation"}:
        return []
    if not group or not series_scope:
        return []

    placeholders = ",".join("?" for _ in series_scope)
    if group_field == "clan":
        group_clause = "clan = ?"
        group_args = [group]
    else:
        group_clause = "(nation = ? or nation like ?)"
        group_args = [group, f"{group}%"]

    query = (
        "select card_id, name_th, series_code, clan, nation, grade, trigger, deck_limit, type_1 "
        "from cards where "
        f"{group_clause} and series_code in ({placeholders})"
    )
    with closing(sqlite3.connect(CARDS_SQLITE)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, [*group_args, *series_scope]).fetchall()
    return [dict(row) for row in rows]


def build_seventh_slice_fixture_readiness(selection_report: dict[str, Any] | None = None) -> dict[str, Any]:
    selection_report = selection_report or load_json(M59_01_SELECTION)
    selected = selection_report.get("selected_target", {})
    selection_ready = bool(selection_report.get("summary", {}).get("ready_for_m59_02")) and bool(selected)
    era_preset = selected.get("era_preset", "")
    series_scope = _series_scope(era_preset)
    cards = _load_cards(selected.get("group", ""), selected.get("group_field", ""), series_scope)
    grade_counts = Counter(str(card.get("grade")) for card in cards)
    trigger_counts = Counter(card.get("trigger") or "" for card in cards)
    series_counts = Counter(card.get("series_code") or "" for card in cards)
    trigger_capacity = sum(int(card.get("deck_limit") or 4) for card in cards if card.get("trigger"))
    non_trigger_capacity = sum(int(card.get("deck_limit") or 4) for card in cards if not card.get("trigger"))
    has_grade_setup = all(grade_counts.get(str(grade), 0) > 0 for grade in range(4))
    g_zone_fixture_boundary_required = era_preset in {"g_series_first", "g_next_z"}
    has_required_g_unit_pool = (not g_zone_fixture_boundary_required) or grade_counts.get("4", 0) > 0
    trigger_type_gaps = _trigger_type_gaps(era_preset, trigger_counts)
    has_trigger_capacity = trigger_capacity >= 16 and not trigger_type_gaps
    has_main_deck_capacity = trigger_capacity + non_trigger_capacity >= 50 and non_trigger_capacity >= 34
    source_backed = bool(cards) and bool(series_scope)
    fixture_expectations_met = (
        selection_ready
        and source_backed
        and has_grade_setup
        and has_required_g_unit_pool
        and has_trigger_capacity
        and has_main_deck_capacity
    )
    repair_reasons: list[str] = []
    if not selection_ready:
        repair_reasons.append("selection_not_ready")
    if not series_scope:
        repair_reasons.append("era_scope_missing")
    if not source_backed:
        repair_reasons.append("source_pool_missing")
    if not has_grade_setup:
        repair_reasons.append("grade_0_to_3_gap")
    if g_zone_fixture_boundary_required and not has_required_g_unit_pool:
        repair_reasons.append("grade_4_g_unit_pool_gap")
    if trigger_type_gaps:
        repair_reasons.append("trigger_type_gap")
    if not has_main_deck_capacity:
        repair_reasons.append("main_deck_capacity_gap")

    return {
        "version": "M59-02",
        "description": "Seventh-slice fixture/format readiness",
        "source_inputs": {
            "seventh_target_slice_selection": str(M59_01_SELECTION.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "selected_target": selected,
        "format_scope": {
            "era_preset": era_preset,
            "set_specs": list(ERA_SET_SPECS.get(era_preset, ())),
            "series_scope": series_scope,
            "new_format_or_mechanic_fixtures_required": era_preset != "classic_part1",
            "g_zone_fixture_boundary_required": g_zone_fixture_boundary_required,
            "policy_reuse_decision": "requires_seventh_slice_fixture_scaffold"
            if fixture_expectations_met
            else "requires_seventh_slice_readiness_repair",
        },
        "card_pool_summary": {
            "source_card_count": len(cards),
            "series_counts": dict(sorted(series_counts.items())),
            "grade_counts": {str(grade): grade_counts.get(str(grade), 0) for grade in range(5)},
            "trigger_counts": dict(sorted(trigger_counts.items())),
            "trigger_capacity": trigger_capacity,
            "non_trigger_capacity": non_trigger_capacity,
            "trigger_type_gaps": trigger_type_gaps,
        },
        "readiness": {
            "selection_ready": selection_ready,
            "source_backed": source_backed,
            "has_grade_setup": has_grade_setup,
            "has_required_g_unit_pool": has_required_g_unit_pool,
            "has_trigger_capacity": has_trigger_capacity,
            "has_main_deck_capacity": has_main_deck_capacity,
            "all_fixture_expectations_met": fixture_expectations_met,
            "semantic_probe_ready": fixture_expectations_met,
            "runtime_or_bot_promotion_allowed": False,
            "repair_required": not fixture_expectations_met,
            "repair_reasons": repair_reasons,
        },
        "runtime_boundary": {
            "offline_fixture_readiness_only": True,
            "does_not_create_deck": True,
            "does_not_create_runtime_fixture": True,
            "does_not_mutate_runtime_pack": True,
            "does_not_publish_to_ui": True,
            "does_not_publish_to_bot": True,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "GameState_mutation": False,
        },
        "next_target": {
            "milestone": "M59-03" if fixture_expectations_met else "M59-repair",
            "task": "Seventh-slice semantic/compatibility probe"
            if fixture_expectations_met
            else "Repair seventh-slice readiness blockers",
        },
        "summary": {
            "selected_group": selected.get("group", ""),
            "era_preset": era_preset,
            "source_card_count": len(cards),
            "all_fixture_expectations_met": fixture_expectations_met,
            "semantic_probe_ready": fixture_expectations_met,
            "ready_for_m59_03": fixture_expectations_met,
            "repair_required": not fixture_expectations_met,
            "repair_reasons": repair_reasons,
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    selected = report["selected_target"]
    readiness = report["readiness"]
    pool = report["card_pool_summary"]
    lines = [
        "# M59-02 Seventh-Slice Fixture/Format Readiness",
        "",
        "## Selected Target",
        "",
        f"- Group: `{selected.get('group', '')}`",
        f"- Era preset: `{selected.get('era_preset', '')}`",
        f"- Rank: `{selected.get('rank', '')}`",
        "",
        "## Readiness",
        "",
        f"- Selection ready: `{readiness['selection_ready']}`",
        f"- Source backed: `{readiness['source_backed']}`",
        f"- Source card count: `{pool['source_card_count']}`",
        f"- Trigger capacity: `{pool['trigger_capacity']}`",
        f"- Non-trigger capacity: `{pool['non_trigger_capacity']}`",
        f"- Grade counts: `{pool['grade_counts']}`",
        f"- Trigger counts: `{pool['trigger_counts']}`",
        f"- Trigger type gaps: `{pool['trigger_type_gaps']}`",
        f"- Required G-unit pool present: `{readiness['has_required_g_unit_pool']}`",
        f"- All fixture expectations met: `{readiness['all_fixture_expectations_met']}`",
        f"- Semantic probe ready: `{readiness['semantic_probe_ready']}`",
        f"- Repair required: `{readiness['repair_required']}`",
        f"- Repair reasons: `{readiness['repair_reasons']}`",
        f"- Runtime/bot promotion allowed: `{readiness['runtime_or_bot_promotion_allowed']}`",
        "",
        "## Format Scope",
        "",
        f"- Set specs: `{report['format_scope']['set_specs']}`",
        f"- Series scope: `{report['format_scope']['series_scope']}`",
        f"- G Zone fixture boundary required: `{report['format_scope']['g_zone_fixture_boundary_required']}`",
        f"- Policy reuse decision: `{report['format_scope']['policy_reuse_decision']}`",
        "",
        "## Policy",
        "",
        "- Offline fixture readiness only.",
        "- Does not create a deck or runtime fixture.",
        "- Does not mutate runtime pack, UI, bot, or GameState.",
        "- Does not enable G Zone or Stride runtime.",
        "",
        "## Next",
        "",
        f"`{report['next_target']['milestone']}`: {report['next_target']['task']}.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M59-02 seventh-slice fixture readiness report.")
    parser.add_argument("--selection-report", type=Path, default=M59_01_SELECTION)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    selection = load_json(args.selection_report)
    report = build_seventh_slice_fixture_readiness(selection)
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M59-02 seventh-slice fixture readiness wrote {json_path}")
    print(f"M59-02 seventh-slice fixture readiness summary wrote {md_path}")
    print(
        "expectations_met={met} source_cards={cards} repair_required={repair} next={next_target}".format(
            met=report["summary"]["all_fixture_expectations_met"],
            cards=report["summary"]["source_card_count"],
            repair=report["summary"]["repair_required"],
            next_target=report["next_target"]["milestone"],
        )
    )
    return 0 if report["summary"]["ready_for_m59_03"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
