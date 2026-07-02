"""Tests for tools/deck/build_seventh_slice_fixture_readiness.py."""

from __future__ import annotations

import copy
import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.combo.discover_clan_combos import ERA_SET_SPECS, expand_set_specs  # noqa: E402
from tools.deck.build_five_fixture_scale_decision import (  # noqa: E402
    ARCHETYPE_RANKING,
    M39_03_SMOKE,
    M42_03_SMOKE,
    M46_03_SMOKE,
    M50_03_SMOKE,
    M54_03_SMOKE,
    load_json,
)
from tools.deck.build_seventh_slice_fixture_readiness import (  # noqa: E402
    build_seventh_slice_fixture_readiness,
    write_json,
    write_markdown,
)
from tools.deck.build_seventh_target_slice_selection import build_seventh_target_slice_selection  # noqa: E402
from tools.deck.build_six_fixture_scale_decision import build_six_fixture_scale_decision  # noqa: E402
from tools.deck.build_sixth_headless_fixture_load_smoke import (  # noqa: E402
    HEADLESS_RULESET,
    HEADLESS_SEED,
    build_sixth_headless_fixture_load_smoke,
)
from tools.deck.build_sixth_slice_g_zone_stride_decision_artifact import (  # noqa: E402
    build_sixth_slice_g_zone_stride_decision_artifact,
)
from tools.deck.build_sixth_slice_human_accepted_repair_artifact import (  # noqa: E402
    M56_DRAFTS,
    M56_SCAFFOLD,
    build_sixth_slice_human_accepted_repair_artifact,
)
from tools.deck.build_sixth_slice_human_selected_recipe_artifact import (  # noqa: E402
    M57_01_REVIEW,
    build_sixth_slice_human_selected_recipe_artifact,
)
from tools.deck.build_sixth_slice_runtime_fixture_promotion_gate import (  # noqa: E402
    build_sixth_slice_runtime_fixture_promotion_gate,
)
from tools.deck.export_sixth_fixture_deck_text import build_sixth_fixture_deck_text_export  # noqa: E402
from tools.deck.validate_sixth_runtime_fixture_schema import (  # noqa: E402
    EXPECTED_G_ZONE_OPTION,
    build_sixth_runtime_fixture_schema_validation_report,
)
from tools.deck.validate_sixth_slice_repaired_recipe import (  # noqa: E402
    build_sixth_slice_repaired_validation_report,
)


SELECTED_REVIEW_ITEM_ID = "m57_01_m56_recipe_001_repair_review"


def _unity_result_file(tmp: Path, accepted: bool = True) -> Path:
    path = tmp / "unity_result.json"
    path.write_text(
        json.dumps(
            {
                "accepted": accepted,
                "seed": HEADLESS_SEED,
                "ruleset": HEADLESS_RULESET,
                "deck_source": "deck_code",
                "actions_executed": 4,
                "event_count": 4,
            }
        ),
        encoding="utf-8",
    )
    return path


def _build_in_memory_m59_01_selection() -> dict:
    review_packet = load_json(M57_01_REVIEW)
    selected_artifact = build_sixth_slice_human_selected_recipe_artifact(
        review_packet,
        selected_review_item_id=SELECTED_REVIEW_ITEM_ID,
        selection_text="explicit test selection",
        selected_by="unit-test",
        selected_at="2026-07-02",
    )
    accepted_artifact = build_sixth_slice_human_accepted_repair_artifact(
        selected_artifact,
        load_json(M56_DRAFTS),
        load_json(M56_SCAFFOLD),
        acceptance_text="explicit test acceptance",
        accepted_by="unit-test",
        accepted_at="2026-07-02",
    )
    g_zone_decision = build_sixth_slice_g_zone_stride_decision_artifact(
        accepted_artifact,
        selected_option=EXPECTED_G_ZONE_OPTION,
    )
    repaired_validation = build_sixth_slice_repaired_validation_report(
        accepted_artifact,
        g_zone_decision,
    )
    gate_report = build_sixth_slice_runtime_fixture_promotion_gate(
        accepted_artifact,
        repaired_validation,
    )
    sixth_fixture = gate_report["runtime_fixture"]
    m58_01_report = build_sixth_runtime_fixture_schema_validation_report(sixth_fixture)
    m58_02_report = build_sixth_fixture_deck_text_export(sixth_fixture, m58_01_report)
    with tempfile.TemporaryDirectory() as tmp:
        tmp_path = Path(tmp)
        unity_result = _unity_result_file(tmp_path)
        unity_replay = tmp_path / "unity_replay.json"
        unity_replay.write_text("{}", encoding="utf-8")
        sixth_smoke = build_sixth_headless_fixture_load_smoke(
            sixth_fixture,
            m58_02_report,
            m58_02_report["_deck_text"],
            unity_result,
            unity_replay,
        )

    m58_decision = build_six_fixture_scale_decision(
        load_json(M39_03_SMOKE),
        load_json(M42_03_SMOKE),
        load_json(M46_03_SMOKE),
        load_json(M50_03_SMOKE),
        load_json(M54_03_SMOKE),
        sixth_smoke,
        load_json(ARCHETYPE_RANKING),
        sixth_fixture,
    )
    return build_seventh_target_slice_selection(m58_decision)


