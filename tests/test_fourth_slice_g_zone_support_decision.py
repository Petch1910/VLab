"""Tests for tools/deck/build_fourth_slice_g_zone_support_decision.py (M49-02)."""

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

from tools.deck.build_fourth_slice_g_zone_support_decision import (  # noqa: E402
    DEFAULT_SELECTED_OPTION,
    M48_CLOSEOUT,
    M49_REVIEW_PACKET,
    build_fourth_slice_g_zone_support_decision,
    load_json,
    write_json,
    write_markdown,
)


class TestFourthSliceGZoneSupportDecision(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.review_packet = load_json(M49_REVIEW_PACKET)
        cls.closeout = load_json(M48_CLOSEOUT)
        cls.report = build_fourth_slice_g_zone_support_decision(
            cls.review_packet,
            cls.closeout,
        )

    def test_default_decision_selects_main_deck_only_boundary(self):
        summary = self.report["summary"]
        decision = self.report["decision"]

        self.assertEqual("M49-02", self.report["version"])
        self.assertEqual(DEFAULT_SELECTED_OPTION, summary["selected_option_id"])
        self.assertEqual("selected_for_main_deck_only_validation", decision["decision_status"])
        self.assertEqual(25, summary["review_item_count"])
        self.assertEqual(25, summary["g_zone_decision_item_count"])
        self.assertEqual(25, summary["decision_item_count"])
        self.assertTrue(summary["main_deck_only_validation_allowed"])
        self.assertFalse(summary["g_zone_runtime_enabled"])
        self.assertFalse(summary["stride_runtime_enabled"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertFalse(summary["human_acceptance_recorded"])
        self.assertTrue(summary["review_packet_ready_for_m49_02"])
        self.assertTrue(summary["closeout_allows_human_g_zone_review"])
        self.assertTrue(summary["ready_for_m49_03"])

    def test_scope_records_boundary_decision_only(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_boundary_decision"])
        self.assertTrue(scope["records_g_zone_decision"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])

    def test_boundary_policy_keeps_g_zone_and_stride_runtime_disabled(self):
        policy = self.report["boundary_policy"]

        self.assertTrue(policy["main_deck_only_validation_allowed"])
        self.assertTrue(policy["boundary_resolves_g_zone_deferred_for_main_deck_validation"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertFalse(policy["g_zone_runtime_enabled"])
        self.assertFalse(policy["stride_runtime_enabled"])
        self.assertFalse(policy["g_zone_slot_model_enabled"])
        self.assertFalse(policy["stride_deck_building_validation_enabled"])
        self.assertFalse(policy["generation_break_runtime_enabled"])
        self.assertFalse(policy["grade4_main_deck_allowed"])
        self.assertFalse(policy["g_units_allowed_in_main_deck"])
        self.assertFalse(policy["g_zone_cards_allowed_in_current_windows_fixture"])
        self.assertTrue(policy["m49_03_must_record_explicit_human_acceptance"])
        self.assertTrue(policy["m49_04_must_rerun_main_deck_validation"])
        self.assertTrue(policy["m49_05_runtime_gate_must_remain_blocked_until_acceptance_and_validation"])

    def test_m49_04_validation_policy_is_main_deck_only(self):
        validation_policy = self.report["m49_04_validation_policy"]

        self.assertEqual("main_deck_only", validation_policy["validation_scope"])
        self.assertEqual(
            ["g_zone_support_deferred"],
            validation_policy["may_suppress_issue_codes_after_boundary_decision"],
        )
        self.assertIn("main_deck_count", validation_policy["must_still_enforce_issue_codes"])
        self.assertIn("manual_review_overlap", validation_policy["must_still_enforce_issue_codes"])
        self.assertTrue(validation_policy["must_keep_grade4_cards_out_of_main_deck"])
        self.assertTrue(validation_policy["must_keep_g_units_out_of_runtime_fixture"])

    def test_each_decision_item_preserves_non_runtime_gate(self):
        for item in self.report["decision_items"]:
            self.assertTrue(item["decision_item_id"].startswith("m49_02_m48_recipe_"))
            self.assertTrue(item["source_review_item_id"].startswith("m49_01_m48_recipe_"))
            self.assertEqual(DEFAULT_SELECTED_OPTION, item["selected_option_id"])
            self.assertEqual("selected_for_main_deck_only_validation", item["decision_status"])
            self.assertTrue(item["main_deck_only_validation_allowed"])
            self.assertTrue(item["boundary_resolves_g_zone_deferred_for_main_deck_validation"])
            self.assertFalse(item["g_zone_runtime_enabled"])
            self.assertFalse(item["stride_runtime_enabled"])
            self.assertFalse(item["grade4_main_deck_allowed"])
            self.assertFalse(item["g_units_allowed_in_main_deck"])
            self.assertFalse(item["runtime_promotion_allowed"])
            self.assertFalse(item["saved_deck_injection"])
            self.assertFalse(item["ui_deck_list_publication"])
            self.assertFalse(item["bot_playbook"])
            self.assertTrue(item["requires_human_acceptance"])
            self.assertTrue(item["requires_validation_rerun"])
            self.assertIn("G Zone deck slot model", item["g_zone_requires_future_system_work"])

    def test_invalid_selected_option_is_rejected(self):
        with self.assertRaises(ValueError):
            build_fourth_slice_g_zone_support_decision(
                self.review_packet,
                self.closeout,
                selected_option="enable_g_zone_runtime_now",
            )

    def test_missing_review_packet_gate_blocks_readiness(self):
        review_packet = copy.deepcopy(self.review_packet)
        review_packet["summary"]["ready_for_m49_02"] = False

        report = build_fourth_slice_g_zone_support_decision(review_packet, self.closeout)

        self.assertFalse(report["summary"]["review_packet_ready_for_m49_02"])
        self.assertFalse(report["summary"]["ready_for_m49_03"])
        self.assertTrue(report["summary"]["main_deck_only_validation_allowed"])
        self.assertFalse(report["summary"]["runtime_promotion_allowed"])

    def test_defer_option_keeps_recipe_advisory_and_blocks_next_acceptance_gate(self):
        report = build_fourth_slice_g_zone_support_decision(
            self.review_packet,
            self.closeout,
            selected_option="defer_recipe_until_g_zone_support",
        )

        self.assertEqual("defer_recipe_until_g_zone_support", report["summary"]["selected_option_id"])
        self.assertFalse(report["summary"]["main_deck_only_validation_allowed"])
        self.assertFalse(report["summary"]["ready_for_m49_03"])
        self.assertEqual("advisory_only", report["m49_04_validation_policy"]["validation_scope"])
        self.assertEqual([], report["m49_04_validation_policy"]["may_suppress_issue_codes_after_boundary_decision"])
        self.assertEqual("M49-closeout", report["next_target"]["milestone"])

    def test_next_target_is_m49_03_for_default_decision(self):
        next_target = self.report["next_target"]

        self.assertEqual("M49-03", next_target["milestone"])
        self.assertEqual("Fourth-slice human-accepted repair artifact", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m49_02.json"
            md_path = out / "m49_02.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M49-02", loaded["version"])
            self.assertEqual(DEFAULT_SELECTED_OPTION, loaded["summary"]["selected_option_id"])
            self.assertIn("M49-02 Fourth-Slice G Zone Support Decision", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
