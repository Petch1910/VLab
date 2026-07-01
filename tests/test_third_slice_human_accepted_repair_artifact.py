"""Tests for tools/deck/build_third_slice_human_accepted_repair_artifact.py (M45-02)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_third_slice_human_accepted_repair_artifact import (  # noqa: E402
    DEFAULT_ACCEPTED_REVIEW_ITEM_ID,
    M44_DRAFTS,
    M45_01_REVIEW,
    build_third_slice_human_accepted_repair_artifact,
    load_json,
    write_json,
    write_markdown,
)


class TestThirdSliceHumanAcceptedRepairArtifact(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.review = load_json(M45_01_REVIEW)
        cls.drafts = load_json(M44_DRAFTS)
        cls.report = build_third_slice_human_accepted_repair_artifact(
            cls.review,
            cls.drafts,
            accepted_review_item_id=DEFAULT_ACCEPTED_REVIEW_ITEM_ID,
            acceptance_text="\u0e07\u0e31\u0e49\u0e19\u0e08\u0e31\u0e14\u0e44\u0e1b",
            accepted_at="2026-06-30",
        )

    def test_artifact_records_acceptance_for_first_ranked_candidate(self):
        summary = self.report["summary"]
        record = self.report["acceptance_record"]

        self.assertEqual("M45-02", self.report["version"])
        self.assertEqual(DEFAULT_ACCEPTED_REVIEW_ITEM_ID, summary["accepted_review_item_id"])
        self.assertEqual("m44_recipe_001", summary["accepted_recipe_id"])
        self.assertEqual("m44_recipe_001_manual_overlap_pkg_001", summary["accepted_manual_repair_package_id"])
        self.assertEqual("m44_recipe_001_grade_profile_pkg_001", summary["accepted_source_grade_profile_package_id"])
        self.assertEqual("m44_recipe_001_combined_manual_grade_pkg_001", summary["accepted_combined_repair_package_id"])
        self.assertTrue(summary["human_acceptance_recorded"])
        self.assertEqual("accepted", record["decision"])
        self.assertEqual("user", record["accepted_by"])
        self.assertEqual("2026-06-30", record["accepted_at"])
        self.assertEqual(
            "accept_first_ranked_combined_manual_and_grade_repair_candidate",
            record["interpreted_decision"],
        )

    def test_source_grade_conflicts_are_visible_and_combined_repair_is_recomputed(self):
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]
        combined = repair["combined_grade_repair_package"]

        self.assertEqual(2, summary["source_grade_package_conflict_count"])
        self.assertTrue(summary["combined_grade_repair_recomputed"])
        self.assertFalse(combined["source_grade_package_directly_applied"])
        self.assertTrue(combined["source_grade_package_recomputed_after_manual_substitution"])
        self.assertEqual(2, len(repair["source_grade_package_conflicts_after_manual"]))
        self.assertEqual("manual_then_recomputed_grade_profile_substitution", combined["repair_type"])

    def test_accepted_repair_preview_is_ready_for_validation_but_not_validated(self):
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]
        combined = repair["combined_grade_repair_package"]

        self.assertEqual(50, summary["main_deck_count_after_repair"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, summary["grade_counts_after_repair"])
        self.assertEqual(0, summary["repair_application_issue_count"])
        self.assertTrue(summary["ready_for_m45_03"])
        self.assertEqual(6, len(repair["manual_substitutions"]))
        self.assertEqual(5, combined["added_card_count"])
        self.assertEqual(5, combined["removed_card_count"])
        self.assertEqual(50, sum(row["quantity"] for row in repair["repaired_quantities"]))
        self.assertTrue(combined["complete_candidate"])
        self.assertTrue(repair["requires_m45_03_validation"])
        self.assertTrue(repair["ready_for_m45_03_validation_rerun"])
        self.assertFalse(summary["declares_recipe_valid"])

    def test_recomputed_grade_repair_uses_post_manual_removals(self):
        combined = self.report["accepted_repair"]["combined_grade_repair_package"]
        removed_ids = {row["card_id"] for row in combined["removals"]}
        source_conflict_ids = {
            row["card_id"]
            for row in self.report["accepted_repair"]["source_grade_package_conflicts_after_manual"]
        }

        self.assertEqual({"EB06-006TH", "EB06-012TH"}, source_conflict_ids)
        self.assertNotIn("EB06-006TH", removed_ids)
        self.assertNotIn("EB06-012TH", removed_ids)
        self.assertIn("EB06-007TH", removed_ids)
        self.assertIn("EB06-021TH", removed_ids)

    def test_scope_does_not_promote_or_declare_valid(self):
        scope = self.report["scope"]
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]

        self.assertTrue(scope["offline_human_accepted_artifact"])
        self.assertTrue(scope["records_human_acceptance"])
        self.assertFalse(scope["changes_review_packet_file"])
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
        report = build_third_slice_human_accepted_repair_artifact(
            self.review,
            self.drafts,
            accepted_review_item_id=DEFAULT_ACCEPTED_REVIEW_ITEM_ID,
            acceptance_text="",
            accepted_at="2026-06-30",
        )

        self.assertFalse(report["summary"]["human_acceptance_recorded"])
        self.assertFalse(report["summary"]["ready_for_m45_03"])
        self.assertEqual("pending", report["acceptance_record"]["decision"])

    def test_invalid_review_item_is_rejected(self):
        with self.assertRaises(ValueError):
            build_third_slice_human_accepted_repair_artifact(
                self.review,
                self.drafts,
                accepted_review_item_id="missing-review-item",
                acceptance_text="\u0e07\u0e31\u0e49\u0e19\u0e08\u0e31\u0e14\u0e44\u0e1b",
                accepted_at="2026-06-30",
            )

    def test_next_target_is_m45_03(self):
        next_target = self.report["next_target"]

        self.assertEqual("M45-03", next_target["milestone"])
        self.assertEqual("Third-slice repaired recipe validation rerun", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m45_02.json"
            md_path = out / "m45_02.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M45-02", loaded["version"])
            self.assertIn("M45-02 Third-Slice Human-Accepted Repair Artifact", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
