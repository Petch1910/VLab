"""Import a custom card pack source into the runtime SQLite pack format."""

from __future__ import annotations

import argparse
import csv
import hashlib
import json
import shutil
import sqlite3
import sys
import tempfile
import zipfile
from contextlib import contextmanager
from datetime import datetime, timezone
from pathlib import Path
from typing import Any, Iterator

from build_vanguard_th_pack import create_fts_if_available, create_schema, insert_manifest, insert_summaries
from validate_custom_pack_schema import (
    RECOMMENDED_CARD_COLUMNS,
    ValidationReport,
    is_safe_relative_path,
    read_json,
    validate_source_dir,
)


ROOT = Path(__file__).resolve().parents[2]
DEFAULT_OUTPUT_ROOT = ROOT / "data/packs"


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Import a custom card pack into runtime SQLite format.")
    parser.add_argument("source", type=Path, help="Custom pack source directory or zip archive.")
    parser.add_argument("--output-dir", type=Path, help="Output pack directory. Defaults to data/packs/<pack_id>.")
    parser.add_argument("--output-root", type=Path, default=DEFAULT_OUTPUT_ROOT)
    parser.add_argument("--strict-images", action="store_true", help="Reject missing image files.")
    parser.add_argument("--overwrite", action="store_true", help="Overwrite an existing output directory.")
    return parser.parse_args()


def write_json(path: Path, data: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="\n") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
        f.write("\n")


def normalize_path(path: Path | str) -> str:
    return str(path).replace("\\", "/")


def safe_manifest_path(path: Path) -> str:
    try:
        return normalize_path(path.resolve().relative_to(ROOT))
    except ValueError:
        return normalize_path(path.resolve())


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def sha256_text(value: str) -> str:
    return hashlib.sha256(value.encode("utf-8")).hexdigest()


def text(value: Any) -> str:
    if value is None:
        return ""
    return str(value).strip()


def text_or_none(value: Any) -> str | None:
    value = text(value)
    return value if value else None


def int_or_none(value: Any) -> int | None:
    value = text(value).replace(",", "")
    if not value:
        return None
    return int(value)


def read_csv_rows(path: Path) -> list[dict[str, str]]:
    with path.open("r", encoding="utf-8-sig", newline="") as f:
        reader = csv.DictReader(f)
        return [{key: value or "" for key, value in row.items() if key is not None} for row in reader]


def load_source_cards(source_dir: Path, pack: dict[str, Any]) -> list[dict[str, str]]:
    cards_file = source_dir / text(pack.get("cards_file"))
    if cards_file.suffix.lower() == ".xlsx":
        raise NotImplementedError("cards.xlsx is reserved by the schema but not implemented until a later importer pass.")
    return read_csv_rows(cards_file)


def clean_output_dir(output_dir: Path, overwrite: bool) -> None:
    if output_dir.exists():
        if not overwrite:
            raise FileExistsError(f"Output directory already exists: {output_dir}. Pass --overwrite to replace it.")
        shutil.rmtree(output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)


def copy_images(source_dir: Path, output_dir: Path, pack: dict[str, Any], rows: list[dict[str, str]]) -> dict[str, dict[str, Any]]:
    source_image_root = source_dir / text(pack.get("image_root"))
    output_image_root = output_dir / "images"
    copied: dict[str, dict[str, Any]] = {}

    for row in rows:
        image_file = text(row.get("image_file")).replace("\\", "/")
        card_id = text(row.get("card_id"))
        if not image_file or not is_safe_relative_path(image_file):
            continue
        source_image = source_image_root / image_file
        if not source_image.exists() or not source_image.is_file():
            continue
        destination = output_image_root / image_file
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source_image, destination)
        copied[card_id] = {
            "image_relative_path": image_file,
            "local_image_path": safe_manifest_path(destination),
            "bytes": destination.stat().st_size,
            "sha256": sha256_file(destination),
        }

    output_image_root.mkdir(parents=True, exist_ok=True)
    return copied


