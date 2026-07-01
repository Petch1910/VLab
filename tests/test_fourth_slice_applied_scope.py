"""Tests for tools/deck/build_fourth_slice_applied_scope.py."""

from __future__ import annotations

import copy
import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_fourth_slice_applied_scope import (  # noqa: E402
    M47_EXPAND_SCOPE_REVIEW,
    build_fourth_slice_applied_scope,
    load_json,
    write_json,
    write_markdown,
)


class TestFourthSliceAppliedScope(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.review = load_json(M47_EXPAND_SCOPE_REVIEW)
        cls.report = build_fourth_slice_applied_scope(cls.review)

    def test_applies_recommended_g_era_scope(self):
        scope = self.report["applied_scope"]
        summary = self.report["summary"]

        self.assertEqual("M47-repair-apply-scope", self.report["version"])
        self.assertEqual("g_era_heal_expansion", scope["expansion_id"])
        self.assertEqual(25, scope["base_series_count"])
        self.assertEqual(7, scope["added_series_count"])
        self.assertEqual(32, scope["effective_series_count"])
        self.assertEqual(190, summary["source_card_count"])
        self.assertEqual([], summary["trigger_type_gaps"])

    def test_added_series_match_review_decision(self):
        scope = self.report["applied_scope"]

        self.assertEqual(
            ["G-BT14", "G-CHB01", "G-CMB01", "G-FC04", "G-LD03", "G-MT01", "G-TD11"],
            scope["added_series"],
        )
        for series_code in scope["base_series_scope"]:
            self.assertIn(series_code, scope["effective_series_scope"])
        for series_code in scope["added_series"]:
            self.assertIn(series_code, scope["effective_series_scope"])

    def test_card_pool_is_fixture_ready_after_scope_apply(self):
        pool = self.report["card_pool_summary"]
        readiness = self.report["readiness"]

        self.assertEqual({"0": 47, "1": 48, "2": 46, "3": 27, "4": 22}, pool["grade_counts"])
        self.assertEqual(15, pool["trigger_counts"]["Critical"])
        self.assertEqual(6, pool["trigger_counts"]["Draw"])
        self.assertEqual(7, pool["trigger_counts"]["Heal"])
        self.assertEqual(6, pool["trigger_counts"]["Stand"])
        self.assertEqual(136, pool["trigger_capacity"])
        self.assertEqual(624, pool["non_trigger_capacity"])
        self.assertTrue(readiness["source_backed"])
        self.assertTrue(readiness["has_grade_setup"])
        self.assertTrue(readiness["has_classic_trigger_capacity"])
        self.assertTrue(readiness["has_main_deck_capacity"])
        self.assertTrue(readiness["all_fixture_expectations_met"])
        self.assertTrue(readiness["semantic_probe_ready"])
        self.assertTrue(readiness["ready_for_m47_03"])

    def test_boundary_blocks_runtime_and_bot_promotion(self):
        boundary = self.report["boundary"]
        readiness = self.report["readiness"]

        self.assertTrue(boundary["applies_scope_to_offline_fixture_pipeline"])
        self.assertFalse(boundary["card_data_mutated"])
        self.assertFalse(boundary["recipe_draft_created"])
        self.assertFalse(boundary["runtime_fixture_created"])
        self.assertFalse(boundary["runtime_pack_mutated"])
        self.assertFalse(boundary["saved_deck_enabled"])
        self.assertFalse(boundary["ui_deck_list_enabled"])
        self.assertFalse(boundary["bot_playbook_enabled"])
        self.assertFalse(boundary["GameState_mutation"])
        self.assertFalse(readiness["runtime_or_bot_promotion_allowed"])

    def test_missing_recommendation_routes_back_to_review(self):
        review = copy.deepcopy(self.review)
        review["decision"]["recommended_expansion_id"] = ""

        report = build_fourth_slice_applied_scope(review)

        self.assertFalse(report["readiness"]["ready_for_m47_03"])
        self.assertIn("recommended_expansion_missing", report["readiness"]["blockers"])
        self.assertEqual("M47-repair-expand-scope", report["next_target"]["milestone"])

    def test_next_target_is_m47_03(self):
        self.assertEqual("M47-03", self.report["next_target"]["milestone"])
        self.assertEqual("Fourth-slice semantic/compatibility probe", self.report["next_target"]["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m47_apply_scope.json"
            md_path = out / "m47_apply_scope.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M47-repair-apply-scope", loaded["version"])
            self.assertIn("M47-repair-apply-scope Applied Source Scope", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
