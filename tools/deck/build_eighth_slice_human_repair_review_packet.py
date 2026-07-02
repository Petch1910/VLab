"""Build the M65-01 eighth-slice human repair review packet."""

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
M64_CLOSEOUT = OUTPUT_DIR / "m64_closeout_eighth_slice_runtime_readiness.json"
M64_REPAIRS = OUTPUT_DIR / "m64_06_eighth_slice_blocker_repair_candidates.json"
M64_DRAFTS = OUTPUT_DIR / "m64_03_eighth_slice_recipe_draft_model.json"


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
            "option_id": "accept_recipe_grade_repair_keep_lock_legion_deferred_for_validation_rerun",
            "label": "Accept recipe grade repair and keep Lock/Legion deferred for validation rerun",
            "effect": "Allows M65-02/M65-03 to record recipe and grade repair choice, but runtime promotion remains blocked by system decisions.",
        },
        {
            "option_id": "accept_recipe_main_deck_review_no_runtime_promotion",
            "label": "Accept main-deck review only, no runtime promotion",
            "effect": "Records that the team accepts this recipe for advisory review while keeping runtime fixture promotion blocked.",
        },
        {
            "option_id": "request_different_grade_repair_or_system_scope",
            "label": "Request different grade repair or system scope",
            "effect": "Keeps the recipe advisory and asks for another grade repair candidate or explicit Lock/Legion scope decision.",
        },
        {
            "option_id": "reject_recipe_runtime_candidate",
            "label": "Reject recipe runtime candidate",
            "effect": "Keeps this recipe out of future fixture promotion gates.",
        },
    ]


def _lock_decision_options() -> list[dict[str, str]]:
    return [
        {
            "option_id": "defer_until_lock_unlock_runtime_exists",
            "label": "Defer until Lock/Unlock runtime exists",
            "effect": "Keeps the recipe advisory until locked-circle state, unlock timing, and cleanup support exist.",
        },
        {
            "option_id": "main_deck_only_review_no_runtime_promotion",
            "label": "Main-deck-only review, no runtime promotion",
            "effect": "Allows review of the 50-card main deck but blocks fixture/runtime promotion.",
        },
    ]


def _legion_decision_options() -> list[dict[str, str]]:
    return [
        {
            "option_id": "defer_until_legion_mate_runtime_exists",
            "label": "Defer until Legion/Mate runtime exists",
            "effect": "Keeps the recipe advisory until Legion declaration, Mate validation, and zone movement helpers exist.",
        },
        {
            "option_id": "main_deck_only_review_no_runtime_promotion",
            "label": "Main-deck-only review, no runtime promotion",
            "effect": "Allows review of the 50-card main deck but blocks fixture/runtime promotion.",
        },
    ]


def _review_item(repair_item: dict[str, Any], draft: dict[str, Any] | None) -> dict[str, Any]:
    human_package = repair_item.get("human_selection_package") or {}
    grade_package = repair_item.get("grade_profile_repair_package") or {}
    lock_package = repair_item.get("lock_deferred_package") or {}
    legion_package = repair_item.get("legion_deferred_package") or {}
    pair = (draft or {}).get("pair", {})
    additions = _compact_card_delta(grade_package.get("additions", []))
    removals = _compact_card_delta(grade_package.get("removals", []))
    return {
        "review_item_id": f"m65_01_{repair_item['recipe_id']}_repair_review",
        "item_type": "eighth_slice_human_grade_lock_legion_review",
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
        "structural_blockers": repair_item.get("structural_blockers", []),
        "manual_review_card_ids": repair_item.get("manual_review_card_ids", []),
        "human_selection_package_id": human_package.get("package_id", ""),
        "human_selection_required": bool(human_package.get("requires_human_selection", False)),
        "human_selection_records_choice": bool(human_package.get("records_human_selection", False)),
        "human_selection_pair_cards_present": bool(human_package.get("pair_cards_present", False)),
        "grade_profile_package_id": grade_package.get("package_id", ""),
        "grade_repair_type": grade_package.get("repair_type", ""),
        "target_grade_counts": grade_package.get("target_grade_counts", {}),
        "grade_counts_before": grade_package.get("grade_counts_before", {}),
        "grade_counts_after": grade_package.get("grade_counts_after", {}),
        "grade_additions": additions,
        "grade_removals": removals,
        "grade_added_card_count": int(grade_package.get("added_card_count", 0)),
        "grade_removed_card_count": int(grade_package.get("removed_card_count", 0)),
        "grade_repair_complete": bool(grade_package.get("complete_candidate")),
        "grade_repair_not_needed": grade_package.get("reason") == "no_grade_profile_issue",
        "lock_package_id": lock_package.get("package_id", ""),
        "lock_deferred": bool(lock_package),
        "lock_can_be_repaired_in_m65_01": False,
        "lock_future_system_work": lock_package.get("requires_future_system_work", []),
        "legion_package_id": legion_package.get("package_id", ""),
        "legion_deferred": bool(legion_package),
        "legion_can_be_repaired_in_m65_01": False,
        "legion_future_system_work": legion_package.get("requires_future_system_work", []),
        "ready_for_human_repair_review": bool(repair_item.get("ready_for_human_repair_review")),
        "human_decision_required": True,
        "runtime_promotion_allowed": False,
        "decision_options": _decision_options(),
        "lock_decision_options": _lock_decision_options(),
        "legion_decision_options": _legion_decision_options(),
        "recommended_reviewer_action": "select_one_recipe_review_grade_repair_and_record_lock_legion_decisions_before_m65_02",
    }


