"""Build a read-only M57-02 human selection preflight report.

The real M57-02 artifact requires explicit user/team selection. This tool
checks whether a proposed selection would satisfy the M57-02 generator contract
without writing the selected artifact.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
JSON_OUTPUT = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_preflight.json"
MD_OUTPUT = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_preflight.md"

if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


from tools.deck.build_sixth_slice_human_selection_request_packet import (  # noqa: E402
    build_sixth_slice_human_selection_request_packet,
    load_json,
)
from tools.deck.build_sixth_slice_human_selected_recipe_artifact import (  # noqa: E402
    M57_01_REVIEW,
    build_sixth_slice_human_selected_recipe_artifact,
)


def _candidate_by_id(request_packet: dict[str, Any]) -> dict[str, dict[str, Any]]:
    return {
        candidate.get("review_item_id", ""): candidate
        for candidate in request_packet.get("candidates", [])
        if candidate.get("review_item_id")
    }


def _issue(code: str, severity: str, message: str, details: dict[str, Any] | None = None) -> dict[str, Any]:
    return {
        "code": code,
        "severity": severity,
        "message": message,
        "details": details or {},
    }


def _selection_command(review_item_id: str, selection_text: str) -> str:
    return (
        "python tools\\deck\\build_sixth_slice_human_selected_recipe_artifact.py "
        f"--review-item-id {review_item_id} "
        f'--selection-text "{selection_text}"'
    )


def build_sixth_slice_human_selection_preflight(
    request_packet: dict[str, Any] | None = None,
    review_packet: dict[str, Any] | None = None,
    *,
    review_item_id: str = "",
    selection_text: str = "",
) -> dict[str, Any]:
    request_packet = request_packet or build_sixth_slice_human_selection_request_packet()
    review_packet = review_packet or load_json(M57_01_REVIEW)
    review_item_id = review_item_id.strip()
    selection_text = selection_text.strip()
    candidates = _candidate_by_id(request_packet)
    issues: list[dict[str, Any]] = []
    dry_run_artifact: dict[str, Any] | None = None

    if not request_packet.get("summary", {}).get("selection_request_ready"):
        issues.append(
            _issue(
                "selection_request_not_ready",
                "blocker",
                "M57-02 request packet is not ready for selection.",
                {"request_summary": request_packet.get("summary", {})},
            )
        )

    if not review_item_id:
        issues.append(
            _issue(
                "missing_review_item_id",
                "requires_input",
                "Preflight needs an explicit M57-01 review_item_id to dry-run the real M57-02 command.",
            )
        )
    elif review_item_id not in candidates:
        issues.append(
            _issue(
                "unknown_review_item_id",
                "blocker",
                "The selected review_item_id is not present in the M57-02 request packet.",
                {"review_item_id": review_item_id},
            )
        )
    elif not candidates[review_item_id].get("ready_for_user_selection"):
        issues.append(
            _issue(
                "candidate_not_ready",
                "blocker",
                "The selected review item is present but is not ready for user selection.",
                {"review_item_id": review_item_id},
            )
        )

    if not selection_text:
        issues.append(
            _issue(
                "missing_selection_text",
                "requires_input",
                "Preflight needs non-empty selection_text to dry-run the real M57-02 command.",
            )
        )

    blocking_issues = [issue for issue in issues if issue["severity"] == "blocker"]
    input_issues = [issue for issue in issues if issue["severity"] == "requires_input"]
    if not blocking_issues and not input_issues:
        try:
            dry_run_artifact = build_sixth_slice_human_selected_recipe_artifact(
                review_packet,
                selected_review_item_id=review_item_id,
                selection_text=selection_text,
                selected_by="preflight-dry-run",
                selected_at="preflight",
            )
            if not dry_run_artifact.get("summary", {}).get("ready_for_m57_03"):
                issues.append(
                    _issue(
                        "dry_run_not_ready_for_m57_03",
                        "blocker",
                        "The real M57-02 artifact would be generated but would not be ready for M57-03.",
                        {"summary": dry_run_artifact.get("summary", {})},
                    )
                )
        except (KeyError, ValueError) as exc:
            issues.append(
                _issue(
                    "dry_run_generator_rejected_selection",
                    "blocker",
                    "The real M57-02 generator rejected the proposed selection.",
                    {"error": str(exc)},
                )
            )

    blocking_issues = [issue for issue in issues if issue["severity"] == "blocker"]
    input_issues = [issue for issue in issues if issue["severity"] == "requires_input"]
    preflight_passed = not blocking_issues and not input_issues and dry_run_artifact is not None
    selected_summary = dry_run_artifact.get("summary", {}) if dry_run_artifact else {}
    selected_selection = dry_run_artifact.get("selection", {}) if dry_run_artifact else {}

    return {
        "version": "M57-02-preflight",
        "description": "Sixth-slice human selection preflight report",
        "source_inputs": {
            "m57_02_selection_request_packet": str(
                (OUTPUT_DIR / "m57_02_sixth_slice_human_selection_request_packet.json").relative_to(ROOT)
            ),
            "m57_01_human_repair_review_packet": str(M57_01_REVIEW.relative_to(ROOT)),
        },
        "scope": {
            "read_only_preflight": True,
            "records_human_selection": False,
            "records_human_acceptance": False,
            "records_g_zone_decision": False,
            "creates_m57_02_artifact": False,
            "changes_recipe_draft_file": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "direct_GameState_mutation": False,
        },
        "input": {
            "review_item_id": review_item_id,
            "selection_text_provided": bool(selection_text),
            "selection_text_preview": selection_text if selection_text else "",
        },
        "candidate": candidates.get(review_item_id, {}) if review_item_id else {},
        "dry_run": {
            "executed": dry_run_artifact is not None,
            "would_create_m57_02_artifact": preflight_passed,
            "would_record_human_selection": bool(selected_summary.get("records_human_selection")),
            "would_record_human_acceptance": bool(selected_summary.get("records_human_acceptance")),
            "would_record_g_zone_decision": bool(selected_summary.get("records_g_zone_decision")),
            "would_allow_runtime_promotion": bool(selected_summary.get("runtime_promotion_allowed")),
            "would_be_ready_for_m57_03": bool(selected_summary.get("ready_for_m57_03")),
            "selected_recipe_id": selected_summary.get("selected_recipe_id", ""),
            "source_candidate_edge": selected_selection.get("source_candidate_edge", ""),
            "real_command": _selection_command(review_item_id, selection_text) if preflight_passed else "",
        },
        "issues": issues,
        "decision": {
            "preflight_report_ready": True,
            "preflight_passed": preflight_passed,
            "ready_for_real_m57_02_command": preflight_passed,
            "human_selection_recorded": False,
            "runtime_promotion_allowed": False,
            "recommended_milestone": "M57-02" if preflight_passed else "M57-02-user-selection",
            "recommended_task": (
                "Run the real M57-02 selected recipe artifact command with the preflighted inputs"
                if preflight_passed
                else "Provide one ready review_item_id and non-empty selection_text, then rerun preflight"
            ),
        },
        "summary": {
            "request_ready": bool(request_packet.get("summary", {}).get("selection_request_ready")),
            "ready_candidate_count": int(request_packet.get("summary", {}).get("ready_candidate_count", 0)),
            "issue_count": len(issues),
            "blocking_issue_count": len(blocking_issues),
            "input_issue_count": len(input_issues),
            "preflight_passed": preflight_passed,
            "ready_for_real_m57_02_command": preflight_passed,
            "selected_review_item_id": review_item_id if preflight_passed else "",
            "selected_recipe_id": selected_summary.get("selected_recipe_id", "") if preflight_passed else "",
            "human_selection_recorded": False,
        },
        "next_target": {
            "milestone": "M57-02" if preflight_passed else "M57-02-user-selection",
            "task": (
                "Run real sixth-slice human-selected recipe artifact"
                if preflight_passed
                else "Choose one ready sixth-slice review item and provide selection text"
            ),
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    dry_run = report["dry_run"]
    lines = [
        "# M57-02 Sixth-Slice Human Selection Preflight",
        "",
        "## Summary",
        "",
        f"- Request ready: `{summary['request_ready']}`",
        f"- Ready candidates: `{summary['ready_candidate_count']}`",
        f"- Issue count: `{summary['issue_count']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        f"- Input issues: `{summary['input_issue_count']}`",
        f"- Preflight passed: `{summary['preflight_passed']}`",
        f"- Ready for real M57-02 command: `{summary['ready_for_real_m57_02_command']}`",
        f"- Human selection recorded: `{summary['human_selection_recorded']}`",
        "",
        "## Dry Run",
        "",
        f"- Executed: `{dry_run['executed']}`",
        f"- Would create M57-02 artifact: `{dry_run['would_create_m57_02_artifact']}`",
        f"- Would record human selection: `{dry_run['would_record_human_selection']}`",
        f"- Would record human acceptance: `{dry_run['would_record_human_acceptance']}`",
        f"- Would record G Zone decision: `{dry_run['would_record_g_zone_decision']}`",
        f"- Would allow runtime promotion: `{dry_run['would_allow_runtime_promotion']}`",
        f"- Would be ready for M57-03: `{dry_run['would_be_ready_for_m57_03']}`",
        f"- Selected recipe: `{dry_run['selected_recipe_id']}`",
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
            "- This preflight does not create the real M57-02 selected artifact.",
            "- This preflight does not record human selection or acceptance.",
            "- This preflight does not record a G Zone / Stride decision.",
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
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M57-02 human selection preflight report.")
    parser.add_argument("--review-item-id", default="", help="Optional M57-01 review_item_id to dry-run.")
    parser.add_argument("--selection-text", default="", help="Optional non-empty selection text to dry-run.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_sixth_slice_human_selection_preflight(
        review_item_id=args.review_item_id,
        selection_text=args.selection_text,
    )
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M57-02 selection preflight wrote {json_path}")
    print(f"M57-02 selection preflight summary wrote {md_path}")
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
