"""Tests for tools/deck/build_fourth_slice_human_repair_review_packet.py (M49-01)."""

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

from tools.deck.build_fourth_slice_human_repair_review_packet import (  # noqa: E402
    M48_CLOSEOUT,
    M48_DRAFTS,
    M48_REPAIRS,
    build_fourth_slice_human_repair_review_packet,
    load_json,
    write_csv,
    write_json,
    write_markdown,
)


class TestFourthSliceHumanRepairReviewPacket(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.closeout = load_json(M48_CLOSEOUT)
        cls.repairs = load_json(M48_REPAIRS)
        cls.drafts = load_json(M48_DRAFTS)
        cls.report = build_fourth_slice_human_repair_review_packet(
            cls.closeout,
            cls.repairs,
            cls.drafts,
        )

    def test_packet_exports_all_fourth_slice_repair_candidates(self):
        summary = self.report["summary"]

        self.assertEqual("M49-01", self.report["version"])
        self.assertEqual(25, summary["review_item_count"])
        self.assertEqual(25, summary["ready_for_human_repair_review_count"])
        self.assertEqual(25, summary["complete_manual_repair_count"])
        self.assertEqual(24, summary["complete_grade_profile_candidate_count"])
        self.assertEqual(1, summary["grade_profile_not_needed_count"])
        self.assertEqual(25, summary["g_zone_decision_item_count"])
        self.assertEqual(25, summary["manual_overlap_recipe_count"])
        self.assertTrue(summary["human_g_zone_review_allowed"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertEqual(3, summary["decision_option_count"])
        self.assertEqual(3, summary["g_zone_decision_option_count"])
        self.assertTrue(summary["ready_for_m49_02"])

    def test_first_item_carries_manual_grade_and_g_zone_context(self):
        item = self.report["review_items"][0]
        pair = item["pair"]

        self.assertEqual("m49_01_m48_recipe_001_repair_review", item["review_item_id"])
        self.assertEqual("fourth_slice_repair_and_g_zone_review", item["item_type"])
        self.assertEqual("m48_recipe_001", item["recipe_id"])
        self.assertEqual("G-CMB01-003TH->G-TD02-004TH", item["source_candidate_edge"])
        self.assertEqual("G-CMB01-003TH", pair["source_card_id"])
        self.assertEqual("G-TD02-004TH", pair["target_card_id"])
        self.assertEqual("m48_recipe_001_manual_overlap_pkg_001", item["manual_repair_package_id"])
        self.assertEqual("m48_recipe_001_grade_profile_pkg_001", item["grade_profile_package_id"])
        self.assertEqual("m48_recipe_001_g_zone_deferred_pkg_001", item["g_zone_package_id"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, item["grade_counts_after"])
        self.assertIn("G Zone deck slot model", item["g_zone_requires_future_system_work"])

    def test_each_item_requires_human_and_g_zone_decisions_and_stays_non_runtime(self):
        for item in self.report["review_items"]:
            self.assertEqual("blocked_by_manual_review", item["validation_status"])
            self.assertEqual("blocked_by_manual_review", item["consistency_status"])
            self.assertTrue(item["manual_review_card_ids"])
            self.assertTrue(item["manual_repair_complete"])
            self.assertTrue(item["grade_repair_complete"])
            self.assertTrue(item["g_zone_support_deferred"])
            self.assertTrue(item["g_zone_decision_required"])
            self.assertTrue(item["g_zone_requires_future_system_work"])
            self.assertTrue(item["ready_for_human_repair_review"])
            self.assertTrue(item["human_decision_required"])
            self.assertFalse(item["runtime_promotion_allowed"])
            self.assertEqual(
                "review_main_deck_repair_then_route_to_m49_02_g_zone_decision",
                item["recommended_reviewer_action"],
            )

    def test_manual_substitutions_are_source_backed_previews(self):
        for item in self.report["review_items"]:
            self.assertTrue(item["manual_substitutions"])
            for substitution in item["manual_substitutions"]:
                self.assertTrue(substitution["has_selected_replacement"])
                self.assertTrue(substitution["replacement_card_id"])
                self.assertGreater(substitution["replacement_quantity"], 0)
                self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", substitution["source"])

    def test_grade_packages_are_complete_or_explicitly_not_needed(self):
        target = {"0": 17, "1": 14, "2": 11, "3": 8}
        complete_packages = 0
        not_needed = 0

        for item in self.report["review_items"]:
            self.assertEqual("grade_profile_substitution_preview", item["grade_repair_type"])
            if item["grade_repair_not_needed"]:
                not_needed += 1
                self.assertEqual({}, item["grade_counts_after"])
                continue

            complete_packages += 1
            self.assertEqual(target, item["target_grade_counts"])
            self.assertEqual(target, item["grade_counts_after"])
            self.assertEqual(item["grade_added_card_count"], item["grade_removed_card_count"])
            self.assertGreater(item["grade_added_card_count"], 0)
            for card in item["grade_additions"]:
                self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", card["source"])
            for card in item["grade_removals"]:
                self.assertEqual("m48_03_fourth_slice_recipe_draft_model", card["source"])

        self.assertEqual(24, complete_packages)
        self.assertEqual(1, not_needed)

    def test_decision_options_do_not_record_acceptance_or_g_zone_choice(self):
        item = self.report["review_items"][0]
        option_ids = [option["option_id"] for option in item["decision_options"]]
        g_zone_option_ids = [option["option_id"] for option in item["g_zone_decision_options"]]

        self.assertEqual(
            [
                "accept_main_deck_repair_for_g_zone_decision",
                "request_different_main_deck_repair",
                "reject_recipe_runtime_candidate",
            ],
            option_ids,
        )
        self.assertEqual(
            [
                "main_deck_only_for_current_windows_fixture",
                "defer_recipe_until_g_zone_support",
                "open_g_zone_implementation_queue",
            ],
            g_zone_option_ids,
        )

    def test_scope_and_policy_do_not_record_decisions(self):
        scope = self.report["scope"]
        policy = self.report["review_policy"]

        self.assertTrue(scope["offline_human_review_packet"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["records_g_zone_decision"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertTrue(policy["human_decision_required"])
        self.assertTrue(policy["g_zone_decision_required"])
        self.assertTrue(policy["packet_is_not_acceptance"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertTrue(policy["m49_02_must_record_g_zone_boundary_decision"])
        self.assertTrue(policy["m49_03_must_record_explicit_acceptance"])
        self.assertTrue(policy["m49_04_must_rerun_validation"])

    def test_missing_closeout_gate_blocks_m49_02_readiness(self):
        closeout = copy.deepcopy(self.closeout)
        closeout["summary"]["human_g_zone_review_allowed"] = False

        report = build_fourth_slice_human_repair_review_packet(
            closeout,
            self.repairs,
            self.drafts,
        )

        self.assertFalse(report["summary"]["human_g_zone_review_allowed"])
        self.assertFalse(report["summary"]["ready_for_m49_02"])

    def test_next_target_is_m49_02(self):
        next_target = self.report["next_target"]

        self.assertEqual("M49-02", next_target["milestone"])
        self.assertEqual("Fourth-slice G Zone support decision", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m49_01.json"
            md_path = out / "m49_01.md"
            csv_path = out / "m49_01.csv"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)
            write_csv(self.report, csv_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M49-01", loaded["version"])
            self.assertIn("M49-01 Fourth-Slice Human Repair and G Zone Review Packet", md_path.read_text(encoding="utf-8"))
            rows = list(csv.DictReader(csv_path.read_text(encoding="utf-8").splitlines()))
            self.assertEqual(25, len(rows))
            self.assertEqual("m48_recipe_001", rows[0]["recipe_id"])
            self.assertEqual("4", rows[0]["g_zone_future_work_count"])


if __name__ == "__main__":
    unittest.main()
