"""Validate a custom card pack source directory.

M7-01 validates the source contract only. M7-02 will reuse this validator before
building SQLite/runtime manifest files.
"""

from __future__ import annotations

import argparse
import csv
import hashlib
import json
import re
import sys
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any

from validate_ability_schema import validate_ability_file


ROOT = Path(__file__).resolve().parents[2]

PACK_ID_RE = re.compile(r"^[a-z0-9][a-z0-9_-]{2,63}$")
CARD_ID_RE = re.compile(r"^[A-Za-z0-9][A-Za-z0-9_.:-]{1,63}$")
SUPPORTED_PACK_SCHEMA_VERSIONS = {1, 2}
SAFE_FORMATS = {"standard", "v_premium", "premium", "mixed", "custom"}
SAFE_RULESET_PROFILES = {"standard", "v_premium", "premium", "mixed", "custom"}
ALLOWED_IMAGE_EXTENSIONS = {".png", ".jpg", ".jpeg", ".webp"}
ALLOWED_TRIGGERS = {"", "critical", "draw", "front", "heal", "stand", "over", "none"}
SUPPORTED_CARD_FILES = {".csv", ".xlsx"}
SUPPORTED_AUXILIARY_JSON_FILES = {".json"}
SUPPORTED_V2_CAPABILITIES = ("cards", "images", "abilities", "custom_formats")

REQUIRED_PACK_FIELDS = (
    "schema_version",
    "pack_id",
    "display_name",
    "source_version",
    "format",
    "language",
    "cards_file",
    "image_root",
)

REQUIRED_CARD_COLUMNS = (
    "card_id",
    "name",
    "text",
    "series",
    "clan",
    "grade",
    "deck_limit",
    "image_file",
)

RECOMMENDED_CARD_COLUMNS = (
    "card_id",
    "name",
    "language",
    "format",
    "series",
    "series_code",
    "nation",
    "clan",
    "race",
    "type_1",
    "type_2",
    "grade",
    "power",
    "shield",
    "critical",
    "trigger",
    "deck_limit",
    "ability_timing",
    "ability_tags",
    "text",
    "flavor_text",
    "image_file",
    "artist",
    "rarity",
    "notes",
)

DEFINITION_HASH_EXCLUDED_COLUMNS = {"image_file", "notes"}


@dataclass
class ValidationReport:
    source_dir: str
    schema_version: int | None = None
    cards_file: str | None = None
    abilities_file: str | None = None
    formats_file: str | None = None
    dependency_count: int = 0
    capabilities: dict[str, bool] = field(default_factory=dict)
    ability_count: int = 0
    ability_data_hash: str | None = None
    card_count: int = 0
    definition_hash: str | None = None
    image_manifest_hash: str | None = None
    errors: list[str] = field(default_factory=list)
    warnings: list[str] = field(default_factory=list)

    @property
    def all_ok(self) -> bool:
        return not self.errors

    def to_dict(self) -> dict[str, Any]:
        return {
            "source_dir": self.source_dir,
            "schema_version": self.schema_version,
            "cards_file": self.cards_file,
            "abilities_file": self.abilities_file,
            "formats_file": self.formats_file,
            "dependency_count": self.dependency_count,
            "capabilities": self.capabilities,
            "ability_count": self.ability_count,
            "ability_data_hash": self.ability_data_hash,
            "card_count": self.card_count,
            "definition_hash": self.definition_hash,
            "image_manifest_hash": self.image_manifest_hash,
            "errors": self.errors,
            "warnings": self.warnings,
            "all_ok": self.all_ok,
        }


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate custom card pack source schema.")
    parser.add_argument("source_dir", type=Path)
    parser.add_argument("--strict-images", action="store_true", help="Treat missing image files as errors.")
    parser.add_argument("--json", action="store_true", help="Print machine-readable JSON report.")
    return parser.parse_args()


def read_json(path: Path, report: ValidationReport) -> dict[str, Any]:
    try:
        with path.open("r", encoding="utf-8") as f:
            data = json.load(f)
    except FileNotFoundError:
        report.errors.append(f"Missing pack metadata: {path.name}")
        return {}
    except json.JSONDecodeError as exc:
        report.errors.append(f"Invalid JSON in {path.name}: {exc}")
        return {}

    if not isinstance(data, dict):
        report.errors.append(f"{path.name} must contain a JSON object.")
        return {}
    return data


