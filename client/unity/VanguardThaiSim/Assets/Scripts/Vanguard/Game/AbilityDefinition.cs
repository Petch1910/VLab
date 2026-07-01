using System;
using System.Collections.Generic;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class AbilityDefinition
    {
        public string ability_id;
        public string label;
        public AbilityTiming timing;
        public bool manual_fallback;
        public List<AbilityEffectDefinition> effects = new List<AbilityEffectDefinition>();

        public void EnsureLists()
        {
            if (effects == null)
            {
                effects = new List<AbilityEffectDefinition>();
            }
        }
    }
}
