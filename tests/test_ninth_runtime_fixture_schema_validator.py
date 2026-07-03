"""Tests for tools/deck/validate_ninth_runtime_fixture_schema.py."""

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

import tests.test_ninth_slice_runtime_fixture_promotion_gate as fixture_gate  # noqa: E402
from tools.deck.validate_ninth_runtime_fixture_schema import (  # noqa: E402
    EXPECTED_AQUA_FORCE_OPTION,
    EXPECTED_G_ZONE_OPTION,
    EXPECTED_STRIDE_OPTION,
    build_ninth_runtime_fixture_schema_validation_report,
    write_json,
    write_markdown,
)


class TestNinthRuntimeFixtureSchemaValidator(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        fixture_gate.TestNinthSliceRuntimeFixturePromotionGate.setUpClass()
        cls.fixture = fixture_gate.TestNinthSliceRuntimeFixturePromotionGate.report["runtime_fixture"]
        cls.report = build_ninth_runtime_fixture_schema_validation_report(cls.fixture)

    def test_valid_ninth_fixture_passes_schema_validation(self):
        summary = self.report["summary"]
        fixture = self.report["fixture_summary"]

        self.assertEqual("M70-01", self.report["version"])
        self.assertTrue(summary["schema_valid"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertEqual(0, summary["issue_count"])
        self.assertTrue(summary["ready_for_m70_02"])
        self.assertEqual("runtime_fixture_m68_recipe_001_aqua_force_m69_06", fixture["fixture_id"])
        self.assertEqual("m68_recipe_001", fixture["recipe_id"])

    def test_validator_recomputes_counts_from_sqlite(self):
        fixture = self.report["fixture_summary"]

        self.assertEqual(50, fixture["main_deck_count"])
        self.assertGreater(fixture["unique_card_count"], 0)
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, fixture["trigger_counts"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, fixture["grade_counts"])
        self.assertEqual({"อควอฟอร์ซ": 50}, fixture["clan_counts"])

    def test_accepts_m69_06_source_artifacts_plural(self):
        codes = [issue["code"] for issue in self.report["issues"]]
        policy = self.report["validation_policy"]["system_boundary"]

        self.assertIn("source_artifacts", self.fixture)
        self.assertNotIn("source_artifact", self.fixture)
        self.assertNotIn("missing_required_field", codes)
        self.assertTrue(policy["accepts_m69_06_source_artifacts_plural"])

    def test_scope_does_not_mutate_runtime_ui_bot_or_system_runtime(self):
        scope = self.report["scope"]
        policy = self.report["validation_policy"]["system_boundary"]

        self.assertTrue(scope["offline_fixture_schema_validator"])
        self.assertTrue(scope["ninth_fixture_schema_validator"])
        self.assertTrue(scope["system_boundary_enforced"])
        self.assertTrue(scope["g_zone_boundary_enforced"])
        self.assertTrue(scope["stride_boundary_enforced"])
        self.assertTrue(scope["aqua_force_boundary_enforced"])
        self.assertTrue(scope["saved_deck_boundary_enforced"])
        self.assertFalse(scope["mutates_fixture_artifact"])
        self.assertFalse(scope["mutates_runtime_deck_library"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["GameState_mutation"])
        self.assertEqual(EXPECTED_G_ZONE_OPTION, policy["selected_g_zone_option_id"])
        self.assertEqual(EXPECTED_STRIDE_OPTION, policy["selected_stride_option_id"])
        self.assertEqual(EXPECTED_AQUA_FORCE_OPTION, policy["selected_aqua_force_option_id"])
        self.assertFalse(policy["g_zone_runtime_enabled"])
        self.assertFalse(policy["stride_runtime_enabled"])
        self.assertFalse(policy["aqua_force_battle_order_runtime_enabled"])

    def test_invalid_g_zone_runtime_boundary_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["runtime_boundaries"]["g_zone_runtime_enabled"] = True

        report = build_ninth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("system_runtime_boundary_violation", codes)
        self.assertFalse(report["summary"]["ready_for_m70_02"])

    def test_invalid_stride_runtime_boundary_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["runtime_boundaries"]["stride_runtime_enabled"] = True

        report = build_ninth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("system_runtime_boundary_violation", codes)

    def test_invalid_aqua_force_runtime_boundary_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["runtime_boundaries"]["aqua_force_battle_order_runtime_enabled"] = True

        report = build_ninth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("system_runtime_boundary_violation", codes)

    def test_saved_deck_boundary_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["runtime_boundaries"]["saved_deck_injection"] = True

        report = build_ninth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("system_runtime_boundary_violation", codes)

    def test_invalid_g_zone_policy_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["source_packages"]["selected_g_zone_option_id"] = "defer_until_g_zone_runtime_exists"

        report = build_ninth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("system_policy_mismatch", codes)

    def test_invalid_stride_policy_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["source_packages"]["selected_stride_option_id"] = "defer_until_stride_runtime_exists"

        report = build_ninth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("system_policy_mismatch", codes)

    def test_invalid_aqua_force_policy_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["source_packages"]["selected_aqua_force_option_id"] = (
            "defer_until_aqua_force_battle_order_runtime_exists"
        )

        report = build_ninth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("system_policy_mismatch", codes)

    def test_invalid_system_boundary_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["system_boundaries"]["g_zone_boundary_applied"] = False

        report = build_ninth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("system_boundary_violation", codes)

    def test_aqua_force_battle_counter_boundary_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["system_boundaries"]["battle_count_tracker_enabled"] = True

        report = build_ninth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("system_boundary_violation", codes)

    def test_grade_four_main_deck_count_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["count_summary"]["grade4_main_deck_count"] = 1

        report = build_ninth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("grade4_main_deck_count_violation", codes)

    def test_selected_group_mismatch_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["selected_target"]["group"] = "Wrong Group"

        report = build_ninth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("group_mismatch", codes)

    def test_next_target_is_m70_02(self):
        next_target = self.report["next_target"]

        self.assertEqual("M70-02", next_target["milestone"])
        self.assertEqual("Ninth fixture deck text exporter", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m70_01.json"
            md_path = out / "m70_01.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M70-01", loaded["version"])
            self.assertIn("M70-01 Ninth Runtime Fixture Schema Validation", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
