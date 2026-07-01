using System;
using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityResolutionRequestTransportHookTests
    {
        [Test]
        public void FakeTransportCanSendAndEmitResolutionRequestPayload()
        {
            FakeTransport transport = new FakeTransport();
            NetworkPendingAutoAbilityResolutionRequestPayload payload = CreatePayload();
            NetworkPendingAutoAbilityResolutionRequestPayload received = null;
            transport.PendingAutoAbilityResolutionRequestReceived += value => received = value;

            MultiplayerTransportResult sendResult = transport.SendPendingAutoAbilityResolutionRequest(payload);
            transport.EmitPendingAutoAbilityResolutionRequest(payload);

            Assert.IsTrue(sendResult.ok);
            Assert.AreEqual(payload.payload_id, transport.sentResolutionRequest.payload_id);
            Assert.NotNull(received);
            Assert.AreEqual(payload.payload_id, received.payload_id);
        }

        [Test]
        public void PhotonAdapterDispatchesDecodedResolutionRequestPayload()
        {
            PhotonRealtimeTransportAdapter adapter = new PhotonRealtimeTransportAdapter(new PhotonRealtimeTransportConfig
            {
                app_id = "test-app-id"
            });
            NetworkPendingAutoAbilityResolutionRequestPayload payload = CreatePayload();
            NetworkPendingAutoAbilityResolutionRequestPayload received = null;
            adapter.PendingAutoAbilityResolutionRequestReceived += value => received = value;

            adapter.ReceivePendingAutoAbilityResolutionRequestForTest(
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityResolutionRequest(payload));

            Assert.NotNull(received);
            Assert.AreEqual(payload.payload_id, received.payload_id);
            Assert.AreEqual(
                payload.pending_auto_ability_resolution_request_json,
                received.pending_auto_ability_resolution_request_json);
        }

        [Test]
        public void PhotonAdapterRejectsNullResolutionRequestPayload()
        {
            PhotonRealtimeTransportAdapter adapter = new PhotonRealtimeTransportAdapter(new PhotonRealtimeTransportConfig
            {
                app_id = "test-app-id"
            });

            MultiplayerTransportResult result = adapter.SendPendingAutoAbilityResolutionRequest(null);

            Assert.IsFalse(result.ok);
            Assert.AreEqual("PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_MISSING", result.error_code);
        }

        [Test]
        public void TransportHookDoesNotMutateGameState()
        {
            GameState state = CreateState();
            string before = state.ToJson();
            FakeTransport transport = new FakeTransport();
            NetworkPendingAutoAbilityResolutionRequestPayload payload = CreatePayload();

            transport.SendPendingAutoAbilityResolutionRequest(payload);
            transport.EmitPendingAutoAbilityResolutionRequest(payload);

            Assert.AreEqual(before, state.ToJson());
        }

        private static NetworkPendingAutoAbilityResolutionRequestPayload CreatePayload()
        {
            return PendingAutoAbilityResolutionRequestPayloadCodec.Encode(
                "ROOM-1",
                0,
                new PendingAutoAbilityResolutionRequest
                {
                    selected_index = 0,
                    pending_id = "pending-1",
                    player_index = 0,
                    timing_event = "OnDraw",
                    source_card_instance_id = "src-1",
                    source_card_id = "CARD-1",
                    summary = "Resolve CARD-1"
                },
                GameStateViewPerspective.Player,
                0);
        }

        private static GameState CreateState()
        {
            return new GameState
            {
                game_id = "game-1",
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        vanguard = new List<GameCardInstance>
                        {
                            new GameCardInstance("vg", "VG-001", 0)
                        }
                    }
                }
            };
        }

        private sealed class FakeTransport : IMultiplayerTransport
        {
            public NetworkPendingAutoAbilityResolutionRequestPayload sentResolutionRequest;

            public string TransportName
            {
                get { return "Fake"; }
            }

            public MultiplayerTransportStatus Status
            {
                get { return MultiplayerTransportStatus.InRoom; }
            }

            public string LastError
            {
                get { return null; }
            }

            public event Action<MultiplayerRoomState> RoomStateReceived;
            public event Action<NetworkEventEnvelope> GameEventReceived;
            public event Action<NetworkPublicGameEvent> PublicGameEventReceived;
            public event Action<NetworkTriggerCheckReplayLogPayload> TriggerCheckReplayLogReceived;
            public event Action<NetworkPendingAutoAbilityQueuePayload> PendingAutoAbilityQueueReceived;
            public event Action<NetworkPendingAutoAbilityResolutionRequestPayload> PendingAutoAbilityResolutionRequestReceived;
            public event Action<NetworkPendingAutoAbilityManualResolutionDecisionPayload> PendingAutoAbilityManualResolutionDecisionReceived;
            public event Action<NetworkReconnectRequest> ReconnectRequestReceived;
            public event Action<NetworkEventBatch> ReconnectBatchReceived;
            public event Action<NetworkDeckRevealRequest> DeckRevealRequestReceived;
            public event Action<NetworkDeckRevealResponse> DeckRevealResponseReceived;

            public MultiplayerTransportResult Connect()
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult CreateRoom(MultiplayerRoomState room, RoomPlayerInfo localPlayer)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult JoinRoom(string roomId, RoomPlayerInfo localPlayer, PackSyncInfo localPack)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendRoomState(MultiplayerRoomState room)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendGameEvent(NetworkEventEnvelope envelope)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPublicGameEvent(NetworkPublicGameEvent publicEvent)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendTriggerCheckReplayLog(NetworkTriggerCheckReplayLogPayload payload)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPendingAutoAbilityQueue(NetworkPendingAutoAbilityQueuePayload payload)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPendingAutoAbilityResolutionRequest(
                NetworkPendingAutoAbilityResolutionRequestPayload payload)
            {
                sentResolutionRequest = payload;
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPendingAutoAbilityManualResolutionDecision(
                NetworkPendingAutoAbilityManualResolutionDecisionPayload payload)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendReconnectRequest(NetworkReconnectRequest request)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendReconnectBatch(NetworkEventBatch batch)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendDeckRevealRequest(NetworkDeckRevealRequest request)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendDeckRevealResponse(NetworkDeckRevealResponse response)
            {
                return MultiplayerTransportResult.Ok();
            }

            public void Tick()
            {
            }

            public void Disconnect()
            {
            }

            public void EmitPendingAutoAbilityResolutionRequest(
                NetworkPendingAutoAbilityResolutionRequestPayload payload)
            {
                Action<NetworkPendingAutoAbilityResolutionRequestPayload> handler =
                    PendingAutoAbilityResolutionRequestReceived;
                if (handler != null)
                {
                    handler(payload);
                }
            }
        }
    }
}
