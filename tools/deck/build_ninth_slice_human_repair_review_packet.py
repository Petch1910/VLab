"""Build the M69-01 ninth-slice human repair review packet."""

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
M68_CLOSEOUT = OUTPUT_DIR / "m68_closeout_ninth_slice_runtime_readiness.json"
M68_REPAIRS = OUTPUT_DIR / "m68_06_ninth_slice_blocker_repair_candidates.json"
M68_DRAFTS = OUTPUT_DIR / "m68_03_ninth_slice_recipe_draft_model.json"


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


def _compact_manual_substitutions(package: dict[str, Any] | None) -> list[dict[str, Any]]:
    if not package:
        return []
    rows: list[dict[str, Any]] = []
    for item in package.get("substitutions", []):
        selected = item.get("selected_replacement") or {}
        rows.append(
            {
                "remove_card_id": item.get("remove_card_id", ""),
                "remove_quantity": int(item.get("remove_quantity", 0)),
                "selected_replacement_card_id": selected.get("card_id", ""),
                "selected_replacement_quantity": int(selected.get("quantity", 0) or 0),
                "selected_replacement_source": selected.get("source", ""),
                "replacement_option_count": len(item.get("replacement_options", [])),
            }
        )
    return rows


def _decision_options() -> list[dict[str, str]]:
    return [
        {
            "option_id": "accept_recipe_repairs_keep_g_zone_stride_aqua_deferred_for_validation_rerun",
            "label": "Accept recipe repairs and keep G Zone/Stride/Aqua deferred for validation rerun",
            "effect": "Allows M69-02/M69-03 to record recipe/repair choice, but runtime promotion remains blocked by system decisions.",
        },
        {
            "option_id": "accept_original_manual_cards_and_keep_advisory",
            "label": "Accept original manual-review cards and keep advisory",
            "effect": "Records that the team accepts manual cards for later review, but does not promote runtime.",
        },
        {
            "option_id": "request_different_repair_or_system_scope",
            "label": "Request different repair or system scope",
            "effect": "Keeps the recipe advisory and asks for another repair candidate or explicit G Zone/Stride/Aqua scope decision.",
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
            "option_id": "defer_until_g_zone_runtime_exists",
            "label": "Defer until G Zone runtime exists",
            "effect": "Keeps the recipe advisory until G Zone deck slots, G unit visibility, and Generation Break support exist.",
        },
        {
            "option_id": "main_deck_only_review_no_runtime_promotion",
            "label": "Main-deck-only review, no runtime promotion",
            "effect": "Allows review of the 50-card main deck but blocks fixture/runtime promotion.",
        },
    ]


def _stride_decision_options() -> list[dict[str, str]]:
    return [
        {
            "option_id": "defer_until_stride_runtime_exists",
            "label": "Defer until Stride runtime exists",
            "effect": "Keeps the recipe advisory until Stride declaration timing, cost validation, heart-card state, and end-phase return exist.",
        },
        {
            "option_id": "main_deck_only_review_no_runtime_promotion",
            "label": "Main-deck-only review, no runtime promotion",
            "effect": "Allows review of the 50-card main deck but blocks fixture/runtime promotion.",
        },
    ]


def _aqua_force_decision_options() -> list[dict[str, str]]:
    return [
        {
            "option_id": "defer_until_aqua_force_battle_order_runtime_exists",
            "label": "Defer until Aqua Force battle-order runtime exists",
            "effect": "Keeps the recipe advisory until battle-count tracking, attack-order predicates, and multi-attack labels exist.",
        },
        {
            "option_id": "manual_semantic_review_only_no_runtime_promotion",
            "label": "Manual semantic review only, no runtime promotion",
            "effect": "Allows review of Aqua Force battle-order text but blocks fixture/runtime promotion.",
        },
    ]


