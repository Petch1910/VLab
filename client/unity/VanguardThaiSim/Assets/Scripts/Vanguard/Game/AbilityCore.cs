using System;

namespace VanguardThaiSim.Game
{
    public static class AbilityCore
    {
        public static AbilityResolutionResult Resolve(GameState state, int playerIndex, AbilityDefinition ability)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (ability == null)
            {
                return AbilityResolutionResult.Rejected("Ability is null.");
            }

            ability.EnsureLists();
            LiveEffectResolutionPolicyValidationResult policyResult =
                LiveEffectResolutionTextParsingGuard.ValidateAbilityDefinition(ability);
            if (!policyResult.accepted)
            {
                return policyResult.requires_manual_resolution
                    ? AbilityResolutionResult.NeedsManualResolution(policyResult.rejection_reason)
                    : AbilityResolutionResult.Rejected(policyResult.rejection_reason);
            }

            if (ability.effects.Count == 0)
            {
                return ability.manual_fallback
                    ? AbilityResolutionResult.NeedsManualResolution("Ability has no structured effects.")
                    : AbilityResolutionResult.Rejected("Ability has no structured effects.");
            }

            GameState branch = GameStateSnapshot.Capture(state).Clone();
            AbilityResolutionResult preview = ResolveInto(branch, playerIndex, ability);
            if (!preview.accepted)
            {
                return preview;
            }

            return ResolveInto(state, playerIndex, ability);
        }

        private static AbilityResolutionResult ResolveInto(GameState state, int playerIndex, AbilityDefinition ability)
        {
            AbilityResolutionResult result = AbilityResolutionResult.Accepted();
            foreach (AbilityEffectDefinition effect in ability.effects)
            {
                AbilityResolutionResult effectResult = ResolveEffect(state, playerIndex, ability, effect);
                if (!effectResult.accepted)
                {
                    return effectResult;
                }

                result.events.AddRange(effectResult.events);
            }

            return result;
        }

        private static AbilityResolutionResult ResolveEffect(
            GameState state,
            int playerIndex,
            AbilityDefinition ability,
            AbilityEffectDefinition effect)
        {
            if (effect == null)
            {
                return AbilityResolutionResult.Rejected("Ability effect is null.");
            }

            int amount = effect.amount <= 0 ? 1 : effect.amount;
            switch (effect.effect_type)
            {
                case AbilityEffectType.Draw:
                    return RepeatCommand(state, amount, () => FirstAction(state, playerIndex, GameActionType.Draw));
                case AbilityEffectType.AddGiftMarker:
                    return RepeatCommand(
                        state,
                        amount,
                        () => new LegalGameAction
                        {
                            action_type = GameActionType.AddGiftMarker,
                            actor_index = playerIndex,
                            gift_marker_type = effect.gift_marker_type,
                            marker_delta = 1
                        });
                case AbilityEffectType.SetPhase:
                    return ExecuteCommand(
                        state,
                        new LegalGameAction
                        {
                            action_type = GameActionType.SetPhase,
                            actor_index = playerIndex,
                            phase = effect.phase
                        });
                case AbilityEffectType.MoveFirstFromZoneToZone:
                    return ExecuteCommand(state, FirstMove(state, playerIndex, effect.from_zone, effect.to_zone));
                case AbilityEffectType.Custom:
                    if (AbilityEffectRegistry.TryResolve(effect.custom_effect_id, state, playerIndex, ability, effect, out AbilityResolutionResult customResult))
                    {
                        return customResult;
                    }

                    return ability.manual_fallback
                        ? AbilityResolutionResult.NeedsManualResolution("Ability effect is not structured yet.")
                        : AbilityResolutionResult.Rejected("Missing custom ability effect handler.");
                default:
                    return AbilityResolutionResult.Rejected("Unsupported ability effect type.");
            }
        }

        private static AbilityResolutionResult RepeatCommand(GameState state, int count, Func<LegalGameAction> commandFactory)
        {
            AbilityResolutionResult result = AbilityResolutionResult.Accepted();
            for (int i = 0; i < count; i++)
            {
                AbilityResolutionResult step = ExecuteCommand(state, commandFactory());
                if (!step.accepted)
                {
                    return step;
                }

                result.events.AddRange(step.events);
            }

            return result;
        }

        private static AbilityResolutionResult ExecuteCommand(GameState state, LegalGameAction action)
        {
            RulesCommandResult commandResult = RulesCore.TryExecute(state, action);
            if (!commandResult.accepted)
            {
                return AbilityResolutionResult.Rejected(commandResult.rejection_reason);
            }

            AbilityResolutionResult result = AbilityResolutionResult.Accepted();
            result.events.Add(commandResult.game_event);
            return result;
        }

        private static LegalGameAction FirstAction(GameState state, int playerIndex, GameActionType actionType)
        {
            foreach (LegalGameAction action in RulesCore.GetLegalActions(state, playerIndex))
            {
                if (action.action_type == actionType)
                {
                    return action;
                }
            }

            return null;
        }

        private static LegalGameAction FirstMove(GameState state, int playerIndex, GameZone from, GameZone to)
        {
            foreach (LegalGameAction action in RulesCore.GetLegalActions(state, playerIndex))
            {
                if (action.action_type == GameActionType.MoveCard && action.from_zone == from && action.to_zone == to)
                {
                    return action;
                }
            }

            return null;
        }
    }
}
