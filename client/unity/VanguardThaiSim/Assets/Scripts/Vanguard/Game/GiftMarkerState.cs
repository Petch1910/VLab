using System;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class GiftMarkerState
    {
        public GiftMarkerType type;
        public int count;

        public GiftMarkerState()
        {
        }

        public GiftMarkerState(GiftMarkerType type, int count)
        {
            this.type = type;
            this.count = count;
        }
    }
}
