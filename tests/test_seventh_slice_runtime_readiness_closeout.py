"""Tests for tools/deck/build_seventh_slice_runtime_readiness_closeout.py."""

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

from tests.test_seventh_slice_fixture_readiness import _build_in_memory_m59_01_selection  # noqa: E402
from tools.deck.build_seventh_slice_blocker_repair_candidates import (  # noqa: E402
    build_seventh_slice_blocker_repair_candidates,
)
from tools.deck.build_seventh_slice_fixture_readiness import build_seventh_slice_fixture_readiness  # noqa: E402
from tools.deck.build_seventh_slice_fixture_scaffold import build_seventh_slice_fixture_scaffold  # noqa: E402
from tools.deck.build_seventh_slice_recipe_draft_model import build_seventh_slice_recipe_drafts  # noqa: E402
from tools.deck.build_seventh_slice_recipe_pipeline_entry_gate import (  # noqa: E402
    build_seventh_slice_recipe_pipeline_entry_gate,
)
from tools.deck.build_seventh_slice_review_packet import build_seventh_slice_review_packet  # noqa: E402
from tools.deck.build_seventh_slice_runtime_readiness_closeout import (  # noqa: E402
    build_seventh_slice_runtime_readiness_closeout,
    write_json,
    write_markdown,
)
from tools.deck.build_seventh_slice_semantic_compatibility_probe import (  # noqa: E402
    build_seventh_slice_semantic_compatibility_probe,
)
from tools.deck.check_seventh_slice_combo_recipe_consistency import (  # noqa: E402
    build_seventh_slice_consistency_report,
)
from tools.deck.validate_seventh_slice_recipe_drafts import (  # noqa: E402
    _all_card_ids,
    build_seventh_slice_validation_report,
    load_card_rows,
)


