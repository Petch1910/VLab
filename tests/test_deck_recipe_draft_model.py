"""Tests for tools/deck/build_deck_recipe_draft_model.py (M36-02)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_deck_recipe_draft_model import (  # noqa: E402
    D2_SKELETONS,
    D3_COMBO_LINES,
    M36_REVIEW_PACKET,
    build_recipe_drafts,
    load_json,
    write_json,
    write_markdown,
)


class TestDeckRecipeDraftModel(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.d2 = load_json(D2_SKELETONS)
        cls.d3 = load_json(D3_COMBO_LINES)
        cls.packet = load_json(M36_REVIEW_PACKET)
        cls.report = build_recipe_drafts(cls.d2, cls.d3, cls.packet)

    def test_recipe_drafts_are_ready_for_validator(self):
        summary = self.report["summary"]

        self.assertEqual("M36-02", self.report["version"])
        self.assertTrue(summary["ready_for_m36_03"])
        self.assertEqual(25, summary["source_skeleton_count"])
        self.assertEqual(25, summary["recipe_draft_count"])
        self.assertEqual(1, summary["accepted_seed_recipe_count"])
        self.assertEqual(24, summary["rejected_line_recipe_count"])

    def test_scope_is_offline_draft_only(self):
        scope = self.report["scope"]
        policy = self.report["draft_policy"]

        self.assertTrue(scope["offline_recipe_draft_model"])
        self.assertFalse(scope["runtime_deck"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["deck_validator_result"])
        self.assertFalse(scope["automatic_deck_injection"])
        self.assertTrue(policy["card_quantities_are_advisory"])
        self.assertTrue(policy["slot_gaps_are_allowed_until_m36_03"])
        self.assertTrue(policy["no_live_card_text_parsing"])
        self.assertTrue(policy["no_direct_GameState_mutation"])

    def test_each_recipe_has_bounded_quantities_and_slot_metadata(self):
        for recipe in self.report["recipe_drafts"]:
            self.assertTrue(recipe["quantities"])
            for row in recipe["quantities"]:
                self.assertGreater(row["quantity"], 0)
                self.assertLessEqual(row["quantity"], 4)
                self.assertTrue(row["roles"])
            slots = recipe["slot_summary"]
            self.assertEqual(50, slots["main_deck_target"])
            self.assertEqual(
                slots["explicit_card_count"],
                slots["trigger_slots_assigned"] + slots["normal_slots_assigned"],
            )
            self.assertEqual(
                50,
                slots["explicit_card_count"] + slots["total_unfilled_slots"],
            )

    def test_accepted_seed_recipe_is_preserved_but_not_runtime_ready(self):
        accepted = [
            item
            for item in self.report["recipe_drafts"]
            if item["review_status"].startswith("accepted_seed")
        ]

        self.assertEqual(1, len(accepted))
        recipe = accepted[0]
        self.assertEqual("line_003", recipe["source_line_id"])
        self.assertEqual("skel_003", recipe["source_skeleton_id"])
        self.assertIn("human_acceptance", recipe["review_blockers"])
        self.assertTrue(recipe["validation_metadata"]["requires_m36_03_validator"])
        self.assertTrue(recipe["validation_metadata"]["not_runtime_deck"])

    def test_rejected_lines_remain_blocked(self):
        rejected = [
            item
            for item in self.report["recipe_drafts"]
            if item["review_status"].startswith("blocked_rejected_line")
        ]

        self.assertEqual(24, len(rejected))
        for recipe in rejected:
            self.assertIn("support_gap_resolved", recipe["review_blockers"])

    def test_next_target_is_m36_03(self):
        next_target = self.report["next_target"]

        self.assertEqual("M36-03", next_target["milestone"])
        self.assertEqual("Deck recipe validator", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "recipes.json"
            md_path = out / "recipes.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M36-02", loaded["version"])
            self.assertIn("M36-02 Deck Recipe Draft Model", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
