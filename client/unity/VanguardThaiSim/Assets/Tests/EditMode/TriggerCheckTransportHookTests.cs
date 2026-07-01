using System;
using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckTransportHookTests
    {
        [Test]
        public void FakeTransportCanSendAndEmitTriggerCheckReplayPayload()
        {
            FakeTransport transport = new FakeTransport();
            NetworkTriggerCheckReplayLogPayload payload = CreatePayload();
            NetworkTriggerCheckReplayLogPayload received = null;
            transport.TriggerCheckReplayLogReceived += value => received = value;

            MultiplayerTransportResult sendResult = transport.SendTriggerCheckReplayLog(payload);
            transport.EmitTriggerCheckReplayLog(payload);

            Assert.IsTrue(sendResult.ok);
            Assert.AreEqual(payload.payload_id, transport.sentTriggerCheckReplayLog.payload_id);
            Assert.NotNull(received);
            Assert.AreEqual(payload.payload_id, received.payload_id);
        }

        [Test]
        public void PhotonAdapterDispatchesDecodedTriggerCheckReplayPayload()
        {
            PhotonRealtimeTransportAdapter adapter = new PhotonRealtimeTransportAdapter(new PhotonRealtimeTransportConfig
            {
                app_id = "test-app-id"
            });
            NetworkTriggerCheckReplayLogPayload payload = CreatePayload();
            NetworkTriggerCheckReplayLogPayload received = null;
            adapter.TriggerCheckReplayLogReceived += value => received = value;

            adapter.ReceiveTriggerCheckReplayLogForTest(PhotonRealtimePayloadCodec.EncodeTriggerCheckReplayLog(payload));

            Assert.NotNull(received);
            Assert.AreEqual(payload.payload_id, received.payload_id);
            Assert.AreEqual(payload.trigger_check_log_json, received.trigger_check_log_json);
        }

        [Test]
        public void PhotonAdapterRejectsNullTriggerCheckReplayPayload()
        {
            PhotonRealtimeTransportAdapter adapter = new PhotonRealtimeTransportAdapter(new PhotonRealtimeTransportConfig
            {
                app_id = "test-app-id"
            });

            MultiplayerTransportResult result = adapter.SendTriggerCheckReplayLog(null);

            Assert.IsFalse(result.ok);
            Assert.AreEqual("TRIGGER_CHECK_REPLAY_LOG_MISSING", result.error_code);
        }

        [Test]
        public void TransportHookDoesNotMutateGameState()
        {
            GameState state = CreateState();
            string before = state.ToJson();
            FakeTransport transport = new FakeTransport();
            NetworkTriggerCheckReplayLogPayload payload = CreatePayload(state);

            transport.SendTriggerCheckReplayLog(payload);
            transport.EmitTriggerCheckReplayLog(payload);

            Assert.AreEqual(before, state.ToJson());
        }

        private static NetworkTriggerCheckReplayLogPayload CreatePayload()
        {
            return CreatePayload(CreateState());
        }

        private static NetworkTriggerCheckReplayLogPayload CreatePayload(GameState state)
        {
            TriggerCheckReplayLog log = TriggerCheckReplayLogBuilder.Append(
                null,
                TriggerCheckLogEntryFactory.FromBundle(
                    TriggerCheckResolutionBundler.Build(
                        state,
                        0,
                        TriggerCheckSource.Drive,
                        0,
                        "drive-card-1",
                        "CRIT-001",
                        TriggerType.Critical,
                        CombatModifierExpiration.EndOfTurn)));
            TriggerCheckReplayLog maskedLog = TriggerCheckReplayLogMasker.CreateView(log, GameStateViewPerspective.Spectator);

            return TriggerCheckReplayLogPayloadCodec.Encode(
                "ROOM-1",
                "p1",
                maskedLog,
                GameStateViewPerspective.Spectator);
        }

        private static GameState CreateState()
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
                            new GameCardInstance("vg", "VG-001", 0)
                        },
                        rear_guard = new List<GameCardInstance>
                        {
                            new GameCardInstance("rg", "RG-001", 0)
                        }
                    }
                }
            };
        }

        private sealed class FakeTransport : IMultiplayerTransport
        {
            public NetworkTriggerCheckReplayLogPayload sentTriggerCheckReplayLog;

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
                sentTriggerCheckReplayLog = payload;
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPendingAutoAbilityQueue(NetworkPendingAutoAbilityQueuePayload payload)
            {
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

            public void EmitTriggerCheckReplayLog(NetworkTriggerCheckReplayLogPayload payload)
            {
                Action<NetworkTriggerCheckReplayLogPayload> handler = TriggerCheckReplayLogReceived;
                if (handler != null)
                {
                    handler(payload);
                }
            }
        }
    }
}
