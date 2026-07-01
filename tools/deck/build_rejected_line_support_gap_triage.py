"""Build the M37-03 rejected-line support-gap triage report.

The report classifies the 24 M36 rejected combo lines by review/support-gap
signals. It does not resolve card semantics, mutate recipe drafts, or promote
runtime playbooks.
"""

from __future__ import annotations

import argparse
import json
import sys
from collections import Counter, defaultdict
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M36_REVIEW_PACKET = OUTPUT_DIR / "m36_01_first_slice_review_packet.json"
M35_COMBO_LINES = OUTPUT_DIR / "m35_d3_first_slice_combo_line_explainer.json"
M37_02_REPAIR = OUTPUT_DIR / "m37_02_trigger_package_repair_proposal.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


GAP_RULES: tuple[tuple[str, str, str], ...] = (
    (
        "resource_pressure_gap",
        "Needs another card/package to cover target resource pressure.",
        "Map resource requirements/providers or keep line rejected.",
    ),
    (
        "zone_access_gap",
        "Needs normal board setup or another package to cover target zone access.",
        "Map zone/target requirements/providers or require normal setup note.",
    ),
    (
        "broad_timing_review",
        "Target timing is broad; confirm actual line during review.",
        "Tighten timing-window mapping or keep manual review.",
    ),
    (
        "detector_gap_not_found_manual_review",
        "No additional support gap detected by C2-C4 advisory detectors.",
        "Confirm whether line can be accepted after human review.",
    ),
    (
        "no_resource_dependency_manual_review",
        "No resource dependency detected for this edge.",
        "Confirm this is not a false dependency before accepting the line.",
    ),
)

GROUP_ACTIONS = {code: action for code, _, action in GAP_RULES}


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _combo_line_map(combo_report: dict[str, Any]) -> dict[str, dict[str, Any]]:
    return {line["line_id"]: line for line in combo_report.get("combo_lines", [])}


def _gap_labels(needs_to_work: Sequence[str]) -> list[str]:
    labels: list[str] = []
    for code, trigger_text, _ in GAP_RULES:
        if trigger_text in needs_to_work:
            labels.append(code)
    return labels or ["unclassified_support_gap_review"]


def _triage_priority(labels: Sequence[str]) -> str:
    label_set = set(labels)
    if {"resource_pressure_gap", "zone_access_gap"}.issubset(label_set):
        return "P1_resource_and_zone_gap"
    if "resource_pressure_gap" in label_set or "zone_access_gap" in label_set:
        return "P2_single_structural_gap"
    return "P3_manual_confirmation"


def _triage_item(item: dict[str, Any], combo_line: dict[str, Any] | None) -> dict[str, Any]:
    needs = item.get("needs_to_work", [])
    labels = _gap_labels(needs)
    return {
        "line_id": item["item_id"],
        "source_skeleton_id": item.get("source_skeleton_id", ""),
        "source_package_id": item.get("source_package_id", ""),
        "anchor_card_id": item.get("anchor_card_id", ""),
        "anchor_name_th": (combo_line or {}).get("anchor_name_th", ""),
        "line_title": (combo_line or {}).get("line_title", ""),
        "review_priority": item.get("review_priority", ""),
        "triage_priority": _triage_priority(labels),
        "gap_labels": labels,
        "needs_to_work": needs,
        "review_reasons": item.get("review_reasons", []),
        "blocked_until": item.get("blocked_until", []),
        "recommended_next_action": item.get("recommended_next_action", ""),
        "cards_involved": (combo_line or {}).get("cards_involved", []),
        "support_cards": (combo_line or {}).get("support_cards", []),
        "trigger_cards": (combo_line or {}).get("trigger_cards", []),
        "advisory_only": True,
    }


def _group_summaries(triage_items: Sequence[dict[str, Any]]) -> list[dict[str, Any]]:
    line_ids_by_group: dict[str, list[str]] = defaultdict(list)
    for item in triage_items:
        for label in item["gap_labels"]:
            line_ids_by_group[label].append(item["line_id"])
    return [
        {
            "gap_label": label,
            "line_count": len(line_ids),
            "line_ids": sorted(line_ids),
            "recommended_m37_04_action": GROUP_ACTIONS.get(
                label, "Review unclassified support gap manually."
            ),
        }
        for label, line_ids in sorted(line_ids_by_group.items())
    ]


def _manual_mapping_backlog(groups: Sequence[dict[str, Any]]) -> list[dict[str, Any]]:
    mapping = {
        "resource_pressure_gap": "resource_requirement_provider_mapping",
        "zone_access_gap": "zone_target_requirement_provider_mapping",
        "broad_timing_review": "timing_window_specificity_mapping",
        "detector_gap_not_found_manual_review": "human_acceptance_without_new_mapping",
        "no_resource_dependency_manual_review": "false_dependency_or_acceptance_review",
    }
    return [
        {
            "backlog_id": f"m37_04_{index:02d}_{group['gap_label']}",
            "gap_label": group["gap_label"],
            "mapping_type": mapping.get(group["gap_label"], "manual_review"),
            "line_count": group["line_count"],
            "source_line_ids": group["line_ids"],
            "recommended_action": group["recommended_m37_04_action"],
            "runtime_promotion_allowed": False,
        }
        for index, group in enumerate(groups, start=1)
    ]


