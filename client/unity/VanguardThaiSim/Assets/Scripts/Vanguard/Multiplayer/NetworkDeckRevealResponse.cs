using System;
using UnityEngine;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class NetworkDeckRevealResponse
    {
        public int protocol_version = MultiplayerProtocol.ProtocolVersion;
        public string room_id;
        public string player_id;
        public string revealed_deck_code;
        public string deck_reveal_nonce;
        public string deck_commitment;
        public string deck_commitment_algorithm = DeckCommitmentService.Algorithm;
        public string pack_definition_hash;
        public string revealed_at_utc;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static NetworkDeckRevealResponse FromJson(string json)
        {
            NetworkDeckRevealResponse response = JsonUtility.FromJson<NetworkDeckRevealResponse>(json);
            if (response == null)
            {
                throw new ArgumentException("Deck reveal response JSON could not be parsed.", nameof(json));
            }

            return response;
        }
    }
}
