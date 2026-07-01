"""Tests for tools/deck/build_fifth_slice_human_selected_recipe_artifact.py (M53-02)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_fifth_slice_human_selected_recipe_artifact import (  # noqa: E402
    M53_01_REVIEW,
    build_fifth_slice_human_selected_recipe_artifact,
    load_json,
    write_json,
    write_markdown,
)


SELECTED_REVIEW_ITEM_ID = "m53_01_m52_recipe_001_repair_review"
SELECTION_TEXT = "explicit test selection for m52_recipe_001"


class TestFifthSliceHumanSelectedRecipeArtifact(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.review_packet = load_json(M53_01_REVIEW)
        cls.report = build_fifth_slice_human_selected_recipe_artifact(
            cls.review_packet,
            selected_review_item_id=SELECTED_REVIEW_ITEM_ID,
            selection_text=SELECTION_TEXT,
            selected_by="unit-test",
            selected_at="2026-06-30",
        )

    def test_explicit_selection_records_exactly_one_recipe(self):
        summary = self.report["summary"]

        self.assertEqual("M53-02", self.report["version"])
        self.assertEqual(25, summary["available_review_item_count"])
        self.assertEqual(1, summary["selected_review_item_count"])
        self.assertEqual("m52_recipe_001", summary["selected_recipe_id"])
        self.assertEqual("m52_recipe_001_grade_profile_pkg_001", summary["selected_grade_profile_package_id"])
        self.assertTrue(summary["records_human_selection"])
        self.assertFalse(summary["records_human_acceptance"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertTrue(summary["ready_for_m53_03"])

    def test_selection_carries_pair_and_grade_repair_context(self):
        selection = self.report["selection"]
        pair = selection["pair"]

        self.assertEqual(SELECTED_REVIEW_ITEM_ID, selection["selected_review_item_id"])
        self.assertEqual("m52_recipe_001", selection["recipe_id"])
        self.assertEqual("2026-06-30", selection["selected_at"])
        self.assertEqual("unit-test", selection["selected_by"])
        self.assertEqual(SELECTION_TEXT, selection["selection_text"])
        self.assertEqual("accept_recipe_and_grade_repair_for_validation_rerun", selection["selected_decision_option"])
        self.assertEqual("BT14-003TH->BT12-053TH", selection["source_candidate_edge"])
        self.assertEqual("BT14-003TH", pair["source_card_id"])
        self.assertEqual("BT12-053TH", pair["target_card_id"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, selection["grade_counts_after"])
        self.assertEqual(5, selection["grade_added_card_count"])
        self.assertEqual(5, selection["grade_removed_card_count"])
        self.assertTrue(selection["grade_repair_complete"])
        self.assertTrue(selection["ready_for_human_repair_review"])
        self.assertEqual([], selection["structural_blockers"])

    def test_scope_and_policy_block_acceptance_and_runtime(self):
        scope = self.report["scope"]
        policy = self.report["selection_policy"]

        self.assertTrue(scope["offline_human_selected_artifact"])
        self.assertTrue(scope["records_human_selection"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertTrue(policy["requires_explicit_review_item_id"])
        self.assertTrue(policy["requires_non_empty_selection_text"])
        self.assertTrue(policy["exactly_one_recipe_selected"])
        self.assertTrue(policy["selection_is_not_acceptance"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertTrue(policy["m53_03_must_record_explicit_acceptance"])
        self.assertTrue(policy["m53_04_must_rerun_validation"])

    def test_blank_review_item_id_is_rejected(self):
        with self.assertRaises(ValueError) as context:
            build_fifth_slice_human_selected_recipe_artifact(
                self.review_packet,
                selected_review_item_id=" ",
                selection_text=SELECTION_TEXT,
            )

        self.assertIn("explicit non-empty review_item_id", str(context.exception))

    def test_invalid_review_item_id_is_rejected(self):
        with self.assertRaises(ValueError) as context:
            build_fifth_slice_human_selected_recipe_artifact(
                self.review_packet,
                selected_review_item_id="m53_01_missing_review",
                selection_text=SELECTION_TEXT,
            )

        self.assertIn("Review item not found", str(context.exception))

    def test_blank_selection_text_is_rejected(self):
        with self.assertRaises(ValueError) as context:
            build_fifth_slice_human_selected_recipe_artifact(
                self.review_packet,
                selected_review_item_id=SELECTED_REVIEW_ITEM_ID,
                selection_text=" ",
            )

        self.assertIn("non-empty selection_text", str(context.exception))

    def test_next_target_is_m53_03(self):
        next_target = self.report["next_target"]

        self.assertEqual("M53-03", next_target["milestone"])
        self.assertEqual("Fifth-slice human-accepted repair artifact", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m53_02.json"
            md_path = out / "m53_02.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M53-02", loaded["version"])
            self.assertEqual("m52_recipe_001", loaded["summary"]["selected_recipe_id"])
            self.assertIn("M53-02 Fifth-Slice Human-Selected Recipe Artifact", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
