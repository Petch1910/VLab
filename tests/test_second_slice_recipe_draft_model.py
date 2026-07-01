"""Tests for tools/deck/build_second_slice_recipe_draft_model.py (M40-02)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_second_slice_recipe_draft_model import (  # noqa: E402
    M40_REVIEW_PACKET,
    SECOND_SLICE_READINESS,
    build_second_slice_recipe_drafts,
    load_json,
    write_json,
    write_markdown,
)


class TestSecondSliceRecipeDraftModel(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.packet = load_json(M40_REVIEW_PACKET)
        cls.readiness = load_json(SECOND_SLICE_READINESS)
        cls.report = build_second_slice_recipe_drafts(cls.packet, cls.readiness)

    def test_recipe_drafts_are_ready_for_validator(self):
        summary = self.report["summary"]

        self.assertEqual("M40-02", self.report["version"])
        self.assertTrue(summary["ready_for_m40_03"])
        self.assertEqual(259, summary["candidate_edge_input_count"])
        self.assertEqual(25, summary["recipe_draft_count"])
        self.assertEqual(25, summary["quantity_complete_recipe_count"])
        self.assertEqual(14, summary["fixture_scaffold_card_count"])
        self.assertEqual(50, summary["fixture_scaffold_total_cards"])

    def test_scope_is_offline_draft_only(self):
        scope = self.report["scope"]
        policy = self.report["draft_policy"]

        self.assertTrue(scope["offline_recipe_draft_model"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["runtime_deck"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["automatic_deck_injection"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertTrue(policy["pair_anchored_from_m40_01_candidate_edges"])
        self.assertTrue(policy["card_quantities_are_advisory"])
        self.assertTrue(policy["no_live_card_text_parsing"])
        self.assertTrue(policy["no_direct_GameState_mutation"])

    def test_each_recipe_is_quantity_complete_and_bounded(self):
        for recipe in self.report["recipe_drafts"]:
            quantities = recipe["quantities"]
            slots = recipe["slot_summary"]

            self.assertTrue(quantities)
            self.assertEqual("draft_quantity_complete", recipe["recipe_status"])
            self.assertEqual(50, slots["main_deck_target"])
            self.assertEqual(50, slots["explicit_card_count"])
            self.assertEqual(0, slots["total_unfilled_slots"])
            self.assertEqual(0, slots["main_deck_delta"])
            self.assertEqual(16, slots["trigger_count"])
            for row in quantities:
                self.assertGreater(row["quantity"], 0)
                self.assertLessEqual(row["quantity"], 4)
                self.assertTrue(row["roles"])

    def test_candidate_pair_cards_are_in_each_recipe(self):
        for recipe in self.report["recipe_drafts"]:
            by_id = {row["card_id"]: row for row in recipe["quantities"]}
            pair = recipe["pair"]

            self.assertIn(pair["source_card_id"], by_id)
            self.assertIn(pair["target_card_id"], by_id)
            self.assertIn("combo_source", by_id[pair["source_card_id"]]["roles"])
            self.assertIn("combo_target", by_id[pair["target_card_id"]]["roles"])
            self.assertEqual(4, by_id[pair["source_card_id"]]["quantity"])
            self.assertEqual(4, by_id[pair["target_card_id"]]["quantity"])

    def test_review_blockers_keep_drafts_out_of_runtime(self):
        for recipe in self.report["recipe_drafts"]:
            metadata = recipe["validation_metadata"]

            self.assertIn("human_recipe_selection", recipe["review_blockers"])
            self.assertIn("m40_03_recipe_validator", recipe["review_blockers"])
            self.assertTrue(metadata["draft_only"])
            self.assertTrue(metadata["requires_m40_03_validator"])
            self.assertTrue(metadata["not_runtime_deck"])
            self.assertTrue(metadata["not_saved_deck"])
            self.assertTrue(metadata["not_ui_published"])
            self.assertTrue(metadata["not_bot_playbook"])
            self.assertTrue(metadata["not_auto_injected"])

    def test_next_target_is_m40_03(self):
        next_target = self.report["next_target"]

        self.assertEqual("M40-03", next_target["milestone"])
        self.assertEqual("Second-slice recipe validator", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m40_02.json"
            md_path = out / "m40_02.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M40-02", loaded["version"])
            self.assertIn("M40-02 Second-Slice Recipe Draft Model", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
