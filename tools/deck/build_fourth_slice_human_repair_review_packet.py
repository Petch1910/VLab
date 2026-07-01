"""Build the M49-01 fourth-slice human repair review packet."""

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
M48_CLOSEOUT = OUTPUT_DIR / "m48_closeout_fourth_slice_runtime_readiness.json"
M48_REPAIRS = OUTPUT_DIR / "m48_06_fourth_slice_blocker_repair_candidates.json"
M48_DRAFTS = OUTPUT_DIR / "m48_03_fourth_slice_recipe_draft_model.json"


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


def _manual_substitution_summary(substitutions: list[dict[str, Any]]) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    for item in substitutions:
        selected = item.get("selected_replacement") or {}
        rows.append(
            {
                "remove_card_id": item.get("remove_card_id", ""),
                "remove_quantity": int(item.get("remove_quantity", 0)),
                "replacement_card_id": selected.get("card_id", ""),
                "replacement_quantity": int(selected.get("quantity", 0) or 0),
                "source": selected.get("source", ""),
                "has_selected_replacement": bool(selected),
            }
        )
    return rows


def _decision_options() -> list[dict[str, str]]:
    return [
        {
            "option_id": "accept_main_deck_repair_for_g_zone_decision",
            "label": "Accept main-deck repair for G Zone decision",
            "effect": "Allows M49-02 to decide whether the accepted recipe stays main-deck-only or waits for G Zone support; does not promote runtime.",
        },
        {
            "option_id": "request_different_main_deck_repair",
            "label": "Request different main-deck repair",
            "effect": "Keeps the recipe advisory and asks for another repair candidate.",
        },
        {
            "option_id": "reject_recipe_runtime_candidate",
            "label": "Reject recipe runtime candidate",
            "effect": "Keeps this recipe out of future fixture promotion gates.",
        },
    ]


def _g_zone_decision_options() -> list[dict[str, str]]:
    return [
        {
            "option_id": "main_deck_only_for_current_windows_fixture",
            "label": "Use as main-deck-only fixture for current Windows scope",
            "effect": "Allows later validation to ignore G Zone recipe slots while keeping Stride/G-unit runtime disabled.",
        },
        {
            "option_id": "defer_recipe_until_g_zone_support",
            "label": "Defer this recipe until G Zone support exists",
            "effect": "Keeps the recipe advisory and blocks runtime fixture promotion.",
        },
        {
            "option_id": "open_g_zone_implementation_queue",
            "label": "Open a separate G Zone implementation queue",
            "effect": "Requires new specs/tests for G Zone slots, Stride timing, visibility, and validation before runtime use.",
        },
    ]


def _review_item(repair_item: dict[str, Any], draft: dict[str, Any] | None) -> dict[str, Any]:
    manual_package = repair_item["manual_overlap_repair_package"]
    grade_package = repair_item["grade_profile_repair_package"]
    g_zone_package = repair_item.get("g_zone_deferred_package") or {}
    pair = (draft or {}).get("pair", {})
    additions = _compact_card_delta(grade_package.get("additions", []))
    removals = _compact_card_delta(grade_package.get("removals", []))
    manual_substitutions = _manual_substitution_summary(manual_package.get("substitutions", []))
    grade_complete = bool(grade_package.get("complete_candidate")) or grade_package.get("reason") == "no_grade_profile_issue"
    return {
        "review_item_id": f"m49_01_{repair_item['recipe_id']}_repair_review",
        "item_type": "fourth_slice_repair_and_g_zone_review",
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
        "manual_repair_package_id": manual_package.get("package_id", ""),
        "manual_repair_complete": bool(manual_package.get("complete_candidate")),
        "manual_substitutions": manual_substitutions,
        "grade_profile_package_id": grade_package.get("package_id", ""),
        "grade_repair_type": grade_package.get("repair_type", ""),
        "target_grade_counts": grade_package.get("target_grade_counts", {}),
        "grade_counts_after": grade_package.get("grade_counts_after", {}),
        "grade_additions": additions,
        "grade_removals": removals,
        "grade_added_card_count": int(grade_package.get("added_card_count", 0)),
        "grade_removed_card_count": int(grade_package.get("removed_card_count", 0)),
        "grade_repair_complete": grade_complete,
        "grade_repair_not_needed": grade_package.get("reason") == "no_grade_profile_issue",
        "manual_overlap_cleared_by_grade_package": bool(repair_item.get("grade_package_clears_manual_overlap")),
        "g_zone_package_id": g_zone_package.get("package_id", ""),
        "g_zone_support_deferred": bool(g_zone_package),
        "g_zone_requires_future_system_work": list(g_zone_package.get("requires_future_system_work", [])),
        "g_zone_decision_required": bool(g_zone_package),
        "ready_for_human_repair_review": bool(repair_item.get("ready_for_human_repair_review")),
        "human_decision_required": True,
        "runtime_promotion_allowed": False,
        "decision_options": _decision_options(),
        "g_zone_decision_options": _g_zone_decision_options(),
        "recommended_reviewer_action": "review_main_deck_repair_then_route_to_m49_02_g_zone_decision",
    }


