"""Tests for tools/deck/validate_fourth_slice_repaired_recipe.py (M49-04)."""

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

from tools.deck.validate_fourth_slice_repaired_recipe import (  # noqa: E402
    M49_03_ACCEPTED_REPAIR,
    build_fourth_slice_repaired_validation_report,
    load_json,
    write_json,
    write_markdown,
)


class TestFourthSliceRepairedRecipeValidation(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.accepted_repair = load_json(M49_03_ACCEPTED_REPAIR)
        cls.report = build_fourth_slice_repaired_validation_report(cls.accepted_repair)

    def test_repaired_recipe_validation_passes(self):
        summary = self.report["summary"]

        self.assertEqual("M49-04", self.report["version"])
        self.assertEqual("m48_recipe_001", summary["accepted_recipe_id"])
        self.assertTrue(summary["human_acceptance_recorded"])
        self.assertEqual(1, summary["recipe_count"])
        self.assertEqual(1, summary["runtime_ready_recipe_count"])
        self.assertEqual(1, summary["validator_passed_count"])
        self.assertEqual(0, summary["invalid_draft_count"])
        self.assertEqual(0, summary["blocked_by_manual_review_count"])
        self.assertTrue(summary["ready_for_m49_05"])

    def test_repaired_recipe_has_no_validation_blockers(self):
        validation = self.report["recipe_validations"][0]
        counts = validation["count_summary"]

        self.assertEqual("validator_passed", validation["validation_status"])
        self.assertTrue(validation["runtime_ready"])
        self.assertEqual(0, validation["blocking_issue_count"])
        self.assertEqual([], validation["issues"])
        self.assertEqual(50, counts["explicit_card_count"])
        self.assertEqual(16, counts["trigger_count"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, counts["trigger_counts"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, counts["grade_counts"])
        self.assertEqual(0, counts["grade4_main_deck_count"])
        self.assertEqual({"รอยัล พาลาดิน": 50}, counts["clan_counts"])

    def test_g_zone_boundary_is_main_deck_only_and_suppressed_issue_is_absent(self):
        policy = self.report["validation_policy"]
        summary = self.report["summary"]

        self.assertEqual("m49_03_repaired_quantity_preview", policy["validated_input"])
        self.assertTrue(policy["human_acceptance_required"])
        self.assertTrue(policy["g_zone_boundary_required"])
        self.assertTrue(policy["main_deck_only_validation"])
        self.assertEqual(["g_zone_support_deferred"], policy["suppressed_by_m49_02_boundary_issue_codes"])
        self.assertEqual("main_deck_only_for_current_windows_fixture", summary["selected_g_zone_option_id"])
        self.assertTrue(summary["main_deck_only_boundary_applied"])
        self.assertFalse(summary["g_zone_runtime_enabled"])
        self.assertFalse(summary["stride_runtime_enabled"])
        self.assertEqual(0, summary["g_zone_deferred_recipe_count"])

    def test_issue_counts_are_clear_for_fixture_gate(self):
        summary = self.report["summary"]

        self.assertEqual(0, summary["missing_card_recipe_count"])
        self.assertEqual(0, summary["copy_limit_violation_recipe_count"])
        self.assertEqual(0, summary["slot_gap_recipe_count"])
        self.assertEqual(0, summary["trigger_count_mismatch_recipe_count"])
        self.assertEqual(0, summary["manual_review_overlap_recipe_count"])
        self.assertEqual(0, summary["grade_profile_review_recipe_count"])
        self.assertEqual(0, summary["grade4_main_deck_recipe_count"])
        self.assertEqual({}, summary["issue_counts"])

    def test_scope_does_not_promote_runtime_fixture(self):
        scope = self.report["scope"]
        summary = self.report["summary"]

        self.assertTrue(scope["offline_validation_rerun"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["records_g_zone_decision"])
        self.assertFalse(scope["changes_accepted_repair_artifact"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["automatic_deck_injection"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertFalse(scope["g_zone_runtime_enabled"])
        self.assertFalse(scope["stride_runtime_enabled"])
        self.assertFalse(summary["runtime_fixture_created"])
        self.assertFalse(summary["runtime_promotion_allowed"])

    def test_missing_acceptance_blocks_fixture_gate_even_if_counts_pass(self):
        accepted = copy.deepcopy(self.accepted_repair)
        accepted["summary"]["human_acceptance_recorded"] = False

        report = build_fourth_slice_repaired_validation_report(accepted)

        self.assertEqual(1, report["summary"]["validator_passed_count"])
        self.assertFalse(report["summary"]["human_acceptance_recorded"])
        self.assertFalse(report["summary"]["ready_for_m49_05"])

    def test_next_target_is_m49_05(self):
        next_target = self.report["next_target"]

        self.assertEqual("M49-05", next_target["milestone"])
        self.assertEqual("Fourth-slice runtime fixture gate", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m49_04.json"
            md_path = out / "m49_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M49-04", loaded["version"])
            self.assertIn("M49-04 Fourth-Slice Repaired Recipe Validation Rerun", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
