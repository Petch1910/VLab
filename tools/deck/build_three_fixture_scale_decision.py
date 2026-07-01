"""Build the M46-04 three-fixture scale decision artifact."""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M39_03_SMOKE = OUTPUT_DIR / "m39_03_headless_fixture_load_smoke.json"
M42_03_SMOKE = OUTPUT_DIR / "m42_03_second_fixture_load_smoke.json"
M46_03_SMOKE = OUTPUT_DIR / "m46_03_third_fixture_load_smoke.json"
ARCHETYPE_RANKING = ROOT / "outputs" / "archetype_priority" / "archetype_priority_ranking.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _resolve_path(value: str) -> Path:
    path = Path(value)
    if path.is_absolute():
        return path
    return ROOT / path


def _load_fixture_from_smoke(smoke: dict[str, Any]) -> dict[str, Any]:
    fixture_path = smoke.get("source_inputs", {}).get("runtime_fixture", "")
    if not fixture_path:
        return {}
    return load_json(_resolve_path(fixture_path))


def _fixture_evidence(label: str, smoke: dict[str, Any]) -> dict[str, Any]:
    summary = smoke.get("summary", {})
    fixture = smoke.get("fixture_summary", {})
    unity = smoke.get("unity_headless_result", {})
    return {
        "label": label,
        "fixture_id": fixture.get("fixture_id", ""),
        "recipe_id": fixture.get("recipe_id", ""),
        "main_deck_count": fixture.get("main_deck_count", 0),
        "unique_card_count": fixture.get("unique_card_count", 0),
        "offline_load_ready": bool(summary.get("offline_load_ready")),
        "deck_code_created": bool(summary.get("deck_code_created")),
        "unity_headless_smoke_passed": bool(summary.get("unity_headless_smoke_passed")),
        "blocking_issue_count": int(summary.get("blocking_issue_count", 0)),
        "deck_source": unity.get("deck_source", ""),
        "actions_executed": unity.get("actions_executed"),
        "event_count": unity.get("event_count"),
        "deck_code_sha256": smoke.get("headless_request", {}).get("deck_code_sha256", ""),
    }


def _fixture_passes(evidence: dict[str, Any]) -> bool:
    return (
        evidence["offline_load_ready"]
        and evidence["deck_code_created"]
        and evidence["unity_headless_smoke_passed"]
        and evidence["blocking_issue_count"] == 0
        and evidence["deck_source"] == "deck_code"
    )


def _completed_groups(fixtures: Sequence[dict[str, Any]]) -> set[str]:
    groups: set[str] = set()
    for fixture in fixtures:
        group = fixture.get("selected_target", {}).get("group", "")
        if group:
            groups.add(group)
    return groups


def _candidate_queue(ranking: dict[str, Any], completed_groups: set[str], limit: int = 5) -> list[dict[str, Any]]:
    candidates: list[dict[str, Any]] = []
    for row in ranking.get("rankings", []):
        if not row.get("feasible"):
            continue
        if row.get("group") in completed_groups:
            continue
        candidates.append(
            {
                "rank": row.get("rank"),
                "group": row.get("group", ""),
                "group_field": row.get("group_field", ""),
                "best_era": row.get("best_era", ""),
                "best_era_candidates": row.get("best_era_candidates", 0),
                "mechanic_tier": row.get("mechanic_tier", 0),
                "mechanic_tier_label": row.get("mechanic_tier_label", ""),
                "priority_score": row.get("priority_score", 0),
                "priority_reasons": row.get("priority_reasons", []),
            }
        )
        if len(candidates) >= limit:
            break
    return candidates


