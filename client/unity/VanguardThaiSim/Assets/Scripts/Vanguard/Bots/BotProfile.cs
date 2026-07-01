namespace VanguardThaiSim.Bots
{
    public sealed class BotProfile
    {
        public BotProfileType Type { get; }
        public int RearGuardTarget { get; }
        public bool EntersBattle { get; }

        private BotProfile(BotProfileType type, int rearGuardTarget, bool entersBattle)
        {
            Type = type;
            RearGuardTarget = rearGuardTarget;
            EntersBattle = entersBattle;
        }

        public static BotProfile Create(BotProfileType type)
        {
            switch (type)
            {
                case BotProfileType.Aggro:
                    return new BotProfile(type, 5, true);
                case BotProfileType.Balanced:
                    return new BotProfile(type, 3, true);
                case BotProfileType.Defensive:
                    return new BotProfile(type, 1, false);
                default:
                    return new BotProfile(BotProfileType.Balanced, 3, true);
            }
        }
    }
}
