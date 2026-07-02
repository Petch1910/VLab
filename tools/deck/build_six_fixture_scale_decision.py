"""Build the M58-04 six-fixture scale decision artifact."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_four_fixture_scale_decision import (  # noqa: E402
    ARCHETYPE_RANKING,
    M39_03_SMOKE,
    M42_03_SMOKE,
    M46_03_SMOKE,
    M50_03_SMOKE,
    _candidate_queue,
    _completed_groups,
    _fixture_evidence,
    _fixture_passes,
    _load_fixture_from_smoke,
    load_json,
)
from tools.deck.build_five_fixture_scale_decision import M54_03_SMOKE  # noqa: E402


OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M58_03_SMOKE = OUTPUT_DIR / "m58_03_sixth_fixture_load_smoke.json"
JSON_OUTPUT = OUTPUT_DIR / "m58_04_six_fixture_scale_decision.json"
MD_OUTPUT = OUTPUT_DIR / "m58_04_six_fixture_scale_decision.md"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def build_six_fixture_scale_decision(
    first_smoke: dict[str, Any] | None = None,
    second_smoke: dict[str, Any] | None = None,
    third_smoke: dict[str, Any] | None = None,
    fourth_smoke: dict[str, Any] | None = None,
    fifth_smoke: dict[str, Any] | None = None,
    sixth_smoke: dict[str, Any] | None = None,
    ranking: dict[str, Any] | None = None,
    sixth_fixture: dict[str, Any] | None = None,
) -> dict[str, Any]:
    first_smoke = first_smoke or load_json(M39_03_SMOKE)
    second_smoke = second_smoke or load_json(M42_03_SMOKE)
    third_smoke = third_smoke or load_json(M46_03_SMOKE)
    fourth_smoke = fourth_smoke or load_json(M50_03_SMOKE)
    fifth_smoke = fifth_smoke or load_json(M54_03_SMOKE)
    sixth_smoke = sixth_smoke or load_json(M58_03_SMOKE)
    ranking = ranking or load_json(ARCHETYPE_RANKING)

    fixtures = [
        _load_fixture_from_smoke(first_smoke),
        _load_fixture_from_smoke(second_smoke),
        _load_fixture_from_smoke(third_smoke),
        _load_fixture_from_smoke(fourth_smoke),
        _load_fixture_from_smoke(fifth_smoke),
        sixth_fixture if sixth_fixture is not None else _load_fixture_from_smoke(sixth_smoke),
    ]
    fixture_evidence = [
        _fixture_evidence("first_fixture_nova_grappler", first_smoke),
        _fixture_evidence("second_fixture_oracle_think_tank", second_smoke),
        _fixture_evidence("third_fixture_bermuda_triangle", third_smoke),
        _fixture_evidence("fourth_fixture_royal_paladin_g_series", fourth_smoke),
        _fixture_evidence("fifth_fixture_gold_paladin", fifth_smoke),
        _fixture_evidence("sixth_fixture_shadow_paladin", sixth_smoke),
    ]
    failed = [item for item in fixture_evidence if not _fixture_passes(item)]
    completed_groups = _completed_groups(fixtures)
    candidates = _candidate_queue(ranking, completed_groups)
    scale_allowed = not failed and len(fixture_evidence) == 6 and bool(candidates)

    return {
        "version": "M58-04",
        "description": "Six-fixture scale decision",
        "source_inputs": {
            "first_fixture_headless_smoke": str(M39_03_SMOKE.relative_to(ROOT)),
            "second_fixture_headless_smoke": str(M42_03_SMOKE.relative_to(ROOT)),
            "third_fixture_headless_smoke": str(M46_03_SMOKE.relative_to(ROOT)),
            "fourth_fixture_headless_smoke": str(M50_03_SMOKE.relative_to(ROOT)),
            "fifth_fixture_headless_smoke": str(M54_03_SMOKE.relative_to(ROOT)),
            "sixth_fixture_headless_smoke": str(M58_03_SMOKE.relative_to(ROOT)),
            "archetype_priority_ranking": str(ARCHETYPE_RANKING.relative_to(ROOT)),
        },
        "scope": {
            "offline_scale_decision": True,
            "selects_runtime_deck": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "GameState_mutation": False,
        },
        "fixture_evidence": fixture_evidence,
        "completed_groups": sorted(completed_groups),
        "candidate_queue": candidates,
        "decision": {
            "seventh_slice_offline_pipeline_allowed": scale_allowed,
            "seventh_slice_selected_now": False,
            "recommended_next_action": "M59-01 seventh target slice selection" if scale_allowed else "repair_failed_fixture_evidence",
            "live_runtime_deck_enabled": False,
            "saved_deck_enabled": False,
            "ui_deck_list_enabled": False,
            "bot_playbook_enabled": False,
            "g_zone_runtime_enabled": False,
            "stride_runtime_enabled": False,
            "decision_notes": [
                "Six independent fixtures now pass offline and Unity headless load smoke.",
                "The next step may define and select a seventh slice for offline analysis only.",
                "Do not publish fixture decks to UI or bot runtime without a later explicit gate.",
                "G Zone and Stride runtime remain disabled despite G-series fixture evidence.",
            ]
            if scale_allowed
            else [
                "At least one fixture smoke is not ready for scale-out.",
                "Repair failed evidence before selecting another slice.",
            ],
        },
        "summary": {
            "fixture_evidence_count": len(fixture_evidence),
            "passed_fixture_count": len(fixture_evidence) - len(failed),
            "failed_fixture_count": len(failed),
            "candidate_count": len(candidates),
            "seventh_slice_offline_pipeline_allowed": scale_allowed,
            "ready_for_m59": scale_allowed,
        },
        "next_target": {
            "milestone": "M59-01" if scale_allowed else "M58-repair",
            "task": "Seventh target slice selection" if scale_allowed else "Repair failed fixture evidence",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    lines = [
        "# M58-04 Six-Fixture Scale Decision",
        "",
        "## Summary",
        "",
        f"- Fixture evidence count: `{summary['fixture_evidence_count']}`",
        f"- Passed fixtures: `{summary['passed_fixture_count']}`",
        f"- Failed fixtures: `{summary['failed_fixture_count']}`",
        f"- Candidate count: `{summary['candidate_count']}`",
        f"- Seventh-slice offline pipeline allowed: `{summary['seventh_slice_offline_pipeline_allowed']}`",
        f"- Ready for M59: `{summary['ready_for_m59']}`",
        "",
        "## Fixture Evidence",
        "",
    ]
    for item in report["fixture_evidence"]:
        lines.append(
            "- `{label}` fixture=`{fixture}` recipe=`{recipe}` unity=`{unity}` actions=`{actions}` events=`{events}` g_zone_count=`{g}`".format(
                label=item["label"],
                fixture=item["fixture_id"],
                recipe=item["recipe_id"],
                unity=item["unity_headless_smoke_passed"],
                actions=item["actions_executed"],
                events=item["event_count"],
                g=item["g_zone_count"],
            )
        )
    lines.extend(["", "## Candidate Queue", ""])
    for candidate in report["candidate_queue"]:
        lines.append(
            "- rank `{rank}` group `{group}` era `{era}` score `{score}`".format(
                rank=candidate["rank"],
                group=candidate["group"],
                era=candidate["best_era"],
                score=candidate["priority_score"],
            )
        )
    lines.extend(
        [
            "",
            "## Decision",
            "",
            f"- Seventh slice selected now: `{decision['seventh_slice_selected_now']}`",
            f"- Recommended next action: `{decision['recommended_next_action']}`",
            f"- Live runtime deck enabled: `{decision['live_runtime_deck_enabled']}`",
            f"- Saved deck enabled: `{decision['saved_deck_enabled']}`",
            f"- UI deck list enabled: `{decision['ui_deck_list_enabled']}`",
            f"- Bot playbook enabled: `{decision['bot_playbook_enabled']}`",
            f"- G Zone runtime enabled: `{decision['g_zone_runtime_enabled']}`",
            f"- Stride runtime enabled: `{decision['stride_runtime_enabled']}`",
            "",
            "## Policy",
            "",
            "- This is an offline scale decision only.",
            "- It does not create a runtime fixture.",
            "- It does not inject saved decks.",
            "- It does not publish UI deck lists.",
            "- It does not enable bot playbooks.",
            "- It does not enable G Zone or Stride runtime.",
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
    parser = argparse.ArgumentParser(description="Build M58-04 six-fixture scale decision.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_six_fixture_scale_decision()
    json_path = args.output_dir / JSON_OUTPUT.name
    md_path = args.output_dir / MD_OUTPUT.name
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M58-04 six-fixture scale decision wrote {json_path}")
    print(f"M58-04 six-fixture scale decision summary wrote {md_path}")
    print(
        "ready_for_m59={ready} fixtures_passed={passed} candidates={candidates}".format(
            ready=report["summary"]["ready_for_m59"],
            passed=report["summary"]["passed_fixture_count"],
            candidates=report["summary"]["candidate_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m59"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
