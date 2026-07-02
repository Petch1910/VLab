"""Tests for tools/deck/build_eighth_slice_human_accepted_grade_repair_artifact.py (M65-03)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import tests.test_eighth_slice_human_selected_recipe_artifact as selection_fixture  # noqa: E402
from tools.deck.build_eighth_slice_human_accepted_grade_repair_artifact import (  # noqa: E402
    build_eighth_slice_human_accepted_grade_repair_artifact,
    write_json,
    write_markdown,
)


SELECTED_REVIEW_ITEM_ID = "m65_01_m64_recipe_001_repair_review"
DECISION_TEXT = "explicit test grade repair acceptance for m64_recipe_001"


class TestEighthSliceHumanAcceptedGradeRepairArtifact(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        selection_fixture.TestEighthSliceHumanSelectedRecipeArtifact.setUpClass()
        cls.selected_artifact = selection_fixture.TestEighthSliceHumanSelectedRecipeArtifact.report
        cls.drafts = selection_fixture.review_fixture.TestEighthSliceHumanRepairReviewPacket.drafts
        cls.report = build_eighth_slice_human_accepted_grade_repair_artifact(
            cls.selected_artifact,
            cls.drafts,
            decision_text=DECISION_TEXT,
            grade_decision="accepted",
            decided_by="unit-test",
            decided_at="2026-07-02",
        )

    def test_artifact_records_grade_acceptance_for_selected_recipe(self):
        summary = self.report["summary"]
        record = self.report["grade_repair_decision_record"]

        self.assertEqual("M65-03", self.report["version"])
        self.assertEqual(SELECTED_REVIEW_ITEM_ID, summary["selected_review_item_id"])
        self.assertEqual("m64_recipe_001", summary["selected_recipe_id"])
        self.assertEqual("m64_recipe_001_human_selection_pkg_001", summary["selected_human_selection_package_id"])
        self.assertEqual("m64_recipe_001_grade_profile_pkg_001", summary["source_grade_profile_package_id"])
        self.assertEqual("m64_recipe_001_accepted_grade_pkg_001", summary["accepted_grade_repair_package_id"])
        self.assertEqual("m64_recipe_001_lock_deferred_pkg_001", summary["selected_lock_package_id"])
        self.assertEqual("m64_recipe_001_legion_deferred_pkg_001", summary["selected_legion_package_id"])
        self.assertTrue(summary["human_selection_recorded"])
        self.assertTrue(summary["human_acceptance_recorded"])
        self.assertEqual("accepted", summary["grade_repair_decision"])
        self.assertTrue(summary["grade_repair_accepted"])
        self.assertFalse(summary["grade_repair_rejected"])
        self.assertFalse(summary["lock_decision_recorded"])
        self.assertFalse(summary["legion_decision_recorded"])
        self.assertEqual("accepted", record["decision"])
        self.assertEqual("unit-test", record["decided_by"])
        self.assertEqual("2026-07-02", record["decided_at"])
        self.assertEqual(DECISION_TEXT, record["decision_text"])

    def test_direct_grade_repair_preview_is_ready_for_lock_legion_decision(self):
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]
        grade_package = repair["accepted_grade_repair_package"]

        self.assertTrue(summary["source_grade_package_directly_applied"])
        self.assertTrue(grade_package["source_grade_package_directly_applied"])
        self.assertFalse(grade_package["source_grade_package_recomputed_after_manual_substitution"])
        self.assertEqual("accepted_grade_profile_substitution_preview", grade_package["repair_type"])
        self.assertEqual("m64_recipe_001_grade_profile_pkg_001", grade_package["source_grade_profile_package_id"])
        self.assertEqual(1, grade_package["added_card_count"])
        self.assertEqual(1, grade_package["removed_card_count"])
        self.assertEqual("BT11-068TH", grade_package["additions"][0]["card_id"])
        self.assertEqual("BT11-011TH", grade_package["removals"][0]["card_id"])
        self.assertEqual(50, summary["main_deck_count_after_repair"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, summary["grade_counts_after_repair"])
        self.assertEqual(0, summary["repair_application_issue_count"])
        self.assertTrue(grade_package["complete_candidate"])
        self.assertTrue(summary["ready_for_m65_04"])
        self.assertTrue(repair["ready_for_m65_04_system_decision"])

    def test_lock_and_legion_remain_deferred_and_runtime_stays_blocked(self):
        scope = self.report["scope"]
        policy = self.report["acceptance_policy"]
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]

        self.assertTrue(scope["offline_human_grade_repair_decision_artifact"])
        self.assertTrue(scope["records_human_selection"])
        self.assertTrue(scope["records_human_acceptance"])
        self.assertTrue(scope["records_grade_repair_acceptance"])
        self.assertFalse(scope["records_grade_repair_rejection"])
        self.assertFalse(scope["records_lock_decision"])
        self.assertFalse(scope["records_legion_decision"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["lock_runtime"])
        self.assertFalse(scope["legion_runtime"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertFalse(scope["declares_recipe_valid"])
        self.assertTrue(policy["requires_m65_02_selection"])
        self.assertTrue(policy["requires_non_empty_decision_text"])
        self.assertTrue(policy["acceptance_is_not_validation"])
        self.assertTrue(policy["acceptance_is_not_lock_decision"])
        self.assertTrue(policy["acceptance_is_not_legion_decision"])
        self.assertTrue(policy["supports_grade_repair_rejection"])
        self.assertTrue(policy["source_grade_package_directly_applied"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertTrue(policy["m65_04_must_record_explicit_lock_legion_decision"])
        self.assertTrue(policy["m65_05_must_rerun_validation"])
        self.assertTrue(summary["lock_deferred"])
        self.assertTrue(summary["legion_deferred"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertFalse(summary["declares_recipe_valid"])
        self.assertTrue(repair["lock_deferred"])
        self.assertTrue(repair["legion_deferred"])
        self.assertIn("Lock/Unlock runtime rules module", repair["lock_future_system_work"])
        self.assertIn("Legion/Mate declaration rules", repair["legion_future_system_work"])
        self.assertFalse(repair["runtime_promotion_allowed"])
        self.assertTrue(repair["requires_m65_04_lock_legion_decision"])
        self.assertTrue(repair["requires_m65_05_validation"])

    def test_rejected_grade_decision_is_recorded_but_blocks_m65_04(self):
        report = build_eighth_slice_human_accepted_grade_repair_artifact(
            self.selected_artifact,
            self.drafts,
            decision_text="explicit test grade repair rejection",
            grade_decision="rejected",
            decided_by="unit-test",
            decided_at="2026-07-02",
        )

        self.assertEqual("rejected", report["summary"]["grade_repair_decision"])
        self.assertFalse(report["summary"]["grade_repair_accepted"])
        self.assertTrue(report["summary"]["grade_repair_rejected"])
        self.assertEqual("", report["summary"]["accepted_grade_repair_package_id"])
        self.assertFalse(report["summary"]["ready_for_m65_04"])
        self.assertFalse(report["accepted_repair"]["ready_for_m65_04_system_decision"])

    def test_blank_decision_text_is_rejected(self):
        with self.assertRaises(ValueError) as context:
            build_eighth_slice_human_accepted_grade_repair_artifact(
                self.selected_artifact,
                self.drafts,
                decision_text=" ",
            )

        self.assertIn("non-empty decision_text", str(context.exception))

    def test_invalid_grade_decision_is_rejected(self):
        with self.assertRaises(ValueError) as context:
            build_eighth_slice_human_accepted_grade_repair_artifact(
                self.selected_artifact,
                self.drafts,
                decision_text=DECISION_TEXT,
                grade_decision="maybe",
            )

        self.assertIn("grade_decision must be one of", str(context.exception))

    def test_invalid_selected_recipe_is_rejected(self):
        selected = json.loads(json.dumps(self.selected_artifact))
        selected["selection"]["recipe_id"] = "m64_missing_recipe"

        with self.assertRaises(ValueError) as context:
            build_eighth_slice_human_accepted_grade_repair_artifact(
                selected,
                self.drafts,
                decision_text=DECISION_TEXT,
            )

        self.assertIn("Recipe not found", str(context.exception))

    def test_not_ready_selection_blocks_m65_04_readiness(self):
        selected = json.loads(json.dumps(self.selected_artifact))
        selected["summary"]["ready_for_m65_03"] = False

        report = build_eighth_slice_human_accepted_grade_repair_artifact(
            selected,
            self.drafts,
            decision_text=DECISION_TEXT,
        )

        self.assertFalse(report["summary"]["ready_for_m65_04"])

    def test_next_target_is_m65_04(self):
        next_target = self.report["next_target"]

        self.assertEqual("M65-04", next_target["milestone"])
        self.assertEqual("Eighth-slice Lock and Legion decision artifact", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m65_03.json"
            md_path = out / "m65_03.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M65-03", loaded["version"])
            self.assertEqual("m64_recipe_001", loaded["summary"]["selected_recipe_id"])
            self.assertIn("M65-03 Eighth-Slice Human-Accepted Grade Repair Artifact", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
