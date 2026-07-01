#!/usr/bin/env python3
"""
Download KK Card Fight card images from an exported card JSON file.

Default input:
  outputs/kk_cardfight_export/data/vanguard_th_cards.json

Default output:
  outputs/kk_cardfight_export/data/images/<series_code>/<clan_key>/<card_id>.jpg
"""

from __future__ import annotations

import argparse
import csv
import hashlib
import json
import re
import sys
import time
from concurrent.futures import ThreadPoolExecutor, as_completed
from pathlib import Path
from typing import Any
from urllib.error import HTTPError, URLError
from urllib.parse import quote, urlparse, urlsplit, urlunsplit
from urllib.request import Request, urlopen


DEFAULT_CARDS = "outputs/kk_cardfight_export/data/vanguard_th_cards.json"
DEFAULT_OUTPUT = "outputs/kk_cardfight_export/data/images"
DEFAULT_MANIFEST = "outputs/kk_cardfight_export/data/image_manifest.csv"
DEFAULT_CARDS_WITH_IMAGES = "outputs/kk_cardfight_export/data/vanguard_th_cards_with_images.json"


def safe_token(value: Any, fallback: str = "N_A", max_len: int = 80) -> str:
    text = str(value or "").strip()
    if not text:
        text = fallback
    text = re.sub(r"[\\/:*?\"<>|]+", "_", text)
    text = re.sub(r"\s+", "_", text)
    text = text.strip("._ ")
    return (text or fallback)[:max_len]


def series_code(series: str | None, card_id: str | None) -> str:
    if series:
        match = re.match(r"\[([^\]]+)\]", series)
        if match:
            return safe_token(match.group(1), max_len=40)
    if card_id and "-" in card_id:
        return safe_token(card_id.split("-", 1)[0], max_len=40)
    return "N_A"


def file_extension(url: str) -> str:
    suffix = Path(urlparse(url).path).suffix.lower()
    if suffix in {".jpg", ".jpeg", ".png", ".webp"}:
        return ".jpg" if suffix == ".jpeg" else suffix
    return ".jpg"


def encode_url(url: str) -> str:
    parts = urlsplit(url)
    path = quote(parts.path, safe="/%")
    query = quote(parts.query, safe="=&?/%")
    return urlunsplit((parts.scheme, parts.netloc, path, query, parts.fragment))


def image_path(output_dir: Path, card: dict[str, Any]) -> Path:
    card_id = safe_token(card.get("card_id") or card.get("key") or card.get("id"), max_len=80)
    series = series_code(card.get("series"), card.get("card_id"))
    clan = safe_token(card.get("clan"), max_len=80)
    ext = file_extension(card.get("image_url") or "")
    return output_dir / series / clan / f"{card_id}{ext}"


def download_one(card: dict[str, Any], path: Path, retries: int, timeout: int) -> dict[str, Any]:
    url = card.get("image_url")
    if not url:
        return {"status": "missing_url", "path": str(path), "bytes": 0}

    if path.exists() and path.stat().st_size > 0:
        return {"status": "skipped", "path": str(path), "bytes": path.stat().st_size}

    path.parent.mkdir(parents=True, exist_ok=True)
    temp_path = path.with_suffix(path.suffix + ".part")
    headers = {"User-Agent": "kk-vanguard-image-export/1.0", "Accept": "image/*,*/*;q=0.8"}

    last_error = ""
    for attempt in range(1, retries + 1):
        try:
            request = Request(encode_url(url), headers=headers)
            with urlopen(request, timeout=timeout) as response:
                payload = response.read()
            if not payload:
                raise RuntimeError("empty response")
            temp_path.write_bytes(payload)
            temp_path.replace(path)
            return {"status": "downloaded", "path": str(path), "bytes": len(payload)}
        except (HTTPError, URLError, TimeoutError, RuntimeError, OSError) as exc:
            last_error = str(exc)
            if temp_path.exists():
                temp_path.unlink(missing_ok=True)
            if attempt < retries:
                time.sleep(min(2 * attempt, 8))

    return {"status": "failed", "path": str(path), "bytes": 0, "error": last_error}


def make_manifest_row(card: dict[str, Any], path: Path, result: dict[str, Any]) -> dict[str, Any]:
    return {
        "card_id": card.get("card_id"),
        "name_th": card.get("name_th"),
        "series": card.get("series"),
        "clan": card.get("clan"),
        "local_image_path": str(path),
        "image_url": card.get("image_url"),
        "status": result.get("status"),
        "bytes": result.get("bytes", 0),
        "error": result.get("error", ""),
    }


