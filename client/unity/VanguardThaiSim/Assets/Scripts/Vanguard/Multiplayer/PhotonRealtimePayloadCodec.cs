using System;
using System.Collections.Generic;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    public static class PhotonRealtimePayloadCodec
    {
        public const byte RoomStateEventCode = 1;
        public const byte GameEventCode = 2;
        public const byte ReconnectRequestEventCode = 3;
        public const byte ReconnectBatchEventCode = 4;
        public const byte PublicGameEventCode = 5;
        public const byte DeckRevealRequestEventCode = 6;
        public const byte DeckRevealResponseEventCode = 7;
        public const byte TriggerCheckReplayLogEventCode = 8;
        public const byte PendingAutoAbilityQueueEventCode = 9;
        public const byte PendingAutoAbilityResolutionRequestEventCode = 10;
        public const byte PendingAutoAbilityManualResolutionDecisionEventCode = 11;
        public const byte CommandEnvelopeEventCode = 12;
        public const byte PublicEventBatchEventCode = 13;

        public const string RoomJsonProperty = "vg_room_json";
        public const string ProtocolVersionProperty = "vg_protocol";
        public const string FormatProperty = "vg_format";
        public const string StateProperty = "vg_state";
        public const string HostPlayerIdProperty = "vg_host";
        public const string RandomSeedProperty = "vg_seed";
        public const string PackIdProperty = "vg_pack_id";
        public const string PackSourceVersionProperty = "vg_pack_version";
        public const string PackDefinitionHashProperty = "vg_pack_def";
        public const string PackImageManifestHashProperty = "vg_pack_img_manifest";
        public const string PackImageContentHashProperty = "vg_pack_img_content";

        public static Dictionary<string, string> RoomToProperties(MultiplayerRoomState room)
        {
            if (room == null)
            {
                throw new ArgumentNullException(nameof(room));
            }

            room.EnsureLists();
            Dictionary<string, string> properties = new Dictionary<string, string>();
            Set(properties, RoomJsonProperty, room.ToJson(false));
            Set(properties, ProtocolVersionProperty, room.protocol_version.ToString());
            Set(properties, FormatProperty, room.format);
            Set(properties, StateProperty, room.state);
            Set(properties, HostPlayerIdProperty, room.host_player_id);
            Set(properties, RandomSeedProperty, room.random_seed.ToString());

            if (room.pack != null)
            {
                Set(properties, PackIdProperty, room.pack.pack_id);
                Set(properties, PackSourceVersionProperty, room.pack.source_version);
                Set(properties, PackDefinitionHashProperty, room.pack.definition_hash);
                Set(properties, PackImageManifestHashProperty, room.pack.image_manifest_hash);
                Set(properties, PackImageContentHashProperty, room.pack.image_content_hash);
            }

            return properties;
        }

        public static MultiplayerRoomState RoomFromProperties(IDictionary<string, string> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            string roomJson;
            if (TryGet(properties, RoomJsonProperty, out roomJson) && !string.IsNullOrWhiteSpace(roomJson))
            {
                return MultiplayerRoomState.FromJson(roomJson);
            }

            MultiplayerRoomState room = new MultiplayerRoomState
            {
                protocol_version = ParseInt(Get(properties, ProtocolVersionProperty), MultiplayerProtocol.ProtocolVersion),
                format = Get(properties, FormatProperty),
                state = Get(properties, StateProperty),
                host_player_id = Get(properties, HostPlayerIdProperty),
                random_seed = ParseInt(Get(properties, RandomSeedProperty), 0),
                pack = new PackSyncInfo
                {
                    pack_id = Get(properties, PackIdProperty),
                    source_version = Get(properties, PackSourceVersionProperty),
                    definition_hash = Get(properties, PackDefinitionHashProperty),
                    image_manifest_hash = Get(properties, PackImageManifestHashProperty),
                    image_content_hash = Get(properties, PackImageContentHashProperty)
                }
            };
            room.EnsureLists();
            return room;
        }

        public static PhotonRealtimePayload EncodeRoomState(MultiplayerRoomState room, string senderPlayerId = null)
        {
            if (room == null)
            {
                throw new ArgumentNullException(nameof(room));
            }

            return new PhotonRealtimePayload
            {
                event_code = RoomStateEventCode,
                sender_player_id = senderPlayerId,
                json = room.ToJson(false)
            };
        }

        public static bool TryDecodeRoomState(PhotonRealtimePayload payload, out MultiplayerRoomState room, out string rejectionReason)
        {
            room = null;
            rejectionReason = null;
            if (payload == null || payload.event_code != RoomStateEventCode || string.IsNullOrWhiteSpace(payload.json))
            {
                rejectionReason = "ROOM_STATE_PAYLOAD_INVALID";
                return false;
            }

            try
            {
                room = MultiplayerRoomState.FromJson(payload.json);
                return true;
            }
            catch (Exception exception)
            {
                rejectionReason = "ROOM_STATE_PARSE_FAILED: " + exception.Message;
                return false;
            }
        }

        public static PhotonRealtimePayload EncodeGameEvent(NetworkEventEnvelope envelope)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }

            return new PhotonRealtimePayload
            {
                event_code = GameEventCode,
                sender_player_id = envelope.player_id,
                json = envelope.ToJson(false)
            };
        }

        public static bool TryDecodeGameEvent(PhotonRealtimePayload payload, out NetworkEventEnvelope envelope, out string rejectionReason)
        {
            envelope = null;
            rejectionReason = null;
            if (payload == null || payload.event_code != GameEventCode || string.IsNullOrWhiteSpace(payload.json))
            {
                rejectionReason = "GAME_EVENT_PAYLOAD_INVALID";
                return false;
            }

            try
            {
                envelope = NetworkEventEnvelope.FromJson(payload.json);
                return true;
            }
            catch (Exception exception)
            {
                rejectionReason = "GAME_EVENT_PARSE_FAILED: " + exception.Message;
                return false;
            }
        }

        public static PhotonRealtimePayload EncodeCommandEnvelope(NetworkCommandEnvelope envelope)
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }

            return new PhotonRealtimePayload
            {
                event_code = CommandEnvelopeEventCode,
                sender_player_id = envelope.player_id,
                json = envelope.ToJson(false)
            };
        }

        public static bool TryDecodeCommandEnvelope(
            PhotonRealtimePayload payload,
            out NetworkCommandEnvelope envelope,
            out string rejectionReason)
        {
            envelope = null;
            rejectionReason = null;
            if (payload == null ||
                payload.event_code != CommandEnvelopeEventCode ||
                string.IsNullOrWhiteSpace(payload.json))
            {
                rejectionReason = "COMMAND_ENVELOPE_PAYLOAD_INVALID";
                return false;
            }

            try
            {
                envelope = NetworkCommandEnvelope.FromJson(payload.json);
                if (envelope.protocol_version != MultiplayerProtocol.ProtocolVersion)
                {
                    rejectionReason = "PROTOCOL_VERSION_MISMATCH";
                    envelope = null;
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                rejectionReason = "COMMAND_ENVELOPE_PARSE_FAILED: " + exception.Message;
                return false;
            }
        }

        public static PhotonRealtimePayload EncodePublicGameEvent(NetworkPublicGameEvent publicEvent, string senderPlayerId = null)
        {
            if (publicEvent == null)
            {
                throw new ArgumentNullException(nameof(publicEvent));
            }

            return new PhotonRealtimePayload
            {
                event_code = PublicGameEventCode,
                sender_player_id = senderPlayerId,
                json = publicEvent.ToJson(false)
            };
        }

        public static bool TryDecodePublicGameEvent(PhotonRealtimePayload payload, out NetworkPublicGameEvent publicEvent, out string rejectionReason)
        {
            publicEvent = null;
            rejectionReason = null;
            if (payload == null || payload.event_code != PublicGameEventCode || string.IsNullOrWhiteSpace(payload.json))
            {
                rejectionReason = "PUBLIC_GAME_EVENT_PAYLOAD_INVALID";
                return false;
            }

            try
            {
                publicEvent = NetworkPublicGameEvent.FromJson(payload.json);
                if (publicEvent.protocol_version != MultiplayerProtocol.ProtocolVersion)
                {
                    rejectionReason = "PROTOCOL_VERSION_MISMATCH";
                    publicEvent = null;
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                rejectionReason = "PUBLIC_GAME_EVENT_PARSE_FAILED: " + exception.Message;
                return false;
            }
        }

        public static PhotonRealtimePayload EncodePublicEventBatch(NetworkPublicEventBatch batch, string senderPlayerId = null)
        {
            if (batch == null)
            {
                throw new ArgumentNullException(nameof(batch));
            }

            return new PhotonRealtimePayload
            {
                event_code = PublicEventBatchEventCode,
                sender_player_id = senderPlayerId,
                json = batch.ToJson(false)
            };
        }

        public static bool TryDecodePublicEventBatch(
            PhotonRealtimePayload payload,
            out NetworkPublicEventBatch batch,
            out string rejectionReason)
        {
            batch = null;
            rejectionReason = null;
            if (payload == null ||
                payload.event_code != PublicEventBatchEventCode ||
                string.IsNullOrWhiteSpace(payload.json))
            {
                rejectionReason = "PUBLIC_EVENT_BATCH_PAYLOAD_INVALID";
                return false;
            }

            try
            {
                batch = NetworkPublicEventBatch.FromJson(payload.json);
                if (batch.protocol_version != MultiplayerProtocol.ProtocolVersion)
                {
                    rejectionReason = "PROTOCOL_VERSION_MISMATCH";
                    batch = null;
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                rejectionReason = "PUBLIC_EVENT_BATCH_PARSE_FAILED: " + exception.Message;
                return false;
            }
        }

        public static PhotonRealtimePayload EncodeTriggerCheckReplayLog(NetworkTriggerCheckReplayLogPayload triggerLogPayload)
        {
            if (triggerLogPayload == null)
            {
                throw new ArgumentNullException(nameof(triggerLogPayload));
            }

            return new PhotonRealtimePayload
            {
                event_code = TriggerCheckReplayLogEventCode,
                sender_player_id = triggerLogPayload.sender_player_id,
                json = triggerLogPayload.ToJson(false)
            };
        }

        public static bool TryDecodeTriggerCheckReplayLog(
            PhotonRealtimePayload payload,
            out NetworkTriggerCheckReplayLogPayload triggerLogPayload,
            out string rejectionReason)
        {
            triggerLogPayload = null;
            rejectionReason = null;
            if (payload == null || payload.event_code != TriggerCheckReplayLogEventCode || string.IsNullOrWhiteSpace(payload.json))
            {
                rejectionReason = "TRIGGER_CHECK_REPLAY_LOG_PAYLOAD_INVALID";
                return false;
            }

            try
            {
                triggerLogPayload = NetworkTriggerCheckReplayLogPayload.FromJson(payload.json);
                TriggerCheckReplayLog decodedLog;
                if (!TriggerCheckReplayLogPayloadCodec.TryDecode(triggerLogPayload, out decodedLog, out rejectionReason))
                {
                    triggerLogPayload = null;
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                rejectionReason = "TRIGGER_CHECK_REPLAY_LOG_PAYLOAD_PARSE_FAILED: " + exception.Message;
                return false;
            }
        }

        public static PhotonRealtimePayload EncodePendingAutoAbilityQueue(
            NetworkPendingAutoAbilityQueuePayload pendingQueuePayload)
        {
            if (pendingQueuePayload == null)
            {
                throw new ArgumentNullException(nameof(pendingQueuePayload));
            }

            return new PhotonRealtimePayload
            {
                event_code = PendingAutoAbilityQueueEventCode,
                sender_player_id = pendingQueuePayload.sender_player_index.ToString(),
                json = pendingQueuePayload.ToJson(false)
            };
        }

        public static bool TryDecodePendingAutoAbilityQueue(
            PhotonRealtimePayload payload,
            out NetworkPendingAutoAbilityQueuePayload pendingQueuePayload,
            out string rejectionReason)
        {
            pendingQueuePayload = null;
            rejectionReason = null;
            if (payload == null || payload.event_code != PendingAutoAbilityQueueEventCode || string.IsNullOrWhiteSpace(payload.json))
            {
                rejectionReason = "PENDING_AUTO_ABILITY_QUEUE_PAYLOAD_INVALID";
                return false;
            }

            try
            {
                pendingQueuePayload = NetworkPendingAutoAbilityQueuePayload.FromJson(payload.json);
                PendingAutoAbilityQueue decodedQueue;
                if (!PendingAutoAbilityQueuePayloadCodec.TryDecode(
                        pendingQueuePayload,
                        out decodedQueue,
                        out rejectionReason))
                {
                    pendingQueuePayload = null;
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                rejectionReason = "PENDING_AUTO_ABILITY_QUEUE_PAYLOAD_PARSE_FAILED: " + exception.Message;
                return false;
            }
        }

        public static PhotonRealtimePayload EncodePendingAutoAbilityResolutionRequest(
            NetworkPendingAutoAbilityResolutionRequestPayload resolutionRequestPayload)
        {
            if (resolutionRequestPayload == null)
            {
                throw new ArgumentNullException(nameof(resolutionRequestPayload));
            }

            return new PhotonRealtimePayload
            {
                event_code = PendingAutoAbilityResolutionRequestEventCode,
                sender_player_id = resolutionRequestPayload.sender_player_index.ToString(),
                json = resolutionRequestPayload.ToJson(false)
            };
        }

        public static bool TryDecodePendingAutoAbilityResolutionRequest(
            PhotonRealtimePayload payload,
            out NetworkPendingAutoAbilityResolutionRequestPayload resolutionRequestPayload,
            out string rejectionReason)
        {
            resolutionRequestPayload = null;
            rejectionReason = null;
            if (payload == null ||
                payload.event_code != PendingAutoAbilityResolutionRequestEventCode ||
                string.IsNullOrWhiteSpace(payload.json))
            {
                rejectionReason = "PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_PAYLOAD_INVALID";
                return false;
            }

            try
            {
                resolutionRequestPayload =
                    NetworkPendingAutoAbilityResolutionRequestPayload.FromJson(payload.json);
                PendingAutoAbilityResolutionRequest decodedRequest;
                if (!PendingAutoAbilityResolutionRequestPayloadCodec.TryDecode(
                        resolutionRequestPayload,
                        out decodedRequest,
                        out rejectionReason))
                {
                    resolutionRequestPayload = null;
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                rejectionReason =
                    "PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_PAYLOAD_PARSE_FAILED: " + exception.Message;
                return false;
            }
        }

        public static PhotonRealtimePayload EncodePendingAutoAbilityManualResolutionDecision(
            NetworkPendingAutoAbilityManualResolutionDecisionPayload decisionPayload)
        {
            if (decisionPayload == null)
            {
                throw new ArgumentNullException(nameof(decisionPayload));
            }

            return new PhotonRealtimePayload
            {
                event_code = PendingAutoAbilityManualResolutionDecisionEventCode,
                sender_player_id = decisionPayload.sender_player_index.ToString(),
                json = decisionPayload.ToJson(false)
            };
        }

        public static bool TryDecodePendingAutoAbilityManualResolutionDecision(
            PhotonRealtimePayload payload,
            out NetworkPendingAutoAbilityManualResolutionDecisionPayload decisionPayload,
            out string rejectionReason)
        {
            decisionPayload = null;
            rejectionReason = null;
            if (payload == null ||
                payload.event_code != PendingAutoAbilityManualResolutionDecisionEventCode ||
                string.IsNullOrWhiteSpace(payload.json))
            {
                rejectionReason = "PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_PAYLOAD_INVALID";
                return false;
            }

            try
            {
                decisionPayload =
                    NetworkPendingAutoAbilityManualResolutionDecisionPayload.FromJson(payload.json);
                PendingAutoAbilityManualResolutionDecision decodedDecision;
                if (!PendingAutoAbilityManualResolutionDecisionPayloadCodec.TryDecode(
                        decisionPayload,
                        out decodedDecision,
                        out rejectionReason))
                {
                    decisionPayload = null;
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                rejectionReason =
                    "PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_PAYLOAD_PARSE_FAILED: " +
                    exception.Message;
                return false;
            }
        }

        public static PhotonRealtimePayload EncodeDeckRevealRequest(NetworkDeckRevealRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new PhotonRealtimePayload
            {
                event_code = DeckRevealRequestEventCode,
                sender_player_id = request.requester_player_id,
                json = request.ToJson(false)
            };
        }

        public static bool TryDecodeDeckRevealRequest(PhotonRealtimePayload payload, out NetworkDeckRevealRequest request, out string rejectionReason)
        {
            request = null;
            rejectionReason = null;
            if (payload == null || payload.event_code != DeckRevealRequestEventCode || string.IsNullOrWhiteSpace(payload.json))
            {
                rejectionReason = "DECK_REVEAL_REQUEST_PAYLOAD_INVALID";
                return false;
            }

            try
            {
                request = NetworkDeckRevealRequest.FromJson(payload.json);
                if (request.protocol_version != MultiplayerProtocol.ProtocolVersion)
                {
                    rejectionReason = "PROTOCOL_VERSION_MISMATCH";
                    request = null;
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                rejectionReason = "DECK_REVEAL_REQUEST_PARSE_FAILED: " + exception.Message;
                return false;
            }
        }

        public static PhotonRealtimePayload EncodeDeckRevealResponse(NetworkDeckRevealResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            return new PhotonRealtimePayload
            {
                event_code = DeckRevealResponseEventCode,
                sender_player_id = response.player_id,
                json = response.ToJson(false)
            };
        }

        public static bool TryDecodeDeckRevealResponse(PhotonRealtimePayload payload, out NetworkDeckRevealResponse response, out string rejectionReason)
        {
            response = null;
            rejectionReason = null;
            if (payload == null || payload.event_code != DeckRevealResponseEventCode || string.IsNullOrWhiteSpace(payload.json))
            {
                rejectionReason = "DECK_REVEAL_RESPONSE_PAYLOAD_INVALID";
                return false;
            }

            try
            {
                response = NetworkDeckRevealResponse.FromJson(payload.json);
                if (response.protocol_version != MultiplayerProtocol.ProtocolVersion)
                {
                    rejectionReason = "PROTOCOL_VERSION_MISMATCH";
                    response = null;
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                rejectionReason = "DECK_REVEAL_RESPONSE_PARSE_FAILED: " + exception.Message;
                return false;
            }
        }

        public static PhotonRealtimePayload EncodeReconnectRequest(NetworkReconnectRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return new PhotonRealtimePayload
            {
                event_code = ReconnectRequestEventCode,
                sender_player_id = request.player_id,
                json = request.ToJson(false)
            };
        }

        public static bool TryDecodeReconnectRequest(PhotonRealtimePayload payload, out NetworkReconnectRequest request, out string rejectionReason)
        {
            request = null;
            rejectionReason = null;
            if (payload == null || payload.event_code != ReconnectRequestEventCode || string.IsNullOrWhiteSpace(payload.json))
            {
                rejectionReason = "RECONNECT_REQUEST_PAYLOAD_INVALID";
                return false;
            }

            try
            {
                request = NetworkReconnectRequest.FromJson(payload.json);
                return true;
            }
            catch (Exception exception)
            {
                rejectionReason = "RECONNECT_REQUEST_PARSE_FAILED: " + exception.Message;
                return false;
            }
        }

        public static PhotonRealtimePayload EncodeReconnectBatch(NetworkEventBatch batch, string senderPlayerId = null)
        {
            if (batch == null)
            {
                throw new ArgumentNullException(nameof(batch));
            }

            return new PhotonRealtimePayload
            {
                event_code = ReconnectBatchEventCode,
                sender_player_id = senderPlayerId,
                json = batch.ToJson(false)
            };
        }

        public static bool TryDecodeReconnectBatch(PhotonRealtimePayload payload, out NetworkEventBatch batch, out string rejectionReason)
        {
            batch = null;
            rejectionReason = null;
            if (payload == null || payload.event_code != ReconnectBatchEventCode || string.IsNullOrWhiteSpace(payload.json))
            {
                rejectionReason = "RECONNECT_BATCH_PAYLOAD_INVALID";
                return false;
            }

            try
            {
                batch = NetworkEventBatch.FromJson(payload.json);
                return true;
            }
            catch (Exception exception)
            {
                rejectionReason = "RECONNECT_BATCH_PARSE_FAILED: " + exception.Message;
                return false;
            }
        }

        private static void Set(Dictionary<string, string> properties, string key, string value)
        {
            properties[key] = value ?? "";
        }

        private static string Get(IDictionary<string, string> properties, string key)
        {
            string value;
            return TryGet(properties, key, out value) ? value : "";
        }

        private static bool TryGet(IDictionary<string, string> properties, string key, out string value)
        {
            value = null;
            return properties != null && properties.TryGetValue(key, out value);
        }

        private static int ParseInt(string value, int fallback)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : fallback;
        }
    }
}