def detail_rows_for_card(card_id: str, row: dict[str, str]) -> list[tuple[str, int, str, str]]:
    detail_fields = (
        ("Language", "language"),
        ("Format", "format"),
        ("Nation", "nation"),
        ("Race", "race"),
        ("Critical", "critical"),
        ("Ability Timing", "ability_timing"),
        ("Ability Tags", "ability_tags"),
        ("Flavor", "flavor_text"),
        ("Artist", "artist"),
        ("Rarity", "rarity"),
        ("Notes", "notes"),
    )
    details: list[tuple[str, int, str, str]] = []
    for sort_order, (label, key) in enumerate(detail_fields):
        value = text(row.get(key))
        if value:
            details.append((card_id, sort_order, label, value))
    return details


def search_text(row: dict[str, str]) -> str:
    values = [text(row.get(column)) for column in RECOMMENDED_CARD_COLUMNS]
    return " ".join(value for value in values if value)


def insert_custom_cards(
    conn: sqlite3.Connection,
    rows: list[dict[str, str]],
    pack: dict[str, Any],
    copied_images: dict[str, dict[str, Any]],
) -> None:
    card_rows: list[tuple[Any, ...]] = []
    detail_rows: list[tuple[Any, ...]] = []
    format_rows: list[tuple[Any, ...]] = []
    image_rows: list[tuple[Any, ...]] = []
    search_rows: list[tuple[Any, ...]] = []

    pack_format = text(pack.get("format"))
    for row in rows:
        card_id = text(row.get("card_id"))
        copied = copied_images.get(card_id, {})
        image_relative_path = text(copied.get("image_relative_path"))
        local_image_path = text(copied.get("local_image_path"))
        image_bytes = copied.get("bytes")

        card_rows.append(
            (
                card_id,
                card_id,
                card_id,
                text_or_none(row.get("name")),
                text_or_none(row.get("text")),
                text(row.get("series")),
                text_or_none(row.get("series_code")),
                text(row.get("clan")),
                text_or_none(row.get("nation")),
                None,
                int_or_none(row.get("grade")),
                int_or_none(row.get("power")),
                int_or_none(row.get("shield")),
                text_or_none(row.get("trigger")),
                int_or_none(row.get("deck_limit")) or 4,
                text_or_none(row.get("type_1")),
                text_or_none(row.get("type_2")),
                text_or_none(row.get("race")),
                None,
                text_or_none(row.get("notes")),
                json.dumps(row, ensure_ascii=False, separators=(",", ":")),
            )
        )

        detail_rows.extend(detail_rows_for_card(card_id, row))
        custom_format = text(row.get("format")) or pack_format
        if custom_format:
            format_rows.append((card_id, "custom_format", custom_format))
        if pack_format and pack_format != custom_format:
            format_rows.append((card_id, "pack_format", pack_format))

        image_rows.append(
            (
                card_id,
                "",
                local_image_path,
                image_relative_path,
                image_bytes,
                "copied" if image_relative_path else "missing",
                1 if image_relative_path else 0,
            )
        )
        search_rows.append((card_id, search_text(row)))

    conn.executemany(
        """
        INSERT INTO cards (
          card_id, source_id, source_key, name_th, text_th, series, series_code,
          clan, nation, nation_2, grade, power, shield, trigger, deck_limit,
          type_1, type_2, race_1, race_2, warning, raw_json
        ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
        """,
        card_rows,
    )
    conn.executemany("INSERT INTO card_details (card_id, sort_order, label, value) VALUES (?, ?, ?, ?)", detail_rows)
    conn.executemany("INSERT INTO card_formats (card_id, format_key, format_value) VALUES (?, ?, ?)", format_rows)
    conn.executemany(
        """
        INSERT INTO card_images (
          card_id, image_url, local_image_path, image_relative_path, image_bytes,
          image_download_status, image_exists
        ) VALUES (?, ?, ?, ?, ?, ?, ?)
        """,
        image_rows,
    )
    conn.executemany("INSERT INTO search_terms (card_id, text) VALUES (?, ?)", search_rows)


