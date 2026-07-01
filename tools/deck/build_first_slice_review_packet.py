"""Build the M36-01 first-slice review packet.

The packet combines the accepted advisory seed, rejected combo lines, and
manual-review cards so a human reviewer can decide what is safe to turn into
deck recipe drafts later.
"""

from __future__ import annotations

import argparse
import csv
import json
import sys
from pathlib import Path
from typing import Any, Iterable, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
B4_MANUAL_REVIEW = OUTPUT_DIR / "m35_b4_first_slice_manual_review_queue.json"
D4_REVIEWED_SEED = OUTPUT_DIR / "m35_d4_first_slice_reviewed_playbook_seed.json"
M35_CLOSEOUT = OUTPUT_DIR / "m35_closeout_hybrid_vertical_slice.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _accepted_seed_item(seed: dict[str, Any]) -> dict[str, Any]:
    return {
        "item_type": "accepted_seed",
        "item_id": seed["seed_id"],
        "source_line_id": seed["source_line_id"],
        "anchor_card_id": seed["anchor_card_id"],
        "anchor_name_th": seed.get("anchor_name_th", ""),
        "review_priority": "P1_confirm_before_recipe",
        "review_reasons": [
            "accepted_by_static_review",
            "requires_human_acceptance_before_runtime_or_recipe_promotion",
        ],
        "review_questions": [
            "Does the seed line make sense as a human-playable Nova Grappler plan?",
            "Are all involved cards acceptable for a draft deck recipe?",
            "Does any step rely on unsupported or manually reviewed card text?",
        ],
        "cards_involved": seed.get("cards_involved", []),
        "blocked_until": [
            "human_acceptance",
            "deck_recipe_validator",
            "combo_line_to_recipe_consistency_check",
        ],
        "recommended_next_action": "confirm_or_demote_before_m36_02_recipe_draft",
    }


def _rejected_line_item(rejection: dict[str, Any]) -> dict[str, Any]:
    return {
        "item_type": "rejected_line",
        "item_id": rejection["source_line_id"],
        "source_skeleton_id": rejection.get("source_skeleton_id", ""),
        "source_package_id": rejection.get("source_package_id", ""),
        "anchor_card_id": rejection.get("anchor_card_id", ""),
        "review_priority": "P2_support_gap_review",
        "review_reasons": rejection.get("review_reasons", []),
        "needs_to_work": rejection.get("needs_to_work", []),
        "blocked_until": [
            "support_gap_resolved",
            "human_acceptance",
            "deck_recipe_validator",
        ],
        "recommended_next_action": "resolve_support_gap_or_keep_rejected",
    }


def _manual_card_item(card: dict[str, Any]) -> dict[str, Any]:
    return {
        "item_type": "manual_review_card",
        "item_id": card["card_id"],
        "card_id": card["card_id"],
        "name_th": card.get("name_th", ""),
        "series_code": card.get("series_code", ""),
        "grade": card.get("grade", ""),
        "type_1": card.get("type_1", ""),
        "trigger": card.get("trigger", ""),
        "review_priority": "P1_semantic_gap_review",
        "review_reasons": card.get("manual_review_reasons", []),
        "unmapped_feature_tags": card.get("unmapped_feature_tags", []),
        "requirements": card.get("requirements", []),
        "providers": card.get("providers", []),
        "blocked_until": [
            "semantic_mapping_reviewed",
            "fixture_added_if_needed",
            "human_acceptance",
        ],
        "recommended_next_action": "map_or_defer_unmapped_feature_tags",
    }


def _csv_rows(report: dict[str, Any]) -> Iterable[dict[str, str]]:
    for section in ("accepted_seed_review_items", "rejected_line_review_items", "manual_card_review_items"):
        for item in report[section]:
            yield {
                "item_type": item["item_type"],
                "item_id": item["item_id"],
                "anchor_or_card_id": item.get("anchor_card_id") or item.get("card_id", ""),
                "review_priority": item["review_priority"],
                "review_reasons": ";".join(item.get("review_reasons", [])),
                "blocked_until": ";".join(item.get("blocked_until", [])),
                "recommended_next_action": item["recommended_next_action"],
            }