def write_manifest(path: Path, rows: list[dict[str, Any]]) -> None:
    fields = [
        "card_id",
        "name_th",
        "series",
        "clan",
        "local_image_path",
        "image_url",
        "status",
        "bytes",
        "error",
    ]
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fields)
        writer.writeheader()
        writer.writerows(rows)


def write_cards_with_images(path: Path, cards: list[dict[str, Any]], rows: list[dict[str, Any]]) -> None:
    by_card_id = {row.get("card_id"): row for row in rows}
    enriched = []
    for card in cards:
        item = dict(card)
        row = by_card_id.get(card.get("card_id"))
        if row:
            item["local_image_path"] = row.get("local_image_path")
            item["image_download_status"] = row.get("status")
            item["image_bytes"] = int(row.get("bytes") or 0)
        enriched.append(item)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(enriched, ensure_ascii=False, indent=2), encoding="utf-8")


def build_summary(rows: list[dict[str, Any]]) -> dict[str, Any]:
    counts: dict[str, int] = {}
    total_bytes = 0
    for row in rows:
        status = str(row.get("status"))
        counts[status] = counts.get(status, 0) + 1
        try:
            total_bytes += int(row.get("bytes") or 0)
        except ValueError:
            pass
    return {"counts": counts, "total_bytes": total_bytes}


def main() -> None:
    parser = argparse.ArgumentParser(description="Download images for exported KK Card Fight cards.")
    parser.add_argument("--cards", default=DEFAULT_CARDS)
    parser.add_argument("--output", default=DEFAULT_OUTPUT)
    parser.add_argument("--manifest", default=DEFAULT_MANIFEST)
    parser.add_argument("--cards-with-images", default=DEFAULT_CARDS_WITH_IMAGES)
    parser.add_argument("--workers", type=int, default=24)
    parser.add_argument("--retries", type=int, default=3)
    parser.add_argument("--timeout", type=int, default=45)
    parser.add_argument("--limit", type=int, default=0, help="For testing only. 0 means all cards.")
    args = parser.parse_args()

    cards = json.loads(Path(args.cards).read_text(encoding="utf-8"))
    if args.limit:
        cards = cards[: args.limit]

    output_dir = Path(args.output)
    rows: list[dict[str, Any]] = []
    tasks: list[tuple[dict[str, Any], Path]] = [(card, image_path(output_dir, card)) for card in cards]

    completed = 0
    with ThreadPoolExecutor(max_workers=max(1, args.workers)) as executor:
        future_map = {
            executor.submit(download_one, card, path, args.retries, args.timeout): (card, path)
            for card, path in tasks
        }
        for future in as_completed(future_map):
            card, path = future_map[future]
            try:
                result = future.result()
            except Exception as exc:  # Defensive fallback; worker handles expected failures.
                result = {"status": "failed", "path": str(path), "bytes": 0, "error": str(exc)}
            rows.append(make_manifest_row(card, path, result))
            completed += 1
            if completed % 250 == 0 or completed == len(tasks):
                summary = build_summary(rows)
                print(
                    f"{completed}/{len(tasks)} "
                    f"downloaded={summary['counts'].get('downloaded', 0)} "
                    f"skipped={summary['counts'].get('skipped', 0)} "
                    f"failed={summary['counts'].get('failed', 0)}",
                    flush=True,
                )

    rows.sort(key=lambda row: (str(row.get("series")), str(row.get("clan")), str(row.get("card_id"))))
    write_manifest(Path(args.manifest), rows)
    write_cards_with_images(Path(args.cards_with_images), cards, rows)

    summary = build_summary(rows)
    summary.update(
        {
            "cards": len(cards),
            "output_dir": str(output_dir),
            "manifest": args.manifest,
            "cards_with_images": args.cards_with_images,
            "total_gib": round(summary["total_bytes"] / 1024 / 1024 / 1024, 3),
            "content_hash": hashlib.sha256(json.dumps(summary, sort_keys=True).encode("utf-8")).hexdigest(),
        }
    )
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    if summary["counts"].get("failed", 0):
        sys.exit(1)


if __name__ == "__main__":
    main()