class TestSeventhSliceRuntimeReadinessCloseout(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.selection = _build_in_memory_m59_01_selection()
        cls.readiness = build_seventh_slice_fixture_readiness(cls.selection)
        cls.probe = build_seventh_slice_semantic_compatibility_probe(cls.selection, cls.readiness)
        cls.gate = build_seventh_slice_recipe_pipeline_entry_gate(cls.readiness, cls.probe)
        cls.scaffold = build_seventh_slice_fixture_scaffold(cls.gate, cls.readiness, cls.probe)
        cls.packet = build_seventh_slice_review_packet(
            cls.selection,
            cls.readiness,
            cls.probe,
            cls.gate,
            cls.scaffold,
        )
        cls.drafts = build_seventh_slice_recipe_drafts(cls.packet, cls.scaffold)
        cls.card_rows = load_card_rows(_all_card_ids(cls.drafts))
        cls.validation = build_seventh_slice_validation_report(cls.drafts, cls.card_rows)
        cls.consistency = build_seventh_slice_consistency_report(cls.drafts, cls.validation)
        cls.repairs = build_seventh_slice_blocker_repair_candidates(
            cls.drafts,
            cls.validation,
            cls.consistency,
            cls.scaffold,
        )
        cls.report = build_seventh_slice_runtime_readiness_closeout(
            cls.scaffold,
            cls.packet,
            cls.drafts,
            cls.validation,
            cls.consistency,
            cls.repairs,
        )

    def test_closeout_completes_m60_scaffold_and_routes_to_m61(self):
        summary = self.report["summary"]

        self.assertEqual("M60-closeout", self.report["version"])
        self.assertEqual("in_memory_reports", self.report["input_mode"])
        self.assertTrue(summary["m60_scaffold_complete"])
        self.assertFalse(summary["real_artifacts_available"])
        self.assertFalse(summary["runtime_ready_recipe_available"])
        self.assertTrue(summary["human_selection_review_allowed"])
        self.assertEqual("M61", summary["next_queue_id"])
        self.assertTrue(summary["ready_for_next_queue"])

    def test_key_results_are_carried_forward(self):
        results = self.report["key_results"]

        self.assertTrue(results["fixture_scaffold_ready"])
        self.assertEqual(118, results["review_items"])
        self.assertEqual(23, results["recipe_drafts"])
        self.assertEqual(0, results["runtime_ready_recipe_count"])
        self.assertEqual(0, results["promotion_allowed_count"])
        self.assertEqual(23, results["blocked_by_manual_review_count"])
        self.assertEqual(23, results["manual_review_overlap_recipe_count"])
        self.assertEqual(21, results["grade_profile_review_recipe_count"])
        self.assertEqual(23, results["g_zone_deferred_recipe_count"])
        self.assertEqual(23, results["bloom_token_deferred_recipe_count"])
        self.assertEqual(23, results["repair_candidates_ready_for_human_review"])
        self.assertEqual(23, results["manual_repair_complete_count"])
        self.assertEqual(21, results["grade_profile_complete_candidate_count"])
        self.assertEqual(0, results["unexpected_structural_blocker_recipe_count"])

    def test_decision_keeps_runtime_saved_deck_ui_and_bot_disabled(self):
        decision = self.report["decision"]
        blockers = decision["decision_blockers"]

        self.assertTrue(decision["m60_scaffold_complete"])
        self.assertFalse(decision["real_artifacts_available"])
        self.assertFalse(decision["seventh_slice_runtime_ready_recipe_available"])
        self.assertFalse(decision["seventh_slice_can_enter_runtime_fixture_gate_now"])
        self.assertTrue(decision["seventh_slice_remains_advisory"])
        self.assertTrue(decision["human_selection_review_allowed"])
        self.assertTrue(decision["g_zone_runtime_support_required_before_promotion"])
        self.assertTrue(decision["bloom_token_runtime_support_required_before_promotion"])
        self.assertFalse(decision["live_runtime_deck_enabled"])
        self.assertFalse(decision["saved_deck_or_ui_publication_allowed"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertEqual(
            "open_m61_seventh_slice_human_selection_repair_and_g_zone_bloom_decision_gate",
            decision["recommendation"],
        )
        self.assertIn("no_runtime_ready_recipe", blockers)
        self.assertIn("no_promotion_allowed_consistency_check", blockers)
        self.assertIn("manual_review_overlap_requires_acceptance", blockers)
        self.assertIn("grade_profile_review_requires_acceptance", blockers)
        self.assertIn("g_zone_support_deferred", blockers)
        self.assertIn("bloom_token_support_deferred", blockers)
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
        self.assertFalse(scope["g_zone_runtime"])
        self.assertFalse(scope["stride_runtime"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])

    def test_next_queue_is_bounded_human_and_system_decision_gate(self):
        next_queue = self.report["next_queue"]
        task_ids = [task["id"] for task in next_queue["tasks"]]

        self.assertEqual("M61", next_queue["id"])
        self.assertEqual("Seventh-slice Human Selection, Repair, and G Zone/Bloom Decision Gate", next_queue["title"])
        self.assertEqual(
            ["M61-01", "M61-02", "M61-03", "M61-04", "M61-05", "M61-06", "M61-closeout"],
            task_ids,
        )

    def test_missing_repair_readiness_routes_to_m60_repair(self):
        repairs = copy.deepcopy(self.repairs)
        repairs["summary"]["ready_for_m60_closeout"] = False
        repairs["summary"]["ready_for_human_repair_review_count"] = 0

        report = build_seventh_slice_runtime_readiness_closeout(
            self.scaffold,
            self.packet,
            self.drafts,
            self.validation,
            self.consistency,
            repairs,
        )

        self.assertFalse(report["summary"]["m60_scaffold_complete"])
        self.assertFalse(report["summary"]["human_selection_review_allowed"])
        self.assertFalse(report["summary"]["ready_for_next_queue"])
        self.assertEqual("M60-repair", report["summary"]["next_queue_id"])
        self.assertEqual("repair_m60_evidence_before_runtime_decision", report["decision"]["recommendation"])

    def test_manual_g_zone_or_bloom_blocks_runtime_even_if_counts_are_positive(self):
        validation = copy.deepcopy(self.validation)
        consistency = copy.deepcopy(self.consistency)
        validation["summary"]["runtime_ready_recipe_count"] = 1
        consistency["summary"]["promotion_allowed_count"] = 1

        report = build_seventh_slice_runtime_readiness_closeout(
            self.scaffold,
            self.packet,
            self.drafts,
            validation,
            consistency,
            self.repairs,
        )

        self.assertTrue(report["summary"]["m60_scaffold_complete"])
        self.assertFalse(report["summary"]["runtime_ready_recipe_available"])
        self.assertIn("manual_review_overlap_requires_acceptance", report["decision"]["decision_blockers"])
        self.assertIn("g_zone_support_deferred", report["decision"]["decision_blockers"])
        self.assertIn("bloom_token_support_deferred", report["decision"]["decision_blockers"])

    def test_artifact_checks_are_metadata_not_scaffold_completion_gate(self):
        checks = self.report["artifact_checks"]

        self.assertEqual(6, len(checks))
        self.assertTrue(self.report["summary"]["m60_scaffold_complete"])
        for check in checks:
            self.assertIn("outputs/target_slice/", check["path"].replace("\\", "/"))
            self.assertIn("exists", check)

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m60_closeout.json"
            md_path = out / "m60_closeout.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M60-closeout", loaded["version"])
            self.assertIn("M60 Seventh-Slice Runtime Readiness Closeout", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
