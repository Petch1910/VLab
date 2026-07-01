"""Tests for tools/deck/build_first_slice_semantic_vocabulary.py (M35-B1)."""

from __future__ import annotations

import json
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_first_slice_semantic_vocabulary import build_vocabulary  # noqa: E402


class TestFirstSliceSemanticVocabulary(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = build_vocabulary()

    def test_vocabulary_ready_for_extractor(self):
        self.assertTrue(self.report["ready_for_m35_b2"])
        self.assertEqual([], self.report["missing_source_terms"])
        self.assertEqual("M35-B2", self.report["next_target"])

    def test_scope_is_advisory_only(self):
        scope = self.report["scope"]
        self.assertTrue(scope["advisory_only"])
        self.assertFalse(scope["runtime_effect_execution"])
        self.assertFalse(scope["live_card_text_parser"])

    def test_classic_core_terms_are_present(self):
        vocab = self.report["vocabulary"]
        self.assertIn("ACT", vocab["ability_types"])
        self.assertIn("AUTO", vocab["ability_types"])
        self.assertIn("CONT", vocab["ability_types"])
        self.assertIn("counter_blast", vocab["costs"])
        self.assertIn("stand_unit", vocab["effects"])
        self.assertIn("limit_break", vocab["mechanic_groups"])
        self.assertIn("stand", vocab["trigger_icons"])

    def test_later_mechanics_are_excluded(self):
        excluded = set(self.report["excluded_first_slice_tags"])
        for tag in ["stride", "imaginary_gift", "ride_deck", "energy", "divine_skill"]:
            self.assertIn(tag, excluded)
            self.assertNotIn(tag, self.report["vocabulary"]["mechanic_groups"])

    def test_output_file_can_round_trip_after_cli_generation(self):
        path = ROOT / "outputs" / "target_slice" / "m35_b1_first_slice_semantic_vocabulary.json"
        if path.exists():
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual("M35-B1", data["version"])


if __name__ == "__main__":
    unittest.main()
