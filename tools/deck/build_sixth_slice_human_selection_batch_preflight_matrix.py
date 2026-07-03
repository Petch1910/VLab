"""Build a read-only batch preflight matrix for all M57-02 candidates.

This checks every ready candidate against the real M57-02 generator contract in
memory, without writing the selected artifact and without recording human
selection.
"""

from __future__ import annotations

import argparse
import csv
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
REQUEST_PACKET = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_request_packet.json"
REVIEW_PACKET = OUTPUT_DIR / "m57_01_sixth_slice_human_repair_review_packet.json"
JSON_OUTPUT = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_batch_preflight_matrix.json"
MD_OUTPUT = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_batch_preflight_matrix.md"
CSV_OUTPUT = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_batch_preflight_matrix.csv"


if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


from tools.deck.build_sixth_slice_human_selection_request_packet import load_json  # noqa: E402
from tools.deck.build_sixth_slice_human_selected_recipe_artifact import (  # noqa: E402
    build_sixth_slice_human_selected_recipe_artifact,
)


def _dry_run_text(review_item_id: str) -> str:
    return f"batch preflight contract check only for {review_item_id}"


def _candidate_result(candidate: dict[str, Any], review_packet: dict[str, Any]) -> dict[str, Any]:
    review_item_id = candidate.get("review_item_id", "")
    row = {
        "order": int(candidate.get("order", 0)),
        "review_item_id": review_item_id,
        "recipe_id": candidate.get("recipe_id", ""),
        "ready_for_user_selection": bool(candidate.get("ready_for_user_selection")),
        "source_candidate_edge": candidate.get("source_candidate_edge", ""),
        "source_card_id": candidate.get("source_card_id", ""),
        "target_card_id": candidate.get("target_card_id", ""),
        "preflight_executed": False,
        "preflight_passed": False,
        "generator_accepted": False,
        "would_record_human_selection": False,
        "would_record_human_acceptance": False,
        "would_record_g_zone_decision": False,
        "would_allow_runtime_promotion": False,
        "would_be_ready_for_m57_03": False,
        "selected_recipe_id": "",
        "error_code": "",
        "error_message": "",
    }
    if not row["ready_for_user_selection"]:
        row["error_code"] = "candidate_not_ready"
        row["error_message"] = "Candidate is not ready for user selection."
        return row
    try:
        artifact = build_sixth_slice_human_selected_recipe_artifact(
            review_packet,
            selected_review_item_id=review_item_id,
            selection_text=_dry_run_text(review_item_id),
            selected_by="batch-preflight-matrix",
            selected_at="preflight",
        )
        summary = artifact.get("summary", {})
        row.update(
            {
                "preflight_executed": True,
                "generator_accepted": True,
                "would_record_human_selection": bool(summary.get("records_human_selection")),
                "would_record_human_acceptance": bool(summary.get("records_human_acceptance")),
                "would_record_g_zone_decision": bool(summary.get("records_g_zone_decision")),
                "would_allow_runtime_promotion": bool(summary.get("runtime_promotion_allowed")),
                "would_be_ready_for_m57_03": bool(summary.get("ready_for_m57_03")),
                "selected_recipe_id": summary.get("selected_recipe_id", ""),
            }
        )
        row["preflight_passed"] = (
            row["generator_accepted"]
            and row["would_record_human_selection"]
            and not row["would_record_human_acceptance"]
            and not row["would_record_g_zone_decision"]
            and not row["would_allow_runtime_promotion"]
            and row["would_be_ready_for_m57_03"]
            and row["selected_recipe_id"] == row["recipe_id"]
        )
    except (KeyError, ValueError) as exc:
        row["error_code"] = "generator_rejected_candidate"
        row["error_message"] = str(exc)
    return row


