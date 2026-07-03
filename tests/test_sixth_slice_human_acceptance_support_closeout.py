"""Tests for tools/deck/build_sixth_slice_human_acceptance_support_closeout.py."""

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
)
from tools.deck.build_sixth_slice_human_acceptance_request_packet import (  # noqa: E402
    build_sixth_slice_human_acceptance_request_packet,
)
from tools.deck.build_sixth_slice_human_acceptance_support_closeout import (  # noqa: E402
    build_sixth_slice_human_acceptance_support_closeout,
    write_json,
    write_markdown,
)


class TestSixthSliceHumanAcceptanceSupportCloseout(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.request_packet = build_sixth_slice_human_acceptance_request_packet()
        cls.preflight_report = build_sixth_slice_human_acceptance_preflight(cls.request_packet)
        cls.report = build_sixth_slice_human_acceptance_support_closeout(
            cls.request_packet,
            cls.preflight_report,
            accepted_artifact_exists=False,
        )

    def test_closeout_completes_acceptance_support_without_creating_acceptance(self):
        summary = self.report["summary"]
        evidence = self.report["evidence"]
        handoff = self.report["handoff"]

        self.assertEqual("M57-03-acceptance-support-closeout", self.report["version"])
        self.assertTrue(summary["support_closeout_complete"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertTrue(summary["acceptance_request_ready"])
        self.assertTrue(summary["preflight_report_ready"])
        self.assertTrue(summary["default_preflight_requires_input"])
        self.assertFalse(summary["human_acceptance_recorded"])
        self.assertFalse(summary["real_m57_03_artifact_created"])
        self.assertTrue(summary["ready_for_user_acceptance"])
        self.assertEqual("m57_01_m56_recipe_001_repair_review", evidence["selected_review_item_id"])
        self.assertEqual("m56_recipe_001", evidence["selected_recipe_id"])
        self.assertEqual("G-BT12-062TH", evidence["source_card_id"])
        self.assertEqual("G-BT12-066TH", evidence["target_card_id"])
        self.assertEqual(4, evidence["decision_option_count"])
        self.assertEqual(1, evidence["acceptance_option_count"])
        self.assertEqual(1, evidence["preflight_input_issue_count"])
        self.assertEqual(0, evidence["preflight_blocking_issue_count"])
        self.assertFalse(evidence["human_acceptance_recorded_anywhere"])
        self.assertTrue(handoff["requires_explicit_acceptance_text"])
        self.assertTrue(handoff["selection_text_is_not_acceptance_text"])
        self.assertTrue(handoff["real_m57_03_artifact_blocked_until_acceptance"])
        self.assertIn("--acceptance-text", handoff["preflight_command_template"])
        self.assertEqual("M57-03-user-acceptance", self.report["next_target"]["milestone"])

    def test_scope_blocks_acceptance_runtime_ui_bot_and_gamestate(self):
        scope = self.report["scope"]

        self.assertTrue(scope["read_only_support_closeout"])
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

    def test_unready_request_packet_blocks_closeout(self):
        request_packet = json.loads(json.dumps(self.request_packet, ensure_ascii=False))
        request_packet["summary"]["acceptance_request_ready"] = False

        report = build_sixth_slice_human_acceptance_support_closeout(
            request_packet,
            self.preflight_report,
            accepted_artifact_exists=False,
        )

        self.assertFalse(report["summary"]["support_closeout_complete"])
        self.assertFalse(report["summary"]["ready_for_user_acceptance"])
        self.assertEqual(1, report["summary"]["blocking_issue_count"])
        self.assertEqual("acceptance_request_not_ready", report["issues"][0]["code"])
        self.assertEqual("M57-03-support-repair", report["next_target"]["milestone"])

    def test_real_accepted_artifact_existing_blocks_support_closeout(self):
        report = build_sixth_slice_human_acceptance_support_closeout(
            self.request_packet,
            self.preflight_report,
            accepted_artifact_exists=True,
        )

        self.assertFalse(report["summary"]["support_closeout_complete"])
        self.assertEqual(1, report["summary"]["blocking_issue_count"])
        self.assertEqual("real_m57_03_artifact_already_exists", report["issues"][0]["code"])
        self.assertTrue(report["evidence"]["human_acceptance_recorded_anywhere"])

    def test_preflight_blocker_blocks_closeout(self):
        preflight_report = json.loads(json.dumps(self.preflight_report, ensure_ascii=False))
        preflight_report["summary"]["blocking_issue_count"] = 1

        report = build_sixth_slice_human_acceptance_support_closeout(
            self.request_packet,
            preflight_report,
            accepted_artifact_exists=False,
        )

        self.assertFalse(report["summary"]["support_closeout_complete"])
        self.assertEqual(1, report["summary"]["blocking_issue_count"])
        self.assertEqual("preflight_has_blockers", report["issues"][0]["code"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "closeout.json"
            md_path = out / "closeout.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M57-03-acceptance-support-closeout", loaded["version"])
            self.assertTrue(loaded["summary"]["support_closeout_complete"])
            self.assertIn("M57-03 Sixth-Slice Human Acceptance Support Closeout", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
