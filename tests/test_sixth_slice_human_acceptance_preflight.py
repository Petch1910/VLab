"""Tests for tools/deck/build_sixth_slice_human_acceptance_preflight.py."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_sixth_slice_human_acceptance_preflight import (  # noqa: E402
    build_sixth_slice_human_acceptance_preflight,
    write_json,
    write_markdown,
)
from tools.deck.build_sixth_slice_human_acceptance_request_packet import (  # noqa: E402
    build_sixth_slice_human_acceptance_request_packet,
)


ACCEPTANCE_TEXT = "team dry-run accepts selected M57-02 repairs"


class TestSixthSliceHumanAcceptancePreflight(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.request_packet = build_sixth_slice_human_acceptance_request_packet()

    def test_preflight_without_input_reports_required_acceptance_without_recording(self):
        report = build_sixth_slice_human_acceptance_preflight(self.request_packet)

        summary = report["summary"]
        decision = report["decision"]
        dry_run = report["dry_run"]

        self.assertEqual("M57-03-preflight", report["version"])
        self.assertTrue(summary["request_ready"])
        self.assertEqual("m57_01_m56_recipe_001_repair_review", summary["selected_review_item_id"])
        self.assertEqual("m56_recipe_001", summary["selected_recipe_id"])
        self.assertEqual(1, summary["input_issue_count"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertFalse(summary["preflight_passed"])
        self.assertFalse(summary["ready_for_real_m57_03_command"])
        self.assertFalse(summary["human_acceptance_recorded"])
        self.assertFalse(dry_run["executed"])
        self.assertFalse(dry_run["would_create_m57_03_artifact"])
        self.assertFalse(decision["ready_for_real_m57_03_command"])
        self.assertEqual("M57-03-user-acceptance", report["next_target"]["milestone"])
        self.assertEqual(["missing_acceptance_text"], [issue["code"] for issue in report["issues"]])

    def test_valid_acceptance_preflight_dry_runs_real_generator_contract(self):
        report = build_sixth_slice_human_acceptance_preflight(
            self.request_packet,
            acceptance_text=ACCEPTANCE_TEXT,
        )

        summary = report["summary"]
        dry_run = report["dry_run"]

        self.assertTrue(summary["preflight_passed"])
        self.assertTrue(summary["ready_for_real_m57_03_command"])
        self.assertEqual(0, summary["issue_count"])
        self.assertEqual("m56_recipe_001", summary["accepted_recipe_id"])
        self.assertTrue(dry_run["executed"])
        self.assertTrue(dry_run["would_create_m57_03_artifact"])
        self.assertTrue(dry_run["would_record_human_selection"])
        self.assertTrue(dry_run["would_record_human_acceptance"])
        self.assertFalse(dry_run["would_record_g_zone_decision"])
        self.assertFalse(dry_run["would_declare_recipe_valid"])
        self.assertFalse(dry_run["would_allow_runtime_promotion"])
        self.assertTrue(dry_run["would_be_ready_for_m57_04"])
        self.assertEqual("m57_01_m56_recipe_001_repair_review", dry_run["accepted_review_item_id"])
        self.assertEqual("m56_recipe_001_combined_manual_grade_pkg_001", dry_run["accepted_combined_repair_package_id"])
        self.assertEqual("accepted", dry_run["acceptance_decision"])
        self.assertIn("--acceptance-text", dry_run["real_command"])
        self.assertIn(ACCEPTANCE_TEXT, dry_run["real_command"])
        self.assertEqual("M57-03", report["next_target"]["milestone"])

    def test_unready_request_packet_is_blocking(self):
        request_packet = json.loads(json.dumps(self.request_packet, ensure_ascii=False))
        request_packet["summary"]["acceptance_request_ready"] = False

        report = build_sixth_slice_human_acceptance_preflight(
            request_packet,
            acceptance_text=ACCEPTANCE_TEXT,
        )

        self.assertFalse(report["summary"]["preflight_passed"])
        self.assertEqual(1, report["summary"]["blocking_issue_count"])
        self.assertEqual(0, report["summary"]["input_issue_count"])
        self.assertEqual("acceptance_request_not_ready", report["issues"][0]["code"])
        self.assertFalse(report["dry_run"]["executed"])
        self.assertEqual("M57-03-user-acceptance", report["next_target"]["milestone"])

    def test_blank_acceptance_text_is_input_issue(self):
        report = build_sixth_slice_human_acceptance_preflight(
            self.request_packet,
            acceptance_text=" ",
        )

        self.assertFalse(report["summary"]["preflight_passed"])
        self.assertEqual(0, report["summary"]["blocking_issue_count"])
        self.assertEqual(1, report["summary"]["input_issue_count"])
        self.assertEqual("missing_acceptance_text", report["issues"][0]["code"])
        self.assertFalse(report["dry_run"]["executed"])

    def test_scope_does_not_record_or_mutate_runtime(self):
        report = build_sixth_slice_human_acceptance_preflight(
            self.request_packet,
            acceptance_text=ACCEPTANCE_TEXT,
        )
        scope = report["scope"]

        self.assertTrue(scope["read_only_preflight"])
        self.assertFalse(scope["records_human_selection"])
        self.assertFalse(scope["records_human_acceptance"])
        self.assertFalse(scope["records_g_zone_decision"])
        self.assertFalse(scope["creates_m57_03_artifact"])
        self.assertFalse(scope["declares_recipe_valid"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])

    def test_output_round_trip(self):
        report = build_sixth_slice_human_acceptance_preflight(
            self.request_packet,
            acceptance_text=ACCEPTANCE_TEXT,
        )

        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "preflight.json"
            md_path = out / "preflight.md"

            write_json(report, json_path)
            write_markdown(report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M57-03-preflight", loaded["version"])
            self.assertTrue(loaded["summary"]["preflight_passed"])
            self.assertIn("M57-03 Sixth-Slice Human Acceptance Preflight", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
