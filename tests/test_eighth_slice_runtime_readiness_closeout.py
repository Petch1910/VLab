"""Tests for tools/deck/build_eighth_slice_runtime_readiness_closeout.py."""

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

import tests.test_eighth_slice_recipe_draft_model as eighth_draft_fixture  # noqa: E402
from tools.deck.build_eighth_slice_blocker_repair_candidates import (  # noqa: E402
    build_eighth_slice_blocker_repair_candidates,
)
from tools.deck.build_eighth_slice_runtime_readiness_closeout import (  # noqa: E402
    build_eighth_slice_runtime_readiness_closeout,
    write_json,
    write_markdown,
)
from tools.deck.check_eighth_slice_combo_recipe_consistency import (  # noqa: E402
    build_eighth_slice_consistency_report,
)
from tools.deck.validate_eighth_slice_recipe_drafts import (  # noqa: E402
    _all_card_ids,
    build_eighth_slice_validation_report,
    load_card_rows,
)


class TestEighthSliceRuntimeReadinessCloseout(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        eighth_draft_fixture.TestEighthSliceRecipeDraftModel.setUpClass()
        cls.scaffold = eighth_draft_fixture.TestEighthSliceRecipeDraftModel.scaffold
        cls.packet = eighth_draft_fixture.TestEighthSliceRecipeDraftModel.packet
        cls.drafts = eighth_draft_fixture.TestEighthSliceRecipeDraftModel.report
        cls.card_rows = load_card_rows(_all_card_ids(cls.drafts))
        cls.validation = build_eighth_slice_validation_report(cls.drafts, cls.card_rows)
        cls.consistency = build_eighth_slice_consistency_report(cls.drafts, cls.validation)
        cls.repairs = build_eighth_slice_blocker_repair_candidates(
            cls.drafts,
            cls.validation,
            cls.consistency,
            cls.scaffold,
        )
        cls.report = build_eighth_slice_runtime_readiness_closeout(
            cls.scaffold,
            cls.packet,
            cls.drafts,
            cls.validation,
            cls.consistency,
            cls.repairs,
        )

    def test_closeout_completes_m64_scaffold_and_routes_to_m65(self):
        summary = self.report["summary"]

        self.assertEqual("M64-closeout", self.report["version"])
        self.assertEqual("in_memory_reports", self.report["input_mode"])
        self.assertTrue(summary["m64_scaffold_complete"])
        self.assertFalse(summary["real_artifacts_available"])
        self.assertFalse(summary["runtime_ready_recipe_available"])
        self.assertTrue(summary["human_selection_review_allowed"])
        self.assertEqual("M65", summary["next_queue_id"])
        self.assertTrue(summary["ready_for_next_queue"])

    def test_key_results_are_carried_forward(self):
        results = self.report["key_results"]

        self.assertTrue(results["fixture_scaffold_ready"])
        self.assertEqual(362, results["review_items"])
        self.assertEqual(25, results["recipe_drafts"])
        self.assertEqual(0, results["runtime_ready_recipe_count"])
        self.assertEqual(0, results["promotion_allowed_count"])
        self.assertEqual(0, results["blocked_by_manual_review_count"])
        self.assertEqual(0, results["manual_review_overlap_recipe_count"])
        self.assertEqual(25, results["grade_profile_review_recipe_count"])
        self.assertEqual(25, results["lock_deferred_recipe_count"])
        self.assertEqual(25, results["legion_deferred_recipe_count"])
        self.assertEqual(25, results["repair_candidates_ready_for_human_review"])
        self.assertEqual(25, results["human_selection_candidate_count"])
        self.assertEqual(25, results["grade_profile_complete_candidate_count"])
        self.assertEqual(0, results["unexpected_structural_blocker_recipe_count"])

    def test_decision_keeps_runtime_saved_deck_ui_and_bot_disabled(self):
        decision = self.report["decision"]
        blockers = decision["decision_blockers"]

        self.assertTrue(decision["m64_scaffold_complete"])
        self.assertFalse(decision["real_artifacts_available"])
        self.assertFalse(decision["eighth_slice_runtime_ready_recipe_available"])
        self.assertFalse(decision["eighth_slice_can_enter_runtime_fixture_gate_now"])
        self.assertTrue(decision["eighth_slice_remains_advisory"])
        self.assertTrue(decision["human_selection_review_allowed"])
        self.assertTrue(decision["grade_profile_acceptance_required_before_promotion"])
        self.assertTrue(decision["lock_runtime_support_required_before_promotion"])
        self.assertTrue(decision["legion_runtime_support_required_before_promotion"])
        self.assertFalse(decision["live_runtime_deck_enabled"])
        self.assertFalse(decision["saved_deck_or_ui_publication_allowed"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertEqual(
            "open_m65_eighth_slice_human_selection_grade_repair_and_lock_legion_decision_gate",
            decision["recommendation"],
        )
        self.assertIn("no_runtime_ready_recipe", blockers)
        self.assertIn("no_promotion_allowed_consistency_check", blockers)
        self.assertNotIn("manual_review_overlap_requires_acceptance", blockers)
        self.assertIn("grade_profile_review_requires_acceptance", blockers)
        self.assertIn("lock_runtime_support_deferred", blockers)
        self.assertIn("legion_runtime_support_deferred", blockers)
        self.assertIn("human_recipe_selection_pending", blockers)
        self.assertIn("repair_candidates_require_human_selection", blockers)
        self.assertIn("runtime_fixture_gate_not_run", blockers)

    def test_scope_is_closeout_only(self):
        scope = self.report["scope"]

        self.assertTrue(scope["closeout_report"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["records_human_selection"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["lock_runtime"])
        self.assertFalse(scope["legion_runtime"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])

    def test_next_queue_is_bounded_human_and_system_decision_gate(self):
        next_queue = self.report["next_queue"]
        task_ids = [task["id"] for task in next_queue["tasks"]]

        self.assertEqual("M65", next_queue["id"])
        self.assertEqual(
            "Eighth-slice Human Selection, Grade Repair, and Lock/Legion Decision Gate",
            next_queue["title"],
        )
        self.assertEqual(
            ["M65-01", "M65-02", "M65-03", "M65-04", "M65-05", "M65-06", "M65-closeout"],
            task_ids,
        )

    def test_missing_repair_readiness_routes_to_m64_repair(self):
        repairs = copy.deepcopy(self.repairs)
        repairs["summary"]["ready_for_m64_closeout"] = False
        repairs["summary"]["ready_for_human_repair_review_count"] = 0

        report = build_eighth_slice_runtime_readiness_closeout(
            self.scaffold,
            self.packet,
            self.drafts,
            self.validation,
            self.consistency,
            repairs,
        )

        self.assertFalse(report["summary"]["m64_scaffold_complete"])
        self.assertFalse(report["summary"]["human_selection_review_allowed"])
        self.assertFalse(report["summary"]["ready_for_next_queue"])
        self.assertEqual("M64-repair", report["summary"]["next_queue_id"])
        self.assertEqual("repair_m64_evidence_before_runtime_decision", report["decision"]["recommendation"])

    def test_grade_lock_or_legion_blocks_runtime_even_if_counts_are_positive(self):
        validation = copy.deepcopy(self.validation)
        consistency = copy.deepcopy(self.consistency)
        validation["summary"]["runtime_ready_recipe_count"] = 1
        consistency["summary"]["promotion_allowed_count"] = 1

        report = build_eighth_slice_runtime_readiness_closeout(
            self.scaffold,
            self.packet,
            self.drafts,
            validation,
            consistency,
            self.repairs,
        )

        self.assertTrue(report["summary"]["m64_scaffold_complete"])
        self.assertFalse(report["summary"]["runtime_ready_recipe_available"])
        self.assertIn("grade_profile_review_requires_acceptance", report["decision"]["decision_blockers"])
        self.assertIn("lock_runtime_support_deferred", report["decision"]["decision_blockers"])
        self.assertIn("legion_runtime_support_deferred", report["decision"]["decision_blockers"])

    def test_artifact_checks_are_metadata_not_scaffold_completion_gate(self):
        checks = self.report["artifact_checks"]

        self.assertEqual(6, len(checks))
        self.assertTrue(self.report["summary"]["m64_scaffold_complete"])
        for check in checks:
            self.assertIn("outputs/target_slice/", check["path"].replace("\\", "/"))
            self.assertIn("exists", check)

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m64_closeout.json"
            md_path = out / "m64_closeout.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M64-closeout", loaded["version"])
            self.assertIn("M64 Eighth-Slice Runtime Readiness Closeout", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
