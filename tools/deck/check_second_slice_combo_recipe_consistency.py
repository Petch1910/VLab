"""Check M40-04 second-slice combo-to-recipe consistency.

For the second slice, the combo unit is the M40-01 candidate edge used by each
M40-02 recipe draft. This checker verifies that the candidate source/target
cards are present in the recipe and carries forward M40-03 validator blockers.
It remains offline-only.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M40_RECIPE_DRAFTS = OUTPUT_DIR / "m40_02_second_slice_recipe_draft_model.json"
M40_VALIDATION = OUTPUT_DIR / "m40_03_second_slice_recipe_validation_report.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _by_recipe_id(items: Sequence[dict[str, Any]]) -> dict[str, dict[str, Any]]:
    return {item.get("recipe_id", ""): item for item in items if item.get("recipe_id")}


def _quantity_ids(recipe: dict[str, Any]) -> set[str]:
    return {row["card_id"] for row in recipe.get("quantities", []) if int(row.get("quantity", 0)) > 0}


def _manual_overlap_from_validation(validation: dict[str, Any]) -> list[str]:
    for issue in validation.get("issues", []):
        if issue.get("code") == "manual_review_card_overlap":
            return sorted(issue.get("details", {}).get("card_ids", []))
    return []


def _consistency_status(missing_pair_cards: list[str], validation_status: str) -> str:
    if missing_pair_cards:
        return "inconsistent_missing_pair_cards"
    if validation_status == "blocked_by_manual_review":
        return "blocked_by_manual_review"
    if validation_status == "invalid_draft":
        return "invalid_recipe"
    if validation_status == "validator_passed_pending_human_selection":
        return "consistent_pending_human_selection"
    if validation_status == "validator_passed":
        return "consistent_validator_passed"
    return "unclassified_review_required"


def build_second_slice_consistency_report(
    recipe_report: dict[str, Any] | None = None,
    validation_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    recipe_report = recipe_report or load_json(M40_RECIPE_DRAFTS)
    validation_report = validation_report or load_json(M40_VALIDATION)
    validations_by_recipe = _by_recipe_id(validation_report.get("recipe_validations", []))
    checks: list[dict[str, Any]] = []

    for recipe in recipe_report.get("recipe_drafts", []):
        recipe_id = recipe["recipe_id"]
        validation = validations_by_recipe.get(recipe_id, {})
        recipe_ids = _quantity_ids(recipe)
        pair = recipe.get("pair", {})
        pair_ids = {pair.get("source_card_id", ""), pair.get("target_card_id", "")} - {""}
        missing_pair_cards = sorted(pair_ids - recipe_ids)
        validation_status = validation.get("validation_status", "missing_validation")
        status = _consistency_status(missing_pair_cards, validation_status)
        recipe_manual_overlap = _manual_overlap_from_validation(validation)
        pair_manual_overlap = sorted(pair_ids & set(recipe_manual_overlap))
        checks.append(
            {
                "recipe_id": recipe_id,
                "source_candidate_edge": recipe.get("source_candidate_edge", ""),
                "source_edge_rank": recipe.get("source_edge_rank"),
                "source_card_id": pair.get("source_card_id", ""),
                "target_card_id": pair.get("target_card_id", ""),
                "pair_card_count": len(pair_ids),
                "recipe_card_id_count": len(recipe_ids),
                "pair_cards_present": not missing_pair_cards,
                "missing_pair_card_ids": missing_pair_cards,
                "pair_manual_review_dependencies": pair_manual_overlap,
                "recipe_manual_review_dependencies": recipe_manual_overlap,
                "recipe_validation_status": validation_status,
                "recipe_runtime_ready": bool(validation.get("runtime_ready", False)),
                "consistency_status": status,
                "promotion_allowed": status == "consistent_validator_passed" and bool(validation.get("runtime_ready")),
            }
        )

    status_counts: dict[str, int] = {}
    for item in checks:
        status_counts[item["consistency_status"]] = status_counts.get(item["consistency_status"], 0) + 1

    return {
        "version": "M40-04",
        "description": "Second-slice candidate edge to recipe consistency check",
        "selected_target": recipe_report.get("selected_target", {}),
        "source_inputs": {
            "second_slice_recipe_draft_model": str(M40_RECIPE_DRAFTS.relative_to(ROOT)),
            "second_slice_recipe_validation_report": str(M40_VALIDATION.relative_to(ROOT)),
        },
        "scope": {
            "offline_consistency_check": True,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "runtime_deck": False,
            "bot_playbook": False,
            "automatic_deck_injection": False,
            "direct_GameState_mutation": False,
        },
        "summary": {
            "recipe_count": len(recipe_report.get("recipe_drafts", [])),
            "consistency_check_count": len(checks),
            "pair_cards_present_count": sum(1 for item in checks if item["pair_cards_present"]),
            "missing_pair_card_check_count": sum(1 for item in checks if item["missing_pair_card_ids"]),
            "pair_manual_dependency_check_count": sum(1 for item in checks if item["pair_manual_review_dependencies"]),
            "recipe_manual_dependency_check_count": sum(1 for item in checks if item["recipe_manual_review_dependencies"]),
            "promotion_allowed_count": sum(1 for item in checks if item["promotion_allowed"]),
            "runtime_ready_consistent_count": sum(
                1 for item in checks if item["promotion_allowed"] and item["recipe_runtime_ready"]
            ),
            "status_counts": dict(sorted(status_counts.items())),
            "ready_for_m40_05": bool(checks),
        },
        "consistency_checks": checks,
        "next_target": {
            "milestone": "M40-05",
            "task": "Second-slice blocker repair candidates",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M40-04 Second-Slice Combo-To-Recipe Consistency",
        "",
        "## Summary",
        "",
        f"- Consistency checks: `{summary['consistency_check_count']}`",
        f"- Pair cards present: `{summary['pair_cards_present_count']}`",
        f"- Missing pair-card checks: `{summary['missing_pair_card_check_count']}`",
        f"- Pair manual-review dependencies: `{summary['pair_manual_dependency_check_count']}`",
        f"- Recipe manual-review dependencies: `{summary['recipe_manual_dependency_check_count']}`",
        f"- Promotion allowed: `{summary['promotion_allowed_count']}`",
        f"- Runtime-ready consistent: `{summary['runtime_ready_consistent_count']}`",
        f"- Ready for M40-05: `{summary['ready_for_m40_05']}`",
        "",
        "## Status Counts",
        "",
    ]
    for status, count in summary["status_counts"].items():
        lines.append(f"- `{status}`: `{count}`")
    lines.extend(["", "## Checks", ""])
    for item in report["consistency_checks"]:
        lines.append(
            f"- `{item['recipe_id']}` edge=`{item['source_candidate_edge']}` "
            f"present=`{item['pair_cards_present']}` status=`{item['consistency_status']}` "
            f"missing=`{','.join(item['missing_pair_card_ids'])}`"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Offline consistency check only.",
            "- Promotion remains blocked unless validation passes and review blockers clear.",
            "- No saved-deck injection, UI publication, runtime deck creation, or bot integration.",
            "",
            "## Next",
            "",
            "`M40-05`: Second-slice blocker repair candidates.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Check M40-04 second-slice combo-to-recipe consistency.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_second_slice_consistency_report()
    json_path = args.output_dir / "m40_04_second_slice_combo_recipe_consistency_report.json"
    md_path = args.output_dir / "m40_04_second_slice_combo_recipe_consistency_report.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M40-04 second-slice combo recipe consistency wrote {json_path}")
    print(f"M40-04 second-slice combo recipe consistency summary wrote {md_path}")
    print(
        "ready_for_m40_05={ready} checked={checked} promotion_allowed={allowed}".format(
            ready=report["summary"]["ready_for_m40_05"],
            checked=report["summary"]["consistency_check_count"],
            allowed=report["summary"]["promotion_allowed_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m40_05"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
