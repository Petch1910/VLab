"""Build the M40-01 second-slice review packet.

The packet rebuilds the Oracle Think Tank selected-slice semantic pipeline in
memory and exports reviewable fixture notes, manual-review cards, and candidate
compatibility edges. It stays offline-only: no deck recipe, saved deck, runtime
fixture, UI publication, or bot playbook is created here.
"""

from __future__ import annotations

import argparse
import csv
import json
import sys
from pathlib import Path
from typing import Any, Iterable, Sequence


ROOT = Path(__file__).resolve().parents[2]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_selected_slice_semantic_compatibility_probe import (  # noqa: E402
    SECOND_SLICE_READINESS,
    SECOND_SLICE_REPORT,
    build_compatibility_output,
    build_pair_graph,
    build_probe,
    build_queue,
    build_requirement_provider,
    build_resource_detector,
    build_semantic_tags,
    build_timing_detector,
    build_vocabulary,
    build_zone_detector,
)


OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M35_E3_REPORT = OUTPUT_DIR / "m35_e3_generalized_semantic_compatibility_probe.json"
M39_04_REPORT = OUTPUT_DIR / "m39_04_second_slice_recipe_scale_decision.json"


if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise FileNotFoundError(f"Required input not found: {path}")
    return json.loads(path.read_text(encoding="utf-8-sig"))


def _rel(path: Path) -> str:
    return str(path.relative_to(ROOT))


def _build_pipeline_details(
    selected_report: dict[str, Any],
    readiness_report: dict[str, Any],
) -> dict[str, Any]:
    vocabulary = build_vocabulary(readiness_report)
    semantic_report = build_semantic_tags(selected_report, vocabulary)
    provider_report = build_requirement_provider(semantic_report)
    manual_review_report = build_queue(semantic_report, provider_report)
    pair_graph_report = build_pair_graph(provider_report, manual_review_report)
    resource_report = build_resource_detector(pair_graph_report)
    timing_report = build_timing_detector(pair_graph_report, resource_report)
    zone_report = build_zone_detector(pair_graph_report, resource_report, timing_report)
    compatibility_report = build_compatibility_output(
        pair_graph_report,
        resource_report,
        timing_report,
        zone_report,
    )
    probe_report = build_probe(selected_report, readiness_report)
    return {
        "probe_report": probe_report,
        "manual_review_report": manual_review_report,
        "compatibility_report": compatibility_report,
    }


def _fixture_note_item(fixture: dict[str, Any]) -> dict[str, Any]:
    validation = fixture.get("validation", {})
    cards = fixture.get("cards", [])
    expected = fixture.get("expected", "")
    accepted = bool(validation.get("accepted"))
    expectation_met = bool(fixture.get("expectation_met"))
    return {
        "item_type": "fixture_note",
        "item_id": fixture.get("fixture_id", ""),
        "fixture_id": fixture.get("fixture_id", ""),
        "expected": expected,
        "expectation_met": expectation_met,
        "validation_accepted": accepted,
        "validation_reasons": validation.get("reasons", []),
        "description": fixture.get("description", ""),
        "unique_card_count": len(cards),
        "total_card_count": sum(int(card.get("count", 0)) for card in cards),
        "review_priority": "P0_fixture_policy_evidence" if expected == "pass" else "P2_negative_fixture_evidence",
        "review_reasons": (
            ["fixture_expectation_met"]
            if expectation_met
            else ["fixture_expectation_mismatch"]
        ),
        "blocked_until": [
            "fixture_expectations_met",
            "offline_recipe_packet_reviewed",
        ],
        "recommended_next_action": (
            "use_as_second_slice_policy_evidence"
            if expectation_met
            else "repair_fixture_before_recipe_draft"
        ),
    }