def is_safe_relative_path(value: str) -> bool:
    if not value:
        return False
    path = Path(value)
    if path.is_absolute():
        return False
    if re.match(r"^[A-Za-z]:", value):
        return False
    normalized = value.replace("\\", "/")
    if normalized.startswith("/") or normalized.startswith("../"):
        return False
    return ".." not in normalized.split("/")


def require_text(data: dict[str, Any], key: str, report: ValidationReport, source: str) -> str:
    value = str(data.get(key, "")).strip()
    if not value:
        report.errors.append(f"{source} is missing required field `{key}`.")
    return value


def validate_pack_json(pack: dict[str, Any], source_dir: Path, report: ValidationReport) -> tuple[Path | None, Path | None]:
    for field_name in REQUIRED_PACK_FIELDS:
        if field_name not in pack:
            report.errors.append(f"pack.json is missing required field `{field_name}`.")

    schema_version = pack.get("schema_version")
    if schema_version not in SUPPORTED_PACK_SCHEMA_VERSIONS:
        report.errors.append(
            "pack.json `schema_version` must be one of: "
            + ", ".join(str(version) for version in sorted(SUPPORTED_PACK_SCHEMA_VERSIONS))
            + "."
        )
    elif isinstance(schema_version, int):
        report.schema_version = schema_version

    pack_id = require_text(pack, "pack_id", report, "pack.json")
    if pack_id and not PACK_ID_RE.match(pack_id):
        report.errors.append("pack.json `pack_id` must match ^[a-z0-9][a-z0-9_-]{2,63}$.")

    fmt = require_text(pack, "format", report, "pack.json").lower()
    if fmt and fmt not in SAFE_FORMATS:
        report.errors.append(f"pack.json `format` must be one of: {', '.join(sorted(SAFE_FORMATS))}.")

    cards_file_value = require_text(pack, "cards_file", report, "pack.json")
    cards_file: Path | None = None
    if cards_file_value:
        if not is_safe_relative_path(cards_file_value):
            report.errors.append("pack.json `cards_file` must be a safe relative path.")
        else:
            cards_file = source_dir / cards_file_value
            report.cards_file = cards_file_value.replace("\\", "/")
            if cards_file.suffix.lower() not in SUPPORTED_CARD_FILES:
                report.errors.append("pack.json `cards_file` must point to .csv or .xlsx.")
            if not cards_file.exists():
                report.errors.append(f"Cards file does not exist: {cards_file_value}")

    image_root_value = require_text(pack, "image_root", report, "pack.json")
    image_root: Path | None = None
    if image_root_value:
        if not is_safe_relative_path(image_root_value):
            report.errors.append("pack.json `image_root` must be a safe relative directory path.")
        else:
            image_root = source_dir / image_root_value
            if image_root.exists() and not image_root.is_dir():
                report.errors.append("pack.json `image_root` must point to a directory.")

    if schema_version == 2:
        validate_pack_json_v2(pack, source_dir, report)

    return cards_file, image_root


