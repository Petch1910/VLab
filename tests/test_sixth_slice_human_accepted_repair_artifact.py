"""Tests for tools/deck/build_sixth_slice_human_accepted_repair_artifact.py (M57-03)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_sixth_slice_human_accepted_repair_artifact import (  # noqa: E402
    M56_DRAFTS,
    M56_SCAFFOLD,
    build_sixth_slice_human_accepted_repair_artifact,
    load_json,
    write_json,
    write_markdown,
)
from tools.deck.build_sixth_slice_human_selected_recipe_artifact import (  # noqa: E402
    M57_01_REVIEW,
    build_sixth_slice_human_selected_recipe_artifact,
)


SELECTED_REVIEW_ITEM_ID = "m57_01_m56_recipe_001_repair_review"
SELECTION_TEXT = "explicit test selection for m56_recipe_001"
ACCEPTANCE_TEXT = "explicit test acceptance for m56_recipe_001"


class TestSixthSliceHumanAcceptedRepairArtifact(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.review_packet = load_json(M57_01_REVIEW)
        cls.drafts = load_json(M56_DRAFTS)
        cls.scaffold = load_json(M56_SCAFFOLD)
        cls.selected_artifact = build_sixth_slice_human_selected_recipe_artifact(
            cls.review_packet,
            selected_review_item_id=SELECTED_REVIEW_ITEM_ID,
            selection_text=SELECTION_TEXT,
            selected_by="unit-test",
            selected_at="2026-07-01",
        )
        cls.report = build_sixth_slice_human_accepted_repair_artifact(
            cls.selected_artifact,
            cls.drafts,
            cls.scaffold,
            acceptance_text=ACCEPTANCE_TEXT,
            accepted_by="unit-test",
            accepted_at="2026-07-01",
        )

    def test_artifact_records_acceptance_for_selected_recipe(self):
        summary = self.report["summary"]
        record = self.report["acceptance_record"]

        self.assertEqual("M57-03", self.report["version"])
        self.assertEqual(SELECTED_REVIEW_ITEM_ID, summary["accepted_review_item_id"])
        self.assertEqual("m56_recipe_001", summary["accepted_recipe_id"])
        self.assertEqual("m56_recipe_001_manual_overlap_pkg_001", summary["accepted_manual_overlap_package_id"])
        self.assertEqual("m56_recipe_001_grade_profile_pkg_001", summary["accepted_source_grade_profile_package_id"])
        self.assertEqual("m56_recipe_001_combined_manual_grade_pkg_001", summary["accepted_combined_repair_package_id"])
        self.assertEqual("m56_recipe_001_g_zone_deferred_pkg_001", summary["accepted_g_zone_package_id"])
        self.assertTrue(summary["human_selection_recorded"])
        self.assertTrue(summary["human_acceptance_recorded"])
        self.assertFalse(summary["g_zone_decision_recorded"])
        self.assertEqual("accepted", record["decision"])
        self.assertEqual("unit-test", record["accepted_by"])
        self.assertEqual("2026-07-01", record["accepted_at"])
        self.assertEqual(ACCEPTANCE_TEXT, record["acceptance_text"])

    def test_manual_then_recomputed_grade_repair_is_ready_for_g_zone_decision(self):
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]
        combined = repair["combined_grade_repair_package"]

        self.assertEqual(1, summary["source_grade_package_conflict_count"])
        self.assertTrue(summary["combined_grade_repair_recomputed"])
        self.assertFalse(combined["source_grade_package_directly_applied"])
        self.assertTrue(combined["source_grade_package_recomputed_after_manual_substitution"])
        self.assertEqual("manual_then_recomputed_grade_profile_substitution", combined["repair_type"])
        self.assertEqual(7, len(repair["manual_substitutions"]))
        self.assertEqual(1, len(repair["source_grade_package_conflicts_after_manual"]))
        self.assertEqual("G-BT09-026TH", repair["source_grade_package_conflicts_after_manual"][0]["card_id"])
        self.assertEqual(50, summary["main_deck_count_after_repair"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, summary["grade_counts_after_repair"])
        self.assertEqual(0, summary["repair_application_issue_count"])
        self.assertTrue(combined["complete_candidate"])
        self.assertTrue(summary["ready_for_m57_04"])
        self.assertTrue(repair["ready_for_m57_04_g_zone_decision"])

    def test_g_zone_remains_deferred_and_runtime_stays_blocked(self):
        scope = self.report["scope"]
        policy = self.report["acceptance_policy"]
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]

        self.assertTrue(scope["offline_human_accepted_artifact"])
        self.assertTrue(scope["records_human_selection"])
        self.assertTrue(scope["records_human_acceptance"])
        self.assertFalse(scope["records_g_zone_decision"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertFalse(scope["declares_recipe_valid"])
        self.assertTrue(policy["requires_m57_02_selection"])
        self.assertTrue(policy["requires_non_empty_acceptance_text"])
        self.assertTrue(policy["acceptance_is_not_validation"])
        self.assertTrue(policy["acceptance_is_not_g_zone_decision"])
        self.assertTrue(policy["source_grade_package_recomputed_after_manual_substitution"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertTrue(policy["m57_04_must_record_explicit_g_zone_decision"])
        self.assertTrue(policy["m57_05_must_rerun_validation"])
        self.assertTrue(summary["g_zone_deferred"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertFalse(summary["declares_recipe_valid"])
        self.assertTrue(repair["g_zone_deferred"])
        self.assertGreaterEqual(len(repair["g_zone_future_system_work"]), 1)
        self.assertFalse(repair["runtime_promotion_allowed"])
        self.assertTrue(repair["requires_m57_04_g_zone_decision"])
        self.assertTrue(repair["requires_m57_05_validation"])

    def test_blank_acceptance_text_is_rejected(self):
        with self.assertRaises(ValueError) as context:
            build_sixth_slice_human_accepted_repair_artifact(
                self.selected_artifact,
                self.drafts,
                self.scaffold,
                acceptance_text=" ",
            )

        self.assertIn("non-empty acceptance_text", str(context.exception))

    def test_invalid_selected_recipe_is_rejected(self):
        selected = json.loads(json.dumps(self.selected_artifact))
        selected["selection"]["recipe_id"] = "m56_missing_recipe"

        with self.assertRaises(ValueError) as context:
            build_sixth_slice_human_accepted_repair_artifact(
                selected,
                self.drafts,
                self.scaffold,
                acceptance_text=ACCEPTANCE_TEXT,
            )

        self.assertIn("Recipe not found", str(context.exception))

    def test_next_target_is_m57_04(self):
        next_target = self.report["next_target"]

        self.assertEqual("M57-04", next_target["milestone"])
        self.assertEqual("Sixth-slice G Zone / Stride decision artifact", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m57_03.json"
            md_path = out / "m57_03.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M57-03", loaded["version"])
            self.assertEqual("m56_recipe_001", loaded["summary"]["accepted_recipe_id"])
            self.assertIn("M57-03 Sixth-Slice Human-Accepted Repair Artifact", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
