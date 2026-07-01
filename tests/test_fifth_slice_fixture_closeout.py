"""Tests for tools/deck/build_fifth_slice_fixture_closeout.py (M53-closeout)."""

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

from tools.deck.build_fifth_slice_fixture_closeout import (  # noqa: E402
    build_fifth_slice_fixture_closeout,
    write_json,
    write_markdown,
)
from tools.deck.build_fifth_slice_human_accepted_repair_artifact import (  # noqa: E402
    M52_DRAFTS,
    build_fifth_slice_human_accepted_repair_artifact,
    load_json,
)
from tools.deck.build_fifth_slice_human_selected_recipe_artifact import (  # noqa: E402
    M53_01_REVIEW,
    build_fifth_slice_human_selected_recipe_artifact,
)
from tools.deck.build_fifth_slice_repaired_recipe_validation_rerun import (  # noqa: E402
    build_fifth_slice_repaired_recipe_validation_rerun,
)
from tools.deck.build_fifth_slice_runtime_fixture_promotion_gate import (  # noqa: E402
    build_fifth_slice_runtime_fixture_promotion_gate,
)


SELECTED_REVIEW_ITEM_ID = "m53_01_m52_recipe_001_repair_review"
SELECTION_TEXT = "explicit test selection for m52_recipe_001"
ACCEPTANCE_TEXT = "explicit test acceptance for m52_recipe_001"


class TestFifthSliceFixtureCloseout(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        review_packet = load_json(M53_01_REVIEW)
        drafts = load_json(M52_DRAFTS)
        selected_artifact = build_fifth_slice_human_selected_recipe_artifact(
            review_packet,
            selected_review_item_id=SELECTED_REVIEW_ITEM_ID,
            selection_text=SELECTION_TEXT,
            selected_by="unit-test",
            selected_at="2026-06-30",
        )
        accepted_artifact = build_fifth_slice_human_accepted_repair_artifact(
            selected_artifact,
            drafts,
            acceptance_text=ACCEPTANCE_TEXT,
            accepted_by="unit-test",
            accepted_at="2026-06-30",
        )
        rerun = build_fifth_slice_repaired_recipe_validation_rerun(accepted_artifact)
        cls.gate = build_fifth_slice_runtime_fixture_promotion_gate(rerun)
        cls.report = build_fifth_slice_fixture_closeout(cls.gate)

    def test_closeout_completes_m53_with_fifth_fixture(self):
        summary = self.report["summary"]
        results = self.report["key_results"]

        self.assertEqual("M53-closeout", self.report["version"])
        self.assertTrue(summary["m53_complete"])
        self.assertTrue(summary["fifth_runtime_fixture_available"])
        self.assertEqual("M54", summary["next_queue_id"])
        self.assertTrue(summary["ready_for_next_queue"])
        self.assertEqual("m52_recipe_001", results["recipe_id"])
        self.assertEqual(5, results["gate_passed_check_count"])
        self.assertEqual(0, results["gate_failed_check_count"])

    def test_decision_keeps_live_runtime_ui_and_bot_disabled(self):
        decision = self.report["decision"]
        results = self.report["key_results"]

        self.assertTrue(decision["fifth_recipe_enters_fixture_scope"])
        self.assertFalse(decision["fifth_recipe_remains_advisory_only"])
        self.assertFalse(decision["live_runtime_deck_enabled"])
        self.assertFalse(decision["saved_deck_enabled"])
        self.assertFalse(decision["ui_deck_list_enabled"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertTrue(decision["needs_user_team_review_before_live_deck_ui"])
        self.assertFalse(results["runtime_deck_library_mutated"])
        self.assertFalse(results["saved_deck_injected"])
        self.assertFalse(results["ui_deck_list_published"])
        self.assertFalse(results["bot_playbook_enabled"])

    def test_next_queue_is_explicit_and_bounded(self):
        next_queue = self.report["next_queue"]
        task_ids = [task["id"] for task in next_queue["tasks"]]

        self.assertEqual("M54", next_queue["id"])
        self.assertEqual("Fifth Fixture Consumption and Next-Slice Scale Gate", next_queue["title"])
        self.assertEqual(["M54-01", "M54-02", "M54-03", "M54-04"], task_ids)

    def test_failed_gate_routes_to_repair_queue(self):
        gate = copy.deepcopy(self.gate)
        gate["summary"]["promotion_allowed"] = False
        gate["promotion_decision"]["fixture_created"] = False
        gate["promotion_decision"]["fixture_path"] = ""
        gate["runtime_fixture"] = None

        report = build_fifth_slice_fixture_closeout(gate)

        self.assertFalse(report["summary"]["m53_complete"])
        self.assertEqual("M53-repair", report["summary"]["next_queue_id"])
        self.assertFalse(report["summary"]["ready_for_next_queue"])
        self.assertTrue(report["decision"]["fifth_recipe_remains_advisory_only"])

    def test_scope_does_not_mutate_runtime_ui_or_bot(self):
        scope = self.report["scope"]

        self.assertTrue(scope["closeout_report"])
        self.assertFalse(scope["changes_fixture_artifact"])
        self.assertFalse(scope["mutates_runtime_deck_library"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["automatic_playbook_promotion"])
        self.assertFalse(scope["GameState_mutation"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m53_closeout.json"
            md_path = out / "m53_closeout.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M53-closeout", loaded["version"])
            self.assertIn("M53 Fifth-Slice Fixture Closeout", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
