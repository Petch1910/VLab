"""Build combo line explanations for selected first-slice skeletons.

M35-D3 of the Hybrid Vertical-Slice Strategy.

This turns D1/D2 package data into human-reviewable combo line explanations.
It is not a playbook export and does not publish anything to runtime or bot
systems.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
CANDIDATE_PACKAGES_REPORT = ROOT / "outputs" / "target_slice" / "m35_d1_first_slice_candidate_packages.json"
SKELETON_REPORT = ROOT / "outputs" / "target_slice" / "m35_d2_first_slice_deck_skeleton_ratio_plans.json"
COMPATIBILITY_REPORT = ROOT / "outputs" / "target_slice" / "m35_c5_first_slice_selected_compatibility_output.json"
OUTPUT_DIR = ROOT / "outputs" / "target_slice"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


NEEDS_BY_VERDICT = {
    "target_resource_need_not_supported_by_source": "Needs another card/package to cover target resource pressure.",
    "target_zone_need_not_supported_by_source": "Needs normal board setup or another package to cover target zone access.",
    "target_timing_not_constrained": "Target timing is broad; confirm actual line during review.",
    "not_resource_relevant": "No resource dependency detected for this edge.",
    "not_zone_relevant": "No zone dependency detected for this edge.",
}


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _compatibility_index(report: dict[str, Any]) -> dict[str, dict[str, Any]]:
    return {edge["edge"]: edge for edge in report["compatibility_edges"]}


def _package_index(report: dict[str, Any]) -> dict[str, dict[str, Any]]:
    return {package["package_id"]: package for package in report["packages"]}


def _top_edges(package: dict[str, Any], compatibility: dict[str, dict[str, Any]], limit: int = 4) -> list[dict[str, Any]]:
    edges = [compatibility[edge_key] for edge_key in package["edge_keys"] if edge_key in compatibility]
    return sorted(edges, key=lambda edge: (-edge["net_score"], edge["edge"]))[:limit]


def _need_notes(edge: dict[str, Any]) -> list[str]:
    notes: list[str] = []
    for verdict_key in ("resource_verdict", "timing_verdict", "zone_verdict"):
        verdict = edge.get(verdict_key)
        note = NEEDS_BY_VERDICT.get(verdict)
        if note and note not in notes:
            notes.append(note)
    if edge.get("manual_review_reasons"):
        notes.append("Manual review required before any playbook promotion.")
    if not notes:
        notes.append("No additional support gap detected by C2-C4 advisory detectors.")
    return notes


def _step(edge: dict[str, Any], index: int) -> dict[str, Any]:
    return {
        "step_no": index,
        "edge": edge["edge"],
        "source_card_id": edge["source_card_id"],
        "source_name_th": edge["source_name_th"],
        "target_card_id": edge["target_card_id"],
        "target_name_th": edge["target_name_th"],
        "resource_verdict": edge["resource_verdict"],
        "timing_verdict": edge["timing_verdict"],
        "zone_verdict": edge["zone_verdict"],
        "why_it_matters": [
            *edge.get("synergy_reasons", []),
        ],
        "needs_to_work": _need_notes(edge),
    }


def build_combo_line(
    skeleton: dict[str, Any],
    package: dict[str, Any],
    compatibility: dict[str, dict[str, Any]],
    index: int,
) -> dict[str, Any]:
    edges = _top_edges(package, compatibility)
    cards_involved = sorted({card_id for edge in edges for card_id in (edge["source_card_id"], edge["target_card_id"])})
    roles = skeleton["roles"]
    return {
        "line_id": f"line_{index:03d}",
        "source_skeleton_id": skeleton["skeleton_id"],
        "source_package_id": package["package_id"],
        "anchor_card_id": skeleton["anchor_card_id"],
        "anchor_name_th": skeleton["anchor_name_th"],
        "line_title": f"{skeleton['anchor_card_id']} support package line",
        "cards_involved": cards_involved,
        "key_cards": roles["key_cards"],
        "support_cards": roles["support_cards"][:10],
        "trigger_cards": roles["trigger_cards"],
        "resource_recovery_cards": roles["resource_recovery_cards"],
        "steps": [_step(edge, step_index) for step_index, edge in enumerate(edges, start=1)],
        "why_package_is_included": [
            "Built from clean M35-C5 synergy edges.",
            "Selected by M35-D1 anchor package scoring.",
            "Attached to M35-D2 grade/trigger ratio skeleton for review.",
        ],
        "needs_to_work": sorted({note for edge in edges for note in _need_notes(edge)}),
        "known_limits": [
            "Advisory explanation only.",
            "No per-card quantities.",
            "No final play sequence legality claim.",
            "No runtime bot/playbook export.",
        ],
        "candidate_for_m35_d4_review": True,
    }


def build_report(
    candidate_packages_report: dict[str, Any] | None = None,
    skeleton_report: dict[str, Any] | None = None,
    compatibility_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    candidate_packages_report = candidate_packages_report or load_json(CANDIDATE_PACKAGES_REPORT)
    skeleton_report = skeleton_report or load_json(SKELETON_REPORT)
    compatibility_report = compatibility_report or load_json(COMPATIBILITY_REPORT)
    packages = _package_index(candidate_packages_report)
    compatibility = _compatibility_index(compatibility_report)
    lines = []
    for index, skeleton in enumerate(skeleton_report["skeletons"], start=1):
        package = packages.get(skeleton["source_package_id"])
        if not package:
            continue
        lines.append(build_combo_line(skeleton, package, compatibility, index))
    return {
        "version": "M35-D3",
        "description": "Combo line explainer for selected first-slice skeletons",
        "selected_target": skeleton_report["selected_target"],
        "source_inputs": {
            "candidate_packages": str(CANDIDATE_PACKAGES_REPORT.relative_to(ROOT)),
            "deck_skeleton_ratio_plans": str(SKELETON_REPORT.relative_to(ROOT)),
            "selected_compatibility_output": str(COMPATIBILITY_REPORT.relative_to(ROOT)),
        },
        "scope": {
            "advisory_only": True,
            "combo_line_explainer": True,
            "reviewed_playbook_seed": False,
            "deck_quantities": False,
            "bot_playbook": False,
            "runtime_effect_execution": False,
        },
        "policy": {
            "uses_clean_m35_c5_edges_only": True,
            "does_not_claim_final_play_sequence_legality": True,
            "does_not_publish_to_runtime_or_bot": True,
            "d4_review_required_before_playbook_seed": True,
        },
        "summary": {
            "source_skeleton_count": len(skeleton_report["skeletons"]),
            "combo_line_count": len(lines),
            "ready_for_m35_d4": True,
        },
        "combo_lines": lines,
        "next_target": "M35-D4",
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    summary = report["summary"]
    lines = [
        "# M35-D3 Combo Line Explainer",
        "",
        "## Selected Target",
        "",
        f"- Slice: `{target['slice']}`",
        f"- Era preset: `{target['era_preset']}`",
        f"- Group: `{target['group']}`",
        "",
        "## Summary",
        "",
        f"- Source skeletons: `{summary['source_skeleton_count']}`",
        f"- Combo lines: `{summary['combo_line_count']}`",
        f"- Ready for M35-D4: `{summary['ready_for_m35_d4']}`",
        "",
        "## Top Lines",
        "",
    ]
    for line in report["combo_lines"][:10]:
        lines.append(
            f"- `{line['line_id']}` skeleton=`{line['source_skeleton_id']}` "
            f"anchor=`{line['anchor_card_id']}` steps=`{len(line['steps'])}`"
        )
        for step in line["steps"][:2]:
            lines.append(
                f"  - `{step['source_card_id']}` -> `{step['target_card_id']}` "
                f"resource=`{step['resource_verdict']}` timing=`{step['timing_verdict']}` zone=`{step['zone_verdict']}`"
            )
    lines.extend(
        [
            "",
            "## Scope",
            "",
            "- Combo line explanation only.",
            "- No per-card quantities or final play sequence legality claim.",
            "- No bot/playbook export.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35-D3 combo line explanations.")
    parser.add_argument("--candidate-packages", type=Path, default=CANDIDATE_PACKAGES_REPORT)
    parser.add_argument("--skeleton-report", type=Path, default=SKELETON_REPORT)
    parser.add_argument("--compatibility-report", type=Path, default=COMPATIBILITY_REPORT)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_report(
        load_json(args.candidate_packages),
        load_json(args.skeleton_report),
        load_json(args.compatibility_report),
    )
    json_path = args.output_dir / "m35_d3_first_slice_combo_line_explainer.json"
    md_path = args.output_dir / "m35_d3_first_slice_combo_line_explainer.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35-D3 combo line explainer wrote {json_path}")
    print(f"M35-D3 combo line summary wrote {md_path}")
    print(
        "source_skeletons={skeletons} combo_lines={lines} ready_for_m35_d4={ready}".format(
            skeletons=report["summary"]["source_skeleton_count"],
            lines=report["summary"]["combo_line_count"],
            ready=report["summary"]["ready_for_m35_d4"],
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
