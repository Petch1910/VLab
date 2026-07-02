"""Tests for tools/deck/build_seventh_slice_recipe_pipeline_entry_gate.py."""

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
from tools.deck.build_seventh_slice_fixture_readiness import build_seventh_slice_fixture_readiness  # noqa: E402
from tools.deck.build_seventh_slice_recipe_pipeline_entry_gate import (  # noqa: E402
    build_seventh_slice_recipe_pipeline_entry_gate,
    write_json,
    write_markdown,
)
from tools.deck.build_seventh_slice_semantic_compatibility_probe import (  # noqa: E402
    build_seventh_slice_semantic_compatibility_probe,
)


class TestSeventhSliceRecipePipelineEntryGate(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.selection = _build_in_memory_m59_01_selection()
        cls.readiness = build_seventh_slice_fixture_readiness(cls.selection)
        cls.probe = build_seventh_slice_semantic_compatibility_probe(cls.selection, cls.readiness)
        cls.report = build_seventh_slice_recipe_pipeline_entry_gate(cls.readiness, cls.probe)

    def test_gate_allows_offline_m60_pipeline(self):
        summary = self.report["summary"]
        decision = self.report["decision"]

        self.assertEqual("M59-04", self.report["version"])
        self.assertTrue(summary["decision_ready"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertTrue(summary["offline_recipe_pipeline_allowed"])
        self.assertTrue(summary["ready_for_m60"])
        self.assertTrue(decision["offline_recipe_pipeline_allowed"])
        self.assertEqual("M60 Seventh-slice offline recipe pipeline", decision["recommended_next_queue"])

    def test_evidence_matches_m59_02_and_m59_03_outputs(self):
        evidence = self.report["evidence_summary"]

        self.assertTrue(evidence["fixture_readiness_passed"])
        self.assertTrue(evidence["semantic_probe_passed"])
        self.assertEqual(78, evidence["source_card_count"])
        self.assertEqual(78, evidence["semantic_card_count"])
        self.assertEqual(10, evidence["manual_review_card_count"])
        self.assertEqual(2885, evidence["pair_graph_edge_count"])
        self.assertEqual(107, evidence["candidate_edge_count"])
        self.assertEqual("requires_seventh_slice_fixture_scaffold", evidence["policy_reuse_decision"])
        self.assertTrue(evidence["g_zone_fixture_boundary_required"])

    def test_requires_fixture_scaffold_before_recipe_validation(self):
        decision = self.report["decision"]
        queue = self.report["proposed_m60_queue"]

        self.assertTrue(decision["fixture_scaffold_required_before_recipe_validation"])
        self.assertEqual("M60-01", queue[0]["milestone"])
        self.assertEqual("Seventh-slice fixture scaffold", queue[0]["task"])
        self.assertEqual("M60-closeout", queue[-1]["milestone"])

    def test_runtime_saved_deck_ui_bot_and_g_zone_stay_blocked(self):
        decision = self.report["decision"]
        boundary = self.report["runtime_boundary"]

        self.assertFalse(decision["runtime_deck_promotion_allowed"])
        self.assertFalse(decision["saved_deck_or_ui_publication_allowed"])
        self.assertFalse(decision["bot_playbook_promotion_allowed"])
        self.assertFalse(decision["g_zone_runtime_enabled"])
        self.assertFalse(decision["stride_runtime_enabled"])
        self.assertTrue(boundary["decision_artifact_only"])
        self.assertTrue(boundary["does_not_create_deck"])
        self.assertTrue(boundary["does_not_create_recipe_draft"])
        self.assertTrue(boundary["does_not_create_runtime_fixture"])
        self.assertTrue(boundary["does_not_mutate_runtime_pack"])
        self.assertTrue(boundary["does_not_publish_to_ui"])
        self.assertTrue(boundary["does_not_publish_to_bot"])
        self.assertFalse(boundary["g_zone_runtime_enabled"])
        self.assertFalse(boundary["stride_runtime_enabled"])
        self.assertFalse(boundary["GameState_mutation"])

    def test_failed_fixture_readiness_blocks_gate(self):
        readiness = copy.deepcopy(self.readiness)
        readiness["readiness"]["all_fixture_expectations_met"] = False

        report = build_seventh_slice_recipe_pipeline_entry_gate(readiness, self.probe)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["ready_for_m60"])
        self.assertIn("fixture_expectations_not_met", codes)
        self.assertEqual("M59-repair-semantic", report["next_target"]["milestone"])

    def test_failed_probe_blocks_gate(self):
        probe = copy.deepcopy(self.probe)
        probe["readiness"]["ready_for_m59_04"] = False

        report = build_seventh_slice_recipe_pipeline_entry_gate(self.readiness, probe)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["ready_for_m60"])
        self.assertIn("semantic_compatibility_probe_not_ready", codes)

    def test_unexpected_runtime_promotion_blocks_gate(self):
        probe = copy.deepcopy(self.probe)
        probe["readiness"]["runtime_or_bot_promotion_allowed"] = True

        report = build_seventh_slice_recipe_pipeline_entry_gate(self.readiness, probe)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["ready_for_m60"])
        self.assertIn("unexpected_runtime_or_bot_promotion", codes)

    def test_next_target_is_m60_01(self):
        self.assertEqual("M60-01", self.report["next_target"]["milestone"])
        self.assertEqual("Seventh-slice fixture scaffold", self.report["next_target"]["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m59_04.json"
            md_path = out / "m59_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M59-04", loaded["version"])
            self.assertIn("M59-04 Seventh-Slice Recipe", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
