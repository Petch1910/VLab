"""Build the M53-04 fifth-slice repaired recipe validation rerun."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M53_03_ACCEPTED = OUTPUT_DIR / "m53_03_fifth_slice_human_accepted_repair_artifact.json"


if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.check_fifth_slice_combo_recipe_consistency import (  # noqa: E402
    build_fifth_slice_consistency_report,
)
from tools.deck.validate_fifth_slice_recipe_drafts import (  # noqa: E402
    _all_card_ids,
    build_fifth_slice_validation_report,
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


def _recipe_from_accepted_artifact(accepted_artifact: dict[str, Any]) -> dict[str, Any]:
    repair = accepted_artifact["accepted_repair"]
    pair = repair.get("pair", {})
    return {
        "recipe_id": repair["recipe_id"],
        "source_candidate_edge": repair.get("source_candidate_edge", ""),
        "source_edge_rank": None,
        "anchor_card_id": pair.get("source_card_id", ""),
        "review_status": "human_accepted_repair_pending_validation",
        "review_blockers": [],
        "pair": pair,
        "quantities": [
            {
                "card_id": row["card_id"],
                "quantity": int(row.get("quantity", 0)),
                "roles": ["m53_04_repaired_recipe_candidate"],
                "quantity_source": "m53_03_accepted_repair_preview",
            }
            for row in repair.get("repaired_quantities", [])
        ],
        "slot_summary": {
            "main_deck_target": 50,
            "explicit_card_count": int(repair.get("main_deck_count_after_repair", 0)),
            "total_unfilled_slots": max(0, 50 - int(repair.get("main_deck_count_after_repair", 0))),
        },
        "validation_metadata": {
            "manual_review_card_ids": [],
            "m53_04_in_memory_repaired_validation": True,
            "not_runtime_deck": True,
            "not_saved_deck": True,
            "not_ui_published": True,
            "not_bot_playbook": True,
        },
    }


def _issue_codes(validation: dict[str, Any], severity: str | None = None) -> list[str]:
    return [
        issue["code"]
        for issue in validation.get("issues", [])
        if severity is None or issue.get("severity") == severity
    ]


def build_fifth_slice_repaired_recipe_validation_rerun(
    accepted_artifact: dict[str, Any] | None = None,
) -> dict[str, Any]:
    accepted_artifact = accepted_artifact or load_json(M53_03_ACCEPTED)
    accepted_summary = accepted_artifact.get("summary", {})
    repaired_recipe = _recipe_from_accepted_artifact(accepted_artifact)
    recipe_report = {
        "version": "M53-04-input",
        "selected_target": accepted_artifact.get("selected_target", {}),
        "recipe_drafts": [repaired_recipe],
    }
    card_rows = load_card_rows(_all_card_ids(recipe_report))
    validation_report = build_fifth_slice_validation_report(recipe_report, card_rows)
    validation = validation_report["recipe_validations"][0]
    consistency_report = build_fifth_slice_consistency_report(recipe_report, validation_report)
    consistency = consistency_report["consistency_checks"][0]
    ready_for_m53_05 = (
        bool(accepted_summary.get("ready_for_m53_04"))
        and validation["validation_status"] == "validator_passed"
        and bool(validation["runtime_ready"])
        and bool(consistency["promotion_allowed"])
    )
    return {
        "version": "M53-04",
        "description": "Fifth-slice repaired recipe validation rerun",
        "selected_target": accepted_artifact.get("selected_target", {}),
        "source_inputs": {
            "m53_03_human_accepted_repair_artifact": str(M53_03_ACCEPTED.relative_to(ROOT)),
        },
        "scope": {
            "offline_in_memory_validation_rerun": True,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
            "declares_runtime_fixture_ready": False,
        },
        "accepted_context": {
            "accepted_review_item_id": accepted_summary.get("accepted_review_item_id", ""),
            "accepted_recipe_id": accepted_summary.get("accepted_recipe_id", ""),
            "accepted_grade_profile_package_id": accepted_summary.get("accepted_grade_profile_package_id", ""),
            "human_selection_recorded": accepted_summary.get("human_selection_recorded", False),
            "human_acceptance_recorded": accepted_summary.get("human_acceptance_recorded", False),
            "accepted_artifact_ready_for_validation": accepted_summary.get("ready_for_m53_04", False),
        },
        "repaired_recipe_rows": [
            {
                "card_id": row["card_id"],
                "quantity": int(row.get("quantity", 0)),
            }
            for row in repaired_recipe.get("quantities", [])
        ],
        "repaired_recipe_validation": {
            "recipe_id": validation["recipe_id"],
            "validation_status": validation["validation_status"],
            "runtime_ready": validation["runtime_ready"],
            "blocking_issue_count": validation["blocking_issue_count"],
            "blocker_codes": _issue_codes(validation, "blocker"),
            "review_codes": _issue_codes(validation, "review"),
            "count_summary": validation["count_summary"],
        },
        "repaired_recipe_consistency": {
            "recipe_id": consistency["recipe_id"],
            "consistency_status": consistency["consistency_status"],
            "pair_cards_present": consistency["pair_cards_present"],
            "promotion_allowed_by_validation_and_consistency": consistency["promotion_allowed"],
            "source_card_id": consistency["source_card_id"],
            "target_card_id": consistency["target_card_id"],
        },
        "validation_report_preview": validation_report,
        "consistency_report_preview": consistency_report,
        "summary": {
            "accepted_recipe_id": accepted_summary.get("accepted_recipe_id", ""),
            "validation_status": validation["validation_status"],
            "consistency_status": consistency["consistency_status"],
            "runtime_ready_recipe_count": validation_report["summary"]["runtime_ready_recipe_count"],
            "promotion_allowed_count": consistency_report["summary"]["promotion_allowed_count"],
            "blocking_issue_count": validation["blocking_issue_count"],
            "review_issue_count": len(_issue_codes(validation, "review")),
            "runtime_fixture_promotion_allowed": False,
            "ready_for_m53_05": ready_for_m53_05,
        },
        "next_target": {
            "milestone": "M53-05",
            "task": "Fifth-slice runtime fixture promotion gate",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    validation = report["repaired_recipe_validation"]
    consistency = report["repaired_recipe_consistency"]
    lines = [
        "# M53-04 Fifth-Slice Repaired Recipe Validation Rerun",
        "",
        "## Summary",
        "",
        f"- Accepted recipe: `{summary['accepted_recipe_id']}`",
        f"- Validation status: `{summary['validation_status']}`",
        f"- Consistency status: `{summary['consistency_status']}`",
        f"- Runtime-ready recipes: `{summary['runtime_ready_recipe_count']}`",
        f"- Promotion-allowed checks: `{summary['promotion_allowed_count']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        f"- Review issues: `{summary['review_issue_count']}`",
        f"- Runtime fixture promotion allowed: `{summary['runtime_fixture_promotion_allowed']}`",
        f"- Ready for M53-05: `{summary['ready_for_m53_05']}`",
        "",
        "## Validation",
        "",
        f"- Blocker codes: `{validation['blocker_codes']}`",
        f"- Review codes: `{validation['review_codes']}`",
        f"- Count summary: `{validation['count_summary']}`",
        "",
        "## Consistency",
        "",
        f"- Pair cards present: `{consistency['pair_cards_present']}`",
        f"- Source -> target: `{consistency['source_card_id']}` -> `{consistency['target_card_id']}`",
        f"- Promotion allowed by validation/consistency: `{consistency['promotion_allowed_by_validation_and_consistency']}`",
        "",
        "## Policy",
        "",
        "- Rerun is in-memory only.",
        "- Source recipe artifacts are not modified.",
        "- Runtime fixture promotion remains disabled until M53-05.",
        "",
        "## Next",
        "",
        "`M53-05`: Fifth-slice runtime fixture promotion gate.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M53-04 fifth-slice repaired recipe validation rerun.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_fifth_slice_repaired_recipe_validation_rerun()
    json_path = args.output_dir / "m53_04_fifth_slice_repaired_recipe_validation_rerun.json"
    md_path = args.output_dir / "m53_04_fifth_slice_repaired_recipe_validation_rerun.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M53-04 fifth-slice repaired recipe validation rerun wrote {json_path}")
    print(f"M53-04 fifth-slice repaired recipe validation rerun summary wrote {md_path}")
    print(
        "ready_for_m53_05={ready} validation={validation} consistency={consistency}".format(
            ready=report["summary"]["ready_for_m53_05"],
            validation=report["summary"]["validation_status"],
            consistency=report["summary"]["consistency_status"],
        )
    )
    return 0 if report["summary"]["ready_for_m53_05"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
