"""Select the second deck/combo vertical slice for M35-E1.

This is an offline planning helper. It does not mutate runtime card data,
does not create player decks, and does not publish anything to bot/runtime
playbooks.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.select_first_target_slice import (  # noqa: E402
    ARCHETYPE_PRIORITY_JSON,
    DECK_POSSIBILITY_DIR,
    OUTPUT_DIR,
    SLICE_CONFIGS,
    SliceConfig,
    build_format_policy,
    load_deck_possibility,
    load_json,
    selected_group_deck_report,
)


FIRST_SLICE_REPORT_JSON = OUTPUT_DIR / "m35_a2_first_target_slice_report.json"
FIRST_SLICE_CLOSEOUT_JSON = OUTPUT_DIR / "m35_d4_first_slice_reviewed_playbook_seed.json"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def completed_groups_from_first_slice(first_report: dict[str, Any], closeout: dict[str, Any]) -> list[str]:
    groups: list[str] = []
    selected = first_report.get("selected_target", {}).get("group")
    closeout_selected = closeout.get("selected_target", {}).get("group")
    if selected:
        groups.append(str(selected))
    if closeout_selected and closeout_selected not in groups:
        groups.append(str(closeout_selected))
    return groups


def select_second_candidate(
    rankings: Sequence[dict[str, Any]],
    deck_report: dict[str, Any],
    config: SliceConfig,
    excluded_groups: Sequence[str],
) -> dict[str, Any]:
    deck_groups = {group["group"]: group for group in deck_report.get("groups", [])}
    excluded = set(excluded_groups)
    candidates: list[dict[str, Any]] = []
    for row in rankings:
        if not row.get("feasible"):
            continue
        if row.get("best_era") != config.era_preset:
            continue
        if row.get("group") in excluded:
            continue
        if row.get("group") not in deck_groups:
            continue
        candidates.append(row)

    if not candidates:
        raise ValueError(f"No second-slice candidate found for slice {config.key}")

    candidates.sort(key=lambda row: (int(row.get("rank", 999999)), -float(row.get("priority_score", 0))))
    return candidates[0]


def build_candidate_summary(
    rankings: Sequence[dict[str, Any]],
    config: SliceConfig,
    excluded_groups: Sequence[str],
    limit: int = 10,
) -> list[dict[str, Any]]:
    excluded = set(excluded_groups)
    rows = [
        row
        for row in rankings
        if row.get("feasible")
        and row.get("best_era") == config.era_preset
        and row.get("group") not in excluded
    ]
    rows.sort(key=lambda row: (int(row.get("rank", 999999)), -float(row.get("priority_score", 0))))
    return [
        {
            "rank": row.get("rank"),
            "group": row.get("group"),
            "priority_score": row.get("priority_score"),
            "best_era": row.get("best_era"),
            "best_era_candidates": row.get("best_era_candidates"),
            "mechanic_tier": row.get("mechanic_tier"),
            "mechanic_tier_label": row.get("mechanic_tier_label"),
            "priority_reasons": row.get("priority_reasons", []),
        }
        for row in rows[:limit]
    ]


def build_report(preferred_slice: str = "classic_core") -> dict[str, Any]:
    if preferred_slice not in SLICE_CONFIGS:
        raise ValueError(f"Unknown preferred slice: {preferred_slice}")
    config = SLICE_CONFIGS[preferred_slice]
    priority = load_json(ARCHETYPE_PRIORITY_JSON)
    first_report = load_json(FIRST_SLICE_REPORT_JSON)
    closeout = load_json(FIRST_SLICE_CLOSEOUT_JSON)
    deck_report = load_deck_possibility(config.era_preset)

    excluded_groups = completed_groups_from_first_slice(first_report, closeout)
    rankings = priority.get("rankings", [])
    selected = select_second_candidate(rankings, deck_report, config, excluded_groups)
    group_report = selected_group_deck_report(deck_report, selected["group"])

    first_target = first_report.get("selected_target", {})
    closeout_summary = closeout.get("summary", {})
    return {
        "version": "M35-E1",
        "description": "Second target slice selection after the first reviewed playbook seed export",
        "selection_policy": {
            "preferred_slice": config.key,
            "reason": (
                "Continue the current user/team Classic Core clan-era deck/combo scope "
                "before broad format scale-out. Exclude first-slice groups that already "
                "closed through M35-D4, then choose the highest-ranked feasible M34-03 "
                "candidate in the same era preset."
            ),
            "source_priority": "M34-03 archetype priority ranking",
            "excluded_completed_groups": excluded_groups,
            "do_not_assume_standard_by_default": True,
            "no_runtime_or_bot_promotion": True,
        },
        "previous_target": {
            "version": first_report.get("version"),
            "slice": first_target.get("slice"),
            "era_preset": first_target.get("era_preset"),
            "group": first_target.get("group"),
            "rank": first_target.get("rank"),
            "priority_score": first_target.get("priority_score"),
            "d4_seed_entries": closeout_summary.get("seed_entry_count", 0),
            "d4_rejected_lines": closeout_summary.get("rejected_line_count", 0),
        },
        "selected_target": {
            "slice": config.display_name,
            "era_preset": config.era_preset,
            "group": selected["group"],
            "group_field": selected.get("group_field", ""),
            "rank": selected.get("rank"),
            "priority_score": selected.get("priority_score"),
            "best_era_candidates": selected.get("best_era_candidates"),
            "mechanic_tier": selected.get("mechanic_tier"),
            "mechanic_tier_label": selected.get("mechanic_tier_label"),
            "priority_reasons": selected.get("priority_reasons", []),
        },
        "candidate_summary": build_candidate_summary(rankings, config, excluded_groups),
        "input_evidence": {
            "archetype_priority": str(ARCHETYPE_PRIORITY_JSON.relative_to(ROOT)),
            "first_target_slice": str(FIRST_SLICE_REPORT_JSON.relative_to(ROOT)),
            "first_slice_closeout": str(FIRST_SLICE_CLOSEOUT_JSON.relative_to(ROOT)),
            "deck_possibility": str((DECK_POSSIBILITY_DIR / f"{config.era_preset}_deck_possibility.json").relative_to(ROOT)),
            "missing_set_codes": deck_report.get("scope", {}).get("missing_set_codes", []),
        },
        "format_policy": build_format_policy(config, selected, group_report),
        "runtime_boundary": {
            "advisory_selection_only": True,
            "does_not_create_deck": True,
            "does_not_mutate_runtime_pack": True,
            "does_not_publish_to_bot": True,
            "requires_m35_e2_fixture_or_slice_refresh_before_semantic_scaleout": True,
        },
        "next_target": {
            "milestone": "M35-E2",
            "task": "Second-slice fixture/format readiness check before semantic scale-out",
            "must_create": [
                "selected second-slice source-backed deck fixture",
                "selected second-slice failing trigger/count fixtures",
                "decision on reusing Classic Core policy or adding new format/mechanic fixtures",
                "no promotion to bot/runtime until the second slice repeats B-D gates or an explicit shortcut is approved",
            ],
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    target = report["selected_target"]
    previous = report["previous_target"]
    candidates = [
        f"- #{row['rank']} {row['group']} score={row['priority_score']} "
        f"candidates={row['best_era_candidates']} tier={row['mechanic_tier_label']}"
        for row in report["candidate_summary"][:5]
    ]
    text = f"""# M35-E1 Second Target Slice Report

