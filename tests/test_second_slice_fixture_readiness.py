"""Tests for tools/deck/build_second_slice_fixture_readiness.py (M35-E2)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_second_slice_fixture_readiness import (  # noqa: E402
    build_readiness_report,
    load_second_slice_report,
    rewrite_fixture_ids,
    write_json,
    write_markdown,
)


class TestSecondSliceFixtureReadiness(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.second_slice_report = load_second_slice_report()
        cls.report = build_readiness_report(cls.second_slice_report)

    def test_all_fixture_expectations_are_met(self):
        readiness = self.report["readiness"]

        self.assertTrue(readiness["all_fixture_expectations_met"])
        self.assertTrue(readiness["selected_group_fixture_ready"])
        self.assertTrue(readiness["classic_core_policy_reusable"])
        self.assertTrue(readiness["semantic_scaleout_ready"])

    def test_runtime_or_bot_promotion_stays_blocked(self):
        readiness = self.report["readiness"]
        boundary = self.report["runtime_boundary"]

        self.assertFalse(readiness["runtime_or_bot_promotion_allowed"])
        self.assertTrue(boundary["offline_fixture_readiness_only"])
        self.assertTrue(boundary["does_not_create_deck"])
        self.assertTrue(boundary["does_not_mutate_runtime_pack"])
        self.assertTrue(boundary["does_not_publish_to_bot"])

    def test_selected_target_matches_m35_e1(self):
        self.assertEqual("M35-E2", self.report["version"])
        self.assertEqual(
            self.second_slice_report["selected_target"]["group"],
            self.report["selected_target"]["group"],
        )
        self.assertEqual("Classic Core", self.report["selected_target"]["slice"])
        self.assertEqual("classic_part1", self.report["selected_target"]["era_preset"])

    def test_fixture_ids_are_second_slice_scoped(self):
        fixture_ids = [fixture["fixture_id"] for fixture in self.report["fixtures"]]

        self.assertTrue(all("second_slice" in fixture_id for fixture_id in fixture_ids))
        self.assertIn("classic_core_second_slice_valid_minimal", fixture_ids)

    def test_invalid_fixtures_reject_for_expected_reason_prefixes(self):
        for fixture in self.report["fixtures"]:
            if fixture["expected"] != "fail":
                continue
            reasons = fixture["validation"]["reasons"]
            self.assertFalse(fixture["validation"]["accepted"], fixture["fixture_id"])
            for expected_prefix in fixture["expected_reasons"]:
                self.assertTrue(
                    any(reason.startswith(expected_prefix) for reason in reasons),
                    f"{fixture['fixture_id']} missing {expected_prefix} in {reasons}",
                )

    def test_policy_reuse_decision_is_explicit(self):
        policy = self.report["fixture_policy"]

        self.assertEqual("reuse_classic_core_policy_for_second_slice", policy["policy_reuse_decision"])
        self.assertFalse(policy["new_format_or_mechanic_fixtures_required"])
        self.assertEqual(50, policy["main_deck_exact"])
        self.assertEqual(16, policy["trigger_target"])

    def test_rewrite_fixture_ids_is_non_mutating(self):
        original = [{"fixture_id": "classic_core_selected_group_valid_minimal"}]
        rewritten = rewrite_fixture_ids(original)

        self.assertEqual("classic_core_selected_group_valid_minimal", original[0]["fixture_id"])
        self.assertEqual("classic_core_second_slice_valid_minimal", rewritten[0]["fixture_id"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "readiness.json"
            md_path = out / "readiness.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M35-E2", loaded["version"])
            self.assertIn("M35-E2 Second Slice Fixture Readiness", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
