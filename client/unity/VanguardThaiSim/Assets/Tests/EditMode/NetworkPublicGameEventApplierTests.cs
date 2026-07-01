using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class NetworkPublicGameEventApplierTests
    {
        [Test]
        public void HiddenDrawUpdatesCountsWithoutCardIdentityLeak()
        {
            GameState view = CreatePublicView();
            NetworkPublicGameEvent publicEvent = new NetworkPublicGameEvent
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                event_id = "public-draw",
                action_type = GameActionType.Draw,
                actor_index = 1,
                from_zone = GameZone.Deck,
                to_zone = GameZone.Hand,
                from_zone_count_delta = -1,
                to_zone_count_delta = 1,
                hides_card_identity = true
            };

            NetworkPublicGameEventApplyResult result =
                NetworkPublicGameEventApplier.ApplyToPublicView(view, publicEvent);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(44, view.GetPlayer(1).deck.Count);
            Assert.AreEqual(6, view.GetPlayer(1).hand.Count);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, view.GetPlayer(1).hand[5].card_id);
            Assert.AreEqual(0, view.event_log.Count);
            Assert.IsFalse(view.ToJson(false).Contains("REAL-OPPONENT"));
        }

        [Test]
        public void PrivateToPublicRevealAddsOnlyPublicIdentity()
        {
            GameState view = CreatePublicView();
            NetworkPublicGameEvent publicEvent = new NetworkPublicGameEvent
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                event_id = "public-call",
                action_type = GameActionType.MoveCard,
                actor_index = 1,
                from_zone = GameZone.Hand,
                to_zone = GameZone.RearGuard,
                from_zone_count_delta = -1,
                to_zone_count_delta = 1,
                hides_card_identity = false,
                public_card_id = "BT01-001TH",
                public_card_instance_id = "public-call-card"
            };

            NetworkPublicGameEventApplyResult result =
                NetworkPublicGameEventApplier.ApplyToPublicView(view, publicEvent);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(4, view.GetPlayer(1).hand.Count);
            Assert.AreEqual(1, view.GetPlayer(1).rear_guard.Count);
            Assert.AreEqual("BT01-001TH", view.GetPlayer(1).rear_guard[0].card_id);
            Assert.AreEqual("public-call-card", view.GetPlayer(1).rear_guard[0].instance_id);
            Assert.IsTrue(view.GetPlayer(1).rear_guard[0].face_up);
        }

        [Test]
        public void ApplyToSessionStoresPublicLogAndAdvancesCursorOnly()
        {
            LocalOwnerPrivateSession session = new LocalOwnerPrivateSession
            {
                room_id = "ROOM-PUBLIC",
                local_player_id = "p1",
                local_player_index = 0,
                local_true_state = CreatePublicView(),
                opponent_public_view = CreatePublicView()
            };
            string trueStateBefore = session.local_true_state.ToJson(false);
            NetworkPublicGameEvent publicEvent = new NetworkPublicGameEvent
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                event_id = "public-phase",
                action_type = GameActionType.SetPhase,
                actor_index = 1,
                new_phase = GamePhase.Main
            };

            NetworkPublicGameEventApplyResult result =
                NetworkPublicGameEventApplier.ApplyToSession(session, publicEvent);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(1, session.public_event_log.Count);
            Assert.AreEqual(1, session.event_cursor);
            Assert.AreEqual(GamePhase.Main, session.opponent_public_view.phase);
            Assert.AreEqual(trueStateBefore, session.local_true_state.ToJson(false));
            Assert.AreEqual(0, session.opponent_public_view.event_log.Count);
        }

        [Test]
        public void RejectsInvalidActorWithoutMutatingPublicView()
        {
            GameState view = CreatePublicView();
            string before = view.ToJson(false);
            NetworkPublicGameEvent publicEvent = new NetworkPublicGameEvent
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                event_id = "public-invalid",
                action_type = GameActionType.Draw,
                actor_index = 4,
                from_zone = GameZone.Deck,
                to_zone = GameZone.Hand,
                from_zone_count_delta = -1,
                to_zone_count_delta = 1,
                hides_card_identity = true
            };

            NetworkPublicGameEventApplyResult result =
                NetworkPublicGameEventApplier.ApplyToPublicView(view, publicEvent);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual("ACTOR_INDEX_OUT_OF_RANGE", result.rejection_reason);
            Assert.AreEqual(before, view.ToJson(false));
        }

        private static GameState CreatePublicView()
        {
            GameState state = new GameState
            {
                game_id = "public-view",
                format = "D",
                random_seed = 1,
                turn_number = 1,
                turn_player_index = 0,
                phase = GamePhase.Mulligan
            };
            PlayerGameState p0 = new PlayerGameState { player_id = "p1" };
            PlayerGameState p1 = new PlayerGameState { player_id = "p2" };
            for (int i = 0; i < 45; i++)
            {
                p1.deck.Add(new GameCardInstance("hidden-p1-deck-" + i, GameStateViewFactory.HiddenCardId, 1, false));
            }

            for (int i = 0; i < 5; i++)
            {
                p1.hand.Add(new GameCardInstance("hidden-p1-hand-" + i, GameStateViewFactory.HiddenCardId, 1, false));
            }

            state.players.Add(p0);
            state.players.Add(p1);
            return state;
        }
    }
}
