"""Tests for tools/deck/build_second_slice_trigger_profile_repair_candidates.py."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_second_slice_trigger_profile_repair_candidates import (  # noqa: E402
    M41_02_ACCEPTED,
    M41_03_VALIDATION,
    build_second_slice_trigger_profile_repair_candidates,
    load_json,
    write_json,
    write_markdown,
)


class TestSecondSliceTriggerProfileRepairCandidates(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.accepted = load_json(M41_02_ACCEPTED)
        cls.validation = load_json(M41_03_VALIDATION)
        cls.report = build_second_slice_trigger_profile_repair_candidates(cls.accepted, cls.validation)

    def test_repair_candidates_are_ready_for_acceptance(self):
        summary = self.report["summary"]

        self.assertEqual("M41-repair", self.report["version"])
        self.assertTrue(summary["trigger_blocker_present"])
        self.assertEqual(3, summary["candidate_package_count"])
        self.assertEqual(3, summary["complete_candidate_count"])
        self.assertEqual(3, summary["ready_for_human_review_count"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertTrue(summary["ready_for_repair_acceptance"])

    def test_balanced_candidate_restores_trigger_and_grade_counts(self):
        package = self.report["repair_candidates"][0]

        self.assertEqual("m41_repair_pkg_001", package["package_id"])
        self.assertEqual("balanced_classic_trigger_restore", package["profile_id"])
        self.assertTrue(package["complete_candidate"])
        self.assertEqual(4, len(package["additions"]))
        self.assertEqual(5, len(package["removals"]))
        self.assertEqual(14, sum(row["quantity"] for row in package["additions"]))
        self.assertEqual(14, sum(row["quantity"] for row in package["removals"]))
        self.assertEqual(50, package["counts_after"]["main_deck_count"])
        self.assertEqual(16, package["counts_after"]["trigger_count"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, package["counts_after"]["grade_counts"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, package["counts_after"]["trigger_counts"])

    def test_all_candidates_are_source_backed_and_non_runtime(self):
        for package in self.report["repair_candidates"]:
            self.assertEqual("trigger_profile_substitution", package["repair_type"])
            self.assertTrue(package["advisory_only"])
            self.assertIn("trigger_count_mismatch", package["resolves_issue_codes"])
            self.assertEqual(0, package["addition_issue_count"])
            self.assertTrue(package["removal_complete"])
            self.assertFalse(package["runtime_promotion_allowed"])
            self.assertTrue(package["ready_for_human_review"])
            for addition in package["additions"]:
                self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", addition["source"])

    def test_scope_and_policy_require_human_acceptance(self):
        scope = self.report["scope"]
        policy = self.report["repair_policy"]

        self.assertTrue(scope["offline_repair_candidates"])
        self.assertFalse(scope["changes_accepted_artifact"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertTrue(policy["source_backed_candidates"])
        self.assertEqual(16, policy["trigger_count_target"])
        self.assertTrue(policy["human_acceptance_required_after_candidate"])
        self.assertFalse(policy["runtime_promotion_allowed"])

    def test_next_target_is_repair_acceptance(self):
        next_target = self.report["next_target"]

        self.assertEqual("M41-repair-accept", next_target["milestone"])
        self.assertEqual("Second-slice trigger repair acceptance artifact", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m41_repair.json"
            md_path = out / "m41_repair.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M41-repair", loaded["version"])
            self.assertIn("M41 Repair Second-Slice Trigger/Profile Repair Candidates", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
