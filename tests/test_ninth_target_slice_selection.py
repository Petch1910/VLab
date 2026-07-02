"""Tests for tools/deck/build_ninth_target_slice_selection.py."""

from __future__ import annotations

import copy
import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

import tests.test_eight_fixture_scale_decision as eight_scale_fixture  # noqa: E402
from tools.deck.build_ninth_target_slice_selection import (  # noqa: E402
    build_ninth_target_slice_selection,
    write_json,
    write_markdown,
)


class TestNinthTargetSliceSelection(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        eight_scale_fixture.TestEightFixtureScaleDecision.setUpClass()
        cls.m66_decision = eight_scale_fixture.TestEightFixtureScaleDecision.report
        cls.report = build_ninth_target_slice_selection(cls.m66_decision)

    def test_selects_first_available_ninth_slice_candidate(self):
        summary = self.report["summary"]
        selected = self.report["selected_target"]
        first_candidate = self.m66_decision["candidate_queue"][0]

        self.assertEqual("M67-01", self.report["version"])
        self.assertTrue(summary["ninth_slice_selected"])
        self.assertEqual(first_candidate["group"], summary["selected_group"])
        self.assertEqual(first_candidate["best_era"], summary["selected_era_preset"])
        self.assertEqual(first_candidate["rank"], summary["selected_rank"])
        self.assertEqual(first_candidate["priority_score"], selected["priority_score"])
        self.assertTrue(summary["ready_for_m67_02"])

    def test_selection_is_offline_only_and_non_mutating(self):
        scope = self.report["scope"]
        decision = self.report["decision"]

        self.assertTrue(scope["offline_target_selection"])
        self.assertFalse(scope["creates_recipe_draft"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["g_zone_runtime_enabled"])
        self.assertFalse(scope["stride_runtime_enabled"])
        self.assertFalse(scope["bloom_token_runtime_enabled"])
        self.assertFalse(scope["lock_runtime_enabled"])
        self.assertFalse(scope["unlock_runtime_enabled"])
        self.assertFalse(scope["legion_runtime_enabled"])
        self.assertFalse(scope["mate_identity_check_enabled"])
        self.assertFalse(scope["GameState_mutation"])
        self.assertTrue(decision["offline_analysis_only"])
        self.assertFalse(decision["live_runtime_deck_enabled"])
        self.assertFalse(decision["saved_deck_enabled"])
        self.assertFalse(decision["ui_deck_list_enabled"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertFalse(decision["g_zone_runtime_enabled"])
        self.assertFalse(decision["stride_runtime_enabled"])
        self.assertFalse(decision["bloom_token_runtime_enabled"])
        self.assertFalse(decision["lock_runtime_enabled"])
        self.assertFalse(decision["unlock_runtime_enabled"])
        self.assertFalse(decision["legion_runtime_enabled"])
        self.assertFalse(decision["mate_identity_check_enabled"])

    def test_selection_uses_candidate_queue_snapshot(self):
        self.assertEqual(self.m66_decision["candidate_queue"], self.report["candidate_queue_snapshot"])
        self.assertGreaterEqual(self.report["summary"]["candidate_count"], 1)

    def test_not_allowed_routes_to_m66_repair(self):
        m66_decision = copy.deepcopy(self.m66_decision)
        m66_decision["summary"]["ready_for_m67"] = False

        report = build_ninth_target_slice_selection(m66_decision)

        self.assertFalse(report["summary"]["ninth_slice_selected"])
        self.assertFalse(report["summary"]["ready_for_m67_02"])
        self.assertEqual("M66-repair", report["next_target"]["milestone"])

    def test_scale_not_allowed_routes_to_m66_repair(self):
        m66_decision = copy.deepcopy(self.m66_decision)
        m66_decision["summary"]["ninth_slice_offline_pipeline_allowed"] = False
        m66_decision["decision"]["ninth_slice_offline_pipeline_allowed"] = False

        report = build_ninth_target_slice_selection(m66_decision)

        self.assertFalse(report["summary"]["ninth_slice_selected"])
        self.assertEqual("M66-repair", report["next_target"]["milestone"])

    def test_empty_candidate_queue_routes_to_m66_repair(self):
        m66_decision = copy.deepcopy(self.m66_decision)
        m66_decision["candidate_queue"] = []

        report = build_ninth_target_slice_selection(m66_decision)

        self.assertFalse(report["summary"]["ninth_slice_selected"])
        self.assertEqual("M66-repair", report["next_target"]["milestone"])

    def test_next_target_is_m67_02(self):
        next_target = self.report["next_target"]

        self.assertEqual("M67-02", next_target["milestone"])
        self.assertEqual("Ninth-slice fixture/format readiness", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m67_01.json"
            md_path = out / "m67_01.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M67-01", loaded["version"])
            self.assertIn("M67-01 Ninth Target Slice Selection", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
