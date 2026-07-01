"""Build the M35-E4 bot integration gate report.

This is an offline safety gate. It decides which reviewed advisory artifacts
may be considered as future bot hints and explicitly blocks unreviewed semantic
or compatibility probe data from runtime/bot use.
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any, Sequence


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_DIR = ROOT / "outputs" / "target_slice"
D4_REVIEWED_SEED = OUTPUT_DIR / "m35_d4_first_slice_reviewed_playbook_seed.json"
E3_PROBE = OUTPUT_DIR / "m35_e3_generalized_semantic_compatibility_probe.json"

EVIDENCE_FILES = [
    ROOT / "docs" / "CORE_DEVELOPMENT_GUARDRAILS.md",
    ROOT / "docs" / "specs" / "bot_and_headless" / "BOT_LEGAL_ACTION_MASKED_STATE_GATE_SPEC.md",
    ROOT / "docs" / "specs" / "bot_and_headless" / "PLAYBOOK_INTEGRATION_SPEC.md",
]

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _evidence_checks() -> list[dict[str, Any]]:
    return [
        {
            "path": str(path.relative_to(ROOT)),
            "exists": path.exists(),
        }
        for path in EVIDENCE_FILES
    ]


def _seed_candidate(seed: dict[str, Any], d4_report: dict[str, Any]) -> dict[str, Any]:
    policy = seed.get("runtime_policy", {})
    top_policy = d4_report.get("policy", {})
    checks = {
        "human_acceptance_required_before_runtime": bool(
            top_policy.get("human_acceptance_required_before_runtime")
        ),
        "not_runtime_playbook": bool(policy.get("not_runtime_playbook")),
        "not_published_to_bot": bool(policy.get("not_published_to_bot")),
        "not_auto_injected_into_decks": bool(policy.get("not_auto_injected_into_decks")),
        "legal_action_mask_required_before_future_bot_use": bool(
            policy.get("legal_action_mask_required_before_future_bot_use")
        ),
        "masked_state_required_before_future_bot_use": bool(
            policy.get("masked_state_required_before_future_bot_use")
        ),
    }
    allowed = all(checks.values())
    return {
        "seed_id": seed["seed_id"],
        "source_line_id": seed["source_line_id"],
        "anchor_card_id": seed["anchor_card_id"],
        "seed_status": seed.get("seed_status", ""),
        "allowed_as_future_bot_hint_candidate": allowed,
        "required_before_runtime_use": [
            "human_acceptance",
            "legal_action_mask",
            "masked_state_view",
            "RulesCore_command_validation",
            "no_live_card_text_parser",
            "player_readable_bot_trace",
        ],
        "checks": checks,
    }


def _blocked_probe_source(e3_report: dict[str, Any]) -> dict[str, Any]:
    readiness = e3_report.get("readiness", {})
    boundary = e3_report.get("runtime_boundary", {})
    return {
        "source": "M35-E3 semantic/compatibility probe",
        "selected_group": e3_report.get("selected_target", {}).get("group"),
        "edge_count": e3_report.get("stage_summaries", {})
        .get("c5_compatibility", {})
        .get("edge_count", 0),
        "candidate_edge_count": e3_report.get("stage_summaries", {})
        .get("c5_compatibility", {})
        .get("m35_d1_candidate_edge_count", 0),
        "blocked_from_runtime_bot": True,
        "reason": "E3 edges are generalized advisory compatibility evidence, not reviewed playbook hints.",
        "source_declares_no_runtime_or_bot_promotion": (
            bool(boundary.get("advisory_probe_only"))
            and bool(boundary.get("does_not_publish_to_bot"))
            and bool(boundary.get("does_not_publish_playbook_seed"))
            and not bool(readiness.get("runtime_or_bot_promotion_allowed"))
        ),
    }


def build_gate(
    d4_report: dict[str, Any] | None = None,
    e3_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    d4_report = d4_report or load_json(D4_REVIEWED_SEED)
    e3_report = e3_report or load_json(E3_PROBE)

    candidates = [
        _seed_candidate(seed, d4_report)
        for seed in d4_report.get("playbook_seed_entries", [])
    ]
    blocked_sources = [_blocked_probe_source(e3_report)]
    evidence_checks = _evidence_checks()
    all_evidence_present = all(item["exists"] for item in evidence_checks)
    all_candidates_safe = all(item["allowed_as_future_bot_hint_candidate"] for item in candidates)
    all_blocked_sources_safe = all(item["source_declares_no_runtime_or_bot_promotion"] for item in blocked_sources)
    gate_passed = all_evidence_present and all_candidates_safe and all_blocked_sources_safe

    return {
        "version": "M35-E4",
        "description": "Bot integration gate for reviewed playbook hints only",
        "source_inputs": {
            "reviewed_playbook_seed": str(D4_REVIEWED_SEED.relative_to(ROOT)),
            "generalized_semantic_compatibility_probe": str(E3_PROBE.relative_to(ROOT)),
        },
        "allowed_hint_candidates": candidates,
        "blocked_sources": blocked_sources,
        "gate_policy": {
            "bot_may_consume_only_reviewed_hint_candidates": True,
            "legal_action_mask_required": True,
            "masked_state_view_required": True,
            "RulesCore_command_validation_required": True,
            "direct_GameState_mutation_forbidden": True,
            "true_hidden_state_access_forbidden": True,
            "live_card_text_parsing_forbidden": True,
            "unreviewed_e3_probe_edges_forbidden": True,
            "auto_publish_to_runtime_or_bot_forbidden": True,
        },
        "evidence_checks": evidence_checks,
        "readiness": {
            "reviewed_hint_candidate_count": len(candidates),
            "blocked_source_count": len(blocked_sources),
            "all_evidence_present": all_evidence_present,
            "all_hint_candidates_require_safety_gates": all_candidates_safe,
            "all_unreviewed_sources_blocked": all_blocked_sources_safe,
            "gate_passed": gate_passed,
            "runtime_bot_integration_enabled": False,
            "ready_for_future_runtime_implementation_after_tests": gate_passed,
        },
        "next_target": {
            "milestone": "M35-closeout",
            "task": "Close Hybrid Vertical-Slice Strategy status and choose the next implementation queue",
            "must_create": [
                "phase closeout summary",
                "explicit next queue selection",
                "no runtime bot wiring until a Unity/C# implementation milestone is opened",
            ],
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_markdown(report: dict[str, Any], path: Path) -> None:
    readiness = report["readiness"]
    lines = [
        "# M35-E4 Bot Integration Gate",
        "",
        "## Summary",
        "",
        f"- Reviewed hint candidates: `{readiness['reviewed_hint_candidate_count']}`",
        f"- Blocked sources: `{readiness['blocked_source_count']}`",
        f"- Gate passed: `{readiness['gate_passed']}`",
        f"- Runtime bot integration enabled: `{readiness['runtime_bot_integration_enabled']}`",
        "",
        "## Allowed Future Hint Candidates",
        "",
    ]
    for candidate in report["allowed_hint_candidates"]:
        lines.append(
            f"- `{candidate['seed_id']}` anchor=`{candidate['anchor_card_id']}` "
            f"allowed=`{candidate['allowed_as_future_bot_hint_candidate']}`"
        )
    lines.extend(["", "## Blocked Sources", ""])
    for source in report["blocked_sources"]:
        lines.append(
            f"- `{source['source']}` edges=`{source['edge_count']}` "
            f"candidates=`{source['candidate_edge_count']}` blocked=`{source['blocked_from_runtime_bot']}`"
        )
    lines.extend(["", "## Required Runtime Gates", ""])
    for key, value in report["gate_policy"].items():
        lines.append(f"- `{key}`: `{value}`")
    lines.extend(
        [
            "",
            "## Next",
            "",
            "`M35-closeout`: close the Hybrid Vertical-Slice Strategy status and choose",
            "the next implementation queue. Runtime bot wiring remains disabled until a",
            "separate Unity/C# milestone opens with legal-action and masked-state tests.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M35-E4 bot integration gate report.")
    parser.add_argument("--reviewed-seed", type=Path, default=D4_REVIEWED_SEED)
    parser.add_argument("--e3-probe", type=Path, default=E3_PROBE)
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    d4_report = load_json(args.reviewed_seed)
    e3_report = load_json(args.e3_probe)
    report = build_gate(d4_report, e3_report)
    json_path = args.output_dir / "m35_e4_bot_integration_gate.json"
    md_path = args.output_dir / "m35_e4_bot_integration_gate.md"
    write_json(report, json_path)
    write_markdown(report, md_path)
    print(f"M35-E4 bot integration gate wrote {json_path}")
    print(f"M35-E4 bot integration gate summary wrote {md_path}")
    print(
        "gate_passed={passed} hint_candidates={hints} blocked_sources={blocked}".format(
            passed=report["readiness"]["gate_passed"],
            hints=report["readiness"]["reviewed_hint_candidate_count"],
            blocked=report["readiness"]["blocked_source_count"],
        )
    )
    return 0 if report["readiness"]["gate_passed"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
