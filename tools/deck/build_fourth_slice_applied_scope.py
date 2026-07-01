"""Build the M47-repair-apply-scope applied offline source scope artifact."""

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
M47_EXPAND_SCOPE_REVIEW = OUTPUT_DIR / "m47_repair_expand_scope_review.json"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"
REQUIRED_CLASSIC_TRIGGERS = ["Critical", "Draw", "Heal", "Stand"]


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _recommended_option(review_report: dict[str, Any]) -> dict[str, Any] | None:
    recommended_id = review_report.get("decision", {}).get("recommended_expansion_id", "")
    if not recommended_id:
        return None
    for option in review_report.get("expansion_options", []):
        if option.get("id") == recommended_id:
            return dict(option)
    return None


def _load_cards(group: str, series_scope: Sequence[str]) -> list[dict[str, Any]]:
    if not group or not series_scope:
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


def _evaluate_scope(group: str, series_scope: Sequence[str]) -> dict[str, Any]:
    cards = _load_cards(group, series_scope)
    grade_counts = Counter(str(card.get("grade")) for card in cards)
    trigger_counts = Counter(card.get("trigger") or "" for card in cards)
    series_counts = Counter(card.get("series_code") or "" for card in cards)
    trigger_capacity = sum(int(card.get("deck_limit") or 4) for card in cards if card.get("trigger"))
    non_trigger_capacity = sum(int(card.get("deck_limit") or 4) for card in cards if not card.get("trigger"))
    trigger_type_gaps = [trigger for trigger in REQUIRED_CLASSIC_TRIGGERS if trigger_counts.get(trigger, 0) <= 0]
    has_grade_setup = all(grade_counts.get(str(grade), 0) > 0 for grade in range(4))
    has_classic_trigger_capacity = trigger_capacity >= 16 and not trigger_type_gaps
    has_main_deck_capacity = trigger_capacity + non_trigger_capacity >= 50 and non_trigger_capacity >= 34
    source_backed = bool(cards)
    all_fixture_expectations_met = (
        source_backed and has_grade_setup and has_classic_trigger_capacity and has_main_deck_capacity
    )
    return {
        "source_backed": source_backed,
        "source_card_count": len(cards),
        "series_counts": dict(sorted(series_counts.items())),
        "grade_counts": {str(grade): grade_counts.get(str(grade), 0) for grade in range(5)},
        "trigger_counts": dict(sorted(trigger_counts.items())),
        "trigger_capacity": trigger_capacity,
        "non_trigger_capacity": non_trigger_capacity,
        "trigger_type_gaps": trigger_type_gaps,
        "has_grade_setup": has_grade_setup,
        "has_classic_trigger_capacity": has_classic_trigger_capacity,
        "has_main_deck_capacity": has_main_deck_capacity,
        "all_fixture_expectations_met": all_fixture_expectations_met,
    }


def _relative(path: Path) -> str:
    return str(path.relative_to(ROOT))


