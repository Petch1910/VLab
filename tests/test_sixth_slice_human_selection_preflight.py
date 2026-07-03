"""Tests for tools/deck/build_sixth_slice_human_selection_preflight.py."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_sixth_slice_human_selection_preflight import (  # noqa: E402
    build_sixth_slice_human_selection_preflight,
    write_json,
    write_markdown,
)
from tools.deck.build_sixth_slice_human_selection_request_packet import (  # noqa: E402
    build_sixth_slice_human_selection_request_packet,
    load_json,
)
from tools.deck.build_sixth_slice_human_selected_recipe_artifact import M57_01_REVIEW  # noqa: E402


SELECTED_REVIEW_ITEM_ID = "m57_01_m56_recipe_001_repair_review"
SELECTION_TEXT = "team dry-run selection for m56_recipe_001"


class TestSixthSliceHumanSelectionPreflight(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.review_packet = load_json(M57_01_REVIEW)
        cls.request_packet = build_sixth_slice_human_selection_request_packet(cls.review_packet)

    def test_preflight_without_input_reports_required_selection_without_recording(self):
        report = build_sixth_slice_human_selection_preflight(
            self.request_packet,
            self.review_packet,
        )

        summary = report["summary"]
        decision = report["decision"]
        dry_run = report["dry_run"]

        self.assertEqual("M57-02-preflight", report["version"])
        self.assertTrue(summary["request_ready"])
        self.assertEqual(12, summary["ready_candidate_count"])
        self.assertEqual(2, summary["input_issue_count"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertFalse(summary["preflight_passed"])
        self.assertFalse(summary["ready_for_real_m57_02_command"])
        self.assertFalse(summary["human_selection_recorded"])
        self.assertFalse(dry_run["executed"])
        self.assertFalse(dry_run["would_create_m57_02_artifact"])
        self.assertFalse(decision["ready_for_real_m57_02_command"])
        self.assertEqual("M57-02-user-selection", report["next_target"]["milestone"])
        self.assertEqual(
            ["missing_review_item_id", "missing_selection_text"],
            [issue["code"] for issue in report["issues"]],
        )

    def test_valid_selection_preflight_dry_runs_real_generator_contract(self):
        report = build_sixth_slice_human_selection_preflight(
            self.request_packet,
            self.review_packet,
            review_item_id=SELECTED_REVIEW_ITEM_ID,
            selection_text=SELECTION_TEXT,
        )

        summary = report["summary"]
        dry_run = report["dry_run"]

        self.assertTrue(summary["preflight_passed"])
        self.assertTrue(summary["ready_for_real_m57_02_command"])
        self.assertEqual(0, summary["issue_count"])
        self.assertEqual(SELECTED_REVIEW_ITEM_ID, summary["selected_review_item_id"])
        self.assertEqual("m56_recipe_001", summary["selected_recipe_id"])
        self.assertTrue(dry_run["executed"])
        self.assertTrue(dry_run["would_create_m57_02_artifact"])
        self.assertTrue(dry_run["would_record_human_selection"])
        self.assertFalse(dry_run["would_record_human_acceptance"])
        self.assertFalse(dry_run["would_record_g_zone_decision"])
        self.assertFalse(dry_run["would_allow_runtime_promotion"])
        self.assertTrue(dry_run["would_be_ready_for_m57_03"])
        self.assertIn("--review-item-id m57_01_m56_recipe_001_repair_review", dry_run["real_command"])
        self.assertIn(SELECTION_TEXT, dry_run["real_command"])
        self.assertEqual("M57-02", report["next_target"]["milestone"])

    def test_unknown_review_item_id_is_blocking(self):
        report = build_sixth_slice_human_selection_preflight(
            self.request_packet,
            self.review_packet,
            review_item_id="m57_01_missing_review",
            selection_text=SELECTION_TEXT,
        )

        self.assertFalse(report["summary"]["preflight_passed"])
        self.assertEqual(1, report["summary"]["blocking_issue_count"])
        self.assertEqual(0, report["summary"]["input_issue_count"])
        self.assertEqual("unknown_review_item_id", report["issues"][0]["code"])
        self.assertFalse(report["dry_run"]["executed"])

    def test_old_fifth_slice_id_is_not_accepted(self):
        report = build_sixth_slice_human_selection_preflight(
            self.request_packet,
            self.review_packet,
            review_item_id="m53_01_m52_recipe_001_repair_review",
            selection_text=SELECTION_TEXT,
        )

        self.assertFalse(report["summary"]["preflight_passed"])
        self.assertEqual("unknown_review_item_id", report["issues"][0]["code"])
        self.assertEqual("", report["summary"]["selected_review_item_id"])

    def test_blank_selection_text_is_input_issue(self):
        report = build_sixth_slice_human_selection_preflight(
            self.request_packet,
            self.review_packet,
            review_item_id=SELECTED_REVIEW_ITEM_ID,
            selection_text=" ",
        )

        self.assertFalse(report["summary"]["preflight_passed"])
        self.assertEqual(0, report["summary"]["blocking_issue_count"])
        self.assertEqual(1, report["summary"]["input_issue_count"])
        self.assertEqual("missing_selection_text", report["issues"][0]["code"])
        self.assertFalse(report["dry_run"]["executed"])

    def test_scope_does_not_record_or_mutate_runtime(self):
        report = build_sixth_slice_human_selection_preflight(
            self.request_packet,
            self.review_packet,
            review_item_id=SELECTED_REVIEW_ITEM_ID,
            selection_text=SELECTION_TEXT,
        )
        scope = report["scope"]

        self.assertTrue(scope["read_only_preflight"])
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

    def test_output_round_trip(self):
        report = build_sixth_slice_human_selection_preflight(
            self.request_packet,
            self.review_packet,
            review_item_id=SELECTED_REVIEW_ITEM_ID,
            selection_text=SELECTION_TEXT,
        )

        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "preflight.json"
            md_path = out / "preflight.md"

            write_json(report, json_path)
            write_markdown(report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M57-02-preflight", loaded["version"])
            self.assertTrue(loaded["summary"]["preflight_passed"])
            self.assertIn("M57-02 Sixth-Slice Human Selection Preflight", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
