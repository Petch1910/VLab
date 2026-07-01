"""Tests for tools/deck/build_first_slice_zone_target_detector.py (M35-C4)."""

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
from tools.deck.build_first_slice_timing_compatibility_detector import build_report as build_timing_report  # noqa: E402
from tools.deck.build_first_slice_zone_target_detector import build_report  # noqa: E402
from tools.deck.extract_first_slice_semantics import build_report as build_semantic_report  # noqa: E402


class TestFirstSliceZoneTargetDetector(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.semantic_report = build_semantic_report()
        cls.provider_report = build_provider_report(cls.semantic_report)
        cls.manual_review_report = build_queue(cls.semantic_report, cls.provider_report)
        cls.pair_graph = build_pair_graph(cls.provider_report, cls.manual_review_report)
        cls.resource_report = build_resource_report(cls.pair_graph)
        cls.timing_report = build_timing_report(cls.pair_graph, cls.resource_report)
        cls.report = build_report(cls.pair_graph, cls.resource_report, cls.timing_report)

    def test_zone_detector_has_profiles_and_findings(self):
        self.assertEqual("M35-C4", self.report["version"])
        self.assertGreater(self.report["summary"]["node_count"], 0)
        self.assertGreater(self.report["summary"]["zone_relevant_edge_count"], 0)
        self.assertEqual(
            self.report["summary"]["zone_relevant_edge_count"],
            len(self.report["edge_zone_findings"]),
        )

    def test_scope_does_not_claim_deck_or_bot_output(self):
        scope = self.report["scope"]
        self.assertTrue(scope["advisory_only"])
        self.assertFalse(scope["resource_conflict_detector"])
        self.assertFalse(scope["timing_compatibility_detector"])
        self.assertTrue(scope["zone_target_compatibility_detector"])
        self.assertFalse(scope["deck_skeleton"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["runtime_effect_execution"])

    def test_policy_keeps_zone_findings_advisory(self):
        policy = self.report["policy"]
        self.assertTrue(policy["vanguard_circle_conflict_is_advisory_until_archetype_review"])
        self.assertTrue(policy["rear_guard_slot_pressure_is_not_exact_board_capacity"])
        self.assertTrue(policy["manual_review_blocks_high_confidence"])

    def test_vanguard_and_rear_guard_zones_are_indexed(self):
        indexes = self.report["zone_indexes"]
        self.assertIn("vanguard_circle", indexes["requirement_index"])
        self.assertIn("rear_guard_circle", indexes["requirement_index"])

    def test_common_zone_verdicts_exist(self):
        verdicts = self.report["summary"]["verdict_counts"]
        self.assertIn("vanguard_role_conflict", verdicts)
        self.assertIn("rear_guard_slot_pressure", verdicts)

    def test_resource_and_timing_verdicts_are_carried_when_available(self):
        carried = [
            finding
            for finding in self.report["edge_zone_findings"]
            if finding["resource_verdict"] != "not_resource_relevant"
            and finding["timing_verdict"] != "not_timing_relevant"
        ]
        self.assertTrue(carried)

    def test_manual_review_findings_remain_review_required(self):
        manual = [finding for finding in self.report["edge_zone_findings"] if finding["manual_review_required"]]
        self.assertTrue(manual)
        for finding in manual:
            self.assertEqual("review_required", finding["confidence"])

    def test_ready_for_selected_slice_output(self):
        self.assertTrue(self.report["summary"]["ready_for_m35_c5"])
        self.assertEqual("M35-C5", self.report["next_target"])

    def test_output_file_can_round_trip_after_cli_generation(self):
        path = ROOT / "outputs" / "target_slice" / "m35_c4_first_slice_zone_target_detector.json"
        if path.exists():
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual("M35-C4", data["version"])


if __name__ == "__main__":
    unittest.main()
