"""Tests for tools/deck/build_ninth_slice_runtime_fixture_promotion_gate.py (M69-06)."""

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

import tests.test_ninth_slice_repaired_recipe_validation as validation_fixture  # noqa: E402
from tools.deck.build_ninth_slice_runtime_fixture_promotion_gate import (  # noqa: E402
    AQUA_FORCE_OPTION,
    G_ZONE_OPTION,
    STRIDE_OPTION,
    build_ninth_slice_runtime_fixture_promotion_gate,
    write_json,
    write_markdown,
)


SELECTED_REVIEW_ITEM_ID = "m69_01_m68_recipe_001_repair_review"


class TestNinthSliceRuntimeFixturePromotionGate(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        validation_fixture.TestNinthSliceRepairedRecipeValidation.setUpClass()
        cls.accepted_artifact = validation_fixture.TestNinthSliceRepairedRecipeValidation.accepted_artifact
        cls.validation_report = validation_fixture.TestNinthSliceRepairedRecipeValidation.report
        cls.report = build_ninth_slice_runtime_fixture_promotion_gate(
            cls.accepted_artifact,
            cls.validation_report,
        )

    def test_gate_allows_fixture_when_all_checks_pass(self):
        summary = self.report["summary"]
        decision = self.report["promotion_decision"]

        self.assertEqual("M69-06", self.report["version"])
        self.assertEqual("m68_recipe_001", summary["recipe_id"])
        self.assertEqual(SELECTED_REVIEW_ITEM_ID, summary["selected_review_item_id"])
        self.assertEqual(G_ZONE_OPTION, summary["selected_g_zone_option_id"])
        self.assertEqual(STRIDE_OPTION, summary["selected_stride_option_id"])
        self.assertEqual(AQUA_FORCE_OPTION, summary["selected_aqua_force_option_id"])
        self.assertTrue(summary["promotion_allowed"])
        self.assertEqual(7, summary["passed_check_count"])
        self.assertEqual(0, summary["failed_check_count"])
        self.assertTrue(summary["fixture_created"])
        self.assertTrue(summary["ready_for_m69_closeout"])
        self.assertTrue(decision["fixture_created"])
        self.assertFalse(decision["runtime_deck_library_mutated"])
        self.assertFalse(decision["saved_deck_injected"])
        self.assertFalse(decision["ui_deck_list_published"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertFalse(decision["g_zone_runtime_enabled"])
        self.assertFalse(decision["stride_runtime_enabled"])
        self.assertFalse(decision["aqua_force_battle_order_runtime_enabled"])

    def test_gate_checks_have_required_categories(self):
        checks = {item["check_id"]: item for item in self.report["gate_checks"]}

        self.assertEqual(
            {
                "human_selection_acceptance_and_repair",
                "g_zone_stride_aqua_boundary",
                "validation",
                "combo_consistency",
                "accepted_repair_rows",
                "rerun_ready",
                "runtime_boundary",
            },
            set(checks),
        )
        self.assertTrue(all(item["passed"] for item in checks.values()))

    def test_runtime_fixture_is_fixture_only_with_aqua_force_counts(self):
        fixture = self.report["runtime_fixture"]

        self.assertEqual("deck_recipe_runtime_fixture_v1", fixture["schema_version"])
        self.assertEqual("offline_runtime_test_fixture", fixture["fixture_scope"])
        self.assertEqual("m68_recipe_001", fixture["recipe_id"])
        self.assertEqual("runtime_fixture_m68_recipe_001_aqua_force_m69_06", fixture["fixture_id"])
        self.assertEqual(50, sum(row["quantity"] for row in fixture["main_deck"]))
        self.assertEqual(50, fixture["count_summary"]["main_deck_count"])
        self.assertEqual(16, fixture["count_summary"]["trigger_count"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, fixture["count_summary"]["trigger_counts"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, fixture["count_summary"]["grade_counts"])
        self.assertEqual(0, fixture["count_summary"]["grade4_main_deck_count"])
        self.assertEqual({"อควอฟอร์ซ": 50}, fixture["count_summary"]["clan_counts"])
        self.assertEqual({"G-BT02": 30, "G-CB02": 12, "G-TD04": 8}, fixture["count_summary"]["series_counts"])
        self.assertEqual(SELECTED_REVIEW_ITEM_ID, fixture["source_packages"]["selected_review_item_id"])
        self.assertEqual(
            "m68_recipe_001_combined_manual_grade_pkg_001",
            fixture["source_packages"]["accepted_combined_repair_package_id"],
        )
        self.assertEqual(G_ZONE_OPTION, fixture["source_packages"]["selected_g_zone_option_id"])
        self.assertEqual(STRIDE_OPTION, fixture["source_packages"]["selected_stride_option_id"])
        self.assertEqual(AQUA_FORCE_OPTION, fixture["source_packages"]["selected_aqua_force_option_id"])

    def test_fixture_keeps_g_zone_stride_and_aqua_runtime_disabled(self):
        fixture = self.report["runtime_fixture"]
        system = fixture["system_boundaries"]
        runtime = fixture["runtime_boundaries"]

        self.assertEqual(G_ZONE_OPTION, system["selected_g_zone_option_id"])
        self.assertEqual(STRIDE_OPTION, system["selected_stride_option_id"])
        self.assertEqual(AQUA_FORCE_OPTION, system["selected_aqua_force_option_id"])
        self.assertTrue(system["main_deck_only_validation_allowed"])
        self.assertTrue(system["g_zone_boundary_applied"])
        self.assertTrue(system["stride_boundary_applied"])
        self.assertTrue(system["aqua_force_boundary_applied"])
        self.assertFalse(system["g_zone_runtime_enabled"])
        self.assertFalse(system["stride_runtime_enabled"])
        self.assertFalse(system["aqua_force_battle_order_runtime_enabled"])
        self.assertFalse(system["g_zone_slot_model_enabled"])
        self.assertFalse(system["stride_deck_building_validation_enabled"])
        self.assertFalse(system["generation_break_runtime_enabled"])
        self.assertFalse(system["battle_count_tracker_enabled"])
        self.assertFalse(system["attack_order_predicate_runtime_enabled"])
        self.assertFalse(system["multi_attack_label_runtime_enabled"])
        self.assertFalse(system["grade4_main_deck_allowed"])
        self.assertFalse(system["g_units_allowed_in_main_deck"])
        self.assertTrue(runtime["test_fixture_only"])
        self.assertFalse(runtime["auto_injected_into_player_decks"])
        self.assertFalse(runtime["saved_deck_injection"])
        self.assertFalse(runtime["bot_playbook_enabled"])
        self.assertFalse(runtime["ui_deck_library_mutated"])
        self.assertFalse(runtime["game_state_mutated"])
        self.assertFalse(runtime["g_zone_runtime_enabled"])
        self.assertFalse(runtime["stride_runtime_enabled"])
        self.assertFalse(runtime["aqua_force_battle_order_runtime_enabled"])

    def test_gate_blocks_when_g_zone_boundary_is_not_main_deck_only(self):
        validation = copy.deepcopy(self.validation_report)
        validation["system_decision_context"]["selected_g_zone_option_id"] = "defer_until_g_zone_runtime_exists"
        validation["system_decision_context"]["main_deck_only_boundary_applied"] = False
        validation["system_decision_context"]["g_zone_boundary_applied"] = False
        validation["summary"]["ready_for_m69_06"] = False

        report = build_ninth_slice_runtime_fixture_promotion_gate(self.accepted_artifact, validation)

        self.assertFalse(report["summary"]["promotion_allowed"])
        self.assertEqual(2, report["summary"]["failed_check_count"])
        self.assertFalse(report["promotion_decision"]["fixture_created"])
        self.assertIsNone(report["runtime_fixture"])
        checks = {item["check_id"]: item for item in report["gate_checks"]}
        self.assertFalse(checks["g_zone_stride_aqua_boundary"]["passed"])
        self.assertFalse(checks["rerun_ready"]["passed"])

    def test_gate_blocks_when_validation_has_review_codes(self):
        validation = copy.deepcopy(self.validation_report)
        validation["repaired_recipe_validation"]["review_codes"] = ["aqua_force_battle_order_support_deferred"]

        report = build_ninth_slice_runtime_fixture_promotion_gate(self.accepted_artifact, validation)

        self.assertFalse(report["summary"]["promotion_allowed"])
        self.assertEqual(1, report["summary"]["failed_check_count"])
        self.assertFalse(report["promotion_decision"]["fixture_created"])
        checks = {item["check_id"]: item for item in report["gate_checks"]}
        self.assertFalse(checks["validation"]["passed"])

    def test_gate_blocks_when_repaired_rows_do_not_match_validation_recipe(self):
        accepted = copy.deepcopy(self.accepted_artifact)
        accepted["summary"]["selected_recipe_id"] = "wrong_recipe"

        report = build_ninth_slice_runtime_fixture_promotion_gate(accepted, self.validation_report)

        self.assertFalse(report["summary"]["promotion_allowed"])
        self.assertEqual(1, report["summary"]["failed_check_count"])
        checks = {item["check_id"]: item for item in report["gate_checks"]}
        self.assertFalse(checks["accepted_repair_rows"]["passed"])

    def test_gate_blocks_when_human_acceptance_is_missing(self):
        accepted = copy.deepcopy(self.accepted_artifact)
        validation = copy.deepcopy(self.validation_report)
        accepted["summary"]["human_acceptance_recorded"] = False
        validation["accepted_context"]["human_acceptance_recorded"] = False
        validation["summary"]["ready_for_m69_06"] = False

        report = build_ninth_slice_runtime_fixture_promotion_gate(accepted, validation)

        self.assertFalse(report["summary"]["promotion_allowed"])
        self.assertEqual(2, report["summary"]["failed_check_count"])
        checks = {item["check_id"]: item for item in report["gate_checks"]}
        self.assertFalse(checks["human_selection_acceptance_and_repair"]["passed"])
        self.assertFalse(checks["rerun_ready"]["passed"])

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
        self.assertFalse(scope["g_zone_runtime_enabled"])
        self.assertFalse(scope["stride_runtime_enabled"])
        self.assertFalse(scope["aqua_force_battle_order_runtime_enabled"])

    def test_next_target_is_m69_closeout(self):
        next_target = self.report["next_target"]

        self.assertEqual("M69-closeout", next_target["milestone"])
        self.assertEqual("Ninth-slice fixture closeout", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m69_06.json"
            md_path = out / "m69_06.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M69-06", loaded["version"])
            self.assertTrue(loaded["summary"]["promotion_allowed"])
            self.assertIn("M69-06 Ninth-Slice Runtime Fixture Promotion Gate", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
