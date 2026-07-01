using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class CombatModifierLedgerTests
    {
        [Test]
        public void SummaryAddsPowerAndCriticalDeltasIndependently()
        {
            CombatModifierLedger ledger = CreateLedger();

            CombatModifierSummary summary = ledger.Summarize("unit-a");

            Assert.AreEqual("unit-a", summary.target_card_instance_id);
            Assert.AreEqual(15000, summary.total_power_delta);
            Assert.AreEqual(1, summary.total_critical_delta);
            Assert.AreEqual(2, summary.modifier_count);
        }

        [Test]
        public void FilteringExpiredTimingReturnsNewLedgerAndLeavesOriginalIntact()
        {
            CombatModifierLedger ledger = CreateLedger();

            CombatModifierLedger filtered = ledger.WithoutExpired(CombatModifierExpiration.EndOfTurn);

            Assert.AreEqual(3, ledger.modifiers.Count);
            Assert.AreEqual(2, filtered.modifiers.Count);
            CombatModifierSummary original = ledger.Summarize("unit-a");
            CombatModifierSummary afterFilter = filtered.Summarize("unit-a");
            Assert.AreEqual(15000, original.total_power_delta);
            Assert.AreEqual(1, original.total_critical_delta);
            Assert.AreEqual(10000, afterFilter.total_power_delta);
            Assert.AreEqual(0, afterFilter.total_critical_delta);
        }

        [Test]
        public void ListForTargetReturnsClones()
        {
            CombatModifierLedger ledger = CreateLedger();

            List<CombatModifier> modifiers = ledger.ListForTarget("unit-a");
            modifiers[0].power_delta = 999999;

            Assert.AreEqual(15000, ledger.Summarize("unit-a").total_power_delta);
        }

        [Test]
        public void LedgerJsonRoundTrips()
        {
            CombatModifierLedger ledger = CreateLedger();

            CombatModifierLedger roundTrip = CombatModifierLedger.FromJson(ledger.ToJson());

            Assert.AreEqual(ledger.modifiers.Count, roundTrip.modifiers.Count);
            Assert.AreEqual("mod-power", roundTrip.modifiers[0].modifier_id);
            Assert.AreEqual(CombatModifierExpiration.EndOfBattle, roundTrip.modifiers[0].expires_at);
            Assert.AreEqual(15000, roundTrip.Summarize("unit-a").total_power_delta);
        }

        [Test]
        public void LedgerIsDeterministicAndDoesNotMutateState()
        {
            GameState state = CreateState();
            string before = state.ToJson();
            CombatModifierLedger ledger = CreateLedger();

            CombatModifierSummary first = ledger.Summarize("unit-a");
            CombatModifierSummary second = ledger.Summarize("unit-a");
            string after = state.ToJson();

            Assert.AreEqual(before, after);
            Assert.AreEqual(first.total_power_delta, second.total_power_delta);
            Assert.AreEqual(first.total_critical_delta, second.total_critical_delta);
            Assert.AreEqual(first.modifier_count, second.modifier_count);
        }

        private static CombatModifierLedger CreateLedger()
        {
            var ledger = new CombatModifierLedger();
            ledger.Add(new CombatModifier
            {
                modifier_id = "mod-power",
                source_id = "trigger-critical",
                target_card_instance_id = "unit-a",
                power_delta = 10000,
                critical_delta = 0,
                expires_at = CombatModifierExpiration.EndOfBattle,
                note = "critical trigger power"
            });
            ledger.Add(new CombatModifier
            {
                modifier_id = "mod-critical",
                source_id = "trigger-critical",
                target_card_instance_id = "unit-a",
                power_delta = 5000,
                critical_delta = 1,
                expires_at = CombatModifierExpiration.EndOfTurn,
                note = "critical trigger critical"
            });
            ledger.Add(new CombatModifier
            {
                modifier_id = "mod-other",
                source_id = "ability",
                target_card_instance_id = "unit-b",
                power_delta = 3000,
                critical_delta = 0,
                expires_at = CombatModifierExpiration.Permanent,
                note = "other unit"
            });
            return ledger;
        }

        private static GameState CreateState()
        {
            return new GameState
            {
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        vanguard = new List<GameCardInstance>
                        {
                            new GameCardInstance("unit-a", "VG", 0)
                        }
                    }
                }
            };
        }
    }
}
