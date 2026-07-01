from __future__ import annotations

import json
import sqlite3
import subprocess
import sys
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
PACK_DIR = ROOT / "data/packs/vanguard_th"
DB_PATH = PACK_DIR / "cards.sqlite"
MANIFEST_PATH = PACK_DIR / "manifest.json"
VERIFY_SCRIPT = ROOT / "tools/verification/verify_vanguard_th_pack.py"
QUERY_SCRIPT = ROOT / "tools/data/query_vanguard_th_pack.py"


class VanguardThPackTests(unittest.TestCase):
    def setUp(self) -> None:
        self.conn = sqlite3.connect(DB_PATH)
        self.conn.row_factory = sqlite3.Row

    def tearDown(self) -> None:
        self.conn.close()

    def scalar(self, sql: str, params: tuple[object, ...] = ()) -> object:
        return self.conn.execute(sql, params).fetchone()[0]

    def test_manifest_counts_match_database(self) -> None:
        manifest = json.loads(MANIFEST_PATH.read_text(encoding="utf-8"))
        self.assertEqual(manifest["card_count"], self.scalar("SELECT COUNT(*) FROM cards"))
        self.assertEqual(manifest["image_count"], self.scalar("SELECT COUNT(*) FROM card_images"))
        self.assertEqual(manifest["series_count"], self.scalar("SELECT COUNT(*) FROM series"))
        self.assertEqual(manifest["clan_count"], self.scalar("SELECT COUNT(*) FROM clans"))

    def test_known_card_lookup(self) -> None:
        row = self.conn.execute(
            """
            SELECT c.card_id, c.name_th, c.series_code, c.clan, i.image_exists
            FROM cards c
            JOIN card_images i ON i.card_id = c.card_id
            WHERE c.card_id = ?
            """,
            ("BT01-001TH",),
        ).fetchone()
        self.assertIsNotNone(row)
        self.assertEqual(row["name_th"], "ราชาแห่งอัศวิน, อัลเฟรด")
        self.assertEqual(row["series_code"], "BT01")
        self.assertEqual(row["clan"], "รอยัล พาลาดิน")
        self.assertEqual(row["image_exists"], 1)

    def test_no_missing_images_or_duplicate_image_paths(self) -> None:
        self.assertEqual(0, self.scalar("SELECT COUNT(*) FROM card_images WHERE image_exists = 0"))
        duplicate_groups = self.scalar(
            """
            SELECT COUNT(*) FROM (
              SELECT image_relative_path
              FROM card_images
              GROUP BY image_relative_path
              HAVING COUNT(*) > 1
            )
            """
        )
        self.assertEqual(0, duplicate_groups)

    def test_search_terms_exist_for_every_card(self) -> None:
        self.assertEqual(self.scalar("SELECT COUNT(*) FROM cards"), self.scalar("SELECT COUNT(*) FROM search_terms"))

    def test_verification_script_passes(self) -> None:
        result = subprocess.run(
            [sys.executable, str(VERIFY_SCRIPT)],
            cwd=ROOT,
            text=True,
            encoding="utf-8",
            errors="replace",
            capture_output=True,
            check=False,
        )
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn("All OK: True", result.stdout)

    def test_query_cli_finds_known_card(self) -> None:
        result = subprocess.run(
            [sys.executable, str(QUERY_SCRIPT), "card", "BT01-001TH"],
            cwd=ROOT,
            text=True,
            encoding="utf-8",
            errors="replace",
            capture_output=True,
            check=False,
        )
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn("ราชาแห่งอัศวิน, อัลเฟรด", result.stdout)


if __name__ == "__main__":
    unittest.main()
