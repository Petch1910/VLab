"""Tests for tools/deck/build_selected_slice_semantic_compatibility_probe.py (M35-E3)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_selected_slice_semantic_compatibility_probe import (  # noqa: E402
    build_probe,
    load_json,
    SECOND_SLICE_REPORT,
    SECOND_SLICE_READINESS,
    write_json,
    write_markdown,
)


class TestSelectedSliceSemanticCompatibilityProbe(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.selected_report = load_json(SECOND_SLICE_REPORT)
        cls.readiness_report = load_json(SECOND_SLICE_READINESS)
        cls.report = build_probe(cls.selected_report, cls.readiness_report)

    def test_probe_targets_second_slice(self):
        self.assertEqual("M35-E3", self.report["version"])
        self.assertEqual(
            self.selected_report["selected_target"]["group"],
            self.report["selected_target"]["group"],
        )
        self.assertNotEqual(
            self.readiness_report["previous_target"]["group"],
            self.report["selected_target"]["group"],
        )

    def test_all_stage_readiness_flags_pass(self):
        self.assertTrue(self.report["readiness"]["generalized_selected_slice_contract_ready"])
        self.assertTrue(self.report["readiness"]["second_slice_semantic_compatibility_probe_passed"])
        for key, value in self.report["stage_readiness"].items():
            self.assertTrue(value, key)

    def test_stage_summaries_are_non_empty(self):
        summaries = self.report["stage_summaries"]

        self.assertGreater(summaries["b2_semantic_tags"]["card_count"], 0)
        self.assertGreater(summaries["b2_semantic_tags"]["cards_with_any_semantic_tag"], 0)
        self.assertGreater(summaries["b3_requirement_provider"]["cards_with_providers"], 0)
        self.assertGreater(summaries["c1_pair_graph"]["edge_count"], 0)
        self.assertGreater(summaries["c5_compatibility"]["edge_count"], 0)
        self.assertGreater(summaries["c5_compatibility"]["m35_d1_candidate_edge_count"], 0)

    def test_contract_uses_injected_reports(self):
        contract = self.report["pipeline_contract"]

        self.assertTrue(contract["uses_injected_reports_in_memory"])
        self.assertTrue(contract["does_not_require_first_slice_file_paths_for_execution"])
        self.assertIn("selected_target", contract["selected_report_required_keys"])
        self.assertIn("readiness", contract["readiness_report_required_keys"])

    def test_runtime_or_bot_promotion_stays_blocked(self):
        boundary = self.report["runtime_boundary"]
        readiness = self.report["readiness"]

        self.assertTrue(boundary["advisory_probe_only"])
        self.assertTrue(boundary["does_not_create_deck"])
        self.assertTrue(boundary["does_not_mutate_runtime_pack"])
        self.assertTrue(boundary["does_not_publish_to_bot"])
        self.assertTrue(boundary["does_not_publish_playbook_seed"])
        self.assertFalse(readiness["runtime_or_bot_promotion_allowed"])

    def test_next_target_is_bot_gate(self):
        self.assertEqual("M35-E4", self.report["next_target"]["milestone"])
        self.assertIn("masked-state requirement audit", self.report["next_target"]["must_create"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "probe.json"
            md_path = out / "probe.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M35-E3", loaded["version"])
            self.assertIn("M35-E3 Generalized Semantic", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
