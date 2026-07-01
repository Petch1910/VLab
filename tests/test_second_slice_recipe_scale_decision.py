"""Tests for tools/deck/build_second_slice_recipe_scale_decision.py (M39-04)."""

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

from tools.deck.build_second_slice_recipe_scale_decision import (  # noqa: E402
    M35_E2_REPORT,
    M35_E3_REPORT,
    M35_E4_REPORT,
    M36_05_REPORT,
    M39_03_REPORT,
    build_second_slice_recipe_scale_decision,
    load_json,
    write_json,
    write_markdown,
)


class TestSecondSliceRecipeScaleDecision(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.m39_03 = load_json(M39_03_REPORT)
        cls.m36_05 = load_json(M36_05_REPORT)
        cls.m35_e2 = load_json(M35_E2_REPORT)
        cls.m35_e3 = load_json(M35_E3_REPORT)
        cls.m35_e4 = load_json(M35_E4_REPORT)
        cls.report = build_second_slice_recipe_scale_decision(
            cls.m39_03,
            cls.m36_05,
            cls.m35_e2,
            cls.m35_e3,
            cls.m35_e4,
        )

    def test_decision_allows_only_offline_recipe_pipeline(self):
        summary = self.report["summary"]
        decision = self.report["decision"]

        self.assertEqual("M39-04", self.report["version"])
        self.assertTrue(summary["decision_ready"])
        self.assertTrue(summary["offline_recipe_pipeline_allowed"])
        self.assertTrue(summary["ready_for_m40"])
        self.assertTrue(decision["offline_recipe_pipeline_allowed"])
        self.assertFalse(decision["runtime_deck_promotion_allowed"])
        self.assertFalse(decision["saved_deck_or_ui_publication_allowed"])
        self.assertFalse(decision["bot_playbook_promotion_allowed"])

    def test_evidence_summarizes_first_fixture_and_second_slice(self):
        evidence = self.report["evidence_summary"]

        self.assertTrue(evidence["first_fixture_headless_consumed"])
        self.assertTrue(evidence["second_slice_fixture_ready"])
        self.assertTrue(evidence["classic_core_policy_reusable"])
        self.assertTrue(evidence["second_slice_probe_ready"])
        self.assertEqual(103, evidence["probe_card_count"])
        self.assertEqual(2660, evidence["probe_edge_count"])
        self.assertEqual(259, evidence["probe_candidate_edges"])
        self.assertEqual(7, evidence["manual_review_count"])

    def test_next_queue_is_m40_offline_work(self):
        next_target = self.report["next_target"]
        queue = self.report["proposed_m40_queue"]

        self.assertEqual("M40-01", next_target["milestone"])
        self.assertEqual("Second-slice review packet", next_target["task"])
        self.assertEqual(6, len(queue))
        self.assertEqual("M40-closeout", queue[-1]["milestone"])

    def test_runtime_boundary_remains_closed(self):
        boundary = self.report["runtime_boundary"]

        self.assertTrue(boundary["decision_artifact_only"])
        self.assertTrue(boundary["does_not_create_deck"])
        self.assertTrue(boundary["does_not_mutate_runtime_pack"])
        self.assertTrue(boundary["does_not_publish_to_bot"])
        self.assertTrue(boundary["does_not_mutate_GameState"])

    def test_first_fixture_not_ready_blocks_scale_decision(self):
        m39_03 = copy.deepcopy(self.m39_03)
        m39_03["summary"]["ready_for_m39_04"] = False

        report = build_second_slice_recipe_scale_decision(
            m39_03,
            self.m36_05,
            self.m35_e2,
            self.m35_e3,
            self.m35_e4,
        )
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["decision_ready"])
        self.assertFalse(report["summary"]["offline_recipe_pipeline_allowed"])
        self.assertIn("first_fixture_consumption_not_ready", codes)

    def test_unexpected_runtime_or_bot_flags_block_decision(self):
        m35_e3 = copy.deepcopy(self.m35_e3)
        m35_e4 = copy.deepcopy(self.m35_e4)
        m35_e3["readiness"]["runtime_or_bot_promotion_allowed"] = True
        m35_e4["readiness"]["runtime_bot_integration_enabled"] = True

        report = build_second_slice_recipe_scale_decision(
            self.m39_03,
            self.m36_05,
            self.m35_e2,
            m35_e3,
            m35_e4,
        )
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["decision_ready"])
        self.assertIn("unexpected_runtime_promotion_flag", codes)
        self.assertIn("unexpected_bot_integration_enabled", codes)

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m39_04.json"
            md_path = out / "m39_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M39-04", loaded["version"])
            self.assertIn("M39-04 Second-Slice Recipe Scale Decision", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
