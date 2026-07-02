"""Tests for tools/deck/build_eighth_slice_human_repair_review_packet.py (M65-01)."""

from __future__ import annotations

import copy
import csv
import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import tests.test_eighth_slice_runtime_readiness_closeout as closeout_fixture  # noqa: E402
from tools.deck.build_eighth_slice_human_repair_review_packet import (  # noqa: E402
    build_eighth_slice_human_repair_review_packet,
    write_csv,
    write_json,
    write_markdown,
)


class TestEighthSliceHumanRepairReviewPacket(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        closeout_fixture.TestEighthSliceRuntimeReadinessCloseout.setUpClass()
        cls.closeout = closeout_fixture.TestEighthSliceRuntimeReadinessCloseout.report
        cls.repairs = closeout_fixture.TestEighthSliceRuntimeReadinessCloseout.repairs
        cls.drafts = closeout_fixture.TestEighthSliceRuntimeReadinessCloseout.drafts
        cls.report = build_eighth_slice_human_repair_review_packet(
            cls.closeout,
            cls.repairs,
            cls.drafts,
        )

    def test_packet_exports_all_eighth_slice_repair_candidates(self):
        summary = self.report["summary"]

        self.assertEqual("M65-01", self.report["version"])
        self.assertEqual(25, summary["review_item_count"])
        self.assertEqual(25, summary["ready_for_human_repair_review_count"])
        self.assertEqual(25, summary["human_selection_candidate_count"])
        self.assertEqual(25, summary["complete_grade_profile_candidate_count"])
        self.assertEqual(0, summary["grade_profile_not_needed_count"])
        self.assertEqual(0, summary["manual_overlap_item_count"])
        self.assertEqual(25, summary["lock_deferred_item_count"])
        self.assertEqual(25, summary["legion_deferred_item_count"])
        self.assertEqual(0, summary["unexpected_structural_blocker_item_count"])
        self.assertTrue(summary["human_selection_review_allowed"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertEqual(4, summary["decision_option_count"])
        self.assertEqual(2, summary["lock_decision_option_count"])
        self.assertEqual(2, summary["legion_decision_option_count"])
        self.assertTrue(summary["ready_for_m65_02"])

    def test_first_item_carries_pair_selection_grade_lock_and_legion_context(self):
        item = self.report["review_items"][0]
        pair = item["pair"]

        self.assertEqual("m65_01_m64_recipe_001_repair_review", item["review_item_id"])
        self.assertEqual("eighth_slice_human_grade_lock_legion_review", item["item_type"])
        self.assertEqual("m64_recipe_001", item["recipe_id"])
        self.assertEqual("BT11-005TH->EB09-025TH", item["source_candidate_edge"])
        self.assertEqual(1, item["source_edge_rank"])
        self.assertEqual("BT11-005TH", pair["source_card_id"])
        self.assertEqual("EB09-025TH", pair["target_card_id"])
        self.assertEqual(16, pair["net_score"])
        self.assertEqual("not_resource_relevant", pair["resource_verdict"])
        self.assertEqual("timing_can_precede", pair["timing_verdict"])
        self.assertEqual("target_zone_need_not_supported_by_source", pair["zone_verdict"])
        self.assertEqual("m64_recipe_001_human_selection_pkg_001", item["human_selection_package_id"])
        self.assertEqual("m64_recipe_001_grade_profile_pkg_001", item["grade_profile_package_id"])
        self.assertEqual("m64_recipe_001_lock_deferred_pkg_001", item["lock_package_id"])
        self.assertEqual("m64_recipe_001_legion_deferred_pkg_001", item["legion_package_id"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, item["grade_counts_after"])
        self.assertEqual("BT11-068TH", item["grade_additions"][0]["card_id"])
        self.assertEqual("BT11-011TH", item["grade_removals"][0]["card_id"])

    def test_each_item_requires_human_decision_and_stays_non_runtime(self):
        for item in self.report["review_items"]:
            self.assertEqual("validator_passed_pending_human_selection", item["validation_status"])
            self.assertEqual("consistent_pending_human_selection", item["consistency_status"])
            self.assertEqual([], item["structural_blockers"])
            self.assertEqual([], item["manual_review_card_ids"])
            self.assertTrue(item["human_selection_required"])
            self.assertFalse(item["human_selection_records_choice"])
            self.assertTrue(item["human_selection_pair_cards_present"])
            self.assertTrue(item["grade_repair_complete"])
            self.assertFalse(item["grade_repair_not_needed"])
            self.assertTrue(item["lock_deferred"])
            self.assertTrue(item["legion_deferred"])
            self.assertFalse(item["lock_can_be_repaired_in_m65_01"])
            self.assertFalse(item["legion_can_be_repaired_in_m65_01"])
            self.assertTrue(item["ready_for_human_repair_review"])
            self.assertTrue(item["human_decision_required"])
            self.assertFalse(item["runtime_promotion_allowed"])
            self.assertEqual(item["grade_added_card_count"], item["grade_removed_card_count"])
            self.assertEqual(
                "select_one_recipe_review_grade_repair_and_record_lock_legion_decisions_before_m65_02",
                item["recommended_reviewer_action"],
            )

    def test_grade_packages_are_complete_source_backed_previews(self):
        target = {"0": 17, "1": 14, "2": 11, "3": 8}

        for item in self.report["review_items"]:
            self.assertEqual("grade_profile_substitution_preview", item["grade_repair_type"])
            self.assertEqual(target, item["target_grade_counts"])
            self.assertNotEqual(target, item["grade_counts_before"])
            self.assertEqual(target, item["grade_counts_after"])
            self.assertTrue(item["grade_additions"])
            self.assertTrue(item["grade_removals"])
            for card in item["grade_additions"]:
                self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", card["source"])
                self.assertIn(card["grade"], target)
            for card in item["grade_removals"]:
                self.assertEqual("m64_03_eighth_slice_recipe_draft_model", card["source"])

    def test_lock_and_legion_decision_context_is_deferred_only(self):
        for item in self.report["review_items"]:
            self.assertIn("Lock/Unlock runtime rules module", item["lock_future_system_work"])
            self.assertIn("Legion/Mate declaration rules", item["legion_future_system_work"])
            self.assertFalse(item["lock_can_be_repaired_in_m65_01"])
            self.assertFalse(item["legion_can_be_repaired_in_m65_01"])

    def test_decision_options_do_not_record_selection_acceptance_or_system_decisions(self):
        item = self.report["review_items"][0]
        option_ids = [option["option_id"] for option in item["decision_options"]]
        lock_option_ids = [option["option_id"] for option in item["lock_decision_options"]]
        legion_option_ids = [option["option_id"] for option in item["legion_decision_options"]]

        self.assertEqual(
            [
                "accept_recipe_grade_repair_keep_lock_legion_deferred_for_validation_rerun",
                "accept_recipe_main_deck_review_no_runtime_promotion",
                "request_different_grade_repair_or_system_scope",
                "reject_recipe_runtime_candidate",
            ],
            option_ids,
        )
        self.assertEqual(
            ["defer_until_lock_unlock_runtime_exists", "main_deck_only_review_no_runtime_promotion"],
            lock_option_ids,
        )
        self.assertEqual(
            ["defer_until_legion_mate_runtime_exists", "main_deck_only_review_no_runtime_promotion"],
            legion_option_ids,
        )

    def test_scope_and_policy_do_not_record_selection_acceptance_or_system_decisions(self):
        scope = self.report["scope"]
        policy = self.report["review_policy"]

        self.assertTrue(scope["offline_human_review_packet"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["records_human_selection"])
        self.assertFalse(scope["records_lock_decision"])
        self.assertFalse(scope["records_legion_decision"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["lock_runtime"])
        self.assertFalse(scope["legion_runtime"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertTrue(policy["human_decision_required"])
        self.assertTrue(policy["exactly_one_recipe_should_be_selected_later"])
        self.assertTrue(policy["packet_is_not_acceptance"])
        self.assertTrue(policy["packet_is_not_selection"])
        self.assertTrue(policy["packet_is_not_lock_decision"])
        self.assertTrue(policy["packet_is_not_legion_decision"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertTrue(policy["m65_02_must_record_explicit_selection"])
        self.assertTrue(policy["m65_03_must_record_explicit_grade_acceptance"])
        self.assertTrue(policy["m65_04_must_record_lock_legion_decision"])
        self.assertTrue(policy["m65_05_must_rerun_validation"])

    def test_missing_closeout_gate_blocks_m65_02_readiness(self):
        closeout = copy.deepcopy(self.closeout)
        closeout["summary"]["human_selection_review_allowed"] = False

        report = build_eighth_slice_human_repair_review_packet(
            closeout,
            self.repairs,
            self.drafts,
        )

        self.assertFalse(report["summary"]["human_selection_review_allowed"])
        self.assertFalse(report["summary"]["ready_for_m65_02"])

    def test_next_target_is_m65_02(self):
        next_target = self.report["next_target"]

        self.assertEqual("M65-02", next_target["milestone"])
        self.assertEqual("Eighth-slice human-selected recipe artifact", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m65_01.json"
            md_path = out / "m65_01.md"
            csv_path = out / "m65_01.csv"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)
            write_csv(self.report, csv_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M65-01", loaded["version"])
            self.assertIn("M65-01 Eighth-Slice Human Repair Review Packet", md_path.read_text(encoding="utf-8"))
            rows = list(csv.DictReader(csv_path.read_text(encoding="utf-8").splitlines()))
            self.assertEqual(25, len(rows))
            self.assertEqual("m64_recipe_001", rows[0]["recipe_id"])


if __name__ == "__main__":
    unittest.main()
