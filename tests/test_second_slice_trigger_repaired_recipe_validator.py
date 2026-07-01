"""Tests for tools/deck/validate_second_slice_trigger_repaired_recipe.py."""

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

from tools.deck.validate_second_slice_trigger_repaired_recipe import (  # noqa: E402
    M41_TRIGGER_REPAIR_ACCEPTED,
    build_second_slice_trigger_repaired_recipe_validation_report,
    load_json,
    write_json,
    write_markdown,
)


class TestSecondSliceTriggerRepairedRecipeValidator(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.accepted = load_json(M41_TRIGGER_REPAIR_ACCEPTED)
        cls.report = build_second_slice_trigger_repaired_recipe_validation_report(cls.accepted)

    def test_trigger_repaired_recipe_passes_validation(self):
        summary = self.report["summary"]

        self.assertEqual("M41-repair-validate", self.report["version"])
        self.assertEqual("m40_recipe_001", summary["recipe_id"])
        self.assertEqual("validator_passed", summary["validation_status"])
        self.assertTrue(summary["runtime_ready"])
        self.assertFalse(summary["promotion_allowed"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertEqual(50, summary["main_deck_count"])
        self.assertEqual(16, summary["trigger_count"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, summary["grade_counts"])
        self.assertTrue(summary["manual_review_card_overlap_cleared"])
        self.assertTrue(summary["ready_for_m41_04"])
        self.assertFalse(summary["ready_for_repair_loop"])

    def test_trigger_profile_is_balanced_and_source_accepted(self):
        validation = self.report["recipe_validation"]
        acceptance = self.report["trigger_repair_acceptance"]

        self.assertEqual([], validation["issues"])
        self.assertEqual(
            {"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4},
            validation["count_summary"]["trigger_counts"],
        )
        self.assertEqual([], validation["count_summary"]["manual_review_card_ids_present"])
        self.assertEqual("m41_repair_pkg_001", acceptance["accepted_package_id"])
        self.assertEqual("balanced_classic_trigger_restore", acceptance["accepted_profile_id"])
        self.assertTrue(acceptance["human_acceptance_recorded"])
        self.assertTrue(acceptance["ready_for_validation_rerun"])

    def test_scope_stays_offline_and_non_mutating(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_validator"])
        self.assertFalse(scope["changes_accepted_artifact"])
        self.assertFalse(scope["changes_trigger_repair_acceptance_artifact"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])

    def test_passing_validation_routes_to_m41_04_gate_only(self):
        next_target = self.report["next_target"]

        self.assertEqual("M41-04", next_target["milestone"])
        self.assertEqual("Second-slice runtime fixture promotion gate", next_target["task"])
        self.assertFalse(self.report["recipe_validation"]["promotion_allowed"])

    def test_missing_acceptance_fails_and_routes_back_to_repair(self):
        accepted = copy.deepcopy(self.accepted)
        accepted["summary"]["human_acceptance_recorded"] = False
        accepted["summary"]["ready_for_validation_rerun"] = False
        accepted["acceptance_record"]["decision"] = "pending"

        report = build_second_slice_trigger_repaired_recipe_validation_report(accepted)
        codes = {issue["code"] for issue in report["recipe_validation"]["issues"]}

        self.assertIn("human_acceptance_missing", codes)
        self.assertFalse(report["summary"]["ready_for_m41_04"])
        self.assertEqual("M41-repair", report["next_target"]["milestone"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m41_repair_validate.json"
            md_path = out / "m41_repair_validate.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M41-repair-validate", loaded["version"])
            self.assertIn(
                "M41 Repair Validate Second-Slice Repaired Recipe Validation",
                md_path.read_text(encoding="utf-8"),
            )


if __name__ == "__main__":
    unittest.main()
