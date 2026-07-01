"""Validate the M41-02 accepted second-slice repaired recipe."""

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
M41_02_ACCEPTED = OUTPUT_DIR / "m41_02_second_slice_human_accepted_repair_artifact.json"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"
CLASSIC_GRADE_TARGET = {"0": 17, "1": 14, "2": 11, "3": 8}
CLASSIC_TRIGGER_TOTAL = 16


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


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


def _validate_repaired_recipe(accepted: dict[str, Any], card_rows: dict[str, dict[str, Any]]) -> dict[str, Any]:
    repair = accepted["accepted_repair"]
    rows = repair.get("repaired_quantities", [])
    expected_clan = accepted.get("selected_target", {}).get("group", "")
    manual_review_card_ids = set(repair.get("manual_review_card_ids", []))
    issues: list[dict[str, Any]] = []
    trigger_counts: Counter[str] = Counter()
    grade_counts: Counter[str] = Counter()
    clan_counts: Counter[str] = Counter()
    missing_cards: list[str] = []
    total = 0
    present_card_ids: set[str] = set()

    if not accepted.get("summary", {}).get("human_acceptance_recorded"):
        issues.append(
            _issue(
                "human_acceptance_missing",
                "blocker",
                "M41-02 acceptance record is not complete.",
                {},
            )
        )

    for row in rows:
        card_id = row["card_id"]
        present_card_ids.add(card_id)
        quantity = int(row.get("quantity", 0))
        total += quantity
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
        grade_counts[str(card.get("grade"))] += quantity
        clan_counts[card.get("clan") or ""] += quantity

    if missing_cards:
        issues.append(
            _issue(
                "missing_cards",
                "blocker",
                "Repaired recipe contains card ids missing from SQLite.",
                {"card_ids": sorted(missing_cards)},
            )
        )
    if total != 50:
        issues.append(
            _issue(
                "main_deck_size_mismatch",
                "blocker",
                "Repaired recipe must have 50 main-deck cards.",
                {"expected": 50, "actual": total},
            )
        )
    trigger_total = sum(trigger_counts.values())
    if trigger_total != CLASSIC_TRIGGER_TOTAL:
        issues.append(
            _issue(
                "trigger_count_mismatch",
                "blocker",
                "Classic trigger count must be 16 before fixture promotion.",
                {"expected": CLASSIC_TRIGGER_TOTAL, "actual": trigger_total, "by_trigger": dict(trigger_counts)},
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
                "Repaired recipe contains cards outside selected clan.",
                {"expected_clan": expected_clan, "off_clan_counts": off_clan},
            )
        )
    grade_mismatches = {
        grade: {"expected": expected, "actual": grade_counts.get(grade, 0)}
        for grade, expected in CLASSIC_GRADE_TARGET.items()
        if grade_counts.get(grade, 0) != expected
    }
    if grade_mismatches:
        issues.append(
            _issue(
                "grade_profile_mismatch",
                "blocker",
                "Repaired recipe no longer matches the accepted grade target.",
                grade_mismatches,
            )
        )
    manual_overlap = sorted(manual_review_card_ids & present_card_ids)
    if manual_overlap:
        issues.append(
            _issue(
                "manual_review_card_overlap",
                "blocker",
                "Manual-review cards are still present after repair.",
                {"card_ids": manual_overlap},
            )
        )

    blocker_count = sum(1 for issue in issues if issue["severity"] == "blocker")
    validation_status = "validator_passed" if blocker_count == 0 else "invalid_repaired_recipe"
    return {
        "recipe_id": repair["recipe_id"],
        "source_candidate_edge": repair.get("source_candidate_edge", ""),
        "accepted_repair_package_id": repair.get("grade_profile_package_id", ""),
        "validation_status": validation_status,
        "blocking_issue_count": blocker_count,
        "issues": issues,
        "count_summary": {
            "main_deck_count": total,
            "trigger_count": trigger_total,
            "trigger_counts": dict(sorted(trigger_counts.items())),
            "grade_counts": {grade: grade_counts.get(grade, 0) for grade in sorted(CLASSIC_GRADE_TARGET)},
            "clan_counts": dict(sorted(clan_counts.items())),
            "manual_review_card_ids_present": manual_overlap,
        },
        "runtime_ready": validation_status == "validator_passed",
        "promotion_allowed": False,
    }


