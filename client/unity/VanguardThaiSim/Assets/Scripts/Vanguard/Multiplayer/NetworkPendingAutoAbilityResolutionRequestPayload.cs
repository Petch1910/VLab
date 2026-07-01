using System;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class NetworkPendingAutoAbilityResolutionRequestPayload
    {
        public int protocol_version = MultiplayerProtocol.ProtocolVersion;
        public string payload_id;
        public string room_id;
        public int sender_player_index;
        public int selected_index = -1;
        public string pending_id;
        public string perspective = GameStateViewPerspective.Spectator.ToString();
        public int viewer_player_index = -1;
        public string pending_auto_ability_resolution_request_json;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static NetworkPendingAutoAbilityResolutionRequestPayload FromJson(string json)
        {
            NetworkPendingAutoAbilityResolutionRequestPayload payload =
                JsonUtility.FromJson<NetworkPendingAutoAbilityResolutionRequestPayload>(json);
            if (payload == null)
            {
                throw new ArgumentException(
                    "Pending auto ability resolution request payload JSON could not be parsed.",
                    "json");
            }

            if (string.IsNullOrEmpty(payload.perspective))
            {
                payload.perspective = GameStateViewPerspective.Spectator.ToString();
            }

            return payload;
        }
    }

    public static class PendingAutoAbilityResolutionRequestPayloadCodec
    {
        public static NetworkPendingAutoAbilityResolutionRequestPayload Encode(
            string roomId,
            int senderPlayerIndex,
            PendingAutoAbilityResolutionRequest request,
            GameStateViewPerspective perspective,
            int viewerPlayerIndex = -1)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            PendingAutoAbilityResolutionRequest safeRequest = CloneAndSanitizeRequest(request);
            string safeRoomId = roomId ?? string.Empty;
            string safePendingId = safeRequest.pending_id ?? string.Empty;
            string safePerspective = perspective.ToString();

            return new NetworkPendingAutoAbilityResolutionRequestPayload
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                payload_id = BuildPayloadId(
                    safeRoomId,
                    senderPlayerIndex,
                    safeRequest.selected_index,
                    safePendingId,
                    safePerspective,
                    viewerPlayerIndex),
                room_id = safeRoomId,
                sender_player_index = senderPlayerIndex,
                selected_index = safeRequest.selected_index,
                pending_id = safePendingId,
                perspective = safePerspective,
                viewer_player_index = viewerPlayerIndex,
                pending_auto_ability_resolution_request_json = safeRequest.ToJson(false)
            };
        }

        public static bool TryDecode(
            NetworkPendingAutoAbilityResolutionRequestPayload payload,
            out PendingAutoAbilityResolutionRequest request,
            out string rejectionReason)
        {
            request = null;
            rejectionReason = null;
            if (payload == null ||
                string.IsNullOrWhiteSpace(payload.pending_auto_ability_resolution_request_json))
            {
                rejectionReason = "PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_PAYLOAD_INVALID";
                return false;
            }

            if (payload.protocol_version != MultiplayerProtocol.ProtocolVersion)
            {
                rejectionReason = "PROTOCOL_VERSION_MISMATCH";
                return false;
            }

            try
            {
                request = PendingAutoAbilityResolutionRequest.FromJson(
                    payload.pending_auto_ability_resolution_request_json);
                return true;
            }
            catch (Exception exception)
            {
                rejectionReason = "PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_PARSE_FAILED: " + exception.Message;
                return false;
            }
        }

        private static PendingAutoAbilityResolutionRequest CloneAndSanitizeRequest(
            PendingAutoAbilityResolutionRequest source)
        {
            bool hidesSource =
                source.hides_source_card_identity ||
                string.Equals(source.source_card_id, GameStateViewFactory.HiddenCardId, StringComparison.Ordinal);

            return new PendingAutoAbilityResolutionRequest
            {
                selected_index = source.selected_index,
                pending_id = source.pending_id ?? string.Empty,
                player_index = source.player_index,
                timing_event = source.timing_event ?? string.Empty,
                source_card_instance_id = hidesSource ? string.Empty : source.source_card_instance_id ?? string.Empty,
                source_card_id = hidesSource
                    ? GameStateViewFactory.HiddenCardId
                    : source.source_card_id ?? string.Empty,
                hides_source_card_identity = hidesSource,
                summary = source.summary ?? string.Empty
            };
        }

        private static string BuildPayloadId(
            string roomId,
            int senderPlayerIndex,
            int selectedIndex,
            string pendingId,
            string perspective,
            int viewerPlayerIndex)
        {
            return "pending-auto-resolution-request-payload|" +
                   roomId +
                   "|" + senderPlayerIndex +
                   "|" + selectedIndex +
                   "|" + pendingId +
                   "|" + perspective +
                   "|" + viewerPlayerIndex;
        }
    }
}
