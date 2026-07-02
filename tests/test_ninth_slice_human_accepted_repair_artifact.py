"""Tests for tools/deck/build_ninth_slice_human_accepted_repair_artifact.py (M69-03)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import tests.test_ninth_slice_human_selected_recipe_artifact as selection_fixture  # noqa: E402
from tools.deck.build_ninth_slice_human_accepted_repair_artifact import (  # noqa: E402
    build_ninth_slice_human_accepted_repair_artifact,
    write_json,
    write_markdown,
)
from tools.deck.build_ninth_slice_human_selected_recipe_artifact import (  # noqa: E402
    build_ninth_slice_human_selected_recipe_artifact,
)


SELECTED_REVIEW_ITEM_ID = "m69_01_m68_recipe_001_repair_review"
DECISION_TEXT = "explicit test repair acceptance for m68_recipe_001"


class TestNinthSliceHumanAcceptedRepairArtifact(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        selection_fixture.TestNinthSliceHumanSelectedRecipeArtifact.setUpClass()
        cls.selected_artifact = selection_fixture.TestNinthSliceHumanSelectedRecipeArtifact.report
        cls.review_packet = selection_fixture.review_fixture.TestNinthSliceHumanRepairReviewPacket.report
        cls.drafts = selection_fixture.review_fixture.TestNinthSliceHumanRepairReviewPacket.drafts
        cls.scaffold = selection_fixture.review_fixture.closeout_fixture.TestNinthSliceRuntimeReadinessCloseout.scaffold
        cls.report = build_ninth_slice_human_accepted_repair_artifact(
            cls.selected_artifact,
            cls.drafts,
            cls.scaffold,
            decision_text=DECISION_TEXT,
            repair_decision="accepted",
            decided_by="unit-test",
            decided_at="2026-07-02",
        )

    def test_artifact_records_acceptance_for_selected_recipe(self):
        summary = self.report["summary"]
        record = self.report["repair_decision_record"]

        self.assertEqual("M69-03", self.report["version"])
        self.assertEqual(SELECTED_REVIEW_ITEM_ID, summary["selected_review_item_id"])
        self.assertEqual("m68_recipe_001", summary["selected_recipe_id"])
        self.assertEqual("m68_recipe_001_manual_overlap_pkg_001", summary["selected_manual_overlap_package_id"])
        self.assertEqual("m68_recipe_001_grade_profile_pkg_001", summary["selected_source_grade_profile_package_id"])
        self.assertEqual("m68_recipe_001_combined_manual_grade_pkg_001", summary["accepted_combined_repair_package_id"])
        self.assertEqual("m68_recipe_001_g_zone_deferred_pkg_001", summary["selected_g_zone_package_id"])
        self.assertEqual("m68_recipe_001_stride_deferred_pkg_001", summary["selected_stride_package_id"])
        self.assertEqual(
            "m68_recipe_001_aqua_force_battle_order_pkg_001",
            summary["selected_aqua_force_battle_order_package_id"],
        )
        self.assertTrue(summary["human_selection_recorded"])
        self.assertTrue(summary["human_acceptance_recorded"])
        self.assertFalse(summary["human_rejection_recorded"])
        self.assertEqual("accepted", summary["repair_decision"])
        self.assertTrue(summary["repair_accepted"])
        self.assertFalse(summary["repair_rejected"])
        self.assertFalse(summary["g_zone_decision_recorded"])
        self.assertFalse(summary["stride_decision_recorded"])
        self.assertFalse(summary["aqua_force_battle_order_decision_recorded"])
        self.assertEqual("accepted", record["decision"])
        self.assertEqual("unit-test", record["decided_by"])
        self.assertEqual("2026-07-02", record["decided_at"])
        self.assertEqual(DECISION_TEXT, record["decision_text"])

    def test_manual_then_recomputed_grade_repair_is_ready_for_system_decision(self):
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]
        combined = repair["combined_grade_repair_package"]

        self.assertEqual(0, summary["source_grade_package_conflict_count"])
        self.assertTrue(summary["combined_grade_repair_recomputed"])
        self.assertFalse(combined["source_grade_package_directly_applied"])
        self.assertTrue(combined["source_grade_package_recomputed_after_manual_substitution"])
        self.assertEqual("manual_then_recomputed_grade_profile_substitution", combined["repair_type"])
        self.assertEqual(1, len(repair["manual_substitutions"]))
        self.assertEqual(0, len(repair["source_grade_package_conflicts_after_manual"]))
        self.assertEqual(50, summary["main_deck_count_after_repair"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, summary["grade_counts_after_repair"])
        self.assertEqual(0, summary["repair_application_issue_count"])
        self.assertEqual(5, combined["added_card_count"])
        self.assertEqual(5, combined["removed_card_count"])
        self.assertTrue(combined["complete_candidate"])
        self.assertEqual("G-TD04-014TH", combined["additions"][0]["card_id"])
        self.assertEqual("G-BT02-030TH", combined["removals"][0]["card_id"])
        self.assertTrue(summary["ready_for_m69_04"])
        self.assertTrue(repair["ready_for_m69_04_system_decision"])

    def test_g_zone_stride_and_aqua_remain_deferred_and_runtime_stays_blocked(self):
        scope = self.report["scope"]
        policy = self.report["acceptance_policy"]
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]

        self.assertTrue(scope["offline_human_repair_decision_artifact"])
        self.assertTrue(scope["records_human_selection"])
        self.assertTrue(scope["records_human_acceptance"])
        self.assertFalse(scope["records_human_rejection"])
        self.assertFalse(scope["records_g_zone_decision"])
        self.assertFalse(scope["records_stride_decision"])
        self.assertFalse(scope["records_aqua_force_battle_order_decision"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["g_zone_runtime"])
        self.assertFalse(scope["stride_runtime"])
        self.assertFalse(scope["aqua_force_battle_order_runtime"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertFalse(scope["declares_recipe_valid"])
        self.assertTrue(policy["requires_m69_02_selection"])
        self.assertTrue(policy["requires_non_empty_decision_text"])
        self.assertTrue(policy["supports_repair_rejection"])
        self.assertTrue(policy["acceptance_is_not_validation"])
        self.assertTrue(policy["acceptance_is_not_g_zone_decision"])
        self.assertTrue(policy["acceptance_is_not_stride_decision"])
        self.assertTrue(policy["acceptance_is_not_aqua_force_battle_order_decision"])
        self.assertTrue(policy["source_grade_package_recomputed_after_manual_substitution"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertTrue(policy["m69_04_must_record_explicit_g_zone_stride_aqua_decision"])
        self.assertTrue(policy["m69_05_must_rerun_validation"])
        self.assertTrue(summary["g_zone_deferred"])
        self.assertTrue(summary["stride_deferred"])
        self.assertTrue(summary["aqua_force_battle_order_deferred"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertFalse(summary["declares_recipe_valid"])
        self.assertTrue(repair["g_zone_deferred"])
        self.assertTrue(repair["stride_deferred"])
        self.assertTrue(repair["aqua_force_battle_order_deferred"])
        self.assertIn("G Zone deck slot model", repair["g_zone_future_system_work"])
        self.assertIn("Stride declaration timing", repair["stride_future_system_work"])
        self.assertIn("battle-count tracker", repair["aqua_force_battle_order_future_system_work"])
        self.assertFalse(repair["runtime_promotion_allowed"])
        self.assertTrue(repair["requires_m69_04_g_zone_stride_aqua_decision"])
        self.assertTrue(repair["requires_m69_05_validation"])

    def test_grade_not_needed_selection_can_be_accepted_for_system_decision(self):
        grade_not_needed = next(
            item for item in self.review_packet["review_items"] if item["grade_repair_not_needed"]
        )
        selected = build_ninth_slice_human_selected_recipe_artifact(
            self.review_packet,
            selected_review_item_id=grade_not_needed["review_item_id"],
            selection_text="explicit test selection for grade-not-needed item",
            selected_by="unit-test",
            selected_at="2026-07-02",
        )

        report = build_ninth_slice_human_accepted_repair_artifact(
            selected,
            self.drafts,
            self.scaffold,
            decision_text="explicit test repair acceptance for grade-not-needed item",
            repair_decision="accepted",
            decided_by="unit-test",
            decided_at="2026-07-02",
        )

        combined = report["accepted_repair"]["combined_grade_repair_package"]
        self.assertTrue(combined["complete_candidate"])
        self.assertTrue(report["summary"]["ready_for_m69_04"])

    def test_rejected_repair_decision_is_recorded_but_blocks_m69_04(self):
        report = build_ninth_slice_human_accepted_repair_artifact(
            self.selected_artifact,
            self.drafts,
            self.scaffold,
            decision_text="explicit test repair rejection",
            repair_decision="rejected",
            decided_by="unit-test",
            decided_at="2026-07-02",
        )

        self.assertEqual("rejected", report["summary"]["repair_decision"])
        self.assertFalse(report["summary"]["repair_accepted"])
        self.assertTrue(report["summary"]["repair_rejected"])
        self.assertEqual("", report["summary"]["accepted_combined_repair_package_id"])
        self.assertFalse(report["summary"]["ready_for_m69_04"])
        self.assertFalse(report["accepted_repair"]["ready_for_m69_04_system_decision"])

    def test_blank_decision_text_is_rejected(self):
        with self.assertRaises(ValueError) as context:
            build_ninth_slice_human_accepted_repair_artifact(
                self.selected_artifact,
                self.drafts,
                self.scaffold,
                decision_text=" ",
            )

        self.assertIn("non-empty decision_text", str(context.exception))

    def test_invalid_repair_decision_is_rejected(self):
        with self.assertRaises(ValueError) as context:
            build_ninth_slice_human_accepted_repair_artifact(
                self.selected_artifact,
                self.drafts,
                self.scaffold,
                decision_text=DECISION_TEXT,
                repair_decision="maybe",
            )

        self.assertIn("repair_decision must be one of", str(context.exception))

    def test_invalid_selected_recipe_is_rejected(self):
        selected = json.loads(json.dumps(self.selected_artifact))
        selected["selection"]["recipe_id"] = "m68_missing_recipe"

        with self.assertRaises(ValueError) as context:
            build_ninth_slice_human_accepted_repair_artifact(
                selected,
                self.drafts,
                self.scaffold,
                decision_text=DECISION_TEXT,
            )

        self.assertIn("Recipe not found", str(context.exception))

    def test_not_ready_selection_blocks_m69_04_readiness(self):
        selected = json.loads(json.dumps(self.selected_artifact))
        selected["summary"]["ready_for_m69_03"] = False

        report = build_ninth_slice_human_accepted_repair_artifact(
            selected,
            self.drafts,
            self.scaffold,
            decision_text=DECISION_TEXT,
        )

        self.assertFalse(report["summary"]["ready_for_m69_04"])

    def test_next_target_is_m69_04(self):
        next_target = self.report["next_target"]

        self.assertEqual("M69-04", next_target["milestone"])
        self.assertEqual("Ninth-slice G Zone, Stride, and Aqua Force decision artifact", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m69_03.json"
            md_path = out / "m69_03.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M69-03", loaded["version"])
            self.assertEqual("m68_recipe_001", loaded["summary"]["selected_recipe_id"])
            self.assertIn("M69-03 Ninth-Slice Human-Accepted Repair Artifact", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
