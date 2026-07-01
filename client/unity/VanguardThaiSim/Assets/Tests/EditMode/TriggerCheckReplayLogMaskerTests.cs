using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckReplayLogMaskerTests
    {
        [Test]
        public void TrueStateViewPreservesCheckedCardIdentity()
        {
            TriggerCheckReplayLog log = CreateTwoPlayerLog();

            TriggerCheckReplayLog view = TriggerCheckReplayLogMasker.CreateView(
                log,
                GameStateViewPerspective.TrueState);

            Assert.AreEqual("CRIT-001", view.entries[0].checked_card_id);
            Assert.AreEqual("HEAL-001", view.entries[1].checked_card_id);
            Assert.IsFalse(view.entries[0].hides_checked_card_identity);
        }

        [Test]
        public void OwnerPlayerViewPreservesOwnEntryAndMasksOpponentEntry()
        {
            TriggerCheckReplayLog log = CreateTwoPlayerLog();

            TriggerCheckReplayLog view = TriggerCheckReplayLogMasker.CreateView(
                log,
                GameStateViewPerspective.Player,
                0);

            Assert.AreEqual("CRIT-001", view.entries[0].checked_card_id);
            Assert.IsFalse(view.entries[0].hides_checked_card_identity);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, view.entries[1].checked_card_id);
            Assert.AreEqual("hidden-trigger-check-0001", view.entries[1].checked_card_instance_id);
            Assert.IsTrue(view.entries[1].hides_checked_card_identity);
            Assert.IsFalse(view.entries[1].log_entry_id.Contains("HEAL-001"));
        }

        [Test]
        public void SpectatorViewMasksIdentityLogIdsSummariesAndModifierIds()
        {
            TriggerCheckReplayLog log = CreateTwoPlayerLog();

            TriggerCheckReplayLog view = TriggerCheckReplayLogMasker.CreateView(
                log,
                GameStateViewPerspective.Spectator);

            Assert.AreEqual(GameStateViewFactory.HiddenCardId, view.entries[0].checked_card_id);
            Assert.AreEqual("hidden-trigger-check-0000", view.entries[0].checked_card_instance_id);
            Assert.IsTrue(view.entries[0].hides_checked_card_identity);
            Assert.IsTrue(view.entries[0].log_entry_id.StartsWith("trigger-log-hidden|Drive|0|0|0000|Critical"));
            Assert.IsFalse(view.entries[0].log_entry_id.Contains("CRIT-001"));
            Assert.IsFalse(view.entries[0].summary.Contains("CRIT-001"));
            Assert.AreEqual("hidden-trigger-modifier-0000-0000", view.entries[0].modifier_ids[0]);
        }

        [Test]
        public void MaskedLogJsonRoundTrips()
        {
            TriggerCheckReplayLog view = TriggerCheckReplayLogMasker.CreateView(
                CreateTwoPlayerLog(),
                GameStateViewPerspective.Spectator);

            TriggerCheckReplayLog roundTrip = TriggerCheckReplayLog.FromJson(view.ToJson());

            Assert.AreEqual(view.entries[0].log_entry_id, roundTrip.entries[0].log_entry_id);
            Assert.AreEqual(view.entries[0].checked_card_id, roundTrip.entries[0].checked_card_id);
            Assert.AreEqual(view.entries[0].modifier_ids[0], roundTrip.entries[0].modifier_ids[0]);
            Assert.IsTrue(roundTrip.entries[0].hides_checked_card_identity);
        }

        [Test]
        public void MaskingIsDeterministicAndDoesNotMutateSource()
        {
            TriggerCheckReplayLog log = CreateTwoPlayerLog();
            string before = log.ToJson();

            TriggerCheckReplayLog first = TriggerCheckReplayLogMasker.CreateView(
                log,
                GameStateViewPerspective.Spectator);
            TriggerCheckReplayLog second = TriggerCheckReplayLogMasker.CreateView(
                log,
                GameStateViewPerspective.Spectator);

            Assert.AreEqual(first.ToJson(), second.ToJson());
            Assert.AreEqual(before, log.ToJson());
        }

        [Test]
        public void DerivingAndMaskingDoesNotMutateGameState()
        {
            GameState state = CreateTwoPlayerState();
            string before = state.ToJson();
            TriggerCheckReplayLog log = TriggerCheckReplayLogBuilder.Append(
                null,
                CreateEntry(state, 0, 0, "drive-card-p0", "CRIT-001", TriggerType.Critical));

            TriggerCheckReplayLogMasker.CreateView(log, GameStateViewPerspective.Spectator);

            Assert.AreEqual(before, state.ToJson());
        }

        private static TriggerCheckReplayLog CreateTwoPlayerLog()
        {
            GameState state = CreateTwoPlayerState();
            TriggerCheckReplayLog log = TriggerCheckReplayLogBuilder.Append(
                null,
                CreateEntry(state, 0, 0, "drive-card-p0", "CRIT-001", TriggerType.Critical));
            return TriggerCheckReplayLogBuilder.Append(
                log,
                CreateEntry(state, 1, 0, "drive-card-p1", "HEAL-001", TriggerType.Heal));
        }

        private static TriggerCheckLogEntry CreateEntry(
            GameState state,
            int playerIndex,
            int checkIndex,
            string checkedCardInstanceId,
            string checkedCardId,
            TriggerType triggerType)
        {
            TriggerCheckResolutionBundle bundle = TriggerCheckResolutionBundler.Build(
                state,
                playerIndex,
                TriggerCheckSource.Drive,
                checkIndex,
                checkedCardInstanceId,
                checkedCardId,
                triggerType,
                CombatModifierExpiration.EndOfTurn);

            return TriggerCheckLogEntryFactory.FromBundle(bundle);
        }

        private static GameState CreateTwoPlayerState()
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
                            new GameCardInstance("vg-p0", "VG-P0", 0)
                        },
                        rear_guard = new List<GameCardInstance>
                        {
                            new GameCardInstance("rg-p0", "RG-P0", 0)
                        }
                    },
                    new PlayerGameState
                    {
                        player_id = "p2",
                        vanguard = new List<GameCardInstance>
                        {
                            new GameCardInstance("vg-p1", "VG-P1", 1)
                        },
                        rear_guard = new List<GameCardInstance>
                        {
                            new GameCardInstance("rg-p1", "RG-P1", 1)
                        }
                    }
                }
            };
        }
    }
}