def validate_pack_json_v2(pack: dict[str, Any], source_dir: Path, report: ValidationReport) -> None:
    capabilities = pack.get("capabilities")
    if not isinstance(capabilities, dict):
        report.errors.append("pack.json v2 requires `capabilities` as an object.")
        capabilities = {}

    normalized_capabilities: dict[str, bool] = {}
    for key in SUPPORTED_V2_CAPABILITIES:
        value = capabilities.get(key, False)
        if not isinstance(value, bool):
            report.errors.append(f"pack.json `capabilities.{key}` must be true or false.")
            value = False
        normalized_capabilities[key] = value

    for key in capabilities:
        if key not in SUPPORTED_V2_CAPABILITIES:
            report.warnings.append(f"pack.json `capabilities.{key}` is not recognized by schema v2.")

    if not normalized_capabilities.get("cards"):
        report.errors.append("pack.json v2 `capabilities.cards` must be true.")
    report.capabilities = normalized_capabilities

    ruleset_profile = str(pack.get("ruleset_profile", "")).strip().lower()
    if ruleset_profile and ruleset_profile not in SAFE_RULESET_PROFILES:
        report.errors.append(
            "pack.json `ruleset_profile` must be one of: "
            + ", ".join(sorted(SAFE_RULESET_PROFILES))
            + "."
        )

    if normalized_capabilities.get("abilities"):
        validate_optional_auxiliary_file(pack, "abilities_file", source_dir, report, required=True)
    elif "abilities_file" in pack:
        validate_optional_auxiliary_file(pack, "abilities_file", source_dir, report, required=False)

    if normalized_capabilities.get("custom_formats"):
        validate_optional_auxiliary_file(pack, "formats_file", source_dir, report, required=True)
    elif "formats_file" in pack:
        validate_optional_auxiliary_file(pack, "formats_file", source_dir, report, required=False)

    dependencies = pack.get("dependencies", [])
    if dependencies in (None, ""):
        dependencies = []
    if not isinstance(dependencies, list):
        report.errors.append("pack.json `dependencies` must be an array when present.")
        return

    report.dependency_count = len(dependencies)
    for index, dependency in enumerate(dependencies, start=1):
        if not isinstance(dependency, dict):
            report.errors.append(f"pack.json dependency {index} must be an object.")
            continue
        dependency_pack_id = str(dependency.get("pack_id", "")).strip()
        if not dependency_pack_id:
            report.errors.append(f"pack.json dependency {index} is missing `pack_id`.")
        elif not PACK_ID_RE.match(dependency_pack_id):
            report.errors.append(f"pack.json dependency {index} `pack_id` is invalid: {dependency_pack_id}")
        for hash_key in ("definition_hash", "image_manifest_hash"):
            hash_value = str(dependency.get(hash_key, "")).strip()
            if hash_value and not re.match(r"^[a-fA-F0-9]{64}$", hash_value):
                report.errors.append(f"pack.json dependency {index} `{hash_key}` must be a SHA-256 hex string.")


def validate_optional_auxiliary_file(
    pack: dict[str, Any],
    key: str,
    source_dir: Path,
    report: ValidationReport,
    *,
    required: bool,
) -> None:
    value = str(pack.get(key, "")).strip()
    if not value:
        if required:
            report.errors.append(f"pack.json v2 requires `{key}` when its capability is true.")
        return
    if not is_safe_relative_path(value):
        report.errors.append(f"pack.json `{key}` must be a safe relative path.")
        return
    if Path(value).suffix.lower() not in SUPPORTED_AUXILIARY_JSON_FILES:
        report.errors.append(f"pack.json `{key}` must point to a .json file.")
        return

    if key == "abilities_file":
        report.abilities_file = value.replace("\\", "/")
    elif key == "formats_file":
        report.formats_file = value.replace("\\", "/")

    target = source_dir / value
    if required and not target.exists():
        report.errors.append(f"pack.json `{key}` does not exist: {value}")


def read_csv_cards(path: Path, report: ValidationReport) -> tuple[list[str], list[dict[str, str]]]:
    try:
        with path.open("r", encoding="utf-8-sig", newline="") as f:
            sample = f.read(4096)
            f.seek(0)
            dialect = csv.Sniffer().sniff(sample) if sample.strip() else csv.excel
            reader = csv.DictReader(f, dialect=dialect)
            headers = list(reader.fieldnames or [])
            if len(headers) != len(set(headers)):
                report.errors.append("cards.csv contains duplicate column names.")
            rows = [{key: value or "" for key, value in row.items() if key is not None} for row in reader]
            return headers, rows
    except csv.Error as exc:
        report.errors.append(f"Invalid CSV: {exc}")
        return [], []


def read_cards(path: Path, report: ValidationReport) -> tuple[list[str], list[dict[str, str]]]:
    suffix = path.suffix.lower()
    if suffix == ".csv":
        return read_csv_cards(path, report)
    if suffix == ".xlsx":
        report.errors.append("cards.xlsx is reserved by the schema but not implemented until M7-02.")
        return [], []
    report.errors.append("Cards file must be .csv or .xlsx.")
    return [], []


def int_in_range(value: str, minimum: int, maximum: int, row_number: int, field_name: str, report: ValidationReport, *, required: bool) -> None:
    value = value.strip()
    if not value:
        if required:
            report.errors.append(f"Row {row_number}: `{field_name}` is required.")
        return
    try:
        parsed = int(value.replace(",", ""))
    except ValueError:
        report.errors.append(f"Row {row_number}: `{field_name}` must be an integer.")
        return
    if parsed < minimum or parsed > maximum:
        report.errors.append(f"Row {row_number}: `{field_name}` must be between {minimum} and {maximum}.")


