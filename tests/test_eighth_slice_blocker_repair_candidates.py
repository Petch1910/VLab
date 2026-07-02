"""Tests for tools/deck/build_eighth_slice_blocker_repair_candidates.py (M64-06)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import tests.test_eighth_slice_recipe_draft_model as eighth_draft_fixture  # noqa: E402
from tools.deck.build_eighth_slice_blocker_repair_candidates import (  # noqa: E402
    build_eighth_slice_blocker_repair_candidates,
    write_json,
    write_markdown,
)
from tools.deck.check_eighth_slice_combo_recipe_consistency import (  # noqa: E402
    build_eighth_slice_consistency_report,
)
from tools.deck.validate_eighth_slice_recipe_drafts import (  # noqa: E402
    _all_card_ids,
    build_eighth_slice_validation_report,
    load_card_rows,
)


class TestEighthSliceBlockerRepairCandidates(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        eighth_draft_fixture.TestEighthSliceRecipeDraftModel.setUpClass()
        cls.recipes = eighth_draft_fixture.TestEighthSliceRecipeDraftModel.report
        cls.scaffold = eighth_draft_fixture.TestEighthSliceRecipeDraftModel.scaffold
        cls.card_rows = load_card_rows(_all_card_ids(cls.recipes))
        cls.validation = build_eighth_slice_validation_report(cls.recipes, cls.card_rows)
        cls.consistency = build_eighth_slice_consistency_report(cls.recipes, cls.validation)
        cls.report = build_eighth_slice_blocker_repair_candidates(
            cls.recipes,
            cls.validation,
            cls.consistency,
            cls.scaffold,
        )

    def test_repair_candidates_are_ready_for_closeout(self):
        summary = self.report["summary"]

        self.assertEqual("M64-06", self.report["version"])
        self.assertTrue(summary["ready_for_m64_closeout"])
        self.assertEqual(25, summary["recipe_count"])
        self.assertEqual(0, summary["manual_overlap_recipe_count"])
        self.assertEqual(25, summary["human_selection_candidate_count"])
        self.assertEqual(25, summary["grade_profile_repair_candidate_count"])
        self.assertEqual(25, summary["grade_profile_complete_candidate_count"])
        self.assertEqual(25, summary["lock_deferred_recipe_count"])
        self.assertEqual(25, summary["legion_deferred_recipe_count"])
        self.assertEqual(0, summary["unexpected_structural_blocker_recipe_count"])
        self.assertEqual(25, summary["ready_for_human_repair_review_count"])
        self.assertFalse(summary["runtime_promotion_allowed"])

    def test_scope_is_offline_repair_only(self):
        scope = self.report["scope"]
        policy = self.report["repair_policy"]

        self.assertTrue(scope["offline_repair_candidates"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["records_human_selection"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["runtime_deck"])
        self.assertFalse(scope["lock_runtime"])
        self.assertFalse(scope["legion_runtime"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["automatic_deck_injection"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertTrue(policy["source_backed_candidates"])
        self.assertTrue(policy["substitution_only"])
        self.assertTrue(policy["manual_review_cards_are_not_resolved_here"])
        self.assertTrue(policy["human_selection_is_not_recorded_here"])
        self.assertTrue(policy["lock_support_is_deferred_system_work"])
        self.assertTrue(policy["legion_support_is_deferred_system_work"])
        self.assertIn("BT10", policy["series_scope_from_m64_01_fixture_scaffold"])
        self.assertFalse(policy["runtime_promotion_allowed"])

    def test_each_repair_item_has_selection_grade_lock_and_legion_context(self):
        for item in self.report["repair_items"]:
            human = item["human_selection_package"]
            grade = item["grade_profile_repair_package"]
            lock = item["lock_deferred_package"]
            legion = item["legion_deferred_package"]

            self.assertEqual("validator_passed_pending_human_selection", item["validation_status"])
            self.assertEqual("consistent_pending_human_selection", item["consistency_status"])
            self.assertEqual([], item["manual_review_card_ids"])
            self.assertEqual([], item["structural_blockers"])
            self.assertTrue(human["requires_human_selection"])
            self.assertFalse(human["records_human_selection"])
            self.assertTrue(human["pair_cards_present"])
            self.assertTrue(grade["complete_candidate"])
            self.assertIsNotNone(lock)
            self.assertIsNotNone(legion)
            self.assertTrue(item["blocked_by_lock_deferred"])
            self.assertTrue(item["blocked_by_legion_deferred"])
            self.assertTrue(item["ready_for_human_repair_review"])
            self.assertFalse(item["runtime_promotion_allowed"])

    def test_grade_packages_are_source_backed_previews(self):
        target = {"0": 17, "1": 14, "2": 11, "3": 8}

        for item in self.report["repair_items"]:
            grade = item["grade_profile_repair_package"]

            self.assertEqual("grade_profile_substitution_preview", grade["repair_type"])
            self.assertTrue(grade["advisory_only"])
            self.assertFalse(grade["runtime_promotion_allowed"])
            self.assertEqual(target, grade["target_grade_counts"])
            self.assertNotEqual(target, grade["grade_counts_before"])
            self.assertEqual(target, grade["grade_counts_after"])
            self.assertEqual(grade["added_card_count"], grade["removed_card_count"])
            self.assertGreater(grade["added_card_count"], 0)
            for addition in grade["additions"]:
                self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", addition["source"])
                self.assertIn(addition["grade"], target)
            for removal in grade["removals"]:
                self.assertEqual("m64_03_eighth_slice_recipe_draft_model", removal["source"])

    def test_deferred_runtime_packages_never_promote_runtime(self):
        for item in self.report["repair_items"]:
            lock = item["lock_deferred_package"]
            legion = item["legion_deferred_package"]

            self.assertEqual("lock_runtime_support_deferred_decision", lock["repair_type"])
            self.assertEqual("legion_runtime_support_deferred_decision", legion["repair_type"])
            self.assertTrue(lock["advisory_only"])
            self.assertTrue(legion["advisory_only"])
            self.assertFalse(lock["can_be_repaired_in_m64_06"])
            self.assertFalse(legion["can_be_repaired_in_m64_06"])
            self.assertIn("Lock/Unlock runtime rules module", lock["requires_future_system_work"])
            self.assertIn("Legion/Mate declaration rules", legion["requires_future_system_work"])
            self.assertFalse(lock["runtime_promotion_allowed"])
            self.assertFalse(legion["runtime_promotion_allowed"])

    def test_next_target_is_m64_closeout(self):
        next_target = self.report["next_target"]

        self.assertEqual("M64-closeout", next_target["milestone"])
        self.assertEqual("Eighth-slice runtime readiness decision", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m64_06.json"
            md_path = out / "m64_06.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M64-06", loaded["version"])
            self.assertIn("M64-06 Eighth-Slice Blocker Repair Candidates", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
