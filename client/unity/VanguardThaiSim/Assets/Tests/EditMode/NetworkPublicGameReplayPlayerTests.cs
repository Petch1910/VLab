using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class NetworkPublicGameReplayPlayerTests
    {
        [Test]
        public void StepForwardAppliesPublicEventsIntoSpectatorState()
        {
            GameState initial = CreateTrueStateWithPrivateOpponentCards();
            NetworkPublicGameReplay replay = NetworkPublicGameReplay.Create(
                initial,
                new[]
                {
                    HiddenDraw("public-draw", 1),
                    PublicPhase("public-phase", 1, GamePhase.Main)
                },
                GameStateViewPerspective.Spectator);
            NetworkPublicGameReplayPlayer player = new NetworkPublicGameReplayPlayer(replay);

            Assert.AreEqual(45, player.CurrentStateView.GetPlayer(1).deck.Count);
            Assert.AreEqual(5, player.CurrentStateView.GetPlayer(1).hand.Count);
            Assert.IsTrue(player.StepForward());
            Assert.AreEqual(44, player.CurrentStateView.GetPlayer(1).deck.Count);
            Assert.AreEqual(6, player.CurrentStateView.GetPlayer(1).hand.Count);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, player.CurrentStateView.GetPlayer(1).hand[5].card_id);
            Assert.IsTrue(player.StepForward());
            Assert.AreEqual(GamePhase.Main, player.CurrentStateView.phase);
            Assert.AreEqual(0, player.CurrentStateView.event_log.Count);
            Assert.IsFalse(player.CurrentStateView.ToJson(false).Contains("REAL-OPPONENT"));
            Assert.AreEqual(2, player.CreateVisibleEventLog().Count);
        }

        [Test]
        public void ApplyBatchAppendsAndAppliesFromCurrentPublicCursor()
        {
            GameState initial = CreateTrueStateWithPrivateOpponentCards();
            NetworkPublicGameReplay replay = NetworkPublicGameReplay.Create(
                initial,
                null,
                GameStateViewPerspective.Spectator);
            NetworkPublicGameReplayPlayer player = new NetworkPublicGameReplayPlayer(replay);
            NetworkPublicEventBatch batch = new NetworkPublicEventBatch
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = "ROOM-SPECTATOR",
                from_event_index = 0
            };
            batch.events.Add(HiddenDraw("public-draw", 1));
            batch.events.Add(PublicPhase("public-phase", 1, GamePhase.Battle));
            string batchBefore = batch.ToJson(false);

            NetworkPublicReconnectApplyResult result = player.ApplyBatch(batch);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(2, result.applied_count);
            Assert.AreEqual(2, player.EventCount);
            Assert.AreEqual(2, player.CurrentIndex);
            Assert.AreEqual(44, player.CurrentStateView.GetPlayer(1).deck.Count);
            Assert.AreEqual(6, player.CurrentStateView.GetPlayer(1).hand.Count);
            Assert.AreEqual(GamePhase.Battle, player.CurrentStateView.phase);
            Assert.AreEqual(2, player.CreateVisibleEventLog().Count);
            Assert.AreEqual(batchBefore, batch.ToJson(false));
            Assert.IsFalse(replay.ToJson(false).Contains("REAL-OPPONENT"));
            Assert.IsFalse(player.CurrentStateView.ToJson(false).Contains("REAL-OPPONENT"));
        }

        [Test]
        public void ApplyBatchRejectsCursorMismatchWithoutMutatingReplayState()
        {
            GameState initial = CreateTrueStateWithPrivateOpponentCards();
            NetworkPublicGameReplay replay = NetworkPublicGameReplay.Create(
                initial,
                null,
                GameStateViewPerspective.Spectator);
            NetworkPublicGameReplayPlayer player = new NetworkPublicGameReplayPlayer(replay);
            string stateBefore = player.CreateCurrentStateSnapshot().ToJson(false);
            NetworkPublicEventBatch batch = new NetworkPublicEventBatch
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = "ROOM-SPECTATOR",
                from_event_index = 1
            };
            batch.events.Add(HiddenDraw("public-draw", 1));

            NetworkPublicReconnectApplyResult result = player.ApplyBatch(batch);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual("PUBLIC_REPLAY_CURSOR_MISMATCH", result.rejection_reason);
            Assert.AreEqual(0, player.EventCount);
            Assert.AreEqual(0, player.CurrentIndex);
            Assert.AreEqual(stateBefore, player.CreateCurrentStateSnapshot().ToJson(false));
        }

        [Test]
        public void ApplyBatchRejectsInvalidEventWithoutMutatingReplayState()
        {
            GameState initial = CreateTrueStateWithPrivateOpponentCards();
            NetworkPublicGameReplay replay = NetworkPublicGameReplay.Create(
                initial,
                null,
                GameStateViewPerspective.Spectator);
            NetworkPublicGameReplayPlayer player = new NetworkPublicGameReplayPlayer(replay);
            string stateBefore = player.CreateCurrentStateSnapshot().ToJson(false);
            NetworkPublicEventBatch batch = new NetworkPublicEventBatch
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = "ROOM-SPECTATOR",
                from_event_index = 0
            };
            batch.events.Add(HiddenDraw("public-invalid", 9));

            NetworkPublicReconnectApplyResult result = player.ApplyBatch(batch);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual("ACTOR_INDEX_OUT_OF_RANGE", result.rejection_reason);
            Assert.AreEqual(0, player.EventCount);
            Assert.AreEqual(0, player.CurrentIndex);
            Assert.AreEqual(stateBefore, player.CreateCurrentStateSnapshot().ToJson(false));
        }

        [Test]
        public void VisibleEventLogReturnsClonedPublicEvents()
        {
            NetworkPublicGameEvent publicEvent = PublicPhase("public-phase", 1, GamePhase.Main);
            NetworkPublicGameReplay replay = NetworkPublicGameReplay.Create(
                CreateTrueStateWithPrivateOpponentCards(),
                new[] { publicEvent },
                GameStateViewPerspective.Spectator);
            publicEvent.event_id = "mutated-source";
            NetworkPublicGameReplayPlayer player = new NetworkPublicGameReplayPlayer(replay);
            player.StepForward();

            NetworkPublicGameEvent visibleEvent = player.CreateVisibleEventLog()[0];
            visibleEvent.event_id = "mutated-visible";

            Assert.AreEqual("public-phase", player.CreateVisibleEventLog()[0].event_id);
        }

        private static NetworkPublicGameEvent HiddenDraw(string eventId, int actorIndex)
        {
            return new NetworkPublicGameEvent
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                event_id = eventId,
                source_event_id = "true-" + eventId,
                action_type = GameActionType.Draw,
                actor_index = actorIndex,
                from_zone = GameZone.Deck,
                to_zone = GameZone.Hand,
                from_zone_count_delta = -1,
                to_zone_count_delta = 1,
                hides_card_identity = true
            };
        }

        private static NetworkPublicGameEvent PublicPhase(string eventId, int actorIndex, GamePhase phase)
        {
            return new NetworkPublicGameEvent
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                event_id = eventId,
                source_event_id = "true-" + eventId,
                action_type = GameActionType.SetPhase,
                actor_index = actorIndex,
                new_phase = phase
            };
        }

        private static GameState CreateTrueStateWithPrivateOpponentCards()
        {
            GameState state = new GameState
            {
                game_id = "spectator-replay-state",
                format = "D",
                random_seed = 1,
                turn_number = 1,
                turn_player_index = 0,
                phase = GamePhase.Mulligan
            };
            PlayerGameState local = new PlayerGameState { player_id = "p1" };
            PlayerGameState opponent = new PlayerGameState { player_id = "p2" };
            for (int i = 0; i < 45; i++)
            {
                opponent.deck.Add(new GameCardInstance(
                    "REAL-OPPONENT-DECK-" + i,
                    "REAL-OPPONENT-CARD-" + i,
                    1,
                    false));
            }

            for (int i = 0; i < 5; i++)
            {
                opponent.hand.Add(new GameCardInstance(
                    "REAL-OPPONENT-HAND-" + i,
                    "REAL-OPPONENT-HAND-CARD-" + i,
                    1,
                    false));
            }

            state.players.Add(local);
            state.players.Add(opponent);
            return state;
        }
    }
}
