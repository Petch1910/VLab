"""Build M40-02 second-slice advisory recipe drafts.

This turns the M40-01 Oracle Think Tank review packet into bounded, reviewable
recipe drafts. The drafts are pair-anchored and fixture-scaffolded: each one
starts from the valid M35-E2 second-slice fixture, forces a candidate edge pair
into the list, and trims non-trigger filler cards to keep a 50-card main deck.

The output is still advisory only. It does not create saved decks, publish UI
deck entries, promote runtime fixtures, enable bot playbooks, or mutate
GameState.
"""

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
M40_REVIEW_PACKET = OUTPUT_DIR / "m40_01_second_slice_review_packet.json"
SECOND_SLICE_READINESS = OUTPUT_DIR / "m35_e2_second_slice_fixture_readiness.json"
CARDS_SQLITE = ROOT / "data" / "packs" / "vanguard_th" / "cards.sqlite"

DEFAULT_DRAFT_LIMIT = 25
MAIN_DECK_TARGET = 50
PAIR_CARD_TARGET_QTY = 4


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def load_card_rows(card_ids: Sequence[str], sqlite_path: Path = CARDS_SQLITE) -> dict[str, dict[str, Any]]:
    if not sqlite_path.exists():
        raise FileNotFoundError(f"Required SQLite pack not found: {sqlite_path}")
    ids = sorted(set(card_ids))
    if not ids:
        return {}
    placeholders = ",".join("?" for _ in ids)
    query = (
        "select card_id, name_th, clan, nation, grade, trigger, deck_limit, type_1, series_code "
        f"from cards where card_id in ({placeholders})"
    )
    with closing(sqlite3.connect(sqlite_path)) as connection:
        connection.row_factory = sqlite3.Row
        rows = connection.execute(query, ids).fetchall()
    return {row["card_id"]: dict(row) for row in rows}


def _valid_fixture(readiness_report: dict[str, Any]) -> dict[str, Any]:
    for fixture in readiness_report.get("fixtures", []):
        if fixture.get("expected") == "pass" and fixture.get("validation", {}).get("accepted"):
            return fixture
    raise ValueError("No accepted pass fixture found in second-slice readiness report.")


def _base_quantities(valid_fixture: dict[str, Any]) -> dict[str, int]:
    quantities: dict[str, int] = {}
    for card in valid_fixture.get("cards", []):
        card_id = card["card_id"]
        quantities[card_id] = quantities.get(card_id, 0) + int(card.get("count", 0))
    return {card_id: qty for card_id, qty in quantities.items() if qty > 0}


def _all_needed_card_ids(packet: dict[str, Any], valid_fixture: dict[str, Any], limit: int) -> list[str]:
    ids = {card["card_id"] for card in valid_fixture.get("cards", [])}
    for edge in packet.get("candidate_edge_review_items", [])[:limit]:
        ids.add(edge["source_card_id"])
        ids.add(edge["target_card_id"])
    return sorted(ids)


def _is_trigger(card_id: str, card_rows: dict[str, dict[str, Any]]) -> bool:
    return bool(card_rows.get(card_id, {}).get("trigger") or "")


def _deck_limit(card_id: str, card_rows: dict[str, dict[str, Any]]) -> int:
    value = card_rows.get(card_id, {}).get("deck_limit")
    return int(value or 4)


