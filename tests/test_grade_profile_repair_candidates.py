"""Tests for tools/deck/build_grade_profile_repair_candidates.py (M38-02)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_grade_profile_repair_candidates import (  # noqa: E402
    M37_02_REPAIR,
    M37_05_RERUN,
    M38_01_REVIEW,
    RECIPE_DRAFTS,
    build_grade_profile_candidates,
    load_json,
    write_json,
    write_markdown,
)


class TestGradeProfileRepairCandidates(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = build_grade_profile_candidates(
            load_json(RECIPE_DRAFTS),
            load_json(M37_02_REPAIR),
            load_json(M37_05_RERUN),
            load_json(M38_01_REVIEW),
        )

    def test_report_builds_complete_grade_repair_candidates(self):
        summary = self.report["summary"]

        self.assertEqual("M38-02", self.report["version"])
        self.assertEqual("recipe_003", summary["recipe_id"])
        self.assertEqual(2, summary["repair_candidate_count"])
        self.assertEqual(2, summary["complete_candidate_count"])
        self.assertEqual(20, summary["grade_deficit_total"])
        self.assertEqual(20, summary["grade_surplus_total"])
        self.assertTrue(summary["ready_for_m38_03"])

    def test_current_profile_carries_deficits_and_surplus(self):
        profile = self.report["current_profile"]

        self.assertEqual({"3": 28, "2": 6, "0": 16}, profile["grade_counts"])
        self.assertEqual({"0": 1, "1": 14, "2": 5, "3": 0}, profile["deficits"])
        self.assertEqual({"0": 0, "1": 0, "2": 0, "3": 20}, profile["surpluses"])
        self.assertEqual(["grade_profile_review", "human_acceptance_pending"], profile["review_codes"])
        self.assertTrue(profile["human_review_packet_ready"])

    def test_each_candidate_hits_classic_grade_target_by_substitution(self):
        target = {"0": 17, "1": 14, "2": 11, "3": 8}

        for candidate in self.report["repair_candidates"]:
            self.assertTrue(candidate["advisory_only"])
            self.assertEqual("complete_candidate", candidate["completion_status"])
            self.assertEqual(target, candidate["grade_counts_after"])
            self.assertEqual(20, candidate["added_card_count"])
            self.assertEqual(20, candidate["removed_card_count"])
            self.assertEqual(0, candidate["net_card_count_change"])
            self.assertFalse(candidate["runtime_promotion_allowed"])

    def test_first_candidate_adds_source_backed_grade_zero_one_two(self):
        candidate = self.report["repair_candidates"][0]
        additions = {(item["card_id"], item["quantity"], item["grade"]) for item in candidate["additions"]}

        self.assertEqual("m38_02_grade_pkg_001", candidate["package_id"])
        self.assertIn(("BT01-063TH", 1, "0"), additions)
        self.assertTrue(any(item["grade"] == "1" for item in candidate["additions"]))
        self.assertTrue(any(item["grade"] == "2" for item in candidate["additions"]))
        for item in candidate["additions"]:
            self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", item["source"])

    def test_candidates_remove_grade_three_surplus_only(self):
        for candidate in self.report["repair_candidates"]:
            self.assertEqual(20, sum(item["quantity"] for item in candidate["removals"]))
            self.assertTrue(all(item["grade"] == "3" for item in candidate["removals"]))
            self.assertTrue(all(item["source"] == "m36_02_deck_recipe_draft_model" for item in candidate["removals"]))

    def test_scope_and_policy_are_non_runtime(self):
        scope = self.report["scope"]
        policy = self.report["grade_profile_policy"]

        self.assertTrue(scope["offline_grade_profile_candidates"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_deck"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["live_card_text_parsing"])
        self.assertTrue(policy["source_backed_candidates"])
        self.assertTrue(policy["substitution_only"])
        self.assertFalse(policy["runtime_promotion_allowed"])

    def test_next_target_is_m38_03(self):
        next_target = self.report["next_target"]

        self.assertEqual("M38-03", next_target["milestone"])
        self.assertEqual("Human-accepted recipe artifact", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m38_02.json"
            md_path = out / "m38_02.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M38-02", loaded["version"])
            self.assertIn("M38-02 Grade Profile Repair Candidates", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
