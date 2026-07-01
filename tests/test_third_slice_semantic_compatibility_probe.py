"""Tests for tools/deck/build_third_slice_semantic_compatibility_probe.py."""

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

from tools.deck.build_third_slice_semantic_compatibility_probe import (  # noqa: E402
    M43_01_SELECTION,
    M43_02_READINESS,
    build_third_slice_semantic_compatibility_probe,
    load_json,
    write_json,
    write_markdown,
)


class TestThirdSliceSemanticCompatibilityProbe(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.selection = load_json(M43_01_SELECTION)
        cls.readiness = load_json(M43_02_READINESS)
        cls.report = build_third_slice_semantic_compatibility_probe(cls.selection, cls.readiness)

    def test_probe_targets_third_slice(self):
        selected = self.report["selected_target"]
        summary = self.report["summary"]

        self.assertEqual("M43-03", self.report["version"])
        self.assertEqual(self.selection["selected_target"]["group"], selected["group"])
        self.assertEqual("link_joker_legion_mate", selected["era_preset"])
        self.assertEqual(selected["group"], summary["selected_group"])
        self.assertEqual("link_joker_legion_mate", summary["era_preset"])

    def test_normalization_reuses_selected_slice_probe_without_writing_intermediates(self):
        normalization = self.report["normalization"]
        contract = self.report["pipeline_contract"]

        self.assertTrue(normalization["uses_m35_b_c_probe_in_memory"])
        self.assertTrue(normalization["adds_format_policy_for_contract"])
        self.assertTrue(normalization["adds_fixture_policy_for_contract"])
        self.assertTrue(normalization["maps_m43_readiness_to_semantic_scaleout_ready"])
        self.assertTrue(normalization["does_not_write_intermediate_m35_outputs"])
        self.assertTrue(contract["uses_injected_reports_in_memory"])
        self.assertTrue(contract["does_not_require_first_slice_file_paths_for_execution"])

    def test_all_stage_readiness_flags_pass(self):
        readiness = self.report["readiness"]

        self.assertTrue(readiness["third_slice_semantic_compatibility_probe_passed"])
        self.assertTrue(readiness["ready_for_m43_04"])
        for key, value in self.report["stage_readiness"].items():
            self.assertTrue(value, key)

    def test_stage_summaries_match_source_backed_probe(self):
        summary = self.report["summary"]
        stages = self.report["stage_summaries"]

        self.assertEqual(127, summary["semantic_card_count"])
        self.assertEqual(61, summary["manual_review_card_count"])
        self.assertEqual(4835, summary["pair_graph_edge_count"])
        self.assertEqual(109, summary["candidate_edge_count"])
        self.assertEqual(127, stages["b2_semantic_tags"]["cards_with_any_semantic_tag"])
        self.assertEqual(112, stages["b3_requirement_provider"]["cards_with_providers"])
        self.assertEqual(3788, stages["c5_compatibility"]["status_counts"]["manual_review_required"])
        self.assertEqual(109, stages["c5_compatibility"]["status_counts"]["synergy"])

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

    def test_failed_readiness_routes_to_repair(self):
        readiness = copy.deepcopy(self.readiness)
        readiness["readiness"]["semantic_probe_ready"] = False
        readiness["readiness"]["all_fixture_expectations_met"] = False

        report = build_third_slice_semantic_compatibility_probe(self.selection, readiness)

        self.assertFalse(report["readiness"]["ready_for_m43_04"])
        self.assertEqual("M43-repair", report["next_target"]["milestone"])

    def test_next_target_is_m43_04(self):
        self.assertEqual("M43-04", self.report["next_target"]["milestone"])
        self.assertEqual("Third-slice recipe pipeline entry gate", self.report["next_target"]["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m43_03.json"
            md_path = out / "m43_03.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M43-03", loaded["version"])
            self.assertIn("M43-03 Third-Slice Semantic", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