def validate_card_rows(
    headers: list[str],
    rows: list[dict[str, str]],
    image_root: Path | None,
    report: ValidationReport,
    *,
    strict_images: bool,
) -> None:
    header_set = set(headers)
    for column in REQUIRED_CARD_COLUMNS:
        if column not in header_set:
            report.errors.append(f"Cards file is missing required column `{column}`.")

    for column in RECOMMENDED_CARD_COLUMNS:
        if column not in header_set:
            report.warnings.append(f"Cards file is missing recommended column `{column}`.")

    seen_ids: set[str] = set()
    seen_images: set[str] = set()
    for index, row in enumerate(rows, start=2):
        card_id = row.get("card_id", "").strip()
        if not card_id:
            report.errors.append(f"Row {index}: `card_id` is required.")
        elif not CARD_ID_RE.match(card_id):
            report.errors.append(f"Row {index}: `card_id` has invalid characters: {card_id}")
        elif card_id in seen_ids:
            report.errors.append(f"Row {index}: duplicate `card_id`: {card_id}")
        else:
            seen_ids.add(card_id)

        for field_name in ("name", "text", "series", "clan"):
            if not row.get(field_name, "").strip():
                report.errors.append(f"Row {index}: `{field_name}` is required.")

        int_in_range(row.get("grade", ""), 0, 13, index, "grade", report, required=True)
        int_in_range(row.get("deck_limit", ""), 0, 4, index, "deck_limit", report, required=True)
        int_in_range(row.get("power", ""), 0, 999999, index, "power", report, required=False)
        int_in_range(row.get("shield", ""), 0, 999999, index, "shield", report, required=False)
        int_in_range(row.get("critical", ""), 0, 99, index, "critical", report, required=False)

        trigger = row.get("trigger", "").strip().lower()
        if trigger not in ALLOWED_TRIGGERS:
            report.errors.append(f"Row {index}: `trigger` must be one of: {', '.join(sorted(ALLOWED_TRIGGERS - {''}))}.")

        image_file = row.get("image_file", "").strip()
        if not image_file:
            report.warnings.append(f"Row {index}: `image_file` is empty; runtime will use a fallback image.")
            continue

        normalized_image = image_file.replace("\\", "/")
        if not is_safe_relative_path(normalized_image):
            report.errors.append(f"Row {index}: `image_file` must be a safe relative path.")
            continue
        if Path(normalized_image).suffix.lower() not in ALLOWED_IMAGE_EXTENSIONS:
            report.errors.append(f"Row {index}: `image_file` has unsupported extension: {normalized_image}")
        if normalized_image in seen_images:
            report.errors.append(f"Row {index}: duplicate `image_file`: {normalized_image}")
        seen_images.add(normalized_image)

        if image_root is None:
            report.warnings.append(f"Row {index}: cannot check image because image_root is invalid.")
            continue

        image_path = image_root / normalized_image
        if not image_path.exists():
            message = f"Row {index}: image file does not exist: {normalized_image}"
            if strict_images:
                report.errors.append(message)
            else:
                report.warnings.append(message)

    report.card_count = len(rows)
    if not rows:
        report.errors.append("Cards file must contain at least one card row.")


