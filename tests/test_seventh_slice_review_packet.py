"""Tests for tools/deck/build_seventh_slice_review_packet.py."""

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

from tests.test_seventh_slice_fixture_readiness import _build_in_memory_m59_01_selection  # noqa: E402
from tools.deck.build_seventh_slice_fixture_readiness import build_seventh_slice_fixture_readiness  # noqa: E402
from tools.deck.build_seventh_slice_fixture_scaffold import build_seventh_slice_fixture_scaffold  # noqa: E402
from tools.deck.build_seventh_slice_recipe_pipeline_entry_gate import (  # noqa: E402
    build_seventh_slice_recipe_pipeline_entry_gate,
)
from tools.deck.build_seventh_slice_review_packet import (  # noqa: E402
    build_seventh_slice_review_packet,
    write_csv,
    write_json,
    write_markdown,
)
from tools.deck.build_seventh_slice_semantic_compatibility_probe import (  # noqa: E402
    build_seventh_slice_semantic_compatibility_probe,
)


class TestSeventhSliceReviewPacket(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.selection = _build_in_memory_m59_01_selection()
        cls.readiness = build_seventh_slice_fixture_readiness(cls.selection)
        cls.probe = build_seventh_slice_semantic_compatibility_probe(cls.selection, cls.readiness)
        cls.gate = build_seventh_slice_recipe_pipeline_entry_gate(cls.readiness, cls.probe)
        cls.scaffold = build_seventh_slice_fixture_scaffold(cls.gate, cls.readiness, cls.probe)
        cls.report = build_seventh_slice_review_packet(
            cls.selection,
            cls.readiness,
            cls.probe,
            cls.gate,
            cls.scaffold,
        )

    def test_review_packet_ready_for_m60_03(self):
        summary = self.report["summary"]

        self.assertEqual("M60-02", self.report["version"])
        self.assertTrue(summary["ready_for_m60_03"])
        self.assertEqual(1, summary["fixture_scaffold_item_count"])
        self.assertEqual(10, summary["manual_card_item_count"])
        self.assertEqual(107, summary["candidate_edge_item_count"])
        self.assertEqual(118, summary["total_review_item_count"])

    def test_evidence_matches_probe_and_scaffold_counts(self):
        evidence = self.report["evidence_summary"]

        self.assertTrue(evidence["m59_04_offline_recipe_pipeline_allowed"])
        self.assertTrue(evidence["m59_04_runtime_promotion_closed"])
        self.assertTrue(evidence["m60_01_scaffold_ready"])
        self.assertEqual(78, evidence["probe_card_count"])
        self.assertEqual(2885, evidence["probe_edge_count"])
        self.assertEqual(10, evidence["expected_manual_review_count"])
        self.assertEqual(107, evidence["expected_candidate_edge_count"])
        self.assertEqual(10, evidence["rebuilt_manual_review_count"])
        self.assertEqual(107, evidence["rebuilt_candidate_edge_count"])

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
        self.assertTrue(policy["g_zone_and_grade4_support_deferred"])
        self.assertTrue(policy["stride_generation_break_text_requires_manual_review"])
        self.assertTrue(policy["bloom_token_text_requires_manual_review"])
        self.assertTrue(policy["no_live_card_text_parsing"])

    def test_fixture_scaffold_item_blocks_recipe_until_reviewed(self):
        item = self.report["fixture_scaffold_items"][0]

        self.assertEqual("fixture_scaffold_note", item["item_type"])
        self.assertEqual("m60_01_fixture_scaffold", item["item_id"])
        self.assertEqual("P0_fixture_policy_review", item["review_priority"])
        self.assertEqual(50, item["main_deck_exact"])
        self.assertEqual(16, item["trigger_target"])
        self.assertTrue(item["mechanic_manual_review_required"])
        self.assertTrue(item["g_zone_recipe_validation_deferred"])
        self.assertTrue(item["grade4_cards_advisory_only"])
        self.assertTrue(item["stride_or_generation_break_text_manual_review_required"])
        self.assertTrue(item["bloom_or_token_like_text_manual_review_required"])
        self.assertIn("m60_04_recipe_validator_uses_scaffold", item["blocked_until"])
        self.assertIn("bloom_token_manual_review_policy_confirmed", item["blocked_until"])

    def test_manual_cards_are_blocked_until_semantic_review(self):
        manual = self.report["manual_card_review_items"]

        for item in manual:
            self.assertEqual("manual_review_card", item["item_type"])
            self.assertEqual("P1_semantic_gap_review", item["review_priority"])
            self.assertIn("semantic_mapping_reviewed", item["blocked_until"])
            self.assertIn("fixture_scaffold_considered", item["blocked_until"])
            self.assertTrue(item["review_reasons"])

    def test_candidate_edges_are_advisory_inputs(self):
        edges = self.report["candidate_edge_review_items"]
        first = edges[0]

        self.assertEqual(107, len(edges))
        self.assertEqual("candidate_edge", first["item_type"])
        self.assertEqual("synergy", first["status"])
        self.assertGreater(first["net_score"], 0)
        self.assertIn("m60_04_recipe_validator", first["blocked_until"])
        self.assertEqual("consider_for_m60_03_advisory_recipe_draft", first["recommended_next_action"])

    def test_runtime_or_saved_deck_flag_blocks_ready(self):
        gate = copy.deepcopy(self.gate)
        gate["decision"]["saved_deck_or_ui_publication_allowed"] = True

        report = build_seventh_slice_review_packet(
            self.selection,
            self.readiness,
            self.probe,
            gate,
            self.scaffold,
        )

        self.assertFalse(report["summary"]["ready_for_m60_03"])
        self.assertFalse(report["evidence_summary"]["m59_04_runtime_promotion_closed"])

    def test_scaffold_not_ready_blocks_ready(self):
        scaffold = copy.deepcopy(self.scaffold)
        scaffold["summary"]["ready_for_m60_02"] = False

        report = build_seventh_slice_review_packet(
            self.selection,
            self.readiness,
            self.probe,
            self.gate,
            scaffold,
        )

        self.assertFalse(report["summary"]["ready_for_m60_03"])
        self.assertFalse(report["evidence_summary"]["m60_01_scaffold_ready"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m60_02.json"
            md_path = out / "m60_02.md"
            csv_path = out / "m60_02.csv"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)
            write_csv(self.report, csv_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M60-02", loaded["version"])
            self.assertIn("M60-02 Seventh-Slice Review Packet", md_path.read_text(encoding="utf-8"))
            with csv_path.open(encoding="utf-8") as handle:
                rows = list(csv.DictReader(handle))
            self.assertEqual(118, len(rows))


if __name__ == "__main__":
    unittest.main()
