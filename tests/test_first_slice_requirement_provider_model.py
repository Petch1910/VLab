"""Tests for tools/deck/build_first_slice_requirement_provider_model.py (M35-B3)."""

from __future__ import annotations

import json
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_first_slice_requirement_provider_model import build_report  # noqa: E402


class TestFirstSliceRequirementProviderModel(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.report = build_report()

    def test_model_has_requirements_and_providers(self):
        summary = self.report["summary"]
        self.assertEqual("M35-B3", self.report["version"])
        self.assertGreater(summary["card_count"], 0)
        self.assertGreater(summary["cards_with_requirements"], 0)
        self.assertGreater(summary["cards_with_providers"], 0)

    def test_common_provider_types_exist(self):
        providers = self.report["indexes"]["provider_index"]
        self.assertIn("provides_power_pressure", providers)
        self.assertIn("provides_restand_or_multi_attack", providers)

    def test_common_requirement_types_exist(self):
        requirements = self.report["indexes"]["requirement_index"]
        self.assertIn("requires_vanguard_circle", requirements)
        self.assertIn("requires_rear_guard_circle", requirements)

    def test_scope_does_not_promote_to_compatibility_or_playbook(self):
        scope = self.report["scope"]
        self.assertTrue(scope["advisory_only"])
        self.assertFalse(scope["compatibility_graph"])
        self.assertFalse(scope["deck_skeleton"])
        self.assertFalse(scope["bot_playbook"])

    def test_manual_review_count_is_card_level_count(self):
        manual = [card for card in self.report["cards"] if card["manual_review_required"]]
        self.assertEqual(self.report["summary"]["manual_review_count"], len(manual))

    def test_output_file_can_round_trip_after_cli_generation(self):
        path = ROOT / "outputs" / "target_slice" / "m35_b3_first_slice_requirement_provider_model.json"
        if path.exists():
            data = json.loads(path.read_text(encoding="utf-8"))
            self.assertEqual("M35-B3", data["version"])


if __name__ == "__main__":
    unittest.main()
