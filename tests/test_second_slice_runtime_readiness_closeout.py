"""Tests for tools/deck/build_second_slice_runtime_readiness_closeout.py (M40-closeout)."""

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

from tools.deck.build_second_slice_runtime_readiness_closeout import (  # noqa: E402
    M40_01_PACKET,
    M40_02_DRAFTS,
    M40_03_VALIDATION,
    M40_04_CONSISTENCY,
    M40_05_REPAIRS,
    build_second_slice_runtime_readiness_closeout,
    load_json,
    write_json,
    write_markdown,
)


class TestSecondSliceRuntimeReadinessCloseout(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.packet = load_json(M40_01_PACKET)
        cls.drafts = load_json(M40_02_DRAFTS)
        cls.validation = load_json(M40_03_VALIDATION)
        cls.consistency = load_json(M40_04_CONSISTENCY)
        cls.repairs = load_json(M40_05_REPAIRS)
        cls.report = build_second_slice_runtime_readiness_closeout(
            cls.packet,
            cls.drafts,
            cls.validation,
            cls.consistency,
            cls.repairs,
        )

    def test_closeout_completes_m40_and_routes_to_m41(self):
        summary = self.report["summary"]

        self.assertEqual("M40-closeout", self.report["version"])
        self.assertTrue(summary["m40_complete"])
        self.assertFalse(summary["runtime_ready_recipe_available"])
        self.assertTrue(summary["human_repair_review_allowed"])
        self.assertEqual("M41", summary["next_queue_id"])
        self.assertTrue(summary["ready_for_next_queue"])

    def test_key_results_are_carried_forward(self):
        results = self.report["key_results"]

        self.assertEqual(272, results["review_items"])
        self.assertEqual(25, results["recipe_drafts"])
        self.assertEqual(0, results["runtime_ready_recipe_count"])
        self.assertEqual(0, results["promotion_allowed_count"])
        self.assertEqual(25, results["manual_review_overlap_recipe_count"])
        self.assertEqual(25, results["repair_candidates_ready_for_human_review"])
        self.assertEqual(25, results["grade_profile_complete_candidate_count"])

    def test_decision_keeps_runtime_saved_deck_ui_and_bot_disabled(self):
        decision = self.report["decision"]
        blockers = decision["decision_blockers"]

        self.assertFalse(decision["second_slice_runtime_ready_recipe_available"])
        self.assertFalse(decision["second_slice_can_enter_runtime_fixture_gate_now"])
        self.assertTrue(decision["second_slice_remains_advisory"])
        self.assertTrue(decision["human_repair_review_allowed"])
        self.assertFalse(decision["live_runtime_deck_enabled"])
        self.assertFalse(decision["saved_deck_or_ui_publication_allowed"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertEqual("open_m41_human_repair_review_gate", decision["recommendation"])
        self.assertIn("no_runtime_ready_recipe", blockers)
        self.assertIn("no_promotion_allowed_consistency_check", blockers)
        self.assertIn("manual_review_overlap_unresolved", blockers)
        self.assertIn("repair_candidates_require_human_acceptance", blockers)
        self.assertIn("runtime_fixture_gate_not_run", blockers)

    def test_scope_is_closeout_only(self):
        scope = self.report["scope"]

        self.assertTrue(scope["closeout_report"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])

    def test_next_queue_is_bounded_human_repair_gate(self):
        next_queue = self.report["next_queue"]
        task_ids = [task["id"] for task in next_queue["tasks"]]

        self.assertEqual("M41", next_queue["id"])
        self.assertEqual("Second-slice Human Repair Review Gate", next_queue["title"])
        self.assertEqual(["M41-01", "M41-02", "M41-03", "M41-04", "M41-closeout"], task_ids)

    def test_missing_repair_readiness_routes_to_m40_repair(self):
        repairs = copy.deepcopy(self.repairs)
        repairs["summary"]["ready_for_m40_closeout"] = False
        repairs["summary"]["ready_for_human_repair_review_count"] = 0

        report = build_second_slice_runtime_readiness_closeout(
            self.packet,
            self.drafts,
            self.validation,
            self.consistency,
            repairs,
        )

        self.assertFalse(report["summary"]["m40_complete"])
        self.assertFalse(report["summary"]["human_repair_review_allowed"])
        self.assertFalse(report["summary"]["ready_for_next_queue"])
        self.assertEqual("M40-repair", report["summary"]["next_queue_id"])
        self.assertEqual("repair_m40_evidence_before_runtime_decision", report["decision"]["recommendation"])

    def test_every_input_check_points_to_existing_output(self):
        checks = self.report["input_checks"]

        self.assertEqual(5, len(checks))
        for check in checks:
            self.assertTrue(check["exists"], check["path"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m40_closeout.json"
            md_path = out / "m40_closeout.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M40-closeout", loaded["version"])
            self.assertIn("M40 Second-Slice Runtime Readiness Closeout", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
