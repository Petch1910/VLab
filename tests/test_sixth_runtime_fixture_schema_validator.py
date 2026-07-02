"""Tests for tools/deck/validate_sixth_runtime_fixture_schema.py."""

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

from tools.deck.build_sixth_slice_g_zone_stride_decision_artifact import (  # noqa: E402
    build_sixth_slice_g_zone_stride_decision_artifact,
)
from tools.deck.build_sixth_slice_human_accepted_repair_artifact import (  # noqa: E402
    M56_DRAFTS,
    M56_SCAFFOLD,
    build_sixth_slice_human_accepted_repair_artifact,
    load_json,
)
from tools.deck.build_sixth_slice_human_selected_recipe_artifact import (  # noqa: E402
    M57_01_REVIEW,
    build_sixth_slice_human_selected_recipe_artifact,
)
from tools.deck.build_sixth_slice_runtime_fixture_promotion_gate import (  # noqa: E402
    build_sixth_slice_runtime_fixture_promotion_gate,
)
from tools.deck.validate_sixth_runtime_fixture_schema import (  # noqa: E402
    EXPECTED_G_ZONE_OPTION,
    build_sixth_runtime_fixture_schema_validation_report,
    write_json,
    write_markdown,
)
from tools.deck.validate_sixth_slice_repaired_recipe import (  # noqa: E402
    build_sixth_slice_repaired_validation_report,
)


SELECTED_REVIEW_ITEM_ID = "m57_01_m56_recipe_001_repair_review"


class TestSixthRuntimeFixtureSchemaValidator(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        review_packet = load_json(M57_01_REVIEW)
        selected_artifact = build_sixth_slice_human_selected_recipe_artifact(
            review_packet,
            selected_review_item_id=SELECTED_REVIEW_ITEM_ID,
            selection_text="explicit test selection",
            selected_by="unit-test",
            selected_at="2026-07-01",
        )
        accepted_artifact = build_sixth_slice_human_accepted_repair_artifact(
            selected_artifact,
            load_json(M56_DRAFTS),
            load_json(M56_SCAFFOLD),
            acceptance_text="explicit test acceptance",
            accepted_by="unit-test",
            accepted_at="2026-07-01",
        )
        g_zone_decision = build_sixth_slice_g_zone_stride_decision_artifact(
            accepted_artifact,
            selected_option=EXPECTED_G_ZONE_OPTION,
        )
        validation_report = build_sixth_slice_repaired_validation_report(
            accepted_artifact,
            g_zone_decision,
        )
        gate_report = build_sixth_slice_runtime_fixture_promotion_gate(
            accepted_artifact,
            validation_report,
        )
        cls.fixture = gate_report["runtime_fixture"]
        cls.report = build_sixth_runtime_fixture_schema_validation_report(cls.fixture)

    def test_valid_sixth_fixture_passes_schema_validation(self):
        summary = self.report["summary"]
        fixture = self.report["fixture_summary"]

        self.assertEqual("M58-01", self.report["version"])
        self.assertTrue(summary["schema_valid"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertEqual(0, summary["issue_count"])
        self.assertTrue(summary["ready_for_m58_02"])
        self.assertEqual("runtime_fixture_m56_recipe_001_shadow_paladin_m57_06", fixture["fixture_id"])
        self.assertEqual("m56_recipe_001", fixture["recipe_id"])

    def test_validator_recomputes_counts_from_sqlite(self):
        fixture = self.report["fixture_summary"]

        self.assertEqual(50, fixture["main_deck_count"])
        self.assertGreater(fixture["unique_card_count"], 0)
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, fixture["trigger_counts"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, fixture["grade_counts"])

    def test_accepts_m57_06_source_artifacts_plural(self):
        codes = [issue["code"] for issue in self.report["issues"]]
        policy = self.report["validation_policy"]["g_zone_stride_boundary"]

        self.assertIn("source_artifacts", self.fixture)
        self.assertNotIn("source_artifact", self.fixture)
        self.assertNotIn("missing_required_field", codes)
        self.assertTrue(policy["accepts_m57_06_source_artifacts_plural"])

    def test_scope_does_not_mutate_runtime_ui_bot_or_g_zone(self):
        scope = self.report["scope"]
        policy = self.report["validation_policy"]["g_zone_stride_boundary"]

        self.assertTrue(scope["offline_fixture_schema_validator"])
        self.assertTrue(scope["sixth_fixture_schema_validator"])
        self.assertTrue(scope["g_zone_stride_boundary_enforced"])
        self.assertFalse(scope["mutates_fixture_artifact"])
        self.assertFalse(scope["mutates_runtime_deck_library"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["GameState_mutation"])
        self.assertFalse(policy["g_zone_runtime_enabled"])
        self.assertFalse(policy["stride_runtime_enabled"])

    def test_invalid_runtime_boundary_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["runtime_boundaries"]["g_zone_runtime_enabled"] = True

        report = build_sixth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("g_zone_runtime_boundary_violation", codes)
        self.assertFalse(report["summary"]["ready_for_m58_02"])

    def test_invalid_g_zone_boundary_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["g_zone_boundary"]["selected_option_id"] = "defer_until_g_zone_runtime_exists"

        report = build_sixth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("g_zone_boundary_violation", codes)

    def test_invalid_g_zone_policy_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["source_packages"]["selected_g_zone_option_id"] = "defer_until_g_zone_runtime_exists"

        report = build_sixth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("g_zone_policy_mismatch", codes)

    def test_grade_four_main_deck_count_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["count_summary"]["grade4_main_deck_count"] = 1

        report = build_sixth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("grade4_main_deck_count_violation", codes)

    def test_selected_group_mismatch_blocks_fixture(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["selected_target"]["group"] = "Wrong Group"

        report = build_sixth_runtime_fixture_schema_validation_report(fixture)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["schema_valid"])
        self.assertIn("group_mismatch", codes)

    def test_next_target_is_m58_02(self):
        next_target = self.report["next_target"]

        self.assertEqual("M58-02", next_target["milestone"])
        self.assertEqual("Sixth fixture deck text exporter", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m58_01.json"
            md_path = out / "m58_01.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M58-01", loaded["version"])
            self.assertIn("M58-01 Sixth Runtime Fixture Schema Validation", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
