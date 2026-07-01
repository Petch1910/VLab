"""Build M47-repair analysis for fourth-slice readiness blockers."""

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
M47_02_READINESS = OUTPUT_DIR / "m47_02_fourth_slice_fixture_readiness.json"
M46_04_DECISION = OUTPUT_DIR / "m46_04_three_fixture_scale_decision.json"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _load_group_card_summary(group: str) -> dict[str, Any]:
    query = "select card_id, series_code, grade, trigger, deck_limit from cards where clan = ?"
    with closing(sqlite3.connect(CARDS_SQLITE)) as connection:
        connection.row_factory = sqlite3.Row
        rows = [dict(row) for row in connection.execute(query, [group]).fetchall()]
    trigger_counts = Counter(row.get("trigger") or "" for row in rows)
    series_counts = Counter(row.get("series_code") or "" for row in rows)
    grade_counts = Counter(str(row.get("grade")) for row in rows)
    heal_cards = [row["card_id"] for row in rows if row.get("trigger") == "Heal"]
    return {
        "total_card_count": len(rows),
        "series_counts": dict(sorted(series_counts.items())),
        "grade_counts": dict(sorted(grade_counts.items())),
        "trigger_counts": dict(sorted(trigger_counts.items())),
        "heal_card_ids": heal_cards,
        "has_heal_anywhere_in_group": bool(heal_cards),
    }


def _alternative_candidates(m46_decision: dict[str, Any], selected_group: str, limit: int = 4) -> list[dict[str, Any]]:
    candidates: list[dict[str, Any]] = []
    for candidate in m46_decision.get("candidate_queue", []):
        if candidate.get("group") == selected_group:
            continue
        candidates.append(candidate)
        if len(candidates) >= limit:
            break
    return candidates


