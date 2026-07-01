"""Tests for tools/deck/build_fourth_slice_recipe_pipeline_entry_gate.py."""

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

from tools.deck.build_fourth_slice_recipe_pipeline_entry_gate import (  # noqa: E402
    M47_03_PROBE,
    M47_APPLIED_SCOPE,
    build_fourth_slice_recipe_pipeline_entry_gate,
    load_json,
    write_json,
    write_markdown,
)


class TestFourthSliceRecipePipelineEntryGate(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.applied_scope = load_json(M47_APPLIED_SCOPE)
        cls.probe = load_json(M47_03_PROBE)
        cls.report = build_fourth_slice_recipe_pipeline_entry_gate(cls.applied_scope, cls.probe)

    def test_gate_allows_offline_m48_pipeline(self):
        summary = self.report["summary"]
        decision = self.report["decision"]

        self.assertEqual("M47-04", self.report["version"])
        self.assertTrue(summary["decision_ready"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertTrue(summary["offline_recipe_pipeline_allowed"])
        self.assertTrue(summary["ready_for_m48"])
        self.assertTrue(decision["offline_recipe_pipeline_allowed"])
        self.assertEqual("M48 Fourth-slice offline recipe pipeline", decision["recommended_next_queue"])

    def test_evidence_matches_applied_scope_and_probe_outputs(self):
        evidence = self.report["evidence_summary"]

        self.assertTrue(evidence["applied_scope_readiness_passed"])
        self.assertTrue(evidence["semantic_probe_passed"])
        self.assertEqual("g_era_heal_expansion", evidence["applied_expansion_id"])
        self.assertEqual(190, evidence["source_card_count"])
        self.assertEqual(32, evidence["effective_series_count"])
        self.assertEqual(190, evidence["semantic_card_count"])
        self.assertEqual(15, evidence["manual_review_card_count"])
        self.assertEqual(14150, evidence["pair_graph_edge_count"])
        self.assertEqual(785, evidence["candidate_edge_count"])
        self.assertEqual("requires_fourth_slice_fixture_scaffold", evidence["policy_reuse_decision"])

    def test_requires_fixture_scaffold_before_recipe_validation(self):
        decision = self.report["decision"]
        queue = self.report["proposed_m48_queue"]

        self.assertTrue(decision["fixture_scaffold_required_before_recipe_validation"])
        self.assertEqual("M48-01", queue[0]["milestone"])
        self.assertEqual("Fourth-slice fixture scaffold", queue[0]["task"])
        self.assertEqual("M48-closeout", queue[-1]["milestone"])

    def test_runtime_saved_deck_ui_and_bot_stay_blocked(self):
        decision = self.report["decision"]
        boundary = self.report["runtime_boundary"]

        self.assertFalse(decision["runtime_deck_promotion_allowed"])
        self.assertFalse(decision["saved_deck_or_ui_publication_allowed"])
        self.assertFalse(decision["bot_playbook_promotion_allowed"])
        self.assertTrue(boundary["decision_artifact_only"])
        self.assertTrue(boundary["does_not_create_deck"])
        self.assertTrue(boundary["does_not_create_recipe_draft"])
        self.assertTrue(boundary["does_not_create_runtime_fixture"])
        self.assertTrue(boundary["does_not_mutate_runtime_pack"])
        self.assertTrue(boundary["does_not_publish_to_ui"])
        self.assertTrue(boundary["does_not_publish_to_bot"])
        self.assertFalse(boundary["GameState_mutation"])

    def test_failed_applied_scope_blocks_gate(self):
        applied_scope = copy.deepcopy(self.applied_scope)
        applied_scope["readiness"]["all_fixture_expectations_met"] = False

        report = build_fourth_slice_recipe_pipeline_entry_gate(applied_scope, self.probe)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["ready_for_m48"])
        self.assertIn("applied_scope_fixture_expectations_not_met", codes)
        self.assertEqual("M47-repair-semantic", report["next_target"]["milestone"])

    def test_failed_probe_blocks_gate(self):
        probe = copy.deepcopy(self.probe)
        probe["readiness"]["ready_for_m47_04"] = False

        report = build_fourth_slice_recipe_pipeline_entry_gate(self.applied_scope, probe)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["ready_for_m48"])
        self.assertIn("semantic_compatibility_probe_not_ready", codes)

    def test_next_target_is_m48_01(self):
        self.assertEqual("M48-01", self.report["next_target"]["milestone"])
        self.assertEqual("Fourth-slice fixture scaffold", self.report["next_target"]["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m47_04.json"
            md_path = out / "m47_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M47-04", loaded["version"])
            self.assertIn("M47-04 Fourth-Slice Recipe", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
