"""Build the M52-02 fifth-slice review packet."""

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

from tools.deck.build_fifth_slice_semantic_compatibility_probe import (  # noqa: E402
    _normalize_readiness,
    _normalize_selection,
)
from tools.deck.build_selected_slice_semantic_compatibility_probe import (  # noqa: E402
    build_compatibility_output,
    build_pair_graph,
    build_queue,
    build_requirement_provider,
    build_resource_detector,
    build_semantic_tags,
    build_timing_detector,
    build_vocabulary,
    build_zone_detector,
)


OUTPUT_DIR = ROOT / "outputs" / "target_slice"
M51_01_SELECTION = OUTPUT_DIR / "m51_01_fifth_target_slice_selection.json"
M51_02_READINESS = OUTPUT_DIR / "m51_02_fifth_slice_fixture_readiness.json"
M51_03_PROBE = OUTPUT_DIR / "m51_03_fifth_slice_semantic_compatibility_probe.json"
M51_04_GATE = OUTPUT_DIR / "m51_04_fifth_slice_recipe_pipeline_entry_gate.json"
M52_01_SCAFFOLD = OUTPUT_DIR / "m52_01_fifth_slice_fixture_scaffold.json"


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


def _build_pipeline_details(selection_report: dict[str, Any], readiness_report: dict[str, Any]) -> dict[str, Any]:
    normalized_selection = _normalize_selection(selection_report, readiness_report)
    normalized_readiness = _normalize_readiness(readiness_report)
    vocabulary = build_vocabulary(normalized_readiness)
    semantic_report = build_semantic_tags(normalized_selection, vocabulary)
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
    return {
        "manual_review_report": manual_review_report,
        "compatibility_report": compatibility_report,
    }


def _scaffold_note_item(scaffold_report: dict[str, Any]) -> dict[str, Any]:
    scaffold = scaffold_report.get("fixture_scaffold", {})
    mechanic = scaffold_report.get("mechanic_scope", {})
    return {
        "item_type": "fixture_scaffold_note",
        "item_id": "m52_01_fixture_scaffold",
        "review_priority": "P0_fixture_policy_review",
        "policy_level": scaffold.get("policy_level", ""),
        "main_deck_exact": scaffold.get("main_deck_exact"),
        "trigger_target": scaffold.get("trigger_target"),
        "recommended_trigger_profile": scaffold.get("recommended_trigger_profile", {}),
        "preferred_grade_profile": scaffold.get("preferred_grade_profile", {}),
        "mechanic_manual_review_required": bool(
            mechanic.get("legion_text_requires_manual_review")
            or mechanic.get("lock_or_unlock_text_requires_manual_review")
        ),
        "legion_text_manual_review_required": bool(mechanic.get("legion_text_requires_manual_review")),
        "lock_or_unlock_text_manual_review_required": bool(
            mechanic.get("lock_or_unlock_text_requires_manual_review")
        ),
        "review_reasons": [
            "new_format_or_mechanic_fixture_scaffold",
            "link_joker_legion_mate_boundary_review",
            "must_review_before_recipe_validator",
        ],
        "blocked_until": [
            "fixture_scaffold_reviewed",
            "m52_04_recipe_validator_uses_scaffold",
            "legion_lock_manual_review_policy_confirmed",
        ],
        "recommended_next_action": "confirm_fixture_scaffold_before_recipe_draft",
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
            "fixture_scaffold_considered",
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
            "fifth_slice_candidate_compatibility_edge",
            "requires_human_recipe_selection_before_m52_03",
        ],
        "blocked_until": [
            "human_recipe_selection",
            "m52_04_recipe_validator",
            "combo_line_to_recipe_consistency_check",
        ],
        "recommended_next_action": "consider_for_m52_03_advisory_recipe_draft",
    }


def _csv_rows(report: dict[str, Any]) -> Iterable[dict[str, str]]:
    for section in ("fixture_scaffold_items", "manual_card_review_items", "candidate_edge_review_items"):
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


