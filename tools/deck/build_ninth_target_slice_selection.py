"""Build the M67-01 ninth target slice selection scaffold artifact."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M66_04_DECISION = OUTPUT_DIR / "m66_04_eight_fixture_scale_decision.json"
JSON_OUTPUT = OUTPUT_DIR / "m67_01_ninth_target_slice_selection.json"
MD_OUTPUT = OUTPUT_DIR / "m67_01_ninth_target_slice_selection.md"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _candidate_to_selected_target(candidate: dict[str, Any]) -> dict[str, Any]:
    return {
        "slice": "Ninth Offline Slice",
        "era_preset": candidate.get("best_era", ""),
        "group": candidate.get("group", ""),
        "group_field": candidate.get("group_field", ""),
        "rank": candidate.get("rank"),
        "priority_score": candidate.get("priority_score", 0),
        "best_era_candidates": candidate.get("best_era_candidates", 0),
        "mechanic_tier": candidate.get("mechanic_tier", 0),
        "mechanic_tier_label": candidate.get("mechanic_tier_label", ""),
        "priority_reasons": candidate.get("priority_reasons", []),
    }


def build_ninth_target_slice_selection(m66_decision: dict[str, Any] | None = None) -> dict[str, Any]:
    m66_decision = m66_decision or load_json(M66_04_DECISION)
    summary = m66_decision.get("summary", {})
    decision = m66_decision.get("decision", {})
    candidates = list(m66_decision.get("candidate_queue", []))
    selection_allowed = (
        bool(summary.get("ready_for_m67"))
        and bool(summary.get("ninth_slice_offline_pipeline_allowed"))
        and bool(decision.get("ninth_slice_offline_pipeline_allowed"))
        and bool(candidates)
    )
    selected_target = _candidate_to_selected_target(candidates[0]) if selection_allowed else {}

    return {
        "version": "M67-01",
        "description": "Ninth target slice selection",
        "source_inputs": {
            "eight_fixture_scale_decision": str(M66_04_DECISION.relative_to(ROOT)),
        },
        "scope": {
            "offline_target_selection": True,
            "creates_recipe_draft": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "bloom_token_runtime_enabled": False,
            "lock_runtime_enabled": False,
            "unlock_runtime_enabled": False,
            "legion_runtime_enabled": False,
            "mate_identity_check_enabled": False,
            "GameState_mutation": False,
        },
        "selection_policy": {
            "requires_m66_scale_allowed": True,
            "requires_ready_for_m67": True,
            "selects_first_available_candidate": True,
            "excludes_completed_fixture_groups": True,
            "selection_is_for_offline_analysis_only": True,
        },
        "selected_target": selected_target,
        "candidate_queue_snapshot": candidates,
        "decision": {
            "selection_allowed": selection_allowed,
            "ninth_slice_selected": bool(selected_target),
            "offline_analysis_only": True,
            "live_runtime_deck_enabled": False,
            "saved_deck_enabled": False,
            "ui_deck_list_enabled": False,
            "bot_playbook_enabled": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "bloom_token_runtime_enabled": False,
            "lock_runtime_enabled": False,
            "unlock_runtime_enabled": False,
            "legion_runtime_enabled": False,
            "mate_identity_check_enabled": False,
        },
        "summary": {
            "ninth_slice_selected": bool(selected_target),
            "selected_group": selected_target.get("group", ""),
            "selected_era_preset": selected_target.get("era_preset", ""),
            "selected_rank": selected_target.get("rank"),
            "candidate_count": len(candidates),
            "ready_for_m67_02": bool(selected_target),
        },
        "next_target": {
            "milestone": "M67-02" if selected_target else "M66-repair",
            "task": "Ninth-slice fixture/format readiness" if selected_target else "Repair eight-fixture scale decision",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    selected = report["selected_target"]
    decision = report["decision"]
    lines = [
        "# M67-01 Ninth Target Slice Selection",
        "",
        "## Summary",
        "",
        f"- Ninth slice selected: `{summary['ninth_slice_selected']}`",
        f"- Selected group: `{summary['selected_group']}`",
        f"- Selected era preset: `{summary['selected_era_preset']}`",
        f"- Selected rank: `{summary['selected_rank']}`",
        f"- Candidate count: `{summary['candidate_count']}`",
        f"- Ready for M67-02: `{summary['ready_for_m67_02']}`",
        "",
        "## Selected Target",
        "",
    ]
    if selected:
        for key in [
            "slice",
            "era_preset",
            "group",
            "group_field",
            "rank",
            "priority_score",
            "best_era_candidates",
            "mechanic_tier_label",
        ]:
            lines.append(f"- {key}: `{selected.get(key, '')}`")
    else:
        lines.append("- None")
    lines.extend(
        [
            "",
            "## Decision",
            "",
            f"- Selection allowed: `{decision['selection_allowed']}`",
            f"- Offline analysis only: `{decision['offline_analysis_only']}`",
            f"- Live runtime deck enabled: `{decision['live_runtime_deck_enabled']}`",
            f"- Saved deck enabled: `{decision['saved_deck_enabled']}`",
            f"- UI deck list enabled: `{decision['ui_deck_list_enabled']}`",
            f"- Bot playbook enabled: `{decision['bot_playbook_enabled']}`",
            f"- G Zone runtime enabled: `{decision['g_zone_runtime_enabled']}`",
            f"- Stride runtime enabled: `{decision['stride_runtime_enabled']}`",
            f"- Bloom/token runtime enabled: `{decision['bloom_token_runtime_enabled']}`",
            f"- Lock runtime enabled: `{decision['lock_runtime_enabled']}`",
            f"- Unlock runtime enabled: `{decision['unlock_runtime_enabled']}`",
            f"- Legion runtime enabled: `{decision['legion_runtime_enabled']}`",
            f"- Mate identity check enabled: `{decision['mate_identity_check_enabled']}`",
            "",
            "## Policy",
            "",
            "- This selects an offline analysis target only.",
            "- It does not create recipe drafts.",
            "- It does not create runtime fixtures.",
            "- It does not publish saved decks, UI entries, or bot playbooks.",
            "- It does not enable G Zone, Stride, Bloom/token, Lock, Unlock, Legion, or Mate runtime.",
            "- It does not mutate GameState.",
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
    parser = argparse.ArgumentParser(description="Build M67-01 ninth target slice selection.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_ninth_target_slice_selection()
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M67-01 ninth target slice selection wrote {json_path}")
    print(f"M67-01 ninth target slice selection summary wrote {md_path}")
    print(
        "selected={selected} group={group} next={next_ready}".format(
            selected=report["summary"]["ninth_slice_selected"],
            group=report["summary"]["selected_group"],
            next_ready=report["summary"]["ready_for_m67_02"],
        )
    )
    return 0 if report["summary"]["ready_for_m67_02"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
