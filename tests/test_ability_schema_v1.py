import json
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SCHEMA_PATH = ROOT / "data" / "schemas" / "ability_schema_v1.json"
SAMPLE_PATH = ROOT / "data" / "templates" / "ability_schema_v1" / "sample_abilities.json"


class AbilitySchemaV1Tests(unittest.TestCase):
    def load_schema(self):
        with SCHEMA_PATH.open("r", encoding="utf-8") as handle:
            return json.load(handle)

    def load_sample(self):
        with SAMPLE_PATH.open("r", encoding="utf-8") as handle:
            return json.load(handle)

    def test_schema_has_required_top_level_shape(self):
        schema = self.load_schema()

        self.assertEqual("ability_schema_v1", schema["properties"]["schema_version"]["const"])
        self.assertIn("schema_version", schema["required"])
        self.assertIn("abilities", schema["required"])
        self.assertEqual("#/$defs/ability", schema["properties"]["abilities"]["items"]["$ref"])

    def test_ability_definition_requires_core_sections(self):
        schema = self.load_schema()
        ability = schema["$defs"]["ability"]

        for field in [
            "ability_id",
            "card_id",
            "kind",
            "trigger",
            "timing",
            "costs",
            "targets",
            "effects",
            "duration",
            "manual_fallback",
        ]:
            self.assertIn(field, ability["required"])

    def test_schema_enums_include_m12_template_targets(self):
        schema = self.load_schema()

        self.assertIn("counter_blast", schema["$defs"]["cost"]["properties"]["type"]["enum"])
        self.assertIn("soul_blast", schema["$defs"]["cost"]["properties"]["type"]["enum"])
        self.assertIn("energy_blast", schema["$defs"]["cost"]["properties"]["type"]["enum"])
        self.assertIn("move_zone", schema["$defs"]["effect"]["properties"]["type"]["enum"])
        self.assertIn("power_plus", schema["$defs"]["effect"]["properties"]["type"]["enum"])
        self.assertIn("critical_plus", schema["$defs"]["effect"]["properties"]["type"]["enum"])
        self.assertIn("OnResourceFlip", schema["$defs"]["timing_window"]["enum"])
        self.assertIn("Soul", schema["$defs"]["zone"]["enum"])
        self.assertIn("GZone", schema["$defs"]["zone"]["enum"])

    def test_sample_uses_schema_version_and_required_sections(self):
        sample = self.load_sample()

        self.assertEqual("ability_schema_v1", sample["schema_version"])
        self.assertEqual(1, len(sample["abilities"]))
        ability = sample["abilities"][0]
        self.assertEqual("sample_bt01_001_auto_01", ability["ability_id"])
        self.assertEqual("auto", ability["kind"])
        self.assertEqual("on_timing", ability["trigger"]["type"])
        self.assertEqual("Main", ability["timing"]["phase"])
        self.assertEqual("instant", ability["duration"]["type"])
        self.assertTrue(ability["manual_fallback"])
        self.assertGreaterEqual(len(ability["costs"]), 1)
        self.assertGreaterEqual(len(ability["targets"]), 1)
        self.assertGreaterEqual(len(ability["effects"]), 1)


if __name__ == "__main__":
    unittest.main()
