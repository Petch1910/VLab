"""Tests for tools/deck/build_rejected_line_support_gap_triage.py (M37-03)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_rejected_line_support_gap_triage import (  # noqa: E402
    M35_COMBO_LINES,
    M36_REVIEW_PACKET,
    M37_02_REPAIR,
    build_support_gap_triage,
    load_json,
    write_json,
    write_markdown,
)


class TestRejectedLineSupportGapTriage(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = build_support_gap_triage(
            load_json(M36_REVIEW_PACKET),
            load_json(M35_COMBO_LINES),
            load_json(M37_02_REPAIR),
        )

    def test_report_triages_all_rejected_lines(self):
        summary = self.report["summary"]

        self.assertEqual("M37-03", self.report["version"])
        self.assertEqual(24, summary["rejected_line_count"])
        self.assertEqual(24, summary["triage_item_count"])
        self.assertEqual(5, summary["gap_group_count"])
        self.assertEqual(5, summary["manual_mapping_backlog_count"])
        self.assertTrue(summary["ready_for_m37_04"])

    def test_gap_label_counts_are_stable(self):
        counts = self.report["summary"]["gap_label_counts"]

        self.assertEqual(10, counts["broad_timing_review"])
        self.assertEqual(10, counts["detector_gap_not_found_manual_review"])
        self.assertEqual(5, counts["no_resource_dependency_manual_review"])
        self.assertEqual(9, counts["resource_pressure_gap"])
        self.assertEqual(15, counts["zone_access_gap"])

    def test_triage_priority_counts_are_stable(self):
        counts = self.report["summary"]["triage_priority_counts"]

        self.assertEqual(7, counts["P1_resource_and_zone_gap"])
        self.assertEqual(10, counts["P2_single_structural_gap"])
        self.assertEqual(7, counts["P3_manual_confirmation"])
        self.assertEqual(19, self.report["summary"]["multi_label_line_count"])

    def test_resource_and_zone_groups_contain_expected_lines(self):
        groups = {group["gap_label"]: group for group in self.report["gap_groups"]}

        self.assertIn("line_001", groups["resource_pressure_gap"]["line_ids"])
        self.assertIn("line_025", groups["resource_pressure_gap"]["line_ids"])
        self.assertIn("line_001", groups["zone_access_gap"]["line_ids"])
        self.assertIn("line_020", groups["zone_access_gap"]["line_ids"])
        self.assertEqual(9, groups["resource_pressure_gap"]["line_count"])
        self.assertEqual(15, groups["zone_access_gap"]["line_count"])

    def test_timing_and_manual_groups_contain_expected_lines(self):
        groups = {group["gap_label"]: group for group in self.report["gap_groups"]}

        self.assertIn("line_004", groups["broad_timing_review"]["line_ids"])
        self.assertIn("line_024", groups["broad_timing_review"]["line_ids"])
        self.assertIn("line_014", groups["detector_gap_not_found_manual_review"]["line_ids"])
        self.assertIn("line_025", groups["no_resource_dependency_manual_review"]["line_ids"])

    def test_triage_items_preserve_combo_context(self):
        items = {item["line_id"]: item for item in self.report["triage_items"]}
        line = items["line_001"]

        self.assertEqual("BT04-038TH", line["anchor_card_id"])
        self.assertTrue(line["anchor_name_th"])
        self.assertEqual("P1_resource_and_zone_gap", line["triage_priority"])
        self.assertEqual(["resource_pressure_gap", "zone_access_gap"], line["gap_labels"])
        self.assertTrue(line["cards_involved"])
        self.assertTrue(line["advisory_only"])

    def test_accepted_seed_repair_context_is_carried_forward(self):
        context = self.report["accepted_seed_repair_context"]

        self.assertEqual("m37_01_pkg_001", context["recommended_package_id"])
        self.assertEqual("balanced_classic", context["recommended_profile_id"])
        self.assertFalse(context["runtime_promotion_allowed"])

    def test_scope_does_not_promote_runtime_or_playbook(self):
        scope = self.report["scope"]
        policy = self.report["triage_policy"]

        self.assertTrue(scope["offline_triage"])
        self.assertFalse(scope["changes_recipe_draft"])
        self.assertFalse(scope["creates_runtime_deck"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_playbook_promotion"])
        self.assertFalse(scope["live_card_text_parsing"])
        self.assertTrue(policy["human_review_required"])
        self.assertFalse(policy["runtime_promotion_allowed"])

    def test_manual_mapping_backlog_has_expected_mapping_types(self):
        mapping_types = {item["mapping_type"] for item in self.report["manual_mapping_backlog"]}

        self.assertIn("resource_requirement_provider_mapping", mapping_types)
        self.assertIn("zone_target_requirement_provider_mapping", mapping_types)
        self.assertIn("timing_window_specificity_mapping", mapping_types)
        self.assertIn("human_acceptance_without_new_mapping", mapping_types)
        self.assertIn("false_dependency_or_acceptance_review", mapping_types)

    def test_next_target_is_m37_04(self):
        next_target = self.report["next_target"]

        self.assertEqual("M37-04", next_target["milestone"])
        self.assertEqual("Manual semantic mapping candidates", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m37_03.json"
            md_path = out / "m37_03.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M37-03", loaded["version"])
            self.assertIn("M37-03 Rejected-Line Support-Gap Triage", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
