"""Tests for tools/deck/check_second_slice_combo_recipe_consistency.py (M40-04)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.check_second_slice_combo_recipe_consistency import (  # noqa: E402
    M40_RECIPE_DRAFTS,
    M40_VALIDATION,
    build_second_slice_consistency_report,
    load_json,
    write_json,
    write_markdown,
)


class TestSecondSliceComboRecipeConsistency(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.recipes = load_json(M40_RECIPE_DRAFTS)
        cls.validation = load_json(M40_VALIDATION)
        cls.report = build_second_slice_consistency_report(cls.recipes, cls.validation)

    def test_consistency_checks_all_second_slice_drafts(self):
        summary = self.report["summary"]

        self.assertEqual("M40-04", self.report["version"])
        self.assertTrue(summary["ready_for_m40_05"])
        self.assertEqual(25, summary["recipe_count"])
        self.assertEqual(25, summary["consistency_check_count"])
        self.assertEqual(25, summary["pair_cards_present_count"])
        self.assertEqual(0, summary["missing_pair_card_check_count"])

    def test_promotion_remains_blocked_by_manual_review(self):
        summary = self.report["summary"]

        self.assertEqual(0, summary["promotion_allowed_count"])
        self.assertEqual(0, summary["runtime_ready_consistent_count"])
        self.assertEqual(0, summary["pair_manual_dependency_check_count"])
        self.assertEqual(25, summary["recipe_manual_dependency_check_count"])
        self.assertEqual({"blocked_by_manual_review": 25}, summary["status_counts"])

    def test_scope_is_offline_only(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_consistency_check"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["runtime_deck"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["automatic_deck_injection"])
        self.assertFalse(scope["direct_GameState_mutation"])

    def test_each_candidate_pair_is_present_but_validation_blocks(self):
        for item in self.report["consistency_checks"]:
            self.assertTrue(item["pair_cards_present"])
            self.assertEqual([], item["missing_pair_card_ids"])
            self.assertEqual("blocked_by_manual_review", item["recipe_validation_status"])
            self.assertEqual("blocked_by_manual_review", item["consistency_status"])
            self.assertFalse(item["promotion_allowed"])

    def test_next_target_is_m40_05(self):
        next_target = self.report["next_target"]

        self.assertEqual("M40-05", next_target["milestone"])
        self.assertEqual("Second-slice blocker repair candidates", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m40_04.json"
            md_path = out / "m40_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M40-04", loaded["version"])
            self.assertIn("M40-04 Second-Slice Combo-To-Recipe Consistency", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
