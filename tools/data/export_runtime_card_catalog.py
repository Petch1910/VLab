"""Export a runtime JSON card catalog from the SQLite card pack.

The catalog is a mobile/development fallback for environments where the native
SQLite provider is unavailable. It intentionally exports the card fields needed
by the current Unity card browser, deck validator, and detail panel without the
large raw-details table.
"""

from __future__ import annotations

import argparse
import json
import sqlite3
from datetime import datetime, timezone
from pathlib import Path
from typing import Any, Dict, Iterable, List


ROOT = Path(__file__).resolve().parents[2]
DEFAULT_PACK = ROOT / "data" / "packs" / "vanguard_th"
DEFAULT_DATABASE = DEFAULT_PACK / "cards.sqlite"
DEFAULT_OUTPUT = DEFAULT_PACK / "card_catalog.json"


def optional_int(value: Any) -> Dict[str, Any]:
    if value is None:
        return {"has_value": False, "value": 0}
    return {"has_value": True, "value": int(value)}


def load_formats(connection: sqlite3.Connection) -> Dict[str, List[Dict[str, str]]]:
    formats: Dict[str, List[Dict[str, str]]] = {}
    query = """
        SELECT card_id, format_key, format_value
        FROM card_formats
        ORDER BY card_id, format_key
    """
    for card_id, format_key, format_value in connection.execute(query):
        formats.setdefault(card_id, []).append(
            {
                "format_key": format_key or "",
                "format_value": format_value or "",
            }
        )
    return formats


def iter_card_records(connection: sqlite3.Connection) -> Iterable[Dict[str, Any]]:
    formats_by_card = load_formats(connection)
    query = """
        SELECT
          c.card_id,
          c.source_id,
          c.source_key,
          c.name_th,
          c.text_th,
          c.series,
          c.series_code,
          c.clan,
          c.nation,
          c.nation_2,
          c.grade,
          c.power,
          c.shield,
          c.trigger,
          c.deck_limit,
          c.type_1,
          c.type_2,
          c.race_1,
          c.race_2,
          c.warning,
          i.image_url,
          i.image_relative_path,
          i.image_exists
        FROM cards c
        JOIN card_images i ON i.card_id = c.card_id
        ORDER BY c.card_id
    """
    for row in connection.execute(query):
        grade = optional_int(row[10])
        power = optional_int(row[11])
        shield = optional_int(row[12])
        card_id = row[0]
        yield {
            "card_id": card_id,
            "source_id": row[1] or "",
            "source_key": row[2] or "",
            "name_th": row[3] or "",
            "text_th": row[4] or "",
            "series": row[5] or "",
            "series_code": row[6] or "",
            "clan": row[7] or "",
            "nation": row[8] or "",
            "nation_2": row[9] or "",
            "grade_has_value": grade["has_value"],
            "grade": grade["value"],
            "power_has_value": power["has_value"],
            "power": power["value"],
            "shield_has_value": shield["has_value"],
            "shield": shield["value"],
            "trigger": row[13] or "",
            "deck_limit": int(row[14] or 0),
            "type_1": row[15] or "",
            "type_2": row[16] or "",
            "race_1": row[17] or "",
            "race_2": row[18] or "",
            "warning": row[19] or "",
            "image_url": row[20] or "",
            "image_relative_path": row[21] or "",
            "image_exists": bool(row[22]),
            "formats": formats_by_card.get(card_id, []),
        }


def build_catalog(database_path: Path) -> Dict[str, Any]:
    connection = sqlite3.connect(str(database_path))
    try:
        cards = list(iter_card_records(connection))
    finally:
        connection.close()

    return {
        "schema_version": 1,
        "generated_at_utc": datetime.now(timezone.utc).isoformat(),
        "source_database": str(database_path.as_posix()),
        "card_count": len(cards),
        "raw_details_policy": "omitted_v1_current_ui_uses_card_text",
        "cards": cards,
    }


def export_catalog(database_path: Path, output_path: Path) -> Dict[str, Any]:
    if not database_path.exists():
        raise FileNotFoundError(f"SQLite database was not found: {database_path}")

    catalog = build_catalog(database_path)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps(catalog, ensure_ascii=False, separators=(",", ":")) + "\n", encoding="utf-8")
    return catalog


def build_arg_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Export Unity runtime JSON card catalog from cards.sqlite.")
    parser.add_argument("--database", default=str(DEFAULT_DATABASE), help="Input cards.sqlite path.")
    parser.add_argument("--output", default=str(DEFAULT_OUTPUT), help="Output card_catalog.json path.")
    return parser


def main() -> int:
    args = build_arg_parser().parse_args()
    catalog = export_catalog(Path(args.database), Path(args.output))
    print(f"Exported {catalog['card_count']} cards to {args.output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