def build_three_fixture_scale_decision(
    first_smoke: dict[str, Any] | None = None,
    second_smoke: dict[str, Any] | None = None,
    third_smoke: dict[str, Any] | None = None,
    ranking: dict[str, Any] | None = None,
) -> dict[str, Any]:
    first_smoke = first_smoke or load_json(M39_03_SMOKE)
    second_smoke = second_smoke or load_json(M42_03_SMOKE)
    third_smoke = third_smoke or load_json(M46_03_SMOKE)
    ranking = ranking or load_json(ARCHETYPE_RANKING)
    fixtures = [
        _load_fixture_from_smoke(first_smoke),
        _load_fixture_from_smoke(second_smoke),
        _load_fixture_from_smoke(third_smoke),
    ]
    fixture_evidence = [
        _fixture_evidence("first_fixture_nova_grappler", first_smoke),
        _fixture_evidence("second_fixture_oracle_think_tank", second_smoke),
        _fixture_evidence("third_fixture_bermuda_triangle", third_smoke),
    ]
    failed = [item for item in fixture_evidence if not _fixture_passes(item)]
    completed_groups = _completed_groups(fixtures)
    candidates = _candidate_queue(ranking, completed_groups)
    scale_allowed = not failed and len(fixture_evidence) == 3 and bool(candidates)

    return {
        "version": "M46-04",
        "description": "Three-fixture scale decision",
        "source_inputs": {
            "first_fixture_headless_smoke": str(M39_03_SMOKE.relative_to(ROOT)),
            "second_fixture_headless_smoke": str(M42_03_SMOKE.relative_to(ROOT)),
            "third_fixture_headless_smoke": str(M46_03_SMOKE.relative_to(ROOT)),
            "archetype_priority_ranking": str(ARCHETYPE_RANKING.relative_to(ROOT)),
        },
        "scope": {
            "offline_scale_decision": True,
            "selects_runtime_deck": False,
            "creates_runtime_fixture": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "bot_playbook": False,
            "GameState_mutation": False,
        },
        "fixture_evidence": fixture_evidence,
        "completed_groups": sorted(completed_groups),
        "candidate_queue": candidates,
        "decision": {
            "fourth_slice_offline_pipeline_allowed": scale_allowed,
            "fourth_slice_selected_now": False,
            "recommended_next_action": "M47-01 fourth target slice selection" if scale_allowed else "repair_failed_fixture_evidence",
            "live_runtime_deck_enabled": False,
            "saved_deck_enabled": False,
            "ui_deck_list_enabled": False,
            "bot_playbook_enabled": False,
            "decision_notes": [
                "Three independent fixtures now pass offline and Unity headless load smoke.",
                "The next step may select a fourth slice for offline analysis only.",
                "Do not publish fixture decks to UI or bot runtime without a later explicit gate.",
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
            "fourth_slice_offline_pipeline_allowed": scale_allowed,
            "ready_for_m47": scale_allowed,
        },
        "next_target": {
            "milestone": "M47-01" if scale_allowed else "M46-repair",
            "task": "Fourth target slice selection" if scale_allowed else "Repair failed fixture evidence",
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    decision = report["decision"]
    lines = [
        "# M46-04 Three-Fixture Scale Decision",
        "",
        "## Summary",
        "",
        f"- Fixture evidence count: `{summary['fixture_evidence_count']}`",
        f"- Passed fixtures: `{summary['passed_fixture_count']}`",
        f"- Failed fixtures: `{summary['failed_fixture_count']}`",
        f"- Candidate count: `{summary['candidate_count']}`",
        f"- Fourth-slice offline pipeline allowed: `{summary['fourth_slice_offline_pipeline_allowed']}`",
        f"- Ready for M47: `{summary['ready_for_m47']}`",
        "",
        "## Fixture Evidence",
        "",
    ]
    for item in report["fixture_evidence"]:
        lines.append(
            "- `{label}` fixture=`{fixture}` recipe=`{recipe}` unity=`{unity}` actions=`{actions}` events=`{events}`".format(
                label=item["label"],
                fixture=item["fixture_id"],
                recipe=item["recipe_id"],
                unity=item["unity_headless_smoke_passed"],
                actions=item["actions_executed"],
                events=item["event_count"],
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
            f"- Fourth slice selected now: `{decision['fourth_slice_selected_now']}`",
            f"- Recommended next action: `{decision['recommended_next_action']}`",
            f"- Live runtime deck enabled: `{decision['live_runtime_deck_enabled']}`",
            f"- Saved deck enabled: `{decision['saved_deck_enabled']}`",
            f"- UI deck list enabled: `{decision['ui_deck_list_enabled']}`",
            f"- Bot playbook enabled: `{decision['bot_playbook_enabled']}`",
            "",
            "## Policy",
            "",
            "- This is an offline scale decision only.",
            "- It does not create a runtime fixture.",
            "- It does not inject saved decks.",
            "- It does not publish UI deck lists.",
            "- It does not enable bot playbooks.",
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
    parser = argparse.ArgumentParser(description="Build M46-04 three-fixture scale decision.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_three_fixture_scale_decision()
    json_path = args.output_dir / "m46_04_three_fixture_scale_decision.json"
    md_path = args.output_dir / "m46_04_three_fixture_scale_decision.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M46-04 three-fixture scale decision wrote {json_path}")
    print(f"M46-04 three-fixture scale decision summary wrote {md_path}")
    print(
        "ready_for_m47={ready} fixtures_passed={passed} candidates={candidates}".format(
            ready=report["summary"]["ready_for_m47"],
            passed=report["summary"]["passed_fixture_count"],
            candidates=report["summary"]["candidate_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m47"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
