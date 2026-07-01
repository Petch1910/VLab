"""Tests for tools/deck/build_second_slice_review_packet.py (M40-01)."""

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

from tools.deck.build_second_slice_review_packet import (  # noqa: E402
    M35_E3_REPORT,
    M39_04_REPORT,
    SECOND_SLICE_READINESS,
    SECOND_SLICE_REPORT,
    build_second_slice_review_packet,
    load_json,
    write_csv,
    write_json,
    write_markdown,
)


class TestSecondSliceReviewPacket(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.selected = load_json(SECOND_SLICE_REPORT)
        cls.readiness = load_json(SECOND_SLICE_READINESS)
        cls.e3 = load_json(M35_E3_REPORT)
        cls.m39_04 = load_json(M39_04_REPORT)
        cls.report = build_second_slice_review_packet(
            cls.selected,
            cls.readiness,
            cls.e3,
            cls.m39_04,
        )

    def test_review_packet_ready_for_m40_02(self):
        summary = self.report["summary"]

        self.assertEqual("M40-01", self.report["version"])
        self.assertTrue(summary["ready_for_m40_02"])
        self.assertEqual(6, summary["fixture_note_count"])
        self.assertEqual(7, summary["manual_card_item_count"])
        self.assertEqual(259, summary["candidate_edge_item_count"])
        self.assertEqual(272, summary["total_review_item_count"])

    def test_evidence_matches_second_slice_probe_counts(self):
        evidence = self.report["evidence_summary"]

        self.assertTrue(evidence["m39_04_offline_recipe_pipeline_allowed"])
        self.assertTrue(evidence["m39_04_runtime_promotion_closed"])
        self.assertTrue(evidence["fixture_expectations_met"])
        self.assertEqual(103, evidence["probe_card_count"])
        self.assertEqual(2660, evidence["probe_edge_count"])
        self.assertEqual(7, evidence["expected_manual_review_count"])
        self.assertEqual(259, evidence["expected_candidate_edge_count"])
        self.assertEqual(7, evidence["rebuilt_manual_review_count"])
        self.assertEqual(259, evidence["rebuilt_candidate_edge_count"])

    def test_scope_is_offline_review_only(self):
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
        self.assertTrue(policy["candidate_edges_are_advisory_inputs_only"])
        self.assertTrue(policy["no_live_card_text_parsing"])
        self.assertTrue(policy["no_direct_GameState_mutation"])

    def test_fixture_notes_include_positive_and_negative_evidence(self):
        fixtures = self.report["fixture_note_items"]
        expected_by_id = {item["fixture_id"]: item["expected"] for item in fixtures}

        self.assertIn("classic_core_second_slice_valid_minimal", expected_by_id)
        self.assertEqual("pass", expected_by_id["classic_core_second_slice_valid_minimal"])
        self.assertIn("classic_core_second_slice_identity_mismatch", expected_by_id)
        self.assertEqual("fail", expected_by_id["classic_core_second_slice_identity_mismatch"])
        for item in fixtures:
            self.assertTrue(item["expectation_met"])
            self.assertIn("fixture_expectations_met", item["blocked_until"])

    def test_manual_cards_are_blocked_until_semantic_review(self):
        manual = self.report["manual_card_review_items"]

        for item in manual:
            self.assertEqual("manual_review_card", item["item_type"])
            self.assertIn("semantic_mapping_reviewed", item["blocked_until"])
            self.assertIn("unmapped_feature_tags", item["review_reasons"])
            self.assertTrue(item["unmapped_feature_tags"])

    def test_candidate_edges_are_advisory_inputs(self):
        edges = self.report["candidate_edge_review_items"]
        first = edges[0]

        self.assertEqual(259, len(edges))
        self.assertEqual("candidate_edge", first["item_type"])
        self.assertEqual("synergy", first["status"])
        self.assertGreater(first["net_score"], 0)
        self.assertIn("deck_recipe_validator", first["blocked_until"])
        self.assertEqual("consider_for_m40_02_advisory_recipe_draft", first["recommended_next_action"])

    def test_runtime_or_saved_deck_flag_blocks_ready(self):
        m39_04 = copy.deepcopy(self.m39_04)
        m39_04["decision"]["saved_deck_or_ui_publication_allowed"] = True

        report = build_second_slice_review_packet(
            self.selected,
            self.readiness,
            self.e3,
            m39_04,
        )

        self.assertFalse(report["summary"]["ready_for_m40_02"])
        self.assertFalse(report["evidence_summary"]["m39_04_runtime_promotion_closed"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m40_01.json"
            md_path = out / "m40_01.md"
            csv_path = out / "m40_01.csv"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)
            write_csv(self.report, csv_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M40-01", loaded["version"])
            self.assertIn("M40-01 Second-Slice Review Packet", md_path.read_text(encoding="utf-8"))
            with csv_path.open(encoding="utf-8") as handle:
                rows = list(csv.DictReader(handle))
            self.assertEqual(272, len(rows))


if __name__ == "__main__":
    unittest.main()
