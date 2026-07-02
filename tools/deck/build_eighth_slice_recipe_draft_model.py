"""Build M64-03 eighth-slice advisory recipe drafts."""

from __future__ import annotations

import argparse
import json
import sqlite3
import sys
from collections import Counter
from contextlib import closing
from copy import deepcopy
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M64_REVIEW_PACKET = OUTPUT_DIR / "m64_02_eighth_slice_review_packet.json"
M64_SCAFFOLD = OUTPUT_DIR / "m64_01_eighth_slice_fixture_scaffold.json"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"

DEFAULT_DRAFT_LIMIT = 25
PAIR_CARD_TARGET_QTY = 4


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _selected_main_deck_cards(scaffold: dict[str, Any], sqlite_path: Path = CARDS_SQLITE) -> dict[str, dict[str, Any]]:
    fixture = scaffold["fixture_scaffold"]
    group = fixture["selected_identity"]
    series_scope = fixture["set_scope"]
    placeholders = ",".join("?" for _ in series_scope)
    query = (
        "select card_id, name_th, clan, nation, grade, trigger, deck_limit, type_1, series_code "
        "from cards where clan = ? and series_code in "
        f"({placeholders}) and coalesce(deck_limit, 4) > 0 and grade between 0 and 3"
    )
    with closing(sqlite3.connect(sqlite_path)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, [group, *series_scope]).fetchall()
    return {row["card_id"]: dict(row) for row in rows}


def _card_sort(card: dict[str, Any]) -> tuple[int, str, str]:
    grade = card.get("grade")
    return int(grade if grade is not None else 99), str(card.get("series_code", "")), str(card.get("card_id", ""))


def _is_trigger(card: dict[str, Any]) -> bool:
    return bool(card.get("trigger") or "")


def _deck_limit(card: dict[str, Any]) -> int:
    return int(card.get("deck_limit") or 4)


def _add_quantity(quantities: dict[str, int], card: dict[str, Any], requested: int) -> int:
    card_id = card["card_id"]
    current = quantities.get(card_id, 0)
    add = min(max(0, requested), max(0, _deck_limit(card) - current))
    if add:
        quantities[card_id] = current + add
    return add


def _allocate_by_pool(
    quantities: dict[str, int],
    pool: Sequence[dict[str, Any]],
    target: int,
) -> int:
    assigned = 0
    for card in sorted(pool, key=_card_sort):
        if assigned >= target:
            break
        assigned += _add_quantity(quantities, card, target - assigned)
    return assigned


def _build_base_quantities(scaffold: dict[str, Any], card_rows: dict[str, dict[str, Any]]) -> dict[str, int]:
    fixture = scaffold["fixture_scaffold"]
    trigger_profile = fixture["recommended_trigger_profile"]
    preferred_grade_profile = {str(k): int(v) for k, v in fixture["preferred_grade_profile"].items()}
    quantities: dict[str, int] = {}
    cards = list(card_rows.values())

    for trigger_type, target in trigger_profile.items():
        pool = [card for card in cards if (card.get("trigger") or "") == trigger_type]
        assigned = _allocate_by_pool(quantities, pool, int(target))
        if assigned != int(target):
            raise ValueError(f"Could not allocate trigger profile {trigger_type}: {assigned}/{target}")

    trigger_grade_counts: Counter[str] = Counter()
    for card_id, quantity in quantities.items():
        card = card_rows[card_id]
        trigger_grade_counts[str(card.get("grade"))] += quantity

    for grade, grade_target in preferred_grade_profile.items():
        remaining = grade_target - trigger_grade_counts.get(grade, 0)
        if remaining <= 0:
            continue
        pool = [
            card
            for card in cards
            if not _is_trigger(card) and str(card.get("grade")) == grade
        ]
        assigned = _allocate_by_pool(quantities, pool, remaining)
        if assigned != remaining:
            raise ValueError(f"Could not allocate grade profile G{grade}: {assigned}/{remaining}")

    return quantities


