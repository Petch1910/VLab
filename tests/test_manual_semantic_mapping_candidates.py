"""Tests for tools/deck/build_manual_semantic_mapping_candidates.py (M37-04)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_manual_semantic_mapping_candidates import (  # noqa: E402
    M37_03_TRIAGE,
    build_mapping_candidates,
    load_json,
    write_json,
    write_markdown,
)


class TestManualSemanticMappingCandidates(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = build_mapping_candidates(load_json(M37_03_TRIAGE))

    def test_report_builds_mapping_candidates_from_triage_backlog(self):
        summary = self.report["summary"]

        self.assertEqual("M37-04", self.report["version"])
        self.assertEqual(5, summary["mapping_candidate_count"])
        self.assertEqual(2, summary["structural_mapping_candidate_count"])
        self.assertEqual(1, summary["timing_mapping_candidate_count"])
        self.assertEqual(2, summary["review_only_candidate_count"])
        self.assertEqual(49, summary["line_mapping_link_count"])
        self.assertTrue(summary["ready_for_m37_05"])

    def test_mapping_types_cover_all_triage_groups(self):
        mapping_types = {candidate["mapping_type"] for candidate in self.report["mapping_candidates"]}

        self.assertEqual(
            {
                "resource_requirement_provider_mapping",
                "zone_target_requirement_provider_mapping",
                "timing_window_specificity_mapping",
                "human_acceptance_without_new_mapping",
                "false_dependency_or_acceptance_review",
            },
            mapping_types,
        )

    def test_structural_mapping_candidates_have_expected_requirements(self):
        candidates = {candidate["mapping_type"]: candidate for candidate in self.report["mapping_candidates"]}
        resource = candidates["resource_requirement_provider_mapping"]
        zone = candidates["zone_target_requirement_provider_mapping"]

        self.assertEqual("structural_semantic_mapping", resource["candidate_kind"])
        self.assertEqual(9, resource["line_count"])
        self.assertIn("resource_pressure", resource["abstract_requirements"])
        self.assertIn("resource_recovery_cards", resource["candidate_provider_checks"])
        self.assertEqual("structural_semantic_mapping", zone["candidate_kind"])
        self.assertEqual(15, zone["line_count"])
        self.assertIn("rear_guard_zone_access", zone["abstract_requirements"])
        self.assertIn("call_or_move_to_rear_guard_mapping", zone["candidate_provider_checks"])

    def test_timing_mapping_candidate_is_reviewable_not_executable(self):
        candidates = {candidate["mapping_type"]: candidate for candidate in self.report["mapping_candidates"]}
        timing = candidates["timing_window_specificity_mapping"]

        self.assertEqual("timing_semantic_mapping", timing["candidate_kind"])
        self.assertEqual(10, timing["line_count"])
        self.assertIn("specific_timing_window", timing["abstract_requirements"])
        self.assertIn("on_attack_window", timing["candidate_provider_checks"])
        self.assertFalse(timing["executable_schema_change"])

    def test_review_only_candidates_do_not_expand_schema(self):
        candidates = {candidate["mapping_type"]: candidate for candidate in self.report["mapping_candidates"]}
        detector = candidates["human_acceptance_without_new_mapping"]
        dependency = candidates["false_dependency_or_acceptance_review"]

        self.assertEqual("review_only_gate", detector["candidate_kind"])
        self.assertEqual(10, detector["line_count"])
        self.assertEqual("human_review_status", detector["target_schema_area"])
        self.assertEqual("review_only_gate", dependency["candidate_kind"])
        self.assertEqual(5, dependency["line_count"])
        self.assertEqual("dependency_review_status", dependency["target_schema_area"])

    def test_linked_lines_preserve_line_context(self):
        candidates = {candidate["mapping_type"]: candidate for candidate in self.report["mapping_candidates"]}
        resource = candidates["resource_requirement_provider_mapping"]
        linked = {line["line_id"]: line for line in resource["linked_lines"]}

        self.assertIn("line_001", linked)
        self.assertIn("line_025", linked)
        self.assertEqual("BT04-038TH", linked["line_001"]["anchor_card_id"])
        self.assertEqual("P1_resource_and_zone_gap", linked["line_001"]["triage_priority"])
        self.assertIn("resource_pressure_gap", linked["line_001"]["gap_labels"])

    def test_scope_and_policy_are_non_runtime(self):
        scope = self.report["scope"]
        policy = self.report["mapping_policy"]

        self.assertTrue(scope["offline_mapping_candidates"])
        self.assertFalse(scope["changes_ability_schema"])
        self.assertFalse(scope["changes_recipe_draft"])
        self.assertFalse(scope["creates_runtime_deck"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_playbook_promotion"])
        self.assertFalse(scope["live_card_text_parsing"])
        self.assertFalse(policy["executable_schema_change"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertTrue(policy["card_text_not_parsed"])

    def test_next_target_is_m37_05(self):
        next_target = self.report["next_target"]

        self.assertEqual("M37-05", next_target["milestone"])
        self.assertEqual("Revised recipe validation rerun", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m37_04.json"
            md_path = out / "m37_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M37-04", loaded["version"])
            self.assertIn("M37-04 Manual Semantic Mapping Candidates", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
