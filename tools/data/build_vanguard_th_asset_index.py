"""Build a content-addressed image asset index for the Vanguard TH pack."""

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


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build Vanguard TH image asset index.")
    parser.add_argument("--pack-dir", type=Path, default=DEFAULT_PACK_DIR)
    parser.add_argument("--update-manifest", action="store_true", default=True)
    return parser.parse_args()


def read_json(path: Path) -> Any:
    with path.open("r", encoding="utf-8") as f:
        return json.load(f)


def write_json(path: Path, data: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="\n") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)
        f.write("\n")


def normalize_path(path: Path | str) -> str:
    return str(path).replace("\\", "/")


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def content_address(image_hash: str, extension: str) -> str:
    clean_extension = extension.lower() if extension else ".jpg"
    return normalize_path(Path("sha256") / image_hash[:2] / (image_hash + clean_extension))


def load_image_rows(pack_dir: Path) -> list[sqlite3.Row]:
    sqlite_path = pack_dir / "cards.sqlite"
    conn = sqlite3.connect(sqlite_path)
    conn.row_factory = sqlite3.Row
    try:
        return list(
            conn.execute(
                """
                SELECT card_id, local_image_path, image_relative_path
                FROM card_images
                ORDER BY card_id
                """
            )
        )
    finally:
        conn.close()


def build_asset_index(pack_dir: Path) -> dict[str, Any]:
    manifest_path = pack_dir / "manifest.json"
    manifest = read_json(manifest_path)
    assets: list[dict[str, Any]] = []
    aggregate = hashlib.sha256()

    for row in load_image_rows(pack_dir):
        image_path = ROOT / row["local_image_path"]
        image_hash = sha256_file(image_path)
        image_size = image_path.stat().st_size
        image_relative_path = normalize_path(row["image_relative_path"])
        entry = {
            "card_id": row["card_id"],
            "image_relative_path": image_relative_path,
            "bytes": image_size,
            "sha256": image_hash,
            "content_address": content_address(image_hash, image_path.suffix),
        }
        assets.append(entry)
        aggregate.update(entry["card_id"].encode("utf-8"))
        aggregate.update(b"\0")
        aggregate.update(image_relative_path.encode("utf-8"))
        aggregate.update(b"\0")
        aggregate.update(str(image_size).encode("ascii"))
        aggregate.update(b"\0")
        aggregate.update(image_hash.encode("ascii"))
        aggregate.update(b"\n")

    return {
        "schema_version": 1,
        "pack_id": manifest.get("pack_id", "vanguard_th"),
        "generated_at": datetime.now(timezone.utc).isoformat(),
        "image_root": manifest.get("image_root"),
        "image_count": len(assets),
        "aggregate_hash": aggregate.hexdigest(),
        "assets": assets,
    }


def update_manifest(pack_dir: Path, asset_index: dict[str, Any]) -> None:
    manifest_path = pack_dir / "manifest.json"
    manifest = read_json(manifest_path)
    manifest["asset_index_file"] = "asset_index.json"
    manifest["asset_index_schema_version"] = asset_index["schema_version"]
    manifest["image_content_hash"] = asset_index["aggregate_hash"]
    manifest["image_cache_strategy"] = "content_addressed_external_source"
    manifest["cache_layout_version"] = 1
    manifest["user_data_policy"] = "user_data_must_not_be_stored_in_generated_pack"
    write_json(manifest_path, manifest)


def main() -> int:
    args = parse_args()
    asset_index = build_asset_index(args.pack_dir)
    write_json(args.pack_dir / "asset_index.json", asset_index)
    if args.update_manifest:
        update_manifest(args.pack_dir, asset_index)

    print(f"Asset index: {args.pack_dir / 'asset_index.json'}")
    print(f"Images: {asset_index['image_count']}")
    print(f"Image content hash: {asset_index['aggregate_hash']}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
