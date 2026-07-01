"""Tests for tools/deck/build_second_slice_human_repair_review_packet.py (M41-01)."""

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

from tools.deck.build_second_slice_human_repair_review_packet import (  # noqa: E402
    M40_CLOSEOUT,
    M40_DRAFTS,
    M40_REPAIRS,
    build_second_slice_human_repair_review_packet,
    load_json,
    write_csv,
    write_json,
    write_markdown,
)


class TestSecondSliceHumanRepairReviewPacket(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.closeout = load_json(M40_CLOSEOUT)
        cls.repairs = load_json(M40_REPAIRS)
        cls.drafts = load_json(M40_DRAFTS)
        cls.report = build_second_slice_human_repair_review_packet(
            cls.closeout,
            cls.repairs,
            cls.drafts,
        )

    def test_packet_exports_all_repair_candidates(self):
        summary = self.report["summary"]

        self.assertEqual("M41-01", self.report["version"])
        self.assertEqual(25, summary["review_item_count"])
        self.assertEqual(25, summary["ready_for_human_repair_review_count"])
        self.assertEqual(25, summary["complete_grade_profile_candidate_count"])
        self.assertEqual(25, summary["manual_overlap_recipe_count"])
        self.assertTrue(summary["human_repair_review_allowed"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertTrue(summary["ready_for_m41_02"])

    def test_first_item_carries_pair_and_repair_package_context(self):
        item = self.report["review_items"][0]
        pair = item["pair"]

        self.assertEqual("m41_01_m40_recipe_001_repair_review", item["review_item_id"])
        self.assertEqual("second_slice_repair_review", item["item_type"])
        self.assertEqual("m40_recipe_001", item["recipe_id"])
        self.assertEqual("BT01-006TH->BT02-033TH", item["source_candidate_edge"])
        self.assertEqual("BT01-006TH", pair["source_card_id"])
        self.assertEqual("CEO อามาเทราสึ", pair["source_name_th"])
        self.assertEqual("BT02-033TH", pair["target_card_id"])
        self.assertEqual("ลัค เบิร์ด", pair["target_name_th"])
        self.assertEqual("m40_recipe_001_grade_profile_pkg_001", item["grade_profile_package_id"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, item["grade_counts_after"])

    def test_each_item_requires_human_decision_and_stays_non_runtime(self):
        for item in self.report["review_items"]:
            self.assertEqual("blocked_by_manual_review", item["validation_status"])
            self.assertEqual("blocked_by_manual_review", item["consistency_status"])
            self.assertEqual(["BT07-096TH", "BT09-068TH"], item["manual_review_card_ids"])
            self.assertTrue(item["complete_candidate"])
            self.assertTrue(item["manual_overlap_cleared_by_grade_package"])
            self.assertTrue(item["ready_for_human_repair_review"])
            self.assertTrue(item["human_decision_required"])
            self.assertFalse(item["runtime_promotion_allowed"])
            self.assertEqual(20, item["added_card_count"])
            self.assertEqual(20, item["removed_card_count"])

    def test_decision_options_do_not_promote_runtime(self):
        item = self.report["review_items"][0]
        option_ids = [option["option_id"] for option in item["decision_options"]]

        self.assertEqual(
            [
                "accept_repair_for_validation_rerun",
                "request_different_repair",
                "reject_recipe_runtime_candidate",
            ],
            option_ids,
        )
        self.assertEqual(
            "review_grade_profile_package_before_m41_02_acceptance",
            item["recommended_reviewer_action"],
        )

    def test_scope_and_policy_do_not_record_acceptance(self):
        scope = self.report["scope"]
        policy = self.report["review_policy"]

        self.assertTrue(scope["offline_human_review_packet"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertTrue(policy["human_decision_required"])
        self.assertTrue(policy["packet_is_not_acceptance"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertTrue(policy["m41_02_must_record_explicit_acceptance"])
        self.assertTrue(policy["m41_03_must_rerun_validation"])

    def test_missing_closeout_gate_blocks_m41_02_readiness(self):
        closeout = copy.deepcopy(self.closeout)
        closeout["summary"]["human_repair_review_allowed"] = False

        report = build_second_slice_human_repair_review_packet(
            closeout,
            self.repairs,
            self.drafts,
        )

        self.assertFalse(report["summary"]["human_repair_review_allowed"])
        self.assertFalse(report["summary"]["ready_for_m41_02"])

    def test_next_target_is_m41_02(self):
        next_target = self.report["next_target"]

        self.assertEqual("M41-02", next_target["milestone"])
        self.assertEqual("Second-slice human-accepted repair artifact", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m41_01.json"
            md_path = out / "m41_01.md"
            csv_path = out / "m41_01.csv"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)
            write_csv(self.report, csv_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M41-01", loaded["version"])
            self.assertIn("M41-01 Second-Slice Human Repair Review Packet", md_path.read_text(encoding="utf-8"))
            rows = list(csv.DictReader(csv_path.read_text(encoding="utf-8").splitlines()))
            self.assertEqual(25, len(rows))
            self.assertEqual("m40_recipe_001", rows[0]["recipe_id"])


if __name__ == "__main__":
    unittest.main()
