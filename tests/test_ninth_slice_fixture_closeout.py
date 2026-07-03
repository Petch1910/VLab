"""Tests for tools/deck/build_ninth_slice_fixture_closeout.py (M69-closeout)."""

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

import tests.test_ninth_slice_runtime_fixture_promotion_gate as gate_fixture  # noqa: E402
from tools.deck.build_ninth_slice_fixture_closeout import (  # noqa: E402
    build_ninth_slice_fixture_closeout,
    write_json,
    write_markdown,
)
from tools.deck.build_ninth_slice_runtime_fixture_promotion_gate import (  # noqa: E402
    AQUA_FORCE_OPTION,
    G_ZONE_OPTION,
    STRIDE_OPTION,
)


SELECTED_REVIEW_ITEM_ID = "m69_01_m68_recipe_001_repair_review"


class TestNinthSliceFixtureCloseout(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        gate_fixture.TestNinthSliceRuntimeFixturePromotionGate.setUpClass()
        cls.gate = gate_fixture.TestNinthSliceRuntimeFixturePromotionGate.report
        cls.report = build_ninth_slice_fixture_closeout(cls.gate)

    def test_closeout_completes_m69_with_ninth_fixture(self):
        summary = self.report["summary"]
        results = self.report["key_results"]

        self.assertEqual("M69-closeout", self.report["version"])
        self.assertTrue(summary["m69_complete"])
        self.assertTrue(summary["ninth_runtime_fixture_available"])
        self.assertEqual("M70", summary["next_queue_id"])
        self.assertTrue(summary["ready_for_next_queue"])
        self.assertTrue(summary["g_zone_boundary_applied"])
        self.assertTrue(summary["stride_boundary_applied"])
        self.assertTrue(summary["aqua_force_boundary_applied"])
        self.assertEqual("m68_recipe_001", results["recipe_id"])
        self.assertEqual(SELECTED_REVIEW_ITEM_ID, results["selected_review_item_id"])
        self.assertEqual(G_ZONE_OPTION, results["selected_g_zone_option_id"])
        self.assertEqual(STRIDE_OPTION, results["selected_stride_option_id"])
        self.assertEqual(AQUA_FORCE_OPTION, results["selected_aqua_force_option_id"])
        self.assertEqual(7, results["gate_passed_check_count"])
        self.assertEqual(0, results["gate_failed_check_count"])
        self.assertEqual(50, results["main_deck_count"])
        self.assertEqual(16, results["trigger_count"])
        self.assertEqual(0, results["grade4_main_deck_count"])

    def test_decision_keeps_live_runtime_ui_bot_and_system_runtime_disabled(self):
        decision = self.report["decision"]
        results = self.report["key_results"]

        self.assertTrue(decision["ninth_recipe_enters_fixture_scope"])
        self.assertFalse(decision["ninth_recipe_remains_advisory_only"])
        self.assertFalse(decision["live_runtime_deck_enabled"])
        self.assertFalse(decision["saved_deck_enabled"])
        self.assertFalse(decision["ui_deck_list_enabled"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertFalse(decision["live_card_text_parsing_enabled"])
        self.assertFalse(decision["g_zone_runtime_enabled"])
        self.assertFalse(decision["stride_runtime_enabled"])
        self.assertFalse(decision["aqua_force_battle_order_runtime_enabled"])
        self.assertTrue(decision["needs_user_team_review_before_live_deck_ui"])
        self.assertFalse(results["runtime_deck_library_mutated"])
        self.assertFalse(results["saved_deck_injected"])
        self.assertFalse(results["ui_deck_list_published"])
        self.assertFalse(results["bot_playbook_enabled"])
        self.assertFalse(results["g_zone_runtime_enabled"])
        self.assertFalse(results["stride_runtime_enabled"])
        self.assertFalse(results["aqua_force_battle_order_runtime_enabled"])

    def test_next_queue_is_explicit_and_bounded(self):
        next_queue = self.report["next_queue"]
        task_ids = [task["id"] for task in next_queue["tasks"]]

        self.assertEqual("M70", next_queue["id"])
        self.assertEqual("Ninth Fixture Consumption and Nine-Fixture Scale Gate", next_queue["title"])
        self.assertEqual(["M70-01", "M70-02", "M70-03", "M70-04"], task_ids)

    def test_failed_gate_routes_to_repair_queue(self):
        gate = copy.deepcopy(self.gate)
        gate["summary"]["promotion_allowed"] = False
        gate["promotion_decision"]["fixture_created"] = False
        gate["promotion_decision"]["fixture_path"] = ""
        gate["runtime_fixture"] = None

        report = build_ninth_slice_fixture_closeout(gate)

        self.assertFalse(report["summary"]["m69_complete"])
        self.assertEqual("M69-repair", report["summary"]["next_queue_id"])
        self.assertFalse(report["summary"]["ready_for_next_queue"])
        self.assertTrue(report["decision"]["ninth_recipe_remains_advisory_only"])
        self.assertEqual([], report["next_queue"]["tasks"])

    def test_missing_runtime_fixture_payload_does_not_complete_m69(self):
        gate = copy.deepcopy(self.gate)
        gate["runtime_fixture"] = None

        report = build_ninth_slice_fixture_closeout(gate)

        self.assertFalse(report["summary"]["m69_complete"])
        self.assertEqual("M69-repair", report["summary"]["next_queue_id"])
        self.assertFalse(report["decision"]["ninth_recipe_enters_fixture_scope"])

    def test_scope_does_not_mutate_runtime_ui_bot_or_system_runtime(self):
        scope = self.report["scope"]

        self.assertTrue(scope["closeout_report"])
        self.assertFalse(scope["changes_fixture_artifact"])
        self.assertFalse(scope["mutates_runtime_deck_library"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_playbook_promotion"])
        self.assertFalse(scope["live_card_text_parsing"])
        self.assertFalse(scope["g_zone_runtime_enabled"])
        self.assertFalse(scope["stride_runtime_enabled"])
        self.assertFalse(scope["aqua_force_battle_order_runtime_enabled"])
        self.assertFalse(scope["GameState_mutation"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m69_closeout.json"
            md_path = out / "m69_closeout.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M69-closeout", loaded["version"])
            self.assertIn("M69 Ninth-Slice Fixture Closeout", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
