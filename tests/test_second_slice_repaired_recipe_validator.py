"""Tests for tools/deck/validate_second_slice_repaired_recipe.py (M41-03)."""

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

from tools.deck.validate_second_slice_repaired_recipe import (  # noqa: E402
    M41_02_ACCEPTED,
    build_second_slice_repaired_recipe_validation_report,
    load_card_rows,
    load_json,
    write_json,
    write_markdown,
)


class TestSecondSliceRepairedRecipeValidator(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.accepted = load_json(M41_02_ACCEPTED)
        card_ids = [row["card_id"] for row in cls.accepted["accepted_repair"]["repaired_quantities"]]
        cls.card_rows = load_card_rows(card_ids)
        cls.report = build_second_slice_repaired_recipe_validation_report(cls.accepted, cls.card_rows)

    def test_validation_rerun_detects_trigger_blocker(self):
        summary = self.report["summary"]

        self.assertEqual("M41-03", self.report["version"])
        self.assertEqual("m40_recipe_001", summary["recipe_id"])
        self.assertEqual("invalid_repaired_recipe", summary["validation_status"])
        self.assertFalse(summary["runtime_ready"])
        self.assertFalse(summary["promotion_allowed"])
        self.assertEqual(1, summary["blocking_issue_count"])
        self.assertEqual(50, summary["main_deck_count"])
        self.assertEqual(2, summary["trigger_count"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, summary["grade_counts"])
        self.assertTrue(summary["manual_review_card_overlap_cleared"])
        self.assertFalse(summary["ready_for_m41_04"])
        self.assertTrue(summary["ready_for_repair_loop"])

    def test_trigger_issue_is_explicit(self):
        validation = self.report["recipe_validation"]
        issues = validation["issues"]

        self.assertEqual(1, len(issues))
        self.assertEqual("trigger_count_mismatch", issues[0]["code"])
        self.assertEqual("blocker", issues[0]["severity"])
        self.assertEqual(16, issues[0]["details"]["expected"])
        self.assertEqual(2, issues[0]["details"]["actual"])
        self.assertEqual({"Draw": 2}, validation["count_summary"]["trigger_counts"])
        self.assertEqual([], validation["count_summary"]["manual_review_card_ids_present"])

    def test_scope_stays_offline_and_non_mutating(self):
        scope = self.report["scope"]
        policy = self.report["validation_policy"]

        self.assertTrue(scope["offline_validator"])
        self.assertFalse(scope["changes_accepted_artifact"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertEqual(50, policy["classic_main_deck_size"])
        self.assertEqual(16, policy["classic_trigger_count"])
        self.assertTrue(policy["runtime_promotion_requires_zero_blockers"])

    def test_failed_validation_routes_to_repair_loop(self):
        next_target = self.report["next_target"]

        self.assertEqual("M41-repair", next_target["milestone"])
        self.assertEqual("Second-slice trigger/profile repair loop", next_target["task"])

    def test_missing_acceptance_is_blocking(self):
        accepted = copy.deepcopy(self.accepted)
        accepted["summary"]["human_acceptance_recorded"] = False
        accepted["acceptance_record"]["decision"] = "pending"

        report = build_second_slice_repaired_recipe_validation_report(accepted, self.card_rows)
        codes = {issue["code"] for issue in report["recipe_validation"]["issues"]}

        self.assertIn("human_acceptance_missing", codes)
        self.assertFalse(report["summary"]["ready_for_m41_04"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m41_03.json"
            md_path = out / "m41_03.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M41-03", loaded["version"])
            self.assertIn("M41-03 Second-Slice Repaired Recipe Validation Rerun", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
