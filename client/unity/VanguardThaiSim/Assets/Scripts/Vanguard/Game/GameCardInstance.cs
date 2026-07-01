using System;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class GameCardInstance
    {
        public string instance_id;
        public string card_id;
        public int owner_index;
        public bool face_up;
        public int power_delta;

        public GameCardInstance()
        {
        }

        public GameCardInstance(string instanceId, string cardId, int ownerIndex, bool faceUp = true)
        {
            instance_id = instanceId;
            card_id = cardId;
            owner_index = ownerIndex;
            face_up = faceUp;
        }
    }
}
