"""Tests for tools/deck/build_six_fixture_scale_decision.py."""

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

from tools.deck.build_five_fixture_scale_decision import (  # noqa: E402
    ARCHETYPE_RANKING,
    M39_03_SMOKE,
    M42_03_SMOKE,
    M46_03_SMOKE,
    M50_03_SMOKE,
    M54_03_SMOKE,
    load_json,
)
from tools.deck.build_six_fixture_scale_decision import (  # noqa: E402
    build_six_fixture_scale_decision,
    write_json,
    write_markdown,
)
from tools.deck.build_sixth_headless_fixture_load_smoke import (  # noqa: E402
    HEADLESS_RULESET,
    HEADLESS_SEED,
    build_sixth_headless_fixture_load_smoke,
)
from tools.deck.build_sixth_slice_g_zone_stride_decision_artifact import (  # noqa: E402
    build_sixth_slice_g_zone_stride_decision_artifact,
)
from tools.deck.build_sixth_slice_human_accepted_repair_artifact import (  # noqa: E402
    M56_DRAFTS,
    M56_SCAFFOLD,
    build_sixth_slice_human_accepted_repair_artifact,
)
from tools.deck.build_sixth_slice_human_selected_recipe_artifact import (  # noqa: E402
    M57_01_REVIEW,
    build_sixth_slice_human_selected_recipe_artifact,
)
from tools.deck.build_sixth_slice_runtime_fixture_promotion_gate import (  # noqa: E402
    build_sixth_slice_runtime_fixture_promotion_gate,
)
from tools.deck.export_sixth_fixture_deck_text import build_sixth_fixture_deck_text_export  # noqa: E402
from tools.deck.validate_sixth_runtime_fixture_schema import (  # noqa: E402
    EXPECTED_G_ZONE_OPTION,
    build_sixth_runtime_fixture_schema_validation_report,
)
from tools.deck.validate_sixth_slice_repaired_recipe import (  # noqa: E402
    build_sixth_slice_repaired_validation_report,
)


SELECTED_REVIEW_ITEM_ID = "m57_01_m56_recipe_001_repair_review"


def _unity_result_file(tmp: Path, accepted: bool = True) -> Path:
    path = tmp / "unity_result.json"
    path.write_text(
        json.dumps(
            {
                "accepted": accepted,
                "seed": HEADLESS_SEED,
                "ruleset": HEADLESS_RULESET,
                "deck_source": "deck_code",
                "actions_executed": 4,
                "event_count": 4,
            }
        ),
        encoding="utf-8",
    )
    return path


