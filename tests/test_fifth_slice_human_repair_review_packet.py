"""Tests for tools/deck/build_fifth_slice_human_repair_review_packet.py (M53-01)."""

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

from tools.deck.build_fifth_slice_human_repair_review_packet import (  # noqa: E402
    M52_CLOSEOUT,
    M52_DRAFTS,
    M52_REPAIRS,
    build_fifth_slice_human_repair_review_packet,
    load_json,
    write_csv,
    write_json,
    write_markdown,
)


class TestFifthSliceHumanRepairReviewPacket(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.closeout = load_json(M52_CLOSEOUT)
        cls.repairs = load_json(M52_REPAIRS)
        cls.drafts = load_json(M52_DRAFTS)
        cls.report = build_fifth_slice_human_repair_review_packet(
            cls.closeout,
            cls.repairs,
            cls.drafts,
        )

    def test_packet_exports_all_fifth_slice_repair_candidates(self):
        summary = self.report["summary"]

        self.assertEqual("M53-01", self.report["version"])
        self.assertEqual(25, summary["review_item_count"])
        self.assertEqual(25, summary["ready_for_human_repair_review_count"])
        self.assertEqual(25, summary["complete_grade_profile_candidate_count"])
        self.assertEqual(25, summary["human_selection_required_count"])
        self.assertEqual(0, summary["unexpected_structural_blocker_item_count"])
        self.assertTrue(summary["human_selection_review_allowed"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertEqual(3, summary["decision_option_count"])
        self.assertTrue(summary["ready_for_m53_02"])

    def test_first_item_carries_pair_and_grade_context(self):
        item = self.report["review_items"][0]
        pair = item["pair"]

        self.assertEqual("m53_01_m52_recipe_001_repair_review", item["review_item_id"])
        self.assertEqual("fifth_slice_human_repair_review", item["item_type"])
        self.assertEqual("m52_recipe_001", item["recipe_id"])
        self.assertEqual("BT14-003TH->BT12-053TH", item["source_candidate_edge"])
        self.assertEqual("BT14-003TH", pair["source_card_id"])
        self.assertEqual("BT12-053TH", pair["target_card_id"])
        self.assertEqual("resource_support", pair["resource_verdict"])
        self.assertEqual("m52_recipe_001_grade_profile_pkg_001", item["grade_profile_package_id"])
        self.assertEqual("grade_profile_substitution_preview", item["grade_repair_type"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, item["grade_counts_after"])

    def test_each_item_requires_human_decision_and_stays_non_runtime(self):
        for item in self.report["review_items"]:
            self.assertEqual("validator_passed_pending_human_selection", item["validation_status"])
            self.assertEqual("consistent_pending_human_selection", item["consistency_status"])
            self.assertEqual([], item["structural_blockers"])
            self.assertTrue(item["grade_repair_complete"])
            self.assertTrue(item["human_selection_required"])
            self.assertTrue(item["ready_for_human_repair_review"])
            self.assertTrue(item["human_decision_required"])
            self.assertFalse(item["runtime_promotion_allowed"])
            self.assertEqual(item["grade_added_card_count"], item["grade_removed_card_count"])
            self.assertGreater(item["grade_added_card_count"], 0)
            self.assertEqual(
                "select_one_recipe_and_review_grade_package_before_m53_02_acceptance",
                item["recommended_reviewer_action"],
            )

    def test_grade_packages_are_complete_source_backed_substitutions(self):
        target = {"0": 17, "1": 14, "2": 11, "3": 8}

        for item in self.report["review_items"]:
            self.assertEqual("grade_profile_substitution_preview", item["grade_repair_type"])
            self.assertEqual(target, item["target_grade_counts"])
            self.assertEqual(target, item["grade_counts_after"])
            self.assertTrue(item["grade_additions"])
            self.assertTrue(item["grade_removals"])
            for card in item["grade_additions"]:
                self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", card["source"])
            for card in item["grade_removals"]:
                self.assertEqual("m52_03_fifth_slice_recipe_draft_model", card["source"])

    def test_decision_options_do_not_record_selection_or_acceptance(self):
        item = self.report["review_items"][0]
        option_ids = [option["option_id"] for option in item["decision_options"]]

        self.assertEqual(
            [
                "accept_recipe_and_grade_repair_for_validation_rerun",
                "request_different_repair",
                "reject_recipe_runtime_candidate",
            ],
            option_ids,
        )

    def test_scope_and_policy_do_not_record_selection_or_acceptance(self):
        scope = self.report["scope"]
        policy = self.report["review_policy"]

        self.assertTrue(scope["offline_human_review_packet"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["records_human_selection"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertTrue(policy["human_decision_required"])
        self.assertTrue(policy["exactly_one_recipe_should_be_selected_later"])
        self.assertTrue(policy["packet_is_not_acceptance"])
        self.assertTrue(policy["packet_is_not_selection"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertTrue(policy["m53_02_must_record_explicit_selection"])
        self.assertTrue(policy["m53_03_must_record_explicit_acceptance"])
        self.assertTrue(policy["m53_04_must_rerun_validation"])

    def test_missing_closeout_gate_blocks_m53_02_readiness(self):
        closeout = copy.deepcopy(self.closeout)
        closeout["summary"]["human_selection_review_allowed"] = False

        report = build_fifth_slice_human_repair_review_packet(
            closeout,
            self.repairs,
            self.drafts,
        )

        self.assertFalse(report["summary"]["human_selection_review_allowed"])
        self.assertFalse(report["summary"]["ready_for_m53_02"])

    def test_next_target_is_m53_02(self):
        next_target = self.report["next_target"]

        self.assertEqual("M53-02", next_target["milestone"])
        self.assertEqual("Fifth-slice human-selected recipe artifact", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m53_01.json"
            md_path = out / "m53_01.md"
            csv_path = out / "m53_01.csv"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)
            write_csv(self.report, csv_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M53-01", loaded["version"])
            self.assertIn("M53-01 Fifth-Slice Human Repair Review Packet", md_path.read_text(encoding="utf-8"))
            rows = list(csv.DictReader(csv_path.read_text(encoding="utf-8").splitlines()))
            self.assertEqual(25, len(rows))
            self.assertEqual("m52_recipe_001", rows[0]["recipe_id"])


if __name__ == "__main__":
    unittest.main()
