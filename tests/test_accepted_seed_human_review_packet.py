"""Tests for tools/deck/build_accepted_seed_human_review_packet.py (M38-01)."""

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

from tools.deck.build_accepted_seed_human_review_packet import (  # noqa: E402
    M37_02_REPAIR,
    M37_05_RERUN,
    M37_CLOSEOUT,
    build_review_packet,
    load_json,
    write_csv,
    write_json,
    write_markdown,
)


class TestAcceptedSeedHumanReviewPacket(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = build_review_packet(
            load_json(M37_CLOSEOUT),
            load_json(M37_05_RERUN),
            load_json(M37_02_REPAIR),
        )

    def test_packet_has_single_review_item(self):
        summary = self.report["summary"]

        self.assertEqual("M38-01", self.report["version"])
        self.assertEqual(1, summary["review_item_count"])
        self.assertEqual("recipe_003", summary["recipe_id"])
        self.assertEqual(3, summary["quantity_delta_card_count"])
        self.assertEqual(2, summary["unresolved_review_code_count"])
        self.assertEqual(3, summary["decision_option_count"])
        self.assertTrue(summary["ready_for_m38_02"])

    def test_review_item_carries_repair_and_status(self):
        item = self.report["review_items"][0]

        self.assertEqual("m38_01_recipe_003_human_acceptance", item["review_item_id"])
        self.assertEqual("accepted_seed_recipe_review", item["item_type"])
        self.assertEqual("recipe_003", item["recipe_id"])
        self.assertEqual("m37_01_pkg_001", item["recommended_package_id"])
        self.assertEqual("balanced_classic", item["recommended_profile_id"])
        self.assertEqual("validator_passed_pending_human_acceptance", item["validation_status_after"])
        self.assertEqual("consistent_pending_human_acceptance", item["consistency_status_after"])

    def test_quantity_delta_is_the_recommended_trigger_repair(self):
        item = self.report["review_items"][0]
        delta = {(row["card_id"], row["trigger"], row["quantity"]) for row in item["quantity_delta"]}

        self.assertEqual(3, len(item["quantity_delta"]))
        self.assertIn(("BT04-077TH", "Critical", 4), delta)
        self.assertIn(("BT02-073TH", "Draw", 4), delta)
        self.assertIn(("BT01-065TH", "Heal", 4), delta)

    def test_review_codes_and_decision_blockers_stay_visible(self):
        item = self.report["review_items"][0]

        self.assertEqual(["grade_profile_review", "human_acceptance_pending"], item["review_codes"])
        self.assertEqual(
            ["human_acceptance_pending", "grade_profile_review", "promotion_not_allowed"],
            item["decision_blockers"],
        )
        self.assertTrue(item["human_decision_required"])
        self.assertFalse(item["runtime_promotion_allowed"])

    def test_decision_options_do_not_promote_runtime(self):
        item = self.report["review_items"][0]
        option_ids = [option["option_id"] for option in item["decision_options"]]

        self.assertEqual(
            [
                "accept_advisory_trigger_repair_only",
                "request_grade_profile_repair",
                "reject_runtime_promotion",
            ],
            option_ids,
        )
        self.assertEqual(
            "request_grade_profile_repair_before_runtime_acceptance",
            item["recommended_reviewer_action"],
        )

    def test_scope_and_policy_do_not_record_acceptance(self):
        scope = self.report["scope"]
        policy = self.report["review_policy"]

        self.assertTrue(scope["offline_human_review_packet"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_deck"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_playbook_promotion"])
        self.assertTrue(policy["human_decision_required"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertTrue(policy["grade_profile_review_must_clear"])
        self.assertTrue(policy["packet_is_not_acceptance"])

    def test_next_target_is_m38_02(self):
        next_target = self.report["next_target"]

        self.assertEqual("M38-02", next_target["milestone"])
        self.assertEqual("Grade profile repair candidates", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m38_01.json"
            md_path = out / "m38_01.md"
            csv_path = out / "m38_01.csv"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)
            write_csv(self.report, csv_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M38-01", loaded["version"])
            self.assertIn("M38-01 Accepted Seed Human Review Packet", md_path.read_text(encoding="utf-8"))
            rows = list(csv.DictReader(csv_path.read_text(encoding="utf-8").splitlines()))
            self.assertEqual(1, len(rows))
            self.assertEqual("recipe_003", rows[0]["recipe_id"])


if __name__ == "__main__":
    unittest.main()
