"""Tests for tools/deck/build_first_slice_reviewed_playbook_seed.py (M35-D4)."""

from __future__ import annotations

import json
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_first_slice_candidate_packages import build_report as build_candidate_packages  # noqa: E402
from tools.deck.build_first_slice_combo_line_explainer import build_report as build_combo_lines  # noqa: E402
from tools.deck.build_first_slice_deck_skeleton_ratio_planner import build_report as build_skeleton_report  # noqa: E402
from tools.deck.build_first_slice_manual_review_queue import build_queue  # noqa: E402
from tools.deck.build_first_slice_pair_compatibility_graph import build_report as build_pair_graph  # noqa: E402
from tools.deck.build_first_slice_requirement_provider_model import build_report as build_provider_report  # noqa: E402
from tools.deck.build_first_slice_resource_conflict_detector import build_report as build_resource_report  # noqa: E402
from tools.deck.build_first_slice_reviewed_playbook_seed import NO_GAP_NOTE, build_report  # noqa: E402
from tools.deck.build_first_slice_selected_compatibility_output import build_report as build_compatibility_report  # noqa: E402
from tools.deck.build_first_slice_timing_compatibility_detector import build_report as build_timing_report  # noqa: E402
from tools.deck.build_first_slice_zone_target_detector import build_report as build_zone_report  # noqa: E402
from tools.deck.extract_first_slice_semantics import build_report as build_semantic_report  # noqa: E402


class TestFirstSliceReviewedPlaybookSeed(unittest.TestCase):
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
        cls.combo_line_report = build_combo_lines(cls.package_report, cls.skeleton_report, cls.compatibility_report)
        cls.report = build_report(cls.combo_line_report)

    def test_reviewed_seed_report_has_entries_or_rejections(self):
        self.assertEqual("M35-D4", self.report["version"])
        self.assertGreater(self.report["summary"]["source_combo_line_count"], 0)
        self.assertEqual(
            self.report["summary"]["source_combo_line_count"],
            self.report["summary"]["seed_entry_count"] + self.report["summary"]["rejected_line_count"],
        )

    def test_scope_is_advisory_not_runtime(self):
        scope = self.report["scope"]
        self.assertTrue(scope["advisory_only"])
        self.assertTrue(scope["reviewed_playbook_seed_export"])
        self.assertFalse(scope["runtime_playbook"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["deck_quantities"])
        self.assertFalse(scope["runtime_effect_execution"])

    def test_policy_requires_human_acceptance_before_runtime(self):
        policy = self.report["policy"]
        self.assertTrue(policy["ai_static_review_only"])
        self.assertTrue(policy["human_acceptance_required_before_runtime"])
        self.assertTrue(policy["no_auto_inject_into_player_decks"])
        self.assertTrue(policy["no_auto_publish_to_bot"])

    def test_seed_entries_have_runtime_lockouts(self):
        for seed in self.report["playbook_seed_entries"]:
            self.assertTrue(seed["review"]["passed"])
            self.assertTrue(seed["review"]["requires_human_acceptance_before_runtime"])
            self.assertTrue(seed["runtime_policy"]["not_runtime_playbook"])
            self.assertTrue(seed["runtime_policy"]["not_published_to_bot"])
            self.assertTrue(seed["runtime_policy"]["not_auto_injected_into_decks"])

    def test_seed_entries_come_from_no_gap_lines_only(self):
        combo_lines_by_id = {line["line_id"]: line for line in self.combo_line_report["combo_lines"]}
        for seed in self.report["playbook_seed_entries"]:
            line = combo_lines_by_id[seed["source_line_id"]]
            self.assertEqual([NO_GAP_NOTE], line["needs_to_work"])

    def test_rejections_include_review_reasons(self):
        for rejection in self.report["review_rejections"]:
            self.assertTrue(rejection["review_reasons"])
            self.assertEqual("not_exported_to_seed", rejection["review_status"])

    def test_ready_for_second_slice_selection(self):
        self.assertTrue(self.report["summary"]["ready_for_m35_e1"])
        self.assertEqual("M35-E1", self.report["next_target"])

    def test_output_file_can_round_trip_after_cli_generation(self):
        path = ROOT / "outputs" / "target_slice" / "m35_d4_first_slice_reviewed_playbook_seed.json"
        if path.exists():
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual("M35-D4", data["version"])


if __name__ == "__main__":
    unittest.main()
