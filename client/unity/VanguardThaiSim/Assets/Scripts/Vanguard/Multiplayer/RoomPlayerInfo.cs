using System;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class RoomPlayerInfo
    {
        public string player_id;
        public string display_name;
        public string deck_id;
        public string deck_hash;
        public string deck_code;
        public string deck_commitment;
        public string deck_commitment_algorithm;
        public string deck_reveal_policy;
        public string deck_reveal_nonce;
        public int main_deck_count;
        public int ride_deck_count;
        public int g_deck_count;
        public int opening_hand_count;
        public bool connected;
        public bool ready;
        public int event_cursor;
    }
}
