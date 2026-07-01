"""Build the M36-02 deck recipe draft model.

This converts advisory M35 deck skeletons into explicit, reviewable draft
recipe quantities. The drafts are not legal deck claims and are not injected
into runtime decks.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
D2_SKELETONS = OUTPUT_DIR / "m35_d2_first_slice_deck_skeleton_ratio_plans.json"
D3_COMBO_LINES = OUTPUT_DIR / "m35_d3_first_slice_combo_line_explainer.json"
M36_REVIEW_PACKET = OUTPUT_DIR / "m36_01_first_slice_review_packet.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _unique(items: Sequence[str]) -> list[str]:
    seen: set[str] = set()
    result: list[str] = []
    for item in items:
        if item and item not in seen:
            seen.add(item)
            result.append(item)
    return result


def _roles_for(card_id: str, roles: dict[str, list[str]]) -> list[str]:
    result: list[str] = []
    for role, cards in roles.items():
        if card_id in cards:
            result.append(role)
    return result


def _line_review_maps(review_packet: dict[str, Any]) -> tuple[dict[str, dict[str, Any]], dict[str, dict[str, Any]], set[str]]:
    accepted_by_line = {
        item["source_line_id"]: item
        for item in review_packet.get("accepted_seed_review_items", [])
    }
    rejected_by_line = {
        item["item_id"]: item
        for item in review_packet.get("rejected_line_review_items", [])
    }
    manual_card_ids = {
        item["card_id"]
        for item in review_packet.get("manual_card_review_items", [])
    }
    return accepted_by_line, rejected_by_line, manual_card_ids


def _combo_line_by_skeleton(d3_report: dict[str, Any]) -> dict[str, dict[str, Any]]:
    return {
        line["source_skeleton_id"]: line
        for line in d3_report.get("combo_lines", [])
    }


def _allocate_triggers(trigger_cards: list[str], trigger_slots: int) -> tuple[dict[str, int], int]:
    quantities: dict[str, int] = {}
    remaining = trigger_slots
    for card_id in trigger_cards:
        if remaining <= 0:
            break
        qty = min(4, remaining)
        quantities[card_id] = qty
        remaining -= qty
    return quantities, remaining


def _allocate_normal_units(normal_cards: list[str], key_cards: set[str], normal_slots: int) -> tuple[dict[str, int], int]:
    quantities: dict[str, int] = {}
    remaining = normal_slots
    for card_id in normal_cards:
        if remaining <= 0:
            break
        base = 4 if card_id in key_cards else 2
        qty = min(base, remaining)
        quantities[card_id] = qty
        remaining -= qty

    while remaining > 0:
        changed = False
        for card_id in normal_cards:
            if remaining <= 0:
                break
            current = quantities.get(card_id, 0)
            if current < 4:
                quantities[card_id] = current + 1
                remaining -= 1
                changed = True
        if not changed:
            break
    return quantities, remaining


def _quantity_rows(
    skeleton: dict[str, Any],
    trigger_quantities: dict[str, int],
    normal_quantities: dict[str, int],
) -> list[dict[str, Any]]:
    roles = skeleton.get("roles", {})
    rows: list[dict[str, Any]] = []
    for card_id in _unique(list(normal_quantities) + list(trigger_quantities)):
        quantity = normal_quantities.get(card_id, 0) + trigger_quantities.get(card_id, 0)
        rows.append(
            {
                "card_id": card_id,
                "quantity": quantity,
                "roles": _roles_for(card_id, roles),
                "quantity_source": "m36_02_role_based_draft_allocation",
            }
        )
    return rows


def _build_recipe(
    index: int,
    skeleton: dict[str, Any],
    combo_line: dict[str, Any] | None,
    accepted_by_line: dict[str, dict[str, Any]],
    rejected_by_line: dict[str, dict[str, Any]],
    manual_card_ids: set[str],
) -> dict[str, Any]:
    roles = skeleton.get("roles", {})
    ratio = skeleton.get("ratio_targets", {})
    trigger_slots = int(ratio.get("trigger_slots", 16))
    normal_slots = int(ratio.get("normal_unit_slots", 34))
    trigger_cards = _unique(roles.get("trigger_cards", []))
    key_cards = set(roles.get("key_cards", []))
    normal_cards = _unique(
        [
            *roles.get("key_cards", []),
            *roles.get("support_cards", []),
            *roles.get("resource_recovery_cards", []),
        ]
    )
    normal_cards = [card_id for card_id in normal_cards if card_id not in set(trigger_cards)]
    trigger_quantities, unfilled_trigger = _allocate_triggers(trigger_cards, trigger_slots)
    normal_quantities, unfilled_normal = _allocate_normal_units(normal_cards, key_cards, normal_slots)
    rows = _quantity_rows(skeleton, trigger_quantities, normal_quantities)
    line_id = combo_line.get("line_id") if combo_line else ""
    accepted_item = accepted_by_line.get(line_id)
    rejected_item = rejected_by_line.get(line_id)
    manual_overlap = sorted({row["card_id"] for row in rows if row["card_id"] in manual_card_ids})
    if accepted_item:
        review_status = "accepted_seed_pending_recipe_validation"
        review_blockers = accepted_item.get("blocked_until", [])
    elif rejected_item:
        review_status = "blocked_rejected_line_support_gap"
        review_blockers = rejected_item.get("blocked_until", [])
    else:
        review_status = "unreviewed_combo_line"
        review_blockers = ["human_review_required"]
    if manual_overlap:
        review_status = f"{review_status}_manual_card_overlap"
        review_blockers = _unique([*review_blockers, "manual_card_semantic_review"])

    explicit_total = sum(row["quantity"] for row in rows)
    target_total = int(ratio.get("main_deck_size", 50))
    slot_gap = unfilled_trigger + unfilled_normal + max(0, target_total - explicit_total - unfilled_trigger - unfilled_normal)
    return {
        "recipe_id": f"recipe_{index:03d}",
        "source_skeleton_id": skeleton["skeleton_id"],
        "source_package_id": skeleton["source_package_id"],
        "source_line_id": line_id,
        "anchor_card_id": skeleton["anchor_card_id"],
        "anchor_name_th": skeleton.get("anchor_name_th", ""),
        "review_status": review_status,
        "review_blockers": review_blockers,
        "recipe_status": "draft_with_gaps" if slot_gap else "draft_quantity_complete",
        "quantities": rows,
        "slot_summary": {
            "main_deck_target": target_total,
            "explicit_card_count": explicit_total,
            "trigger_slots_target": trigger_slots,
            "trigger_slots_assigned": sum(trigger_quantities.values()),
            "trigger_slots_unfilled": unfilled_trigger,
            "normal_slots_target": normal_slots,
            "normal_slots_assigned": sum(normal_quantities.values()),
            "normal_slots_unfilled": unfilled_normal,
            "total_unfilled_slots": max(0, target_total - explicit_total),
            "max_copy_limit_assumed": 4,
        },
        "validation_metadata": {
            "draft_only": True,
            "requires_m36_03_validator": True,
            "requires_human_acceptance": True,
            "contains_manual_review_cards": bool(manual_overlap),
            "manual_review_card_ids": manual_overlap,
            "not_runtime_deck": True,
            "not_auto_injected": True,
        },
    }


def build_recipe_drafts(
    d2_report: dict[str, Any] | None = None,
    d3_report: dict[str, Any] | None = None,
    review_packet: dict[str, Any] | None = None,
) -> dict[str, Any]:
    d2_report = d2_report or load_json(D2_SKELETONS)
    d3_report = d3_report or load_json(D3_COMBO_LINES)
    review_packet = review_packet or load_json(M36_REVIEW_PACKET)
    accepted_by_line, rejected_by_line, manual_card_ids = _line_review_maps(review_packet)
    combo_by_skeleton = _combo_line_by_skeleton(d3_report)
    recipes = [
        _build_recipe(
            index=index,
            skeleton=skeleton,
            combo_line=combo_by_skeleton.get(skeleton["skeleton_id"]),
            accepted_by_line=accepted_by_line,
            rejected_by_line=rejected_by_line,
            manual_card_ids=manual_card_ids,
        )
        for index, skeleton in enumerate(d2_report.get("skeletons", []), start=1)
    ]
    accepted_count = sum(1 for item in recipes if item["review_status"].startswith("accepted_seed"))
    rejected_count = sum(1 for item in recipes if item["review_status"].startswith("blocked_rejected_line"))
    return {
        "version": "M36-02",
        "description": "Deck recipe draft model from first-slice advisory skeletons",
        "selected_target": d2_report.get("selected_target", {}),
        "source_inputs": {
            "deck_skeleton_ratio_plans": str(D2_SKELETONS.relative_to(ROOT)),
            "combo_line_explainer": str(D3_COMBO_LINES.relative_to(ROOT)),
            "first_slice_review_packet": str(M36_REVIEW_PACKET.relative_to(ROOT)),
        },
        "scope": {
            "offline_recipe_draft_model": True,
            "runtime_deck": False,
            "bot_integration": False,
            "deck_validator_result": False,
            "automatic_deck_injection": False,
        },
        "draft_policy": {
            "card_quantities_are_advisory": True,
            "slot_gaps_are_allowed_until_m36_03": True,
            "copy_limit_assumption": 4,
            "human_review_required_before_runtime": True,
            "no_live_card_text_parsing": True,
            "no_direct_GameState_mutation": True,
        },
        "summary": {
            "source_skeleton_count": len(d2_report.get("skeletons", [])),
            "recipe_draft_count": len(recipes),
            "accepted_seed_recipe_count": accepted_count,
            "rejected_line_recipe_count": rejected_count,
            "manual_overlap_recipe_count": sum(
                1 for item in recipes if item["validation_metadata"]["contains_manual_review_cards"]
            ),
            "recipes_with_slot_gaps": sum(1 for item in recipes if item["recipe_status"] == "draft_with_gaps"),
            "ready_for_m36_03": len(recipes) == len(d2_report.get("skeletons", [])) and bool(recipes),
        },
        "recipe_drafts": recipes,
        "next_target": {
            "milestone": "M36-03",
            "task": "Deck recipe validator",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M36-02 Deck Recipe Draft Model",
        "",
        "## Summary",
        "",
        f"- Recipe drafts: `{summary['recipe_draft_count']}`",
        f"- Accepted seed recipes: `{summary['accepted_seed_recipe_count']}`",
        f"- Rejected-line recipes: `{summary['rejected_line_recipe_count']}`",
        f"- Recipes with manual card overlap: `{summary['manual_overlap_recipe_count']}`",
        f"- Recipes with slot gaps: `{summary['recipes_with_slot_gaps']}`",
        f"- Ready for M36-03: `{summary['ready_for_m36_03']}`",
        "",
        "## Drafts",
        "",
    ]
    for recipe in report["recipe_drafts"][:25]:
        slots = recipe["slot_summary"]
        lines.append(
            f"- `{recipe['recipe_id']}` skeleton=`{recipe['source_skeleton_id']}` "
            f"line=`{recipe['source_line_id']}` anchor=`{recipe['anchor_card_id']}` "
            f"cards=`{slots['explicit_card_count']}` gaps=`{slots['total_unfilled_slots']}` "
            f"status=`{recipe['review_status']}`"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Draft quantities are advisory.",
            "- M36-03 must validate deck legality and slot gaps.",
            "- No runtime deck creation.",
            "- No bot integration.",
            "- No automatic deck injection.",
            "",
            "## Next",
            "",
            "`M36-03`: Deck recipe validator.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M36-02 deck recipe draft model.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_recipe_drafts()
    json_path = args.output_dir / "m36_02_deck_recipe_draft_model.json"
    md_path = args.output_dir / "m36_02_deck_recipe_draft_model.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M36-02 deck recipe draft model wrote {json_path}")
    print(f"M36-02 deck recipe draft summary wrote {md_path}")
    print(
        "ready_for_m36_03={ready} recipes={recipes} gaps={gaps}".format(
            ready=report["summary"]["ready_for_m36_03"],
            recipes=report["summary"]["recipe_draft_count"],
            gaps=report["summary"]["recipes_with_slot_gaps"],
        )
    )
    return 0 if report["summary"]["ready_for_m36_03"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
