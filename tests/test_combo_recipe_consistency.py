"""Tests for tools/deck/check_combo_recipe_consistency.py (M36-04)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.check_combo_recipe_consistency import (  # noqa: E402
    D3_COMBO_LINES,
    M36_RECIPE_DRAFTS,
    M36_REVIEW_PACKET,
    M36_VALIDATION,
    build_consistency_report,
    load_json,
    write_json,
    write_markdown,
)


class TestComboRecipeConsistency(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.combo = load_json(D3_COMBO_LINES)
        cls.packet = load_json(M36_REVIEW_PACKET)
        cls.recipes = load_json(M36_RECIPE_DRAFTS)
        cls.validation = load_json(M36_VALIDATION)
        cls.report = build_consistency_report(cls.combo, cls.packet, cls.recipes, cls.validation)

    def test_consistency_checks_all_combo_lines(self):
        summary = self.report["summary"]

        self.assertEqual("M36-04", self.report["version"])
        self.assertTrue(summary["ready_for_m36_05"])
        self.assertEqual(25, summary["combo_line_count"])
        self.assertEqual(25, summary["consistency_check_count"])
        self.assertEqual(0, summary["promotion_allowed_count"])
        self.assertEqual(0, summary["runtime_ready_consistent_count"])

    def test_scope_is_offline_only(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_consistency_check"])
        self.assertFalse(scope["runtime_deck"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_deck_injection"])

    def test_accepted_seed_combo_cards_are_present_but_recipe_invalid(self):
        accepted = [
            item
            for item in self.report["consistency_checks"]
            if item["line_id"] == "line_003"
        ]

        self.assertEqual(1, len(accepted))
        item = accepted[0]
        self.assertTrue(item["combo_cards_present"])
        self.assertEqual([], item["missing_combo_card_ids"])
        self.assertEqual("invalid_draft", item["recipe_validation_status"])
        self.assertEqual("invalid_recipe", item["consistency_status"])
        self.assertFalse(item["promotion_allowed"])

    def test_rejected_lines_remain_blocked_by_review(self):
        blocked = [
            item
            for item in self.report["consistency_checks"]
            if item["consistency_status"] == "blocked_by_review"
        ]

        self.assertEqual(24, len(blocked))
        for item in blocked:
            self.assertEqual("blocked_by_review", item["recipe_validation_status"])
            self.assertFalse(item["promotion_allowed"])

    def test_no_missing_combo_cards_or_manual_dependencies_in_current_drafts(self):
        summary = self.report["summary"]

        self.assertEqual(25, summary["combo_cards_present_count"])
        self.assertEqual(0, summary["missing_combo_card_check_count"])
        self.assertEqual(0, summary["manual_review_dependency_check_count"])

    def test_next_target_is_m36_05(self):
        next_target = self.report["next_target"]

        self.assertEqual("M36-05", next_target["milestone"])
        self.assertEqual("Second-slice readiness comparison", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "consistency.json"
            md_path = out / "consistency.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M36-04", loaded["version"])
            self.assertIn("M36-04 Combo-Line To Recipe Consistency", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
