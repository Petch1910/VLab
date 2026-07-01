"""Build the M41-02 second-slice human-accepted repair artifact."""

from __future__ import annotations

import argparse
import json
import sys
from datetime import date
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M41_01_REVIEW = OUTPUT_DIR / "m41_01_second_slice_human_repair_review_packet.json"
M40_DRAFTS = OUTPUT_DIR / "m40_02_second_slice_recipe_draft_model.json"

DEFAULT_ACCEPTED_REVIEW_ITEM_ID = "m41_01_m40_recipe_001_repair_review"
DEFAULT_ACCEPTANCE_TEXT = "\u0e07\u0e31\u0e49\u0e19\u0e08\u0e31\u0e14\u0e44\u0e1b"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _find_review_item(review_packet: dict[str, Any], review_item_id: str) -> dict[str, Any]:
    for item in review_packet.get("review_items", []):
        if item.get("review_item_id") == review_item_id:
            return item
    raise ValueError(f"Review item not found: {review_item_id}")


def _find_recipe(drafts: dict[str, Any], recipe_id: str) -> dict[str, Any]:
    for recipe in drafts.get("recipe_drafts", []):
        if recipe.get("recipe_id") == recipe_id:
            return recipe
    raise ValueError(f"Recipe not found: {recipe_id}")


def _apply_repair(recipe: dict[str, Any], review_item: dict[str, Any]) -> tuple[dict[str, int], list[dict[str, Any]]]:
    quantities = {row["card_id"]: int(row.get("quantity", 0)) for row in recipe.get("quantities", [])}
    issues: list[dict[str, Any]] = []
    for removal in review_item.get("removals", []):
        card_id = removal["card_id"]
        quantity = int(removal.get("quantity", 0))
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
            quantities.pop(card_id, None)
            continue
        after = before - quantity
        if after:
            quantities[card_id] = after
        else:
            quantities.pop(card_id, None)
    for addition in review_item.get("additions", []):
        card_id = addition["card_id"]
        quantities[card_id] = quantities.get(card_id, 0) + int(addition.get("quantity", 0))
    return quantities, issues


def _repaired_quantity_rows(quantities: dict[str, int], recipe: dict[str, Any], review_item: dict[str, Any]) -> list[dict[str, Any]]:
    source_rows: dict[str, dict[str, Any]] = {}
    for row in recipe.get("quantities", []):
        source_rows[row["card_id"]] = row
    for row in review_item.get("additions", []):
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
                "source": "m41_02_accepted_repair_preview",
            }
        )
    return rows


