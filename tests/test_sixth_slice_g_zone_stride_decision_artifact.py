"""Tests for tools/deck/build_sixth_slice_g_zone_stride_decision_artifact.py (M57-04)."""

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
    write_json,
    write_markdown,
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


SELECTED_REVIEW_ITEM_ID = "m57_01_m56_recipe_001_repair_review"


class TestSixthSliceGZoneStrideDecisionArtifact(unittest.TestCase):
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
        cls.report = build_sixth_slice_g_zone_stride_decision_artifact(
            cls.accepted_artifact,
            selected_option="main_deck_only_review_no_runtime_promotion",
        )

    def test_main_deck_only_decision_opens_validation_but_not_runtime(self):
        summary = self.report["summary"]
        decision = self.report["decision"]

        self.assertEqual("M57-04", self.report["version"])
        self.assertEqual(SELECTED_REVIEW_ITEM_ID, summary["accepted_review_item_id"])
        self.assertEqual("m56_recipe_001", summary["accepted_recipe_id"])
        self.assertEqual("m56_recipe_001_g_zone_deferred_pkg_001", summary["accepted_g_zone_package_id"])
        self.assertEqual("main_deck_only_review_no_runtime_promotion", summary["selected_option_id"])
        self.assertEqual("selected_for_main_deck_only_validation", decision["decision_status"])
        self.assertTrue(summary["main_deck_only_validation_allowed"])
        self.assertFalse(summary["g_zone_runtime_enabled"])
        self.assertFalse(summary["stride_runtime_enabled"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertTrue(summary["human_selection_recorded"])
        self.assertTrue(summary["human_acceptance_recorded"])
        self.assertTrue(summary["g_zone_decision_recorded"])
        self.assertTrue(summary["accepted_artifact_ready_for_m57_04"])
        self.assertTrue(summary["ready_for_m57_05"])

    def test_scope_records_boundary_only(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_boundary_decision"])
        self.assertFalse(scope["records_human_selection"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertTrue(scope["records_g_zone_decision"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertFalse(scope["declares_recipe_valid"])

    def test_boundary_policy_keeps_g_zone_and_stride_disabled(self):
        policy = self.report["boundary_policy"]
        validation = self.report["m57_05_validation_policy"]
        item = self.report["decision_item"]

        self.assertTrue(policy["main_deck_only_validation_allowed"])
        self.assertTrue(policy["boundary_resolves_g_zone_deferred_for_main_deck_validation"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertFalse(policy["g_zone_runtime_enabled"])
        self.assertFalse(policy["stride_runtime_enabled"])
        self.assertFalse(policy["g_zone_slot_model_enabled"])
        self.assertFalse(policy["stride_deck_building_validation_enabled"])
        self.assertFalse(policy["generation_break_runtime_enabled"])
        self.assertFalse(policy["grade4_main_deck_allowed"])
        self.assertFalse(policy["g_units_allowed_in_main_deck"])
        self.assertFalse(policy["g_zone_cards_allowed_in_current_windows_fixture"])
        self.assertTrue(policy["m57_05_must_rerun_main_deck_validation"])
        self.assertTrue(policy["runtime_gate_must_remain_blocked_until_validation"])
        self.assertEqual("main_deck_only", validation["validation_scope"])
        self.assertEqual(["g_zone_support_deferred"], validation["may_suppress_issue_codes_after_boundary_decision"])
        self.assertIn("manual_review_overlap", validation["must_still_enforce_issue_codes"])
        self.assertTrue(validation["must_keep_grade4_cards_out_of_main_deck"])
        self.assertTrue(validation["must_keep_g_units_out_of_runtime_fixture"])
        self.assertEqual("m57_04_m56_recipe_001_g_zone_stride_decision", item["decision_item_id"])
        self.assertFalse(item["g_zone_runtime_enabled"])
        self.assertFalse(item["stride_runtime_enabled"])
        self.assertGreaterEqual(len(item["g_zone_future_system_work"]), 1)

    def test_defer_option_keeps_recipe_advisory_and_blocks_validation(self):
        report = build_sixth_slice_g_zone_stride_decision_artifact(
            self.accepted_artifact,
            selected_option="defer_until_g_zone_runtime_exists",
        )

        self.assertEqual("defer_until_g_zone_runtime_exists", report["summary"]["selected_option_id"])
        self.assertFalse(report["summary"]["main_deck_only_validation_allowed"])
        self.assertFalse(report["summary"]["ready_for_m57_05"])
        self.assertEqual("advisory_only", report["m57_05_validation_policy"]["validation_scope"])
        self.assertEqual([], report["m57_05_validation_policy"]["may_suppress_issue_codes_after_boundary_decision"])
        self.assertEqual("M57-closeout", report["next_target"]["milestone"])

    def test_unsupported_option_is_rejected(self):
        with self.assertRaises(ValueError):
            build_sixth_slice_g_zone_stride_decision_artifact(
                self.accepted_artifact,
                selected_option="enable_g_zone_runtime_now",
            )

    def test_option_missing_from_accepted_artifact_is_rejected(self):
        accepted = copy.deepcopy(self.accepted_artifact)
        accepted["accepted_repair"]["g_zone_decision_options"] = []

        with self.assertRaises(ValueError):
            build_sixth_slice_g_zone_stride_decision_artifact(
                accepted,
                selected_option="main_deck_only_review_no_runtime_promotion",
            )

    def test_not_ready_accepted_artifact_blocks_validation_readiness(self):
        accepted = copy.deepcopy(self.accepted_artifact)
        accepted["summary"]["ready_for_m57_04"] = False

        report = build_sixth_slice_g_zone_stride_decision_artifact(
            accepted,
            selected_option="main_deck_only_review_no_runtime_promotion",
        )

        self.assertFalse(report["summary"]["accepted_artifact_ready_for_m57_04"])
        self.assertFalse(report["summary"]["ready_for_m57_05"])
        self.assertFalse(report["summary"]["runtime_promotion_allowed"])

    def test_next_target_is_m57_05_for_main_deck_only_decision(self):
        next_target = self.report["next_target"]

        self.assertEqual("M57-05", next_target["milestone"])
        self.assertEqual("Sixth-slice repaired recipe validation rerun", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m57_04.json"
            md_path = out / "m57_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M57-04", loaded["version"])
            self.assertEqual("main_deck_only_review_no_runtime_promotion", loaded["summary"]["selected_option_id"])
            self.assertIn("M57-04 Sixth-Slice G Zone / Stride Decision Artifact", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
