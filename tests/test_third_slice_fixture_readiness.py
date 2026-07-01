"""Tests for tools/deck/build_third_slice_fixture_readiness.py."""

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

from tools.deck.build_third_slice_fixture_readiness import (  # noqa: E402
    M43_01_SELECTION,
    build_third_slice_fixture_readiness,
    load_json,
    write_json,
    write_markdown,
)


class TestThirdSliceFixtureReadiness(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.selection = load_json(M43_01_SELECTION)
        cls.report = build_third_slice_fixture_readiness(cls.selection)

    def test_selected_slice_is_ready_for_semantic_probe(self):
        summary = self.report["summary"]
        selected = self.report["selected_target"]
        readiness = self.report["readiness"]

        self.assertEqual("M43-02", self.report["version"])
        self.assertEqual(self.selection["selected_target"]["group"], selected["group"])
        self.assertEqual("link_joker_legion_mate", selected["era_preset"])
        self.assertTrue(readiness["source_backed"])
        self.assertTrue(readiness["all_fixture_expectations_met"])
        self.assertTrue(readiness["semantic_probe_ready"])
        self.assertTrue(summary["ready_for_m43_03"])

    def test_card_pool_summary_is_source_backed(self):
        pool = self.report["card_pool_summary"]

        self.assertEqual(127, pool["source_card_count"])
        self.assertEqual({"0": 31, "1": 35, "2": 30, "3": 31}, pool["grade_counts"])
        self.assertEqual(84, pool["trigger_capacity"])
        self.assertEqual(424, pool["non_trigger_capacity"])
        self.assertEqual(6, pool["trigger_counts"]["Critical"])
        self.assertEqual(6, pool["trigger_counts"]["Draw"])
        self.assertEqual(3, pool["trigger_counts"]["Heal"])
        self.assertEqual(6, pool["trigger_counts"]["Stand"])
        self.assertEqual({"EB06": 41, "EB10": 86}, pool["series_counts"])

    def test_link_joker_legion_format_scope_requires_new_fixture_scaffold(self):
        scope = self.report["format_scope"]

        self.assertEqual("link_joker_legion_mate", scope["era_preset"])
        self.assertIn("TD07", scope["series_scope"])
        self.assertIn("TD17", scope["series_scope"])
        self.assertIn("BT10", scope["series_scope"])
        self.assertIn("BT17", scope["series_scope"])
        self.assertIn("EB06", scope["series_scope"])
        self.assertIn("EB12", scope["series_scope"])
        self.assertTrue(scope["new_format_or_mechanic_fixtures_required"])
        self.assertEqual("requires_third_slice_fixture_scaffold", scope["policy_reuse_decision"])

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

        report = build_third_slice_fixture_readiness(selection)

        self.assertFalse(report["summary"]["ready_for_m43_03"])
        self.assertEqual("M43-repair", report["next_target"]["milestone"])

    def test_missing_group_routes_to_repair(self):
        selection = copy.deepcopy(self.selection)
        selection["selected_target"]["group"] = "Missing Clan"

        report = build_third_slice_fixture_readiness(selection)

        self.assertFalse(report["readiness"]["source_backed"])
        self.assertFalse(report["summary"]["ready_for_m43_03"])
        self.assertEqual("M43-repair", report["next_target"]["milestone"])

    def test_next_target_is_m43_03(self):
        next_target = self.report["next_target"]

        self.assertEqual("M43-03", next_target["milestone"])
        self.assertEqual("Third-slice semantic/compatibility probe", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m43_02.json"
            md_path = out / "m43_02.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M43-02", loaded["version"])
            self.assertIn("M43-02 Third-Slice Fixture/Format Readiness", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