def _manual_card_item(card: dict[str, Any]) -> dict[str, Any]:
    return {
        "item_type": "manual_review_card",
        "item_id": card["card_id"],
        "card_id": card["card_id"],
        "name_th": card.get("name_th", ""),
        "series_code": card.get("series_code", ""),
        "grade": card.get("grade", ""),
        "type_1": card.get("type_1", ""),
        "trigger": card.get("trigger", ""),
        "review_priority": "P1_semantic_gap_review",
        "review_reasons": card.get("manual_review_reasons", []),
        "unmapped_feature_tags": card.get("unmapped_feature_tags", []),
        "requirements": card.get("requirements", []),
        "providers": card.get("providers", []),
        "blocked_until": [
            "semantic_mapping_reviewed",
            "fixture_added_if_needed",
            "human_acceptance",
        ],
        "recommended_next_action": "map_or_defer_unmapped_feature_tags_before_recipe",
    }


def _candidate_edge_item(edge: dict[str, Any], rank: int) -> dict[str, Any]:
    return {
        "item_type": "candidate_edge",
        "item_id": edge["edge"],
        "edge_rank": rank,
        "edge": edge["edge"],
        "source_card_id": edge["source_card_id"],
        "source_name_th": edge.get("source_name_th", ""),
        "target_card_id": edge["target_card_id"],
        "target_name_th": edge.get("target_name_th", ""),
        "status": edge.get("status", ""),
        "net_score": edge.get("net_score", 0),
        "pair_graph_score": edge.get("pair_graph_score", 0),
        "resource_verdict": edge.get("resource_verdict", ""),
        "timing_verdict": edge.get("timing_verdict", ""),
        "zone_verdict": edge.get("zone_verdict", ""),
        "synergy_reasons": edge.get("synergy_reasons", []),
        "conflict_reasons": edge.get("conflict_reasons", []),
        "missing_data_reasons": edge.get("missing_data_reasons", []),
        "manual_review_reasons": edge.get("manual_review_reasons", []),
        "review_priority": "P2_candidate_edge_review",
        "review_reasons": [
            "second_slice_candidate_compatibility_edge",
            "requires_human_recipe_selection_before_m40_02",
        ],
        "blocked_until": [
            "human_recipe_selection",
            "deck_recipe_validator",
            "combo_line_to_recipe_consistency_check",
        ],
        "recommended_next_action": "consider_for_m40_02_advisory_recipe_draft",
    }


def _csv_rows(report: dict[str, Any]) -> Iterable[dict[str, str]]:
    for section in ("fixture_note_items", "manual_card_review_items", "candidate_edge_review_items"):
        for item in report[section]:
            yield {
                "item_type": item["item_type"],
                "item_id": item["item_id"],
                "source_card_id": item.get("source_card_id") or item.get("card_id", ""),
                "target_card_id": item.get("target_card_id", ""),
                "review_priority": item["review_priority"],
                "status": item.get("status", ""),
                "net_score": str(item.get("net_score", "")),
                "review_reasons": ";".join(item.get("review_reasons", [])),
                "blocked_until": ";".join(item.get("blocked_until", [])),
                "recommended_next_action": item["recommended_next_action"],
            }


