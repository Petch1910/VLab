"""Tests for tools/deck/build_hybrid_vertical_slice_closeout.py (M35-closeout)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_hybrid_vertical_slice_closeout import (  # noqa: E402
    MILESTONE_INPUTS,
    build_closeout,
    load_json,
    write_json,
    write_markdown,
)


class TestHybridVerticalSliceCloseout(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.reports = {item.milestone: load_json(item.path) for item in MILESTONE_INPUTS}
        cls.report = build_closeout(cls.reports)

    def test_m35_closeout_marks_all_phases_done(self):
        readiness = self.report["readiness"]

        self.assertEqual("M35-closeout", self.report["version"])
        self.assertTrue(readiness["all_inputs_present"])
        self.assertTrue(readiness["all_phases_closed"])
        self.assertTrue(readiness["m35_hybrid_vertical_slice_complete"])

    def test_scope_does_not_enable_runtime_or_ui_changes(self):
        scope = self.report["scope"]
        readiness = self.report["readiness"]

        self.assertTrue(scope["offline_coordination_artifact"])
        self.assertFalse(scope["changes_runtime_gameplay"])
        self.assertFalse(scope["changes_unity_ui"])
        self.assertFalse(scope["enables_bot_runtime"])
        self.assertFalse(readiness["runtime_bot_integration_enabled"])

    def test_first_slice_results_are_carried_forward(self):
        first = self.report["key_results"]["first_slice"]

        self.assertGreater(first["clean_candidate_edges"], 0)
        self.assertEqual(25, first["candidate_packages"])
        self.assertEqual(25, first["deck_skeletons"])
        self.assertEqual(25, first["combo_lines"])
        self.assertEqual(1, first["reviewed_playbook_seed_entries"])
        self.assertEqual(24, first["rejected_playbook_lines"])

    def test_second_slice_probe_results_are_carried_forward(self):
        second = self.report["key_results"]["second_slice"]

        self.assertTrue(second["fixture_expectations_met"])
        self.assertTrue(second["classic_core_policy_reusable"])
        self.assertTrue(second["probe_ready"])
        self.assertEqual(103, second["probe_card_count"])
        self.assertEqual(2660, second["probe_edge_count"])
        self.assertEqual(259, second["probe_candidate_edges"])

    def test_bot_gate_stays_closed_for_runtime(self):
        bot = self.report["key_results"]["bot_gate"]

        self.assertTrue(bot["gate_passed"])
        self.assertEqual(1, bot["future_hint_candidate_count"])
        self.assertEqual(1, bot["blocked_source_count"])
        self.assertFalse(bot["runtime_bot_integration_enabled"])

    def test_next_queue_is_m36_deck_recipe_validation(self):
        next_queue = self.report["next_queue_selection"]

        self.assertEqual("M36", next_queue["milestone"])
        self.assertEqual("Human-review-assisted deck recipe validation", next_queue["name"])
        task_ids = [task["id"] for task in next_queue["first_tasks"]]
        self.assertIn("M36-01", task_ids)
        self.assertIn("M36-closeout", task_ids)
        self.assertIn("no runtime bot wiring", next_queue["hard_gates"])
        self.assertIn("human review required before playbook/runtime promotion", next_queue["hard_gates"])

    def test_every_input_check_points_to_existing_output(self):
        checks = self.report["input_checks"]

        self.assertEqual(len(MILESTONE_INPUTS), len(checks))
        for check in checks:
            self.assertTrue(check["exists"], check["path"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "closeout.json"
            md_path = out / "closeout.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M35-closeout", loaded["version"])
            self.assertIn("M35 Hybrid Vertical-Slice Closeout", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
