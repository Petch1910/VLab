"""Tests for tools/deck/build_fourth_slice_human_accepted_repair_artifact.py (M49-03)."""

from __future__ import annotations

import copy
import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_fourth_slice_human_accepted_repair_artifact import (  # noqa: E402
    DEFAULT_ACCEPTED_REVIEW_ITEM_ID,
    DEFAULT_G_ZONE_OPTION,
    M48_DRAFTS,
    M49_01_REVIEW,
    M49_02_G_ZONE_DECISION,
    build_fourth_slice_human_accepted_repair_artifact,
    load_json,
    write_json,
    write_markdown,
)


class TestFourthSliceHumanAcceptedRepairArtifact(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.review = load_json(M49_01_REVIEW)
        cls.g_zone = load_json(M49_02_G_ZONE_DECISION)
        cls.drafts = load_json(M48_DRAFTS)
        cls.report = build_fourth_slice_human_accepted_repair_artifact(
            cls.review,
            cls.g_zone,
            cls.drafts,
            accepted_review_item_id=DEFAULT_ACCEPTED_REVIEW_ITEM_ID,
            acceptance_text="\u0e07\u0e31\u0e49\u0e19\u0e08\u0e31\u0e14\u0e44\u0e1b",
            accepted_at="2026-06-30",
        )

    def test_artifact_records_acceptance_for_first_main_deck_candidate(self):
        summary = self.report["summary"]
        record = self.report["acceptance_record"]

        self.assertEqual("M49-03", self.report["version"])
        self.assertEqual(DEFAULT_ACCEPTED_REVIEW_ITEM_ID, summary["accepted_review_item_id"])
        self.assertEqual("m48_recipe_001", summary["accepted_recipe_id"])
        self.assertEqual("m48_recipe_001_manual_overlap_pkg_001", summary["accepted_manual_repair_package_id"])
        self.assertEqual("m48_recipe_001_grade_profile_pkg_001", summary["accepted_source_grade_profile_package_id"])
        self.assertEqual("m48_recipe_001_combined_manual_grade_pkg_001", summary["accepted_combined_repair_package_id"])
        self.assertTrue(summary["human_acceptance_recorded"])
        self.assertEqual("accepted", record["decision"])
        self.assertEqual("user", record["accepted_by"])
        self.assertEqual("2026-06-30", record["accepted_at"])
        self.assertEqual(
            "accept_first_ranked_main_deck_only_combined_manual_and_grade_repair_candidate",
            record["interpreted_decision"],
        )

    def test_g_zone_boundary_is_consumed_but_runtime_remains_disabled(self):
        summary = self.report["summary"]
        boundary = self.report["g_zone_boundary_record"]

        self.assertEqual(DEFAULT_G_ZONE_OPTION, summary["selected_g_zone_option_id"])
        self.assertEqual(DEFAULT_G_ZONE_OPTION, boundary["selected_option_id"])
        self.assertTrue(summary["main_deck_only_boundary_applied"])
        self.assertTrue(boundary["main_deck_only_validation_allowed"])
        self.assertTrue(boundary["boundary_allows_m49_03_acceptance"])
        self.assertFalse(summary["g_zone_runtime_enabled"])
        self.assertFalse(summary["stride_runtime_enabled"])
        self.assertFalse(boundary["g_zone_runtime_enabled"])
        self.assertFalse(boundary["stride_runtime_enabled"])
        self.assertFalse(boundary["grade4_main_deck_allowed"])
        self.assertFalse(boundary["g_units_allowed_in_main_deck"])
        self.assertIn("Stride timing and Generation Break runtime support", boundary["g_zone_requires_future_system_work"])

    def test_accepted_repair_preview_is_ready_for_validation_but_not_validated(self):
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]
        combined = repair["combined_grade_repair_package"]

        self.assertEqual(50, summary["main_deck_count_after_repair"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, summary["grade_counts_after_repair"])
        self.assertEqual(0, summary["repair_application_issue_count"])
        self.assertTrue(summary["ready_for_m49_04"])
        self.assertEqual(1, len(repair["manual_substitutions"]))
        self.assertEqual(8, combined["added_card_count"])
        self.assertEqual(8, combined["removed_card_count"])
        self.assertEqual(50, sum(row["quantity"] for row in repair["repaired_quantities"]))
        self.assertTrue(combined["complete_candidate"])
        self.assertTrue(repair["requires_m49_04_validation"])
        self.assertTrue(repair["ready_for_m49_04_validation_rerun"])
        self.assertFalse(summary["declares_recipe_valid"])

    def test_combined_grade_repair_is_recomputed_after_manual_substitution(self):
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]
        combined = repair["combined_grade_repair_package"]

        self.assertEqual(0, summary["source_grade_package_conflict_count"])
        self.assertTrue(summary["combined_grade_repair_recomputed"])
        self.assertFalse(combined["source_grade_package_directly_applied"])
        self.assertTrue(combined["source_grade_package_recomputed_after_manual_substitution"])
        self.assertEqual("manual_then_recomputed_grade_profile_substitution", combined["repair_type"])
        self.assertEqual({"0": 16, "1": 10, "2": 8, "3": 16}, combined["grade_counts_after_manual"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, combined["grade_counts_after"])

    def test_scope_does_not_promote_or_declare_valid(self):
        scope = self.report["scope"]
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]

        self.assertTrue(scope["offline_human_accepted_artifact"])
        self.assertTrue(scope["records_human_acceptance"])
        self.assertFalse(scope["records_g_zone_decision"])
        self.assertFalse(scope["changes_review_packet_file"])
        self.assertFalse(scope["changes_g_zone_decision_file"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertFalse(scope["declares_recipe_valid"])
        self.assertFalse(scope["g_zone_runtime_enabled"])
        self.assertFalse(scope["stride_runtime_enabled"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertFalse(summary["declares_recipe_valid"])
        self.assertFalse(repair["runtime_promotion_allowed"])

    def test_blank_acceptance_text_blocks_next_validation(self):
        report = build_fourth_slice_human_accepted_repair_artifact(
            self.review,
            self.g_zone,
            self.drafts,
            accepted_review_item_id=DEFAULT_ACCEPTED_REVIEW_ITEM_ID,
            acceptance_text="",
            accepted_at="2026-06-30",
        )

        self.assertFalse(report["summary"]["human_acceptance_recorded"])
        self.assertFalse(report["summary"]["ready_for_m49_04"])
        self.assertEqual("pending", report["acceptance_record"]["decision"])

    def test_non_main_deck_g_zone_decision_blocks_acceptance(self):
        g_zone = copy.deepcopy(self.g_zone)
        g_zone["summary"]["ready_for_m49_03"] = False
        g_zone["decision_items"][0]["main_deck_only_validation_allowed"] = False

        report = build_fourth_slice_human_accepted_repair_artifact(
            self.review,
            g_zone,
            self.drafts,
            accepted_review_item_id=DEFAULT_ACCEPTED_REVIEW_ITEM_ID,
            acceptance_text="\u0e07\u0e31\u0e49\u0e19\u0e08\u0e31\u0e14\u0e44\u0e1b",
            accepted_at="2026-06-30",
        )

        self.assertFalse(report["g_zone_boundary_record"]["boundary_allows_m49_03_acceptance"])
        self.assertFalse(report["summary"]["human_acceptance_recorded"])
        self.assertFalse(report["summary"]["ready_for_m49_04"])

    def test_invalid_review_item_is_rejected(self):
        with self.assertRaises(ValueError):
            build_fourth_slice_human_accepted_repair_artifact(
                self.review,
                self.g_zone,
                self.drafts,
                accepted_review_item_id="missing-review-item",
                acceptance_text="\u0e07\u0e31\u0e49\u0e19\u0e08\u0e31\u0e14\u0e44\u0e1b",
                accepted_at="2026-06-30",
            )

    def test_next_target_is_m49_04(self):
        next_target = self.report["next_target"]

        self.assertEqual("M49-04", next_target["milestone"])
        self.assertEqual("Fourth-slice repaired recipe validation rerun", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m49_03.json"
            md_path = out / "m49_03.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M49-03", loaded["version"])
            self.assertIn("M49-03 Fourth-Slice Human-Accepted Repair Artifact", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
