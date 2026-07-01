using System.Collections.Generic;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    public static class EasyBotController
    {
        public static IReadOnlyList<GameEvent> PlayTurn(GameState state, int playerIndex)
        {
            List<GameEvent> events = new List<GameEvent>();

            ExecuteIfAvailable(state, playerIndex, events, PickDraw);
            ExecuteIfAvailable(state, playerIndex, events, PickFirstMoveToVanguard);

            while (state.GetPlayer(playerIndex).rear_guard.Count < 3)
            {
                if (!ExecuteIfAvailable(state, playerIndex, events, PickFirstMoveToRearGuard))
                {
                    break;
                }
            }

            ExecuteIfAvailable(state, playerIndex, events, PickBattlePhase);
            ExecuteIfAvailable(state, playerIndex, events, PickEndPhase);
            return events;
        }

        public static BotDecision DecideNext(GameState state, int playerIndex)
        {
            return DecideNext(BotDecisionContextFactory.Create(state, playerIndex));
        }

        public static BotDecision DecideNext(BotDecisionContext context)
        {
            return
                PickDraw(context) ??
                PickFirstMoveToVanguard(context) ??
                PickFirstMoveToRearGuard(context) ??
                PickBattlePhase(context) ??
                PickEndPhase(context);
        }

        private static bool ExecuteIfAvailable(
            GameState state,
            int playerIndex,
            List<GameEvent> events,
            System.Func<GameState, int, BotDecision> picker)
        {
            BotDecision decision = picker(
                BotDecisionContextFactory.Create(state, playerIndex).MaskedState,
                playerIndex);
            if (decision == null || decision.Action == null)
            {
                return false;
            }

            events.Add(RulesCore.ExecuteOrThrow(state, decision.Action));
            return true;
        }

        private static BotDecision PickDraw(GameState state, int playerIndex)
        {
            return PickDraw(BotDecisionContextFactory.Create(state, playerIndex));
        }

        private static BotDecision PickDraw(BotDecisionContext context)
        {
            foreach (LegalGameAction action in context.LegalActions)
            {
                if (action.action_type == GameActionType.Draw)
                {
                    return new BotDecision(action, "draw one card");
                }
            }

            return null;
        }

        private static BotDecision PickFirstMoveToVanguard(GameState state, int playerIndex)
        {
            return PickFirstMoveToVanguard(BotDecisionContextFactory.Create(state, playerIndex));
        }

        private static BotDecision PickFirstMoveToVanguard(BotDecisionContext context)
        {
            if (context.MaskedState.GetPlayer(context.PlayerIndex).vanguard.Count > 0)
            {
                return null;
            }

            return PickFirstMove(context, GameZone.Hand, GameZone.Vanguard, "place first hand card as vanguard");
        }

        private static BotDecision PickFirstMoveToRearGuard(GameState state, int playerIndex)
        {
            return PickFirstMoveToRearGuard(BotDecisionContextFactory.Create(state, playerIndex));
        }

        private static BotDecision PickFirstMoveToRearGuard(BotDecisionContext context)
        {
            return PickFirstMove(context, GameZone.Hand, GameZone.RearGuard, "call first hand card to rear-guard");
        }

        private static BotDecision PickFirstMove(GameState state, int playerIndex, GameZone from, GameZone to, string reason)
        {
            return PickFirstMove(BotDecisionContextFactory.Create(state, playerIndex), from, to, reason);
        }

        private static BotDecision PickFirstMove(BotDecisionContext context, GameZone from, GameZone to, string reason)
        {
            foreach (LegalGameAction action in context.LegalActions)
            {
                if (action.action_type == GameActionType.MoveCard && action.from_zone == from && action.to_zone == to)
                {
                    return new BotDecision(action, reason);
                }
            }

            return null;
        }

        private static BotDecision PickBattlePhase(GameState state, int playerIndex)
        {
            return PickBattlePhase(BotDecisionContextFactory.Create(state, playerIndex));
        }

        private static BotDecision PickBattlePhase(BotDecisionContext context)
        {
            return PickPhase(context, GamePhase.Battle, "enter battle phase");
        }

        private static BotDecision PickEndPhase(GameState state, int playerIndex)
        {
            return PickEndPhase(BotDecisionContextFactory.Create(state, playerIndex));
        }

        private static BotDecision PickEndPhase(BotDecisionContext context)
        {
            return PickPhase(context, GamePhase.End, "end turn");
        }

        private static BotDecision PickPhase(GameState state, int playerIndex, GamePhase phase, string reason)
        {
            return PickPhase(BotDecisionContextFactory.Create(state, playerIndex), phase, reason);
        }

        private static BotDecision PickPhase(BotDecisionContext context, GamePhase phase, string reason)
        {
            foreach (LegalGameAction action in context.LegalActions)
            {
                if (action.action_type == GameActionType.SetPhase && action.phase == phase)
                {
                    return new BotDecision(action, reason);
                }
            }

            return null;
        }
    }
}
