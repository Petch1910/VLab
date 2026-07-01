"""Tests for tools/deck/build_first_slice_manual_review_queue.py (M35-B4)."""

from __future__ import annotations

import json
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_first_slice_manual_review_queue import build_queue  # noqa: E402
from tools.deck.extract_first_slice_semantics import build_report as build_semantic_report  # noqa: E402
from tools.deck.build_first_slice_requirement_provider_model import build_report as build_provider_report  # noqa: E402


class TestFirstSliceManualReviewQueue(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.semantic_report = build_semantic_report()
        cls.provider_report = build_provider_report(cls.semantic_report)
        cls.report = build_queue(cls.semantic_report, cls.provider_report)

    def test_queue_matches_semantic_manual_review_count(self):
        expected = self.semantic_report["summary"]["manual_review_count"]
        self.assertEqual(expected, self.report["summary"]["manual_review_count"])
        self.assertEqual(expected, len(self.report["manual_review_queue"]))

    def test_queue_blocks_playbook_and_high_confidence_compatibility(self):
        policy = self.report["queue_policy"]
        self.assertTrue(policy["blocks_playbook_promotion"])
        self.assertTrue(policy["blocks_high_confidence_compatibility"])
        self.assertTrue(policy["does_not_remove_cards_from_dataset"])

    def test_excluded_from_playbook_inputs_matches_queue_ids(self):
        queue_ids = [item["card_id"] for item in self.report["manual_review_queue"]]
        self.assertEqual(queue_ids, self.report["excluded_from_playbook_inputs"])

    def test_queue_items_have_review_reasons(self):
        for item in self.report["manual_review_queue"]:
            self.assertTrue(item["manual_review_reasons"], item["card_id"])

    def test_ready_for_phase_c(self):
        self.assertTrue(self.report["summary"]["ready_for_phase_c"])
        self.assertEqual("M35-C1", self.report["next_target"])

    def test_output_file_can_round_trip_after_cli_generation(self):
        path = ROOT / "outputs" / "target_slice" / "m35_b4_first_slice_manual_review_queue.json"
        if path.exists():
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual("M35-B4", data["version"])


if __name__ == "__main__":
    unittest.main()
