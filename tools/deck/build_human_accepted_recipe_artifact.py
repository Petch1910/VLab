"""Build the M38-03 human-accepted recipe artifact for recipe_003."""

from __future__ import annotations

import argparse
import json
import sqlite3
import sys
from collections import Counter
from contextlib import closing
from datetime import date
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
RECIPE_DRAFTS = OUTPUT_DIR / "m36_02_deck_recipe_draft_model.json"
M37_02_REPAIR = OUTPUT_DIR / "m37_02_trigger_package_repair_proposal.json"
M37_05_RERUN = OUTPUT_DIR / "m37_05_revised_recipe_validation_rerun.json"
M38_01_REVIEW = OUTPUT_DIR / "m38_01_accepted_seed_human_review_packet.json"
M38_02_GRADE = OUTPUT_DIR / "m38_02_grade_profile_repair_candidates.json"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"

DEFAULT_ACCEPTED_PACKAGE_ID = "m38_02_grade_pkg_001"
DEFAULT_ACCEPTANCE_TEXT = "\u0e07\u0e31\u0e49\u0e19\u0e08\u0e31\u0e14\u0e44\u0e1b"
CLASSIC_GRADE_TARGET = {"0": 17, "1": 14, "2": 11, "3": 8}


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _find_recipe(recipe_report: dict[str, Any], recipe_id: str) -> dict[str, Any]:
    for recipe in recipe_report.get("recipe_drafts", []):
        if recipe["recipe_id"] == recipe_id:
            return recipe
    raise ValueError(f"Recipe not found: {recipe_id}")


def _find_candidate(grade_report: dict[str, Any], package_id: str) -> dict[str, Any]:
    for candidate in grade_report.get("repair_candidates", []):
        if candidate["package_id"] == package_id:
            return candidate
    raise ValueError(f"Grade repair candidate not found: {package_id}")


def _add_delta(quantities: dict[str, int], rows: Sequence[dict[str, Any]]) -> None:
    for row in rows:
        card_id = row["card_id"]
        quantities[card_id] = quantities.get(card_id, 0) + int(row.get("quantity", 0))


def _remove_delta(quantities: dict[str, int], rows: Sequence[dict[str, Any]]) -> list[dict[str, Any]]:
    issues: list[dict[str, Any]] = []
    for row in rows:
        card_id = row["card_id"]
        quantity = int(row.get("quantity", 0))
        before = quantities.get(card_id, 0)
        if before < quantity:
            issues.append(
                {
                    "code": "negative_quantity_after_removal",
                    "severity": "blocker",
                    "card_id": card_id,
                    "before": before,
                    "remove": quantity,
                }
            )
            quantities[card_id] = 0
        else:
            quantities[card_id] = before - quantity
        if quantities.get(card_id, 0) <= 0:
            quantities.pop(card_id, None)
    return issues


def _load_cards(card_ids: Sequence[str]) -> dict[str, dict[str, Any]]:
    if not card_ids:
        return {}
    placeholders = ",".join("?" for _ in card_ids)
    query = (
        "select card_id, name_th, series_code, clan, grade, trigger, deck_limit, "
        "type_1, shield, power from cards where card_id in "
        f"({placeholders})"
    )
    with closing(sqlite3.connect(CARDS_SQLITE)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, list(card_ids)).fetchall()
    return {row["card_id"]: dict(row) for row in rows}


def _summarize_quantities(
    quantities: dict[str, int],
    cards: dict[str, dict[str, Any]],
    expected_clan: str,
) -> tuple[dict[str, Any], list[dict[str, Any]]]:
    issues: list[dict[str, Any]] = []
    trigger_counts: Counter[str] = Counter()
    grade_counts: Counter[str] = Counter()
    clan_counts: Counter[str] = Counter()
    missing_cards: list[str] = []
    total = 0

    for card_id, quantity in sorted(quantities.items()):
        total += quantity
        card = cards.get(card_id)
        if card is None:
            missing_cards.append(card_id)
            continue
        deck_limit = int(card.get("deck_limit") or 4)
        if quantity <= 0:
            issues.append(_issue("non_positive_quantity", "blocker", {"card_id": card_id, "quantity": quantity}))
        if quantity > deck_limit:
            issues.append(
                _issue(
                    "copy_limit_exceeded",
                    "blocker",
                    {"card_id": card_id, "quantity": quantity, "deck_limit": deck_limit},
                )
            )
        trigger = card.get("trigger") or ""
        if trigger:
            trigger_counts[trigger] += quantity
        grade_counts[str(card.get("grade"))] += quantity
        clan_counts[card.get("clan") or ""] += quantity

    if missing_cards:
        issues.append(_issue("missing_cards", "blocker", {"card_ids": missing_cards}))
    if total != 50:
        issues.append(_issue("main_deck_size_mismatch", "blocker", {"expected": 50, "actual": total}))
    if sum(trigger_counts.values()) != 16:
        issues.append(
            _issue(
                "trigger_count_mismatch",
                "blocker",
                {"expected": 16, "actual": sum(trigger_counts.values()), "by_trigger": dict(trigger_counts)},
            )
        )
    if trigger_counts.get("Heal", 0) > 4:
        issues.append(_issue("heal_trigger_limit_exceeded", "blocker", {"heal_count": trigger_counts["Heal"]}))

    off_clan = {clan: count for clan, count in clan_counts.items() if clan and clan != expected_clan}
    if off_clan:
        issues.append(_issue("clan_mismatch", "blocker", {"expected_clan": expected_clan, "off_clan_counts": off_clan}))

    grade_mismatches = {
        grade: {"expected": expected, "actual": grade_counts.get(grade, 0)}
        for grade, expected in CLASSIC_GRADE_TARGET.items()
        if grade_counts.get(grade, 0) != expected
    }
    if grade_mismatches:
        issues.append(_issue("grade_profile_mismatch", "blocker", grade_mismatches))

    return (
        {
            "main_deck_count": total,
            "trigger_count": sum(trigger_counts.values()),
            "trigger_counts": dict(sorted(trigger_counts.items())),
            "grade_counts": {grade: grade_counts.get(grade, 0) for grade in sorted(CLASSIC_GRADE_TARGET)},
            "clan_counts": dict(sorted(clan_counts.items())),
        },
        issues,
    )


