"""Tests for tools/deck/build_sixth_slice_human_selection_support_closeout.py."""

from __future__ import annotations

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
)
from tools.deck.build_sixth_slice_human_selection_candidate_digest import (  # noqa: E402
    build_sixth_slice_human_selection_candidate_digest,
)
from tools.deck.build_sixth_slice_human_selection_preflight import (  # noqa: E402
    build_sixth_slice_human_selection_preflight,
)
from tools.deck.build_sixth_slice_human_selection_request_packet import (  # noqa: E402
    build_sixth_slice_human_selection_request_packet,
    load_json,
)
from tools.deck.build_sixth_slice_human_selection_support_closeout import (  # noqa: E402
    build_sixth_slice_human_selection_support_closeout,
    write_json,
    write_markdown,
)
from tools.deck.build_sixth_slice_human_selected_recipe_artifact import M57_01_REVIEW  # noqa: E402


class TestSixthSliceHumanSelectionSupportCloseout(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.review_packet = load_json(M57_01_REVIEW)
        cls.request_packet = build_sixth_slice_human_selection_request_packet(cls.review_packet)
        cls.preflight_report = build_sixth_slice_human_selection_preflight(
            cls.request_packet,
            cls.review_packet,
        )
        cls.candidate_digest = build_sixth_slice_human_selection_candidate_digest(
            cls.request_packet,
            cls.preflight_report,
        )
        cls.batch_preflight = build_sixth_slice_human_selection_batch_preflight_matrix(
            cls.request_packet,
            cls.review_packet,
        )
        cls.report = build_sixth_slice_human_selection_support_closeout(
            cls.request_packet,
            cls.preflight_report,
            cls.candidate_digest,
            cls.batch_preflight,
        )

    def test_closeout_completes_selection_support_without_creating_selection(self):
        summary = self.report["summary"]
        evidence = self.report["evidence"]
        handoff = self.report["handoff"]

        self.assertEqual("M57-02-selection-support-closeout", self.report["version"])
        self.assertTrue(summary["support_closeout_complete"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertEqual(12, summary["ready_candidate_count"])
        self.assertEqual(12, summary["batch_preflight_passed_count"])
        self.assertEqual(0, summary["batch_preflight_failed_count"])
        self.assertFalse(summary["human_selection_recorded"])
        self.assertFalse(summary["real_m57_02_artifact_created"])
        self.assertTrue(summary["ready_for_user_selection"])
        self.assertTrue(evidence["request_ready"])
        self.assertTrue(evidence["preflight_report_ready"])
        self.assertTrue(evidence["default_preflight_requires_input"])
        self.assertTrue(evidence["digest_ready"])
        self.assertEqual(2, evidence["digest_source_group_count"])
        self.assertEqual(7, evidence["digest_target_group_count"])
        self.assertTrue(evidence["batch_all_candidates_pass"])
        self.assertFalse(evidence["human_selection_recorded_anywhere"])
        self.assertTrue(handoff["requires_user_or_team_selection"])
        self.assertTrue(handoff["requires_non_empty_selection_text"])
        self.assertTrue(handoff["real_m57_02_artifact_blocked_until_selection"])
        self.assertEqual("M57-02-user-selection", self.report["next_target"]["milestone"])

    def test_scope_blocks_runtime_artifact_ui_bot_and_gamestate(self):
        scope = self.report["scope"]

        self.assertTrue(scope["read_only_support_closeout"])
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

    def test_handoff_contains_template_but_no_selected_id(self):
        handoff = self.report["handoff"]
        example = handoff["example_candidate_not_selected"]

        self.assertIn("--review-item-id <review_item_id>", handoff["command_template"])
        self.assertIn("--selection-text", handoff["command_template"])
        self.assertEqual("m57_01_m56_recipe_001_repair_review", example["review_item_id"])
        self.assertEqual("m56_recipe_001", example["recipe_id"])
        self.assertEqual("G-BT12-062TH->G-BT12-066TH", example["source_candidate_edge"])

    def test_failed_batch_preflight_blocks_closeout(self):
        batch_preflight = json.loads(json.dumps(self.batch_preflight, ensure_ascii=False))
        batch_preflight["summary"]["all_candidates_pass_preflight"] = False
        batch_preflight["summary"]["preflight_passed_count"] = 11
        batch_preflight["summary"]["preflight_failed_count"] = 1

        report = build_sixth_slice_human_selection_support_closeout(
            self.request_packet,
            self.preflight_report,
            self.candidate_digest,
            batch_preflight,
        )

        self.assertFalse(report["summary"]["support_closeout_complete"])
        self.assertFalse(report["summary"]["ready_for_user_selection"])
        self.assertEqual(2, report["summary"]["blocking_issue_count"])
        self.assertEqual(
            ["batch_preflight_not_all_passed", "preflight_pass_count_mismatch"],
            [issue["code"] for issue in report["issues"]],
        )
        self.assertEqual("M57-02-support-repair", report["next_target"]["milestone"])

    def test_unexpected_selection_recording_blocks_closeout(self):
        digest = json.loads(json.dumps(self.candidate_digest, ensure_ascii=False))
        digest["summary"]["human_selection_recorded"] = True

        report = build_sixth_slice_human_selection_support_closeout(
            self.request_packet,
            self.preflight_report,
            digest,
            self.batch_preflight,
        )

        self.assertFalse(report["summary"]["support_closeout_complete"])
        self.assertEqual(1, report["summary"]["blocking_issue_count"])
        self.assertEqual("unexpected_digest_selection", report["issues"][0]["code"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "closeout.json"
            md_path = out / "closeout.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M57-02-selection-support-closeout", loaded["version"])
            self.assertTrue(loaded["summary"]["support_closeout_complete"])
            self.assertIn("M57-02 Sixth-Slice Human Selection Support Closeout", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
