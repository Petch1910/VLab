"""Tests for tools/deck/build_fourth_slice_scope_expansion_review.py."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_fourth_slice_scope_expansion_review import (  # noqa: E402
    M47_02_READINESS,
    M47_REPAIR,
    build_fourth_slice_scope_expansion_review,
    load_json,
    write_json,
    write_markdown,
)


class TestFourthSliceScopeExpansionReview(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.readiness = load_json(M47_02_READINESS)
        cls.repair = load_json(M47_REPAIR)
        cls.report = build_fourth_slice_scope_expansion_review(cls.readiness, cls.repair)

    def test_recommends_g_era_heal_expansion(self):
        summary = self.report["summary"]
        decision = self.report["decision"]

        self.assertEqual("M47-repair-expand-scope", self.report["version"])
        self.assertEqual("g_era_heal_expansion", decision["recommended_expansion_id"])
        self.assertTrue(summary["g_era_expectations_met"])
        self.assertTrue(summary["ready_for_m47_repair_apply_scope"])
        self.assertEqual(71, summary["base_source_card_count"])
        self.assertEqual(190, summary["g_era_expanded_source_card_count"])
        self.assertEqual(7, summary["g_era_added_series_count"])

    def test_g_era_expansion_closes_heal_gap(self):
        option = {item["id"]: item for item in self.report["expansion_options"]}["g_era_heal_expansion"]

        self.assertTrue(option["recommended"])
        self.assertEqual([], option["trigger_type_gaps"])
        self.assertEqual(7, option["trigger_counts"]["Heal"])
        self.assertEqual({"0": 47, "1": 48, "2": 46, "3": 27, "4": 22}, option["grade_counts"])
        self.assertEqual(136, option["trigger_capacity"])
        self.assertEqual(624, option["non_trigger_capacity"])
        self.assertTrue(option["all_fixture_expectations_met"])

    def test_all_era_expansion_is_not_recommended(self):
        option = {item["id"]: item for item in self.report["expansion_options"]}["all_same_group_heal_series"]

        self.assertFalse(option["recommended"])
        self.assertEqual("not_recommended_cross_era_mixed", option["policy"])
        self.assertEqual(295, option["source_card_count"])
        self.assertEqual(14, option["trigger_counts"]["Heal"])

    def test_decision_does_not_mutate_data_or_runtime(self):
        decision = self.report["decision"]

        self.assertFalse(decision["scope_expansion_applied"])
        self.assertFalse(decision["card_data_mutated"])
        self.assertFalse(decision["runtime_fixture_created"])
        self.assertFalse(decision["saved_deck_enabled"])
        self.assertFalse(decision["ui_deck_list_enabled"])
        self.assertFalse(decision["bot_playbook_enabled"])

    def test_next_target_is_apply_scope(self):
        next_target = self.report["next_target"]

        self.assertEqual("M47-repair-apply-scope", next_target["milestone"])
        self.assertEqual("Apply reviewed source scope expansion", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m47_expand.json"
            md_path = out / "m47_expand.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M47-repair-expand-scope", loaded["version"])
            self.assertIn("M47-repair-expand-scope Same-Group Source Expansion Review", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
