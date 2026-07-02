"""Tests for tools/deck/export_sixth_fixture_deck_text.py."""

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
    write_deck_text,
    write_json,
    write_markdown,
)
from tools.deck.validate_sixth_runtime_fixture_schema import (  # noqa: E402
    EXPECTED_G_ZONE_OPTION,
    build_sixth_runtime_fixture_schema_validation_report,
)
from tools.deck.validate_sixth_slice_repaired_recipe import (  # noqa: E402
    build_sixth_slice_repaired_validation_report,
)


SELECTED_REVIEW_ITEM_ID = "m57_01_m56_recipe_001_repair_review"


def deck_card_lines(deck_text: str) -> list[str]:
    return [
        line.strip()
        for line in deck_text.splitlines()
        if line.strip() and not line.strip().startswith("#") and line.strip()[0].isdigit()
    ]


class TestSixthFixtureDeckTextExport(unittest.TestCase):
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
        cls.validation_report = build_sixth_runtime_fixture_schema_validation_report(cls.fixture)
        cls.report = build_sixth_fixture_deck_text_export(cls.fixture, cls.validation_report)
        cls.deck_text = cls.report["_deck_text"]

    def test_export_is_ready_for_sixth_headless_fixture_smoke(self):
        summary = self.report["summary"]
        fixture = self.report["fixture_summary"]

        self.assertEqual("M58-02", self.report["version"])
        self.assertTrue(summary["export_ready"])
        self.assertEqual(0, summary["blocking_issue_count"])
        self.assertTrue(summary["ready_for_m58_03"])
        self.assertEqual("runtime_fixture_m56_recipe_001_shadow_paladin_m57_06", fixture["fixture_id"])
        self.assertEqual("m56_recipe_001", fixture["recipe_id"])

    def test_text_uses_count_line_deck_codec_shape(self):
        deck_text = self.deck_text

        self.assertIn("# Vanguard Thai Sim Deck List", deck_text)
        self.assertIn("Name: G NEXT/Z Shadow Paladin Fixture (m56_recipe_001)", deck_text)
        self.assertIn("Format: g_next_z", deck_text)
        self.assertIn("PackId: vanguard_th", deck_text)
        self.assertIn("PackVersion: 251", deck_text)
        self.assertIn("PackDefinitionHash: ", deck_text)
        self.assertIn("[Main]", deck_text)
        self.assertIn("[Ride]", deck_text)
        self.assertIn("[G]", deck_text)
        self.assertIn("G Zone and Stride runtime are disabled", deck_text)

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

    def test_scope_does_not_mutate_runtime_ui_bot_or_g_zone(self):
        scope = self.report["scope"]

        self.assertTrue(scope["offline_fixture_deck_text_export"])
        self.assertTrue(scope["sixth_fixture_deck_text_export"])
        self.assertTrue(scope["review_text_only"])
        self.assertFalse(scope["mutates_fixture_artifact"])
        self.assertFalse(scope["mutates_runtime_deck_library"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_integration"])
        self.assertFalse(scope["g_zone_runtime_enabled"])
        self.assertFalse(scope["stride_runtime_enabled"])
        self.assertFalse(scope["GameState_mutation"])

    def test_invalid_m58_01_validation_blocks_export(self):
        validation_report = copy.deepcopy(self.validation_report)
        validation_report["summary"]["schema_valid"] = False
        validation_report["summary"]["ready_for_m58_02"] = False

        report = build_sixth_fixture_deck_text_export(self.fixture, validation_report)
        codes = [issue["code"] for issue in report["issues"]]

        self.assertFalse(report["summary"]["export_ready"])
        self.assertFalse(report["summary"]["ready_for_m58_03"])
        self.assertEqual("", report["_deck_text"])
        self.assertIn("m58_01_validation_not_ready", codes)

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            text_path = out / "sixth_fixture.txt"
            json_path = out / "sixth_fixture.json"
            md_path = out / "sixth_fixture.md"

            write_deck_text(self.report, text_path)
            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            self.assertIn("[Main]", text_path.read_text(encoding="utf-8"))
            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M58-02", loaded["version"])
            self.assertNotIn("_deck_text", loaded)
            self.assertIn("M58-02 Sixth Fixture Deck Text Export", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
