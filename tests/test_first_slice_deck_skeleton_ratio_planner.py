"""Tests for tools/deck/build_first_slice_deck_skeleton_ratio_planner.py (M35-D2)."""

from __future__ import annotations

import json
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_first_slice_candidate_packages import build_report as build_candidate_packages  # noqa: E402
from tools.deck.build_first_slice_deck_skeleton_ratio_planner import build_report  # noqa: E402
from tools.deck.build_first_slice_manual_review_queue import build_queue  # noqa: E402
from tools.deck.build_first_slice_pair_compatibility_graph import build_report as build_pair_graph  # noqa: E402
from tools.deck.build_first_slice_requirement_provider_model import build_report as build_provider_report  # noqa: E402
from tools.deck.build_first_slice_resource_conflict_detector import build_report as build_resource_report  # noqa: E402
from tools.deck.build_first_slice_selected_compatibility_output import build_report as build_compatibility_report  # noqa: E402
from tools.deck.build_first_slice_timing_compatibility_detector import build_report as build_timing_report  # noqa: E402
from tools.deck.build_first_slice_zone_target_detector import build_report as build_zone_report  # noqa: E402
from tools.deck.extract_first_slice_semantics import build_report as build_semantic_report  # noqa: E402


class TestFirstSliceDeckSkeletonRatioPlanner(unittest.TestCase):
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
        cls.report = build_report(cls.package_report, cls.pair_graph)

    def test_ratio_planner_has_skeletons(self):
        self.assertEqual("M35-D2", self.report["version"])
        self.assertGreater(self.report["summary"]["source_package_count"], 0)
        self.assertGreater(self.report["summary"]["skeleton_count"], 0)
        self.assertEqual(self.report["summary"]["skeleton_count"], len(self.report["skeletons"]))

    def test_scope_has_no_final_deck_or_card_quantities(self):
        scope = self.report["scope"]
        self.assertTrue(scope["advisory_only"])
        self.assertTrue(scope["deck_skeleton_ratio_planner"])
        self.assertFalse(scope["final_deck_list"])
        self.assertFalse(scope["per_card_quantities"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["runtime_effect_execution"])

    def test_ratio_targets_sum_to_main_deck(self):
        for skeleton in self.report["skeletons"]:
            ratio = skeleton["ratio_targets"]
            grade_total = sum(item["target"] for item in ratio["grade_targets"].values())
            self.assertEqual(ratio["main_deck_size"], grade_total)
            self.assertEqual(50, ratio["main_deck_size"])
            self.assertEqual(16, ratio["trigger_slots"])

    def test_skeletons_include_guard_shield_profile_from_sqlite(self):
        for skeleton in self.report["skeletons"]:
            profile = skeleton["guard_shield_profile"]
            self.assertIn("cards_with_shield_value", profile)
            self.assertIn("average_shield", profile)
            self.assertTrue(profile["limited_by_package_not_full_deck"])

    def test_skeleton_cards_have_roles_but_no_deck_counts(self):
        for skeleton in self.report["skeletons"]:
            self.assertIn("key_cards", skeleton["roles"])
            self.assertIn("support_cards", skeleton["roles"])
            self.assertIn("trigger_cards", skeleton["roles"])
            self.assertNotIn("card_quantities", skeleton)
            self.assertNotIn("deck_list", skeleton)

    def test_ready_for_combo_line_explainer(self):
        self.assertTrue(self.report["summary"]["ready_for_m35_d3"])
        self.assertEqual("M35-D3", self.report["next_target"])

    def test_output_file_can_round_trip_after_cli_generation(self):
        path = ROOT / "outputs" / "target_slice" / "m35_d2_first_slice_deck_skeleton_ratio_plans.json"
        if path.exists():
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual("M35-D2", data["version"])


if __name__ == "__main__":
    unittest.main()
