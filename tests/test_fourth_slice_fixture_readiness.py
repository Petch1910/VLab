"""Tests for tools/deck/build_fourth_slice_fixture_readiness.py."""

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

from tools.deck.build_fourth_slice_fixture_readiness import (  # noqa: E402
    M47_01_SELECTION,
    build_fourth_slice_fixture_readiness,
    load_json,
    write_json,
    write_markdown,
)


class TestFourthSliceFixtureReadiness(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.selection = load_json(M47_01_SELECTION)
        cls.report = build_fourth_slice_fixture_readiness(cls.selection)

    def test_selected_slice_is_source_backed_but_needs_repair(self):
        summary = self.report["summary"]
        selected = self.report["selected_target"]
        readiness = self.report["readiness"]

        self.assertEqual("M47-02", self.report["version"])
        self.assertEqual(self.selection["selected_target"]["group"], selected["group"])
        self.assertEqual("g_series_first", selected["era_preset"])
        self.assertTrue(readiness["source_backed"])
        self.assertFalse(readiness["all_fixture_expectations_met"])
        self.assertFalse(readiness["semantic_probe_ready"])
        self.assertTrue(summary["repair_required"])
        self.assertFalse(summary["ready_for_m47_03"])

    def test_card_pool_summary_exposes_trigger_gap(self):
        pool = self.report["card_pool_summary"]

        self.assertEqual(71, pool["source_card_count"])
        self.assertEqual({"0": 15, "1": 22, "2": 18, "3": 9, "4": 7}, pool["grade_counts"])
        self.assertEqual(40, pool["trigger_capacity"])
        self.assertEqual(244, pool["non_trigger_capacity"])
        self.assertEqual(6, pool["trigger_counts"]["Critical"])
        self.assertEqual(1, pool["trigger_counts"]["Draw"])
        self.assertEqual(3, pool["trigger_counts"]["Stand"])
        self.assertNotIn("Heal", pool["trigger_counts"])
        self.assertEqual(["Heal"], pool["trigger_type_gaps"])
        self.assertEqual({"G-BT01": 14, "G-BT02": 6, "G-BT04": 8, "G-BT06": 15, "G-BT08": 12, "G-TD02": 16}, pool["series_counts"])

    def test_g_series_first_scope_is_applied(self):
        scope = self.report["format_scope"]

        self.assertEqual("g_series_first", scope["era_preset"])
        self.assertIn("G-TD01", scope["series_scope"])
        self.assertIn("G-TD09", scope["series_scope"])
        self.assertIn("G-SD01", scope["series_scope"])
        self.assertIn("G-SD02", scope["series_scope"])
        self.assertIn("G-BT01", scope["series_scope"])
        self.assertIn("G-BT08", scope["series_scope"])
        self.assertIn("G-CB01", scope["series_scope"])
        self.assertIn("G-CB04", scope["series_scope"])
        self.assertIn("G-TCB01", scope["series_scope"])
        self.assertIn("G-TCB02", scope["series_scope"])
        self.assertTrue(scope["new_format_or_mechanic_fixtures_required"])
        self.assertEqual("requires_fourth_slice_readiness_repair", scope["policy_reuse_decision"])

    def test_scope_does_not_create_runtime_assets(self):
        boundary = self.report["runtime_boundary"]
        readiness = self.report["readiness"]

        self.assertTrue(boundary["offline_fixture_readiness_only"])
        self.assertTrue(boundary["does_not_create_deck"])
        self.assertTrue(boundary["does_not_create_runtime_fixture"])
        self.assertTrue(boundary["does_not_mutate_runtime_pack"])
        self.assertTrue(boundary["does_not_publish_to_ui"])
        self.assertTrue(boundary["does_not_publish_to_bot"])
        self.assertFalse(boundary["GameState_mutation"])
        self.assertFalse(readiness["runtime_or_bot_promotion_allowed"])

    def test_unknown_era_routes_to_repair(self):
        selection = copy.deepcopy(self.selection)
        selection["selected_target"]["era_preset"] = "unknown_era"

        report = build_fourth_slice_fixture_readiness(selection)

        self.assertFalse(report["summary"]["ready_for_m47_03"])
        self.assertEqual("M47-repair", report["next_target"]["milestone"])

    def test_missing_group_routes_to_repair(self):
        selection = copy.deepcopy(self.selection)
        selection["selected_target"]["group"] = "Missing Clan"

        report = build_fourth_slice_fixture_readiness(selection)

        self.assertFalse(report["readiness"]["source_backed"])
        self.assertFalse(report["summary"]["ready_for_m47_03"])
        self.assertEqual("M47-repair", report["next_target"]["milestone"])

    def test_next_target_is_m47_repair(self):
        next_target = self.report["next_target"]

        self.assertEqual("M47-repair", next_target["milestone"])
        self.assertEqual("Repair fourth-slice readiness blockers", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m47_02.json"
            md_path = out / "m47_02.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M47-02", loaded["version"])
            self.assertIn("M47-02 Fourth-Slice Fixture/Format Readiness", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
