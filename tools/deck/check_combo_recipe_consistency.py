"""Check M36-04 combo-line to recipe consistency.

This confirms whether combo-line cards are present in the corresponding recipe
draft and carries forward validator/review blockers. It remains offline.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
D3_COMBO_LINES = OUTPUT_DIR / "m35_d3_first_slice_combo_line_explainer.json"
M36_REVIEW_PACKET = OUTPUT_DIR / "m36_01_first_slice_review_packet.json"
M36_RECIPE_DRAFTS = OUTPUT_DIR / "m36_02_deck_recipe_draft_model.json"
M36_VALIDATION = OUTPUT_DIR / "m36_03_deck_recipe_validation_report.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _by_line(items: Sequence[dict[str, Any]], key: str = "source_line_id") -> dict[str, dict[str, Any]]:
    return {item.get(key, ""): item for item in items if item.get(key)}


def _manual_card_ids(review_packet: dict[str, Any]) -> set[str]:
    return {
        item["card_id"]
        for item in review_packet.get("manual_card_review_items", [])
    }


def _quantity_ids(recipe: dict[str, Any]) -> set[str]:
    return {row["card_id"] for row in recipe.get("quantities", []) if row.get("quantity", 0) > 0}


def _consistency_status(
    missing_cards: list[str],
    manual_overlap: list[str],
    validation_status: str,
) -> str:
    if missing_cards:
        return "inconsistent_missing_combo_cards"
    if manual_overlap:
        return "blocked_manual_review_card_dependency"
    if validation_status == "blocked_by_review":
        return "blocked_by_review"
    if validation_status == "invalid_draft":
        return "invalid_recipe"
    if validation_status == "validator_passed_pending_human_acceptance":
        return "consistent_pending_human_acceptance"
    if validation_status == "validator_passed":
        return "consistent_validator_passed"
    return "unclassified_review_required"


def build_consistency_report(
    combo_report: dict[str, Any] | None = None,
    review_packet: dict[str, Any] | None = None,
    recipe_report: dict[str, Any] | None = None,
    validation_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    combo_report = combo_report or load_json(D3_COMBO_LINES)
    review_packet = review_packet or load_json(M36_REVIEW_PACKET)
    recipe_report = recipe_report or load_json(M36_RECIPE_DRAFTS)
    validation_report = validation_report or load_json(M36_VALIDATION)
    recipes_by_line = _by_line(recipe_report.get("recipe_drafts", []))
    validations_by_line = _by_line(validation_report.get("recipe_validations", []))
    manual_ids = _manual_card_ids(review_packet)
    checks: list[dict[str, Any]] = []

    for line in combo_report.get("combo_lines", []):
        line_id = line["line_id"]
        recipe = recipes_by_line.get(line_id, {})
        validation = validations_by_line.get(line_id, {})
        recipe_ids = _quantity_ids(recipe)
        combo_ids = set(line.get("cards_involved", []))
        missing_cards = sorted(combo_ids - recipe_ids)
        manual_overlap = sorted(combo_ids & manual_ids)
        validation_status = validation.get("validation_status", "missing_validation")
        status = _consistency_status(missing_cards, manual_overlap, validation_status)
        checks.append(
            {
                "line_id": line_id,
                "recipe_id": recipe.get("recipe_id", ""),
                "source_skeleton_id": line.get("source_skeleton_id", ""),
                "anchor_card_id": line.get("anchor_card_id", ""),
                "combo_card_count": len(combo_ids),
                "recipe_card_id_count": len(recipe_ids),
                "combo_cards_present": not missing_cards,
                "missing_combo_card_ids": missing_cards,
                "manual_review_card_dependencies": manual_overlap,
                "recipe_validation_status": validation_status,
                "recipe_runtime_ready": bool(validation.get("runtime_ready", False)),
                "consistency_status": status,
                "promotion_allowed": status == "consistent_validator_passed",
            }
        )

    status_counts: dict[str, int] = {}
    for item in checks:
        status_counts[item["consistency_status"]] = status_counts.get(item["consistency_status"], 0) + 1

    return {
        "version": "M36-04",
        "description": "Combo-line to deck recipe consistency check",
        "selected_target": combo_report.get("selected_target", {}),
        "source_inputs": {
            "combo_line_explainer": str(D3_COMBO_LINES.relative_to(ROOT)),
            "first_slice_review_packet": str(M36_REVIEW_PACKET.relative_to(ROOT)),
            "deck_recipe_draft_model": str(M36_RECIPE_DRAFTS.relative_to(ROOT)),
            "deck_recipe_validation_report": str(M36_VALIDATION.relative_to(ROOT)),
        },
        "scope": {
            "offline_consistency_check": True,
            "runtime_deck": False,
            "bot_integration": False,
            "automatic_deck_injection": False,
        },
        "summary": {
            "combo_line_count": len(combo_report.get("combo_lines", [])),
            "consistency_check_count": len(checks),
            "combo_cards_present_count": sum(1 for item in checks if item["combo_cards_present"]),
            "missing_combo_card_check_count": sum(1 for item in checks if item["missing_combo_card_ids"]),
            "manual_review_dependency_check_count": sum(1 for item in checks if item["manual_review_card_dependencies"]),
            "promotion_allowed_count": sum(1 for item in checks if item["promotion_allowed"]),
            "runtime_ready_consistent_count": sum(
                1 for item in checks if item["promotion_allowed"] and item["recipe_runtime_ready"]
            ),
            "status_counts": dict(sorted(status_counts.items())),
            "ready_for_m36_05": bool(checks),
        },
        "consistency_checks": checks,
        "next_target": {
            "milestone": "M36-05",
            "task": "Second-slice readiness comparison",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M36-04 Combo-Line To Recipe Consistency",
        "",
        "## Summary",
        "",
        f"- Combo lines checked: `{summary['consistency_check_count']}`",
        f"- Combo cards present: `{summary['combo_cards_present_count']}`",
        f"- Missing combo-card checks: `{summary['missing_combo_card_check_count']}`",
        f"- Manual-review dependency checks: `{summary['manual_review_dependency_check_count']}`",
        f"- Promotion allowed: `{summary['promotion_allowed_count']}`",
        f"- Runtime-ready consistent: `{summary['runtime_ready_consistent_count']}`",
        f"- Ready for M36-05: `{summary['ready_for_m36_05']}`",
        "",
        "## Status Counts",
        "",
    ]
    for status, count in summary["status_counts"].items():
        lines.append(f"- `{status}`: `{count}`")
    lines.extend(["", "## Checks", ""])
    for item in report["consistency_checks"]:
        lines.append(
            f"- `{item['line_id']}` recipe=`{item['recipe_id']}` "
            f"present=`{item['combo_cards_present']}` status=`{item['consistency_status']}` "
            f"missing=`{','.join(item['missing_combo_card_ids'])}`"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Offline consistency check only.",
            "- Promotion remains blocked unless recipe validation passes and review blockers clear.",
            "- No runtime deck creation.",
            "- No bot integration.",
            "",
            "## Next",
            "",
            "`M36-05`: Second-slice readiness comparison.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Check M36-04 combo-line to recipe consistency.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_consistency_report()
    json_path = args.output_dir / "m36_04_combo_recipe_consistency_report.json"
    md_path = args.output_dir / "m36_04_combo_recipe_consistency_report.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M36-04 combo recipe consistency wrote {json_path}")
    print(f"M36-04 combo recipe consistency summary wrote {md_path}")
    print(
        "ready_for_m36_05={ready} checked={checked} promotion_allowed={allowed}".format(
            ready=report["summary"]["ready_for_m36_05"],
            checked=report["summary"]["consistency_check_count"],
            allowed=report["summary"]["promotion_allowed_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m36_05"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
