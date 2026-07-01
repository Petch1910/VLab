using System;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    public static class NetworkCommandEnvelopeFactory
    {
        public static NetworkCommandEnvelope Create(
            MultiplayerRoomState room,
            GameState state,
            string playerId,
            int playerIndex,
            int sequence,
            LegalGameAction action)
        {
            if (room == null)
            {
                throw new ArgumentNullException(nameof(room));
            }

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            state.EnsureLists();
            return new NetworkCommandEnvelope
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                command_id = CreateCommandId(room.room_id, playerId, sequence),
                room_id = room.room_id,
                room_game_id = state.game_id,
                player_id = playerId ?? string.Empty,
                player_index = playerIndex,
                sequence = Math.Max(0, sequence),
                state_cursor = state.event_log.Count,
                sent_at_utc = DateTime.UtcNow.ToString("O"),
                action = CloneAction(action)
            };
        }

        private static LegalGameAction CloneAction(LegalGameAction action)
        {
            return new LegalGameAction
            {
                label = action.label,
                action_type = action.action_type,
                actor_index = action.actor_index,
                card_instance_id = action.card_instance_id,
                from_zone = action.from_zone,
                to_zone = action.to_zone,
                phase = action.phase,
                gift_marker_type = action.gift_marker_type,
                marker_delta = action.marker_delta,
                resource_operation_type = action.resource_operation_type,
                resource_delta = action.resource_delta
            };
        }

        private static string CreateCommandId(string roomId, string playerId, int sequence)
        {
            return "cmd-" +
                Normalize(roomId) +
                "-" +
                Normalize(playerId) +
                "-" +
                Math.Max(0, sequence).ToString("D8");
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim();
        }
    }
}
