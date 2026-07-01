"""Tests for tools/deck/build_deck_recipe_validation_closeout.py (M36-closeout)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_deck_recipe_validation_closeout import (  # noqa: E402
    MILESTONE_INPUTS,
    build_closeout,
    load_json,
    write_json,
    write_markdown,
)


class TestDeckRecipeValidationCloseout(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.reports = {item.milestone: load_json(item.path) for item in MILESTONE_INPUTS}
        cls.report = build_closeout(cls.reports)

    def test_m36_closeout_is_closed_but_not_promoted(self):
        decision = self.report["decision"]

        self.assertEqual("M36-closeout", self.report["version"])
        self.assertTrue(decision["m36_deck_recipe_validation_closed"])
        self.assertFalse(decision["runtime_recipe_promotion_allowed"])
        self.assertFalse(decision["broader_slice_scaleout_allowed"])
        self.assertEqual("M37", decision["recommended_next_milestone"])

    def test_scope_remains_offline_only(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_coordination_artifact"])
        self.assertFalse(scope["changes_runtime_gameplay"])
        self.assertFalse(scope["changes_unity_ui"])
        self.assertFalse(scope["creates_runtime_deck"])
        self.assertFalse(scope["enables_bot_runtime"])

    def test_m36_key_counts_are_preserved(self):
        results = self.report["key_results"]

        self.assertEqual(31, results["review_packet"]["total_review_item_count"])
        self.assertEqual(1, results["review_packet"]["accepted_seed_item_count"])
        self.assertEqual(24, results["review_packet"]["rejected_line_item_count"])
        self.assertEqual(6, results["review_packet"]["manual_card_item_count"])
        self.assertEqual(25, results["recipe_drafts"]["recipe_draft_count"])
        self.assertEqual(1, results["recipe_drafts"]["accepted_seed_recipe_count"])

    def test_validation_blockers_are_visible(self):
        validation = self.report["key_results"]["validation"]

        self.assertEqual(0, validation["runtime_ready_recipe_count"])
        self.assertEqual(0, validation["validator_passed_count"])
        self.assertEqual(1, validation["invalid_draft_count"])
        self.assertEqual(24, validation["blocked_by_review_count"])
        self.assertEqual(0, validation["missing_card_recipe_count"])
        self.assertEqual(0, validation["copy_limit_violation_recipe_count"])
        self.assertEqual(16, validation["slot_gap_recipe_count"])
        self.assertEqual(12, validation["trigger_count_mismatch_recipe_count"])

    def test_combo_consistency_blocks_runtime_promotion(self):
        consistency = self.report["key_results"]["consistency"]

        self.assertEqual(25, consistency["combo_line_count"])
        self.assertEqual(25, consistency["combo_cards_present_count"])
        self.assertEqual(0, consistency["missing_combo_card_check_count"])
        self.assertEqual(0, consistency["manual_review_dependency_check_count"])
        self.assertEqual(0, consistency["promotion_allowed_count"])
        self.assertEqual(0, consistency["runtime_ready_consistent_count"])

    def test_second_slice_is_future_ready_only(self):
        second = self.report["key_results"]["second_slice"]

        self.assertTrue(second["future_recipe_pipeline_ready"])
        self.assertFalse(second["broader_scaleout_runtime_allowed"])
        self.assertEqual(259, second["probe_candidate_edges"])
        self.assertEqual(
            "second_slice_semantic_ready_but_hold_recipe_drafting_until_first_slice_review_blockers_clear",
            second["recommendation"],
        )

    def test_next_queue_targets_m37_blocker_resolution(self):
        next_queue = self.report["next_queue_selection"]

        self.assertEqual("M37", next_queue["milestone"])
        self.assertEqual("First-slice blocker resolution and recipe repair", next_queue["name"])
        task_ids = [task["id"] for task in next_queue["first_tasks"]]
        self.assertEqual("M37-01", task_ids[0])
        self.assertIn("M37-05", task_ids)
        self.assertIn("M37-closeout", task_ids)
        self.assertIn(
            "no runtime deck promotion until validator_passed and human_acceptance are both true",
            next_queue["hard_gates"],
        )

    def test_blocker_summary_lists_actionable_blockers(self):
        blockers = self.report["blocker_summary"]["blockers"]
        blocker_ids = {blocker["id"] for blocker in blockers}

        self.assertTrue(self.report["blocker_summary"]["runtime_promotion_blocked"])
        self.assertIn("human_review_blocked_lines", blocker_ids)
        self.assertIn("unfilled_recipe_slots", blocker_ids)
        self.assertIn("trigger_count_mismatch", blocker_ids)
        self.assertIn("invalid_draft", blocker_ids)
        self.assertIn("no_promotable_combo_line", blocker_ids)

    def test_every_input_check_points_to_existing_output(self):
        checks = self.report["input_checks"]

        self.assertEqual(len(MILESTONE_INPUTS), len(checks))
        for check in checks:
            self.assertTrue(check["exists"], check["path"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "closeout.json"
            md_path = out / "closeout.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M36-closeout", loaded["version"])
            self.assertIn("M36 Deck Recipe Validation Closeout", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
