"""Tests for tools/deck/build_sixth_slice_human_selection_batch_preflight_matrix.py."""

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

from tools.deck.build_sixth_slice_human_selection_batch_preflight_matrix import (  # noqa: E402
    build_sixth_slice_human_selection_batch_preflight_matrix,
    write_csv,
    write_json,
    write_markdown,
)
from tools.deck.build_sixth_slice_human_selection_request_packet import (  # noqa: E402
    build_sixth_slice_human_selection_request_packet,
    load_json,
)
from tools.deck.build_sixth_slice_human_selected_recipe_artifact import M57_01_REVIEW  # noqa: E402


class TestSixthSliceHumanSelectionBatchPreflightMatrix(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.review_packet = load_json(M57_01_REVIEW)
        cls.request_packet = build_sixth_slice_human_selection_request_packet(cls.review_packet)
        cls.report = build_sixth_slice_human_selection_batch_preflight_matrix(
            cls.request_packet,
            cls.review_packet,
        )

    def test_all_ready_candidates_pass_generator_contract_without_selection(self):
        summary = self.report["summary"]

        self.assertEqual("M57-02-batch-preflight-matrix", self.report["version"])
        self.assertEqual(12, summary["candidate_count"])
        self.assertEqual(12, summary["ready_candidate_count"])
        self.assertEqual(12, summary["preflight_executed_count"])
        self.assertEqual(12, summary["preflight_passed_count"])
        self.assertEqual(0, summary["preflight_failed_count"])
        self.assertTrue(summary["all_candidates_pass_preflight"])
        self.assertTrue(summary["request_ready"])
        self.assertFalse(summary["human_selection_recorded"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertTrue(summary["ready_for_user_selection_decision"])
        self.assertEqual("M57-02-user-selection", self.report["next_target"]["milestone"])

    def test_matrix_rows_preserve_candidate_identity_and_contract_flags(self):
        first = self.report["matrix_rows"][0]

        self.assertEqual(1, first["order"])
        self.assertEqual("m57_01_m56_recipe_001_repair_review", first["review_item_id"])
        self.assertEqual("m56_recipe_001", first["recipe_id"])
        self.assertEqual("G-BT12-062TH->G-BT12-066TH", first["source_candidate_edge"])
        self.assertTrue(first["preflight_executed"])
        self.assertTrue(first["preflight_passed"])
        self.assertTrue(first["generator_accepted"])
        self.assertTrue(first["would_record_human_selection"])
        self.assertFalse(first["would_record_human_acceptance"])
        self.assertFalse(first["would_record_g_zone_decision"])
        self.assertFalse(first["would_allow_runtime_promotion"])
        self.assertTrue(first["would_be_ready_for_m57_03"])
        self.assertEqual("m56_recipe_001", first["selected_recipe_id"])
        self.assertEqual("", first["error_code"])
        self.assertEqual("", first["error_message"])

    def test_scope_and_policy_keep_batch_preflight_read_only(self):
        scope = self.report["scope"]
        policy = self.report["selection_policy"]

        self.assertTrue(scope["read_only_batch_preflight"])
        self.assertFalse(scope["records_human_selection"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["records_g_zone_decision"])
        self.assertFalse(scope["creates_m57_02_artifact"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertTrue(policy["must_not_recommend_or_auto_select"])
        self.assertTrue(policy["dry_run_text_is_not_user_selection_text"])
        self.assertTrue(policy["requires_user_or_team_to_choose_one_review_item_id"])
        self.assertTrue(policy["requires_non_empty_selection_text_for_real_m57_02"])

    def test_unready_request_packet_records_blocker(self):
        request_packet = json.loads(json.dumps(self.request_packet, ensure_ascii=False))
        request_packet["summary"]["selection_request_ready"] = False

        report = build_sixth_slice_human_selection_batch_preflight_matrix(
            request_packet,
            self.review_packet,
        )

        self.assertFalse(report["summary"]["ready_for_user_selection_decision"])
        self.assertEqual(1, report["summary"]["blocking_issue_count"])
        self.assertEqual("selection_request_not_ready", report["issues"][0]["code"])
        self.assertEqual("M57-02-prerequisite-repair", report["next_target"]["milestone"])

    def test_unready_candidate_is_counted_as_failed_preflight(self):
        request_packet = json.loads(json.dumps(self.request_packet, ensure_ascii=False))
        request_packet["candidates"][0]["ready_for_user_selection"] = False

        report = build_sixth_slice_human_selection_batch_preflight_matrix(
            request_packet,
            self.review_packet,
        )

        self.assertEqual(11, report["summary"]["preflight_passed_count"])
        self.assertEqual(1, report["summary"]["preflight_failed_count"])
        self.assertFalse(report["summary"]["all_candidates_pass_preflight"])
        self.assertEqual("candidate_preflight_failures", report["issues"][0]["code"])
        self.assertEqual("candidate_not_ready", report["matrix_rows"][0]["error_code"])

    def test_invalid_candidate_id_is_counted_as_generator_rejection(self):
        request_packet = json.loads(json.dumps(self.request_packet, ensure_ascii=False))
        request_packet["candidates"][0]["review_item_id"] = "m57_01_missing_review"

        report = build_sixth_slice_human_selection_batch_preflight_matrix(
            request_packet,
            self.review_packet,
        )

        self.assertEqual(11, report["summary"]["preflight_passed_count"])
        self.assertEqual(1, report["summary"]["preflight_failed_count"])
        self.assertEqual("candidate_preflight_failures", report["issues"][0]["code"])
        self.assertEqual("generator_rejected_candidate", report["matrix_rows"][0]["error_code"])
        self.assertIn("M57-01 review item not found", report["matrix_rows"][0]["error_message"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "matrix.json"
            md_path = out / "matrix.md"
            csv_path = out / "matrix.csv"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)
            write_csv(self.report, csv_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M57-02-batch-preflight-matrix", loaded["version"])
            self.assertEqual(12, loaded["summary"]["preflight_passed_count"])
            self.assertIn("M57-02 Sixth-Slice Human Selection Batch Preflight Matrix", md_path.read_text(encoding="utf-8"))
            with csv_path.open("r", encoding="utf-8", newline="") as handle:
                rows = list(csv.DictReader(handle))
            self.assertEqual(12, len(rows))
            self.assertEqual("m57_01_m56_recipe_001_repair_review", rows[0]["review_item_id"])


if __name__ == "__main__":
    unittest.main()
