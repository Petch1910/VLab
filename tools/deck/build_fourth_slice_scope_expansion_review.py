"""Build M47-repair-expand-scope review for Royal Paladin source expansion."""

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
M47_REPAIR = OUTPUT_DIR / "m47_repair_fourth_slice_readiness_blockers.json"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"
REQUIRED_TRIGGERS = ["Critical", "Draw", "Heal", "Stand"]


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _load_rows(group: str, series_scope: Sequence[str]) -> list[dict[str, Any]]:
    if not series_scope:
        return []
    placeholders = ",".join("?" for _ in series_scope)
    query = (
        "select card_id, series_code, grade, trigger, deck_limit "
        "from cards where clan = ? and series_code in "
        f"({placeholders})"
    )
    with closing(sqlite3.connect(CARDS_SQLITE)) as connection:
        connection.row_factory = sqlite3.Row
        return [dict(row) for row in connection.execute(query, [group, *series_scope]).fetchall()]


def _load_heal_series(group: str) -> list[str]:
    query = "select distinct series_code from cards where clan = ? and trigger = 'Heal'"
    with closing(sqlite3.connect(CARDS_SQLITE)) as connection:
        rows = connection.execute(query, [group]).fetchall()
    return sorted(str(row[0]) for row in rows if row[0])


def _evaluate_scope(group: str, base_scope: Sequence[str], added_series: Sequence[str]) -> dict[str, Any]:
    series_scope = sorted(set(base_scope) | set(added_series))
    rows = _load_rows(group, series_scope)
    grade_counts = Counter(str(row.get("grade")) for row in rows)
    trigger_counts = Counter(row.get("trigger") or "" for row in rows)
    series_counts = Counter(row.get("series_code") or "" for row in rows)
    trigger_capacity = sum(int(row.get("deck_limit") or 4) for row in rows if row.get("trigger"))
    non_trigger_capacity = sum(int(row.get("deck_limit") or 4) for row in rows if not row.get("trigger"))
    trigger_type_gaps = [trigger for trigger in REQUIRED_TRIGGERS if trigger_counts.get(trigger, 0) <= 0]
    has_grade_setup = all(grade_counts.get(str(grade), 0) > 0 for grade in range(4))
    has_classic_trigger_capacity = trigger_capacity >= 16 and not trigger_type_gaps
    has_main_deck_capacity = trigger_capacity + non_trigger_capacity >= 50 and non_trigger_capacity >= 34
    expectations_met = bool(rows) and has_grade_setup and has_classic_trigger_capacity and has_main_deck_capacity
    return {
        "series_scope": series_scope,
        "added_series": sorted(set(added_series)),
        "source_card_count": len(rows),
        "series_counts": dict(sorted(series_counts.items())),
        "grade_counts": {str(grade): grade_counts.get(str(grade), 0) for grade in range(5)},
        "trigger_counts": dict(sorted(trigger_counts.items())),
        "trigger_capacity": trigger_capacity,
        "non_trigger_capacity": non_trigger_capacity,
        "trigger_type_gaps": trigger_type_gaps,
        "has_grade_setup": has_grade_setup,
        "has_classic_trigger_capacity": has_classic_trigger_capacity,
        "has_main_deck_capacity": has_main_deck_capacity,
        "all_fixture_expectations_met": expectations_met,
    }


