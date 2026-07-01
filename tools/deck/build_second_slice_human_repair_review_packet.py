"""Build the M41-01 second-slice human repair review packet."""

from __future__ import annotations

import argparse
import csv
import io
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M40_CLOSEOUT = OUTPUT_DIR / "m40_closeout_second_slice_runtime_readiness.json"
M40_REPAIRS = OUTPUT_DIR / "m40_05_second_slice_blocker_repair_candidates.json"
M40_DRAFTS = OUTPUT_DIR / "m40_02_second_slice_recipe_draft_model.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _draft_map(drafts: dict[str, Any]) -> dict[str, dict[str, Any]]:
    return {item["recipe_id"]: item for item in drafts.get("recipe_drafts", [])}


def _compact_card_delta(cards: list[dict[str, Any]]) -> list[dict[str, Any]]:
    return [
        {
            "card_id": card.get("card_id", ""),
            "name_th": card.get("name_th", ""),
            "quantity": int(card.get("quantity", 0)),
            "grade": str(card.get("grade", "")),
            "series_code": card.get("series_code", ""),
            "source": card.get("source", ""),
        }
        for card in cards
    ]


def _decision_options() -> list[dict[str, str]]:
    return [
        {
            "option_id": "accept_repair_for_validation_rerun",
            "label": "Accept repair for validation rerun",
            "effect": "Allows M41-02 to record explicit acceptance and M41-03 to rerun validation; does not promote runtime.",
        },
        {
            "option_id": "request_different_repair",
            "label": "Request different repair",
            "effect": "Keeps the recipe advisory and asks for another repair candidate.",
        },
        {
            "option_id": "reject_recipe_runtime_candidate",
            "label": "Reject recipe runtime candidate",
            "effect": "Keeps this recipe out of future fixture promotion gates.",
        },
    ]


def _review_item(repair_item: dict[str, Any], draft: dict[str, Any] | None) -> dict[str, Any]:
    grade_package = repair_item["grade_profile_repair_package"]
    pair = (draft or {}).get("pair", {})
    additions = _compact_card_delta(grade_package.get("additions", []))
    removals = _compact_card_delta(grade_package.get("removals", []))
    return {
        "review_item_id": f"m41_01_{repair_item['recipe_id']}_repair_review",
        "item_type": "second_slice_repair_review",
        "recipe_id": repair_item["recipe_id"],
        "source_candidate_edge": repair_item.get("source_candidate_edge", ""),
        "source_edge_rank": (draft or {}).get("source_edge_rank"),
        "pair": {
            "source_card_id": pair.get("source_card_id", ""),
            "source_name_th": pair.get("source_name_th", ""),
            "target_card_id": pair.get("target_card_id", ""),
            "target_name_th": pair.get("target_name_th", ""),
            "net_score": pair.get("net_score"),
            "resource_verdict": pair.get("resource_verdict", ""),
            "timing_verdict": pair.get("timing_verdict", ""),
            "zone_verdict": pair.get("zone_verdict", ""),
        },
        "validation_status": repair_item.get("validation_status", ""),
        "consistency_status": repair_item.get("consistency_status", ""),
        "manual_review_card_ids": repair_item.get("manual_review_card_ids", []),
        "grade_profile_package_id": grade_package.get("package_id", ""),
        "repair_type": grade_package.get("repair_type", ""),
        "target_grade_counts": grade_package.get("target_grade_counts", {}),
        "grade_counts_after": grade_package.get("grade_counts_after", {}),
        "additions": additions,
        "removals": removals,
        "added_card_count": int(grade_package.get("added_card_count", 0)),
        "removed_card_count": int(grade_package.get("removed_card_count", 0)),
        "complete_candidate": bool(grade_package.get("complete_candidate")),
        "manual_overlap_cleared_by_grade_package": bool(repair_item.get("grade_package_clears_manual_overlap")),
        "ready_for_human_repair_review": bool(repair_item.get("ready_for_human_repair_review")),
        "human_decision_required": True,
        "runtime_promotion_allowed": False,
        "decision_options": _decision_options(),
        "recommended_reviewer_action": "review_grade_profile_package_before_m41_02_acceptance",
    }


