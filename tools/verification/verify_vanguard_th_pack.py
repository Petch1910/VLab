"""Verify the Vanguard TH runtime card pack."""

from __future__ import annotations

import argparse
import hashlib
import json
import sqlite3
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[2]
DEFAULT_PACK_DIR = ROOT / "data/packs/vanguard_th"
DEFAULT_SOURCE_JSON = ROOT / "outputs/kk_cardfight_export/data/vanguard_th_cards_with_images.json"


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Verify Vanguard TH runtime pack.")
    parser.add_argument("--pack-dir", type=Path, default=DEFAULT_PACK_DIR)
    parser.add_argument("--source-json", type=Path, default=DEFAULT_SOURCE_JSON)
    parser.add_argument("--write-report", action="store_true", default=True)
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


def scalar(conn: sqlite3.Connection, sql: str, params: tuple[Any, ...] = ()) -> Any:
    return conn.execute(sql, params).fetchone()[0]


def verify(args: argparse.Namespace) -> dict[str, Any]:
    manifest_path = args.pack_dir / "manifest.json"
    sqlite_path = args.pack_dir / "cards.sqlite"
    manifest = read_json(manifest_path)
    source_cards = read_json(args.source_json)
    asset_index_path = args.pack_dir / manifest.get("asset_index_file", "asset_index.json")
    asset_index = read_json(asset_index_path) if asset_index_path.exists() else None

    conn = sqlite3.connect(sqlite_path)
    try:
        cards_count = scalar(conn, "SELECT COUNT(*) FROM cards")
        images_count = scalar(conn, "SELECT COUNT(*) FROM card_images")
        existing_images_count = scalar(conn, "SELECT COUNT(*) FROM card_images WHERE image_exists = 1")
        missing_images = scalar(conn, "SELECT COUNT(*) FROM card_images WHERE image_exists = 0")
        series_count = scalar(conn, "SELECT COUNT(*) FROM series")
        clan_count = scalar(conn, "SELECT COUNT(*) FROM clans")
        details_count = scalar(conn, "SELECT COUNT(*) FROM card_details")
        search_count = scalar(conn, "SELECT COUNT(*) FROM search_terms")
        duplicate_image_paths = scalar(
            conn,
            """
            SELECT COUNT(*) FROM (
              SELECT image_relative_path
              FROM card_images
              GROUP BY image_relative_path
              HAVING COUNT(*) > 1
            )
            """,
        )
        bt01001 = conn.execute(
            """
            SELECT c.card_id, c.name_th, c.series, c.clan, i.image_exists
            FROM cards c
            JOIN card_images i ON i.card_id = c.card_id
            WHERE c.card_id = ?
            """,
            ("BT01-001TH",),
        ).fetchone()
        soft_missing = {
            "name_th": scalar(conn, "SELECT COUNT(*) FROM cards WHERE name_th IS NULL OR name_th = ''"),
            "text_th": scalar(conn, "SELECT COUNT(*) FROM cards WHERE text_th IS NULL OR text_th = ''"),
            "nation": scalar(conn, "SELECT COUNT(*) FROM cards WHERE nation IS NULL OR nation = ''"),
            "grade": scalar(conn, "SELECT COUNT(*) FROM cards WHERE grade IS NULL"),
            "power": scalar(conn, "SELECT COUNT(*) FROM cards WHERE power IS NULL"),
            "shield": scalar(conn, "SELECT COUNT(*) FROM cards WHERE shield IS NULL"),
            "trigger": scalar(conn, "SELECT COUNT(*) FROM cards WHERE trigger IS NULL OR trigger = ''"),
            "type_1": scalar(conn, "SELECT COUNT(*) FROM cards WHERE type_1 IS NULL OR type_1 = ''"),
        }
    finally:
        conn.close()

    source_count = len(source_cards)
    image_hash_report = verify_asset_index(asset_index, manifest)
    hard_checks = {
        "manifest_exists": manifest_path.exists(),
        "sqlite_exists": sqlite_path.exists(),
        "asset_index_exists": asset_index_path.exists(),
        "source_count_matches_manifest": source_count == manifest.get("card_count"),
        "sqlite_cards_match_source": cards_count == source_count,
        "sqlite_images_match_source": images_count == source_count,
        "no_missing_images": missing_images == 0,
        "no_duplicate_image_paths": duplicate_image_paths == 0,
        "asset_index_images_match_manifest": image_hash_report["asset_index_images_match_manifest"],
        "image_sizes_match_asset_index": image_hash_report["image_sizes_match_asset_index"],
        "image_hashes_match_asset_index": image_hash_report["image_hashes_match_asset_index"],
        "image_content_hash_matches_manifest": image_hash_report["image_content_hash_matches_manifest"],
        "series_count_matches_manifest": series_count == manifest.get("series_count"),
        "clan_count_matches_manifest": clan_count == manifest.get("clan_count"),
        "search_terms_for_every_card": search_count == source_count,
        "bt01_001th_lookup_ok": bool(bt01001 and bt01001[4] == 1),
    }

    report = {
        "verified_at": datetime.now(timezone.utc).isoformat(),
        "pack_dir": str(args.pack_dir.relative_to(ROOT)),
        "manifest_path": str(manifest_path.relative_to(ROOT)),
        "sqlite_path": str(sqlite_path.relative_to(ROOT)),
        "source_json": str(args.source_json.relative_to(ROOT)),
        "asset_index_path": str(asset_index_path.relative_to(ROOT)),
        "source_count": source_count,
        "manifest_card_count": manifest.get("card_count"),
        "sqlite_cards_count": cards_count,
        "sqlite_images_count": images_count,
        "existing_images_count": existing_images_count,
        "missing_images": missing_images,
        "duplicate_image_path_groups": duplicate_image_paths,
        "series_count": series_count,
        "clan_count": clan_count,
        "details_count": details_count,
        "search_terms_count": search_count,
        "asset_index_image_count": image_hash_report["asset_index_image_count"],
        "image_hash_mismatch_count": image_hash_report["image_hash_mismatch_count"],
        "image_size_mismatch_count": image_hash_report["image_size_mismatch_count"],
        "image_content_hash": image_hash_report["image_content_hash"],
        "bt01_001th_sample": {
            "card_id": bt01001[0],
            "name_th": bt01001[1],
            "series": bt01001[2],
            "clan": bt01001[3],
            "image_exists": bool(bt01001[4]),
        }
        if bt01001
        else None,
        "soft_missing_field_counts": soft_missing,
        "checks": hard_checks,
        "all_ok": all(hard_checks.values()),
    }
    return report


