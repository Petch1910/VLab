using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class TriggerCheckDraftSelectorCycleHelper
    {
        public const int CheckIndexCount = 4;

        public static TriggerType NextTriggerType(TriggerType triggerType)
        {
            switch (triggerType)
            {
                case TriggerType.Unknown:
                    return TriggerType.Critical;
                case TriggerType.Critical:
                    return TriggerType.Draw;
                case TriggerType.Draw:
                    return TriggerType.Front;
                case TriggerType.Front:
                    return TriggerType.Heal;
                case TriggerType.Heal:
                    return TriggerType.Over;
                case TriggerType.Over:
                    return TriggerType.None;
                case TriggerType.None:
                default:
                    return TriggerType.Unknown;
            }
        }

        public static TriggerCheckSource NextCheckSource(TriggerCheckSource checkSource)
        {
            switch (checkSource)
            {
                case TriggerCheckSource.Manual:
                    return TriggerCheckSource.Drive;
                case TriggerCheckSource.Drive:
                    return TriggerCheckSource.Damage;
                case TriggerCheckSource.Damage:
                default:
                    return TriggerCheckSource.Manual;
            }
        }

        public static int NextCheckIndex(int checkIndex)
        {
            int normalized = checkIndex % CheckIndexCount;
            if (normalized < 0)
            {
                normalized += CheckIndexCount;
            }

            return (normalized + 1) % CheckIndexCount;
        }
    }
}
