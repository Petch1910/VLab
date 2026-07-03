"""Build a read-only M57-02 human selection request packet."""

from __future__ import annotations

import argparse
import csv
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M57_01_REVIEW = OUTPUT_DIR / "m57_01_sixth_slice_human_repair_review_packet.json"
JSON_OUTPUT = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_request_packet.json"
MD_OUTPUT = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_request_packet.md"
CSV_OUTPUT = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_request_packet.csv"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _bool(value: Any) -> bool:
    return bool(value)


def _selection_command(review_item_id: str) -> str:
    return (
        "python tools\\deck\\build_sixth_slice_human_selected_recipe_artifact.py "
        f"--review-item-id {review_item_id} "
        '--selection-text "<explicit user/team selection reason>"'
    )


def _candidate_item(item: dict[str, Any], order: int) -> dict[str, Any]:
    pair = item.get("pair", {})
    ready = (
        _bool(item.get("ready_for_human_repair_review"))
        and _bool(item.get("manual_repair_complete"))
        and _bool(item.get("grade_repair_complete"))
        and not item.get("structural_blockers")
    )
    return {
        "order": order,
        "review_item_id": item.get("review_item_id", ""),
        "recipe_id": item.get("recipe_id", ""),
        "ready_for_user_selection": ready,
        "source_candidate_edge": item.get("source_candidate_edge", ""),
        "source_card_id": pair.get("source_card_id", ""),
        "source_name_th": pair.get("source_name_th", ""),
        "target_card_id": pair.get("target_card_id", ""),
        "target_name_th": pair.get("target_name_th", ""),
        "manual_review_card_count": len(item.get("manual_review_card_ids", [])),
        "manual_substitution_count": len(item.get("manual_substitutions", [])),
        "manual_repair_complete": _bool(item.get("manual_repair_complete")),
        "grade_repair_complete": _bool(item.get("grade_repair_complete")),
        "grade_counts_after": item.get("grade_counts_after", {}),
        "g_zone_deferred": _bool(item.get("g_zone_deferred")),
        "g_zone_decision_option_count": len(item.get("g_zone_decision_options", [])),
        "decision_option_count": len(item.get("decision_options", [])),
        "structural_blockers": item.get("structural_blockers", []),
        "selection_command_template": _selection_command(item.get("review_item_id", "")),
    }


