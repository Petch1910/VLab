"""Build the M65-03 eighth-slice human-accepted grade repair artifact.

This tool consumes the explicit M65-02 selected recipe artifact. It records a
human grade-repair decision only when non-empty decision text is provided and
does not declare the repaired recipe valid.
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
M65_02_SELECTION = OUTPUT_DIR / "m65_02_eighth_slice_human_selected_recipe_artifact.json"
M64_DRAFTS = OUTPUT_DIR / "m64_03_eighth_slice_recipe_draft_model.json"

if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_eighth_slice_blocker_repair_candidates import (  # noqa: E402
    CLASSIC_GRADE_TARGET,
    _grade_counts,
    _load_cards,
)


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


GRADE_DECISIONS = {"accepted", "rejected"}


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _find_recipe(drafts: dict[str, Any], recipe_id: str) -> dict[str, Any]:
    for recipe in drafts.get("recipe_drafts", []):
        if recipe.get("recipe_id") == recipe_id:
            return recipe
    raise ValueError(f"Recipe not found: {recipe_id}")


def _quantity_map(recipe: dict[str, Any]) -> dict[str, int]:
    return {row["card_id"]: int(row.get("quantity", 0)) for row in recipe.get("quantities", [])}


def _require_decision_text(decision_text: str) -> str:
    normalized = decision_text.strip()
    if not normalized:
        raise ValueError("M65-03 requires non-empty decision_text.")
    return normalized


def _normalize_grade_decision(grade_decision: str) -> str:
    normalized = grade_decision.strip().lower()
    if normalized not in GRADE_DECISIONS:
        raise ValueError("M65-03 grade_decision must be one of: accepted, rejected.")
    return normalized


def _grade_target(selection: dict[str, Any]) -> dict[str, int]:
    target = selection.get("target_grade_counts") or CLASSIC_GRADE_TARGET
    return {str(grade): int(count) for grade, count in target.items()}


def _direct_grade_package(selection: dict[str, Any]) -> dict[str, Any]:
    additions = list(selection.get("grade_additions", []))
    removals = list(selection.get("grade_removals", []))
    added_count = sum(int(item.get("quantity", 0)) for item in additions)
    removed_count = sum(int(item.get("quantity", 0)) for item in removals)
    grade_not_needed = bool(selection.get("grade_repair_not_needed"))
    return {
        "package_id": f"{selection['recipe_id']}_accepted_grade_pkg_001",
        "repair_type": "accepted_grade_profile_substitution_preview",
        "advisory_only": True,
        "source_grade_profile_package_id": selection.get("grade_profile_package_id", ""),
        "source_grade_package_directly_applied": True,
        "source_grade_package_recomputed_after_manual_substitution": False,
        "target_grade_counts": _grade_target(selection),
        "grade_counts_before": selection.get("grade_counts_before", {}),
        "additions": additions,
        "removals": removals,
        "grade_counts_after": selection.get("grade_counts_after", {}),
        "added_card_count": added_count,
        "removed_card_count": removed_count,
        "grade_repair_not_needed": grade_not_needed,
        "complete_candidate": bool(selection.get("grade_repair_complete")) or grade_not_needed,
        "runtime_promotion_allowed": False,
    }


def _apply_grade_package(
    quantities: dict[str, int],
    grade_package: dict[str, Any],
) -> tuple[dict[str, int], list[dict[str, Any]]]:
    repaired = dict(quantities)
    issues: list[dict[str, Any]] = []
    for removal in grade_package.get("removals", []):
        card_id = removal["card_id"]
        remove_quantity = int(removal.get("quantity", 0))
        before = repaired.get(card_id, 0)
        if before < remove_quantity:
            issues.append(
                {
                    "code": "grade_substitution_negative_quantity",
                    "severity": "blocker",
                    "card_id": card_id,
                    "before": before,
                    "remove": remove_quantity,
                }
            )
            continue
        after = before - remove_quantity
        if after:
            repaired[card_id] = after
        else:
            repaired.pop(card_id, None)
    for addition in grade_package.get("additions", []):
        card_id = addition["card_id"]
        quantity = int(addition.get("quantity", 0))
        if quantity <= 0:
            issues.append(
                {
                    "code": "grade_addition_non_positive_quantity",
                    "severity": "blocker",
                    "card_id": card_id,
                    "quantity": quantity,
                }
            )
            continue
        repaired[card_id] = repaired.get(card_id, 0) + quantity
    return repaired, issues


def _repaired_quantity_rows(quantities: dict[str, int]) -> list[dict[str, Any]]:
    cards = _load_cards(quantities.keys())
    rows: list[dict[str, Any]] = []
    for card_id in sorted(quantities):
        card = cards.get(card_id, {})
        rows.append(
            {
                "card_id": card_id,
                "name_th": card.get("name_th", ""),
                "quantity": int(quantities[card_id]),
                "grade": str(card.get("grade", "")),
                "trigger": card.get("trigger") or "",
                "type_1": card.get("type_1", ""),
                "series_code": card.get("series_code", ""),
                "source": "m65_03_accepted_grade_repair_preview",
            }
        )
    return rows


def _grade_counts_for_output(quantities: dict[str, int], target: dict[str, int]) -> dict[str, int]:
    cards = _load_cards(quantities.keys())
    counts = _grade_counts(quantities, cards)
    return {grade: int(counts.get(grade, 0)) for grade in sorted(target)}


def build_eighth_slice_human_accepted_grade_repair_artifact(
    selected_artifact: dict[str, Any] | None = None,
    drafts: dict[str, Any] | None = None,
    *,
    decision_text: str,
    grade_decision: str = "accepted",
    decided_by: str = "user",
    decided_at: str | None = None,
) -> dict[str, Any]:
    selected_artifact = selected_artifact or load_json(M65_02_SELECTION)
    drafts = drafts or load_json(M64_DRAFTS)
    decision_text = _require_decision_text(decision_text)
    grade_decision = _normalize_grade_decision(grade_decision)
    decided_at = decided_at or date.today().isoformat()
    selection = selected_artifact["selection"]
    recipe = _find_recipe(drafts, selection["recipe_id"])
    original_quantities = _quantity_map(recipe)
    grade_package = _direct_grade_package(selection)
    target = _grade_target(selection)

    if grade_decision == "accepted":
        repaired_quantities, repair_issues = _apply_grade_package(original_quantities, grade_package)
    else:
        repaired_quantities = dict(original_quantities)
        repair_issues = []

    repaired_rows = _repaired_quantity_rows(repaired_quantities)
    final_grade_counts = _grade_counts_for_output(repaired_quantities, target)
    total_cards = sum(repaired_quantities.values())
    accepted = (
        grade_decision == "accepted"
        and bool(selected_artifact.get("summary", {}).get("ready_for_m65_03"))
        and bool(selection.get("ready_for_human_repair_review"))
        and bool(grade_package.get("complete_candidate"))
    )
    ready_for_m65_04 = (
        accepted
        and total_cards == 50
        and not repair_issues
        and final_grade_counts == target
    )

    return {
        "version": "M65-03",
        "description": "Eighth-slice human-accepted grade repair artifact",
        "selected_target": selected_artifact.get("selected_target", {}),
        "source_inputs": {
            "m65_02_human_selected_recipe_artifact": str(M65_02_SELECTION.relative_to(ROOT)),
            "m64_recipe_drafts": str(M64_DRAFTS.relative_to(ROOT)),
        },
        "scope": {
            "offline_human_grade_repair_decision_artifact": True,
            "records_human_selection": True,
            "records_human_acceptance": True,
            "records_grade_repair_acceptance": grade_decision == "accepted",
            "records_grade_repair_rejection": grade_decision == "rejected",
            "records_lock_decision": False,
            "records_legion_decision": False,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "lock_runtime": False,
            "legion_runtime": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
            "declares_recipe_valid": False,
        },
        "acceptance_policy": {
            "requires_m65_02_selection": True,
            "requires_non_empty_decision_text": True,
            "acceptance_is_not_validation": True,
            "acceptance_is_not_lock_decision": True,
            "acceptance_is_not_legion_decision": True,
            "supports_grade_repair_rejection": True,
            "source_grade_package_directly_applied": True,
            "runtime_promotion_allowed": False,
            "m65_04_must_record_explicit_lock_legion_decision": True,
            "m65_05_must_rerun_validation": True,
        },
        "grade_repair_decision_record": {
            "decision": grade_decision,
            "decided_by": decided_by,
            "decided_at": decided_at,
            "decision_text": decision_text,
            "selected_review_item_id": selection["selected_review_item_id"],
            "selected_recipe_id": selection["recipe_id"],
            "selected_human_selection_package_id": selection.get("human_selection_package_id", ""),
            "source_grade_profile_package_id": selection.get("grade_profile_package_id", ""),
            "accepted_grade_package_id": grade_package["package_id"] if grade_decision == "accepted" else "",
            "selected_lock_package_id": selection.get("lock_package_id", ""),
            "selected_legion_package_id": selection.get("legion_package_id", ""),
            "source_note": "Recorded from explicit grade repair decision for the M65-02 selected eighth-slice recipe.",
        },
        "accepted_repair": {
            "recipe_id": selection["recipe_id"],
            "source_candidate_edge": selection.get("source_candidate_edge", ""),
            "pair": selection.get("pair", {}),
            "human_selection_package_id": selection.get("human_selection_package_id", ""),
            "source_grade_profile_package_id": selection.get("grade_profile_package_id", ""),
            "accepted_grade_repair_package": grade_package,
            "repaired_quantities": repaired_rows,
            "repair_application_issues": repair_issues,
            "main_deck_count_after_repair": total_cards,
            "grade_counts_after_repair": final_grade_counts,
            "lock_package_id": selection.get("lock_package_id", ""),
            "lock_deferred": bool(selection.get("lock_deferred")),
            "lock_future_system_work": selection.get("lock_future_system_work", []),
            "lock_decision_options": selection.get("lock_decision_options", []),
            "legion_package_id": selection.get("legion_package_id", ""),
            "legion_deferred": bool(selection.get("legion_deferred")),
            "legion_future_system_work": selection.get("legion_future_system_work", []),
            "legion_decision_options": selection.get("legion_decision_options", []),
            "runtime_promotion_allowed": False,
            "requires_m65_04_lock_legion_decision": True,
            "requires_m65_05_validation": True,
            "ready_for_m65_04_system_decision": ready_for_m65_04,
        },
        "summary": {
            "selected_review_item_id": selection["selected_review_item_id"],
            "selected_recipe_id": selection["recipe_id"],
            "selected_human_selection_package_id": selection.get("human_selection_package_id", ""),
            "source_grade_profile_package_id": selection.get("grade_profile_package_id", ""),
            "accepted_grade_repair_package_id": grade_package["package_id"] if grade_decision == "accepted" else "",
            "selected_lock_package_id": selection.get("lock_package_id", ""),
            "selected_legion_package_id": selection.get("legion_package_id", ""),
            "human_selection_recorded": True,
            "human_acceptance_recorded": True,
            "grade_repair_decision": grade_decision,
            "grade_repair_accepted": grade_decision == "accepted",
            "grade_repair_rejected": grade_decision == "rejected",
            "lock_decision_recorded": False,
            "legion_decision_recorded": False,
            "lock_deferred": bool(selection.get("lock_deferred")),
            "legion_deferred": bool(selection.get("legion_deferred")),
            "source_grade_package_directly_applied": bool(grade_package.get("source_grade_package_directly_applied")),
            "main_deck_count_after_repair": total_cards,
            "grade_counts_after_repair": final_grade_counts,
            "repair_application_issue_count": len(repair_issues),
            "runtime_promotion_allowed": False,
            "declares_recipe_valid": False,
            "ready_for_m65_04": ready_for_m65_04,
        },
        "next_target": {
            "milestone": "M65-04",
            "task": "Eighth-slice Lock and Legion decision artifact",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    record = report["grade_repair_decision_record"]
    repair = report["accepted_repair"]
    grade_package = repair["accepted_grade_repair_package"]
    lines = [
        "# M65-03 Eighth-Slice Human-Accepted Grade Repair Artifact",
        "",
        "## Summary",
        "",
        f"- Selected review item: `{summary['selected_review_item_id']}`",
        f"- Selected recipe: `{summary['selected_recipe_id']}`",
        f"- Human-selection package: `{summary['selected_human_selection_package_id']}`",
        f"- Source grade package: `{summary['source_grade_profile_package_id']}`",
        f"- Accepted grade package: `{summary['accepted_grade_repair_package_id']}`",
        f"- Selected Lock package: `{summary['selected_lock_package_id']}`",
        f"- Selected Legion package: `{summary['selected_legion_package_id']}`",
        f"- Grade repair decision: `{summary['grade_repair_decision']}`",
        f"- Grade repair accepted: `{summary['grade_repair_accepted']}`",
        f"- Grade repair rejected: `{summary['grade_repair_rejected']}`",
        f"- Lock decision recorded: `{summary['lock_decision_recorded']}`",
        f"- Legion decision recorded: `{summary['legion_decision_recorded']}`",
        f"- Lock deferred: `{summary['lock_deferred']}`",
        f"- Legion deferred: `{summary['legion_deferred']}`",
        f"- Source grade package directly applied: `{summary['source_grade_package_directly_applied']}`",
        f"- Main deck count after repair: `{summary['main_deck_count_after_repair']}`",
        f"- Grade counts after repair: `{summary['grade_counts_after_repair']}`",
        f"- Repair application issues: `{summary['repair_application_issue_count']}`",
        f"- Declares recipe valid: `{summary['declares_recipe_valid']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M65-04: `{summary['ready_for_m65_04']}`",
        "",
        "## Decision Record",
        "",
        f"- Decision: `{record['decision']}`",
        f"- Decided by: `{record['decided_by']}`",
        f"- Decided at: `{record['decided_at']}`",
        f"- Decision text: `{record['decision_text']}`",
        "",
        "## Accepted Repair",
        "",
        f"- Source edge: `{repair['source_candidate_edge']}`",
        f"- Pair: `{repair['pair'].get('source_card_id', '')}` -> `{repair['pair'].get('target_card_id', '')}`",
        f"- Grade added cards: `{grade_package['added_card_count']}`",
        f"- Grade removed cards: `{grade_package['removed_card_count']}`",
        f"- Repaired quantity rows: `{len(repair['repaired_quantities'])}`",
        f"- Lock future system work: `{len(repair['lock_future_system_work'])}`",
        f"- Legion future system work: `{len(repair['legion_future_system_work'])}`",
        "",
        "## Policy",
        "",
        "- Grade repair acceptance does not declare the recipe valid.",
        "- This artifact does not record a Lock decision.",
        "- This artifact does not record a Legion decision.",
        "- Runtime promotion remains disabled.",
        "- M65-04 must record explicit Lock and Legion decisions.",
        "- M65-05 must rerun validation before any fixture gate.",
        "",
        "## Next",
        "",
        "`M65-04`: Eighth-slice Lock and Legion decision artifact.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M65-03 eighth-slice human-accepted grade repair artifact.")
    parser.add_argument("--decision-text", required=True, help="Non-empty text proving explicit grade repair decision.")
    parser.add_argument("--grade-decision", choices=sorted(GRADE_DECISIONS), default="accepted")
    parser.add_argument("--decided-by", default="user")
    parser.add_argument("--decided-at", default=None)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_eighth_slice_human_accepted_grade_repair_artifact(
        decision_text=args.decision_text,
        grade_decision=args.grade_decision,
        decided_by=args.decided_by,
        decided_at=args.decided_at,
    )
    json_path = args.output_dir / "m65_03_eighth_slice_human_accepted_grade_repair_artifact.json"
    md_path = args.output_dir / "m65_03_eighth_slice_human_accepted_grade_repair_artifact.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M65-03 eighth-slice human-accepted grade repair artifact wrote {json_path}")
    print(f"M65-03 eighth-slice human-accepted grade repair artifact summary wrote {md_path}")
    print(
        "ready_for_m65_04={ready} selected_recipe={recipe}".format(
            ready=report["summary"]["ready_for_m65_04"],
            recipe=report["summary"]["selected_recipe_id"],
        )
    )
    return 0 if report["summary"]["ready_for_m65_04"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
