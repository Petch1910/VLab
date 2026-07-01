"""Tests for tools/deck/build_fourth_slice_readiness_repair.py."""

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

from tools.deck.build_fourth_slice_readiness_repair import (  # noqa: E402
    M46_04_DECISION,
    M47_02_READINESS,
    build_fourth_slice_readiness_repair,
    load_json,
    write_json,
    write_markdown,
)


class TestFourthSliceReadinessRepair(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.readiness = load_json(M47_02_READINESS)
        cls.m46_decision = load_json(M46_04_DECISION)
        cls.report = build_fourth_slice_readiness_repair(cls.readiness, cls.m46_decision)

    def test_repair_detects_heal_gap_can_use_same_group_source_expansion(self):
        summary = self.report["summary"]
        blockers = self.report["blocker_summary"]

        self.assertEqual("M47-repair", self.report["version"])
        self.assertEqual(1, summary["trigger_gap_count"])
        self.assertTrue(summary["heal_exists_anywhere_in_group"])
        self.assertTrue(blockers["can_repair_with_existing_source"])
        self.assertIn("Heal", blockers["trigger_type_gaps"])
        self.assertFalse(summary["ready_for_reselection"])
        self.assertTrue(summary["ready_for_scope_expansion_review"])

    def test_full_group_summary_confirms_heal_cards_exist_outside_scope(self):
        group_summary = self.report["full_group_card_summary"]

        self.assertEqual(14, len(group_summary["heal_card_ids"]))
        self.assertTrue(group_summary["has_heal_anywhere_in_group"])
        self.assertGreaterEqual(group_summary["total_card_count"], 71)

    def test_repair_options_recommend_scope_review_not_policy_relaxation(self):
        options = {option["id"]: option for option in self.report["repair_options"]}
        decision = self.report["decision"]

        self.assertTrue(options["source_expansion_same_group"]["available"])
        self.assertFalse(options["relax_classic_trigger_profile"]["available"])
        self.assertTrue(options["select_next_candidate"]["available"])
        self.assertEqual("review_same_group_source_expansion", decision["recommended_action"])
        self.assertFalse(decision["card_data_mutated"])
        self.assertFalse(decision["runtime_fixture_created"])

    def test_alternative_candidates_exclude_selected_group(self):
        selected_group = self.report["selected_target"]["group"]
        alternatives = self.report["alternative_candidate_queue"]

        self.assertGreaterEqual(len(alternatives), 1)
        self.assertTrue(all(candidate["group"] != selected_group for candidate in alternatives))

    def test_next_target_is_scope_expansion_review(self):
        next_target = self.report["next_target"]

        self.assertEqual("M47-repair-expand-scope", next_target["milestone"])
        self.assertEqual("Review same-group source expansion", next_target["task"])

    def test_repairable_same_group_source_routes_to_data_repair(self):
        readiness = copy.deepcopy(self.readiness)
        readiness["card_pool_summary"]["trigger_type_gaps"] = []
        readiness["readiness"]["repair_reasons"] = []

        report = build_fourth_slice_readiness_repair(readiness, self.m46_decision)

        self.assertFalse(report["summary"]["ready_for_reselection"])
        self.assertEqual("M47-data-repair", report["next_target"]["milestone"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m47_repair.json"
            md_path = out / "m47_repair.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M47-repair", loaded["version"])
            self.assertIn("M47-repair Fourth-Slice Readiness Blocker Repair", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