def build_review_packet(
    b4_report: dict[str, Any] | None = None,
    d4_report: dict[str, Any] | None = None,
    closeout_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    b4_report = b4_report or load_json(B4_MANUAL_REVIEW)
    d4_report = d4_report or load_json(D4_REVIEWED_SEED)
    closeout_report = closeout_report or load_json(M35_CLOSEOUT)

    accepted = [_accepted_seed_item(seed) for seed in d4_report.get("playbook_seed_entries", [])]
    rejected = [_rejected_line_item(item) for item in d4_report.get("review_rejections", [])]
    manual_cards = [_manual_card_item(card) for card in b4_report.get("manual_review_queue", [])]
    next_queue = closeout_report.get("next_queue_selection", {})
    ready = (
        next_queue.get("milestone") == "M36"
        and len(accepted) == d4_report.get("summary", {}).get("seed_entry_count")
        and len(rejected) == d4_report.get("summary", {}).get("rejected_line_count")
        and len(manual_cards) == b4_report.get("summary", {}).get("manual_review_count")
    )

    return {
        "version": "M36-01",
        "description": "First-slice human review packet before deck recipe drafting",
        "selected_target": d4_report.get("selected_target", {}),
        "source_inputs": {
            "manual_review_queue": str(B4_MANUAL_REVIEW.relative_to(ROOT)),
            "reviewed_playbook_seed": str(D4_REVIEWED_SEED.relative_to(ROOT)),
            "m35_closeout": str(M35_CLOSEOUT.relative_to(ROOT)),
        },
        "scope": {
            "offline_review_packet": True,
            "runtime_playbook": False,
            "bot_integration": False,
            "deck_recipe_draft": False,
            "automatic_deck_injection": False,
        },
        "review_policy": {
            "human_acceptance_required": True,
            "manual_review_cards_blocked_from_recipe_until_resolved": True,
            "rejected_lines_blocked_until_support_gap_resolved": True,
            "accepted_seed_still_requires_recipe_validation": True,
            "no_live_card_text_parsing": True,
            "no_direct_GameState_mutation": True,
        },
        "summary": {
            "accepted_seed_item_count": len(accepted),
            "rejected_line_item_count": len(rejected),
            "manual_card_item_count": len(manual_cards),
            "total_review_item_count": len(accepted) + len(rejected) + len(manual_cards),
            "ready_for_m36_02": ready,
        },
        "accepted_seed_review_items": accepted,
        "rejected_line_review_items": rejected,
        "manual_card_review_items": manual_cards,
        "next_target": {
            "milestone": "M36-02",
            "task": "Deck recipe draft model",
            "blocked_until": [
                "M36-01 review packet is available",
                "human/team can inspect accepted seed, rejected lines, and manual-review cards",
            ],
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_csv(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    rows = list(_csv_rows(report))
    fieldnames = [
        "item_type",
        "item_id",
        "anchor_or_card_id",
        "review_priority",
        "review_reasons",
        "blocked_until",
        "recommended_next_action",
    ]
    with path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M36-01 First-Slice Review Packet",
        "",
        "## Summary",
        "",
        f"- Accepted seed items: `{summary['accepted_seed_item_count']}`",
        f"- Rejected line items: `{summary['rejected_line_item_count']}`",
        f"- Manual-review card items: `{summary['manual_card_item_count']}`",
        f"- Total review items: `{summary['total_review_item_count']}`",
        f"- Ready for M36-02: `{summary['ready_for_m36_02']}`",
        "",
        "## Accepted Seed Items",
        "",
    ]
    for item in report["accepted_seed_review_items"]:
        lines.append(
            f"- `{item['item_id']}` line=`{item['source_line_id']}` "
            f"anchor=`{item['anchor_card_id']}` action=`{item['recommended_next_action']}`"
        )
    lines.extend(["", "## Rejected Line Items", ""])
    for item in report["rejected_line_review_items"]:
        lines.append(
            f"- `{item['item_id']}` anchor=`{item['anchor_card_id']}` "
            f"reasons=`{','.join(item['review_reasons'])}` action=`{item['recommended_next_action']}`"
        )
    lines.extend(["", "## Manual-Review Card Items", ""])
    for item in report["manual_card_review_items"]:
        lines.append(
            f"- `{item['card_id']}` {item['name_th']} "
            f"unmapped=`{','.join(item['unmapped_feature_tags'])}` action=`{item['recommended_next_action']}`"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- This is an offline review packet only.",
            "- No runtime bot wiring.",
            "- No live card text parsing.",
            "- No direct `GameState` mutation.",
            "- No automatic deck injection.",
            "",
            "## Next",
            "",
            "`M36-02`: Deck recipe draft model.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M36-01 first-slice review packet.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_review_packet()
    json_path = args.output_dir / "m36_01_first_slice_review_packet.json"
    md_path = args.output_dir / "m36_01_first_slice_review_packet.md"
    csv_path = args.output_dir / "m36_01_first_slice_review_packet.csv"
    write_json(report, json_path)
    write_markdown(report, md_path)
    write_csv(report, csv_path)
    print(f"M36-01 review packet wrote {json_path}")
    print(f"M36-01 review packet summary wrote {md_path}")
    print(f"M36-01 review packet CSV wrote {csv_path}")
    print(
        "ready_for_m36_02={ready} review_items={items}".format(
            ready=report["summary"]["ready_for_m36_02"],
            items=report["summary"]["total_review_item_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m36_02"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