def build_fourth_slice_scope_expansion_review(
    readiness_report: dict[str, Any] | None = None,
    repair_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    readiness_report = readiness_report or load_json(M47_02_READINESS)
    repair_report = repair_report or load_json(M47_REPAIR)
    selected = readiness_report.get("selected_target", {})
    group = selected.get("group", "")
    base_scope = list(readiness_report.get("format_scope", {}).get("series_scope", []))
    heal_series = _load_heal_series(group)
    g_era_heal_series = [series for series in heal_series if series.startswith("G-")]
    g_era = _evaluate_scope(group, base_scope, g_era_heal_series)
    all_heal = _evaluate_scope(group, base_scope, heal_series)
    no_expansion = _evaluate_scope(group, base_scope, [])
    recommended = "g_era_heal_expansion" if g_era["all_fixture_expectations_met"] else ""

    return {
        "version": "M47-repair-expand-scope",
        "description": "Fourth-slice same-group source expansion review",
        "source_inputs": {
            "fourth_slice_fixture_readiness": str(M47_02_READINESS.relative_to(ROOT)),
            "fourth_slice_readiness_repair": str(M47_REPAIR.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "selected_target": selected,
        "base_scope": {
            "series_scope": base_scope,
            "source_card_count": readiness_report.get("card_pool_summary", {}).get("source_card_count", 0),
            "trigger_type_gaps": readiness_report.get("card_pool_summary", {}).get("trigger_type_gaps", []),
        },
        "heal_series_candidates": {
            "all_same_group_heal_series": heal_series,
            "g_era_heal_series": g_era_heal_series,
        },
        "expansion_options": [
            {
                "id": "no_expansion",
                "recommended": False,
                "policy": "baseline_failed",
                **no_expansion,
            },
            {
                "id": "g_era_heal_expansion",
                "recommended": recommended == "g_era_heal_expansion",
                "policy": "recommended_same_era_family",
                **g_era,
            },
            {
                "id": "all_same_group_heal_series",
                "recommended": False,
                "policy": "not_recommended_cross_era_mixed",
                **all_heal,
            },
        ],
        "decision": {
            "recommended_expansion_id": recommended,
            "scope_expansion_applied": False,
            "card_data_mutated": False,
            "runtime_fixture_created": False,
            "saved_deck_enabled": False,
            "ui_deck_list_enabled": False,
            "bot_playbook_enabled": False,
        },
        "summary": {
            "selected_group": group,
            "base_source_card_count": readiness_report.get("card_pool_summary", {}).get("source_card_count", 0),
            "g_era_expanded_source_card_count": g_era["source_card_count"],
            "g_era_added_series_count": len(g_era_heal_series),
            "g_era_expectations_met": g_era["all_fixture_expectations_met"],
            "ready_for_m47_repair_apply_scope": bool(recommended),
        },
        "next_target": {
            "milestone": "M47-repair-apply-scope" if recommended else "M47-reselect",
            "task": "Apply reviewed source scope expansion" if recommended else "Reselect fourth target slice",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    lines = [
        "# M47-repair-expand-scope Same-Group Source Expansion Review",
        "",
        "## Summary",
        "",
        f"- Selected group: `{summary['selected_group']}`",
        f"- Base source card count: `{summary['base_source_card_count']}`",
        f"- G-era expanded source card count: `{summary['g_era_expanded_source_card_count']}`",
        f"- G-era added series count: `{summary['g_era_added_series_count']}`",
        f"- G-era expectations met: `{summary['g_era_expectations_met']}`",
        f"- Ready for apply-scope: `{summary['ready_for_m47_repair_apply_scope']}`",
        "",
        "## Expansion Options",
        "",
    ]
    for option in report["expansion_options"]:
        lines.append(
            "- `{id}` recommended=`{recommended}` policy=`{policy}` cards=`{cards}` gaps=`{gaps}`".format(
                id=option["id"],
                recommended=option["recommended"],
                policy=option["policy"],
                cards=option["source_card_count"],
                gaps=option["trigger_type_gaps"],
            )
        )
    lines.extend(
        [
            "",
            "## Decision",
            "",
            f"- Recommended expansion: `{decision['recommended_expansion_id']}`",
            f"- Scope expansion applied: `{decision['scope_expansion_applied']}`",
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
    parser = argparse.ArgumentParser(description="Build M47-repair-expand-scope same-group source expansion review.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_fourth_slice_scope_expansion_review()
    json_path = args.output_dir / "m47_repair_expand_scope_review.json"
    md_path = args.output_dir / "m47_repair_expand_scope_review.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M47-repair-expand-scope review wrote {json_path}")
    print(f"M47-repair-expand-scope review summary wrote {md_path}")
    print(
        "recommended={recommended} g_era_ready={ready} added_series={series} next={next_target}".format(
            recommended=report["decision"]["recommended_expansion_id"],
            ready=report["summary"]["g_era_expectations_met"],
            series=report["summary"]["g_era_added_series_count"],
            next_target=report["next_target"]["milestone"],
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
