using System;
using System.Collections.Generic;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class NetworkPendingAutoAbilityQueuePayload
    {
        public int protocol_version = MultiplayerProtocol.ProtocolVersion;
        public string payload_id;
        public string room_id;
        public int sender_player_index;
        public string source_queue_id;
        public int pending_count;
        public string perspective = GameStateViewPerspective.Spectator.ToString();
        public int viewer_player_index = -1;
        public string pending_auto_ability_queue_json;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static NetworkPendingAutoAbilityQueuePayload FromJson(string json)
        {
            NetworkPendingAutoAbilityQueuePayload payload =
                JsonUtility.FromJson<NetworkPendingAutoAbilityQueuePayload>(json);
            if (payload == null)
            {
                throw new ArgumentException("Pending auto ability queue payload JSON could not be parsed.", "json");
            }

            if (string.IsNullOrEmpty(payload.perspective))
            {
                payload.perspective = GameStateViewPerspective.Spectator.ToString();
            }

            return payload;
        }
    }

    public static class PendingAutoAbilityQueuePayloadCodec
    {
        public static NetworkPendingAutoAbilityQueuePayload Encode(
            string roomId,
            int senderPlayerIndex,
            PendingAutoAbilityQueue maskedQueue,
            GameStateViewPerspective perspective,
            int viewerPlayerIndex = -1)
        {
            if (maskedQueue == null)
            {
                throw new ArgumentNullException("maskedQueue");
            }

            PendingAutoAbilityQueue safeQueue = CloneQueue(maskedQueue);
            string safeRoomId = roomId ?? string.Empty;
            string safeQueueId = safeQueue.queue_id ?? string.Empty;
            string safePerspective = perspective.ToString();

            return new NetworkPendingAutoAbilityQueuePayload
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                payload_id = BuildPayloadId(
                    safeRoomId,
                    senderPlayerIndex,
                    safeQueueId,
                    safePerspective,
                    viewerPlayerIndex,
                    safeQueue.pending.Count),
                room_id = safeRoomId,
                sender_player_index = senderPlayerIndex,
                source_queue_id = safeQueueId,
                pending_count = safeQueue.pending.Count,
                perspective = safePerspective,
                viewer_player_index = viewerPlayerIndex,
                pending_auto_ability_queue_json = safeQueue.ToJson(false)
            };
        }

        public static bool TryDecode(
            NetworkPendingAutoAbilityQueuePayload payload,
            out PendingAutoAbilityQueue queue,
            out string rejectionReason)
        {
            queue = null;
            rejectionReason = null;
            if (payload == null || string.IsNullOrWhiteSpace(payload.pending_auto_ability_queue_json))
            {
                rejectionReason = "PENDING_AUTO_ABILITY_QUEUE_PAYLOAD_INVALID";
                return false;
            }

            if (payload.protocol_version != MultiplayerProtocol.ProtocolVersion)
            {
                rejectionReason = "PROTOCOL_VERSION_MISMATCH";
                return false;
            }

            try
            {
                queue = PendingAutoAbilityQueue.FromJson(payload.pending_auto_ability_queue_json);
                return true;
            }
            catch (Exception exception)
            {
                rejectionReason = "PENDING_AUTO_ABILITY_QUEUE_PARSE_FAILED: " + exception.Message;
                return false;
            }
        }

        private static PendingAutoAbilityQueue CloneQueue(PendingAutoAbilityQueue source)
        {
            var clone = new PendingAutoAbilityQueue
            {
                queue_id = source == null || string.IsNullOrEmpty(source.queue_id)
                    ? "pending-auto-ability-queue"
                    : source.queue_id,
                pending = new List<PendingAutoAbility>()
            };

            if (source == null || source.pending == null)
            {
                return clone;
            }

            for (int i = 0; i < source.pending.Count; i++)
            {
                PendingAutoAbility ability = source.pending[i];
                if (ability == null)
                {
                    continue;
                }

                clone.pending.Add(new PendingAutoAbility
                {
                    pending_id = ability.pending_id,
                    source_card_instance_id = ability.source_card_instance_id,
                    source_card_id = ability.source_card_id,
                    player_index = ability.player_index,
                    timing_event = ability.timing_event,
                    summary = ability.summary,
                    hides_source_card_identity = ability.hides_source_card_identity
                });
            }

            return clone;
        }

        private static string BuildPayloadId(
            string roomId,
            int senderPlayerIndex,
            string sourceQueueId,
            string perspective,
            int viewerPlayerIndex,
            int pendingCount)
        {
            return "pending-auto-queue-payload|" +
                   roomId +
                   "|" + senderPlayerIndex +
                   "|" + sourceQueueId +
                   "|" + perspective +
                   "|" + viewerPlayerIndex +
                   "|" + pendingCount;
        }
    }
}
