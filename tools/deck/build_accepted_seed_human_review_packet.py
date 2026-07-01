"""Build the M38-01 accepted seed human review packet."""

from __future__ import annotations

import argparse
import csv
import io
import json
import sys
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M37_CLOSEOUT = OUTPUT_DIR / "m37_closeout_first_runtime_ready_recipe_decision.json"
M37_05_RERUN = OUTPUT_DIR / "m37_05_revised_recipe_validation_rerun.json"
M37_02_REPAIR = OUTPUT_DIR / "m37_02_trigger_package_repair_proposal.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _review_item(closeout: dict[str, Any], rerun: dict[str, Any], repair: dict[str, Any]) -> dict[str, Any]:
    revised = closeout["key_results"]["revised_validation"]
    return {
        "review_item_id": "m38_01_recipe_003_human_acceptance",
        "item_type": "accepted_seed_recipe_review",
        "recipe_id": revised["recipe_id"],
        "source_line_id": rerun["accepted_seed_after"].get("source_line_id", "line_003"),
        "recommended_package_id": repair["summary"]["recommended_package_id"],
        "recommended_profile_id": repair["summary"]["recommended_profile_id"],
        "quantity_delta": repair["recommended_repair"]["quantity_delta"],
        "validation_status_after": revised["validation_status_after"],
        "consistency_status_after": revised["consistency_status_after"],
        "trigger_counts_after": revised["trigger_counts"],
        "grade_counts_after": revised["grade_counts"],
        "review_codes": revised["review_codes"],
        "decision_blockers": closeout["decision"]["decision_blockers"],
        "human_decision_required": True,
        "runtime_promotion_allowed": False,
        "decision_options": [
            {
                "option_id": "accept_advisory_trigger_repair_only",
                "label": "Accept trigger repair as advisory only",
                "effect": "Allows later grade-profile repair work, not runtime promotion.",
            },
            {
                "option_id": "request_grade_profile_repair",
                "label": "Request grade profile repair before acceptance",
                "effect": "Keeps recipe advisory and moves to M38-02 grade repair.",
            },
            {
                "option_id": "reject_runtime_promotion",
                "label": "Reject runtime promotion",
                "effect": "Keeps recipe out of runtime/test fixtures.",
            },
        ],
        "recommended_reviewer_action": "request_grade_profile_repair_before_runtime_acceptance",
    }


def build_review_packet(
    closeout: dict[str, Any] | None = None,
    rerun: dict[str, Any] | None = None,
    repair: dict[str, Any] | None = None,
) -> dict[str, Any]:
    closeout = closeout or load_json(M37_CLOSEOUT)
    rerun = rerun or load_json(M37_05_RERUN)
    repair = repair or load_json(M37_02_REPAIR)
    item = _review_item(closeout, rerun, repair)
    return {
        "version": "M38-01",
        "description": "Accepted seed human review packet for recipe_003",
        "selected_target": closeout.get("key_results", {}).get("revised_validation", {}),
        "source_inputs": {
            "m37_closeout": str(M37_CLOSEOUT.relative_to(ROOT)),
            "revised_recipe_validation_rerun": str(M37_05_RERUN.relative_to(ROOT)),
            "trigger_package_repair_proposal": str(M37_02_REPAIR.relative_to(ROOT)),
        },
        "scope": {
            "offline_human_review_packet": True,
            "records_human_acceptance": False,
            "changes_recipe_draft_file": False,
            "creates_runtime_deck": False,
            "bot_integration": False,
            "automatic_playbook_promotion": False,
        },
        "review_policy": {
            "human_decision_required": True,
            "runtime_promotion_allowed": False,
            "grade_profile_review_must_clear": True,
            "packet_is_not_acceptance": True,
        },
        "summary": {
            "review_item_count": 1,
            "recipe_id": item["recipe_id"],
            "quantity_delta_card_count": len(item["quantity_delta"]),
            "unresolved_review_code_count": len(item["review_codes"]),
            "decision_option_count": len(item["decision_options"]),
            "runtime_promotion_allowed": False,
            "ready_for_m38_02": True,
        },
        "review_items": [item],
        "next_target": {
            "milestone": "M38-02",
            "task": "Grade profile repair candidates",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    item = report["review_items"][0]
    lines = [
        "# M38-01 Accepted Seed Human Review Packet",
        "",
        "## Summary",
        "",
        f"- Recipe: `{item['recipe_id']}`",
        f"- Recommended package: `{item['recommended_package_id']}` / `{item['recommended_profile_id']}`",
        f"- Validation status after: `{item['validation_status_after']}`",
        f"- Consistency status after: `{item['consistency_status_after']}`",
        f"- Review codes: `{item['review_codes']}`",
        f"- Runtime promotion allowed: `{item['runtime_promotion_allowed']}`",
        f"- Ready for M38-02: `{report['summary']['ready_for_m38_02']}`",
        "",
        "## Quantity Delta",
        "",
    ]
    for delta in item["quantity_delta"]:
        lines.append(f"- `{delta['quantity']}x` `{delta['card_id']}` ({delta['trigger']}, {delta['series_code']})")
    lines.extend(["", "## Decision Options", ""])
    for option in item["decision_options"]:
        lines.append(f"- `{option['option_id']}`: {option['label']} - {option['effect']}")
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- This packet is not human acceptance.",
            "- Runtime promotion remains disabled.",
            "- Grade-profile review must clear before promotion.",
            "",
            "## Next",
            "",
            "`M38-02`: Grade profile repair candidates.",
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
            "recommended_package_id",
            "recommended_profile_id",
            "validation_status_after",
            "consistency_status_after",
            "review_codes",
            "decision_blockers",
            "recommended_reviewer_action",
        ],
    )
    writer.writeheader()
    for item in report["review_items"]:
        writer.writerow(
            {
                "review_item_id": item["review_item_id"],
                "recipe_id": item["recipe_id"],
                "recommended_package_id": item["recommended_package_id"],
                "recommended_profile_id": item["recommended_profile_id"],
                "validation_status_after": item["validation_status_after"],
                "consistency_status_after": item["consistency_status_after"],
                "review_codes": "|".join(item["review_codes"]),
                "decision_blockers": "|".join(item["decision_blockers"]),
                "recommended_reviewer_action": item["recommended_reviewer_action"],
            }
        )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(output.getvalue(), encoding="utf-8")


def parse_args(argv: list[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M38-01 accepted seed human review packet.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_review_packet()
    json_path = args.output_dir / "m38_01_accepted_seed_human_review_packet.json"
    md_path = args.output_dir / "m38_01_accepted_seed_human_review_packet.md"
    csv_path = args.output_dir / "m38_01_accepted_seed_human_review_packet.csv"
    write_json(report, json_path)
    write_markdown(report, md_path)
    write_csv(report, csv_path)
    print(f"M38-01 accepted seed human review packet wrote {json_path}")
    print(f"M38-01 accepted seed human review packet summary wrote {md_path}")
    print(f"M38-01 accepted seed human review packet CSV wrote {csv_path}")
    print(
        "ready_for_m38_02={ready} review_items={items}".format(
            ready=report["summary"]["ready_for_m38_02"],
            items=report["summary"]["review_item_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m38_02"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
