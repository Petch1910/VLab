"""Tests for tools/deck/build_sixth_slice_runtime_readiness_closeout.py."""

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

from tools.deck.build_sixth_slice_runtime_readiness_closeout import (  # noqa: E402
    M56_01_SCAFFOLD,
    M56_02_PACKET,
    M56_03_DRAFTS,
    M56_04_VALIDATION,
    M56_05_CONSISTENCY,
    M56_06_REPAIRS,
    build_sixth_slice_runtime_readiness_closeout,
    load_json,
    write_json,
    write_markdown,
)


class TestSixthSliceRuntimeReadinessCloseout(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.scaffold = load_json(M56_01_SCAFFOLD)
        cls.packet = load_json(M56_02_PACKET)
        cls.drafts = load_json(M56_03_DRAFTS)
        cls.validation = load_json(M56_04_VALIDATION)
        cls.consistency = load_json(M56_05_CONSISTENCY)
        cls.repairs = load_json(M56_06_REPAIRS)
        cls.report = build_sixth_slice_runtime_readiness_closeout(
            cls.scaffold,
            cls.packet,
            cls.drafts,
            cls.validation,
            cls.consistency,
            cls.repairs,
        )

    def test_closeout_completes_m56_and_routes_to_m57(self):
        summary = self.report["summary"]

        self.assertEqual("M56-closeout", self.report["version"])
        self.assertTrue(summary["m56_complete"])
        self.assertFalse(summary["runtime_ready_recipe_available"])
        self.assertTrue(summary["human_selection_review_allowed"])
        self.assertEqual("M57", summary["next_queue_id"])
        self.assertTrue(summary["ready_for_next_queue"])

    def test_key_results_are_carried_forward(self):
        results = self.report["key_results"]

        self.assertTrue(results["fixture_scaffold_ready"])
        self.assertEqual(82, results["review_items"])
        self.assertEqual(12, results["recipe_drafts"])
        self.assertEqual(0, results["runtime_ready_recipe_count"])
        self.assertEqual(0, results["promotion_allowed_count"])
        self.assertEqual(12, results["blocked_by_manual_review_count"])
        self.assertEqual(12, results["manual_review_overlap_recipe_count"])
        self.assertEqual(12, results["grade_profile_review_recipe_count"])
        self.assertEqual(12, results["g_zone_deferred_recipe_count"])
        self.assertEqual(12, results["repair_candidates_ready_for_human_review"])
        self.assertEqual(12, results["manual_repair_complete_count"])
        self.assertEqual(12, results["grade_profile_complete_candidate_count"])
        self.assertEqual(0, results["unexpected_structural_blocker_recipe_count"])

    def test_decision_keeps_runtime_saved_deck_ui_and_bot_disabled(self):
        decision = self.report["decision"]
        blockers = decision["decision_blockers"]

        self.assertFalse(decision["sixth_slice_runtime_ready_recipe_available"])
        self.assertFalse(decision["sixth_slice_can_enter_runtime_fixture_gate_now"])
        self.assertTrue(decision["sixth_slice_remains_advisory"])
        self.assertTrue(decision["human_selection_review_allowed"])
        self.assertTrue(decision["g_zone_runtime_support_required_before_promotion"])
        self.assertFalse(decision["live_runtime_deck_enabled"])
        self.assertFalse(decision["saved_deck_or_ui_publication_allowed"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertEqual(
            "open_m57_sixth_slice_human_selection_and_g_zone_decision_gate",
            decision["recommendation"],
        )
        self.assertIn("no_runtime_ready_recipe", blockers)
        self.assertIn("no_promotion_allowed_consistency_check", blockers)
        self.assertIn("manual_review_overlap_requires_acceptance", blockers)
        self.assertIn("grade_profile_review_requires_acceptance", blockers)
        self.assertIn("g_zone_support_deferred", blockers)
        self.assertIn("human_recipe_selection_pending", blockers)
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

    def test_next_queue_is_bounded_human_and_g_zone_gate(self):
        next_queue = self.report["next_queue"]
        task_ids = [task["id"] for task in next_queue["tasks"]]

        self.assertEqual("M57", next_queue["id"])
        self.assertEqual("Sixth-slice Human Selection and G Zone Decision Gate", next_queue["title"])
        self.assertEqual(
            ["M57-01", "M57-02", "M57-03", "M57-04", "M57-05", "M57-06", "M57-closeout"],
            task_ids,
        )

    def test_missing_repair_readiness_routes_to_m56_repair(self):
        repairs = copy.deepcopy(self.repairs)
        repairs["summary"]["ready_for_m56_closeout"] = False
        repairs["summary"]["ready_for_human_repair_review_count"] = 0

        report = build_sixth_slice_runtime_readiness_closeout(
            self.scaffold,
            self.packet,
            self.drafts,
            self.validation,
            self.consistency,
            repairs,
        )

        self.assertFalse(report["summary"]["m56_complete"])
        self.assertFalse(report["summary"]["human_selection_review_allowed"])
        self.assertFalse(report["summary"]["ready_for_next_queue"])
        self.assertEqual("M56-repair", report["summary"]["next_queue_id"])
        self.assertEqual("repair_m56_evidence_before_runtime_decision", report["decision"]["recommendation"])

    def test_manual_or_g_zone_blocks_runtime_even_if_counts_are_positive(self):
        validation = copy.deepcopy(self.validation)
        consistency = copy.deepcopy(self.consistency)
        validation["summary"]["runtime_ready_recipe_count"] = 1
        consistency["summary"]["promotion_allowed_count"] = 1

        report = build_sixth_slice_runtime_readiness_closeout(
            self.scaffold,
            self.packet,
            self.drafts,
            validation,
            consistency,
            self.repairs,
        )

        self.assertTrue(report["summary"]["m56_complete"])
        self.assertFalse(report["summary"]["runtime_ready_recipe_available"])
        self.assertIn("manual_review_overlap_requires_acceptance", report["decision"]["decision_blockers"])
        self.assertIn("g_zone_support_deferred", report["decision"]["decision_blockers"])

    def test_every_input_check_points_to_existing_output(self):
        checks = self.report["input_checks"]

        self.assertEqual(6, len(checks))
        for check in checks:
            self.assertTrue(check["exists"], check["path"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m56_closeout.json"
            md_path = out / "m56_closeout.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M56-closeout", loaded["version"])
            self.assertIn("M56 Sixth-Slice Runtime Readiness Closeout", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
