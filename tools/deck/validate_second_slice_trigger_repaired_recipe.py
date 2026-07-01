"""Validate the M41 trigger-repaired second-slice recipe."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence

ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.validate_second_slice_repaired_recipe import (
    CARDS_SQLITE,
    build_second_slice_repaired_recipe_validation_report,
    load_json,
)


OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M41_TRIGGER_REPAIR_ACCEPTED = OUTPUT_DIR / "m41_repair_accept_second_slice_trigger_repair_artifact.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def build_second_slice_trigger_repaired_recipe_validation_report(
    accepted_artifact: dict[str, Any] | None = None,
) -> dict[str, Any]:
    accepted_artifact = accepted_artifact or load_json(M41_TRIGGER_REPAIR_ACCEPTED)
    report = build_second_slice_repaired_recipe_validation_report(accepted_artifact)
    ready_for_m41_04 = report["summary"]["ready_for_m41_04"]
    report["version"] = "M41-repair-validate"
    report["description"] = "Second-slice repaired recipe validation rerun after trigger repair"
    report["source_inputs"] = {
        "trigger_repair_acceptance_artifact": str(M41_TRIGGER_REPAIR_ACCEPTED.relative_to(ROOT)),
        "runtime_cards_sqlite": str(CARDS_SQLITE.relative_to(ROOT)),
    }
    report["scope"]["changes_trigger_repair_acceptance_artifact"] = False
    report["trigger_repair_acceptance"] = {
        "accepted_package_id": accepted_artifact.get("summary", {}).get("accepted_package_id", ""),
        "accepted_profile_id": accepted_artifact.get("summary", {}).get("accepted_profile_id", ""),
        "human_acceptance_recorded": accepted_artifact.get("summary", {}).get("human_acceptance_recorded", False),
        "ready_for_validation_rerun": accepted_artifact.get("summary", {}).get("ready_for_validation_rerun", False),
    }
    report["next_target"] = (
        {
            "milestone": "M41-04",
            "task": "Second-slice runtime fixture promotion gate",
        }
        if ready_for_m41_04
        else {
            "milestone": "M41-repair",
            "task": "Second-slice trigger/profile repair loop",
        }
    )
    return report


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    validation = report["recipe_validation"]
    acceptance = report["trigger_repair_acceptance"]
    next_target = report["next_target"]
    lines = [
        "# M41 Repair Validate Second-Slice Repaired Recipe Validation",
        "",
        "## Summary",
        "",
        f"- Recipe: `{summary['recipe_id']}`",
        f"- Validation status: `{summary['validation_status']}`",
        f"- Runtime ready: `{summary['runtime_ready']}`",
        f"- Promotion allowed: `{summary['promotion_allowed']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        f"- Main deck count: `{summary['main_deck_count']}`",
        f"- Trigger count: `{summary['trigger_count']}`",
        f"- Trigger counts: `{validation['count_summary']['trigger_counts']}`",
        f"- Grade counts: `{summary['grade_counts']}`",
        f"- Manual-review overlap cleared: `{summary['manual_review_card_overlap_cleared']}`",
        f"- Ready for M41-04: `{summary['ready_for_m41_04']}`",
        f"- Ready for repair loop: `{summary['ready_for_repair_loop']}`",
        "",
        "## Accepted Trigger Repair",
        "",
        f"- Package: `{acceptance['accepted_package_id']}`",
        f"- Profile: `{acceptance['accepted_profile_id']}`",
        f"- Human acceptance recorded: `{acceptance['human_acceptance_recorded']}`",
        f"- Ready for validation rerun: `{acceptance['ready_for_validation_rerun']}`",
        "",
        "## Issues",
        "",
    ]
    if validation["issues"]:
        for issue in validation["issues"]:
            lines.append(f"- `{issue['severity']}` `{issue['code']}`: {issue['message']} `{issue['details']}`")
    else:
        lines.append("- No validation issues.")
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Offline validation only.",
            "- No runtime fixture, saved deck, UI deck list, bot playbook, or GameState mutation.",
            "- Passing validation only opens the next promotion gate; it does not promote by itself.",
            "",
            "## Next",
            "",
            f"`{next_target['milestone']}`: {next_target['task']}.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate the M41 trigger-repaired second-slice recipe.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_second_slice_trigger_repaired_recipe_validation_report()
    json_path = args.output_dir / "m41_repair_validate_second_slice_repaired_recipe.json"
    md_path = args.output_dir / "m41_repair_validate_second_slice_repaired_recipe.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M41 trigger-repaired recipe validation wrote {json_path}")
    print(f"M41 trigger-repaired recipe validation summary wrote {md_path}")
    print(
        "ready_for_m41_04={ready} status={status} blockers={blockers} trigger_count={trigger_count}".format(
            ready=report["summary"]["ready_for_m41_04"],
            status=report["summary"]["validation_status"],
            blockers=report["summary"]["blocking_issue_count"],
            trigger_count=report["summary"]["trigger_count"],
        )
    )
    return 0 if report["summary"]["recipe_id"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
