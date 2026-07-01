using System;

namespace VanguardThaiSim.Multiplayer
{
    public interface IMultiplayerTransport
    {
        string TransportName { get; }
        MultiplayerTransportStatus Status { get; }
        string LastError { get; }

        event Action<MultiplayerRoomState> RoomStateReceived;
        event Action<NetworkEventEnvelope> GameEventReceived;
        event Action<NetworkPublicGameEvent> PublicGameEventReceived;
        event Action<NetworkTriggerCheckReplayLogPayload> TriggerCheckReplayLogReceived;
        event Action<NetworkPendingAutoAbilityQueuePayload> PendingAutoAbilityQueueReceived;
        event Action<NetworkPendingAutoAbilityResolutionRequestPayload> PendingAutoAbilityResolutionRequestReceived;
        event Action<NetworkPendingAutoAbilityManualResolutionDecisionPayload> PendingAutoAbilityManualResolutionDecisionReceived;
        event Action<NetworkReconnectRequest> ReconnectRequestReceived;
        event Action<NetworkEventBatch> ReconnectBatchReceived;
        event Action<NetworkDeckRevealRequest> DeckRevealRequestReceived;
        event Action<NetworkDeckRevealResponse> DeckRevealResponseReceived;

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
        void Tick();
        void Disconnect();
    }
}