def _force_pair_cards(
    quantities: dict[str, int],
    edge: dict[str, Any],
    card_rows: dict[str, dict[str, Any]],
) -> list[dict[str, Any]]:
    adjustments: list[dict[str, Any]] = []
    for role, card_id in (("combo_source", edge["source_card_id"]), ("combo_target", edge["target_card_id"])):
        before = quantities.get(card_id, 0)
        target = min(PAIR_CARD_TARGET_QTY, _deck_limit(card_id, card_rows))
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
    target_total: int = MAIN_DECK_TARGET,
) -> list[dict[str, Any]]:
    removals: list[dict[str, Any]] = []

    def total() -> int:
        return sum(quantities.values())

    def candidates() -> list[str]:
        return sorted(
            [
                card_id
                for card_id, quantity in quantities.items()
                if quantity > 0 and card_id not in protected_card_ids and not _is_trigger(card_id, card_rows)
            ],
            key=lambda card_id: (
                card_id not in manual_card_ids,
                card_rows.get(card_id, {}).get("grade") not in (0, "0"),
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
                    "reason": "trim_non_trigger_fixture_filler",
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
        card = card_rows.get(card_id, {})
        rows.append(
            {
                "card_id": card_id,
                "quantity": quantities[card_id],
                "roles": _roles_for(card_id, edge, base_quantities),
                "name_th": card.get("name_th", ""),
                "grade": card.get("grade"),
                "trigger": card.get("trigger") or "",
                "type_1": card.get("type_1", ""),
                "quantity_source": "m40_02_fixture_scaffold_plus_candidate_edge_pair",
            }
        )
    return rows


def _slot_summary(rows: list[dict[str, Any]]) -> dict[str, Any]:
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
        "main_deck_target": MAIN_DECK_TARGET,
        "explicit_card_count": explicit_total,
        "total_unfilled_slots": max(0, MAIN_DECK_TARGET - explicit_total),
        "main_deck_delta": MAIN_DECK_TARGET - explicit_total,
        "trigger_count": sum(trigger_counts.values()),
        "trigger_counts": dict(sorted(trigger_counts.items())),
        "grade_counts": dict(sorted(grade_counts.items())),
        "max_copy_limit_from_sqlite": True,
    }


def _build_recipe(
    index: int,
    edge: dict[str, Any],
    base_quantities: dict[str, int],
    card_rows: dict[str, dict[str, Any]],
    manual_card_ids: set[str],
) -> dict[str, Any]:
    quantities = deepcopy(base_quantities)
    protected = {edge["source_card_id"], edge["target_card_id"]}
    forced_adjustments = _force_pair_cards(quantities, edge, card_rows)
    trim_adjustments = _trim_excess(quantities, protected, card_rows, manual_card_ids)
    rows = _quantity_rows(quantities, edge, base_quantities, card_rows)
    slot_summary = _slot_summary(rows)
    manual_overlap = sorted({row["card_id"] for row in rows if row["card_id"] in manual_card_ids})
    review_blockers = [
        "human_recipe_selection",
        "m40_03_recipe_validator",
        "m40_04_combo_recipe_consistency_check",
    ]
    if manual_overlap:
        review_blockers.append("manual_card_semantic_review")
    return {
        "recipe_id": f"m40_recipe_{index:03d}",
        "source_candidate_edge": edge["edge"],
        "source_edge_rank": edge["edge_rank"],
        "source_line_id": edge["edge"],
        "source_skeleton_id": "",
        "source_package_id": "m40_02_pair_anchored_fixture_scaffold",
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
            if slot_summary["explicit_card_count"] == MAIN_DECK_TARGET
            else "draft_quantity_mismatch"
        ),
        "quantities": rows,
        "quantity_adjustments": forced_adjustments + trim_adjustments,
        "slot_summary": slot_summary,
        "validation_metadata": {
            "draft_only": True,
            "requires_m40_03_validator": True,
            "requires_human_selection": True,
            "contains_manual_review_cards": bool(manual_overlap),
            "manual_review_card_ids": manual_overlap,
            "not_runtime_deck": True,
            "not_saved_deck": True,
            "not_ui_published": True,
            "not_bot_playbook": True,
            "not_auto_injected": True,
        },
    }


