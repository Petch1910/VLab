using System;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class NetworkPendingAutoAbilityManualResolutionDecisionPayload
    {
        public int protocol_version = MultiplayerProtocol.ProtocolVersion;
        public string payload_id;
        public string room_id;
        public int sender_player_index;
        public string decision_id;
        public string decision_type;
        public int selected_index = -1;
        public string pending_id;
        public string perspective = GameStateViewPerspective.Spectator.ToString();
        public int viewer_player_index = -1;
        public string pending_auto_ability_manual_resolution_decision_json;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static NetworkPendingAutoAbilityManualResolutionDecisionPayload FromJson(string json)
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                JsonUtility.FromJson<NetworkPendingAutoAbilityManualResolutionDecisionPayload>(json);
            if (payload == null)
            {
                throw new ArgumentException(
                    "Pending auto ability manual resolution decision payload JSON could not be parsed.",
                    "json");
            }

            if (string.IsNullOrEmpty(payload.perspective))
            {
                payload.perspective = GameStateViewPerspective.Spectator.ToString();
            }

            return payload;
        }
    }

    public static class PendingAutoAbilityManualResolutionDecisionPayloadCodec
    {
        public static NetworkPendingAutoAbilityManualResolutionDecisionPayload Encode(
            string roomId,
            int senderPlayerIndex,
            PendingAutoAbilityManualResolutionDecision decision,
            GameStateViewPerspective perspective,
            int viewerPlayerIndex = -1)
        {
            if (decision == null)
            {
                throw new ArgumentNullException("decision");
            }

            PendingAutoAbilityManualResolutionDecision safeDecision = CloneAndSanitizeDecision(decision);
            string safeRoomId = roomId ?? string.Empty;
            string safeDecisionId = safeDecision.decision_id ?? string.Empty;
            string safeDecisionType = safeDecision.decision_type ?? string.Empty;
            string safePendingId = safeDecision.pending_id ?? string.Empty;
            string safePerspective = perspective.ToString();

            return new NetworkPendingAutoAbilityManualResolutionDecisionPayload
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                payload_id = BuildPayloadId(
                    safeRoomId,
                    senderPlayerIndex,
                    safeDecisionId,
                    safeDecisionType,
                    safePerspective,
                    viewerPlayerIndex),
                room_id = safeRoomId,
                sender_player_index = senderPlayerIndex,
                decision_id = safeDecisionId,
                decision_type = safeDecisionType,
                selected_index = safeDecision.selected_index,
                pending_id = safePendingId,
                perspective = safePerspective,
                viewer_player_index = viewerPlayerIndex,
                pending_auto_ability_manual_resolution_decision_json = safeDecision.ToJson(false)
            };
        }

        public static bool TryDecode(
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload,
            out PendingAutoAbilityManualResolutionDecision decision,
            out string rejectionReason)
        {
            decision = null;
            rejectionReason = null;
            if (payload == null ||
                string.IsNullOrWhiteSpace(payload.pending_auto_ability_manual_resolution_decision_json))
            {
                rejectionReason = "PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_PAYLOAD_INVALID";
                return false;
            }

            if (payload.protocol_version != MultiplayerProtocol.ProtocolVersion)
            {
                rejectionReason = "PROTOCOL_VERSION_MISMATCH";
                return false;
            }

            try
            {
                decision = PendingAutoAbilityManualResolutionDecision.FromJson(
                    payload.pending_auto_ability_manual_resolution_decision_json);
                return true;
            }
            catch (Exception exception)
            {
                rejectionReason =
                    "PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_PARSE_FAILED: " + exception.Message;
                return false;
            }
        }

        private static PendingAutoAbilityManualResolutionDecision CloneAndSanitizeDecision(
            PendingAutoAbilityManualResolutionDecision source)
        {
            bool hidesSource =
                source.hides_source_card_identity ||
                string.Equals(source.source_card_id, GameStateViewFactory.HiddenCardId, StringComparison.Ordinal);

            return new PendingAutoAbilityManualResolutionDecision
            {
                decision_id = source.decision_id ?? string.Empty,
                decision_type = source.decision_type ?? string.Empty,
                selected_index = source.selected_index,
                pending_id = source.pending_id ?? string.Empty,
                player_index = source.player_index,
                timing_event = source.timing_event ?? string.Empty,
                source_card_instance_id = hidesSource ? string.Empty : source.source_card_instance_id ?? string.Empty,
                source_card_id = hidesSource
                    ? GameStateViewFactory.HiddenCardId
                    : source.source_card_id ?? string.Empty,
                hides_source_card_identity = hidesSource,
                reason = source.reason ?? string.Empty,
                summary = source.summary ?? string.Empty
            };
        }

        private static string BuildPayloadId(
            string roomId,
            int senderPlayerIndex,
            string decisionId,
            string decisionType,
            string perspective,
            int viewerPlayerIndex)
        {
            return "pending-auto-manual-decision-payload|" +
                   roomId +
                   "|" + senderPlayerIndex +
                   "|" + decisionId +
                   "|" + decisionType +
                   "|" + perspective +
                   "|" + viewerPlayerIndex;
        }
    }
}
