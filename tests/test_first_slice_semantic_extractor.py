"""Tests for tools/deck/extract_first_slice_semantics.py (M35-B2)."""

from __future__ import annotations

import json
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.extract_first_slice_semantics import build_report, load_json, VOCABULARY_REPORT  # noqa: E402


class TestFirstSliceSemanticExtractor(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = build_report()
        cls.vocabulary = load_json(VOCABULARY_REPORT)

    def test_extracts_selected_slice_cards(self):
        self.assertEqual("M35-B2", self.report["version"])
        self.assertGreater(self.report["summary"]["card_count"], 0)
        self.assertGreater(self.report["summary"]["cards_with_any_semantic_tag"], 0)

    def test_output_is_advisory_only(self):
        scope = self.report["scope"]
        self.assertTrue(scope["advisory_only"])
        self.assertFalse(scope["runtime_effect_execution"])
        self.assertFalse(scope["live_card_text_parser"])

    def test_all_semantic_tags_are_in_vocabulary(self):
        allowed = {
            category: set(values)
            for category, values in self.vocabulary["vocabulary"].items()
        }
        for card in self.report["cards"]:
            for category, tags in card["semantic_tags"].items():
                for tag in tags:
                    self.assertIn(tag, allowed[category], f"{card['card_id']} {category}:{tag}")

    def test_excluded_first_slice_tags_are_not_used(self):
        self.assertEqual([], self.report["excluded_first_slice_tags_used"])
        self.assertTrue(self.report["ready_for_m35_b3"])

    def test_manual_review_queue_can_be_derived(self):
        manual = [card for card in self.report["cards"] if card["manual_review_required"]]
        self.assertEqual(self.report["summary"]["manual_review_count"], len(manual))

    def test_output_file_can_round_trip_after_cli_generation(self):
        path = ROOT / "outputs" / "target_slice" / "m35_b2_first_slice_semantic_tags.json"
        if path.exists():
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual("M35-B2", data["version"])


if __name__ == "__main__":
    unittest.main()
