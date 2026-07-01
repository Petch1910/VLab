"""Tests for tools/deck/build_fifth_slice_blocker_repair_candidates.py (M52-06)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_fifth_slice_blocker_repair_candidates import (  # noqa: E402
    M52_CONSISTENCY,
    M52_RECIPE_DRAFTS,
    M52_SCAFFOLD,
    M52_VALIDATION,
    build_fifth_slice_blocker_repair_candidates,
    load_json,
    write_json,
    write_markdown,
)


class TestFifthSliceBlockerRepairCandidates(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.recipes = load_json(M52_RECIPE_DRAFTS)
        cls.validation = load_json(M52_VALIDATION)
        cls.consistency = load_json(M52_CONSISTENCY)
        cls.scaffold = load_json(M52_SCAFFOLD)
        cls.report = build_fifth_slice_blocker_repair_candidates(
            cls.recipes,
            cls.validation,
            cls.consistency,
            cls.scaffold,
        )

    def test_repair_candidates_cover_all_fifth_slice_drafts(self):
        summary = self.report["summary"]

        self.assertEqual("M52-06", self.report["version"])
        self.assertTrue(summary["ready_for_m52_closeout"])
        self.assertEqual(25, summary["recipe_count"])
        self.assertEqual(25, summary["grade_profile_repair_candidate_count"])
        self.assertEqual(25, summary["grade_profile_complete_candidate_count"])
        self.assertEqual(25, summary["human_selection_required_count"])
        self.assertEqual(25, summary["ready_for_human_repair_review_count"])
        self.assertEqual(0, summary["unexpected_structural_blocker_recipe_count"])

    def test_scope_keeps_repair_candidates_offline_only(self):
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
        self.assertTrue(policy["grade_profile_candidates_are_previews"])
        self.assertTrue(policy["human_selection_is_not_recorded_here"])
        self.assertFalse(policy["runtime_promotion_allowed"])

    def test_each_repair_item_is_ready_for_human_review_not_runtime(self):
        for item in self.report["repair_items"]:
            self.assertEqual("validator_passed_pending_human_selection", item["validation_status"])
            self.assertEqual("consistent_pending_human_selection", item["consistency_status"])
            self.assertEqual([], item["structural_blockers"])
            self.assertTrue(item["human_selection_required"])
            self.assertTrue(item["ready_for_human_repair_review"])
            self.assertFalse(item["runtime_promotion_allowed"])

    def test_grade_profile_package_balances_classic_targets(self):
        for item in self.report["repair_items"]:
            package = item["grade_profile_repair_package"]

            self.assertEqual("grade_profile_substitution_preview", package["repair_type"])
            self.assertTrue(package["advisory_only"])
            self.assertTrue(package["complete_candidate"])
            self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, package["target_grade_counts"])
            self.assertEqual(package["target_grade_counts"], package["grade_counts_after"])
            self.assertEqual(package["added_card_count"], package["removed_card_count"])
            self.assertGreater(package["added_card_count"], 0)
            self.assertFalse(package["runtime_promotion_allowed"])

    def test_grade_profile_candidates_are_source_backed(self):
        recipe_pairs = {
            recipe["recipe_id"]: {
                recipe["pair"]["source_card_id"],
                recipe["pair"]["target_card_id"],
            }
            for recipe in self.recipes["recipe_drafts"]
        }

        for item in self.report["repair_items"]:
            package = item["grade_profile_repair_package"]
            for addition in package["additions"]:
                self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", addition["source"])
                self.assertGreater(addition["quantity"], 0)
                self.assertLessEqual(addition["final_quantity_if_chosen"], addition["deck_limit"])
            for removal in package["removals"]:
                self.assertEqual("m52_03_fifth_slice_recipe_draft_model", removal["source"])
                self.assertNotIn(removal["card_id"], recipe_pairs[item["recipe_id"]])

    def test_next_target_is_m52_closeout(self):
        next_target = self.report["next_target"]

        self.assertEqual("M52-closeout", next_target["milestone"])
        self.assertEqual("Fifth-slice runtime readiness decision", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m52_06.json"
            md_path = out / "m52_06.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M52-06", loaded["version"])
            self.assertIn(
                "M52-06 Fifth-Slice Blocker Repair Candidates",
                md_path.read_text(encoding="utf-8"),
            )


if __name__ == "__main__":
    unittest.main()
