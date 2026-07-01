"""Build M41-repair trigger profile candidates for the accepted second slice."""

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
M41_03_VALIDATION = OUTPUT_DIR / "m41_03_second_slice_repaired_recipe_validation_rerun.json"
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
        "select card_id, name_th, clan, grade, trigger, deck_limit, series_code, type_1 "
        f"from cards where card_id in ({placeholders})"
    )
    with closing(sqlite3.connect(sqlite_path)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, ids).fetchall()
    return {row["card_id"]: dict(row) for row in rows}


def _quantity_map(rows: list[dict[str, Any]]) -> dict[str, int]:
    return {row["card_id"]: int(row.get("quantity", 0)) for row in rows}


def _grade_zero_non_trigger_removal_pool(
    accepted: dict[str, Any],
    card_rows: dict[str, dict[str, Any]],
) -> list[dict[str, Any]]:
    pool: list[dict[str, Any]] = []
    for row in accepted["accepted_repair"].get("repaired_quantities", []):
        card = card_rows.get(row["card_id"], {})
        if str(card.get("grade")) != "0":
            continue
        if card.get("trigger"):
            continue
        pool.append(
            {
                "card_id": row["card_id"],
                "name_th": card.get("name_th", row.get("name_th", "")),
                "quantity": int(row.get("quantity", 0)),
                "grade": "0",
                "trigger": "",
                "series_code": card.get("series_code", row.get("series_code", "")),
                "source": "m41_02_accepted_repair_preview",
            }
        )
    return pool


def _take_removals(pool: list[dict[str, Any]], required_quantity: int) -> list[dict[str, Any]]:
    removals: list[dict[str, Any]] = []
    remaining = required_quantity
    for row in pool:
        if remaining <= 0:
            break
        quantity = min(int(row["quantity"]), remaining)
        if quantity <= 0:
            continue
        copy = dict(row)
        copy["quantity"] = quantity
        removals.append(copy)
        remaining -= quantity
    return removals


def _trigger_lookup(card_rows: dict[str, dict[str, Any]]) -> dict[str, dict[str, Any]]:
    return {card_id: row for card_id, row in card_rows.items() if row.get("trigger")}


def _addition_rows(
    quantities: dict[str, int],
    trigger_rows: dict[str, dict[str, Any]],
    plan: list[tuple[str, int]],
) -> tuple[list[dict[str, Any]], list[dict[str, Any]]]:
    additions: list[dict[str, Any]] = []
    issues: list[dict[str, Any]] = []
    for card_id, quantity in plan:
        card = trigger_rows.get(card_id)
        if card is None:
            issues.append({"code": "missing_trigger_candidate", "card_id": card_id})
            continue
        current = int(quantities.get(card_id, 0))
        deck_limit = int(card.get("deck_limit") or 4)
        final = current + quantity
        if final > deck_limit:
            issues.append(
                {
                    "code": "copy_limit_exceeded_by_repair",
                    "card_id": card_id,
                    "current": current,
                    "add": quantity,
                    "deck_limit": deck_limit,
                }
            )
        additions.append(
            {
                "card_id": card_id,
                "name_th": card.get("name_th", ""),
                "quantity": quantity,
                "grade": "0",
                "trigger": card.get("trigger") or "",
                "series_code": card.get("series_code", ""),
                "current_quantity": current,
                "final_quantity_if_chosen": final,
                "deck_limit": deck_limit,
                "source": "data/packs/vanguard_th/cards.sqlite:cards",
            }
        )
    return additions, issues


def _apply_delta(
    quantities: dict[str, int],
    removals: list[dict[str, Any]],
    additions: list[dict[str, Any]],
) -> dict[str, int]:
    result = dict(quantities)
    for row in removals:
        card_id = row["card_id"]
        after = result.get(card_id, 0) - int(row["quantity"])
        if after > 0:
            result[card_id] = after
        else:
            result.pop(card_id, None)
    for row in additions:
        result[row["card_id"]] = result.get(row["card_id"], 0) + int(row["quantity"])
    return result


