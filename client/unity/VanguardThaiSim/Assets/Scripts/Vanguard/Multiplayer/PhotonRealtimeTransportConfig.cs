using System;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class PhotonRealtimeTransportConfig
    {
        public string app_id;
        public string app_version = "0.1.0";
        public string fixed_region = "asia";
        public byte room_state_event_code = PhotonRealtimePayloadCodec.RoomStateEventCode;
        public byte game_event_code = PhotonRealtimePayloadCodec.GameEventCode;
        public byte public_game_event_code = PhotonRealtimePayloadCodec.PublicGameEventCode;
        public byte trigger_check_replay_log_event_code = PhotonRealtimePayloadCodec.TriggerCheckReplayLogEventCode;
        public byte pending_auto_ability_queue_event_code = PhotonRealtimePayloadCodec.PendingAutoAbilityQueueEventCode;
        public byte pending_auto_ability_resolution_request_event_code =
            PhotonRealtimePayloadCodec.PendingAutoAbilityResolutionRequestEventCode;
        public byte pending_auto_ability_manual_resolution_decision_event_code =
            PhotonRealtimePayloadCodec.PendingAutoAbilityManualResolutionDecisionEventCode;
        public byte reconnect_request_event_code = PhotonRealtimePayloadCodec.ReconnectRequestEventCode;
        public byte reconnect_batch_event_code = PhotonRealtimePayloadCodec.ReconnectBatchEventCode;
        public byte deck_reveal_request_event_code = PhotonRealtimePayloadCodec.DeckRevealRequestEventCode;
        public byte deck_reveal_response_event_code = PhotonRealtimePayloadCodec.DeckRevealResponseEventCode;
        public bool send_reliable = true;

        public bool IsConfigured
        {
            get { return !string.IsNullOrWhiteSpace(app_id); }
        }
    }
}
