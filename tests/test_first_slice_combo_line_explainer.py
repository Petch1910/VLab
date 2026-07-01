"""Tests for tools/deck/build_first_slice_combo_line_explainer.py (M35-D3)."""

from __future__ import annotations

import json
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_first_slice_candidate_packages import build_report as build_candidate_packages  # noqa: E402
from tools.deck.build_first_slice_combo_line_explainer import build_report  # noqa: E402
from tools.deck.build_first_slice_deck_skeleton_ratio_planner import build_report as build_skeleton_report  # noqa: E402
from tools.deck.build_first_slice_manual_review_queue import build_queue  # noqa: E402
from tools.deck.build_first_slice_pair_compatibility_graph import build_report as build_pair_graph  # noqa: E402
from tools.deck.build_first_slice_requirement_provider_model import build_report as build_provider_report  # noqa: E402
from tools.deck.build_first_slice_resource_conflict_detector import build_report as build_resource_report  # noqa: E402
from tools.deck.build_first_slice_selected_compatibility_output import build_report as build_compatibility_report  # noqa: E402
from tools.deck.build_first_slice_timing_compatibility_detector import build_report as build_timing_report  # noqa: E402
from tools.deck.build_first_slice_zone_target_detector import build_report as build_zone_report  # noqa: E402
from tools.deck.extract_first_slice_semantics import build_report as build_semantic_report  # noqa: E402


class TestFirstSliceComboLineExplainer(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.semantic_report = build_semantic_report()
        cls.provider_report = build_provider_report(cls.semantic_report)
        cls.manual_review_report = build_queue(cls.semantic_report, cls.provider_report)
        cls.pair_graph = build_pair_graph(cls.provider_report, cls.manual_review_report)
        cls.resource_report = build_resource_report(cls.pair_graph)
        cls.timing_report = build_timing_report(cls.pair_graph, cls.resource_report)
        cls.zone_report = build_zone_report(cls.pair_graph, cls.resource_report, cls.timing_report)
        cls.compatibility_report = build_compatibility_report(
            cls.pair_graph,
            cls.resource_report,
            cls.timing_report,
            cls.zone_report,
        )
        cls.package_report = build_candidate_packages(cls.pair_graph, cls.compatibility_report)
        cls.skeleton_report = build_skeleton_report(cls.package_report, cls.pair_graph)
        cls.report = build_report(cls.package_report, cls.skeleton_report, cls.compatibility_report)

    def test_combo_line_report_has_lines(self):
        self.assertEqual("M35-D3", self.report["version"])
        self.assertGreater(self.report["summary"]["source_skeleton_count"], 0)
        self.assertGreater(self.report["summary"]["combo_line_count"], 0)
        self.assertEqual(self.report["summary"]["combo_line_count"], len(self.report["combo_lines"]))

    def test_scope_is_explainer_not_playbook(self):
        scope = self.report["scope"]
        self.assertTrue(scope["advisory_only"])
        self.assertTrue(scope["combo_line_explainer"])
        self.assertFalse(scope["reviewed_playbook_seed"])
        self.assertFalse(scope["deck_quantities"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["runtime_effect_execution"])

    def test_policy_requires_review_before_playbook_seed(self):
        policy = self.report["policy"]
        self.assertTrue(policy["uses_clean_m35_c5_edges_only"])
        self.assertTrue(policy["does_not_claim_final_play_sequence_legality"])
        self.assertTrue(policy["d4_review_required_before_playbook_seed"])

    def test_lines_have_steps_and_needs(self):
        for line in self.report["combo_lines"]:
            self.assertTrue(line["steps"])
            self.assertTrue(line["why_package_is_included"])
            self.assertTrue(line["needs_to_work"])
            self.assertTrue(line["candidate_for_m35_d4_review"])

    def test_steps_reference_clean_compatibility_edges(self):
        clean_edges = {
            edge["edge"]
            for edge in self.compatibility_report["compatibility_edges"]
            if edge["candidate_for_m35_d1"]
        }
        for line in self.report["combo_lines"]:
            for step in line["steps"]:
                self.assertIn(step["edge"], clean_edges)

    def test_ready_for_reviewed_playbook_seed(self):
        self.assertTrue(self.report["summary"]["ready_for_m35_d4"])
        self.assertEqual("M35-D4", self.report["next_target"])

    def test_output_file_can_round_trip_after_cli_generation(self):
        path = ROOT / "outputs" / "target_slice" / "m35_d3_first_slice_combo_line_explainer.json"
        if path.exists():
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual("M35-D3", data["version"])


if __name__ == "__main__":
    unittest.main()
