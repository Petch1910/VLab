"""Tests for tools/deck/build_eighth_slice_fixture_readiness.py."""

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

import tests.test_eighth_target_slice_selection as eighth_selection_fixture  # noqa: E402
from tools.combo.discover_clan_combos import ERA_SET_SPECS, expand_set_specs  # noqa: E402
from tools.deck.build_eighth_slice_fixture_readiness import (  # noqa: E402
    build_eighth_slice_fixture_readiness,
    write_json,
    write_markdown,
)


class TestEighthSliceFixtureReadiness(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        eighth_selection_fixture.TestEighthTargetSliceSelection.setUpClass()
        cls.selection = eighth_selection_fixture.TestEighthTargetSliceSelection.report
        cls.report = build_eighth_slice_fixture_readiness(cls.selection)

    def test_selected_slice_is_ready_for_semantic_probe(self):
        summary = self.report["summary"]
        selected = self.report["selected_target"]
        readiness = self.report["readiness"]

        self.assertEqual("M63-02", self.report["version"])
        self.assertEqual(self.selection["selected_target"]["group"], selected["group"])
        self.assertEqual(self.selection["selected_target"]["era_preset"], selected["era_preset"])
        self.assertTrue(readiness["selection_ready"])
        self.assertTrue(readiness["source_backed"])
        self.assertTrue(readiness["has_grade_setup"])
        self.assertTrue(readiness["has_required_g_unit_pool"])
        self.assertTrue(readiness["all_fixture_expectations_met"])
        self.assertTrue(readiness["semantic_probe_ready"])
        self.assertFalse(summary["repair_required"])
        self.assertTrue(summary["ready_for_m63_03"])

    def test_card_pool_summary_is_source_backed(self):
        pool = self.report["card_pool_summary"]

        self.assertGreater(pool["source_card_count"], 0)
        self.assertGreaterEqual(pool["trigger_capacity"], 16)
        self.assertGreaterEqual(pool["non_trigger_capacity"], 34)
        self.assertEqual([], pool["trigger_type_gaps"])
        for grade in range(4):
            self.assertGreater(pool["grade_counts"][str(grade)], 0)
        if self.report["format_scope"]["g_zone_fixture_boundary_required"]:
            self.assertGreater(pool["grade_counts"]["4"], 0)

    def test_selected_era_scope_is_applied(self):
        scope = self.report["format_scope"]
        era_preset = self.selection["selected_target"]["era_preset"]

        self.assertEqual(era_preset, scope["era_preset"])
        self.assertEqual(list(ERA_SET_SPECS[era_preset]), scope["set_specs"])
        self.assertEqual(expand_set_specs(ERA_SET_SPECS[era_preset]), scope["series_scope"])
        self.assertEqual(era_preset != "classic_part1", scope["new_format_or_mechanic_fixtures_required"])
        self.assertEqual("requires_eighth_slice_fixture_scaffold", scope["policy_reuse_decision"])

    def test_scope_does_not_create_runtime_assets(self):
        boundary = self.report["runtime_boundary"]
        readiness = self.report["readiness"]

        self.assertTrue(boundary["offline_fixture_readiness_only"])
        self.assertTrue(boundary["does_not_create_deck"])
        self.assertTrue(boundary["does_not_create_runtime_fixture"])
        self.assertTrue(boundary["does_not_mutate_runtime_pack"])
        self.assertTrue(boundary["does_not_publish_to_ui"])
        self.assertTrue(boundary["does_not_publish_to_bot"])
        self.assertFalse(boundary["g_zone_runtime_enabled"])
        self.assertFalse(boundary["stride_runtime_enabled"])
        self.assertFalse(boundary["bloom_token_runtime_enabled"])
        self.assertFalse(boundary["GameState_mutation"])
        self.assertFalse(readiness["runtime_or_bot_promotion_allowed"])

    def test_unknown_era_routes_to_repair(self):
        selection = copy.deepcopy(self.selection)
        selection["selected_target"]["era_preset"] = "unknown_era"

        report = build_eighth_slice_fixture_readiness(selection)

        self.assertFalse(report["summary"]["ready_for_m63_03"])
        self.assertIn("era_scope_missing", report["summary"]["repair_reasons"])
        self.assertEqual("M63-repair", report["next_target"]["milestone"])

    def test_missing_group_routes_to_repair(self):
        selection = copy.deepcopy(self.selection)
        selection["selected_target"]["group"] = "Missing Clan"

        report = build_eighth_slice_fixture_readiness(selection)

        self.assertFalse(report["readiness"]["source_backed"])
        self.assertFalse(report["summary"]["ready_for_m63_03"])
        self.assertIn("source_pool_missing", report["summary"]["repair_reasons"])
        self.assertEqual("M63-repair", report["next_target"]["milestone"])

    def test_selection_not_ready_routes_to_repair(self):
        selection = copy.deepcopy(self.selection)
        selection["summary"]["ready_for_m63_02"] = False

        report = build_eighth_slice_fixture_readiness(selection)

        self.assertFalse(report["readiness"]["selection_ready"])
        self.assertFalse(report["summary"]["ready_for_m63_03"])
        self.assertIn("selection_not_ready", report["summary"]["repair_reasons"])
        self.assertEqual("M63-repair", report["next_target"]["milestone"])

    def test_next_target_is_m63_03(self):
        next_target = self.report["next_target"]

        self.assertEqual("M63-03", next_target["milestone"])
        self.assertEqual("Eighth-slice semantic/compatibility probe", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m63_02.json"
            md_path = out / "m63_02.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M63-02", loaded["version"])
            self.assertIn("M63-02 Eighth-Slice Fixture/Format Readiness", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
