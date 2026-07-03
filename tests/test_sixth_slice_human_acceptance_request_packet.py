"""Tests for tools/deck/build_sixth_slice_human_acceptance_request_packet.py."""

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

from tools.deck.build_sixth_slice_human_acceptance_request_packet import (  # noqa: E402
    M57_02_SELECTION,
    build_sixth_slice_human_acceptance_request_packet,
    load_json,
    write_csv,
    write_json,
    write_markdown,
)


class TestSixthSliceHumanAcceptanceRequestPacket(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.selected_artifact = load_json(M57_02_SELECTION)
        cls.report = build_sixth_slice_human_acceptance_request_packet(cls.selected_artifact)

    def test_request_packet_uses_selected_m57_02_artifact_without_accepting(self):
        summary = self.report["summary"]
        decision = self.report["decision"]

        self.assertEqual("M57-03-prerequisite", self.report["version"])
        self.assertEqual("m57_01_m56_recipe_001_repair_review", summary["selected_review_item_id"])
        self.assertEqual("m56_recipe_001", summary["selected_recipe_id"])
        self.assertEqual("G-BT12-062TH", summary["source_card_id"])
        self.assertEqual("G-BT12-066TH", summary["target_card_id"])
        self.assertEqual(4, summary["decision_option_count"])
        self.assertEqual(1, summary["acceptance_option_count"])
        self.assertEqual(0, summary["issue_count"])
        self.assertTrue(summary["acceptance_request_ready"])
        self.assertTrue(summary["human_selection_recorded"])
        self.assertFalse(summary["human_acceptance_recorded"])
        self.assertEqual("M57-03", decision["recommended_milestone"])
        self.assertEqual("M57-03", self.report["next_target"]["milestone"])

    def test_selected_repair_context_is_preserved(self):
        selected = self.report["selected_recipe"]

        self.assertEqual("m57_01_m56_recipe_001_repair_review", selected["selected_review_item_id"])
        self.assertEqual("m56_recipe_001", selected["recipe_id"])
        self.assertEqual("G-BT12-062TH->G-BT12-066TH", selected["source_candidate_edge"])
        self.assertIn("pair G-BT12-062TH -> G-BT12-066TH", selected["selection_text"])
        self.assertEqual("m56_recipe_001_manual_overlap_pkg_001", selected["manual_overlap_package_id"])
        self.assertEqual(7, selected["manual_substitution_count"])
        self.assertEqual("m56_recipe_001_grade_profile_pkg_001", selected["grade_profile_package_id"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, selected["grade_counts_after"])
        self.assertEqual("m56_recipe_001_g_zone_deferred_pkg_001", selected["g_zone_package_id"])
        self.assertTrue(selected["g_zone_deferred"])

    def test_scope_and_policy_do_not_promote_runtime_or_record_acceptance(self):
        scope = self.report["scope"]
        policy = self.report["acceptance_policy"]

        self.assertTrue(scope["read_only_acceptance_request"])
        self.assertTrue(scope["records_human_selection"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["records_g_zone_decision"])
        self.assertFalse(scope["creates_m57_03_artifact"])
        self.assertFalse(scope["declares_recipe_valid"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertTrue(policy["requires_explicit_acceptance_text"])
        self.assertTrue(policy["selection_text_is_not_acceptance_text"])
        self.assertTrue(policy["acceptance_does_not_record_g_zone_decision"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertIn("--acceptance-text", policy["m57_03_command_template"])

    def test_decision_options_mark_only_acceptance_option_as_m57_03_command(self):
        options = {row["option_id"]: row for row in self.report["decision_options"]}

        self.assertEqual(
            "run_acceptance_artifact",
            options["accept_recipe_repairs_and_keep_g_zone_deferred_for_validation_rerun"]["m57_03_action"],
        )
        self.assertIn(
            "--acceptance-text",
            options["accept_recipe_repairs_and_keep_g_zone_deferred_for_validation_rerun"]["command_template"],
        )
        self.assertEqual(
            "do_not_run_m57_03",
            options["accept_original_manual_cards_and_keep_advisory"]["m57_03_action"],
        )
        self.assertEqual(
            "do_not_run_m57_03",
            options["request_different_repair_or_g_zone_scope"]["m57_03_action"],
        )
        self.assertEqual(
            "do_not_run_m57_03",
            options["reject_recipe_runtime_candidate"]["m57_03_action"],
        )

    def test_unready_selection_artifact_routes_to_repair(self):
        selected_artifact = json.loads(json.dumps(self.selected_artifact, ensure_ascii=False))
        selected_artifact["summary"]["ready_for_m57_03"] = False

        report = build_sixth_slice_human_acceptance_request_packet(selected_artifact)

        self.assertFalse(report["summary"]["acceptance_request_ready"])
        self.assertEqual(1, report["summary"]["issue_count"])
        self.assertEqual("m57_02_not_ready_for_m57_03", report["issues"][0]["code"])
        self.assertEqual("M57-02-repair", report["next_target"]["milestone"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "request.json"
            md_path = out / "request.md"
            csv_path = out / "request.csv"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)
            write_csv(self.report, csv_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M57-03-prerequisite", loaded["version"])
            self.assertIn("M57-03 Sixth-Slice Human Acceptance Request Packet", md_path.read_text(encoding="utf-8"))
            with csv_path.open("r", encoding="utf-8", newline="") as handle:
                rows = list(csv.DictReader(handle))
            self.assertEqual(4, len(rows))
            self.assertEqual(
                "accept_recipe_repairs_and_keep_g_zone_deferred_for_validation_rerun",
                rows[0]["option_id"],
            )


if __name__ == "__main__":
    unittest.main()
