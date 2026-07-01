"""Tests for tools/deck/build_fifth_slice_human_accepted_repair_artifact.py (M53-03)."""

from __future__ import annotations

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
    write_json,
    write_markdown,
)
from tools.deck.build_fifth_slice_human_selected_recipe_artifact import (  # noqa: E402
    M53_01_REVIEW,
    build_fifth_slice_human_selected_recipe_artifact,
)


SELECTED_REVIEW_ITEM_ID = "m53_01_m52_recipe_001_repair_review"
SELECTION_TEXT = "explicit test selection for m52_recipe_001"
ACCEPTANCE_TEXT = "explicit test acceptance for m52_recipe_001"


class TestFifthSliceHumanAcceptedRepairArtifact(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.review_packet = load_json(M53_01_REVIEW)
        cls.drafts = load_json(M52_DRAFTS)
        cls.selected_artifact = build_fifth_slice_human_selected_recipe_artifact(
            cls.review_packet,
            selected_review_item_id=SELECTED_REVIEW_ITEM_ID,
            selection_text=SELECTION_TEXT,
            selected_by="unit-test",
            selected_at="2026-06-30",
        )
        cls.report = build_fifth_slice_human_accepted_repair_artifact(
            cls.selected_artifact,
            cls.drafts,
            acceptance_text=ACCEPTANCE_TEXT,
            accepted_by="unit-test",
            accepted_at="2026-06-30",
        )

    def test_artifact_records_acceptance_for_selected_recipe(self):
        summary = self.report["summary"]
        record = self.report["acceptance_record"]

        self.assertEqual("M53-03", self.report["version"])
        self.assertEqual(SELECTED_REVIEW_ITEM_ID, summary["accepted_review_item_id"])
        self.assertEqual("m52_recipe_001", summary["accepted_recipe_id"])
        self.assertEqual("m52_recipe_001_grade_profile_pkg_001", summary["accepted_grade_profile_package_id"])
        self.assertTrue(summary["human_selection_recorded"])
        self.assertTrue(summary["human_acceptance_recorded"])
        self.assertEqual("accepted", record["decision"])
        self.assertEqual("unit-test", record["accepted_by"])
        self.assertEqual("2026-06-30", record["accepted_at"])
        self.assertEqual(ACCEPTANCE_TEXT, record["acceptance_text"])

    def test_accepted_repair_is_applied_but_not_validated(self):
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]

        self.assertEqual(50, summary["main_deck_count_after_repair"])
        self.assertEqual(0, summary["repair_application_issue_count"])
        self.assertTrue(summary["ready_for_m53_04"])
        self.assertEqual("BT14-003TH->BT12-053TH", repair["source_candidate_edge"])
        self.assertEqual("BT14-003TH", repair["pair"]["source_card_id"])
        self.assertEqual("BT12-053TH", repair["pair"]["target_card_id"])
        self.assertEqual(2, len(repair["grade_additions"]))
        self.assertEqual(2, len(repair["grade_removals"]))
        self.assertEqual(50, sum(row["quantity"] for row in repair["repaired_quantities"]))
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, repair["grade_counts_after_repair_package"])
        self.assertTrue(repair["requires_m53_04_validation"])
        self.assertTrue(repair["ready_for_m53_04_validation_rerun"])

    def test_scope_does_not_promote_or_declare_valid(self):
        scope = self.report["scope"]
        policy = self.report["acceptance_policy"]
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]

        self.assertTrue(scope["offline_human_accepted_artifact"])
        self.assertTrue(scope["records_human_selection"])
        self.assertTrue(scope["records_human_acceptance"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertFalse(scope["declares_recipe_valid"])
        self.assertTrue(policy["requires_m53_02_selection"])
        self.assertTrue(policy["requires_non_empty_acceptance_text"])
        self.assertTrue(policy["acceptance_is_not_validation"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertTrue(policy["m53_04_must_rerun_validation"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertFalse(summary["declares_recipe_valid"])
        self.assertFalse(repair["runtime_promotion_allowed"])

    def test_blank_acceptance_text_is_rejected(self):
        with self.assertRaises(ValueError) as context:
            build_fifth_slice_human_accepted_repair_artifact(
                self.selected_artifact,
                self.drafts,
                acceptance_text=" ",
            )

        self.assertIn("non-empty acceptance_text", str(context.exception))

    def test_invalid_selected_recipe_is_rejected(self):
        selected = json.loads(json.dumps(self.selected_artifact))
        selected["selection"]["recipe_id"] = "m52_missing_recipe"

        with self.assertRaises(ValueError) as context:
            build_fifth_slice_human_accepted_repair_artifact(
                selected,
                self.drafts,
                acceptance_text=ACCEPTANCE_TEXT,
            )

        self.assertIn("Recipe not found", str(context.exception))

    def test_next_target_is_m53_04(self):
        next_target = self.report["next_target"]

        self.assertEqual("M53-04", next_target["milestone"])
        self.assertEqual("Fifth-slice repaired recipe validation rerun", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m53_03.json"
            md_path = out / "m53_03.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M53-03", loaded["version"])
            self.assertEqual("m52_recipe_001", loaded["summary"]["accepted_recipe_id"])
            self.assertIn("M53-03 Fifth-Slice Human-Accepted Repair Artifact", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
