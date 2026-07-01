using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class NetworkPublicReconnectRecoveryTests
    {
        [Test]
        public void CreatesPublicBatchFromCursorAndRoundTripsPhotonPayload()
        {
            MultiplayerRoomState room = CreateCommitmentRoom();
            List<NetworkPublicGameEvent> events = new List<NetworkPublicGameEvent>
            {
                PublicPhase("e0", GamePhase.Main),
                PublicPhase("e1", GamePhase.Battle),
                PublicPhase("e2", GamePhase.End)
            };

            NetworkPublicEventBatch batch = NetworkPublicReconnectRecovery.CreateBatch(room, events, 1);
            PhotonRealtimePayload payload = PhotonRealtimePayloadCodec.EncodePublicEventBatch(batch, "p1");
            NetworkPublicEventBatch decoded;
            string rejectionReason;

            Assert.AreEqual(1, batch.from_event_index);
            Assert.AreEqual(2, batch.events.Count);
            Assert.AreEqual("e1", batch.events[0].event_id);
            Assert.AreEqual(PhotonRealtimePayloadCodec.PublicEventBatchEventCode, payload.event_code);
            Assert.IsTrue(PhotonRealtimePayloadCodec.TryDecodePublicEventBatch(payload, out decoded, out rejectionReason), rejectionReason);
            Assert.AreEqual(2, decoded.events.Count);
            Assert.AreEqual(GamePhase.End, decoded.events[1].new_phase);
        }

        [Test]
        public void AppliesPublicBatchToOwnerPrivateSession()
        {
            LocalOwnerPrivateSession session = CreateSession();
            MultiplayerRoomState room = CreateCommitmentRoom();
            NetworkPublicEventBatch batch = NetworkPublicReconnectRecovery.CreateBatch(
                room,
                new[]
                {
                    HiddenDraw("draw-1"),
                    PublicPhase("phase-1", GamePhase.Main)
                },
                0);

            NetworkPublicReconnectApplyResult result = NetworkPublicReconnectRecovery.ApplyBatch(session, batch);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(2, result.applied_count);
            Assert.AreEqual(2, session.event_cursor);
            Assert.AreEqual(2, session.public_event_log.Count);
            Assert.AreEqual(44, session.opponent_public_view.GetPlayer(1).deck.Count);
            Assert.AreEqual(6, session.opponent_public_view.GetPlayer(1).hand.Count);
            Assert.AreEqual(GamePhase.Main, session.opponent_public_view.phase);
            Assert.AreEqual(0, session.opponent_public_view.event_log.Count);
        }

        [Test]
        public void CursorMismatchRejectsWithoutMutatingSession()
        {
            LocalOwnerPrivateSession session = CreateSession();
            session.event_cursor = 1;
            string before = session.ToJson(false);
            NetworkPublicEventBatch batch = NetworkPublicReconnectRecovery.CreateBatch(
                CreateCommitmentRoom(),
                new[] { HiddenDraw("draw-1") },
                0);

            NetworkPublicReconnectApplyResult result = NetworkPublicReconnectRecovery.ApplyBatch(session, batch);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual("PUBLIC_RECONNECT_CURSOR_MISMATCH", result.rejection_reason);
            Assert.AreEqual(before, session.ToJson(false));
        }

        [Test]
        public void CommitmentSessionDoesNotCreateTrueReconnectEvents()
        {
            MultiplayerRoomState room = CreateCommitmentRoom();
            GameState state = CreatePublicView();
            state.event_log.Add(new GameEvent { event_id = "true-event", action_type = GameActionType.Draw });
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, state, "p1");

            NetworkEventBatch batch = session.CreateReconnectBatch(0);

            Assert.AreEqual(0, batch.events.Count);
            Assert.AreEqual(NetworkPublicReconnectRecovery.CommitmentTrueReconnectBlocked, session.LastMessage);
        }

        private static LocalOwnerPrivateSession CreateSession()
        {
            return new LocalOwnerPrivateSession
            {
                room_id = "ROOM-PUBLIC-REC",
                local_player_id = "p1",
                local_player_index = 0,
                local_true_state = CreatePublicView(),
                opponent_public_view = CreatePublicView()
            };
        }

        private static MultiplayerRoomState CreateCommitmentRoom()
        {
            MultiplayerRoomState room = MultiplayerProtocol.CreateRoom(
                "ROOM-PUBLIC-REC",
                "D",
                "p1",
                9191,
                new PackSyncInfo
                {
                    pack_id = "vanguard_th",
                    source_version = "test",
                    definition_hash = "pack-hash",
                    image_manifest_hash = "image-manifest",
                    image_content_hash = "image-content"
                });
            room.deck_privacy_mode = DeckPrivacyModes.DeckCommitment;
            return room;
        }

        private static NetworkPublicGameEvent HiddenDraw(string eventId)
        {
            return new NetworkPublicGameEvent
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                event_id = eventId,
                action_type = GameActionType.Draw,
                actor_index = 1,
                from_zone = GameZone.Deck,
                to_zone = GameZone.Hand,
                from_zone_count_delta = -1,
                to_zone_count_delta = 1,
                hides_card_identity = true
            };
        }

        private static NetworkPublicGameEvent PublicPhase(string eventId, GamePhase phase)
        {
            return new NetworkPublicGameEvent
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                event_id = eventId,
                action_type = GameActionType.SetPhase,
                actor_index = 1,
                new_phase = phase
            };
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

        private sealed class FakeTransport : IMultiplayerTransport
        {
            public string TransportName { get { return "Fake"; } }
            public MultiplayerTransportStatus Status { get { return MultiplayerTransportStatus.InRoom; } }
            public string LastError { get { return null; } }

            public event System.Action<MultiplayerRoomState> RoomStateReceived;
            public event System.Action<NetworkEventEnvelope> GameEventReceived;
            public event System.Action<NetworkPublicGameEvent> PublicGameEventReceived;
            public event System.Action<NetworkTriggerCheckReplayLogPayload> TriggerCheckReplayLogReceived;
            public event System.Action<NetworkPendingAutoAbilityQueuePayload> PendingAutoAbilityQueueReceived;
            public event System.Action<NetworkPendingAutoAbilityResolutionRequestPayload> PendingAutoAbilityResolutionRequestReceived;
            public event System.Action<NetworkPendingAutoAbilityManualResolutionDecisionPayload> PendingAutoAbilityManualResolutionDecisionReceived;
            public event System.Action<NetworkReconnectRequest> ReconnectRequestReceived;
            public event System.Action<NetworkEventBatch> ReconnectBatchReceived;
            public event System.Action<NetworkDeckRevealRequest> DeckRevealRequestReceived;
            public event System.Action<NetworkDeckRevealResponse> DeckRevealResponseReceived;

            public MultiplayerTransportResult Connect() { return MultiplayerTransportResult.Ok(); }
            public MultiplayerTransportResult CreateRoom(MultiplayerRoomState room, RoomPlayerInfo player) { return MultiplayerTransportResult.Ok(); }
            public MultiplayerTransportResult JoinRoom(string roomId, RoomPlayerInfo player, PackSyncInfo localPack) { return MultiplayerTransportResult.Ok(); }
            public MultiplayerTransportResult SendRoomState(MultiplayerRoomState room) { return MultiplayerTransportResult.Ok(); }
            public MultiplayerTransportResult SendGameEvent(NetworkEventEnvelope envelope) { return MultiplayerTransportResult.Ok(); }
            public MultiplayerTransportResult SendPublicGameEvent(NetworkPublicGameEvent publicEvent) { return MultiplayerTransportResult.Ok(); }
            public MultiplayerTransportResult SendTriggerCheckReplayLog(NetworkTriggerCheckReplayLogPayload payload) { return MultiplayerTransportResult.Ok(); }
            public MultiplayerTransportResult SendPendingAutoAbilityQueue(NetworkPendingAutoAbilityQueuePayload payload) { return MultiplayerTransportResult.Ok(); }
            public MultiplayerTransportResult SendPendingAutoAbilityResolutionRequest(NetworkPendingAutoAbilityResolutionRequestPayload payload) { return MultiplayerTransportResult.Ok(); }
            public MultiplayerTransportResult SendPendingAutoAbilityManualResolutionDecision(NetworkPendingAutoAbilityManualResolutionDecisionPayload payload) { return MultiplayerTransportResult.Ok(); }
            public MultiplayerTransportResult SendReconnectRequest(NetworkReconnectRequest request) { return MultiplayerTransportResult.Ok(); }
            public MultiplayerTransportResult SendReconnectBatch(NetworkEventBatch batch) { return MultiplayerTransportResult.Ok(); }
            public MultiplayerTransportResult SendDeckRevealRequest(NetworkDeckRevealRequest request) { return MultiplayerTransportResult.Ok(); }
            public MultiplayerTransportResult SendDeckRevealResponse(NetworkDeckRevealResponse response) { return MultiplayerTransportResult.Ok(); }
            public void Tick() {}
            public void Disconnect() {}
        }
    }
}