def build_second_slice_human_repair_review_packet(
    closeout: dict[str, Any] | None = None,
    repairs: dict[str, Any] | None = None,
    drafts: dict[str, Any] | None = None,
) -> dict[str, Any]:
    closeout = closeout or load_json(M40_CLOSEOUT)
    repairs = repairs or load_json(M40_REPAIRS)
    drafts = drafts or load_json(M40_DRAFTS)
    drafts_by_recipe = _draft_map(drafts)
    review_items = [
        _review_item(item, drafts_by_recipe.get(item["recipe_id"]))
        for item in repairs.get("repair_items", [])
    ]
    ready_items = [item for item in review_items if item["ready_for_human_repair_review"]]
    complete_items = [item for item in review_items if item["complete_candidate"]]
    human_review_allowed = bool(closeout.get("summary", {}).get("human_repair_review_allowed"))
    ready_for_m41_02 = human_review_allowed and len(ready_items) == len(review_items) and len(review_items) > 0
    return {
        "version": "M41-01",
        "description": "Second-slice human repair review packet for Oracle Think Tank",
        "selected_target": repairs.get("selected_target", closeout.get("key_results", {}).get("selected_target", {})),
        "source_inputs": {
            "m40_closeout": str(M40_CLOSEOUT.relative_to(ROOT)),
            "m40_repair_candidates": str(M40_REPAIRS.relative_to(ROOT)),
            "m40_recipe_drafts": str(M40_DRAFTS.relative_to(ROOT)),
        },
        "scope": {
            "offline_human_review_packet": True,
            "records_human_acceptance": False,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
        },
        "review_policy": {
            "human_decision_required": True,
            "packet_is_not_acceptance": True,
            "runtime_promotion_allowed": False,
            "m41_02_must_record_explicit_acceptance": True,
            "m41_03_must_rerun_validation": True,
        },
        "summary": {
            "review_item_count": len(review_items),
            "ready_for_human_repair_review_count": len(ready_items),
            "complete_grade_profile_candidate_count": len(complete_items),
            "manual_overlap_recipe_count": int(repairs.get("summary", {}).get("manual_overlap_recipe_count", 0)),
            "runtime_promotion_allowed": False,
            "human_repair_review_allowed": human_review_allowed,
            "decision_option_count": len(_decision_options()),
            "ready_for_m41_02": ready_for_m41_02,
        },
        "review_items": review_items,
        "next_target": {
            "milestone": "M41-02",
            "task": "Second-slice human-accepted repair artifact",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M41-01 Second-Slice Human Repair Review Packet",
        "",
        "## Summary",
        "",
        f"- Review items: `{summary['review_item_count']}`",
        f"- Ready for human repair review: `{summary['ready_for_human_repair_review_count']}`",
        f"- Complete grade-profile candidates: `{summary['complete_grade_profile_candidate_count']}`",
        f"- Manual-overlap recipes: `{summary['manual_overlap_recipe_count']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M41-02: `{summary['ready_for_m41_02']}`",
        "",
        "## Review Items",
        "",
    ]
    for item in report["review_items"]:
        pair = item["pair"]
        lines.extend(
            [
                f"### `{item['recipe_id']}`",
                "",
                f"- Edge: `{item['source_candidate_edge']}`",
                f"- Pair: `{pair['source_card_id']}` {pair['source_name_th']} -> `{pair['target_card_id']}` {pair['target_name_th']}",
                f"- Repair package: `{item['grade_profile_package_id']}`",
                f"- Add/remove cards: `{item['added_card_count']}` / `{item['removed_card_count']}`",
                f"- Grade counts after: `{item['grade_counts_after']}`",
                f"- Manual review cards removed by package: `{item['manual_review_card_ids']}`",
                f"- Recommended action: `{item['recommended_reviewer_action']}`",
                "",
            ]
        )
    lines.extend(
        [
            "## Decision Options",
            "",
        ]
    )
    for option in _decision_options():
        lines.append(f"- `{option['option_id']}`: {option['label']} - {option['effect']}")
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- This packet is not human acceptance.",
            "- Runtime promotion remains disabled.",
            "- M41-02 must record explicit acceptance or rejection.",
            "- M41-03 must rerun repaired validation before any fixture gate.",
            "",
            "## Next",
            "",
            "`M41-02`: Second-slice human-accepted repair artifact.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def write_csv(report: dict[str, Any], path: Path) -> None:
    output = io.StringIO()
    writer = csv.DictWriter(
        output,
        fieldnames=[
            "review_item_id",
            "recipe_id",
            "source_candidate_edge",
            "source_card_id",
            "target_card_id",
            "grade_profile_package_id",
            "manual_review_card_ids",
            "added_card_ids",
            "removed_card_ids",
            "grade_counts_after",
            "recommended_reviewer_action",
        ],
    )
    writer.writeheader()
    for item in report["review_items"]:
        writer.writerow(
            {
                "review_item_id": item["review_item_id"],
                "recipe_id": item["recipe_id"],
                "source_candidate_edge": item["source_candidate_edge"],
                "source_card_id": item["pair"]["source_card_id"],
                "target_card_id": item["pair"]["target_card_id"],
                "grade_profile_package_id": item["grade_profile_package_id"],
                "manual_review_card_ids": "|".join(item["manual_review_card_ids"]),
                "added_card_ids": "|".join(card["card_id"] for card in item["additions"]),
                "removed_card_ids": "|".join(card["card_id"] for card in item["removals"]),
                "grade_counts_after": json.dumps(item["grade_counts_after"], ensure_ascii=False, sort_keys=True),
                "recommended_reviewer_action": item["recommended_reviewer_action"],
            }
        )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(output.getvalue(), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M41-01 second-slice human repair review packet.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_second_slice_human_repair_review_packet()
    json_path = args.output_dir / "m41_01_second_slice_human_repair_review_packet.json"
    md_path = args.output_dir / "m41_01_second_slice_human_repair_review_packet.md"
    csv_path = args.output_dir / "m41_01_second_slice_human_repair_review_packet.csv"
    write_json(report, json_path)
    write_markdown(report, md_path)
    write_csv(report, csv_path)
    print(f"M41-01 second-slice human repair review packet wrote {json_path}")
    print(f"M41-01 second-slice human repair review packet summary wrote {md_path}")
    print(f"M41-01 second-slice human repair review packet CSV wrote {csv_path}")
    print(
        "ready_for_m41_02={ready} review_items={items}".format(
            ready=report["summary"]["ready_for_m41_02"],
            items=report["summary"]["review_item_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m41_02"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
