using System;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class ManualTriggerCheckDraftRequest
    {
        public string room_id;
        public string sender_player_id;
        public int player_index;
        public TriggerCheckSource check_source = TriggerCheckSource.Manual;
        public int check_index;
        public string checked_card_instance_id;
        public string checked_card_id;
        public TriggerType trigger_type = TriggerType.Unknown;
        public CombatModifierExpiration modifier_expiration = CombatModifierExpiration.EndOfTurn;
        public GameStateViewPerspective perspective = GameStateViewPerspective.Spectator;
        public int viewer_player_index = -1;
    }

    [Serializable]
    public sealed class ManualTriggerCheckDraftResult
    {
        public bool accepted;
        public bool sent;
        public string rejection_reason;
        public string summary;
        public string transport_error_code;
        public string transport_message;
        public NetworkTriggerCheckReplayLogPayload payload;
    }

    public static class ManualTriggerCheckDraftPayloadBuilder
    {
        public static ManualTriggerCheckDraftResult Build(
            GameState state,
            ManualTriggerCheckDraftRequest request)
        {
            string rejectionReason = Validate(state, request);
            if (!string.IsNullOrEmpty(rejectionReason))
            {
                return Reject(rejectionReason);
            }

            try
            {
                TriggerCheckResolutionBundle bundle = TriggerCheckResolutionBundler.Build(
                    state,
                    request.player_index,
                    request.check_source,
                    request.check_index,
                    request.checked_card_instance_id,
                    request.checked_card_id,
                    request.trigger_type,
                    request.modifier_expiration);
                TriggerCheckLogEntry entry = TriggerCheckLogEntryFactory.FromBundle(bundle);
                TriggerCheckReplayLog log = TriggerCheckReplayLogBuilder.Append(null, entry);
                TriggerCheckReplayLog maskedLog = TriggerCheckReplayLogMasker.CreateView(
                    log,
                    request.perspective,
                    request.viewer_player_index);
                NetworkTriggerCheckReplayLogPayload payload = TriggerCheckReplayLogPayloadCodec.Encode(
                    request.room_id,
                    request.sender_player_id,
                    maskedLog,
                    request.perspective,
                    request.viewer_player_index);

                return new ManualTriggerCheckDraftResult
                {
                    accepted = true,
                    sent = false,
                    rejection_reason = string.Empty,
                    summary = entry.summary ?? string.Empty,
                    transport_error_code = string.Empty,
                    transport_message = string.Empty,
                    payload = payload
                };
            }
            catch (ArgumentOutOfRangeException)
            {
                return Reject("PLAYER_INDEX_INVALID");
            }
        }

        private static string Validate(GameState state, ManualTriggerCheckDraftRequest request)
        {
            if (state == null)
            {
                return "GAME_STATE_MISSING";
            }

            if (request == null)
            {
                return "TRIGGER_CHECK_DRAFT_REQUEST_MISSING";
            }

            if (string.IsNullOrWhiteSpace(request.room_id))
            {
                return "ROOM_ID_MISSING";
            }

            if (string.IsNullOrWhiteSpace(request.sender_player_id))
            {
                return "SENDER_PLAYER_ID_MISSING";
            }

            if (request.player_index < 0)
            {
                return "PLAYER_INDEX_INVALID";
            }

            if (string.IsNullOrWhiteSpace(request.checked_card_id))
            {
                return "CHECKED_CARD_ID_MISSING";
            }

            return string.Empty;
        }

        private static ManualTriggerCheckDraftResult Reject(string rejectionReason)
        {
            return new ManualTriggerCheckDraftResult
            {
                accepted = false,
                sent = false,
                rejection_reason = rejectionReason ?? string.Empty,
                summary = string.Empty,
                transport_error_code = string.Empty,
                transport_message = string.Empty,
                payload = null
            };
        }
    }
}
