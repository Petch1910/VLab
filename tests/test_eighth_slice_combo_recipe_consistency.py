"""Tests for tools/deck/check_eighth_slice_combo_recipe_consistency.py (M64-05)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import tests.test_eighth_slice_recipe_draft_model as eighth_draft_fixture  # noqa: E402
from tools.deck.check_eighth_slice_combo_recipe_consistency import (  # noqa: E402
    build_eighth_slice_consistency_report,
    write_json,
    write_markdown,
)
from tools.deck.validate_eighth_slice_recipe_drafts import (  # noqa: E402
    _all_card_ids,
    build_eighth_slice_validation_report,
    load_card_rows,
)


class TestEighthSliceComboRecipeConsistency(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        eighth_draft_fixture.TestEighthSliceRecipeDraftModel.setUpClass()
        cls.recipes = eighth_draft_fixture.TestEighthSliceRecipeDraftModel.report
        cls.card_rows = load_card_rows(_all_card_ids(cls.recipes))
        cls.validation = build_eighth_slice_validation_report(cls.recipes, cls.card_rows)
        cls.report = build_eighth_slice_consistency_report(cls.recipes, cls.validation)

    def test_consistency_checks_all_eighth_slice_drafts(self):
        summary = self.report["summary"]

        self.assertEqual("M64-05", self.report["version"])
        self.assertTrue(summary["ready_for_m64_06"])
        self.assertEqual(25, summary["recipe_count"])
        self.assertEqual(25, summary["consistency_check_count"])
        self.assertEqual(25, summary["pair_cards_present_count"])
        self.assertEqual(0, summary["missing_pair_card_check_count"])

    def test_promotion_remains_blocked_by_human_selection_and_deferred_mechanics(self):
        summary = self.report["summary"]

        self.assertEqual(0, summary["promotion_allowed_count"])
        self.assertEqual(0, summary["runtime_ready_consistent_count"])
        self.assertEqual(0, summary["pair_manual_dependency_check_count"])
        self.assertEqual(0, summary["recipe_manual_dependency_check_count"])
        self.assertEqual(25, summary["lock_deferred_check_count"])
        self.assertEqual(25, summary["legion_deferred_check_count"])
        self.assertEqual({"consistent_pending_human_selection": 25}, summary["status_counts"])

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
            self.assertEqual("validator_passed_pending_human_selection", item["recipe_validation_status"])
            self.assertEqual("consistent_pending_human_selection", item["consistency_status"])
            self.assertEqual([], item["pair_manual_review_dependencies"])
            self.assertEqual([], item["recipe_manual_review_dependencies"])
            self.assertTrue(item["lock_runtime_support_deferred"])
            self.assertTrue(item["legion_runtime_support_deferred"])
            self.assertFalse(item["promotion_allowed"])

    def test_next_target_is_m64_06(self):
        next_target = self.report["next_target"]

        self.assertEqual("M64-06", next_target["milestone"])
        self.assertEqual("Eighth-slice blocker repair candidates", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m64_05.json"
            md_path = out / "m64_05.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M64-05", loaded["version"])
            self.assertIn("M64-05 Eighth-Slice Combo-To-Recipe Consistency", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
