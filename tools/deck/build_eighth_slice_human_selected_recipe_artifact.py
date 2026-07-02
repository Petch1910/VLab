"""Build the M65-02 eighth-slice human-selected recipe artifact.

This tool intentionally has no default selected review item. A caller must pass
an explicit M65-01 review item id and non-empty selection text.
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
M65_01_REVIEW = OUTPUT_DIR / "m65_01_eighth_slice_human_repair_review_packet.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _find_review_item(review_packet: dict[str, Any], review_item_id: str) -> dict[str, Any]:
    if not review_item_id.strip():
        raise ValueError("M65-02 requires an explicit non-empty review_item_id.")
    matches = [
        item
        for item in review_packet.get("review_items", [])
        if item.get("review_item_id") == review_item_id
    ]
    if not matches:
        raise ValueError(f"M65-01 review item not found: {review_item_id}")
    if len(matches) > 1:
        raise ValueError(f"M65-01 review item id is not unique: {review_item_id}")
    return matches[0]


def _require_selection_text(selection_text: str) -> str:
    normalized = selection_text.strip()
    if not normalized:
        raise ValueError("M65-02 requires non-empty selection_text.")
    return normalized


def _compact_grade_delta(rows: list[dict[str, Any]]) -> list[dict[str, Any]]:
    return [
        {
            "card_id": row.get("card_id", ""),
            "name_th": row.get("name_th", ""),
            "quantity": int(row.get("quantity", 0)),
            "grade": str(row.get("grade", "")),
            "series_code": row.get("series_code", ""),
            "source": row.get("source", ""),
        }
        for row in rows
    ]


def build_eighth_slice_human_selected_recipe_artifact(
    review_packet: dict[str, Any] | None = None,
    *,
    selected_review_item_id: str,
    selection_text: str,
    selected_by: str = "user",
    selected_at: str | None = None,
) -> dict[str, Any]:
    review_packet = review_packet or load_json(M65_01_REVIEW)
    selected_item = _find_review_item(review_packet, selected_review_item_id)
    selection_text = _require_selection_text(selection_text)
    selected_at = selected_at or date.today().isoformat()
    grade_context_ready = bool(selected_item.get("grade_repair_complete")) or bool(
        selected_item.get("grade_repair_not_needed")
    )

    ready_for_m65_03 = (
        bool(review_packet.get("summary", {}).get("ready_for_m65_02"))
        and bool(selected_item.get("ready_for_human_repair_review"))
        and bool(selected_item.get("human_selection_required"))
        and bool(selected_item.get("human_selection_pair_cards_present"))
        and grade_context_ready
        and not selected_item.get("structural_blockers")
    )

    return {
        "version": "M65-02",
        "description": "Eighth-slice human-selected recipe artifact",
        "selected_target": review_packet.get("selected_target", {}),
        "source_inputs": {
            "m65_01_human_repair_review_packet": str(M65_01_REVIEW.relative_to(ROOT)),
        },
        "scope": {
            "offline_human_selected_artifact": True,
            "records_human_selection": True,
            "records_human_acceptance": False,
            "records_grade_repair_acceptance": False,
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
        "selection_policy": {
            "requires_explicit_review_item_id": True,
            "requires_non_empty_selection_text": True,
            "exactly_one_recipe_selected": True,
            "selection_is_not_acceptance": True,
            "selection_is_not_grade_repair_acceptance": True,
            "selection_is_not_lock_decision": True,
            "selection_is_not_legion_decision": True,
            "runtime_promotion_allowed": False,
            "m65_03_must_record_explicit_grade_acceptance": True,
            "m65_04_must_record_explicit_lock_legion_decision": True,
            "m65_05_must_rerun_validation": True,
        },
        "selection": {
            "selected_review_item_id": selected_review_item_id,
            "recipe_id": selected_item["recipe_id"],
            "selected_at": selected_at,
            "selected_by": selected_by,
            "selection_text": selection_text,
            "selected_action": "select_recipe_for_m65_03_grade_acceptance_review",
            "source_candidate_edge": selected_item.get("source_candidate_edge", ""),
            "source_edge_rank": selected_item.get("source_edge_rank"),
            "pair": selected_item.get("pair", {}),
            "human_selection_package_id": selected_item.get("human_selection_package_id", ""),
            "human_selection_required": bool(selected_item.get("human_selection_required")),
            "human_selection_records_choice": bool(selected_item.get("human_selection_records_choice")),
            "human_selection_pair_cards_present": bool(
                selected_item.get("human_selection_pair_cards_present")
            ),
            "grade_profile_package_id": selected_item.get("grade_profile_package_id", ""),
            "grade_repair_type": selected_item.get("grade_repair_type", ""),
            "target_grade_counts": selected_item.get("target_grade_counts", {}),
            "grade_counts_before": selected_item.get("grade_counts_before", {}),
            "grade_counts_after": selected_item.get("grade_counts_after", {}),
            "grade_additions": _compact_grade_delta(selected_item.get("grade_additions", [])),
            "grade_removals": _compact_grade_delta(selected_item.get("grade_removals", [])),
            "grade_added_card_count": int(selected_item.get("grade_added_card_count", 0)),
            "grade_removed_card_count": int(selected_item.get("grade_removed_card_count", 0)),
            "grade_repair_complete": bool(selected_item.get("grade_repair_complete")),
            "grade_repair_not_needed": bool(selected_item.get("grade_repair_not_needed")),
            "lock_package_id": selected_item.get("lock_package_id", ""),
            "lock_deferred": bool(selected_item.get("lock_deferred")),
            "lock_can_be_repaired_in_m65_01": bool(selected_item.get("lock_can_be_repaired_in_m65_01")),
            "lock_future_system_work": selected_item.get("lock_future_system_work", []),
            "legion_package_id": selected_item.get("legion_package_id", ""),
            "legion_deferred": bool(selected_item.get("legion_deferred")),
            "legion_can_be_repaired_in_m65_01": bool(
                selected_item.get("legion_can_be_repaired_in_m65_01")
            ),
            "legion_future_system_work": selected_item.get("legion_future_system_work", []),
            "decision_options": selected_item.get("decision_options", []),
            "lock_decision_options": selected_item.get("lock_decision_options", []),
            "legion_decision_options": selected_item.get("legion_decision_options", []),
            "recommended_reviewer_action": selected_item.get("recommended_reviewer_action", ""),
            "ready_for_human_repair_review": bool(selected_item.get("ready_for_human_repair_review")),
            "runtime_promotion_allowed": False,
            "structural_blockers": selected_item.get("structural_blockers", []),
        },
        "summary": {
            "available_review_item_count": len(review_packet.get("review_items", [])),
            "selected_review_item_count": 1,
            "selected_recipe_id": selected_item["recipe_id"],
            "selected_human_selection_package_id": selected_item.get("human_selection_package_id", ""),
            "selected_grade_profile_package_id": selected_item.get("grade_profile_package_id", ""),
            "selected_lock_package_id": selected_item.get("lock_package_id", ""),
            "selected_legion_package_id": selected_item.get("legion_package_id", ""),
            "records_human_selection": True,
            "records_human_acceptance": False,
            "records_grade_repair_acceptance": False,
            "records_lock_decision": False,
            "records_legion_decision": False,
            "runtime_promotion_allowed": False,
            "lock_deferred": bool(selected_item.get("lock_deferred")),
            "legion_deferred": bool(selected_item.get("legion_deferred")),
            "ready_for_m65_03": ready_for_m65_03,
        },
        "next_target": {
            "milestone": "M65-03",
            "task": "Eighth-slice human-accepted grade repair artifact",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    selection = report["selection"]
    summary = report["summary"]
    pair = selection["pair"]
    lines = [
        "# M65-02 Eighth-Slice Human-Selected Recipe Artifact",
        "",
        "## Summary",
        "",
        f"- Available review items: `{summary['available_review_item_count']}`",
        f"- Selected review items: `{summary['selected_review_item_count']}`",
        f"- Selected recipe: `{summary['selected_recipe_id']}`",
        f"- Selected human-selection package: `{summary['selected_human_selection_package_id']}`",
        f"- Selected grade package: `{summary['selected_grade_profile_package_id']}`",
        f"- Selected Lock package: `{summary['selected_lock_package_id']}`",
        f"- Selected Legion package: `{summary['selected_legion_package_id']}`",
        f"- Records human selection: `{summary['records_human_selection']}`",
        f"- Records human acceptance: `{summary['records_human_acceptance']}`",
        f"- Records grade repair acceptance: `{summary['records_grade_repair_acceptance']}`",
        f"- Records Lock decision: `{summary['records_lock_decision']}`",
        f"- Records Legion decision: `{summary['records_legion_decision']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Lock deferred: `{summary['lock_deferred']}`",
        f"- Legion deferred: `{summary['legion_deferred']}`",
        f"- Ready for M65-03: `{summary['ready_for_m65_03']}`",
        "",
        "## Selection",
        "",
        f"- Review item: `{selection['selected_review_item_id']}`",
        f"- Selected at: `{selection['selected_at']}`",
        f"- Selected by: `{selection['selected_by']}`",
        f"- Selection text: `{selection['selection_text']}`",
        f"- Edge: `{selection['source_candidate_edge']}`",
        f"- Pair: `{pair.get('source_card_id', '')}` {pair.get('source_name_th', '')} -> `{pair.get('target_card_id', '')}` {pair.get('target_name_th', '')}",
        f"- Human-selection package: `{selection['human_selection_package_id']}`",
        f"- Grade counts after: `{selection['grade_counts_after']}`",
        f"- Lock deferred package: `{selection['lock_package_id']}`",
        f"- Legion deferred package: `{selection['legion_package_id']}`",
        "",
        "## Policy",
        "",
        "- This artifact records recipe selection only, not acceptance.",
        "- This artifact does not record grade repair acceptance.",
        "- This artifact does not record a Lock decision.",
        "- This artifact does not record a Legion decision.",
        "- Runtime promotion remains disabled.",
        "- M65-03 must record explicit grade repair acceptance or rejection.",
        "- M65-04 must record explicit Lock and Legion decisions.",
        "- M65-05 must rerun repaired validation before any fixture gate.",
        "",
        "## Next",
        "",
        "`M65-03`: Eighth-slice human-accepted grade repair artifact.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M65-02 eighth-slice human-selected recipe artifact.")
    parser.add_argument("--review-item-id", required=True, help="Explicit M65-01 review_item_id selected by the user.")
    parser.add_argument("--selection-text", required=True, help="Non-empty text proving an explicit user selection.")
    parser.add_argument("--selected-by", default="user")
    parser.add_argument("--selected-at", default=None)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_eighth_slice_human_selected_recipe_artifact(
        selected_review_item_id=args.review_item_id,
        selection_text=args.selection_text,
        selected_by=args.selected_by,
        selected_at=args.selected_at,
    )
    json_path = args.output_dir / "m65_02_eighth_slice_human_selected_recipe_artifact.json"
    md_path = args.output_dir / "m65_02_eighth_slice_human_selected_recipe_artifact.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M65-02 eighth-slice human-selected recipe artifact wrote {json_path}")
    print(f"M65-02 eighth-slice human-selected recipe artifact summary wrote {md_path}")
    print(
        "ready_for_m65_03={ready} selected_recipe={recipe}".format(
            ready=report["summary"]["ready_for_m65_03"],
            recipe=report["summary"]["selected_recipe_id"],
        )
    )
    return 0 if report["summary"]["ready_for_m65_03"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