def build_fourth_slice_readiness_repair(
    readiness_report: dict[str, Any] | None = None,
    m46_decision: dict[str, Any] | None = None,
) -> dict[str, Any]:
    readiness_report = readiness_report or load_json(M47_02_READINESS)
    m46_decision = m46_decision or load_json(M46_04_DECISION)
    selected = readiness_report.get("selected_target", {})
    selected_group = selected.get("group", "")
    group_summary = _load_group_card_summary(selected_group)
    trigger_gaps = list(readiness_report.get("card_pool_summary", {}).get("trigger_type_gaps", []))
    repair_reasons = list(readiness_report.get("readiness", {}).get("repair_reasons", []))
    can_repair_with_existing_source = not trigger_gaps or all(
        gap != "Heal" or group_summary["has_heal_anywhere_in_group"] for gap in trigger_gaps
    )
    alternatives = _alternative_candidates(m46_decision, selected_group)
    if can_repair_with_existing_source and trigger_gaps:
        recommended_action = "review_same_group_source_expansion"
    elif not can_repair_with_existing_source and alternatives:
        recommended_action = "select_next_candidate_from_m46_queue"
    else:
        recommended_action = "manual_data_repair_required"

    return {
        "version": "M47-repair",
        "description": "Fourth-slice readiness blocker repair analysis",
        "source_inputs": {
            "fourth_slice_fixture_readiness": str(M47_02_READINESS.relative_to(ROOT)),
            "three_fixture_scale_decision": str(M46_04_DECISION.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "selected_target": selected,
        "blocker_summary": {
            "repair_required": bool(readiness_report.get("summary", {}).get("repair_required")),
            "repair_reasons": repair_reasons,
            "trigger_type_gaps": trigger_gaps,
            "source_backed": bool(readiness_report.get("readiness", {}).get("source_backed")),
            "can_repair_with_existing_source": can_repair_with_existing_source,
        },
        "full_group_card_summary": group_summary,
        "repair_options": [
            {
                "id": "source_expansion_same_group",
                "available": group_summary["has_heal_anywhere_in_group"],
                "reason": "No Heal trigger exists for the selected group in the current runtime SQLite."
                if not group_summary["has_heal_anywhere_in_group"]
                else "A same-group Heal trigger exists outside the selected scope and could be reviewed.",
            },
            {
                "id": "relax_classic_trigger_profile",
                "available": False,
                "reason": "Rejected for fixture pipeline correctness; fixture recipes should not silently relax trigger-type requirements.",
            },
            {
                "id": "select_next_candidate",
                "available": bool(alternatives),
                "reason": "Use the next source-backed candidate from the M46 queue without mutating card data.",
            },
        ],
        "alternative_candidate_queue": alternatives,
        "decision": {
            "selected_group_repairable_now": can_repair_with_existing_source,
            "recommended_action": recommended_action,
            "selection_repair_performed": False,
            "card_data_mutated": False,
            "runtime_fixture_created": False,
            "saved_deck_enabled": False,
            "ui_deck_list_enabled": False,
            "bot_playbook_enabled": False,
        },
        "summary": {
            "selected_group": selected_group,
            "trigger_gap_count": len(trigger_gaps),
            "heal_exists_anywhere_in_group": group_summary["has_heal_anywhere_in_group"],
            "alternative_candidate_count": len(alternatives),
            "ready_for_reselection": recommended_action == "select_next_candidate_from_m46_queue",
            "ready_for_scope_expansion_review": recommended_action == "review_same_group_source_expansion",
        },
        "next_target": {
            "milestone": "M47-reselect"
            if recommended_action == "select_next_candidate_from_m46_queue"
            else "M47-repair-expand-scope"
            if recommended_action == "review_same_group_source_expansion"
            else "M47-data-repair",
            "task": "Reselect fourth target slice"
            if recommended_action == "select_next_candidate_from_m46_queue"
            else "Review same-group source expansion"
            if recommended_action == "review_same_group_source_expansion"
            else "Repair missing Royal Paladin source data",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    blockers = report["blocker_summary"]
    decision = report["decision"]
    lines = [
        "# M47-repair Fourth-Slice Readiness Blocker Repair",
        "",
        "## Summary",
        "",
        f"- Selected group: `{summary['selected_group']}`",
        f"- Trigger gap count: `{summary['trigger_gap_count']}`",
        f"- Heal exists anywhere in group: `{summary['heal_exists_anywhere_in_group']}`",
        f"- Alternative candidate count: `{summary['alternative_candidate_count']}`",
        f"- Ready for reselection: `{summary['ready_for_reselection']}`",
        f"- Ready for scope expansion review: `{summary['ready_for_scope_expansion_review']}`",
        "",
        "## Blockers",
        "",
        f"- Repair required: `{blockers['repair_required']}`",
        f"- Repair reasons: `{blockers['repair_reasons']}`",
        f"- Trigger type gaps: `{blockers['trigger_type_gaps']}`",
        f"- Can repair with existing source: `{blockers['can_repair_with_existing_source']}`",
        "",
        "## Repair Options",
        "",
    ]
    for option in report["repair_options"]:
        lines.append(f"- `{option['id']}` available=`{option['available']}` reason=`{option['reason']}`")
    lines.extend(["", "## Alternative Candidates", ""])
    for candidate in report["alternative_candidate_queue"]:
        lines.append(
            "- rank `{rank}` group `{group}` era `{era}` score `{score}`".format(
                rank=candidate.get("rank"),
                group=candidate.get("group", ""),
                era=candidate.get("best_era", ""),
                score=candidate.get("priority_score", 0),
            )
        )
    lines.extend(
        [
            "",
            "## Decision",
            "",
            f"- Recommended action: `{decision['recommended_action']}`",
            f"- Selection repair performed: `{decision['selection_repair_performed']}`",
            f"- Card data mutated: `{decision['card_data_mutated']}`",
            f"- Runtime fixture created: `{decision['runtime_fixture_created']}`",
            f"- Saved deck enabled: `{decision['saved_deck_enabled']}`",
            f"- UI deck list enabled: `{decision['ui_deck_list_enabled']}`",
            f"- Bot playbook enabled: `{decision['bot_playbook_enabled']}`",
            "",
            "## Next",
            "",
            f"`{report['next_target']['milestone']}`: {report['next_target']['task']}.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M47-repair fourth-slice readiness blocker repair analysis.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_fourth_slice_readiness_repair()
    json_path = args.output_dir / "m47_repair_fourth_slice_readiness_blockers.json"
    md_path = args.output_dir / "m47_repair_fourth_slice_readiness_blockers.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M47-repair fourth-slice readiness repair wrote {json_path}")
    print(f"M47-repair fourth-slice readiness repair summary wrote {md_path}")
    print(
        "ready_for_reselection={ready} heal_exists={heal} alternatives={alternatives} next={next_target}".format(
            ready=report["summary"]["ready_for_reselection"],
            heal=report["summary"]["heal_exists_anywhere_in_group"],
            alternatives=report["summary"]["alternative_candidate_count"],
            next_target=report["next_target"]["milestone"],
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