def _review_item(repair_item: dict[str, Any], draft: dict[str, Any] | None) -> dict[str, Any]:
    manual_package = repair_item.get("manual_overlap_repair_package") or {}
    grade_package = repair_item.get("grade_profile_repair_package") or {}
    g_zone_package = repair_item.get("g_zone_deferred_package") or {}
    stride_package = repair_item.get("stride_deferred_package") or {}
    aqua_package = repair_item.get("aqua_force_battle_order_deferred_package") or {}
    pair = (draft or {}).get("pair", {})
    additions = _compact_card_delta(grade_package.get("additions", []))
    removals = _compact_card_delta(grade_package.get("removals", []))
    manual_substitutions = _compact_manual_substitutions(manual_package)
    return {
        "review_item_id": f"m69_01_{repair_item['recipe_id']}_repair_review",
        "item_type": "ninth_slice_human_repair_g_zone_stride_aqua_review",
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
        "manual_overlap_package_id": manual_package.get("package_id", ""),
        "manual_repair_type": manual_package.get("repair_type", ""),
        "manual_substitutions": manual_substitutions,
        "manual_repair_complete": bool(manual_package.get("complete_candidate")),
        "manual_review_may_accept_original_card": bool(manual_package.get("manual_review_may_accept_original_card")),
        "grade_profile_package_id": grade_package.get("package_id", ""),
        "grade_repair_type": grade_package.get("repair_type", ""),
        "target_grade_counts": grade_package.get("target_grade_counts", {}),
        "grade_counts_after": grade_package.get("grade_counts_after", {}),
        "grade_additions": additions,
        "grade_removals": removals,
        "grade_added_card_count": int(grade_package.get("added_card_count", 0)),
        "grade_removed_card_count": int(grade_package.get("removed_card_count", 0)),
        "grade_repair_complete": bool(grade_package.get("complete_candidate")),
        "grade_repair_not_needed": grade_package.get("reason") == "no_grade_profile_issue",
        "g_zone_package_id": g_zone_package.get("package_id", ""),
        "g_zone_deferred": bool(g_zone_package),
        "g_zone_can_be_repaired_in_m69_01": False,
        "g_zone_future_system_work": g_zone_package.get("requires_future_system_work", []),
        "stride_package_id": stride_package.get("package_id", ""),
        "stride_deferred": bool(stride_package),
        "stride_can_be_repaired_in_m69_01": False,
        "stride_future_system_work": stride_package.get("requires_future_system_work", []),
        "aqua_force_battle_order_package_id": aqua_package.get("package_id", ""),
        "aqua_force_battle_order_deferred": bool(aqua_package),
        "aqua_force_battle_order_can_be_repaired_in_m69_01": False,
        "aqua_force_battle_order_future_system_work": aqua_package.get("requires_future_system_work", []),
        "ready_for_human_repair_review": bool(repair_item.get("ready_for_human_repair_review")),
        "human_decision_required": True,
        "runtime_promotion_allowed": False,
        "decision_options": _decision_options(),
        "g_zone_decision_options": _g_zone_decision_options(),
        "stride_decision_options": _stride_decision_options(),
        "aqua_force_decision_options": _aqua_force_decision_options(),
        "recommended_reviewer_action": "select_one_recipe_review_manual_grade_repairs_and_record_g_zone_stride_aqua_decisions_before_m69_02",
    }


