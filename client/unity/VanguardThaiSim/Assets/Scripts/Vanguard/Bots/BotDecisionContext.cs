using System;
using System.Collections.Generic;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Bots
{
    public sealed class BotDecisionContext
    {
        public int PlayerIndex { get; private set; }
        public GameState MaskedState { get; private set; }
        public IReadOnlyList<LegalGameAction> LegalActions { get; private set; }
        public string Source { get; private set; }

        public BotDecisionContext(
            int playerIndex,
            GameState maskedState,
            IReadOnlyList<LegalGameAction> legalActions,
            string source)
        {
            if (maskedState == null)
            {
                throw new ArgumentNullException(nameof(maskedState));
            }

            PlayerIndex = playerIndex;
            MaskedState = maskedState;
            LegalActions = legalActions ?? new List<LegalGameAction>();
            Source = source ?? string.Empty;
        }
    }

    public static class BotDecisionContextFactory
    {
        public static BotDecisionContext Create(GameState trueState, int playerIndex)
        {
            if (trueState == null)
            {
                throw new ArgumentNullException(nameof(trueState));
            }

            GameState maskedState = GameStateViewFactory.CreatePlayerView(trueState, playerIndex);
            IReadOnlyList<LegalGameAction> legalActions =
                RulesCore.GetLegalActions(maskedState, playerIndex);
            return new BotDecisionContext(
                playerIndex,
                maskedState,
                CloneActions(legalActions),
                "player-masked-view");
        }

        private static IReadOnlyList<LegalGameAction> CloneActions(
            IReadOnlyList<LegalGameAction> actions)
        {
            List<LegalGameAction> clones = new List<LegalGameAction>();
            if (actions == null)
            {
                return clones;
            }

            for (int i = 0; i < actions.Count; i++)
            {
                LegalGameAction action = actions[i];
                if (action == null)
                {
                    continue;
                }

                clones.Add(CloneAction(action));
            }

            return clones;
        }

        private static LegalGameAction CloneAction(LegalGameAction action)
        {
            LegalGameAction clone = new LegalGameAction
            {
                label = action.label,
                action_type = action.action_type,
                actor_index = action.actor_index,
                card_instance_id = action.card_instance_id,
                target_card_instance_id = action.target_card_instance_id,
                from_zone = action.from_zone,
                to_zone = action.to_zone,
                phase = action.phase,
                gift_marker_type = action.gift_marker_type,
                marker_delta = action.marker_delta,
                resource_operation_type = action.resource_operation_type,
                resource_delta = action.resource_delta,
                trigger_check_source = action.trigger_check_source
            };
            if (action.card_instance_ids != null)
            {
                clone.card_instance_ids = new List<string>(action.card_instance_ids);
            }

            return clone;
        }
    }
}
