using System;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class NetworkTriggerCheckReplayLogPayload
    {
        public int protocol_version = MultiplayerProtocol.ProtocolVersion;
        public string payload_id;
        public string room_id;
        public string sender_player_id;
        public string source_log_id;
        public string perspective = GameStateViewPerspective.Spectator.ToString();
        public int viewer_player_index = -1;
        public string trigger_check_log_json;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static NetworkTriggerCheckReplayLogPayload FromJson(string json)
        {
            NetworkTriggerCheckReplayLogPayload payload =
                JsonUtility.FromJson<NetworkTriggerCheckReplayLogPayload>(json);
            if (payload == null)
            {
                throw new ArgumentException("Trigger check replay log payload JSON could not be parsed.", "json");
            }

            if (string.IsNullOrEmpty(payload.perspective))
            {
                payload.perspective = GameStateViewPerspective.Spectator.ToString();
            }

            return payload;
        }
    }

    public static class TriggerCheckReplayLogPayloadCodec
    {
        public static NetworkTriggerCheckReplayLogPayload Encode(
            string roomId,
            string senderPlayerId,
            TriggerCheckReplayLog maskedLog,
            GameStateViewPerspective perspective,
            int viewerPlayerIndex = -1)
        {
            if (maskedLog == null)
            {
                throw new ArgumentNullException("maskedLog");
            }

            maskedLog.EnsureLists();
            string sourceLogId = maskedLog.log_id ?? string.Empty;
            string safeRoomId = roomId ?? string.Empty;
            string safeSender = senderPlayerId ?? string.Empty;
            string safePerspective = perspective.ToString();

            return new NetworkTriggerCheckReplayLogPayload
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                payload_id = BuildPayloadId(safeRoomId, safeSender, sourceLogId, safePerspective, viewerPlayerIndex),
                room_id = safeRoomId,
                sender_player_id = safeSender,
                source_log_id = sourceLogId,
                perspective = safePerspective,
                viewer_player_index = viewerPlayerIndex,
                trigger_check_log_json = maskedLog.ToJson(false)
            };
        }

        public static bool TryDecode(
            NetworkTriggerCheckReplayLogPayload payload,
            out TriggerCheckReplayLog log,
            out string rejectionReason)
        {
            log = null;
            rejectionReason = null;
            if (payload == null || string.IsNullOrWhiteSpace(payload.trigger_check_log_json))
            {
                rejectionReason = "TRIGGER_CHECK_REPLAY_LOG_PAYLOAD_INVALID";
                return false;
            }

            if (payload.protocol_version != MultiplayerProtocol.ProtocolVersion)
            {
                rejectionReason = "PROTOCOL_VERSION_MISMATCH";
                return false;
            }

            try
            {
                log = TriggerCheckReplayLog.FromJson(payload.trigger_check_log_json);
                return true;
            }
            catch (Exception exception)
            {
                rejectionReason = "TRIGGER_CHECK_REPLAY_LOG_PARSE_FAILED: " + exception.Message;
                return false;
            }
        }

        private static string BuildPayloadId(
            string roomId,
            string senderPlayerId,
            string sourceLogId,
            string perspective,
            int viewerPlayerIndex)
        {
            return "trigger-check-log-payload|" +
                   roomId +
                   "|" + senderPlayerId +
                   "|" + sourceLogId +
                   "|" + perspective +
                   "|" + viewerPlayerIndex;
        }
    }
}
