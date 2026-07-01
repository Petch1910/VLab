"""Build the M37-02 trigger package repair proposal.

This turns M37-01 advisory package candidates into a reviewable repair
proposal. It simulates blocker resolution for the accepted seed recipe but does
not mutate the recipe draft or promote a runtime deck.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M37_01_CANDIDATES = OUTPUT_DIR / "m37_01_accepted_seed_slot_gap_candidates.json"
VALIDATION_REPORT = OUTPUT_DIR / "m36_03_deck_recipe_validation_report.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _validation_for_recipe(validation_report: dict[str, Any], recipe_id: str) -> dict[str, Any]:
    for validation in validation_report.get("recipe_validations", []):
        if validation.get("recipe_id") == recipe_id:
            return validation
    raise ValueError(f"Validation not found for recipe: {recipe_id}")


def _issue_codes(issues: Sequence[dict[str, Any]], severity: str | None = None) -> list[str]:
    return [
        issue["code"]
        for issue in issues
        if severity is None or issue.get("severity") == severity
    ]


def _simulate_package(package: dict[str, Any], validation: dict[str, Any]) -> dict[str, Any]:
    count_summary = validation.get("count_summary", {})
    original_issues = validation.get("issues", [])
    blocker_codes = set(_issue_codes(original_issues, "blocker"))
    review_codes = _issue_codes(original_issues, "review")
    explicit_after = int(count_summary.get("explicit_card_count", 0)) + int(package["added_trigger_count"])
    main_target = int(count_summary.get("main_deck_target", 50))
    final_trigger_total = sum(int(count) for count in package["final_trigger_counts"].values())
    grade_counts_after = {str(key): int(value) for key, value in count_summary.get("grade_counts", {}).items()}
    grade_counts_after["0"] = grade_counts_after.get("0", 0) + int(package["added_trigger_count"])

    resolved: list[str] = []
    if "main_deck_size_mismatch" in blocker_codes and explicit_after == main_target:
        resolved.append("main_deck_size_mismatch")
    if "unfilled_slots" in blocker_codes and package["added_trigger_count"] == 12:
        resolved.append("unfilled_slots")
    if "trigger_count_mismatch" in blocker_codes and final_trigger_total == 16:
        resolved.append("trigger_count_mismatch")

    unresolved_blockers = sorted(blocker_codes.difference(resolved))
    if package["completion_status"] != "complete_candidate":
        unresolved_blockers.append("incomplete_trigger_package")
    if package["copy_limit_violations"]:
        unresolved_blockers.append("copy_limit_violation_after_repair")
    if not package["heal_limit_ok"]:
        unresolved_blockers.append("heal_limit_violation_after_repair")

    return {
        "package_id": package["package_id"],
        "profile_id": package["profile_id"],
        "score": package["score"],
        "status": "trigger_blockers_resolved_pending_review" if not unresolved_blockers else "still_blocked",
        "resolved_blockers": sorted(resolved),
        "unresolved_blockers": sorted(set(unresolved_blockers)),
        "carried_review_issues": review_codes,
        "explicit_card_count_after": explicit_after,
        "trigger_count_after": final_trigger_total,
        "final_trigger_counts": package["final_trigger_counts"],
        "grade_counts_after": dict(sorted(grade_counts_after.items())),
        "quantity_delta": [
            {
                "card_id": item["card_id"],
                "name_th": item.get("name_th", ""),
                "quantity": item["quantity"],
                "trigger": item["trigger"],
                "series_code": item["series_code"],
                "source": item["source"],
            }
            for item in package["additions"]
        ],
        "runtime_promotion_allowed": False,
    }


def _recommended_simulation(simulations: Sequence[dict[str, Any]]) -> dict[str, Any]:
    complete = [item for item in simulations if item["status"] == "trigger_blockers_resolved_pending_review"]
    if not complete:
        return max(simulations, key=lambda item: (item["score"], item["package_id"]))
    return max(complete, key=lambda item: (item["score"], item["package_id"]))


def build_repair_proposal(
    candidates_report: dict[str, Any] | None = None,
    validation_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    candidates_report = candidates_report or load_json(M37_01_CANDIDATES)
    validation_report = validation_report or load_json(VALIDATION_REPORT)
    recipe_id = candidates_report["accepted_seed"]["recipe_id"]
    validation = _validation_for_recipe(validation_report, recipe_id)
    simulations = [
        _simulate_package(package, validation)
        for package in candidates_report.get("completion_packages", [])
    ]
    recommended = _recommended_simulation(simulations)
    resolved_count = sum(
        1 for simulation in simulations if simulation["status"] == "trigger_blockers_resolved_pending_review"
    )

    return {
        "version": "M37-02",
        "description": "Trigger package repair proposal for the accepted seed recipe",
        "selected_target": candidates_report.get("selected_target", {}),
        "source_inputs": {
            "accepted_seed_slot_gap_candidates": str(M37_01_CANDIDATES.relative_to(ROOT)),
            "deck_recipe_validation_report": str(VALIDATION_REPORT.relative_to(ROOT)),
        },
        "scope": {
            "offline_repair_proposal": True,
            "changes_recipe_draft": False,
            "creates_runtime_deck": False,
            "bot_integration": False,
            "automatic_deck_injection": False,
            "live_card_text_parsing": False,
        },
        "accepted_seed": candidates_report["accepted_seed"],
        "original_validation": {
            "validation_status": validation["validation_status"],
            "blocking_issue_count": validation["blocking_issue_count"],
            "blocker_codes": _issue_codes(validation.get("issues", []), "blocker"),
            "review_codes": _issue_codes(validation.get("issues", []), "review"),
            "count_summary": validation.get("count_summary", {}),
        },
        "package_simulations": simulations,
        "recommended_repair": {
            "package_id": recommended["package_id"],
            "profile_id": recommended["profile_id"],
            "reason": "highest_scoring_complete_candidate_that_resolves_trigger_blockers",
            "quantity_delta": recommended["quantity_delta"],
            "resolved_blockers": recommended["resolved_blockers"],
            "remaining_review_issues": recommended["carried_review_issues"],
            "final_trigger_counts": recommended["final_trigger_counts"],
            "grade_counts_after": recommended["grade_counts_after"],
            "runtime_promotion_allowed": False,
            "requires_human_acceptance": True,
        },
        "summary": {
            "recipe_id": recipe_id,
            "package_count": len(simulations),
            "packages_resolving_trigger_blockers": resolved_count,
            "recommended_package_id": recommended["package_id"],
            "recommended_profile_id": recommended["profile_id"],
            "resolved_blockers": recommended["resolved_blockers"],
            "remaining_review_issue_count": len(recommended["carried_review_issues"]),
            "runtime_promotion_allowed": False,
            "ready_for_m37_03": resolved_count > 0,
        },
        "next_target": {
            "milestone": "M37-03",
            "task": "Rejected-line support-gap triage",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    recommended = report["recommended_repair"]
    lines = [
        "# M37-02 Trigger Package Repair Proposal",
        "",
        "## Summary",
        "",
        f"- Recipe: `{summary['recipe_id']}`",
        f"- Packages simulated: `{summary['package_count']}`",
        f"- Packages resolving trigger blockers: `{summary['packages_resolving_trigger_blockers']}`",
        f"- Recommended package: `{summary['recommended_package_id']}` / `{summary['recommended_profile_id']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M37-03: `{summary['ready_for_m37_03']}`",
        "",
        "## Recommended Repair",
        "",
        f"- Reason: `{recommended['reason']}`",
        f"- Final trigger counts: `{recommended['final_trigger_counts']}`",
        f"- Resolved blockers: `{recommended['resolved_blockers']}`",
        f"- Remaining review issues: `{recommended['remaining_review_issues']}`",
        "",
        "Quantity delta:",
        "",
    ]
    for item in recommended["quantity_delta"]:
        lines.append(
            f"- `{item['quantity']}x` `{item['card_id']}` ({item['trigger']}, {item['series_code']})"
        )
    lines.extend(["", "## Package Simulations", ""])
    for simulation in report["package_simulations"]:
        lines.append(
            f"- `{simulation['package_id']}` `{simulation['profile_id']}` "
            f"status=`{simulation['status']}` resolved={simulation['resolved_blockers']} "
            f"remaining={simulation['unresolved_blockers']} final={simulation['final_trigger_counts']}"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- This is a repair proposal only.",
            "- The recipe draft is not modified.",
            "- Runtime deck promotion remains disabled.",
            "- Human review is required before accepting the quantity delta.",
            "",
            "## Next",
            "",
            "`M37-03`: Rejected-line support-gap triage.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M37-02 trigger package repair proposal.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_repair_proposal()
    json_path = args.output_dir / "m37_02_trigger_package_repair_proposal.json"
    md_path = args.output_dir / "m37_02_trigger_package_repair_proposal.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M37-02 trigger package repair proposal wrote {json_path}")
    print(f"M37-02 trigger package repair proposal summary wrote {md_path}")
    print(
        "ready_for_m37_03={ready} recommended={recommended} resolving_packages={packages}".format(
            ready=report["summary"]["ready_for_m37_03"],
            recommended=report["summary"]["recommended_package_id"],
            packages=report["summary"]["packages_resolving_trigger_blockers"],
        )
    )
    return 0 if report["summary"]["ready_for_m37_03"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
