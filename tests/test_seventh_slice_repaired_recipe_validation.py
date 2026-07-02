"""Tests for tools/deck/validate_seventh_slice_repaired_recipe.py (M61-05)."""

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

import tests.test_seventh_slice_system_decision_artifact as system_decision_fixture  # noqa: E402
from tools.deck.build_seventh_slice_system_decision_artifact import (  # noqa: E402
    build_seventh_slice_system_decision_artifact,
)
from tools.deck.validate_seventh_slice_repaired_recipe import (  # noqa: E402
    build_seventh_slice_repaired_validation_report,
    write_json,
    write_markdown,
)


G_OPTION = "main_deck_only_review_no_runtime_promotion"
BLOOM_OPTION = "manual_semantic_review_only_no_runtime_promotion"


class TestSeventhSliceRepairedRecipeValidation(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        system_decision_fixture.TestSeventhSliceSystemDecisionArtifact.setUpClass()
        cls.accepted_artifact = system_decision_fixture.TestSeventhSliceSystemDecisionArtifact.accepted_artifact
        cls.system_decision = build_seventh_slice_system_decision_artifact(
            cls.accepted_artifact,
            selected_g_zone_option=G_OPTION,
            selected_bloom_token_option=BLOOM_OPTION,
        )
        cls.report = build_seventh_slice_repaired_validation_report(cls.accepted_artifact, cls.system_decision)

    def test_repaired_recipe_validation_passes_and_opens_fixture_gate(self):
        summary = self.report["summary"]

        self.assertEqual("M61-05", self.report["version"])
        self.assertEqual("m60_recipe_001", summary["accepted_recipe_id"])
        self.assertEqual("m61_01_m60_recipe_001_repair_review", summary["accepted_review_item_id"])
        self.assertTrue(summary["human_acceptance_recorded"])
        self.assertTrue(summary["g_zone_decision_recorded"])
        self.assertTrue(summary["bloom_token_decision_recorded"])
        self.assertEqual(G_OPTION, summary["selected_g_zone_option_id"])
        self.assertEqual(BLOOM_OPTION, summary["selected_bloom_token_option_id"])
        self.assertTrue(summary["main_deck_only_boundary_applied"])
        self.assertEqual(1, summary["recipe_count"])
        self.assertEqual(1, summary["runtime_ready_recipe_count"])
        self.assertEqual(1, summary["validator_passed_count"])
        self.assertEqual(1, summary["promotion_allowed_count"])
        self.assertEqual("consistent_validator_passed", summary["consistency_status"])
        self.assertTrue(summary["ready_for_m61_06"])

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

    def test_system_boundary_is_main_deck_only_and_runtime_stays_disabled(self):
        policy = self.report["validation_policy"]
        context = self.report["system_decision_context"]
        summary = self.report["summary"]
        consistency = self.report["repaired_recipe_consistency"]

        self.assertEqual("m61_03_repaired_quantity_preview", policy["validated_input"])
        self.assertTrue(policy["human_acceptance_required"])
        self.assertTrue(policy["system_boundary_required"])
        self.assertTrue(policy["main_deck_only_validation"])
        self.assertTrue(policy["must_not_enable_bloom_or_token_effect_resolution"])
        self.assertEqual(
            ["g_zone_support_deferred", "bloom_token_support_deferred"],
            policy["suppressed_by_m61_04_boundary_issue_codes"],
        )
        self.assertEqual(G_OPTION, context["selected_g_zone_option_id"])
        self.assertEqual(BLOOM_OPTION, context["selected_bloom_token_option_id"])
        self.assertTrue(context["main_deck_only_boundary_applied"])
        self.assertTrue(context["g_zone_boundary_applied"])
        self.assertTrue(context["bloom_token_boundary_applied"])
        self.assertFalse(context["g_zone_runtime_enabled"])
        self.assertFalse(context["stride_runtime_enabled"])
        self.assertFalse(context["bloom_token_runtime_enabled"])
        self.assertFalse(summary["g_zone_runtime_enabled"])
        self.assertFalse(summary["stride_runtime_enabled"])
        self.assertFalse(summary["bloom_token_runtime_enabled"])
        self.assertEqual(0, summary["g_zone_deferred_recipe_count"])
        self.assertEqual(0, summary["bloom_token_deferred_recipe_count"])
        self.assertFalse(consistency["g_zone_support_deferred"])
        self.assertFalse(consistency["bloom_token_support_deferred"])

    def test_consistency_passes_for_pair_cards(self):
        consistency = self.report["repaired_recipe_consistency"]

        self.assertEqual("m60_recipe_001", consistency["recipe_id"])
        self.assertEqual("consistent_validator_passed", consistency["consistency_status"])
        self.assertTrue(consistency["pair_cards_present"])
        self.assertTrue(consistency["promotion_allowed_by_validation_and_consistency"])
        self.assertEqual("G-BT02-021TH", consistency["source_card_id"])
        self.assertEqual("G-BT02-096TH", consistency["target_card_id"])

    def test_scope_does_not_promote_runtime_fixture(self):
        scope = self.report["scope"]
        summary = self.report["summary"]

        self.assertTrue(scope["offline_validation_rerun"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["records_g_zone_decision"])
        self.assertFalse(scope["records_bloom_token_decision"])
        self.assertFalse(scope["changes_accepted_repair_artifact"])
        self.assertFalse(scope["changes_system_decision_artifact"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["automatic_deck_injection"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertFalse(scope["g_zone_runtime_enabled"])
        self.assertFalse(scope["stride_runtime_enabled"])
        self.assertFalse(scope["bloom_token_runtime_enabled"])
        self.assertFalse(summary["runtime_fixture_created"])
        self.assertFalse(summary["runtime_promotion_allowed"])

    def test_deferred_g_zone_decision_blocks_fixture_gate(self):
        deferred_decision = build_seventh_slice_system_decision_artifact(
            self.accepted_artifact,
            selected_g_zone_option="defer_until_g_zone_stride_runtime_exists",
            selected_bloom_token_option=BLOOM_OPTION,
        )

        report = build_seventh_slice_repaired_validation_report(self.accepted_artifact, deferred_decision)

        self.assertFalse(report["summary"]["main_deck_only_boundary_applied"])
        self.assertFalse(report["summary"]["g_zone_boundary_applied"])
        self.assertTrue(report["summary"]["bloom_token_boundary_applied"])
        self.assertFalse(report["summary"]["ready_for_m61_06"])
        self.assertEqual(1, report["summary"]["validator_passed_count"])
        self.assertEqual(1, report["summary"]["g_zone_deferred_recipe_count"])
        self.assertEqual(0, report["summary"]["bloom_token_deferred_recipe_count"])
        self.assertEqual(["g_zone_support_deferred"], report["repaired_recipe_validation"]["review_codes"])
        self.assertFalse(report["summary"]["runtime_promotion_allowed"])

    def test_deferred_bloom_token_decision_blocks_fixture_gate(self):
        deferred_decision = build_seventh_slice_system_decision_artifact(
            self.accepted_artifact,
            selected_g_zone_option=G_OPTION,
            selected_bloom_token_option="defer_until_bloom_token_runtime_exists",
        )

        report = build_seventh_slice_repaired_validation_report(self.accepted_artifact, deferred_decision)

        self.assertFalse(report["summary"]["main_deck_only_boundary_applied"])
        self.assertTrue(report["summary"]["g_zone_boundary_applied"])
        self.assertFalse(report["summary"]["bloom_token_boundary_applied"])
        self.assertFalse(report["summary"]["ready_for_m61_06"])
        self.assertEqual(1, report["summary"]["validator_passed_count"])
        self.assertEqual(0, report["summary"]["g_zone_deferred_recipe_count"])
        self.assertEqual(1, report["summary"]["bloom_token_deferred_recipe_count"])
        self.assertEqual(
            ["bloom_token_support_deferred"],
            report["repaired_recipe_validation"]["review_codes"],
        )
        self.assertFalse(report["summary"]["runtime_promotion_allowed"])

    def test_missing_acceptance_blocks_fixture_gate_even_if_counts_pass(self):
        accepted = copy.deepcopy(self.accepted_artifact)
        accepted["summary"]["human_acceptance_recorded"] = False
        decision = build_seventh_slice_system_decision_artifact(
            accepted,
            selected_g_zone_option=G_OPTION,
            selected_bloom_token_option=BLOOM_OPTION,
        )

        report = build_seventh_slice_repaired_validation_report(accepted, decision)

        self.assertEqual(1, report["summary"]["validator_passed_count"])
        self.assertFalse(report["summary"]["human_acceptance_recorded"])
        self.assertFalse(report["summary"]["ready_for_m61_06"])

    def test_next_target_is_m61_06(self):
        next_target = self.report["next_target"]

        self.assertEqual("M61-06", next_target["milestone"])
        self.assertEqual("Seventh-slice runtime fixture promotion gate", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m61_05.json"
            md_path = out / "m61_05.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M61-05", loaded["version"])
            self.assertIn("M61-05 Seventh-Slice Repaired Recipe Validation Rerun", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
