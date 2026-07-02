"""Tests for tools/deck/build_ninth_slice_human_selected_recipe_artifact.py (M69-02)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import tests.test_ninth_slice_human_repair_review_packet as review_fixture  # noqa: E402
from tools.deck.build_ninth_slice_human_selected_recipe_artifact import (  # noqa: E402
    build_ninth_slice_human_selected_recipe_artifact,
    write_json,
    write_markdown,
)


SELECTED_REVIEW_ITEM_ID = "m69_01_m68_recipe_001_repair_review"
SELECTION_TEXT = "explicit test selection for m68_recipe_001"


class TestNinthSliceHumanSelectedRecipeArtifact(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        review_fixture.TestNinthSliceHumanRepairReviewPacket.setUpClass()
        cls.review_packet = review_fixture.TestNinthSliceHumanRepairReviewPacket.report
        cls.report = build_ninth_slice_human_selected_recipe_artifact(
            cls.review_packet,
            selected_review_item_id=SELECTED_REVIEW_ITEM_ID,
            selection_text=SELECTION_TEXT,
            selected_by="unit-test",
            selected_at="2026-07-02",
        )

    def test_explicit_selection_records_exactly_one_recipe(self):
        summary = self.report["summary"]

        self.assertEqual("M69-02", self.report["version"])
        self.assertEqual(25, summary["available_review_item_count"])
        self.assertEqual(1, summary["selected_review_item_count"])
        self.assertEqual("m68_recipe_001", summary["selected_recipe_id"])
        self.assertEqual("m68_recipe_001_manual_overlap_pkg_001", summary["selected_manual_overlap_package_id"])
        self.assertEqual("m68_recipe_001_grade_profile_pkg_001", summary["selected_grade_profile_package_id"])
        self.assertEqual("m68_recipe_001_g_zone_deferred_pkg_001", summary["selected_g_zone_package_id"])
        self.assertEqual("m68_recipe_001_stride_deferred_pkg_001", summary["selected_stride_package_id"])
        self.assertEqual(
            "m68_recipe_001_aqua_force_battle_order_pkg_001",
            summary["selected_aqua_force_battle_order_package_id"],
        )
        self.assertTrue(summary["records_human_selection"])
        self.assertFalse(summary["records_human_acceptance"])
        self.assertFalse(summary["records_g_zone_decision"])
        self.assertFalse(summary["records_stride_decision"])
        self.assertFalse(summary["records_aqua_force_battle_order_decision"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertTrue(summary["g_zone_deferred"])
        self.assertTrue(summary["stride_deferred"])
        self.assertTrue(summary["aqua_force_battle_order_deferred"])
        self.assertTrue(summary["ready_for_m69_03"])

    def test_selection_carries_pair_manual_grade_g_zone_stride_and_aqua_context(self):
        selection = self.report["selection"]
        pair = selection["pair"]

        self.assertEqual(SELECTED_REVIEW_ITEM_ID, selection["selected_review_item_id"])
        self.assertEqual("m68_recipe_001", selection["recipe_id"])
        self.assertEqual("2026-07-02", selection["selected_at"])
        self.assertEqual("unit-test", selection["selected_by"])
        self.assertEqual(SELECTION_TEXT, selection["selection_text"])
        self.assertEqual("select_recipe_for_m69_03_acceptance_review", selection["selected_action"])
        self.assertEqual("G-BT02-070TH->G-CB02-015TH", selection["source_candidate_edge"])
        self.assertEqual(1, selection["source_edge_rank"])
        self.assertEqual("G-BT02-070TH", pair["source_card_id"])
        self.assertEqual("G-CB02-015TH", pair["target_card_id"])
        self.assertEqual(14, pair["net_score"])
        self.assertTrue(selection["manual_review_card_ids"])
        self.assertTrue(selection["manual_substitutions"])
        self.assertTrue(selection["manual_repair_complete"])
        self.assertTrue(selection["manual_review_may_accept_original_card"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, selection["grade_counts_after"])
        self.assertTrue(selection["grade_additions"])
        self.assertTrue(selection["grade_removals"])
        self.assertTrue(selection["grade_repair_complete"])
        self.assertFalse(selection["grade_repair_not_needed"])
        self.assertTrue(selection["g_zone_deferred"])
        self.assertTrue(selection["stride_deferred"])
        self.assertTrue(selection["aqua_force_battle_order_deferred"])
        self.assertFalse(selection["g_zone_can_be_repaired_in_m69_01"])
        self.assertFalse(selection["stride_can_be_repaired_in_m69_01"])
        self.assertFalse(selection["aqua_force_battle_order_can_be_repaired_in_m69_01"])
        self.assertIn("G Zone deck slot model", selection["g_zone_future_system_work"])
        self.assertIn("Stride declaration timing", selection["stride_future_system_work"])
        self.assertIn("battle-count tracker", selection["aqua_force_battle_order_future_system_work"])
        self.assertEqual([], selection["structural_blockers"])
        self.assertFalse(selection["runtime_promotion_allowed"])

    def test_grade_not_needed_item_can_be_selected_for_acceptance_review(self):
        grade_not_needed = next(
            item for item in self.review_packet["review_items"] if item["grade_repair_not_needed"]
        )

        report = build_ninth_slice_human_selected_recipe_artifact(
            self.review_packet,
            selected_review_item_id=grade_not_needed["review_item_id"],
            selection_text="explicit test selection for grade-not-needed item",
            selected_by="unit-test",
            selected_at="2026-07-02",
        )

        self.assertEqual("m68_recipe_013", report["selection"]["recipe_id"])
        self.assertTrue(report["selection"]["grade_repair_not_needed"])
        self.assertFalse(report["selection"]["grade_repair_complete"])
        self.assertTrue(report["summary"]["ready_for_m69_03"])

    def test_scope_and_policy_block_acceptance_system_decisions_and_runtime(self):
        scope = self.report["scope"]
        policy = self.report["selection_policy"]

        self.assertTrue(scope["offline_human_selected_artifact"])
        self.assertTrue(scope["records_human_selection"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["records_g_zone_decision"])
        self.assertFalse(scope["records_stride_decision"])
        self.assertFalse(scope["records_aqua_force_battle_order_decision"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["g_zone_runtime"])
        self.assertFalse(scope["stride_runtime"])
        self.assertFalse(scope["aqua_force_battle_order_runtime"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertTrue(policy["requires_explicit_review_item_id"])
        self.assertTrue(policy["requires_non_empty_selection_text"])
        self.assertTrue(policy["exactly_one_recipe_selected"])
        self.assertTrue(policy["selection_is_not_acceptance"])
        self.assertTrue(policy["selection_is_not_g_zone_decision"])
        self.assertTrue(policy["selection_is_not_stride_decision"])
        self.assertTrue(policy["selection_is_not_aqua_force_battle_order_decision"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertTrue(policy["m69_03_must_record_explicit_acceptance"])
        self.assertTrue(policy["m69_04_must_record_explicit_g_zone_stride_aqua_decision"])
        self.assertTrue(policy["m69_05_must_rerun_validation"])

    def test_blank_review_item_id_is_rejected(self):
        with self.assertRaises(ValueError) as context:
            build_ninth_slice_human_selected_recipe_artifact(
                self.review_packet,
                selected_review_item_id=" ",
                selection_text=SELECTION_TEXT,
            )

        self.assertIn("explicit non-empty review_item_id", str(context.exception))

    def test_invalid_review_item_id_is_rejected(self):
        with self.assertRaises(ValueError) as context:
            build_ninth_slice_human_selected_recipe_artifact(
                self.review_packet,
                selected_review_item_id="m69_01_missing_review",
                selection_text=SELECTION_TEXT,
            )

        self.assertIn("M69-01 review item not found", str(context.exception))

    def test_old_eighth_slice_review_item_id_is_rejected(self):
        with self.assertRaises(ValueError) as context:
            build_ninth_slice_human_selected_recipe_artifact(
                self.review_packet,
                selected_review_item_id="m65_01_m64_recipe_001_repair_review",
                selection_text=SELECTION_TEXT,
            )

        self.assertIn("M69-01 review item not found", str(context.exception))

    def test_blank_selection_text_is_rejected(self):
        with self.assertRaises(ValueError) as context:
            build_ninth_slice_human_selected_recipe_artifact(
                self.review_packet,
                selected_review_item_id=SELECTED_REVIEW_ITEM_ID,
                selection_text=" ",
            )

        self.assertIn("non-empty selection_text", str(context.exception))

    def test_not_ready_review_packet_blocks_m69_03_readiness(self):
        review_packet = json.loads(json.dumps(self.review_packet, ensure_ascii=False))
        review_packet["summary"]["ready_for_m69_02"] = False

        report = build_ninth_slice_human_selected_recipe_artifact(
            review_packet,
            selected_review_item_id=SELECTED_REVIEW_ITEM_ID,
            selection_text=SELECTION_TEXT,
        )

        self.assertFalse(report["summary"]["ready_for_m69_03"])

    def test_next_target_is_m69_03(self):
        next_target = self.report["next_target"]

        self.assertEqual("M69-03", next_target["milestone"])
        self.assertEqual("Ninth-slice human-accepted repair artifact", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m69_02.json"
            md_path = out / "m69_02.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M69-02", loaded["version"])
            self.assertEqual("m68_recipe_001", loaded["summary"]["selected_recipe_id"])
            self.assertIn("M69-02 Ninth-Slice Human-Selected Recipe Artifact", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
