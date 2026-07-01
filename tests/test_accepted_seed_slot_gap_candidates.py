"""Tests for tools/deck/build_accepted_seed_slot_gap_candidates.py (M37-01)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_accepted_seed_slot_gap_candidates import (  # noqa: E402
    M36_CLOSEOUT,
    RECIPE_DRAFTS,
    VALIDATION_REPORT,
    build_slot_gap_candidates,
    load_json,
    write_json,
    write_markdown,
)


class TestAcceptedSeedSlotGapCandidates(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = build_slot_gap_candidates(
            load_json(RECIPE_DRAFTS),
            load_json(VALIDATION_REPORT),
            load_json(M36_CLOSEOUT),
        )

    def test_report_targets_the_single_accepted_seed_recipe(self):
        seed = self.report["accepted_seed"]
        summary = self.report["summary"]

        self.assertEqual("M37-01", self.report["version"])
        self.assertEqual("recipe_003", seed["recipe_id"])
        self.assertEqual("skel_003", seed["source_skeleton_id"])
        self.assertEqual("line_003", seed["source_line_id"])
        self.assertEqual("BT04-078TH", seed["anchor_card_id"])
        self.assertEqual("recipe_003", summary["accepted_seed_recipe_id"])

    def test_gap_summary_focuses_on_trigger_slots(self):
        summary = self.report["summary"]
        seed = self.report["accepted_seed"]

        self.assertEqual(12, summary["trigger_slots_unfilled"])
        self.assertEqual(0, summary["normal_slots_unfilled"])
        self.assertEqual({"Stand": 4}, seed["current_trigger_counts"])
        self.assertEqual(38, seed["slot_summary"]["explicit_card_count"])

    def test_candidate_cards_are_source_backed_and_available(self):
        candidates = self.report["candidate_cards"]

        self.assertEqual(18, len(candidates))
        candidate_ids = {candidate["card_id"] for candidate in candidates}
        self.assertNotIn("BT04-078TH", candidate_ids)
        for candidate in candidates:
            self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", candidate["source"])
            self.assertGreater(candidate["available_quantity"], 0)
            self.assertEqual("Trigger Unit", candidate["type_1"])
            self.assertIn(candidate["trigger"], {"Critical", "Draw", "Heal", "Stand"})

    def test_completion_packages_fill_the_trigger_gap_without_promotion(self):
        packages = self.report["completion_packages"]

        self.assertEqual(5, len(packages))
        for package in packages:
            self.assertTrue(package["advisory_only"])
            self.assertFalse(package["runtime_promotion_allowed"])
            self.assertEqual("complete_candidate", package["completion_status"])
            self.assertEqual(12, package["added_trigger_count"])
            self.assertEqual(16, sum(package["final_trigger_counts"].values()))
            self.assertTrue(package["heal_limit_ok"])
            self.assertEqual([], package["copy_limit_violations"])

    def test_balanced_classic_package_is_first_and_uses_three_trigger_types(self):
        package = self.report["completion_packages"][0]
        additions = {(item["card_id"], item["trigger"], item["quantity"]) for item in package["additions"]}

        self.assertEqual("balanced_classic", package["profile_id"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, package["final_trigger_counts"])
        self.assertIn(("BT04-077TH", "Critical", 4), additions)
        self.assertIn(("BT02-073TH", "Draw", 4), additions)
        self.assertIn(("BT01-065TH", "Heal", 4), additions)

    def test_eb04_local_package_is_available_for_review(self):
        packages_by_profile = {package["profile_id"]: package for package in self.report["completion_packages"]}
        package = packages_by_profile["eb04_local_balanced"]
        addition_ids = {item["card_id"] for item in package["additions"]}

        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, package["final_trigger_counts"])
        self.assertEqual({"EB04-029TH", "EB04-030TH", "EB04-032TH"}, addition_ids)

    def test_scope_and_policy_do_not_mutate_runtime(self):
        scope = self.report["scope"]
        policy = self.report["candidate_policy"]

        self.assertTrue(scope["offline_candidate_generation"])
        self.assertFalse(scope["changes_recipe_draft"])
        self.assertFalse(scope["creates_runtime_deck"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_deck_injection"])
        self.assertFalse(scope["live_card_text_parsing"])
        self.assertTrue(policy["human_review_required"])
        self.assertFalse(policy["runtime_promotion_allowed"])

    def test_blocker_context_carries_m36_decision(self):
        context = self.report["blocker_context"]

        self.assertEqual(0, context["m36_runtime_ready_recipe_count"])
        self.assertEqual(0, context["m36_promotable_combo_lines"])
        self.assertEqual(
            "repair_first_slice_recipe_blockers_before_runtime_or_broader_scaleout",
            context["reason_for_m37"],
        )

    def test_next_target_is_m37_02(self):
        summary = self.report["summary"]
        next_target = self.report["next_target"]

        self.assertTrue(summary["ready_for_m37_02"])
        self.assertEqual("M37-02", next_target["milestone"])
        self.assertEqual("Trigger package repair proposal", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m37_01.json"
            md_path = out / "m37_01.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M37-01", loaded["version"])
            self.assertIn("M37-01 Accepted Seed Slot-Gap", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
