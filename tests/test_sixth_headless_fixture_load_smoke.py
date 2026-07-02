"""Tests for tools/deck/build_sixth_headless_fixture_load_smoke.py."""

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

from tools.deck.build_sixth_headless_fixture_load_smoke import (  # noqa: E402
    HEADLESS_RULESET,
    HEADLESS_SEED,
    _deck_from_deck_code,
    build_sixth_headless_fixture_load_smoke,
    parse_count_line_deck_text,
    write_deck_code,
    write_json,
    write_markdown,
)
from tools.deck.build_sixth_slice_g_zone_stride_decision_artifact import (  # noqa: E402
    build_sixth_slice_g_zone_stride_decision_artifact,
)
from tools.deck.build_sixth_slice_human_accepted_repair_artifact import (  # noqa: E402
    M56_DRAFTS,
    M56_SCAFFOLD,
    build_sixth_slice_human_accepted_repair_artifact,
    load_json,
)
from tools.deck.build_sixth_slice_human_selected_recipe_artifact import (  # noqa: E402
    M57_01_REVIEW,
    build_sixth_slice_human_selected_recipe_artifact,
)
from tools.deck.build_sixth_slice_runtime_fixture_promotion_gate import (  # noqa: E402
    build_sixth_slice_runtime_fixture_promotion_gate,
)
from tools.deck.export_sixth_fixture_deck_text import (  # noqa: E402
    build_sixth_fixture_deck_text_export,
)
from tools.deck.validate_sixth_runtime_fixture_schema import (  # noqa: E402
    EXPECTED_G_ZONE_OPTION,
    build_sixth_runtime_fixture_schema_validation_report,
)
from tools.deck.validate_sixth_slice_repaired_recipe import (  # noqa: E402
    build_sixth_slice_repaired_validation_report,
)


SELECTED_REVIEW_ITEM_ID = "m57_01_m56_recipe_001_repair_review"


class TestSixthHeadlessFixtureLoadSmoke(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
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
        cls.fixture = gate_report["runtime_fixture"]
        m58_01_report = build_sixth_runtime_fixture_schema_validation_report(cls.fixture)
        cls.m58_02_report = build_sixth_fixture_deck_text_export(cls.fixture, m58_01_report)
        cls.deck_text = cls.m58_02_report["_deck_text"]
        cls.report = build_sixth_headless_fixture_load_smoke(cls.fixture, cls.m58_02_report, cls.deck_text)

    def test_offline_load_smoke_creates_deck_code(self):
        summary = self.report["summary"]
        fixture = self.report["fixture_summary"]
        request = self.report["headless_request"]

        self.assertEqual("M58-03", self.report["version"])
        self.assertTrue(summary["offline_load_ready"])
        self.assertTrue(summary["deck_code_created"])
        self.assertFalse(summary["unity_headless_result_provided"])
        self.assertFalse(summary["ready_for_m58_04"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertEqual(50, fixture["main_deck_count"])
        self.assertGreater(fixture["unique_card_count"], 0)
        self.assertEqual(0, fixture["g_zone_count"])
        self.assertEqual(HEADLESS_SEED, request["seed"])
        self.assertEqual(HEADLESS_RULESET, request["ruleset"])
        self.assertTrue(request["deck_code_prefix"].startswith("VGTH1."))

    def test_count_line_parser_reads_main_ride_and_empty_g_sections(self):
        parsed = parse_count_line_deck_text(self.deck_text)

        self.assertEqual([], parsed["issues"])
        self.assertEqual("G NEXT/Z Shadow Paladin Fixture (m56_recipe_001)", parsed["metadata"]["Name"])
        self.assertEqual("g_next_z", parsed["metadata"]["Format"])
        self.assertEqual("vanguard_th", parsed["metadata"]["PackId"])
        self.assertGreater(len(parsed["zones"]["Main"]), 0)
        self.assertEqual([], parsed["zones"]["Ride"])
        self.assertEqual([], parsed["zones"]["G"])

    def test_deck_code_round_trips_to_vanguard_deck_shape(self):
        deck = _deck_from_deck_code(self.report["_deck_code"])

        self.assertEqual("G NEXT/Z Shadow Paladin Fixture (m56_recipe_001)", deck["name"])
        self.assertEqual("g_next_z", deck["format"])
        self.assertEqual("vanguard_th", deck["card_pack_id"])
        self.assertEqual("251", deck["card_pack_version"])
        self.assertEqual(self.report["fixture_summary"]["unique_card_count"], len(deck["main"]))
        self.assertEqual(50, sum(entry["quantity"] for entry in deck["main"]))
        self.assertEqual([], deck["ride"])
        self.assertEqual([], deck["g"])

    def test_scope_does_not_mutate_runtime_ui_bot_or_g_zone(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_headless_fixture_load_smoke"])
        self.assertTrue(scope["sixth_fixture_headless_load_smoke"])
        self.assertTrue(scope["creates_deck_code_artifact"])
        self.assertFalse(scope["mutates_saved_decks"])
        self.assertFalse(scope["mutates_ui_deck_library"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["g_zone_runtime_enabled"])
        self.assertFalse(scope["stride_runtime_enabled"])
        self.assertFalse(scope["GameState_mutation_from_python"])

    def test_m58_02_not_ready_blocks_smoke(self):
        report_input = copy.deepcopy(self.m58_02_report)
        report_input["summary"]["export_ready"] = False

        report = build_sixth_headless_fixture_load_smoke(self.fixture, report_input, self.deck_text)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["offline_load_ready"])
        self.assertFalse(report["summary"]["deck_code_created"])
        self.assertIn("m58_02_export_not_ready", codes)

    def test_non_empty_g_zone_blocks_smoke(self):
        deck_text = self.deck_text + "1 G-BT01-001TH\n"

        report = build_sixth_headless_fixture_load_smoke(self.fixture, self.m58_02_report, deck_text)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["offline_load_ready"])
        self.assertIn("g_zone_not_empty", codes)

    def test_unity_result_can_complete_m58_03_gate(self):
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

            report = build_sixth_headless_fixture_load_smoke(
                self.fixture,
                self.m58_02_report,
                self.deck_text,
                unity_result,
                unity_replay,
            )

            self.assertTrue(report["summary"]["unity_headless_result_provided"])
            self.assertTrue(report["summary"]["unity_headless_smoke_passed"])
            self.assertTrue(report["summary"]["ready_for_m58_04"])

    def test_unity_rejected_result_blocks_m58_04(self):
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

            report = build_sixth_headless_fixture_load_smoke(
                self.fixture,
                self.m58_02_report,
                self.deck_text,
                unity_result,
                None,
            )
            codes = [issue["code"] for issue in report["issues"]]

            self.assertFalse(report["summary"]["ready_for_m58_04"])
            self.assertIn("unity_headless_rejected", codes)

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            deck_code_path = out / "deck_code.txt"
            json_path = out / "m58_03.json"
            md_path = out / "m58_03.md"

            write_deck_code(self.report, deck_code_path)
            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            self.assertTrue(deck_code_path.read_text(encoding="utf-8").startswith("VGTH1."))
            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M58-03", loaded["version"])
            self.assertNotIn("_deck_code", loaded)
            self.assertIn("M58-03 Sixth Fixture Headless Load Smoke", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
