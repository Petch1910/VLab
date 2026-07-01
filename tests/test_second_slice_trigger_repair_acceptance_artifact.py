"""Tests for tools/deck/build_second_slice_trigger_repair_acceptance_artifact.py."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_second_slice_trigger_repair_acceptance_artifact import (  # noqa: E402
    DEFAULT_ACCEPTED_PACKAGE_ID,
    M41_02_ACCEPTED,
    M41_REPAIR_CANDIDATES,
    build_second_slice_trigger_repair_acceptance_artifact,
    load_json,
    write_json,
    write_markdown,
)


class TestSecondSliceTriggerRepairAcceptanceArtifact(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.accepted = load_json(M41_02_ACCEPTED)
        cls.candidates = load_json(M41_REPAIR_CANDIDATES)
        cls.report = build_second_slice_trigger_repair_acceptance_artifact(
            cls.accepted,
            cls.candidates,
            accepted_package_id=DEFAULT_ACCEPTED_PACKAGE_ID,
            acceptance_text="\u0e07\u0e31\u0e49\u0e19\u0e08\u0e31\u0e14\u0e44\u0e1b",
            accepted_at="2026-06-30",
        )

    def test_artifact_records_balanced_trigger_repair_acceptance(self):
        summary = self.report["summary"]
        record = self.report["acceptance_record"]

        self.assertEqual("M41-repair-accept", self.report["version"])
        self.assertEqual(DEFAULT_ACCEPTED_PACKAGE_ID, summary["accepted_package_id"])
        self.assertEqual("balanced_classic_trigger_restore", summary["accepted_profile_id"])
        self.assertTrue(summary["human_acceptance_recorded"])
        self.assertEqual("accepted", record["decision"])
        self.assertEqual("user", record["accepted_by"])
        self.assertEqual("2026-06-30", record["accepted_at"])
        self.assertEqual("accept_balanced_trigger_profile_repair", record["interpreted_decision"])

    def test_accepted_trigger_repair_preview_has_expected_counts(self):
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]

        self.assertEqual(50, summary["main_deck_count_after_repair"])
        self.assertEqual(0, summary["repair_application_issue_count"])
        self.assertEqual(16, summary["expected_trigger_count_after"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, summary["expected_grade_counts_after"])
        self.assertEqual("m40_recipe_001", repair["recipe_id"])
        self.assertEqual("m41_repair_pkg_001", repair["trigger_repair_package_id"])
        self.assertEqual(4, len(repair["additions"]))
        self.assertEqual(5, len(repair["removals"]))
        self.assertEqual(50, sum(row["quantity"] for row in repair["repaired_quantities"]))
        self.assertTrue(repair["requires_validation_rerun"])
        self.assertTrue(repair["ready_for_validation_rerun"])

    def test_scope_does_not_mutate_or_promote(self):
        scope = self.report["scope"]
        summary = self.report["summary"]

        self.assertTrue(scope["offline_human_accepted_artifact"])
        self.assertTrue(scope["records_human_acceptance"])
        self.assertFalse(scope["changes_previous_artifacts"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertFalse(scope["declares_recipe_valid"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertFalse(summary["declares_recipe_valid"])

    def test_blank_acceptance_blocks_validation_rerun(self):
        report = build_second_slice_trigger_repair_acceptance_artifact(
            self.accepted,
            self.candidates,
            accepted_package_id=DEFAULT_ACCEPTED_PACKAGE_ID,
            acceptance_text="",
            accepted_at="2026-06-30",
        )

        self.assertFalse(report["summary"]["human_acceptance_recorded"])
        self.assertFalse(report["summary"]["ready_for_validation_rerun"])
        self.assertEqual("pending", report["acceptance_record"]["decision"])

    def test_invalid_package_is_rejected(self):
        with self.assertRaises(ValueError):
            build_second_slice_trigger_repair_acceptance_artifact(
                self.accepted,
                self.candidates,
                accepted_package_id="missing-package",
                acceptance_text="\u0e07\u0e31\u0e49\u0e19\u0e08\u0e31\u0e14\u0e44\u0e1b",
                accepted_at="2026-06-30",
            )

    def test_next_target_is_repair_validation(self):
        next_target = self.report["next_target"]

        self.assertEqual("M41-repair-validate", next_target["milestone"])
        self.assertEqual("Second-slice repaired recipe validation rerun after trigger repair", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m41_repair_accept.json"
            md_path = out / "m41_repair_accept.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M41-repair-accept", loaded["version"])
            self.assertIn(
                "M41 Repair Accept Second-Slice Trigger Repair Acceptance Artifact",
                md_path.read_text(encoding="utf-8"),
            )


if __name__ == "__main__":
    unittest.main()
