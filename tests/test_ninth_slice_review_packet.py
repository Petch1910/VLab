"""Tests for tools/deck/build_ninth_slice_review_packet.py."""

from __future__ import annotations

import copy
import csv
import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import tests.test_ninth_slice_fixture_scaffold as ninth_scaffold_fixture  # noqa: E402
import tests.test_ninth_target_slice_selection as ninth_selection_fixture  # noqa: E402
from tools.deck.build_ninth_slice_review_packet import (  # noqa: E402
    build_ninth_slice_review_packet,
    write_csv,
    write_json,
    write_markdown,
)


class TestNinthSliceReviewPacket(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        ninth_scaffold_fixture.TestNinthSliceFixtureScaffold.setUpClass()
        ninth_selection_fixture.TestNinthTargetSliceSelection.setUpClass()
        cls.selection = ninth_selection_fixture.TestNinthTargetSliceSelection.report
        cls.readiness = ninth_scaffold_fixture.TestNinthSliceFixtureScaffold.readiness
        cls.probe = ninth_scaffold_fixture.TestNinthSliceFixtureScaffold.probe
        cls.gate = ninth_scaffold_fixture.TestNinthSliceFixtureScaffold.gate
        cls.scaffold = ninth_scaffold_fixture.TestNinthSliceFixtureScaffold.report
        cls.report = build_ninth_slice_review_packet(
            cls.selection,
            cls.readiness,
            cls.probe,
            cls.gate,
            cls.scaffold,
        )

    def test_review_packet_ready_for_m68_03(self):
        summary = self.report["summary"]

        self.assertEqual("M68-02", self.report["version"])
        self.assertTrue(summary["ready_for_m68_03"])
        self.assertEqual(1, summary["fixture_scaffold_item_count"])
        self.assertEqual(10, summary["manual_card_item_count"])
        self.assertEqual(95, summary["candidate_edge_item_count"])
        self.assertEqual(106, summary["total_review_item_count"])
        self.assertEqual("M68-03", self.report["next_target"]["milestone"])

    def test_evidence_matches_probe_and_scaffold_counts(self):
        evidence = self.report["evidence_summary"]

        self.assertTrue(evidence["m67_04_offline_recipe_pipeline_allowed"])
        self.assertTrue(evidence["m67_04_runtime_promotion_closed"])
        self.assertTrue(evidence["m68_01_scaffold_ready"])
        self.assertEqual(77, evidence["probe_card_count"])
        self.assertEqual(2708, evidence["probe_edge_count"])
        self.assertEqual(10, evidence["expected_manual_review_count"])
        self.assertEqual(95, evidence["expected_candidate_edge_count"])
        self.assertEqual(10, evidence["rebuilt_manual_review_count"])
        self.assertEqual(95, evidence["rebuilt_candidate_edge_count"])

    def test_scope_and_policy_are_review_only(self):
        scope = self.report["scope"]
        policy = self.report["review_policy"]

        self.assertTrue(scope["offline_review_packet"])
        self.assertFalse(scope["deck_recipe_draft"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["runtime_deck_promotion"])
        self.assertFalse(scope["bot_playbook_promotion"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertTrue(policy["human_selection_required_before_recipe_draft"])
        self.assertTrue(policy["manual_review_cards_blocked_from_recipe_until_resolved"])
        self.assertTrue(policy["candidate_edges_are_advisory_inputs_only"])
        self.assertTrue(policy["fixture_scaffold_is_policy_evidence_not_saved_deck"])
        self.assertTrue(policy["g_zone_and_stride_runtime_support_deferred"])
        self.assertTrue(policy["stride_or_g_unit_text_requires_manual_review"])
        self.assertTrue(policy["generation_break_text_requires_manual_review"])
        self.assertTrue(policy["aqua_force_attack_count_text_requires_manual_review"])
        self.assertTrue(policy["grade4_g_zone_cards_review_only"])
        self.assertTrue(policy["no_live_card_text_parsing"])
        self.assertTrue(policy["no_direct_GameState_mutation"])

    def test_fixture_scaffold_item_blocks_recipe_until_reviewed(self):
        item = self.report["fixture_scaffold_items"][0]

        self.assertEqual("fixture_scaffold_note", item["item_type"])
        self.assertEqual("m68_01_fixture_scaffold", item["item_id"])
        self.assertEqual("P0_fixture_policy_review", item["review_priority"])
        self.assertEqual(50, item["main_deck_exact"])
        self.assertEqual(16, item["trigger_target"])
        self.assertTrue(item["g_zone_fixture_boundary_required"])
        self.assertTrue(item["mechanic_manual_review_required"])
        self.assertFalse(item["g_zone_runtime_enabled"])
        self.assertFalse(item["stride_runtime_enabled"])
        self.assertTrue(item["stride_or_g_unit_text_manual_review_required"])
        self.assertTrue(item["generation_break_text_manual_review_required"])
        self.assertTrue(item["aqua_force_attack_count_text_manual_review_required"])
        self.assertTrue(item["grade4_cards_g_zone_advisory_only"])
        self.assertIn("m68_04_recipe_validator_uses_scaffold", item["blocked_until"])
        self.assertIn("g_zone_stride_policy_confirmed", item["blocked_until"])
        self.assertIn("aqua_force_battle_order_policy_confirmed", item["blocked_until"])

    def test_manual_cards_are_blocked_until_semantic_review(self):
        manual = self.report["manual_card_review_items"]

        self.assertEqual(10, len(manual))
        for item in manual:
            self.assertEqual("manual_review_card", item["item_type"])
            self.assertEqual("P1_semantic_gap_review", item["review_priority"])
            self.assertIn("semantic_mapping_reviewed", item["blocked_until"])
            self.assertIn("fixture_scaffold_considered", item["blocked_until"])
            self.assertTrue(item["review_reasons"])

    def test_candidate_edges_are_advisory_inputs(self):
        edges = self.report["candidate_edge_review_items"]
        first = edges[0]

        self.assertEqual(95, len(edges))
        self.assertEqual("candidate_edge", first["item_type"])
        self.assertEqual("synergy", first["status"])
        self.assertGreater(first["net_score"], 0)
        self.assertIn("m68_04_recipe_validator", first["blocked_until"])
        self.assertIn("aqua_force_battle_order_review_boundary", first["review_reasons"])
        self.assertEqual("consider_for_m68_03_advisory_recipe_draft", first["recommended_next_action"])

    def test_runtime_or_saved_deck_flag_blocks_ready(self):
        gate = copy.deepcopy(self.gate)
        gate["decision"]["saved_deck_or_ui_publication_allowed"] = True

        report = build_ninth_slice_review_packet(
            self.selection,
            self.readiness,
            self.probe,
            gate,
            self.scaffold,
        )

        self.assertFalse(report["summary"]["ready_for_m68_03"])
        self.assertFalse(report["evidence_summary"]["m67_04_runtime_promotion_closed"])

    def test_scaffold_not_ready_blocks_ready(self):
        scaffold = copy.deepcopy(self.scaffold)
        scaffold["summary"]["ready_for_m68_02"] = False

        report = build_ninth_slice_review_packet(
            self.selection,
            self.readiness,
            self.probe,
            self.gate,
            scaffold,
        )

        self.assertFalse(report["summary"]["ready_for_m68_03"])
        self.assertFalse(report["evidence_summary"]["m68_01_scaffold_ready"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m68_02.json"
            md_path = out / "m68_02.md"
            csv_path = out / "m68_02.csv"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)
            write_csv(self.report, csv_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M68-02", loaded["version"])
            self.assertIn("M68-02 Ninth-Slice Review Packet", md_path.read_text(encoding="utf-8"))
            with csv_path.open(encoding="utf-8") as handle:
                rows = list(csv.DictReader(handle))
            self.assertEqual(106, len(rows))


if __name__ == "__main__":
    unittest.main()
