"""Tests for tools/deck/build_fifth_slice_repaired_recipe_validation_rerun.py (M53-04)."""

from __future__ import annotations

import copy
import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_fifth_slice_human_accepted_repair_artifact import (  # noqa: E402
    M52_DRAFTS,
    build_fifth_slice_human_accepted_repair_artifact,
    load_json,
)
from tools.deck.build_fifth_slice_human_selected_recipe_artifact import (  # noqa: E402
    M53_01_REVIEW,
    build_fifth_slice_human_selected_recipe_artifact,
)
from tools.deck.build_fifth_slice_repaired_recipe_validation_rerun import (  # noqa: E402
    build_fifth_slice_repaired_recipe_validation_rerun,
    write_json,
    write_markdown,
)


SELECTED_REVIEW_ITEM_ID = "m53_01_m52_recipe_001_repair_review"
SELECTION_TEXT = "explicit test selection for m52_recipe_001"
ACCEPTANCE_TEXT = "explicit test acceptance for m52_recipe_001"


class TestFifthSliceRepairedRecipeValidationRerun(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        review_packet = load_json(M53_01_REVIEW)
        drafts = load_json(M52_DRAFTS)
        selected_artifact = build_fifth_slice_human_selected_recipe_artifact(
            review_packet,
            selected_review_item_id=SELECTED_REVIEW_ITEM_ID,
            selection_text=SELECTION_TEXT,
            selected_by="unit-test",
            selected_at="2026-06-30",
        )
        cls.accepted_artifact = build_fifth_slice_human_accepted_repair_artifact(
            selected_artifact,
            drafts,
            acceptance_text=ACCEPTANCE_TEXT,
            accepted_by="unit-test",
            accepted_at="2026-06-30",
        )
        cls.report = build_fifth_slice_repaired_recipe_validation_rerun(cls.accepted_artifact)

    def test_rerun_validates_repaired_recipe_and_routes_to_m53_05(self):
        summary = self.report["summary"]

        self.assertEqual("M53-04", self.report["version"])
        self.assertEqual("m52_recipe_001", summary["accepted_recipe_id"])
        self.assertEqual("validator_passed", summary["validation_status"])
        self.assertEqual("consistent_validator_passed", summary["consistency_status"])
        self.assertEqual(1, summary["runtime_ready_recipe_count"])
        self.assertEqual(1, summary["promotion_allowed_count"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertEqual(0, summary["review_issue_count"])
        self.assertFalse(summary["runtime_fixture_promotion_allowed"])
        self.assertTrue(summary["ready_for_m53_05"])

    def test_validation_counts_are_classic_complete(self):
        validation = self.report["repaired_recipe_validation"]
        counts = validation["count_summary"]

        self.assertEqual("validator_passed", validation["validation_status"])
        self.assertTrue(validation["runtime_ready"])
        self.assertEqual([], validation["blocker_codes"])
        self.assertEqual([], validation["review_codes"])
        self.assertEqual(50, counts["explicit_card_count"])
        self.assertEqual(16, counts["trigger_count"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, counts["trigger_counts"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, counts["grade_counts"])

    def test_consistency_passes_but_runtime_fixture_gate_is_separate(self):
        consistency = self.report["repaired_recipe_consistency"]
        scope = self.report["scope"]

        self.assertEqual("m52_recipe_001", consistency["recipe_id"])
        self.assertEqual("consistent_validator_passed", consistency["consistency_status"])
        self.assertTrue(consistency["pair_cards_present"])
        self.assertTrue(consistency["promotion_allowed_by_validation_and_consistency"])
        self.assertEqual("BT14-003TH", consistency["source_card_id"])
        self.assertEqual("BT12-053TH", consistency["target_card_id"])
        self.assertTrue(scope["offline_in_memory_validation_rerun"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertFalse(scope["declares_runtime_fixture_ready"])

    def test_accepted_context_is_carried_forward(self):
        context = self.report["accepted_context"]

        self.assertEqual(SELECTED_REVIEW_ITEM_ID, context["accepted_review_item_id"])
        self.assertEqual("m52_recipe_001", context["accepted_recipe_id"])
        self.assertEqual("m52_recipe_001_grade_profile_pkg_001", context["accepted_grade_profile_package_id"])
        self.assertTrue(context["human_selection_recorded"])
        self.assertTrue(context["human_acceptance_recorded"])
        self.assertTrue(context["accepted_artifact_ready_for_validation"])

    def test_unready_accepted_artifact_blocks_m53_05(self):
        accepted = copy.deepcopy(self.accepted_artifact)
        accepted["summary"]["ready_for_m53_04"] = False

        report = build_fifth_slice_repaired_recipe_validation_rerun(accepted)

        self.assertEqual("validator_passed", report["summary"]["validation_status"])
        self.assertEqual("consistent_validator_passed", report["summary"]["consistency_status"])
        self.assertFalse(report["summary"]["ready_for_m53_05"])

    def test_next_target_is_m53_05(self):
        next_target = self.report["next_target"]

        self.assertEqual("M53-05", next_target["milestone"])
        self.assertEqual("Fifth-slice runtime fixture promotion gate", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m53_04.json"
            md_path = out / "m53_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M53-04", loaded["version"])
            self.assertTrue(loaded["summary"]["ready_for_m53_05"])
            self.assertIn("M53-04 Fifth-Slice Repaired Recipe Validation Rerun", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
