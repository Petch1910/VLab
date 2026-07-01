"""Build the M36-05 second-slice readiness comparison report."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M35_CLOSEOUT = OUTPUT_DIR / "m35_closeout_hybrid_vertical_slice.json"
E2_READINESS = OUTPUT_DIR / "m35_e2_second_slice_fixture_readiness.json"
E3_PROBE = OUTPUT_DIR / "m35_e3_generalized_semantic_compatibility_probe.json"
M36_VALIDATION = OUTPUT_DIR / "m36_03_deck_recipe_validation_report.json"
M36_CONSISTENCY = OUTPUT_DIR / "m36_04_combo_recipe_consistency_report.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _get(data: dict[str, Any], path: Sequence[str], default: Any = None) -> Any:
    cursor: Any = data
    for key in path:
        if not isinstance(cursor, dict) or key not in cursor:
            return default
        cursor = cursor[key]
    return cursor


def build_comparison_report(
    closeout: dict[str, Any] | None = None,
    e2: dict[str, Any] | None = None,
    e3: dict[str, Any] | None = None,
    validation: dict[str, Any] | None = None,
    consistency: dict[str, Any] | None = None,
) -> dict[str, Any]:
    closeout = closeout or load_json(M35_CLOSEOUT)
    e2 = e2 or load_json(E2_READINESS)
    e3 = e3 or load_json(E3_PROBE)
    validation = validation or load_json(M36_VALIDATION)
    consistency = consistency or load_json(M36_CONSISTENCY)

    first = closeout["key_results"]["first_slice"]
    second = closeout["key_results"]["second_slice"]
    first_candidate_edges = int(first.get("clean_candidate_edges", 0))
    second_candidate_edges = int(second.get("probe_candidate_edges", 0))
    candidate_ratio = round(second_candidate_edges / first_candidate_edges, 3) if first_candidate_edges else None
    second_probe_ready = bool(_get(e3, ["readiness", "second_slice_semantic_compatibility_probe_passed"], False))
    second_fixture_ready = bool(_get(e2, ["readiness", "all_fixture_expectations_met"], False))
    first_has_runtime_ready = int(_get(validation, ["summary", "runtime_ready_recipe_count"], 0)) > 0
    first_has_promotable_line = int(_get(consistency, ["summary", "promotion_allowed_count"], 0)) > 0

    recommendation = (
        "second_slice_semantic_ready_but_hold_recipe_drafting_until_first_slice_review_blockers_clear"
        if second_probe_ready and second_fixture_ready and not first_has_runtime_ready
        else "second_slice_not_ready_for_recipe_pipeline"
    )

    return {
        "version": "M36-05",
        "description": "Second-slice readiness comparison before broader scale-out",
        "source_inputs": {
            "m35_closeout": str(M35_CLOSEOUT.relative_to(ROOT)),
            "second_slice_fixture_readiness": str(E2_READINESS.relative_to(ROOT)),
            "second_slice_probe": str(E3_PROBE.relative_to(ROOT)),
            "first_slice_recipe_validation": str(M36_VALIDATION.relative_to(ROOT)),
            "first_slice_combo_recipe_consistency": str(M36_CONSISTENCY.relative_to(ROOT)),
        },
        "scope": {
            "offline_readiness_comparison": True,
            "starts_second_slice_recipe_pipeline": False,
            "runtime_deck": False,
            "bot_integration": False,
            "automatic_deck_injection": False,
        },
        "first_slice_status": {
            "selected_target": first.get("selected_target", {}),
            "clean_candidate_edges": first_candidate_edges,
            "candidate_packages": first.get("candidate_packages", 0),
            "deck_skeletons": first.get("deck_skeletons", 0),
            "reviewed_playbook_seed_entries": first.get("reviewed_playbook_seed_entries", 0),
            "rejected_playbook_lines": first.get("rejected_playbook_lines", 0),
            "runtime_ready_recipe_count": _get(validation, ["summary", "runtime_ready_recipe_count"], 0),
            "promotion_allowed_count": _get(consistency, ["summary", "promotion_allowed_count"], 0),
            "blocked_by_review_count": _get(validation, ["summary", "blocked_by_review_count"], 0),
            "slot_gap_recipe_count": _get(validation, ["summary", "slot_gap_recipe_count"], 0),
        },
        "second_slice_status": {
            "selected_target": second.get("selected_target", {}),
            "fixture_expectations_met": second_fixture_ready,
            "classic_core_policy_reusable": bool(_get(e2, ["readiness", "classic_core_policy_reusable"], False)),
            "semantic_probe_ready": second_probe_ready,
            "probe_card_count": second.get("probe_card_count", 0),
            "probe_edge_count": second.get("probe_edge_count", 0),
            "probe_candidate_edges": second_candidate_edges,
            "manual_review_count": _get(e3, ["stage_summaries", "b4_manual_review_queue", "manual_review_count"], 0),
            "runtime_or_bot_promotion_allowed": bool(_get(e3, ["readiness", "runtime_or_bot_promotion_allowed"], False)),
        },
        "comparison": {
            "second_candidate_edge_ratio_vs_first": candidate_ratio,
            "second_manual_review_delta_vs_first": int(_get(e3, ["stage_summaries", "b4_manual_review_queue", "manual_review_count"], 0))
            - int(first.get("manual_review_cards", 0)),
            "first_slice_has_runtime_ready_recipe": first_has_runtime_ready,
            "first_slice_has_promotable_combo_line": first_has_promotable_line,
            "recommendation": recommendation,
        },
        "readiness": {
            "second_slice_ready_for_future_recipe_pipeline": second_probe_ready and second_fixture_ready,
            "broader_scaleout_runtime_allowed": False,
            "ready_for_m36_closeout": True,
        },
        "next_target": {
            "milestone": "M36-closeout",
            "task": "Deck recipe validation closeout",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    first = report["first_slice_status"]
    second = report["second_slice_status"]
    comparison = report["comparison"]
    readiness = report["readiness"]
    lines = [
        "# M36-05 Second-Slice Readiness Comparison",
        "",
        "## Summary",
        "",
        f"- First-slice runtime-ready recipes: `{first['runtime_ready_recipe_count']}`",
        f"- First-slice promotable combo lines: `{first['promotion_allowed_count']}`",
        f"- Second-slice fixture ready: `{second['fixture_expectations_met']}`",
        f"- Second-slice semantic probe ready: `{second['semantic_probe_ready']}`",
        f"- Second-slice probe candidate edges: `{second['probe_candidate_edges']}`",
        f"- Candidate edge ratio vs first slice: `{comparison['second_candidate_edge_ratio_vs_first']}`",
        f"- Broader scale-out runtime allowed: `{readiness['broader_scaleout_runtime_allowed']}`",
        f"- Ready for M36-closeout: `{readiness['ready_for_m36_closeout']}`",
        "",
        "## Recommendation",
        "",
        f"`{comparison['recommendation']}`",
        "",
        "## Policy",
        "",
        "- Second slice is probe-ready for future offline recipe work.",
        "- Do not start runtime/bot promotion from this comparison.",
        "- Keep first-slice review blockers visible before broader expansion.",
        "",
        "## Next",
        "",
        "`M36-closeout`: Deck recipe validation closeout.",
        "",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M36-05 second-slice readiness comparison.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_comparison_report()
    json_path = args.output_dir / "m36_05_second_slice_readiness_comparison.json"
    md_path = args.output_dir / "m36_05_second_slice_readiness_comparison.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M36-05 second-slice readiness comparison wrote {json_path}")
    print(f"M36-05 second-slice readiness comparison summary wrote {md_path}")
    print(
        "ready_for_m36_closeout={ready} second_ready={second} runtime_allowed={runtime}".format(
            ready=report["readiness"]["ready_for_m36_closeout"],
            second=report["readiness"]["second_slice_ready_for_future_recipe_pipeline"],
            runtime=report["readiness"]["broader_scaleout_runtime_allowed"],
        )
    )
    return 0 if report["readiness"]["ready_for_m36_closeout"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
