using System;
using UnityEngine;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class PhotonRealtimePayload
    {
        public byte event_code;
        public string sender_player_id;
        public string json;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static PhotonRealtimePayload FromJson(string json)
        {
            PhotonRealtimePayload payload = JsonUtility.FromJson<PhotonRealtimePayload>(json);
            if (payload == null)
            {
                throw new ArgumentException("Photon Realtime payload JSON could not be parsed.", nameof(json));
            }

            return payload;
        }
    }
}
