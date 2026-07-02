"""Tests for tools/deck/build_seventh_slice_human_accepted_repair_artifact.py (M61-03)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tests.test_seventh_slice_fixture_readiness import _build_in_memory_m59_01_selection  # noqa: E402
from tools.deck.build_seventh_slice_blocker_repair_candidates import (  # noqa: E402
    build_seventh_slice_blocker_repair_candidates,
)
from tools.deck.build_seventh_slice_fixture_readiness import build_seventh_slice_fixture_readiness  # noqa: E402
from tools.deck.build_seventh_slice_fixture_scaffold import build_seventh_slice_fixture_scaffold  # noqa: E402
from tools.deck.build_seventh_slice_human_accepted_repair_artifact import (  # noqa: E402
    build_seventh_slice_human_accepted_repair_artifact,
    write_json,
    write_markdown,
)
from tools.deck.build_seventh_slice_human_repair_review_packet import (  # noqa: E402
    build_seventh_slice_human_repair_review_packet,
)
from tools.deck.build_seventh_slice_human_selected_recipe_artifact import (  # noqa: E402
    build_seventh_slice_human_selected_recipe_artifact,
)
from tools.deck.build_seventh_slice_recipe_draft_model import build_seventh_slice_recipe_drafts  # noqa: E402
from tools.deck.build_seventh_slice_recipe_pipeline_entry_gate import (  # noqa: E402
    build_seventh_slice_recipe_pipeline_entry_gate,
)
from tools.deck.build_seventh_slice_review_packet import build_seventh_slice_review_packet  # noqa: E402
from tools.deck.build_seventh_slice_runtime_readiness_closeout import (  # noqa: E402
    build_seventh_slice_runtime_readiness_closeout,
)
from tools.deck.build_seventh_slice_semantic_compatibility_probe import (  # noqa: E402
    build_seventh_slice_semantic_compatibility_probe,
)
from tools.deck.check_seventh_slice_combo_recipe_consistency import (  # noqa: E402
    build_seventh_slice_consistency_report,
)
from tools.deck.validate_seventh_slice_recipe_drafts import (  # noqa: E402
    _all_card_ids,
    build_seventh_slice_validation_report,
    load_card_rows,
)


SELECTED_REVIEW_ITEM_ID = "m61_01_m60_recipe_001_repair_review"
SELECTION_TEXT = "explicit test selection for m60_recipe_001"
ACCEPTANCE_TEXT = "explicit test acceptance for m60_recipe_001"


class TestSeventhSliceHumanAcceptedRepairArtifact(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.selection = _build_in_memory_m59_01_selection()
        cls.readiness = build_seventh_slice_fixture_readiness(cls.selection)
        cls.probe = build_seventh_slice_semantic_compatibility_probe(cls.selection, cls.readiness)
        cls.gate = build_seventh_slice_recipe_pipeline_entry_gate(cls.readiness, cls.probe)
        cls.scaffold = build_seventh_slice_fixture_scaffold(cls.gate, cls.readiness, cls.probe)
        cls.packet = build_seventh_slice_review_packet(
            cls.selection,
            cls.readiness,
            cls.probe,
            cls.gate,
            cls.scaffold,
        )
        cls.drafts = build_seventh_slice_recipe_drafts(cls.packet, cls.scaffold)
        cls.card_rows = load_card_rows(_all_card_ids(cls.drafts))
        cls.validation = build_seventh_slice_validation_report(cls.drafts, cls.card_rows)
        cls.consistency = build_seventh_slice_consistency_report(cls.drafts, cls.validation)
        cls.repairs = build_seventh_slice_blocker_repair_candidates(
            cls.drafts,
            cls.validation,
            cls.consistency,
            cls.scaffold,
        )
        cls.closeout = build_seventh_slice_runtime_readiness_closeout(
            cls.scaffold,
            cls.packet,
            cls.drafts,
            cls.validation,
            cls.consistency,
            cls.repairs,
        )
        cls.review_packet = build_seventh_slice_human_repair_review_packet(
            cls.closeout,
            cls.repairs,
            cls.drafts,
        )
        cls.selected_artifact = build_seventh_slice_human_selected_recipe_artifact(
            cls.review_packet,
            selected_review_item_id=SELECTED_REVIEW_ITEM_ID,
            selection_text=SELECTION_TEXT,
            selected_by="unit-test",
            selected_at="2026-07-02",
        )
        cls.report = build_seventh_slice_human_accepted_repair_artifact(
            cls.selected_artifact,
            cls.drafts,
            cls.scaffold,
            acceptance_text=ACCEPTANCE_TEXT,
            accepted_by="unit-test",
            accepted_at="2026-07-02",
        )

    def test_artifact_records_acceptance_for_selected_recipe(self):
        summary = self.report["summary"]
        record = self.report["acceptance_record"]

        self.assertEqual("M61-03", self.report["version"])
        self.assertEqual(SELECTED_REVIEW_ITEM_ID, summary["accepted_review_item_id"])
        self.assertEqual("m60_recipe_001", summary["accepted_recipe_id"])
        self.assertEqual("m60_recipe_001_manual_overlap_pkg_001", summary["accepted_manual_overlap_package_id"])
        self.assertEqual("m60_recipe_001_grade_profile_pkg_001", summary["accepted_source_grade_profile_package_id"])
        self.assertEqual("m60_recipe_001_combined_manual_grade_pkg_001", summary["accepted_combined_repair_package_id"])
        self.assertEqual("m60_recipe_001_g_zone_deferred_pkg_001", summary["accepted_g_zone_package_id"])
        self.assertEqual("m60_recipe_001_bloom_token_deferred_pkg_001", summary["accepted_bloom_token_package_id"])
        self.assertTrue(summary["human_selection_recorded"])
        self.assertTrue(summary["human_acceptance_recorded"])
        self.assertFalse(summary["g_zone_decision_recorded"])
        self.assertFalse(summary["bloom_token_decision_recorded"])
        self.assertEqual("accepted", record["decision"])
        self.assertEqual("unit-test", record["accepted_by"])
        self.assertEqual("2026-07-02", record["accepted_at"])
        self.assertEqual(ACCEPTANCE_TEXT, record["acceptance_text"])

    def test_manual_then_recomputed_grade_repair_is_ready_for_system_decision(self):
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]
        combined = repair["combined_grade_repair_package"]

        self.assertEqual(0, summary["source_grade_package_conflict_count"])
        self.assertTrue(summary["combined_grade_repair_recomputed"])
        self.assertFalse(combined["source_grade_package_directly_applied"])
        self.assertTrue(combined["source_grade_package_recomputed_after_manual_substitution"])
        self.assertEqual("manual_then_recomputed_grade_profile_substitution", combined["repair_type"])
        self.assertEqual(1, len(repair["manual_substitutions"]))
        self.assertEqual(0, len(repair["source_grade_package_conflicts_after_manual"]))
        self.assertEqual(50, summary["main_deck_count_after_repair"])
        self.assertEqual({"0": 17, "1": 14, "2": 11, "3": 8}, summary["grade_counts_after_repair"])
        self.assertEqual(0, summary["repair_application_issue_count"])
        self.assertEqual(4, combined["added_card_count"])
        self.assertEqual(4, combined["removed_card_count"])
        self.assertTrue(combined["complete_candidate"])
        self.assertTrue(summary["ready_for_m61_04"])
        self.assertTrue(repair["ready_for_m61_04_system_decision"])

    def test_g_zone_and_bloom_remain_deferred_and_runtime_stays_blocked(self):
        scope = self.report["scope"]
        policy = self.report["acceptance_policy"]
        summary = self.report["summary"]
        repair = self.report["accepted_repair"]

        self.assertTrue(scope["offline_human_accepted_artifact"])
        self.assertTrue(scope["records_human_selection"])
        self.assertTrue(scope["records_human_acceptance"])
        self.assertFalse(scope["records_g_zone_decision"])
        self.assertFalse(scope["records_bloom_token_decision"])
        self.assertFalse(scope["changes_recipe_draft_file"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["g_zone_runtime"])
        self.assertFalse(scope["stride_runtime"])
        self.assertFalse(scope["bloom_token_runtime"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["direct_GameState_mutation"])
        self.assertFalse(scope["declares_recipe_valid"])
        self.assertTrue(policy["requires_m61_02_selection"])
        self.assertTrue(policy["requires_non_empty_acceptance_text"])
        self.assertTrue(policy["acceptance_is_not_validation"])
        self.assertTrue(policy["acceptance_is_not_g_zone_decision"])
        self.assertTrue(policy["acceptance_is_not_bloom_token_decision"])
        self.assertTrue(policy["source_grade_package_recomputed_after_manual_substitution"])
        self.assertFalse(policy["runtime_promotion_allowed"])
        self.assertTrue(policy["m61_04_must_record_explicit_g_zone_stride_bloom_token_decision"])
        self.assertTrue(policy["m61_05_must_rerun_validation"])
        self.assertTrue(summary["g_zone_deferred"])
        self.assertTrue(summary["bloom_token_deferred"])
        self.assertFalse(summary["runtime_promotion_allowed"])
        self.assertFalse(summary["declares_recipe_valid"])
        self.assertTrue(repair["g_zone_deferred"])
        self.assertTrue(repair["bloom_token_deferred"])
        self.assertGreaterEqual(len(repair["g_zone_future_system_work"]), 1)
        self.assertGreaterEqual(len(repair["bloom_token_future_system_work"]), 1)
        self.assertFalse(repair["runtime_promotion_allowed"])
        self.assertTrue(repair["requires_m61_04_g_zone_stride_bloom_token_decision"])
        self.assertTrue(repair["requires_m61_05_validation"])

    def test_grade_not_needed_selection_can_be_accepted_for_system_decision(self):
        grade_not_needed = next(
            item for item in self.review_packet["review_items"] if item["grade_repair_not_needed"]
        )
        selected = build_seventh_slice_human_selected_recipe_artifact(
            self.review_packet,
            selected_review_item_id=grade_not_needed["review_item_id"],
            selection_text="explicit test selection for grade-not-needed item",
            selected_by="unit-test",
            selected_at="2026-07-02",
        )

        report = build_seventh_slice_human_accepted_repair_artifact(
            selected,
            self.drafts,
            self.scaffold,
            acceptance_text="explicit test acceptance for grade-not-needed item",
            accepted_by="unit-test",
            accepted_at="2026-07-02",
        )

        combined = report["accepted_repair"]["combined_grade_repair_package"]
        self.assertEqual(0, combined["added_card_count"])
        self.assertEqual(0, combined["removed_card_count"])
        self.assertTrue(combined["complete_candidate"])
        self.assertTrue(report["summary"]["ready_for_m61_04"])

    def test_blank_acceptance_text_is_rejected(self):
        with self.assertRaises(ValueError) as context:
            build_seventh_slice_human_accepted_repair_artifact(
                self.selected_artifact,
                self.drafts,
                self.scaffold,
                acceptance_text=" ",
            )

        self.assertIn("non-empty acceptance_text", str(context.exception))

    def test_invalid_selected_recipe_is_rejected(self):
        selected = json.loads(json.dumps(self.selected_artifact))
        selected["selection"]["recipe_id"] = "m60_missing_recipe"

        with self.assertRaises(ValueError) as context:
            build_seventh_slice_human_accepted_repair_artifact(
                selected,
                self.drafts,
                self.scaffold,
                acceptance_text=ACCEPTANCE_TEXT,
            )

        self.assertIn("Recipe not found", str(context.exception))

    def test_next_target_is_m61_04(self):
        next_target = self.report["next_target"]

        self.assertEqual("M61-04", next_target["milestone"])
        self.assertEqual("Seventh-slice G Zone, Stride, and Bloom/token decision artifact", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m61_03.json"
            md_path = out / "m61_03.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M61-03", loaded["version"])
            self.assertEqual("m60_recipe_001", loaded["summary"]["accepted_recipe_id"])
            self.assertIn("M61-03 Seventh-Slice Human-Accepted Repair Artifact", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
