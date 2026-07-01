"""Validate M40-02 second-slice recipe drafts for M40-03.

The validator is offline and read-only. It checks card existence, quantities,
copy limits, clan identity, trigger count, grade profile, and unresolved
manual-review overlap. It does not create saved decks, publish UI entries,
promote runtime fixtures, enable bot playbooks, or mutate GameState.
"""

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
RECIPE_DRAFTS = OUTPUT_DIR / "m40_02_second_slice_recipe_draft_model.json"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _all_card_ids(report: dict[str, Any]) -> list[str]:
    return sorted(
        {
            row["card_id"]
            for recipe in report.get("recipe_drafts", [])
            for row in recipe.get("quantities", [])
        }
    )


def load_card_rows(card_ids: Sequence[str], sqlite_path: Path = CARDS_SQLITE) -> dict[str, dict[str, Any]]:
    if not sqlite_path.exists():
        raise FileNotFoundError(f"Required SQLite pack not found: {sqlite_path}")
    ids = sorted(set(card_ids))
    if not ids:
        return {}
    placeholders = ",".join("?" for _ in ids)
    query = (
        "select card_id, name_th, clan, grade, trigger, deck_limit, type_1 "
        f"from cards where card_id in ({placeholders})"
    )
    with closing(sqlite3.connect(sqlite_path)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, ids).fetchall()
    return {row["card_id"]: dict(row) for row in rows}


def _issue(code: str, severity: str, message: str, details: dict[str, Any] | None = None) -> dict[str, Any]:
    return {
        "code": code,
        "severity": severity,
        "message": message,
        "details": details or {},
    }


