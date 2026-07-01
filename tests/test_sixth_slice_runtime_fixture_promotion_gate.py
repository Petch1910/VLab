"""Tests for tools/deck/build_sixth_slice_runtime_fixture_promotion_gate.py (M57-06)."""

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
    write_json,
    write_markdown,
)
from tools.deck.validate_sixth_slice_repaired_recipe import (  # noqa: E402
    build_sixth_slice_repaired_validation_report,
)


SELECTED_REVIEW_ITEM_ID = "m57_01_m56_recipe_001_repair_review"


class TestSixthSliceRuntimeFixturePromotionGate(unittest.TestCase):
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
        cls.accepted_artifact = build_sixth_slice_human_accepted_repair_artifact(
            selected_artifact,
            load_json(M56_DRAFTS),
            load_json(M56_SCAFFOLD),
            acceptance_text="explicit test acceptance",
            accepted_by="unit-test",
            accepted_at="2026-07-01",
        )
        g_zone_decision = build_sixth_slice_g_zone_stride_decision_artifact(
            cls.accepted_artifact,
            selected_option="main_deck_only_review_no_runtime_promotion",
        )
        cls.validation_report = build_sixth_slice_repaired_validation_report(
            cls.accepted_artifact,
            g_zone_decision,
        )
        cls.report = build_sixth_slice_runtime_fixture_promotion_gate(
            cls.accepted_artifact,
            cls.validation_report,
        )

    def test_gate_allows_fixture_when_all_checks_pass(self):
        summary = self.report["summary"]
        decision = self.report["promotion_decision"]

        self.assertEqual("M57-06", self.report["version"])
        self.assertEqual("m56_recipe_001", summary["recipe_id"])
        self.assertEqual(SELECTED_REVIEW_ITEM_ID, summary["accepted_review_item_id"])
        self.assertTrue(summary["promotion_allowed"])
        self.assertEqual(7, summary["passed_check_count"])
        self.assertEqual(0, summary["failed_check_count"])
        self.assertTrue(summary["fixture_created"])
        self.assertTrue(summary["ready_for_m57_closeout"])
        self.assertTrue(decision["fixture_created"])
        self.assertFalse(decision["runtime_deck_library_mutated"])
        self.assertFalse(decision["saved_deck_injected"])
        self.assertFalse(decision["ui_deck_list_published"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertFalse(decision["g_zone_runtime_enabled"])
        self.assertFalse(decision["stride_runtime_enabled"])

    def test_gate_checks_have_required_categories(self):
        checks = {item["check_id"]: item for item in self.report["gate_checks"]}

        self.assertEqual(
            {
                "human_selection_and_acceptance",
                "g_zone_boundary",
                "validation",
                "combo_consistency",
                "accepted_repair_rows",
                "rerun_ready",
                "runtime_boundary",
            },
            set(checks),
        )
        self.assertTrue(all(item["passed"] for item in checks.values()))

    def test_runtime_fixture_is_fixture_only_with_shadow_paladin_counts(self):
        fixture = self.report["runtime_fixture"]

        self.assertEqual("deck_recipe_runtime_fixture_v1", fixture["schema_version"])
        self.assertEqual("offline_runtime_test_fixture", fixture["fixture_scope"])
        self.assertEqual("m56_recipe_001", fixture["recipe_id"])
        self.assertEqual(50, sum(row["quantity"] for row in fixture["main_deck"]))
        self.assertEqual(50, fixture["count_summary"]["main_deck_count"])
        self.assertEqual(16, fixture["count_summary"]["trigger_count"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, fixture["count_summary"]["trigger_counts"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, fixture["count_summary"]["grade_counts"])
        self.assertEqual(0, fixture["count_summary"]["grade4_main_deck_count"])
        self.assertEqual({"ชาโดว์ พาลาดิน": 50}, fixture["count_summary"]["clan_counts"])
        self.assertEqual(SELECTED_REVIEW_ITEM_ID, fixture["source_packages"]["selected_review_item_id"])
        self.assertEqual(
            "main_deck_only_review_no_runtime_promotion",
            fixture["source_packages"]["selected_g_zone_option_id"],
        )

    def test_fixture_keeps_g_zone_stride_and_runtime_boundaries_disabled(self):
        fixture = self.report["runtime_fixture"]
        boundary = fixture["g_zone_boundary"]
        runtime = fixture["runtime_boundaries"]

        self.assertEqual("main_deck_only_review_no_runtime_promotion", boundary["selected_option_id"])
        self.assertTrue(boundary["main_deck_only_validation_allowed"])
        self.assertFalse(boundary["g_zone_runtime_enabled"])
        self.assertFalse(boundary["stride_runtime_enabled"])
        self.assertFalse(boundary["grade4_main_deck_allowed"])
        self.assertFalse(boundary["g_units_allowed_in_main_deck"])
        self.assertTrue(runtime["test_fixture_only"])
        self.assertFalse(runtime["auto_injected_into_player_decks"])
        self.assertFalse(runtime["bot_playbook_enabled"])
        self.assertFalse(runtime["ui_deck_library_mutated"])
        self.assertFalse(runtime["game_state_mutated"])
        self.assertFalse(runtime["g_zone_runtime_enabled"])
        self.assertFalse(runtime["stride_runtime_enabled"])

    def test_gate_blocks_when_g_zone_boundary_is_not_main_deck_only(self):
        validation = copy.deepcopy(self.validation_report)
        validation["g_zone_decision_context"]["selected_option_id"] = "defer_until_g_zone_runtime_exists"
        validation["g_zone_decision_context"]["main_deck_only_boundary_applied"] = False

        report = build_sixth_slice_runtime_fixture_promotion_gate(self.accepted_artifact, validation)

        self.assertFalse(report["summary"]["promotion_allowed"])
        self.assertEqual(1, report["summary"]["failed_check_count"])
        self.assertFalse(report["promotion_decision"]["fixture_created"])
        self.assertIsNone(report["runtime_fixture"])
        checks = {item["check_id"]: item for item in report["gate_checks"]}
        self.assertFalse(checks["g_zone_boundary"]["passed"])

    def test_gate_blocks_when_repaired_rows_do_not_match_validation_recipe(self):
        accepted = copy.deepcopy(self.accepted_artifact)
        accepted["summary"]["accepted_recipe_id"] = "wrong_recipe"

        report = build_sixth_slice_runtime_fixture_promotion_gate(accepted, self.validation_report)

        self.assertFalse(report["summary"]["promotion_allowed"])
        self.assertEqual(1, report["summary"]["failed_check_count"])
        checks = {item["check_id"]: item for item in report["gate_checks"]}
        self.assertFalse(checks["accepted_repair_rows"]["passed"])

    def test_scope_does_not_mutate_runtime_ui_bot_or_g_zone(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_promotion_gate"])
        self.assertTrue(scope["creates_runtime_test_fixture_artifact"])
        self.assertFalse(scope["mutates_runtime_deck_library"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_playbook_promotion"])
        self.assertFalse(scope["live_card_text_parsing"])
        self.assertFalse(scope["GameState_mutation"])
        self.assertFalse(scope["g_zone_runtime_enabled"])
        self.assertFalse(scope["stride_runtime_enabled"])

    def test_next_target_is_m57_closeout(self):
        next_target = self.report["next_target"]

        self.assertEqual("M57-closeout", next_target["milestone"])
        self.assertEqual("Sixth-slice fixture closeout", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m57_06.json"
            md_path = out / "m57_06.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M57-06", loaded["version"])
            self.assertTrue(loaded["summary"]["promotion_allowed"])
            self.assertIn("M57-06 Sixth-Slice Runtime Fixture Promotion Gate", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