def verify_asset_index(asset_index: dict[str, Any] | None, manifest: dict[str, Any]) -> dict[str, Any]:
    if not asset_index:
        return {
            "asset_index_image_count": 0,
            "asset_index_images_match_manifest": False,
            "image_sizes_match_asset_index": False,
            "image_hashes_match_asset_index": False,
            "image_content_hash_matches_manifest": False,
            "image_hash_mismatch_count": 0,
            "image_size_mismatch_count": 0,
            "image_content_hash": None,
        }

    aggregate = hashlib.sha256()
    hash_mismatches = 0
    size_mismatches = 0
    assets = asset_index.get("assets") or []
    for entry in assets:
        image_relative_path = entry.get("image_relative_path")
        if not image_relative_path:
            hash_mismatches += 1
            size_mismatches += 1
            continue

        image_path = ROOT / manifest["image_root"] / image_relative_path
        if not image_path.exists():
            hash_mismatches += 1
            size_mismatches += 1
            continue

        actual_size = image_path.stat().st_size
        actual_hash = sha256_file(image_path)
        expected_size = entry.get("bytes")
        expected_hash = entry.get("sha256")
        if actual_size != expected_size:
            size_mismatches += 1
        if actual_hash != expected_hash:
            hash_mismatches += 1

        aggregate.update(str(entry.get("card_id")).encode("utf-8"))
        aggregate.update(b"\0")
        aggregate.update(str(image_relative_path).encode("utf-8"))
        aggregate.update(b"\0")
        aggregate.update(str(actual_size).encode("ascii"))
        aggregate.update(b"\0")
        aggregate.update(actual_hash.encode("ascii"))
        aggregate.update(b"\n")

    image_content_hash = aggregate.hexdigest()
    asset_count = len(assets)
    return {
        "asset_index_image_count": asset_count,
        "asset_index_images_match_manifest": asset_count == manifest.get("image_count"),
        "image_sizes_match_asset_index": size_mismatches == 0,
        "image_hashes_match_asset_index": hash_mismatches == 0,
        "image_content_hash_matches_manifest": image_content_hash == manifest.get("image_content_hash"),
        "image_hash_mismatch_count": hash_mismatches,
        "image_size_mismatch_count": size_mismatches,
        "image_content_hash": image_content_hash,
    }


def main() -> int:
    args = parse_args()
    report = verify(args)
    if args.write_report:
        write_json(args.pack_dir / "verification_report.json", report)

    print(f"Pack: {report['pack_dir']}")
    print(f"Cards: {report['sqlite_cards_count']}/{report['source_count']}")
    print(f"Images: {report['existing_images_count']}/{report['sqlite_images_count']}")
    print(f"Series: {report['series_count']}")
    print(f"Clans: {report['clan_count']}")
    print(f"All OK: {report['all_ok']}")
    if not report["all_ok"]:
        failed = [key for key, value in report["checks"].items() if not value]
        print("Failed checks:")
        for key in failed:
            print(f"- {key}")
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
