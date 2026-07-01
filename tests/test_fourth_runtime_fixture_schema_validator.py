"""Tests for tools/deck/validate_fourth_runtime_fixture_schema.py."""

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

from tools.deck.validate_fourth_runtime_fixture_schema import (  # noqa: E402
    DEFAULT_FIXTURE,
    build_fourth_runtime_fixture_schema_validation_report,
    load_json,
    write_json,
    write_markdown,
)


class TestFourthRuntimeFixtureSchemaValidator(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.fixture = load_json(DEFAULT_FIXTURE)
        cls.report = build_fourth_runtime_fixture_schema_validation_report(cls.fixture)

    def test_valid_fourth_fixture_passes_schema_validation(self):
        summary = self.report["summary"]
        fixture = self.report["fixture_summary"]

        self.assertEqual("M50-01", self.report["version"])
        self.assertTrue(summary["schema_valid"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertEqual(0, summary["issue_count"])
        self.assertTrue(summary["ready_for_m50_02"])
        self.assertEqual("runtime_fixture_m48_recipe_001_g_series_first_royal_paladin_m49_05", fixture["fixture_id"])
        self.assertEqual("m48_recipe_001", fixture["recipe_id"])

    def test_validator_recomputes_counts_from_sqlite(self):
        fixture = self.report["fixture_summary"]

        self.assertEqual(50, fixture["main_deck_count"])
        self.assertEqual(14, fixture["unique_card_count"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, fixture["trigger_counts"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, fixture["grade_counts"])

    def test_scope_does_not_mutate_runtime_ui_bot_or_g_zone(self):
        scope = self.report["scope"]
        policy = self.report["validation_policy"]["g_zone_boundary"]

        self.assertTrue(scope["offline_fixture_schema_validator"])
        self.assertTrue(scope["fourth_fixture_schema_validator"])
        self.assertTrue(scope["g_zone_boundary_enforced"])
        self.assertFalse(scope["mutates_fixture_artifact"])
        self.assertFalse(scope["mutates_runtime_deck_library"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["GameState_mutation"])
        self.assertFalse(policy["g_zone_runtime_enabled"])
        self.assertFalse(policy["stride_runtime_enabled"])

    def test_invalid_runtime_boundary_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["runtime_boundaries"]["g_zone_runtime_enabled"] = True

        report = build_fourth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("g_zone_runtime_boundary_violation", codes)
        self.assertFalse(report["summary"]["ready_for_m50_02"])

    def test_invalid_g_zone_boundary_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["g_zone_boundary"]["stride_runtime_enabled"] = True

        report = build_fourth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("g_zone_boundary_violation", codes)

    def test_invalid_count_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["main_deck"][0]["quantity"] += 1

        report = build_fourth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("main_deck_count_mismatch", codes)
        self.assertIn("grade_profile_mismatch", codes)

    def test_selected_group_mismatch_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["selected_target"]["group"] = "Wrong Group"

        report = build_fourth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("group_mismatch", codes)

    def test_next_target_is_m50_02(self):
        next_target = self.report["next_target"]

        self.assertEqual("M50-02", next_target["milestone"])
        self.assertEqual("Fourth fixture deck text exporter", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m50_01.json"
            md_path = out / "m50_01.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M50-01", loaded["version"])
            self.assertIn("M50-01 Fourth Runtime Fixture Schema Validation", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
