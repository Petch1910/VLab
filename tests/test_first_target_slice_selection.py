"""Tests for tools/deck/select_first_target_slice.py (M35-A2)."""

from __future__ import annotations

import json
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.select_first_target_slice import (  # noqa: E402
    ARCHETYPE_PRIORITY_JSON,
    build_report,
    select_candidate,
    load_deck_possibility,
    SLICE_CONFIGS,
)


class TestFirstTargetSliceSelection(unittest.TestCase):
    def test_classic_core_selects_highest_ranked_feasible_classic_candidate(self):
        priority = json.loads(ARCHETYPE_PRIORITY_JSON.read_text(encoding="utf-8"))
        deck_report = load_deck_possibility("classic_part1")
        config = SLICE_CONFIGS["classic_core"]

        selected = select_candidate(priority["rankings"], deck_report, config)
        classic_candidates = [
            row
            for row in priority["rankings"]
            if row["feasible"] and row["best_era"] == "classic_part1"
        ]

        self.assertGreater(len(classic_candidates), 0)
        self.assertEqual(selected["rank"], min(row["rank"] for row in classic_candidates))
        self.assertEqual(selected["best_era"], "classic_part1")

    def test_report_uses_classic_core_policy_not_standard_default(self):
        report = build_report("classic_core")

        self.assertEqual("M35-A2", report["version"])
        self.assertEqual("Classic Core", report["selected_target"]["slice"])
        self.assertEqual("classic_part1", report["selected_target"]["era_preset"])
        self.assertTrue(report["selection_policy"]["do_not_assume_standard_by_default"])

    def test_format_policy_keeps_ride_deck_and_g_zone_out_of_first_slice(self):
        report = build_report("classic_core")
        limits = report["format_policy"]["deck_limits_for_next_fixtures"]

        self.assertEqual(50, limits["main_deck_exact"])
        self.assertEqual(16, limits["trigger_target"])
        self.assertFalse(limits["ride_deck_required"])
        self.assertFalse(limits["g_zone_required"])

    def test_taxonomy_report_has_no_missing_classic_required_terms(self):
        report = build_report("classic_core")
        missing = report["taxonomy_gap_report"]["missing_required_terms"]

        self.assertEqual([], missing)

    def test_report_keeps_next_step_at_legality_fixtures(self):
        report = build_report("classic_core")

        self.assertEqual("M35-A3", report["next_target"]["milestone"])
        self.assertIn("failing trigger-count deck fixture", report["next_target"]["must_create"])


if __name__ == "__main__":
    unittest.main()
