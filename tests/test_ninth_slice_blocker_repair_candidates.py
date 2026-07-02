"""Tests for tools/deck/build_ninth_slice_blocker_repair_candidates.py (M68-06)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import tests.test_ninth_slice_recipe_draft_model as ninth_draft_fixture  # noqa: E402
from tools.deck.build_ninth_slice_blocker_repair_candidates import (  # noqa: E402
    build_ninth_slice_blocker_repair_candidates,
    write_json,
    write_markdown,
)
from tools.deck.check_ninth_slice_combo_recipe_consistency import (  # noqa: E402
    build_ninth_slice_consistency_report,
)
from tools.deck.validate_ninth_slice_recipe_drafts import (  # noqa: E402
    _all_card_ids,
    build_ninth_slice_validation_report,
    load_card_rows,
)


class TestNinthSliceBlockerRepairCandidates(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        ninth_draft_fixture.TestNinthSliceRecipeDraftModel.setUpClass()
        cls.recipes = ninth_draft_fixture.TestNinthSliceRecipeDraftModel.report
        cls.scaffold = ninth_draft_fixture.TestNinthSliceRecipeDraftModel.scaffold
        cls.card_rows = load_card_rows(_all_card_ids(cls.recipes))
        cls.validation = build_ninth_slice_validation_report(cls.recipes, cls.card_rows)
        cls.consistency = build_ninth_slice_consistency_report(cls.recipes, cls.validation)
        cls.report = build_ninth_slice_blocker_repair_candidates(
            cls.recipes,
            cls.validation,
            cls.consistency,
            cls.scaffold,
        )

    def test_repair_candidates_are_ready_for_closeout(self):
        summary = self.report["summary"]

        self.assertEqual("M68-06", self.report["version"])
        self.assertTrue(summary["ready_for_m68_closeout"])
        self.assertEqual(25, summary["recipe_count"])
        self.assertEqual(25, summary["manual_overlap_recipe_count"])
        self.assertEqual(25, summary["manual_repair_complete_count"])
        self.assertEqual(23, summary["grade_profile_repair_candidate_count"])
        self.assertEqual(23, summary["grade_profile_complete_candidate_count"])
        self.assertEqual(1, summary["grade_package_clears_manual_overlap_count"])
        self.assertEqual(25, summary["g_zone_deferred_recipe_count"])
        self.assertEqual(25, summary["stride_deferred_recipe_count"])
        self.assertEqual(25, summary["aqua_force_battle_order_deferred_recipe_count"])
        self.assertEqual(0, summary["unexpected_structural_blocker_recipe_count"])
        self.assertEqual(25, summary["ready_for_human_repair_review_count"])
        self.assertFalse(summary["runtime_promotion_allowed"])

    def test_scope_is_offline_repair_only(self):
        scope = self.report["scope"]
        policy = self.report["repair_policy"]

        self.assertTrue(scope["offline_repair_candidates"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["runtime_deck"])
        self.assertFalse(scope["g_zone_runtime"])
        self.assertFalse(scope["stride_runtime"])
        self.assertFalse(scope["aqua_force_battle_order_runtime"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertTrue(policy["source_backed_candidates"])
        self.assertTrue(policy["substitution_only"])
        self.assertTrue(policy["manual_review_cards_are_not_resolved_here"])
        self.assertTrue(policy["g_zone_support_is_deferred_system_work"])
        self.assertTrue(policy["stride_support_is_deferred_system_work"])
        self.assertTrue(policy["aqua_force_battle_order_support_is_deferred_system_work"])
        self.assertIn("G-BT02", policy["series_scope_from_m68_01_fixture_scaffold"])
        self.assertFalse(policy["runtime_promotion_allowed"])

    def test_each_repair_item_has_manual_g_zone_stride_and_battle_order_context(self):
        for item in self.report["repair_items"]:
            manual = item["manual_overlap_repair_package"]
            g_zone = item["g_zone_deferred_package"]
            stride = item["stride_deferred_package"]
            battle_order = item["aqua_force_battle_order_deferred_package"]

            self.assertEqual("blocked_by_manual_review", item["validation_status"])
            self.assertEqual("blocked_by_manual_review", item["consistency_status"])
            self.assertTrue(item["manual_review_card_ids"])
            self.assertEqual([], item["structural_blockers"])
            self.assertTrue(manual["complete_candidate"])
            self.assertIsNotNone(g_zone)
            self.assertIsNotNone(stride)
            self.assertIsNotNone(battle_order)
            self.assertFalse(g_zone["can_be_repaired_in_m68_06"])
            self.assertFalse(stride["can_be_repaired_in_m68_06"])
            self.assertFalse(battle_order["can_be_repaired_in_m68_06"])
            self.assertTrue(item["blocked_by_g_zone_deferred"])
            self.assertTrue(item["blocked_by_stride_deferred"])
            self.assertTrue(item["blocked_by_aqua_force_battle_order_deferred"])
            self.assertTrue(item["ready_for_human_repair_review"])
            self.assertFalse(item["runtime_promotion_allowed"])

    def test_manual_packages_avoid_all_manual_review_cards_as_replacements(self):
        global_manual_ids = {
            card_id
            for item in self.report["repair_items"]
            for card_id in item["manual_review_card_ids"]
        }

        for item in self.report["repair_items"]:
            manual = item["manual_overlap_repair_package"]

            self.assertEqual("manual_review_overlap_substitution_preview", manual["repair_type"])
            self.assertTrue(manual["advisory_only"])
            self.assertTrue(manual["manual_review_may_accept_original_card"])
            self.assertFalse(manual["runtime_promotion_allowed"])
            for substitution in manual["substitutions"]:
                selected = substitution["selected_replacement"]
                self.assertIsNotNone(selected)
                self.assertNotIn(selected["card_id"], global_manual_ids)
                self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", selected["source"])

    def test_grade_packages_are_source_backed_when_review_exists(self):
        target = {"0": 17, "1": 14, "2": 11, "3": 8}
        packages_with_grade_review = 0

        for item in self.report["repair_items"]:
            grade = item["grade_profile_repair_package"]

            self.assertEqual("grade_profile_substitution_preview", grade["repair_type"])
            self.assertTrue(grade["advisory_only"])
            self.assertFalse(grade["runtime_promotion_allowed"])
            if grade.get("reason") == "no_grade_profile_issue":
                self.assertFalse(grade["complete_candidate"])
                continue
            packages_with_grade_review += 1
            self.assertEqual(target, grade["target_grade_counts"])
            self.assertEqual(target, grade["grade_counts_after"])
            self.assertEqual(grade["added_card_count"], grade["removed_card_count"])
            self.assertTrue(grade["complete_candidate"])
            for addition in grade["additions"]:
                self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", addition["source"])

        self.assertEqual(23, packages_with_grade_review)

    def test_deferred_system_packages_never_promote_runtime(self):
        for item in self.report["repair_items"]:
            g_zone = item["g_zone_deferred_package"]
            stride = item["stride_deferred_package"]
            battle_order = item["aqua_force_battle_order_deferred_package"]

            self.assertEqual("g_zone_support_deferred_decision", g_zone["repair_type"])
            self.assertEqual("stride_support_deferred_decision", stride["repair_type"])
            self.assertEqual("aqua_force_battle_order_deferred_decision", battle_order["repair_type"])
            self.assertTrue(g_zone["advisory_only"])
            self.assertTrue(stride["advisory_only"])
            self.assertTrue(battle_order["advisory_only"])
            self.assertTrue(g_zone["main_deck_validation_only"])
            self.assertTrue(stride["main_deck_validation_only"])
            self.assertTrue(battle_order["manual_semantic_review_only"])
            self.assertIn("G Zone deck slot model", g_zone["requires_future_system_work"])
            self.assertIn("Stride declaration timing", stride["requires_future_system_work"])
            self.assertIn("battle-count tracker", battle_order["requires_future_system_work"])
            self.assertFalse(g_zone["runtime_promotion_allowed"])
            self.assertFalse(stride["runtime_promotion_allowed"])
            self.assertFalse(battle_order["runtime_promotion_allowed"])

    def test_next_target_is_m68_closeout(self):
        next_target = self.report["next_target"]

        self.assertEqual("M68-closeout", next_target["milestone"])
        self.assertEqual("Ninth-slice runtime readiness decision", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m68_06.json"
            md_path = out / "m68_06.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M68-06", loaded["version"])
            self.assertIn("M68-06 Ninth-Slice Blocker Repair Candidates", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
