"""Build a read-only closeout for M57-03 acceptance support artifacts.

This closes the AI-generated support work around M57-03 without creating the
real human-accepted artifact. The next action remains explicit human/team
acceptance text.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
REQUEST_PACKET = OUTPUT_DIR / "m57_03_sixth_slice_human_acceptance_request_packet.json"
PREFLIGHT_REPORT = OUTPUT_DIR / "m57_03_sixth_slice_human_acceptance_preflight.json"
ACCEPTED_ARTIFACT = OUTPUT_DIR / "m57_03_sixth_slice_human_accepted_repair_artifact.json"
JSON_OUTPUT = OUTPUT_DIR / "m57_03_sixth_slice_human_acceptance_support_closeout.json"
MD_OUTPUT = OUTPUT_DIR / "m57_03_sixth_slice_human_acceptance_support_closeout.md"


if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


from tools.deck.build_sixth_slice_human_acceptance_request_packet import load_json  # noqa: E402


def _issue(code: str, severity: str, message: str, details: dict[str, Any] | None = None) -> dict[str, Any]:
    return {
        "code": code,
        "severity": severity,
        "message": message,
        "details": details or {},
    }


def build_sixth_slice_human_acceptance_support_closeout(
    request_packet: dict[str, Any] | None = None,
    preflight_report: dict[str, Any] | None = None,
    *,
    accepted_artifact_exists: bool | None = None,
) -> dict[str, Any]:
    request_packet = request_packet or load_json(REQUEST_PACKET)
    preflight_report = preflight_report or load_json(PREFLIGHT_REPORT)
    accepted_artifact_exists = ACCEPTED_ARTIFACT.exists() if accepted_artifact_exists is None else accepted_artifact_exists
    issues: list[dict[str, Any]] = []

    request_summary = request_packet.get("summary", {})
    preflight_summary = preflight_report.get("summary", {})
    preflight_decision = preflight_report.get("decision", {})
    request_scope = request_packet.get("scope", {})
    preflight_scope = preflight_report.get("scope", {})

    if not request_summary.get("acceptance_request_ready"):
        issues.append(
            _issue(
                "acceptance_request_not_ready",
                "blocker",
                "M57-03 acceptance request packet is not ready.",
                {"summary": request_summary},
            )
        )
    if not request_summary.get("selected_recipe_id"):
        issues.append(
            _issue(
                "missing_selected_recipe",
                "blocker",
                "M57-03 acceptance request does not identify a selected recipe.",
                {"summary": request_summary},
            )
        )
    if int(request_summary.get("acceptance_option_count", 0)) != 1:
        issues.append(
            _issue(
                "unexpected_acceptance_option_count",
                "blocker",
                "Expected exactly one M57-03 acceptance option.",
                {"acceptance_option_count": request_summary.get("acceptance_option_count")},
            )
        )
    if request_summary.get("human_acceptance_recorded") or request_scope.get("records_human_acceptance"):
        issues.append(
            _issue(
                "unexpected_request_acceptance_recorded",
                "blocker",
                "Acceptance request packet must not record human acceptance.",
                {"summary": request_summary, "scope": request_scope},
            )
        )
    if not preflight_decision.get("preflight_report_ready"):
        issues.append(
            _issue(
                "preflight_report_not_ready",
                "blocker",
                "M57-03 acceptance preflight report is not ready.",
                {"decision": preflight_decision},
            )
        )
    if int(preflight_summary.get("blocking_issue_count", 0)) != 0:
        issues.append(
            _issue(
                "preflight_has_blockers",
                "blocker",
                "M57-03 acceptance preflight has blocking issues.",
                {"summary": preflight_summary},
            )
        )
    if preflight_summary.get("human_acceptance_recorded") or preflight_scope.get("records_human_acceptance"):
        issues.append(
            _issue(
                "unexpected_preflight_acceptance_recorded",
                "blocker",
                "Acceptance preflight must not record human acceptance.",
                {"summary": preflight_summary, "scope": preflight_scope},
            )
        )
    if accepted_artifact_exists:
        issues.append(
            _issue(
                "real_m57_03_artifact_already_exists",
                "blocker",
                "Support closeout expected no real M57-03 accepted artifact yet.",
                {"artifact": str(ACCEPTED_ARTIFACT.relative_to(ROOT))},
            )
        )

    default_preflight_requires_input = (
        not preflight_summary.get("preflight_passed")
        and int(preflight_summary.get("input_issue_count", 0)) > 0
    )
    human_acceptance_recorded_anywhere = any(
        bool(value)
        for value in [
            request_summary.get("human_acceptance_recorded"),
            request_scope.get("records_human_acceptance"),
            preflight_summary.get("human_acceptance_recorded"),
            preflight_scope.get("records_human_acceptance"),
            accepted_artifact_exists,
        ]
    )

    return {
        "version": "M57-03-acceptance-support-closeout",
        "description": "Sixth-slice human acceptance support closeout",
        "source_inputs": {
            "m57_03_acceptance_request_packet": str(REQUEST_PACKET.relative_to(ROOT)),
            "m57_03_acceptance_preflight": str(PREFLIGHT_REPORT.relative_to(ROOT)),
        },
        "scope": {
            "read_only_support_closeout": True,
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
        "evidence": {
            "request_ready": bool(request_summary.get("acceptance_request_ready")),
            "selected_review_item_id": request_summary.get("selected_review_item_id", ""),
            "selected_recipe_id": request_summary.get("selected_recipe_id", ""),
            "source_card_id": request_summary.get("source_card_id", ""),
            "target_card_id": request_summary.get("target_card_id", ""),
            "decision_option_count": int(request_summary.get("decision_option_count", 0)),
            "acceptance_option_count": int(request_summary.get("acceptance_option_count", 0)),
            "preflight_report_ready": bool(preflight_decision.get("preflight_report_ready")),
            "default_preflight_requires_input": default_preflight_requires_input,
            "preflight_input_issue_count": int(preflight_summary.get("input_issue_count", 0)),
            "preflight_blocking_issue_count": int(preflight_summary.get("blocking_issue_count", 0)),
            "preflight_passed": bool(preflight_summary.get("preflight_passed")),
            "real_m57_03_artifact_created": accepted_artifact_exists,
            "human_acceptance_recorded_anywhere": human_acceptance_recorded_anywhere,
        },
        "handoff": {
            "requires_explicit_acceptance_text": True,
            "selection_text_is_not_acceptance_text": True,
            "real_m57_03_artifact_blocked_until_acceptance": not accepted_artifact_exists,
            "safe_next_milestone": "M57-03-user-acceptance",
            "preflight_command_template": (
                "python tools\\deck\\build_sixth_slice_human_acceptance_preflight.py "
                '--acceptance-text "<explicit user/team acceptance text>"'
            ),
            "real_command_template": request_packet.get("acceptance_policy", {}).get("m57_03_command_template", ""),
        },
        "issues": issues,
        "summary": {
            "support_closeout_complete": not issues,
            "blocking_issue_count": len(issues),
            "acceptance_request_ready": bool(request_summary.get("acceptance_request_ready")),
            "preflight_report_ready": bool(preflight_decision.get("preflight_report_ready")),
            "default_preflight_requires_input": default_preflight_requires_input,
            "human_acceptance_recorded": False,
            "real_m57_03_artifact_created": accepted_artifact_exists,
            "ready_for_user_acceptance": not issues,
        },
        "next_target": {
            "milestone": "M57-03-user-acceptance" if not issues else "M57-03-support-repair",
            "task": (
                "User/team provides non-empty acceptance_text"
                if not issues
                else "Repair M57-03 support artifacts before user acceptance"
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
    evidence = report["evidence"]
    handoff = report["handoff"]
    lines = [
        "# M57-03 Sixth-Slice Human Acceptance Support Closeout",
        "",
        "## Summary",
        "",
        f"- Support closeout complete: `{summary['support_closeout_complete']}`",
        f"- Acceptance request ready: `{summary['acceptance_request_ready']}`",
        f"- Preflight report ready: `{summary['preflight_report_ready']}`",
        f"- Default preflight requires input: `{summary['default_preflight_requires_input']}`",
        f"- Human acceptance recorded: `{summary['human_acceptance_recorded']}`",
        f"- Real M57-03 artifact created: `{summary['real_m57_03_artifact_created']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        "",
        "## Evidence",
        "",
        f"- Selected review item: `{evidence['selected_review_item_id']}`",
        f"- Selected recipe: `{evidence['selected_recipe_id']}`",
        f"- Pair: `{evidence['source_card_id']}` -> `{evidence['target_card_id']}`",
        f"- Decision options: `{evidence['decision_option_count']}`",
        f"- Acceptance options: `{evidence['acceptance_option_count']}`",
        f"- Preflight input issues: `{evidence['preflight_input_issue_count']}`",
        f"- Preflight blockers: `{evidence['preflight_blocking_issue_count']}`",
        f"- Human acceptance recorded anywhere: `{evidence['human_acceptance_recorded_anywhere']}`",
        "",
        "## Required Human/Team Action",
        "",
        "Provide non-empty `acceptance_text`, then dry-run it first:",
        "",
        "```powershell",
        handoff["preflight_command_template"],
        "```",
        "",
        "If the preflight passes, run the real command emitted by the preflight.",
        "",
        "## Boundary",
        "",
        "- This closeout does not record human acceptance.",
        "- This closeout does not record a G Zone / Stride decision.",
        "- This closeout does not create the real M57-03 accepted artifact.",
        "- This closeout does not declare the recipe valid.",
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
    _write_text_lf(path, "\n".join(lines))


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M57-03 human acceptance support closeout.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_sixth_slice_human_acceptance_support_closeout()
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M57-03 acceptance support closeout wrote {json_path}")
    print(f"M57-03 acceptance support closeout summary wrote {md_path}")
    print(
        "support_closeout_complete={complete} acceptance_request_ready={request_ready} "
        "preflight_report_ready={preflight_ready} next={next_target}".format(
            complete=report["summary"]["support_closeout_complete"],
            request_ready=report["summary"]["acceptance_request_ready"],
            preflight_ready=report["summary"]["preflight_report_ready"],
            next_target=report["next_target"]["milestone"],
        )
    )
    return 0 if report["summary"]["support_closeout_complete"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