def build_eighth_slice_human_repair_review_packet(
    closeout: dict[str, Any] | None = None,
    repairs: dict[str, Any] | None = None,
    drafts: dict[str, Any] | None = None,
) -> dict[str, Any]:
    closeout = closeout or load_json(M64_CLOSEOUT)
    repairs = repairs or load_json(M64_REPAIRS)
    drafts = drafts or load_json(M64_DRAFTS)
    drafts_by_recipe = _draft_map(drafts)
    review_items = [
        _review_item(item, drafts_by_recipe.get(item["recipe_id"]))
        for item in repairs.get("repair_items", [])
    ]
    ready_items = [item for item in review_items if item["ready_for_human_repair_review"]]
    human_selection_items = [item for item in review_items if item["human_selection_required"]]
    complete_grade_items = [item for item in review_items if item["grade_repair_complete"]]
    grade_not_needed_items = [item for item in review_items if item["grade_repair_not_needed"]]
    lock_items = [item for item in review_items if item["lock_deferred"]]
    legion_items = [item for item in review_items if item["legion_deferred"]]
    human_selection_review_allowed = bool(closeout.get("summary", {}).get("human_selection_review_allowed"))
    ready_for_m65_02 = (
        human_selection_review_allowed
        and len(ready_items) == len(review_items)
        and len(review_items) > 0
    )
    return {
        "version": "M65-01",
        "description": "Eighth-slice human repair review packet",
        "selected_target": repairs.get("selected_target", closeout.get("key_results", {}).get("selected_target", {})),
        "source_inputs": {
            "m64_closeout": str(M64_CLOSEOUT.relative_to(ROOT)),
            "m64_repair_candidates": str(M64_REPAIRS.relative_to(ROOT)),
            "m64_recipe_drafts": str(M64_DRAFTS.relative_to(ROOT)),
        },
        "scope": {
            "offline_human_review_packet": True,
            "records_human_acceptance": False,
            "records_human_selection": False,
            "records_lock_decision": False,
            "records_legion_decision": False,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "lock_runtime": False,
            "legion_runtime": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
        },
        "review_policy": {
            "human_decision_required": True,
            "exactly_one_recipe_should_be_selected_later": True,
            "packet_is_not_acceptance": True,
            "packet_is_not_selection": True,
            "packet_is_not_lock_decision": True,
            "packet_is_not_legion_decision": True,
            "runtime_promotion_allowed": False,
            "m65_02_must_record_explicit_selection": True,
            "m65_03_must_record_explicit_grade_acceptance": True,
            "m65_04_must_record_lock_legion_decision": True,
            "m65_05_must_rerun_validation": True,
        },
        "summary": {
            "review_item_count": len(review_items),
            "ready_for_human_repair_review_count": len(ready_items),
            "human_selection_candidate_count": len(human_selection_items),
            "complete_grade_profile_candidate_count": len(complete_grade_items),
            "grade_profile_not_needed_count": len(grade_not_needed_items),
            "manual_overlap_item_count": sum(1 for item in review_items if item["manual_review_card_ids"]),
            "lock_deferred_item_count": len(lock_items),
            "legion_deferred_item_count": len(legion_items),
            "unexpected_structural_blocker_item_count": sum(1 for item in review_items if item["structural_blockers"]),
            "runtime_promotion_allowed": False,
            "human_selection_review_allowed": human_selection_review_allowed,
            "decision_option_count": len(_decision_options()),
            "lock_decision_option_count": len(_lock_decision_options()),
            "legion_decision_option_count": len(_legion_decision_options()),
            "ready_for_m65_02": ready_for_m65_02,
        },
        "review_items": review_items,
        "next_target": {
            "milestone": "M65-02",
            "task": "Eighth-slice human-selected recipe artifact",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M65-01 Eighth-Slice Human Repair Review Packet",
        "",
        "## Summary",
        "",
        f"- Review items: `{summary['review_item_count']}`",
        f"- Ready for human repair review: `{summary['ready_for_human_repair_review_count']}`",
        f"- Human selection candidates: `{summary['human_selection_candidate_count']}`",
        f"- Complete grade-profile candidates: `{summary['complete_grade_profile_candidate_count']}`",
        f"- Grade profile not needed: `{summary['grade_profile_not_needed_count']}`",
        f"- Manual-overlap items: `{summary['manual_overlap_item_count']}`",
        f"- Lock deferred items: `{summary['lock_deferred_item_count']}`",
        f"- Legion deferred items: `{summary['legion_deferred_item_count']}`",
        f"- Unexpected structural blockers: `{summary['unexpected_structural_blocker_item_count']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M65-02: `{summary['ready_for_m65_02']}`",
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
                f"- Human selection package: `{item['human_selection_package_id']}`",
                f"- Grade repair package: `{item['grade_profile_package_id']}`",
                f"- Grade add/remove cards: `{item['grade_added_card_count']}` / `{item['grade_removed_card_count']}`",
                f"- Lock package: `{item['lock_package_id']}`",
                f"- Legion package: `{item['legion_package_id']}`",
                f"- Recommended action: `{item['recommended_reviewer_action']}`",
                "",
            ]
        )
    lines.extend(["## Decision Options", ""])
    for option in _decision_options():
        lines.append(f"- `{option['option_id']}`: {option['label']} - {option['effect']}")
    lines.extend(["", "## Lock Decision Options", ""])
    for option in _lock_decision_options():
        lines.append(f"- `{option['option_id']}`: {option['label']} - {option['effect']}")
    lines.extend(["", "## Legion Decision Options", ""])
    for option in _legion_decision_options():
        lines.append(f"- `{option['option_id']}`: {option['label']} - {option['effect']}")
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- This packet is not human selection, grade acceptance, Lock decision, or Legion decision.",
            "- Runtime promotion remains disabled.",
            "- M65-02 must record exactly one selected recipe.",
            "- M65-03 must record explicit grade repair acceptance or rejection.",
            "- M65-04 must record explicit Lock/Legion decisions.",
            "- M65-05 must rerun repaired validation before any fixture gate.",
            "",
            "## Next",
            "",
            "`M65-02`: Eighth-slice human-selected recipe artifact.",
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
            "human_selection_package_id",
            "grade_profile_package_id",
            "grade_added_card_ids",
            "grade_removed_card_ids",
            "lock_package_id",
            "legion_package_id",
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
                "human_selection_package_id": item["human_selection_package_id"],
                "grade_profile_package_id": item["grade_profile_package_id"],
                "grade_added_card_ids": "|".join(card["card_id"] for card in item["grade_additions"]),
                "grade_removed_card_ids": "|".join(card["card_id"] for card in item["grade_removals"]),
                "lock_package_id": item["lock_package_id"],
                "legion_package_id": item["legion_package_id"],
                "recommended_reviewer_action": item["recommended_reviewer_action"],
            }
        )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(output.getvalue(), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M65-01 eighth-slice human repair review packet.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_eighth_slice_human_repair_review_packet()
    json_path = args.output_dir / "m65_01_eighth_slice_human_repair_review_packet.json"
    md_path = args.output_dir / "m65_01_eighth_slice_human_repair_review_packet.md"
    csv_path = args.output_dir / "m65_01_eighth_slice_human_repair_review_packet.csv"
    write_json(report, json_path)
    write_markdown(report, md_path)
    write_csv(report, csv_path)
    print(f"M65-01 eighth-slice human repair review packet wrote {json_path}")
    print(f"M65-01 eighth-slice human repair review packet summary wrote {md_path}")
    print(f"M65-01 eighth-slice human repair review packet CSV wrote {csv_path}")
    print(
        "ready_for_m65_02={ready} review_items={items}".format(
            ready=report["summary"]["ready_for_m65_02"],
            items=report["summary"]["review_item_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m65_02"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
