"""Build reviewed advisory playbook seed for the selected first slice.

M35-D4 of the Hybrid Vertical-Slice Strategy.

This is a static AI review of M35-D3 combo lines. It exports advisory seed
entries only for lines without unresolved support gaps. The output is not a
runtime bot playbook and is not auto-injected into any game system.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
COMBO_LINE_REPORT = ROOT / "outputs" / "target_slice" / "m35_d3_first_slice_combo_line_explainer.json"
OUTPUT_DIR = ROOT / "outputs" / "target_slice"

NO_GAP_NOTE = "No additional support gap detected by C2-C4 advisory detectors."

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def line_passes_static_review(line: dict[str, Any]) -> tuple[bool, list[str]]:
    reasons: list[str] = []
    if not line.get("steps"):
        reasons.append("missing_steps")
    if line.get("needs_to_work") != [NO_GAP_NOTE]:
        reasons.append("unresolved_support_gap")
    for step in line.get("steps", []):
        if any("Manual review" in note for note in step.get("needs_to_work", [])):
            reasons.append("manual_review_step")
        if step.get("resource_verdict") in {"missing_slice_recovery"}:
            reasons.append("resource_missing_data")
        if step.get("timing_verdict") in {"source_timing_unknown_or_static", "provider_after_consumer_window"}:
            reasons.append("timing_not_review_clean")
        if step.get("zone_verdict") in {"missing_zone_support_in_slice", "vanguard_role_conflict"}:
            reasons.append("zone_not_review_clean")
    return not reasons, sorted(set(reasons))


def build_seed_entry(line: dict[str, Any], index: int) -> dict[str, Any]:
    return {
        "seed_id": f"seed_{index:03d}",
        "source_line_id": line["line_id"],
        "source_skeleton_id": line["source_skeleton_id"],
        "source_package_id": line["source_package_id"],
        "anchor_card_id": line["anchor_card_id"],
        "anchor_name_th": line["anchor_name_th"],
        "seed_status": "ai_static_reviewed_advisory",
        "cards_involved": line["cards_involved"],
        "key_cards": line["key_cards"],
        "support_cards": line["support_cards"],
        "trigger_cards": line["trigger_cards"],
        "resource_recovery_cards": line["resource_recovery_cards"],
        "steps": [
            {
                "step_no": step["step_no"],
                "edge": step["edge"],
                "source_card_id": step["source_card_id"],
                "target_card_id": step["target_card_id"],
                "why_it_matters": step["why_it_matters"],
                "review_note": "static_advisory_step_only_not_runtime_action",
            }
            for step in line["steps"]
        ],
        "review": {
            "reviewer": "ai_static_review",
            "policy": "m35_d4_clean_no_gap_lines_only",
            "passed": True,
            "requires_human_acceptance_before_runtime": True,
        },
        "runtime_policy": {
            "not_runtime_playbook": True,
            "not_published_to_bot": True,
            "not_auto_injected_into_decks": True,
            "legal_action_mask_required_before_future_bot_use": True,
            "masked_state_required_before_future_bot_use": True,
        },
    }


def build_rejection(line: dict[str, Any], reasons: list[str]) -> dict[str, Any]:
    return {
        "source_line_id": line["line_id"],
        "source_skeleton_id": line["source_skeleton_id"],
        "source_package_id": line["source_package_id"],
        "anchor_card_id": line["anchor_card_id"],
        "review_status": "not_exported_to_seed",
        "review_reasons": reasons,
        "needs_to_work": line.get("needs_to_work", []),
    }


def build_report(combo_line_report: dict[str, Any] | None = None) -> dict[str, Any]:
    combo_line_report = combo_line_report or load_json(COMBO_LINE_REPORT)
    seeds: list[dict[str, Any]] = []
    rejections: list[dict[str, Any]] = []
    for line in combo_line_report["combo_lines"]:
        passed, reasons = line_passes_static_review(line)
        if passed:
            seeds.append(build_seed_entry(line, len(seeds) + 1))
        else:
            rejections.append(build_rejection(line, reasons))
    return {
        "version": "M35-D4",
        "description": "Reviewed advisory playbook seed export for selected first slice",
        "selected_target": combo_line_report["selected_target"],
        "source_inputs": {
            "combo_line_explainer": str(COMBO_LINE_REPORT.relative_to(ROOT)),
        },
        "scope": {
            "advisory_only": True,
            "reviewed_playbook_seed_export": True,
            "runtime_playbook": False,
            "bot_integration": False,
            "deck_quantities": False,
            "runtime_effect_execution": False,
        },
        "policy": {
            "ai_static_review_only": True,
            "human_acceptance_required_before_runtime": True,
            "no_auto_inject_into_player_decks": True,
            "no_auto_publish_to_bot": True,
            "future_bot_use_requires_legal_action_mask_and_masked_state": True,
        },
        "summary": {
            "source_combo_line_count": len(combo_line_report["combo_lines"]),
            "seed_entry_count": len(seeds),
            "rejected_line_count": len(rejections),
            "ready_for_m35_e1": True,
        },
        "playbook_seed_entries": seeds,
        "review_rejections": rejections,
        "next_target": "M35-E1",
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    summary = report["summary"]
    lines = [
        "# M35-D4 Reviewed Advisory Playbook Seed",
        "",
        "## Selected Target",
        "",
        f"- Slice: `{target['slice']}`",
        f"- Era preset: `{target['era_preset']}`",
        f"- Group: `{target['group']}`",
        "",
        "## Summary",
        "",
        f"- Source combo lines: `{summary['source_combo_line_count']}`",
        f"- Seed entries: `{summary['seed_entry_count']}`",
        f"- Rejected lines: `{summary['rejected_line_count']}`",
        f"- Ready for M35-E1: `{summary['ready_for_m35_e1']}`",
        "",
        "## Seed Entries",
        "",
    ]
    for seed in report["playbook_seed_entries"]:
        lines.append(
            f"- `{seed['seed_id']}` line=`{seed['source_line_id']}` "
            f"anchor=`{seed['anchor_card_id']}` steps=`{len(seed['steps'])}`"
        )
    lines.extend(["", "## Rejection Summary", ""])
    for rejection in report["review_rejections"][:20]:
        lines.append(
            f"- `{rejection['source_line_id']}` anchor=`{rejection['anchor_card_id']}` "
            f"reasons=`{', '.join(rejection['review_reasons'])}`"
        )
    lines.extend(
        [
            "",
            "## Runtime Policy",
            "",
            "- Advisory seed export only.",
            "- Human acceptance is required before any runtime use.",
            "- Not published to bot runtime.",
            "- Not auto-injected into player decks.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35-D4 reviewed advisory playbook seed.")
    parser.add_argument("--combo-lines", type=Path, default=COMBO_LINE_REPORT)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_report(load_json(args.combo_lines))
    json_path = args.output_dir / "m35_d4_first_slice_reviewed_playbook_seed.json"
    md_path = args.output_dir / "m35_d4_first_slice_reviewed_playbook_seed.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35-D4 reviewed playbook seed wrote {json_path}")
    print(f"M35-D4 reviewed playbook seed summary wrote {md_path}")
    print(
        "source_lines={source} seeds={seeds} rejected={rejected} ready_for_m35_e1={ready}".format(
            source=report["summary"]["source_combo_line_count"],
            seeds=report["summary"]["seed_entry_count"],
            rejected=report["summary"]["rejected_line_count"],
            ready=report["summary"]["ready_for_m35_e1"],
        )
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
