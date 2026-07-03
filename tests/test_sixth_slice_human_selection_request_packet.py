"""Tests for tools/deck/build_sixth_slice_human_selection_request_packet.py."""

from __future__ import annotations

import csv
import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_sixth_slice_human_selection_request_packet import (  # noqa: E402
    build_sixth_slice_human_selection_request_packet,
    load_json,
    write_csv,
    write_json,
    write_markdown,
)
from tools.deck.build_sixth_slice_human_selected_recipe_artifact import M57_01_REVIEW  # noqa: E402


class TestSixthSliceHumanSelectionRequestPacket(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.review_packet = load_json(M57_01_REVIEW)
        cls.report = build_sixth_slice_human_selection_request_packet(cls.review_packet)

    def test_request_packet_lists_all_ready_candidates_without_selecting(self):
        summary = self.report["summary"]
        decision = self.report["decision"]

        self.assertEqual("M57-02-prerequisite", self.report["version"])
        self.assertEqual(12, summary["review_item_count"])
        self.assertEqual(12, summary["ready_candidate_count"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertTrue(summary["selection_request_ready"])
        self.assertTrue(summary["ready_for_m57_02"])
        self.assertFalse(summary["human_selection_recorded"])
        self.assertEqual("", decision["selected_review_item_id"])
        self.assertEqual("M57-02", decision["recommended_milestone"])
        self.assertEqual("M57-02", self.report["next_target"]["milestone"])

    def test_candidates_preserve_review_context_and_command_template(self):
        first = self.report["candidates"][0]

        self.assertEqual(1, first["order"])
        self.assertEqual("m57_01_m56_recipe_001_repair_review", first["review_item_id"])
        self.assertEqual("m56_recipe_001", first["recipe_id"])
        self.assertTrue(first["ready_for_user_selection"])
        self.assertEqual("G-BT12-062TH->G-BT12-066TH", first["source_candidate_edge"])
        self.assertEqual("G-BT12-062TH", first["source_card_id"])
        self.assertEqual("G-BT12-066TH", first["target_card_id"])
        self.assertEqual(7, first["manual_substitution_count"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, first["grade_counts_after"])
        self.assertTrue(first["g_zone_deferred"])
        self.assertIn("--review-item-id m57_01_m56_recipe_001_repair_review", first["selection_command_template"])
        self.assertIn("--selection-text", first["selection_command_template"])

    def test_scope_blocks_selection_acceptance_runtime_ui_and_bot(self):
        scope = self.report["scope"]
        policy = self.report["selection_policy"]
        decision = self.report["decision"]

        self.assertTrue(scope["read_only_selection_request"])
        self.assertFalse(scope["records_human_selection"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["records_g_zone_decision"])
        self.assertFalse(scope["creates_m57_02_artifact"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertTrue(policy["requires_user_or_team_to_choose_one_review_item_id"])
        self.assertTrue(policy["requires_non_empty_selection_text"])
        self.assertTrue(policy["tool_must_not_choose_automatically"])
        self.assertFalse(decision["runtime_promotion_allowed"])

    def test_unready_review_packet_routes_to_repair(self):
        review_packet = json.loads(json.dumps(self.review_packet, ensure_ascii=False))
        review_packet["summary"]["ready_for_m57_02"] = False

        report = build_sixth_slice_human_selection_request_packet(review_packet)

        self.assertFalse(report["summary"]["selection_request_ready"])
        self.assertEqual(1, report["summary"]["blocking_issue_count"])
        self.assertEqual("m57_01_not_ready_for_selection", report["issues"][0]["code"])
        self.assertEqual("M57-01-repair", report["next_target"]["milestone"])

    def test_no_ready_items_routes_to_repair(self):
        review_packet = json.loads(json.dumps(self.review_packet, ensure_ascii=False))
        for item in review_packet["review_items"]:
            item["ready_for_human_repair_review"] = False

        report = build_sixth_slice_human_selection_request_packet(review_packet)

        self.assertFalse(report["summary"]["selection_request_ready"])
        self.assertEqual("no_ready_review_items", report["issues"][0]["code"])
        self.assertEqual("M57-01-repair", report["next_target"]["milestone"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "request.json"
            md_path = out / "request.md"
            csv_path = out / "request.csv"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)
            write_csv(self.report, csv_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M57-02-prerequisite", loaded["version"])
            self.assertIn("M57-02 Sixth-Slice Human Selection Request Packet", md_path.read_text(encoding="utf-8"))
            with csv_path.open("r", encoding="utf-8", newline="") as handle:
                rows = list(csv.DictReader(handle))
            self.assertEqual(12, len(rows))
            self.assertEqual("m57_01_m56_recipe_001_repair_review", rows[0]["review_item_id"])


if __name__ == "__main__":
    unittest.main()