def build_asset_index(output_dir: Path, pack_id: str, copied_images: dict[str, dict[str, Any]]) -> dict[str, Any]:
    assets: list[dict[str, Any]] = []
    aggregate = hashlib.sha256()
    for card_id in sorted(copied_images):
        copied = copied_images[card_id]
        entry = {
            "card_id": card_id,
            "image_relative_path": copied["image_relative_path"],
            "bytes": copied["bytes"],
            "sha256": copied["sha256"],
            "content_address": normalize_path(Path("sha256") / copied["sha256"][:2] / (copied["sha256"] + Path(copied["image_relative_path"]).suffix.lower())),
        }
        assets.append(entry)
        aggregate.update(card_id.encode("utf-8"))
        aggregate.update(b"\0")
        aggregate.update(entry["image_relative_path"].encode("utf-8"))
        aggregate.update(b"\0")
        aggregate.update(str(entry["bytes"]).encode("ascii"))
        aggregate.update(b"\0")
        aggregate.update(entry["sha256"].encode("ascii"))
        aggregate.update(b"\n")

    return {
        "schema_version": 1,
        "pack_id": pack_id,
        "generated_at": datetime.now(timezone.utc).isoformat(),
        "image_root": safe_manifest_path(output_dir / "images"),
        "image_count": len(assets),
        "aggregate_hash": aggregate.hexdigest(),
        "assets": assets,
    }


def build_manifest(
    output_dir: Path,
    source_dir: Path,
    pack: dict[str, Any],
    rows: list[dict[str, str]],
    validation: ValidationReport,
    copied_images: dict[str, dict[str, Any]],
    fts_enabled: bool,
    sqlite_path: Path,
    asset_index: dict[str, Any],
) -> dict[str, Any]:
    pack_id = text(pack.get("pack_id"))
    return {
        "pack_id": pack_id,
        "display_name": text(pack.get("display_name")),
        "schema_version": 1,
        "source_schema_version": int(pack.get("schema_version") or 1),
        "source": "custom_pack",
        "source_version": text(pack.get("source_version")),
        "source_capabilities": pack.get("capabilities") if isinstance(pack.get("capabilities"), dict) else None,
        "source_ruleset_profile": text_or_none(pack.get("ruleset_profile")),
        "source_abilities_file": text_or_none(pack.get("abilities_file")),
        "source_formats_file": text_or_none(pack.get("formats_file")),
        "source_ability_count": validation.ability_count,
        "source_ability_data_hash": validation.ability_data_hash,
        "source_exported_at": None,
        "created_at": datetime.now(timezone.utc).isoformat(),
        "card_count": len(rows),
        "image_count": len(rows),
        "existing_image_count": len(copied_images),
        "series_count": len({text(row.get("series")) for row in rows if text(row.get("series"))}),
        "clan_count": len({text(row.get("clan")) for row in rows if text(row.get("clan"))}),
        "definition_hash": validation.definition_hash,
        "image_manifest_hash": validation.image_manifest_hash,
        "source_cards_file": safe_manifest_path(source_dir / text(pack.get("cards_file"))),
        "source_verification_file": None,
        "image_root": safe_manifest_path(output_dir / "images"),
        "sqlite_file": sqlite_path.name,
        "sqlite_fts_enabled": fts_enabled,
        "source_verification_all_ok": True,
        "sqlite_bytes": sqlite_path.stat().st_size,
        "asset_index_file": "asset_index.json",
        "asset_index_schema_version": asset_index["schema_version"],
        "image_content_hash": asset_index["aggregate_hash"],
        "image_cache_strategy": "custom_pack_local_images",
        "cache_layout_version": 1,
        "user_data_policy": "user_data_must_not_be_stored_in_generated_pack",
    }