def build_second_slice_repaired_recipe_validation_report(
    accepted_artifact: dict[str, Any] | None = None,
    card_rows: dict[str, dict[str, Any]] | None = None,
) -> dict[str, Any]:
    accepted_artifact = accepted_artifact or load_json(M41_02_ACCEPTED)
    card_ids = [row["card_id"] for row in accepted_artifact["accepted_repair"].get("repaired_quantities", [])]
    card_rows = card_rows or load_card_rows(card_ids)
    validation = _validate_repaired_recipe(accepted_artifact, card_rows)
    issue_counts = Counter(issue["code"] for issue in validation["issues"])
    ready_for_m41_04 = validation["runtime_ready"]
    next_target = (
        {
            "milestone": "M41-04",
            "task": "Second-slice runtime fixture promotion gate",
        }
        if ready_for_m41_04
        else {
            "milestone": "M41-repair",
            "task": "Second-slice trigger/profile repair loop",
        }
    )
    return {
        "version": "M41-03",
        "description": "Second-slice repaired recipe validation rerun for Oracle Think Tank",
        "selected_target": accepted_artifact.get("selected_target", {}),
        "source_inputs": {
            "human_accepted_repair_artifact": str(M41_02_ACCEPTED.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "scope": {
            "offline_validator": True,
            "changes_accepted_artifact": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
        },
        "validation_policy": {
            "classic_main_deck_size": 50,
            "classic_trigger_count": CLASSIC_TRIGGER_TOTAL,
            "heal_trigger_max": 4,
            "classic_grade_target": CLASSIC_GRADE_TARGET,
            "manual_review_card_overlap_is_blocking": True,
            "runtime_promotion_requires_zero_blockers": True,
        },
        "summary": {
            "recipe_id": validation["recipe_id"],
            "validation_status": validation["validation_status"],
            "runtime_ready": validation["runtime_ready"],
            "promotion_allowed": validation["promotion_allowed"],
            "blocking_issue_count": validation["blocking_issue_count"],
            "main_deck_count": validation["count_summary"]["main_deck_count"],
            "trigger_count": validation["count_summary"]["trigger_count"],
            "grade_counts": validation["count_summary"]["grade_counts"],
            "manual_review_card_overlap_cleared": not validation["count_summary"]["manual_review_card_ids_present"],
            "issue_counts": dict(sorted(issue_counts.items())),
            "ready_for_m41_04": ready_for_m41_04,
            "ready_for_repair_loop": not ready_for_m41_04,
        },
        "recipe_validation": validation,
        "next_target": next_target,
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    validation = report["recipe_validation"]
    next_target = report["next_target"]
    lines = [
        "# M41-03 Second-Slice Repaired Recipe Validation Rerun",
        "",
        "## Summary",
        "",
        f"- Recipe: `{summary['recipe_id']}`",
        f"- Validation status: `{summary['validation_status']}`",
        f"- Runtime ready: `{summary['runtime_ready']}`",
        f"- Promotion allowed: `{summary['promotion_allowed']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        f"- Main deck count: `{summary['main_deck_count']}`",
        f"- Trigger count: `{summary['trigger_count']}`",
        f"- Grade counts: `{summary['grade_counts']}`",
        f"- Manual-review overlap cleared: `{summary['manual_review_card_overlap_cleared']}`",
        f"- Ready for M41-04: `{summary['ready_for_m41_04']}`",
        f"- Ready for repair loop: `{summary['ready_for_repair_loop']}`",
        "",
        "## Issues",
        "",
    ]
    if validation["issues"]:
        for issue in validation["issues"]:
            lines.append(f"- `{issue['severity']}` `{issue['code']}`: {issue['message']} `{issue['details']}`")
    else:
        lines.append("- No validation issues.")
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Offline validation only.",
            "- No runtime fixture, saved deck, UI deck list, bot playbook, or GameState mutation.",
            "- Failed validation routes to a repair loop before any promotion gate.",
            "",
            "## Next",
            "",
            f"`{next_target['milestone']}`: {next_target['task']}.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate the M41-02 accepted second-slice repaired recipe.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_second_slice_repaired_recipe_validation_report()
    json_path = args.output_dir / "m41_03_second_slice_repaired_recipe_validation_rerun.json"
    md_path = args.output_dir / "m41_03_second_slice_repaired_recipe_validation_rerun.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M41-03 second-slice repaired recipe validation wrote {json_path}")
    print(f"M41-03 second-slice repaired recipe validation summary wrote {md_path}")
    print(
        "ready_for_m41_04={ready} status={status} blockers={blockers} trigger_count={trigger_count}".format(
            ready=report["summary"]["ready_for_m41_04"],
            status=report["summary"]["validation_status"],
            blockers=report["summary"]["blocking_issue_count"],
            trigger_count=report["summary"]["trigger_count"],
        )
    )
    return 0 if report["summary"]["recipe_id"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
