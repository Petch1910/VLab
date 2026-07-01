#!/usr/bin/env python3
"""
Export KK Card Fight Vanguard TH card data into bot-friendly files.

The app exposes a public card-file pointer through:
  https://user-api.kkcardfight.com/api/player/card/list?gameId=<game_id>

This script downloads that JSON file and normalizes the Thai detail labels into
stable fields that are easier to query from a bot.
"""

from __future__ import annotations

import argparse
import csv
import json
import re
from collections import Counter, defaultdict
from datetime import datetime, timezone
from pathlib import Path
from typing import Any
from urllib.request import Request, urlopen


GAME_ID_VANGUARD_TH = "62a192ac1991c4ed95b6a12e"
API_BASE = "https://user-api.kkcardfight.com/api"
CARD_LIST_ENDPOINT = f"{API_BASE}/player/card/list"

DETAIL_LABELS = {
    "ชื่อการ์ด": "name_th",
    "รหัสของการ์ด": "card_id",
    "รายละเอียดของการ์ด": "text_th",
    "จำนวนที่ใส่ได้": "deck_limit",
    "ซีรี่ส์": "series",
    "ชนิดที่ 1": "type_1",
    "ชนิดที่ 2": "type_2",
    "แคลน": "clan",
    "เกรด": "grade",
    "ชิลด์": "shield",
    "ทริกเกอร์": "trigger",
    "เนชั่น": "nation",
    "เนชั่น2": "nation_2",
    "เผ่าที่ 1": "race_1",
    "เผ่าที่ 2": "race_2",
    "พลัง": "power",
    "อิมเมจินนารี่กิฟต์": "imaginary_gift",
    "เพอร์โซนาไรด์": "persona_ride",
    "D-Standard Format": "d_standard_format",
    "V-Standard Format": "v_standard_format",
    "Premium Standard Format": "premium_standard_format",
    "ฟอร์แมท": "format",
    "คำเตือน": "warning_detail",
}


def fetch_json(url: str) -> dict[str, Any]:
    request = Request(url, headers={"Accept": "application/json", "User-Agent": "kk-vanguard-export/1.0"})
    with urlopen(request, timeout=120) as response:
        raw = response.read()
    return json.loads(raw.decode("utf-8"))


def detail_map(card: dict[str, Any]) -> dict[str, Any]:
    mapped: dict[str, Any] = {}
    for item in card.get("detail", []):
        label = item.get("label")
        key = DETAIL_LABELS.get(label, label)
        if key:
            mapped[key] = item.get("value")
    return mapped


def normalize_card(card: dict[str, Any]) -> dict[str, Any]:
    details = detail_map(card)
    normalized = {
        "id": card.get("id"),
        "key": card.get("key"),
        "card_id": details.get("card_id") or card.get("cardId"),
        "name_th": details.get("name_th") or card.get("name"),
        "text_th": details.get("text_th") or card.get("cardContentThai"),
        "series": details.get("series") or "N/A",
        "clan": details.get("clan") or "N/A",
        "nation": details.get("nation"),
        "nation_2": details.get("nation_2"),
        "race_1": details.get("race_1"),
        "race_2": details.get("race_2"),
        "type_1": details.get("type_1") or card.get("typeOne"),
        "type_2": details.get("type_2") or card.get("typeTwo"),
        "grade": details.get("grade"),
        "power": details.get("power") if details.get("power") is not None else card.get("power"),
        "shield": details.get("shield") if details.get("shield") is not None else card.get("shield"),
        "trigger": details.get("trigger"),
        "deck_limit": details.get("deck_limit"),
        "image_url": card.get("imagePath"),
        "warning": card.get("warning") or details.get("warning_detail"),
        "d_standard_format": details.get("d_standard_format"),
        "v_standard_format": details.get("v_standard_format"),
        "premium_standard_format": details.get("premium_standard_format"),
        "raw_detail": card.get("detail", []),
    }
    return normalized


def safe_name(value: str) -> str:
    value = re.sub(r"[\\/:*?\"<>|]+", "_", value)
    value = re.sub(r"\s+", " ", value).strip()
    return value[:120] or "N_A"