def _validate_recipe(recipe: dict[str, Any], card_rows: dict[str, dict[str, Any]], expected_clan: str) -> dict[str, Any]:
    issues: list[dict[str, Any]] = []
    trigger_counts: Counter[str] = Counter()
    grade_counts: Counter[str] = Counter()
    clan_counts: Counter[str] = Counter()
    missing_cards: list[str] = []
    explicit_total = 0

    for row in recipe.get("quantities", []):
        card_id = row["card_id"]
        quantity = int(row.get("quantity", 0))
        explicit_total += quantity
        card = card_rows.get(card_id)
        if card is None:
            missing_cards.append(card_id)
            continue
        deck_limit = int(card.get("deck_limit") or 4)
        if quantity <= 0:
            issues.append(
                _issue(
                    "non_positive_quantity",
                    "blocker",
                    "Card quantity must be positive.",
                    {"card_id": card_id, "quantity": quantity},
                )
            )
        if quantity > deck_limit:
            issues.append(
                _issue(
                    "copy_limit_exceeded",
                    "blocker",
                    "Card quantity exceeds deck_limit.",
                    {"card_id": card_id, "quantity": quantity, "deck_limit": deck_limit},
                )
            )
        trigger = card.get("trigger") or ""
        if trigger:
            trigger_counts[trigger] += quantity
        grade = card.get("grade")
        grade_counts[str(grade) if grade is not None else "unknown"] += quantity
        clan_counts[card.get("clan") or ""] += quantity

    if missing_cards:
        issues.append(
            _issue(
                "missing_cards",
                "blocker",
                "Recipe contains card ids missing from SQLite.",
                {"card_ids": sorted(missing_cards)},
            )
        )

    slot_summary = recipe.get("slot_summary", {})
    target_total = int(slot_summary.get("main_deck_target", 50))
    if explicit_total != target_total:
        issues.append(
            _issue(
                "main_deck_size_mismatch",
                "blocker",
                "Explicit quantities do not equal main deck target.",
                {"expected": target_total, "actual": explicit_total},
            )
        )
    if int(slot_summary.get("total_unfilled_slots", 0)) > 0:
        issues.append(
            _issue(
                "unfilled_slots",
                "blocker",
                "Draft still has unfilled slots.",
                {"unfilled": slot_summary.get("total_unfilled_slots")},
            )
        )

    trigger_total = sum(trigger_counts.values())
    if trigger_total != 16:
        issues.append(
            _issue(
                "trigger_count_mismatch",
                "blocker",
                "Classic trigger count must be 16 before recipe can be promoted.",
                {"expected": 16, "actual": trigger_total, "by_trigger": dict(trigger_counts)},
            )
        )
    if trigger_counts.get("Heal", 0) > 4:
        issues.append(
            _issue(
                "heal_trigger_limit_exceeded",
                "blocker",
                "Heal trigger count exceeds classic guardrail.",
                {"heal_count": trigger_counts["Heal"]},
            )
        )

    off_clan = {clan: count for clan, count in clan_counts.items() if clan and clan != expected_clan}
    if off_clan:
        issues.append(
            _issue(
                "clan_mismatch",
                "blocker",
                "Recipe contains cards outside selected clan.",
                {"expected_clan": expected_clan, "off_clan_counts": off_clan},
            )
        )

    grade_targets = {"0": 17, "1": 14, "2": 11, "3": 8}
    grade_mismatches = {
        grade: {"expected": expected, "actual": grade_counts.get(grade, 0)}
        for grade, expected in grade_targets.items()
        if grade_counts.get(grade, 0) != expected
    }
    if grade_mismatches:
        issues.append(
            _issue(
                "grade_profile_review",
                "review",
                "Grade profile differs from advisory target.",
                grade_mismatches,
            )
        )

    manual_ids = recipe.get("validation_metadata", {}).get("manual_review_card_ids", [])
    if manual_ids:
        issues.append(
            _issue(
                "manual_review_card_overlap",
                "blocker",
                "Recipe contains cards that M40-01 marked as manual-review required.",
                {"card_ids": sorted(manual_ids)},
            )
        )

    if "human_recipe_selection" in recipe.get("review_blockers", []):
        issues.append(
            _issue(
                "human_recipe_selection_pending",
                "review",
                "Recipe is advisory until a human/team selects it for repair or promotion.",
                {},
            )
        )

    blocker_count = sum(1 for issue in issues if issue["severity"] == "blocker")
    if any(issue["code"] == "manual_review_card_overlap" for issue in issues):
        status = "blocked_by_manual_review"
    elif blocker_count:
        status = "invalid_draft"
    elif any(issue["code"] == "human_recipe_selection_pending" for issue in issues):
        status = "validator_passed_pending_human_selection"
    else:
        status = "validator_passed"

    return {
        "recipe_id": recipe["recipe_id"],
        "source_candidate_edge": recipe.get("source_candidate_edge", ""),
        "source_edge_rank": recipe.get("source_edge_rank"),
        "anchor_card_id": recipe.get("anchor_card_id", ""),
        "review_status": recipe.get("review_status", ""),
        "validation_status": status,
        "blocking_issue_count": blocker_count,
        "issues": issues,
        "count_summary": {
            "main_deck_target": target_total,
            "explicit_card_count": explicit_total,
            "trigger_count": trigger_total,
            "trigger_counts": dict(sorted(trigger_counts.items())),
            "grade_counts": dict(sorted(grade_counts.items())),
            "clan_counts": dict(sorted(clan_counts.items())),
        },
        "runtime_ready": status == "validator_passed",
    }


