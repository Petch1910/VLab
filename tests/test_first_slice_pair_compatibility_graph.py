"""Tests for tools/deck/build_first_slice_pair_compatibility_graph.py (M35-C1)."""

from __future__ import annotations

import json
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_first_slice_manual_review_queue import build_queue  # noqa: E402
from tools.deck.build_first_slice_pair_compatibility_graph import build_report  # noqa: E402
from tools.deck.build_first_slice_requirement_provider_model import build_report as build_provider_report  # noqa: E402
from tools.deck.extract_first_slice_semantics import build_report as build_semantic_report  # noqa: E402


class TestFirstSlicePairCompatibilityGraph(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.semantic_report = build_semantic_report()
        cls.provider_report = build_provider_report(cls.semantic_report)
        cls.manual_review_report = build_queue(cls.semantic_report, cls.provider_report)
        cls.report = build_report(cls.provider_report, cls.manual_review_report)

    def test_graph_has_nodes_and_edges(self):
        self.assertEqual("M35-C1", self.report["version"])
        self.assertGreater(self.report["summary"]["node_count"], 0)
        self.assertGreater(self.report["summary"]["edge_count"], 0)
        self.assertEqual(self.report["summary"]["node_count"], len(self.report["nodes"]))
        self.assertEqual(self.report["summary"]["edge_count"], len(self.report["edges"]))

    def test_graph_scope_does_not_promote_to_deck_or_bot(self):
        scope = self.report["scope"]
        self.assertTrue(scope["advisory_only"])
        self.assertTrue(scope["pair_compatibility_graph"])
        self.assertFalse(scope["resource_conflict_detector"])
        self.assertFalse(scope["timing_compatibility_detector"])
        self.assertFalse(scope["zone_target_compatibility_detector"])
        self.assertFalse(scope["deck_skeleton"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["runtime_effect_execution"])

    def test_edges_are_directed_and_do_not_include_self_pairs(self):
        for edge in self.report["edges"]:
            self.assertEqual("provider_to_consumer", edge["direction"])
            self.assertNotEqual(edge["source_card_id"], edge["target_card_id"])

    def test_counter_charge_provider_supports_counter_blast_consumer(self):
        edges = self.report["edges"]
        matching_edges = [
            edge
            for edge in edges
            if "resource_support" in edge["categories"]
            and "provides_counter_charge" in edge["source_providers"]
            and "requires_counter_blast" in edge["target_requirements"]
        ]
        self.assertTrue(matching_edges)

    def test_manual_review_edges_are_not_high_confidence(self):
        manual_review_edges = [edge for edge in self.report["edges"] if edge["manual_review_required"]]
        self.assertTrue(manual_review_edges)
        for edge in manual_review_edges:
            self.assertEqual("review_required", edge["confidence"])
            self.assertEqual(
                "manual_review_required_before_high_confidence_compatibility",
                edge["allowed_next_use"],
            )

    def test_indexes_match_edges(self):
        by_category = self.report["indexes"]["by_category"]
        category_counts = self.report["indexes"]["category_counts"]
        self.assertIn("resource_support", by_category)
        self.assertEqual(len(by_category["resource_support"]), category_counts["resource_support"])

    def test_ready_for_next_resource_conflict_detector(self):
        self.assertTrue(self.report["summary"]["ready_for_m35_c2"])
        self.assertEqual("M35-C2", self.report["next_target"])

    def test_output_file_can_round_trip_after_cli_generation(self):
        path = ROOT / "outputs" / "target_slice" / "m35_c1_first_slice_pair_compatibility_graph.json"
        if path.exists():
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual("M35-C1", data["version"])


if __name__ == "__main__":
    unittest.main()