class TestSixFixtureScaleDecision(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.first_smoke = load_json(M39_03_SMOKE)
        cls.second_smoke = load_json(M42_03_SMOKE)
        cls.third_smoke = load_json(M46_03_SMOKE)
        cls.fourth_smoke = load_json(M50_03_SMOKE)
        cls.fifth_smoke = load_json(M54_03_SMOKE)
        cls.ranking = load_json(ARCHETYPE_RANKING)

        review_packet = load_json(M57_01_REVIEW)
        selected_artifact = build_sixth_slice_human_selected_recipe_artifact(
            review_packet,
            selected_review_item_id=SELECTED_REVIEW_ITEM_ID,
            selection_text="explicit test selection",
            selected_by="unit-test",
            selected_at="2026-07-02",
        )
        accepted_artifact = build_sixth_slice_human_accepted_repair_artifact(
            selected_artifact,
            load_json(M56_DRAFTS),
            load_json(M56_SCAFFOLD),
            acceptance_text="explicit test acceptance",
            accepted_by="unit-test",
            accepted_at="2026-07-02",
        )
        g_zone_decision = build_sixth_slice_g_zone_stride_decision_artifact(
            accepted_artifact,
            selected_option=EXPECTED_G_ZONE_OPTION,
        )
        repaired_validation = build_sixth_slice_repaired_validation_report(
            accepted_artifact,
            g_zone_decision,
        )
        gate_report = build_sixth_slice_runtime_fixture_promotion_gate(
            accepted_artifact,
            repaired_validation,
        )
        cls.sixth_fixture = gate_report["runtime_fixture"]
        m58_01_report = build_sixth_runtime_fixture_schema_validation_report(cls.sixth_fixture)
        m58_02_report = build_sixth_fixture_deck_text_export(cls.sixth_fixture, m58_01_report)
        with tempfile.TemporaryDirectory() as tmp:
            tmp_path = Path(tmp)
            unity_result = _unity_result_file(tmp_path)
            unity_replay = tmp_path / "unity_replay.json"
            unity_replay.write_text("{}", encoding="utf-8")
            cls.sixth_smoke = build_sixth_headless_fixture_load_smoke(
                cls.sixth_fixture,
                m58_02_report,
                m58_02_report["_deck_text"],
                unity_result,
                unity_replay,
            )

        cls.report = build_six_fixture_scale_decision(
            cls.first_smoke,
            cls.second_smoke,
            cls.third_smoke,
            cls.fourth_smoke,
            cls.fifth_smoke,
            cls.sixth_smoke,
            cls.ranking,
            cls.sixth_fixture,
        )

    def test_decision_allows_seventh_slice_offline_pipeline(self):
        summary = self.report["summary"]
        decision = self.report["decision"]

        self.assertEqual("M58-04", self.report["version"])
        self.assertEqual(6, summary["fixture_evidence_count"])
        self.assertEqual(6, summary["passed_fixture_count"])
        self.assertEqual(0, summary["failed_fixture_count"])
        self.assertGreaterEqual(summary["candidate_count"], 1)
        self.assertTrue(summary["seventh_slice_offline_pipeline_allowed"])
        self.assertTrue(summary["ready_for_m59"])
        self.assertTrue(decision["seventh_slice_offline_pipeline_allowed"])
        self.assertFalse(decision["seventh_slice_selected_now"])

    def test_fixture_evidence_requires_unity_deck_code_acceptance(self):
        for item in self.report["fixture_evidence"]:
            self.assertTrue(item["offline_load_ready"])
            self.assertTrue(item["deck_code_created"])
            self.assertTrue(item["unity_headless_smoke_passed"])
            self.assertEqual(0, item["blocking_issue_count"])
            self.assertEqual("deck_code", item["deck_source"])
            self.assertEqual(4, item["actions_executed"])
            self.assertEqual(4, item["event_count"])

    def test_sixth_fixture_shadow_paladin_is_counted(self):
        sixth = [item for item in self.report["fixture_evidence"] if item["label"] == "sixth_fixture_shadow_paladin"][0]

        self.assertEqual("runtime_fixture_m56_recipe_001_shadow_paladin_m57_06", sixth["fixture_id"])
        self.assertEqual("m56_recipe_001", sixth["recipe_id"])
        self.assertEqual(50, sixth["main_deck_count"])
        self.assertGreater(sixth["unique_card_count"], 0)
        self.assertEqual(0, sixth["g_zone_count"])
        self.assertTrue(sixth["unity_headless_smoke_passed"])

    def test_candidate_queue_excludes_completed_fixture_groups(self):
        completed = set(self.report["completed_groups"])
        candidate_groups = {candidate["group"] for candidate in self.report["candidate_queue"]}

        self.assertEqual(6, len(completed))
        self.assertTrue(completed.isdisjoint(candidate_groups))

    def test_scope_does_not_mutate_runtime_ui_bot_or_g_zone(self):
        scope = self.report["scope"]
        decision = self.report["decision"]

        self.assertTrue(scope["offline_scale_decision"])
        self.assertFalse(scope["selects_runtime_deck"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["g_zone_runtime_enabled"])
        self.assertFalse(scope["stride_runtime_enabled"])
        self.assertFalse(scope["GameState_mutation"])
        self.assertFalse(decision["live_runtime_deck_enabled"])
        self.assertFalse(decision["saved_deck_enabled"])
        self.assertFalse(decision["ui_deck_list_enabled"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertFalse(decision["g_zone_runtime_enabled"])
        self.assertFalse(decision["stride_runtime_enabled"])

    def test_failed_sixth_fixture_routes_to_repair(self):
        sixth_smoke = copy.deepcopy(self.sixth_smoke)
        sixth_smoke["summary"]["unity_headless_smoke_passed"] = False
        sixth_smoke["unity_headless_result"]["accepted"] = False

        report = build_six_fixture_scale_decision(
            self.first_smoke,
            self.second_smoke,
            self.third_smoke,
            self.fourth_smoke,
            self.fifth_smoke,
            sixth_smoke,
            self.ranking,
            self.sixth_fixture,
        )

        self.assertFalse(report["summary"]["seventh_slice_offline_pipeline_allowed"])
        self.assertFalse(report["summary"]["ready_for_m59"])
        self.assertEqual("M58-repair", report["next_target"]["milestone"])

    def test_next_target_is_m59_01(self):
        next_target = self.report["next_target"]

        self.assertEqual("M59-01", next_target["milestone"])
        self.assertEqual("Seventh target slice selection", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m58_04.json"
            md_path = out / "m58_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M58-04", loaded["version"])
            self.assertIn("M58-04 Six-Fixture Scale Decision", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
