from __future__ import annotations

import csv
import hashlib
import json
import sys
import tempfile
import unittest
import zipfile
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "tools/data"))

from validate_local_custom_import import validate_local_custom_import  # noqa: E402


class LocalCustomImportValidatorTests(unittest.TestCase):
    def test_valid_directory_package_validates(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            package = create_valid_package(Path(temp))

            report = validate_local_custom_import(package)

        self.assertTrue(report.accepted, report.to_dict())
        self.assertEqual("local_custom_pack", report.pack_id)
        self.assertEqual(2, report.card_count)
        self.assertEqual(2, report.image_count)
        self.assertEqual(0, report.missing_image_count)

    def test_valid_zip_package_validates(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            package = create_valid_package(Path(temp) / "source")
            archive_path = Path(temp) / "custom_import.zip"
            with zipfile.ZipFile(archive_path, "w", zipfile.ZIP_DEFLATED) as archive:
                for path in package.rglob("*"):
                    archive.write(path, path.relative_to(package))

            report = validate_local_custom_import(archive_path)

        self.assertTrue(report.accepted, report.to_dict())
        self.assertEqual(2, report.card_count)
        self.assertEqual(2, report.image_count)

    def test_missing_manifest_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            report = validate_local_custom_import(Path(temp))

        self.assertFalse(report.accepted)
        self.assertTrue(any("Missing manifest" in error for error in report.errors), report.errors)

    def test_unsupported_xlsx_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            package = create_valid_package(Path(temp))
            cards_path = package / "cards.xlsx"
            cards_path.write_bytes(b"not-real-xlsx")
            rewrite_manifest(package, cards_file="cards.xlsx", sha_files=("cards.xlsx", "images.zip"))

            report = validate_local_custom_import(package)

        self.assertFalse(report.accepted)
        self.assertTrue(any("cards.xlsx is reserved" in error for error in report.errors), report.errors)

    def test_missing_declared_file_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            package = create_valid_package(Path(temp))
            (package / "cards.csv").unlink()

            report = validate_local_custom_import(package)

        self.assertFalse(report.accepted)
        self.assertTrue(any("Declared file does not exist" in error for error in report.errors), report.errors)

    def test_hash_mismatch_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            package = create_valid_package(Path(temp))
            manifest_path = package / "manifest.json"
            manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
            manifest["sha256"]["cards.csv"] = "0" * 64
            manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")

            report = validate_local_custom_import(package)

        self.assertFalse(report.accepted)
        self.assertTrue(any("SHA-256 mismatch" in error for error in report.errors), report.errors)

    def test_unsafe_manifest_path_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            package = create_valid_package(Path(temp))
            manifest_path = package / "manifest.json"
            manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
            manifest["cards_file"] = "../cards.csv"
            manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")

            report = validate_local_custom_import(package)

        self.assertFalse(report.accepted)
        self.assertTrue(any("cards_file" in error and "safe relative path" in error for error in report.errors), report.errors)

    def test_images_zip_traversal_is_rejected_without_extraction(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            package = create_valid_package(Path(temp))
            with zipfile.ZipFile(package / "images.zip", "w", zipfile.ZIP_DEFLATED) as archive:
                archive.writestr("../evil.png", b"bad")
            rewrite_manifest(package, sha_files=("cards.csv", "images.zip"))

            report = validate_local_custom_import(package)

            self.assertFalse((package.parent / "evil.png").exists())

        self.assertFalse(report.accepted)
        self.assertTrue(any("Unsafe images.zip member path" in error for error in report.errors), report.errors)

    def test_invalid_abilities_json_is_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as temp:
            package = create_valid_package(Path(temp))
            (package / "abilities.json").write_text("{bad json", encoding="utf-8")
            rewrite_manifest(package, abilities_file="abilities.json", sha_files=("cards.csv", "images.zip", "abilities.json"))

            report = validate_local_custom_import(package)

        self.assertFalse(report.accepted)
        self.assertTrue(any("Invalid abilities.json" in error for error in report.errors), report.errors)


CARD_COLUMNS = (
    "card_id",
    "name_th",
    "series",
    "clan",
    "card_type",
    "grade",
    "power",
    "shield",
    "critical",
    "trigger_type",
    "text_th",
    "image_relative_path",
)


def create_valid_package(root: Path) -> Path:
    package = root / "custom_import"
    package.mkdir(parents=True, exist_ok=True)
    write_cards(package / "cards.csv")
    with zipfile.ZipFile(package / "images.zip", "w", zipfile.ZIP_DEFLATED) as archive:
        archive.writestr("cards/card-001.png", b"image-one")
        archive.writestr("cards/card-002.png", b"image-two")
    rewrite_manifest(package, sha_files=("cards.csv", "images.zip"))
    return package


def write_cards(path: Path) -> None:
    rows = [
        {
            "card_id": "LCP-001",
            "name_th": "Local Unit One",
            "series": "Local Series",
            "clan": "Royal Paladin",
            "card_type": "Normal Unit",
            "grade": "1",
            "power": "8000",
            "shield": "5000",
            "critical": "1",
            "trigger_type": "",
            "text_th": "Text one",
            "image_relative_path": "cards/card-001.png",
        },
        {
            "card_id": "LCP-002",
            "name_th": "Local Unit Two",
            "series": "Local Series",
            "clan": "Royal Paladin",
            "card_type": "Trigger Unit",
            "grade": "0",
            "power": "5000",
            "shield": "15000",
            "critical": "1",
            "trigger_type": "critical",
            "text_th": "Text two",
            "image_relative_path": "cards/card-002.png",
        },
    ]
    with path.open("w", encoding="utf-8", newline="") as f:
        writer = csv.DictWriter(f, fieldnames=list(CARD_COLUMNS))
        writer.writeheader()
        writer.writerows(rows)


def rewrite_manifest(
    package: Path,
    *,
    cards_file: str = "cards.csv",
    abilities_file: str | None = None,
    sha_files: tuple[str, ...],
) -> None:
    manifest = {
        "schema": "vanguard-custom-import-v1",
        "pack_id": "local_custom_pack",
        "display_name": "Local Custom Pack",
        "source_version": "1.0.0",
        "language": "th",
        "format": "custom",
        "cards_file": cards_file,
        "images_zip": "images.zip",
        "sha256": {},
    }
    if abilities_file:
        manifest["abilities_file"] = abilities_file
    for relative_path in sha_files:
        manifest["sha256"][relative_path] = sha256_file(package / relative_path)
    (package / "manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


if __name__ == "__main__":
    unittest.main()

