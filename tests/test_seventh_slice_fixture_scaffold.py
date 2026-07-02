"""Tests for tools/deck/build_seventh_slice_fixture_scaffold.py."""

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

from tests.test_seventh_slice_fixture_readiness import _build_in_memory_m59_01_selection  # noqa: E402
from tools.deck.build_seventh_slice_fixture_readiness import build_seventh_slice_fixture_readiness  # noqa: E402
from tools.deck.build_seventh_slice_fixture_scaffold import (  # noqa: E402
    build_seventh_slice_fixture_scaffold,
    write_json,
    write_markdown,
)
from tools.deck.build_seventh_slice_recipe_pipeline_entry_gate import (  # noqa: E402
    build_seventh_slice_recipe_pipeline_entry_gate,
)
from tools.deck.build_seventh_slice_semantic_compatibility_probe import (  # noqa: E402
    build_seventh_slice_semantic_compatibility_probe,
)


class TestSeventhSliceFixtureScaffold(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.selection = _build_in_memory_m59_01_selection()
        cls.readiness = build_seventh_slice_fixture_readiness(cls.selection)
        cls.probe = build_seventh_slice_semantic_compatibility_probe(cls.selection, cls.readiness)
        cls.gate = build_seventh_slice_recipe_pipeline_entry_gate(cls.readiness, cls.probe)
        cls.report = build_seventh_slice_fixture_scaffold(cls.gate, cls.readiness, cls.probe)

    def test_scaffold_is_ready_for_review_packet(self):
        summary = self.report["summary"]

        self.assertEqual("M60-01", self.report["version"])
        self.assertTrue(summary["scaffold_ready"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertTrue(summary["ready_for_m60_02"])
        self.assertEqual("M60-02", self.report["next_target"]["milestone"])

    def test_fixture_policy_scaffold_is_source_backed(self):
        scaffold = self.report["fixture_scaffold"]

        self.assertEqual("seventh_slice_g_series_first_fixture_scaffold_not_full_official_legality", scaffold["policy_level"])
        self.assertEqual("clan", scaffold["identity_field"])
        self.assertEqual(self.gate["selected_target"]["group"], scaffold["selected_identity"])
        self.assertEqual("g_series_first", scaffold["era_preset"])
        self.assertIn("G-TD03", scaffold["set_scope"])
        self.assertIn("G-BT08", scaffold["set_scope"])
        self.assertEqual(5, len(scaffold["source_series_present"]))
        self.assertEqual(50, scaffold["main_deck_exact"])
        self.assertEqual(16, scaffold["trigger_target"])
        self.assertEqual(["Critical", "Draw", "Heal", "Stand"], scaffold["required_trigger_types"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, scaffold["recommended_trigger_profile"])
        self.assertEqual([0, 1, 2, 3], scaffold["required_non_trigger_setup_grades"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, scaffold["preferred_grade_profile"])

    def test_mechanic_scope_defers_g_zone_runtime_assumptions(self):
        mechanic = self.report["mechanic_scope"]

        self.assertEqual("G Series first Stride era", mechanic["format_family"])
        self.assertFalse(mechanic["runtime_stride_enabled"])
        self.assertTrue(mechanic["g_zone_recipe_validation_deferred"])
        self.assertTrue(mechanic["grade4_cards_present"])
        self.assertTrue(mechanic["grade4_cards_advisory_only_until_g_zone_support"])
        self.assertTrue(mechanic["stride_or_generation_break_text_requires_manual_review"])
        self.assertTrue(mechanic["bloom_or_token_like_text_requires_manual_review"])
        self.assertFalse(mechanic["imaginary_gift_enabled"])
        self.assertFalse(mechanic["ride_deck_enabled"])
        self.assertFalse(mechanic["over_trigger_enabled"])
        self.assertFalse(mechanic["front_trigger_enabled"])
        self.assertFalse(mechanic["order_cards_enabled"])

    def test_validator_contract_lists_required_checks(self):
        contract = self.report["validator_contract_for_m60_04"]

        self.assertTrue(contract["must_validate_main_deck_count"])
        self.assertTrue(contract["must_validate_trigger_count"])
        self.assertTrue(contract["must_validate_required_trigger_types"])
        self.assertTrue(contract["must_validate_required_grade_coverage"])
        self.assertTrue(contract["must_validate_selected_identity"])
        self.assertTrue(contract["must_validate_set_scope"])
        self.assertTrue(contract["must_validate_runtime_deck_limit"])
        self.assertTrue(contract["must_block_missing_cards"])
        self.assertTrue(contract["must_keep_grade4_cards_out_of_main_deck_until_g_zone_support"])
        self.assertTrue(contract["must_not_accept_manual_review_dependencies_as_runtime_ready"])

    def test_evidence_matches_prior_gates(self):
        evidence = self.report["evidence_summary"]

        self.assertEqual(78, evidence["source_card_count"])
        self.assertEqual(5, len(evidence["series_counts"]))
        self.assertEqual({"0": 17, "1": 23, "2": 18, "3": 12, "4": 8}, evidence["grade_counts"])
        self.assertEqual({"": 66, "Critical": 5, "Draw": 2, "Heal": 2, "Stand": 3}, evidence["trigger_counts"])
        self.assertEqual(48, evidence["trigger_capacity"])
        self.assertEqual(264, evidence["non_trigger_capacity"])
        self.assertEqual(107, evidence["candidate_edge_count"])
        self.assertEqual(10, evidence["manual_review_card_count"])

    def test_runtime_boundaries_stay_closed(self):
        boundary = self.report["runtime_boundary"]

        self.assertTrue(boundary["scaffold_artifact_only"])
        self.assertTrue(boundary["does_not_create_deck"])
        self.assertTrue(boundary["does_not_create_recipe_draft"])
        self.assertTrue(boundary["does_not_create_runtime_fixture"])
        self.assertTrue(boundary["does_not_mutate_runtime_pack"])
        self.assertTrue(boundary["does_not_publish_to_ui"])
        self.assertTrue(boundary["does_not_publish_to_bot"])
        self.assertFalse(boundary["g_zone_runtime_enabled"])
        self.assertFalse(boundary["stride_runtime_enabled"])
        self.assertFalse(boundary["GameState_mutation"])

    def test_failed_entry_gate_blocks_scaffold(self):
        gate = copy.deepcopy(self.gate)
        gate["summary"]["ready_for_m60"] = False

        report = build_seventh_slice_fixture_scaffold(gate, self.readiness, self.probe)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["ready_for_m60_02"])
        self.assertIn("entry_gate_not_ready", codes)
        self.assertEqual("M60-repair", report["next_target"]["milestone"])

    def test_missing_trigger_type_blocks_scaffold(self):
        readiness = copy.deepcopy(self.readiness)
        readiness["card_pool_summary"]["trigger_counts"]["Heal"] = 0

        report = build_seventh_slice_fixture_scaffold(self.gate, readiness, self.probe)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["ready_for_m60_02"])
        self.assertIn("missing_required_trigger_types", codes)
        self.assertEqual("M60-repair", report["next_target"]["milestone"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m60_01.json"
            md_path = out / "m60_01.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M60-01", loaded["version"])
            self.assertIn("M60-01 Seventh-Slice Fixture", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
