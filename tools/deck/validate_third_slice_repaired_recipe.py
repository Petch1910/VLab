"""Validate the M45-02 third-slice repaired recipe preview for M45-03."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M45_02_ACCEPTED_REPAIR = OUTPUT_DIR / "m45_02_third_slice_human_accepted_repair_artifact.json"

if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.validate_third_slice_recipe_drafts import (  # noqa: E402
    _all_card_ids,
    build_third_slice_validation_report,
    load_card_rows,
)


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _recipe_report_from_accepted_repair(artifact: dict[str, Any]) -> dict[str, Any]:
    repair = artifact["accepted_repair"]
    return {
        "selected_target": artifact.get("selected_target", {}),
        "recipe_drafts": [
            {
                "recipe_id": repair["recipe_id"],
                "source_candidate_edge": repair.get("source_candidate_edge", ""),
                "source_edge_rank": 1,
                "anchor_card_id": repair.get("pair", {}).get("source_card_id", ""),
                "review_status": "human_accepted_repair_preview",
                "quantities": repair.get("repaired_quantities", []),
                "slot_summary": {
                    "main_deck_target": 50,
                    "total_unfilled_slots": 0,
                },
                "validation_metadata": {
                    "manual_review_card_ids": [],
                    "accepted_review_item_id": artifact.get("summary", {}).get("accepted_review_item_id", ""),
                    "accepted_combined_repair_package_id": artifact.get("summary", {}).get(
                        "accepted_combined_repair_package_id", ""
                    ),
                },
                "review_blockers": [],
                "pair": repair.get("pair", {}),
            }
        ],
    }


def build_third_slice_repaired_validation_report(
    accepted_repair_artifact: dict[str, Any] | None = None,
) -> dict[str, Any]:
    accepted_repair_artifact = accepted_repair_artifact or load_json(M45_02_ACCEPTED_REPAIR)
    recipe_report = _recipe_report_from_accepted_repair(accepted_repair_artifact)
    card_rows = load_card_rows(_all_card_ids(recipe_report))
    base_report = build_third_slice_validation_report(recipe_report, card_rows)
    validation = base_report["recipe_validations"][0] if base_report["recipe_validations"] else {}
    ready_for_m45_04 = (
        bool(accepted_repair_artifact.get("summary", {}).get("human_acceptance_recorded"))
        and bool(accepted_repair_artifact.get("summary", {}).get("ready_for_m45_03"))
        and validation.get("validation_status") == "validator_passed"
        and bool(validation.get("runtime_ready"))
    )

    return {
        "version": "M45-03",
        "description": "Third-slice repaired recipe validation rerun",
        "selected_target": accepted_repair_artifact.get("selected_target", {}),
        "source_inputs": {
            "human_accepted_repair_artifact": str(M45_02_ACCEPTED_REPAIR.relative_to(ROOT)),
            "runtime_cards_sqlite": "data/packs/vanguard_th/cards.sqlite",
        },
        "scope": {
            "offline_validation_rerun": True,
            "records_human_acceptance": False,
            "changes_accepted_repair_artifact": False,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "automatic_deck_injection": False,
            "direct_GameState_mutation": False,
        },
        "validation_policy": {
            **base_report.get("validation_policy", {}),
            "validated_input": "m45_02_repaired_quantity_preview",
            "human_acceptance_required": True,
            "runtime_fixture_gate_still_required": True,
        },
        "summary": {
            **base_report["summary"],
            "accepted_recipe_id": accepted_repair_artifact.get("summary", {}).get("accepted_recipe_id", ""),
            "accepted_review_item_id": accepted_repair_artifact.get("summary", {}).get("accepted_review_item_id", ""),
            "accepted_combined_repair_package_id": accepted_repair_artifact.get("summary", {}).get(
                "accepted_combined_repair_package_id", ""
            ),
            "human_acceptance_recorded": bool(
                accepted_repair_artifact.get("summary", {}).get("human_acceptance_recorded")
            ),
            "runtime_fixture_created": False,
            "runtime_promotion_allowed": False,
            "ready_for_m45_04": ready_for_m45_04,
        },
        "recipe_validations": base_report["recipe_validations"],
        "next_target": {
            "milestone": "M45-04",
            "task": "Third-slice runtime fixture promotion gate",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M45-03 Third-Slice Repaired Recipe Validation Rerun",
        "",
        "## Summary",
        "",
        f"- Accepted recipe: `{summary['accepted_recipe_id']}`",
        f"- Accepted review item: `{summary['accepted_review_item_id']}`",
        f"- Accepted combined repair package: `{summary['accepted_combined_repair_package_id']}`",
        f"- Human acceptance recorded: `{summary['human_acceptance_recorded']}`",
        f"- Recipes validated: `{summary['recipe_count']}`",
        f"- Runtime-ready recipes: `{summary['runtime_ready_recipe_count']}`",
        f"- Validator passed: `{summary['validator_passed_count']}`",
        f"- Invalid drafts: `{summary['invalid_draft_count']}`",
        f"- Blocked by manual review: `{summary['blocked_by_manual_review_count']}`",
        f"- Missing-card recipes: `{summary['missing_card_recipe_count']}`",
        f"- Copy-limit violation recipes: `{summary['copy_limit_violation_recipe_count']}`",
        f"- Slot-gap recipes: `{summary['slot_gap_recipe_count']}`",
        f"- Trigger-count mismatch recipes: `{summary['trigger_count_mismatch_recipe_count']}`",
        f"- Manual-review overlap recipes: `{summary['manual_review_overlap_recipe_count']}`",
        f"- Grade-profile review recipes: `{summary['grade_profile_review_recipe_count']}`",
        f"- Runtime fixture created: `{summary['runtime_fixture_created']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M45-04: `{summary['ready_for_m45_04']}`",
        "",
        "## Recipe Status",
        "",
    ]
    for item in report["recipe_validations"]:
        lines.append(
            f"- `{item['recipe_id']}` edge=`{item['source_candidate_edge']}` "
            f"status=`{item['validation_status']}` blockers=`{item['blocking_issue_count']}` "
            f"runtime_ready=`{item['runtime_ready']}`"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- This rerun validates the M45-02 repaired quantity preview only.",
            "- It does not create runtime fixtures, saved decks, UI entries, or bot playbooks.",
            "- M45-04 must decide whether the validated preview may enter fixture scope.",
            "",
            "## Next",
            "",
            "`M45-04`: Third-slice runtime fixture promotion gate.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate M45-02 third-slice repaired recipe preview.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_third_slice_repaired_validation_report()
    json_path = args.output_dir / "m45_03_third_slice_repaired_recipe_validation_report.json"
    md_path = args.output_dir / "m45_03_third_slice_repaired_recipe_validation_report.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M45-03 third-slice repaired recipe validation report wrote {json_path}")
    print(f"M45-03 third-slice repaired recipe validation summary wrote {md_path}")
    print(
        "ready_for_m45_04={ready} runtime_ready={runtime_ready} validator_passed={passed}".format(
            ready=report["summary"]["ready_for_m45_04"],
            runtime_ready=report["summary"]["runtime_ready_recipe_count"],
            passed=report["summary"]["validator_passed_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m45_04"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