def _issue(code: str, severity: str, details: dict[str, Any]) -> dict[str, Any]:
    return {
        "code": code,
        "severity": severity,
        "details": details,
    }


def _quantity_rows(quantities: dict[str, int], cards: dict[str, dict[str, Any]]) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    for card_id in sorted(quantities):
        card = cards.get(card_id, {})
        rows.append(
            {
                "card_id": card_id,
                "name_th": card.get("name_th", ""),
                "quantity": quantities[card_id],
                "grade": str(card.get("grade", "")),
                "trigger": card.get("trigger") or "",
                "series_code": card.get("series_code", ""),
                "clan": card.get("clan", ""),
                "source": "accepted_recipe_artifact_m38_03",
            }
        )
    return rows


def build_human_accepted_recipe_artifact(
    recipe_report: dict[str, Any] | None = None,
    trigger_repair: dict[str, Any] | None = None,
    rerun_report: dict[str, Any] | None = None,
    review_packet: dict[str, Any] | None = None,
    grade_report: dict[str, Any] | None = None,
    *,
    accepted_grade_package_id: str = DEFAULT_ACCEPTED_PACKAGE_ID,
    acceptance_text: str = DEFAULT_ACCEPTANCE_TEXT,
    accepted_at: str | None = None,
) -> dict[str, Any]:
    recipe_report = recipe_report or load_json(RECIPE_DRAFTS)
    trigger_repair = trigger_repair or load_json(M37_02_REPAIR)
    rerun_report = rerun_report or load_json(M37_05_RERUN)
    review_packet = review_packet or load_json(M38_01_REVIEW)
    grade_report = grade_report or load_json(M38_02_GRADE)

    recipe_id = rerun_report["summary"]["recipe_id"]
    recipe = _find_recipe(recipe_report, recipe_id)
    candidate = _find_candidate(grade_report, accepted_grade_package_id)

    quantities = {row["card_id"]: int(row.get("quantity", 0)) for row in recipe.get("quantities", [])}
    _add_delta(quantities, trigger_repair["recommended_repair"]["quantity_delta"])
    removal_issues = _remove_delta(quantities, candidate["removals"])
    _add_delta(quantities, candidate["additions"])

    cards = _load_cards(list(quantities))
    expected_clan = recipe_report.get("selected_target", {}).get("group", "")
    count_summary, validation_issues = _summarize_quantities(quantities, cards, expected_clan)
    validation_issues = removal_issues + validation_issues
    blocker_count = sum(1 for issue in validation_issues if issue["severity"] == "blocker")
    accepted = bool(acceptance_text.strip()) and candidate["completion_status"] == "complete_candidate"
    grade_profile_cleared = count_summary["grade_counts"] == CLASSIC_GRADE_TARGET
    ready_for_runtime_gate = accepted and blocker_count == 0 and grade_profile_cleared

    validation_status = (
        "accepted_review_artifact_ready_for_runtime_gate"
        if ready_for_runtime_gate
        else "accepted_review_artifact_blocked"
    )
    accepted_at = accepted_at or date.today().isoformat()

    return {
        "version": "M38-03",
        "description": "Human-accepted recipe artifact for recipe_003",
        "selected_target": recipe_report.get("selected_target", {}),
        "source_inputs": {
            "deck_recipe_draft_model": str(RECIPE_DRAFTS.relative_to(ROOT)),
            "trigger_package_repair_proposal": str(M37_02_REPAIR.relative_to(ROOT)),
            "revised_recipe_validation_rerun": str(M37_05_RERUN.relative_to(ROOT)),
            "accepted_seed_human_review_packet": str(M38_01_REVIEW.relative_to(ROOT)),
            "grade_profile_repair_candidates": str(M38_02_GRADE.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "scope": {
            "offline_human_accepted_artifact": True,
            "records_human_acceptance": True,
            "changes_recipe_draft_file": False,
            "creates_runtime_deck": False,
            "bot_integration": False,
            "automatic_playbook_promotion": False,
            "live_card_text_parsing": False,
        },
        "acceptance_record": {
            "decision": "accepted",
            "accepted_by": "user",
            "accepted_at": accepted_at,
            "acceptance_text": acceptance_text,
            "interpreted_decision": "accept_first_complete_grade_profile_candidate",
            "accepted_grade_package_id": candidate["package_id"],
            "accepted_grade_package_title": candidate["title"],
            "accepted_trigger_package_id": trigger_repair["summary"]["recommended_package_id"],
            "accepted_trigger_profile_id": trigger_repair["summary"]["recommended_profile_id"],
            "source_note": "Recorded from the user's go-ahead message for M38-03.",
        },
        "accepted_recipe": {
            "recipe_id": recipe_id,
            "source_recipe_id": recipe["recipe_id"],
            "source_line_id": recipe.get("source_line_id", ""),
            "anchor_card_id": recipe.get("anchor_card_id", ""),
            "anchor_name_th": recipe.get("anchor_name_th", ""),
            "quantities": _quantity_rows(quantities, cards),
            "count_summary": count_summary,
            "validation_status": validation_status,
            "validation_issues": validation_issues,
            "human_acceptance_cleared": accepted,
            "grade_profile_review_cleared": grade_profile_cleared,
            "runtime_promotion_allowed": False,
            "ready_for_m38_04_runtime_gate": ready_for_runtime_gate,
        },
        "summary": {
            "recipe_id": recipe_id,
            "accepted_grade_package_id": candidate["package_id"],
            "accepted_trigger_package_id": trigger_repair["summary"]["recommended_package_id"],
            "human_acceptance_cleared": accepted,
            "grade_profile_review_cleared": grade_profile_cleared,
            "blocking_issue_count": blocker_count,
            "main_deck_count": count_summary["main_deck_count"],
            "trigger_count": count_summary["trigger_count"],
            "grade_counts": count_summary["grade_counts"],
            "runtime_promotion_allowed": False,
            "ready_for_m38_04": ready_for_runtime_gate,
        },
        "next_target": {
            "milestone": "M38-04",
            "task": "Runtime fixture promotion gate",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    accepted = report["accepted_recipe"]
    record = report["acceptance_record"]
    lines = [
        "# M38-03 Human-Accepted Recipe Artifact",
        "",
        "## Summary",
        "",
        f"- Recipe: `{summary['recipe_id']}`",
        f"- Accepted grade package: `{summary['accepted_grade_package_id']}`",
        f"- Accepted trigger package: `{summary['accepted_trigger_package_id']}`",
        f"- Human acceptance cleared: `{summary['human_acceptance_cleared']}`",
        f"- Grade profile review cleared: `{summary['grade_profile_review_cleared']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        f"- Main deck count: `{summary['main_deck_count']}`",
        f"- Trigger count: `{summary['trigger_count']}`",
        f"- Grade counts: `{summary['grade_counts']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M38-04: `{summary['ready_for_m38_04']}`",
        "",
        "## Acceptance Record",
        "",
        f"- Decision: `{record['decision']}`",
        f"- Accepted by: `{record['accepted_by']}`",
        f"- Accepted at: `{record['accepted_at']}`",
        f"- Acceptance text: `{record['acceptance_text']}`",
        f"- Interpreted decision: `{record['interpreted_decision']}`",
        "",
        "## Accepted Recipe Checks",
        "",
        f"- Validation status: `{accepted['validation_status']}`",
        f"- Validation issues: `{len(accepted['validation_issues'])}`",
        "",
        "## Policy",
        "",
        "- This artifact records human acceptance for review purposes.",
        "- It does not mutate the M36 source recipe draft.",
        "- It does not create a runtime deck.",
        "- Runtime promotion remains gated to M38-04.",
        "",
        "## Next",
        "",
        "`M38-04`: Runtime fixture promotion gate.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M38-03 human-accepted recipe artifact.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    parser.add_argument("--accepted-grade-package-id", default=DEFAULT_ACCEPTED_PACKAGE_ID)
    parser.add_argument("--acceptance-text", default=DEFAULT_ACCEPTANCE_TEXT)
    parser.add_argument("--accepted-at", default=None)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_human_accepted_recipe_artifact(
        accepted_grade_package_id=args.accepted_grade_package_id,
        acceptance_text=args.acceptance_text,
        accepted_at=args.accepted_at,
    )
    json_path = args.output_dir / "m38_03_human_accepted_recipe_artifact.json"
    md_path = args.output_dir / "m38_03_human_accepted_recipe_artifact.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M38-03 human-accepted recipe artifact wrote {json_path}")
    print(f"M38-03 human-accepted recipe artifact summary wrote {md_path}")
    print(
        "ready_for_m38_04={ready} blockers={blockers} grade={grade}".format(
            ready=report["summary"]["ready_for_m38_04"],
            blockers=report["summary"]["blocking_issue_count"],
            grade=report["summary"]["grade_counts"],
        )
    )
    return 0 if report["summary"]["ready_for_m38_04"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
