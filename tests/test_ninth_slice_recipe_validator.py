"""Tests for tools/deck/validate_ninth_slice_recipe_drafts.py (M68-04)."""

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
from tools.deck.validate_ninth_slice_recipe_drafts import (  # noqa: E402
    _all_card_ids,
    build_ninth_slice_validation_report,
    load_card_rows,
    write_json,
    write_markdown,
)


class TestNinthSliceRecipeValidator(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        ninth_draft_fixture.TestNinthSliceRecipeDraftModel.setUpClass()
        cls.recipe_report = ninth_draft_fixture.TestNinthSliceRecipeDraftModel.report
        cls.card_rows = load_card_rows(_all_card_ids(cls.recipe_report))
        cls.report = build_ninth_slice_validation_report(cls.recipe_report, cls.card_rows)

    def test_validator_runs_all_ninth_slice_drafts(self):
        summary = self.report["summary"]

        self.assertEqual("M68-04", self.report["version"])
        self.assertTrue(summary["ready_for_m68_05"])
        self.assertEqual(25, summary["recipe_count"])
        self.assertEqual(0, summary["missing_card_recipe_count"])
        self.assertEqual(0, summary["copy_limit_violation_recipe_count"])
        self.assertEqual(0, summary["slot_gap_recipe_count"])
        self.assertEqual(0, summary["trigger_count_mismatch_recipe_count"])
        self.assertEqual(0, summary["trigger_profile_mismatch_recipe_count"])
        self.assertEqual(0, summary["required_grade_coverage_missing_recipe_count"])
        self.assertEqual(0, summary["set_scope_mismatch_recipe_count"])
        self.assertEqual(0, summary["grade4_main_deck_recipe_count"])

    def test_manual_review_overlap_blocks_runtime_readiness(self):
        summary = self.report["summary"]

        self.assertEqual(0, summary["runtime_ready_recipe_count"])
        self.assertEqual(0, summary["validator_passed_count"])
        self.assertEqual(0, summary["validator_passed_pending_human_selection_count"])
        self.assertEqual(0, summary["invalid_draft_count"])
        self.assertEqual(25, summary["blocked_by_manual_review_count"])
        self.assertEqual(25, summary["manual_review_overlap_recipe_count"])
        self.assertEqual(23, summary["grade_profile_review_recipe_count"])
        self.assertEqual(25, summary["g_zone_deferred_recipe_count"])
        self.assertEqual(25, summary["stride_deferred_recipe_count"])
        self.assertEqual(25, summary["aqua_force_battle_order_deferred_recipe_count"])

    def test_scope_is_offline_validator_only(self):
        scope = self.report["scope"]
        policy = self.report["validation_policy"]

        self.assertTrue(scope["offline_validator"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["runtime_deck"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["automatic_deck_injection"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertEqual(50, policy["main_deck_size"])
        self.assertEqual(16, policy["trigger_count"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, policy["required_trigger_profile"])
        self.assertEqual(4, policy["heal_trigger_max"])
        self.assertEqual(["0", "1", "2", "3"], policy["required_setup_grades"])
        self.assertEqual(0, policy["grade4_main_deck_max_until_g_zone_support"])
        self.assertEqual(["G-BT02", "G-CB02", "G-TD04"], policy["expected_series_scope"])
        self.assertTrue(policy["manual_review_card_overlap_is_blocking"])
        self.assertTrue(policy["human_selection_pending_is_review_not_blocker"])
        self.assertTrue(policy["g_zone_support_deferred_is_review_not_blocker"])
        self.assertTrue(policy["stride_support_deferred_is_review_not_blocker"])
        self.assertTrue(policy["aqua_force_battle_order_deferred_is_review_not_blocker"])

    def test_each_recipe_has_expected_counts_and_manual_blocker(self):
        expected_clan = self.recipe_report["selected_target"]["group"]

        for item in self.report["recipe_validations"]:
            self.assertEqual("blocked_by_manual_review", item["validation_status"])
            self.assertFalse(item["runtime_ready"])
            self.assertEqual(1, item["blocking_issue_count"])
            counts = item["count_summary"]
            self.assertEqual(50, counts["explicit_card_count"])
            self.assertEqual(16, counts["trigger_count"])
            self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, counts["trigger_counts"])
            self.assertEqual(0, counts["grade4_main_deck_count"])
            self.assertEqual({expected_clan: 50}, counts["clan_counts"])
            self.assertTrue(set(counts["series_counts"]).issubset({"G-BT02", "G-CB02", "G-TD04"}))
            for grade in ("0", "1", "2", "3"):
                self.assertGreater(counts["grade_counts"].get(grade, 0), 0)
            codes = {issue["code"] for issue in item["issues"]}
            self.assertIn("manual_review_card_overlap", codes)
            self.assertIn("g_zone_support_deferred", codes)
            self.assertIn("stride_support_deferred", codes)
            self.assertIn("aqua_force_battle_order_support_deferred", codes)
            self.assertIn("human_recipe_selection_pending", codes)
            self.assertNotIn("grade4_main_deck_not_allowed", codes)
            self.assertNotIn("trigger_count_mismatch", codes)
            self.assertNotIn("trigger_profile_mismatch", codes)
            self.assertNotIn("set_scope_mismatch", codes)

    def test_issue_counts_are_stable(self):
        issue_counts = self.report["summary"]["issue_counts"]

        self.assertEqual(25, issue_counts["manual_review_card_overlap"])
        self.assertEqual(23, issue_counts["grade_profile_review"])
        self.assertEqual(25, issue_counts["g_zone_support_deferred"])
        self.assertEqual(25, issue_counts["stride_support_deferred"])
        self.assertEqual(25, issue_counts["aqua_force_battle_order_support_deferred"])
        self.assertEqual(25, issue_counts["human_recipe_selection_pending"])

    def test_next_target_is_m68_05(self):
        next_target = self.report["next_target"]

        self.assertEqual("M68-05", next_target["milestone"])
        self.assertEqual("Ninth-slice combo-to-recipe consistency", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m68_04.json"
            md_path = out / "m68_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M68-04", loaded["version"])
            self.assertIn("M68-04 Ninth-Slice Recipe Validation Report", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
