from __future__ import annotations

import csv
import json
import shutil
import sqlite3
import sys
import tempfile
import unittest
import zipfile
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools/data"))

from validate_custom_pack_schema import (  # noqa: E402
    RECOMMENDED_CARD_COLUMNS,
    validate_source_dir,
)
from import_custom_pack import import_custom_pack, source_context  # noqa: E402


TEMPLATE_DIR = ROOT / "data/templates/custom_pack"
V2_TEMPLATE_DIR = ROOT / "data/templates/custom_pack_v2"


class CustomPackSchemaTests(unittest.TestCase):
    def test_template_validates(self) -> None:
        report = validate_source_dir(TEMPLATE_DIR)

        self.assertTrue(report.all_ok, report.to_dict())
        self.assertEqual(1, report.schema_version)
        self.assertEqual(2, report.card_count)
        self.assertIsNotNone(report.definition_hash)
        self.assertIsNotNone(report.image_manifest_hash)

    def test_v2_template_validates(self) -> None:
        report = validate_source_dir(V2_TEMPLATE_DIR)

        self.assertTrue(report.all_ok, report.to_dict())
        self.assertEqual(2, report.schema_version)
        self.assertEqual(2, report.card_count)
        self.assertEqual(0, report.dependency_count)
        self.assertEqual(
            {
                "cards": True,
                "images": True,
                "abilities": True,
                "custom_formats": False,
            },
            report.capabilities,
        )
        self.assertEqual("abilities.json", report.abilities_file)
        self.assertEqual(1, report.ability_count)
        self.assertIsNotNone(report.ability_data_hash)

    def test_v2_rejects_ability_for_unknown_card(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            source_dir = copy_template(Path(temp), source=V2_TEMPLATE_DIR)
            ability_path = source_dir / "abilities.json"
            data = json.loads(ability_path.read_text(encoding="utf-8"))
            data["abilities"][0]["card_id"] = "MISSING-CARD"
            ability_path.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")

            report = validate_source_dir(source_dir)

        self.assertFalse(report.all_ok)
        self.assertTrue(any("references card_id not present" in error for error in report.errors), report.errors)

    def test_v2_rejects_invalid_ability_schema(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            source_dir = copy_template(Path(temp), source=V2_TEMPLATE_DIR)
            ability_path = source_dir / "abilities.json"
            data = json.loads(ability_path.read_text(encoding="utf-8"))
            data["abilities"][0]["effects"] = []
            ability_path.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")

            report = validate_source_dir(source_dir)

        self.assertFalse(report.all_ok)
        self.assertTrue(any("abilities_file:" in error and "`effects`" in error for error in report.errors), report.errors)

    def test_v2_requires_cards_capability(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            source_dir = copy_template(Path(temp), source=V2_TEMPLATE_DIR)
            pack_path = source_dir / "pack.json"
            pack = json.loads(pack_path.read_text(encoding="utf-8"))
            pack["capabilities"]["cards"] = False
            pack_path.write_text(json.dumps(pack, ensure_ascii=False, indent=2), encoding="utf-8")

            report = validate_source_dir(source_dir)

        self.assertFalse(report.all_ok)
        self.assertTrue(any("capabilities.cards" in error for error in report.errors), report.errors)

    def test_v2_rejects_unsafe_abilities_file(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            source_dir = copy_template(Path(temp), source=V2_TEMPLATE_DIR)
            pack_path = source_dir / "pack.json"
            pack = json.loads(pack_path.read_text(encoding="utf-8"))
            pack["capabilities"]["abilities"] = True
            pack["abilities_file"] = "../abilities.json"
            pack_path.write_text(json.dumps(pack, ensure_ascii=False, indent=2), encoding="utf-8")

            report = validate_source_dir(source_dir)

        self.assertFalse(report.all_ok)
        self.assertTrue(any("abilities_file" in error and "safe relative path" in error for error in report.errors), report.errors)

    def test_v2_rejects_invalid_dependency_pack_id(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            source_dir = copy_template(Path(temp), source=V2_TEMPLATE_DIR)
            pack_path = source_dir / "pack.json"
            pack = json.loads(pack_path.read_text(encoding="utf-8"))
            pack["dependencies"] = [{"pack_id": "../bad"}]
            pack_path.write_text(json.dumps(pack, ensure_ascii=False, indent=2), encoding="utf-8")

            report = validate_source_dir(source_dir)

        self.assertFalse(report.all_ok)
        self.assertTrue(any("dependency 1 `pack_id`" in error for error in report.errors), report.errors)

    def test_duplicate_card_id_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            source_dir = copy_template(Path(temp))
            rows = read_rows(source_dir / "cards.csv")
            rows[1]["card_id"] = rows[0]["card_id"]
            write_rows(source_dir / "cards.csv", rows)

            report = validate_source_dir(source_dir)

        self.assertFalse(report.all_ok)
        self.assertTrue(any("duplicate `card_id`" in error for error in report.errors), report.errors)

    def test_unsafe_image_path_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            source_dir = copy_template(Path(temp))
            rows = read_rows(source_dir / "cards.csv")
            rows[0]["image_file"] = "../outside.png"
            write_rows(source_dir / "cards.csv", rows)

            report = validate_source_dir(source_dir)

        self.assertFalse(report.all_ok)
        self.assertTrue(any("image_file" in error and "safe relative path" in error for error in report.errors), report.errors)

    def test_definition_hash_is_stable_when_rows_are_reordered(self) -> None:
        with tempfile.TemporaryDirectory() as left_temp, tempfile.TemporaryDirectory() as right_temp:
            left_dir = copy_template(Path(left_temp))
            right_dir = copy_template(Path(right_temp))
            rows = read_rows(right_dir / "cards.csv")
            write_rows(right_dir / "cards.csv", list(reversed(rows)))

            left = validate_source_dir(left_dir)
            right = validate_source_dir(right_dir)

        self.assertTrue(left.all_ok, left.to_dict())
        self.assertTrue(right.all_ok, right.to_dict())
        self.assertEqual(left.definition_hash, right.definition_hash)
        self.assertEqual(left.image_manifest_hash, right.image_manifest_hash)

    def test_import_custom_pack_builds_runtime_sqlite(self) -> None:
        with tempfile.TemporaryDirectory() as source_temp, tempfile.TemporaryDirectory() as output_temp:
            source_dir = copy_template(Path(source_temp))
            output_dir = Path(output_temp) / "runtime_pack"

            manifest = import_custom_pack(source_dir, output_dir, overwrite=True)

            self.assertEqual("custom_starter_pack", manifest["pack_id"])
            self.assertEqual(2, manifest["card_count"])
            self.assertEqual(0, manifest["existing_image_count"])
            self.assertTrue((output_dir / "manifest.json").exists())
            self.assertTrue((output_dir / "cards.sqlite").exists())
            self.assertTrue((output_dir / "asset_index.json").exists())

            conn = sqlite3.connect(output_dir / "cards.sqlite")
            try:
                self.assertEqual(2, conn.execute("SELECT COUNT(*) FROM cards").fetchone()[0])
                row = conn.execute("SELECT name_th, image_exists FROM cards JOIN card_images USING(card_id) WHERE card_id = ?", ("CSTM-001",)).fetchone()
                self.assertEqual("Custom Vanguard", row[0])
                self.assertEqual(0, row[1])
            finally:
                conn.close()

    def test_import_custom_pack_copies_images_when_present(self) -> None:
        with tempfile.TemporaryDirectory() as source_temp, tempfile.TemporaryDirectory() as output_temp:
            source_dir = copy_template(Path(source_temp))
            image_path = source_dir / "images" / "sample.png"
            image_path.write_bytes(b"not-a-real-png-but-content-addressed")
            rows = read_rows(source_dir / "cards.csv")
            rows[0]["image_file"] = "sample.png"
            write_rows(source_dir / "cards.csv", rows)

            output_dir = Path(output_temp) / "runtime_pack"
            manifest = import_custom_pack(source_dir, output_dir, overwrite=True)
            asset_index = json.loads((output_dir / "asset_index.json").read_text(encoding="utf-8"))

            self.assertEqual(1, manifest["existing_image_count"])
            self.assertEqual(1, asset_index["image_count"])
            self.assertTrue((output_dir / "images" / "sample.png").exists())

    def test_import_custom_pack_from_zip_source(self) -> None:
        with tempfile.TemporaryDirectory() as source_temp, tempfile.TemporaryDirectory() as output_temp:
            source_dir = copy_template(Path(source_temp))
            archive_path = Path(source_temp) / "custom_pack.zip"
            with zipfile.ZipFile(archive_path, "w", zipfile.ZIP_DEFLATED) as archive:
                for path in source_dir.rglob("*"):
                    archive.write(path, path.relative_to(source_dir.parent))

            with source_context(archive_path) as extracted:
                manifest = import_custom_pack(extracted, Path(output_temp) / "runtime_pack", overwrite=True)

            self.assertEqual("custom_starter_pack", manifest["pack_id"])
            self.assertEqual(2, manifest["card_count"])

    def test_import_custom_pack_v2_preserves_source_metadata(self) -> None:
        with tempfile.TemporaryDirectory() as source_temp, tempfile.TemporaryDirectory() as output_temp:
            source_dir = copy_template(Path(source_temp), source=V2_TEMPLATE_DIR)
            output_dir = Path(output_temp) / "runtime_pack"

            manifest = import_custom_pack(source_dir, output_dir, overwrite=True)

            self.assertEqual("custom_starter_pack_v2", manifest["pack_id"])
            self.assertEqual(2, manifest["source_schema_version"])
            self.assertEqual("custom", manifest["source_ruleset_profile"])
        self.assertEqual(
            {
                "cards": True,
                "images": True,
                "abilities": True,
                "custom_formats": False,
            },
            manifest["source_capabilities"],
        )
        self.assertEqual("abilities.json", manifest["source_abilities_file"])
        self.assertEqual(1, manifest["source_ability_count"])
        self.assertIsNotNone(manifest["source_ability_data_hash"])

    def test_failed_import_does_not_mutate_existing_output(self) -> None:
        with tempfile.TemporaryDirectory() as source_temp, tempfile.TemporaryDirectory() as output_temp:
            source_dir = copy_template(Path(source_temp))
            rows = read_rows(source_dir / "cards.csv")
            rows[1]["card_id"] = rows[0]["card_id"]
            write_rows(source_dir / "cards.csv", rows)

            output_dir = Path(output_temp) / "runtime_pack"
            output_dir.mkdir()
            sentinel = output_dir / "existing.txt"
            sentinel.write_text("keep", encoding="utf-8")

            with self.assertRaises(ValueError):
                import_custom_pack(source_dir, output_dir, overwrite=True)

            self.assertTrue(sentinel.exists())
            self.assertEqual("keep", sentinel.read_text(encoding="utf-8"))
            self.assertFalse((output_dir / "cards.sqlite").exists())

    def test_custom_pack_import_output_isolated_from_default_runtime_pack(self) -> None:
        with tempfile.TemporaryDirectory() as source_temp, tempfile.TemporaryDirectory() as output_temp:
            source_dir = copy_template(Path(source_temp))
            output_dir = Path(output_temp) / "runtime_pack"

            manifest = import_custom_pack(source_dir, output_dir, overwrite=True)

            default_pack = (ROOT / "data/packs/vanguard_th").resolve()
            imported_manifest = (output_dir / "manifest.json").resolve()
            self.assertFalse(str(imported_manifest).startswith(str(default_pack)))
            self.assertEqual("custom_pack", manifest["source"])
            self.assertTrue((output_dir / "cards.sqlite").exists())


def copy_template(temp: Path, *, source: Path = TEMPLATE_DIR) -> Path:
    target = temp / "custom_pack"
    shutil.copytree(source, target)
    return target


def read_rows(path: Path) -> list[dict[str, str]]:
    with path.open("r", encoding="utf-8", newline="") as f:
        return list(csv.DictReader(f))


def write_rows(path: Path, rows: list[dict[str, str]]) -> None:
    with path.open("w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=list(RECOMMENDED_CARD_COLUMNS))
        writer.writeheader()
        writer.writerows(rows)


if __name__ == "__main__":
    unittest.main()