def write_json(path: Path, data: Any) -> None:
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")


def write_cards_csv(path: Path, cards: list[dict[str, Any]]) -> None:
    fields = [
        "card_id",
        "name_th",
        "series",
        "clan",
        "nation",
        "type_1",
        "type_2",
        "grade",
        "power",
        "shield",
        "trigger",
        "deck_limit",
        "image_url",
        "text_th",
    ]
    with path.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fields)
        writer.writeheader()
        for card in cards:
            writer.writerow({field: card.get(field) for field in fields})


def write_summary_csv(path: Path, rows: list[dict[str, Any]], fields: list[str]) -> None:
    with path.open("w", encoding="utf-8-sig", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fields)
        writer.writeheader()
        writer.writerows(rows)


def build_nested(cards: list[dict[str, Any]]) -> dict[str, Any]:
    nested: dict[str, Any] = defaultdict(lambda: defaultdict(list))
    for card in cards:
        item = {
            "card_id": card["card_id"],
            "name_th": card["name_th"],
            "grade": card["grade"],
            "type_1": card["type_1"],
            "power": card["power"],
            "shield": card["shield"],
            "image_url": card["image_url"],
        }
        nested[card["series"]][card["clan"]].append(item)
    return {series: dict(clans) for series, clans in nested.items()}


def export(args: argparse.Namespace) -> None:
    out_dir = Path(args.output)
    out_dir.mkdir(parents=True, exist_ok=True)

    pointer_url = f"{CARD_LIST_ENDPOINT}?gameId={args.game_id}"
    pointer = fetch_json(pointer_url)
    card_file_url = pointer["filePath"]
    raw = fetch_json(card_file_url)

    cards = [normalize_card(card) for card in raw["cardList"]]
    cards.sort(key=lambda card: (card["series"], card["clan"], card["card_id"] or ""))

    series_counts = Counter(card["series"] for card in cards)
    clan_counts = Counter(card["clan"] for card in cards)
    series_clan_counts = Counter((card["series"], card["clan"]) for card in cards)

    series_summary = [{"series": key, "count": count} for key, count in series_counts.items()]
    clan_summary = [{"clan": key, "count": count} for key, count in clan_counts.items()]
    series_clan_summary = [
        {"series": series, "clan": clan, "count": count}
        for (series, clan), count in series_clan_counts.items()
    ]

    meta = {
        "exported_at": datetime.now(timezone.utc).isoformat(),
        "source_pointer_url": pointer_url,
        "source_card_file_url": card_file_url,
        "game_detail": raw.get("gameDetail"),
        "total_from_source": raw.get("total"),
        "total_exported": len(cards),
        "series_count": len(series_counts),
        "clan_count": len(clan_counts),
    }

    write_json(out_dir / "meta.json", meta)
    write_json(out_dir / "vanguard_th_cards.json", cards)
    write_json(out_dir / "vanguard_th_by_series_clan.json", build_nested(cards))
    write_json(out_dir / "series_summary.json", series_summary)
    write_json(out_dir / "clan_summary.json", clan_summary)
    write_json(out_dir / "series_clan_summary.json", series_clan_summary)
    write_cards_csv(out_dir / "vanguard_th_cards.csv", cards)
    write_summary_csv(out_dir / "series_summary.csv", series_summary, ["series", "count"])
    write_summary_csv(out_dir / "clan_summary.csv", clan_summary, ["clan", "count"])
    write_summary_csv(out_dir / "series_clan_summary.csv", series_clan_summary, ["series", "clan", "count"])

    if args.raw:
        write_json(out_dir / "raw_card_detail.json", raw)

    print(json.dumps(meta, ensure_ascii=False, indent=2))


def main() -> None:
    parser = argparse.ArgumentParser(description="Export KK Card Fight Vanguard TH cards.")
    parser.add_argument("--game-id", default=GAME_ID_VANGUARD_TH)
    parser.add_argument("--output", default="outputs/kk_cardfight_export/data")
    parser.add_argument("--raw", action="store_true", help="Also save the unnormalized source JSON.")
    export(parser.parse_args())


if __name__ == "__main__":
    main()
