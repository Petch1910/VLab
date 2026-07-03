"""Tests for tools/deck/build_sixth_slice_human_selection_candidate_digest.py."""

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

from tools.deck.build_sixth_slice_human_selection_candidate_digest import (  # noqa: E402
    build_sixth_slice_human_selection_candidate_digest,
    write_csv,
    write_json,
    write_markdown,
)
from tools.deck.build_sixth_slice_human_selection_preflight import (  # noqa: E402
    build_sixth_slice_human_selection_preflight,
)
from tools.deck.build_sixth_slice_human_selection_request_packet import (  # noqa: E402
    build_sixth_slice_human_selection_request_packet,
    load_json,
)
from tools.deck.build_sixth_slice_human_selected_recipe_artifact import M57_01_REVIEW  # noqa: E402


class TestSixthSliceHumanSelectionCandidateDigest(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.review_packet = load_json(M57_01_REVIEW)
        cls.request_packet = build_sixth_slice_human_selection_request_packet(cls.review_packet)
        cls.preflight_report = build_sixth_slice_human_selection_preflight(
            cls.request_packet,
            cls.review_packet,
        )
        cls.report = build_sixth_slice_human_selection_candidate_digest(
            cls.request_packet,
            cls.preflight_report,
        )

    def test_digest_summarizes_all_ready_candidates_without_selection(self):
        summary = self.report["summary"]

        self.assertEqual("M57-02-candidate-digest", self.report["version"])
        self.assertEqual(12, summary["candidate_count"])
        self.assertEqual(12, summary["ready_candidate_count"])
        self.assertEqual(2, summary["source_group_count"])
        self.assertEqual(7, summary["target_group_count"])
        self.assertEqual(1, summary["unique_structural_profile_count"])
        self.assertTrue(summary["all_ready_candidates_share_same_profile"])
        self.assertTrue(summary["request_ready"])
        self.assertTrue(summary["preflight_report_ready"])
        self.assertFalse(summary["human_selection_recorded"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertTrue(summary["ready_for_user_selection_decision"])
        self.assertEqual("M57-02-user-selection", self.report["next_target"]["milestone"])

    def test_comparison_rows_preserve_command_and_pair_context(self):
        first = self.report["comparison_rows"][0]

        self.assertEqual(1, first["order"])
        self.assertEqual("m57_01_m56_recipe_001_repair_review", first["review_item_id"])
        self.assertEqual("m56_recipe_001", first["recipe_id"])
        self.assertTrue(first["ready_for_user_selection"])
        self.assertEqual("G-BT12-062TH->G-BT12-066TH", first["source_candidate_edge"])
        self.assertEqual("G-BT12-062TH", first["source_card_id"])
        self.assertEqual("G-BT12-066TH", first["target_card_id"])
        self.assertEqual(7, first["manual_review_card_count"])
        self.assertEqual(7, first["manual_substitution_count"])
        self.assertTrue(first["manual_repair_complete"])
        self.assertTrue(first["grade_repair_complete"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, first["grade_counts_after"])
        self.assertTrue(first["grade_counts_match_target"])
        self.assertTrue(first["g_zone_deferred"])
        self.assertEqual(2, first["g_zone_decision_option_count"])
        self.assertEqual(4, first["decision_option_count"])
        self.assertEqual(0, first["structural_blocker_count"])
        self.assertIn("--review-item-id m57_01_m56_recipe_001_repair_review", first["selection_command_template"])

    def test_groups_are_readable_for_human_decision(self):
        by_source = self.report["groups"]["by_source_card"]
        by_target = self.report["groups"]["by_target_card"]

        self.assertEqual(2, len(by_source))
        self.assertEqual(7, len(by_target))
        self.assertEqual(6, by_source[0]["candidate_count"])
        self.assertEqual(6, by_source[1]["candidate_count"])
        self.assertEqual(12, sum(group["candidate_count"] for group in by_target))
        self.assertTrue(all(group["review_item_ids"] for group in by_source))
        self.assertTrue(all(group["recipe_ids"] for group in by_target))

    def test_scope_and_policy_forbid_auto_selection_and_runtime_mutation(self):
        scope = self.report["scope"]
        policy = self.report["selection_policy"]
        guidance = self.report["guidance"]

        self.assertTrue(scope["read_only_candidate_digest"])
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
        self.assertTrue(policy["requires_user_or_team_to_choose_one_review_item_id"])
        self.assertTrue(policy["requires_non_empty_selection_text"])
        self.assertTrue(policy["preflight_before_real_artifact_recommended"])
        self.assertEqual("disabled", guidance["auto_selection"])
        self.assertIn("review_item_id", guidance["next_safe_command"])

    def test_unready_request_packet_routes_to_repair(self):
        request_packet = json.loads(json.dumps(self.request_packet, ensure_ascii=False))
        request_packet["summary"]["selection_request_ready"] = False

        report = build_sixth_slice_human_selection_candidate_digest(
            request_packet,
            self.preflight_report,
        )

        self.assertFalse(report["summary"]["ready_for_user_selection_decision"])
        self.assertEqual(1, report["summary"]["blocking_issue_count"])
        self.assertEqual("selection_request_not_ready", report["issues"][0]["code"])
        self.assertEqual("M57-02-prerequisite-repair", report["next_target"]["milestone"])

    def test_no_ready_candidates_routes_to_repair(self):
        request_packet = json.loads(json.dumps(self.request_packet, ensure_ascii=False))
        for candidate in request_packet["candidates"]:
            candidate["ready_for_user_selection"] = False

        report = build_sixth_slice_human_selection_candidate_digest(
            request_packet,
            self.preflight_report,
        )

        self.assertFalse(report["summary"]["ready_for_user_selection_decision"])
        self.assertEqual(1, report["summary"]["blocking_issue_count"])
        self.assertEqual("no_ready_candidates", report["issues"][0]["code"])
        self.assertEqual("M57-02-prerequisite-repair", report["next_target"]["milestone"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "digest.json"
            md_path = out / "digest.md"
            csv_path = out / "digest.csv"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)
            write_csv(self.report, csv_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M57-02-candidate-digest", loaded["version"])
            self.assertEqual(12, loaded["summary"]["ready_candidate_count"])
            self.assertIn("M57-02 Sixth-Slice Human Selection Candidate Digest", md_path.read_text(encoding="utf-8"))
            with csv_path.open("r", encoding="utf-8", newline="") as handle:
                rows = list(csv.DictReader(handle))
            self.assertEqual(12, len(rows))
            self.assertEqual("m57_01_m56_recipe_001_repair_review", rows[0]["review_item_id"])


if __name__ == "__main__":
    unittest.main()
