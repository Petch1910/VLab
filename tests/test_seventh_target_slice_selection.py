"""Tests for tools/deck/build_seventh_target_slice_selection.py."""

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

from tools.deck.build_five_fixture_scale_decision import (  # noqa: E402
    ARCHETYPE_RANKING,
    M39_03_SMOKE,
    M42_03_SMOKE,
    M46_03_SMOKE,
    M50_03_SMOKE,
    M54_03_SMOKE,
    load_json,
)
from tools.deck.build_seventh_target_slice_selection import (  # noqa: E402
    build_seventh_target_slice_selection,
    write_json,
    write_markdown,
)
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


class TestSeventhTargetSliceSelection(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
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

        cls.m58_decision = build_six_fixture_scale_decision(
            load_json(M39_03_SMOKE),
            load_json(M42_03_SMOKE),
            load_json(M46_03_SMOKE),
            load_json(M50_03_SMOKE),
            load_json(M54_03_SMOKE),
            sixth_smoke,
            load_json(ARCHETYPE_RANKING),
            sixth_fixture,
        )
        cls.report = build_seventh_target_slice_selection(cls.m58_decision)

    def test_selects_first_available_seventh_slice_candidate(self):
        summary = self.report["summary"]
        selected = self.report["selected_target"]
        first_candidate = self.m58_decision["candidate_queue"][0]

        self.assertEqual("M59-01", self.report["version"])
        self.assertTrue(summary["seventh_slice_selected"])
        self.assertEqual(first_candidate["group"], summary["selected_group"])
        self.assertEqual(first_candidate["best_era"], summary["selected_era_preset"])
        self.assertEqual(first_candidate["rank"], summary["selected_rank"])
        self.assertEqual(first_candidate["priority_score"], selected["priority_score"])
        self.assertTrue(summary["ready_for_m59_02"])

    def test_selection_is_offline_only_and_non_mutating(self):
        scope = self.report["scope"]
        decision = self.report["decision"]

        self.assertTrue(scope["offline_target_selection"])
        self.assertFalse(scope["creates_recipe_draft"])
        self.assertFalse(scope["creates_runtime_fixture"])
        self.assertFalse(scope["saved_deck_injection"])
        self.assertFalse(scope["ui_deck_list_publication"])
        self.assertFalse(scope["bot_playbook"])
        self.assertFalse(scope["g_zone_runtime_enabled"])
        self.assertFalse(scope["stride_runtime_enabled"])
        self.assertFalse(scope["GameState_mutation"])
        self.assertTrue(decision["offline_analysis_only"])
        self.assertFalse(decision["live_runtime_deck_enabled"])
        self.assertFalse(decision["saved_deck_enabled"])
        self.assertFalse(decision["ui_deck_list_enabled"])
        self.assertFalse(decision["bot_playbook_enabled"])
        self.assertFalse(decision["g_zone_runtime_enabled"])
        self.assertFalse(decision["stride_runtime_enabled"])

    def test_selection_uses_candidate_queue_snapshot(self):
        self.assertEqual(self.m58_decision["candidate_queue"], self.report["candidate_queue_snapshot"])
        self.assertGreaterEqual(self.report["summary"]["candidate_count"], 1)

    def test_not_allowed_routes_to_m58_repair(self):
        m58_decision = copy.deepcopy(self.m58_decision)
        m58_decision["summary"]["ready_for_m59"] = False

        report = build_seventh_target_slice_selection(m58_decision)

        self.assertFalse(report["summary"]["seventh_slice_selected"])
        self.assertFalse(report["summary"]["ready_for_m59_02"])
        self.assertEqual("M58-repair", report["next_target"]["milestone"])

    def test_empty_candidate_queue_routes_to_m58_repair(self):
        m58_decision = copy.deepcopy(self.m58_decision)
        m58_decision["candidate_queue"] = []

        report = build_seventh_target_slice_selection(m58_decision)

        self.assertFalse(report["summary"]["seventh_slice_selected"])
        self.assertEqual("M58-repair", report["next_target"]["milestone"])

    def test_next_target_is_m59_02(self):
        next_target = self.report["next_target"]

        self.assertEqual("M59-02", next_target["milestone"])
        self.assertEqual("Seventh-slice fixture/format readiness", next_target["task"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "m59_01.json"
            md_path = out / "m59_01.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M59-01", loaded["version"])
            self.assertIn("M59-01 Seventh Target Slice Selection", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
