"""Tests for tools/deck/validate_seventh_runtime_fixture_schema.py."""

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

import tests.test_seventh_slice_runtime_fixture_promotion_gate as fixture_gate  # noqa: E402
from tools.deck.validate_seventh_runtime_fixture_schema import (  # noqa: E402
    EXPECTED_BLOOM_TOKEN_OPTION,
    EXPECTED_G_ZONE_OPTION,
    build_seventh_runtime_fixture_schema_validation_report,
    write_json,
    write_markdown,
)


class TestSeventhRuntimeFixtureSchemaValidator(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        fixture_gate.TestSeventhSliceRuntimeFixturePromotionGate.setUpClass()
        cls.fixture = fixture_gate.TestSeventhSliceRuntimeFixturePromotionGate.report["runtime_fixture"]
        cls.report = build_seventh_runtime_fixture_schema_validation_report(cls.fixture)

    def test_valid_seventh_fixture_passes_schema_validation(self):
        summary = self.report["summary"]
        fixture = self.report["fixture_summary"]

        self.assertEqual("M62-01", self.report["version"])
        self.assertTrue(summary["schema_valid"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertEqual(0, summary["issue_count"])
        self.assertTrue(summary["ready_for_m62_02"])
        self.assertEqual("runtime_fixture_m60_recipe_001_neo_nectar_m61_06", fixture["fixture_id"])
        self.assertEqual("m60_recipe_001", fixture["recipe_id"])

    def test_validator_recomputes_counts_from_sqlite(self):
        fixture = self.report["fixture_summary"]

        self.assertEqual(50, fixture["main_deck_count"])
        self.assertGreater(fixture["unique_card_count"], 0)
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, fixture["trigger_counts"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, fixture["grade_counts"])
        self.assertEqual({"เนโอ เนคต้า": 50}, fixture["clan_counts"])

    def test_accepts_m61_06_source_artifacts_plural(self):
        codes = [issue["code"] for issue in self.report["issues"]]
        policy = self.report["validation_policy"]["system_boundary"]

        self.assertIn("source_artifacts", self.fixture)
        self.assertNotIn("source_artifact", self.fixture)
        self.assertNotIn("missing_required_field", codes)
        self.assertTrue(policy["accepts_m61_06_source_artifacts_plural"])

    def test_scope_does_not_mutate_runtime_ui_bot_or_system_runtime(self):
        scope = self.report["scope"]
        policy = self.report["validation_policy"]["system_boundary"]

        self.assertTrue(scope["offline_fixture_schema_validator"])
        self.assertTrue(scope["seventh_fixture_schema_validator"])
        self.assertTrue(scope["system_boundary_enforced"])
        self.assertTrue(scope["g_zone_stride_boundary_enforced"])
        self.assertTrue(scope["bloom_token_boundary_enforced"])
        self.assertFalse(scope["mutates_fixture_artifact"])
        self.assertFalse(scope["mutates_runtime_deck_library"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["GameState_mutation"])
        self.assertEqual(EXPECTED_G_ZONE_OPTION, policy["selected_g_zone_option_id"])
        self.assertEqual(EXPECTED_BLOOM_TOKEN_OPTION, policy["selected_bloom_token_option_id"])
        self.assertFalse(policy["g_zone_runtime_enabled"])
        self.assertFalse(policy["stride_runtime_enabled"])
        self.assertFalse(policy["bloom_token_runtime_enabled"])
        self.assertFalse(policy["tokens_allowed_in_main_deck"])

    def test_invalid_runtime_boundary_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["runtime_boundaries"]["bloom_token_runtime_enabled"] = True

        report = build_seventh_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("system_runtime_boundary_violation", codes)
        self.assertFalse(report["summary"]["ready_for_m62_02"])

    def test_invalid_g_zone_policy_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["source_packages"]["selected_g_zone_option_id"] = "defer_until_g_zone_stride_runtime_exists"

        report = build_seventh_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("system_policy_mismatch", codes)

    def test_invalid_bloom_token_policy_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["source_packages"]["selected_bloom_token_option_id"] = "defer_until_bloom_token_runtime_exists"

        report = build_seventh_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("system_policy_mismatch", codes)

    def test_invalid_system_boundary_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["system_boundaries"]["bloom_token_boundary_applied"] = False

        report = build_seventh_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("system_boundary_violation", codes)

    def test_token_main_deck_allowance_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["system_boundaries"]["tokens_allowed_in_main_deck"] = True

        report = build_seventh_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("system_boundary_violation", codes)

    def test_grade_four_main_deck_count_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["count_summary"]["grade4_main_deck_count"] = 1

        report = build_seventh_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("grade4_main_deck_count_violation", codes)

    def test_selected_group_mismatch_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["selected_target"]["group"] = "Wrong Group"

        report = build_seventh_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("group_mismatch", codes)

    def test_next_target_is_m62_02(self):
        next_target = self.report["next_target"]

        self.assertEqual("M62-02", next_target["milestone"])
        self.assertEqual("Seventh fixture deck text exporter", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m62_01.json"
            md_path = out / "m62_01.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M62-01", loaded["version"])
            self.assertIn("M62-01 Seventh Runtime Fixture Schema Validation", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
