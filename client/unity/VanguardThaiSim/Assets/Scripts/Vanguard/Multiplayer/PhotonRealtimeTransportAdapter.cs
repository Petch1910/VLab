using System;
using System.Collections.Generic;

#if VANGUARD_PHOTON_REALTIME
using Photon.Client;
using Photon.Realtime;
#endif

namespace VanguardThaiSim.Multiplayer
{
    public sealed class PhotonRealtimeTransportAdapter : IMultiplayerTransport
    {
        public const string RequiredSdkDefine = "VANGUARD_PHOTON_REALTIME";
        private readonly PhotonRealtimeTransportConfig config;
        private readonly IPhotonRealtimeBridge bridge;

        public PhotonRealtimeTransportAdapter(PhotonRealtimeTransportConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            bridge = CreateBridge(this.config, this);
        }

        public string TransportName
        {
            get { return "Photon Realtime"; }
        }

        public MultiplayerTransportStatus Status
        {
            get { return bridge.Status; }
        }

        public string LastError
        {
            get { return bridge.LastError; }
        }

        public bool IsSdkBridgeEnabled
        {
            get { return bridge.IsSdkBridgeEnabled; }
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
            return bridge.Connect();
        }

        public MultiplayerTransportResult CreateRoom(MultiplayerRoomState room, RoomPlayerInfo localPlayer)
        {
            if (room == null)
            {
                return bridge.Fail("ROOM_MISSING", "Room state is required.");
            }

            if (localPlayer == null)
            {
                return bridge.Fail("PLAYER_MISSING", "Local player is required.");
            }

            PhotonRealtimePayloadCodec.RoomToProperties(room);
            return bridge.CreateRoom(room, localPlayer);
        }

        public MultiplayerTransportResult JoinRoom(string roomId, RoomPlayerInfo localPlayer, PackSyncInfo localPack)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                return bridge.Fail("ROOM_ID_MISSING", "Room id is required.");
            }

            if (localPlayer == null)
            {
                return bridge.Fail("PLAYER_MISSING", "Local player is required.");
            }

            if (localPack == null)
            {
                return bridge.Fail("PACK_MISSING", "Local pack sync info is required.");
            }

