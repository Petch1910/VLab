using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class TournamentAuditLog
    {
        public int protocol_version = MultiplayerProtocol.ProtocolVersion;
        public string audit_id;
        public string generated_at_utc;
        public string room_id;
        public string format;
        public string room_state;
        public string room_visibility;
        public string deck_privacy_mode;
        public string host_player_id;
        public int random_seed;
        public PackSyncInfo pack;
        public List<TournamentAuditPlayerRecord> players = new List<TournamentAuditPlayerRecord>();
        public int public_event_count;
        public List<NetworkPublicGameEvent> public_events = new List<NetworkPublicGameEvent>();
        public TournamentAuditResult result;

        public void EnsureLists()
        {
            if (players == null)
            {
                players = new List<TournamentAuditPlayerRecord>();
            }

            if (public_events == null)
            {
                public_events = new List<NetworkPublicGameEvent>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static TournamentAuditLog FromJson(string json)
        {
            TournamentAuditLog log = JsonUtility.FromJson<TournamentAuditLog>(json);
            if (log == null)
            {
                throw new ArgumentException("Tournament audit log JSON could not be parsed.", nameof(json));
            }

            log.EnsureLists();
            if (log.protocol_version == 0)
            {
                log.protocol_version = MultiplayerProtocol.ProtocolVersion;
            }

            return log;
        }
    }

    [Serializable]
    public sealed class TournamentAuditPlayerRecord
    {
        public string player_id;
        public string display_name;
        public string deck_id;
        public string deck_hash;
        public string deck_commitment;
        public string deck_commitment_algorithm;
        public string deck_reveal_policy;
        public int main_deck_count;
        public int ride_deck_count;
        public int g_deck_count;
        public int opening_hand_count;
        public bool connected;
        public bool ready;
        public int event_cursor;
    }

    [Serializable]
    public sealed class TournamentAuditResult
    {
        public string status;
        public string winner_player_id;
        public int winner_player_index = -1;
        public string end_reason;
        public string ended_at_utc;

        public static TournamentAuditResult Create(
            string status,
            string winnerPlayerId = null,
            int winnerPlayerIndex = -1,
            string endReason = null,
            string endedAtUtc = null)
        {
            return new TournamentAuditResult
            {
                status = status ?? "",
                winner_player_id = winnerPlayerId ?? "",
                winner_player_index = winnerPlayerIndex,
                end_reason = endReason ?? "",
                ended_at_utc = endedAtUtc ?? ""
            };
        }
    }

    public static class TournamentAuditLogFactory
    {
        public static TournamentAuditLog Create(
            MultiplayerRoomState room,
            IReadOnlyList<NetworkPublicGameEvent> publicEvents,
            TournamentAuditResult result,
            string generatedAtUtc = null)
        {
            if (room == null)
            {
                throw new ArgumentNullException(nameof(room));
            }

            room.EnsureLists();
            TournamentAuditLog log = new TournamentAuditLog
            {
                audit_id = Guid.NewGuid().ToString("N"),
                generated_at_utc = string.IsNullOrWhiteSpace(generatedAtUtc)
                    ? DateTime.UtcNow.ToString("O")
                    : generatedAtUtc,
                room_id = room.room_id ?? "",
                format = room.format ?? "",
                room_state = room.state ?? "",
                room_visibility = room.room_visibility ?? "",
                deck_privacy_mode = room.deck_privacy_mode ?? "",
                host_player_id = room.host_player_id ?? "",
                random_seed = room.random_seed,
                pack = ClonePack(room.pack),
                result = result ?? TournamentAuditResult.Create("")
            };

            for (int i = 0; i < room.players.Count; i++)
            {
                log.players.Add(CreatePlayerRecord(room.players[i]));
            }

            if (publicEvents != null)
            {
                for (int i = 0; i < publicEvents.Count; i++)
                {
                    NetworkPublicGameEvent publicEvent = SanitizePublicEvent(publicEvents[i]);
                    if (publicEvent != null)
                    {
                        log.public_events.Add(publicEvent);
                    }
                }
            }

            log.public_event_count = log.public_events.Count;
            return log;
        }

        private static PackSyncInfo ClonePack(PackSyncInfo pack)
        {
            if (pack == null)
            {
                return null;
            }

            return new PackSyncInfo
            {
                pack_id = pack.pack_id ?? "",
                source_version = pack.source_version ?? "",
                definition_hash = pack.definition_hash ?? "",
                image_manifest_hash = pack.image_manifest_hash ?? "",
                image_content_hash = pack.image_content_hash ?? ""
            };
        }

        private static TournamentAuditPlayerRecord CreatePlayerRecord(RoomPlayerInfo player)
        {
            if (player == null)
            {
                return new TournamentAuditPlayerRecord();
            }

            return new TournamentAuditPlayerRecord
            {
                player_id = player.player_id ?? "",
                display_name = player.display_name ?? "",
                deck_id = player.deck_id ?? "",
                deck_hash = player.deck_hash ?? "",
                deck_commitment = player.deck_commitment ?? "",
                deck_commitment_algorithm = player.deck_commitment_algorithm ?? "",
                deck_reveal_policy = player.deck_reveal_policy ?? "",
                main_deck_count = player.main_deck_count,
                ride_deck_count = player.ride_deck_count,
                g_deck_count = player.g_deck_count,
                opening_hand_count = player.opening_hand_count,
                connected = player.connected,
                ready = player.ready,
                event_cursor = player.event_cursor
            };
        }

        private static NetworkPublicGameEvent SanitizePublicEvent(NetworkPublicGameEvent publicEvent)
        {
            NetworkPublicGameEvent clone = NetworkPublicGameReplay.CloneEvent(publicEvent);
            if (clone == null)
            {
                return null;
            }

            if (clone.hides_card_identity)
            {
                clone.public_card_id = "";
                clone.public_card_instance_id = "";
                clone.reveal_proof = "";
            }

            return clone;
        }
    }
}