def build_ninth_slice_human_repair_review_packet(
    closeout: dict[str, Any] | None = None,
    repairs: dict[str, Any] | None = None,
    drafts: dict[str, Any] | None = None,
) -> dict[str, Any]:
    closeout = closeout or load_json(M68_CLOSEOUT)
    repairs = repairs or load_json(M68_REPAIRS)
    drafts = drafts or load_json(M68_DRAFTS)
    drafts_by_recipe = _draft_map(drafts)
    review_items = [
        _review_item(item, drafts_by_recipe.get(item["recipe_id"]))
        for item in repairs.get("repair_items", [])
    ]
    ready_items = [item for item in review_items if item["ready_for_human_repair_review"]]
    complete_manual_items = [item for item in review_items if item["manual_repair_complete"]]
    complete_grade_items = [item for item in review_items if item["grade_repair_complete"]]
    grade_not_needed_items = [item for item in review_items if item["grade_repair_not_needed"]]
    g_zone_items = [item for item in review_items if item["g_zone_deferred"]]
    stride_items = [item for item in review_items if item["stride_deferred"]]
    aqua_items = [item for item in review_items if item["aqua_force_battle_order_deferred"]]
    human_selection_review_allowed = bool(closeout.get("summary", {}).get("human_selection_review_allowed"))
    ready_for_m69_02 = (
        human_selection_review_allowed
        and len(ready_items) == len(review_items)
        and len(review_items) > 0
    )
    return {
        "version": "M69-01",
        "description": "Ninth-slice human repair review packet",
        "selected_target": repairs.get("selected_target", closeout.get("key_results", {}).get("selected_target", {})),
        "source_inputs": {
            "m68_closeout": str(M68_CLOSEOUT.relative_to(ROOT)),
            "m68_repair_candidates": str(M68_REPAIRS.relative_to(ROOT)),
            "m68_recipe_drafts": str(M68_DRAFTS.relative_to(ROOT)),
        },
        "scope": {
            "offline_human_review_packet": True,
            "records_human_acceptance": False,
            "records_human_selection": False,
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
        "review_policy": {
            "human_decision_required": True,
            "exactly_one_recipe_should_be_selected_later": True,
            "packet_is_not_acceptance": True,
            "packet_is_not_selection": True,
            "packet_is_not_g_zone_decision": True,
            "packet_is_not_stride_decision": True,
            "packet_is_not_aqua_force_battle_order_decision": True,
            "runtime_promotion_allowed": False,
            "m69_02_must_record_explicit_selection": True,
            "m69_03_must_record_explicit_acceptance": True,
            "m69_04_must_record_g_zone_stride_aqua_decision": True,
            "m69_05_must_rerun_validation": True,
        },
        "summary": {
            "review_item_count": len(review_items),
            "ready_for_human_repair_review_count": len(ready_items),
            "complete_manual_repair_candidate_count": len(complete_manual_items),
            "complete_grade_profile_candidate_count": len(complete_grade_items),
            "grade_profile_not_needed_count": len(grade_not_needed_items),
            "g_zone_deferred_item_count": len(g_zone_items),
            "stride_deferred_item_count": len(stride_items),
            "aqua_force_battle_order_deferred_item_count": len(aqua_items),
            "unexpected_structural_blocker_item_count": sum(1 for item in review_items if item["structural_blockers"]),
            "runtime_promotion_allowed": False,
            "human_selection_review_allowed": human_selection_review_allowed,
            "decision_option_count": len(_decision_options()),
            "g_zone_decision_option_count": len(_g_zone_decision_options()),
            "stride_decision_option_count": len(_stride_decision_options()),
            "aqua_force_decision_option_count": len(_aqua_force_decision_options()),
            "ready_for_m69_02": ready_for_m69_02,
        },
        "review_items": review_items,
        "next_target": {
            "milestone": "M69-02",
            "task": "Ninth-slice human-selected recipe artifact",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M69-01 Ninth-Slice Human Repair Review Packet",
        "",
        "## Summary",
        "",
        f"- Review items: `{summary['review_item_count']}`",
        f"- Ready for human repair review: `{summary['ready_for_human_repair_review_count']}`",
        f"- Complete manual repair candidates: `{summary['complete_manual_repair_candidate_count']}`",
        f"- Complete grade-profile candidates: `{summary['complete_grade_profile_candidate_count']}`",
        f"- Grade profile not needed: `{summary['grade_profile_not_needed_count']}`",
        f"- G Zone deferred items: `{summary['g_zone_deferred_item_count']}`",
        f"- Stride deferred items: `{summary['stride_deferred_item_count']}`",
        f"- Aqua Force battle-order deferred items: `{summary['aqua_force_battle_order_deferred_item_count']}`",
        f"- Unexpected structural blockers: `{summary['unexpected_structural_blocker_item_count']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M69-02: `{summary['ready_for_m69_02']}`",
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
                f"- Manual repair package: `{item['manual_overlap_package_id']}`",
                f"- Manual substitutions: `{len(item['manual_substitutions'])}`",
                f"- Grade repair package: `{item['grade_profile_package_id']}`",
                f"- Grade add/remove cards: `{item['grade_added_card_count']}` / `{item['grade_removed_card_count']}`",
                f"- G Zone package: `{item['g_zone_package_id']}`",
                f"- Stride package: `{item['stride_package_id']}`",
                f"- Aqua Force package: `{item['aqua_force_battle_order_package_id']}`",
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
    lines.extend(["", "## Stride Decision Options", ""])
    for option in _stride_decision_options():
        lines.append(f"- `{option['option_id']}`: {option['label']} - {option['effect']}")
    lines.extend(["", "## Aqua Force Decision Options", ""])
    for option in _aqua_force_decision_options():
        lines.append(f"- `{option['option_id']}`: {option['label']} - {option['effect']}")
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- This packet is not human selection, acceptance, G Zone decision, Stride decision, or Aqua Force decision.",
            "- Runtime promotion remains disabled.",
            "- M69-02 must record exactly one selected recipe.",
            "- M69-03 must record explicit acceptance or rejection.",
            "- M69-04 must record explicit G Zone/Stride and Aqua Force battle-order decisions.",
            "- M69-05 must rerun repaired validation before any fixture gate.",
            "",
            "## Next",
            "",
            "`M69-02`: Ninth-slice human-selected recipe artifact.",
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
            "manual_overlap_package_id",
            "manual_remove_card_ids",
            "manual_replacement_card_ids",
            "grade_profile_package_id",
            "grade_added_card_ids",
            "grade_removed_card_ids",
            "g_zone_package_id",
            "stride_package_id",
            "aqua_force_battle_order_package_id",
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
                "manual_overlap_package_id": item["manual_overlap_package_id"],
                "manual_remove_card_ids": "|".join(row["remove_card_id"] for row in item["manual_substitutions"]),
                "manual_replacement_card_ids": "|".join(
                    row["selected_replacement_card_id"] for row in item["manual_substitutions"]
                ),
                "grade_profile_package_id": item["grade_profile_package_id"],
                "grade_added_card_ids": "|".join(card["card_id"] for card in item["grade_additions"]),
                "grade_removed_card_ids": "|".join(card["card_id"] for card in item["grade_removals"]),
                "g_zone_package_id": item["g_zone_package_id"],
                "stride_package_id": item["stride_package_id"],
                "aqua_force_battle_order_package_id": item["aqua_force_battle_order_package_id"],
                "recommended_reviewer_action": item["recommended_reviewer_action"],
            }
        )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(output.getvalue(), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M69-01 ninth-slice human repair review packet.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_ninth_slice_human_repair_review_packet()
    json_path = args.output_dir / "m69_01_ninth_slice_human_repair_review_packet.json"
    md_path = args.output_dir / "m69_01_ninth_slice_human_repair_review_packet.md"
    csv_path = args.output_dir / "m69_01_ninth_slice_human_repair_review_packet.csv"
    write_json(report, json_path)
    write_markdown(report, md_path)
    write_csv(report, csv_path)
    print(f"M69-01 ninth-slice human repair review packet wrote {json_path}")
    print(f"M69-01 ninth-slice human repair review packet summary wrote {md_path}")
    print(f"M69-01 ninth-slice human repair review packet CSV wrote {csv_path}")
    print(
        "ready_for_m69_02={ready} review_items={items}".format(
            ready=report["summary"]["ready_for_m69_02"],
            items=report["summary"]["review_item_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m69_02"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
