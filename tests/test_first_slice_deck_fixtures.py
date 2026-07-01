"""Tests for tools/deck/build_first_slice_deck_fixtures.py (M35-A3)."""

from __future__ import annotations

import json
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_first_slice_deck_fixtures import (  # noqa: E402
    build_fixtures,
    load_selected_report,
)


class TestFirstSliceDeckFixtures(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.selected_report = load_selected_report()
        cls.report = build_fixtures(cls.selected_report)

    def test_all_fixture_expectations_are_met(self):
        self.assertTrue(self.report["all_expectations_met"])

    def test_valid_fixture_passes(self):
        valid = next(
            fixture
            for fixture in self.report["fixtures"]
            if fixture["fixture_id"] == "classic_core_selected_group_valid_minimal"
        )
        self.assertEqual("pass", valid["expected"])
        self.assertTrue(valid["validation"]["accepted"])
        self.assertEqual(50, valid["validation"]["main_count"])
        self.assertEqual(16, valid["validation"]["trigger_count"])

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

    def test_policy_is_selected_slice_only(self):
        policy = self.report["fixture_policy"]
        self.assertEqual("minimal_selected_slice_only_not_full_official_legality", policy["policy_level"])
        self.assertEqual("runtime SQLite cards.deck_limit", policy["copy_limit_source"])
        self.assertEqual(self.selected_report["selected_target"]["group"], policy["selected_identity"])

    def test_output_file_can_round_trip_after_cli_generation(self):
        path = ROOT / "outputs" / "target_slice" / "m35_a3_first_slice_deck_legality_fixtures.json"
        if path.exists():
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual("M35-A3", data["version"])


if __name__ == "__main__":
    unittest.main()
