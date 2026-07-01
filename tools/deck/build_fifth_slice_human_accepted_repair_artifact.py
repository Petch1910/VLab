"""Build the M53-03 fifth-slice human-accepted repair artifact.

This tool consumes the explicit M53-02 selected recipe artifact. It records
acceptance only when a non-empty acceptance text is provided and does not
declare the repaired recipe valid.
"""

from __future__ import annotations

import argparse
import json
import sys
from datetime import date
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M53_02_SELECTION = OUTPUT_DIR / "m53_02_fifth_slice_human_selected_recipe_artifact.json"
M52_DRAFTS = OUTPUT_DIR / "m52_03_fifth_slice_recipe_draft_model.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _find_recipe(drafts: dict[str, Any], recipe_id: str) -> dict[str, Any]:
    for recipe in drafts.get("recipe_drafts", []):
        if recipe.get("recipe_id") == recipe_id:
            return recipe
    raise ValueError(f"Recipe not found: {recipe_id}")


def _quantity_map(recipe: dict[str, Any]) -> dict[str, int]:
    return {row["card_id"]: int(row.get("quantity", 0)) for row in recipe.get("quantities", [])}


def _require_acceptance_text(acceptance_text: str) -> str:
    normalized = acceptance_text.strip()
    if not normalized:
        raise ValueError("M53-03 requires non-empty acceptance_text.")
    return normalized


def _apply_grade_repair(
    quantities: dict[str, int],
    selection: dict[str, Any],
) -> tuple[dict[str, int], list[dict[str, Any]]]:
    repaired = dict(quantities)
    issues: list[dict[str, Any]] = []
    for removal in selection.get("grade_removals", []):
        card_id = removal["card_id"]
        quantity = int(removal.get("quantity", 0))
        before = repaired.get(card_id, 0)
        if before < quantity:
            issues.append(
                {
                    "code": "grade_repair_negative_quantity_after_removal",
                    "severity": "blocker",
                    "card_id": card_id,
                    "before": before,
                    "remove": quantity,
                }
            )
            repaired.pop(card_id, None)
            continue
        after = before - quantity
        if after:
            repaired[card_id] = after
        else:
            repaired.pop(card_id, None)
    for addition in selection.get("grade_additions", []):
        card_id = addition["card_id"]
        repaired[card_id] = repaired.get(card_id, 0) + int(addition.get("quantity", 0))
    return repaired, issues


def _repaired_quantity_rows(
    quantities: dict[str, int],
    recipe: dict[str, Any],
    selection: dict[str, Any],
) -> list[dict[str, Any]]:
    source_rows: dict[str, dict[str, Any]] = {}
    for row in recipe.get("quantities", []):
        source_rows[row["card_id"]] = row
    for row in selection.get("grade_additions", []):
        source_rows.setdefault(row["card_id"], row)
    rows: list[dict[str, Any]] = []
    for card_id in sorted(quantities):
        source = source_rows.get(card_id, {})
        rows.append(
            {
                "card_id": card_id,
                "name_th": source.get("name_th", ""),
                "quantity": quantities[card_id],
                "grade": str(source.get("grade", "")),
                "trigger": source.get("trigger", ""),
                "series_code": source.get("series_code", ""),
                "source": "m53_03_accepted_repair_preview",
            }
        )
    return rows