def _summarize_counts(quantities: dict[str, int], card_rows: dict[str, dict[str, Any]]) -> dict[str, Any]:
    trigger_counts: Counter[str] = Counter()
    grade_counts: Counter[str] = Counter()
    for card_id, quantity in quantities.items():
        card = card_rows.get(card_id, {})
        trigger = card.get("trigger") or ""
        if trigger:
            trigger_counts[trigger] += quantity
        grade_counts[str(card.get("grade"))] += quantity
    return {
        "main_deck_count": sum(quantities.values()),
        "trigger_count": sum(trigger_counts.values()),
        "trigger_counts": dict(sorted(trigger_counts.items())),
        "grade_counts": {grade: grade_counts.get(grade, 0) for grade in sorted(CLASSIC_GRADE_TARGET)},
    }


def _candidate_package(
    package_id: str,
    profile_id: str,
    trigger_plan: list[tuple[str, int]],
    accepted: dict[str, Any],
    all_card_rows: dict[str, dict[str, Any]],
) -> dict[str, Any]:
    quantities = _quantity_map(accepted["accepted_repair"].get("repaired_quantities", []))
    removal_quantity = sum(quantity for _, quantity in trigger_plan)
    removal_pool = _grade_zero_non_trigger_removal_pool(accepted, all_card_rows)
    removals = _take_removals(removal_pool, removal_quantity)
    additions, addition_issues = _addition_rows(quantities, _trigger_lookup(all_card_rows), trigger_plan)
    repaired_quantities = _apply_delta(quantities, removals, additions)
    counts_after = _summarize_counts(repaired_quantities, all_card_rows)
    removal_complete = sum(row["quantity"] for row in removals) == removal_quantity
    complete = (
        removal_complete
        and not addition_issues
        and counts_after["main_deck_count"] == 50
        and counts_after["trigger_count"] == CLASSIC_TRIGGER_TOTAL
        and counts_after["grade_counts"] == CLASSIC_GRADE_TARGET
        and counts_after["trigger_counts"].get("Heal", 0) <= 4
    )
    return {
        "package_id": package_id,
        "profile_id": profile_id,
        "repair_type": "trigger_profile_substitution",
        "advisory_only": True,
        "resolves_issue_codes": ["trigger_count_mismatch"],
        "additions": additions,
        "removals": removals,
        "addition_issue_count": len(addition_issues),
        "addition_issues": addition_issues,
        "removal_complete": removal_complete,
        "counts_after": counts_after,
        "complete_candidate": complete,
        "runtime_promotion_allowed": False,
        "ready_for_human_review": complete,
    }


