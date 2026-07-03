"""Tests for tools/deck/build_post_nine_fixture_queue_plan.py."""

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

import tests.test_nine_fixture_scale_decision as nine_scale_fixture  # noqa: E402
from tools.deck.build_post_nine_fixture_queue_plan import (  # noqa: E402
    RUNTIME_BOUNDARY_FLAGS,
    build_post_nine_fixture_queue_plan,
    write_json,
    write_markdown,
)


class TestPostNineFixtureQueuePlan(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        nine_scale_fixture.TestNineFixtureScaleDecision.setUpClass()
        cls.m70_report = nine_scale_fixture.TestNineFixtureScaleDecision.report
        cls.report = build_post_nine_fixture_queue_plan(cls.m70_report)

    def test_plan_recommends_materialization_audit_before_tenth_slice(self):
        self.assertEqual("M71-01", self.report["version"])
        self.assertTrue(self.report["summary"]["m71_01_ready"])
        self.assertTrue(self.report["summary"]["m70_ready_for_m71"])
        self.assertTrue(self.report["summary"]["ready_for_m72_01"])
        self.assertEqual(0, self.report["summary"]["blocking_issue_count"])
        self.assertEqual("gated_real_artifact_materialization_audit", self.report["decision"]["recommended_queue_id"])
        self.assertEqual("M72-01", self.report["decision"]["recommended_milestone"])
        self.assertTrue(self.report["decision"]["opens_m72_01"])
        self.assertFalse(self.report["decision"]["tenth_slice_selected_now"])
        self.assertFalse(self.report["decision"]["runtime_promotion_allowed"])
        self.assertFalse(self.report["decision"]["real_artifact_materialization_allowed_now"])

    def test_input_summary_carries_nine_fixture_evidence(self):
        summary = self.report["input_summary"]

        self.assertEqual("M70-04", summary["m70_version"])
        self.assertTrue(summary["m70_ready_for_m71"])
        self.assertEqual(9, summary["fixture_evidence_count"])
        self.assertEqual(9, summary["passed_fixture_count"])
        self.assertEqual(0, summary["failed_fixture_count"])
        self.assertIn("ninth_fixture_aqua_force", summary["fixture_labels"])

    def test_scope_keeps_runtime_ui_bot_and_advanced_systems_disabled(self):
        scope = self.report["scope"]

        self.assertTrue(scope["post_nine_queue_planning"])
        self.assertTrue(scope["artifact_materialization_audit_only"])
        self.assertTrue(scope["uses_m70_04_evidence"])
        for flag in RUNTIME_BOUNDARY_FLAGS:
            self.assertFalse(scope[flag], flag)

    def test_queue_options_include_recommended_audit_and_deferred_tenth_slice(self):
        options = {item["id"]: item for item in self.report["queue_options"]}

        self.assertIn("gated_real_artifact_materialization_audit", options)
        self.assertIn("tenth_slice_selection", options)
        self.assertIn("runtime_ui_bot_promotion", options)
        self.assertEqual("recommended", options["gated_real_artifact_materialization_audit"]["status"])
        self.assertTrue(options["gated_real_artifact_materialization_audit"]["enabled_now"])
        self.assertEqual("deferred", options["tenth_slice_selection"]["status"])
        self.assertFalse(options["tenth_slice_selection"]["enabled_now"])
        self.assertEqual("blocked_by_explicit_gate", options["runtime_ui_bot_promotion"]["status"])
        self.assertFalse(options["runtime_ui_bot_promotion"]["enabled_now"])

    def test_failed_m70_routes_to_repair(self):
        m70_report = copy.deepcopy(self.m70_report)
        m70_report["summary"]["failed_fixture_count"] = 1
        m70_report["summary"]["passed_fixture_count"] = 8
        m70_report["summary"]["post_m70_queue_review_ready"] = False
        m70_report["summary"]["ready_for_m71_planning"] = False
        m70_report["decision"]["post_m70_queue_review_ready"] = False

        report = build_post_nine_fixture_queue_plan(m70_report)

        self.assertFalse(report["summary"]["m70_ready_for_m71"])
        self.assertFalse(report["summary"]["ready_for_m72_01"])
        self.assertEqual(1, report["summary"]["blocking_issue_count"])
        self.assertEqual("repair_failed_fixture_evidence", report["decision"]["recommended_queue_id"])
        self.assertEqual("M70-repair", report["next_target"]["milestone"])
        self.assertFalse(report["decision"]["opens_m72_01"])
        self.assertTrue(report["decision"]["routes_to_m70_repair"])

    def test_wrong_m70_version_routes_to_repair(self):
        m70_report = copy.deepcopy(self.m70_report)
        m70_report["version"] = "M70-03"

        report = build_post_nine_fixture_queue_plan(m70_report)

        self.assertFalse(report["summary"]["m70_ready_for_m71"])
        self.assertEqual("M70-repair", report["decision"]["recommended_milestone"])
        self.assertEqual("M70-repair", report["next_target"]["milestone"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m71_01.json"
            md_path = out / "m71_01.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M71-01", loaded["version"])
            self.assertEqual("M72-01", loaded["next_target"]["milestone"])
            self.assertIn("M71-01 Post-Nine Fixture Queue Plan", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
