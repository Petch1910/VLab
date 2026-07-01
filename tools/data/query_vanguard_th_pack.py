"""Inspect and query the Vanguard TH runtime pack SQLite database."""

from __future__ import annotations

import argparse
import json
import sqlite3
import sys
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[2]
DEFAULT_DB = ROOT / "data/packs/vanguard_th/cards.sqlite"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Query Vanguard TH runtime pack.")
    parser.add_argument("--db", type=Path, default=DEFAULT_DB)

    subparsers = parser.add_subparsers(dest="command", required=True)

    search = subparsers.add_parser("search", help="Search cards by text.")
    search.add_argument("query")
    search.add_argument("--limit", type=int, default=20)

    card = subparsers.add_parser("card", help="Show one card by card id.")
    card.add_argument("card_id")

    series = subparsers.add_parser("series", help="List series counts.")
    series.add_argument("--limit", type=int, default=20)

    clans = subparsers.add_parser("clans", help="List clan counts.")
    clans.add_argument("--limit", type=int, default=20)

    subparsers.add_parser("summary", help="Show pack summary.")
    return parser.parse_args()


def connect(path: Path) -> sqlite3.Connection:
    conn = sqlite3.connect(path)
    conn.row_factory = sqlite3.Row
    return conn


def print_rows(rows: list[sqlite3.Row], columns: list[str]) -> None:
    if not rows:
        print("No rows.")
        return

    widths = {
        column: min(max(len(column), *(len(str(row[column] or "")) for row in rows)), 80)
        for column in columns
    }
    print(" | ".join(column.ljust(widths[column]) for column in columns))
    print("-+-".join("-" * widths[column] for column in columns))
    for row in rows:
        values = []
        for column in columns:
            text = str(row[column] or "")
            if len(text) > widths[column]:
                text = text[: widths[column] - 1] + "…"
            values.append(text.ljust(widths[column]))
        print(" | ".join(values))


def search_cards(conn: sqlite3.Connection, query: str, limit: int) -> list[sqlite3.Row]:
    fts_available = conn.execute(
        "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'cards_fts'"
    ).fetchone()[0]
    if fts_available:
        try:
            return conn.execute(
                """
                SELECT c.card_id, c.name_th, c.series_code, c.clan, c.grade, c.type_1
                FROM cards_fts f
                JOIN cards c ON c.card_id = f.card_id
                WHERE cards_fts MATCH ?
                ORDER BY rank
                LIMIT ?
                """,
                (query, limit),
            ).fetchall()
        except sqlite3.OperationalError:
            pass

    pattern = f"%{query}%"
    return conn.execute(
        """
        SELECT c.card_id, c.name_th, c.series_code, c.clan, c.grade, c.type_1
        FROM search_terms s
        JOIN cards c ON c.card_id = s.card_id
        WHERE s.text LIKE ?
        ORDER BY c.card_id
        LIMIT ?
        """,
        (pattern, limit),
    ).fetchall()


def show_card(conn: sqlite3.Connection, card_id: str) -> dict[str, Any] | None:
    row = conn.execute(
        """
        SELECT c.*, i.image_url, i.local_image_path, i.image_relative_path, i.image_exists
        FROM cards c
        LEFT JOIN card_images i ON i.card_id = c.card_id
        WHERE c.card_id = ?
        """,
        (card_id,),
    ).fetchone()
    if not row:
        return None

    data = dict(row)
    data["image_exists"] = bool(data["image_exists"])
    data["details"] = [
        dict(detail)
        for detail in conn.execute(
            "SELECT label, value FROM card_details WHERE card_id = ? ORDER BY sort_order",
            (card_id,),
        ).fetchall()
    ]
    data["formats"] = [
        dict(fmt)
        for fmt in conn.execute(
            "SELECT format_key, format_value FROM card_formats WHERE card_id = ? ORDER BY format_key",
            (card_id,),
        ).fetchall()
    ]
    data.pop("raw_json", None)
    return data


def list_series(conn: sqlite3.Connection, limit: int) -> list[sqlite3.Row]:
    return conn.execute(
        "SELECT series_code, series, card_count FROM series ORDER BY series_code LIMIT ?",
        (limit,),
    ).fetchall()


def list_clans(conn: sqlite3.Connection, limit: int) -> list[sqlite3.Row]:
    return conn.execute(
        "SELECT clan, card_count FROM clans ORDER BY card_count DESC, clan LIMIT ?",
        (limit,),
    ).fetchall()


def summary(conn: sqlite3.Connection) -> dict[str, Any]:
    return {
        "cards": conn.execute("SELECT COUNT(*) FROM cards").fetchone()[0],
        "images": conn.execute("SELECT COUNT(*) FROM card_images").fetchone()[0],
        "existing_images": conn.execute("SELECT COUNT(*) FROM card_images WHERE image_exists = 1").fetchone()[0],
        "series": conn.execute("SELECT COUNT(*) FROM series").fetchone()[0],
        "clans": conn.execute("SELECT COUNT(*) FROM clans").fetchone()[0],
        "fts_enabled": bool(
            conn.execute(
                "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'cards_fts'"
            ).fetchone()[0]
        ),
    }


def main() -> int:
    args = parse_args()
    with connect(args.db) as conn:
        if args.command == "search":
            rows = search_cards(conn, args.query, args.limit)
            print_rows(rows, ["card_id", "name_th", "series_code", "clan", "grade", "type_1"])
        elif args.command == "card":
            card = show_card(conn, args.card_id)
            if not card:
                print(f"Card not found: {args.card_id}")
                return 1
            print(json.dumps(card, ensure_ascii=False, indent=2))
        elif args.command == "series":
            print_rows(list_series(conn, args.limit), ["series_code", "series", "card_count"])
        elif args.command == "clans":
            print_rows(list_clans(conn, args.limit), ["clan", "card_count"])
        elif args.command == "summary":
            print(json.dumps(summary(conn), ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
