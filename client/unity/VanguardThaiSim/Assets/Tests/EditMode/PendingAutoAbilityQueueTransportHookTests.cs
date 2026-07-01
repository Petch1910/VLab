using System;
using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityQueueTransportHookTests
    {
        [Test]
        public void FakeTransportCanSendAndEmitPendingQueuePayload()
        {
            FakeTransport transport = new FakeTransport();
            NetworkPendingAutoAbilityQueuePayload payload = CreatePayload();
            NetworkPendingAutoAbilityQueuePayload received = null;
            transport.PendingAutoAbilityQueueReceived += value => received = value;

            MultiplayerTransportResult sendResult = transport.SendPendingAutoAbilityQueue(payload);
            transport.EmitPendingAutoAbilityQueue(payload);

            Assert.IsTrue(sendResult.ok);
            Assert.AreEqual(payload.payload_id, transport.sentPendingAutoAbilityQueue.payload_id);
            Assert.NotNull(received);
            Assert.AreEqual(payload.payload_id, received.payload_id);
        }

        [Test]
        public void PhotonAdapterDispatchesDecodedPendingQueuePayload()
        {
            PhotonRealtimeTransportAdapter adapter = new PhotonRealtimeTransportAdapter(new PhotonRealtimeTransportConfig
            {
                app_id = "test-app-id"
            });
            NetworkPendingAutoAbilityQueuePayload payload = CreatePayload();
            NetworkPendingAutoAbilityQueuePayload received = null;
            adapter.PendingAutoAbilityQueueReceived += value => received = value;

            adapter.ReceivePendingAutoAbilityQueueForTest(
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityQueue(payload));

            Assert.NotNull(received);
            Assert.AreEqual(payload.payload_id, received.payload_id);
            Assert.AreEqual(payload.pending_auto_ability_queue_json, received.pending_auto_ability_queue_json);
        }

        [Test]
        public void PhotonAdapterRejectsNullPendingQueuePayload()
        {
            PhotonRealtimeTransportAdapter adapter = new PhotonRealtimeTransportAdapter(new PhotonRealtimeTransportConfig
            {
                app_id = "test-app-id"
            });

            MultiplayerTransportResult result = adapter.SendPendingAutoAbilityQueue(null);

            Assert.IsFalse(result.ok);
            Assert.AreEqual("PENDING_AUTO_ABILITY_QUEUE_MISSING", result.error_code);
        }

        [Test]
        public void TransportHookDoesNotMutateGameState()
        {
            GameState state = CreateState();
            string before = state.ToJson();
            FakeTransport transport = new FakeTransport();
            NetworkPendingAutoAbilityQueuePayload payload = CreatePayload();

            transport.SendPendingAutoAbilityQueue(payload);
            transport.EmitPendingAutoAbilityQueue(payload);

            Assert.AreEqual(before, state.ToJson());
        }

        private static NetworkPendingAutoAbilityQueuePayload CreatePayload()
        {
            return PendingAutoAbilityQueuePayloadCodec.Encode(
                "ROOM-1",
                0,
                CreateMaskedQueue(),
                GameStateViewPerspective.Spectator);
        }

        private static PendingAutoAbilityQueue CreateMaskedQueue()
        {
            var queue = new PendingAutoAbilityQueue
            {
                queue_id = "queue-1",
                pending = new List<PendingAutoAbility>
                {
                    new PendingAutoAbility
                    {
                        pending_id = "pending-1",
                        source_card_instance_id = "src-1",
                        source_card_id = "CARD-1",
                        player_index = 0,
                        timing_event = "OnDraw",
                        summary = "CARD-1 ability"
                    }
                }
            };

            return PendingAutoAbilityQueueMasker.CreateView(
                queue,
                GameStateViewPerspective.Spectator);
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
            public NetworkPendingAutoAbilityQueuePayload sentPendingAutoAbilityQueue;

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
                sentPendingAutoAbilityQueue = payload;
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPendingAutoAbilityResolutionRequest(
                NetworkPendingAutoAbilityResolutionRequestPayload payload)
            {
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

            public void EmitPendingAutoAbilityQueue(NetworkPendingAutoAbilityQueuePayload payload)
            {
                Action<NetworkPendingAutoAbilityQueuePayload> handler = PendingAutoAbilityQueueReceived;
                if (handler != null)
                {
                    handler(payload);
                }
            }
        }
    }
}
