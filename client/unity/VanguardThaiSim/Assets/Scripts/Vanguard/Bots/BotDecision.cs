using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    public sealed class BotDecision
    {
        public LegalGameAction Action { get; }
        public string Reason { get; }

        public BotDecision(LegalGameAction action, string reason)
        {
            Action = action;
            Reason = reason;
        }
    }
}