def build_second_slice_review_packet(
    selected_report: dict[str, Any] | None = None,
    readiness_report: dict[str, Any] | None = None,
    e3_report: dict[str, Any] | None = None,
    m39_04_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    selected_report = selected_report or load_json(SECOND_SLICE_REPORT)
    readiness_report = readiness_report or load_json(SECOND_SLICE_READINESS)
    e3_report = e3_report or load_json(M35_E3_REPORT)
    m39_04_report = m39_04_report or load_json(M39_04_REPORT)

    pipeline = _build_pipeline_details(selected_report, readiness_report)
    manual_report = pipeline["manual_review_report"]
    compatibility_report = pipeline["compatibility_report"]
    probe_report = pipeline["probe_report"]

    fixtures = [_fixture_note_item(fixture) for fixture in readiness_report.get("fixtures", [])]
    manual_cards = [_manual_card_item(card) for card in manual_report.get("manual_review_queue", [])]
    candidate_edges = [
        _candidate_edge_item(edge, rank)
        for rank, edge in enumerate(
            [item for item in compatibility_report.get("compatibility_edges", []) if item.get("candidate_for_m35_d1")],
            start=1,
        )
    ]

    expected_manual_count = e3_report.get("stage_summaries", {}).get("b4_manual_review_queue", {}).get(
        "manual_review_count"
    )
    expected_candidate_count = e3_report.get("stage_summaries", {}).get("c5_compatibility", {}).get(
        "m35_d1_candidate_edge_count"
    )
    all_fixture_expectations_met = all(item["expectation_met"] for item in fixtures)
    offline_recipe_allowed = bool(
        m39_04_report.get("decision", {}).get("offline_recipe_pipeline_allowed")
        and m39_04_report.get("summary", {}).get("ready_for_m40")
    )
    promotion_closed = not any(
        bool(m39_04_report.get("decision", {}).get(key))
        for key in (
            "runtime_deck_promotion_allowed",
            "saved_deck_or_ui_publication_allowed",
            "bot_playbook_promotion_allowed",
        )
    )
    ready = (
        offline_recipe_allowed
        and promotion_closed
        and readiness_report.get("readiness", {}).get("selected_group_fixture_ready") is True
        and e3_report.get("readiness", {}).get("second_slice_semantic_compatibility_probe_passed") is True
        and probe_report.get("readiness", {}).get("second_slice_semantic_compatibility_probe_passed") is True
        and all_fixture_expectations_met
        and len(manual_cards) == expected_manual_count
        and len(candidate_edges) == expected_candidate_count
    )

    return {
        "version": "M40-01",
        "description": "Second-slice review packet before advisory deck recipe drafting",
        "selected_target": selected_report.get("selected_target", {}),
        "source_inputs": {
            "second_slice_report": _rel(SECOND_SLICE_REPORT),
            "second_slice_fixture_readiness": _rel(SECOND_SLICE_READINESS),
            "generalized_semantic_compatibility_probe": _rel(M35_E3_REPORT),
            "second_slice_recipe_scale_decision": _rel(M39_04_REPORT),
        },
        "scope": {
            "offline_review_packet": True,
            "deck_recipe_draft": False,
            "saved_deck_injection": False,
            "ui_deck_list_publication": False,
            "runtime_deck_promotion": False,
            "bot_playbook_promotion": False,
            "direct_GameState_mutation": False,
        },
        "review_policy": {
            "human_selection_required_before_recipe_draft": True,
            "manual_review_cards_blocked_from_recipe_until_resolved": True,
            "candidate_edges_are_advisory_inputs_only": True,
            "fixtures_are_policy_evidence_not_saved_decks": True,
            "no_live_card_text_parsing": True,
            "no_direct_GameState_mutation": True,
        },
        "evidence_summary": {
            "m39_04_offline_recipe_pipeline_allowed": offline_recipe_allowed,
            "m39_04_runtime_promotion_closed": promotion_closed,
            "fixture_expectations_met": all_fixture_expectations_met,
            "probe_card_count": e3_report.get("stage_summaries", {}).get("b2_semantic_tags", {}).get("card_count"),
            "probe_edge_count": e3_report.get("stage_summaries", {}).get("c5_compatibility", {}).get("edge_count"),
            "expected_manual_review_count": expected_manual_count,
            "expected_candidate_edge_count": expected_candidate_count,
            "rebuilt_manual_review_count": len(manual_cards),
            "rebuilt_candidate_edge_count": len(candidate_edges),
        },
        "summary": {
            "fixture_note_count": len(fixtures),
            "manual_card_item_count": len(manual_cards),
            "candidate_edge_item_count": len(candidate_edges),
            "total_review_item_count": len(fixtures) + len(manual_cards) + len(candidate_edges),
            "ready_for_m40_02": ready,
        },
        "fixture_note_items": fixtures,
        "manual_card_review_items": manual_cards,
        "candidate_edge_review_items": candidate_edges,
        "next_target": {
            "milestone": "M40-02",
            "task": "Second-slice recipe draft model",
            "blocked_until": [
                "M40-01 review packet is available",
                "candidate edges and manual-review cards are inspectable",
                "drafts remain advisory and offline only",
            ],
        },
    }


def write_json(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_csv(report: dict[str, Any], path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    rows = list(_csv_rows(report))
    fieldnames = [
        "item_type",
        "item_id",
        "source_card_id",
        "target_card_id",
        "review_priority",
        "status",
        "net_score",
        "review_reasons",
        "blocked_until",
        "recommended_next_action",
    ]
    with path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        writer.writerows(rows)


def write_markdown(report: dict[str, Any], path: Path) -> None:
    summary = report["summary"]
    target = report["selected_target"]
    evidence = report["evidence_summary"]
    lines = [
        "# M40-01 Second-Slice Review Packet",
        "",
        "## Summary",
        "",
        f"- Target slice: `{target.get('slice', '')}` / `{target.get('group', '')}`",
        f"- Fixture notes: `{summary['fixture_note_count']}`",
        f"- Manual-review cards: `{summary['manual_card_item_count']}`",
        f"- Candidate edges: `{summary['candidate_edge_item_count']}`",
        f"- Total review items: `{summary['total_review_item_count']}`",
        f"- Ready for M40-02: `{summary['ready_for_m40_02']}`",
        "",
        "## Evidence",
        "",
        f"- M39-04 offline recipe pipeline allowed: `{evidence['m39_04_offline_recipe_pipeline_allowed']}`",
        f"- Runtime/saved deck/bot promotion closed: `{evidence['m39_04_runtime_promotion_closed']}`",
        f"- Fixture expectations met: `{evidence['fixture_expectations_met']}`",
        f"- Probe cards: `{evidence['probe_card_count']}`",
        f"- Probe edges: `{evidence['probe_edge_count']}`",
        f"- Expected candidate edges: `{evidence['expected_candidate_edge_count']}`",
        "",
        "## Fixture Notes",
        "",
    ]
    for item in report["fixture_note_items"]:
        lines.append(
            f"- `{item['fixture_id']}` expected=`{item['expected']}` "
            f"met=`{item['expectation_met']}` reasons=`{','.join(item['validation_reasons']) or 'none'}`"
        )
    lines.extend(["", "## Manual-Review Cards", ""])
    for item in report["manual_card_review_items"]:
        lines.append(
            f"- `{item['card_id']}` {item['name_th']} "
            f"unmapped=`{','.join(item['unmapped_feature_tags'])}`"
        )
    lines.extend(["", "## Candidate Edge Sample", ""])
    for item in report["candidate_edge_review_items"][:25]:
        lines.append(
            f"- `{item['edge_rank']}` `{item['edge']}` score=`{item['net_score']}` "
            f"{item['source_name_th']} -> {item['target_name_th']}"
        )
    if len(report["candidate_edge_review_items"]) > 25:
        lines.append(f"- ... plus `{len(report['candidate_edge_review_items']) - 25}` more candidate edges in JSON/CSV.")
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Offline review packet only.",
            "- No deck recipe draft is created in M40-01.",
            "- No saved-deck injection or UI deck-list publication.",
            "- No runtime deck promotion.",
            "- No bot/playbook promotion.",
            "- No live card text parsing.",
            "- No direct `GameState` mutation.",
            "",
            "## Next",
            "",
            "`M40-02`: Second-slice recipe draft model.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M40-01 second-slice review packet.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_second_slice_review_packet()
    json_path = args.output_dir / "m40_01_second_slice_review_packet.json"
    md_path = args.output_dir / "m40_01_second_slice_review_packet.md"
    csv_path = args.output_dir / "m40_01_second_slice_review_packet.csv"
    write_json(report, json_path)
    write_markdown(report, md_path)
    write_csv(report, csv_path)
    print(f"M40-01 review packet wrote {json_path}")
    print(f"M40-01 review packet summary wrote {md_path}")
    print(f"M40-01 review packet CSV wrote {csv_path}")
    print(
        "ready_for_m40_02={ready} review_items={items} candidate_edges={edges}".format(
            ready=report["summary"]["ready_for_m40_02"],
            items=report["summary"]["total_review_item_count"],
            edges=report["summary"]["candidate_edge_item_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m40_02"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
