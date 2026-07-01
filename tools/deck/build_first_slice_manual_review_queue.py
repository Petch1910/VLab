"""Build manual review queue for selected first-slice semantic cards.

M35-B4 of the Hybrid Vertical-Slice Strategy.

Cards in this queue are not removed from the data, but later compatibility and
playbook tooling must treat them as review-required until resolved.
"""

from __future__ import annotations

import argparse
import csv
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
SEMANTIC_TAGS_REPORT = ROOT / "outputs" / "target_slice" / "m35_b2_first_slice_semantic_tags.json"
REQUIREMENT_PROVIDER_REPORT = (
    ROOT / "outputs" / "target_slice" / "m35_b3_first_slice_requirement_provider_model.json"
)
OUTPUT_DIR = ROOT / "outputs" / "target_slice"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def build_queue(
    semantic_report: dict[str, Any] | None = None,
    requirement_provider_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    semantic_report = semantic_report or load_json(SEMANTIC_TAGS_REPORT)
    requirement_provider_report = requirement_provider_report or load_json(REQUIREMENT_PROVIDER_REPORT)
    provider_by_id = {card["card_id"]: card for card in requirement_provider_report["cards"]}
    queue_items: list[dict[str, Any]] = []
    for card in semantic_report["cards"]:
        provider_card = provider_by_id.get(card["card_id"], {})
        reasons = sorted(set(card.get("manual_review_reasons", []) + provider_card.get("manual_review_reasons", [])))
        if not card.get("manual_review_required") and not provider_card.get("manual_review_required"):
            continue
        queue_items.append(
            {
                "card_id": card["card_id"],
                "name_th": card["name_th"],
                "series_code": card["series_code"],
                "grade": card["grade"],
                "trigger": card["trigger"],
                "type_1": card["type_1"],
                "manual_review_reasons": reasons,
                "unmapped_feature_tags": card.get("unmapped_feature_tags", []),
                "semantic_tags": card.get("semantic_tags", {}),
                "requirements": provider_card.get("requirements", []),
                "providers": provider_card.get("providers", []),
                "allowed_next_use": "manual_review_required_before_playbook_or_high_confidence_compatibility",
            }
        )
    queue_items.sort(key=lambda item: (item["series_code"], item["card_id"]))
    return {
        "version": "M35-B4",
        "description": "Manual review queue for selected first-slice semantic cards",
        "selected_target": semantic_report["selected_target"],
        "source_inputs": {
            "semantic_tags": str(SEMANTIC_TAGS_REPORT.relative_to(ROOT)),
            "requirement_provider_model": str(REQUIREMENT_PROVIDER_REPORT.relative_to(ROOT)),
        },
        "queue_policy": {
            "blocks_playbook_promotion": True,
            "blocks_high_confidence_compatibility": True,
            "does_not_remove_cards_from_dataset": True,
            "advisory_only": True,
        },
        "summary": {
            "card_count": semantic_report["summary"]["card_count"],
            "manual_review_count": len(queue_items),
            "ready_for_phase_c": True,
        },
        "manual_review_queue": queue_items,
        "excluded_from_playbook_inputs": [item["card_id"] for item in queue_items],
        "next_target": "M35-C1",
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_csv(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(
            handle,
            fieldnames=[
                "card_id",
                "name_th",
                "series_code",
                "grade",
                "trigger",
                "type_1",
                "manual_review_reasons",
                "unmapped_feature_tags",
                "requirements",
                "providers",
            ],
        )
        writer.writeheader()
        for item in report["manual_review_queue"]:
            writer.writerow(
                {
                    "card_id": item["card_id"],
                    "name_th": item["name_th"],
                    "series_code": item["series_code"],
                    "grade": item["grade"],
                    "trigger": item["trigger"],
                    "type_1": item["type_1"],
                    "manual_review_reasons": ";".join(item["manual_review_reasons"]),
                    "unmapped_feature_tags": ";".join(item["unmapped_feature_tags"]),
                    "requirements": ";".join(item["requirements"]),
                    "providers": ";".join(item["providers"]),
                }
            )


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    lines = [
        "# M35-B4 Manual Review Queue",
        "",
        "## Selected Target",
        "",
        f"- Slice: `{target['slice']}`",
        f"- Era preset: `{target['era_preset']}`",
        f"- Group: `{target['group']}`",
        "",
        "## Summary",
        "",
        f"- Cards in selected slice: `{report['summary']['card_count']}`",
        f"- Manual review cards: `{report['summary']['manual_review_count']}`",
        f"- Ready for Phase C: `{report['summary']['ready_for_phase_c']}`",
        "",
        "## Queue",
        "",
    ]
    for item in report["manual_review_queue"]:
        lines.append(
            f"- `{item['card_id']}` {item['name_th']} "
            f"reasons=`{', '.join(item['manual_review_reasons'])}` "
            f"unmapped=`{', '.join(item['unmapped_feature_tags']) or 'none'}`"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "Cards in this queue are blocked from playbook promotion and high-confidence compatibility until reviewed.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35-B4 selected-slice manual review queue.")
    parser.add_argument("--semantic-tags", type=Path, default=SEMANTIC_TAGS_REPORT)
    parser.add_argument("--requirement-provider", type=Path, default=REQUIREMENT_PROVIDER_REPORT)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    semantic_report = load_json(args.semantic_tags)
    requirement_provider_report = load_json(args.requirement_provider)
    report = build_queue(semantic_report, requirement_provider_report)
    json_path = args.output_dir / "m35_b4_first_slice_manual_review_queue.json"
    csv_path = args.output_dir / "m35_b4_first_slice_manual_review_queue.csv"
    md_path = args.output_dir / "m35_b4_first_slice_manual_review_queue.md"
    write_json(report, json_path)
    write_csv(report, csv_path)
    write_markdown(report, md_path)
    print(f"M35-B4 manual review queue wrote {json_path}")
    print(f"M35-B4 manual review CSV wrote {csv_path}")
    print(f"M35-B4 manual review summary wrote {md_path}")
    print(f"manual_review_count={report['summary']['manual_review_count']}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