def build_sixth_slice_human_selection_request_packet(
    review_packet: dict[str, Any] | None = None,
) -> dict[str, Any]:
    review_packet = review_packet or load_json(M57_01_REVIEW)
    items = review_packet.get("review_items", [])
    candidates = [_candidate_item(item, index) for index, item in enumerate(items, start=1)]
    ready_candidates = [item for item in candidates if item["ready_for_user_selection"]]
    blockers: list[dict[str, Any]] = []
    if not review_packet.get("summary", {}).get("ready_for_m57_02"):
        blockers.append(
            {
                "code": "m57_01_not_ready_for_selection",
                "severity": "blocker",
                "message": "M57-01 review packet is not ready for M57-02 selection.",
                "details": {"summary": review_packet.get("summary", {})},
            }
        )
    if not ready_candidates:
        blockers.append(
            {
                "code": "no_ready_review_items",
                "severity": "blocker",
                "message": "No review items are ready for explicit user selection.",
                "details": {"review_item_count": len(candidates)},
            }
        )

    return {
        "version": "M57-02-prerequisite",
        "description": "Sixth-slice human selection request packet",
        "source_inputs": {
            "m57_01_human_repair_review_packet": str(M57_01_REVIEW.relative_to(ROOT)),
        },
        "scope": {
            "read_only_selection_request": True,
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
            "requires_user_or_team_to_choose_one_review_item_id": True,
            "requires_non_empty_selection_text": True,
            "tool_must_not_choose_automatically": True,
            "m57_02_command_template": (
                "python tools\\deck\\build_sixth_slice_human_selected_recipe_artifact.py "
                "--review-item-id <review_item_id> "
                '--selection-text "<explicit user/team selection reason>"'
            ),
        },
        "candidates": candidates,
        "issues": blockers,
        "decision": {
            "selection_request_ready": not blockers,
            "human_selection_recorded": False,
            "selected_review_item_id": "",
            "recommended_milestone": "M57-02" if not blockers else "M57-01-repair",
            "recommended_task": (
                "User/team selects exactly one sixth-slice review item with non-empty selection text"
                if not blockers
                else "Repair M57-01 review packet readiness before selection"
            ),
            "runtime_promotion_allowed": False,
        },
        "summary": {
            "review_item_count": len(candidates),
            "ready_candidate_count": len(ready_candidates),
            "blocking_issue_count": len(blockers),
            "issue_count": len(blockers),
            "selection_request_ready": not blockers,
            "human_selection_recorded": False,
            "ready_for_m57_02": not blockers,
        },
        "next_target": {
            "milestone": "M57-02" if not blockers else "M57-01-repair",
            "task": (
                "Sixth-slice human-selected recipe artifact"
                if not blockers
                else "Repair sixth-slice human repair review packet"
            ),
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_csv(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    fieldnames = [
        "order",
        "review_item_id",
        "recipe_id",
        "ready_for_user_selection",
        "source_candidate_edge",
        "source_card_id",
        "target_card_id",
        "manual_substitution_count",
        "grade_counts_after",
        "g_zone_deferred",
        "selection_command_template",
    ]
    with path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        for item in report["candidates"]:
            writer.writerow({field: item.get(field, "") for field in fieldnames})


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    policy = report["selection_policy"]
    lines = [
        "# M57-02 Sixth-Slice Human Selection Request Packet",
        "",
        "## Summary",
        "",
        f"- Review items: `{summary['review_item_count']}`",
        f"- Ready candidates: `{summary['ready_candidate_count']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        f"- Selection request ready: `{summary['selection_request_ready']}`",
        f"- Human selection recorded: `{summary['human_selection_recorded']}`",
        "",
        "## Required User/Team Action",
        "",
        "Choose exactly one `review_item_id` and provide non-empty selection text.",
        "",
        "```powershell",
        policy["m57_02_command_template"],
        "```",
        "",
        "## Ready Candidates",
        "",
    ]
    for item in report["candidates"]:
        if not item["ready_for_user_selection"]:
            continue
        lines.append(
            "- `{order}` `{review}` recipe=`{recipe}` edge=`{edge}` manual_subs=`{manual}` grades=`{grades}`".format(
                order=item["order"],
                review=item["review_item_id"],
                recipe=item["recipe_id"],
                edge=item["source_candidate_edge"],
                manual=item["manual_substitution_count"],
                grades=item["grade_counts_after"],
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
            "- This packet does not choose a review item.",
            "- This packet does not record human selection or acceptance.",
            "- This packet does not record a G Zone / Stride decision.",
            "- This packet does not create a runtime fixture.",
            "- This packet does not publish saved decks, UI deck lists, or bot playbooks.",
            "- This packet does not mutate GameState.",
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
    parser = argparse.ArgumentParser(description="Build M57-02 human selection request packet.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_sixth_slice_human_selection_request_packet()
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    csv_path = args.output_dir / CSV_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    write_csv(report, csv_path)
    print(f"M57-02 selection request packet wrote {json_path}")
    print(f"M57-02 selection request summary wrote {md_path}")
    print(f"M57-02 selection request CSV wrote {csv_path}")
    print(
        "ready_candidates={ready}/{total} blockers={blockers} next={next_target}".format(
            ready=report["summary"]["ready_candidate_count"],
            total=report["summary"]["review_item_count"],
            blockers=report["summary"]["blocking_issue_count"],
            next_target=report["next_target"]["milestone"],
        )
    )
    return 0 if report["summary"]["selection_request_ready"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
