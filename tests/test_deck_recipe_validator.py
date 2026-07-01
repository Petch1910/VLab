"""Tests for tools/deck/validate_deck_recipe_drafts.py (M36-03)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.validate_deck_recipe_drafts import (  # noqa: E402
    RECIPE_DRAFTS,
    build_validation_report,
    load_card_rows,
    load_json,
    write_json,
    write_markdown,
    _all_card_ids,
)


class TestDeckRecipeValidator(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.recipe_report = load_json(RECIPE_DRAFTS)
        cls.card_rows = load_card_rows(_all_card_ids(cls.recipe_report))
        cls.report = build_validation_report(cls.recipe_report, cls.card_rows)

    def test_validator_runs_all_recipe_drafts(self):
        summary = self.report["summary"]

        self.assertEqual("M36-03", self.report["version"])
        self.assertTrue(summary["ready_for_m36_04"])
        self.assertEqual(25, summary["recipe_count"])
        self.assertEqual(0, summary["missing_card_recipe_count"])
        self.assertEqual(0, summary["copy_limit_violation_recipe_count"])

    def test_runtime_ready_is_separate_from_validator_availability(self):
        summary = self.report["summary"]

        self.assertEqual(0, summary["runtime_ready_recipe_count"])
        self.assertEqual(24, summary["blocked_by_review_count"])
        self.assertGreaterEqual(summary["invalid_draft_count"], 1)
        self.assertEqual(16, summary["slot_gap_recipe_count"])
        self.assertEqual(12, summary["trigger_count_mismatch_recipe_count"])

    def test_scope_is_offline_validator_only(self):
        scope = self.report["scope"]
        policy = self.report["validation_policy"]

        self.assertTrue(scope["offline_validator"])
        self.assertFalse(scope["runtime_deck"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_deck_injection"])
        self.assertEqual(50, policy["classic_main_deck_size"])
        self.assertEqual(16, policy["classic_trigger_count"])
        self.assertTrue(policy["review_blockers_are_blocking"])

    def test_accepted_seed_recipe_is_invalid_due_to_slot_gap(self):
        accepted = [
            item
            for item in self.report["recipe_validations"]
            if item["source_line_id"] == "line_003"
        ]

        self.assertEqual(1, len(accepted))
        item = accepted[0]
        self.assertEqual("invalid_draft", item["validation_status"])
        codes = {issue["code"] for issue in item["issues"]}
        self.assertIn("unfilled_slots", codes)
        self.assertIn("trigger_count_mismatch", codes)
        self.assertIn("human_acceptance_pending", codes)
        self.assertFalse(item["runtime_ready"])

    def test_rejected_lines_are_blocked_by_review(self):
        blocked = [
            item
            for item in self.report["recipe_validations"]
            if item["validation_status"] == "blocked_by_review"
        ]

        self.assertEqual(24, len(blocked))
        for item in blocked:
            codes = {issue["code"] for issue in item["issues"]}
            self.assertIn("review_status_blocked", codes)

    def test_next_target_is_m36_04(self):
        next_target = self.report["next_target"]

        self.assertEqual("M36-04", next_target["milestone"])
        self.assertEqual("Combo-line to recipe consistency check", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "validation.json"
            md_path = out / "validation.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M36-03", loaded["version"])
            self.assertIn("M36-03 Deck Recipe Validation Report", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
