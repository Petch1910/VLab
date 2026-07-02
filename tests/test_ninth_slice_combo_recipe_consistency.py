"""Tests for tools/deck/check_ninth_slice_combo_recipe_consistency.py (M68-05)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import tests.test_ninth_slice_recipe_draft_model as ninth_draft_fixture  # noqa: E402
from tools.deck.check_ninth_slice_combo_recipe_consistency import (  # noqa: E402
    build_ninth_slice_consistency_report,
    write_json,
    write_markdown,
)
from tools.deck.validate_ninth_slice_recipe_drafts import (  # noqa: E402
    _all_card_ids,
    build_ninth_slice_validation_report,
    load_card_rows,
)


class TestNinthSliceComboRecipeConsistency(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        ninth_draft_fixture.TestNinthSliceRecipeDraftModel.setUpClass()
        cls.recipes = ninth_draft_fixture.TestNinthSliceRecipeDraftModel.report
        cls.card_rows = load_card_rows(_all_card_ids(cls.recipes))
        cls.validation = build_ninth_slice_validation_report(cls.recipes, cls.card_rows)
        cls.report = build_ninth_slice_consistency_report(cls.recipes, cls.validation)

    def test_consistency_checks_all_ninth_slice_drafts(self):
        summary = self.report["summary"]

        self.assertEqual("M68-05", self.report["version"])
        self.assertTrue(summary["ready_for_m68_06"])
        self.assertEqual(25, summary["recipe_count"])
        self.assertEqual(25, summary["consistency_check_count"])
        self.assertEqual(25, summary["pair_cards_present_count"])
        self.assertEqual(0, summary["missing_pair_card_check_count"])

    def test_promotion_remains_blocked_by_manual_review_and_g_series_gates(self):
        summary = self.report["summary"]

        self.assertEqual(0, summary["promotion_allowed_count"])
        self.assertEqual(0, summary["runtime_ready_consistent_count"])
        self.assertEqual(0, summary["pair_manual_dependency_check_count"])
        self.assertEqual(25, summary["recipe_manual_dependency_check_count"])
        self.assertEqual(25, summary["g_zone_deferred_check_count"])
        self.assertEqual(25, summary["stride_deferred_check_count"])
        self.assertEqual(25, summary["aqua_force_battle_order_deferred_check_count"])
        self.assertEqual(23, summary["grade_profile_review_check_count"])
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

    def test_each_candidate_pair_is_present_but_not_promoted(self):
        for item in self.report["consistency_checks"]:
            self.assertTrue(item["pair_cards_present"])
            self.assertEqual([], item["missing_pair_card_ids"])
            self.assertEqual("blocked_by_manual_review", item["recipe_validation_status"])
            self.assertEqual("blocked_by_manual_review", item["consistency_status"])
            self.assertEqual([], item["pair_manual_review_dependencies"])
            self.assertTrue(item["recipe_manual_review_dependencies"])
            self.assertTrue(item["g_zone_support_deferred"])
            self.assertTrue(item["stride_support_deferred"])
            self.assertTrue(item["aqua_force_battle_order_support_deferred"])
            self.assertFalse(item["promotion_allowed"])

    def test_next_target_is_m68_06(self):
        next_target = self.report["next_target"]

        self.assertEqual("M68-06", next_target["milestone"])
        self.assertEqual("Ninth-slice blocker repair candidates", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m68_05.json"
            md_path = out / "m68_05.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M68-05", loaded["version"])
            self.assertIn("M68-05 Ninth-Slice Combo-To-Recipe Consistency", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
