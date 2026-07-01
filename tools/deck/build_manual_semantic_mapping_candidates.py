"""Build M37-04 manual semantic mapping candidates.

The candidates translate M37-03 support-gap groups into reviewable semantic
mapping work items. They are not executable ability schema changes and do not
accept rejected combo lines.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M37_03_TRIAGE = OUTPUT_DIR / "m37_03_rejected_line_support_gap_triage.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


MAPPING_TEMPLATES: dict[str, dict[str, Any]] = {
    "resource_requirement_provider_mapping": {
        "candidate_kind": "structural_semantic_mapping",
        "target_schema_area": "resource_requirements_and_providers",
        "abstract_requirements": ["resource_pressure"],
        "candidate_provider_checks": [
            "resource_recovery_cards",
            "counter_charge_or_soul_support_review",
            "costless_line_review",
        ],
        "acceptance_criteria": [
            "Each source line must identify which resource pressure exists.",
            "A provider/support card must be source-backed or the line stays rejected.",
            "No runtime cost resolver changes are allowed from this candidate alone.",
        ],
    },
    "zone_target_requirement_provider_mapping": {
        "candidate_kind": "structural_semantic_mapping",
        "target_schema_area": "zone_target_requirements_and_providers",
        "abstract_requirements": ["rear_guard_zone_access", "target_zone_access"],
        "candidate_provider_checks": [
            "normal_board_setup_note",
            "call_or_move_to_rear_guard_mapping",
            "target_zone_supported_by_source",
        ],
        "acceptance_criteria": [
            "Each line must state whether normal setup is enough or another card is required.",
            "Zone provider evidence must be attached before line acceptance.",
            "No board mutation shortcut is allowed.",
        ],
    },
    "timing_window_specificity_mapping": {
        "candidate_kind": "timing_semantic_mapping",
        "target_schema_area": "timing_window_tags",
        "abstract_requirements": ["specific_timing_window"],
        "candidate_provider_checks": [
            "main_phase_window",
            "battle_phase_window",
            "on_attack_window",
            "on_hit_window",
        ],
        "acceptance_criteria": [
            "Broad timing must be narrowed to a known timing enum or kept manual.",
            "Ordering claims require a source line and cannot be inferred from score alone.",
        ],
    },
    "human_acceptance_without_new_mapping": {
        "candidate_kind": "review_only_gate",
        "target_schema_area": "human_review_status",
        "abstract_requirements": ["human_acceptance"],
        "candidate_provider_checks": [
            "reviewer_accept_or_keep_rejected",
            "no_new_structural_mapping_needed",
        ],
        "acceptance_criteria": [
            "Reviewer must explicitly accept the line or keep it rejected.",
            "No schema expansion is implied by this candidate.",
        ],
    },
    "false_dependency_or_acceptance_review": {
        "candidate_kind": "review_only_gate",
        "target_schema_area": "dependency_review_status",
        "abstract_requirements": ["false_dependency_check"],
        "candidate_provider_checks": [
            "confirm_no_resource_dependency",
            "remove_false_dependency_or_keep_manual",
        ],
        "acceptance_criteria": [
            "If no resource dependency exists, remove that blocker in a reviewed artifact.",
            "If evidence is unclear, keep the line rejected.",
        ],
    },
}


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _line_items_by_id(triage_report: dict[str, Any]) -> dict[str, dict[str, Any]]:
    return {item["line_id"]: item for item in triage_report.get("triage_items", [])}


def _candidate_from_backlog(
    index: int,
    backlog_item: dict[str, Any],
    line_items: dict[str, dict[str, Any]],
) -> dict[str, Any]:
    template = MAPPING_TEMPLATES.get(backlog_item["mapping_type"], {})
    source_line_ids = backlog_item.get("source_line_ids", [])
    linked_lines = [
        {
            "line_id": line_id,
            "anchor_card_id": line_items.get(line_id, {}).get("anchor_card_id", ""),
            "anchor_name_th": line_items.get(line_id, {}).get("anchor_name_th", ""),
            "triage_priority": line_items.get(line_id, {}).get("triage_priority", ""),
            "gap_labels": line_items.get(line_id, {}).get("gap_labels", []),
        }
        for line_id in source_line_ids
    ]
    return {
        "candidate_id": f"m37_04_map_{index:02d}_{backlog_item['gap_label']}",
        "gap_label": backlog_item["gap_label"],
        "mapping_type": backlog_item["mapping_type"],
        "candidate_kind": template.get("candidate_kind", "manual_review"),
        "target_schema_area": template.get("target_schema_area", "manual_review"),
        "line_count": backlog_item["line_count"],
        "source_line_ids": source_line_ids,
        "linked_lines": linked_lines,
        "abstract_requirements": template.get("abstract_requirements", []),
        "candidate_provider_checks": template.get("candidate_provider_checks", []),
        "acceptance_criteria": template.get("acceptance_criteria", []),
        "recommended_action": backlog_item.get("recommended_action", ""),
        "review_required": True,
        "runtime_promotion_allowed": False,
        "executable_schema_change": False,
    }


def build_mapping_candidates(triage_report: dict[str, Any] | None = None) -> dict[str, Any]:
    triage_report = triage_report or load_json(M37_03_TRIAGE)
    line_items = _line_items_by_id(triage_report)
    candidates = [
        _candidate_from_backlog(index, item, line_items)
        for index, item in enumerate(triage_report.get("manual_mapping_backlog", []), start=1)
    ]
    structural_count = sum(
        1 for item in candidates if item["candidate_kind"] == "structural_semantic_mapping"
    )
    timing_count = sum(1 for item in candidates if item["candidate_kind"] == "timing_semantic_mapping")
    review_only_count = sum(1 for item in candidates if item["candidate_kind"] == "review_only_gate")
    line_link_count = sum(item["line_count"] for item in candidates)

    return {
        "version": "M37-04",
        "description": "Manual semantic mapping candidates from rejected-line triage",
        "selected_target": triage_report.get("selected_target", {}),
        "source_inputs": {
            "rejected_line_support_gap_triage": str(M37_03_TRIAGE.relative_to(ROOT)),
        },
        "scope": {
            "offline_mapping_candidates": True,
            "changes_ability_schema": False,
            "changes_recipe_draft": False,
            "creates_runtime_deck": False,
            "bot_integration": False,
            "automatic_playbook_promotion": False,
            "live_card_text_parsing": False,
        },
        "mapping_policy": {
            "review_required": True,
            "executable_schema_change": False,
            "runtime_promotion_allowed": False,
            "source_is_triage_artifact": True,
            "card_text_not_parsed": True,
        },
        "summary": {
            "mapping_candidate_count": len(candidates),
            "structural_mapping_candidate_count": structural_count,
            "timing_mapping_candidate_count": timing_count,
            "review_only_candidate_count": review_only_count,
            "line_mapping_link_count": line_link_count,
            "runtime_promotion_allowed": False,
            "ready_for_m37_05": bool(candidates),
        },
        "mapping_candidates": candidates,
        "next_target": {
            "milestone": "M37-05",
            "task": "Revised recipe validation rerun",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    lines = [
        "# M37-04 Manual Semantic Mapping Candidates",
        "",
        "## Summary",
        "",
        f"- Mapping candidates: `{summary['mapping_candidate_count']}`",
        f"- Structural candidates: `{summary['structural_mapping_candidate_count']}`",
        f"- Timing candidates: `{summary['timing_mapping_candidate_count']}`",
        f"- Review-only candidates: `{summary['review_only_candidate_count']}`",
        f"- Line mapping links: `{summary['line_mapping_link_count']}`",
        f"- Runtime promotion allowed: `{summary['runtime_promotion_allowed']}`",
        f"- Ready for M37-05: `{summary['ready_for_m37_05']}`",
        "",
        "## Candidates",
        "",
    ]
    for candidate in report["mapping_candidates"]:
        lines.append(
            f"- `{candidate['candidate_id']}` `{candidate['mapping_type']}` "
            f"kind=`{candidate['candidate_kind']}` lines=`{candidate['line_count']}` "
            f"target=`{candidate['target_schema_area']}`"
        )
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Candidates are review work items, not executable schema changes.",
            "- Rejected combo lines remain rejected.",
            "- Runtime deck and bot/playbook promotion remain disabled.",
            "",
            "## Next",
            "",
            "`M37-05`: Revised recipe validation rerun.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M37-04 manual semantic mapping candidates.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_mapping_candidates()
    json_path = args.output_dir / "m37_04_manual_semantic_mapping_candidates.json"
    md_path = args.output_dir / "m37_04_manual_semantic_mapping_candidates.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M37-04 manual semantic mapping candidates wrote {json_path}")
    print(f"M37-04 manual semantic mapping candidates summary wrote {md_path}")
    print(
        "ready_for_m37_05={ready} candidates={candidates} links={links}".format(
            ready=report["summary"]["ready_for_m37_05"],
            candidates=report["summary"]["mapping_candidate_count"],
            links=report["summary"]["line_mapping_link_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m37_05"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
