using System;

namespace VanguardThaiSim.Game
{
    public enum TriggerType
    {
        None,
        Critical,
        Draw,
        Front,
        Heal,
        Over,
        Unknown
    }

    [Serializable]
    public sealed class TriggerDefinition
    {
        public TriggerType trigger_type;
        public int power_bonus;
        public int critical_bonus;
        public int draw_cards;
        public int front_row_power_bonus;
        public bool heal_attempt;
        public bool over_trigger;
        public string label;

        public static TriggerDefinition ForType(TriggerType triggerType)
        {
            switch (triggerType)
            {
                case TriggerType.None:
                    return new TriggerDefinition { trigger_type = triggerType, label = "No trigger" };
                case TriggerType.Critical:
                    return new TriggerDefinition
                    {
                        trigger_type = triggerType,
                        power_bonus = 10000,
                        critical_bonus = 1,
                        label = "Critical trigger"
                    };
                case TriggerType.Draw:
                    return new TriggerDefinition
                    {
                        trigger_type = triggerType,
                        power_bonus = 10000,
                        draw_cards = 1,
                        label = "Draw trigger"
                    };
                case TriggerType.Front:
                    return new TriggerDefinition
                    {
                        trigger_type = triggerType,
                        front_row_power_bonus = 10000,
                        label = "Front trigger"
                    };
                case TriggerType.Heal:
                    return new TriggerDefinition
                    {
                        trigger_type = triggerType,
                        power_bonus = 10000,
                        heal_attempt = true,
                        label = "Heal trigger"
                    };
                case TriggerType.Over:
                    return new TriggerDefinition
                    {
                        trigger_type = triggerType,
                        power_bonus = 100000000,
                        over_trigger = true,
                        label = "Over trigger"
                    };
                case TriggerType.Unknown:
                default:
                    return new TriggerDefinition
                    {
                        trigger_type = TriggerType.Unknown,
                        label = "Unknown trigger"
                    };
            }
        }
    }

    [Serializable]
    public sealed class TriggerResolveResult
    {
        public bool accepted;
        public bool needs_manual_resolution;
        public string rejection_reason;
        public TriggerType trigger_type;
        public int power_bonus;
        public int critical_bonus;
        public int draw_cards;
        public int front_row_power_bonus;
        public bool heal_attempt;
        public bool over_trigger;
        public string explanation;

        public static TriggerResolveResult FromDefinition(TriggerDefinition definition)
        {
            if (definition == null || definition.trigger_type == TriggerType.Unknown)
            {
                return NeedsManualResolution("Unknown trigger type.");
            }

            return new TriggerResolveResult
            {
                accepted = true,
                needs_manual_resolution = false,
                rejection_reason = string.Empty,
                trigger_type = definition.trigger_type,
                power_bonus = Math.Max(0, definition.power_bonus),
                critical_bonus = Math.Max(0, definition.critical_bonus),
                draw_cards = Math.Max(0, definition.draw_cards),
                front_row_power_bonus = Math.Max(0, definition.front_row_power_bonus),
                heal_attempt = definition.heal_attempt,
                over_trigger = definition.over_trigger,
                explanation = definition.label ?? string.Empty
            };
        }

        public static TriggerResolveResult NeedsManualResolution(string reason)
        {
            return new TriggerResolveResult
            {
                accepted = false,
                needs_manual_resolution = true,
                rejection_reason = reason ?? string.Empty,
                trigger_type = TriggerType.Unknown,
                explanation = reason ?? string.Empty
            };
        }
    }

    public static class TriggerResolver
    {
        public static TriggerResolveResult Resolve(TriggerType triggerType)
        {
            return TriggerResolveResult.FromDefinition(TriggerDefinition.ForType(triggerType));
        }
    }
}
