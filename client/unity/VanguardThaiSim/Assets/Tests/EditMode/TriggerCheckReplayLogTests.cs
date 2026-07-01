using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckReplayLogTests
    {
        [Test]
        public void AppendPreservesOrderAndStableIds()
        {
            TriggerCheckLogEntry first = CreateEntry(0, "CRIT-001", TriggerType.Critical);
            TriggerCheckLogEntry second = CreateEntry(1, "NORMAL-001", TriggerType.None);

            TriggerCheckReplayLog log = TriggerCheckReplayLogBuilder.Append(null, first);
            log = TriggerCheckReplayLogBuilder.Append(log, second);

            Assert.AreEqual(2, log.entries.Count);
            Assert.AreEqual(first.log_entry_id, log.entries[0].log_entry_id);
            Assert.AreEqual(second.log_entry_id, log.entries[1].log_entry_id);
        }

        [Test]
        public void ReplayLogJsonRoundTrips()
        {
            TriggerCheckReplayLog log = TriggerCheckReplayLogBuilder.Append(
                null,
                CreateEntry(0, "CRIT-001", TriggerType.Critical));

            TriggerCheckReplayLog roundTrip = TriggerCheckReplayLog.FromJson(log.ToJson());

            Assert.AreEqual(log.log_id, roundTrip.log_id);
            Assert.AreEqual(1, roundTrip.entries.Count);
            Assert.AreEqual(log.entries[0].log_entry_id, roundTrip.entries[0].log_entry_id);
            Assert.AreEqual(log.entries[0].modifier_count, roundTrip.entries[0].modifier_count);
        }

        [Test]
        public void VisiblePrefixReturnsClonedSubsetAndDoesNotMutateSource()
        {
            TriggerCheckReplayLog log = TriggerCheckReplayLogBuilder.Append(
                null,
                CreateEntry(0, "CRIT-001", TriggerType.Critical));
            log = TriggerCheckReplayLogBuilder.Append(
                log,
                CreateEntry(1, "NORMAL-001", TriggerType.None));

            TriggerCheckReplayLog prefix = TriggerCheckReplayLogBuilder.VisiblePrefix(log, 1);
            prefix.entries[0].summary = "changed";

            Assert.AreEqual(2, log.entries.Count);
            Assert.AreEqual(1, prefix.entries.Count);
            Assert.AreEqual("trigger-log|Drive|0|0|drive-card-0|CRIT-001|Critical", prefix.entries[0].log_entry_id);
            Assert.AreNotEqual("changed", log.entries[0].summary);
        }

        [Test]
        public void AppendSequenceIsDeterministic()
        {
            TriggerCheckLogEntry first = CreateEntry(0, "CRIT-001", TriggerType.Critical);
            TriggerCheckLogEntry second = CreateEntry(1, "NORMAL-001", TriggerType.None);

            TriggerCheckReplayLog a = TriggerCheckReplayLogBuilder.Append(null, first);
            a = TriggerCheckReplayLogBuilder.Append(a, second);
            TriggerCheckReplayLog b = TriggerCheckReplayLogBuilder.Append(null, first);
            b = TriggerCheckReplayLogBuilder.Append(b, second);

            Assert.AreEqual(a.ToJson(), b.ToJson());
        }

        [Test]
        public void DerivingEntriesAndAppendingDoesNotMutateGameState()
        {
            GameState state = CreateBoardState();
            string before = state.ToJson();

            TriggerCheckLogEntry entry = TriggerCheckLogEntryFactory.FromBundle(
                TriggerCheckResolutionBundler.Build(
                    state,
                    0,
                    TriggerCheckSource.Drive,
                    0,
                    "drive-card-0",
                    "CRIT-001",
                    TriggerType.Critical,
                    CombatModifierExpiration.EndOfTurn));
            TriggerCheckReplayLogBuilder.Append(null, entry);

            Assert.AreEqual(before, state.ToJson());
        }

        private static TriggerCheckLogEntry CreateEntry(int checkIndex, string cardId, TriggerType triggerType)
        {
            TriggerCheckResolutionBundle bundle = TriggerCheckResolutionBundler.Build(
                CreateBoardState(),
                0,
                TriggerCheckSource.Drive,
                checkIndex,
                "drive-card-" + checkIndex,
                cardId,
                triggerType,
                CombatModifierExpiration.EndOfTurn);

            return TriggerCheckLogEntryFactory.FromBundle(bundle);
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