def _eligible_candidate_edges(
    review_packet: dict[str, Any],
    card_rows: dict[str, dict[str, Any]],
    draft_limit: int,
) -> tuple[list[dict[str, Any]], list[dict[str, Any]]]:
    accepted: list[dict[str, Any]] = []
    skipped: list[dict[str, Any]] = []
    for edge in review_packet.get("candidate_edge_review_items", []):
        missing = [
            card_id
            for card_id in (edge["source_card_id"], edge["target_card_id"])
            if card_id not in card_rows
        ]
        if missing:
            skipped.append(
                {
                    "edge": edge["edge"],
                    "source_card_id": edge["source_card_id"],
                    "target_card_id": edge["target_card_id"],
                    "reason": "trigger_grade4_or_missing_from_main_deck_pool",
                    "missing_or_deferred_card_ids": missing,
                }
            )
            continue
        trigger_pair_cards = [
            card_id
            for card_id in (edge["source_card_id"], edge["target_card_id"])
            if _is_trigger(card_rows[card_id])
        ]
        if trigger_pair_cards:
            skipped.append(
                {
                    "edge": edge["edge"],
                    "source_card_id": edge["source_card_id"],
                    "target_card_id": edge["target_card_id"],
                    "reason": "trigger_grade4_or_missing_from_main_deck_pool",
                    "missing_or_deferred_card_ids": trigger_pair_cards,
                }
            )
            continue
        accepted.append(edge)
        if len(accepted) >= draft_limit:
            break
    return accepted, skipped


def _force_pair_cards(
    quantities: dict[str, int],
    edge: dict[str, Any],
    card_rows: dict[str, dict[str, Any]],
) -> list[dict[str, Any]]:
    adjustments: list[dict[str, Any]] = []
    for role, card_id in (("combo_source", edge["source_card_id"]), ("combo_target", edge["target_card_id"])):
        card = card_rows[card_id]
        before = quantities.get(card_id, 0)
        target = min(PAIR_CARD_TARGET_QTY, _deck_limit(card))
        if before < target:
            quantities[card_id] = target
            adjustments.append(
                {
                    "card_id": card_id,
                    "role": role,
                    "before": before,
                    "after": target,
                    "delta": target - before,
                    "reason": "force_candidate_edge_pair_card",
                }
            )
    return adjustments


def _trim_excess(
    quantities: dict[str, int],
    protected_card_ids: set[str],
    card_rows: dict[str, dict[str, Any]],
    manual_card_ids: set[str],
    target_total: int,
) -> list[dict[str, Any]]:
    removals: list[dict[str, Any]] = []

    def total() -> int:
        return sum(quantities.values())

    def candidates() -> list[str]:
        return sorted(
            [
                card_id
                for card_id, quantity in quantities.items()
                if quantity > 0
                and card_id not in protected_card_ids
                and not _is_trigger(card_rows[card_id])
            ],
            key=lambda card_id: (
                card_id not in manual_card_ids,
                int(card_rows[card_id].get("grade") if card_rows[card_id].get("grade") is not None else 99),
                card_id,
            ),
        )

    while total() > target_total:
        changed = False
        for card_id in candidates():
            if total() <= target_total:
                break
            before = quantities[card_id]
            quantities[card_id] = before - 1
            removals.append(
                {
                    "card_id": card_id,
                    "before": before,
                    "after": quantities[card_id],
                    "delta": -1,
                    "reason": "trim_non_trigger_scaffold_filler",
                }
            )
            if quantities[card_id] <= 0:
                del quantities[card_id]
            changed = True
        if not changed:
            break
    return removals


def _roles_for(card_id: str, edge: dict[str, Any], base_quantities: dict[str, int]) -> list[str]:
    roles: list[str] = []
    if card_id == edge["source_card_id"]:
        roles.append("combo_source")
    if card_id == edge["target_card_id"]:
        roles.append("combo_target")
    if card_id in base_quantities:
        roles.append("fixture_scaffold")
    if not roles:
        roles.append("candidate_pair_addition")
    return roles


