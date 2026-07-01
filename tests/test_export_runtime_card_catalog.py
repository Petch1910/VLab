import json
import sqlite3
import tempfile
import unittest
from pathlib import Path

from tools.data.export_runtime_card_catalog import export_catalog


class RuntimeCardCatalogExportTests(unittest.TestCase):
    def test_export_catalog_writes_card_fields_and_formats(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            root = Path(temp_dir)
            database = root / "cards.sqlite"
            output = root / "card_catalog.json"
            self._create_database(database)

            catalog = export_catalog(database, output)

            self.assertEqual(1, catalog["schema_version"])
            self.assertEqual(2, catalog["card_count"])
            self.assertTrue(output.exists())

            saved = json.loads(output.read_text(encoding="utf-8"))
            self.assertEqual(2, len(saved["cards"]))
            first = saved["cards"][0]
            self.assertEqual("BT01-001TH", first["card_id"])
            self.assertTrue(first["grade_has_value"])
            self.assertEqual(3, first["grade"])
            self.assertFalse(first["shield_has_value"])
            self.assertEqual("BT01/royal/BT01-001TH.jpg", first["image_relative_path"])
            self.assertEqual(
                [{"format_key": "premium_standard_format", "format_value": "Premium Standard Format"}],
                first["formats"],
            )

            second = saved["cards"][1]
            self.assertFalse(second["grade_has_value"])
            self.assertTrue(second["shield_has_value"])
            self.assertEqual(15000, second["shield"])

    @staticmethod
    def _create_database(database: Path) -> None:
        connection = sqlite3.connect(str(database))
        try:
            connection.executescript(
                """
                CREATE TABLE cards (
                    card_id TEXT PRIMARY KEY,
                    source_id TEXT,
                    source_key TEXT,
                    name_th TEXT,
                    text_th TEXT,
                    series TEXT,
                    series_code TEXT,
                    clan TEXT,
                    nation TEXT,
                    nation_2 TEXT,
                    grade INTEGER,
                    power INTEGER,
                    shield INTEGER,
                    trigger TEXT,
                    deck_limit INTEGER,
                    type_1 TEXT,
                    type_2 TEXT,
                    race_1 TEXT,
                    race_2 TEXT,
                    warning TEXT
                );
                CREATE TABLE card_images (
                    card_id TEXT PRIMARY KEY,
                    image_url TEXT,
                    image_relative_path TEXT,
                    image_exists INTEGER
                );
                CREATE TABLE card_formats (
                    card_id TEXT,
                    format_key TEXT,
                    format_value TEXT
                );
                """
            )
            connection.execute(
                """
                INSERT INTO cards VALUES (
                    'BT01-001TH','source-1','BT01-001TH','Alfred','text',
                    '[BT01] Test','BT01','Royal Paladin','United Sanctuary','',
                    3,10000,NULL,'',4,'Normal Unit','','Human','',''
                )
                """
            )
            connection.execute(
                """
                INSERT INTO cards VALUES (
                    'BT01-002TH','source-2','BT01-002TH','Trigger','trigger text',
                    '[BT01] Test','BT01','Royal Paladin','United Sanctuary','',
                    NULL,5000,15000,'Critical',4,'Trigger Unit','','Human','',''
                )
                """
            )
            connection.execute(
                "INSERT INTO card_images VALUES ('BT01-001TH','url','BT01/royal/BT01-001TH.jpg',1)"
            )
            connection.execute(
                "INSERT INTO card_images VALUES ('BT01-002TH','url','BT01/royal/BT01-002TH.jpg',1)"
            )
            connection.execute(
                "INSERT INTO card_formats VALUES ('BT01-001TH','premium_standard_format','Premium Standard Format')"
            )
            connection.commit()
        finally:
            connection.close()


if __name__ == "__main__":
    unittest.main()