            return bridge.JoinRoom(roomId, localPlayer, localPack);
        }

        public MultiplayerTransportResult SendRoomState(MultiplayerRoomState room)
        {
            if (room == null)
            {
                return bridge.Fail("ROOM_MISSING", "Room state is required.");
            }

            PhotonRealtimePayloadCodec.EncodeRoomState(room);
            return bridge.SendRoomState(room);
        }

        public MultiplayerTransportResult SendGameEvent(NetworkEventEnvelope envelope)
        {
            if (envelope == null)
            {
                return bridge.Fail("EVENT_MISSING", "Network event envelope is required.");
            }

            PhotonRealtimePayloadCodec.EncodeGameEvent(envelope);
            return bridge.SendGameEvent(envelope);
        }

        public MultiplayerTransportResult SendPublicGameEvent(NetworkPublicGameEvent publicEvent)
        {
            if (publicEvent == null)
            {
                return bridge.Fail("PUBLIC_EVENT_MISSING", "Public game event is required.");
            }

            PhotonRealtimePayloadCodec.EncodePublicGameEvent(publicEvent);
            return bridge.SendPublicGameEvent(publicEvent);
        }

        public MultiplayerTransportResult SendTriggerCheckReplayLog(NetworkTriggerCheckReplayLogPayload payload)
        {
            if (payload == null)
            {
                return bridge.Fail("TRIGGER_CHECK_REPLAY_LOG_MISSING", "Trigger check replay log payload is required.");
            }

            PhotonRealtimePayloadCodec.EncodeTriggerCheckReplayLog(payload);
            return bridge.SendTriggerCheckReplayLog(payload);
        }

        public MultiplayerTransportResult SendPendingAutoAbilityQueue(NetworkPendingAutoAbilityQueuePayload payload)
        {
            if (payload == null)
            {
                return bridge.Fail("PENDING_AUTO_ABILITY_QUEUE_MISSING", "Pending auto ability queue payload is required.");
            }

            PhotonRealtimePayloadCodec.EncodePendingAutoAbilityQueue(payload);
            return bridge.SendPendingAutoAbilityQueue(payload);
        }

        public MultiplayerTransportResult SendPendingAutoAbilityResolutionRequest(
            NetworkPendingAutoAbilityResolutionRequestPayload payload)
        {
            if (payload == null)
            {
                return bridge.Fail(
                    "PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_MISSING",
                    "Pending auto ability resolution request payload is required.");
            }

            PhotonRealtimePayloadCodec.EncodePendingAutoAbilityResolutionRequest(payload);
            return bridge.SendPendingAutoAbilityResolutionRequest(payload);
        }

        public MultiplayerTransportResult SendPendingAutoAbilityManualResolutionDecision(
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload)
        {
            if (payload == null)
            {
                return bridge.Fail(
                    "PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_MISSING",
                    "Pending auto ability manual resolution decision payload is required.");
            }

            PhotonRealtimePayloadCodec.EncodePendingAutoAbilityManualResolutionDecision(payload);
            return bridge.SendPendingAutoAbilityManualResolutionDecision(payload);
        }

        public MultiplayerTransportResult SendReconnectRequest(NetworkReconnectRequest request)
        {
            if (request == null)
            {
                return bridge.Fail("RECONNECT_REQUEST_MISSING", "Reconnect request is required.");
            }

            PhotonRealtimePayloadCodec.EncodeReconnectRequest(request);
            return bridge.SendReconnectRequest(request);
        }

        public MultiplayerTransportResult SendReconnectBatch(NetworkEventBatch batch)
        {
            if (batch == null)
            {
                return bridge.Fail("RECONNECT_BATCH_MISSING", "Reconnect batch is required.");
            }

            PhotonRealtimePayloadCodec.EncodeReconnectBatch(batch);
            return bridge.SendReconnectBatch(batch);
        }

        public MultiplayerTransportResult SendDeckRevealRequest(NetworkDeckRevealRequest request)
        {
            if (request == null)
            {
                return bridge.Fail("DECK_REVEAL_REQUEST_MISSING", "Deck reveal request is required.");
            }

            PhotonRealtimePayloadCodec.EncodeDeckRevealRequest(request);
            return bridge.SendDeckRevealRequest(request);
        }

        public MultiplayerTransportResult SendDeckRevealResponse(NetworkDeckRevealResponse response)
        {
            if (response == null)
            {
                return bridge.Fail("DECK_REVEAL_RESPONSE_MISSING", "Deck reveal response is required.");
            }

            PhotonRealtimePayloadCodec.EncodeDeckRevealResponse(response);
            return bridge.SendDeckRevealResponse(response);
        }

        public void Tick()
        {
            bridge.Tick();
        }

        public void Disconnect()
        {
            bridge.Disconnect();
        }

        public void ReceiveRoomStateForTest(PhotonRealtimePayload payload)
        {
            MultiplayerRoomState room;
            string rejectionReason;
            if (!PhotonRealtimePayloadCodec.TryDecodeRoomState(payload, out room, out rejectionReason))
            {
                bridge.Fail("ROOM_STATE_REJECTED", rejectionReason);
                return;
            }

            RaiseRoomStateReceived(room);
        }

        public void ReceiveGameEventForTest(PhotonRealtimePayload payload)
        {
            NetworkEventEnvelope envelope;
            string rejectionReason;
            if (!PhotonRealtimePayloadCodec.TryDecodeGameEvent(payload, out envelope, out rejectionReason))
            {
                bridge.Fail("GAME_EVENT_REJECTED", rejectionReason);
                return;
            }

            RaiseGameEventReceived(envelope);
        }

        public void ReceivePublicGameEventForTest(PhotonRealtimePayload payload)
        {
            NetworkPublicGameEvent publicEvent;
            string rejectionReason;
            if (!PhotonRealtimePayloadCodec.TryDecodePublicGameEvent(payload, out publicEvent, out rejectionReason))
            {
                bridge.Fail("PUBLIC_GAME_EVENT_REJECTED", rejectionReason);
                return;
            }

            RaisePublicGameEventReceived(publicEvent);
        }

        public void ReceiveTriggerCheckReplayLogForTest(PhotonRealtimePayload payload)
        {
            NetworkTriggerCheckReplayLogPayload triggerLogPayload;
            string rejectionReason;
            if (!PhotonRealtimePayloadCodec.TryDecodeTriggerCheckReplayLog(payload, out triggerLogPayload, out rejectionReason))
            {
                bridge.Fail("TRIGGER_CHECK_REPLAY_LOG_REJECTED", rejectionReason);
                return;
            }

            RaiseTriggerCheckReplayLogReceived(triggerLogPayload);
        }

        public void ReceivePendingAutoAbilityQueueForTest(PhotonRealtimePayload payload)
        {
            NetworkPendingAutoAbilityQueuePayload pendingQueuePayload;
            string rejectionReason;
            if (!PhotonRealtimePayloadCodec.TryDecodePendingAutoAbilityQueue(payload, out pendingQueuePayload, out rejectionReason))
            {
                bridge.Fail("PENDING_AUTO_ABILITY_QUEUE_REJECTED", rejectionReason);
                return;
            }

            RaisePendingAutoAbilityQueueReceived(pendingQueuePayload);
        }

        public void ReceivePendingAutoAbilityResolutionRequestForTest(PhotonRealtimePayload payload)
        {
            NetworkPendingAutoAbilityResolutionRequestPayload requestPayload;
            string rejectionReason;
            if (!PhotonRealtimePayloadCodec.TryDecodePendingAutoAbilityResolutionRequest(
                    payload,
                    out requestPayload,
                    out rejectionReason))
            {
                bridge.Fail("PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_REJECTED", rejectionReason);
                return;
            }

            RaisePendingAutoAbilityResolutionRequestReceived(requestPayload);
        }

        public void ReceivePendingAutoAbilityManualResolutionDecisionForTest(PhotonRealtimePayload payload)
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload decisionPayload;
            string rejectionReason;
            if (!PhotonRealtimePayloadCodec.TryDecodePendingAutoAbilityManualResolutionDecision(
                    payload,
                    out decisionPayload,
                    out rejectionReason))
            {
                bridge.Fail("PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_REJECTED", rejectionReason);
                return;
            }

            RaisePendingAutoAbilityManualResolutionDecisionReceived(decisionPayload);
        }

        public void ReceiveReconnectRequestForTest(PhotonRealtimePayload payload)
        {
            NetworkReconnectRequest request;
            string rejectionReason;
            if (!PhotonRealtimePayloadCodec.TryDecodeReconnectRequest(payload, out request, out rejectionReason))
            {
                bridge.Fail("RECONNECT_REQUEST_REJECTED", rejectionReason);
                return;
            }

            RaiseReconnectRequestReceived(request);
        }

        public void ReceiveReconnectBatchForTest(PhotonRealtimePayload payload)
        {
            NetworkEventBatch batch;
            string rejectionReason;
            if (!PhotonRealtimePayloadCodec.TryDecodeReconnectBatch(payload, out batch, out rejectionReason))
            {
                bridge.Fail("RECONNECT_BATCH_REJECTED", rejectionReason);
                return;
            }

            RaiseReconnectBatchReceived(batch);
        }

        public void ReceiveDeckRevealRequestForTest(PhotonRealtimePayload payload)
        {
            NetworkDeckRevealRequest request;
            string rejectionReason;
            if (!PhotonRealtimePayloadCodec.TryDecodeDeckRevealRequest(payload, out request, out rejectionReason))
            {
                bridge.Fail("DECK_REVEAL_REQUEST_REJECTED", rejectionReason);
                return;
            }

            RaiseDeckRevealRequestReceived(request);
        }

        public void ReceiveDeckRevealResponseForTest(PhotonRealtimePayload payload)
        {
            NetworkDeckRevealResponse response;
            string rejectionReason;
            if (!PhotonRealtimePayloadCodec.TryDecodeDeckRevealResponse(payload, out response, out rejectionReason))
            {
                bridge.Fail("DECK_REVEAL_RESPONSE_REJECTED", rejectionReason);
                return;
            }

            RaiseDeckRevealResponseReceived(response);
        }

        private void RaiseRoomStateReceived(MultiplayerRoomState room)
        {
            Action<MultiplayerRoomState> handler = RoomStateReceived;
            if (handler != null)
            {
                handler(room);
            }
        }

        private void RaiseGameEventReceived(NetworkEventEnvelope envelope)
        {
            Action<NetworkEventEnvelope> handler = GameEventReceived;
            if (handler != null)
            {
                handler(envelope);
            }
        }

        private void RaisePublicGameEventReceived(NetworkPublicGameEvent publicEvent)
        {
            Action<NetworkPublicGameEvent> handler = PublicGameEventReceived;
            if (handler != null)
            {
                handler(publicEvent);
            }
        }

        private void RaiseTriggerCheckReplayLogReceived(NetworkTriggerCheckReplayLogPayload payload)
        {
            Action<NetworkTriggerCheckReplayLogPayload> handler = TriggerCheckReplayLogReceived;
            if (handler != null)
            {
                handler(payload);
            }
        }

        private void RaisePendingAutoAbilityQueueReceived(NetworkPendingAutoAbilityQueuePayload payload)
        {
            Action<NetworkPendingAutoAbilityQueuePayload> handler = PendingAutoAbilityQueueReceived;
            if (handler != null)
            {
                handler(payload);
            }
        }

        private void RaisePendingAutoAbilityResolutionRequestReceived(
            NetworkPendingAutoAbilityResolutionRequestPayload payload)
        {
            Action<NetworkPendingAutoAbilityResolutionRequestPayload> handler =
                PendingAutoAbilityResolutionRequestReceived;
            if (handler != null)
            {
                handler(payload);
            }
        }

        private void RaisePendingAutoAbilityManualResolutionDecisionReceived(
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload)
        {
            Action<NetworkPendingAutoAbilityManualResolutionDecisionPayload> handler =
                PendingAutoAbilityManualResolutionDecisionReceived;
            if (handler != null)
            {
                handler(payload);
            }
        }

        private void RaiseReconnectRequestReceived(NetworkReconnectRequest request)
        {
            Action<NetworkReconnectRequest> handler = ReconnectRequestReceived;
            if (handler != null)
            {
                handler(request);
            }
        }

        private void RaiseReconnectBatchReceived(NetworkEventBatch batch)
        {
            Action<NetworkEventBatch> handler = ReconnectBatchReceived;
            if (handler != null)
            {
                handler(batch);
            }
        }

        private void RaiseDeckRevealRequestReceived(NetworkDeckRevealRequest request)
        {
            Action<NetworkDeckRevealRequest> handler = DeckRevealRequestReceived;
            if (handler != null)
            {
                handler(request);
            }
        }

        private void RaiseDeckRevealResponseReceived(NetworkDeckRevealResponse response)
        {
            Action<NetworkDeckRevealResponse> handler = DeckRevealResponseReceived;
            if (handler != null)
            {
                handler(response);
            }
        }

        private static IPhotonRealtimeBridge CreateBridge(PhotonRealtimeTransportConfig config, PhotonRealtimeTransportAdapter owner)
        {
#if VANGUARD_PHOTON_REALTIME
            return new LivePhotonRealtimeBridge(config, owner);
#else
            return new MissingPhotonRealtimeBridge(config);
#endif
        }

        private interface IPhotonRealtimeBridge
        {
            MultiplayerTransportStatus Status { get; }
            string LastError { get; }
            bool IsSdkBridgeEnabled { get; }

            MultiplayerTransportResult Connect();
            MultiplayerTransportResult CreateRoom(MultiplayerRoomState room, RoomPlayerInfo localPlayer);
            MultiplayerTransportResult JoinRoom(string roomId, RoomPlayerInfo localPlayer, PackSyncInfo localPack);
            MultiplayerTransportResult SendRoomState(MultiplayerRoomState room);
            MultiplayerTransportResult SendGameEvent(NetworkEventEnvelope envelope);
            MultiplayerTransportResult SendPublicGameEvent(NetworkPublicGameEvent publicEvent);
            MultiplayerTransportResult SendTriggerCheckReplayLog(NetworkTriggerCheckReplayLogPayload payload);
            MultiplayerTransportResult SendPendingAutoAbilityQueue(NetworkPendingAutoAbilityQueuePayload payload);
            MultiplayerTransportResult SendPendingAutoAbilityResolutionRequest(
                NetworkPendingAutoAbilityResolutionRequestPayload payload);
            MultiplayerTransportResult SendPendingAutoAbilityManualResolutionDecision(
                NetworkPendingAutoAbilityManualResolutionDecisionPayload payload);
            MultiplayerTransportResult SendReconnectRequest(NetworkReconnectRequest request);
            MultiplayerTransportResult SendReconnectBatch(NetworkEventBatch batch);
            MultiplayerTransportResult SendDeckRevealRequest(NetworkDeckRevealRequest request);
            MultiplayerTransportResult SendDeckRevealResponse(NetworkDeckRevealResponse response);
            MultiplayerTransportResult Fail(string errorCode, string message);
            void Tick();
            void Disconnect();
        }

        private sealed class MissingPhotonRealtimeBridge : IPhotonRealtimeBridge
        {
            private readonly PhotonRealtimeTransportConfig config;
            private MultiplayerTransportStatus status = MultiplayerTransportStatus.Disconnected;
            private string lastError;

            public MissingPhotonRealtimeBridge(PhotonRealtimeTransportConfig config)
            {
                this.config = config;
            }

            public MultiplayerTransportStatus Status
            {
                get { return status; }
            }

            public string LastError
            {
                get { return lastError; }
            }

            public bool IsSdkBridgeEnabled
            {
                get { return false; }
            }

            public MultiplayerTransportResult Connect()
            {
                if (!config.IsConfigured)
                {
                    return Fail("PHOTON_APP_ID_MISSING", "Photon AppId is required before connecting.");
                }

                return Fail(
                    "PHOTON_SDK_NOT_ENABLED",
                    "Install Photon Realtime SDK and add scripting define " + RequiredSdkDefine + " before using live Photon transport.");
            }

            public MultiplayerTransportResult CreateRoom(MultiplayerRoomState room, RoomPlayerInfo localPlayer)
            {
                return Fail("PHOTON_SDK_NOT_ENABLED", "Room payload is valid, but live Photon SDK bridge is not enabled.");
            }

            public MultiplayerTransportResult JoinRoom(string roomId, RoomPlayerInfo localPlayer, PackSyncInfo localPack)
            {
                return Fail("PHOTON_SDK_NOT_ENABLED", "Join request is valid, but live Photon SDK bridge is not enabled.");
            }

            public MultiplayerTransportResult SendRoomState(MultiplayerRoomState room)
            {
                return Fail("PHOTON_SDK_NOT_ENABLED", "Room state payload is valid, but live Photon SDK bridge is not enabled.");
            }

            public MultiplayerTransportResult SendGameEvent(NetworkEventEnvelope envelope)
            {
                return Fail("PHOTON_SDK_NOT_ENABLED", "Game event payload is valid, but live Photon SDK bridge is not enabled.");
            }

            public MultiplayerTransportResult SendPublicGameEvent(NetworkPublicGameEvent publicEvent)
            {
                return Fail("PHOTON_SDK_NOT_ENABLED", "Public game event payload is valid, but live Photon SDK bridge is not enabled.");
            }

            public MultiplayerTransportResult SendTriggerCheckReplayLog(NetworkTriggerCheckReplayLogPayload payload)
            {
                return Fail("PHOTON_SDK_NOT_ENABLED", "Trigger check replay log payload is valid, but live Photon SDK bridge is not enabled.");
            }

            public MultiplayerTransportResult SendPendingAutoAbilityQueue(NetworkPendingAutoAbilityQueuePayload payload)
            {
                return Fail("PHOTON_SDK_NOT_ENABLED", "Pending auto ability queue payload is valid, but live Photon SDK bridge is not enabled.");
            }

            public MultiplayerTransportResult SendPendingAutoAbilityResolutionRequest(
                NetworkPendingAutoAbilityResolutionRequestPayload payload)
            {
                return Fail("PHOTON_SDK_NOT_ENABLED", "Pending auto ability resolution request payload is valid, but live Photon SDK bridge is not enabled.");
            }

            public MultiplayerTransportResult SendPendingAutoAbilityManualResolutionDecision(
                NetworkPendingAutoAbilityManualResolutionDecisionPayload payload)
            {
                return Fail("PHOTON_SDK_NOT_ENABLED", "Pending auto ability manual resolution decision payload is valid, but live Photon SDK bridge is not enabled.");
            }

            public MultiplayerTransportResult SendReconnectRequest(NetworkReconnectRequest request)
            {
                return Fail("PHOTON_SDK_NOT_ENABLED", "Reconnect request payload is valid, but live Photon SDK bridge is not enabled.");
            }

            public MultiplayerTransportResult SendReconnectBatch(NetworkEventBatch batch)
            {
                return Fail("PHOTON_SDK_NOT_ENABLED", "Reconnect batch payload is valid, but live Photon SDK bridge is not enabled.");
            }

            public MultiplayerTransportResult SendDeckRevealRequest(NetworkDeckRevealRequest request)
            {
                return Fail("PHOTON_SDK_NOT_ENABLED", "Deck reveal request payload is valid, but live Photon SDK bridge is not enabled.");
            }

            public MultiplayerTransportResult SendDeckRevealResponse(NetworkDeckRevealResponse response)
            {
                return Fail("PHOTON_SDK_NOT_ENABLED", "Deck reveal response payload is valid, but live Photon SDK bridge is not enabled.");
            }

            public MultiplayerTransportResult Fail(string errorCode, string message)
            {
                status = MultiplayerTransportStatus.Failed;
                lastError = errorCode;
                return MultiplayerTransportResult.Fail(errorCode, message);
            }

            public void Tick()
            {
            }

            public void Disconnect()
            {
                status = MultiplayerTransportStatus.Disconnected;
                lastError = null;
            }
        }

