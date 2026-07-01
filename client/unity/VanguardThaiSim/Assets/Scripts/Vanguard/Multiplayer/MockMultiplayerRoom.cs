using System;
using System.Collections.Generic;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    public sealed class MockMultiplayerRoom
    {
        private readonly MultiplayerRoomState roomState;
        private readonly List<NetworkEventEnvelope> eventLog = new List<NetworkEventEnvelope>();

        public MockMultiplayerRoom(MultiplayerRoomState roomState)
        {
            this.roomState = roomState ?? throw new ArgumentNullException(nameof(roomState));
            this.roomState.EnsureLists();
        }

        public MultiplayerRoomState RoomState
        {
            get { return roomState; }
        }

        public int EventCount
        {
            get { return eventLog.Count; }
        }

        public IReadOnlyList<NetworkEventEnvelope> EventLog
        {
            get { return eventLog; }
        }

        public bool TryConnect(RoomPlayerInfo player, PackSyncInfo pack, out string rejectionReason)
        {
            rejectionReason = null;
            if (player == null)
            {
                rejectionReason = "PLAYER_MISSING";
                return false;
            }

            if (!MultiplayerProtocol.PackMatches(roomState.pack, pack, false))
            {
                rejectionReason = "PACK_HASH_MISMATCH";
                return false;
            }

            RoomPlayerInfo existing = FindPlayer(player.player_id);
            if (existing == null)
            {
                player.connected = true;
                player.event_cursor = Math.Max(0, Math.Min(player.event_cursor, eventLog.Count));
                roomState.players.Add(player);
            }
            else
            {
                existing.connected = true;
                existing.display_name = player.display_name;
                existing.deck_id = player.deck_id;
                existing.deck_hash = player.deck_hash;
                existing.event_cursor = Math.Max(0, Math.Min(player.event_cursor, eventLog.Count));
            }

            return true;
        }

        public NetworkEventEnvelope Publish(string playerId, GameState state, GameEvent gameEvent)
        {
            RoomPlayerInfo publishingPlayer = FindPlayer(playerId);
            if (publishingPlayer == null || !publishingPlayer.connected)
            {
                throw new InvalidOperationException("Player is not connected to the room.");
            }

            NetworkEventEnvelope envelope = MultiplayerProtocol.CreateEnvelope(roomState, playerId, state, gameEvent);
            if (envelope.event_index != eventLog.Count)
            {
                throw new InvalidOperationException("Published event index does not match room cursor.");
            }

            eventLog.Add(envelope);
            publishingPlayer.event_cursor = eventLog.Count;

            roomState.state = "in_game";
            return envelope;
        }

        public NetworkEventBatch CreateReconnectBatch(string playerId)
        {
            RoomPlayerInfo player = FindPlayer(playerId);
            if (player == null)
            {
                throw new InvalidOperationException("Player is not connected to the room.");
            }

            NetworkEventBatch batch = new NetworkEventBatch
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = roomState.room_id,
                from_event_index = player.event_cursor
            };

            for (int i = player.event_cursor; i < eventLog.Count; i++)
            {
                batch.events.Add(eventLog[i]);
            }

            return batch;
        }

        public int SyncInto(string playerId, GameState targetState)
        {
            RoomPlayerInfo player = FindPlayer(playerId);
            if (player == null)
            {
                throw new InvalidOperationException("Player is not connected to the room.");
            }

            int applied = 0;
            for (int i = player.event_cursor; i < eventLog.Count; i++)
            {
                string rejectionReason;
                if (!MultiplayerProtocol.TryApplyEnvelope(targetState, eventLog[i], out rejectionReason))
                {
                    throw new InvalidOperationException(rejectionReason);
                }

                applied++;
            }

            player.event_cursor = eventLog.Count;
            return applied;
        }

        private RoomPlayerInfo FindPlayer(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return null;
            }

            for (int i = 0; i < roomState.players.Count; i++)
            {
                RoomPlayerInfo player = roomState.players[i];
                if (player != null && string.Equals(player.player_id, playerId, StringComparison.Ordinal))
                {
                    return player;
                }
            }

            return null;
        }
    }
}
