"""Tests for tools/deck/build_eighth_slice_runtime_fixture_promotion_gate.py (M65-06)."""

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

import tests.test_eighth_slice_repaired_recipe_validation as validation_fixture  # noqa: E402
from tools.deck.build_eighth_slice_runtime_fixture_promotion_gate import (  # noqa: E402
    build_eighth_slice_runtime_fixture_promotion_gate,
    write_json,
    write_markdown,
)


SELECTED_REVIEW_ITEM_ID = "m65_01_m64_recipe_001_repair_review"
LOCK_OPTION = "main_deck_only_review_no_runtime_promotion"
LEGION_OPTION = "main_deck_only_review_no_runtime_promotion"


class TestEighthSliceRuntimeFixturePromotionGate(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        validation_fixture.TestEighthSliceRepairedRecipeValidation.setUpClass()
        cls.accepted_artifact = validation_fixture.TestEighthSliceRepairedRecipeValidation.accepted_artifact
        cls.validation_report = validation_fixture.TestEighthSliceRepairedRecipeValidation.report
        cls.report = build_eighth_slice_runtime_fixture_promotion_gate(
            cls.accepted_artifact,
            cls.validation_report,
        )

    def test_gate_allows_fixture_when_all_checks_pass(self):
        summary = self.report["summary"]
        decision = self.report["promotion_decision"]

        self.assertEqual("M65-06", self.report["version"])
        self.assertEqual("m64_recipe_001", summary["recipe_id"])
        self.assertEqual(SELECTED_REVIEW_ITEM_ID, summary["selected_review_item_id"])
        self.assertEqual(LOCK_OPTION, summary["selected_lock_option_id"])
        self.assertEqual(LEGION_OPTION, summary["selected_legion_option_id"])
        self.assertTrue(summary["promotion_allowed"])
        self.assertEqual(7, summary["passed_check_count"])
        self.assertEqual(0, summary["failed_check_count"])
        self.assertTrue(summary["fixture_created"])
        self.assertTrue(summary["ready_for_m65_closeout"])
        self.assertTrue(decision["fixture_created"])
        self.assertFalse(decision["runtime_deck_library_mutated"])
        self.assertFalse(decision["saved_deck_injected"])
        self.assertFalse(decision["ui_deck_list_published"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertFalse(decision["lock_runtime_enabled"])
        self.assertFalse(decision["unlock_runtime_enabled"])
        self.assertFalse(decision["legion_runtime_enabled"])
        self.assertFalse(decision["mate_identity_check_enabled"])

    def test_gate_checks_have_required_categories(self):
        checks = {item["check_id"]: item for item in self.report["gate_checks"]}

        self.assertEqual(
            {
                "human_selection_acceptance_and_grade_repair",
                "lock_legion_boundary",
                "validation",
                "combo_consistency",
                "accepted_repair_rows",
                "rerun_ready",
                "runtime_boundary",
            },
            set(checks),
        )
        self.assertTrue(all(item["passed"] for item in checks.values()))

    def test_runtime_fixture_is_fixture_only_with_kagero_counts(self):
        fixture = self.report["runtime_fixture"]

        self.assertEqual("deck_recipe_runtime_fixture_v1", fixture["schema_version"])
        self.assertEqual("offline_runtime_test_fixture", fixture["fixture_scope"])
        self.assertEqual("m64_recipe_001", fixture["recipe_id"])
        self.assertEqual("runtime_fixture_m64_recipe_001_kagero_m65_06", fixture["fixture_id"])
        self.assertEqual(50, sum(row["quantity"] for row in fixture["main_deck"]))
        self.assertEqual(50, fixture["count_summary"]["main_deck_count"])
        self.assertEqual(16, fixture["count_summary"]["trigger_count"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, fixture["count_summary"]["trigger_counts"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, fixture["count_summary"]["grade_counts"])
        self.assertEqual(0, fixture["count_summary"]["grade4_main_deck_count"])
        self.assertEqual({"คาเงโร่": 50}, fixture["count_summary"]["clan_counts"])
        self.assertEqual(SELECTED_REVIEW_ITEM_ID, fixture["source_packages"]["selected_review_item_id"])
        self.assertEqual("m64_recipe_001_accepted_grade_pkg_001", fixture["source_packages"]["accepted_grade_repair_package_id"])
        self.assertEqual(LOCK_OPTION, fixture["source_packages"]["selected_lock_option_id"])
        self.assertEqual(LEGION_OPTION, fixture["source_packages"]["selected_legion_option_id"])

    def test_fixture_keeps_lock_legion_runtime_disabled(self):
        fixture = self.report["runtime_fixture"]
        system = fixture["system_boundaries"]
        runtime = fixture["runtime_boundaries"]

        self.assertEqual(LOCK_OPTION, system["selected_lock_option_id"])
        self.assertEqual(LEGION_OPTION, system["selected_legion_option_id"])
        self.assertTrue(system["main_deck_only_validation_allowed"])
        self.assertTrue(system["lock_boundary_applied"])
        self.assertTrue(system["legion_boundary_applied"])
        self.assertFalse(system["lock_runtime_enabled"])
        self.assertFalse(system["unlock_runtime_enabled"])
        self.assertFalse(system["locked_card_visibility_enabled"])
        self.assertFalse(system["lock_circle_state_enabled"])
        self.assertFalse(system["unlock_timing_runtime_enabled"])
        self.assertFalse(system["legion_runtime_enabled"])
        self.assertFalse(system["mate_identity_check_enabled"])
        self.assertFalse(system["legion_state_enabled"])
        self.assertFalse(system["legion_attack_timing_enabled"])
        self.assertFalse(system["legion_deck_building_validation_enabled"])
        self.assertTrue(runtime["test_fixture_only"])
        self.assertFalse(runtime["auto_injected_into_player_decks"])
        self.assertFalse(runtime["bot_playbook_enabled"])
        self.assertFalse(runtime["ui_deck_library_mutated"])
        self.assertFalse(runtime["game_state_mutated"])
        self.assertFalse(runtime["lock_runtime_enabled"])
        self.assertFalse(runtime["unlock_runtime_enabled"])
        self.assertFalse(runtime["legion_runtime_enabled"])
        self.assertFalse(runtime["mate_identity_check_enabled"])

    def test_gate_blocks_when_lock_boundary_is_not_main_deck_only(self):
        validation = copy.deepcopy(self.validation_report)
        validation["system_decision_context"]["selected_lock_option_id"] = "defer_until_lock_unlock_runtime_exists"
        validation["system_decision_context"]["main_deck_only_boundary_applied"] = False
        validation["system_decision_context"]["lock_boundary_applied"] = False

        report = build_eighth_slice_runtime_fixture_promotion_gate(self.accepted_artifact, validation)

        self.assertFalse(report["summary"]["promotion_allowed"])
        self.assertEqual(1, report["summary"]["failed_check_count"])
        self.assertFalse(report["promotion_decision"]["fixture_created"])
        self.assertIsNone(report["runtime_fixture"])
        checks = {item["check_id"]: item for item in report["gate_checks"]}
        self.assertFalse(checks["lock_legion_boundary"]["passed"])

    def test_gate_blocks_when_legion_boundary_is_not_main_deck_only(self):
        validation = copy.deepcopy(self.validation_report)
        validation["system_decision_context"]["selected_legion_option_id"] = "defer_until_legion_mate_runtime_exists"
        validation["system_decision_context"]["main_deck_only_boundary_applied"] = False
        validation["system_decision_context"]["legion_boundary_applied"] = False

        report = build_eighth_slice_runtime_fixture_promotion_gate(self.accepted_artifact, validation)

        self.assertFalse(report["summary"]["promotion_allowed"])
        self.assertEqual(1, report["summary"]["failed_check_count"])
        self.assertFalse(report["promotion_decision"]["fixture_created"])
        self.assertIsNone(report["runtime_fixture"])
        checks = {item["check_id"]: item for item in report["gate_checks"]}
        self.assertFalse(checks["lock_legion_boundary"]["passed"])

    def test_gate_blocks_when_repaired_rows_do_not_match_validation_recipe(self):
        accepted = copy.deepcopy(self.accepted_artifact)
        accepted["summary"]["selected_recipe_id"] = "wrong_recipe"

        report = build_eighth_slice_runtime_fixture_promotion_gate(accepted, self.validation_report)

        self.assertFalse(report["summary"]["promotion_allowed"])
        self.assertEqual(1, report["summary"]["failed_check_count"])
        checks = {item["check_id"]: item for item in report["gate_checks"]}
        self.assertFalse(checks["accepted_repair_rows"]["passed"])

    def test_scope_does_not_mutate_runtime_ui_bot_or_system_runtime(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_promotion_gate"])
        self.assertTrue(scope["creates_runtime_test_fixture_artifact"])
        self.assertFalse(scope["mutates_runtime_deck_library"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_playbook_promotion"])
        self.assertFalse(scope["live_card_text_parsing"])
        self.assertFalse(scope["GameState_mutation"])
        self.assertFalse(scope["lock_runtime_enabled"])
        self.assertFalse(scope["unlock_runtime_enabled"])
        self.assertFalse(scope["legion_runtime_enabled"])
        self.assertFalse(scope["mate_identity_check_enabled"])

    def test_next_target_is_m65_closeout(self):
        next_target = self.report["next_target"]

        self.assertEqual("M65-closeout", next_target["milestone"])
        self.assertEqual("Eighth-slice fixture closeout", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m65_06.json"
            md_path = out / "m65_06.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M65-06", loaded["version"])
            self.assertTrue(loaded["summary"]["promotion_allowed"])
            self.assertIn("M65-06 Eighth-Slice Runtime Fixture Promotion Gate", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
