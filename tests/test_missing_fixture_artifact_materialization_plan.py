"""Tests for tools/deck/build_missing_fixture_artifact_materialization_plan.py."""

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

import tests.test_gated_fixture_artifact_materialization_audit as m72_audit_fixture  # noqa: E402
from tools.deck.build_missing_fixture_artifact_materialization_plan import (  # noqa: E402
    MATERIALIZATION_STEPS,
    RUNTIME_BOUNDARY_FLAGS,
    build_missing_fixture_artifact_materialization_plan,
    write_json,
    write_markdown,
)


class TestMissingFixtureArtifactMaterializationPlan(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        m72_audit_fixture.TestGatedFixtureArtifactMaterializationAudit.setUpClass()
        cls.audit_report = m72_audit_fixture.TestGatedFixtureArtifactMaterializationAudit.report
        cls.all_present_audit = m72_audit_fixture.build_gated_fixture_artifact_materialization_audit(
            m72_audit_fixture.TestGatedFixtureArtifactMaterializationAudit.m71_report,
            {item["path"] for item in m72_audit_fixture.PRIMARY_ARTIFACTS},
        )
        cls.report = build_missing_fixture_artifact_materialization_plan(cls.audit_report)

    def test_plan_orders_all_missing_primary_artifacts_without_executing(self):
        summary = self.report["summary"]
        decision = self.report["decision"]
        steps = self.report["materialization_steps"]

        self.assertEqual("M72-02", self.report["version"])
        self.assertTrue(summary["m72_02_ready"])
        self.assertEqual(17, summary["planned_step_count"])
        self.assertEqual(5, summary["affected_chain_count"])
        self.assertEqual(0, summary["unknown_missing_artifact_count"])
        self.assertTrue(summary["ready_for_m72_03"])
        self.assertFalse(summary["ready_for_m73_01"])
        self.assertEqual("M72-03", decision["recommended_milestone"])
        self.assertFalse(decision["materialization_executed"])
        self.assertFalse(decision["materialization_allowed_in_this_slice"])
        self.assertEqual("M58-01", steps[0]["milestone"])
        self.assertEqual("M71-01", steps[-1]["milestone"])
        self.assertEqual([index for index in range(1, 18)], [step["order"] for step in steps])

    def test_chain_plan_groups_sixth_through_ninth_and_post_nine(self):
        chains = {item["fixture_chain"]: item for item in self.report["chain_plan"]}

        self.assertEqual(["M58-01", "M58-02", "M58-03", "M58-04"], chains["sixth_fixture_shadow_paladin"]["milestones"])
        self.assertEqual(["M62-01", "M62-02", "M62-03", "M62-04"], chains["seventh_fixture_neo_nectar"]["milestones"])
        self.assertEqual(["M66-01", "M66-02", "M66-03", "M66-04"], chains["eighth_fixture_kagero"]["milestones"])
        self.assertEqual(["M70-01", "M70-02", "M70-03", "M70-04"], chains["ninth_fixture_aqua_force"]["milestones"])
        self.assertEqual(["M71-01"], chains["post_nine_queue"]["milestones"])

    def test_commands_are_review_only_and_cover_expected_tools(self):
        commands = {step["milestone"]: step["command"] for step in self.report["materialization_steps"]}

        self.assertEqual("python tools\\deck\\validate_sixth_runtime_fixture_schema.py", commands["M58-01"])
        self.assertEqual("python tools\\deck\\build_six_fixture_scale_decision.py", commands["M58-04"])
        self.assertEqual("python tools\\deck\\validate_ninth_runtime_fixture_schema.py", commands["M70-01"])
        self.assertEqual("python tools\\deck\\build_nine_fixture_scale_decision.py", commands["M70-04"])
        self.assertEqual("python tools\\deck\\build_post_nine_fixture_queue_plan.py", commands["M71-01"])
        for step in self.report["materialization_steps"]:
            self.assertFalse(step["execution_allowed_in_this_slice"])
            self.assertFalse(step["materialization_allowed_in_this_slice"])

    def test_scope_keeps_runtime_ui_bot_and_tenth_slice_disabled(self):
        scope = self.report["scope"]

        self.assertTrue(scope["materialization_plan_only"])
        self.assertTrue(scope["uses_m72_01_audit"])
        self.assertTrue(scope["primary_json_artifact_plan"])
        for flag in RUNTIME_BOUNDARY_FLAGS:
            self.assertFalse(scope[flag], flag)

    def test_all_artifacts_present_routes_to_tenth_slice_gate_without_selecting(self):
        report = build_missing_fixture_artifact_materialization_plan(self.all_present_audit)

        self.assertTrue(report["summary"]["m72_02_ready"])
        self.assertEqual(0, report["summary"]["planned_step_count"])
        self.assertFalse(report["summary"]["ready_for_m72_03"])
        self.assertTrue(report["summary"]["ready_for_m73_01"])
        self.assertEqual("M73-01", report["decision"]["recommended_milestone"])
        self.assertFalse(report["decision"]["tenth_slice_selected_now"])
        self.assertFalse(report["decision"]["materialization_executed"])

    def test_unready_audit_routes_to_repair(self):
        audit_report = copy.deepcopy(self.audit_report)
        audit_report["summary"]["m72_01_ready"] = False
        audit_report["decision"]["audit_complete"] = False

        report = build_missing_fixture_artifact_materialization_plan(audit_report)

        self.assertFalse(report["summary"]["m72_02_ready"])
        self.assertEqual(1, report["summary"]["blocking_issue_count"])
        self.assertEqual("M72-01-repair", report["decision"]["recommended_milestone"])
        self.assertEqual("M72-01-repair", report["next_target"]["milestone"])

    def test_known_step_table_has_unique_artifact_ids(self):
        ids = [step["artifact_id"] for step in MATERIALIZATION_STEPS]
        self.assertEqual(len(ids), len(set(ids)))

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m72_02.json"
            md_path = out / "m72_02.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M72-02", loaded["version"])
            self.assertEqual("M72-03", loaded["next_target"]["milestone"])
            self.assertIn("M72-02 Missing Fixture Artifact Materialization Plan", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