def build_fifth_slice_human_accepted_repair_artifact(
    selected_artifact: dict[str, Any] | None = None,
    drafts: dict[str, Any] | None = None,
    *,
    acceptance_text: str,
    accepted_by: str = "user",
    accepted_at: str | None = None,
) -> dict[str, Any]:
    selected_artifact = selected_artifact or load_json(M53_02_SELECTION)
    drafts = drafts or load_json(M52_DRAFTS)
    acceptance_text = _require_acceptance_text(acceptance_text)
    accepted_at = accepted_at or date.today().isoformat()
    selection = selected_artifact["selection"]
    recipe = _find_recipe(drafts, selection["recipe_id"])
    repaired_quantities, repair_issues = _apply_grade_repair(_quantity_map(recipe), selection)
    repaired_rows = _repaired_quantity_rows(repaired_quantities, recipe, selection)
    main_deck_count = sum(repaired_quantities.values())
    ready_for_m53_04 = (
        bool(selected_artifact.get("summary", {}).get("ready_for_m53_03"))
        and bool(selection.get("grade_repair_complete"))
        and not repair_issues
        and main_deck_count == 50
    )

    return {
        "version": "M53-03",
        "description": "Fifth-slice human-accepted repair artifact",
        "selected_target": selected_artifact.get("selected_target", {}),
        "source_inputs": {
            "m53_02_human_selected_recipe_artifact": str(M53_02_SELECTION.relative_to(ROOT)),
            "m52_recipe_drafts": str(M52_DRAFTS.relative_to(ROOT)),
        },
        "scope": {
            "offline_human_accepted_artifact": True,
            "records_human_selection": True,
            "records_human_acceptance": True,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
            "declares_recipe_valid": False,
        },
        "acceptance_policy": {
            "requires_m53_02_selection": True,
            "requires_non_empty_acceptance_text": True,
            "acceptance_is_not_validation": True,
            "runtime_promotion_allowed": False,
            "m53_04_must_rerun_validation": True,
        },
        "acceptance_record": {
            "decision": "accepted",
            "accepted_by": accepted_by,
            "accepted_at": accepted_at,
            "acceptance_text": acceptance_text,
            "accepted_review_item_id": selection["selected_review_item_id"],
            "accepted_recipe_id": selection["recipe_id"],
            "accepted_grade_profile_package_id": selection["grade_profile_package_id"],
            "source_note": "Recorded from explicit acceptance of the M53-02 selected fifth-slice recipe.",
        },
        "accepted_repair": {
            "recipe_id": selection["recipe_id"],
            "source_candidate_edge": selection.get("source_candidate_edge", ""),
            "pair": selection.get("pair", {}),
            "grade_profile_package_id": selection.get("grade_profile_package_id", ""),
            "grade_repair_type": selection.get("grade_repair_type", ""),
            "grade_additions": selection.get("grade_additions", []),
            "grade_removals": selection.get("grade_removals", []),
            "repaired_quantities": repaired_rows,
            "repair_application_issues": repair_issues,
            "main_deck_count_after_repair": main_deck_count,
            "grade_counts_after_repair_package": selection.get("grade_counts_after", {}),
            "runtime_promotion_allowed": False,
            "requires_m53_04_validation": True,
            "ready_for_m53_04_validation_rerun": ready_for_m53_04,
        },
        "summary": {
            "accepted_review_item_id": selection["selected_review_item_id"],
            "accepted_recipe_id": selection["recipe_id"],
            "accepted_grade_profile_package_id": selection["grade_profile_package_id"],
            "human_selection_recorded": True,
            "human_acceptance_recorded": True,
            "main_deck_count_after_repair": main_deck_count,
            "repair_application_issue_count": len(repair_issues),
            "runtime_promotion_allowed": False,
            "declares_recipe_valid": False,
            "ready_for_m53_04": ready_for_m53_04,
        },
        "next_target": {
            "milestone": "M53-04",
            "task": "Fifth-slice repaired recipe validation rerun",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    record = report["acceptance_record"]
    repair = report["accepted_repair"]
    lines = [
        "# M53-03 Fifth-Slice Human-Accepted Repair Artifact",
        "",
        "## Summary",
        "",
        f"- Accepted review item: `{summary['accepted_review_item_id']}`",
        f"- Accepted recipe: `{summary['accepted_recipe_id']}`",
        f"- Accepted grade package: `{summary['accepted_grade_profile_package_id']}`",
        f"- Human selection recorded: `{summary['human_selection_recorded']}`",
        f"- Human acceptance recorded: `{summary['human_acceptance_recorded']}`",
        f"- Main deck count after repair: `{summary['main_deck_count_after_repair']}`",
        f"- Repair application issues: `{summary['repair_application_issue_count']}`",
        f"- Declares recipe valid: `{summary['declares_recipe_valid']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M53-04: `{summary['ready_for_m53_04']}`",
        "",
        "## Acceptance Record",
        "",
        f"- Decision: `{record['decision']}`",
        f"- Accepted by: `{record['accepted_by']}`",
        f"- Accepted at: `{record['accepted_at']}`",
        f"- Acceptance text: `{record['acceptance_text']}`",
        "",
        "## Accepted Repair",
        "",
        f"- Source edge: `{repair['source_candidate_edge']}`",
        f"- Pair: `{repair['pair'].get('source_card_id', '')}` -> `{repair['pair'].get('target_card_id', '')}`",
        f"- Added cards: `{len(repair['grade_additions'])}` rows",
        f"- Removed cards: `{len(repair['grade_removals'])}` rows",
        f"- Repaired quantity rows: `{len(repair['repaired_quantities'])}`",
        f"- Grade counts after repair package: `{repair['grade_counts_after_repair_package']}`",
        "",
        "## Policy",
        "",
        "- Acceptance does not declare the recipe valid.",
        "- Runtime promotion remains disabled.",
        "- M53-04 must rerun validation before any fixture gate.",
        "",
        "## Next",
        "",
        "`M53-04`: Fifth-slice repaired recipe validation rerun.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M53-03 fifth-slice human-accepted repair artifact.")
    parser.add_argument("--acceptance-text", required=True, help="Non-empty text proving explicit acceptance.")
    parser.add_argument("--accepted-by", default="user")
    parser.add_argument("--accepted-at", default=None)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_fifth_slice_human_accepted_repair_artifact(
        acceptance_text=args.acceptance_text,
        accepted_by=args.accepted_by,
        accepted_at=args.accepted_at,
    )
    json_path = args.output_dir / "m53_03_fifth_slice_human_accepted_repair_artifact.json"
    md_path = args.output_dir / "m53_03_fifth_slice_human_accepted_repair_artifact.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M53-03 fifth-slice human-accepted repair artifact wrote {json_path}")
    print(f"M53-03 fifth-slice human-accepted repair artifact summary wrote {md_path}")
    print(
        "ready_for_m53_04={ready} accepted_recipe={recipe}".format(
            ready=report["summary"]["ready_for_m53_04"],
            recipe=report["summary"]["accepted_recipe_id"],
        )
    )
    return 0 if report["summary"]["ready_for_m53_04"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
