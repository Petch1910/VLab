"""Tests for tools/deck/validate_eighth_slice_repaired_recipe.py (M65-05)."""

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

import tests.test_eighth_slice_lock_legion_decision_artifact as decision_fixture  # noqa: E402
from tools.deck.build_eighth_slice_lock_legion_decision_artifact import (  # noqa: E402
    build_eighth_slice_lock_legion_decision_artifact,
)
from tools.deck.validate_eighth_slice_repaired_recipe import (  # noqa: E402
    build_eighth_slice_repaired_validation_report,
    write_json,
    write_markdown,
)


LOCK_OPTION = "main_deck_only_review_no_runtime_promotion"
LEGION_OPTION = "main_deck_only_review_no_runtime_promotion"


class TestEighthSliceRepairedRecipeValidation(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        decision_fixture.TestEighthSliceLockLegionDecisionArtifact.setUpClass()
        cls.accepted_artifact = decision_fixture.TestEighthSliceLockLegionDecisionArtifact.accepted_artifact
        cls.lock_legion_decision = build_eighth_slice_lock_legion_decision_artifact(
            cls.accepted_artifact,
            selected_lock_option=LOCK_OPTION,
            selected_legion_option=LEGION_OPTION,
        )
        cls.report = build_eighth_slice_repaired_validation_report(cls.accepted_artifact, cls.lock_legion_decision)

    def test_repaired_recipe_validation_passes_and_opens_fixture_gate(self):
        summary = self.report["summary"]

        self.assertEqual("M65-05", self.report["version"])
        self.assertEqual("m64_recipe_001", summary["selected_recipe_id"])
        self.assertEqual("m65_01_m64_recipe_001_repair_review", summary["selected_review_item_id"])
        self.assertEqual("m64_recipe_001_accepted_grade_pkg_001", summary["accepted_grade_repair_package_id"])
        self.assertTrue(summary["human_selection_recorded"])
        self.assertTrue(summary["human_acceptance_recorded"])
        self.assertTrue(summary["grade_repair_accepted"])
        self.assertTrue(summary["lock_decision_recorded"])
        self.assertTrue(summary["legion_decision_recorded"])
        self.assertEqual(LOCK_OPTION, summary["selected_lock_option_id"])
        self.assertEqual(LEGION_OPTION, summary["selected_legion_option_id"])
        self.assertTrue(summary["main_deck_only_boundary_applied"])
        self.assertEqual(1, summary["recipe_count"])
        self.assertEqual(1, summary["runtime_ready_recipe_count"])
        self.assertEqual(1, summary["validator_passed_count"])
        self.assertEqual(1, summary["promotion_allowed_count"])
        self.assertEqual("consistent_validator_passed", summary["consistency_status"])
        self.assertTrue(summary["ready_for_m65_06"])

    def test_repaired_recipe_has_no_validation_blockers_or_review_codes(self):
        validation = self.report["repaired_recipe_validation"]
        counts = validation["count_summary"]

        self.assertEqual("validator_passed", validation["validation_status"])
        self.assertTrue(validation["runtime_ready"])
        self.assertEqual(0, validation["blocking_issue_count"])
        self.assertEqual([], validation["blocker_codes"])
        self.assertEqual([], validation["review_codes"])
        self.assertEqual(50, counts["explicit_card_count"])
        self.assertEqual(16, counts["trigger_count"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, counts["trigger_counts"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, counts["grade_counts"])
        self.assertEqual(0, counts["grade4_main_deck_count"])

    def test_lock_legion_boundary_is_main_deck_only_and_runtime_stays_disabled(self):
        policy = self.report["validation_policy"]
        context = self.report["system_decision_context"]
        summary = self.report["summary"]
        consistency = self.report["repaired_recipe_consistency"]

        self.assertEqual("m65_03_repaired_quantity_preview", policy["validated_input"])
        self.assertTrue(policy["human_acceptance_required"])
        self.assertTrue(policy["grade_repair_acceptance_required"])
        self.assertTrue(policy["system_boundary_required"])
        self.assertTrue(policy["main_deck_only_validation"])
        self.assertTrue(policy["must_not_enable_lock_or_unlock_effect_resolution"])
        self.assertTrue(policy["must_not_enable_legion_or_mate_effect_resolution"])
        self.assertEqual(
            ["lock_runtime_support_deferred", "legion_runtime_support_deferred"],
            policy["suppressed_by_m65_04_boundary_issue_codes"],
        )
        self.assertEqual(LOCK_OPTION, context["selected_lock_option_id"])
        self.assertEqual(LEGION_OPTION, context["selected_legion_option_id"])
        self.assertTrue(context["main_deck_only_boundary_applied"])
        self.assertTrue(context["lock_boundary_applied"])
        self.assertTrue(context["legion_boundary_applied"])
        self.assertFalse(context["lock_runtime_enabled"])
        self.assertFalse(context["unlock_runtime_enabled"])
        self.assertFalse(context["legion_runtime_enabled"])
        self.assertFalse(context["mate_identity_check_enabled"])
        self.assertFalse(summary["lock_runtime_enabled"])
        self.assertFalse(summary["unlock_runtime_enabled"])
        self.assertFalse(summary["legion_runtime_enabled"])
        self.assertFalse(summary["mate_identity_check_enabled"])
        self.assertEqual(0, summary["lock_deferred_recipe_count"])
        self.assertEqual(0, summary["legion_deferred_recipe_count"])
        self.assertFalse(consistency["lock_runtime_support_deferred"])
        self.assertFalse(consistency["legion_runtime_support_deferred"])

    def test_consistency_passes_for_pair_cards(self):
        consistency = self.report["repaired_recipe_consistency"]

        self.assertEqual("m64_recipe_001", consistency["recipe_id"])
        self.assertEqual("consistent_validator_passed", consistency["consistency_status"])
        self.assertTrue(consistency["pair_cards_present"])
        self.assertTrue(consistency["promotion_allowed_by_validation_and_consistency"])
        self.assertEqual("BT11-005TH", consistency["source_card_id"])
        self.assertEqual("EB09-025TH", consistency["target_card_id"])

    def test_scope_does_not_promote_runtime_fixture(self):
        scope = self.report["scope"]
        summary = self.report["summary"]

        self.assertTrue(scope["offline_validation_rerun"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["records_grade_repair_acceptance"])
        self.assertFalse(scope["records_lock_decision"])
        self.assertFalse(scope["records_legion_decision"])
        self.assertFalse(scope["changes_accepted_repair_artifact"])
        self.assertFalse(scope["changes_lock_legion_decision_artifact"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["automatic_deck_injection"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertFalse(scope["lock_runtime_enabled"])
        self.assertFalse(scope["unlock_runtime_enabled"])
        self.assertFalse(scope["legion_runtime_enabled"])
        self.assertFalse(scope["mate_identity_check_enabled"])
        self.assertFalse(summary["runtime_fixture_created"])
        self.assertFalse(summary["runtime_promotion_allowed"])

    def test_deferred_lock_decision_blocks_fixture_gate(self):
        deferred_decision = build_eighth_slice_lock_legion_decision_artifact(
            self.accepted_artifact,
            selected_lock_option="defer_until_lock_unlock_runtime_exists",
            selected_legion_option=LEGION_OPTION,
        )

        report = build_eighth_slice_repaired_validation_report(self.accepted_artifact, deferred_decision)

        self.assertFalse(report["summary"]["main_deck_only_boundary_applied"])
        self.assertFalse(report["summary"]["lock_boundary_applied"])
        self.assertTrue(report["summary"]["legion_boundary_applied"])
        self.assertFalse(report["summary"]["ready_for_m65_06"])
        self.assertEqual(1, report["summary"]["validator_passed_count"])
        self.assertEqual(1, report["summary"]["lock_deferred_recipe_count"])
        self.assertEqual(0, report["summary"]["legion_deferred_recipe_count"])
        self.assertEqual(["lock_runtime_support_deferred"], report["repaired_recipe_validation"]["review_codes"])
        self.assertFalse(report["summary"]["runtime_promotion_allowed"])

    def test_deferred_legion_decision_blocks_fixture_gate(self):
        deferred_decision = build_eighth_slice_lock_legion_decision_artifact(
            self.accepted_artifact,
            selected_lock_option=LOCK_OPTION,
            selected_legion_option="defer_until_legion_mate_runtime_exists",
        )

        report = build_eighth_slice_repaired_validation_report(self.accepted_artifact, deferred_decision)

        self.assertFalse(report["summary"]["main_deck_only_boundary_applied"])
        self.assertTrue(report["summary"]["lock_boundary_applied"])
        self.assertFalse(report["summary"]["legion_boundary_applied"])
        self.assertFalse(report["summary"]["ready_for_m65_06"])
        self.assertEqual(1, report["summary"]["validator_passed_count"])
        self.assertEqual(0, report["summary"]["lock_deferred_recipe_count"])
        self.assertEqual(1, report["summary"]["legion_deferred_recipe_count"])
        self.assertEqual(
            ["legion_runtime_support_deferred"],
            report["repaired_recipe_validation"]["review_codes"],
        )
        self.assertFalse(report["summary"]["runtime_promotion_allowed"])

    def test_missing_acceptance_blocks_fixture_gate_even_if_counts_pass(self):
        accepted = copy.deepcopy(self.accepted_artifact)
        accepted["summary"]["human_acceptance_recorded"] = False
        decision = build_eighth_slice_lock_legion_decision_artifact(
            accepted,
            selected_lock_option=LOCK_OPTION,
            selected_legion_option=LEGION_OPTION,
        )

        report = build_eighth_slice_repaired_validation_report(accepted, decision)

        self.assertEqual(1, report["summary"]["validator_passed_count"])
        self.assertFalse(report["summary"]["human_acceptance_recorded"])
        self.assertFalse(report["summary"]["ready_for_m65_06"])

    def test_next_target_is_m65_06(self):
        next_target = self.report["next_target"]

        self.assertEqual("M65-06", next_target["milestone"])
        self.assertEqual("Eighth-slice runtime fixture promotion gate", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m65_05.json"
            md_path = out / "m65_05.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M65-05", loaded["version"])
            self.assertIn("M65-05 Eighth-Slice Repaired Recipe Validation Rerun", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
