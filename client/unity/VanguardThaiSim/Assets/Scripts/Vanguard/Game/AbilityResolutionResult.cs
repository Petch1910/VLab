using System.Collections.Generic;

namespace VanguardThaiSim.Game
{
    public sealed class AbilityResolutionResult
    {
        public bool accepted;
        public bool needs_manual_resolution;
        public string rejection_reason;
        public List<GameEvent> events = new List<GameEvent>();

        public static AbilityResolutionResult Accepted()
        {
            return new AbilityResolutionResult
            {
                accepted = true,
                needs_manual_resolution = false,
                rejection_reason = string.Empty
            };
        }

        public static AbilityResolutionResult Rejected(string reason)
        {
            return new AbilityResolutionResult
            {
                accepted = false,
                needs_manual_resolution = false,
                rejection_reason = reason
            };
        }

        public static AbilityResolutionResult NeedsManualResolution(string reason)
        {
            return new AbilityResolutionResult
            {
                accepted = false,
                needs_manual_resolution = true,
                rejection_reason = reason
            };
        }
    }
}
