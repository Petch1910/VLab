"""Tests for tools/deck/export_third_fixture_deck_text.py."""

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

from tools.deck.export_third_fixture_deck_text import (  # noqa: E402
    DEFAULT_FIXTURE,
    DEFAULT_VALIDATION_REPORT,
    build_third_fixture_deck_text_export,
    load_json,
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


class TestThirdFixtureDeckTextExport(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.fixture = load_json(DEFAULT_FIXTURE)
        cls.validation_report = load_json(DEFAULT_VALIDATION_REPORT)
        cls.report = build_third_fixture_deck_text_export(cls.fixture, cls.validation_report)
        cls.deck_text = cls.report["_deck_text"]

    def test_export_is_ready_for_third_headless_fixture_smoke(self):
        summary = self.report["summary"]
        fixture = self.report["fixture_summary"]

        self.assertEqual("M46-02", self.report["version"])
        self.assertTrue(summary["export_ready"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertTrue(summary["ready_for_m46_03"])
        self.assertEqual("runtime_fixture_m44_recipe_001_link_joker_legion_bermuda_triangle_m45_04", fixture["fixture_id"])
        self.assertEqual("m44_recipe_001", fixture["recipe_id"])

    def test_text_uses_count_line_deck_codec_shape(self):
        deck_text = self.deck_text

        self.assertIn("# Vanguard Thai Sim Deck List", deck_text)
        self.assertIn("Name: Link Joker Legion Bermuda Triangle Fixture (m44_recipe_001)", deck_text)
        self.assertIn("Format: link_joker_legion_mate", deck_text)
        self.assertIn("PackId: vanguard_th", deck_text)
        self.assertIn("PackVersion: 251", deck_text)
        self.assertIn("PackDefinitionHash: ", deck_text)
        self.assertIn("[Main]", deck_text)
        self.assertIn("[Ride]", deck_text)
        self.assertIn("[G]", deck_text)

    def test_card_lines_sum_to_fixture_main_deck_count(self):
        lines = deck_card_lines(self.deck_text)
        total = sum(int(line.split()[0]) for line in lines)

        self.assertEqual(15, len(lines))
        self.assertEqual(50, total)
        self.assertEqual("4 EB06-003TH", lines[0])

    def test_review_comments_do_not_change_importable_card_lines(self):
        comment_lines = [line for line in self.deck_text.splitlines() if line.startswith("# Card: ")]
        card_lines = deck_card_lines(self.deck_text)

        self.assertEqual(len(card_lines), len(comment_lines))
        self.assertTrue(all(line.split()[1].endswith(("TH", "TH-B")) for line in card_lines))

    def test_scope_does_not_mutate_runtime_ui_or_bot(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_fixture_deck_text_export"])
        self.assertTrue(scope["third_fixture_deck_text_export"])
        self.assertTrue(scope["review_text_only"])
        self.assertFalse(scope["mutates_fixture_artifact"])
        self.assertFalse(scope["mutates_runtime_deck_library"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["GameState_mutation"])

    def test_invalid_m46_01_validation_blocks_export(self):
        validation_report = copy.deepcopy(self.validation_report)
        validation_report["summary"]["schema_valid"] = False
        validation_report["summary"]["ready_for_m46_02"] = False

        report = build_third_fixture_deck_text_export(self.fixture, validation_report)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["export_ready"])
        self.assertFalse(report["summary"]["ready_for_m46_03"])
        self.assertEqual("", report["_deck_text"])
        self.assertIn("m46_01_validation_not_ready", codes)

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            text_path = out / "third_fixture.txt"
            json_path = out / "third_fixture.json"
            md_path = out / "third_fixture.md"

            write_deck_text(self.report, text_path)
            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            self.assertIn("[Main]", text_path.read_text(encoding="utf-8"))
            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M46-02", loaded["version"])
            self.assertNotIn("_deck_text", loaded)
            self.assertIn("M46-02 Third Fixture Deck Text Export", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