def build_sixth_slice_human_selection_batch_preflight_matrix(
    request_packet: dict[str, Any] | None = None,
    review_packet: dict[str, Any] | None = None,
) -> dict[str, Any]:
    request_packet = request_packet or load_json(REQUEST_PACKET)
    review_packet = review_packet or load_json(REVIEW_PACKET)
    candidates = request_packet.get("candidates", [])
    rows = [_candidate_result(candidate, review_packet) for candidate in candidates]
    passed_rows = [row for row in rows if row["preflight_passed"]]
    failed_rows = [row for row in rows if not row["preflight_passed"]]
    issues: list[dict[str, Any]] = []

    if not request_packet.get("summary", {}).get("selection_request_ready"):
        issues.append(
            {
                "code": "selection_request_not_ready",
                "severity": "blocker",
                "message": "Selection request packet is not ready for batch preflight.",
                "details": {"summary": request_packet.get("summary", {})},
            }
        )
    if failed_rows:
        issues.append(
            {
                "code": "candidate_preflight_failures",
                "severity": "blocker",
                "message": "One or more candidates failed the M57-02 dry-run contract.",
                "details": {
                    "failed_count": len(failed_rows),
                    "failed_review_item_ids": [row["review_item_id"] for row in failed_rows],
                },
            }
        )

    return {
        "version": "M57-02-batch-preflight-matrix",
        "description": "Sixth-slice human selection batch preflight matrix",
        "source_inputs": {
            "m57_02_selection_request_packet": str(REQUEST_PACKET.relative_to(ROOT)),
            "m57_01_human_repair_review_packet": str(REVIEW_PACKET.relative_to(ROOT)),
        },
        "scope": {
            "read_only_batch_preflight": True,
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
        "selection_policy": {
            "must_not_recommend_or_auto_select": True,
            "dry_run_text_is_not_user_selection_text": True,
            "requires_user_or_team_to_choose_one_review_item_id": True,
            "requires_non_empty_selection_text_for_real_m57_02": True,
        },
        "matrix_rows": rows,
        "issues": issues,
        "summary": {
            "candidate_count": len(rows),
            "ready_candidate_count": sum(1 for row in rows if row["ready_for_user_selection"]),
            "preflight_executed_count": sum(1 for row in rows if row["preflight_executed"]),
            "preflight_passed_count": len(passed_rows),
            "preflight_failed_count": len(failed_rows),
            "all_candidates_pass_preflight": bool(rows) and not failed_rows,
            "request_ready": bool(request_packet.get("summary", {}).get("selection_request_ready")),
            "blocking_issue_count": len(issues),
            "human_selection_recorded": False,
            "ready_for_user_selection_decision": bool(rows) and not issues,
        },
        "next_target": {
            "milestone": "M57-02-user-selection" if not issues else "M57-02-prerequisite-repair",
            "task": (
                "Choose one preflight-passing review_item_id and provide real selection_text"
                if not issues
                else "Repair failed preflight candidates before user selection"
            ),
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_csv(report: dict[str, Any], path: Path) -> None:
    fieldnames = [
        "order",
        "review_item_id",
        "recipe_id",
        "source_candidate_edge",
        "preflight_passed",
        "generator_accepted",
        "would_be_ready_for_m57_03",
        "would_record_human_selection",
        "would_record_human_acceptance",
        "would_record_g_zone_decision",
        "would_allow_runtime_promotion",
        "error_code",
        "error_message",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        for row in report["matrix_rows"]:
            writer.writerow({field: row.get(field, "") for field in fieldnames})


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M57-02 Sixth-Slice Human Selection Batch Preflight Matrix",
        "",
        "## Summary",
        "",
        f"- Candidates: `{summary['candidate_count']}`",
        f"- Ready candidates: `{summary['ready_candidate_count']}`",
        f"- Preflight executed: `{summary['preflight_executed_count']}`",
        f"- Preflight passed: `{summary['preflight_passed_count']}`",
        f"- Preflight failed: `{summary['preflight_failed_count']}`",
        f"- All candidates pass: `{summary['all_candidates_pass_preflight']}`",
        f"- Human selection recorded: `{summary['human_selection_recorded']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        "",
        "## Matrix",
        "",
    ]
    for row in report["matrix_rows"]:
        lines.append(
            "- `{order}` `{review}` recipe=`{recipe}` edge=`{edge}` pass=`{passed}` ready_m57_03=`{ready}` error=`{error}`".format(
                order=row["order"],
                review=row["review_item_id"],
                recipe=row["recipe_id"],
                edge=row["source_candidate_edge"],
                passed=row["preflight_passed"],
                ready=row["would_be_ready_for_m57_03"],
                error=row["error_code"],
            )
        )
    lines.extend(["", "## Issues", ""])
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
            "- This matrix does not choose a review item.",
            "- This matrix does not record human selection or acceptance.",
            "- This matrix does not record a G Zone / Stride decision.",
            "- This matrix does not create the real M57-02 selected artifact.",
            "- This matrix does not create a runtime fixture, publish decks, enable bot playbooks, or mutate GameState.",
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
    parser = argparse.ArgumentParser(description="Build M57-02 batch preflight matrix.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_sixth_slice_human_selection_batch_preflight_matrix()
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    csv_path = args.output_dir / CSV_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    write_csv(report, csv_path)
    print(f"M57-02 batch preflight matrix wrote {json_path}")
    print(f"M57-02 batch preflight matrix summary wrote {md_path}")
    print(f"M57-02 batch preflight matrix CSV wrote {csv_path}")
    print(
        "preflight_passed={passed}/{total} failures={failed} next={next_target}".format(
            passed=report["summary"]["preflight_passed_count"],
            total=report["summary"]["candidate_count"],
            failed=report["summary"]["preflight_failed_count"],
            next_target=report["next_target"]["milestone"],
        )
    )
    return 0 if report["summary"]["ready_for_user_selection_decision"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
