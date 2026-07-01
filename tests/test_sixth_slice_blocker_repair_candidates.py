"""Tests for tools/deck/build_sixth_slice_blocker_repair_candidates.py (M56-06)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_sixth_slice_blocker_repair_candidates import (  # noqa: E402
    M56_CONSISTENCY,
    M56_RECIPE_DRAFTS,
    M56_SCAFFOLD,
    M56_VALIDATION,
    build_sixth_slice_blocker_repair_candidates,
    load_json,
    write_json,
    write_markdown,
)


class TestSixthSliceBlockerRepairCandidates(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.recipes = load_json(M56_RECIPE_DRAFTS)
        cls.validation = load_json(M56_VALIDATION)
        cls.consistency = load_json(M56_CONSISTENCY)
        cls.scaffold = load_json(M56_SCAFFOLD)
        cls.report = build_sixth_slice_blocker_repair_candidates(
            cls.recipes,
            cls.validation,
            cls.consistency,
            cls.scaffold,
        )

    def test_repair_candidates_are_ready_for_closeout(self):
        summary = self.report["summary"]

        self.assertEqual("M56-06", self.report["version"])
        self.assertTrue(summary["ready_for_m56_closeout"])
        self.assertEqual(12, summary["recipe_count"])
        self.assertEqual(12, summary["manual_overlap_recipe_count"])
        self.assertEqual(12, summary["manual_repair_complete_count"])
        self.assertEqual(12, summary["grade_profile_repair_candidate_count"])
        self.assertEqual(12, summary["grade_profile_complete_candidate_count"])
        self.assertEqual(0, summary["grade_package_clears_manual_overlap_count"])
        self.assertEqual(12, summary["g_zone_deferred_recipe_count"])
        self.assertEqual(0, summary["unexpected_structural_blocker_recipe_count"])
        self.assertEqual(12, summary["ready_for_human_repair_review_count"])
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
        self.assertTrue(policy["g_zone_support_is_deferred_system_work"])
        self.assertIn("G-TD10", policy["series_scope_from_m56_01_fixture_scaffold"])
        self.assertIn("G-BT14", policy["series_scope_from_m56_01_fixture_scaffold"])
        self.assertFalse(policy["runtime_promotion_allowed"])

    def test_each_repair_item_has_manual_and_g_zone_context(self):
        for item in self.report["repair_items"]:
            manual = item["manual_overlap_repair_package"]
            g_zone = item["g_zone_deferred_package"]

            self.assertEqual("blocked_by_manual_review", item["validation_status"])
            self.assertEqual("blocked_by_manual_review", item["consistency_status"])
            self.assertTrue(item["manual_review_card_ids"])
            self.assertEqual([], item["structural_blockers"])
            self.assertTrue(manual["complete_candidate"])
            self.assertIsNotNone(g_zone)
            self.assertFalse(g_zone["can_be_repaired_in_m56_06"])
            self.assertTrue(item["blocked_by_g_zone_deferred"])
            self.assertTrue(item["ready_for_human_repair_review"])
            self.assertFalse(item["runtime_promotion_allowed"])

    def test_manual_packages_avoid_all_manual_review_cards_as_replacements(self):
        global_manual_ids = {
            card_id
            for item in self.report["repair_items"]
            for card_id in item["manual_review_card_ids"]
        }

        for item in self.report["repair_items"]:
            manual = item["manual_overlap_repair_package"]

            self.assertEqual("manual_review_overlap_substitution_preview", manual["repair_type"])
            self.assertTrue(manual["advisory_only"])
            self.assertTrue(manual["manual_review_may_accept_original_card"])
            self.assertFalse(manual["runtime_promotion_allowed"])
            for substitution in manual["substitutions"]:
                selected = substitution["selected_replacement"]
                self.assertIsNotNone(selected)
                self.assertNotIn(selected["card_id"], global_manual_ids)
                self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", selected["source"])

    def test_grade_packages_are_source_backed_previews(self):
        target = {"0": 17, "1": 14, "2": 11, "3": 8}

        for item in self.report["repair_items"]:
            grade = item["grade_profile_repair_package"]

            self.assertEqual("grade_profile_substitution_preview", grade["repair_type"])
            self.assertTrue(grade["advisory_only"])
            self.assertFalse(grade["runtime_promotion_allowed"])
            self.assertEqual(target, grade["grade_counts_after"])
            self.assertEqual(grade["added_card_count"], grade["removed_card_count"])
            self.assertTrue(grade["complete_candidate"])
            for addition in grade["additions"]:
                self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", addition["source"])

    def test_g_zone_package_never_promotes_runtime(self):
        for item in self.report["repair_items"]:
            g_zone = item["g_zone_deferred_package"]

            self.assertEqual("g_zone_support_deferred_decision", g_zone["repair_type"])
            self.assertTrue(g_zone["advisory_only"])
            self.assertTrue(g_zone["main_deck_validation_only"])
            self.assertIn("G Zone deck slot model", g_zone["requires_future_system_work"])
            self.assertFalse(g_zone["runtime_promotion_allowed"])

    def test_next_target_is_m56_closeout(self):
        next_target = self.report["next_target"]

        self.assertEqual("M56-closeout", next_target["milestone"])
        self.assertEqual("Sixth-slice runtime readiness decision", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m56_06.json"
            md_path = out / "m56_06.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M56-06", loaded["version"])
            self.assertIn("M56-06 Sixth-Slice Blocker Repair Candidates", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
