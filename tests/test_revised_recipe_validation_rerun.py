"""Tests for tools/deck/build_revised_recipe_validation_rerun.py (M37-05)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_revised_recipe_validation_rerun import (  # noqa: E402
    COMBO_LINES,
    M37_02_REPAIR,
    M37_04_MAPPINGS,
    ORIGINAL_VALIDATION,
    RECIPE_DRAFTS,
    REVIEW_PACKET,
    build_revised_validation_rerun,
    load_json,
    write_json,
    write_markdown,
)


class TestRevisedRecipeValidationRerun(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = build_revised_validation_rerun(
            load_json(RECIPE_DRAFTS),
            load_json(ORIGINAL_VALIDATION),
            load_json(COMBO_LINES),
            load_json(REVIEW_PACKET),
            load_json(M37_02_REPAIR),
            load_json(M37_04_MAPPINGS),
        )

    def test_report_reruns_accepted_seed_in_memory(self):
        summary = self.report["summary"]

        self.assertEqual("M37-05", self.report["version"])
        self.assertEqual("recipe_003", summary["recipe_id"])
        self.assertTrue(summary["ready_for_m37_closeout"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertEqual(3, summary["resolved_blocker_count"])
        self.assertEqual(0, summary["remaining_blocker_count"])

    def test_original_validation_was_invalid_with_three_blockers(self):
        before = self.report["accepted_seed_before"]

        self.assertEqual("invalid_draft", before["validation_status"])
        self.assertEqual(3, before["blocking_issue_count"])
        self.assertEqual(
            ["main_deck_size_mismatch", "trigger_count_mismatch", "unfilled_slots"],
            before["blocker_codes"],
        )
        self.assertEqual(38, before["count_summary"]["explicit_card_count"])
        self.assertEqual({"Stand": 4}, before["count_summary"]["trigger_counts"])

    def test_revised_validation_clears_blockers_but_not_human_review(self):
        after = self.report["accepted_seed_after"]

        self.assertEqual("validator_passed_pending_human_acceptance", after["validation_status"])
        self.assertEqual(0, after["blocking_issue_count"])
        self.assertEqual([], after["blocker_codes"])
        self.assertEqual(["grade_profile_review", "human_acceptance_pending"], after["review_codes"])
        self.assertFalse(after["runtime_ready"])
        self.assertEqual("consistent_pending_human_acceptance", after["consistency_status"])
        self.assertFalse(after["promotion_allowed"])

    def test_revised_counts_are_trigger_complete(self):
        after = self.report["accepted_seed_after"]
        counts = after["count_summary"]

        self.assertEqual(50, counts["explicit_card_count"])
        self.assertEqual(16, counts["trigger_count"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, counts["trigger_counts"])
        self.assertEqual({"0": 16, "2": 6, "3": 28}, counts["grade_counts"])

    def test_validation_summary_after_reflects_one_pending_recipe(self):
        summary = self.report["validation_summary_after"]

        self.assertEqual(25, summary["recipe_count"])
        self.assertEqual(0, summary["runtime_ready_recipe_count"])
        self.assertEqual(0, summary["validator_passed_count"])
        self.assertEqual(1, summary["validator_passed_pending_human_acceptance_count"])
        self.assertEqual(0, summary["invalid_draft_count"])
        self.assertEqual(24, summary["blocked_by_review_count"])
        self.assertEqual(15, summary["slot_gap_recipe_count"])
        self.assertEqual(11, summary["trigger_count_mismatch_recipe_count"])

    def test_consistency_summary_after_still_blocks_promotion(self):
        summary = self.report["consistency_summary_after"]

        self.assertEqual(25, summary["consistency_check_count"])
        self.assertEqual(25, summary["combo_cards_present_count"])
        self.assertEqual(0, summary["promotion_allowed_count"])
        self.assertEqual(0, summary["runtime_ready_consistent_count"])
        self.assertEqual(
            {"blocked_by_review": 24, "consistent_pending_human_acceptance": 1},
            summary["status_counts"],
        )

    def test_repair_context_is_non_runtime(self):
        repair = self.report["repair_applied_in_memory"]
        scope = self.report["scope"]

        self.assertEqual("m37_01_pkg_001", repair["package_id"])
        self.assertEqual("balanced_classic", repair["profile_id"])
        self.assertEqual(3, len(repair["quantity_delta"]))
        self.assertEqual(5, repair["mapping_candidate_count"])
        self.assertFalse(repair["human_acceptance_present"])
        self.assertTrue(scope["offline_in_memory_rerun"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_deck"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_playbook_promotion"])
        self.assertFalse(scope["live_card_text_parsing"])

    def test_next_target_is_m37_closeout(self):
        next_target = self.report["next_target"]

        self.assertEqual("M37-closeout", next_target["milestone"])
        self.assertEqual("First runtime-ready recipe decision", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m37_05.json"
            md_path = out / "m37_05.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M37-05", loaded["version"])
            self.assertIn("M37-05 Revised Recipe Validation Rerun", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
