"""Tests for tools/deck/validate_third_slice_repaired_recipe.py (M45-03)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.validate_third_slice_repaired_recipe import (  # noqa: E402
    M45_02_ACCEPTED_REPAIR,
    build_third_slice_repaired_validation_report,
    load_json,
    write_json,
    write_markdown,
)


class TestThirdSliceRepairedRecipeValidation(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.accepted_repair = load_json(M45_02_ACCEPTED_REPAIR)
        cls.report = build_third_slice_repaired_validation_report(cls.accepted_repair)

    def test_repaired_recipe_validation_passes(self):
        summary = self.report["summary"]

        self.assertEqual("M45-03", self.report["version"])
        self.assertEqual("m44_recipe_001", summary["accepted_recipe_id"])
        self.assertTrue(summary["human_acceptance_recorded"])
        self.assertEqual(1, summary["recipe_count"])
        self.assertEqual(1, summary["runtime_ready_recipe_count"])
        self.assertEqual(1, summary["validator_passed_count"])
        self.assertEqual(0, summary["invalid_draft_count"])
        self.assertEqual(0, summary["blocked_by_manual_review_count"])
        self.assertTrue(summary["ready_for_m45_04"])

    def test_repaired_recipe_has_no_validation_blockers(self):
        summary = self.report["summary"]
        validation = self.report["recipe_validations"][0]
        counts = validation["count_summary"]

        self.assertEqual("validator_passed", validation["validation_status"])
        self.assertTrue(validation["runtime_ready"])
        self.assertEqual(0, validation["blocking_issue_count"])
        self.assertEqual([], validation["issues"])
        self.assertEqual(50, counts["explicit_card_count"])
        self.assertEqual(16, counts["trigger_count"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, counts["trigger_counts"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, counts["grade_counts"])

    def test_issue_counts_are_clear_for_fixture_gate(self):
        summary = self.report["summary"]

        self.assertEqual(0, summary["missing_card_recipe_count"])
        self.assertEqual(0, summary["copy_limit_violation_recipe_count"])
        self.assertEqual(0, summary["slot_gap_recipe_count"])
        self.assertEqual(0, summary["trigger_count_mismatch_recipe_count"])
        self.assertEqual(0, summary["manual_review_overlap_recipe_count"])
        self.assertEqual(0, summary["grade_profile_review_recipe_count"])
        self.assertEqual({}, summary["issue_counts"])

    def test_scope_does_not_promote_runtime_fixture(self):
        scope = self.report["scope"]
        policy = self.report["validation_policy"]
        summary = self.report["summary"]

        self.assertTrue(scope["offline_validation_rerun"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["changes_accepted_repair_artifact"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["automatic_deck_injection"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertEqual("m45_02_repaired_quantity_preview", policy["validated_input"])
        self.assertTrue(policy["human_acceptance_required"])
        self.assertTrue(policy["runtime_fixture_gate_still_required"])
        self.assertFalse(summary["runtime_fixture_created"])
        self.assertFalse(summary["runtime_promotion_allowed"])

    def test_next_target_is_m45_04(self):
        next_target = self.report["next_target"]

        self.assertEqual("M45-04", next_target["milestone"])
        self.assertEqual("Third-slice runtime fixture promotion gate", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m45_03.json"
            md_path = out / "m45_03.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M45-03", loaded["version"])
            self.assertIn("M45-03 Third-Slice Repaired Recipe Validation Rerun", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
