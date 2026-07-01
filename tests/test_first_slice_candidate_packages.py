"""Tests for tools/deck/build_first_slice_candidate_packages.py (M35-D1)."""

from __future__ import annotations

import json
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_first_slice_candidate_packages import build_report  # noqa: E402
from tools.deck.build_first_slice_manual_review_queue import build_queue  # noqa: E402
from tools.deck.build_first_slice_pair_compatibility_graph import build_report as build_pair_graph  # noqa: E402
from tools.deck.build_first_slice_requirement_provider_model import build_report as build_provider_report  # noqa: E402
from tools.deck.build_first_slice_resource_conflict_detector import build_report as build_resource_report  # noqa: E402
from tools.deck.build_first_slice_selected_compatibility_output import build_report as build_compatibility_report  # noqa: E402
from tools.deck.build_first_slice_timing_compatibility_detector import build_report as build_timing_report  # noqa: E402
from tools.deck.build_first_slice_zone_target_detector import build_report as build_zone_report  # noqa: E402
from tools.deck.extract_first_slice_semantics import build_report as build_semantic_report  # noqa: E402


class TestFirstSliceCandidatePackages(unittest.TestCase):
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
        cls.report = build_report(cls.pair_graph, cls.compatibility_report)

    def test_candidate_package_report_has_packages(self):
        self.assertEqual("M35-D1", self.report["version"])
        self.assertGreater(self.report["summary"]["clean_edge_count"], 0)
        self.assertGreater(self.report["summary"]["package_count"], 0)
        self.assertEqual(self.report["summary"]["package_count"], len(self.report["packages"]))

    def test_scope_has_no_deck_skeleton_or_quantities(self):
        scope = self.report["scope"]
        self.assertTrue(scope["advisory_only"])
        self.assertTrue(scope["candidate_package_selection"])
        self.assertFalse(scope["deck_skeleton"])
        self.assertFalse(scope["deck_quantities"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["runtime_effect_execution"])

    def test_policy_uses_clean_edges_only(self):
        policy = self.report["policy"]
        self.assertTrue(policy["only_clean_synergy_edges"])
        self.assertTrue(policy["no_manual_review_edges"])
        self.assertTrue(policy["no_missing_data_edges"])
        self.assertTrue(policy["no_conflict_edges"])

    def test_packages_have_cards_edges_and_no_card_counts(self):
        for package in self.report["packages"]:
            self.assertGreaterEqual(len(package["card_ids"]), 2)
            self.assertGreater(package["edge_count"], 0)
            self.assertTrue(package["candidate_for_m35_d2"])
            self.assertNotIn("deck_counts", package)
            self.assertNotIn("quantities", package)

    def test_package_edges_are_from_clean_compatibility_edges(self):
        clean_edge_ids = {
            edge["edge"]
            for edge in self.compatibility_report["compatibility_edges"]
            if edge["candidate_for_m35_d1"]
        }
        for package in self.report["packages"]:
            self.assertTrue(set(package["edge_keys"]).issubset(clean_edge_ids))

    def test_ready_for_deck_skeleton_ratio_planner(self):
        self.assertTrue(self.report["summary"]["ready_for_m35_d2"])
        self.assertEqual("M35-D2", self.report["next_target"])

    def test_output_file_can_round_trip_after_cli_generation(self):
        path = ROOT / "outputs" / "target_slice" / "m35_d1_first_slice_candidate_packages.json"
        if path.exists():
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual("M35-D1", data["version"])


if __name__ == "__main__":
    unittest.main()
