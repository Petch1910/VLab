"""Build a read-only M57-02 human selection candidate digest.

The digest helps a user/team compare all ready sixth-slice review candidates.
It intentionally does not recommend or select a candidate.
"""

from __future__ import annotations

import argparse
import csv
import json
import sys
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
REQUEST_PACKET = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_request_packet.json"
PREFLIGHT_REPORT = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_preflight.json"
JSON_OUTPUT = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_candidate_digest.json"
MD_OUTPUT = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_candidate_digest.md"
CSV_OUTPUT = OUTPUT_DIR / "m57_02_sixth_slice_human_selection_candidate_digest.csv"

TARGET_GRADE_COUNTS = {"0": 17, "1": 14, "2": 11, "3": 8}


if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


from tools.deck.build_sixth_slice_human_selection_request_packet import load_json  # noqa: E402


def _candidate_structural_profile(candidate: dict[str, Any]) -> dict[str, Any]:
    return {
        "manual_review_card_count": int(candidate.get("manual_review_card_count", 0)),
        "manual_substitution_count": int(candidate.get("manual_substitution_count", 0)),
        "manual_repair_complete": bool(candidate.get("manual_repair_complete")),
        "grade_repair_complete": bool(candidate.get("grade_repair_complete")),
        "grade_counts_after": candidate.get("grade_counts_after", {}),
        "g_zone_deferred": bool(candidate.get("g_zone_deferred")),
        "g_zone_decision_option_count": int(candidate.get("g_zone_decision_option_count", 0)),
        "decision_option_count": int(candidate.get("decision_option_count", 0)),
        "structural_blocker_count": len(candidate.get("structural_blockers", [])),
    }


def _profile_key(profile: dict[str, Any]) -> str:
    return json.dumps(profile, ensure_ascii=False, sort_keys=True)


def _comparison_row(candidate: dict[str, Any]) -> dict[str, Any]:
    grade_counts_after = candidate.get("grade_counts_after", {})
    structural_profile = _candidate_structural_profile(candidate)
    return {
        "order": int(candidate.get("order", 0)),
        "review_item_id": candidate.get("review_item_id", ""),
        "recipe_id": candidate.get("recipe_id", ""),
        "ready_for_user_selection": bool(candidate.get("ready_for_user_selection")),
        "source_candidate_edge": candidate.get("source_candidate_edge", ""),
        "source_card_id": candidate.get("source_card_id", ""),
        "source_name_th": candidate.get("source_name_th", ""),
        "target_card_id": candidate.get("target_card_id", ""),
        "target_name_th": candidate.get("target_name_th", ""),
        "manual_review_card_count": structural_profile["manual_review_card_count"],
        "manual_substitution_count": structural_profile["manual_substitution_count"],
        "manual_repair_complete": structural_profile["manual_repair_complete"],
        "grade_repair_complete": structural_profile["grade_repair_complete"],
        "grade_counts_after": grade_counts_after,
        "grade_counts_match_target": grade_counts_after == TARGET_GRADE_COUNTS,
        "g_zone_deferred": structural_profile["g_zone_deferred"],
        "g_zone_decision_option_count": structural_profile["g_zone_decision_option_count"],
        "decision_option_count": structural_profile["decision_option_count"],
        "structural_blocker_count": structural_profile["structural_blocker_count"],
        "selection_command_template": candidate.get("selection_command_template", ""),
    }


def _group_counts(rows: list[dict[str, Any]], key_field: str, label_field: str) -> list[dict[str, Any]]:
    groups: dict[str, dict[str, Any]] = {}
    for row in rows:
        key = row[key_field]
        if key not in groups:
            groups[key] = {
                key_field: key,
                label_field: row[label_field],
                "candidate_count": 0,
                "review_item_ids": [],
                "recipe_ids": [],
            }
        groups[key]["candidate_count"] += 1
        groups[key]["review_item_ids"].append(row["review_item_id"])
        groups[key]["recipe_ids"].append(row["recipe_id"])
    return sorted(groups.values(), key=lambda item: (-item["candidate_count"], item[key_field]))


