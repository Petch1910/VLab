import unittest

from tools.combo.discover_clan_combos import (
    CardRecord,
    ERA_SET_SPECS,
    build_clan_combo_report,
    expand_set_specs,
    extract_features,
    get_card_group,
)


class ClanComboDiscoveryTests(unittest.TestCase):
    def test_expand_set_specs_supports_requested_ranges(self):
        self.assertEqual(
            ["TD01", "TD02", "TD03", "BT01", "BT02", "EB01"],
            expand_set_specs(["TD01-TD03", "BT01-BT02", "EB01"]),
        )

    def test_expand_set_specs_supports_prefixed_ranges(self):
        self.assertEqual(
            ["G-TD01", "G-TD02", "G-TD03", "D-LBT01", "D-LBT02", "DZ-SS01"],
            expand_set_specs(["G-TD01-G-TD03", "D-LBT01-D-LBT02", "DZ-SS01"]),
        )

    def test_era_presets_cover_requested_followup_ranges(self):
        self.assertIn("link_joker_legion_mate", ERA_SET_SPECS)
        self.assertIn("g_series_first", ERA_SET_SPECS)
        self.assertIn("g_next_z", ERA_SET_SPECS)
        self.assertIn("v_reboot", ERA_SET_SPECS)
        self.assertIn("v_shinemon_if", ERA_SET_SPECS)
        self.assertIn("d_overdress", ERA_SET_SPECS)
        self.assertIn("d_willdress", ERA_SET_SPECS)
        self.assertIn("dz_divinez", ERA_SET_SPECS)

    def test_auto_group_uses_nation_when_clan_is_na(self):
        card = CardRecord(
            card_id="D-BT01-001TH",
            name_th="D Unit",
            text_th="",
            series_code="D-BT01",
            clan="N/A",
            grade=3,
            power=13000,
            shield=None,
            trigger="",
            type_1="Normal Unit",
            nation="Dragon Empire D",
        )

        group, field = get_card_group(card, "auto")

        self.assertEqual("Dragon Empire", group)
        self.assertEqual("nation", field)

    def test_extract_features_detects_common_combo_text(self):
        card = CardRecord(
            card_id="BT01-001TH",
            name_th="Searcher",
            text_th='[ACT](VC/RC):[Counter Blast (1)] หาการ์ดแคลน <<รอยัล พาลาดิน>> เกรด 2 ไม่เกิน 1 ใบแล้วคอลลงช่อง (RC)',
            series_code="BT01",
            clan="รอยัล พาลาดิน",
            grade=3,
            power=10000,
            shield=None,
            trigger="",
            type_1="Normal Unit",
        )

        features = extract_features(card)

        self.assertIn("counter_blast_cost", features.tags)
        self.assertIn("search_deck", features.tags)
        self.assertIn("superior_call", features.tags)
        self.assertIn("รอยัล พาลาดิน", features.clan_refs)
        self.assertIn(2, features.grade_refs)

    def test_extract_features_detects_spaced_power_and_critical_bonus(self):
        card = CardRecord(
            card_id="BT01-010TH",
            name_th="Pressure Unit",
            text_th="[ACT](VC): [Counter Blast (4)] ยูนิทนี้ [Power] +3000/[Critical] +1 จนจบเทิร์น",
            series_code="BT01",
            clan="คาเงโร่",
            grade=3,
            power=10000,
            shield=None,
            trigger="",
            type_1="Normal Unit",
        )

        features = extract_features(card)

        self.assertIn("power_plus", features.tags)
        self.assertIn("critical_plus", features.tags)
        self.assertEqual((3000,), features.power_bonuses)

    def test_report_ranks_named_and_resource_pairs_inside_each_clan(self):
        cards = [
            CardRecord(
                card_id="BT01-001TH",
                name_th="Grade Three",
                text_th='[AUTO]: เมื่อการ์ดที่ชื่อ "Grade Two" ไรด์ทับยูนิทนี้, จั่วการ์ด 1 ใบ',
                series_code="BT01",
                clan="Clan A",
                grade=1,
                power=7000,
                shield=5000,
                trigger="",
                type_1="Normal Unit",
            ),
            CardRecord(
                card_id="BT01-002TH",
                name_th="Grade Two",
                text_th="[AUTO](VC/RC): เมื่อยูนิทนี้โจมตีฮิทแวนการ์ด, จั่วการ์ด 1 ใบ",
                series_code="BT01",
                clan="Clan A",
                grade=2,
                power=9000,
                shield=5000,
                trigger="",
                type_1="Normal Unit",
            ),
            CardRecord(
                card_id="BT01-003TH",
                name_th="Soul Maker",
                text_th="[AUTO]: เมื่อยูนิทนี้เข้าสู่ช่อง (RC), Soul Charge (1)",
                series_code="BT01",
                clan="Clan A",
                grade=1,
                power=7000,
                shield=5000,
                trigger="",
                type_1="Normal Unit",
            ),
            CardRecord(
                card_id="BT01-004TH",
                name_th="Soul Spender",
                text_th="[ACT](VC):[Soul Blast (2)] ยูนิทนี้ [Power]+5000 จนจบเทิร์น",
                series_code="BT01",
                clan="Clan A",
                grade=3,
                power=10000,
                shield=None,
                trigger="",
                type_1="Normal Unit",
            ),
            CardRecord(
                card_id="BT01-005TH",
                name_th="Other Clan",
                text_th="[AUTO]: เมื่อยูนิทนี้โจมตี, ยูนิทนี้ [Power]+3000",
                series_code="BT01",
                clan="Clan B",
                grade=2,
                power=9000,
                shield=5000,
                trigger="",
                type_1="Normal Unit",
            ),
        ]

        report = build_clan_combo_report(cards, ["BT01"], top_per_clan=10, min_score=20)
        clan_a = next(clan for clan in report["clans"] if clan["clan"] == "Clan A")
        clan_b = next(clan for clan in report["clans"] if clan["clan"] == "Clan B")

        self.assertEqual(1, report["schema_version"])
        self.assertGreaterEqual(clan_a["combo_pair_count"], 2)
        self.assertEqual(0, clan_b["combo_pair_count"])
        pair_ids = {pair["pair_id"] for pair in clan_a["top_pairs"]}
        self.assertIn("BT01-001TH+BT01-002TH", pair_ids)
        self.assertIn("BT01-003TH+BT01-004TH", pair_ids)

        named_pair = next(pair for pair in clan_a["top_pairs"] if pair["pair_id"] == "BT01-001TH+BT01-002TH")
        self.assertIn("named_reference", named_pair["synergy_tags"])


if __name__ == "__main__":
    unittest.main()
