using System;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class NetworkPublicGameEvent
    {
        public int protocol_version = MultiplayerProtocol.ProtocolVersion;
        public string event_id;
        public string source_event_id;
        public string visibility = PublicEventVisibility.Public;
        public GameActionType action_type;
        public int actor_index;
        public GameZone from_zone;
        public GameZone to_zone;
        public int from_index = -1;
        public int to_index = -1;
        public int from_zone_count_delta;
        public int to_zone_count_delta;
        public bool hides_card_identity;
        public string public_card_id;
        public string public_card_instance_id;
        public GamePhase previous_phase;
        public GamePhase new_phase;
        public GiftMarkerType gift_marker_type;
        public int marker_delta;
        public string reveal_proof;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static NetworkPublicGameEvent FromJson(string json)
        {
            NetworkPublicGameEvent publicEvent = JsonUtility.FromJson<NetworkPublicGameEvent>(json);
            if (publicEvent == null)
            {
                throw new ArgumentException("Public game event JSON could not be parsed.", nameof(json));
            }

            if (string.IsNullOrWhiteSpace(publicEvent.visibility))
            {
                publicEvent.visibility = PublicEventVisibility.Public;
            }

            return publicEvent;
        }
    }
}
