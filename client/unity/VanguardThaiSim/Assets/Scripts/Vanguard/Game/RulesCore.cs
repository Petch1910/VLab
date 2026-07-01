using System;
using System.Collections.Generic;

namespace VanguardThaiSim.Game
{
    public static class RulesCore
    {
        public static IReadOnlyList<LegalGameAction> GetLegalActions(GameState state, int playerIndex)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            return LegalActionGenerator.Generate(state, playerIndex);
        }

        public static bool CanExecute(GameState state, LegalGameAction action)
        {
            return TryFindLegalAction(state, action, out _);
        }

        public static RulesCommandResult TryExecute(GameState state, LegalGameAction action)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (action == null)
            {
                return RulesCommandResult.Rejected("Action is null.");
            }

            if (!TryFindLegalAction(state, action, out LegalGameAction legalAction))
            {
                return RulesCommandResult.Rejected("Action is not legal in the current state.");
            }

            try
            {
                return RulesCommandResult.Accepted(LegalActionExecutor.Execute(state, legalAction));
            }
            catch (Exception exception)
            {
                return RulesCommandResult.Rejected(exception.Message);
            }
        }

        public static GameEvent ExecuteOrThrow(GameState state, LegalGameAction action)
        {
            RulesCommandResult result = TryExecute(state, action);
            if (!result.accepted)
            {
                throw new InvalidOperationException(result.rejection_reason);
            }

            return result.game_event;
        }

        private static bool TryFindLegalAction(GameState state, LegalGameAction action, out LegalGameAction legalAction)
        {
            legalAction = null;
            if (state == null || action == null)
            {
                return false;
            }

            foreach (LegalGameAction candidate in GetLegalActions(state, action.actor_index))
            {
                if (Matches(candidate, action))
                {
                    legalAction = candidate;
                    return true;
                }
            }

            return false;
        }

        private static bool Matches(LegalGameAction candidate, LegalGameAction requested)
        {
            if (candidate.action_type != requested.action_type || candidate.actor_index != requested.actor_index)
            {
                return false;
            }

            switch (candidate.action_type)
            {
                case GameActionType.Draw:
                    return true;
                case GameActionType.MoveCard:
                    return candidate.card_instance_id == requested.card_instance_id &&
                        candidate.from_zone == requested.from_zone &&
                        candidate.to_zone == requested.to_zone;
                case GameActionType.SetPhase:
                    return candidate.phase == requested.phase;
                case GameActionType.AddGiftMarker:
                    return candidate.gift_marker_type == requested.gift_marker_type &&
                        NormalizeMarkerDelta(candidate.marker_delta) == NormalizeMarkerDelta(requested.marker_delta);
                case GameActionType.ResourceFlip:
                    return candidate.card_instance_id == requested.card_instance_id &&
                        candidate.from_zone == GameZone.Damage &&
                        requested.from_zone == GameZone.Damage &&
                        candidate.to_zone == GameZone.Damage &&
                        requested.to_zone == GameZone.Damage &&
                        candidate.resource_operation_type == requested.resource_operation_type;
                case GameActionType.DeclareAttack:
                    return candidate.card_instance_id == requested.card_instance_id &&
                        candidate.target_card_instance_id == requested.target_card_instance_id;
                case GameActionType.Guard:
                    return candidate.card_instance_id == requested.card_instance_id;
                case GameActionType.TriggerCheck:
                    return candidate.from_zone == requested.from_zone &&
                        candidate.to_zone == requested.to_zone &&
                        MatchesTriggerCheckSource(candidate.trigger_check_source, requested.trigger_check_source);
                case GameActionType.MulliganCards:
                    if (candidate.card_instance_ids == null && requested.card_instance_ids == null) return true;
                    if (candidate.card_instance_ids == null || requested.card_instance_ids == null) return false;
                    if (candidate.card_instance_ids.Count != requested.card_instance_ids.Count) return false;
                    for (int i = 0; i < candidate.card_instance_ids.Count; i++)
                    {
                        if (candidate.card_instance_ids[i] != requested.card_instance_ids[i]) return false;
                    }
                    return true;
                default:
                    return false;
            }
        }

        private static int NormalizeMarkerDelta(int markerDelta)
        {
            return markerDelta <= 0 ? 1 : markerDelta;
        }

        private static bool MatchesTriggerCheckSource(
            TriggerCheckSource candidateSource,
            TriggerCheckSource requestedSource)
        {
            return requestedSource == TriggerCheckSource.Manual ||
                   candidateSource == requestedSource;
        }
    }
}
