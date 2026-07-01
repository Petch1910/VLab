"""Build third-slice fixture/format readiness evidence for M43-02."""

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
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M43_01_SELECTION = OUTPUT_DIR / "m43_01_third_target_slice_selection.json"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"

ERA_SCOPES = {
    "link_joker_legion_mate": {
        "trial_decks": [f"TD{i:02d}" for i in range(7, 18)],
        "booster_packs": [f"BT{i:02d}" for i in range(10, 18)],
        "extra_boosters": [f"EB{i:02d}" for i in range(6, 13)],
    }
}


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _series_scope(era_preset: str) -> list[str]:
    scope = ERA_SCOPES.get(era_preset, {})
    return list(scope.get("trial_decks", [])) + list(scope.get("booster_packs", [])) + list(scope.get("extra_boosters", []))


def _load_cards(group: str, group_field: str, series_scope: Sequence[str]) -> list[dict[str, Any]]:
    if group_field != "clan":
        return []
    if not series_scope:
        return []
    placeholders = ",".join("?" for _ in series_scope)
    query = (
        "select card_id, name_th, series_code, clan, grade, trigger, deck_limit, type_1 "
        "from cards where clan = ? and series_code in "
        f"({placeholders})"
    )
    with closing(sqlite3.connect(CARDS_SQLITE)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, [group, *series_scope]).fetchall()
    return [dict(row) for row in rows]