def build_second_slice_validation_report(
    recipe_report: dict[str, Any] | None = None,
    card_rows: dict[str, dict[str, Any]] | None = None,
) -> dict[str, Any]:
    recipe_report = recipe_report or load_json(RECIPE_DRAFTS)
    card_rows = card_rows or load_card_rows(_all_card_ids(recipe_report))
    expected_clan = recipe_report.get("selected_target", {}).get("group", "")
    validations = [
        _validate_recipe(recipe, card_rows, expected_clan)
        for recipe in recipe_report.get("recipe_drafts", [])
    ]
    status_counts = Counter(item["validation_status"] for item in validations)
    issue_counts = Counter(
        issue["code"]
        for item in validations
        for issue in item["issues"]
    )
    return {
        "version": "M40-03",
        "description": "Second-slice recipe validator for M40-02 advisory drafts",
        "selected_target": recipe_report.get("selected_target", {}),
        "source_inputs": {
            "second_slice_recipe_draft_model": str(RECIPE_DRAFTS.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "scope": {
            "offline_validator": True,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "runtime_deck": False,
            "bot_playbook": False,
            "automatic_deck_injection": False,
            "direct_GameState_mutation": False,
        },
        "validation_policy": {
            "classic_main_deck_size": 50,
            "classic_trigger_count": 16,
            "heal_trigger_max": 4,
            "copy_limit_from_sqlite_deck_limit": True,
            "grade_profile_mismatch_is_review_not_blocker": True,
            "manual_review_card_overlap_is_blocking": True,
            "human_selection_pending_is_review_not_blocker": True,
        },
        "summary": {
            "recipe_count": len(validations),
            "runtime_ready_recipe_count": sum(1 for item in validations if item["runtime_ready"]),
            "validator_passed_count": status_counts.get("validator_passed", 0),
            "validator_passed_pending_human_selection_count": status_counts.get(
                "validator_passed_pending_human_selection", 0
            ),
            "invalid_draft_count": status_counts.get("invalid_draft", 0),
            "blocked_by_manual_review_count": status_counts.get("blocked_by_manual_review", 0),
            "missing_card_recipe_count": sum(
                1 for item in validations if any(issue["code"] == "missing_cards" for issue in item["issues"])
            ),
            "copy_limit_violation_recipe_count": sum(
                1 for item in validations if any(issue["code"] == "copy_limit_exceeded" for issue in item["issues"])
            ),
            "slot_gap_recipe_count": sum(
                1 for item in validations if any(issue["code"] == "unfilled_slots" for issue in item["issues"])
            ),
            "trigger_count_mismatch_recipe_count": sum(
                1 for item in validations if any(issue["code"] == "trigger_count_mismatch" for issue in item["issues"])
            ),
            "manual_review_overlap_recipe_count": sum(
                1 for item in validations if any(issue["code"] == "manual_review_card_overlap" for issue in item["issues"])
            ),
            "grade_profile_review_recipe_count": sum(
                1 for item in validations if any(issue["code"] == "grade_profile_review" for issue in item["issues"])
            ),
            "issue_counts": dict(sorted(issue_counts.items())),
            "ready_for_m40_04": bool(validations),
        },
        "recipe_validations": validations,
        "next_target": {
            "milestone": "M40-04",
            "task": "Second-slice combo-to-recipe consistency",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M40-03 Second-Slice Recipe Validation Report",
        "",
        "## Summary",
        "",
        f"- Recipes validated: `{summary['recipe_count']}`",
        f"- Runtime-ready recipes: `{summary['runtime_ready_recipe_count']}`",
        f"- Validator passed: `{summary['validator_passed_count']}`",
        f"- Passed pending human selection: `{summary['validator_passed_pending_human_selection_count']}`",
        f"- Invalid drafts: `{summary['invalid_draft_count']}`",
        f"- Blocked by manual review: `{summary['blocked_by_manual_review_count']}`",
        f"- Slot-gap recipes: `{summary['slot_gap_recipe_count']}`",
        f"- Trigger-count mismatch recipes: `{summary['trigger_count_mismatch_recipe_count']}`",
        f"- Missing-card recipes: `{summary['missing_card_recipe_count']}`",
        f"- Copy-limit violation recipes: `{summary['copy_limit_violation_recipe_count']}`",
        f"- Grade-profile review recipes: `{summary['grade_profile_review_recipe_count']}`",
        f"- Ready for M40-04: `{summary['ready_for_m40_04']}`",
        "",
        "## Recipe Status",
        "",
    ]
    for item in report["recipe_validations"]:
        codes = ",".join(issue["code"] for issue in item["issues"] if issue["severity"] == "blocker")
        lines.append(
            f"- `{item['recipe_id']}` edge=`{item['source_candidate_edge']}` "
            f"status=`{item['validation_status']}` blockers=`{item['blocking_issue_count']}` "
            f"codes=`{codes}`"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Offline validator only.",
            "- Manual-review card overlap blocks runtime readiness.",
            "- Grade-profile mismatch is review evidence, not a blocker.",
            "- No saved-deck injection, UI publication, runtime deck creation, or bot integration.",
            "",
            "## Next",
            "",
            "`M40-04`: Second-slice combo-to-recipe consistency.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate M40-02 second-slice recipe drafts.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_second_slice_validation_report()
    json_path = args.output_dir / "m40_03_second_slice_recipe_validation_report.json"
    md_path = args.output_dir / "m40_03_second_slice_recipe_validation_report.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M40-03 second-slice recipe validation wrote {json_path}")
    print(f"M40-03 second-slice recipe validation summary wrote {md_path}")
    print(
        "ready_for_m40_04={ready} runtime_ready={runtime_ready} manual_blocked={manual_blocked}".format(
            ready=report["summary"]["ready_for_m40_04"],
            runtime_ready=report["summary"]["runtime_ready_recipe_count"],
            manual_blocked=report["summary"]["blocked_by_manual_review_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m40_04"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
