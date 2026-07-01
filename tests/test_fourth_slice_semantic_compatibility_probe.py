"""Tests for tools/deck/build_fourth_slice_semantic_compatibility_probe.py."""

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

from tools.deck.build_fourth_slice_semantic_compatibility_probe import (  # noqa: E402
    M47_01_SELECTION,
    M47_APPLIED_SCOPE,
    build_fourth_slice_semantic_compatibility_probe,
    load_json,
    write_json,
    write_markdown,
)


class TestFourthSliceSemanticCompatibilityProbe(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.selection = load_json(M47_01_SELECTION)
        cls.applied_scope = load_json(M47_APPLIED_SCOPE)
        cls.report = build_fourth_slice_semantic_compatibility_probe(cls.selection, cls.applied_scope)

    def test_probe_targets_fourth_slice_applied_scope(self):
        selected = self.report["selected_target"]
        summary = self.report["summary"]

        self.assertEqual("M47-03", self.report["version"])
        self.assertEqual(self.selection["selected_target"]["group"], selected["group"])
        self.assertEqual("g_series_first", selected["era_preset"])
        self.assertEqual("g_era_heal_expansion", summary["applied_expansion_id"])
        self.assertEqual(32, summary["effective_series_count"])
        self.assertEqual(190, summary["source_card_count"])

    def test_applied_scope_drives_semantic_card_count(self):
        summary = self.report["summary"]
        stages = self.report["stage_summaries"]

        self.assertEqual(190, summary["semantic_card_count"])
        self.assertEqual(15, summary["manual_review_card_count"])
        self.assertEqual(14150, summary["pair_graph_edge_count"])
        self.assertEqual(785, summary["candidate_edge_count"])
        self.assertEqual(190, stages["b2_semantic_tags"]["cards_with_any_semantic_tag"])
        self.assertEqual(172, stages["b3_requirement_provider"]["cards_with_providers"])
        self.assertEqual(1979, stages["c5_compatibility"]["status_counts"]["manual_review_required"])
        self.assertEqual(785, stages["c5_compatibility"]["status_counts"]["synergy"])

    def test_normalization_reuses_b_c_probe_without_writing_intermediates(self):
        normalization = self.report["normalization"]
        contract = self.report["pipeline_contract"]

        self.assertTrue(normalization["uses_m35_b_c_probe_in_memory"])
        self.assertTrue(normalization["uses_m47_applied_scope_artifact"])
        self.assertTrue(normalization["adds_format_policy_for_contract"])
        self.assertTrue(normalization["adds_fixture_policy_for_contract"])
        self.assertTrue(normalization["maps_m47_applied_scope_to_semantic_scaleout_ready"])
        self.assertTrue(normalization["does_not_write_intermediate_m35_outputs"])
        self.assertTrue(contract["uses_injected_reports_in_memory"])
        self.assertTrue(contract["does_not_require_first_slice_file_paths_for_execution"])

    def test_all_stage_readiness_flags_pass(self):
        readiness = self.report["readiness"]

        self.assertTrue(readiness["fourth_slice_semantic_compatibility_probe_passed"])
        self.assertTrue(readiness["ready_for_m47_04"])
        for key, value in self.report["stage_readiness"].items():
            self.assertTrue(value, key)

    def test_runtime_or_bot_promotion_stays_blocked(self):
        boundary = self.report["runtime_boundary"]
        readiness = self.report["readiness"]

        self.assertTrue(boundary["advisory_probe_only"])
        self.assertTrue(boundary["does_not_create_deck"])
        self.assertTrue(boundary["does_not_create_runtime_fixture"])
        self.assertTrue(boundary["does_not_mutate_runtime_pack"])
        self.assertTrue(boundary["does_not_publish_to_ui"])
        self.assertTrue(boundary["does_not_publish_to_bot"])
        self.assertTrue(boundary["does_not_publish_playbook_seed"])
        self.assertFalse(boundary["GameState_mutation"])
        self.assertFalse(readiness["runtime_or_bot_promotion_allowed"])

    def test_failed_applied_scope_readiness_routes_to_repair(self):
        applied_scope = copy.deepcopy(self.applied_scope)
        applied_scope["readiness"]["ready_for_m47_03"] = False
        applied_scope["readiness"]["semantic_probe_ready"] = False

        report = build_fourth_slice_semantic_compatibility_probe(self.selection, applied_scope)

        self.assertFalse(report["readiness"]["ready_for_m47_04"])
        self.assertEqual("M47-repair-semantic", report["next_target"]["milestone"])

    def test_next_target_is_m47_04(self):
        self.assertEqual("M47-04", self.report["next_target"]["milestone"])
        self.assertEqual("Fourth-slice recipe pipeline entry gate", self.report["next_target"]["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m47_03.json"
            md_path = out / "m47_03.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M47-03", loaded["version"])
            self.assertIn("M47-03 Fourth-Slice Semantic", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
