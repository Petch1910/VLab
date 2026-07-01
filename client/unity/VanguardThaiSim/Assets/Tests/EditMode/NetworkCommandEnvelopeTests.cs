using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class NetworkCommandEnvelopeTests
    {
        [Test]
        public void FactoryCapturesPlayerSequenceGameIdAndCursor()
        {
            MultiplayerRoomState room = CreateRoom();
            GameState state = GameStateFactory.CreateTwoPlayerGame(CreateDeck("p1"), CreateDeck("p2"), 8181);
            state.game_id = "game-room-command";
            state.event_log.Add(new GameEvent { event_id = "event-0", action_type = GameActionType.Draw });
            LegalGameAction action = new LegalGameAction
            {
                label = "Draw",
                action_type = GameActionType.Draw,
                actor_index = 0
            };

            NetworkCommandEnvelope envelope = NetworkCommandEnvelopeFactory.Create(
                room,
                state,
                "p1",
                0,
                7,
                action);

            Assert.AreEqual(MultiplayerProtocol.ProtocolVersion, envelope.protocol_version);
            Assert.AreEqual("ROOM-COMMAND", envelope.room_id);
            Assert.AreEqual("game-room-command", envelope.room_game_id);
            Assert.AreEqual("p1", envelope.player_id);
            Assert.AreEqual(0, envelope.player_index);
            Assert.AreEqual(7, envelope.sequence);
            Assert.AreEqual(1, envelope.state_cursor);
            Assert.AreEqual(GameActionType.Draw, envelope.action.action_type);
            Assert.IsTrue(envelope.command_id.Contains("00000007"));
        }

        [Test]
        public void JsonAndPhotonPayloadRoundTrip()
        {
            MultiplayerRoomState room = CreateRoom();
            GameState state = GameStateFactory.CreateTwoPlayerGame(CreateDeck("p1"), CreateDeck("p2"), 8182);
            state.game_id = "game-round-trip";
            NetworkCommandEnvelope envelope = NetworkCommandEnvelopeFactory.Create(
                room,
                state,
                "p2",
                1,
                3,
                new LegalGameAction
                {
                    label = "Main",
                    action_type = GameActionType.SetPhase,
                    actor_index = 1,
                    phase = GamePhase.Main
                });

            NetworkCommandEnvelope decoded = NetworkCommandEnvelope.FromJson(envelope.ToJson(false));
            PhotonRealtimePayload payload = PhotonRealtimePayloadCodec.EncodeCommandEnvelope(decoded);
            NetworkCommandEnvelope roundTrip;
            string rejectionReason;

            Assert.AreEqual(PhotonRealtimePayloadCodec.CommandEnvelopeEventCode, payload.event_code);
            Assert.AreEqual("p2", payload.sender_player_id);
            Assert.IsTrue(PhotonRealtimePayloadCodec.TryDecodeCommandEnvelope(payload, out roundTrip, out rejectionReason), rejectionReason);
            Assert.AreEqual("game-round-trip", roundTrip.room_game_id);
            Assert.AreEqual(3, roundTrip.sequence);
            Assert.AreEqual(GameActionType.SetPhase, roundTrip.action.action_type);
            Assert.AreEqual(GamePhase.Main, roundTrip.action.phase);
        }

        [Test]
        public void ValidatorRejectsMissingCursorAndRoomMismatch()
        {
            MultiplayerRoomState room = CreateRoom();
            NetworkCommandEnvelope envelope = new NetworkCommandEnvelope
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                command_id = "cmd",
                room_id = "OTHER",
                room_game_id = "game",
                player_id = "p1",
                player_index = 0,
                sequence = 0,
                state_cursor = -1,
                action = new LegalGameAction { action_type = GameActionType.Draw }
            };

            string rejectionReason;
            Assert.IsFalse(NetworkCommandEnvelopeValidator.TryValidateShape(envelope, room, out rejectionReason));
            Assert.AreEqual("ROOM_ID_MISMATCH", rejectionReason);

            envelope.room_id = room.room_id;
            Assert.IsFalse(NetworkCommandEnvelopeValidator.TryValidateShape(envelope, room, out rejectionReason));
            Assert.AreEqual("STATE_CURSOR_MISSING", rejectionReason);

            envelope.state_cursor = 0;
            Assert.IsTrue(NetworkCommandEnvelopeValidator.TryValidateShape(envelope, room, out rejectionReason), rejectionReason);
        }

        [Test]
        public void StateValidatorAcceptsCurrentTurnOwnerCommand()
        {
            MultiplayerRoomState room = CreateRoomWithPlayers();
            GameState state = GameStateFactory.CreateTwoPlayerGame(CreateDeck("p1"), CreateDeck("p2"), 8183, "p1", "p2");
            state.game_id = "game-current";
            state.turn_player_index = 0;
            NetworkCommandEnvelope envelope = NetworkCommandEnvelopeFactory.Create(
                room,
                state,
                "p1",
                0,
                0,
                new LegalGameAction
                {
                    action_type = GameActionType.Draw,
                    actor_index = 0
                });

            string rejectionReason;
            Assert.IsTrue(NetworkCommandEnvelopeValidator.TryValidateForState(envelope, room, state, out rejectionReason), rejectionReason);
        }

        [Test]
        public void StateValidatorRejectsStaleCursorWithoutMutatingState()
        {
            MultiplayerRoomState room = CreateRoomWithPlayers();
            GameState state = GameStateFactory.CreateTwoPlayerGame(CreateDeck("p1"), CreateDeck("p2"), 8184, "p1", "p2");
            state.game_id = "game-stale";
            state.event_log.Add(new GameEvent { event_id = "event-0", action_type = GameActionType.Draw });
            string before = state.ToJson(false);
            NetworkCommandEnvelope envelope = new NetworkCommandEnvelope
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                command_id = "cmd-stale",
                room_id = room.room_id,
                room_game_id = state.game_id,
                player_id = "p1",
                player_index = 0,
                sequence = 1,
                state_cursor = 0,
                action = new LegalGameAction
                {
                    action_type = GameActionType.Draw,
                    actor_index = 0
                }
            };

            string rejectionReason;
            Assert.IsFalse(NetworkCommandEnvelopeValidator.TryValidateForState(envelope, room, state, out rejectionReason));
            Assert.AreEqual("STATE_CURSOR_STALE", rejectionReason);
            Assert.AreEqual(before, state.ToJson(false));
        }

        [Test]
        public void StateValidatorRejectsOutOfTurnAndActorMismatch()
        {
            MultiplayerRoomState room = CreateRoomWithPlayers();
            GameState state = GameStateFactory.CreateTwoPlayerGame(CreateDeck("p1"), CreateDeck("p2"), 8185, "p1", "p2");
            state.game_id = "game-turn";
            state.turn_player_index = 0;
            NetworkCommandEnvelope outOfTurn = NetworkCommandEnvelopeFactory.Create(
                room,
                state,
                "p2",
                1,
                0,
                new LegalGameAction
                {
                    action_type = GameActionType.Draw,
                    actor_index = 1
                });
            NetworkCommandEnvelope actorMismatch = NetworkCommandEnvelopeFactory.Create(
                room,
                state,
                "p1",
                0,
                1,
                new LegalGameAction
                {
                    action_type = GameActionType.Draw,
                    actor_index = 1
                });

            string rejectionReason;
            Assert.IsFalse(NetworkCommandEnvelopeValidator.TryValidateForState(outOfTurn, room, state, out rejectionReason));
            Assert.AreEqual("OUT_OF_TURN_COMMAND", rejectionReason);
            Assert.IsFalse(NetworkCommandEnvelopeValidator.TryValidateForState(actorMismatch, room, state, out rejectionReason));
            Assert.AreEqual("ACTION_ACTOR_MISMATCH", rejectionReason);
        }

        [Test]
        public void StateValidatorRejectsPlayerOwnershipMismatch()
        {
            MultiplayerRoomState room = CreateRoomWithPlayers();
            GameState state = GameStateFactory.CreateTwoPlayerGame(CreateDeck("p1"), CreateDeck("p2"), 8186, "p1", "p2");
            state.game_id = "game-owner";
            NetworkCommandEnvelope envelope = NetworkCommandEnvelopeFactory.Create(
                room,
                state,
                "p2",
                0,
                0,
                new LegalGameAction
                {
                    action_type = GameActionType.Draw,
                    actor_index = 0
                });

            string rejectionReason;
            Assert.IsFalse(NetworkCommandEnvelopeValidator.TryValidateForState(envelope, room, state, out rejectionReason));
            Assert.AreEqual("PLAYER_OWNERSHIP_MISMATCH", rejectionReason);
        }

        [Test]
        public void DecodeRejectsWrongPhotonEventCode()
        {
            PhotonRealtimePayload payload = new PhotonRealtimePayload
            {
                event_code = PhotonRealtimePayloadCodec.GameEventCode,
                sender_player_id = "p1",
                json = "{}"
            };

            NetworkCommandEnvelope envelope;
            string rejectionReason;
            Assert.IsFalse(PhotonRealtimePayloadCodec.TryDecodeCommandEnvelope(payload, out envelope, out rejectionReason));
            Assert.AreEqual("COMMAND_ENVELOPE_PAYLOAD_INVALID", rejectionReason);
            Assert.IsNull(envelope);
        }

        private static MultiplayerRoomState CreateRoom()
        {
            return MultiplayerProtocol.CreateRoom(
                "ROOM-COMMAND",
                "D",
                "p1",
                8180,
                new PackSyncInfo
                {
                    pack_id = "vanguard_th",
                    source_version = "test",
                    definition_hash = "pack-hash",
                    image_manifest_hash = "image-manifest",
                    image_content_hash = "image-content"
                });
        }

        private static MultiplayerRoomState CreateRoomWithPlayers()
        {
            MultiplayerRoomState room = CreateRoom();
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "p1",
                display_name = "p1",
                connected = true
            });
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "p2",
                display_name = "p2",
                connected = true
            });
            return room;
        }

        private static VanguardDeck CreateDeck(string prefix)
        {
            VanguardDeck deck = VanguardDeck.Create(prefix + " deck", "D", "vanguard_th", "test");
            for (int i = 0; i < 50; i++)
            {
                deck.AddCard(DeckZone.Main, prefix + "-MAIN-" + i, 1);
            }

            for (int i = 0; i < 4; i++)
            {
                deck.AddCard(DeckZone.Ride, prefix + "-RIDE-" + i, 1);
            }

            return deck;
        }
    }
}
