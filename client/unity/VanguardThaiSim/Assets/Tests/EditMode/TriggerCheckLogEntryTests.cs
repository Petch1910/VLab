using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckLogEntryTests
    {
        [Test]
        public void CriticalBundleCreatesLogEntryWithModifierIds()
        {
            TriggerCheckResolutionBundle bundle = CreateBundle(TriggerType.Critical);

            TriggerCheckLogEntry entry = TriggerCheckLogEntryFactory.FromBundle(bundle);

            Assert.AreEqual("trigger-log|Drive|0|0|drive-card-1|CRIT-001|Critical", entry.log_entry_id);
            Assert.IsTrue(entry.accepted);
            Assert.AreEqual(2, entry.modifier_count);
            Assert.AreEqual(2, entry.modifier_ids.Count);
            Assert.IsTrue(entry.modifier_ids[0].StartsWith("trigger-check|Drive|0|drive-card-1|CRIT-001"));
            Assert.IsTrue(entry.summary.Contains("modifiers=2"));
        }

        [Test]
        public void NoTriggerBundleCreatesLogEntryWithoutModifiers()
        {
            TriggerCheckResolutionBundle bundle = TriggerCheckResolutionBundler.Build(
                CreateBoardState(),
                0,
                TriggerCheckSource.Damage,
                1,
                "damage-card-1",
                "NORMAL-001",
                TriggerType.None,
                CombatModifierExpiration.EndOfBattle);

            TriggerCheckLogEntry entry = TriggerCheckLogEntryFactory.FromBundle(bundle);

            Assert.IsTrue(entry.accepted);
            Assert.AreEqual(0, entry.modifier_count);
            Assert.AreEqual(0, entry.modifier_ids.Count);
            Assert.AreEqual(TriggerCheckSource.Damage, entry.check_source);
        }

        [Test]
        public void LogEntryJsonRoundTrips()
        {
            TriggerCheckLogEntry entry = TriggerCheckLogEntryFactory.FromBundle(CreateBundle(TriggerType.Critical));

            TriggerCheckLogEntry roundTrip = TriggerCheckLogEntry.FromJson(entry.ToJson());

            Assert.AreEqual(entry.log_entry_id, roundTrip.log_entry_id);
            Assert.AreEqual(entry.trigger_type, roundTrip.trigger_type);
            Assert.AreEqual(entry.modifier_count, roundTrip.modifier_count);
            Assert.AreEqual(entry.modifier_ids[0], roundTrip.modifier_ids[0]);
            Assert.AreEqual(entry.summary, roundTrip.summary);
        }

        [Test]
        public void LogEntryCreationIsDeterministic()
        {
            TriggerCheckResolutionBundle bundle = CreateBundle(TriggerType.Critical);

            TriggerCheckLogEntry first = TriggerCheckLogEntryFactory.FromBundle(bundle);
            TriggerCheckLogEntry second = TriggerCheckLogEntryFactory.FromBundle(bundle);

            Assert.AreEqual(first.ToJson(), second.ToJson());
        }

        [Test]
        public void BundleAndLogEntryDoNotMutateGameState()
        {
            GameState state = CreateBoardState();
            string before = state.ToJson();

            TriggerCheckResolutionBundle bundle = TriggerCheckResolutionBundler.Build(
                state,
                0,
                TriggerCheckSource.Drive,
                0,
                "drive-card-1",
                "CRIT-001",
                TriggerType.Critical,
                CombatModifierExpiration.EndOfTurn);
            TriggerCheckLogEntryFactory.FromBundle(bundle);

            Assert.AreEqual(before, state.ToJson());
        }

        private static TriggerCheckResolutionBundle CreateBundle(TriggerType triggerType)
        {
            return TriggerCheckResolutionBundler.Build(
                CreateBoardState(),
                0,
                TriggerCheckSource.Drive,
                0,
                "drive-card-1",
                "CRIT-001",
                triggerType,
                CombatModifierExpiration.EndOfTurn);
        }

        private static GameState CreateBoardState()
        {
            var vanguard = new GameCardInstance("vg", "VG-001", 0);
            var highRearGuard = new GameCardInstance("rg-high", "RG-HIGH", 0);
            highRearGuard.power_delta = 5000;
            var lowRearGuard = new GameCardInstance("rg-low", "RG-LOW", 0);

            return new GameState
            {
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        vanguard = new List<GameCardInstance> { vanguard },
                        rear_guard = new List<GameCardInstance> { highRearGuard, lowRearGuard }
                    }
                }
            };
        }
    }
}
