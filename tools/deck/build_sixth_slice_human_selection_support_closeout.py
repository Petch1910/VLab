"""Build a read-only closeout for M57-02 selection support artifacts.

This closes the AI-generated support work around M57-02 without creating the
real human-selected artifact. The next action remains explicit human/team
selection.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
REQUEST_PACKET = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_request_packet.json"
PREFLIGHT_REPORT = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_preflight.json"
CANDIDATE_DIGEST = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_candidate_digest.json"
BATCH_PREFLIGHT = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_batch_preflight_matrix.json"
JSON_OUTPUT = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_support_closeout.json"
MD_OUTPUT = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_support_closeout.md"


if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


from tools.deck.build_sixth_slice_human_selection_request_packet import load_json  # noqa: E402


def _issue(code: str, severity: str, message: str, details: dict[str, Any] | None = None) -> dict[str, Any]:
    return {
        "code": code,
        "severity": severity,
        "message": message,
        "details": details or {},
    }


def build_sixth_slice_human_selection_support_closeout(
    request_packet: dict[str, Any] | None = None,
    preflight_report: dict[str, Any] | None = None,
    candidate_digest: dict[str, Any] | None = None,
    batch_preflight: dict[str, Any] | None = None,
) -> dict[str, Any]:
    request_packet = request_packet or load_json(REQUEST_PACKET)
    preflight_report = preflight_report or load_json(PREFLIGHT_REPORT)
    candidate_digest = candidate_digest or load_json(CANDIDATE_DIGEST)
    batch_preflight = batch_preflight or load_json(BATCH_PREFLIGHT)
    issues: list[dict[str, Any]] = []

    request_summary = request_packet.get("summary", {})
    preflight_summary = preflight_report.get("summary", {})
    digest_summary = candidate_digest.get("summary", {})
    batch_summary = batch_preflight.get("summary", {})

    if not request_summary.get("selection_request_ready"):
        issues.append(
            _issue(
                "selection_request_not_ready",
                "blocker",
                "M57-02 request packet is not ready.",
                {"summary": request_summary},
            )
        )
    if int(request_summary.get("ready_candidate_count", 0)) != 12:
        issues.append(
            _issue(
                "unexpected_ready_candidate_count",
                "blocker",
                "Expected exactly twelve ready M57-02 candidates.",
                {"ready_candidate_count": request_summary.get("ready_candidate_count")},
            )
        )
    if not preflight_report.get("decision", {}).get("preflight_report_ready"):
        issues.append(
            _issue(
                "preflight_report_not_ready",
                "blocker",
                "M57-02 preflight report is not ready.",
                {"decision": preflight_report.get("decision", {})},
            )
        )
    if preflight_summary.get("human_selection_recorded"):
        issues.append(
            _issue(
                "unexpected_preflight_selection",
                "blocker",
                "Preflight report must not record human selection.",
                {"summary": preflight_summary},
            )
        )
    if not digest_summary.get("ready_for_user_selection_decision"):
        issues.append(
            _issue(
                "candidate_digest_not_ready",
                "blocker",
                "Candidate digest is not ready for human/team selection.",
                {"summary": digest_summary},
            )
        )
    if digest_summary.get("human_selection_recorded"):
        issues.append(
            _issue(
                "unexpected_digest_selection",
                "blocker",
                "Candidate digest must not record human selection.",
                {"summary": digest_summary},
            )
        )
    if not batch_summary.get("all_candidates_pass_preflight"):
        issues.append(
            _issue(
                "batch_preflight_not_all_passed",
                "blocker",
                "Batch preflight did not pass for every candidate.",
                {"summary": batch_summary},
            )
        )
    if int(batch_summary.get("preflight_passed_count", 0)) != int(request_summary.get("ready_candidate_count", 0)):
        issues.append(
            _issue(
                "preflight_pass_count_mismatch",
                "blocker",
                "Batch preflight pass count does not match ready candidate count.",
                {
                    "ready_candidate_count": request_summary.get("ready_candidate_count"),
                    "preflight_passed_count": batch_summary.get("preflight_passed_count"),
                },
            )
        )

    candidate_rows = candidate_digest.get("comparison_rows", [])
    first_candidate = candidate_rows[0] if candidate_rows else {}
    command_template = request_packet.get("selection_policy", {}).get("m57_02_command_template", "")

    return {
        "version": "M57-02-selection-support-closeout",
        "description": "Sixth-slice human selection support closeout",
        "source_inputs": {
            "m57_02_selection_request_packet": str(REQUEST_PACKET.relative_to(ROOT)),
            "m57_02_selection_preflight": str(PREFLIGHT_REPORT.relative_to(ROOT)),
            "m57_02_candidate_digest": str(CANDIDATE_DIGEST.relative_to(ROOT)),
            "m57_02_batch_preflight_matrix": str(BATCH_PREFLIGHT.relative_to(ROOT)),
        },
        "scope": {
            "read_only_support_closeout": True,
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
        "evidence": {
            "request_ready": bool(request_summary.get("selection_request_ready")),
            "ready_candidate_count": int(request_summary.get("ready_candidate_count", 0)),
            "preflight_report_ready": bool(preflight_report.get("decision", {}).get("preflight_report_ready")),
            "default_preflight_requires_input": int(preflight_summary.get("input_issue_count", 0)) > 0,
            "digest_ready": bool(digest_summary.get("ready_for_user_selection_decision")),
            "digest_source_group_count": int(digest_summary.get("source_group_count", 0)),
            "digest_target_group_count": int(digest_summary.get("target_group_count", 0)),
            "batch_all_candidates_pass": bool(batch_summary.get("all_candidates_pass_preflight")),
            "batch_preflight_passed_count": int(batch_summary.get("preflight_passed_count", 0)),
            "batch_preflight_failed_count": int(batch_summary.get("preflight_failed_count", 0)),
            "human_selection_recorded_anywhere": any(
                bool(summary.get("human_selection_recorded"))
                for summary in [request_summary, preflight_summary, digest_summary, batch_summary]
            ),
        },
        "handoff": {
            "requires_user_or_team_selection": True,
            "requires_non_empty_selection_text": True,
            "real_m57_02_artifact_blocked_until_selection": True,
            "safe_next_milestone": "M57-02-user-selection",
            "command_template": command_template,
            "example_candidate_not_selected": {
                "review_item_id": first_candidate.get("review_item_id", ""),
                "recipe_id": first_candidate.get("recipe_id", ""),
                "source_candidate_edge": first_candidate.get("source_candidate_edge", ""),
            },
        },
        "issues": issues,
        "summary": {
            "support_closeout_complete": not issues,
            "blocking_issue_count": len(issues),
            "ready_candidate_count": int(request_summary.get("ready_candidate_count", 0)),
            "batch_preflight_passed_count": int(batch_summary.get("preflight_passed_count", 0)),
            "batch_preflight_failed_count": int(batch_summary.get("preflight_failed_count", 0)),
            "human_selection_recorded": False,
            "real_m57_02_artifact_created": False,
            "ready_for_user_selection": not issues,
        },
        "next_target": {
            "milestone": "M57-02-user-selection" if not issues else "M57-02-support-repair",
            "task": (
                "User/team chooses one review_item_id and provides non-empty selection_text"
                if not issues
                else "Repair M57-02 support artifacts before user selection"
            ),
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    evidence = report["evidence"]
    handoff = report["handoff"]
    lines = [
        "# M57-02 Sixth-Slice Human Selection Support Closeout",
        "",
        "## Summary",
        "",
        f"- Support closeout complete: `{summary['support_closeout_complete']}`",
        f"- Ready candidates: `{summary['ready_candidate_count']}`",
        f"- Batch preflight passed: `{summary['batch_preflight_passed_count']}`",
        f"- Batch preflight failed: `{summary['batch_preflight_failed_count']}`",
        f"- Human selection recorded: `{summary['human_selection_recorded']}`",
        f"- Real M57-02 artifact created: `{summary['real_m57_02_artifact_created']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        "",
        "## Evidence",
        "",
        f"- Request ready: `{evidence['request_ready']}`",
        f"- Preflight report ready: `{evidence['preflight_report_ready']}`",
        f"- Default preflight requires input: `{evidence['default_preflight_requires_input']}`",
        f"- Digest ready: `{evidence['digest_ready']}`",
        f"- Digest source groups: `{evidence['digest_source_group_count']}`",
        f"- Digest target groups: `{evidence['digest_target_group_count']}`",
        f"- Batch all candidates pass: `{evidence['batch_all_candidates_pass']}`",
        f"- Human selection recorded anywhere: `{evidence['human_selection_recorded_anywhere']}`",
        "",
        "## Required Human/Team Action",
        "",
        "Choose exactly one ready `review_item_id` and provide non-empty `selection_text`.",
        "",
        "```powershell",
        handoff["command_template"],
        "```",
        "",
        "## Boundary",
        "",
        "- This closeout does not choose a review item.",
        "- This closeout does not record human selection or acceptance.",
        "- This closeout does not record a G Zone / Stride decision.",
        "- This closeout does not create the real M57-02 selected artifact.",
        "- This closeout does not create a runtime fixture, publish decks, enable bot playbooks, or mutate GameState.",
        "",
        "## Issues",
        "",
    ]
    if report["issues"]:
        for issue in report["issues"]:
            lines.append(f"- `{issue['code']}` severity=`{issue['severity']}` details=`{issue['details']}`")
    else:
        lines.append("- None")
    lines.extend(
        [
            "",
            "## Next",
            "",
            f"`{report['next_target']['milestone']}`: {report['next_target']['task']}.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M57-02 human selection support closeout.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_sixth_slice_human_selection_support_closeout()
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M57-02 selection support closeout wrote {json_path}")
    print(f"M57-02 selection support closeout summary wrote {md_path}")
    print(
        "support_closeout_complete={complete} ready_candidates={ready} batch_passed={passed} next={next_target}".format(
            complete=report["summary"]["support_closeout_complete"],
            ready=report["summary"]["ready_candidate_count"],
            passed=report["summary"]["batch_preflight_passed_count"],
            next_target=report["next_target"]["milestone"],
        )
    )
    return 0 if report["summary"]["support_closeout_complete"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
