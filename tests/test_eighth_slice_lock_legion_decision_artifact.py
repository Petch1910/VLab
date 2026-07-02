"""Tests for tools/deck/build_eighth_slice_lock_legion_decision_artifact.py (M65-04)."""

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

import tests.test_eighth_slice_human_accepted_grade_repair_artifact as accepted_fixture  # noqa: E402
from tools.deck.build_eighth_slice_human_accepted_grade_repair_artifact import (  # noqa: E402
    build_eighth_slice_human_accepted_grade_repair_artifact,
)
from tools.deck.build_eighth_slice_lock_legion_decision_artifact import (  # noqa: E402
    build_eighth_slice_lock_legion_decision_artifact,
    write_json,
    write_markdown,
)


LOCK_OPTION = "main_deck_only_review_no_runtime_promotion"
LEGION_OPTION = "main_deck_only_review_no_runtime_promotion"


class TestEighthSliceLockLegionDecisionArtifact(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        accepted_fixture.TestEighthSliceHumanAcceptedGradeRepairArtifact.setUpClass()
        cls.selected_artifact = accepted_fixture.TestEighthSliceHumanAcceptedGradeRepairArtifact.selected_artifact
        cls.drafts = accepted_fixture.TestEighthSliceHumanAcceptedGradeRepairArtifact.drafts
        cls.accepted_artifact = build_eighth_slice_human_accepted_grade_repair_artifact(
            cls.selected_artifact,
            cls.drafts,
            decision_text=accepted_fixture.DECISION_TEXT,
            grade_decision="accepted",
            decided_by="unit-test",
            decided_at="2026-07-02",
        )
        cls.report = build_eighth_slice_lock_legion_decision_artifact(
            cls.accepted_artifact,
            selected_lock_option=LOCK_OPTION,
            selected_legion_option=LEGION_OPTION,
        )

    def test_main_deck_decisions_open_validation_but_not_runtime(self):
        summary = self.report["summary"]
        decision = self.report["decision"]

        self.assertEqual("M65-04", self.report["version"])
        self.assertEqual(accepted_fixture.SELECTED_REVIEW_ITEM_ID, summary["selected_review_item_id"])
        self.assertEqual("m64_recipe_001", summary["selected_recipe_id"])
        self.assertEqual("m64_recipe_001_accepted_grade_pkg_001", summary["accepted_grade_repair_package_id"])
        self.assertEqual("m64_recipe_001_lock_deferred_pkg_001", summary["selected_lock_package_id"])
        self.assertEqual("m64_recipe_001_legion_deferred_pkg_001", summary["selected_legion_package_id"])
        self.assertEqual(LOCK_OPTION, summary["selected_lock_option_id"])
        self.assertEqual(LEGION_OPTION, summary["selected_legion_option_id"])
        self.assertEqual("selected_for_main_deck_only_validation", decision["lock"]["decision_status"])
        self.assertEqual("selected_for_main_deck_only_validation", decision["legion"]["decision_status"])
        self.assertEqual("continue_to_m65_05_repaired_validation_rerun", decision["recommendation"])
        self.assertTrue(summary["main_deck_only_validation_allowed"])
        self.assertFalse(summary["lock_runtime_enabled"])
        self.assertFalse(summary["unlock_runtime_enabled"])
        self.assertFalse(summary["legion_runtime_enabled"])
        self.assertFalse(summary["mate_identity_check_enabled"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertTrue(summary["human_selection_recorded"])
        self.assertTrue(summary["human_acceptance_recorded"])
        self.assertTrue(summary["grade_repair_accepted"])
        self.assertTrue(summary["lock_decision_recorded"])
        self.assertTrue(summary["legion_decision_recorded"])
        self.assertTrue(summary["accepted_artifact_ready_for_m65_04"])
        self.assertTrue(summary["ready_for_m65_05"])

    def test_scope_records_boundary_only(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_boundary_decision"])
        self.assertFalse(scope["records_human_selection"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["records_grade_repair_acceptance"])
        self.assertTrue(scope["records_lock_decision"])
        self.assertTrue(scope["records_legion_decision"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertFalse(scope["declares_recipe_valid"])

    def test_boundary_policy_keeps_runtime_disabled(self):
        policy = self.report["boundary_policy"]
        validation = self.report["m65_05_validation_policy"]
        item = self.report["decision_item"]

        self.assertTrue(policy["main_deck_only_validation_allowed"])
        self.assertTrue(policy["boundary_resolves_lock_deferred_for_main_deck_validation"])
        self.assertTrue(policy["boundary_resolves_legion_deferred_for_main_deck_validation"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertFalse(policy["lock_runtime_enabled"])
        self.assertFalse(policy["unlock_runtime_enabled"])
        self.assertFalse(policy["locked_card_visibility_enabled"])
        self.assertFalse(policy["lock_circle_state_enabled"])
        self.assertFalse(policy["unlock_timing_runtime_enabled"])
        self.assertFalse(policy["legion_runtime_enabled"])
        self.assertFalse(policy["mate_identity_check_enabled"])
        self.assertFalse(policy["legion_state_enabled"])
        self.assertFalse(policy["legion_attack_timing_enabled"])
        self.assertFalse(policy["legion_deck_building_validation_enabled"])
        self.assertTrue(policy["m65_05_must_rerun_main_deck_validation"])
        self.assertTrue(policy["runtime_gate_must_remain_blocked_until_validation"])
        self.assertEqual("main_deck_only", validation["validation_scope"])
        self.assertEqual(
            ["lock_runtime_support_deferred", "legion_runtime_support_deferred"],
            validation["may_suppress_issue_codes_after_boundary_decision"],
        )
        self.assertIn("grade_profile_outside_target", validation["must_still_enforce_issue_codes"])
        self.assertTrue(validation["must_not_enable_lock_or_unlock_effect_resolution"])
        self.assertTrue(validation["must_not_enable_legion_or_mate_effect_resolution"])
        self.assertTrue(validation["must_not_create_runtime_fixture_before_validation"])
        self.assertEqual("m65_04_m64_recipe_001_lock_legion_boundary_decision", item["decision_item_id"])
        self.assertFalse(item["lock_runtime_enabled"])
        self.assertFalse(item["unlock_runtime_enabled"])
        self.assertFalse(item["legion_runtime_enabled"])
        self.assertFalse(item["mate_identity_check_enabled"])
        self.assertGreaterEqual(len(item["lock_future_system_work"]), 1)
        self.assertGreaterEqual(len(item["legion_future_system_work"]), 1)

    def test_defer_lock_option_keeps_recipe_advisory_and_blocks_validation(self):
        report = build_eighth_slice_lock_legion_decision_artifact(
            self.accepted_artifact,
            selected_lock_option="defer_until_lock_unlock_runtime_exists",
            selected_legion_option=LEGION_OPTION,
        )

        self.assertEqual("defer_until_lock_unlock_runtime_exists", report["summary"]["selected_lock_option_id"])
        self.assertFalse(report["summary"]["main_deck_only_validation_allowed"])
        self.assertFalse(report["summary"]["ready_for_m65_05"])
        self.assertEqual("advisory_only", report["m65_05_validation_policy"]["validation_scope"])
        self.assertEqual([], report["m65_05_validation_policy"]["may_suppress_issue_codes_after_boundary_decision"])
        self.assertEqual("M65-closeout", report["next_target"]["milestone"])

    def test_defer_legion_option_keeps_recipe_advisory_and_blocks_validation(self):
        report = build_eighth_slice_lock_legion_decision_artifact(
            self.accepted_artifact,
            selected_lock_option=LOCK_OPTION,
            selected_legion_option="defer_until_legion_mate_runtime_exists",
        )

        self.assertEqual("defer_until_legion_mate_runtime_exists", report["summary"]["selected_legion_option_id"])
        self.assertFalse(report["summary"]["main_deck_only_validation_allowed"])
        self.assertFalse(report["summary"]["ready_for_m65_05"])
        self.assertEqual("advisory_only", report["m65_05_validation_policy"]["validation_scope"])
        self.assertEqual([], report["m65_05_validation_policy"]["may_suppress_issue_codes_after_boundary_decision"])
        self.assertEqual("M65-closeout", report["next_target"]["milestone"])

    def test_unsupported_options_are_rejected(self):
        with self.assertRaises(ValueError):
            build_eighth_slice_lock_legion_decision_artifact(
                self.accepted_artifact,
                selected_lock_option="enable_lock_runtime_now",
                selected_legion_option=LEGION_OPTION,
            )

        with self.assertRaises(ValueError):
            build_eighth_slice_lock_legion_decision_artifact(
                self.accepted_artifact,
                selected_lock_option=LOCK_OPTION,
                selected_legion_option="enable_legion_runtime_now",
            )

    def test_options_missing_from_accepted_artifact_are_rejected(self):
        accepted = copy.deepcopy(self.accepted_artifact)
        accepted["accepted_repair"]["lock_decision_options"] = []

        with self.assertRaises(ValueError):
            build_eighth_slice_lock_legion_decision_artifact(
                accepted,
                selected_lock_option=LOCK_OPTION,
                selected_legion_option=LEGION_OPTION,
            )

        accepted = copy.deepcopy(self.accepted_artifact)
        accepted["accepted_repair"]["legion_decision_options"] = []

        with self.assertRaises(ValueError):
            build_eighth_slice_lock_legion_decision_artifact(
                accepted,
                selected_lock_option=LOCK_OPTION,
                selected_legion_option=LEGION_OPTION,
            )

    def test_not_ready_accepted_artifact_blocks_validation_readiness(self):
        accepted = copy.deepcopy(self.accepted_artifact)
        accepted["summary"]["ready_for_m65_04"] = False

        report = build_eighth_slice_lock_legion_decision_artifact(
            accepted,
            selected_lock_option=LOCK_OPTION,
            selected_legion_option=LEGION_OPTION,
        )

        self.assertFalse(report["summary"]["accepted_artifact_ready_for_m65_04"])
        self.assertFalse(report["summary"]["ready_for_m65_05"])
        self.assertFalse(report["summary"]["runtime_promotion_allowed"])

    def test_next_target_is_m65_05_for_main_deck_decisions(self):
        next_target = self.report["next_target"]

        self.assertEqual("M65-05", next_target["milestone"])
        self.assertEqual("Eighth-slice repaired recipe validation rerun", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m65_04.json"
            md_path = out / "m65_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M65-04", loaded["version"])
            self.assertEqual(LOCK_OPTION, loaded["summary"]["selected_lock_option_id"])
            self.assertEqual(LEGION_OPTION, loaded["summary"]["selected_legion_option_id"])
            self.assertIn(
                "M65-04 Eighth-Slice Lock / Legion Decision Artifact",
                md_path.read_text(encoding="utf-8"),
            )


if __name__ == "__main__":
    unittest.main()
