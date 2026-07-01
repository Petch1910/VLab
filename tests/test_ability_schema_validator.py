from __future__ import annotations

import copy
import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools/data"))

from validate_ability_schema import validate_ability_file  # noqa: E402


SAMPLE_PATH = ROOT / "data/templates/ability_schema_v1/sample_abilities.json"


class AbilitySchemaValidatorTests(unittest.TestCase):
    def test_sample_ability_file_validates(self) -> None:
        report = validate_ability_file(SAMPLE_PATH)

        self.assertTrue(report.all_ok, report.to_dict())
        self.assertEqual(1, report.ability_count)
        self.assertIn(report.validator_backend, {"builtin", "pydantic"})

    def test_duplicate_ability_id_is_rejected(self) -> None:
        data = load_sample()
        data["abilities"].append(copy.deepcopy(data["abilities"][0]))

        report = validate_temp(data)

        self.assertFalse(report.all_ok)
        self.assertTrue(any("duplicate `ability_id`" in error for error in report.errors), report.errors)

    def test_missing_required_section_is_rejected(self) -> None:
        data = load_sample()
        del data["abilities"][0]["trigger"]

        report = validate_temp(data)

        self.assertFalse(report.all_ok)
        self.assertTrue(any("missing required field `trigger`" in error for error in report.errors), report.errors)

    def test_invalid_enums_are_rejected(self) -> None:
        data = load_sample()
        data["abilities"][0]["timing"]["phase"] = "WrongPhase"
        data["abilities"][0]["effects"][0]["type"] = "gain_everything"

        report = validate_temp(data)

        self.assertFalse(report.all_ok)
        self.assertTrue(any("phase" in error and "WrongPhase" not in error for error in report.errors), report.errors)
        self.assertTrue(any("type" in error and "gain_everything" not in error for error in report.errors), report.errors)

    def test_once_cost_requires_key(self) -> None:
        data = load_sample()
        data["abilities"][0]["costs"][1]["key"] = ""

        report = validate_temp(data)

        self.assertFalse(report.all_ok)
        self.assertTrue(any("once_per_turn" in error and "requires `key`" in error for error in report.errors), report.errors)


def load_sample() -> dict:
    return json.loads(SAMPLE_PATH.read_text(encoding="utf-8"))


def validate_temp(data: dict):
    with tempfile.TemporaryDirectory() as temp:
        path = Path(temp) / "abilities.json"
        path.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")
        return validate_ability_file(path)


if __name__ == "__main__":
    unittest.main()