def build_fourth_slice_applied_scope(review_report: dict[str, Any] | None = None) -> dict[str, Any]:
    review_report = review_report or load_json(M47_EXPAND_SCOPE_REVIEW)
    selected = dict(review_report.get("selected_target", {}))
    base_scope = list(review_report.get("base_scope", {}).get("series_scope", []))
    option = _recommended_option(review_report)
    blockers: list[str] = []
    if option is None:
        blockers.append("recommended_expansion_missing")
        effective_scope: list[str] = list(base_scope)
        added_series: list[str] = []
        expansion_id = ""
    else:
        effective_scope = list(option.get("series_scope", []))
        added_series = list(option.get("added_series", []))
        expansion_id = str(option.get("id", ""))

    group = str(selected.get("group", ""))
    applied_summary = _evaluate_scope(group, effective_scope)
    if not effective_scope:
        blockers.append("effective_scope_missing")
    if not applied_summary["all_fixture_expectations_met"]:
        blockers.append("applied_scope_not_fixture_ready")
    if option and option.get("source_card_count") != applied_summary["source_card_count"]:
        blockers.append("review_source_count_mismatch")
    if option and sorted(option.get("trigger_type_gaps", [])) != sorted(applied_summary["trigger_type_gaps"]):
        blockers.append("review_trigger_gap_mismatch")

    apply_ready = not blockers
    return {
        "version": "M47-repair-apply-scope",
        "description": "Apply reviewed fourth-slice source scope expansion to offline fixture pipeline",
        "source_inputs": {
            "scope_expansion_review": _relative(M47_EXPAND_SCOPE_REVIEW),
            "runtime_cards_sqlite": _relative(CARDS_SQLITE),
        },
        "selected_target": selected,
        "applied_scope": {
            "application_type": "offline_fixture_pipeline_scope_artifact",
            "expansion_id": expansion_id,
            "base_series_scope": base_scope,
            "added_series": sorted(added_series),
            "effective_series_scope": sorted(effective_scope),
            "base_series_count": len(base_scope),
            "added_series_count": len(set(added_series)),
            "effective_series_count": len(set(effective_scope)),
        },
        "card_pool_summary": {
            "source_card_count": applied_summary["source_card_count"],
            "series_counts": applied_summary["series_counts"],
            "grade_counts": applied_summary["grade_counts"],
            "trigger_counts": applied_summary["trigger_counts"],
            "trigger_capacity": applied_summary["trigger_capacity"],
            "non_trigger_capacity": applied_summary["non_trigger_capacity"],
            "trigger_type_gaps": applied_summary["trigger_type_gaps"],
        },
        "readiness": {
            "source_backed": applied_summary["source_backed"],
            "has_grade_setup": applied_summary["has_grade_setup"],
            "has_classic_trigger_capacity": applied_summary["has_classic_trigger_capacity"],
            "has_main_deck_capacity": applied_summary["has_main_deck_capacity"],
            "all_fixture_expectations_met": applied_summary["all_fixture_expectations_met"],
            "semantic_probe_ready": apply_ready,
            "ready_for_m47_03": apply_ready,
            "runtime_or_bot_promotion_allowed": False,
            "blockers": blockers,
        },
        "boundary": {
            "applies_scope_to_offline_fixture_pipeline": apply_ready,
            "card_data_mutated": False,
            "recipe_draft_created": False,
            "runtime_fixture_created": False,
            "runtime_pack_mutated": False,
            "saved_deck_enabled": False,
            "ui_deck_list_enabled": False,
            "bot_playbook_enabled": False,
            "GameState_mutation": False,
        },
        "next_target": {
            "milestone": "M47-03" if apply_ready else "M47-repair-expand-scope",
            "task": "Fourth-slice semantic/compatibility probe"
            if apply_ready
            else "Review fourth-slice source expansion again",
        },
        "summary": {
            "selected_group": group,
            "expansion_id": expansion_id,
            "source_card_count": applied_summary["source_card_count"],
            "added_series_count": len(set(added_series)),
            "effective_series_count": len(set(effective_scope)),
            "trigger_type_gaps": applied_summary["trigger_type_gaps"],
            "ready_for_m47_03": apply_ready,
            "blocker_count": len(blockers),
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    scope = report["applied_scope"]
    pool = report["card_pool_summary"]
    readiness = report["readiness"]
    boundary = report["boundary"]
    lines = [
        "# M47-repair-apply-scope Applied Source Scope",
        "",
        "## Summary",
        "",
        f"- Selected group: `{report['summary']['selected_group']}`",
        f"- Expansion id: `{scope['expansion_id']}`",
        f"- Added series count: `{scope['added_series_count']}`",
        f"- Effective series count: `{scope['effective_series_count']}`",
        f"- Source card count: `{pool['source_card_count']}`",
        f"- Trigger gaps: `{pool['trigger_type_gaps']}`",
        f"- Ready for M47-03: `{readiness['ready_for_m47_03']}`",
        f"- Blockers: `{readiness['blockers']}`",
        "",
        "## Applied Scope",
        "",
        f"- Base series: `{scope['base_series_scope']}`",
        f"- Added series: `{scope['added_series']}`",
        f"- Effective series: `{scope['effective_series_scope']}`",
        "",
        "## Card Pool",
        "",
        f"- Grade counts: `{pool['grade_counts']}`",
        f"- Trigger counts: `{pool['trigger_counts']}`",
        f"- Trigger capacity: `{pool['trigger_capacity']}`",
        f"- Non-trigger capacity: `{pool['non_trigger_capacity']}`",
        "",
        "## Boundary",
        "",
        f"- Applies scope to offline fixture pipeline: `{boundary['applies_scope_to_offline_fixture_pipeline']}`",
        f"- Card data mutated: `{boundary['card_data_mutated']}`",
        f"- Recipe draft created: `{boundary['recipe_draft_created']}`",
        f"- Runtime fixture created: `{boundary['runtime_fixture_created']}`",
        f"- Runtime pack mutated: `{boundary['runtime_pack_mutated']}`",
        f"- Saved deck enabled: `{boundary['saved_deck_enabled']}`",
        f"- UI deck list enabled: `{boundary['ui_deck_list_enabled']}`",
        f"- Bot playbook enabled: `{boundary['bot_playbook_enabled']}`",
        f"- GameState mutation: `{boundary['GameState_mutation']}`",
        "",
        "## Next",
        "",
        f"`{report['next_target']['milestone']}`: {report['next_target']['task']}.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M47-repair-apply-scope applied source scope artifact.")
    parser.add_argument("--review-report", type=Path, default=M47_EXPAND_SCOPE_REVIEW)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    review = load_json(args.review_report)
    report = build_fourth_slice_applied_scope(review)
    json_path = args.output_dir / "m47_repair_apply_scope.json"
    md_path = args.output_dir / "m47_repair_apply_scope.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M47-repair-apply-scope artifact wrote {json_path}")
    print(f"M47-repair-apply-scope summary wrote {md_path}")
    print(
        "ready_for_m47_03={ready} expansion={expansion} cards={cards} blockers={blockers} next={next_target}".format(
            ready=report["summary"]["ready_for_m47_03"],
            expansion=report["summary"]["expansion_id"],
            cards=report["summary"]["source_card_count"],
            blockers=report["summary"]["blocker_count"],
            next_target=report["next_target"]["milestone"],
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