#if VANGUARD_PHOTON_REALTIME
        private sealed class LivePhotonRealtimeBridge :
            IPhotonRealtimeBridge,
            IConnectionCallbacks,
            IMatchmakingCallbacks,
            IInRoomCallbacks,
            IOnEventCallback
        {
            private readonly PhotonRealtimeTransportConfig config;
            private readonly PhotonRealtimeTransportAdapter owner;
            private readonly RealtimeClient client = new RealtimeClient();
            private MultiplayerTransportStatus status = MultiplayerTransportStatus.Disconnected;
            private string lastError;
            private PackSyncInfo pendingLocalPack;

            public LivePhotonRealtimeBridge(PhotonRealtimeTransportConfig config, PhotonRealtimeTransportAdapter owner)
            {
                this.config = config;
                this.owner = owner;
                client.AddCallbackTarget(this);
            }

            public MultiplayerTransportStatus Status
            {
                get { return status; }
            }

            public string LastError
            {
                get { return lastError; }
            }

            public bool IsSdkBridgeEnabled
            {
                get { return true; }
            }

            public MultiplayerTransportResult Connect()
            {
                if (!config.IsConfigured)
                {
                    return Fail("PHOTON_APP_ID_MISSING", "Photon AppId is required before connecting.");
                }

                status = MultiplayerTransportStatus.Connecting;
                lastError = null;
                AppSettings settings = new AppSettings
                {
                    AppIdRealtime = config.app_id,
                    AppVersion = config.app_version,
                    FixedRegion = config.fixed_region
                };

                if (!client.ConnectUsingSettings(settings))
                {
                    return Fail("PHOTON_CONNECT_FAILED", "Photon refused the connect request.");
                }

                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult CreateRoom(MultiplayerRoomState room, RoomPlayerInfo localPlayer)
            {
                if (!client.IsConnectedAndReady)
                {
                    return Fail("PHOTON_NOT_READY", "Photon is not connected and ready.");
                }

                status = MultiplayerTransportStatus.JoiningRoom;
                client.NickName = localPlayer.display_name;
                client.UserId = localPlayer.player_id;

                EnterRoomArgs enterRoomArgs = new EnterRoomArgs
                {
                    RoomName = room.room_id,
                    RoomOptions = new RoomOptions
                    {
                        MaxPlayers = 2,
                        IsVisible = true,
                        IsOpen = true,
                        CustomRoomProperties = ToPhotonHashtable(PhotonRealtimePayloadCodec.RoomToProperties(room)),
                        CustomRoomPropertiesForLobby = new[]
                        {
                            PhotonRealtimePayloadCodec.ProtocolVersionProperty,
                            PhotonRealtimePayloadCodec.FormatProperty,
                            PhotonRealtimePayloadCodec.StateProperty,
                            PhotonRealtimePayloadCodec.PackIdProperty,
                            PhotonRealtimePayloadCodec.PackDefinitionHashProperty
                        }
                    }
                };

                if (!client.OpCreateRoom(enterRoomArgs))
                {
                    return Fail("PHOTON_CREATE_ROOM_FAILED", "Photon refused the create-room request.");
                }

                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult JoinRoom(string roomId, RoomPlayerInfo localPlayer, PackSyncInfo localPack)
            {
                if (!client.IsConnectedAndReady)
                {
                    return Fail("PHOTON_NOT_READY", "Photon is not connected and ready.");
                }

                status = MultiplayerTransportStatus.JoiningRoom;
                pendingLocalPack = localPack;
                client.NickName = localPlayer.display_name;
                client.UserId = localPlayer.player_id;

                EnterRoomArgs enterRoomArgs = new EnterRoomArgs
                {
                    RoomName = roomId
                };

                if (!client.OpJoinRoom(enterRoomArgs))
                {
                    return Fail("PHOTON_JOIN_ROOM_FAILED", "Photon refused the join-room request.");
                }

                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendRoomState(MultiplayerRoomState room)
            {
                if (!client.InRoom || client.CurrentRoom == null)
                {
                    return Fail("PHOTON_NOT_IN_ROOM", "Cannot send room state before joining a room.");
                }

                client.OpSetCustomPropertiesOfRoom(ToPhotonHashtable(PhotonRealtimePayloadCodec.RoomToProperties(room)));
                return RaisePayload(PhotonRealtimePayloadCodec.EncodeRoomState(room), config.room_state_event_code);
            }

            public MultiplayerTransportResult SendGameEvent(NetworkEventEnvelope envelope)
            {
                if (!client.InRoom)
                {
                    return Fail("PHOTON_NOT_IN_ROOM", "Cannot send game event before joining a room.");
                }

                return RaisePayload(PhotonRealtimePayloadCodec.EncodeGameEvent(envelope), config.game_event_code);
            }

            public MultiplayerTransportResult SendPublicGameEvent(NetworkPublicGameEvent publicEvent)
            {
                if (!client.InRoom)
                {
                    return Fail("PHOTON_NOT_IN_ROOM", "Cannot send public game event before joining a room.");
                }

                return RaisePayload(PhotonRealtimePayloadCodec.EncodePublicGameEvent(publicEvent), config.public_game_event_code);
            }

            public MultiplayerTransportResult SendTriggerCheckReplayLog(NetworkTriggerCheckReplayLogPayload payload)
            {
                if (!client.InRoom)
                {
                    return Fail("PHOTON_NOT_IN_ROOM", "Cannot send trigger check replay log before joining a room.");
                }

                return RaisePayload(PhotonRealtimePayloadCodec.EncodeTriggerCheckReplayLog(payload), config.trigger_check_replay_log_event_code);
            }

            public MultiplayerTransportResult SendPendingAutoAbilityQueue(NetworkPendingAutoAbilityQueuePayload payload)
            {
                if (!client.InRoom)
                {
                    return Fail("PHOTON_NOT_IN_ROOM", "Cannot send pending auto ability queue before joining a room.");
                }

                return RaisePayload(PhotonRealtimePayloadCodec.EncodePendingAutoAbilityQueue(payload), config.pending_auto_ability_queue_event_code);
            }

            public MultiplayerTransportResult SendPendingAutoAbilityResolutionRequest(
                NetworkPendingAutoAbilityResolutionRequestPayload payload)
            {
                if (!client.InRoom)
                {
                    return Fail("PHOTON_NOT_IN_ROOM", "Cannot send pending auto ability resolution request before joining a room.");
                }

                return RaisePayload(
                    PhotonRealtimePayloadCodec.EncodePendingAutoAbilityResolutionRequest(payload),
                    config.pending_auto_ability_resolution_request_event_code);
            }

            public MultiplayerTransportResult SendPendingAutoAbilityManualResolutionDecision(
                NetworkPendingAutoAbilityManualResolutionDecisionPayload payload)
            {
                if (!client.InRoom)
                {
                    return Fail("PHOTON_NOT_IN_ROOM", "Cannot send pending auto ability manual resolution decision before joining a room.");
                }

                return RaisePayload(
                    PhotonRealtimePayloadCodec.EncodePendingAutoAbilityManualResolutionDecision(payload),
                    config.pending_auto_ability_manual_resolution_decision_event_code);
            }

            public MultiplayerTransportResult SendReconnectRequest(NetworkReconnectRequest request)
            {
                if (!client.InRoom)
                {
                    return Fail("PHOTON_NOT_IN_ROOM", "Cannot send reconnect request before joining a room.");
                }

                return RaisePayload(PhotonRealtimePayloadCodec.EncodeReconnectRequest(request), config.reconnect_request_event_code);
            }

            public MultiplayerTransportResult SendReconnectBatch(NetworkEventBatch batch)
            {
                if (!client.InRoom)
                {
                    return Fail("PHOTON_NOT_IN_ROOM", "Cannot send reconnect batch before joining a room.");
                }

                return RaisePayload(PhotonRealtimePayloadCodec.EncodeReconnectBatch(batch), config.reconnect_batch_event_code);
            }

            public MultiplayerTransportResult SendDeckRevealRequest(NetworkDeckRevealRequest request)
            {
                if (!client.InRoom)
                {
                    return Fail("PHOTON_NOT_IN_ROOM", "Cannot send deck reveal request before joining a room.");
                }

                return RaisePayload(PhotonRealtimePayloadCodec.EncodeDeckRevealRequest(request), config.deck_reveal_request_event_code);
            }

            public MultiplayerTransportResult SendDeckRevealResponse(NetworkDeckRevealResponse response)
            {
                if (!client.InRoom)
                {
                    return Fail("PHOTON_NOT_IN_ROOM", "Cannot send deck reveal response before joining a room.");
                }

                return RaisePayload(PhotonRealtimePayloadCodec.EncodeDeckRevealResponse(response), config.deck_reveal_response_event_code);
            }

            public MultiplayerTransportResult Fail(string errorCode, string message)
            {
                status = MultiplayerTransportStatus.Failed;
                lastError = errorCode;
                return MultiplayerTransportResult.Fail(errorCode, message);
            }

            public void Tick()
            {
                client.Service();
            }

            public void Disconnect()
            {
                pendingLocalPack = null;
                if (client.IsConnected)
                {
                    client.Disconnect();
                }

                status = MultiplayerTransportStatus.Disconnected;
                lastError = null;
            }

            public void OnConnected()
            {
            }

            public void OnConnectedToMaster()
            {
                status = MultiplayerTransportStatus.ConnectedToLobby;
            }

            public void OnDisconnected(DisconnectCause cause)
            {
                status = MultiplayerTransportStatus.Disconnected;
                lastError = cause.ToString();
            }

            public void OnRegionListReceived(RegionHandler regionHandler)
            {
            }

            public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
            {
            }

            public void OnCustomAuthenticationFailed(string debugMessage)
            {
                Fail("PHOTON_AUTH_FAILED", debugMessage);
            }

            public void OnFriendListUpdate(List<FriendInfo> friendList)
            {
            }

            public void OnCreatedRoom()
            {
            }

            public void OnCreateRoomFailed(short returnCode, string message)
            {
                Fail("PHOTON_CREATE_ROOM_FAILED", returnCode + ": " + message);
            }

            public void OnJoinedRoom()
            {
                status = MultiplayerTransportStatus.InRoom;
                MultiplayerRoomState room = TryReadCurrentRoomState();
                if (room != null)
                {
                    if (pendingLocalPack != null && !MultiplayerProtocol.PackMatches(room.pack, pendingLocalPack, false))
                    {
                        Fail("PACK_HASH_MISMATCH", "Joined room uses a different card pack.");
                        client.OpLeaveRoom(false);
                        return;
                    }

                    owner.RaiseRoomStateReceived(room);
                }
            }

            public void OnJoinRoomFailed(short returnCode, string message)
            {
                Fail("PHOTON_JOIN_ROOM_FAILED", returnCode + ": " + message);
            }

            public void OnJoinRandomFailed(short returnCode, string message)
            {
                Fail("PHOTON_JOIN_RANDOM_FAILED", returnCode + ": " + message);
            }

            public void OnLeftRoom()
            {
                status = client.IsConnected ? MultiplayerTransportStatus.ConnectedToLobby : MultiplayerTransportStatus.Disconnected;
            }

            public void OnPlayerEnteredRoom(Player newPlayer)
            {
            }

            public void OnPlayerLeftRoom(Player otherPlayer)
            {
            }

            public void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
            {
                MultiplayerRoomState room = TryReadRoomState(propertiesThatChanged);
                if (room != null)
                {
                    owner.RaiseRoomStateReceived(room);
                }
            }

            public void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
            {
            }

            public void OnMasterClientSwitched(Player newMasterClient)
            {
            }

            public void OnEvent(EventData photonEvent)
            {
                if (photonEvent == null || photonEvent.CustomData == null)
                {
                    return;
                }

                string payloadJson = photonEvent.CustomData as string;
                if (string.IsNullOrWhiteSpace(payloadJson))
                {
                    return;
                }

                PhotonRealtimePayload payload;
                try
                {
                    payload = PhotonRealtimePayload.FromJson(payloadJson);
                }
                catch (Exception exception)
                {
                    Fail("PHOTON_PAYLOAD_PARSE_FAILED", exception.Message);
                    return;
                }

                if (payload.event_code == config.room_state_event_code)
                {
                    MultiplayerRoomState room;
                    string rejectionReason;
                    if (PhotonRealtimePayloadCodec.TryDecodeRoomState(payload, out room, out rejectionReason))
                    {
                        owner.RaiseRoomStateReceived(room);
                    }
                    else
                    {
                        Fail("ROOM_STATE_REJECTED", rejectionReason);
                    }

                    return;
                }

                if (payload.event_code == config.game_event_code)
                {
                    NetworkEventEnvelope envelope;
                    string rejectionReason;
                    if (PhotonRealtimePayloadCodec.TryDecodeGameEvent(payload, out envelope, out rejectionReason))
                    {
                        owner.RaiseGameEventReceived(envelope);
                    }
                    else
                    {
                        Fail("GAME_EVENT_REJECTED", rejectionReason);
                    }

                    return;
                }

                if (payload.event_code == config.public_game_event_code)
                {
                    NetworkPublicGameEvent publicEvent;
                    string rejectionReason;
                    if (PhotonRealtimePayloadCodec.TryDecodePublicGameEvent(payload, out publicEvent, out rejectionReason))
                    {
                        owner.RaisePublicGameEventReceived(publicEvent);
                    }
                    else
                    {
                        Fail("PUBLIC_GAME_EVENT_REJECTED", rejectionReason);
                    }

                    return;
                }

                if (payload.event_code == config.trigger_check_replay_log_event_code)
                {
                    NetworkTriggerCheckReplayLogPayload triggerLogPayload;
                    string rejectionReason;
                    if (PhotonRealtimePayloadCodec.TryDecodeTriggerCheckReplayLog(payload, out triggerLogPayload, out rejectionReason))
                    {
                        owner.RaiseTriggerCheckReplayLogReceived(triggerLogPayload);
                    }
                    else
                    {
                        Fail("TRIGGER_CHECK_REPLAY_LOG_REJECTED", rejectionReason);
                    }

                    return;
                }

                if (payload.event_code == config.pending_auto_ability_queue_event_code)
                {
                    NetworkPendingAutoAbilityQueuePayload pendingQueuePayload;
                    string rejectionReason;
                    if (PhotonRealtimePayloadCodec.TryDecodePendingAutoAbilityQueue(payload, out pendingQueuePayload, out rejectionReason))
                    {
                        owner.RaisePendingAutoAbilityQueueReceived(pendingQueuePayload);
                    }
                    else
                    {
                        Fail("PENDING_AUTO_ABILITY_QUEUE_REJECTED", rejectionReason);
                    }

                    return;
                }

                if (payload.event_code == config.pending_auto_ability_resolution_request_event_code)
                {
                    NetworkPendingAutoAbilityResolutionRequestPayload requestPayload;
                    string rejectionReason;
                    if (PhotonRealtimePayloadCodec.TryDecodePendingAutoAbilityResolutionRequest(
                            payload,
                            out requestPayload,
                            out rejectionReason))
                    {
                        owner.RaisePendingAutoAbilityResolutionRequestReceived(requestPayload);
                    }
                    else
                    {
                        Fail("PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_REJECTED", rejectionReason);
                    }

                    return;
                }

                if (payload.event_code == config.pending_auto_ability_manual_resolution_decision_event_code)
                {
                    NetworkPendingAutoAbilityManualResolutionDecisionPayload decisionPayload;
                    string rejectionReason;
                    if (PhotonRealtimePayloadCodec.TryDecodePendingAutoAbilityManualResolutionDecision(
                            payload,
                            out decisionPayload,
                            out rejectionReason))
                    {
                        owner.RaisePendingAutoAbilityManualResolutionDecisionReceived(decisionPayload);
                    }
                    else
                    {
                        Fail("PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_REJECTED", rejectionReason);
                    }

                    return;
                }

                if (payload.event_code == config.deck_reveal_request_event_code)
                {
                    NetworkDeckRevealRequest request;
                    string rejectionReason;
                    if (PhotonRealtimePayloadCodec.TryDecodeDeckRevealRequest(payload, out request, out rejectionReason))
                    {
                        owner.RaiseDeckRevealRequestReceived(request);
                    }
                    else
                    {
                        Fail("DECK_REVEAL_REQUEST_REJECTED", rejectionReason);
                    }

                    return;
                }

                if (payload.event_code == config.deck_reveal_response_event_code)
                {
                    NetworkDeckRevealResponse response;
                    string rejectionReason;
                    if (PhotonRealtimePayloadCodec.TryDecodeDeckRevealResponse(payload, out response, out rejectionReason))
                    {
                        owner.RaiseDeckRevealResponseReceived(response);
                    }
                    else
                    {
                        Fail("DECK_REVEAL_RESPONSE_REJECTED", rejectionReason);
                    }

                    return;
                }

                if (payload.event_code == config.reconnect_request_event_code)
                {
                    NetworkReconnectRequest request;
                    string rejectionReason;
                    if (PhotonRealtimePayloadCodec.TryDecodeReconnectRequest(payload, out request, out rejectionReason))
                    {
                        owner.RaiseReconnectRequestReceived(request);
                    }
                    else
                    {
                        Fail("RECONNECT_REQUEST_REJECTED", rejectionReason);
                    }

                    return;
                }

                if (payload.event_code == config.reconnect_batch_event_code)
                {
                    NetworkEventBatch batch;
                    string rejectionReason;
                    if (PhotonRealtimePayloadCodec.TryDecodeReconnectBatch(payload, out batch, out rejectionReason))
                    {
                        owner.RaiseReconnectBatchReceived(batch);
                    }
                    else
                    {
                        Fail("RECONNECT_BATCH_REJECTED", rejectionReason);
                    }
                }
            }

            private MultiplayerTransportResult RaisePayload(PhotonRealtimePayload payload, byte eventCode)
            {
                payload.event_code = eventCode;
                RaiseEventArgs raiseOptions = new RaiseEventArgs
                {
                    Receivers = ReceiverGroup.Others
                };

                SendOptions sendOptions = config.send_reliable ? SendOptions.SendReliable : SendOptions.SendUnreliable;
                if (!client.OpRaiseEvent(eventCode, payload.ToJson(false), raiseOptions, sendOptions))
                {
                    return Fail("PHOTON_RAISE_EVENT_FAILED", "Photon refused the raise-event request.");
                }

                return MultiplayerTransportResult.Ok();
            }

            private MultiplayerRoomState TryReadCurrentRoomState()
            {
                if (client.CurrentRoom == null || client.CurrentRoom.CustomProperties == null)
                {
                    return null;
                }

                return TryReadRoomState(client.CurrentRoom.CustomProperties);
            }

            private MultiplayerRoomState TryReadRoomState(PhotonHashtable properties)
            {
                Dictionary<string, string> strings = FromPhotonHashtable(properties);
                if (strings.Count == 0)
                {
                    return null;
                }

                try
                {
                    return PhotonRealtimePayloadCodec.RoomFromProperties(strings);
                }
                catch (Exception exception)
                {
                    Fail("ROOM_STATE_PARSE_FAILED", exception.Message);
                    return null;
                }
            }

            private static PhotonHashtable ToPhotonHashtable(IDictionary<string, string> properties)
            {
                PhotonHashtable hashtable = new PhotonHashtable();
                if (properties == null)
                {
                    return hashtable;
                }

                foreach (KeyValuePair<string, string> pair in properties)
                {
                    hashtable[pair.Key] = pair.Value ?? "";
                }

                return hashtable;
            }

            private static Dictionary<string, string> FromPhotonHashtable(PhotonHashtable hashtable)
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                if (hashtable == null)
                {
                    return values;
                }

                foreach (object key in hashtable.Keys)
                {
                    string stringKey = key as string;
                    if (string.IsNullOrEmpty(stringKey))
                    {
                        continue;
                    }

                    object raw = hashtable[key];
                    values[stringKey] = raw == null ? "" : raw.ToString();
                }

                return values;
            }
        }
#endif
    }
}