class TestSeventhSliceFixtureReadiness(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.selection = _build_in_memory_m59_01_selection()
        cls.report = build_seventh_slice_fixture_readiness(cls.selection)

    def test_selected_slice_is_ready_for_semantic_probe(self):
        summary = self.report["summary"]
        selected = self.report["selected_target"]
        readiness = self.report["readiness"]

        self.assertEqual("M59-02", self.report["version"])
        self.assertEqual(self.selection["selected_target"]["group"], selected["group"])
        self.assertEqual(self.selection["selected_target"]["era_preset"], selected["era_preset"])
        self.assertTrue(readiness["selection_ready"])
        self.assertTrue(readiness["source_backed"])
        self.assertTrue(readiness["has_grade_setup"])
        self.assertTrue(readiness["has_required_g_unit_pool"])
        self.assertTrue(readiness["all_fixture_expectations_met"])
        self.assertTrue(readiness["semantic_probe_ready"])
        self.assertFalse(summary["repair_required"])
        self.assertTrue(summary["ready_for_m59_03"])

    def test_card_pool_summary_is_source_backed(self):
        pool = self.report["card_pool_summary"]

        self.assertGreater(pool["source_card_count"], 0)
        self.assertGreaterEqual(pool["trigger_capacity"], 16)
        self.assertGreaterEqual(pool["non_trigger_capacity"], 34)
        self.assertEqual([], pool["trigger_type_gaps"])
        for grade in range(4):
            self.assertGreater(pool["grade_counts"][str(grade)], 0)
        if self.report["format_scope"]["g_zone_fixture_boundary_required"]:
            self.assertGreater(pool["grade_counts"]["4"], 0)

    def test_selected_era_scope_is_applied(self):
        scope = self.report["format_scope"]
        era_preset = self.selection["selected_target"]["era_preset"]

        self.assertEqual(era_preset, scope["era_preset"])
        self.assertEqual(list(ERA_SET_SPECS[era_preset]), scope["set_specs"])
        self.assertEqual(expand_set_specs(ERA_SET_SPECS[era_preset]), scope["series_scope"])
        self.assertTrue(scope["new_format_or_mechanic_fixtures_required"])
        self.assertEqual("requires_seventh_slice_fixture_scaffold", scope["policy_reuse_decision"])

    def test_scope_does_not_create_runtime_assets(self):
        boundary = self.report["runtime_boundary"]
        readiness = self.report["readiness"]

        self.assertTrue(boundary["offline_fixture_readiness_only"])
        self.assertTrue(boundary["does_not_create_deck"])
        self.assertTrue(boundary["does_not_create_runtime_fixture"])
        self.assertTrue(boundary["does_not_mutate_runtime_pack"])
        self.assertTrue(boundary["does_not_publish_to_ui"])
        self.assertTrue(boundary["does_not_publish_to_bot"])
        self.assertFalse(boundary["g_zone_runtime_enabled"])
        self.assertFalse(boundary["stride_runtime_enabled"])
        self.assertFalse(boundary["GameState_mutation"])
        self.assertFalse(readiness["runtime_or_bot_promotion_allowed"])

    def test_unknown_era_routes_to_repair(self):
        selection = copy.deepcopy(self.selection)
        selection["selected_target"]["era_preset"] = "unknown_era"

        report = build_seventh_slice_fixture_readiness(selection)

        self.assertFalse(report["summary"]["ready_for_m59_03"])
        self.assertIn("era_scope_missing", report["summary"]["repair_reasons"])
        self.assertEqual("M59-repair", report["next_target"]["milestone"])

    def test_missing_group_routes_to_repair(self):
        selection = copy.deepcopy(self.selection)
        selection["selected_target"]["group"] = "Missing Clan"

        report = build_seventh_slice_fixture_readiness(selection)

        self.assertFalse(report["readiness"]["source_backed"])
        self.assertFalse(report["summary"]["ready_for_m59_03"])
        self.assertIn("source_pool_missing", report["summary"]["repair_reasons"])
        self.assertEqual("M59-repair", report["next_target"]["milestone"])

    def test_selection_not_ready_routes_to_repair(self):
        selection = copy.deepcopy(self.selection)
        selection["summary"]["ready_for_m59_02"] = False

        report = build_seventh_slice_fixture_readiness(selection)

        self.assertFalse(report["readiness"]["selection_ready"])
        self.assertFalse(report["summary"]["ready_for_m59_03"])
        self.assertIn("selection_not_ready", report["summary"]["repair_reasons"])
        self.assertEqual("M59-repair", report["next_target"]["milestone"])

    def test_next_target_is_m59_03(self):
        next_target = self.report["next_target"]

        self.assertEqual("M59-03", next_target["milestone"])
        self.assertEqual("Seventh-slice semantic/compatibility probe", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m59_02.json"
            md_path = out / "m59_02.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M59-02", loaded["version"])
            self.assertIn("M59-02 Seventh-Slice Fixture/Format Readiness", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
