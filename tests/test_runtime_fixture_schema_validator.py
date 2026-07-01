"""Tests for tools/deck/validate_runtime_fixture_schema.py (M39-01)."""

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

from tools.deck.validate_runtime_fixture_schema import (  # noqa: E402
    DEFAULT_FIXTURE,
    load_json,
    validate_fixture_schema,
    write_json,
    write_markdown,
)


class TestRuntimeFixtureSchemaValidator(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.fixture = load_json(DEFAULT_FIXTURE)
        cls.report = validate_fixture_schema(cls.fixture)

    def test_valid_fixture_passes_schema_validation(self):
        summary = self.report["summary"]
        fixture = self.report["fixture_summary"]

        self.assertEqual("M39-01", self.report["version"])
        self.assertTrue(summary["schema_valid"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertEqual(0, summary["issue_count"])
        self.assertTrue(summary["ready_for_m39_02"])
        self.assertEqual("runtime_fixture_recipe_003_classic_core_nova_grappler_m38_04", fixture["fixture_id"])
        self.assertEqual("recipe_003", fixture["recipe_id"])

    def test_validator_recomputes_classic_counts_from_sqlite(self):
        fixture = self.report["fixture_summary"]

        self.assertEqual(50, fixture["main_deck_count"])
        self.assertEqual(17, fixture["unique_card_count"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, fixture["trigger_counts"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, fixture["grade_counts"])

    def test_scope_does_not_mutate_runtime_or_bot(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_fixture_schema_validator"])
        self.assertFalse(scope["mutates_fixture_artifact"])
        self.assertFalse(scope["mutates_runtime_deck_library"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["GameState_mutation"])

    def test_invalid_runtime_boundary_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["runtime_boundaries"]["bot_playbook_enabled"] = True

        report = validate_fixture_schema(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("runtime_boundary_violation", codes)
        self.assertFalse(report["summary"]["ready_for_m39_02"])

    def test_invalid_count_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["main_deck"][0]["quantity"] += 1

        report = validate_fixture_schema(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("main_deck_count_mismatch", codes)
        self.assertIn("grade_profile_mismatch", codes)

    def test_next_target_is_m39_02(self):
        next_target = self.report["next_target"]

        self.assertEqual("M39-02", next_target["milestone"])
        self.assertEqual("Fixture-to-deck text exporter", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m39_01.json"
            md_path = out / "m39_01.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M39-01", loaded["version"])
            self.assertIn("M39-01 Offline Fixture Schema Validation", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