## Selected Target

- Slice: `{target['slice']}`
- Era preset: `{target['era_preset']}`
- Group: `{target['group']}`
- M34-03 rank: `{target['rank']}`
- Priority score: `{target['priority_score']}`
- Mechanic tier: `{target['mechanic_tier_label']}`

## Previous Closed Slice

- Group: `{previous['group']}`
- M34-03 rank: `{previous['rank']}`
- D4 seed entries: `{previous['d4_seed_entries']}`
- D4 rejected lines: `{previous['d4_rejected_lines']}`

## Why This Slice

The current user/team scope is still Classic Core clan-era deck/combo work.
M35-E1 excludes the first completed slice and selects the next highest-ranked
feasible Classic Core group from M34-03.

## Top Remaining Classic Core Candidates

{chr(10).join(candidates)}

## Runtime Boundary

- Advisory selection only.
- Does not create or edit player decks.
- Does not mutate runtime card packs.
- Does not publish to bot/runtime playbooks.

## Next

`M35-E2`: create or refresh source-backed fixtures for this second slice and
decide whether Classic Core policy can be reused as-is before semantic scale-out.
"""
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Select second target deck/combo slice for M35-E1.")
    parser.add_argument("--preferred-slice", choices=sorted(SLICE_CONFIGS), default="classic_core")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_report(args.preferred_slice)
    json_path = args.output_dir / "m35_e1_second_target_slice_report.json"
    md_path = args.output_dir / "m35_e1_second_target_slice_report.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    selected = report["selected_target"]
    print(f"M35-E1 selected {selected['slice']} / {selected['group']} (rank {selected['rank']})")
    print(f"JSON: {json_path}")
    print(f"MD:   {md_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
