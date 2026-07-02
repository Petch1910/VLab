"""Tests for tools/deck/build_ninth_slice_fixture_scaffold.py."""

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

import tests.test_ninth_slice_recipe_pipeline_entry_gate as ninth_gate_fixture  # noqa: E402
from tools.deck.build_ninth_slice_fixture_scaffold import (  # noqa: E402
    build_ninth_slice_fixture_scaffold,
    write_json,
    write_markdown,
)


class TestNinthSliceFixtureScaffold(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        ninth_gate_fixture.TestNinthSliceRecipePipelineEntryGate.setUpClass()
        cls.readiness = ninth_gate_fixture.TestNinthSliceRecipePipelineEntryGate.readiness
        cls.probe = ninth_gate_fixture.TestNinthSliceRecipePipelineEntryGate.probe
        cls.gate = ninth_gate_fixture.TestNinthSliceRecipePipelineEntryGate.report
        cls.report = build_ninth_slice_fixture_scaffold(cls.gate, cls.readiness, cls.probe)

    def test_scaffold_is_ready_for_review_packet(self):
        summary = self.report["summary"]

        self.assertEqual("M68-01", self.report["version"])
        self.assertTrue(summary["scaffold_ready"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertTrue(summary["ready_for_m68_02"])
        self.assertEqual("M68-02", self.report["next_target"]["milestone"])

    def test_fixture_policy_scaffold_is_source_backed(self):
        scaffold = self.report["fixture_scaffold"]

        self.assertEqual(
            "ninth_slice_aquaforce_g_series_fixture_scaffold_not_full_official_legality",
            scaffold["policy_level"],
        )
        self.assertEqual("clan", scaffold["identity_field"])
        self.assertEqual(self.gate["selected_target"]["group"], scaffold["selected_identity"])
        self.assertEqual("g_series_first", scaffold["era_preset"])
        self.assertIn("G-TD04", scaffold["set_scope"])
        self.assertIn("G-BT02", scaffold["set_scope"])
        self.assertIn("G-CB02", scaffold["set_scope"])
        self.assertEqual(["G-BT02", "G-CB02", "G-TD04"], scaffold["source_series_present"])
        self.assertEqual(50, scaffold["main_deck_exact"])
        self.assertEqual(16, scaffold["trigger_target"])
        self.assertEqual(["Critical", "Draw", "Heal", "Stand"], scaffold["required_trigger_types"])
        self.assertEqual({"Critical": 4, "Draw": 4, "Heal": 4, "Stand": 4}, scaffold["recommended_trigger_profile"])
        self.assertEqual([0, 1, 2, 3], scaffold["required_non_trigger_setup_grades"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, scaffold["preferred_grade_profile"])
        self.assertTrue(scaffold["g_zone_fixture_boundary_required"])
        self.assertTrue(scaffold["grade4_source_cards_reserved_for_g_zone_review"])

    def test_mechanic_scope_defers_g_series_runtime_assumptions(self):
        mechanic = self.report["mechanic_scope"]

        self.assertEqual("G-series first / Stride era", mechanic["format_family"])
        self.assertFalse(mechanic["runtime_g_zone_enabled"])
        self.assertFalse(mechanic["runtime_stride_enabled"])
        self.assertTrue(mechanic["g_zone_fixture_boundary_required"])
        self.assertTrue(mechanic["stride_or_g_unit_text_requires_manual_review"])
        self.assertTrue(mechanic["generation_break_text_requires_manual_review"])
        self.assertTrue(mechanic["aqua_force_attack_count_text_requires_manual_review"])
        self.assertTrue(mechanic["grade4_cards_present"])
        self.assertTrue(mechanic["grade4_cards_g_zone_advisory_only_until_format_support"])
        self.assertFalse(mechanic["imaginary_gift_enabled"])
        self.assertFalse(mechanic["ride_deck_enabled"])
        self.assertFalse(mechanic["over_trigger_enabled"])
        self.assertFalse(mechanic["front_trigger_enabled"])
        self.assertFalse(mechanic["order_cards_enabled"])
        self.assertFalse(mechanic["bloom_token_runtime_enabled"])
        self.assertFalse(mechanic["lock_runtime_enabled"])
        self.assertFalse(mechanic["unlock_runtime_enabled"])
        self.assertFalse(mechanic["legion_runtime_enabled"])
        self.assertFalse(mechanic["mate_identity_check_enabled"])

    def test_validator_contract_lists_required_checks(self):
        contract = self.report["validator_contract_for_m68_04"]

        self.assertTrue(contract["must_validate_main_deck_count"])
        self.assertTrue(contract["must_validate_trigger_count"])
        self.assertTrue(contract["must_validate_required_trigger_types"])
        self.assertTrue(contract["must_validate_required_grade_coverage"])
        self.assertTrue(contract["must_validate_selected_identity"])
        self.assertTrue(contract["must_validate_set_scope"])
        self.assertTrue(contract["must_validate_runtime_deck_limit"])
        self.assertTrue(contract["must_block_missing_cards"])
        self.assertTrue(contract["must_keep_grade4_cards_out_of_main_deck_until_format_support"])
        self.assertTrue(contract["must_keep_g_zone_cards_in_review_boundary"])
        self.assertTrue(contract["must_not_accept_manual_review_dependencies_as_runtime_ready"])

    def test_evidence_matches_prior_gates(self):
        evidence = self.report["evidence_summary"]

        self.assertEqual(77, evidence["source_card_count"])
        self.assertEqual({"G-BT02": 18, "G-CB02": 40, "G-TD04": 19}, evidence["series_counts"])
        self.assertEqual({"0": 18, "1": 23, "2": 18, "3": 13, "4": 5}, evidence["grade_counts"])
        self.assertEqual({"": 63, "Critical": 6, "Draw": 3, "Heal": 2, "Stand": 3}, evidence["trigger_counts"])
        self.assertEqual(56, evidence["trigger_capacity"])
        self.assertEqual(252, evidence["non_trigger_capacity"])
        self.assertEqual(95, evidence["candidate_edge_count"])
        self.assertEqual(10, evidence["manual_review_card_count"])
        self.assertTrue(evidence["g_zone_fixture_boundary_required"])

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
        self.assertFalse(boundary["bloom_token_runtime_enabled"])
        self.assertFalse(boundary["lock_runtime_enabled"])
        self.assertFalse(boundary["unlock_runtime_enabled"])
        self.assertFalse(boundary["legion_runtime_enabled"])
        self.assertFalse(boundary["mate_identity_check_enabled"])
        self.assertFalse(boundary["GameState_mutation"])

    def test_failed_entry_gate_blocks_scaffold(self):
        gate = copy.deepcopy(self.gate)
        gate["summary"]["ready_for_m68"] = False

        report = build_ninth_slice_fixture_scaffold(gate, self.readiness, self.probe)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["ready_for_m68_02"])
        self.assertIn("entry_gate_not_ready", codes)
        self.assertEqual("M68-repair", report["next_target"]["milestone"])

    def test_missing_trigger_type_blocks_scaffold(self):
        readiness = copy.deepcopy(self.readiness)
        readiness["card_pool_summary"]["trigger_counts"]["Heal"] = 0

        report = build_ninth_slice_fixture_scaffold(self.gate, readiness, self.probe)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["ready_for_m68_02"])
        self.assertIn("missing_required_trigger_types", codes)
        self.assertEqual("M68-repair", report["next_target"]["milestone"])

    def test_missing_g_unit_pool_blocks_scaffold(self):
        readiness = copy.deepcopy(self.readiness)
        readiness["card_pool_summary"]["grade_counts"]["4"] = 0

        report = build_ninth_slice_fixture_scaffold(self.gate, readiness, self.probe)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["ready_for_m68_02"])
        self.assertIn("missing_g_unit_pool", codes)
        self.assertEqual("M68-repair", report["next_target"]["milestone"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m68_01.json"
            md_path = out / "m68_01.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M68-01", loaded["version"])
            self.assertIn("M68-01 Ninth-Slice Fixture", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
