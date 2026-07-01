"""Build the M41-repair-accept trigger repair acceptance artifact."""

from __future__ import annotations

import argparse
import json
import sys
from datetime import date
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M41_02_ACCEPTED = OUTPUT_DIR / "m41_02_second_slice_human_accepted_repair_artifact.json"
M41_REPAIR_CANDIDATES = OUTPUT_DIR / "m41_repair_second_slice_trigger_profile_candidates.json"

DEFAULT_ACCEPTED_PACKAGE_ID = "m41_repair_pkg_001"
DEFAULT_ACCEPTANCE_TEXT = "\u0e07\u0e31\u0e49\u0e19\u0e08\u0e31\u0e14\u0e44\u0e1b"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _find_package(candidates: dict[str, Any], package_id: str) -> dict[str, Any]:
    for package in candidates.get("repair_candidates", []):
        if package.get("package_id") == package_id:
            return package
    raise ValueError(f"Repair package not found: {package_id}")


def _quantity_map(rows: list[dict[str, Any]]) -> dict[str, int]:
    return {row["card_id"]: int(row.get("quantity", 0)) for row in rows}


def _source_rows(accepted: dict[str, Any], package: dict[str, Any]) -> dict[str, dict[str, Any]]:
    rows: dict[str, dict[str, Any]] = {}
    for row in accepted["accepted_repair"].get("repaired_quantities", []):
        rows[row["card_id"]] = row
    for row in package.get("additions", []):
        rows.setdefault(row["card_id"], row)
    return rows


def _apply_package(accepted: dict[str, Any], package: dict[str, Any]) -> tuple[dict[str, int], list[dict[str, Any]]]:
    quantities = _quantity_map(accepted["accepted_repair"].get("repaired_quantities", []))
    issues: list[dict[str, Any]] = []
    for row in package.get("removals", []):
        card_id = row["card_id"]
        quantity = int(row.get("quantity", 0))
        before = quantities.get(card_id, 0)
        if before < quantity:
            issues.append(
                {
                    "code": "negative_quantity_after_removal",
                    "severity": "blocker",
                    "card_id": card_id,
                    "before": before,
                    "remove": quantity,
                }
            )
            quantities.pop(card_id, None)
            continue
        after = before - quantity
        if after:
            quantities[card_id] = after
        else:
            quantities.pop(card_id, None)
    for row in package.get("additions", []):
        quantities[row["card_id"]] = quantities.get(row["card_id"], 0) + int(row.get("quantity", 0))
    return quantities, issues


def _quantity_rows(quantities: dict[str, int], source_rows: dict[str, dict[str, Any]]) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    for card_id in sorted(quantities):
        source = source_rows.get(card_id, {})
        rows.append(
            {
                "card_id": card_id,
                "name_th": source.get("name_th", ""),
                "quantity": quantities[card_id],
                "grade": str(source.get("grade", "")),
                "trigger": source.get("trigger", ""),
                "series_code": source.get("series_code", ""),
                "source": "m41_repair_accept_trigger_profile_preview",
            }
        )
    return rows


