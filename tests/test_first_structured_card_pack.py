import json
import sys
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools/data"))

from validate_ability_schema import validate_ability_file  # noqa: E402


PACK_PATH = ROOT / "data/packs/vanguard_th/abilities/structured_ability_pack_m12_10.json"


class FirstStructuredCardPackTests(unittest.TestCase):
    def load_pack(self):
        with PACK_PATH.open("r", encoding="utf-8") as handle:
            return json.load(handle)

    def test_pack_validates_with_ability_schema_validator(self):
        report = validate_ability_file(PACK_PATH)

        self.assertTrue(report.all_ok, report.to_dict())
        self.assertEqual(20, report.ability_count)

    def test_pack_is_template_only_and_uses_supported_effect_subset(self):
        pack = self.load_pack()
        effect_types = set()
        card_ids = set()
        ability_ids = set()

        for ability in pack["abilities"]:
            ability_ids.add(ability["ability_id"])
            card_ids.add(ability["card_id"])
            self.assertIn("template smoke pack only", ability.get("notes", ""))
            self.assertTrue(ability["manual_fallback"])
            for effect in ability["effects"]:
                effect_types.add(effect["type"])

        self.assertEqual(20, len(ability_ids))
        self.assertEqual(20, len(card_ids))
        self.assertEqual(
            {"draw", "counter_blast", "counter_charge", "power_plus", "critical_plus"},
            effect_types,
        )


if __name__ == "__main__":
    unittest.main()