def sha256_bytes(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def canonical_rows(rows: list[dict[str, str]], columns: list[str]) -> list[dict[str, str]]:
    normalized: list[dict[str, str]] = []
    hash_columns = [column for column in columns if column not in DEFINITION_HASH_EXCLUDED_COLUMNS]
    for row in rows:
        normalized.append({column: row.get(column, "").strip() for column in hash_columns})
    normalized.sort(key=lambda item: item.get("card_id", ""))
    return normalized


def compute_hashes(rows: list[dict[str, str]], columns: list[str], image_root: Path | None, report: ValidationReport) -> None:
    definition_payload = json.dumps(canonical_rows(rows, columns), ensure_ascii=False, separators=(",", ":")).encode("utf-8")
    report.definition_hash = sha256_bytes(definition_payload)

    image_entries: list[dict[str, str | None]] = []
    for row in sorted(rows, key=lambda item: item.get("card_id", "")):
        image_file = row.get("image_file", "").strip().replace("\\", "/")
        entry: dict[str, str | None] = {"card_id": row.get("card_id", "").strip(), "image_file": image_file or None, "sha256": None}
        if image_file and image_root is not None and is_safe_relative_path(image_file):
            image_path = image_root / image_file
            if image_path.exists() and image_path.is_file():
                entry["sha256"] = sha256_file(image_path)
        image_entries.append(entry)

    image_payload = json.dumps(image_entries, ensure_ascii=False, separators=(",", ":")).encode("utf-8")
    report.image_manifest_hash = sha256_bytes(image_payload)


def validate_pack_ability_data(source_dir: Path, rows: list[dict[str, str]], report: ValidationReport) -> None:
    if not report.abilities_file:
        return

    ability_path = source_dir / report.abilities_file
    if not ability_path.exists():
        return

    ability_report = validate_ability_file(ability_path)
    report.ability_count = ability_report.ability_count
    report.ability_data_hash = sha256_file(ability_path)

    for error in ability_report.errors:
        report.errors.append(f"abilities_file: {error}")
    for warning in ability_report.warnings:
        report.warnings.append(f"abilities_file: {warning}")
    if not ability_report.all_ok:
        return

    try:
        data = json.loads(ability_path.read_text(encoding="utf-8"))
    except json.JSONDecodeError as exc:
        report.errors.append(f"abilities_file: Invalid JSON after validation: {exc}")
        return

    card_ids = {row.get("card_id", "").strip() for row in rows if row.get("card_id", "").strip()}
    for index, ability in enumerate(data.get("abilities", [])):
        if not isinstance(ability, dict):
            continue
        card_id = str(ability.get("card_id", "")).strip()
        if card_id and card_id not in card_ids:
            ability_id = str(ability.get("ability_id", "")).strip() or f"index {index}"
            report.errors.append(
                f"abilities_file: ability `{ability_id}` references card_id not present in pack: {card_id}"
            )


def validate_source_dir(source_dir: Path, *, strict_images: bool = False) -> ValidationReport:
    source_dir = source_dir.resolve()
    report = ValidationReport(source_dir=str(source_dir))
    if not source_dir.exists():
        report.errors.append(f"Source directory does not exist: {source_dir}")
        return report
    if not source_dir.is_dir():
        report.errors.append(f"Source path is not a directory: {source_dir}")
        return report

    pack = read_json(source_dir / "pack.json", report)
    cards_file, image_root = validate_pack_json(pack, source_dir, report)
    if cards_file is None or not cards_file.exists():
        return report

    headers, rows = read_cards(cards_file, report)
    validate_card_rows(headers, rows, image_root, report, strict_images=strict_images)
    compute_hashes(rows, headers, image_root, report)
    validate_pack_ability_data(source_dir, rows, report)
    return report


def print_human_report(report: ValidationReport) -> None:
    print(f"Custom pack source: {report.source_dir}")
    print(f"Cards file: {report.cards_file or '-'}")
    print(f"Cards: {report.card_count}")
    print(f"Definition hash: {report.definition_hash or '-'}")
    print(f"Image manifest hash: {report.image_manifest_hash or '-'}")
    if report.abilities_file:
        print(f"Abilities file: {report.abilities_file}")
        print(f"Abilities: {report.ability_count}")
        print(f"Ability data hash: {report.ability_data_hash or '-'}")
    print(f"Errors: {len(report.errors)}")
    for error in report.errors:
        print(f"  ERROR: {error}")
    print(f"Warnings: {len(report.warnings)}")
    for warning in report.warnings:
        print(f"  WARNING: {warning}")
    print(f"All OK: {report.all_ok}")


def main() -> int:
    args = parse_args()
    report = validate_source_dir(args.source_dir, strict_images=args.strict_images)
    if args.json:
        print(json.dumps(report.to_dict(), ensure_ascii=False, indent=2))
    else:
        print_human_report(report)
    return 0 if report.all_ok else 1


if __name__ == "__main__":
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    if hasattr(sys.stderr, "reconfigure"):
        sys.stderr.reconfigure(encoding="utf-8", errors="replace")
    raise SystemExit(main())
