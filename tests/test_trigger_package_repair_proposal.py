"""Tests for tools/deck/build_trigger_package_repair_proposal.py (M37-02)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_trigger_package_repair_proposal import (  # noqa: E402
    M37_01_CANDIDATES,
    VALIDATION_REPORT,
    build_repair_proposal,
    load_json,
    write_json,
    write_markdown,
)


class TestTriggerPackageRepairProposal(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = build_repair_proposal(
            load_json(M37_01_CANDIDATES),
            load_json(VALIDATION_REPORT),
        )

    def test_report_targets_accepted_seed_recipe(self):
        summary = self.report["summary"]

        self.assertEqual("M37-02", self.report["version"])
        self.assertEqual("recipe_003", summary["recipe_id"])
        self.assertEqual(5, summary["package_count"])
        self.assertEqual(5, summary["packages_resolving_trigger_blockers"])
        self.assertTrue(summary["ready_for_m37_03"])

    def test_original_validation_context_is_preserved(self):
        original = self.report["original_validation"]

        self.assertEqual("invalid_draft", original["validation_status"])
        self.assertEqual(3, original["blocking_issue_count"])
        self.assertEqual(
            ["main_deck_size_mismatch", "unfilled_slots", "trigger_count_mismatch"],
            original["blocker_codes"],
        )
        self.assertEqual(["grade_profile_review", "human_acceptance_pending"], original["review_codes"])
        self.assertEqual(38, original["count_summary"]["explicit_card_count"])
        self.assertEqual(4, original["count_summary"]["trigger_count"])

    def test_recommended_package_is_balanced_classic(self):
        summary = self.report["summary"]
        recommended = self.report["recommended_repair"]

        self.assertEqual("m37_01_pkg_001", summary["recommended_package_id"])
        self.assertEqual("balanced_classic", summary["recommended_profile_id"])
        self.assertEqual("m37_01_pkg_001", recommended["package_id"])
        self.assertEqual("balanced_classic", recommended["profile_id"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, recommended["final_trigger_counts"])

    def test_recommended_delta_resolves_trigger_blockers_only(self):
        recommended = self.report["recommended_repair"]

        self.assertEqual(
            ["main_deck_size_mismatch", "trigger_count_mismatch", "unfilled_slots"],
            recommended["resolved_blockers"],
        )
        self.assertEqual(["grade_profile_review", "human_acceptance_pending"], recommended["remaining_review_issues"])
        self.assertEqual({"0": 16, "2": 6, "3": 28}, recommended["grade_counts_after"])
        self.assertTrue(recommended["requires_human_acceptance"])
        self.assertFalse(recommended["runtime_promotion_allowed"])

    def test_recommended_quantity_delta_is_source_backed(self):
        recommended = self.report["recommended_repair"]
        delta = {(item["card_id"], item["trigger"], item["quantity"], item["series_code"]) for item in recommended["quantity_delta"]}

        self.assertEqual(3, len(recommended["quantity_delta"]))
        self.assertIn(("BT04-077TH", "Critical", 4, "BT04"), delta)
        self.assertIn(("BT02-073TH", "Draw", 4, "BT02"), delta)
        self.assertIn(("BT01-065TH", "Heal", 4, "BT01"), delta)
        for item in recommended["quantity_delta"]:
            self.assertEqual("data/packs/vanguard_th/cards.sqlite:cards", item["source"])

    def test_all_package_simulations_stay_advisory(self):
        for simulation in self.report["package_simulations"]:
            self.assertEqual("trigger_blockers_resolved_pending_review", simulation["status"])
            self.assertEqual([], simulation["unresolved_blockers"])
            self.assertEqual(50, simulation["explicit_card_count_after"])
            self.assertEqual(16, simulation["trigger_count_after"])
            self.assertFalse(simulation["runtime_promotion_allowed"])

    def test_scope_does_not_mutate_recipe_or_runtime(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_repair_proposal"])
        self.assertFalse(scope["changes_recipe_draft"])
        self.assertFalse(scope["creates_runtime_deck"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_deck_injection"])
        self.assertFalse(scope["live_card_text_parsing"])

    def test_next_target_is_m37_03(self):
        next_target = self.report["next_target"]

        self.assertEqual("M37-03", next_target["milestone"])
        self.assertEqual("Rejected-line support-gap triage", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m37_02.json"
            md_path = out / "m37_02.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M37-02", loaded["version"])
            self.assertIn("M37-02 Trigger Package Repair Proposal", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
