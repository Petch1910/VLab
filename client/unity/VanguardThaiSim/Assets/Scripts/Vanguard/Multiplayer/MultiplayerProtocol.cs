using System;
using System.Collections.Generic;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    public static class MultiplayerProtocol
    {
        public const int ProtocolVersion = 1;

        public static MultiplayerRoomState CreateRoom(
            string roomId,
            string format,
            string hostPlayerId,
            int randomSeed,
            PackSyncInfo pack)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                throw new ArgumentException("Room id is required.", nameof(roomId));
            }

            if (string.IsNullOrWhiteSpace(hostPlayerId))
            {
                throw new ArgumentException("Host player id is required.", nameof(hostPlayerId));
            }

            if (pack == null)
            {
                throw new ArgumentNullException(nameof(pack));
            }

            return new MultiplayerRoomState
            {
                protocol_version = ProtocolVersion,
                room_id = roomId,
                format = string.IsNullOrWhiteSpace(format) ? "D" : format,
                state = "waiting",
                host_player_id = hostPlayerId,
                random_seed = randomSeed,
                pack = pack
            };
        }

        public static bool PackMatches(PackSyncInfo expected, PackSyncInfo actual, bool requireImageHash)
        {
            if (expected == null || actual == null)
            {
                return false;
            }

            if (!StringEquals(expected.pack_id, actual.pack_id))
            {
                return false;
            }

            if (!StringEquals(expected.source_version, actual.source_version))
            {
                return false;
            }

            if (!StringEquals(expected.definition_hash, actual.definition_hash))
            {
                return false;
            }

            if (!StringEquals(expected.image_manifest_hash, actual.image_manifest_hash))
            {
                return false;
            }

            if (requireImageHash && !StringEquals(expected.image_content_hash, actual.image_content_hash))
            {
                return false;
            }

            return true;
        }

        public static MultiplayerProtocolValidationResult ValidateRoomReady(
            MultiplayerRoomState room,
            PackSyncInfo localPack,
            int expectedPlayerCount = 2,
            bool requireImageHash = false)
        {
            MultiplayerProtocolValidationResult result = new MultiplayerProtocolValidationResult();
            if (room == null)
            {
                result.Reject("ROOM_MISSING");
                return result;
            }

            room.EnsureLists();
            if (room.protocol_version != ProtocolVersion)
            {
                result.Reject("PROTOCOL_VERSION_MISMATCH");
            }

            ValidateDeckPrivacy(room, result);

            if (!PackMatches(room.pack, localPack, requireImageHash))
            {
                result.Reject("PACK_HASH_MISMATCH");
            }

            if (room.random_seed == 0)
            {
                result.Reject("RANDOM_SEED_MISSING");
            }

            int connected = 0;
            for (int i = 0; i < room.players.Count; i++)
            {
                if (room.players[i] != null && room.players[i].connected)
                {
                    connected++;
                }
            }

            if (connected < expectedPlayerCount)
            {
                result.Reject("PLAYERS_NOT_READY");
            }

            return result;
        }

        private static void ValidateDeckPrivacy(MultiplayerRoomState room, MultiplayerProtocolValidationResult result)
        {
            string visibility = NormalizeMode(room.room_visibility, RoomVisibilityModes.Friend);
            string privacyMode = NormalizeMode(room.deck_privacy_mode, DeckPrivacyModes.SharedDeckCode);
            bool isPublicOrRanked =
                StringEquals(visibility, RoomVisibilityModes.Public) ||
                StringEquals(visibility, RoomVisibilityModes.Ranked);

            if (isPublicOrRanked && StringEquals(privacyMode, DeckPrivacyModes.SharedDeckCode))
            {
                result.Reject("DECK_PRIVACY_SHARED_CODE_NOT_ALLOWED");
            }

            if (!StringEquals(privacyMode, DeckPrivacyModes.DeckCommitment))
            {
                return;
            }

            for (int i = 0; i < room.players.Count; i++)
            {
                RoomPlayerInfo player = room.players[i];
                if (player == null || !player.connected)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(player.deck_commitment))
                {
                    result.Reject("DECK_COMMITMENT_MISSING");
                }

                if (!StringEquals(player.deck_commitment_algorithm, DeckCommitmentService.Algorithm))
                {
                    result.Reject("DECK_COMMITMENT_ALGORITHM_MISMATCH");
                }

                if (!string.IsNullOrWhiteSpace(player.deck_code))
                {
                    result.Reject("DECK_CODE_FORBIDDEN_FOR_COMMITMENT");
                }
            }
        }

        public static NetworkEventEnvelope CreateEnvelope(
            MultiplayerRoomState room,
            string playerId,
            GameState state,
            GameEvent gameEvent)
        {
            if (room == null)
            {
                throw new ArgumentNullException(nameof(room));
            }

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (gameEvent == null)
            {
                throw new ArgumentNullException(nameof(gameEvent));
            }

            state.EnsureLists();
            int eventIndex = FindEventIndex(state.event_log, gameEvent.event_id);
            if (eventIndex < 0)
            {
                eventIndex = state.event_log.Count;
            }

            return new NetworkEventEnvelope
            {
                protocol_version = ProtocolVersion,
                room_id = room.room_id,
                game_id = state.game_id,
                player_id = playerId,
                event_index = eventIndex,
                previous_event_id = eventIndex > 0 && eventIndex - 1 < state.event_log.Count ? state.event_log[eventIndex - 1].event_id : null,
                sent_at_utc = DateTime.UtcNow.ToString("O"),
                game_event = gameEvent
            };
        }

        public static bool TryApplyEnvelope(GameState state, NetworkEventEnvelope envelope, out string rejectionReason)
        {
            rejectionReason = null;
            if (state == null)
            {
                rejectionReason = "STATE_MISSING";
                return false;
            }

            if (envelope == null || envelope.game_event == null)
            {
                rejectionReason = "EVENT_MISSING";
                return false;
            }

            state.EnsureLists();
            if (envelope.protocol_version != ProtocolVersion)
            {
                rejectionReason = "PROTOCOL_VERSION_MISMATCH";
                return false;
            }

            if (!string.IsNullOrEmpty(envelope.game_id) && !StringEquals(envelope.game_id, state.game_id))
            {
                rejectionReason = "GAME_ID_MISMATCH";
                return false;
            }

            if (envelope.event_index < state.event_log.Count)
            {
                GameEvent existing = state.event_log[envelope.event_index];
                if (existing != null && StringEquals(existing.event_id, envelope.game_event.event_id))
                {
                    return true;
                }

                rejectionReason = "EVENT_INDEX_CONFLICT";
                return false;
            }

            if (envelope.event_index != state.event_log.Count)
            {
                rejectionReason = "EVENT_INDEX_GAP";
                return false;
            }

            if (state.event_log.Count > 0)
            {
                string expectedPrevious = state.event_log[state.event_log.Count - 1].event_id;
                if (!StringEquals(expectedPrevious, envelope.previous_event_id))
                {
                    rejectionReason = "PREVIOUS_EVENT_MISMATCH";
                    return false;
                }
            }
            else if (!string.IsNullOrEmpty(envelope.previous_event_id))
            {
                rejectionReason = "PREVIOUS_EVENT_MISMATCH";
                return false;
            }

            try
            {
                GameEventReducer.Apply(state, envelope.game_event, true);
            }
            catch (Exception exception)
            {
                rejectionReason = "EVENT_APPLY_FAILED: " + exception.Message;
                return false;
            }

            return true;
        }

        public static NetworkEventBatch CreateReconnectBatch(
            MultiplayerRoomState room,
            string playerId,
            GameState state,
            int fromEventIndex)
        {
            if (room == null)
            {
                throw new ArgumentNullException(nameof(room));
            }

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            state.EnsureLists();
            int start = Math.Max(0, fromEventIndex);
            NetworkEventBatch batch = new NetworkEventBatch
            {
                protocol_version = ProtocolVersion,
                room_id = room.room_id,
                from_event_index = start
            };

            for (int i = start; i < state.event_log.Count; i++)
            {
                batch.events.Add(CreateEnvelope(room, playerId, state, state.event_log[i]));
            }

            return batch;
        }

        public static GameReplay CreateReplayFromNetwork(GameState initialState, IReadOnlyList<NetworkEventEnvelope> envelopes)
        {
            List<GameEvent> events = new List<GameEvent>();
            if (envelopes != null)
            {
                for (int i = 0; i < envelopes.Count; i++)
                {
                    if (envelopes[i] != null && envelopes[i].game_event != null)
                    {
                        events.Add(envelopes[i].game_event);
                    }
                }
            }

            return GameReplay.Create(initialState, events);
        }

        private static int FindEventIndex(IReadOnlyList<GameEvent> events, string eventId)
        {
            if (events == null || string.IsNullOrEmpty(eventId))
            {
                return -1;
            }

            for (int i = 0; i < events.Count; i++)
            {
                if (events[i] != null && StringEquals(events[i].event_id, eventId))
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool StringEquals(string left, string right)
        {
            return string.Equals(left ?? "", right ?? "", StringComparison.Ordinal);
        }

        private static string NormalizeMode(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
