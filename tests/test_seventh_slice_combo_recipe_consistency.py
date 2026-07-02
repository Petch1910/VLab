"""Tests for tools/deck/check_seventh_slice_combo_recipe_consistency.py (M60-05)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tests.test_seventh_slice_fixture_readiness import _build_in_memory_m59_01_selection  # noqa: E402
from tools.deck.build_seventh_slice_fixture_readiness import build_seventh_slice_fixture_readiness  # noqa: E402
from tools.deck.build_seventh_slice_fixture_scaffold import build_seventh_slice_fixture_scaffold  # noqa: E402
from tools.deck.build_seventh_slice_recipe_draft_model import build_seventh_slice_recipe_drafts  # noqa: E402
from tools.deck.build_seventh_slice_recipe_pipeline_entry_gate import (  # noqa: E402
    build_seventh_slice_recipe_pipeline_entry_gate,
)
from tools.deck.build_seventh_slice_review_packet import build_seventh_slice_review_packet  # noqa: E402
from tools.deck.build_seventh_slice_semantic_compatibility_probe import (  # noqa: E402
    build_seventh_slice_semantic_compatibility_probe,
)
from tools.deck.check_seventh_slice_combo_recipe_consistency import (  # noqa: E402
    build_seventh_slice_consistency_report,
    write_json,
    write_markdown,
)
from tools.deck.validate_seventh_slice_recipe_drafts import (  # noqa: E402
    _all_card_ids,
    build_seventh_slice_validation_report,
    load_card_rows,
)


class TestSeventhSliceComboRecipeConsistency(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.selection = _build_in_memory_m59_01_selection()
        cls.readiness = build_seventh_slice_fixture_readiness(cls.selection)
        cls.probe = build_seventh_slice_semantic_compatibility_probe(cls.selection, cls.readiness)
        cls.gate = build_seventh_slice_recipe_pipeline_entry_gate(cls.readiness, cls.probe)
        cls.scaffold = build_seventh_slice_fixture_scaffold(cls.gate, cls.readiness, cls.probe)
        cls.packet = build_seventh_slice_review_packet(
            cls.selection,
            cls.readiness,
            cls.probe,
            cls.gate,
            cls.scaffold,
        )
        cls.recipes = build_seventh_slice_recipe_drafts(cls.packet, cls.scaffold)
        cls.card_rows = load_card_rows(_all_card_ids(cls.recipes))
        cls.validation = build_seventh_slice_validation_report(cls.recipes, cls.card_rows)
        cls.report = build_seventh_slice_consistency_report(cls.recipes, cls.validation)

    def test_consistency_checks_all_seventh_slice_drafts(self):
        summary = self.report["summary"]

        self.assertEqual("M60-05", self.report["version"])
        self.assertTrue(summary["ready_for_m60_06"])
        self.assertEqual(23, summary["recipe_count"])
        self.assertEqual(23, summary["consistency_check_count"])
        self.assertEqual(23, summary["pair_cards_present_count"])
        self.assertEqual(0, summary["missing_pair_card_check_count"])

    def test_promotion_remains_blocked_by_manual_review_and_deferred_mechanics(self):
        summary = self.report["summary"]

        self.assertEqual(0, summary["promotion_allowed_count"])
        self.assertEqual(0, summary["runtime_ready_consistent_count"])
        self.assertEqual(0, summary["pair_manual_dependency_check_count"])
        self.assertEqual(23, summary["recipe_manual_dependency_check_count"])
        self.assertEqual(23, summary["g_zone_deferred_check_count"])
        self.assertEqual(23, summary["bloom_token_deferred_check_count"])
        self.assertEqual({"blocked_by_manual_review": 23}, summary["status_counts"])

    def test_scope_is_offline_only(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_consistency_check"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["runtime_deck"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["automatic_deck_injection"])
        self.assertFalse(scope["direct_GameState_mutation"])

    def test_each_candidate_pair_is_present_but_validation_blocks(self):
        for item in self.report["consistency_checks"]:
            self.assertTrue(item["pair_cards_present"])
            self.assertEqual([], item["missing_pair_card_ids"])
            self.assertEqual("blocked_by_manual_review", item["recipe_validation_status"])
            self.assertEqual("blocked_by_manual_review", item["consistency_status"])
            self.assertEqual([], item["pair_manual_review_dependencies"])
            self.assertTrue(item["recipe_manual_review_dependencies"])
            self.assertTrue(item["g_zone_support_deferred"])
            self.assertTrue(item["bloom_token_support_deferred"])
            self.assertFalse(item["promotion_allowed"])

    def test_next_target_is_m60_06(self):
        next_target = self.report["next_target"]

        self.assertEqual("M60-06", next_target["milestone"])
        self.assertEqual("Seventh-slice blocker repair candidates", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m60_05.json"
            md_path = out / "m60_05.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M60-05", loaded["version"])
            self.assertIn("M60-05 Seventh-Slice Combo-To-Recipe Consistency", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