def import_custom_pack(source_dir: Path, output_dir: Path | None = None, *, strict_images: bool = False, overwrite: bool = False) -> dict[str, Any]:
    validation = validate_source_dir(source_dir, strict_images=strict_images)
    if not validation.all_ok:
        raise ValueError("Custom pack validation failed: " + "; ".join(validation.errors))

    pack = read_json(source_dir / "pack.json", validation)
    pack_id = text(pack.get("pack_id"))
    if output_dir is None:
        output_dir = DEFAULT_OUTPUT_ROOT / pack_id
    output_dir = output_dir.resolve()

    clean_output_dir(output_dir, overwrite)
    rows = load_source_cards(source_dir, pack)
    copied_images = copy_images(source_dir, output_dir, pack, rows)

    sqlite_path = output_dir / "cards.sqlite"
    temp_sqlite_path = output_dir / "cards.sqlite.tmp"
    conn = sqlite3.connect(temp_sqlite_path)
    try:
        create_schema(conn)
        insert_custom_cards(conn, rows, pack, copied_images)
        synthetic_cards = [{"series": row.get("series"), "clan": row.get("clan")} for row in rows]
        insert_summaries(conn, synthetic_cards)
        fts_enabled = create_fts_if_available(conn)
        conn.commit()
        conn.execute("VACUUM")
    finally:
        conn.close()

    if sqlite_path.exists():
        sqlite_path.unlink()
    temp_sqlite_path.replace(sqlite_path)

    asset_index = build_asset_index(output_dir, pack_id, copied_images)
    write_json(output_dir / "asset_index.json", asset_index)
    manifest = build_manifest(
        output_dir,
        source_dir,
        pack,
        rows,
        validation,
        copied_images,
        fts_enabled,
        sqlite_path,
        asset_index,
    )
    write_json(output_dir / "manifest.json", manifest)
    return manifest


def safe_extract_zip(zip_path: Path, destination: Path) -> Path:
    with zipfile.ZipFile(zip_path) as archive:
        for member in archive.infolist():
            target = (destination / member.filename).resolve()
            if not str(target).startswith(str(destination.resolve())):
                raise ValueError(f"Unsafe zip member path: {member.filename}")
            if member.is_dir():
                target.mkdir(parents=True, exist_ok=True)
            else:
                target.parent.mkdir(parents=True, exist_ok=True)
                with archive.open(member) as src, target.open("wb") as dst:
                    shutil.copyfileobj(src, dst)

    if (destination / "pack.json").exists():
        return destination

    candidates = [path.parent for path in destination.rglob("pack.json")]
    if len(candidates) == 1:
        return candidates[0]
    raise FileNotFoundError("Zip archive must contain exactly one pack.json.")


@contextmanager
def source_context(source: Path) -> Iterator[Path]:
    source = source.resolve()
    if source.is_dir():
        yield source
        return
    if source.suffix.lower() != ".zip":
        raise ValueError("Source must be a directory or .zip archive.")

    with tempfile.TemporaryDirectory(prefix="custom_pack_import_") as temp:
        yield safe_extract_zip(source, Path(temp))


def main() -> int:
    args = parse_args()
    with source_context(args.source) as source_dir:
        output_dir = args.output_dir
        if output_dir is None:
            pack = json.loads((source_dir / "pack.json").read_text(encoding="utf-8"))
            output_dir = args.output_root / text(pack.get("pack_id"))
        manifest = import_custom_pack(
            source_dir,
            output_dir,
            strict_images=args.strict_images,
            overwrite=args.overwrite,
        )

    print(f"Imported custom pack: {output_dir}")
    print(f"Pack id: {manifest['pack_id']}")
    print(f"Cards: {manifest['card_count']}")
    print(f"Images: {manifest['existing_image_count']}/{manifest['image_count']}")
    print(f"Definition hash: {manifest['definition_hash']}")
    print(f"Image manifest hash: {manifest['image_manifest_hash']}")
    return 0


if __name__ == "__main__":
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    if hasattr(sys.stderr, "reconfigure"):
        sys.stderr.reconfigure(encoding="utf-8", errors="replace")
    raise SystemExit(main())
