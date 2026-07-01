"""Tests for tools/deck/build_first_slice_review_packet.py (M36-01)."""

from __future__ import annotations

import csv
import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_first_slice_review_packet import (  # noqa: E402
    B4_MANUAL_REVIEW,
    D4_REVIEWED_SEED,
    M35_CLOSEOUT,
    build_review_packet,
    load_json,
    write_csv,
    write_json,
    write_markdown,
)


class TestFirstSliceReviewPacket(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.b4 = load_json(B4_MANUAL_REVIEW)
        cls.d4 = load_json(D4_REVIEWED_SEED)
        cls.closeout = load_json(M35_CLOSEOUT)
        cls.report = build_review_packet(cls.b4, cls.d4, cls.closeout)

    def test_review_packet_ready_for_m36_02(self):
        summary = self.report["summary"]

        self.assertEqual("M36-01", self.report["version"])
        self.assertTrue(summary["ready_for_m36_02"])
        self.assertEqual(1, summary["accepted_seed_item_count"])
        self.assertEqual(24, summary["rejected_line_item_count"])
        self.assertEqual(6, summary["manual_card_item_count"])
        self.assertEqual(31, summary["total_review_item_count"])

    def test_scope_is_offline_only(self):
        scope = self.report["scope"]
        policy = self.report["review_policy"]

        self.assertTrue(scope["offline_review_packet"])
        self.assertFalse(scope["runtime_playbook"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["deck_recipe_draft"])
        self.assertFalse(scope["automatic_deck_injection"])
        self.assertTrue(policy["human_acceptance_required"])
        self.assertTrue(policy["no_live_card_text_parsing"])
        self.assertTrue(policy["no_direct_GameState_mutation"])

    def test_accepted_seed_requires_human_acceptance_before_recipe(self):
        items = self.report["accepted_seed_review_items"]
        item = items[0]

        self.assertEqual("accepted_seed", item["item_type"])
        self.assertEqual("seed_001", item["item_id"])
        self.assertIn("human_acceptance", item["blocked_until"])
        self.assertIn("deck_recipe_validator", item["blocked_until"])
        self.assertEqual("confirm_or_demote_before_m36_02_recipe_draft", item["recommended_next_action"])

    def test_rejected_lines_keep_support_gap_work(self):
        rejected = self.report["rejected_line_review_items"]

        self.assertEqual(self.d4["summary"]["rejected_line_count"], len(rejected))
        for item in rejected:
            self.assertEqual("rejected_line", item["item_type"])
            self.assertIn("support_gap_resolved", item["blocked_until"])
            self.assertTrue(item["review_reasons"])
            self.assertTrue(item["needs_to_work"])

    def test_manual_cards_are_blocked_until_semantic_review(self):
        manual = self.report["manual_card_review_items"]

        self.assertEqual(self.b4["summary"]["manual_review_count"], len(manual))
        for item in manual:
            self.assertEqual("manual_review_card", item["item_type"])
            self.assertIn("semantic_mapping_reviewed", item["blocked_until"])
            self.assertIn("unmapped_feature_tags", item["review_reasons"])
            self.assertTrue(item["unmapped_feature_tags"])

    def test_next_target_is_m36_02(self):
        next_target = self.report["next_target"]

        self.assertEqual("M36-02", next_target["milestone"])
        self.assertEqual("Deck recipe draft model", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "packet.json"
            md_path = out / "packet.md"
            csv_path = out / "packet.csv"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)
            write_csv(self.report, csv_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M36-01", loaded["version"])
            self.assertIn("M36-01 First-Slice Review Packet", md_path.read_text(encoding="utf-8"))
            with csv_path.open(encoding="utf-8") as handle:
                rows = list(csv.DictReader(handle))
            self.assertEqual(31, len(rows))


if __name__ == "__main__":
    unittest.main()
