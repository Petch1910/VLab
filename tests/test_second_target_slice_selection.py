"""Tests for tools/deck/select_second_target_slice.py (M35-E1)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.select_first_target_slice import (  # noqa: E402
    ARCHETYPE_PRIORITY_JSON,
    SLICE_CONFIGS,
    load_deck_possibility,
)
from tools.deck.select_second_target_slice import (  # noqa: E402
    build_report,
    completed_groups_from_first_slice,
    select_second_candidate,
    write_json,
    write_markdown,
)


class TestSecondTargetSliceSelection(unittest.TestCase):
    def setUp(self) -> None:
        self.report = build_report("classic_core")

    def test_excludes_first_closed_slice_group(self):
        previous_group = self.report["previous_target"]["group"]
        selected_group = self.report["selected_target"]["group"]

        self.assertIn(previous_group, self.report["selection_policy"]["excluded_completed_groups"])
        self.assertNotEqual(previous_group, selected_group)

    def test_selects_highest_ranked_remaining_classic_candidate(self):
        priority = json.loads(ARCHETYPE_PRIORITY_JSON.read_text(encoding="utf-8"))
        deck_report = load_deck_possibility("classic_part1")
        config = SLICE_CONFIGS["classic_core"]
        excluded = self.report["selection_policy"]["excluded_completed_groups"]

        selected = select_second_candidate(priority["rankings"], deck_report, config, excluded)
        remaining = [
            row
            for row in priority["rankings"]
            if row["feasible"]
            and row["best_era"] == "classic_part1"
            and row["group"] not in excluded
        ]

        self.assertGreater(len(remaining), 0)
        self.assertEqual(selected["rank"], min(row["rank"] for row in remaining))
        self.assertEqual(self.report["selected_target"]["rank"], selected["rank"])

    def test_report_is_m35_e1_and_points_to_m35_e2(self):
        self.assertEqual("M35-E1", self.report["version"])
        self.assertEqual("M35-E2", self.report["next_target"]["milestone"])
        self.assertIn("selected second-slice source-backed deck fixture", self.report["next_target"]["must_create"])

    def test_previous_d4_counts_are_carried_forward(self):
        self.assertEqual(1, self.report["previous_target"]["d4_seed_entries"])
        self.assertEqual(24, self.report["previous_target"]["d4_rejected_lines"])

    def test_runtime_boundary_blocks_runtime_promotion(self):
        boundary = self.report["runtime_boundary"]

        self.assertTrue(boundary["advisory_selection_only"])
        self.assertTrue(boundary["does_not_create_deck"])
        self.assertTrue(boundary["does_not_mutate_runtime_pack"])
        self.assertTrue(boundary["does_not_publish_to_bot"])

    def test_selected_group_exists_in_deck_possibility_report(self):
        deck_report = load_deck_possibility("classic_part1")
        groups = {group["group"] for group in deck_report["groups"]}

        self.assertIn(self.report["selected_target"]["group"], groups)
        self.assertEqual("Classic Core", self.report["selected_target"]["slice"])
        self.assertEqual("classic_part1", self.report["selected_target"]["era_preset"])

    def test_completed_groups_deduplicates_a2_and_d4_sources(self):
        first = {"selected_target": {"group": "same-group"}}
        closeout = {"selected_target": {"group": "same-group"}}

        self.assertEqual(["same-group"], completed_groups_from_first_slice(first, closeout))

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "slice.json"
            md_path = out / "slice.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual(self.report["selected_target"]["group"], loaded["selected_target"]["group"])
            self.assertIn("M35-E1 Second Target Slice Report", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
