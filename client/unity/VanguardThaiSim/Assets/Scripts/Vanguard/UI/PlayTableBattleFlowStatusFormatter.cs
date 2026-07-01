using System.Collections.Generic;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class PlayTableBattleFlowStatusFormatter
    {
        public const string NoStateMessage = "Battle flow: no game loaded.";
        public const string NotBattlePhaseMessage = "Battle flow: enter Battle phase to attack.";
        public const string ReadyMessage = "Battle flow: select attacker and target, then attack.";
        public const string AttackDeclaredMessage = "Battle flow: attack declared. Opponent guard step is next.";
        public const string GuardPlacedMessage = "Battle flow: guard placed. Continue checks or resolve manually.";
        public const string TriggerCheckedMessage = "Battle flow: trigger checked. Apply modifiers, then resolve manually.";

        public static string Format(GameState state)
        {
            if (state == null)
            {
                return NoStateMessage;
            }

            if (state.phase != GamePhase.Battle)
            {
                return NotBattlePhaseMessage;
            }

            GameEvent latest = LatestVisibleEvent(state.event_log);
            if (latest == null)
            {
                return ReadyMessage;
            }

            switch (latest.action_type)
            {
                case GameActionType.DeclareAttack:
                    return AttackDeclaredMessage;
                case GameActionType.Guard:
                    return GuardPlacedMessage;
                case GameActionType.TriggerCheck:
                    return TriggerCheckedMessage;
                default:
                    return ReadyMessage;
            }
        }

        private static GameEvent LatestVisibleEvent(IReadOnlyList<GameEvent> events)
        {
            if (events == null || events.Count == 0)
            {
                return null;
            }

            return events[events.Count - 1];
        }
    }
}
