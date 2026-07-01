"""Tests for tools/deck/validate_third_slice_recipe_drafts.py (M44-04)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.validate_third_slice_recipe_drafts import (  # noqa: E402
    RECIPE_DRAFTS,
    _all_card_ids,
    build_third_slice_validation_report,
    load_card_rows,
    load_json,
    write_json,
    write_markdown,
)


class TestThirdSliceRecipeValidator(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.recipe_report = load_json(RECIPE_DRAFTS)
        cls.card_rows = load_card_rows(_all_card_ids(cls.recipe_report))
        cls.report = build_third_slice_validation_report(cls.recipe_report, cls.card_rows)

    def test_validator_runs_all_third_slice_drafts(self):
        summary = self.report["summary"]

        self.assertEqual("M44-04", self.report["version"])
        self.assertTrue(summary["ready_for_m44_05"])
        self.assertEqual(25, summary["recipe_count"])
        self.assertEqual(0, summary["missing_card_recipe_count"])
        self.assertEqual(0, summary["copy_limit_violation_recipe_count"])
        self.assertEqual(0, summary["slot_gap_recipe_count"])
        self.assertEqual(0, summary["trigger_count_mismatch_recipe_count"])

    def test_manual_review_overlap_blocks_runtime_readiness(self):
        summary = self.report["summary"]

        self.assertEqual(0, summary["runtime_ready_recipe_count"])
        self.assertEqual(0, summary["validator_passed_count"])
        self.assertEqual(25, summary["blocked_by_manual_review_count"])
        self.assertEqual(25, summary["manual_review_overlap_recipe_count"])
        self.assertEqual(25, summary["grade_profile_review_recipe_count"])

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
        self.assertEqual(50, policy["classic_main_deck_size"])
        self.assertEqual(16, policy["classic_trigger_count"])
        self.assertTrue(policy["manual_review_card_overlap_is_blocking"])
        self.assertTrue(policy["human_selection_pending_is_review_not_blocker"])

    def test_each_recipe_has_expected_counts_and_manual_blocker(self):
        for item in self.report["recipe_validations"]:
            self.assertEqual("blocked_by_manual_review", item["validation_status"])
            self.assertFalse(item["runtime_ready"])
            counts = item["count_summary"]
            self.assertEqual(50, counts["explicit_card_count"])
            self.assertEqual(16, counts["trigger_count"])
            codes = {issue["code"] for issue in item["issues"]}
            self.assertIn("manual_review_card_overlap", codes)
            self.assertIn("grade_profile_review", codes)
            self.assertIn("human_recipe_selection_pending", codes)

    def test_next_target_is_m44_05(self):
        next_target = self.report["next_target"]

        self.assertEqual("M44-05", next_target["milestone"])
        self.assertEqual("Third-slice combo-to-recipe consistency", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m44_04.json"
            md_path = out / "m44_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M44-04", loaded["version"])
            self.assertIn("M44-04 Third-Slice Recipe Validation Report", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
