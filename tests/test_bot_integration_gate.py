"""Tests for tools/deck/build_bot_integration_gate.py (M35-E4)."""

from __future__ import annotations

import json
import sys
import tempfile
import unittest
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
if str(ROOT) not in sys.path:
    sys.path.insert(0, str(ROOT))

from tools.deck.build_bot_integration_gate import (  # noqa: E402
    build_gate,
    load_json,
    D4_REVIEWED_SEED,
    E3_PROBE,
    write_json,
    write_markdown,
)


class TestBotIntegrationGate(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.d4_report = load_json(D4_REVIEWED_SEED)
        cls.e3_report = load_json(E3_PROBE)
        cls.report = build_gate(cls.d4_report, cls.e3_report)

    def test_gate_passes_but_runtime_integration_stays_disabled(self):
        readiness = self.report["readiness"]

        self.assertEqual("M35-E4", self.report["version"])
        self.assertTrue(readiness["gate_passed"])
        self.assertFalse(readiness["runtime_bot_integration_enabled"])
        self.assertTrue(readiness["ready_for_future_runtime_implementation_after_tests"])

    def test_reviewed_seed_entries_are_only_future_hint_candidates(self):
        seed_count = self.d4_report["summary"]["seed_entry_count"]
        candidates = self.report["allowed_hint_candidates"]

        self.assertEqual(seed_count, len(candidates))
        self.assertGreater(len(candidates), 0)
        for candidate in candidates:
            self.assertTrue(candidate["allowed_as_future_bot_hint_candidate"])
            self.assertTrue(candidate["checks"]["human_acceptance_required_before_runtime"])
            self.assertTrue(candidate["checks"]["legal_action_mask_required_before_future_bot_use"])
            self.assertTrue(candidate["checks"]["masked_state_required_before_future_bot_use"])
            self.assertIn("RulesCore_command_validation", candidate["required_before_runtime_use"])

    def test_e3_probe_is_blocked_from_runtime_bot(self):
        blocked = self.report["blocked_sources"][0]

        self.assertEqual("M35-E3 semantic/compatibility probe", blocked["source"])
        self.assertTrue(blocked["blocked_from_runtime_bot"])
        self.assertTrue(blocked["source_declares_no_runtime_or_bot_promotion"])
        self.assertEqual(
            self.e3_report["stage_summaries"]["c5_compatibility"]["edge_count"],
            blocked["edge_count"],
        )

    def test_gate_policy_requires_legal_action_and_masked_state(self):
        policy = self.report["gate_policy"]

        self.assertTrue(policy["bot_may_consume_only_reviewed_hint_candidates"])
        self.assertTrue(policy["legal_action_mask_required"])
        self.assertTrue(policy["masked_state_view_required"])
        self.assertTrue(policy["RulesCore_command_validation_required"])
        self.assertTrue(policy["direct_GameState_mutation_forbidden"])
        self.assertTrue(policy["true_hidden_state_access_forbidden"])
        self.assertTrue(policy["live_card_text_parsing_forbidden"])
        self.assertTrue(policy["unreviewed_e3_probe_edges_forbidden"])

    def test_evidence_files_exist(self):
        checks = self.report["evidence_checks"]

        self.assertTrue(checks)
        for check in checks:
            self.assertTrue(check["exists"], check["path"])

    def test_next_target_is_closeout_not_runtime_wiring(self):
        next_target = self.report["next_target"]

        self.assertEqual("M35-closeout", next_target["milestone"])
        self.assertIn("no runtime bot wiring until a Unity/C# implementation milestone is opened", next_target["must_create"])

    def test_output_round_trip(self):
        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp)
            json_path = out / "gate.json"
            md_path = out / "gate.md"

            write_json(self.report, json_path)
            write_markdown(self.report, md_path)

            loaded = json.loads(json_path.read_text(encoding="utf-8"))
            self.assertEqual("M35-E4", loaded["version"])
            self.assertIn("M35-E4 Bot Integration Gate", md_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