def _quantity_rows(
    quantities: dict[str, int],
    edge: dict[str, Any],
    base_quantities: dict[str, int],
    card_rows: dict[str, dict[str, Any]],
) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    for card_id in sorted(quantities):
        card = card_rows[card_id]
        rows.append(
            {
                "card_id": card_id,
                "quantity": quantities[card_id],
                "roles": _roles_for(card_id, edge, base_quantities),
                "name_th": card.get("name_th", ""),
                "grade": card.get("grade"),
                "trigger": card.get("trigger") or "",
                "type_1": card.get("type_1", ""),
                "quantity_source": "m64_03_fixture_scaffold_plus_candidate_edge_pair",
            }
        )
    return rows


def _slot_summary(rows: list[dict[str, Any]], target_total: int) -> dict[str, Any]:
    grade_counts: Counter[str] = Counter()
    trigger_counts: Counter[str] = Counter()
    explicit_total = 0
    for row in rows:
        qty = int(row["quantity"])
        explicit_total += qty
        grade = row.get("grade")
        grade_counts[str(grade) if grade is not None else "unknown"] += qty
        trigger = row.get("trigger") or ""
        if trigger:
            trigger_counts[trigger] += qty
    return {
        "main_deck_target": target_total,
        "explicit_card_count": explicit_total,
        "total_unfilled_slots": max(0, target_total - explicit_total),
        "main_deck_delta": target_total - explicit_total,
        "trigger_count": sum(trigger_counts.values()),
        "trigger_counts": dict(sorted(trigger_counts.items())),
        "grade_counts": dict(sorted(grade_counts.items())),
        "max_copy_limit_from_sqlite": True,
        "grade4_main_deck_count": grade_counts.get("4", 0),
    }


def _build_recipe(
    index: int,
    edge: dict[str, Any],
    base_quantities: dict[str, int],
    card_rows: dict[str, dict[str, Any]],
    manual_card_ids: set[str],
    target_total: int,
) -> dict[str, Any]:
    quantities = deepcopy(base_quantities)
    protected = {edge["source_card_id"], edge["target_card_id"]}
    forced_adjustments = _force_pair_cards(quantities, edge, card_rows)
    trim_adjustments = _trim_excess(quantities, protected, card_rows, manual_card_ids, target_total)
    rows = _quantity_rows(quantities, edge, base_quantities, card_rows)
    slot_summary = _slot_summary(rows, target_total)
    manual_overlap = sorted({row["card_id"] for row in rows if row["card_id"] in manual_card_ids})
    review_blockers = [
        "human_recipe_selection",
        "m64_04_recipe_validator",
        "m64_05_combo_recipe_consistency_check",
        "lock_legion_manual_review_policy",
        "grade4_format_support_decision",
    ]
    if manual_overlap:
        review_blockers.append("manual_card_semantic_review")
    return {
        "recipe_id": f"m64_recipe_{index:03d}",
        "source_candidate_edge": edge["edge"],
        "source_edge_rank": edge["edge_rank"],
        "source_line_id": edge["edge"],
        "source_package_id": "m64_03_pair_anchored_fixture_scaffold",
        "anchor_card_id": edge["source_card_id"],
        "anchor_name_th": edge.get("source_name_th", ""),
        "pair": {
            "source_card_id": edge["source_card_id"],
            "source_name_th": edge.get("source_name_th", ""),
            "target_card_id": edge["target_card_id"],
            "target_name_th": edge.get("target_name_th", ""),
            "net_score": edge.get("net_score", 0),
            "resource_verdict": edge.get("resource_verdict", ""),
            "timing_verdict": edge.get("timing_verdict", ""),
            "zone_verdict": edge.get("zone_verdict", ""),
            "synergy_reasons": edge.get("synergy_reasons", []),
        },
        "review_status": (
            "advisory_pair_draft_manual_card_overlap"
            if manual_overlap
            else "advisory_pair_draft_pending_validation"
        ),
        "review_blockers": review_blockers,
        "recipe_status": (
            "draft_quantity_complete"
            if slot_summary["explicit_card_count"] == target_total and slot_summary["grade4_main_deck_count"] == 0
            else "draft_quantity_mismatch_or_grade4_deferred"
        ),
        "quantities": rows,
        "quantity_adjustments": forced_adjustments + trim_adjustments,
        "slot_summary": slot_summary,
        "validation_metadata": {
            "draft_only": True,
            "requires_m64_04_validator": True,
            "requires_human_selection": True,
            "contains_manual_review_cards": bool(manual_overlap),
            "manual_review_card_ids": manual_overlap,
            "grade4_cards_excluded_from_main_deck": True,
            "lock_runtime_support_deferred": True,
            "legion_runtime_support_deferred": True,
            "not_runtime_deck": True,
            "not_saved_deck": True,
            "not_ui_published": True,
            "not_bot_playbook": True,
            "not_auto_injected": True,
        },
    }


