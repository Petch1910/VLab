"""Tests for tools/deck/export_ninth_fixture_deck_text.py."""

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

import tests.test_ninth_runtime_fixture_schema_validator as schema_fixture  # noqa: E402
from tools.deck.export_ninth_fixture_deck_text import (  # noqa: E402
    build_ninth_fixture_deck_text_export,
    write_deck_text,
    write_json,
    write_markdown,
)


def deck_card_lines(deck_text: str) -> list[str]:
    return [
        line.strip()
        for line in deck_text.splitlines()
        if line.strip() and not line.strip().startswith("#") and line.strip()[0].isdigit()
    ]


class TestNinthFixtureDeckTextExport(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        schema_fixture.TestNinthRuntimeFixtureSchemaValidator.setUpClass()
        cls.fixture = schema_fixture.TestNinthRuntimeFixtureSchemaValidator.fixture
        cls.validation_report = schema_fixture.TestNinthRuntimeFixtureSchemaValidator.report
        cls.report = build_ninth_fixture_deck_text_export(cls.fixture, cls.validation_report)
        cls.deck_text = cls.report["_deck_text"]

    def test_export_is_ready_for_ninth_headless_fixture_smoke(self):
        summary = self.report["summary"]
        fixture = self.report["fixture_summary"]

        self.assertEqual("M70-02", self.report["version"])
        self.assertTrue(summary["export_ready"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertTrue(summary["ready_for_m70_03"])
        self.assertEqual("runtime_fixture_m68_recipe_001_aqua_force_m69_06", fixture["fixture_id"])
        self.assertEqual("m68_recipe_001", fixture["recipe_id"])

    def test_text_uses_count_line_deck_codec_shape(self):
        deck_text = self.deck_text

        self.assertIn("# Vanguard Thai Sim Deck List", deck_text)
        self.assertIn("Name: G Series Aqua Force Fixture (m68_recipe_001)", deck_text)
        self.assertIn("Format: g_series_first", deck_text)
        self.assertIn("PackId: vanguard_th", deck_text)
        self.assertIn("PackVersion: 251", deck_text)
        self.assertIn("PackDefinitionHash: ", deck_text)
        self.assertIn("[Main]", deck_text)
        self.assertIn("[Ride]", deck_text)
        self.assertIn("[G]", deck_text)
        self.assertIn("G Zone runtime is disabled", deck_text)
        self.assertIn("Stride runtime is disabled", deck_text)
        self.assertIn("Aqua Force battle-order runtime is disabled", deck_text)
        self.assertIn("GZoneBoundary: main_deck_only_review_no_runtime_promotion", deck_text)
        self.assertIn("StrideBoundary: main_deck_only_review_no_runtime_promotion", deck_text)
        self.assertIn("AquaForceBoundary: manual_semantic_review_only_no_runtime_promotion", deck_text)

    def test_card_lines_sum_to_fixture_main_deck_count(self):
        lines = deck_card_lines(self.deck_text)
        total = sum(int(line.split()[0]) for line in lines)

        self.assertGreater(len(lines), 0)
        self.assertEqual(self.report["fixture_summary"]["unique_card_count"], len(lines))
        self.assertEqual(50, total)
        self.assertTrue(all(line.split()[1].endswith("TH") for line in lines))

    def test_review_comments_do_not_change_importable_card_lines(self):
        comment_lines = [line for line in self.deck_text.splitlines() if line.startswith("# Card: ")]
        card_lines = deck_card_lines(self.deck_text)

        self.assertEqual(len(card_lines), len(comment_lines))
        self.assertTrue(all(line.split()[0].isdigit() for line in card_lines))

    def test_scope_does_not_mutate_runtime_ui_bot_or_system_runtime(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_fixture_deck_text_export"])
        self.assertTrue(scope["ninth_fixture_deck_text_export"])
        self.assertTrue(scope["review_text_only"])
        self.assertFalse(scope["mutates_fixture_artifact"])
        self.assertFalse(scope["mutates_runtime_deck_library"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["g_zone_runtime_enabled"])
        self.assertFalse(scope["stride_runtime_enabled"])
        self.assertFalse(scope["aqua_force_battle_order_runtime_enabled"])
        self.assertFalse(scope["battle_count_tracker_enabled"])
        self.assertFalse(scope["attack_order_predicate_runtime_enabled"])
        self.assertFalse(scope["multi_attack_label_runtime_enabled"])
        self.assertFalse(scope["live_card_text_parsing"])
        self.assertFalse(scope["GameState_mutation"])

    def test_invalid_m70_01_validation_blocks_export(self):
        validation_report = copy.deepcopy(self.validation_report)
        validation_report["summary"]["schema_valid"] = False
        validation_report["summary"]["ready_for_m70_02"] = False

        report = build_ninth_fixture_deck_text_export(self.fixture, validation_report)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["export_ready"])
        self.assertFalse(report["summary"]["ready_for_m70_03"])
        self.assertEqual("", report["_deck_text"])
        self.assertIn("m70_01_validation_not_ready", codes)

    def test_malformed_main_deck_row_blocks_export(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["main_deck"][0]["quantity"] = 0

        report = build_ninth_fixture_deck_text_export(fixture, self.validation_report)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["export_ready"])
        self.assertEqual("", report["_deck_text"])
        self.assertIn("invalid_fixture_main_deck_row", codes)

    def test_missing_card_blocks_export(self):
        fixture = copy.deepcopy(self.fixture)
        fixture["main_deck"][0]["card_id"] = "MISSING-999TH"

        report = build_ninth_fixture_deck_text_export(fixture, self.validation_report)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["export_ready"])
        self.assertEqual("", report["_deck_text"])
        self.assertIn("missing_cards", codes)

    def test_next_target_is_m70_03(self):
        next_target = self.report["next_target"]

        self.assertEqual("M70-03", next_target["milestone"])
        self.assertEqual("Ninth fixture headless load smoke", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            text_path = out / "ninth_fixture.txt"
            json_path = out / "ninth_fixture.json"
            md_path = out / "ninth_fixture.md"

            write_deck_text(self.report, text_path)
            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            self.assertIn("[Main]", text_path.read_text(encoding="utf-8"))
            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M70-02", loaded["version"])
            self.assertNotIn("_deck_text", loaded)
            self.assertIn("M70-02 Ninth Fixture Deck Text Export", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
