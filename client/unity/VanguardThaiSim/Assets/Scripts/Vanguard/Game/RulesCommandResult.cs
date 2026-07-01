namespace VanguardThaiSim.Game
{
    public sealed class RulesCommandResult
    {
        public bool accepted;
        public string rejection_reason;
        public GameEvent game_event;

        private RulesCommandResult(bool accepted, string rejectionReason, GameEvent gameEvent)
        {
            this.accepted = accepted;
            rejection_reason = rejectionReason;
            game_event = gameEvent;
        }

        public static RulesCommandResult Accepted(GameEvent gameEvent)
        {
            return new RulesCommandResult(true, string.Empty, gameEvent);
        }

        public static RulesCommandResult Rejected(string reason)
        {
            return new RulesCommandResult(false, reason, null);
        }
    }
}