def build_second_slice_recipe_drafts(
    review_packet: dict[str, Any] | None = None,
    readiness_report: dict[str, Any] | None = None,
    draft_limit: int = DEFAULT_DRAFT_LIMIT,
) -> dict[str, Any]:
    review_packet = review_packet or load_json(M40_REVIEW_PACKET)
    readiness_report = readiness_report or load_json(SECOND_SLICE_READINESS)
    if not review_packet.get("summary", {}).get("ready_for_m40_02"):
        raise ValueError("M40-01 review packet is not ready for M40-02.")
    valid_fixture = _valid_fixture(readiness_report)
    candidate_edges = review_packet.get("candidate_edge_review_items", [])[:draft_limit]
    card_rows = load_card_rows(_all_needed_card_ids(review_packet, valid_fixture, draft_limit))
    base = _base_quantities(valid_fixture)
    manual_card_ids = {item["card_id"] for item in review_packet.get("manual_card_review_items", [])}
    recipes = [
        _build_recipe(index, edge, base, card_rows, manual_card_ids)
        for index, edge in enumerate(candidate_edges, start=1)
    ]
    quantity_complete_count = sum(1 for item in recipes if item["recipe_status"] == "draft_quantity_complete")
    return {
        "version": "M40-02",
        "description": "Second-slice advisory recipe draft model from M40-01 candidate edges",
        "selected_target": review_packet.get("selected_target", {}),
        "source_inputs": {
            "second_slice_review_packet": str(M40_REVIEW_PACKET.relative_to(ROOT)),
            "second_slice_fixture_readiness": str(SECOND_SLICE_READINESS.relative_to(ROOT)),
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
            "pair_anchored_from_m40_01_candidate_edges": True,
            "fixture_scaffold_source": valid_fixture["fixture_id"],
            "draft_limit": draft_limit,
            "card_quantities_are_advisory": True,
            "copy_limit_from_sqlite_deck_limit": True,
            "human_selection_required_before_runtime": True,
            "no_live_card_text_parsing": True,
            "no_direct_GameState_mutation": True,
        },
        "summary": {
            "candidate_edge_input_count": review_packet.get("summary", {}).get("candidate_edge_item_count", 0),
            "recipe_draft_count": len(recipes),
            "quantity_complete_recipe_count": quantity_complete_count,
            "manual_overlap_recipe_count": sum(
                1 for item in recipes if item["validation_metadata"]["contains_manual_review_cards"]
            ),
            "fixture_scaffold_card_count": len(base),
            "fixture_scaffold_total_cards": sum(base.values()),
            "ready_for_m40_03": bool(recipes) and quantity_complete_count == len(recipes),
        },
        "recipe_drafts": recipes,
        "next_target": {
            "milestone": "M40-03",
            "task": "Second-slice recipe validator",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    target = report["selected_target"]
    lines = [
        "# M40-02 Second-Slice Recipe Draft Model",
        "",
        "## Summary",
        "",
        f"- Target slice: `{target.get('slice', '')}` / `{target.get('group', '')}`",
        f"- Candidate edge inputs: `{summary['candidate_edge_input_count']}`",
        f"- Recipe drafts: `{summary['recipe_draft_count']}`",
        f"- Quantity-complete drafts: `{summary['quantity_complete_recipe_count']}`",
        f"- Drafts with manual-review card overlap: `{summary['manual_overlap_recipe_count']}`",
        f"- Fixture scaffold cards: `{summary['fixture_scaffold_card_count']}`",
        f"- Ready for M40-03: `{summary['ready_for_m40_03']}`",
        "",
        "## Drafts",
        "",
    ]
    for recipe in report["recipe_drafts"]:
        pair = recipe["pair"]
        slots = recipe["slot_summary"]
        lines.append(
            f"- `{recipe['recipe_id']}` edge=`{recipe['source_candidate_edge']}` "
            f"score=`{pair['net_score']}` cards=`{slots['explicit_card_count']}` "
            f"triggers=`{slots['trigger_count']}` status=`{recipe['review_status']}`"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Draft quantities are advisory.",
            "- Drafts are pair-anchored from M40-01 candidate edges.",
            "- Drafts use the M35-E2 accepted fixture as a scaffold.",
            "- M40-03 must validate counts, copy limits, clan identity, trigger profile, and grade profile.",
            "- No saved-deck injection, UI publication, runtime promotion, or bot/playbook promotion.",
            "- No live card text parsing.",
            "- No direct `GameState` mutation.",
            "",
            "## Next",
            "",
            "`M40-03`: Second-slice recipe validator.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M40-02 second-slice recipe draft model.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    parser.add_argument("--draft-limit", type=int, default=DEFAULT_DRAFT_LIMIT)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_second_slice_recipe_drafts(draft_limit=args.draft_limit)
    json_path = args.output_dir / "m40_02_second_slice_recipe_draft_model.json"
    md_path = args.output_dir / "m40_02_second_slice_recipe_draft_model.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M40-02 second-slice recipe draft model wrote {json_path}")
    print(f"M40-02 second-slice recipe draft summary wrote {md_path}")
    print(
        "ready_for_m40_03={ready} recipes={recipes} quantity_complete={complete}".format(
            ready=report["summary"]["ready_for_m40_03"],
            recipes=report["summary"]["recipe_draft_count"],
            complete=report["summary"]["quantity_complete_recipe_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m40_03"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