def build_support_gap_triage(
    review_packet: dict[str, Any] | None = None,
    combo_report: dict[str, Any] | None = None,
    repair_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    review_packet = review_packet or load_json(M36_REVIEW_PACKET)
    combo_report = combo_report or load_json(M35_COMBO_LINES)
    repair_report = repair_report or load_json(M37_02_REPAIR)
    combo_by_line = _combo_line_map(combo_report)
    rejected = review_packet.get("rejected_line_review_items", [])
    triage_items = [
        _triage_item(item, combo_by_line.get(item["item_id"]))
        for item in rejected
    ]
    groups = _group_summaries(triage_items)
    label_counts = Counter(label for item in triage_items for label in item["gap_labels"])
    priority_counts = Counter(item["triage_priority"] for item in triage_items)

    return {
        "version": "M37-03",
        "description": "Rejected-line support-gap triage for first-slice combo lines",
        "selected_target": review_packet.get("selected_target", {}),
        "source_inputs": {
            "first_slice_review_packet": str(M36_REVIEW_PACKET.relative_to(ROOT)),
            "combo_line_explainer": str(M35_COMBO_LINES.relative_to(ROOT)),
            "trigger_package_repair_proposal": str(M37_02_REPAIR.relative_to(ROOT)),
        },
        "scope": {
            "offline_triage": True,
            "changes_recipe_draft": False,
            "creates_runtime_deck": False,
            "bot_integration": False,
            "automatic_playbook_promotion": False,
            "live_card_text_parsing": False,
        },
        "accepted_seed_repair_context": {
            "recommended_package_id": repair_report.get("summary", {}).get("recommended_package_id", ""),
            "recommended_profile_id": repair_report.get("summary", {}).get("recommended_profile_id", ""),
            "runtime_promotion_allowed": False,
        },
        "triage_policy": {
            "multi_label": True,
            "human_review_required": True,
            "runtime_promotion_allowed": False,
            "unclassified_lines_blocked": True,
        },
        "summary": {
            "rejected_line_count": len(rejected),
            "triage_item_count": len(triage_items),
            "gap_group_count": len(groups),
            "multi_label_line_count": sum(1 for item in triage_items if len(item["gap_labels"]) > 1),
            "gap_label_counts": dict(sorted(label_counts.items())),
            "triage_priority_counts": dict(sorted(priority_counts.items())),
            "manual_mapping_backlog_count": len(groups),
            "runtime_promotion_allowed": False,
            "ready_for_m37_04": bool(triage_items) and len(triage_items) == len(rejected),
        },
        "gap_groups": groups,
        "manual_mapping_backlog": _manual_mapping_backlog(groups),
        "triage_items": triage_items,
        "next_target": {
            "milestone": "M37-04",
            "task": "Manual semantic mapping candidates",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M37-03 Rejected-Line Support-Gap Triage",
        "",
        "## Summary",
        "",
        f"- Rejected lines: `{summary['rejected_line_count']}`",
        f"- Triage items: `{summary['triage_item_count']}`",
        f"- Gap groups: `{summary['gap_group_count']}`",
        f"- Multi-label lines: `{summary['multi_label_line_count']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M37-04: `{summary['ready_for_m37_04']}`",
        "",
        "## Gap Groups",
        "",
    ]
    for group in report["gap_groups"]:
        lines.append(
            f"- `{group['gap_label']}`: `{group['line_count']}` lines "
            f"-> {group['recommended_m37_04_action']}"
        )
    lines.extend(["", "## Priority Counts", ""])
    for priority, count in summary["triage_priority_counts"].items():
        lines.append(f"- `{priority}`: `{count}`")
    lines.extend(["", "## Manual Mapping Backlog", ""])
    for item in report["manual_mapping_backlog"]:
        lines.append(
            f"- `{item['backlog_id']}` `{item['mapping_type']}` "
            f"lines=`{item['line_count']}`"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Triage is multi-label and advisory.",
            "- No rejected combo line is promoted by this report.",
            "- Manual semantic mapping candidates are handled in M37-04.",
            "",
            "## Next",
            "",
            "`M37-04`: Manual semantic mapping candidates.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M37-03 rejected-line support-gap triage.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_support_gap_triage()
    json_path = args.output_dir / "m37_03_rejected_line_support_gap_triage.json"
    md_path = args.output_dir / "m37_03_rejected_line_support_gap_triage.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M37-03 rejected-line support-gap triage wrote {json_path}")
    print(f"M37-03 rejected-line support-gap triage summary wrote {md_path}")
    print(
        "ready_for_m37_04={ready} rejected_lines={lines} groups={groups}".format(
            ready=report["summary"]["ready_for_m37_04"],
            lines=report["summary"]["rejected_line_count"],
            groups=report["summary"]["gap_group_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m37_04"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
