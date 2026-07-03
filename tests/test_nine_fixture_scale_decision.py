"""Tests for tools/deck/build_nine_fixture_scale_decision.py."""

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

import tests.test_eight_fixture_scale_decision as eight_scale_fixture  # noqa: E402
import tests.test_ninth_headless_fixture_load_smoke as ninth_smoke_fixture  # noqa: E402
from tools.deck.build_nine_fixture_scale_decision import (  # noqa: E402
    build_nine_fixture_scale_decision,
    write_json,
    write_markdown,
)
from tools.deck.build_ninth_headless_fixture_load_smoke import (  # noqa: E402
    HEADLESS_RULESET,
    HEADLESS_SEED,
    build_ninth_headless_fixture_load_smoke,
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


class TestNineFixtureScaleDecision(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        eight_scale_fixture.TestEightFixtureScaleDecision.setUpClass()
        ninth_smoke_fixture.TestNinthHeadlessFixtureLoadSmoke.setUpClass()

        cls.first_smoke = eight_scale_fixture.TestEightFixtureScaleDecision.first_smoke
        cls.second_smoke = eight_scale_fixture.TestEightFixtureScaleDecision.second_smoke
        cls.third_smoke = eight_scale_fixture.TestEightFixtureScaleDecision.third_smoke
        cls.fourth_smoke = eight_scale_fixture.TestEightFixtureScaleDecision.fourth_smoke
        cls.fifth_smoke = eight_scale_fixture.TestEightFixtureScaleDecision.fifth_smoke
        cls.sixth_smoke = eight_scale_fixture.TestEightFixtureScaleDecision.sixth_smoke
        cls.seventh_smoke = eight_scale_fixture.TestEightFixtureScaleDecision.seventh_smoke
        cls.eighth_smoke = eight_scale_fixture.TestEightFixtureScaleDecision.eighth_smoke
        cls.sixth_fixture = eight_scale_fixture.TestEightFixtureScaleDecision.sixth_fixture
        cls.seventh_fixture = eight_scale_fixture.TestEightFixtureScaleDecision.seventh_fixture
        cls.eighth_fixture = eight_scale_fixture.TestEightFixtureScaleDecision.eighth_fixture
        cls.ranking = eight_scale_fixture.TestEightFixtureScaleDecision.ranking
        cls.ninth_fixture = ninth_smoke_fixture.TestNinthHeadlessFixtureLoadSmoke.fixture
        cls.m70_02_report = ninth_smoke_fixture.TestNinthHeadlessFixtureLoadSmoke.m70_02_report
        cls.ninth_deck_text = ninth_smoke_fixture.TestNinthHeadlessFixtureLoadSmoke.deck_text

        with tempfile.TemporaryDirectory() as tmp:
            tmp_path = Path(tmp)
            unity_result = _unity_result_file(tmp_path)
            unity_replay = tmp_path / "unity_replay.json"
            unity_replay.write_text("{}", encoding="utf-8")
            cls.ninth_smoke = build_ninth_headless_fixture_load_smoke(
                cls.ninth_fixture,
                cls.m70_02_report,
                cls.ninth_deck_text,
                unity_result,
                unity_replay,
            )

        cls.report = build_nine_fixture_scale_decision(
            cls.first_smoke,
            cls.second_smoke,
            cls.third_smoke,
            cls.fourth_smoke,
            cls.fifth_smoke,
            cls.sixth_smoke,
            cls.seventh_smoke,
            cls.eighth_smoke,
            cls.ninth_smoke,
            cls.ranking,
            cls.sixth_fixture,
            cls.seventh_fixture,
            cls.eighth_fixture,
            cls.ninth_fixture,
        )

    def test_decision_allows_post_m70_queue_planning_without_selecting_tenth_slice(self):
        summary = self.report["summary"]
        decision = self.report["decision"]

        self.assertEqual("M70-04", self.report["version"])
        self.assertEqual(9, summary["fixture_evidence_count"])
        self.assertEqual(9, summary["passed_fixture_count"])
        self.assertEqual(0, summary["failed_fixture_count"])
        self.assertGreaterEqual(summary["candidate_count"], 0)
        self.assertTrue(summary["post_m70_queue_review_ready"])
        self.assertTrue(summary["ready_for_m71_planning"])
        self.assertTrue(decision["post_m70_queue_review_ready"])
        self.assertFalse(decision["tenth_slice_selected_now"])

    def test_fixture_evidence_requires_unity_deck_code_acceptance(self):
        for item in self.report["fixture_evidence"]:
            self.assertTrue(item["offline_load_ready"])
            self.assertTrue(item["deck_code_created"])
            self.assertTrue(item["unity_headless_smoke_passed"])
            self.assertEqual(0, item["blocking_issue_count"])
            self.assertEqual("deck_code", item["deck_source"])
            self.assertEqual(4, item["actions_executed"])
            self.assertEqual(4, item["event_count"])

    def test_ninth_fixture_aqua_force_is_counted(self):
        ninth = [item for item in self.report["fixture_evidence"] if item["label"] == "ninth_fixture_aqua_force"][0]

        self.assertEqual("runtime_fixture_m68_recipe_001_aqua_force_m69_06", ninth["fixture_id"])
        self.assertEqual("m68_recipe_001", ninth["recipe_id"])
        self.assertEqual(50, ninth["main_deck_count"])
        self.assertGreater(ninth["unique_card_count"], 0)
        self.assertEqual(0, ninth["g_zone_count"])
        self.assertTrue(ninth["unity_headless_smoke_passed"])

    def test_candidate_queue_excludes_completed_fixture_groups(self):
        completed = set(self.report["completed_groups"])
        candidate_groups = {candidate["group"] for candidate in self.report["candidate_queue"]}

        self.assertEqual(9, len(completed))
        self.assertTrue(completed.isdisjoint(candidate_groups))

    def test_scope_does_not_mutate_runtime_ui_bot_or_advanced_systems(self):
        scope = self.report["scope"]
        decision = self.report["decision"]

        self.assertTrue(scope["offline_scale_decision"])
        self.assertTrue(scope["post_nine_fixture_queue_gate"])
        self.assertFalse(scope["selects_runtime_deck"])
        self.assertFalse(scope["selects_tenth_slice"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["g_zone_runtime_enabled"])
        self.assertFalse(scope["stride_runtime_enabled"])
        self.assertFalse(scope["aqua_force_battle_order_runtime_enabled"])
        self.assertFalse(scope["bloom_token_runtime_enabled"])
        self.assertFalse(scope["lock_runtime_enabled"])
        self.assertFalse(scope["unlock_runtime_enabled"])
        self.assertFalse(scope["legion_runtime_enabled"])
        self.assertFalse(scope["mate_identity_check_enabled"])
        self.assertFalse(scope["GameState_mutation"])
        self.assertFalse(decision["live_runtime_deck_enabled"])
        self.assertFalse(decision["saved_deck_enabled"])
        self.assertFalse(decision["ui_deck_list_enabled"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertFalse(decision["g_zone_runtime_enabled"])
        self.assertFalse(decision["stride_runtime_enabled"])
        self.assertFalse(decision["aqua_force_battle_order_runtime_enabled"])
        self.assertFalse(decision["bloom_token_runtime_enabled"])
        self.assertFalse(decision["lock_runtime_enabled"])
        self.assertFalse(decision["unlock_runtime_enabled"])
        self.assertFalse(decision["legion_runtime_enabled"])
        self.assertFalse(decision["mate_identity_check_enabled"])

    def test_failed_ninth_fixture_routes_to_repair(self):
        ninth_smoke = copy.deepcopy(self.ninth_smoke)
        ninth_smoke["summary"]["unity_headless_smoke_passed"] = False
        ninth_smoke["unity_headless_result"]["accepted"] = False

        report = build_nine_fixture_scale_decision(
            self.first_smoke,
            self.second_smoke,
            self.third_smoke,
            self.fourth_smoke,
            self.fifth_smoke,
            self.sixth_smoke,
            self.seventh_smoke,
            self.eighth_smoke,
            ninth_smoke,
            self.ranking,
            self.sixth_fixture,
            self.seventh_fixture,
            self.eighth_fixture,
            self.ninth_fixture,
        )

        self.assertFalse(report["summary"]["post_m70_queue_review_ready"])
        self.assertFalse(report["summary"]["ready_for_m71_planning"])
        self.assertEqual("M70-repair", report["next_target"]["milestone"])

    def test_next_target_is_m71_01_planning(self):
        next_target = self.report["next_target"]

        self.assertEqual("M71-01", next_target["milestone"])
        self.assertEqual("Post-nine fixture queue planning", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m70_04.json"
            md_path = out / "m70_04.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M70-04", loaded["version"])
            self.assertIn("M70-04 Nine-Fixture Scale Decision", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
