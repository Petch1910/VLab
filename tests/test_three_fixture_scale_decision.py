"""Tests for tools/deck/build_three_fixture_scale_decision.py."""

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

from tools.deck.build_three_fixture_scale_decision import (  # noqa: E402
    ARCHETYPE_RANKING,
    M39_03_SMOKE,
    M42_03_SMOKE,
    M46_03_SMOKE,
    build_three_fixture_scale_decision,
    load_json,
    write_json,
    write_markdown,
)


class TestThreeFixtureScaleDecision(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.first_smoke = load_json(M39_03_SMOKE)
        cls.second_smoke = load_json(M42_03_SMOKE)
        cls.third_smoke = load_json(M46_03_SMOKE)
        cls.ranking = load_json(ARCHETYPE_RANKING)
        cls.report = build_three_fixture_scale_decision(cls.first_smoke, cls.second_smoke, cls.third_smoke, cls.ranking)

    def test_decision_allows_fourth_slice_offline_pipeline(self):
        summary = self.report["summary"]
        decision = self.report["decision"]

        self.assertEqual("M46-04", self.report["version"])
        self.assertEqual(3, summary["fixture_evidence_count"])
        self.assertEqual(3, summary["passed_fixture_count"])
        self.assertEqual(0, summary["failed_fixture_count"])
        self.assertGreaterEqual(summary["candidate_count"], 1)
        self.assertTrue(summary["fourth_slice_offline_pipeline_allowed"])
        self.assertTrue(summary["ready_for_m47"])
        self.assertTrue(decision["fourth_slice_offline_pipeline_allowed"])
        self.assertFalse(decision["fourth_slice_selected_now"])

    def test_fixture_evidence_requires_unity_deck_code_acceptance(self):
        for item in self.report["fixture_evidence"]:
            self.assertTrue(item["offline_load_ready"])
            self.assertTrue(item["deck_code_created"])
            self.assertTrue(item["unity_headless_smoke_passed"])
            self.assertEqual(0, item["blocking_issue_count"])
            self.assertEqual("deck_code", item["deck_source"])
            self.assertEqual(4, item["actions_executed"])
            self.assertEqual(4, item["event_count"])

    def test_candidate_queue_excludes_completed_fixture_groups(self):
        completed = set(self.report["completed_groups"])
        candidate_groups = {candidate["group"] for candidate in self.report["candidate_queue"]}

        self.assertEqual(3, len(completed))
        self.assertTrue(completed.isdisjoint(candidate_groups))

    def test_scope_does_not_mutate_runtime_ui_or_bot(self):
        scope = self.report["scope"]
        decision = self.report["decision"]

        self.assertTrue(scope["offline_scale_decision"])
        self.assertFalse(scope["selects_runtime_deck"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["GameState_mutation"])
        self.assertFalse(decision["live_runtime_deck_enabled"])
        self.assertFalse(decision["saved_deck_enabled"])
        self.assertFalse(decision["ui_deck_list_enabled"])
        self.assertFalse(decision["bot_playbook_enabled"])

    def test_failed_fixture_routes_to_repair(self):
        third_smoke = copy.deepcopy(self.third_smoke)
        third_smoke["summary"]["unity_headless_smoke_passed"] = False
        third_smoke["unity_headless_result"]["accepted"] = False

        report = build_three_fixture_scale_decision(self.first_smoke, self.second_smoke, third_smoke, self.ranking)

        self.assertFalse(report["summary"]["fourth_slice_offline_pipeline_allowed"])
        self.assertFalse(report["summary"]["ready_for_m47"])
        self.assertEqual("M46-repair", report["next_target"]["milestone"])

    def test_next_target_is_m47_01(self):
        next_target = self.report["next_target"]

        self.assertEqual("M47-01", next_target["milestone"])
        self.assertEqual("Fourth target slice selection", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m46_04.json"
            md_path = out / "m46_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M46-04", loaded["version"])
            self.assertIn("M46-04 Three-Fixture Scale Decision", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
