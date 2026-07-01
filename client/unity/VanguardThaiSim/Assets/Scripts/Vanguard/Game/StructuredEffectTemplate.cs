using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class StructuredEffectTemplateRejectionReasons
    {
        public const string StateMissing = "STRUCTURED_EFFECT_STATE_MISSING";
        public const string EffectMissing = "STRUCTURED_EFFECT_MISSING";
        public const string NegativeAmount = "STRUCTURED_EFFECT_NEGATIVE_AMOUNT";
        public const string UnsupportedEffectType = "STRUCTURED_EFFECT_UNSUPPORTED_TYPE";
        public const string UnsupportedZone = "STRUCTURED_EFFECT_UNSUPPORTED_ZONE";
        public const string NoLegalCommand = "STRUCTURED_EFFECT_NO_LEGAL_COMMAND";
    }

    [Serializable]
    public sealed class StructuredEffectTemplateResult
    {
        public bool accepted;
        public string rejection_reason;
        public bool requires_manual_resolution;
        public bool preview_only;
        public List<GameEvent> events = new List<GameEvent>();
        public string summary;

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static StructuredEffectTemplateResult FromJson(string json)
        {
            StructuredEffectTemplateResult result =
                JsonUtility.FromJson<StructuredEffectTemplateResult>(json);
            if (result == null)
            {
                throw new ArgumentException("Structured effect template result JSON could not be parsed.", "json");
            }

            result.EnsureLists();
            return result;
        }

        public void EnsureLists()
        {
            if (events == null)
            {
                events = new List<GameEvent>();
            }
        }
    }

    public static class StructuredEffectTemplate
    {
        public static StructuredEffectTemplateResult Preview(
            GameState state,
            int playerIndex,
            StructuredAbilityEffect effect)
        {
            if (state == null)
            {
                return Reject(StructuredEffectTemplateRejectionReasons.StateMissing, false, true);
            }

            GameState branch = GameStateSnapshot.Capture(state).Clone();
            StructuredEffectTemplateResult result = Apply(branch, playerIndex, effect);
            result.preview_only = true;
            return result;
        }

        public static StructuredEffectTemplateResult Apply(
            GameState state,
            int playerIndex,
            StructuredAbilityEffect effect)
        {
            if (state == null)
            {
                return Reject(StructuredEffectTemplateRejectionReasons.StateMissing, false, false);
            }

            if (effect == null)
            {
                return Reject(StructuredEffectTemplateRejectionReasons.EffectMissing, false, false);
            }

            int amount = effect.amount == 0 ? 1 : effect.amount;
            if (amount < 0)
            {
                return Reject(StructuredEffectTemplateRejectionReasons.NegativeAmount, false, false);
            }

            switch (effect.type ?? string.Empty)
            {
                case "draw":
                    return RepeatCommand(state, playerIndex, amount, () => FindAction(state, playerIndex, GameActionType.Draw));
                case "counter_blast":
                    return RepeatCommand(
                        state,
                        playerIndex,
                        amount,
                        () => FindResourceAction(state, playerIndex, GameResourceOperationType.CounterBlast));
                case "counter_charge":
                    return RepeatCommand(
                        state,
                        playerIndex,
                        amount,
                        () => FindResourceAction(state, playerIndex, GameResourceOperationType.CounterCharge));
                case "soul_blast":
                    return RepeatCommand(
                        state,
                        playerIndex,
                        amount,
                        () => FindMoveAction(state, playerIndex, GameZone.Soul, GameZone.Drop));
                case "soul_charge":
                    return RepeatCommand(
                        state,
                        playerIndex,
                        amount,
                        () => FindMoveAction(state, playerIndex, GameZone.Deck, GameZone.Soul));
                case "move_zone":
                    if (!TryParseZone(effect.from_zone, out GameZone fromZone) ||
                        !TryParseZone(effect.to_zone, out GameZone toZone))
                    {
                        return Reject(
                            StructuredEffectTemplateRejectionReasons.UnsupportedZone + ": " +
                            (effect.from_zone ?? string.Empty) + "->" + (effect.to_zone ?? string.Empty),
                            true,
                            false);
                    }

                    return RepeatCommand(
                        state,
                        playerIndex,
                        amount,
                        () => FindMoveAction(state, playerIndex, fromZone, toZone));
                case "manual":
                    return Reject(StructuredEffectTemplateRejectionReasons.UnsupportedEffectType + ": manual", true, false);
                default:
                    return Reject(
                        StructuredEffectTemplateRejectionReasons.UnsupportedEffectType + ": " + (effect.type ?? string.Empty),
                        true,
                        false);
            }
        }

        private static StructuredEffectTemplateResult RepeatCommand(
            GameState state,
            int playerIndex,
            int amount,
            Func<LegalGameAction> commandFactory)
        {
            var events = new List<GameEvent>();
            for (int i = 0; i < amount; i++)
            {
                LegalGameAction action = commandFactory();
                if (action == null)
                {
                    return Reject(StructuredEffectTemplateRejectionReasons.NoLegalCommand, false, false, events);
                }

                RulesCommandResult result = RulesCore.TryExecute(state, action);
                if (!result.accepted)
                {
                    return Reject(
                        StructuredEffectTemplateRejectionReasons.NoLegalCommand + ": " + result.rejection_reason,
                        false,
                        false,
                        events);
                }

                events.Add(CloneEvent(result.game_event));
            }

            return new StructuredEffectTemplateResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                requires_manual_resolution = false,
                preview_only = false,
                events = CloneEvents(events),
                summary = "Structured effect template applied " + events.Count + " command event(s)."
            };
        }

        private static LegalGameAction FindAction(GameState state, int playerIndex, GameActionType actionType)
        {
            IReadOnlyList<LegalGameAction> actions = RulesCore.GetLegalActions(state, playerIndex);
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].action_type == actionType)
                {
                    return actions[i];
                }
            }

            return null;
        }

        private static LegalGameAction FindResourceAction(
            GameState state,
            int playerIndex,
            GameResourceOperationType operationType)
        {
            IReadOnlyList<LegalGameAction> actions = RulesCore.GetLegalActions(state, playerIndex);
            for (int i = 0; i < actions.Count; i++)
            {
                LegalGameAction action = actions[i];
                if (action.action_type == GameActionType.ResourceFlip &&
                    action.resource_operation_type == operationType)
                {
                    return action;
                }
            }

            return null;
        }

        private static LegalGameAction FindMoveAction(
            GameState state,
            int playerIndex,
            GameZone fromZone,
            GameZone toZone)
        {
            IReadOnlyList<LegalGameAction> actions = RulesCore.GetLegalActions(state, playerIndex);
            for (int i = 0; i < actions.Count; i++)
            {
                LegalGameAction action = actions[i];
                if (action.action_type == GameActionType.MoveCard &&
                    action.from_zone == fromZone &&
                    action.to_zone == toZone)
                {
                    return action;
                }
            }

            return null;
        }

        private static bool TryParseZone(string value, out GameZone zone)
        {
            return Enum.TryParse(value ?? string.Empty, false, out zone);
        }

        private static StructuredEffectTemplateResult Reject(
            string rejectionReason,
            bool requiresManualResolution,
            bool previewOnly,
            List<GameEvent> events = null)
        {
            return new StructuredEffectTemplateResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                requires_manual_resolution = requiresManualResolution,
                preview_only = previewOnly,
                events = CloneEvents(events),
                summary = "Structured effect template rejected: " + (rejectionReason ?? string.Empty)
            };
        }

        private static List<GameEvent> CloneEvents(List<GameEvent> events)
        {
            var result = new List<GameEvent>();
            if (events == null)
            {
                return result;
            }

            for (int i = 0; i < events.Count; i++)
            {
                result.Add(CloneEvent(events[i]));
            }

            return result;
        }

        private static GameEvent CloneEvent(GameEvent gameEvent)
        {
            return gameEvent == null
                ? null
                : JsonUtility.FromJson<GameEvent>(JsonUtility.ToJson(gameEvent, false));
        }
    }
}