def build_second_slice_human_accepted_repair_artifact(
    review_packet: dict[str, Any] | None = None,
    drafts: dict[str, Any] | None = None,
    *,
    accepted_review_item_id: str = DEFAULT_ACCEPTED_REVIEW_ITEM_ID,
    acceptance_text: str = DEFAULT_ACCEPTANCE_TEXT,
    accepted_at: str | None = None,
) -> dict[str, Any]:
    review_packet = review_packet or load_json(M41_01_REVIEW)
    drafts = drafts or load_json(M40_DRAFTS)
    review_item = _find_review_item(review_packet, accepted_review_item_id)
    recipe = _find_recipe(drafts, review_item["recipe_id"])
    repaired_quantities, repair_issues = _apply_repair(recipe, review_item)
    repaired_rows = _repaired_quantity_rows(repaired_quantities, recipe, review_item)
    accepted = bool(acceptance_text.strip()) and review_item["ready_for_human_repair_review"]
    accepted_at = accepted_at or date.today().isoformat()
    total_cards = sum(repaired_quantities.values())

    return {
        "version": "M41-02",
        "description": "Second-slice human-accepted repair artifact for Oracle Think Tank",
        "selected_target": review_packet.get("selected_target", {}),
        "source_inputs": {
            "human_repair_review_packet": str(M41_01_REVIEW.relative_to(ROOT)),
            "m40_recipe_drafts": str(M40_DRAFTS.relative_to(ROOT)),
        },
        "scope": {
            "offline_human_accepted_artifact": True,
            "records_human_acceptance": True,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
            "declares_recipe_valid": False,
        },
        "acceptance_record": {
            "decision": "accepted" if accepted else "pending",
            "accepted_by": "user",
            "accepted_at": accepted_at,
            "acceptance_text": acceptance_text,
            "interpreted_decision": "accept_first_ranked_complete_repair_candidate",
            "accepted_review_item_id": accepted_review_item_id,
            "accepted_recipe_id": review_item["recipe_id"],
            "accepted_repair_package_id": review_item["grade_profile_package_id"],
            "source_note": "Recorded from the user's go-ahead for continuing M41 after the M41-01 review packet.",
        },
        "accepted_repair": {
            "recipe_id": review_item["recipe_id"],
            "source_candidate_edge": review_item["source_candidate_edge"],
            "pair": review_item["pair"],
            "grade_profile_package_id": review_item["grade_profile_package_id"],
            "manual_review_card_ids": review_item["manual_review_card_ids"],
            "additions": review_item["additions"],
            "removals": review_item["removals"],
            "repaired_quantities": repaired_rows,
            "repair_application_issues": repair_issues,
            "main_deck_count_after_repair": total_cards,
            "grade_counts_after_repair_package": review_item["grade_counts_after"],
            "runtime_promotion_allowed": False,
            "requires_m41_03_validation": True,
            "ready_for_m41_03_validation_rerun": accepted and total_cards == 50 and not repair_issues,
        },
        "summary": {
            "accepted_review_item_id": accepted_review_item_id,
            "accepted_recipe_id": review_item["recipe_id"],
            "accepted_repair_package_id": review_item["grade_profile_package_id"],
            "human_acceptance_recorded": accepted,
            "main_deck_count_after_repair": total_cards,
            "repair_application_issue_count": len(repair_issues),
            "runtime_promotion_allowed": False,
            "declares_recipe_valid": False,
            "ready_for_m41_03": accepted and total_cards == 50 and not repair_issues,
        },
        "next_target": {
            "milestone": "M41-03",
            "task": "Second-slice repaired recipe validation rerun",
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
        "# M41-02 Second-Slice Human-Accepted Repair Artifact",
        "",
        "## Summary",
        "",
        f"- Accepted review item: `{summary['accepted_review_item_id']}`",
        f"- Accepted recipe: `{summary['accepted_recipe_id']}`",
        f"- Accepted repair package: `{summary['accepted_repair_package_id']}`",
        f"- Human acceptance recorded: `{summary['human_acceptance_recorded']}`",
        f"- Main deck count after repair: `{summary['main_deck_count_after_repair']}`",
        f"- Repair application issues: `{summary['repair_application_issue_count']}`",
        f"- Declares recipe valid: `{summary['declares_recipe_valid']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M41-03: `{summary['ready_for_m41_03']}`",
        "",
        "## Acceptance Record",
        "",
        f"- Decision: `{record['decision']}`",
        f"- Accepted by: `{record['accepted_by']}`",
        f"- Accepted at: `{record['accepted_at']}`",
        f"- Acceptance text: `{record['acceptance_text']}`",
        f"- Interpreted decision: `{record['interpreted_decision']}`",
        "",
        "## Accepted Repair",
        "",
        f"- Source edge: `{repair['source_candidate_edge']}`",
        f"- Pair: `{repair['pair']['source_card_id']}` -> `{repair['pair']['target_card_id']}`",
        f"- Added cards: `{len(repair['additions'])}` rows",
        f"- Removed cards: `{len(repair['removals'])}` rows",
        f"- Requires M41-03 validation: `{repair['requires_m41_03_validation']}`",
        "",
        "## Policy",
        "",
        "- This artifact records human acceptance of one repair candidate.",
        "- It does not mutate M40-02 recipe drafts.",
        "- It does not declare the recipe valid.",
        "- It does not create runtime fixtures, saved decks, UI entries, or bot playbooks.",
        "",
        "## Next",
        "",
        "`M41-03`: Second-slice repaired recipe validation rerun.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M41-02 second-slice human-accepted repair artifact.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    parser.add_argument("--accepted-review-item-id", default=DEFAULT_ACCEPTED_REVIEW_ITEM_ID)
    parser.add_argument("--acceptance-text", default=DEFAULT_ACCEPTANCE_TEXT)
    parser.add_argument("--accepted-at", default=None)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_second_slice_human_accepted_repair_artifact(
        accepted_review_item_id=args.accepted_review_item_id,
        acceptance_text=args.acceptance_text,
        accepted_at=args.accepted_at,
    )
    json_path = args.output_dir / "m41_02_second_slice_human_accepted_repair_artifact.json"
    md_path = args.output_dir / "m41_02_second_slice_human_accepted_repair_artifact.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M41-02 second-slice human-accepted repair artifact wrote {json_path}")
    print(f"M41-02 second-slice human-accepted repair artifact summary wrote {md_path}")
    print(
        "ready_for_m41_03={ready} accepted={accepted} recipe={recipe}".format(
            ready=report["summary"]["ready_for_m41_03"],
            accepted=report["summary"]["human_acceptance_recorded"],
            recipe=report["summary"]["accepted_recipe_id"],
        )
    )
    return 0 if report["summary"]["ready_for_m41_03"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
