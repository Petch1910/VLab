"""Validate a local custom import package without staging or importing it."""

from __future__ import annotations

import argparse
import csv
import hashlib
import io
import json
import re
import sys
import zipfile
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any, Iterable

from validate_ability_schema import validate_ability_file
from validate_custom_pack_schema import ALLOWED_IMAGE_EXTENSIONS, PACK_ID_RE, is_safe_relative_path


ADAPTER_NAME = "vangpro_like_local_import_v1"
MANIFEST_NAME = "manifest.json"
SUPPORTED_SCHEMA = "vanguard-custom-import-v1"
REQUIRED_MANIFEST_FIELDS = (
    "schema",
    "pack_id",
    "display_name",
    "source_version",
    "language",
    "format",
    "cards_file",
)
REQUIRED_CARD_COLUMNS = (
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
SHA256_RE = re.compile(r"^[a-fA-F0-9]{64}$")


@dataclass
class LocalCustomImportReport:
    source: str
    adapter: str = ADAPTER_NAME
    pack_id: str | None = None
    display_name: str | None = None
    source_version: str | None = None
    language: str | None = None
    format: str | None = None
    cards_file: str | None = None
    images_zip: str | None = None
    abilities_file: str | None = None
    card_count: int = 0
    image_count: int = 0
    missing_image_count: int = 0
    unsupported_field_count: int = 0
    errors: list[str] = field(default_factory=list)
    warnings: list[str] = field(default_factory=list)

    @property
    def accepted(self) -> bool:
        return not self.errors

    def to_dict(self) -> dict[str, Any]:
        return {
            "adapter": self.adapter,
            "accepted": self.accepted,
            "source": self.source,
            "pack_id": self.pack_id,
            "display_name": self.display_name,
            "source_version": self.source_version,
            "language": self.language,
            "format": self.format,
            "cards_file": self.cards_file,
            "images_zip": self.images_zip,
            "abilities_file": self.abilities_file,
            "card_count": self.card_count,
            "image_count": self.image_count,
            "missing_image_count": self.missing_image_count,
            "unsupported_field_count": self.unsupported_field_count,
            "errors": self.errors,
            "warnings": self.warnings,
            "validation_handoff": {
                "custom_pack_schema_validator": "tools/data/validate_custom_pack_schema.py",
                "custom_pack_importer": "tools/data/import_custom_pack.py",
                "direct_runtime_mutation": False,
            },
        }


class PackageReader:
    def exists(self, relative_path: str) -> bool:
        raise NotImplementedError

    def read_bytes(self, relative_path: str) -> bytes:
        raise NotImplementedError


class DirectoryPackageReader(PackageReader):
    def __init__(self, root: Path):
        self.root = root

    def exists(self, relative_path: str) -> bool:
        path = (self.root / relative_path).resolve()
        return is_child_path(path, self.root.resolve()) and path.is_file()

    def read_bytes(self, relative_path: str) -> bytes:
        path = (self.root / relative_path).resolve()
        if not is_child_path(path, self.root.resolve()):
            raise ValueError(f"Unsafe package path: {relative_path}")
        return path.read_bytes()


class ZipPackageReader(PackageReader):
    def __init__(self, archive: zipfile.ZipFile, report: LocalCustomImportReport):
        self.archive = archive
        self.entries: dict[str, zipfile.ZipInfo] = {}
        for entry in archive.infolist():
            normalized = normalize_archive_name(entry.filename)
            if not normalized:
                continue
            if not is_safe_relative_path(normalized):
                report.errors.append(f"Unsafe package zip member path: {entry.filename}")
                continue
            if normalized in self.entries and not entry.is_dir():
                report.errors.append(f"Duplicate package zip member path: {normalized}")
                continue
            self.entries[normalized] = entry

    def exists(self, relative_path: str) -> bool:
        entry = self.entries.get(normalize_archive_name(relative_path))
        return entry is not None and not entry.is_dir()

    def read_bytes(self, relative_path: str) -> bytes:
        normalized = normalize_archive_name(relative_path)
        entry = self.entries.get(normalized)
        if entry is None or entry.is_dir():
            raise FileNotFoundError(relative_path)
        return self.archive.read(entry)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate a local custom import package.")
    parser.add_argument("source", type=Path, help="Directory or .zip package containing manifest.json.")
    parser.add_argument("--json", action="store_true", help="Print machine-readable JSON report.")
    return parser.parse_args()


def normalize_archive_name(value: str) -> str:
    return value.replace("\\", "/").strip("/")


def is_child_path(path: Path, parent: Path) -> bool:
    try:
        path.relative_to(parent)
        return True
    except ValueError:
        return False


def sha256_bytes(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def text(value: Any) -> str:
    if value is None:
        return ""
    return str(value).strip()


def validate_local_custom_import(source: Path) -> LocalCustomImportReport:
    source = source.resolve()
    report = LocalCustomImportReport(source=str(source))
    if not source.exists():
        report.errors.append(f"Source does not exist: {source}")
        return report
    if source.is_dir():
        return validate_package(DirectoryPackageReader(source), report)
    if source.suffix.lower() != ".zip":
        report.errors.append("Source must be a directory or .zip package.")
        return report

    try:
        with zipfile.ZipFile(source) as archive:
            return validate_package(ZipPackageReader(archive, report), report)
    except zipfile.BadZipFile as exc:
        report.errors.append(f"Invalid source zip: {exc}")
        return report


def validate_package(reader: PackageReader, report: LocalCustomImportReport) -> LocalCustomImportReport:
    manifest = read_manifest(reader, report)
    if not manifest:
        return report

    validate_manifest_shape(manifest, report)
    if report.errors:
        return report

    hash_manifest = manifest.get("sha256")
    if not isinstance(hash_manifest, dict):
        report.errors.append("manifest.json is missing required object `sha256`.")
        return report

    cards_file = report.cards_file or ""
    if not validate_declared_file(reader, cards_file, "cards_file", hash_manifest, report, expected_suffixes={".csv", ".xlsx"}):
        return report
    if Path(cards_file).suffix.lower() == ".xlsx":
        report.errors.append("cards.xlsx is reserved but not supported by the local import validator yet.")
        return report

    rows, image_paths = read_and_validate_cards(reader.read_bytes(cards_file), report)

    image_members: set[str] = set()
    if report.images_zip:
        if validate_declared_file(reader, report.images_zip, "images_zip", hash_manifest, report, expected_suffixes={".zip"}):
            image_members = inspect_images_zip(reader.read_bytes(report.images_zip), report)

    if report.abilities_file:
        if validate_declared_file(reader, report.abilities_file, "abilities_file", hash_manifest, report, expected_suffixes={".json"}):
            validate_abilities_json(reader.read_bytes(report.abilities_file), report)

    if image_members:
        missing = sorted(path for path in image_paths if path and path not in image_members)
        report.missing_image_count = len(missing)
        for path in missing[:10]:
            report.warnings.append(f"Card image not found in images.zip: {path}")
        if len(missing) > 10:
            report.warnings.append(f"{len(missing) - 10} additional card images are missing from images.zip.")
    elif image_paths and not report.images_zip:
        report.missing_image_count = len(image_paths)
        report.warnings.append("Cards reference images but manifest does not declare `images_zip`.")

    report.card_count = len(rows)
    return report


def read_manifest(reader: PackageReader, report: LocalCustomImportReport) -> dict[str, Any]:
    if not reader.exists(MANIFEST_NAME):
        report.errors.append("Missing manifest.json.")
        return {}
    try:
        data = json.loads(reader.read_bytes(MANIFEST_NAME).decode("utf-8"))
    except UnicodeDecodeError as exc:
        report.errors.append(f"manifest.json must be UTF-8: {exc}")
        return {}
    except json.JSONDecodeError as exc:
        report.errors.append(f"Invalid manifest.json: {exc}")
        return {}
    if not isinstance(data, dict):
        report.errors.append("manifest.json must contain a JSON object.")
        return {}
    return data


def validate_manifest_shape(manifest: dict[str, Any], report: LocalCustomImportReport) -> None:
    for field_name in REQUIRED_MANIFEST_FIELDS:
        if field_name not in manifest:
            report.errors.append(f"manifest.json is missing required field `{field_name}`.")

    schema = text(manifest.get("schema"))
    if schema and schema != SUPPORTED_SCHEMA:
        report.errors.append(f"manifest.json `schema` must be `{SUPPORTED_SCHEMA}`.")

    pack_id = text(manifest.get("pack_id"))
    if pack_id and not PACK_ID_RE.match(pack_id):
        report.errors.append("manifest.json `pack_id` must match ^[a-z0-9][a-z0-9_-]{2,63}$.")

    report.pack_id = pack_id or None
    report.display_name = text(manifest.get("display_name")) or None
    report.source_version = text(manifest.get("source_version")) or None
    report.language = text(manifest.get("language")) or None
    report.format = text(manifest.get("format")) or None
    report.cards_file = validate_manifest_path_field(manifest, "cards_file", report, required=True)
    report.images_zip = validate_manifest_path_field(manifest, "images_zip", report, required=False)
    report.abilities_file = validate_manifest_path_field(manifest, "abilities_file", report, required=False)


def validate_manifest_path_field(
    manifest: dict[str, Any],
    field_name: str,
    report: LocalCustomImportReport,
    *,
    required: bool,
) -> str | None:
    value = text(manifest.get(field_name))
    if not value:
        if required:
            report.errors.append(f"manifest.json `{field_name}` is required.")
        return None
    normalized = value.replace("\\", "/")
    if not is_safe_relative_path(normalized):
        report.errors.append(f"manifest.json `{field_name}` must be a safe relative path.")
        return normalized
    return normalized


def validate_declared_file(
    reader: PackageReader,
    relative_path: str,
    field_name: str,
    hash_manifest: dict[str, Any],
    report: LocalCustomImportReport,
    *,
    expected_suffixes: set[str],
) -> bool:
    if not relative_path:
        return False
    suffix = Path(relative_path).suffix.lower()
    if suffix not in expected_suffixes:
        report.errors.append(f"manifest.json `{field_name}` must point to: {', '.join(sorted(expected_suffixes))}.")
        return False
    if not is_safe_relative_path(relative_path):
        return False
    if not reader.exists(relative_path):
        report.errors.append(f"Declared file does not exist: {relative_path}")
        return False

    expected_hash = text(hash_manifest.get(relative_path))
    if not expected_hash:
        report.errors.append(f"manifest.json `sha256` is missing entry for `{relative_path}`.")
        return False
    if not SHA256_RE.match(expected_hash):
        report.errors.append(f"manifest.json `sha256.{relative_path}` must be a SHA-256 hex string.")
        return False

    actual_hash = sha256_bytes(reader.read_bytes(relative_path))
    if actual_hash.lower() != expected_hash.lower():
        report.errors.append(f"SHA-256 mismatch for `{relative_path}`.")
        return False
    return True


def read_and_validate_cards(data: bytes, report: LocalCustomImportReport) -> tuple[list[dict[str, str]], list[str]]:
    try:
        text_data = data.decode("utf-8-sig")
    except UnicodeDecodeError as exc:
        report.errors.append(f"cards.csv must be UTF-8: {exc}")
        return [], []

    try:
        sample = text_data[:4096]
        dialect = csv.Sniffer().sniff(sample) if sample.strip() else csv.excel
        reader = csv.DictReader(io.StringIO(text_data), dialect=dialect)
        headers = list(reader.fieldnames or [])
        rows = [{key: value or "" for key, value in row.items() if key is not None} for row in reader]
    except csv.Error as exc:
        report.errors.append(f"Invalid cards.csv: {exc}")
        return [], []

    header_set = set(headers)
    for column in REQUIRED_CARD_COLUMNS:
        if column not in header_set:
            report.errors.append(f"cards.csv is missing required column `{column}`.")

    unsupported = sorted(column for column in header_set if column not in REQUIRED_CARD_COLUMNS)
    report.unsupported_field_count = len(unsupported)
    for column in unsupported:
        report.warnings.append(f"cards.csv column `{column}` is not used by local import v1.")

    if not rows:
        report.errors.append("cards.csv must contain at least one card row.")
        return rows, []

    image_paths: list[str] = []
    seen_ids: set[str] = set()
    for index, row in enumerate(rows, start=2):
        card_id = text(row.get("card_id"))
        if not card_id:
            report.errors.append(f"Row {index}: `card_id` is required.")
        elif card_id in seen_ids:
            report.errors.append(f"Row {index}: duplicate `card_id`: {card_id}")
        seen_ids.add(card_id)

        for field_name in ("name_th", "series", "clan", "card_type", "text_th"):
            if not text(row.get(field_name)):
                report.errors.append(f"Row {index}: `{field_name}` is required.")

        for field_name in ("grade", "power", "shield", "critical"):
            validate_integer(row.get(field_name), index, field_name, report)

        image_path = text(row.get("image_relative_path")).replace("\\", "/")
        if image_path:
            if not is_safe_relative_path(image_path):
                report.errors.append(f"Row {index}: `image_relative_path` must be a safe relative path.")
            elif Path(image_path).suffix.lower() not in ALLOWED_IMAGE_EXTENSIONS:
                report.errors.append(f"Row {index}: `image_relative_path` has unsupported extension: {image_path}")
            else:
                image_paths.append(image_path)
        else:
            report.warnings.append(f"Row {index}: `image_relative_path` is empty; runtime will use fallback art.")

    return rows, image_paths


def validate_integer(value: Any, row_number: int, field_name: str, report: LocalCustomImportReport) -> None:
    raw = text(value).replace(",", "")
    if not raw:
        return
    try:
        int(raw)
    except ValueError:
        report.errors.append(f"Row {row_number}: `{field_name}` must be an integer.")


def inspect_images_zip(data: bytes, report: LocalCustomImportReport) -> set[str]:
    image_members: set[str] = set()
    try:
        with zipfile.ZipFile(io.BytesIO(data)) as archive:
            for entry in archive.infolist():
                normalized = normalize_archive_name(entry.filename)
                if not normalized:
                    continue
                if not is_safe_relative_path(normalized):
                    report.errors.append(f"Unsafe images.zip member path: {entry.filename}")
                    continue
                if entry.is_dir():
                    continue
                suffix = Path(normalized).suffix.lower()
                if suffix in ALLOWED_IMAGE_EXTENSIONS:
                    image_members.add(normalized)
                else:
                    report.warnings.append(f"images.zip contains unsupported file type: {normalized}")
    except zipfile.BadZipFile as exc:
        report.errors.append(f"Invalid images.zip: {exc}")
        return set()

    report.image_count = len(image_members)
    return image_members


def validate_abilities_json(data: bytes, report: LocalCustomImportReport) -> None:
    try:
        json.loads(data.decode("utf-8"))
    except UnicodeDecodeError as exc:
        report.errors.append(f"abilities.json must be UTF-8: {exc}")
        return
    except json.JSONDecodeError as exc:
        report.errors.append(f"Invalid abilities.json: {exc}")
        return

    with io.BytesIO(data):
        # The existing validator reads from a path. Keep this handoff temporary
        # and non-mutating by writing only to an OS temp file.
        import tempfile

        with tempfile.TemporaryDirectory(prefix="local_custom_abilities_") as temp:
            path = Path(temp) / "abilities.json"
            path.write_bytes(data)
            ability_report = validate_ability_file(path)

    for error in ability_report.errors:
        report.errors.append(f"abilities_file: {error}")
    for warning in ability_report.warnings:
        report.warnings.append(f"abilities_file: {warning}")


def print_human_report(report: LocalCustomImportReport) -> None:
    print(f"Local custom import: {report.source}")
    print(f"Pack id: {report.pack_id or '-'}")
    print(f"Cards file: {report.cards_file or '-'}")
    print(f"Images zip: {report.images_zip or '-'}")
    print(f"Abilities file: {report.abilities_file or '-'}")
    print(f"Cards: {report.card_count}")
    print(f"Images: {report.image_count}")
    print(f"Missing images: {report.missing_image_count}")
    print(f"Errors: {len(report.errors)}")
    for error in report.errors:
        print(f"  ERROR: {error}")
    print(f"Warnings: {len(report.warnings)}")
    for warning in report.warnings:
        print(f"  WARNING: {warning}")
    print(f"Accepted: {report.accepted}")


def main() -> int:
    args = parse_args()
    report = validate_local_custom_import(args.source)
    if args.json:
        print(json.dumps(report.to_dict(), ensure_ascii=False, indent=2))
    else:
        print_human_report(report)
    return 0 if report.accepted else 1


if __name__ == "__main__":
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    if hasattr(sys.stderr, "reconfigure"):
        sys.stderr.reconfigure(encoding="utf-8", errors="replace")
    raise SystemExit(main())

