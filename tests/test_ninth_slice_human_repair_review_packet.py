"""Tests for tools/deck/build_ninth_slice_human_repair_review_packet.py (M69-01)."""

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

import tests.test_ninth_slice_runtime_readiness_closeout as closeout_fixture  # noqa: E402
from tools.deck.build_ninth_slice_human_repair_review_packet import (  # noqa: E402
    build_ninth_slice_human_repair_review_packet,
    write_csv,
    write_json,
    write_markdown,
)


class TestNinthSliceHumanRepairReviewPacket(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        closeout_fixture.TestNinthSliceRuntimeReadinessCloseout.setUpClass()
        cls.closeout = closeout_fixture.TestNinthSliceRuntimeReadinessCloseout.report
        cls.repairs = closeout_fixture.TestNinthSliceRuntimeReadinessCloseout.repairs
        cls.drafts = closeout_fixture.TestNinthSliceRuntimeReadinessCloseout.drafts
        cls.report = build_ninth_slice_human_repair_review_packet(
            cls.closeout,
            cls.repairs,
            cls.drafts,
        )

    def test_packet_exports_all_ninth_slice_repair_candidates(self):
        summary = self.report["summary"]

        self.assertEqual("M69-01", self.report["version"])
        self.assertEqual(25, summary["review_item_count"])
        self.assertEqual(25, summary["ready_for_human_repair_review_count"])
        self.assertEqual(25, summary["complete_manual_repair_candidate_count"])
        self.assertEqual(23, summary["complete_grade_profile_candidate_count"])
        self.assertEqual(2, summary["grade_profile_not_needed_count"])
        self.assertEqual(25, summary["g_zone_deferred_item_count"])
        self.assertEqual(25, summary["stride_deferred_item_count"])
        self.assertEqual(25, summary["aqua_force_battle_order_deferred_item_count"])
        self.assertEqual(0, summary["unexpected_structural_blocker_item_count"])
        self.assertTrue(summary["human_selection_review_allowed"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertEqual(4, summary["decision_option_count"])
        self.assertEqual(2, summary["g_zone_decision_option_count"])
        self.assertEqual(2, summary["stride_decision_option_count"])
        self.assertEqual(2, summary["aqua_force_decision_option_count"])
        self.assertTrue(summary["ready_for_m69_02"])

    def test_first_item_carries_pair_manual_grade_g_zone_stride_and_aqua_context(self):
        item = self.report["review_items"][0]
        pair = item["pair"]

        self.assertEqual("m69_01_m68_recipe_001_repair_review", item["review_item_id"])
        self.assertEqual("ninth_slice_human_repair_g_zone_stride_aqua_review", item["item_type"])
        self.assertEqual("m68_recipe_001", item["recipe_id"])
        self.assertEqual("G-BT02-070TH->G-CB02-015TH", item["source_candidate_edge"])
        self.assertEqual(1, item["source_edge_rank"])
        self.assertEqual("G-BT02-070TH", pair["source_card_id"])
        self.assertEqual("G-CB02-015TH", pair["target_card_id"])
        self.assertEqual(14, pair["net_score"])
        self.assertEqual("source_resource_profile_only", pair["resource_verdict"])
        self.assertEqual("timing_can_precede", pair["timing_verdict"])
        self.assertEqual("source_zone_profile_only", pair["zone_verdict"])
        self.assertEqual("m68_recipe_001_manual_overlap_pkg_001", item["manual_overlap_package_id"])
        self.assertEqual("manual_review_overlap_substitution_preview", item["manual_repair_type"])
        self.assertTrue(item["manual_substitutions"])
        self.assertEqual("m68_recipe_001_grade_profile_pkg_001", item["grade_profile_package_id"])
        self.assertEqual("grade_profile_substitution_preview", item["grade_repair_type"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, item["grade_counts_after"])
        self.assertEqual("G-TD04-014TH", item["grade_additions"][0]["card_id"])
        self.assertEqual("G-BT02-030TH", item["grade_removals"][0]["card_id"])
        self.assertEqual("m68_recipe_001_g_zone_deferred_pkg_001", item["g_zone_package_id"])
        self.assertEqual("m68_recipe_001_stride_deferred_pkg_001", item["stride_package_id"])
        self.assertEqual("m68_recipe_001_aqua_force_battle_order_pkg_001", item["aqua_force_battle_order_package_id"])
        self.assertTrue(item["g_zone_deferred"])
        self.assertTrue(item["stride_deferred"])
        self.assertTrue(item["aqua_force_battle_order_deferred"])

    def test_each_item_requires_human_decision_and_stays_non_runtime(self):
        for item in self.report["review_items"]:
            self.assertEqual("blocked_by_manual_review", item["validation_status"])
            self.assertEqual("blocked_by_manual_review", item["consistency_status"])
            self.assertEqual([], item["structural_blockers"])
            self.assertTrue(item["manual_repair_complete"])
            self.assertTrue(item["manual_review_card_ids"])
            self.assertTrue(item["grade_repair_complete"] or item["grade_repair_not_needed"])
            self.assertTrue(item["g_zone_deferred"])
            self.assertTrue(item["stride_deferred"])
            self.assertTrue(item["aqua_force_battle_order_deferred"])
            self.assertFalse(item["g_zone_can_be_repaired_in_m69_01"])
            self.assertFalse(item["stride_can_be_repaired_in_m69_01"])
            self.assertFalse(item["aqua_force_battle_order_can_be_repaired_in_m69_01"])
            self.assertTrue(item["ready_for_human_repair_review"])
            self.assertTrue(item["human_decision_required"])
            self.assertFalse(item["runtime_promotion_allowed"])
            self.assertEqual(item["grade_added_card_count"], item["grade_removed_card_count"])
            self.assertEqual(
                "select_one_recipe_review_manual_grade_repairs_and_record_g_zone_stride_aqua_decisions_before_m69_02",
                item["recommended_reviewer_action"],
            )

    def test_manual_substitutions_are_source_backed(self):
        for item in self.report["review_items"]:
            self.assertTrue(item["manual_substitutions"])
            for substitution in item["manual_substitutions"]:
                self.assertTrue(substitution["remove_card_id"])
                self.assertGreater(substitution["remove_quantity"], 0)
                self.assertTrue(substitution["selected_replacement_card_id"])
                self.assertGreater(substitution["selected_replacement_quantity"], 0)
                self.assertEqual(
                    "data/packs/vanguard_th/cards.sqlite:cards",
                    substitution["selected_replacement_source"],
                )

    def test_grade_packages_are_complete_when_grade_review_exists(self):
        target = {"0": 17, "1": 14, "2": 11, "3": 8}
        repair_count = 0
        not_needed_count = 0

        for item in self.report["review_items"]:
            self.assertEqual("grade_profile_substitution_preview", item["grade_repair_type"])
            if item["grade_repair_not_needed"]:
                not_needed_count += 1
                self.assertFalse(item["grade_repair_complete"])
                self.assertEqual([], item["grade_additions"])
                self.assertEqual([], item["grade_removals"])
                continue
            repair_count += 1
            self.assertEqual(target, item["target_grade_counts"])
            self.assertEqual(target, item["grade_counts_after"])
            self.assertTrue(item["grade_additions"])
            self.assertTrue(item["grade_removals"])
            for card in item["grade_additions"]:
                self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", card["source"])
            for card in item["grade_removals"]:
                self.assertEqual("m68_03_ninth_slice_recipe_draft_model", card["source"])

        self.assertEqual(23, repair_count)
        self.assertEqual(2, not_needed_count)

    def test_deferred_system_work_context_is_preserved_only(self):
        for item in self.report["review_items"]:
            self.assertIn("G Zone deck slot model", item["g_zone_future_system_work"])
            self.assertIn("Stride declaration timing", item["stride_future_system_work"])
            self.assertIn("battle-count tracker", item["aqua_force_battle_order_future_system_work"])
            self.assertFalse(item["g_zone_can_be_repaired_in_m69_01"])
            self.assertFalse(item["stride_can_be_repaired_in_m69_01"])
            self.assertFalse(item["aqua_force_battle_order_can_be_repaired_in_m69_01"])

    def test_decision_options_do_not_record_selection_acceptance_or_system_decisions(self):
        item = self.report["review_items"][0]
        option_ids = [option["option_id"] for option in item["decision_options"]]
        g_zone_option_ids = [option["option_id"] for option in item["g_zone_decision_options"]]
        stride_option_ids = [option["option_id"] for option in item["stride_decision_options"]]
        aqua_option_ids = [option["option_id"] for option in item["aqua_force_decision_options"]]

        self.assertEqual(
            [
                "accept_recipe_repairs_keep_g_zone_stride_aqua_deferred_for_validation_rerun",
                "accept_original_manual_cards_and_keep_advisory",
                "request_different_repair_or_system_scope",
                "reject_recipe_runtime_candidate",
            ],
            option_ids,
        )
        self.assertEqual(
            ["defer_until_g_zone_runtime_exists", "main_deck_only_review_no_runtime_promotion"],
            g_zone_option_ids,
        )
        self.assertEqual(
            ["defer_until_stride_runtime_exists", "main_deck_only_review_no_runtime_promotion"],
            stride_option_ids,
        )
        self.assertEqual(
            [
                "defer_until_aqua_force_battle_order_runtime_exists",
                "manual_semantic_review_only_no_runtime_promotion",
            ],
            aqua_option_ids,
        )

    def test_scope_and_policy_do_not_record_selection_acceptance_or_system_decisions(self):
        scope = self.report["scope"]
        policy = self.report["review_policy"]

        self.assertTrue(scope["offline_human_review_packet"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["records_human_selection"])
        self.assertFalse(scope["records_g_zone_decision"])
        self.assertFalse(scope["records_stride_decision"])
        self.assertFalse(scope["records_aqua_force_battle_order_decision"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["g_zone_runtime"])
        self.assertFalse(scope["stride_runtime"])
        self.assertFalse(scope["aqua_force_battle_order_runtime"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertTrue(policy["human_decision_required"])
        self.assertTrue(policy["exactly_one_recipe_should_be_selected_later"])
        self.assertTrue(policy["packet_is_not_acceptance"])
        self.assertTrue(policy["packet_is_not_selection"])
        self.assertTrue(policy["packet_is_not_g_zone_decision"])
        self.assertTrue(policy["packet_is_not_stride_decision"])
        self.assertTrue(policy["packet_is_not_aqua_force_battle_order_decision"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertTrue(policy["m69_02_must_record_explicit_selection"])
        self.assertTrue(policy["m69_03_must_record_explicit_acceptance"])
        self.assertTrue(policy["m69_04_must_record_g_zone_stride_aqua_decision"])
        self.assertTrue(policy["m69_05_must_rerun_validation"])

    def test_missing_closeout_gate_blocks_m69_02_readiness(self):
        closeout = copy.deepcopy(self.closeout)
        closeout["summary"]["human_selection_review_allowed"] = False

        report = build_ninth_slice_human_repair_review_packet(
            closeout,
            self.repairs,
            self.drafts,
        )

        self.assertFalse(report["summary"]["human_selection_review_allowed"])
        self.assertFalse(report["summary"]["ready_for_m69_02"])

    def test_next_target_is_m69_02(self):
        next_target = self.report["next_target"]

        self.assertEqual("M69-02", next_target["milestone"])
        self.assertEqual("Ninth-slice human-selected recipe artifact", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m69_01.json"
            md_path = out / "m69_01.md"
            csv_path = out / "m69_01.csv"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)
            write_csv(self.report, csv_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M69-01", loaded["version"])
            self.assertIn("M69-01 Ninth-Slice Human Repair Review Packet", md_path.read_text(encoding="utf-8"))
            rows = list(csv.DictReader(csv_path.read_text(encoding="utf-8").splitlines()))
            self.assertEqual(25, len(rows))
            self.assertEqual("m68_recipe_001", rows[0]["recipe_id"])


if __name__ == "__main__":
    unittest.main()
