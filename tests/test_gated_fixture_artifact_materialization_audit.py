"""Tests for tools/deck/build_gated_fixture_artifact_materialization_audit.py."""

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

import tests.test_post_nine_fixture_queue_plan as m71_fixture  # noqa: E402
from tools.deck.build_gated_fixture_artifact_materialization_audit import (  # noqa: E402
    PRIMARY_ARTIFACTS,
    RUNTIME_BOUNDARY_FLAGS,
    build_gated_fixture_artifact_materialization_audit,
    write_json,
    write_markdown,
)


def _first_five_materialized_paths() -> set[str]:
    prefixes = ("m39_", "m42_", "m46_", "m50_", "m54_")
    return {item["path"] for item in PRIMARY_ARTIFACTS if item["path"].split("/")[-1].startswith(prefixes)}


class TestGatedFixtureArtifactMaterializationAudit(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        m71_fixture.TestPostNineFixtureQueuePlan.setUpClass()
        cls.m71_report = m71_fixture.TestPostNineFixtureQueuePlan.report
        cls.first_five_paths = _first_five_materialized_paths()
        cls.report = build_gated_fixture_artifact_materialization_audit(cls.m71_report, cls.first_five_paths)

    def test_audit_detects_missing_sixth_through_ninth_and_m71_artifacts(self):
        summary = self.report["summary"]
        decision = self.report["decision"]

        self.assertEqual("M72-01", self.report["version"])
        self.assertTrue(summary["m72_01_ready"])
        self.assertEqual(37, summary["required_artifact_count"])
        self.assertEqual(20, summary["materialized_artifact_count"])
        self.assertEqual(17, summary["missing_artifact_count"])
        self.assertEqual(5, summary["complete_fixture_chain_count"])
        self.assertEqual(5, summary["incomplete_fixture_chain_count"])
        self.assertTrue(summary["ready_for_m72_02"])
        self.assertFalse(summary["ready_for_m73_01"])
        self.assertEqual("M72-02", decision["recommended_milestone"])
        self.assertFalse(decision["tenth_slice_selected_now"])
        self.assertFalse(decision["runtime_promotion_allowed"])
        self.assertFalse(decision["real_artifact_materialization_allowed_in_this_slice"])

    def test_fixture_chain_audit_identifies_complete_and_incomplete_chains(self):
        chains = {item["fixture_chain"]: item for item in self.report["fixture_chain_audit"]}

        self.assertTrue(chains["first_fixture_nova_grappler"]["complete"])
        self.assertTrue(chains["second_fixture_oracle_think_tank"]["complete"])
        self.assertTrue(chains["third_fixture_bermuda_triangle"]["complete"])
        self.assertTrue(chains["fourth_fixture_royal_paladin_g_series"]["complete"])
        self.assertTrue(chains["fifth_fixture_gold_paladin"]["complete"])
        self.assertFalse(chains["sixth_fixture_shadow_paladin"]["complete"])
        self.assertFalse(chains["seventh_fixture_neo_nectar"]["complete"])
        self.assertFalse(chains["eighth_fixture_kagero"]["complete"])
        self.assertFalse(chains["ninth_fixture_aqua_force"]["complete"])
        self.assertFalse(chains["post_nine_queue"]["complete"])
        self.assertEqual(["m71_01_post_nine_queue_plan"], chains["post_nine_queue"]["missing_artifacts"])

    def test_artifact_audit_lists_m70_and_m71_primary_reports(self):
        artifacts = {item["id"]: item for item in self.report["artifact_audit"]}

        self.assertIn("m70_04_ninth_scale_decision", artifacts)
        self.assertIn("m71_01_post_nine_queue_plan", artifacts)
        self.assertFalse(artifacts["m70_04_ninth_scale_decision"]["present"])
        self.assertFalse(artifacts["m71_01_post_nine_queue_plan"]["present"])

    def test_scope_keeps_runtime_ui_bot_and_tenth_slice_disabled(self):
        scope = self.report["scope"]

        self.assertTrue(scope["artifact_materialization_audit"])
        self.assertTrue(scope["uses_m71_01_queue_plan"])
        self.assertTrue(scope["primary_json_artifact_audit_only"])
        for flag in RUNTIME_BOUNDARY_FLAGS:
            self.assertFalse(scope[flag], flag)

    def test_all_primary_artifacts_present_routes_to_tenth_slice_gate_without_selecting_it(self):
        all_paths = {item["path"] for item in PRIMARY_ARTIFACTS}
        report = build_gated_fixture_artifact_materialization_audit(self.m71_report, all_paths)

        self.assertEqual(0, report["summary"]["missing_artifact_count"])
        self.assertFalse(report["summary"]["ready_for_m72_02"])
        self.assertTrue(report["summary"]["ready_for_m73_01"])
        self.assertEqual("M73-01", report["decision"]["recommended_milestone"])
        self.assertEqual("M73-01", report["next_target"]["milestone"])
        self.assertFalse(report["decision"]["tenth_slice_selected_now"])
        self.assertFalse(report["decision"]["runtime_promotion_allowed"])

    def test_failed_m71_routes_to_repair(self):
        m71_report = copy.deepcopy(self.m71_report)
        m71_report["summary"]["ready_for_m72_01"] = False
        m71_report["decision"]["opens_m72_01"] = False

        report = build_gated_fixture_artifact_materialization_audit(m71_report, self.first_five_paths)

        self.assertFalse(report["summary"]["m72_01_ready"])
        self.assertEqual(1, report["summary"]["blocking_issue_count"])
        self.assertEqual("M71-repair", report["decision"]["recommended_milestone"])
        self.assertEqual("M71-repair", report["next_target"]["milestone"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m72_01.json"
            md_path = out / "m72_01.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M72-01", loaded["version"])
            self.assertEqual("M72-02", loaded["next_target"]["milestone"])
            self.assertIn("M72-01 Gated Fixture Artifact Materialization Audit", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
