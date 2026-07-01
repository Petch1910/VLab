"""Tests for tools/deck/build_fourth_slice_fixture_closeout.py."""

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

from tools.deck.build_fourth_slice_fixture_closeout import (  # noqa: E402
    M49_05_GATE,
    build_fourth_slice_fixture_closeout,
    load_json,
    write_json,
    write_markdown,
)


class TestFourthSliceFixtureCloseout(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.gate = load_json(M49_05_GATE)
        cls.report = build_fourth_slice_fixture_closeout(cls.gate)

    def test_closeout_completes_m49_with_fourth_fixture(self):
        summary = self.report["summary"]
        results = self.report["key_results"]

        self.assertEqual("M49-closeout", self.report["version"])
        self.assertTrue(summary["m49_complete"])
        self.assertTrue(summary["fourth_runtime_fixture_available"])
        self.assertEqual("M50", summary["next_queue_id"])
        self.assertTrue(summary["ready_for_next_queue"])
        self.assertEqual("m48_recipe_001", results["recipe_id"])
        self.assertEqual(8, results["gate_passed_check_count"])
        self.assertEqual(0, results["gate_failed_check_count"])

    def test_decision_keeps_live_runtime_ui_bot_and_stride_disabled(self):
        decision = self.report["decision"]
        results = self.report["key_results"]

        self.assertTrue(decision["fourth_recipe_enters_fixture_scope"])
        self.assertFalse(decision["fourth_recipe_remains_advisory_only"])
        self.assertFalse(decision["live_runtime_deck_enabled"])
        self.assertFalse(decision["saved_deck_enabled"])
        self.assertFalse(decision["ui_deck_list_enabled"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertFalse(decision["g_zone_runtime_enabled"])
        self.assertFalse(decision["stride_runtime_enabled"])
        self.assertTrue(decision["needs_user_team_review_before_live_deck_ui"])
        self.assertFalse(results["runtime_deck_library_mutated"])
        self.assertFalse(results["saved_deck_injected"])
        self.assertFalse(results["ui_deck_list_published"])
        self.assertFalse(results["bot_playbook_enabled"])
        self.assertFalse(results["g_zone_runtime_enabled"])
        self.assertFalse(results["stride_runtime_enabled"])

    def test_next_queue_is_explicit_and_bounded(self):
        next_queue = self.report["next_queue"]
        task_ids = [task["id"] for task in next_queue["tasks"]]

        self.assertEqual("M50", next_queue["id"])
        self.assertEqual("Fourth Fixture Consumption and Four-Fixture Scale Gate", next_queue["title"])
        self.assertEqual(["M50-01", "M50-02", "M50-03", "M50-04"], task_ids)

    def test_failed_gate_routes_to_repair_queue(self):
        gate = copy.deepcopy(self.gate)
        gate["summary"]["promotion_allowed"] = False
        gate["promotion_decision"]["fixture_created"] = False
        gate["promotion_decision"]["fixture_path"] = ""
        gate["runtime_fixture"] = None

        report = build_fourth_slice_fixture_closeout(gate)

        self.assertFalse(report["summary"]["m49_complete"])
        self.assertEqual("M49-repair", report["summary"]["next_queue_id"])
        self.assertFalse(report["summary"]["ready_for_next_queue"])
        self.assertTrue(report["decision"]["fourth_recipe_remains_advisory_only"])

    def test_scope_does_not_mutate_runtime_ui_bot_or_stride(self):
        scope = self.report["scope"]

        self.assertTrue(scope["closeout_report"])
        self.assertFalse(scope["changes_fixture_artifact"])
        self.assertFalse(scope["mutates_runtime_deck_library"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_playbook_promotion"])
        self.assertFalse(scope["g_zone_runtime_enabled"])
        self.assertFalse(scope["stride_runtime_enabled"])
        self.assertFalse(scope["GameState_mutation"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m49_closeout.json"
            md_path = out / "m49_closeout.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M49-closeout", loaded["version"])
            self.assertIn("M49 Fourth-Slice Fixture Closeout", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
