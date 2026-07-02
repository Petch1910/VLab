"""Tests for tools/deck/build_ninth_slice_semantic_compatibility_probe.py."""

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

import tests.test_ninth_slice_fixture_readiness as ninth_readiness_fixture  # noqa: E402
from tools.deck.build_ninth_slice_semantic_compatibility_probe import (  # noqa: E402
    build_ninth_slice_semantic_compatibility_probe,
    write_json,
    write_markdown,
)


class TestNinthSliceSemanticCompatibilityProbe(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        ninth_readiness_fixture.TestNinthSliceFixtureReadiness.setUpClass()
        cls.selection = ninth_readiness_fixture.TestNinthSliceFixtureReadiness.selection
        cls.readiness = ninth_readiness_fixture.TestNinthSliceFixtureReadiness.report
        cls.report = build_ninth_slice_semantic_compatibility_probe(cls.selection, cls.readiness)

    def test_probe_targets_ninth_slice(self):
        selected = self.report["selected_target"]
        summary = self.report["summary"]

        self.assertEqual("M67-03", self.report["version"])
        self.assertEqual(self.selection["selected_target"]["group"], selected["group"])
        self.assertEqual(self.selection["selected_target"]["era_preset"], selected["era_preset"])
        self.assertEqual(selected["group"], summary["selected_group"])
        self.assertEqual(selected["era_preset"], summary["era_preset"])

    def test_normalization_reuses_selected_slice_probe_without_writing_intermediates(self):
        normalization = self.report["normalization"]
        contract = self.report["pipeline_contract"]

        self.assertTrue(normalization["uses_m35_b_c_probe_in_memory"])
        self.assertTrue(normalization["adds_format_policy_for_contract"])
        self.assertTrue(normalization["adds_fixture_policy_for_contract"])
        self.assertTrue(normalization["maps_m67_readiness_to_semantic_scaleout_ready"])
        self.assertTrue(normalization["does_not_write_intermediate_m35_outputs"])
        self.assertTrue(contract["uses_injected_reports_in_memory"])
        self.assertTrue(contract["does_not_require_first_slice_file_paths_for_execution"])

    def test_all_stage_readiness_flags_pass(self):
        readiness = self.report["readiness"]

        self.assertTrue(readiness["ninth_slice_semantic_compatibility_probe_passed"])
        self.assertTrue(readiness["ready_for_m67_04"])
        for key, value in self.report["stage_readiness"].items():
            self.assertTrue(value, key)

    def test_stage_summaries_match_source_backed_probe(self):
        summary = self.report["summary"]
        stages = self.report["stage_summaries"]

        self.assertEqual(77, summary["source_card_count"])
        self.assertEqual(77, summary["semantic_card_count"])
        self.assertEqual(10, summary["manual_review_card_count"])
        self.assertEqual(2708, summary["pair_graph_edge_count"])
        self.assertEqual(95, summary["candidate_edge_count"])
        self.assertEqual(summary["semantic_card_count"], stages["b2_semantic_tags"]["card_count"])
        self.assertEqual(
            summary["semantic_card_count"],
            stages["b2_semantic_tags"]["cards_with_any_semantic_tag"],
        )
        self.assertEqual(70, stages["b3_requirement_provider"]["cards_with_providers"])
        self.assertEqual(summary["candidate_edge_count"], stages["c5_compatibility"]["status_counts"]["synergy"])
        self.assertEqual(650, stages["c5_compatibility"]["status_counts"]["manual_review_required"])
        self.assertEqual(29, stages["c5_compatibility"]["status_counts"]["missing_data"])
        self.assertEqual(1934, stages["c5_compatibility"]["status_counts"]["mixed"])

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
        self.assertFalse(boundary["g_zone_runtime_enabled"])
        self.assertFalse(boundary["stride_runtime_enabled"])
        self.assertFalse(boundary["bloom_token_runtime_enabled"])
        self.assertFalse(boundary["lock_runtime_enabled"])
        self.assertFalse(boundary["unlock_runtime_enabled"])
        self.assertFalse(boundary["legion_runtime_enabled"])
        self.assertFalse(boundary["mate_identity_check_enabled"])
        self.assertFalse(boundary["GameState_mutation"])
        self.assertFalse(readiness["runtime_or_bot_promotion_allowed"])

    def test_failed_readiness_routes_to_repair(self):
        readiness = copy.deepcopy(self.readiness)
        readiness["readiness"]["semantic_probe_ready"] = False
        readiness["readiness"]["all_fixture_expectations_met"] = False

        report = build_ninth_slice_semantic_compatibility_probe(self.selection, readiness)

        self.assertFalse(report["readiness"]["ready_for_m67_04"])
        self.assertEqual("M67-repair-semantic", report["next_target"]["milestone"])

    def test_next_target_is_m67_04(self):
        self.assertEqual("M67-04", self.report["next_target"]["milestone"])
        self.assertEqual("Ninth-slice recipe pipeline entry gate", self.report["next_target"]["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m67_03.json"
            md_path = out / "m67_03.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M67-03", loaded["version"])
            self.assertIn("M67-03 Ninth-Slice Semantic", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
