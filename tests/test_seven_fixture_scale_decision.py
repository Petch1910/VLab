"""Tests for tools/deck/build_seven_fixture_scale_decision.py."""

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

import tests.test_seventh_headless_fixture_load_smoke as seventh_smoke_fixture  # noqa: E402
import tests.test_six_fixture_scale_decision as six_scale_fixture  # noqa: E402
from tools.deck.build_seven_fixture_scale_decision import (  # noqa: E402
    build_seven_fixture_scale_decision,
    write_json,
    write_markdown,
)
from tools.deck.build_seventh_headless_fixture_load_smoke import (  # noqa: E402
    HEADLESS_RULESET,
    HEADLESS_SEED,
    build_seventh_headless_fixture_load_smoke,
)


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


class TestSevenFixtureScaleDecision(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        six_scale_fixture.TestSixFixtureScaleDecision.setUpClass()
        seventh_smoke_fixture.TestSeventhHeadlessFixtureLoadSmoke.setUpClass()

        cls.first_smoke = six_scale_fixture.TestSixFixtureScaleDecision.first_smoke
        cls.second_smoke = six_scale_fixture.TestSixFixtureScaleDecision.second_smoke
        cls.third_smoke = six_scale_fixture.TestSixFixtureScaleDecision.third_smoke
        cls.fourth_smoke = six_scale_fixture.TestSixFixtureScaleDecision.fourth_smoke
        cls.fifth_smoke = six_scale_fixture.TestSixFixtureScaleDecision.fifth_smoke
        cls.sixth_smoke = six_scale_fixture.TestSixFixtureScaleDecision.sixth_smoke
        cls.sixth_fixture = six_scale_fixture.TestSixFixtureScaleDecision.sixth_fixture
        cls.ranking = six_scale_fixture.TestSixFixtureScaleDecision.ranking
        cls.seventh_fixture = seventh_smoke_fixture.TestSeventhHeadlessFixtureLoadSmoke.fixture
        cls.m62_02_report = seventh_smoke_fixture.TestSeventhHeadlessFixtureLoadSmoke.m62_02_report
        cls.seventh_deck_text = seventh_smoke_fixture.TestSeventhHeadlessFixtureLoadSmoke.deck_text

        with tempfile.TemporaryDirectory() as tmp:
            tmp_path = Path(tmp)
            unity_result = _unity_result_file(tmp_path)
            unity_replay = tmp_path / "unity_replay.json"
            unity_replay.write_text("{}", encoding="utf-8")
            cls.seventh_smoke = build_seventh_headless_fixture_load_smoke(
                cls.seventh_fixture,
                cls.m62_02_report,
                cls.seventh_deck_text,
                unity_result,
                unity_replay,
            )

        cls.report = build_seven_fixture_scale_decision(
            cls.first_smoke,
            cls.second_smoke,
            cls.third_smoke,
            cls.fourth_smoke,
            cls.fifth_smoke,
            cls.sixth_smoke,
            cls.seventh_smoke,
            cls.ranking,
            cls.sixth_fixture,
            cls.seventh_fixture,
        )

    def test_decision_allows_eighth_slice_offline_pipeline(self):
        summary = self.report["summary"]
        decision = self.report["decision"]

        self.assertEqual("M62-04", self.report["version"])
        self.assertEqual(7, summary["fixture_evidence_count"])
        self.assertEqual(7, summary["passed_fixture_count"])
        self.assertEqual(0, summary["failed_fixture_count"])
        self.assertGreaterEqual(summary["candidate_count"], 1)
        self.assertTrue(summary["eighth_slice_offline_pipeline_allowed"])
        self.assertTrue(summary["ready_for_m63"])
        self.assertTrue(decision["eighth_slice_offline_pipeline_allowed"])
        self.assertFalse(decision["eighth_slice_selected_now"])

    def test_fixture_evidence_requires_unity_deck_code_acceptance(self):
        for item in self.report["fixture_evidence"]:
            self.assertTrue(item["offline_load_ready"])
            self.assertTrue(item["deck_code_created"])
            self.assertTrue(item["unity_headless_smoke_passed"])
            self.assertEqual(0, item["blocking_issue_count"])
            self.assertEqual("deck_code", item["deck_source"])
            self.assertEqual(4, item["actions_executed"])
            self.assertEqual(4, item["event_count"])

    def test_seventh_fixture_neo_nectar_is_counted(self):
        seventh = [item for item in self.report["fixture_evidence"] if item["label"] == "seventh_fixture_neo_nectar"][0]

        self.assertEqual("runtime_fixture_m60_recipe_001_neo_nectar_m61_06", seventh["fixture_id"])
        self.assertEqual("m60_recipe_001", seventh["recipe_id"])
        self.assertEqual(50, seventh["main_deck_count"])
        self.assertGreater(seventh["unique_card_count"], 0)
        self.assertEqual(0, seventh["g_zone_count"])
        self.assertTrue(seventh["unity_headless_smoke_passed"])

    def test_candidate_queue_excludes_completed_fixture_groups(self):
        completed = set(self.report["completed_groups"])
        candidate_groups = {candidate["group"] for candidate in self.report["candidate_queue"]}

        self.assertEqual(7, len(completed))
        self.assertTrue(completed.isdisjoint(candidate_groups))

    def test_scope_does_not_mutate_runtime_ui_bot_or_advanced_systems(self):
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
        self.assertFalse(scope["bloom_token_runtime_enabled"])
        self.assertFalse(scope["GameState_mutation"])
        self.assertFalse(decision["live_runtime_deck_enabled"])
        self.assertFalse(decision["saved_deck_enabled"])
        self.assertFalse(decision["ui_deck_list_enabled"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertFalse(decision["g_zone_runtime_enabled"])
        self.assertFalse(decision["stride_runtime_enabled"])
        self.assertFalse(decision["bloom_token_runtime_enabled"])

    def test_failed_seventh_fixture_routes_to_repair(self):
        seventh_smoke = copy.deepcopy(self.seventh_smoke)
        seventh_smoke["summary"]["unity_headless_smoke_passed"] = False
        seventh_smoke["unity_headless_result"]["accepted"] = False

        report = build_seven_fixture_scale_decision(
            self.first_smoke,
            self.second_smoke,
            self.third_smoke,
            self.fourth_smoke,
            self.fifth_smoke,
            self.sixth_smoke,
            seventh_smoke,
            self.ranking,
            self.sixth_fixture,
            self.seventh_fixture,
        )

        self.assertFalse(report["summary"]["eighth_slice_offline_pipeline_allowed"])
        self.assertFalse(report["summary"]["ready_for_m63"])
        self.assertEqual("M62-repair", report["next_target"]["milestone"])

    def test_next_target_is_m63_01(self):
        next_target = self.report["next_target"]

        self.assertEqual("M63-01", next_target["milestone"])
        self.assertEqual("Eighth target slice selection", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m62_04.json"
            md_path = out / "m62_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M62-04", loaded["version"])
            self.assertIn("M62-04 Seven-Fixture Scale Decision", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
