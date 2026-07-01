"""Tests for tools/deck/build_third_slice_blocker_repair_candidates.py (M44-06)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_third_slice_blocker_repair_candidates import (  # noqa: E402
    M44_CONSISTENCY,
    M44_RECIPE_DRAFTS,
    M44_SCAFFOLD,
    M44_VALIDATION,
    build_third_slice_blocker_repair_candidates,
    load_json,
    write_json,
    write_markdown,
)


class TestThirdSliceBlockerRepairCandidates(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.recipes = load_json(M44_RECIPE_DRAFTS)
        cls.validation = load_json(M44_VALIDATION)
        cls.consistency = load_json(M44_CONSISTENCY)
        cls.scaffold = load_json(M44_SCAFFOLD)
        cls.report = build_third_slice_blocker_repair_candidates(
            cls.recipes,
            cls.validation,
            cls.consistency,
            cls.scaffold,
        )

    def test_repair_candidates_are_ready_for_closeout(self):
        summary = self.report["summary"]

        self.assertEqual("M44-06", self.report["version"])
        self.assertTrue(summary["ready_for_m44_closeout"])
        self.assertEqual(25, summary["recipe_count"])
        self.assertEqual(25, summary["manual_overlap_recipe_count"])
        self.assertEqual(25, summary["manual_repair_complete_count"])
        self.assertEqual(25, summary["grade_profile_repair_candidate_count"])
        self.assertEqual(25, summary["grade_profile_complete_candidate_count"])
        self.assertEqual(0, summary["grade_package_clears_manual_overlap_count"])
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
        self.assertIn("EB06", policy["series_scope_from_m44_01_fixture_scaffold"])
        self.assertIn("EB10", policy["series_scope_from_m44_01_fixture_scaffold"])
        self.assertFalse(policy["runtime_promotion_allowed"])

    def test_each_repair_item_has_manual_and_grade_packages(self):
        for item in self.report["repair_items"]:
            manual = item["manual_overlap_repair_package"]
            grade = item["grade_profile_repair_package"]

            self.assertEqual("blocked_by_manual_review", item["validation_status"])
            self.assertEqual("blocked_by_manual_review", item["consistency_status"])
            self.assertTrue(item["manual_review_card_ids"])
            self.assertTrue(manual["complete_candidate"])
            self.assertTrue(grade["complete_candidate"])
            self.assertFalse(item["grade_package_clears_manual_overlap"])
            self.assertTrue(item["ready_for_human_repair_review"])
            self.assertFalse(item["runtime_promotion_allowed"])

    def test_manual_packages_are_source_backed_previews(self):
        for item in self.report["repair_items"]:
            manual = item["manual_overlap_repair_package"]

            self.assertEqual("manual_review_overlap_substitution", manual["repair_type"])
            self.assertTrue(manual["advisory_only"])
            self.assertFalse(manual["runtime_promotion_allowed"])
            self.assertTrue(manual["substitutions"])
            for substitution in manual["substitutions"]:
                selected = substitution["selected_replacement"]
                self.assertIsNotNone(selected)
                self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", selected["source"])

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

    def test_next_target_is_m44_closeout(self):
        next_target = self.report["next_target"]

        self.assertEqual("M44-closeout", next_target["milestone"])
        self.assertEqual("Third-slice runtime readiness decision", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m44_06.json"
            md_path = out / "m44_06.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M44-06", loaded["version"])
            self.assertIn("M44-06 Third-Slice Blocker Repair Candidates", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
