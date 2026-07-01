using System;
using System.Collections.Generic;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    public static class ProfileBotController
    {
        public static IReadOnlyList<GameEvent> PlayTurn(GameState state, int playerIndex, BotProfile profile, int seed)
        {
            if (profile == null)
            {
                profile = BotProfile.Create(BotProfileType.Balanced);
            }

            SeededRandomService random = new SeededRandomService(seed);
            List<GameEvent> events = new List<GameEvent>();

            ExecuteIfAvailable(state, playerIndex, events, PickDraw);
            ExecuteIfAvailable(state, playerIndex, events, delegate(GameState s, int p)
            {
                if (s.GetPlayer(p).vanguard.Count > 0)
                {
                    return null;
                }

                return PickRandomMove(s, p, GameZone.Hand, GameZone.Vanguard, random);
            });

            while (state.GetPlayer(playerIndex).rear_guard.Count < profile.RearGuardTarget)
            {
                bool executed = ExecuteIfAvailable(state, playerIndex, events, delegate(GameState s, int p)
                {
                    return PickRandomMove(s, p, GameZone.Hand, GameZone.RearGuard, random);
                });
                if (!executed)
                {
                    break;
                }
            }

            if (profile.EntersBattle)
            {
                ExecuteIfAvailable(state, playerIndex, events, delegate(GameState s, int p)
                {
                    return PickPhase(s, p, GamePhase.Battle);
                });
            }

            ExecuteIfAvailable(state, playerIndex, events, delegate(GameState s, int p)
            {
                return PickPhase(s, p, GamePhase.End);
            });

            return events;
        }

        private static bool ExecuteIfAvailable(
            GameState state,
            int playerIndex,
            List<GameEvent> events,
            Func<GameState, int, LegalGameAction> picker)
        {
            LegalGameAction action = picker(
                BotDecisionContextFactory.Create(state, playerIndex).MaskedState,
                playerIndex);
            if (action == null)
            {
                return false;
            }

            events.Add(RulesCore.ExecuteOrThrow(state, action));
            return true;
        }

        private static LegalGameAction PickDraw(GameState state, int playerIndex)
        {
            return PickDraw(BotDecisionContextFactory.Create(state, playerIndex));
        }

        private static LegalGameAction PickDraw(BotDecisionContext context)
        {
            foreach (LegalGameAction action in context.LegalActions)
            {
                if (action.action_type == GameActionType.Draw)
                {
                    return action;
                }
            }

            return null;
        }

        private static LegalGameAction PickPhase(GameState state, int playerIndex, GamePhase phase)
        {
            return PickPhase(BotDecisionContextFactory.Create(state, playerIndex), phase);
        }

        private static LegalGameAction PickPhase(BotDecisionContext context, GamePhase phase)
        {
            foreach (LegalGameAction action in context.LegalActions)
            {
                if (action.action_type == GameActionType.SetPhase && action.phase == phase)
                {
                    return action;
                }
            }

            return null;
        }

        private static LegalGameAction PickRandomMove(GameState state, int playerIndex, GameZone from, GameZone to, SeededRandomService random)
        {
            return PickRandomMove(BotDecisionContextFactory.Create(state, playerIndex), from, to, random);
        }

        private static LegalGameAction PickRandomMove(BotDecisionContext context, GameZone from, GameZone to, SeededRandomService random)
        {
            List<LegalGameAction> matches = new List<LegalGameAction>();
            foreach (LegalGameAction action in context.LegalActions)
            {
                if (action.action_type == GameActionType.MoveCard && action.from_zone == from && action.to_zone == to)
                {
                    matches.Add(action);
                }
            }

            if (matches.Count == 0)
            {
                return null;
            }

            return matches[random.Next(matches.Count)];
        }
    }
}
