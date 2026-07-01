"""Tests for tools/deck/build_third_slice_human_repair_review_packet.py (M45-01)."""

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

from tools.deck.build_third_slice_human_repair_review_packet import (  # noqa: E402
    M44_CLOSEOUT,
    M44_DRAFTS,
    M44_REPAIRS,
    build_third_slice_human_repair_review_packet,
    load_json,
    write_csv,
    write_json,
    write_markdown,
)


class TestThirdSliceHumanRepairReviewPacket(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.closeout = load_json(M44_CLOSEOUT)
        cls.repairs = load_json(M44_REPAIRS)
        cls.drafts = load_json(M44_DRAFTS)
        cls.report = build_third_slice_human_repair_review_packet(
            cls.closeout,
            cls.repairs,
            cls.drafts,
        )

    def test_packet_exports_all_third_slice_repair_candidates(self):
        summary = self.report["summary"]

        self.assertEqual("M45-01", self.report["version"])
        self.assertEqual(25, summary["review_item_count"])
        self.assertEqual(25, summary["ready_for_human_repair_review_count"])
        self.assertEqual(25, summary["complete_manual_repair_count"])
        self.assertEqual(25, summary["complete_grade_profile_candidate_count"])
        self.assertEqual(25, summary["manual_overlap_recipe_count"])
        self.assertTrue(summary["human_repair_review_allowed"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertEqual(3, summary["decision_option_count"])
        self.assertTrue(summary["ready_for_m45_02"])

    def test_first_item_carries_combined_manual_and_grade_context(self):
        item = self.report["review_items"][0]
        pair = item["pair"]

        self.assertEqual("m45_01_m44_recipe_001_repair_review", item["review_item_id"])
        self.assertEqual("third_slice_repair_review", item["item_type"])
        self.assertEqual("m44_recipe_001", item["recipe_id"])
        self.assertEqual("EB10-007TH-B->EB06-023TH", item["source_candidate_edge"])
        self.assertEqual("EB10-007TH-B", pair["source_card_id"])
        self.assertEqual("EB06-023TH", pair["target_card_id"])
        self.assertEqual("m44_recipe_001_manual_overlap_pkg_001", item["manual_repair_package_id"])
        self.assertEqual("m44_recipe_001_grade_profile_pkg_001", item["grade_profile_package_id"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, item["grade_counts_after"])

    def test_each_item_requires_human_decision_and_stays_non_runtime(self):
        for item in self.report["review_items"]:
            self.assertEqual("blocked_by_manual_review", item["validation_status"])
            self.assertEqual("blocked_by_manual_review", item["consistency_status"])
            self.assertTrue(item["manual_review_card_ids"])
            self.assertTrue(item["manual_repair_complete"])
            self.assertTrue(item["grade_repair_complete"])
            self.assertFalse(item["manual_overlap_cleared_by_grade_package"])
            self.assertTrue(item["ready_for_human_repair_review"])
            self.assertTrue(item["human_decision_required"])
            self.assertFalse(item["runtime_promotion_allowed"])
            self.assertEqual(item["grade_added_card_count"], item["grade_removed_card_count"])
            self.assertGreater(item["grade_added_card_count"], 0)
            self.assertEqual(
                "review_combined_manual_and_grade_packages_before_m45_02_acceptance",
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

    def test_grade_packages_are_complete_source_backed_substitutions(self):
        target = {"0": 17, "1": 14, "2": 11, "3": 8}

        for item in self.report["review_items"]:
            self.assertEqual("grade_profile_substitution", item["grade_repair_type"])
            self.assertEqual(target, item["target_grade_counts"])
            self.assertEqual(target, item["grade_counts_after"])
            self.assertTrue(item["grade_additions"])
            self.assertTrue(item["grade_removals"])
            for card in item["grade_additions"]:
                self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", card["source"])
            for card in item["grade_removals"]:
                self.assertEqual("m44_03_third_slice_recipe_draft_model", card["source"])

    def test_decision_options_do_not_record_acceptance(self):
        item = self.report["review_items"][0]
        option_ids = [option["option_id"] for option in item["decision_options"]]

        self.assertEqual(
            [
                "accept_combined_manual_and_grade_repair_for_validation_rerun",
                "request_different_repair",
                "reject_recipe_runtime_candidate",
            ],
            option_ids,
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
        self.assertTrue(policy["m45_02_must_record_explicit_acceptance"])
        self.assertTrue(policy["m45_03_must_rerun_validation"])

    def test_missing_closeout_gate_blocks_m45_02_readiness(self):
        closeout = copy.deepcopy(self.closeout)
        closeout["summary"]["human_repair_review_allowed"] = False

        report = build_third_slice_human_repair_review_packet(
            closeout,
            self.repairs,
            self.drafts,
        )

        self.assertFalse(report["summary"]["human_repair_review_allowed"])
        self.assertFalse(report["summary"]["ready_for_m45_02"])

    def test_next_target_is_m45_02(self):
        next_target = self.report["next_target"]

        self.assertEqual("M45-02", next_target["milestone"])
        self.assertEqual("Third-slice human-accepted repair artifact", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m45_01.json"
            md_path = out / "m45_01.md"
            csv_path = out / "m45_01.csv"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)
            write_csv(self.report, csv_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M45-01", loaded["version"])
            self.assertIn("M45-01 Third-Slice Human Repair Review Packet", md_path.read_text(encoding="utf-8"))
            rows = list(csv.DictReader(csv_path.read_text(encoding="utf-8").splitlines()))
            self.assertEqual(25, len(rows))
            self.assertEqual("m44_recipe_001", rows[0]["recipe_id"])


if __name__ == "__main__":
    unittest.main()
