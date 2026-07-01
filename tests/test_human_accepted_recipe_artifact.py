"""Tests for tools/deck/build_human_accepted_recipe_artifact.py (M38-03)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_human_accepted_recipe_artifact import (  # noqa: E402
    M37_02_REPAIR,
    M37_05_RERUN,
    M38_01_REVIEW,
    M38_02_GRADE,
    RECIPE_DRAFTS,
    build_human_accepted_recipe_artifact,
    load_json,
    write_json,
    write_markdown,
)


class TestHumanAcceptedRecipeArtifact(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = build_human_accepted_recipe_artifact(
            load_json(RECIPE_DRAFTS),
            load_json(M37_02_REPAIR),
            load_json(M37_05_RERUN),
            load_json(M38_01_REVIEW),
            load_json(M38_02_GRADE),
            accepted_grade_package_id="m38_02_grade_pkg_001",
            acceptance_text="\u0e07\u0e31\u0e49\u0e19\u0e08\u0e31\u0e14\u0e44\u0e1b",
            accepted_at="2026-06-30",
        )

    def test_artifact_records_acceptance_for_recipe_003(self):
        summary = self.report["summary"]
        record = self.report["acceptance_record"]

        self.assertEqual("M38-03", self.report["version"])
        self.assertEqual("recipe_003", summary["recipe_id"])
        self.assertEqual("accepted", record["decision"])
        self.assertEqual("user", record["accepted_by"])
        self.assertEqual("2026-06-30", record["accepted_at"])
        self.assertEqual("\u0e07\u0e31\u0e49\u0e19\u0e08\u0e31\u0e14\u0e44\u0e1b", record["acceptance_text"])
        self.assertEqual("m38_02_grade_pkg_001", summary["accepted_grade_package_id"])
        self.assertEqual("m37_01_pkg_001", summary["accepted_trigger_package_id"])

    def test_accepted_recipe_clears_human_and_grade_reviews(self):
        accepted = self.report["accepted_recipe"]
        summary = self.report["summary"]

        self.assertTrue(accepted["human_acceptance_cleared"])
        self.assertTrue(accepted["grade_profile_review_cleared"])
        self.assertTrue(summary["human_acceptance_cleared"])
        self.assertTrue(summary["grade_profile_review_cleared"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertEqual("accepted_review_artifact_ready_for_runtime_gate", accepted["validation_status"])

    def test_accepted_recipe_has_classic_counts(self):
        summary = self.report["summary"]
        accepted = self.report["accepted_recipe"]

        self.assertEqual(50, summary["main_deck_count"])
        self.assertEqual(16, summary["trigger_count"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, summary["grade_counts"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, accepted["count_summary"]["trigger_counts"])
        self.assertEqual(50, sum(row["quantity"] for row in accepted["quantities"]))

    def test_artifact_does_not_promote_runtime_or_mutate_sources(self):
        scope = self.report["scope"]
        accepted = self.report["accepted_recipe"]

        self.assertTrue(scope["offline_human_accepted_artifact"])
        self.assertTrue(scope["records_human_acceptance"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_deck"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_playbook_promotion"])
        self.assertFalse(scope["live_card_text_parsing"])
        self.assertFalse(accepted["runtime_promotion_allowed"])
        self.assertFalse(self.report["summary"]["runtime_promotion_allowed"])

    def test_next_target_is_runtime_fixture_gate(self):
        next_target = self.report["next_target"]

        self.assertTrue(self.report["summary"]["ready_for_m38_04"])
        self.assertEqual("M38-04", next_target["milestone"])
        self.assertEqual("Runtime fixture promotion gate", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m38_03.json"
            md_path = out / "m38_03.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M38-03", loaded["version"])
            self.assertIn("M38-03 Human-Accepted Recipe Artifact", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
