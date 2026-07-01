"""Build the Vanguard TH runtime card pack.

This script converts the verified KK Card Fight JSON export into:
- data/packs/vanguard_th/manifest.json
- data/packs/vanguard_th/cards.sqlite

Images are not copied. The SQLite database stores source image paths and a
relative image path so the runtime can load/cache images without duplicating the
2GB source image folder.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import re
import sqlite3
from collections import Counter
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[2]
DEFAULT_SOURCE_JSON = ROOT / "outputs/kk_cardfight_export/data/vanguard_th_cards_with_images.json"
DEFAULT_SOURCE_META = ROOT / "outputs/kk_cardfight_export/data/meta.json"
DEFAULT_SOURCE_VERIFICATION = ROOT / "outputs/kk_cardfight_export/data/verification_report.json"
DEFAULT_IMAGE_MANIFEST = ROOT / "outputs/kk_cardfight_export/data/image_manifest.csv"
DEFAULT_OUTPUT_DIR = ROOT / "data/packs/vanguard_th"
DEFAULT_IMAGE_ROOT = ROOT / "outputs/kk_cardfight_export/data/images"


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build Vanguard TH runtime pack.")
    parser.add_argument("--source-json", type=Path, default=DEFAULT_SOURCE_JSON)
    parser.add_argument("--source-meta", type=Path, default=DEFAULT_SOURCE_META)
    parser.add_argument("--source-verification", type=Path, default=DEFAULT_SOURCE_VERIFICATION)
    parser.add_argument("--image-manifest", type=Path, default=DEFAULT_IMAGE_MANIFEST)
    parser.add_argument("--image-root", type=Path, default=DEFAULT_IMAGE_ROOT)
    parser.add_argument("--output-dir", type=Path, default=DEFAULT_OUTPUT_DIR)
    parser.add_argument("--pack-id", default="vanguard_th")
    return parser.parse_args()


def read_json(path: Path) -> Any:
    with path.open("r", encoding="utf-8") as f:
        return json.load(f)


def write_json(path: Path, data: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="\n") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
        f.write("\n")


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def maybe_sha256_file(path: Path) -> str | None:
    return sha256_file(path) if path.exists() else None


def text_or_none(value: Any) -> str | None:
    if value is None:
        return None
    value = str(value)
    return value if value != "" else None


def int_or_none(value: Any) -> int | None:
    if value is None or value == "":
        return None
    if isinstance(value, int):
        return value
    value = str(value).replace(",", "").strip()
    if not value:
        return None
    try:
        return int(value)
    except ValueError:
        return None


def normalize_path(path: str | None) -> str | None:
    if not path:
        return None
    return path.replace("\\", "/")


def relative_image_path(local_image_path: str | None, image_root: Path) -> str | None:
    normalized = normalize_path(local_image_path)
    if not normalized:
        return None

    root_normalized = normalize_path(str(image_root.relative_to(ROOT)))
    if root_normalized and normalized.startswith(root_normalized + "/"):
        return normalized[len(root_normalized) + 1 :]

    try:
        return normalize_path(str((ROOT / normalized).resolve().relative_to(image_root.resolve())))
    except ValueError:
        return normalized


def image_exists(local_image_path: str | None) -> bool:
    if not local_image_path:
        return False
    return (ROOT / local_image_path).exists()


def extract_series_code(series: str | None) -> str | None:
    if not series:
        return None
    match = re.match(r"^\[([^\]]+)\]", series)
    return match.group(1) if match else None


def raw_detail_text(card: dict[str, Any]) -> str:
    parts: list[str] = []
    for item in card.get("raw_detail") or []:
        if isinstance(item, dict):
            label = text_or_none(item.get("label"))
            value = text_or_none(item.get("value"))
            if label:
                parts.append(label)
            if value:
                parts.append(value)
    return " ".join(parts)


def search_text(card: dict[str, Any], series_code: str | None) -> str:
    values = [
        card.get("card_id"),
        card.get("key"),
        card.get("name_th"),
        card.get("text_th"),
        card.get("series"),
        series_code,
        card.get("clan"),
        card.get("nation"),
        card.get("nation_2"),
        card.get("type_1"),
        card.get("type_2"),
        card.get("race_1"),
        card.get("race_2"),
        card.get("trigger"),
        raw_detail_text(card),
    ]
    return " ".join(str(v) for v in values if v not in (None, ""))


def create_schema(conn: sqlite3.Connection) -> None:
    conn.executescript(
        """
        PRAGMA foreign_keys = ON;

        CREATE TABLE pack_manifest (
          key TEXT PRIMARY KEY,
          value TEXT NOT NULL
        );

        CREATE TABLE cards (
          card_id TEXT PRIMARY KEY,
          source_id TEXT NOT NULL,
          source_key TEXT,
          name_th TEXT,
          text_th TEXT,
          series TEXT NOT NULL,
          series_code TEXT,
          clan TEXT NOT NULL,
          nation TEXT,
          nation_2 TEXT,
          grade INTEGER,
          power INTEGER,
          shield INTEGER,
          trigger TEXT,
          deck_limit INTEGER NOT NULL,
          type_1 TEXT,
          type_2 TEXT,
          race_1 TEXT,
          race_2 TEXT,
          warning TEXT,
          raw_json TEXT NOT NULL
        );

        CREATE TABLE card_details (
          card_id TEXT NOT NULL,
          sort_order INTEGER NOT NULL,
          label TEXT,
          value TEXT,
          PRIMARY KEY (card_id, sort_order),
          FOREIGN KEY (card_id) REFERENCES cards(card_id) ON DELETE CASCADE
        );

        CREATE TABLE card_formats (
          card_id TEXT NOT NULL,
          format_key TEXT NOT NULL,
          format_value TEXT NOT NULL,
          PRIMARY KEY (card_id, format_key),
          FOREIGN KEY (card_id) REFERENCES cards(card_id) ON DELETE CASCADE
        );

        CREATE TABLE card_images (
          card_id TEXT PRIMARY KEY,
          image_url TEXT NOT NULL,
          local_image_path TEXT NOT NULL,
          image_relative_path TEXT NOT NULL,
          image_bytes INTEGER,
          image_download_status TEXT,
          image_exists INTEGER NOT NULL,
          FOREIGN KEY (card_id) REFERENCES cards(card_id) ON DELETE CASCADE
        );

        CREATE TABLE series (
          series TEXT PRIMARY KEY,
          series_code TEXT,
          card_count INTEGER NOT NULL
        );

        CREATE TABLE clans (
          clan TEXT PRIMARY KEY,
          card_count INTEGER NOT NULL
        );

        CREATE TABLE series_clans (
          series TEXT NOT NULL,
          clan TEXT NOT NULL,
          card_count INTEGER NOT NULL,
          PRIMARY KEY (series, clan)
        );

        CREATE TABLE search_terms (
          card_id TEXT PRIMARY KEY,
          text TEXT NOT NULL,
          FOREIGN KEY (card_id) REFERENCES cards(card_id) ON DELETE CASCADE
        );

        CREATE INDEX idx_cards_series ON cards(series);
        CREATE INDEX idx_cards_series_code ON cards(series_code);
        CREATE INDEX idx_cards_clan ON cards(clan);
        CREATE INDEX idx_cards_grade ON cards(grade);
        CREATE INDEX idx_cards_type_1 ON cards(type_1);
        CREATE INDEX idx_cards_name_th ON cards(name_th);
        CREATE INDEX idx_card_images_relative_path ON card_images(image_relative_path);
        """
    )


def insert_manifest(conn: sqlite3.Connection, manifest: dict[str, Any]) -> None:
    rows = [(key, json.dumps(value, ensure_ascii=False) if not isinstance(value, str) else value) for key, value in manifest.items()]
    conn.executemany("INSERT INTO pack_manifest (key, value) VALUES (?, ?)", rows)


def insert_cards(conn: sqlite3.Connection, cards: list[dict[str, Any]], image_root: Path) -> None:
    card_rows: list[tuple[Any, ...]] = []
    detail_rows: list[tuple[Any, ...]] = []
    format_rows: list[tuple[Any, ...]] = []
    image_rows: list[tuple[Any, ...]] = []
    search_rows: list[tuple[Any, ...]] = []

    for card in cards:
        card_id = text_or_none(card.get("card_id"))
        if not card_id:
            raise ValueError(f"Card without card_id: {card!r}")

        series = text_or_none(card.get("series"))
        clan = text_or_none(card.get("clan"))
        if not series or not clan:
            raise ValueError(f"Card {card_id} is missing series or clan.")

        series_code = extract_series_code(series)
        local_image_path = normalize_path(text_or_none(card.get("local_image_path")))
        image_rel = relative_image_path(local_image_path, image_root)
        if not local_image_path or not image_rel:
            raise ValueError(f"Card {card_id} is missing local image path.")

        card_rows.append(
            (
                card_id,
                text_or_none(card.get("id")),
                text_or_none(card.get("key")),
                text_or_none(card.get("name_th")),
                text_or_none(card.get("text_th")),
                series,
                series_code,
                clan,
                text_or_none(card.get("nation")),
                text_or_none(card.get("nation_2")),
                int_or_none(card.get("grade")),
                int_or_none(card.get("power")),
                int_or_none(card.get("shield")),
                text_or_none(card.get("trigger")),
                int_or_none(card.get("deck_limit")) or 4,
                text_or_none(card.get("type_1")),
                text_or_none(card.get("type_2")),
                text_or_none(card.get("race_1")),
                text_or_none(card.get("race_2")),
                text_or_none(card.get("warning")),
                json.dumps(card, ensure_ascii=False, separators=(",", ":")),
            )
        )

        for index, detail in enumerate(card.get("raw_detail") or []):
            if isinstance(detail, dict):
                detail_rows.append((card_id, index, text_or_none(detail.get("label")), text_or_none(detail.get("value"))))

        for key in ("premium_standard_format", "v_standard_format", "d_standard_format"):
            value = text_or_none(card.get(key))
            if value:
                format_rows.append((card_id, key, value))

        image_rows.append(
            (
                card_id,
                text_or_none(card.get("image_url")),
                local_image_path,
                image_rel,
                int_or_none(card.get("image_bytes")),
                text_or_none(card.get("image_download_status")),
                1 if image_exists(local_image_path) else 0,
            )
        )

        search_rows.append((card_id, search_text(card, series_code)))

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


def insert_summaries(conn: sqlite3.Connection, cards: list[dict[str, Any]]) -> None:
    series_counts = Counter(text_or_none(card.get("series")) for card in cards)
    clan_counts = Counter(text_or_none(card.get("clan")) for card in cards)
    series_clan_counts = Counter((text_or_none(card.get("series")), text_or_none(card.get("clan"))) for card in cards)

    conn.executemany(
        "INSERT INTO series (series, series_code, card_count) VALUES (?, ?, ?)",
        [(series, extract_series_code(series), count) for series, count in sorted(series_counts.items()) if series],
    )
    conn.executemany(
        "INSERT INTO clans (clan, card_count) VALUES (?, ?)",
        [(clan, count) for clan, count in sorted(clan_counts.items()) if clan],
    )
    conn.executemany(
        "INSERT INTO series_clans (series, clan, card_count) VALUES (?, ?, ?)",
        [(series, clan, count) for (series, clan), count in sorted(series_clan_counts.items()) if series and clan],
    )


def create_fts_if_available(conn: sqlite3.Connection) -> bool:
    try:
        conn.execute("CREATE VIRTUAL TABLE cards_fts USING fts5(card_id UNINDEXED, text)")
        conn.execute("INSERT INTO cards_fts (card_id, text) SELECT card_id, text FROM search_terms")
    except sqlite3.OperationalError:
        return False
    return True


def build_manifest(args: argparse.Namespace, cards: list[dict[str, Any]], fts_enabled: bool) -> dict[str, Any]:
    meta = read_json(args.source_meta) if args.source_meta.exists() else {}
    source_verification = read_json(args.source_verification) if args.source_verification.exists() else {}
    source_json_rel = normalize_path(str(args.source_json.relative_to(ROOT)))
    image_root_rel = normalize_path(str(args.image_root.relative_to(ROOT)))

    image_paths = [normalize_path(card.get("local_image_path")) for card in cards if card.get("local_image_path")]
    existing_image_count = sum(1 for path in image_paths if path and image_exists(path))

    return {
        "pack_id": args.pack_id,
        "display_name": "Vanguard TH",
        "schema_version": 1,
        "source": "KK Card Fight",
        "source_pointer_url": meta.get("source_pointer_url"),
        "source_card_file_url": meta.get("source_card_file_url"),
        "source_version": str((meta.get("game_detail") or {}).get("cardVersionNo") or "unknown"),
        "source_exported_at": meta.get("exported_at"),
        "created_at": datetime.now(timezone.utc).isoformat(),
        "card_count": len(cards),
        "image_count": len(set(image_paths)),
        "existing_image_count": existing_image_count,
        "series_count": len({card.get("series") for card in cards if card.get("series")}),
        "clan_count": len({card.get("clan") for card in cards if card.get("clan")}),
        "definition_hash": sha256_file(args.source_json),
        "image_manifest_hash": maybe_sha256_file(args.image_manifest),
        "source_cards_file": source_json_rel,
        "source_verification_file": normalize_path(str(args.source_verification.relative_to(ROOT))) if args.source_verification.exists() else None,
        "image_root": image_root_rel,
        "sqlite_file": "cards.sqlite",
        "sqlite_fts_enabled": fts_enabled,
        "source_verification_all_ok": source_verification.get("all_ok"),
    }


def build_pack(args: argparse.Namespace) -> dict[str, Any]:
    cards = read_json(args.source_json)
    if not isinstance(cards, list):
        raise TypeError(f"Expected a list of cards in {args.source_json}")

    args.output_dir.mkdir(parents=True, exist_ok=True)
    sqlite_path = args.output_dir / "cards.sqlite"
    temp_sqlite_path = args.output_dir / "cards.sqlite.tmp"
    if temp_sqlite_path.exists():
        temp_sqlite_path.unlink()

    conn = sqlite3.connect(temp_sqlite_path)
    try:
        create_schema(conn)
        insert_cards(conn, cards, args.image_root)
        insert_summaries(conn, cards)
        fts_enabled = create_fts_if_available(conn)
        manifest = build_manifest(args, cards, fts_enabled)
        insert_manifest(conn, manifest)
        conn.commit()
        conn.execute("VACUUM")
    finally:
        conn.close()

    if sqlite_path.exists():
        sqlite_path.unlink()
    temp_sqlite_path.replace(sqlite_path)

    manifest["sqlite_bytes"] = sqlite_path.stat().st_size
    write_json(args.output_dir / "manifest.json", manifest)
    return manifest


def main() -> int:
    args = parse_args()
    manifest = build_pack(args)
    print(f"Built pack: {args.output_dir}")
    print(f"Cards: {manifest['card_count']}")
    print(f"Images: {manifest['existing_image_count']}/{manifest['image_count']}")
    print(f"SQLite: {args.output_dir / manifest['sqlite_file']}")
    print(f"Manifest: {args.output_dir / 'manifest.json'}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

