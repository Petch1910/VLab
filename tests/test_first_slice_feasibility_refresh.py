"""Tests for tools/deck/refresh_first_slice_feasibility.py (M35-A4)."""

from __future__ import annotations

import json
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.refresh_first_slice_feasibility import build_report  # noqa: E402


class TestFirstSliceFeasibilityRefresh(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = build_report()

    def test_phase_a_ready_for_phase_b(self):
        self.assertTrue(self.report["phase_a_ready_for_phase_b"])
        self.assertEqual("M35-B1", self.report["recommended_next"])

    def test_capacity_taxonomy_and_fixture_gates_are_ready(self):
        self.assertTrue(self.report["capacity"]["ready"])
        self.assertTrue(self.report["taxonomy"]["ready"])
        self.assertTrue(self.report["legality_fixtures"]["ready"])

    def test_no_blocking_gaps_but_deferred_limits_remain(self):
        gate = self.report["missing_rule_gate"]
        self.assertEqual([], gate["blocking_gaps_for_phase_b"])
        self.assertTrue(gate["not_a_full_official_legality_claim"])
        self.assertIn("official heal trigger maximum source fixture", gate["deferred_before_full_legality"])

    def test_expected_failure_coverage_complete(self):
        coverage = self.report["legality_fixtures"]["expected_failure_coverage"]
        for key in [
            "short_main",
            "trigger_count",
            "missing_setup_grade",
            "copy_limit",
            "identity_mismatch",
        ]:
            self.assertTrue(coverage[key], key)

    def test_output_file_can_round_trip_after_cli_generation(self):
        path = ROOT / "outputs" / "target_slice" / "m35_a4_first_slice_feasibility_refresh.json"
        if path.exists():
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual("M35-A4", data["version"])


if __name__ == "__main__":
    unittest.main()
