"""Tests for tools/deck/build_ninth_slice_system_decision_artifact.py (M69-04)."""

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

import tests.test_ninth_slice_human_accepted_repair_artifact as accepted_fixture  # noqa: E402
from tools.deck.build_ninth_slice_human_accepted_repair_artifact import (  # noqa: E402
    build_ninth_slice_human_accepted_repair_artifact,
)
from tools.deck.build_ninth_slice_system_decision_artifact import (  # noqa: E402
    build_ninth_slice_system_decision_artifact,
    write_json,
    write_markdown,
)


G_OPTION = "main_deck_only_review_no_runtime_promotion"
STRIDE_OPTION = "main_deck_only_review_no_runtime_promotion"
AQUA_OPTION = "manual_semantic_review_only_no_runtime_promotion"


class TestNinthSliceSystemDecisionArtifact(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        accepted_fixture.TestNinthSliceHumanAcceptedRepairArtifact.setUpClass()
        cls.selected_artifact = accepted_fixture.TestNinthSliceHumanAcceptedRepairArtifact.selected_artifact
        cls.drafts = accepted_fixture.TestNinthSliceHumanAcceptedRepairArtifact.drafts
        cls.scaffold = accepted_fixture.TestNinthSliceHumanAcceptedRepairArtifact.scaffold
        cls.accepted_artifact = build_ninth_slice_human_accepted_repair_artifact(
            cls.selected_artifact,
            cls.drafts,
            cls.scaffold,
            decision_text=accepted_fixture.DECISION_TEXT,
            repair_decision="accepted",
            decided_by="unit-test",
            decided_at="2026-07-02",
        )
        cls.report = build_ninth_slice_system_decision_artifact(
            cls.accepted_artifact,
            selected_g_zone_option=G_OPTION,
            selected_stride_option=STRIDE_OPTION,
            selected_aqua_force_option=AQUA_OPTION,
        )

    def test_main_deck_and_manual_semantic_decisions_open_validation_but_not_runtime(self):
        summary = self.report["summary"]
        decision = self.report["decision"]

        self.assertEqual("M69-04", self.report["version"])
        self.assertEqual(accepted_fixture.SELECTED_REVIEW_ITEM_ID, summary["selected_review_item_id"])
        self.assertEqual("m68_recipe_001", summary["selected_recipe_id"])
        self.assertEqual("m68_recipe_001_combined_manual_grade_pkg_001", summary["accepted_combined_repair_package_id"])
        self.assertEqual("m68_recipe_001_g_zone_deferred_pkg_001", summary["selected_g_zone_package_id"])
        self.assertEqual("m68_recipe_001_stride_deferred_pkg_001", summary["selected_stride_package_id"])
        self.assertEqual(
            "m68_recipe_001_aqua_force_battle_order_pkg_001",
            summary["selected_aqua_force_battle_order_package_id"],
        )
        self.assertEqual(G_OPTION, summary["selected_g_zone_option_id"])
        self.assertEqual(STRIDE_OPTION, summary["selected_stride_option_id"])
        self.assertEqual(AQUA_OPTION, summary["selected_aqua_force_option_id"])
        self.assertEqual("selected_for_main_deck_only_validation", decision["g_zone"]["decision_status"])
        self.assertEqual("selected_for_main_deck_only_validation", decision["stride"]["decision_status"])
        self.assertEqual(
            "selected_for_manual_semantic_main_deck_validation",
            decision["aqua_force_battle_order"]["decision_status"],
        )
        self.assertEqual("continue_to_m69_05_repaired_validation_rerun", decision["recommendation"])
        self.assertTrue(summary["main_deck_only_validation_allowed"])
        self.assertFalse(summary["g_zone_runtime_enabled"])
        self.assertFalse(summary["stride_runtime_enabled"])
        self.assertFalse(summary["aqua_force_battle_order_runtime_enabled"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertTrue(summary["human_selection_recorded"])
        self.assertTrue(summary["human_acceptance_recorded"])
        self.assertTrue(summary["repair_accepted"])
        self.assertTrue(summary["g_zone_decision_recorded"])
        self.assertTrue(summary["stride_decision_recorded"])
        self.assertTrue(summary["aqua_force_battle_order_decision_recorded"])
        self.assertTrue(summary["accepted_artifact_ready_for_m69_04"])
        self.assertTrue(summary["ready_for_m69_05"])

    def test_scope_records_boundary_only(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_boundary_decision"])
        self.assertFalse(scope["records_human_selection"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["records_repair_acceptance"])
        self.assertTrue(scope["records_g_zone_decision"])
        self.assertTrue(scope["records_stride_decision"])
        self.assertTrue(scope["records_aqua_force_battle_order_decision"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertFalse(scope["declares_recipe_valid"])

    def test_boundary_policy_keeps_runtime_disabled_and_sets_validation_scope(self):
        policy = self.report["boundary_policy"]
        validation = self.report["m69_05_validation_policy"]
        item = self.report["decision_item"]

        self.assertTrue(policy["main_deck_only_validation_allowed"])
        self.assertTrue(policy["boundary_resolves_g_zone_deferred_for_main_deck_validation"])
        self.assertTrue(policy["boundary_resolves_stride_deferred_for_main_deck_validation"])
        self.assertTrue(policy["boundary_resolves_aqua_force_battle_order_deferred_for_main_deck_validation"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertFalse(policy["g_zone_runtime_enabled"])
        self.assertFalse(policy["stride_runtime_enabled"])
        self.assertFalse(policy["aqua_force_battle_order_runtime_enabled"])
        self.assertFalse(policy["g_zone_slot_model_enabled"])
        self.assertFalse(policy["stride_deck_building_validation_enabled"])
        self.assertFalse(policy["generation_break_runtime_enabled"])
        self.assertFalse(policy["battle_count_tracker_enabled"])
        self.assertFalse(policy["attack_order_predicate_runtime_enabled"])
        self.assertFalse(policy["multi_attack_label_runtime_enabled"])
        self.assertFalse(policy["grade4_main_deck_allowed"])
        self.assertFalse(policy["g_units_allowed_in_main_deck"])
        self.assertFalse(policy["g_zone_cards_allowed_in_current_windows_fixture"])
        self.assertTrue(policy["m69_05_must_rerun_main_deck_validation"])
        self.assertTrue(policy["runtime_gate_must_remain_blocked_until_validation"])
        self.assertEqual("main_deck_only", validation["validation_scope"])
        self.assertEqual(
            [
                "g_zone_support_deferred",
                "stride_support_deferred",
                "aqua_force_battle_order_support_deferred",
            ],
            validation["may_suppress_issue_codes_after_boundary_decision"],
        )
        self.assertIn("manual_review_overlap", validation["must_still_enforce_issue_codes"])
        self.assertTrue(validation["must_keep_grade4_cards_out_of_main_deck"])
        self.assertTrue(validation["must_keep_g_units_out_of_runtime_fixture"])
        self.assertTrue(validation["must_not_enable_g_zone_or_stride_runtime"])
        self.assertTrue(validation["must_not_enable_aqua_force_battle_order_runtime"])
        self.assertTrue(validation["must_not_create_runtime_fixture_before_validation"])
        self.assertEqual("m69_04_m68_recipe_001_system_boundary_decision", item["decision_item_id"])
        self.assertFalse(item["g_zone_runtime_enabled"])
        self.assertFalse(item["stride_runtime_enabled"])
        self.assertFalse(item["aqua_force_battle_order_runtime_enabled"])
        self.assertIn("G Zone deck slot model", item["g_zone_future_system_work"])
        self.assertIn("Stride declaration timing", item["stride_future_system_work"])
        self.assertIn("battle-count tracker", item["aqua_force_battle_order_future_system_work"])

    def test_defer_g_zone_option_keeps_recipe_advisory_and_blocks_validation(self):
        report = build_ninth_slice_system_decision_artifact(
            self.accepted_artifact,
            selected_g_zone_option="defer_until_g_zone_runtime_exists",
            selected_stride_option=STRIDE_OPTION,
            selected_aqua_force_option=AQUA_OPTION,
        )

        self.assertEqual("defer_until_g_zone_runtime_exists", report["summary"]["selected_g_zone_option_id"])
        self.assertFalse(report["summary"]["main_deck_only_validation_allowed"])
        self.assertFalse(report["summary"]["ready_for_m69_05"])
        self.assertEqual("advisory_only", report["m69_05_validation_policy"]["validation_scope"])
        self.assertEqual([], report["m69_05_validation_policy"]["may_suppress_issue_codes_after_boundary_decision"])
        self.assertEqual("M69-closeout", report["next_target"]["milestone"])

    def test_defer_stride_option_keeps_recipe_advisory_and_blocks_validation(self):
        report = build_ninth_slice_system_decision_artifact(
            self.accepted_artifact,
            selected_g_zone_option=G_OPTION,
            selected_stride_option="defer_until_stride_runtime_exists",
            selected_aqua_force_option=AQUA_OPTION,
        )

        self.assertEqual("defer_until_stride_runtime_exists", report["summary"]["selected_stride_option_id"])
        self.assertFalse(report["summary"]["main_deck_only_validation_allowed"])
        self.assertFalse(report["summary"]["ready_for_m69_05"])
        self.assertEqual("advisory_only", report["m69_05_validation_policy"]["validation_scope"])
        self.assertEqual("M69-closeout", report["next_target"]["milestone"])

    def test_defer_aqua_force_option_keeps_recipe_advisory_and_blocks_validation(self):
        report = build_ninth_slice_system_decision_artifact(
            self.accepted_artifact,
            selected_g_zone_option=G_OPTION,
            selected_stride_option=STRIDE_OPTION,
            selected_aqua_force_option="defer_until_aqua_force_battle_order_runtime_exists",
        )

        self.assertEqual(
            "defer_until_aqua_force_battle_order_runtime_exists",
            report["summary"]["selected_aqua_force_option_id"],
        )
        self.assertFalse(report["summary"]["main_deck_only_validation_allowed"])
        self.assertFalse(report["summary"]["ready_for_m69_05"])
        self.assertEqual("advisory_only", report["m69_05_validation_policy"]["validation_scope"])
        self.assertEqual("M69-closeout", report["next_target"]["milestone"])

    def test_unsupported_options_are_rejected(self):
        with self.assertRaises(ValueError):
            build_ninth_slice_system_decision_artifact(
                self.accepted_artifact,
                selected_g_zone_option="enable_g_zone_runtime_now",
                selected_stride_option=STRIDE_OPTION,
                selected_aqua_force_option=AQUA_OPTION,
            )

        with self.assertRaises(ValueError):
            build_ninth_slice_system_decision_artifact(
                self.accepted_artifact,
                selected_g_zone_option=G_OPTION,
                selected_stride_option="enable_stride_runtime_now",
                selected_aqua_force_option=AQUA_OPTION,
            )

        with self.assertRaises(ValueError):
            build_ninth_slice_system_decision_artifact(
                self.accepted_artifact,
                selected_g_zone_option=G_OPTION,
                selected_stride_option=STRIDE_OPTION,
                selected_aqua_force_option="enable_aqua_runtime_now",
            )

    def test_options_missing_from_accepted_artifact_are_rejected(self):
        accepted = copy.deepcopy(self.accepted_artifact)
        accepted["accepted_repair"]["g_zone_decision_options"] = []
        with self.assertRaises(ValueError):
            build_ninth_slice_system_decision_artifact(
                accepted,
                selected_g_zone_option=G_OPTION,
                selected_stride_option=STRIDE_OPTION,
                selected_aqua_force_option=AQUA_OPTION,
            )

        accepted = copy.deepcopy(self.accepted_artifact)
        accepted["accepted_repair"]["stride_decision_options"] = []
        with self.assertRaises(ValueError):
            build_ninth_slice_system_decision_artifact(
                accepted,
                selected_g_zone_option=G_OPTION,
                selected_stride_option=STRIDE_OPTION,
                selected_aqua_force_option=AQUA_OPTION,
            )

        accepted = copy.deepcopy(self.accepted_artifact)
        accepted["accepted_repair"]["aqua_force_decision_options"] = []
        with self.assertRaises(ValueError):
            build_ninth_slice_system_decision_artifact(
                accepted,
                selected_g_zone_option=G_OPTION,
                selected_stride_option=STRIDE_OPTION,
                selected_aqua_force_option=AQUA_OPTION,
            )

    def test_not_ready_accepted_artifact_blocks_validation_readiness(self):
        accepted = copy.deepcopy(self.accepted_artifact)
        accepted["summary"]["ready_for_m69_04"] = False

        report = build_ninth_slice_system_decision_artifact(
            accepted,
            selected_g_zone_option=G_OPTION,
            selected_stride_option=STRIDE_OPTION,
            selected_aqua_force_option=AQUA_OPTION,
        )

        self.assertFalse(report["summary"]["accepted_artifact_ready_for_m69_04"])
        self.assertFalse(report["summary"]["ready_for_m69_05"])
        self.assertFalse(report["summary"]["runtime_promotion_allowed"])

    def test_next_target_is_m69_05_for_main_deck_boundary_decisions(self):
        next_target = self.report["next_target"]

        self.assertEqual("M69-05", next_target["milestone"])
        self.assertEqual("Ninth-slice repaired recipe validation rerun", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m69_04.json"
            md_path = out / "m69_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M69-04", loaded["version"])
            self.assertEqual(G_OPTION, loaded["summary"]["selected_g_zone_option_id"])
            self.assertEqual(STRIDE_OPTION, loaded["summary"]["selected_stride_option_id"])
            self.assertEqual(AQUA_OPTION, loaded["summary"]["selected_aqua_force_option_id"])
            self.assertIn(
                "M69-04 Ninth-Slice G Zone / Stride / Aqua Force Decision Artifact",
                md_path.read_text(encoding="utf-8"),
            )


if __name__ == "__main__":
    unittest.main()
