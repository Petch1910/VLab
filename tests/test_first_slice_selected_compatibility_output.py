"""Tests for tools/deck/build_first_slice_selected_compatibility_output.py (M35-C5)."""

from __future__ import annotations

import json
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_first_slice_manual_review_queue import build_queue  # noqa: E402
from tools.deck.build_first_slice_pair_compatibility_graph import build_report as build_pair_graph  # noqa: E402
from tools.deck.build_first_slice_requirement_provider_model import build_report as build_provider_report  # noqa: E402
from tools.deck.build_first_slice_resource_conflict_detector import build_report as build_resource_report  # noqa: E402
from tools.deck.build_first_slice_selected_compatibility_output import build_report  # noqa: E402
from tools.deck.build_first_slice_timing_compatibility_detector import build_report as build_timing_report  # noqa: E402
from tools.deck.build_first_slice_zone_target_detector import build_report as build_zone_report  # noqa: E402
from tools.deck.extract_first_slice_semantics import build_report as build_semantic_report  # noqa: E402


class TestFirstSliceSelectedCompatibilityOutput(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.semantic_report = build_semantic_report()
        cls.provider_report = build_provider_report(cls.semantic_report)
        cls.manual_review_report = build_queue(cls.semantic_report, cls.provider_report)
        cls.pair_graph = build_pair_graph(cls.provider_report, cls.manual_review_report)
        cls.resource_report = build_resource_report(cls.pair_graph)
        cls.timing_report = build_timing_report(cls.pair_graph, cls.resource_report)
        cls.zone_report = build_zone_report(cls.pair_graph, cls.resource_report, cls.timing_report)
        cls.report = build_report(cls.pair_graph, cls.resource_report, cls.timing_report, cls.zone_report)

    def test_selected_compatibility_output_has_all_edges(self):
        self.assertEqual("M35-C5", self.report["version"])
        self.assertEqual(len(self.pair_graph["edges"]), self.report["summary"]["edge_count"])
        self.assertEqual(self.report["summary"]["edge_count"], len(self.report["compatibility_edges"]))

    def test_scope_does_not_choose_deck_or_bot_data(self):
        scope = self.report["scope"]
        self.assertTrue(scope["advisory_only"])
        self.assertTrue(scope["selected_slice_compatibility_output"])
        self.assertFalse(scope["deck_skeleton"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["runtime_effect_execution"])

    def test_policy_keeps_d1_candidates_as_inputs_only(self):
        policy = self.report["policy"]
        self.assertTrue(policy["does_not_choose_deck_slots"])
        self.assertTrue(policy["does_not_promote_manual_review_edges"])
        self.assertTrue(policy["d1_candidates_are_inputs_not_final_deck_choices"])

    def test_labels_include_core_review_categories(self):
        labels = self.report["summary"]["label_counts"]
        self.assertIn("synergy", labels)
        self.assertIn("conflict", labels)
        self.assertIn("missing_data", labels)
        self.assertIn("manual_review_required", labels)

    def test_candidate_edges_are_clean_synergy_edges(self):
        for edge in self.report["m35_d1_candidate_edges"]:
            self.assertEqual("synergy", edge["status"])
            self.assertIn("synergy", edge["labels"])
            self.assertNotIn("conflict", edge["labels"])
            self.assertNotIn("missing_data", edge["labels"])
            self.assertNotIn("manual_review_required", edge["labels"])
            self.assertTrue(edge["candidate_for_m35_d1"])

    def test_manual_review_status_has_manual_label(self):
        manual = [edge for edge in self.report["compatibility_edges"] if edge["status"] == "manual_review_required"]
        self.assertTrue(manual)
        for edge in manual[:20]:
            self.assertIn("manual_review_required", edge["labels"])

    def test_ready_for_d1(self):
        self.assertTrue(self.report["summary"]["ready_for_m35_d1"])
        self.assertEqual("M35-D1", self.report["next_target"])

    def test_output_file_can_round_trip_after_cli_generation(self):
        path = ROOT / "outputs" / "target_slice" / "m35_c5_first_slice_selected_compatibility_output.json"
        if path.exists():
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual("M35-C5", data["version"])


if __name__ == "__main__":
    unittest.main()
