"""Tests for tools/deck/rank_archetype_priority.py (M34-03)."""

from __future__ import annotations

import json
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.rank_archetype_priority import (
    ArchetypePriority,
    GroupCombo,
    GroupFeasibility,
    build_rankings,
    compute_priority_score,
    determine_mechanic_tier,
)


class TestComputePriorityScore(unittest.TestCase):
    """Test the composite priority score formula."""

    def test_infeasible_is_zero(self):
        score = compute_priority_score(
            feasible=False, card_count=500, total_candidates=5000, mechanic_tier=1
        )
        self.assertEqual(score, 0.0)

    def test_feasible_positive(self):
        score = compute_priority_score(
            feasible=True, card_count=400, total_candidates=3000, mechanic_tier=2
        )
        self.assertGreater(score, 0.0)

    def test_simpler_tier_higher_score(self):
        s1 = compute_priority_score(True, 300, 2000, mechanic_tier=2)
        s2 = compute_priority_score(True, 300, 2000, mechanic_tier=7)
        self.assertGreater(s1, s2,
                           "Simpler mechanics (tier 2) should score higher than complex (tier 7)")

    def test_more_combos_higher_score(self):
        s1 = compute_priority_score(True, 300, 4000, mechanic_tier=3)
        s2 = compute_priority_score(True, 300, 500, mechanic_tier=3)
        self.assertGreater(s1, s2,
                           "More combo candidates should score higher")

    def test_more_cards_higher_score(self):
        s1 = compute_priority_score(True, 600, 2000, mechanic_tier=3)
        s2 = compute_priority_score(True, 100, 2000, mechanic_tier=3)
        self.assertGreater(s1, s2,
                           "More cards should score higher")

    def test_max_scores_bounded(self):
        score = compute_priority_score(True, 9999, 99999, mechanic_tier=1)
        self.assertLessEqual(score, 100.0)


class TestDetermineMechanicTier(unittest.TestCase):
    """Test mechanic tier determination from combo era data."""

    def test_none_combo_returns_max_tier(self):
        tier, label = determine_mechanic_tier("unknown", None)
        self.assertEqual(tier, 8)

    def test_classic_presence_returns_tier_2(self):
        combo = GroupCombo(
            group="test", group_field="clan",
            total_candidates=100,
            era_candidates={"classic_part1": 50, "g_series_first": 100},
            best_era="g_series_first", best_era_candidates=100,
        )
        tier, label = determine_mechanic_tier("test", combo)
        self.assertEqual(tier, 2, "Should return classic tier when classic has >=10 candidates")

    def test_d_only_returns_tier_7(self):
        combo = GroupCombo(
            group="test", group_field="nation",
            total_candidates=500,
            era_candidates={"d_overdress": 300, "d_willdress": 200},
            best_era="d_overdress", best_era_candidates=300,
        )
        tier, label = determine_mechanic_tier("test", combo)
        self.assertEqual(tier, 7)


class TestBuildRankings(unittest.TestCase):
    """Integration test: build_rankings loads real data and produces valid output."""

    @classmethod
    def setUpClass(cls):
        cls.rankings = build_rankings()

    def test_rankings_not_empty(self):
        self.assertGreater(len(self.rankings), 0)

    def test_ranks_sequential(self):
        for i, r in enumerate(self.rankings):
            self.assertEqual(r.rank, i + 1)

    def test_sorted_by_score_descending(self):
        for i in range(len(self.rankings) - 1):
            self.assertGreaterEqual(
                self.rankings[i].priority_score,
                self.rankings[i + 1].priority_score,
            )

    def test_infeasible_groups_have_zero_score(self):
        for r in self.rankings:
            if not r.feasible:
                self.assertEqual(r.priority_score, 0.0,
                                 f"{r.group} is infeasible but has non-zero score")

    def test_top_group_is_feasible(self):
        top = self.rankings[0]
        self.assertTrue(top.feasible, f"Top ranked group {top.group} should be feasible")

    def test_royal_paladin_in_top_5(self):
        """Royal Paladin is a flagship clan with many cards — should rank high."""
        top5_groups = [r.group for r in self.rankings[:5]]
        self.assertIn("รอยัล พาลาดิน", top5_groups,
                      "Royal Paladin should be in top 5 priority")

    def test_known_infeasible_groups(self):
        """Groups with <50 cards or missing triggers should be infeasible."""
        infeasible = {r.group for r in self.rankings if not r.feasible}
        # These groups have too few cards in the database
        for group in ["ซู", "ดาร์คโซน", "มาร์คเกอร์"]:
            if group in {r.group for r in self.rankings}:
                self.assertIn(group, infeasible,
                              f"{group} should be infeasible (too few cards)")


class TestOutputFiles(unittest.TestCase):
    """Verify output files exist after build_rankings runs."""

    def test_csv_exists(self):
        path = ROOT / "outputs" / "archetype_priority" / "archetype_priority_ranking.csv"
        self.assertTrue(path.exists(), f"CSV not found at {path}")

    def test_json_exists(self):
        path = ROOT / "outputs" / "archetype_priority" / "archetype_priority_ranking.json"
        self.assertTrue(path.exists(), f"JSON not found at {path}")

    def test_json_valid(self):
        path = ROOT / "outputs" / "archetype_priority" / "archetype_priority_ranking.json"
        with open(path, encoding="utf-8") as f:
            data = json.load(f)
        self.assertIn("rankings", data)
        self.assertIn("version", data)
        self.assertEqual(data["version"], "M34-03")
        self.assertGreater(len(data["rankings"]), 0)


if __name__ == "__main__":
    unittest.main()
