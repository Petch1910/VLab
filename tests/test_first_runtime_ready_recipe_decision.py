"""Tests for tools/deck/build_first_runtime_ready_recipe_decision.py (M37-closeout)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_first_runtime_ready_recipe_decision import (  # noqa: E402
    MILESTONE_INPUTS,
    build_decision_report,
    load_json,
    write_json,
    write_markdown,
)


class TestFirstRuntimeReadyRecipeDecision(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.reports = {item.milestone: load_json(item.path) for item in MILESTONE_INPUTS}
        cls.report = build_decision_report(cls.reports)

    def test_m37_closeout_completes_but_does_not_promote_runtime(self):
        decision = self.report["decision"]

        self.assertEqual("M37-closeout", self.report["version"])
        self.assertTrue(decision["m37_complete"])
        self.assertFalse(decision["first_runtime_ready_recipe_available"])
        self.assertFalse(decision["accepted_seed_can_be_runtime_fixture"])
        self.assertTrue(decision["accepted_seed_remains_advisory"])
        self.assertEqual(
            "keep_recipe_003_advisory_until_human_acceptance_and_grade_review_clear",
            decision["recommendation"],
        )

    def test_decision_blockers_are_explicit(self):
        blockers = self.report["decision"]["decision_blockers"]

        self.assertEqual(
            ["human_acceptance_pending", "grade_profile_review", "promotion_not_allowed"],
            blockers,
        )

    def test_revised_validation_status_is_carried_forward(self):
        revised = self.report["key_results"]["revised_validation"]

        self.assertEqual("recipe_003", revised["recipe_id"])
        self.assertEqual("validator_passed_pending_human_acceptance", revised["validation_status_after"])
        self.assertEqual("consistent_pending_human_acceptance", revised["consistency_status_after"])
        self.assertFalse(revised["runtime_ready"])
        self.assertFalse(revised["promotion_allowed"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, revised["trigger_counts"])
        self.assertEqual({"0": 16, "2": 6, "3": 28}, revised["grade_counts"])
        self.assertEqual(["grade_profile_review", "human_acceptance_pending"], revised["review_codes"])

    def test_m37_pipeline_results_are_summarized(self):
        results = self.report["key_results"]

        self.assertEqual(18, results["slot_gap_candidates"]["trigger_candidate_card_count"])
        self.assertEqual(5, results["slot_gap_candidates"]["complete_package_count"])
        self.assertEqual("m37_01_pkg_001", results["trigger_repair"]["recommended_package_id"])
        self.assertEqual("balanced_classic", results["trigger_repair"]["recommended_profile_id"])
        self.assertEqual(24, results["support_gap_triage"]["rejected_line_count"])
        self.assertEqual(5, results["manual_mapping_candidates"]["mapping_candidate_count"])

    def test_scope_is_decision_only(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_decision_artifact"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_deck"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_playbook_promotion"])

    def test_next_queue_is_m38_human_acceptance_gate(self):
        next_queue = self.report["next_queue_selection"]

        self.assertEqual("M38", next_queue["milestone"])
        self.assertEqual("Human acceptance and grade-profile repair gate", next_queue["name"])
        task_ids = [task["id"] for task in next_queue["first_tasks"]]
        self.assertEqual("M38-01", task_ids[0])
        self.assertIn("M38-04", task_ids)
        self.assertIn("M38-closeout", task_ids)
        self.assertIn(
            "no runtime deck promotion without explicit human acceptance",
            next_queue["hard_gates"],
        )

    def test_every_input_check_points_to_existing_output(self):
        checks = self.report["input_checks"]

        self.assertEqual(len(MILESTONE_INPUTS), len(checks))
        for check in checks:
            self.assertTrue(check["exists"], check["path"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m37_closeout.json"
            md_path = out / "m37_closeout.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M37-closeout", loaded["version"])
            self.assertIn("M37 First Runtime-Ready Recipe Decision", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