def build_second_slice_trigger_repair_acceptance_artifact(
    accepted_artifact: dict[str, Any] | None = None,
    repair_candidates: dict[str, Any] | None = None,
    *,
    accepted_package_id: str = DEFAULT_ACCEPTED_PACKAGE_ID,
    acceptance_text: str = DEFAULT_ACCEPTANCE_TEXT,
    accepted_at: str | None = None,
) -> dict[str, Any]:
    accepted_artifact = accepted_artifact or load_json(M41_02_ACCEPTED)
    repair_candidates = repair_candidates or load_json(M41_REPAIR_CANDIDATES)
    package = _find_package(repair_candidates, accepted_package_id)
    quantities, apply_issues = _apply_package(accepted_artifact, package)
    rows = _quantity_rows(quantities, _source_rows(accepted_artifact, package))
    accepted = bool(acceptance_text.strip()) and package.get("complete_candidate")
    accepted_at = accepted_at or date.today().isoformat()
    total = sum(quantities.values())
    return {
        "version": "M41-repair-accept",
        "description": "Second-slice trigger repair acceptance artifact",
        "selected_target": accepted_artifact.get("selected_target", {}),
        "source_inputs": {
            "human_accepted_repair_artifact": str(M41_02_ACCEPTED.relative_to(ROOT)),
            "trigger_profile_repair_candidates": str(M41_REPAIR_CANDIDATES.relative_to(ROOT)),
        },
        "scope": {
            "offline_human_accepted_artifact": True,
            "records_human_acceptance": True,
            "changes_previous_artifacts": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
            "declares_recipe_valid": False,
        },
        "acceptance_record": {
            "decision": "accepted" if accepted else "pending",
            "accepted_by": "user",
            "accepted_at": accepted_at,
            "acceptance_text": acceptance_text,
            "interpreted_decision": "accept_balanced_trigger_profile_repair",
            "accepted_package_id": accepted_package_id,
            "accepted_profile_id": package.get("profile_id", ""),
            "source_note": "Recorded from the user's go-ahead for continuing the repair loop.",
        },
        "accepted_repair": {
            "recipe_id": accepted_artifact["accepted_repair"]["recipe_id"],
            "source_candidate_edge": accepted_artifact["accepted_repair"]["source_candidate_edge"],
            "base_grade_repair_package_id": accepted_artifact["accepted_repair"]["grade_profile_package_id"],
            "trigger_repair_package_id": package["package_id"],
            "trigger_repair_profile_id": package["profile_id"],
            "additions": package["additions"],
            "removals": package["removals"],
            "repaired_quantities": rows,
            "repair_application_issues": apply_issues,
            "main_deck_count_after_repair": total,
            "expected_counts_after": package["counts_after"],
            "runtime_promotion_allowed": False,
            "requires_validation_rerun": True,
            "ready_for_validation_rerun": accepted and total == 50 and not apply_issues,
        },
        "summary": {
            "accepted_package_id": accepted_package_id,
            "accepted_profile_id": package.get("profile_id", ""),
            "human_acceptance_recorded": accepted,
            "main_deck_count_after_repair": total,
            "repair_application_issue_count": len(apply_issues),
            "expected_trigger_count_after": package["counts_after"]["trigger_count"],
            "expected_grade_counts_after": package["counts_after"]["grade_counts"],
            "runtime_promotion_allowed": False,
            "declares_recipe_valid": False,
            "ready_for_validation_rerun": accepted and total == 50 and not apply_issues,
        },
        "next_target": {
            "milestone": "M41-repair-validate",
            "task": "Second-slice repaired recipe validation rerun after trigger repair",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    record = report["acceptance_record"]
    lines = [
        "# M41 Repair Accept Second-Slice Trigger Repair Acceptance Artifact",
        "",
        "## Summary",
        "",
        f"- Accepted package: `{summary['accepted_package_id']}`",
        f"- Accepted profile: `{summary['accepted_profile_id']}`",
        f"- Human acceptance recorded: `{summary['human_acceptance_recorded']}`",
        f"- Main deck count after repair: `{summary['main_deck_count_after_repair']}`",
        f"- Expected trigger count after: `{summary['expected_trigger_count_after']}`",
        f"- Expected grade counts after: `{summary['expected_grade_counts_after']}`",
        f"- Declares recipe valid: `{summary['declares_recipe_valid']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for validation rerun: `{summary['ready_for_validation_rerun']}`",
        "",
        "## Acceptance Record",
        "",
        f"- Decision: `{record['decision']}`",
        f"- Accepted by: `{record['accepted_by']}`",
        f"- Accepted at: `{record['accepted_at']}`",
        f"- Acceptance text: `{record['acceptance_text']}`",
        f"- Interpreted decision: `{record['interpreted_decision']}`",
        "",
        "## Policy",
        "",
        "- This artifact records acceptance of one trigger repair package.",
        "- It does not mutate previous artifacts.",
        "- It does not declare the recipe valid.",
        "- It does not create runtime fixtures, saved decks, UI entries, or bot playbooks.",
        "",
        "## Next",
        "",
        "`M41-repair-validate`: Second-slice repaired recipe validation rerun after trigger repair.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M41 trigger repair acceptance artifact.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    parser.add_argument("--accepted-package-id", default=DEFAULT_ACCEPTED_PACKAGE_ID)
    parser.add_argument("--acceptance-text", default=DEFAULT_ACCEPTANCE_TEXT)
    parser.add_argument("--accepted-at", default=None)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_second_slice_trigger_repair_acceptance_artifact(
        accepted_package_id=args.accepted_package_id,
        acceptance_text=args.acceptance_text,
        accepted_at=args.accepted_at,
    )
    json_path = args.output_dir / "m41_repair_accept_second_slice_trigger_repair_artifact.json"
    md_path = args.output_dir / "m41_repair_accept_second_slice_trigger_repair_artifact.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M41 repair trigger repair acceptance artifact wrote {json_path}")
    print(f"M41 repair trigger repair acceptance artifact summary wrote {md_path}")
    print(
        "ready_for_validation_rerun={ready} accepted={accepted} package={package}".format(
            ready=report["summary"]["ready_for_validation_rerun"],
            accepted=report["summary"]["human_acceptance_recorded"],
            package=report["summary"]["accepted_package_id"],
        )
    )
    return 0 if report["summary"]["ready_for_validation_rerun"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
