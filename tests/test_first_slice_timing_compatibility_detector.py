"""Tests for tools/deck/build_first_slice_timing_compatibility_detector.py (M35-C3)."""

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
from tools.deck.build_first_slice_timing_compatibility_detector import build_report  # noqa: E402
from tools.deck.extract_first_slice_semantics import build_report as build_semantic_report  # noqa: E402


class TestFirstSliceTimingCompatibilityDetector(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.semantic_report = build_semantic_report()
        cls.provider_report = build_provider_report(cls.semantic_report)
        cls.manual_review_report = build_queue(cls.semantic_report, cls.provider_report)
        cls.pair_graph = build_pair_graph(cls.provider_report, cls.manual_review_report)
        cls.resource_report = build_resource_report(cls.pair_graph)
        cls.report = build_report(cls.pair_graph, cls.resource_report)

    def test_timing_detector_has_profiles_and_findings(self):
        self.assertEqual("M35-C3", self.report["version"])
        self.assertGreater(self.report["summary"]["node_count"], 0)
        self.assertGreater(self.report["summary"]["timing_relevant_edge_count"], 0)
        self.assertEqual(
            self.report["summary"]["timing_relevant_edge_count"],
            len(self.report["edge_timing_findings"]),
        )

    def test_scope_declares_card_level_timing_proxy(self):
        scope = self.report["scope"]
        self.assertTrue(scope["advisory_only"])
        self.assertTrue(scope["uses_card_level_timing_proxy"])
        self.assertFalse(scope["resource_conflict_detector"])
        self.assertTrue(scope["timing_compatibility_detector"])
        self.assertFalse(scope["zone_target_compatibility_detector"])
        self.assertFalse(scope["deck_skeleton"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["runtime_effect_execution"])

    def test_policy_does_not_claim_final_timing_without_structured_effect_timing(self):
        policy = self.report["policy"]
        self.assertTrue(policy["no_final_timing_verdict_without_structured_effect_timing"])
        self.assertTrue(policy["manual_review_blocks_high_confidence"])

    def test_timing_windows_are_ordered(self):
        windows = self.report["timing_windows"]
        self.assertLess(windows["requires_on_ride_timing"]["rank"], windows["requires_on_attack_timing"]["rank"])
        self.assertLess(windows["requires_on_attack_timing"]["rank"], windows["requires_hit_timing"]["rank"])

    def test_common_timing_verdicts_exist(self):
        verdicts = self.report["summary"]["verdict_counts"]
        self.assertIn("timing_can_precede", verdicts)
        self.assertIn("same_window_requires_ordering", verdicts)

    def test_resource_verdict_is_carried_when_available(self):
        carried = [
            finding
            for finding in self.report["edge_timing_findings"]
            if finding["resource_verdict"] != "not_resource_relevant"
        ]
        self.assertTrue(carried)

    def test_manual_review_findings_remain_review_required(self):
        manual = [finding for finding in self.report["edge_timing_findings"] if finding["manual_review_required"]]
        self.assertTrue(manual)
        for finding in manual:
            self.assertEqual("review_required", finding["confidence"])

    def test_ready_for_next_zone_target_detector(self):
        self.assertTrue(self.report["summary"]["ready_for_m35_c4"])
        self.assertEqual("M35-C4", self.report["next_target"])

    def test_output_file_can_round_trip_after_cli_generation(self):
        path = ROOT / "outputs" / "target_slice" / "m35_c3_first_slice_timing_compatibility_detector.json"
        if path.exists():
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual("M35-C3", data["version"])


if __name__ == "__main__":
    unittest.main()
