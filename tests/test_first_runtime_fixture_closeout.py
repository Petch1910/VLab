"""Tests for tools/deck/build_first_runtime_fixture_closeout.py (M38-closeout)."""

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

from tools.deck.build_first_runtime_fixture_closeout import (  # noqa: E402
    M38_04_GATE,
    build_first_runtime_fixture_closeout,
    load_json,
    write_json,
    write_markdown,
)


class TestFirstRuntimeFixtureCloseout(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.gate = load_json(M38_04_GATE)
        cls.report = build_first_runtime_fixture_closeout(cls.gate)

    def test_closeout_completes_m38_with_first_fixture(self):
        summary = self.report["summary"]
        results = self.report["key_results"]

        self.assertEqual("M38-closeout", self.report["version"])
        self.assertTrue(summary["m38_complete"])
        self.assertTrue(summary["first_runtime_fixture_available"])
        self.assertEqual("M39", summary["next_queue_id"])
        self.assertTrue(summary["ready_for_next_queue"])
        self.assertEqual("recipe_003", results["recipe_id"])
        self.assertEqual(5, results["gate_passed_check_count"])
        self.assertEqual(0, results["gate_failed_check_count"])

    def test_decision_keeps_live_runtime_and_bot_disabled(self):
        decision = self.report["decision"]
        results = self.report["key_results"]

        self.assertTrue(decision["first_recipe_enters_fixture_scope"])
        self.assertFalse(decision["first_recipe_remains_advisory_only"])
        self.assertFalse(decision["live_runtime_deck_enabled"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertTrue(decision["needs_user_team_review_before_live_deck_ui"])
        self.assertFalse(results["runtime_deck_library_mutated"])
        self.assertFalse(results["bot_playbook_enabled"])

    def test_next_queue_is_explicit_and_bounded(self):
        next_queue = self.report["next_queue"]
        task_ids = [task["id"] for task in next_queue["tasks"]]

        self.assertEqual("M39", next_queue["id"])
        self.assertEqual("Fixture Consumption and Second-Slice Scale Gate", next_queue["title"])
        self.assertEqual(["M39-01", "M39-02", "M39-03", "M39-04"], task_ids)

    def test_failed_gate_routes_to_repair_queue(self):
        gate = copy.deepcopy(self.gate)
        gate["summary"]["promotion_allowed"] = False
        gate["promotion_decision"]["fixture_created"] = False
        gate["promotion_decision"]["fixture_path"] = ""
        gate["runtime_fixture"] = None

        report = build_first_runtime_fixture_closeout(gate)

        self.assertFalse(report["summary"]["m38_complete"])
        self.assertEqual("M38-repair", report["summary"]["next_queue_id"])
        self.assertFalse(report["summary"]["ready_for_next_queue"])
        self.assertTrue(report["decision"]["first_recipe_remains_advisory_only"])

    def test_scope_does_not_mutate_runtime_or_bot(self):
        scope = self.report["scope"]

        self.assertTrue(scope["closeout_report"])
        self.assertFalse(scope["changes_fixture_artifact"])
        self.assertFalse(scope["mutates_runtime_deck_library"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_playbook_promotion"])
        self.assertFalse(scope["GameState_mutation"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m38_closeout.json"
            md_path = out / "m38_closeout.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M38-closeout", loaded["version"])
            self.assertIn("M38 First Runtime Fixture Closeout", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
