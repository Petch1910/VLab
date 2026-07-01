"""Tests for tools/deck/build_second_headless_fixture_load_smoke.py."""

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

from tools.deck.build_second_headless_fixture_load_smoke import (  # noqa: E402
    DEFAULT_DECK_TEXT,
    DEFAULT_FIXTURE,
    DEFAULT_M42_02_REPORT,
    HEADLESS_RULESET,
    HEADLESS_SEED,
    _deck_from_deck_code,
    build_second_headless_fixture_load_smoke,
    load_json,
    parse_count_line_deck_text,
    write_deck_code,
    write_json,
    write_markdown,
)


class TestSecondHeadlessFixtureLoadSmoke(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.fixture = load_json(DEFAULT_FIXTURE)
        cls.m42_02_report = load_json(DEFAULT_M42_02_REPORT)
        cls.deck_text = DEFAULT_DECK_TEXT.read_text(encoding="utf-8")
        cls.report = build_second_headless_fixture_load_smoke(cls.fixture, cls.m42_02_report, cls.deck_text)

    def test_offline_load_smoke_creates_deck_code(self):
        summary = self.report["summary"]
        fixture = self.report["fixture_summary"]
        request = self.report["headless_request"]

        self.assertEqual("M42-03", self.report["version"])
        self.assertTrue(summary["offline_load_ready"])
        self.assertTrue(summary["deck_code_created"])
        self.assertFalse(summary["unity_headless_result_provided"])
        self.assertFalse(summary["ready_for_m42_04"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertEqual(50, fixture["main_deck_count"])
        self.assertEqual(15, fixture["unique_card_count"])
        self.assertEqual(HEADLESS_SEED, request["seed"])
        self.assertEqual(HEADLESS_RULESET, request["ruleset"])
        self.assertTrue(request["deck_code_prefix"].startswith("VGTH1."))

    def test_count_line_parser_reads_main_ride_and_g_sections(self):
        parsed = parse_count_line_deck_text(self.deck_text)

        self.assertEqual([], parsed["issues"])
        self.assertEqual("Classic Core Oracle Think Tank Fixture (m40_recipe_001)", parsed["metadata"]["Name"])
        self.assertEqual("classic_part1", parsed["metadata"]["Format"])
        self.assertEqual("vanguard_th", parsed["metadata"]["PackId"])
        self.assertEqual(15, len(parsed["zones"]["Main"]))
        self.assertEqual([], parsed["zones"]["Ride"])
        self.assertEqual([], parsed["zones"]["G"])

    def test_deck_code_round_trips_to_vanguard_deck_shape(self):
        deck = _deck_from_deck_code(self.report["_deck_code"])

        self.assertEqual("Classic Core Oracle Think Tank Fixture (m40_recipe_001)", deck["name"])
        self.assertEqual("classic_part1", deck["format"])
        self.assertEqual("vanguard_th", deck["card_pack_id"])
        self.assertEqual("251", deck["card_pack_version"])
        self.assertEqual(15, len(deck["main"]))
        self.assertEqual(50, sum(entry["quantity"] for entry in deck["main"]))
        self.assertEqual([], deck["ride"])
        self.assertEqual([], deck["g"])

    def test_scope_does_not_mutate_runtime_ui_or_bot(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_headless_fixture_load_smoke"])
        self.assertTrue(scope["creates_deck_code_artifact"])
        self.assertFalse(scope["mutates_saved_decks"])
        self.assertFalse(scope["mutates_ui_deck_library"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["GameState_mutation_from_python"])

    def test_m42_02_not_ready_blocks_smoke(self):
        report_input = copy.deepcopy(self.m42_02_report)
        report_input["summary"]["export_ready"] = False

        report = build_second_headless_fixture_load_smoke(self.fixture, report_input, self.deck_text)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["offline_load_ready"])
        self.assertFalse(report["summary"]["deck_code_created"])
        self.assertIn("m42_02_export_not_ready", codes)

    def test_unity_result_can_complete_m42_03_gate(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            unity_result = out / "unity_result.json"
            unity_replay = out / "unity_replay.json"
            unity_result.write_text(
                json.dumps(
                    {
                        "accepted": True,
                        "seed": HEADLESS_SEED,
                        "ruleset": HEADLESS_RULESET,
                        "deck_source": "deck_code",
                        "actions_executed": 4,
                        "event_count": 4,
                    }
                ),
                encoding="utf-8",
            )
            unity_replay.write_text("{}", encoding="utf-8")

            report = build_second_headless_fixture_load_smoke(
                self.fixture,
                self.m42_02_report,
                self.deck_text,
                unity_result,
                unity_replay,
            )

            self.assertTrue(report["summary"]["unity_headless_result_provided"])
            self.assertTrue(report["summary"]["unity_headless_smoke_passed"])
            self.assertTrue(report["summary"]["ready_for_m42_04"])

    def test_unity_rejected_result_blocks_m42_04(self):
        with tempfile.TemporaryDirectory() as tmp:
            unity_result = Path(tmp) / "unity_result.json"
            unity_result.write_text(
                json.dumps(
                    {
                        "accepted": False,
                        "seed": HEADLESS_SEED,
                        "ruleset": HEADLESS_RULESET,
                        "deck_source": "deck_code",
                        "failure_reason": "boom",
                    }
                ),
                encoding="utf-8",
            )

            report = build_second_headless_fixture_load_smoke(
                self.fixture,
                self.m42_02_report,
                self.deck_text,
                unity_result,
                None,
            )
            codes = [issue["code"] for issue in report["issues"]]

            self.assertFalse(report["summary"]["ready_for_m42_04"])
            self.assertIn("unity_headless_rejected", codes)

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            deck_code_path = out / "deck_code.txt"
            json_path = out / "m42_03.json"
            md_path = out / "m42_03.md"

            write_deck_code(self.report, deck_code_path)
            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            self.assertTrue(deck_code_path.read_text(encoding="utf-8").startswith("VGTH1."))
            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M42-03", loaded["version"])
            self.assertNotIn("_deck_code", loaded)
            self.assertIn("M42-03 Second Fixture Headless Load Smoke", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