def build_sixth_slice_human_selection_candidate_digest(
    request_packet: dict[str, Any] | None = None,
    preflight_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    request_packet = request_packet or load_json(REQUEST_PACKET)
    preflight_report = preflight_report or load_json(PREFLIGHT_REPORT)
    candidates = request_packet.get("candidates", [])
    rows = [_comparison_row(candidate) for candidate in candidates]
    ready_rows = [row for row in rows if row["ready_for_user_selection"]]
    profile_counter = Counter(_profile_key(_candidate_structural_profile(candidate)) for candidate in candidates)
    issues: list[dict[str, Any]] = []

    if not request_packet.get("summary", {}).get("selection_request_ready"):
        issues.append(
            {
                "code": "selection_request_not_ready",
                "severity": "blocker",
                "message": "Selection request packet is not ready.",
                "details": {"summary": request_packet.get("summary", {})},
            }
        )
    if not ready_rows:
        issues.append(
            {
                "code": "no_ready_candidates",
                "severity": "blocker",
                "message": "No candidates are ready for human selection.",
                "details": {"candidate_count": len(rows)},
            }
        )

    return {
        "version": "M57-02-candidate-digest",
        "description": "Sixth-slice human selection candidate digest",
        "source_inputs": {
            "m57_02_selection_request_packet": str(REQUEST_PACKET.relative_to(ROOT)),
            "m57_02_selection_preflight": str(PREFLIGHT_REPORT.relative_to(ROOT)),
        },
        "scope": {
            "read_only_candidate_digest": True,
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
            "requires_user_or_team_to_choose_one_review_item_id": True,
            "requires_non_empty_selection_text": True,
            "preflight_before_real_artifact_recommended": True,
        },
        "comparison_rows": rows,
        "groups": {
            "by_source_card": _group_counts(ready_rows, "source_card_id", "source_name_th"),
            "by_target_card": _group_counts(ready_rows, "target_card_id", "target_name_th"),
        },
        "structural_profiles": {
            "unique_profile_count": len(profile_counter),
            "all_ready_candidates_share_same_profile": len(profile_counter) == 1 and len(ready_rows) == len(rows),
            "profile_counts": [
                {"profile": json.loads(profile), "candidate_count": count}
                for profile, count in sorted(profile_counter.items(), key=lambda item: item[0])
            ],
        },
        "guidance": {
            "auto_selection": "disabled",
            "tie_breaker": (
                "All ready candidates have the same structural readiness profile; choose by desired source/target combo preference."
                if len(profile_counter) == 1 and ready_rows
                else "Compare structural profile differences, then choose by desired source/target combo preference."
            ),
            "next_safe_command": (
                "python tools\\deck\\build_sixth_slice_human_selection_preflight.py "
                "--review-item-id <review_item_id> "
                '--selection-text "<explicit user/team selection reason>"'
            ),
        },
        "issues": issues,
        "summary": {
            "candidate_count": len(rows),
            "ready_candidate_count": len(ready_rows),
            "source_group_count": len(_group_counts(ready_rows, "source_card_id", "source_name_th")),
            "target_group_count": len(_group_counts(ready_rows, "target_card_id", "target_name_th")),
            "unique_structural_profile_count": len(profile_counter),
            "all_ready_candidates_share_same_profile": len(profile_counter) == 1 and len(ready_rows) == len(rows),
            "request_ready": bool(request_packet.get("summary", {}).get("selection_request_ready")),
            "preflight_report_ready": bool(preflight_report.get("decision", {}).get("preflight_report_ready")),
            "human_selection_recorded": False,
            "blocking_issue_count": len(issues),
            "ready_for_user_selection_decision": not issues,
        },
        "next_target": {
            "milestone": "M57-02-user-selection" if not issues else "M57-02-prerequisite-repair",
            "task": (
                "Choose one ready review_item_id, provide selection_text, then run preflight"
                if not issues
                else "Repair selection request packet before user selection"
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
        "source_card_id",
        "target_card_id",
        "manual_substitution_count",
        "grade_counts_after",
        "g_zone_deferred",
        "g_zone_decision_option_count",
        "selection_command_template",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        for row in report["comparison_rows"]:
            writer.writerow({field: row.get(field, "") for field in fieldnames})


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    guidance = report["guidance"]
    lines = [
        "# M57-02 Sixth-Slice Human Selection Candidate Digest",
        "",
        "## Summary",
        "",
        f"- Candidates: `{summary['candidate_count']}`",
        f"- Ready candidates: `{summary['ready_candidate_count']}`",
        f"- Source groups: `{summary['source_group_count']}`",
        f"- Target groups: `{summary['target_group_count']}`",
        f"- Unique structural profiles: `{summary['unique_structural_profile_count']}`",
        f"- Same structural profile: `{summary['all_ready_candidates_share_same_profile']}`",
        f"- Human selection recorded: `{summary['human_selection_recorded']}`",
        f"- Blocking issues: `{summary['blocking_issue_count']}`",
        "",
        "## Guidance",
        "",
        f"- Auto-selection: `{guidance['auto_selection']}`",
        f"- Tie breaker: {guidance['tie_breaker']}",
        "",
        "```powershell",
        guidance["next_safe_command"],
        "```",
        "",
        "## Comparison Rows",
        "",
    ]
    for row in report["comparison_rows"]:
        lines.append(
            "- `{order}` `{review}` recipe=`{recipe}` edge=`{edge}` source=`{source}` target=`{target}` manual_subs=`{manual}` grades=`{grades}` g_zone_deferred=`{gzone}`".format(
                order=row["order"],
                review=row["review_item_id"],
                recipe=row["recipe_id"],
                edge=row["source_candidate_edge"],
                source=row["source_card_id"],
                target=row["target_card_id"],
                manual=row["manual_substitution_count"],
                grades=row["grade_counts_after"],
                gzone=row["g_zone_deferred"],
            )
        )
    lines.extend(["", "## Source Groups", ""])
    for group in report["groups"]["by_source_card"]:
        lines.append(
            f"- `{group['source_card_id']}` count=`{group['candidate_count']}` recipes=`{group['recipe_ids']}`"
        )
    lines.extend(["", "## Target Groups", ""])
    for group in report["groups"]["by_target_card"]:
        lines.append(
            f"- `{group['target_card_id']}` count=`{group['candidate_count']}` recipes=`{group['recipe_ids']}`"
        )
    lines.extend(["", "## Boundary", ""])
    lines.extend(
        [
            "- This digest does not choose a review item.",
            "- This digest does not record human selection or acceptance.",
            "- This digest does not record a G Zone / Stride decision.",
            "- This digest does not create the real M57-02 selected artifact.",
            "- This digest does not create a runtime fixture, publish decks, enable bot playbooks, or mutate GameState.",
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
    parser = argparse.ArgumentParser(description="Build M57-02 human selection candidate digest.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_sixth_slice_human_selection_candidate_digest()
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    csv_path = args.output_dir / CSV_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    write_csv(report, csv_path)
    print(f"M57-02 selection candidate digest wrote {json_path}")
    print(f"M57-02 selection candidate digest summary wrote {md_path}")
    print(f"M57-02 selection candidate digest CSV wrote {csv_path}")
    print(
        "ready_candidates={ready}/{total} source_groups={sources} target_groups={targets} next={next_target}".format(
            ready=report["summary"]["ready_candidate_count"],
            total=report["summary"]["candidate_count"],
            sources=report["summary"]["source_group_count"],
            targets=report["summary"]["target_group_count"],
            next_target=report["next_target"]["milestone"],
        )
    )
    return 0 if report["summary"]["ready_for_user_selection_decision"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