def build_third_slice_fixture_readiness(selection_report: dict[str, Any] | None = None) -> dict[str, Any]:
    selection_report = selection_report or load_json(M43_01_SELECTION)
    selected = selection_report.get("selected_target", {})
    era_preset = selected.get("era_preset", "")
    series_scope = _series_scope(era_preset)
    cards = _load_cards(selected.get("group", ""), selected.get("group_field", ""), series_scope)
    grade_counts = Counter(str(card.get("grade")) for card in cards)
    trigger_counts = Counter(card.get("trigger") or "" for card in cards)
    series_counts = Counter(card.get("series_code") or "" for card in cards)
    trigger_capacity = sum(int(card.get("deck_limit") or 4) for card in cards if card.get("trigger"))
    non_trigger_capacity = sum(int(card.get("deck_limit") or 4) for card in cards if not card.get("trigger"))
    has_grade_setup = all(grade_counts.get(str(grade), 0) > 0 for grade in range(4))
    has_classic_trigger_capacity = trigger_capacity >= 16 and all(trigger_counts.get(trigger, 0) > 0 for trigger in ["Critical", "Draw", "Heal", "Stand"])
    has_main_deck_capacity = trigger_capacity + non_trigger_capacity >= 50 and non_trigger_capacity >= 34
    source_backed = bool(cards) and bool(series_scope)
    fixture_expectations_met = source_backed and has_grade_setup and has_classic_trigger_capacity and has_main_deck_capacity
    new_format_or_mechanic_fixtures_required = era_preset != "classic_part1"

    return {
        "version": "M43-02",
        "description": "Third-slice fixture/format readiness",
        "source_inputs": {
            "third_target_slice_selection": str(M43_01_SELECTION.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "selected_target": selected,
        "format_scope": {
            "era_preset": era_preset,
            "series_scope": series_scope,
            "trial_decks": ERA_SCOPES.get(era_preset, {}).get("trial_decks", []),
            "booster_packs": ERA_SCOPES.get(era_preset, {}).get("booster_packs", []),
            "extra_boosters": ERA_SCOPES.get(era_preset, {}).get("extra_boosters", []),
            "new_format_or_mechanic_fixtures_required": new_format_or_mechanic_fixtures_required,
            "policy_reuse_decision": "requires_third_slice_fixture_scaffold"
            if new_format_or_mechanic_fixtures_required
            else "classic_core_policy_reusable",
        },
        "card_pool_summary": {
            "source_card_count": len(cards),
            "series_counts": dict(sorted(series_counts.items())),
            "grade_counts": {str(grade): grade_counts.get(str(grade), 0) for grade in range(4)},
            "trigger_counts": dict(sorted(trigger_counts.items())),
            "trigger_capacity": trigger_capacity,
            "non_trigger_capacity": non_trigger_capacity,
        },
        "readiness": {
            "source_backed": source_backed,
            "has_grade_setup": has_grade_setup,
            "has_classic_trigger_capacity": has_classic_trigger_capacity,
            "has_main_deck_capacity": has_main_deck_capacity,
            "all_fixture_expectations_met": fixture_expectations_met,
            "semantic_probe_ready": fixture_expectations_met,
            "runtime_or_bot_promotion_allowed": False,
        },
        "runtime_boundary": {
            "offline_fixture_readiness_only": True,
            "does_not_create_deck": True,
            "does_not_create_runtime_fixture": True,
            "does_not_mutate_runtime_pack": True,
            "does_not_publish_to_ui": True,
            "does_not_publish_to_bot": True,
            "GameState_mutation": False,
        },
        "next_target": {
            "milestone": "M43-03" if fixture_expectations_met else "M43-repair",
            "task": "Third-slice semantic/compatibility probe" if fixture_expectations_met else "Repair third-slice readiness blockers",
        },
        "summary": {
            "selected_group": selected.get("group", ""),
            "era_preset": era_preset,
            "source_card_count": len(cards),
            "all_fixture_expectations_met": fixture_expectations_met,
            "semantic_probe_ready": fixture_expectations_met,
            "ready_for_m43_03": fixture_expectations_met,
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
        "# M43-02 Third-Slice Fixture/Format Readiness",
        "",
        "## Selected Target",
        "",
        f"- Group: `{selected.get('group', '')}`",
        f"- Era preset: `{selected.get('era_preset', '')}`",
        f"- Rank: `{selected.get('rank', '')}`",
        "",
        "## Readiness",
        "",
        f"- Source backed: `{readiness['source_backed']}`",
        f"- Source card count: `{pool['source_card_count']}`",
        f"- Trigger capacity: `{pool['trigger_capacity']}`",
        f"- Non-trigger capacity: `{pool['non_trigger_capacity']}`",
        f"- Grade counts: `{pool['grade_counts']}`",
        f"- Trigger counts: `{pool['trigger_counts']}`",
        f"- All fixture expectations met: `{readiness['all_fixture_expectations_met']}`",
        f"- Semantic probe ready: `{readiness['semantic_probe_ready']}`",
        f"- Runtime/bot promotion allowed: `{readiness['runtime_or_bot_promotion_allowed']}`",
        "",
        "## Format Scope",
        "",
        f"- Series scope: `{report['format_scope']['series_scope']}`",
        f"- Policy reuse decision: `{report['format_scope']['policy_reuse_decision']}`",
        "",
        "## Policy",
        "",
        "- Offline fixture readiness only.",
        "- Does not create a deck or runtime fixture.",
        "- Does not mutate runtime pack, UI, bot, or GameState.",
        "",
        "## Next",
        "",
        f"`{report['next_target']['milestone']}`: {report['next_target']['task']}.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M43-02 third-slice fixture readiness report.")
    parser.add_argument("--selection-report", type=Path, default=M43_01_SELECTION)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    selection = load_json(args.selection_report)
    report = build_third_slice_fixture_readiness(selection)
    json_path = args.output_dir / "m43_02_third_slice_fixture_readiness.json"
    md_path = args.output_dir / "m43_02_third_slice_fixture_readiness.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M43-02 third-slice fixture readiness wrote {json_path}")
    print(f"M43-02 third-slice fixture readiness summary wrote {md_path}")
    print(
        "expectations_met={met} source_cards={cards} next={next_ready}".format(
            met=report["summary"]["all_fixture_expectations_met"],
            cards=report["summary"]["source_card_count"],
            next_ready=report["summary"]["ready_for_m43_03"],
        )
    )
    return 0 if report["summary"]["ready_for_m43_03"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
