using System;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class AbilityEffectDefinition
    {
        public AbilityEffectType effect_type;
        public int amount = 1;
        public GiftMarkerType gift_marker_type;
        public GamePhase phase;
        public GameZone from_zone;
        public GameZone to_zone;
        public string custom_effect_id;
    }
}
