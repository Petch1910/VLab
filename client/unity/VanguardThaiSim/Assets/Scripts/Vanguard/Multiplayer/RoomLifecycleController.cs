using System;

namespace VanguardThaiSim.Multiplayer
{
    public static class RoomLifecycleStates
    {
        public const string Waiting = "waiting";
        public const string Ready = "ready";
        public const string Playing = "playing";
        public const string Ended = "ended";
    }

    public static class RoomLifecycleController
    {
        public static RoomLifecycleTransitionResult SetPlayerReady(
            MultiplayerRoomState room,
            string playerId,
            bool ready)
        {
            MultiplayerRoomState clone;
            RoomPlayerInfo player;
            string rejectionReason;
            if (!TryCloneAndFindPlayer(room, playerId, out clone, out player, out rejectionReason))
            {
                return RoomLifecycleTransitionResult.Rejected(rejectionReason);
            }

            if (IsPlaying(clone))
            {
                return RoomLifecycleTransitionResult.Rejected("ROOM_ALREADY_PLAYING");
            }

            player.ready = ready;
            clone.state = AllConnectedPlayersReady(clone)
                ? RoomLifecycleStates.Ready
                : RoomLifecycleStates.Waiting;
            return RoomLifecycleTransitionResult.Accepted(clone);
        }

        public static RoomLifecycleTransitionResult Start(MultiplayerRoomState room)
        {
            MultiplayerRoomState clone = Clone(room);
            if (clone == null)
            {
                return RoomLifecycleTransitionResult.Rejected("ROOM_MISSING");
            }

            clone.EnsureLists();
            if (IsPlaying(clone))
            {
                return RoomLifecycleTransitionResult.Rejected("ROOM_ALREADY_PLAYING");
            }

            if (string.Equals(clone.state ?? "", RoomLifecycleStates.Ended, StringComparison.Ordinal))
            {
                return RoomLifecycleTransitionResult.Rejected("ROOM_ALREADY_ENDED");
            }

            if (ConnectedPlayerCount(clone) < 2)
            {
                return RoomLifecycleTransitionResult.Rejected("PLAYERS_NOT_READY");
            }

            if (!AllConnectedPlayersReady(clone))
            {
                return RoomLifecycleTransitionResult.Rejected("PLAYERS_NOT_READY");
            }

            clone.state = RoomLifecycleStates.Playing;
            return RoomLifecycleTransitionResult.Accepted(clone);
        }

        public static RoomLifecycleTransitionResult End(MultiplayerRoomState room)
        {
            MultiplayerRoomState clone = Clone(room);
            if (clone == null)
            {
                return RoomLifecycleTransitionResult.Rejected("ROOM_MISSING");
            }

            if (!IsPlaying(clone))
            {
                return RoomLifecycleTransitionResult.Rejected("ROOM_NOT_PLAYING");
            }

            clone.state = RoomLifecycleStates.Ended;
            return RoomLifecycleTransitionResult.Accepted(clone);
        }

        public static RoomLifecycleTransitionResult Rematch(MultiplayerRoomState room)
        {
            MultiplayerRoomState clone = Clone(room);
            if (clone == null)
            {
                return RoomLifecycleTransitionResult.Rejected("ROOM_MISSING");
            }

            clone.EnsureLists();
            if (!string.Equals(clone.state ?? "", RoomLifecycleStates.Ended, StringComparison.Ordinal))
            {
                return RoomLifecycleTransitionResult.Rejected("ROOM_NOT_ENDED");
            }

            clone.state = RoomLifecycleStates.Waiting;
            for (int i = 0; i < clone.players.Count; i++)
            {
                RoomPlayerInfo player = clone.players[i];
                if (player == null)
                {
                    continue;
                }

                player.ready = false;
                player.event_cursor = 0;
            }

            return RoomLifecycleTransitionResult.Accepted(clone);
        }

        private static bool TryCloneAndFindPlayer(
            MultiplayerRoomState room,
            string playerId,
            out MultiplayerRoomState clone,
            out RoomPlayerInfo player,
            out string rejectionReason)
        {
            clone = Clone(room);
            player = null;
            rejectionReason = null;
            if (clone == null)
            {
                rejectionReason = "ROOM_MISSING";
                return false;
            }

            if (string.IsNullOrWhiteSpace(playerId))
            {
                rejectionReason = "PLAYER_ID_MISSING";
                return false;
            }

            clone.EnsureLists();
            for (int i = 0; i < clone.players.Count; i++)
            {
                RoomPlayerInfo candidate = clone.players[i];
                if (candidate != null &&
                    string.Equals(candidate.player_id ?? "", playerId ?? "", StringComparison.Ordinal))
                {
                    player = candidate;
                    return true;
                }
            }

            rejectionReason = "PLAYER_MISSING";
            return false;
        }

        private static MultiplayerRoomState Clone(MultiplayerRoomState room)
        {
            return room == null ? null : MultiplayerRoomState.FromJson(room.ToJson(false));
        }

        private static bool IsPlaying(MultiplayerRoomState room)
        {
            return room != null && string.Equals(room.state ?? "", RoomLifecycleStates.Playing, StringComparison.Ordinal);
        }

        private static int ConnectedPlayerCount(MultiplayerRoomState room)
        {
            int count = 0;
            if (room == null)
            {
                return count;
            }

            room.EnsureLists();
            for (int i = 0; i < room.players.Count; i++)
            {
                if (room.players[i] != null && room.players[i].connected)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool AllConnectedPlayersReady(MultiplayerRoomState room)
        {
            if (room == null || ConnectedPlayerCount(room) < 2)
            {
                return false;
            }

            room.EnsureLists();
            for (int i = 0; i < room.players.Count; i++)
            {
                RoomPlayerInfo player = room.players[i];
                if (player != null && player.connected && !player.ready)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
