"""Tests for tools/deck/build_second_slice_readiness_comparison.py (M36-05)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_second_slice_readiness_comparison import (  # noqa: E402
    E2_READINESS,
    E3_PROBE,
    M35_CLOSEOUT,
    M36_CONSISTENCY,
    M36_VALIDATION,
    build_comparison_report,
    load_json,
    write_json,
    write_markdown,
)


class TestSecondSliceReadinessComparison(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = build_comparison_report(
            load_json(M35_CLOSEOUT),
            load_json(E2_READINESS),
            load_json(E3_PROBE),
            load_json(M36_VALIDATION),
            load_json(M36_CONSISTENCY),
        )

    def test_comparison_ready_for_closeout(self):
        readiness = self.report["readiness"]

        self.assertEqual("M36-05", self.report["version"])
        self.assertTrue(readiness["ready_for_m36_closeout"])
        self.assertTrue(readiness["second_slice_ready_for_future_recipe_pipeline"])
        self.assertFalse(readiness["broader_scaleout_runtime_allowed"])

    def test_first_slice_blockers_are_visible(self):
        first = self.report["first_slice_status"]

        self.assertEqual(604, first["clean_candidate_edges"])
        self.assertEqual(0, first["runtime_ready_recipe_count"])
        self.assertEqual(0, first["promotion_allowed_count"])
        self.assertEqual(24, first["blocked_by_review_count"])
        self.assertEqual(16, first["slot_gap_recipe_count"])

    def test_second_slice_probe_metrics_are_visible(self):
        second = self.report["second_slice_status"]

        self.assertTrue(second["fixture_expectations_met"])
        self.assertTrue(second["classic_core_policy_reusable"])
        self.assertTrue(second["semantic_probe_ready"])
        self.assertEqual(103, second["probe_card_count"])
        self.assertEqual(2660, second["probe_edge_count"])
        self.assertEqual(259, second["probe_candidate_edges"])
        self.assertEqual(7, second["manual_review_count"])
        self.assertFalse(second["runtime_or_bot_promotion_allowed"])

    def test_recommendation_holds_runtime_scaleout(self):
        comparison = self.report["comparison"]

        self.assertEqual(0.429, comparison["second_candidate_edge_ratio_vs_first"])
        self.assertFalse(comparison["first_slice_has_runtime_ready_recipe"])
        self.assertFalse(comparison["first_slice_has_promotable_combo_line"])
        self.assertEqual(
            "second_slice_semantic_ready_but_hold_recipe_drafting_until_first_slice_review_blockers_clear",
            comparison["recommendation"],
        )

    def test_scope_is_offline_only(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_readiness_comparison"])
        self.assertFalse(scope["starts_second_slice_recipe_pipeline"])
        self.assertFalse(scope["runtime_deck"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_deck_injection"])

    def test_next_target_is_m36_closeout(self):
        next_target = self.report["next_target"]

        self.assertEqual("M36-closeout", next_target["milestone"])
        self.assertEqual("Deck recipe validation closeout", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "comparison.json"
            md_path = out / "comparison.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M36-05", loaded["version"])
            self.assertIn("M36-05 Second-Slice Readiness Comparison", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