def build_eighth_slice_recipe_drafts(
    review_packet: dict[str, Any] | None = None,
    scaffold: dict[str, Any] | None = None,
    draft_limit: int = DEFAULT_DRAFT_LIMIT,
) -> dict[str, Any]:
    review_packet = review_packet or load_json(M64_REVIEW_PACKET)
    scaffold = scaffold or load_json(M64_SCAFFOLD)
    if not review_packet.get("summary", {}).get("ready_for_m64_03"):
        raise ValueError("M64-02 review packet is not ready for M64-03.")
    if not scaffold.get("summary", {}).get("ready_for_m64_02"):
        raise ValueError("M64-01 fixture scaffold is not ready for M64-03.")
    card_rows = _selected_main_deck_cards(scaffold)
    base = _build_base_quantities(scaffold, card_rows)
    manual_card_ids = {item["card_id"] for item in review_packet.get("manual_card_review_items", [])}
    candidate_edges, skipped_edges = _eligible_candidate_edges(review_packet, card_rows, draft_limit)
    target_total = int(scaffold["fixture_scaffold"]["main_deck_exact"])
    recipes = [
        _build_recipe(index, edge, base, card_rows, manual_card_ids, target_total)
        for index, edge in enumerate(candidate_edges, start=1)
    ]
    quantity_complete_count = sum(1 for item in recipes if item["recipe_status"] == "draft_quantity_complete")
    return {
        "version": "M64-03",
        "description": "Eighth-slice advisory recipe draft model from M64-02 candidate edges",
        "selected_target": review_packet.get("selected_target", {}),
        "source_inputs": {
            "eighth_slice_review_packet": str(M64_REVIEW_PACKET.relative_to(ROOT)),
            "eighth_slice_fixture_scaffold": str(M64_SCAFFOLD.relative_to(ROOT)),
            "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
        },
        "scope": {
            "offline_recipe_draft_model": True,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "runtime_deck": False,
            "bot_playbook": False,
            "automatic_deck_injection": False,
            "direct_GameState_mutation": False,
        },
        "draft_policy": {
            "pair_anchored_from_m64_02_candidate_edges": True,
            "fixture_scaffold_source": "m64_01_eighth_slice_fixture_scaffold",
            "draft_limit": draft_limit,
            "card_quantities_are_advisory": True,
            "copy_limit_from_sqlite_deck_limit": True,
            "human_selection_required_before_runtime": True,
            "grade4_cards_excluded_until_format_support": True,
            "lock_legion_text_requires_manual_review": True,
            "no_live_card_text_parsing": True,
            "no_direct_GameState_mutation": True,
        },
        "summary": {
            "candidate_edge_input_count": review_packet.get("summary", {}).get("candidate_edge_item_count", 0),
            "candidate_edges_skipped_for_trigger_grade4_or_missing": len(skipped_edges),
            "recipe_draft_count": len(recipes),
            "quantity_complete_recipe_count": quantity_complete_count,
            "manual_overlap_recipe_count": sum(
                1 for item in recipes if item["validation_metadata"]["contains_manual_review_cards"]
            ),
            "fixture_scaffold_card_count": len(base),
            "fixture_scaffold_total_cards": sum(base.values()),
            "ready_for_m64_04": bool(recipes) and quantity_complete_count == len(recipes),
        },
        "base_fixture_quantities": [
            {
                "card_id": card_id,
                "quantity": quantity,
                "name_th": card_rows[card_id].get("name_th", ""),
                "grade": card_rows[card_id].get("grade"),
                "trigger": card_rows[card_id].get("trigger") or "",
            }
            for card_id, quantity in sorted(base.items())
        ],
        "skipped_candidate_edges": skipped_edges,
        "recipe_drafts": recipes,
        "next_target": {
            "milestone": "M64-04",
            "task": "Eighth-slice recipe validator",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M64-03 Eighth-Slice Recipe Draft Model",
        "",
        "## Summary",
        "",
        f"- Candidate edge input count: `{summary['candidate_edge_input_count']}`",
        f"- Candidate edges skipped for trigger/Grade 4/missing: `{summary['candidate_edges_skipped_for_trigger_grade4_or_missing']}`",
        f"- Recipe drafts: `{summary['recipe_draft_count']}`",
        f"- Quantity-complete recipes: `{summary['quantity_complete_recipe_count']}`",
        f"- Recipes with manual card overlap: `{summary['manual_overlap_recipe_count']}`",
        f"- Fixture scaffold cards: `{summary['fixture_scaffold_card_count']}`",
        f"- Fixture scaffold total cards: `{summary['fixture_scaffold_total_cards']}`",
        f"- Ready for M64-04: `{summary['ready_for_m64_04']}`",
        "",
        "## Drafts",
        "",
    ]
    for recipe in report["recipe_drafts"]:
        slots = recipe["slot_summary"]
        lines.append(
            f"- `{recipe['recipe_id']}` edge=`{recipe['source_candidate_edge']}` "
            f"cards=`{slots['explicit_card_count']}` triggers=`{slots['trigger_count']}` "
            f"status=`{recipe['review_status']}`"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Draft quantities are advisory.",
            "- M64-04 must validate deck legality and manual-review blockers.",
            "- Grade 4 format support remains deferred.",
            "- Lock/Unlock and Legion/Mate text remains manual-review only.",
            "- No runtime deck creation.",
            "- No saved deck or UI publication.",
            "- No bot playbook promotion.",
            "- No automatic deck injection.",
            "",
            "## Next",
            "",
            "`M64-04`: Eighth-slice recipe validator.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M64-03 eighth-slice recipe draft model.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    parser.add_argument("--draft-limit", type=int, default=DEFAULT_DRAFT_LIMIT)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_eighth_slice_recipe_drafts(draft_limit=args.draft_limit)
    json_path = args.output_dir / "m64_03_eighth_slice_recipe_draft_model.json"
    md_path = args.output_dir / "m64_03_eighth_slice_recipe_draft_model.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M64-03 recipe draft model wrote {json_path}")
    print(f"M64-03 recipe draft model summary wrote {md_path}")
    print(
        "ready={ready} drafts={drafts} complete={complete} skipped={skipped}".format(
            ready=report["summary"]["ready_for_m64_04"],
            drafts=report["summary"]["recipe_draft_count"],
            complete=report["summary"]["quantity_complete_recipe_count"],
            skipped=report["summary"]["candidate_edges_skipped_for_trigger_grade4_or_missing"],
        )
    )
    return 0 if report["summary"]["ready_for_m64_04"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