def build_fourth_slice_human_repair_review_packet(
    closeout: dict[str, Any] | None = None,
    repairs: dict[str, Any] | None = None,
    drafts: dict[str, Any] | None = None,
) -> dict[str, Any]:
    closeout = closeout or load_json(M48_CLOSEOUT)
    repairs = repairs or load_json(M48_REPAIRS)
    drafts = drafts or load_json(M48_DRAFTS)
    drafts_by_recipe = _draft_map(drafts)
    review_items = [
        _review_item(item, drafts_by_recipe.get(item["recipe_id"]))
        for item in repairs.get("repair_items", [])
    ]
    ready_items = [item for item in review_items if item["ready_for_human_repair_review"]]
    complete_manual_items = [item for item in review_items if item["manual_repair_complete"]]
    complete_grade_items = [item for item in review_items if item["grade_repair_complete"] and not item["grade_repair_not_needed"]]
    grade_not_needed_items = [item for item in review_items if item["grade_repair_not_needed"]]
    g_zone_items = [item for item in review_items if item["g_zone_decision_required"]]
    review_allowed = bool(closeout.get("summary", {}).get("human_g_zone_review_allowed"))
    ready_for_m49_02 = review_allowed and len(ready_items) == len(review_items) and len(g_zone_items) == len(review_items) and len(review_items) > 0
    return {
        "version": "M49-01",
        "description": "Fourth-slice human repair and G Zone review packet",
        "selected_target": repairs.get("selected_target", closeout.get("key_results", {}).get("selected_target", {})),
        "source_inputs": {
            "m48_closeout": str(M48_CLOSEOUT.relative_to(ROOT)),
            "m48_repair_candidates": str(M48_REPAIRS.relative_to(ROOT)),
            "m48_recipe_drafts": str(M48_DRAFTS.relative_to(ROOT)),
        },
        "scope": {
            "offline_human_review_packet": True,
            "records_human_acceptance": False,
            "records_g_zone_decision": False,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
        },
        "review_policy": {
            "human_decision_required": True,
            "g_zone_decision_required": True,
            "packet_is_not_acceptance": True,
            "runtime_promotion_allowed": False,
            "m49_02_must_record_g_zone_boundary_decision": True,
            "m49_03_must_record_explicit_acceptance": True,
            "m49_04_must_rerun_validation": True,
        },
        "summary": {
            "review_item_count": len(review_items),
            "ready_for_human_repair_review_count": len(ready_items),
            "complete_manual_repair_count": len(complete_manual_items),
            "complete_grade_profile_candidate_count": len(complete_grade_items),
            "grade_profile_not_needed_count": len(grade_not_needed_items),
            "g_zone_decision_item_count": len(g_zone_items),
            "manual_overlap_recipe_count": int(repairs.get("summary", {}).get("manual_overlap_recipe_count", 0)),
            "runtime_promotion_allowed": False,
            "human_g_zone_review_allowed": review_allowed,
            "decision_option_count": len(_decision_options()),
            "g_zone_decision_option_count": len(_g_zone_decision_options()),
            "ready_for_m49_02": ready_for_m49_02,
        },
        "review_items": review_items,
        "next_target": {
            "milestone": "M49-02",
            "task": "Fourth-slice G Zone support decision",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M49-01 Fourth-Slice Human Repair and G Zone Review Packet",
        "",
        "## Summary",
        "",
        f"- Review items: `{summary['review_item_count']}`",
        f"- Ready for human repair review: `{summary['ready_for_human_repair_review_count']}`",
        f"- Complete manual repair packages: `{summary['complete_manual_repair_count']}`",
        f"- Complete grade-profile candidates: `{summary['complete_grade_profile_candidate_count']}`",
        f"- Grade profile not needed: `{summary['grade_profile_not_needed_count']}`",
        f"- G Zone decision items: `{summary['g_zone_decision_item_count']}`",
        f"- Manual-overlap recipes: `{summary['manual_overlap_recipe_count']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M49-02: `{summary['ready_for_m49_02']}`",
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
                f"- Manual repair package: `{item['manual_repair_package_id']}`",
                f"- Manual substitutions: `{len(item['manual_substitutions'])}`",
                f"- Grade repair package: `{item['grade_profile_package_id']}`",
                f"- Grade add/remove cards: `{item['grade_added_card_count']}` / `{item['grade_removed_card_count']}`",
                f"- Grade counts after: `{item['grade_counts_after']}`",
                f"- G Zone package: `{item['g_zone_package_id']}`",
                f"- G Zone future work: `{item['g_zone_requires_future_system_work']}`",
                f"- Recommended action: `{item['recommended_reviewer_action']}`",
                "",
            ]
        )
    lines.extend(["## Decision Options", ""])
    for option in _decision_options():
        lines.append(f"- `{option['option_id']}`: {option['label']} - {option['effect']}")
    lines.extend(["", "## G Zone Decision Options", ""])
    for option in _g_zone_decision_options():
        lines.append(f"- `{option['option_id']}`: {option['label']} - {option['effect']}")
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- This packet is not human acceptance.",
            "- This packet is not a G Zone boundary decision.",
            "- Runtime promotion remains disabled.",
            "- M49-02 must record the G Zone boundary decision.",
            "- M49-03 must record explicit acceptance or rejection.",
            "- M49-04 must rerun repaired validation before any fixture gate.",
            "",
            "## Next",
            "",
            "`M49-02`: Fourth-slice G Zone support decision.",
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
            "manual_repair_package_id",
            "manual_review_card_ids",
            "manual_replacement_card_ids",
            "grade_profile_package_id",
            "grade_added_card_ids",
            "grade_removed_card_ids",
            "grade_counts_after",
            "g_zone_package_id",
            "g_zone_future_work_count",
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
                "manual_repair_package_id": item["manual_repair_package_id"],
                "manual_review_card_ids": "|".join(item["manual_review_card_ids"]),
                "manual_replacement_card_ids": "|".join(
                    row["replacement_card_id"] for row in item["manual_substitutions"]
                ),
                "grade_profile_package_id": item["grade_profile_package_id"],
                "grade_added_card_ids": "|".join(card["card_id"] for card in item["grade_additions"]),
                "grade_removed_card_ids": "|".join(card["card_id"] for card in item["grade_removals"]),
                "grade_counts_after": json.dumps(item["grade_counts_after"], ensure_ascii=False, sort_keys=True),
                "g_zone_package_id": item["g_zone_package_id"],
                "g_zone_future_work_count": len(item["g_zone_requires_future_system_work"]),
                "recommended_reviewer_action": item["recommended_reviewer_action"],
            }
        )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(output.getvalue(), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M49-01 fourth-slice human repair review packet.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_fourth_slice_human_repair_review_packet()
    json_path = args.output_dir / "m49_01_fourth_slice_human_repair_review_packet.json"
    md_path = args.output_dir / "m49_01_fourth_slice_human_repair_review_packet.md"
    csv_path = args.output_dir / "m49_01_fourth_slice_human_repair_review_packet.csv"
    write_json(report, json_path)
    write_markdown(report, md_path)
    write_csv(report, csv_path)
    print(f"M49-01 fourth-slice human repair review packet wrote {json_path}")
    print(f"M49-01 fourth-slice human repair review packet summary wrote {md_path}")
    print(f"M49-01 fourth-slice human repair review packet CSV wrote {csv_path}")
    print(
        "ready_for_m49_02={ready} review_items={items} g_zone_items={g_zone}".format(
            ready=report["summary"]["ready_for_m49_02"],
            items=report["summary"]["review_item_count"],
            g_zone=report["summary"]["g_zone_decision_item_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m49_02"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
