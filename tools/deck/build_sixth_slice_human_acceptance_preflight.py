"""Build a read-only M57-03 human acceptance preflight report.

The real M57-03 artifact requires explicit user/team acceptance text. This tool
checks whether a proposed acceptance would satisfy the M57-03 generator contract
without writing the accepted repair artifact.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
JSON_OUTPUT = OUTPUT_DIR / "m57_03_sixth_slice_human_acceptance_preflight.json"
MD_OUTPUT = OUTPUT_DIR / "m57_03_sixth_slice_human_acceptance_preflight.md"

if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


from tools.deck.build_sixth_slice_human_acceptance_request_packet import (  # noqa: E402
    build_sixth_slice_human_acceptance_request_packet,
)
from tools.deck.build_sixth_slice_human_accepted_repair_artifact import (  # noqa: E402
    build_sixth_slice_human_accepted_repair_artifact,
)


def _issue(code: str, severity: str, message: str, details: dict[str, Any] | None = None) -> dict[str, Any]:
    return {
        "code": code,
        "severity": severity,
        "message": message,
        "details": details or {},
    }


def _acceptance_command(acceptance_text: str) -> str:
    return (
        "python tools\\deck\\build_sixth_slice_human_accepted_repair_artifact.py "
        f'--acceptance-text "{acceptance_text}"'
    )


def build_sixth_slice_human_acceptance_preflight(
    request_packet: dict[str, Any] | None = None,
    *,
    acceptance_text: str = "",
) -> dict[str, Any]:
    request_packet = request_packet or build_sixth_slice_human_acceptance_request_packet()
    acceptance_text = acceptance_text.strip()
    issues: list[dict[str, Any]] = []
    dry_run_artifact: dict[str, Any] | None = None

    if not request_packet.get("summary", {}).get("acceptance_request_ready"):
        issues.append(
            _issue(
                "acceptance_request_not_ready",
                "blocker",
                "M57-03 acceptance request packet is not ready.",
                {"request_summary": request_packet.get("summary", {})},
            )
        )

    if not acceptance_text:
        issues.append(
            _issue(
                "missing_acceptance_text",
                "requires_input",
                "Preflight needs non-empty acceptance_text to dry-run the real M57-03 command.",
            )
        )

    blocking_issues = [issue for issue in issues if issue["severity"] == "blocker"]
    input_issues = [issue for issue in issues if issue["severity"] == "requires_input"]
    if not blocking_issues and not input_issues:
        try:
            dry_run_artifact = build_sixth_slice_human_accepted_repair_artifact(
                acceptance_text=acceptance_text,
                accepted_by="preflight-dry-run",
                accepted_at="preflight",
            )
            if not dry_run_artifact.get("summary", {}).get("ready_for_m57_04"):
                issues.append(
                    _issue(
                        "dry_run_not_ready_for_m57_04",
                        "blocker",
                        "The real M57-03 artifact would be generated but would not be ready for M57-04.",
                        {"summary": dry_run_artifact.get("summary", {})},
                    )
                )
        except (KeyError, ValueError, FileNotFoundError) as exc:
            issues.append(
                _issue(
                    "dry_run_generator_rejected_acceptance",
                    "blocker",
                    "The real M57-03 generator rejected the proposed acceptance.",
                    {"error": str(exc)},
                )
            )

    blocking_issues = [issue for issue in issues if issue["severity"] == "blocker"]
    input_issues = [issue for issue in issues if issue["severity"] == "requires_input"]
    preflight_passed = not blocking_issues and not input_issues and dry_run_artifact is not None
    dry_summary = dry_run_artifact.get("summary", {}) if dry_run_artifact else {}
    dry_record = dry_run_artifact.get("acceptance_record", {}) if dry_run_artifact else {}

    return {
        "version": "M57-03-preflight",
        "description": "Sixth-slice human acceptance preflight report",
        "source_inputs": {
            "m57_03_acceptance_request_packet": str(
                (OUTPUT_DIR / "m57_03_sixth_slice_human_acceptance_request_packet.json").relative_to(ROOT)
            ),
            "m57_02_human_selected_recipe_artifact": str(
                (OUTPUT_DIR / "m57_02_sixth_slice_human_selected_recipe_artifact.json").relative_to(ROOT)
            ),
        },
        "scope": {
            "read_only_preflight": True,
            "records_human_selection": False,
            "records_human_acceptance": False,
            "records_g_zone_decision": False,
            "creates_m57_03_artifact": False,
            "declares_recipe_valid": False,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
        },
        "input": {
            "acceptance_text_provided": bool(acceptance_text),
            "acceptance_text_preview": acceptance_text if acceptance_text else "",
        },
        "selected_recipe": request_packet.get("selected_recipe", {}),
        "dry_run": {
            "executed": dry_run_artifact is not None,
            "would_create_m57_03_artifact": preflight_passed,
            "would_record_human_selection": bool(dry_summary.get("human_selection_recorded")),
            "would_record_human_acceptance": bool(dry_summary.get("human_acceptance_recorded")),
            "would_record_g_zone_decision": bool(dry_summary.get("g_zone_decision_recorded")),
            "would_declare_recipe_valid": bool(dry_summary.get("declares_recipe_valid")),
            "would_allow_runtime_promotion": bool(dry_summary.get("runtime_promotion_allowed")),
            "would_be_ready_for_m57_04": bool(dry_summary.get("ready_for_m57_04")),
            "accepted_recipe_id": dry_summary.get("accepted_recipe_id", ""),
            "accepted_review_item_id": dry_summary.get("accepted_review_item_id", ""),
            "accepted_combined_repair_package_id": dry_summary.get("accepted_combined_repair_package_id", ""),
            "acceptance_decision": dry_record.get("decision", ""),
            "real_command": _acceptance_command(acceptance_text) if preflight_passed else "",
        },
        "issues": issues,
        "decision": {
            "preflight_report_ready": True,
            "preflight_passed": preflight_passed,
            "ready_for_real_m57_03_command": preflight_passed,
            "human_acceptance_recorded": False,
            "runtime_promotion_allowed": False,
            "recommended_milestone": "M57-03" if preflight_passed else "M57-03-user-acceptance",
            "recommended_task": (
                "Run the real M57-03 accepted repair artifact command with the preflighted acceptance text"
                if preflight_passed
                else "Provide non-empty acceptance_text, then rerun preflight"
            ),
        },
        "summary": {
            "request_ready": bool(request_packet.get("summary", {}).get("acceptance_request_ready")),
            "selected_review_item_id": request_packet.get("summary", {}).get("selected_review_item_id", ""),
            "selected_recipe_id": request_packet.get("summary", {}).get("selected_recipe_id", ""),
            "issue_count": len(issues),
            "blocking_issue_count": len(blocking_issues),
            "input_issue_count": len(input_issues),
            "preflight_passed": preflight_passed,
            "ready_for_real_m57_03_command": preflight_passed,
            "accepted_recipe_id": dry_summary.get("accepted_recipe_id", "") if preflight_passed else "",
            "human_acceptance_recorded": False,
        },
        "next_target": {
            "milestone": "M57-03" if preflight_passed else "M57-03-user-acceptance",
            "task": (
                "Run real sixth-slice human-accepted repair artifact"
                if preflight_passed
                else "Provide explicit acceptance text for the selected sixth-slice repair"
            ),
        },
    }


def _write_text_lf(path: Path, text: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="\n") as handle:
        handle.write(text)


def write_json(report: dict[str, Any], path: Path) -> None:
    _write_text_lf(path, json.dumps(report, ensure_ascii=False, indent=2) + "\n")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    dry_run = report["dry_run"]
    lines = [
        "# M57-03 Sixth-Slice Human Acceptance Preflight",
        "",
        "## Summary",
        "",
        f"- Request ready: `{summary['request_ready']}`",
        f"- Selected review item: `{summary['selected_review_item_id']}`",
        f"- Selected recipe: `{summary['selected_recipe_id']}`",
        f"- Issue count: `{summary['issue_count']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        f"- Input issues: `{summary['input_issue_count']}`",
        f"- Preflight passed: `{summary['preflight_passed']}`",
        f"- Ready for real M57-03 command: `{summary['ready_for_real_m57_03_command']}`",
        f"- Human acceptance recorded: `{summary['human_acceptance_recorded']}`",
        "",
        "## Dry Run",
        "",
        f"- Executed: `{dry_run['executed']}`",
        f"- Would create M57-03 artifact: `{dry_run['would_create_m57_03_artifact']}`",
        f"- Would record human selection: `{dry_run['would_record_human_selection']}`",
        f"- Would record human acceptance: `{dry_run['would_record_human_acceptance']}`",
        f"- Would record G Zone decision: `{dry_run['would_record_g_zone_decision']}`",
        f"- Would declare recipe valid: `{dry_run['would_declare_recipe_valid']}`",
        f"- Would allow runtime promotion: `{dry_run['would_allow_runtime_promotion']}`",
        f"- Would be ready for M57-04: `{dry_run['would_be_ready_for_m57_04']}`",
        f"- Accepted recipe: `{dry_run['accepted_recipe_id']}`",
        "",
    ]
    if dry_run["real_command"]:
        lines.extend(["## Real Command", "", "```powershell", dry_run["real_command"], "```", ""])
    lines.extend(["## Issues", ""])
    if report["issues"]:
        for issue in report["issues"]:
            lines.append(f"- `{issue['code']}` severity=`{issue['severity']}` details=`{issue['details']}`")
    else:
        lines.append("- None")
    lines.extend(
        [
            "",
            "## Boundary",
            "",
            "- This preflight does not create the real M57-03 accepted artifact.",
            "- This preflight does not record human acceptance.",
            "- This preflight does not record a G Zone / Stride decision.",
            "- This preflight does not declare the recipe valid.",
            "- This preflight does not create a runtime fixture.",
            "- This preflight does not publish saved decks, UI deck lists, or bot playbooks.",
            "- This preflight does not mutate GameState.",
            "",
            "## Next",
            "",
            f"`{decision['recommended_milestone']}`: {decision['recommended_task']}.",
            "",
        ]
    )
    _write_text_lf(path, "\n".join(lines))


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M57-03 human acceptance preflight report.")
    parser.add_argument("--acceptance-text", default="", help="Optional non-empty acceptance text to dry-run.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_sixth_slice_human_acceptance_preflight(
        acceptance_text=args.acceptance_text,
    )
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M57-03 acceptance preflight wrote {json_path}")
    print(f"M57-03 acceptance preflight summary wrote {md_path}")
    print(
        "preflight_passed={passed} blockers={blockers} input_issues={inputs} next={next_target}".format(
            passed=report["summary"]["preflight_passed"],
            blockers=report["summary"]["blocking_issue_count"],
            inputs=report["summary"]["input_issue_count"],
            next_target=report["next_target"]["milestone"],
        )
    )
    return 0 if report["summary"]["blocking_issue_count"] == 0 else 1


if __name__ == "__main__":
    raise SystemExit(main())
