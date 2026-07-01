using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class MultiplayerRoomState
    {
        public int protocol_version;
        public string room_id;
        public string format;
        public string state;
        public string room_visibility = RoomVisibilityModes.Friend;
        public string deck_privacy_mode = DeckPrivacyModes.SharedDeckCode;
        public string host_player_id;
        public int random_seed;
        public PackSyncInfo pack;
        public List<RoomPlayerInfo> players = new List<RoomPlayerInfo>();

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static MultiplayerRoomState FromJson(string json)
        {
            MultiplayerRoomState room = JsonUtility.FromJson<MultiplayerRoomState>(json);
            if (room == null)
            {
                throw new ArgumentException("Room JSON could not be parsed.", nameof(json));
            }

            room.EnsureLists();
            return room;
        }

        public void EnsureLists()
        {
            if (string.IsNullOrWhiteSpace(room_visibility))
            {
                room_visibility = RoomVisibilityModes.Friend;
            }

            if (string.IsNullOrWhiteSpace(deck_privacy_mode))
            {
                deck_privacy_mode = DeckPrivacyModes.SharedDeckCode;
            }

            if (players == null)
            {
                players = new List<RoomPlayerInfo>();
            }
        }
    }
}
