"""Tests for tools/deck/build_second_slice_human_accepted_repair_artifact.py (M41-02)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_second_slice_human_accepted_repair_artifact import (  # noqa: E402
    DEFAULT_ACCEPTED_REVIEW_ITEM_ID,
    M40_DRAFTS,
    M41_01_REVIEW,
    build_second_slice_human_accepted_repair_artifact,
    load_json,
    write_json,
    write_markdown,
)


class TestSecondSliceHumanAcceptedRepairArtifact(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.review = load_json(M41_01_REVIEW)
        cls.drafts = load_json(M40_DRAFTS)
        cls.report = build_second_slice_human_accepted_repair_artifact(
            cls.review,
            cls.drafts,
            accepted_review_item_id=DEFAULT_ACCEPTED_REVIEW_ITEM_ID,
            acceptance_text="\u0e07\u0e31\u0e49\u0e19\u0e08\u0e31\u0e14\u0e44\u0e1b",
            accepted_at="2026-06-30",
        )

    def test_artifact_records_acceptance_for_first_ranked_candidate(self):
        summary = self.report["summary"]
        record = self.report["acceptance_record"]

        self.assertEqual("M41-02", self.report["version"])
        self.assertEqual(DEFAULT_ACCEPTED_REVIEW_ITEM_ID, summary["accepted_review_item_id"])
        self.assertEqual("m40_recipe_001", summary["accepted_recipe_id"])
        self.assertEqual("m40_recipe_001_grade_profile_pkg_001", summary["accepted_repair_package_id"])
        self.assertTrue(summary["human_acceptance_recorded"])
        self.assertEqual("accepted", record["decision"])
        self.assertEqual("user", record["accepted_by"])
        self.assertEqual("2026-06-30", record["accepted_at"])
        self.assertEqual("accept_first_ranked_complete_repair_candidate", record["interpreted_decision"])

    def test_accepted_repair_is_applied_but_not_validated(self):
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]

        self.assertEqual(50, summary["main_deck_count_after_repair"])
        self.assertEqual(0, summary["repair_application_issue_count"])
        self.assertTrue(summary["ready_for_m41_03"])
        self.assertEqual("BT01-006TH->BT02-033TH", repair["source_candidate_edge"])
        self.assertEqual("BT01-006TH", repair["pair"]["source_card_id"])
        self.assertEqual("BT02-033TH", repair["pair"]["target_card_id"])
        self.assertEqual(6, len(repair["additions"]))
        self.assertEqual(6, len(repair["removals"]))
        self.assertEqual(50, sum(row["quantity"] for row in repair["repaired_quantities"]))
        self.assertTrue(repair["requires_m41_03_validation"])
        self.assertTrue(repair["ready_for_m41_03_validation_rerun"])

    def test_scope_does_not_promote_or_declare_valid(self):
        scope = self.report["scope"]
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]

        self.assertTrue(scope["offline_human_accepted_artifact"])
        self.assertTrue(scope["records_human_acceptance"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertFalse(scope["declares_recipe_valid"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertFalse(summary["declares_recipe_valid"])
        self.assertFalse(repair["runtime_promotion_allowed"])

    def test_blank_acceptance_text_blocks_next_validation(self):
        report = build_second_slice_human_accepted_repair_artifact(
            self.review,
            self.drafts,
            accepted_review_item_id=DEFAULT_ACCEPTED_REVIEW_ITEM_ID,
            acceptance_text="",
            accepted_at="2026-06-30",
        )

        self.assertFalse(report["summary"]["human_acceptance_recorded"])
        self.assertFalse(report["summary"]["ready_for_m41_03"])
        self.assertEqual("pending", report["acceptance_record"]["decision"])

    def test_invalid_review_item_is_rejected(self):
        with self.assertRaises(ValueError):
            build_second_slice_human_accepted_repair_artifact(
                self.review,
                self.drafts,
                accepted_review_item_id="missing-review-item",
                acceptance_text="\u0e07\u0e31\u0e49\u0e19\u0e08\u0e31\u0e14\u0e44\u0e1b",
                accepted_at="2026-06-30",
            )

    def test_next_target_is_m41_03(self):
        next_target = self.report["next_target"]

        self.assertEqual("M41-03", next_target["milestone"])
        self.assertEqual("Second-slice repaired recipe validation rerun", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m41_02.json"
            md_path = out / "m41_02.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M41-02", loaded["version"])
            self.assertIn("M41-02 Second-Slice Human-Accepted Repair Artifact", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
