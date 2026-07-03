"""Tests for tools/deck/build_sixth_fixture_primary_artifact_materialization_gate.py."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import tests.test_six_fixture_scale_decision as six_fixture_fixture  # noqa: E402
from tools.deck.build_missing_fixture_artifact_materialization_plan import (  # noqa: E402
    RUNTIME_BOUNDARY_FLAGS,
)
from tools.deck.build_sixth_fixture_primary_artifact_materialization_gate import (  # noqa: E402
    build_sixth_fixture_primary_artifact_materialization_gate,
    write_json,
    write_markdown,
)


def _unity_result_file(tmp: Path, accepted: bool = True) -> Path:
    path = tmp / "unity_result.json"
    path.write_text(
        json.dumps(
            {
                "accepted": accepted,
                "seed": 5803,
                "ruleset": "Premium",
                "deck_source": "deck_code",
                "actions_executed": 4,
                "event_count": 4,
            }
        ),
        encoding="utf-8",
    )
    return path


class TestSixthFixturePrimaryArtifactMaterializationGate(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        six_fixture_fixture.TestSixFixtureScaleDecision.setUpClass()
        cls.sixth_fixture = six_fixture_fixture.TestSixFixtureScaleDecision.sixth_fixture

    def test_missing_real_m57_06_fixture_routes_to_human_prerequisite(self):
        with tempfile.TemporaryDirectory() as tmp:
            report = build_sixth_fixture_primary_artifact_materialization_gate(
                fixture_path=Path(tmp) / "missing_m57_06.json",
            )

        self.assertEqual("M72-03", report["version"])
        self.assertTrue(report["summary"]["m72_03_ready"])
        self.assertFalse(report["summary"]["sixth_chain_ready"])
        self.assertEqual(1, report["summary"]["blocking_issue_count"])
        self.assertEqual("m57_06_runtime_fixture_missing", report["issues"][0]["code"])
        self.assertEqual("M57-02", report["next_target"]["milestone"])
        self.assertEqual(0, report["summary"]["ready_step_count"])
        self.assertEqual(
            ["blocked_prerequisite_missing"] * 4,
            [item["status"] for item in report["materialization"]],
        )

    def test_in_memory_fixture_with_unity_evidence_builds_full_sixth_chain(self):
        with tempfile.TemporaryDirectory() as tmp:
            tmp_path = Path(tmp)
            unity_result = _unity_result_file(tmp_path)
            unity_replay = tmp_path / "unity_replay.json"
            unity_replay.write_text("{}", encoding="utf-8")
            report = build_sixth_fixture_primary_artifact_materialization_gate(
                fixture=self.sixth_fixture,
                fixture_path=tmp_path / "in_memory_fixture.json",
                unity_result_path=unity_result,
                unity_replay_path=unity_replay,
            )

        self.assertEqual(4, report["summary"]["ready_step_count"])
        self.assertTrue(report["summary"]["sixth_chain_ready"])
        self.assertTrue(report["summary"]["ready_for_m72_04"])
        self.assertEqual("M72-04", report["next_target"]["milestone"])
        self.assertFalse(report["decision"]["real_artifacts_written"])
        self.assertEqual(
            ["M58-01", "M58-02", "M58-03", "M58-04"],
            [item["milestone"] for item in report["materialization"]],
        )
        self.assertTrue(all(item["ready"] for item in report["materialization"]))
        self.assertEqual({"m58_01", "m58_02", "m58_03", "m58_04"}, set(report["_reports"]))

    def test_in_memory_fixture_without_unity_evidence_stops_before_scale_decision(self):
        report = build_sixth_fixture_primary_artifact_materialization_gate(fixture=self.sixth_fixture)
        statuses = {item["milestone"]: item for item in report["materialization"]}

        self.assertFalse(report["summary"]["sixth_chain_ready"])
        self.assertEqual(2, report["summary"]["ready_step_count"])
        self.assertTrue(statuses["M58-01"]["ready"])
        self.assertTrue(statuses["M58-02"]["ready"])
        self.assertFalse(statuses["M58-03"]["ready"])
        self.assertEqual("needs_unity_headless_evidence", statuses["M58-03"]["status"])
        self.assertFalse(statuses["M58-04"]["ready"])
        self.assertEqual("blocked_by_m58_03", statuses["M58-04"]["status"])
        self.assertEqual("M58-03-unity-headless-smoke", report["next_target"]["milestone"])

    def test_unready_m72_02_plan_blocks_gate(self):
        report = build_sixth_fixture_primary_artifact_materialization_gate(
            plan_report={"version": "M72-02", "summary": {"m72_02_ready": False}, "decision": {}},
            fixture=self.sixth_fixture,
        )

        self.assertEqual(1, report["summary"]["blocking_issue_count"])
        self.assertEqual("m72_02_plan_not_ready", report["issues"][0]["code"])
        self.assertEqual(0, report["summary"]["ready_step_count"])

    def test_scope_keeps_runtime_ui_bot_and_materialization_disabled(self):
        report = build_sixth_fixture_primary_artifact_materialization_gate(fixture=self.sixth_fixture)
        scope = report["scope"]
        decision = report["decision"]

        self.assertTrue(scope["sixth_fixture_primary_artifact_gate"])
        self.assertFalse(scope["writes_m58_artifacts"])
        self.assertFalse(scope["materializes_real_artifacts"])
        self.assertFalse(decision["saved_deck_enabled"])
        self.assertFalse(decision["ui_deck_list_enabled"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertFalse(decision["g_zone_runtime_enabled"])
        self.assertFalse(decision["stride_runtime_enabled"])
        self.assertFalse(decision["GameState_mutation"])
        for flag in RUNTIME_BOUNDARY_FLAGS:
            self.assertFalse(scope[flag], flag)

    def test_output_round_trip(self):
        report = build_sixth_fixture_primary_artifact_materialization_gate(fixture=self.sixth_fixture)
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m72_03.json"
            md_path = out / "m72_03.md"

            write_json(report, json_path)
            write_markdown(report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M72-03", loaded["version"])
            self.assertNotIn("_reports", loaded)
            self.assertIn("M72-03 Sixth Fixture Primary Artifact Materialization Gate", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
