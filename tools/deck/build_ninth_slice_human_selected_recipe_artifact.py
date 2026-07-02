"""Build the M69-02 ninth-slice human-selected recipe artifact.

This tool intentionally has no default selected review item. A caller must pass
an explicit M69-01 review item id and non-empty selection text.
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
M69_01_REVIEW = OUTPUT_DIR / "m69_01_ninth_slice_human_repair_review_packet.json"


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
        raise ValueError("M69-02 requires an explicit non-empty review_item_id.")
    matches = [
        item
        for item in review_packet.get("review_items", [])
        if item.get("review_item_id") == review_item_id
    ]
    if not matches:
        raise ValueError(f"M69-01 review item not found: {review_item_id}")
    if len(matches) > 1:
        raise ValueError(f"M69-01 review item id is not unique: {review_item_id}")
    return matches[0]


def _require_selection_text(selection_text: str) -> str:
    normalized = selection_text.strip()
    if not normalized:
        raise ValueError("M69-02 requires non-empty selection_text.")
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


def _compact_manual_substitutions(rows: list[dict[str, Any]]) -> list[dict[str, Any]]:
    return [
        {
            "remove_card_id": row.get("remove_card_id", ""),
            "remove_quantity": int(row.get("remove_quantity", 0)),
            "selected_replacement_card_id": row.get("selected_replacement_card_id", ""),
            "selected_replacement_quantity": int(row.get("selected_replacement_quantity", 0)),
            "selected_replacement_source": row.get("selected_replacement_source", ""),
            "replacement_option_count": int(row.get("replacement_option_count", 0)),
        }
        for row in rows
    ]


def build_ninth_slice_human_selected_recipe_artifact(
    review_packet: dict[str, Any] | None = None,
    *,
    selected_review_item_id: str,
    selection_text: str,
    selected_by: str = "user",
    selected_at: str | None = None,
) -> dict[str, Any]:
    review_packet = review_packet or load_json(M69_01_REVIEW)
    selected_item = _find_review_item(review_packet, selected_review_item_id)
    selection_text = _require_selection_text(selection_text)
    selected_at = selected_at or date.today().isoformat()
    grade_context_ready = bool(selected_item.get("grade_repair_complete")) or bool(
        selected_item.get("grade_repair_not_needed")
    )

    ready_for_m69_03 = (
        bool(review_packet.get("summary", {}).get("ready_for_m69_02"))
        and bool(selected_item.get("ready_for_human_repair_review"))
        and bool(selected_item.get("manual_repair_complete"))
        and grade_context_ready
        and not selected_item.get("structural_blockers")
    )

    return {
        "version": "M69-02",
        "description": "Ninth-slice human-selected recipe artifact",
        "selected_target": review_packet.get("selected_target", {}),
        "source_inputs": {
            "m69_01_human_repair_review_packet": str(M69_01_REVIEW.relative_to(ROOT)),
        },
        "scope": {
            "offline_human_selected_artifact": True,
            "records_human_selection": True,
            "records_human_acceptance": False,
            "records_g_zone_decision": False,
            "records_stride_decision": False,
            "records_aqua_force_battle_order_decision": False,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "g_zone_runtime": False,
            "stride_runtime": False,
            "aqua_force_battle_order_runtime": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
        },
        "selection_policy": {
            "requires_explicit_review_item_id": True,
            "requires_non_empty_selection_text": True,
            "exactly_one_recipe_selected": True,
            "selection_is_not_acceptance": True,
            "selection_is_not_g_zone_decision": True,
            "selection_is_not_stride_decision": True,
            "selection_is_not_aqua_force_battle_order_decision": True,
            "runtime_promotion_allowed": False,
            "m69_03_must_record_explicit_acceptance": True,
            "m69_04_must_record_explicit_g_zone_stride_aqua_decision": True,
            "m69_05_must_rerun_validation": True,
        },
        "selection": {
            "selected_review_item_id": selected_review_item_id,
            "recipe_id": selected_item["recipe_id"],
            "selected_at": selected_at,
            "selected_by": selected_by,
            "selection_text": selection_text,
            "selected_action": "select_recipe_for_m69_03_acceptance_review",
            "source_candidate_edge": selected_item.get("source_candidate_edge", ""),
            "source_edge_rank": selected_item.get("source_edge_rank"),
            "pair": selected_item.get("pair", {}),
            "manual_review_card_ids": selected_item.get("manual_review_card_ids", []),
            "manual_overlap_package_id": selected_item.get("manual_overlap_package_id", ""),
            "manual_repair_type": selected_item.get("manual_repair_type", ""),
            "manual_substitutions": _compact_manual_substitutions(selected_item.get("manual_substitutions", [])),
            "manual_repair_complete": bool(selected_item.get("manual_repair_complete")),
            "manual_review_may_accept_original_card": bool(
                selected_item.get("manual_review_may_accept_original_card")
            ),
            "grade_profile_package_id": selected_item.get("grade_profile_package_id", ""),
            "grade_repair_type": selected_item.get("grade_repair_type", ""),
            "target_grade_counts": selected_item.get("target_grade_counts", {}),
            "grade_counts_after": selected_item.get("grade_counts_after", {}),
            "grade_additions": _compact_grade_delta(selected_item.get("grade_additions", [])),
            "grade_removals": _compact_grade_delta(selected_item.get("grade_removals", [])),
            "grade_added_card_count": int(selected_item.get("grade_added_card_count", 0)),
            "grade_removed_card_count": int(selected_item.get("grade_removed_card_count", 0)),
            "grade_repair_complete": bool(selected_item.get("grade_repair_complete")),
            "grade_repair_not_needed": bool(selected_item.get("grade_repair_not_needed")),
            "g_zone_package_id": selected_item.get("g_zone_package_id", ""),
            "g_zone_deferred": bool(selected_item.get("g_zone_deferred")),
            "g_zone_can_be_repaired_in_m69_01": bool(selected_item.get("g_zone_can_be_repaired_in_m69_01")),
            "g_zone_future_system_work": selected_item.get("g_zone_future_system_work", []),
            "stride_package_id": selected_item.get("stride_package_id", ""),
            "stride_deferred": bool(selected_item.get("stride_deferred")),
            "stride_can_be_repaired_in_m69_01": bool(selected_item.get("stride_can_be_repaired_in_m69_01")),
            "stride_future_system_work": selected_item.get("stride_future_system_work", []),
            "aqua_force_battle_order_package_id": selected_item.get("aqua_force_battle_order_package_id", ""),
            "aqua_force_battle_order_deferred": bool(selected_item.get("aqua_force_battle_order_deferred")),
            "aqua_force_battle_order_can_be_repaired_in_m69_01": bool(
                selected_item.get("aqua_force_battle_order_can_be_repaired_in_m69_01")
            ),
            "aqua_force_battle_order_future_system_work": selected_item.get(
                "aqua_force_battle_order_future_system_work", []
            ),
            "decision_options": selected_item.get("decision_options", []),
            "g_zone_decision_options": selected_item.get("g_zone_decision_options", []),
            "stride_decision_options": selected_item.get("stride_decision_options", []),
            "aqua_force_decision_options": selected_item.get("aqua_force_decision_options", []),
            "recommended_reviewer_action": selected_item.get("recommended_reviewer_action", ""),
            "ready_for_human_repair_review": bool(selected_item.get("ready_for_human_repair_review")),
            "runtime_promotion_allowed": False,
            "structural_blockers": selected_item.get("structural_blockers", []),
        },
        "summary": {
            "available_review_item_count": len(review_packet.get("review_items", [])),
            "selected_review_item_count": 1,
            "selected_recipe_id": selected_item["recipe_id"],
            "selected_manual_overlap_package_id": selected_item.get("manual_overlap_package_id", ""),
            "selected_grade_profile_package_id": selected_item.get("grade_profile_package_id", ""),
            "selected_g_zone_package_id": selected_item.get("g_zone_package_id", ""),
            "selected_stride_package_id": selected_item.get("stride_package_id", ""),
            "selected_aqua_force_battle_order_package_id": selected_item.get(
                "aqua_force_battle_order_package_id", ""
            ),
            "records_human_selection": True,
            "records_human_acceptance": False,
            "records_g_zone_decision": False,
            "records_stride_decision": False,
            "records_aqua_force_battle_order_decision": False,
            "runtime_promotion_allowed": False,
            "g_zone_deferred": bool(selected_item.get("g_zone_deferred")),
            "stride_deferred": bool(selected_item.get("stride_deferred")),
            "aqua_force_battle_order_deferred": bool(selected_item.get("aqua_force_battle_order_deferred")),
            "ready_for_m69_03": ready_for_m69_03,
        },
        "next_target": {
            "milestone": "M69-03",
            "task": "Ninth-slice human-accepted repair artifact",
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
        "# M69-02 Ninth-Slice Human-Selected Recipe Artifact",
        "",
        "## Summary",
        "",
        f"- Available review items: `{summary['available_review_item_count']}`",
        f"- Selected review items: `{summary['selected_review_item_count']}`",
        f"- Selected recipe: `{summary['selected_recipe_id']}`",
        f"- Selected manual package: `{summary['selected_manual_overlap_package_id']}`",
        f"- Selected grade package: `{summary['selected_grade_profile_package_id']}`",
        f"- Selected G Zone package: `{summary['selected_g_zone_package_id']}`",
        f"- Selected Stride package: `{summary['selected_stride_package_id']}`",
        f"- Selected Aqua Force package: `{summary['selected_aqua_force_battle_order_package_id']}`",
        f"- Records human selection: `{summary['records_human_selection']}`",
        f"- Records human acceptance: `{summary['records_human_acceptance']}`",
        f"- Records G Zone decision: `{summary['records_g_zone_decision']}`",
        f"- Records Stride decision: `{summary['records_stride_decision']}`",
        f"- Records Aqua Force decision: `{summary['records_aqua_force_battle_order_decision']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- G Zone deferred: `{summary['g_zone_deferred']}`",
        f"- Stride deferred: `{summary['stride_deferred']}`",
        f"- Aqua Force battle-order deferred: `{summary['aqua_force_battle_order_deferred']}`",
        f"- Ready for M69-03: `{summary['ready_for_m69_03']}`",
        "",
        "## Selection",
        "",
        f"- Review item: `{selection['selected_review_item_id']}`",
        f"- Selected at: `{selection['selected_at']}`",
        f"- Selected by: `{selection['selected_by']}`",
        f"- Selection text: `{selection['selection_text']}`",
        f"- Edge: `{selection['source_candidate_edge']}`",
        f"- Pair: `{pair.get('source_card_id', '')}` {pair.get('source_name_th', '')} -> `{pair.get('target_card_id', '')}` {pair.get('target_name_th', '')}",
        f"- Manual substitutions: `{len(selection['manual_substitutions'])}`",
        f"- Grade counts after: `{selection['grade_counts_after']}`",
        f"- G Zone deferred package: `{selection['g_zone_package_id']}`",
        f"- Stride deferred package: `{selection['stride_package_id']}`",
        f"- Aqua Force deferred package: `{selection['aqua_force_battle_order_package_id']}`",
        "",
        "## Policy",
        "",
        "- This artifact records selection only, not acceptance.",
        "- This artifact does not record a G Zone decision.",
        "- This artifact does not record a Stride decision.",
        "- This artifact does not record an Aqua Force battle-order decision.",
        "- Runtime promotion remains disabled.",
        "- M69-03 must record explicit acceptance or rejection.",
        "- M69-04 must record explicit G Zone, Stride, and Aqua Force battle-order decisions.",
        "- M69-05 must rerun repaired validation before any fixture gate.",
        "",
        "## Next",
        "",
        "`M69-03`: Ninth-slice human-accepted repair artifact.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M69-02 ninth-slice human-selected recipe artifact.")
    parser.add_argument("--review-item-id", required=True, help="Explicit M69-01 review_item_id selected by the user.")
    parser.add_argument("--selection-text", required=True, help="Non-empty text proving an explicit user selection.")
    parser.add_argument("--selected-by", default="user")
    parser.add_argument("--selected-at", default=None)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_ninth_slice_human_selected_recipe_artifact(
        selected_review_item_id=args.review_item_id,
        selection_text=args.selection_text,
        selected_by=args.selected_by,
        selected_at=args.selected_at,
    )
    json_path = args.output_dir / "m69_02_ninth_slice_human_selected_recipe_artifact.json"
    md_path = args.output_dir / "m69_02_ninth_slice_human_selected_recipe_artifact.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M69-02 ninth-slice human-selected recipe artifact wrote {json_path}")
    print(f"M69-02 ninth-slice human-selected recipe artifact summary wrote {md_path}")
    print(
        "ready_for_m69_03={ready} selected_recipe={recipe}".format(
            ready=report["summary"]["ready_for_m69_03"],
            recipe=report["summary"]["selected_recipe_id"],
        )
    )
    return 0 if report["summary"]["ready_for_m69_03"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
