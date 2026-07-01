import unittest

from tools.deck.analyze_deck_possibilities import (
    DeckCardRecord,
    analyze_group,
    bounded_distribution_count,
    build_summary_json,
    build_deck_possibility_report,
)


class DeckPossibilityAnalyzerTests(unittest.TestCase):
    def test_bounded_distribution_count_respects_copy_limits(self):
        self.assertEqual(2, bounded_distribution_count([2, 2], 3))
        self.assertEqual(1, bounded_distribution_count([4], 4))
        self.assertEqual(0, bounded_distribution_count([1, 1], 3))

    def test_analyze_group_marks_50_card_trigger_feasible(self):
        cards = []
        for index in range(4):
            cards.append(
                DeckCardRecord(
                    card_id=f"T{index}",
                    series_code="BT01",
                    clan="Clan A",
                    nation="Nation A",
                    grade=0,
                    trigger="Critical",
                    deck_limit=4,
                    type_1="Trigger Unit",
                    type_2="",
                )
            )
        for index in range(9):
            cards.append(
                DeckCardRecord(
                    card_id=f"N{index}",
                    series_code="BT01",
                    clan="Clan A",
                    nation="Nation A",
                    grade=(index % 3) + 1,
                    trigger="",
                    deck_limit=4,
                    type_1="Normal Unit",
                    type_2="",
                )
            )

        report = analyze_group("Clan A", "clan", cards)

        self.assertTrue(report["feasible"]["basic_50_card_main"])
        self.assertTrue(report["feasible"]["main_with_16_triggers_34_non_triggers"])
        self.assertEqual(16, report["trigger_capacity"])
        self.assertEqual(36, report["non_trigger_capacity"])
        self.assertGreater(int(report["possibility_counts"]["main_50_exact_16_triggers"]["exact"]), 0)

    def test_build_report_groups_d_series_by_nation_in_auto_mode(self):
        cards = [
            DeckCardRecord(
                card_id="D1",
                series_code="D-BT01",
                clan="N/A",
                nation="Dragon Empire D",
                grade=3,
                trigger="",
                deck_limit=4,
                type_1="Normal Unit",
                type_2="",
            )
        ]

        report = build_deck_possibility_report(cards, ["D-BT01"], "d_overdress", group_mode="auto")

        self.assertEqual(1, report["group_count"])
        self.assertEqual("Dragon Empire", report["groups"][0]["group"])
        self.assertEqual("nation", report["groups"][0]["group_field"])

    def test_summary_json_counts_feasible_groups(self):
        report = {
            "scope": {"preset": "sample", "missing_set_codes": ["X01"]},
            "card_count": 1,
            "group_count": 2,
            "groups": [
                {
                    "feasible": {
                        "basic_50_card_main": True,
                        "main_with_16_triggers_34_non_triggers": True,
                        "ride_deck_grade_0_1_2_3_choice": False,
                        "g_zone_16": False,
                    }
                },
                {
                    "feasible": {
                        "basic_50_card_main": False,
                        "main_with_16_triggers_34_non_triggers": False,
                        "ride_deck_grade_0_1_2_3_choice": False,
                        "g_zone_16": True,
                    }
                },
            ],
        }

        summary = build_summary_json([report])

        row = summary["reports"][0]
        self.assertEqual(1, row["basic_50_feasible_groups"])
        self.assertEqual(1, row["exact_16_trigger_feasible_groups"])
        self.assertEqual(1, row["g_zone_16_feasible_groups"])
        self.assertEqual(["X01"], row["missing_set_codes"])


if __name__ == "__main__":
    unittest.main()
