using System;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    public static class NetworkCommandEnvelopeValidator
    {
        public static bool TryValidateForState(
            NetworkCommandEnvelope envelope,
            MultiplayerRoomState room,
            GameState state,
            out string rejectionReason)
        {
            if (!TryValidateShape(envelope, room, out rejectionReason))
            {
                return false;
            }

            if (state == null)
            {
                rejectionReason = "STATE_MISSING";
                return false;
            }

            state.EnsureLists();
            if (!string.Equals(state.game_id ?? "", envelope.room_game_id ?? "", StringComparison.Ordinal))
            {
                rejectionReason = "ROOM_GAME_ID_MISMATCH";
                return false;
            }

            if (envelope.state_cursor != state.event_log.Count)
            {
                rejectionReason = "STATE_CURSOR_STALE";
                return false;
            }

            if (envelope.player_index >= state.players.Count)
            {
                rejectionReason = "PLAYER_INDEX_OUT_OF_RANGE";
                return false;
            }

            if (room != null)
            {
                room.EnsureLists();
                if (envelope.player_index >= room.players.Count)
                {
                    rejectionReason = "PLAYER_INDEX_OUT_OF_RANGE";
                    return false;
                }

                RoomPlayerInfo roomPlayer = room.players[envelope.player_index];
                if (roomPlayer == null ||
                    !string.Equals(roomPlayer.player_id ?? "", envelope.player_id ?? "", StringComparison.Ordinal))
                {
                    rejectionReason = "PLAYER_OWNERSHIP_MISMATCH";
                    return false;
                }
            }

            if (envelope.action.actor_index != envelope.player_index)
            {
                rejectionReason = "ACTION_ACTOR_MISMATCH";
                return false;
            }

            if (IsTurnOwnerGated(envelope.action) && state.turn_player_index != envelope.player_index)
            {
                rejectionReason = "OUT_OF_TURN_COMMAND";
                return false;
            }

            return true;
        }

        public static bool TryValidateShape(
            NetworkCommandEnvelope envelope,
            MultiplayerRoomState room,
            out string rejectionReason)
        {
            rejectionReason = null;
            if (envelope == null)
            {
                rejectionReason = "COMMAND_ENVELOPE_MISSING";
                return false;
            }

            if (envelope.protocol_version != MultiplayerProtocol.ProtocolVersion)
            {
                rejectionReason = "PROTOCOL_VERSION_MISMATCH";
                return false;
            }

            if (room != null && !string.Equals(room.room_id ?? "", envelope.room_id ?? "", StringComparison.Ordinal))
            {
                rejectionReason = "ROOM_ID_MISMATCH";
                return false;
            }

            if (string.IsNullOrWhiteSpace(envelope.room_id))
            {
                rejectionReason = "ROOM_ID_MISSING";
                return false;
            }

            if (string.IsNullOrWhiteSpace(envelope.room_game_id))
            {
                rejectionReason = "ROOM_GAME_ID_MISSING";
                return false;
            }

            if (string.IsNullOrWhiteSpace(envelope.player_id))
            {
                rejectionReason = "PLAYER_ID_MISSING";
                return false;
            }

            if (envelope.player_index < 0)
            {
                rejectionReason = "PLAYER_INDEX_MISSING";
                return false;
            }

            if (envelope.sequence < 0)
            {
                rejectionReason = "SEQUENCE_MISSING";
                return false;
            }

            if (envelope.state_cursor < 0)
            {
                rejectionReason = "STATE_CURSOR_MISSING";
                return false;
            }

            if (envelope.action == null)
            {
                rejectionReason = "ACTION_MISSING";
                return false;
            }

            if (!Enum.IsDefined(typeof(GameActionType), envelope.action.action_type))
            {
                rejectionReason = "ACTION_TYPE_INVALID";
                return false;
            }

            return true;
        }

        private static bool IsTurnOwnerGated(LegalGameAction action)
        {
            if (action == null)
            {
                return false;
            }

            switch (action.action_type)
            {
                case GameActionType.Draw:
                case GameActionType.MoveCard:
                case GameActionType.SetPhase:
                case GameActionType.AddGiftMarker:
                case GameActionType.ResourceFlip:
                    return true;
                default:
                    return true;
            }
        }
    }
}
