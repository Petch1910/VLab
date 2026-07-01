using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityGameStateIntegrationTests
    {
        [Test]
        public void GameStateEnsureListsInitializesMissingPendingQueue()
        {
            var state = new GameState
            {
                pending_auto_abilities = null
            };

            state.EnsureLists();

            Assert.NotNull(state.pending_auto_abilities);
            Assert.NotNull(state.pending_auto_abilities.pending);
            Assert.AreEqual(0, state.pending_auto_abilities.pending.Count);
        }

        [Test]
        public void GameStateFromJsonInitializesMissingPendingQueue()
        {
            GameState state = GameState.FromJson("{\"game_id\":\"game-1\"}");

            Assert.NotNull(state.pending_auto_abilities);
            Assert.NotNull(state.pending_auto_abilities.pending);
        }

        [Test]
        public void GameStateRoundTripPreservesPendingQueue()
        {
            var state = new GameState
            {
                game_id = "game-2",
                pending_auto_abilities = PendingAutoAbilityQueueBuilder.Enqueue(
                    new PendingAutoAbilityQueue { queue_id = "queue-1" },
                    new PendingAutoAbility
                    {
                        pending_id = "pending-1",
                        source_card_instance_id = "unit-1",
                        source_card_id = "CARD-001",
                        player_index = 0,
                        timing_event = "OnAttack",
                        summary = "CARD-001 on attack"
                    })
            };

            GameState roundTrip = GameState.FromJson(state.ToJson());

            Assert.AreEqual("queue-1", roundTrip.pending_auto_abilities.queue_id);
            Assert.AreEqual(1, roundTrip.pending_auto_abilities.pending.Count);
            Assert.AreEqual("pending-1", roundTrip.pending_auto_abilities.pending[0].pending_id);
            Assert.AreEqual("unit-1", roundTrip.pending_auto_abilities.pending[0].source_card_instance_id);
            Assert.AreEqual("CARD-001", roundTrip.pending_auto_abilities.pending[0].source_card_id);
            Assert.AreEqual(0, roundTrip.pending_auto_abilities.pending[0].player_index);
            Assert.AreEqual("OnAttack", roundTrip.pending_auto_abilities.pending[0].timing_event);
            Assert.AreEqual("CARD-001 on attack", roundTrip.pending_auto_abilities.pending[0].summary);
        }
    }
}