def build_second_slice_trigger_profile_repair_candidates(
    accepted_artifact: dict[str, Any] | None = None,
    validation_report: dict[str, Any] | None = None,
    card_rows: dict[str, dict[str, Any]] | None = None,
) -> dict[str, Any]:
    accepted_artifact = accepted_artifact or load_json(M41_02_ACCEPTED)
    validation_report = validation_report or load_json(M41_03_VALIDATION)
    accepted_ids = [row["card_id"] for row in accepted_artifact["accepted_repair"].get("repaired_quantities", [])]
    trigger_candidate_ids = [
        "BT01-056TH",
        "BT01-058TH",
        "BT01-027TH",
        "BT02-067TH",
        "BT02-068TH",
        "BT01-057TH",
        "EB05-032TH",
    ]
    card_rows = card_rows or load_card_rows(accepted_ids + trigger_candidate_ids)
    trigger_blocked = "trigger_count_mismatch" in validation_report.get("summary", {}).get("issue_counts", {})
    package_plans = [
        (
            "m41_repair_pkg_001",
            "balanced_classic_trigger_restore",
            [("BT01-056TH", 4), ("BT01-058TH", 2), ("BT01-027TH", 4), ("BT02-067TH", 4)],
        ),
        (
            "m41_repair_pkg_002",
            "critical_pressure_trigger_restore",
            [("BT01-056TH", 4), ("BT02-068TH", 4), ("BT01-058TH", 2), ("BT01-027TH", 4)],
        ),
        (
            "m41_repair_pkg_003",
            "stand_pressure_trigger_restore",
            [("BT02-067TH", 4), ("EB05-032TH", 4), ("BT01-058TH", 2), ("BT01-027TH", 4)],
        ),
    ]
    packages = [
        _candidate_package(package_id, profile_id, plan, accepted_artifact, card_rows)
        for package_id, profile_id, plan in package_plans
    ]
    complete_count = sum(1 for package in packages if package["complete_candidate"])
    return {
        "version": "M41-repair",
        "description": "Second-slice trigger/profile repair candidates after M41-03 validation",
        "selected_target": accepted_artifact.get("selected_target", {}),
        "source_inputs": {
            "human_accepted_repair_artifact": str(M41_02_ACCEPTED.relative_to(ROOT)),
            "repaired_recipe_validation_rerun": str(M41_03_VALIDATION.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "scope": {
            "offline_repair_candidates": True,
            "changes_accepted_artifact": False,
            "records_human_acceptance": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
        },
        "repair_policy": {
            "source_backed_candidates": True,
            "trigger_count_target": CLASSIC_TRIGGER_TOTAL,
            "grade_count_must_remain_target": CLASSIC_GRADE_TARGET,
            "human_acceptance_required_after_candidate": True,
            "runtime_promotion_allowed": False,
        },
        "summary": {
            "trigger_blocker_present": trigger_blocked,
            "candidate_package_count": len(packages),
            "complete_candidate_count": complete_count,
            "ready_for_human_review_count": sum(1 for package in packages if package["ready_for_human_review"]),
            "runtime_promotion_allowed": False,
            "ready_for_repair_acceptance": trigger_blocked and complete_count > 0,
        },
        "repair_candidates": packages,
        "next_target": {
            "milestone": "M41-repair-accept",
            "task": "Second-slice trigger repair acceptance artifact",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M41 Repair Second-Slice Trigger/Profile Repair Candidates",
        "",
        "## Summary",
        "",
        f"- Trigger blocker present: `{summary['trigger_blocker_present']}`",
        f"- Candidate packages: `{summary['candidate_package_count']}`",
        f"- Complete candidates: `{summary['complete_candidate_count']}`",
        f"- Ready for human review: `{summary['ready_for_human_review_count']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for repair acceptance: `{summary['ready_for_repair_acceptance']}`",
        "",
        "## Candidates",
        "",
    ]
    for package in report["repair_candidates"]:
        lines.extend(
            [
                f"### `{package['package_id']}`",
                "",
                f"- Profile: `{package['profile_id']}`",
                f"- Add rows: `{len(package['additions'])}`",
                f"- Remove rows: `{len(package['removals'])}`",
                f"- Counts after: `{package['counts_after']}`",
                f"- Complete candidate: `{package['complete_candidate']}`",
                "",
            ]
        )
    lines.extend(
        [
            "## Policy",
            "",
            "- Candidates are advisory only.",
            "- Human acceptance is required before applying a trigger repair.",
            "- No runtime fixture, saved deck, UI deck list, bot playbook, or GameState mutation.",
            "",
            "## Next",
            "",
            "`M41-repair-accept`: Second-slice trigger repair acceptance artifact.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M41 trigger/profile repair candidates.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_second_slice_trigger_profile_repair_candidates()
    json_path = args.output_dir / "m41_repair_second_slice_trigger_profile_candidates.json"
    md_path = args.output_dir / "m41_repair_second_slice_trigger_profile_candidates.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M41 repair trigger/profile candidates wrote {json_path}")
    print(f"M41 repair trigger/profile candidates summary wrote {md_path}")
    print(
        "ready_for_repair_acceptance={ready} candidates={candidates} complete={complete}".format(
            ready=report["summary"]["ready_for_repair_acceptance"],
            candidates=report["summary"]["candidate_package_count"],
            complete=report["summary"]["complete_candidate_count"],
        )
    )
    return 0 if report["summary"]["ready_for_repair_acceptance"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
