"""Tests for tools/deck/build_first_slice_resource_conflict_detector.py (M35-C2)."""

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
from tools.deck.build_first_slice_resource_conflict_detector import build_report  # noqa: E402
from tools.deck.extract_first_slice_semantics import build_report as build_semantic_report  # noqa: E402


class TestFirstSliceResourceConflictDetector(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.semantic_report = build_semantic_report()
        cls.provider_report = build_provider_report(cls.semantic_report)
        cls.manual_review_report = build_queue(cls.semantic_report, cls.provider_report)
        cls.pair_graph = build_pair_graph(cls.provider_report, cls.manual_review_report)
        cls.report = build_report(cls.pair_graph)

    def test_resource_detector_has_profiles_and_findings(self):
        self.assertEqual("M35-C2", self.report["version"])
        self.assertGreater(self.report["summary"]["node_count"], 0)
        self.assertGreater(self.report["summary"]["resource_relevant_edge_count"], 0)
        self.assertEqual(
            self.report["summary"]["resource_relevant_edge_count"],
            len(self.report["edge_resource_findings"]),
        )

    def test_scope_does_not_claim_final_compatibility(self):
        scope = self.report["scope"]
        self.assertTrue(scope["advisory_only"])
        self.assertTrue(scope["resource_conflict_detector"])
        self.assertFalse(scope["timing_compatibility_detector"])
        self.assertFalse(scope["zone_target_compatibility_detector"])
        self.assertFalse(scope["deck_skeleton"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["runtime_effect_execution"])

    def test_policy_blocks_exact_resource_amount_claims(self):
        policy = self.report["policy"]
        self.assertTrue(policy["no_resource_amount_claim_without_structured_cost_amounts"])
        self.assertTrue(policy["manual_review_blocks_high_confidence"])

    def test_counter_blast_demand_and_recovery_are_indexed(self):
        indexes = self.report["resource_indexes"]
        self.assertIn("counter_blast", indexes["demand_index"])
        self.assertIn("counter_blast", indexes["provider_index"])

    def test_resource_support_finding_exists(self):
        supports = [
            finding
            for finding in self.report["edge_resource_findings"]
            if finding["verdict"] in {"resource_support", "mixed_support_and_shared_pressure"}
            and "counter_blast" in finding["supported_resources"]
        ]
        self.assertTrue(supports)

    def test_shared_pressure_finding_exists(self):
        shared = [
            finding
            for finding in self.report["edge_resource_findings"]
            if "counter_blast" in finding["shared_pressure_resources"]
        ]
        self.assertTrue(shared)

    def test_manual_review_findings_remain_review_required(self):
        manual = [finding for finding in self.report["edge_resource_findings"] if finding["manual_review_required"]]
        self.assertTrue(manual)
        for finding in manual:
            self.assertEqual("review_required", finding["confidence"])

    def test_ready_for_next_timing_detector(self):
        self.assertTrue(self.report["summary"]["ready_for_m35_c3"])
        self.assertEqual("M35-C3", self.report["next_target"])

    def test_output_file_can_round_trip_after_cli_generation(self):
        path = ROOT / "outputs" / "target_slice" / "m35_c2_first_slice_resource_conflict_detector.json"
        if path.exists():
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual("M35-C2", data["version"])


if __name__ == "__main__":
    unittest.main()
