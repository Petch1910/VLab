import unittest

from tools.combo.build_combo_matrices import (
    build_era_summary_rows,
    build_group_matrix_rows,
    build_synergy_tag_matrix_rows,
)


class ComboMatrixBuilderTests(unittest.TestCase):
    def test_group_candidate_matrix_uses_zero_for_missing_groups(self):
        reports = [
            (
                "era_a",
                {
                    "card_count": 3,
                    "group_count": 1,
                    "clans": [
                        {
                            "group": "Clan A",
                            "group_field": "clan",
                            "card_count": 3,
                            "combo_pair_count": 2,
                            "top_pairs": [{"score": 90, "synergy_tags": ["named_reference"]}],
                        }
                    ],
                },
            ),
            (
                "era_b",
                {
                    "card_count": 4,
                    "group_count": 1,
                    "clans": [
                        {
                            "group": "Clan B",
                            "group_field": "clan",
                            "card_count": 4,
                            "combo_pair_count": 7,
                            "top_pairs": [{"score": 120, "synergy_tags": ["search_target", "call_target"]}],
                        }
                    ],
                },
            ),
        ]

        rows = build_group_matrix_rows(reports, "combo_pair_count")

        clan_a = next(row for row in rows if row["group"] == "Clan A")
        clan_b = next(row for row in rows if row["group"] == "Clan B")
        self.assertEqual(2, clan_a["era_a"])
        self.assertEqual(0, clan_a["era_b"])
        self.assertEqual(0, clan_b["era_a"])
        self.assertEqual(7, clan_b["era_b"])

    def test_synergy_tag_matrix_counts_top_pair_tags(self):
        reports = [
            (
                "era_a",
                {
                    "clans": [
                        {
                            "group": "Clan A",
                            "group_field": "clan",
                            "top_pairs": [
                                {"synergy_tags": ["named_reference", "search_target"]},
                                {"synergy_tags": ["search_target"]},
                            ],
                        }
                    ],
                },
            )
        ]

        tags, rows = build_synergy_tag_matrix_rows(reports)

        self.assertEqual(["named_reference", "search_target"], tags)
        self.assertEqual(1, len(rows))
        self.assertEqual(2, rows[0]["top_pair_count"])
        self.assertEqual(1, rows[0]["named_reference"])
        self.assertEqual(2, rows[0]["search_target"])

    def test_era_summary_rows_include_missing_set_count(self):
        reports = [
            (
                "era_a",
                {
                    "card_count": 10,
                    "group_count": 2,
                    "scope": {"missing_set_codes": ["X01", "X02"]},
                    "clans": [
                        {"combo_pair_count": 3},
                        {"combo_pair_count": 0},
                    ],
                },
            )
        ]

        rows = build_era_summary_rows(reports)

        self.assertEqual(1, rows[0]["nonempty_group_count"])
        self.assertEqual(3, rows[0]["candidate_pair_count"])
        self.assertEqual(2, rows[0]["missing_set_count"])


if __name__ == "__main__":
    unittest.main()
