"""Tests for tools/deck/build_second_slice_blocker_repair_candidates.py (M40-05)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_second_slice_blocker_repair_candidates import (  # noqa: E402
    M40_CONSISTENCY,
    M40_RECIPE_DRAFTS,
    M40_VALIDATION,
    build_second_slice_blocker_repair_candidates,
    load_json,
    write_json,
    write_markdown,
)


class TestSecondSliceBlockerRepairCandidates(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.recipes = load_json(M40_RECIPE_DRAFTS)
        cls.validation = load_json(M40_VALIDATION)
        cls.consistency = load_json(M40_CONSISTENCY)
        cls.report = build_second_slice_blocker_repair_candidates(
            cls.recipes,
            cls.validation,
            cls.consistency,
        )

    def test_repair_candidates_are_ready_for_closeout(self):
        summary = self.report["summary"]

        self.assertEqual("M40-05", self.report["version"])
        self.assertTrue(summary["ready_for_m40_closeout"])
        self.assertEqual(25, summary["recipe_count"])
        self.assertEqual(25, summary["manual_overlap_recipe_count"])
        self.assertEqual(0, summary["manual_repair_complete_count"])
        self.assertEqual(25, summary["grade_profile_repair_candidate_count"])
        self.assertEqual(25, summary["grade_profile_complete_candidate_count"])
        self.assertEqual(25, summary["grade_package_clears_manual_overlap_count"])
        self.assertEqual(25, summary["ready_for_human_repair_review_count"])
        self.assertFalse(summary["runtime_promotion_allowed"])

    def test_scope_is_offline_repair_only(self):
        scope = self.report["scope"]
        policy = self.report["repair_policy"]

        self.assertTrue(scope["offline_repair_candidates"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["runtime_deck"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertTrue(policy["source_backed_candidates"])
        self.assertTrue(policy["substitution_only"])
        self.assertFalse(policy["runtime_promotion_allowed"])

    def test_each_repair_item_has_manual_and_grade_packages(self):
        for item in self.report["repair_items"]:
            manual = item["manual_overlap_repair_package"]
            grade = item["grade_profile_repair_package"]

            self.assertEqual("blocked_by_manual_review", item["validation_status"])
            self.assertEqual("blocked_by_manual_review", item["consistency_status"])
            self.assertTrue(item["manual_review_card_ids"])
            self.assertFalse(manual["complete_candidate"])
            self.assertTrue(grade["complete_candidate"])
            self.assertTrue(item["grade_package_clears_manual_overlap"])
            self.assertTrue(item["ready_for_human_repair_review"])
            self.assertFalse(item["runtime_promotion_allowed"])

    def test_grade_packages_hit_classic_profile_by_substitution(self):
        target = {"0": 17, "1": 14, "2": 11, "3": 8}

        for item in self.report["repair_items"]:
            grade = item["grade_profile_repair_package"]

            self.assertEqual(target, grade["grade_counts_after"])
            self.assertEqual(grade["added_card_count"], grade["removed_card_count"])
            self.assertEqual("grade_profile_substitution", grade["repair_type"])
            self.assertFalse(grade["runtime_promotion_allowed"])
            self.assertTrue(grade["additions"])
            self.assertTrue(grade["removals"])
            for addition in grade["additions"]:
                self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", addition["source"])

    def test_next_target_is_m40_closeout(self):
        next_target = self.report["next_target"]

        self.assertEqual("M40-closeout", next_target["milestone"])
        self.assertEqual("Second-slice runtime readiness decision", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m40_05.json"
            md_path = out / "m40_05.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M40-05", loaded["version"])
            self.assertIn("M40-05 Second-Slice Blocker Repair Candidates", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
