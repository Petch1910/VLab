"""Tests for tools/deck/build_second_slice_runtime_fixture_promotion_gate.py."""

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

from tools.deck.build_second_slice_runtime_fixture_promotion_gate import (  # noqa: E402
    M40_COMBO_CONSISTENCY,
    M41_REPAIR_VALIDATE,
    M41_TRIGGER_REPAIR_ACCEPTED,
    build_second_slice_runtime_fixture_promotion_gate,
    load_json,
    write_json,
    write_markdown,
)


class TestSecondSliceRuntimeFixturePromotionGate(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.accepted = load_json(M41_TRIGGER_REPAIR_ACCEPTED)
        cls.validation = load_json(M41_REPAIR_VALIDATE)
        cls.consistency = load_json(M40_COMBO_CONSISTENCY)
        cls.report = build_second_slice_runtime_fixture_promotion_gate(
            cls.accepted,
            cls.validation,
            cls.consistency,
        )

    def test_gate_allows_fixture_when_all_checks_pass(self):
        summary = self.report["summary"]
        decision = self.report["promotion_decision"]

        self.assertEqual("M41-04", self.report["version"])
        self.assertEqual("m40_recipe_001", summary["recipe_id"])
        self.assertTrue(summary["promotion_allowed"])
        self.assertEqual(6, summary["passed_check_count"])
        self.assertEqual(0, summary["failed_check_count"])
        self.assertTrue(decision["fixture_created"])
        self.assertFalse(decision["runtime_deck_library_mutated"])
        self.assertFalse(decision["saved_deck_injected"])
        self.assertFalse(decision["ui_deck_list_published"])
        self.assertFalse(decision["bot_playbook_enabled"])

    def test_gate_checks_have_required_categories(self):
        checks = {item["check_id"]: item for item in self.report["gate_checks"]}

        self.assertEqual(
            {
                "human_acceptance",
                "validation",
                "grade_profile_review",
                "combo_pair_consistency",
                "manual_review_cleared_after_repair",
                "runtime_boundary",
            },
            set(checks),
        )
        self.assertTrue(all(item["passed"] for item in checks.values()))

    def test_runtime_fixture_is_fixture_only_with_classic_counts(self):
        fixture = self.report["runtime_fixture"]

        self.assertEqual("deck_recipe_runtime_fixture_v1", fixture["schema_version"])
        self.assertEqual("offline_runtime_test_fixture", fixture["fixture_scope"])
        self.assertEqual("m40_recipe_001", fixture["recipe_id"])
        self.assertEqual(50, sum(row["quantity"] for row in fixture["main_deck"]))
        self.assertEqual(50, fixture["count_summary"]["main_deck_count"])
        self.assertEqual(16, fixture["count_summary"]["trigger_count"])
        self.assertEqual(
            {"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4},
            fixture["count_summary"]["trigger_counts"],
        )
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, fixture["count_summary"]["grade_counts"])
        self.assertEqual("m40_recipe_001_grade_profile_pkg_001", fixture["source_packages"]["base_grade_package_id"])
        self.assertEqual("m41_repair_pkg_001", fixture["source_packages"]["trigger_repair_package_id"])
        self.assertTrue(fixture["runtime_boundaries"]["test_fixture_only"])
        self.assertFalse(fixture["runtime_boundaries"]["auto_injected_into_player_decks"])
        self.assertFalse(fixture["runtime_boundaries"]["bot_playbook_enabled"])
        self.assertFalse(fixture["runtime_boundaries"]["ui_deck_library_mutated"])
        self.assertFalse(fixture["runtime_boundaries"]["game_state_mutated"])

    def test_gate_blocks_when_human_acceptance_is_missing(self):
        accepted = copy.deepcopy(self.accepted)
        accepted["summary"]["human_acceptance_recorded"] = False
        accepted["acceptance_record"]["decision"] = "pending"

        report = build_second_slice_runtime_fixture_promotion_gate(accepted, self.validation, self.consistency)
        checks = {item["check_id"]: item for item in report["gate_checks"]}

        self.assertFalse(report["summary"]["promotion_allowed"])
        self.assertEqual(1, report["summary"]["failed_check_count"])
        self.assertFalse(report["promotion_decision"]["fixture_created"])
        self.assertIsNone(report["runtime_fixture"])
        self.assertFalse(checks["human_acceptance"]["passed"])

    def test_gate_blocks_when_pair_consistency_is_missing(self):
        consistency = copy.deepcopy(self.consistency)
        for check in consistency["consistency_checks"]:
            if check["recipe_id"] == "m40_recipe_001":
                check["pair_cards_present"] = False
                check["missing_pair_card_ids"] = ["BT01-006TH"]
                break

        report = build_second_slice_runtime_fixture_promotion_gate(self.accepted, self.validation, consistency)
        checks = {item["check_id"]: item for item in report["gate_checks"]}

        self.assertFalse(report["summary"]["promotion_allowed"])
        self.assertEqual(1, report["summary"]["failed_check_count"])
        self.assertFalse(checks["combo_pair_consistency"]["passed"])

    def test_scope_does_not_mutate_runtime_ui_or_bot(self):
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

    def test_next_target_is_m41_closeout(self):
        next_target = self.report["next_target"]

        self.assertTrue(self.report["summary"]["ready_for_m41_closeout"])
        self.assertEqual("M41-closeout", next_target["milestone"])
        self.assertEqual("Second-slice fixture closeout", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m41_04.json"
            md_path = out / "m41_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M41-04", loaded["version"])
            self.assertIn(
                "M41-04 Second-Slice Runtime Fixture Promotion Gate",
                md_path.read_text(encoding="utf-8"),
            )


if __name__ == "__main__":
    unittest.main()