def build_fifth_slice_review_packet(
    selection_report: dict[str, Any] | None = None,
    readiness_report: dict[str, Any] | None = None,
    probe_report: dict[str, Any] | None = None,
    gate_report: dict[str, Any] | None = None,
    scaffold_report: dict[str, Any] | None = None,
) -> dict[str, Any]:
    selection_report = selection_report or load_json(M51_01_SELECTION)
    readiness_report = readiness_report or load_json(M51_02_READINESS)
    probe_report = probe_report or load_json(M51_03_PROBE)
    gate_report = gate_report or load_json(M51_04_GATE)
    scaffold_report = scaffold_report or load_json(M52_01_SCAFFOLD)
    pipeline = _build_pipeline_details(selection_report, readiness_report)
    manual_report = pipeline["manual_review_report"]
    compatibility_report = pipeline["compatibility_report"]

    scaffold_items = [_scaffold_note_item(scaffold_report)]
    manual_cards = [_manual_card_item(card) for card in manual_report.get("manual_review_queue", [])]
    candidate_edges = [
        _candidate_edge_item(edge, rank)
        for rank, edge in enumerate(
            [item for item in compatibility_report.get("compatibility_edges", []) if item.get("candidate_for_m35_d1")],
            start=1,
        )
    ]
    expected_manual_count = probe_report.get("summary", {}).get("manual_review_card_count")
    expected_candidate_count = probe_report.get("summary", {}).get("candidate_edge_count")
    gate_allows_offline = bool(gate_report.get("summary", {}).get("ready_for_m52"))
    scaffold_ready = bool(scaffold_report.get("summary", {}).get("ready_for_m52_02"))
    promotion_closed = not any(
        bool(gate_report.get("decision", {}).get(key))
        for key in (
            "runtime_deck_promotion_allowed",
            "saved_deck_or_ui_publication_allowed",
            "bot_playbook_promotion_allowed",
        )
    )
    ready = (
        gate_allows_offline
        and scaffold_ready
        and promotion_closed
        and len(manual_cards) == expected_manual_count
        and len(candidate_edges) == expected_candidate_count
        and len(scaffold_items) == 1
    )

    return {
        "version": "M52-02",
        "description": "Fifth-slice review packet before advisory deck recipe drafting",
        "selected_target": selection_report.get("selected_target", {}),
        "source_inputs": {
            "fifth_target_slice_selection": _rel(M51_01_SELECTION),
            "fifth_slice_fixture_readiness": _rel(M51_02_READINESS),
            "fifth_slice_semantic_compatibility_probe": _rel(M51_03_PROBE),
            "fifth_slice_recipe_pipeline_entry_gate": _rel(M51_04_GATE),
            "fifth_slice_fixture_scaffold": _rel(M52_01_SCAFFOLD),
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
            "fixture_scaffold_is_policy_evidence_not_saved_deck": True,
            "legion_lock_text_requires_manual_review": True,
            "no_live_card_text_parsing": True,
            "no_direct_GameState_mutation": True,
        },
        "evidence_summary": {
            "m51_04_offline_recipe_pipeline_allowed": gate_allows_offline,
            "m51_04_runtime_promotion_closed": promotion_closed,
            "m52_01_scaffold_ready": scaffold_ready,
            "probe_card_count": probe_report.get("summary", {}).get("semantic_card_count"),
            "probe_edge_count": probe_report.get("summary", {}).get("pair_graph_edge_count"),
            "expected_manual_review_count": expected_manual_count,
            "expected_candidate_edge_count": expected_candidate_count,
            "rebuilt_manual_review_count": len(manual_cards),
            "rebuilt_candidate_edge_count": len(candidate_edges),
        },
        "summary": {
            "fixture_scaffold_item_count": len(scaffold_items),
            "manual_card_item_count": len(manual_cards),
            "candidate_edge_item_count": len(candidate_edges),
            "total_review_item_count": len(scaffold_items) + len(manual_cards) + len(candidate_edges),
            "ready_for_m52_03": ready,
        },
        "fixture_scaffold_items": scaffold_items,
        "manual_card_review_items": manual_cards,
        "candidate_edge_review_items": candidate_edges,
        "next_target": {
            "milestone": "M52-03",
            "task": "Fifth-slice recipe draft model",
            "blocked_until": [
                "M52-02 review packet is available",
                "human/team can inspect fixture scaffold, candidate edges, and manual-review cards",
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
    lines = [
        "# M52-02 Fifth-Slice Review Packet",
        "",
        "## Summary",
        "",
        f"- Fixture scaffold items: `{summary['fixture_scaffold_item_count']}`",
        f"- Manual-review card items: `{summary['manual_card_item_count']}`",
        f"- Candidate edge items: `{summary['candidate_edge_item_count']}`",
        f"- Total review items: `{summary['total_review_item_count']}`",
        f"- Ready for M52-03: `{summary['ready_for_m52_03']}`",
        "",
        "## Fixture Scaffold",
        "",
    ]
    for item in report["fixture_scaffold_items"]:
        lines.append(
            f"- `{item['item_id']}` policy=`{item['policy_level']}` action=`{item['recommended_next_action']}`"
        )
    lines.extend(["", "## Manual-Review Cards", ""])
    for item in report["manual_card_review_items"][:25]:
        lines.append(
            f"- `{item['card_id']}` {item['name_th']} unmapped=`{','.join(item['unmapped_feature_tags'])}`"
        )
    if len(report["manual_card_review_items"]) > 25:
        lines.append(f"- ... {len(report['manual_card_review_items']) - 25} more manual-review cards")
    lines.extend(["", "## Candidate Edges", ""])
    for item in report["candidate_edge_review_items"][:25]:
        lines.append(
            f"- rank `{item['edge_rank']}` `{item['edge']}` score=`{item['net_score']}` action=`{item['recommended_next_action']}`"
        )
    if len(report["candidate_edge_review_items"]) > 25:
        lines.append(f"- ... {len(report['candidate_edge_review_items']) - 25} more candidate edges")
    lines.extend(
        [
            "",
            "## Policy",
            "",
            "- Offline review packet only.",
            "- No deck recipe draft.",
            "- No saved deck or UI publication.",
            "- No runtime deck promotion.",
            "- No bot playbook promotion.",
            "- No direct `GameState` mutation.",
            "",
            "## Next",
            "",
            "`M52-03`: Fifth-slice recipe draft model.",
            "",
        ]
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines), encoding="utf-8")


def parse_args(argv: Sequence[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build M52-02 fifth-slice review packet.")
    parser.add_argument("--output-dir", type=Path, default=OUTPUT_DIR)
    return parser.parse_args(argv)


def main(argv: Sequence[str] | None = None) -> int:
    args = parse_args(argv)
    report = build_fifth_slice_review_packet()
    json_path = args.output_dir / "m52_02_fifth_slice_review_packet.json"
    md_path = args.output_dir / "m52_02_fifth_slice_review_packet.md"
    csv_path = args.output_dir / "m52_02_fifth_slice_review_packet.csv"
    write_json(report, json_path)
    write_markdown(report, md_path)
    write_csv(report, csv_path)
    print(f"M52-02 review packet wrote {json_path}")
    print(f"M52-02 review packet summary wrote {md_path}")
    print(f"M52-02 review packet CSV wrote {csv_path}")
    print(
        "ready={ready} scaffold={scaffold} manual={manual} candidates={candidates}".format(
            ready=report["summary"]["ready_for_m52_03"],
            scaffold=report["summary"]["fixture_scaffold_item_count"],
            manual=report["summary"]["manual_card_item_count"],
            candidates=report["summary"]["candidate_edge_item_count"],
        )
    )
    return 0 if report["summary"]["ready_for_m52_03"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
