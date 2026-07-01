using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class TriggerAllocationTarget
    {
        public string card_instance_id;
        public string card_id;
        public GameZone zone;
        public int zone_index;
        public int current_power_delta;
        public int power_bonus;
        public int critical_bonus;
    }

    [Serializable]
    public sealed class TriggerAllocationPlan
    {
        public bool accepted;
        public bool needs_manual_resolution;
        public string rejection_reason;
        public TriggerType trigger_type;
        public List<TriggerAllocationTarget> power_targets = new List<TriggerAllocationTarget>();
        public List<TriggerAllocationTarget> critical_targets = new List<TriggerAllocationTarget>();
        public List<string> side_effect_notes = new List<string>();
        public List<string> marker_notes = new List<string>();
        public string explanation;

        public void EnsureLists()
        {
            if (power_targets == null) power_targets = new List<TriggerAllocationTarget>();
            if (critical_targets == null) critical_targets = new List<TriggerAllocationTarget>();
            if (side_effect_notes == null) side_effect_notes = new List<string>();
            if (marker_notes == null) marker_notes = new List<string>();
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static TriggerAllocationPlan FromJson(string json)
        {
            TriggerAllocationPlan plan = JsonUtility.FromJson<TriggerAllocationPlan>(json);
            if (plan == null)
            {
                throw new ArgumentException("Trigger allocation plan JSON could not be parsed.", "json");
            }

            plan.EnsureLists();
            return plan;
        }

        internal static TriggerAllocationPlan Accepted(TriggerType triggerType, string explanation)
        {
            return new TriggerAllocationPlan
            {
                accepted = true,
                needs_manual_resolution = false,
                rejection_reason = string.Empty,
                trigger_type = triggerType,
                explanation = explanation ?? string.Empty
            };
        }

        internal static TriggerAllocationPlan NeedsManualResolution(TriggerType triggerType, string reason)
        {
            return new TriggerAllocationPlan
            {
                accepted = false,
                needs_manual_resolution = true,
                rejection_reason = reason ?? string.Empty,
                trigger_type = triggerType,
                explanation = reason ?? string.Empty
            };
        }
    }

    public static class TriggerAllocationPlanner
    {
        public static TriggerAllocationPlan Plan(
            GameState state,
            int playerIndex,
            TriggerResolveResult triggerResult)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            if (triggerResult == null)
            {
                return TriggerAllocationPlan.NeedsManualResolution(
                    TriggerType.Unknown,
                    "Trigger result is required.");
            }

            if (!triggerResult.accepted)
            {
                return TriggerAllocationPlan.NeedsManualResolution(
                    triggerResult.trigger_type,
                    triggerResult.rejection_reason);
            }

            PlayerGameState player = state.GetPlayer(playerIndex);
            List<VisibleTriggerUnit> visibleUnits = CollectVisibleUnits(player);
            TriggerAllocationPlan plan = TriggerAllocationPlan.Accepted(
                triggerResult.trigger_type,
                "advisory trigger allocation plan; no state mutation");

            switch (triggerResult.trigger_type)
            {
                case TriggerType.None:
                    plan.side_effect_notes.Add("No trigger bonuses to allocate.");
                    break;
                case TriggerType.Critical:
                    AddPowerTarget(plan, BestPowerTarget(visibleUnits), triggerResult.power_bonus);
                    AddCriticalTarget(plan, PreferredVanguardTarget(visibleUnits), triggerResult.critical_bonus);
                    break;
                case TriggerType.Draw:
                    AddPowerTarget(plan, BestPowerTarget(visibleUnits), triggerResult.power_bonus);
                    if (triggerResult.draw_cards > 0)
                    {
                        plan.side_effect_notes.Add("Draw " + triggerResult.draw_cards + " card(s) after trigger resolution.");
                    }
                    break;
                case TriggerType.Heal:
                    AddPowerTarget(plan, BestPowerTarget(visibleUnits), triggerResult.power_bonus);
                    if (triggerResult.heal_attempt)
                    {
                        plan.side_effect_notes.Add("Attempt heal if damage condition is legal.");
                    }
                    break;
                case TriggerType.Front:
                    AddFrontPowerTargets(plan, visibleUnits, triggerResult.front_row_power_bonus);
                    break;
                case TriggerType.Over:
                    AddPowerTarget(plan, PreferredVanguardTarget(visibleUnits), triggerResult.power_bonus);
                    if (triggerResult.over_trigger)
                    {
                        plan.marker_notes.Add("Over trigger resolved; nation-specific extra effects remain manual.");
                    }
                    break;
                case TriggerType.Unknown:
                default:
                    return TriggerAllocationPlan.NeedsManualResolution(
                        triggerResult.trigger_type,
                        "Unknown trigger allocation requires manual resolution.");
            }

            if (visibleUnits.Count == 0 && triggerResult.trigger_type != TriggerType.None)
            {
                plan.side_effect_notes.Add("No visible friendly unit target is available.");
            }

            return plan;
        }

        private static List<VisibleTriggerUnit> CollectVisibleUnits(PlayerGameState player)
        {
            var units = new List<VisibleTriggerUnit>();
            AddVisibleUnits(units, player.vanguard, GameZone.Vanguard);
            AddVisibleUnits(units, player.rear_guard, GameZone.RearGuard);
            units.Sort(CompareVisibleUnitsForPower);
            return units;
        }

        private static void AddVisibleUnits(
            List<VisibleTriggerUnit> units,
            IList<GameCardInstance> cards,
            GameZone zone)
        {
            if (cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                GameCardInstance card = cards[i];
                if (!IsVisibleKnownCard(card))
                {
                    continue;
                }

                units.Add(new VisibleTriggerUnit(card, zone, i));
            }
        }

        private static bool IsVisibleKnownCard(GameCardInstance card)
        {
            return card != null &&
                   card.face_up &&
                   !string.IsNullOrEmpty(card.card_id) &&
                   card.card_id != GameStateViewFactory.HiddenCardId;
        }

        private static VisibleTriggerUnit BestPowerTarget(List<VisibleTriggerUnit> units)
        {
            return units.Count == 0 ? null : units[0];
        }

        private static VisibleTriggerUnit PreferredVanguardTarget(List<VisibleTriggerUnit> units)
        {
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].Zone == GameZone.Vanguard)
                {
                    return units[i];
                }
            }

            return BestPowerTarget(units);
        }

        private static void AddPowerTarget(
            TriggerAllocationPlan plan,
            VisibleTriggerUnit unit,
            int powerBonus)
        {
            if (unit == null || powerBonus <= 0)
            {
                return;
            }

            plan.power_targets.Add(unit.ToTarget(powerBonus, 0));
        }

        private static void AddCriticalTarget(
            TriggerAllocationPlan plan,
            VisibleTriggerUnit unit,
            int criticalBonus)
        {
            if (unit == null || criticalBonus <= 0)
            {
                return;
            }

            plan.critical_targets.Add(unit.ToTarget(0, criticalBonus));
        }

        private static void AddFrontPowerTargets(
            TriggerAllocationPlan plan,
            List<VisibleTriggerUnit> units,
            int frontRowPowerBonus)
        {
            if (frontRowPowerBonus <= 0)
            {
                return;
            }

            for (int i = 0; i < units.Count; i++)
            {
                plan.power_targets.Add(units[i].ToTarget(frontRowPowerBonus, 0));
            }

            if (units.Count > 0)
            {
                plan.side_effect_notes.Add(
                    "Front trigger applies to visible vanguard/rear-guard units until circle position data exists.");
            }
        }

        private static int CompareVisibleUnitsForPower(VisibleTriggerUnit left, VisibleTriggerUnit right)
        {
            int powerCompare = right.CurrentPowerDelta.CompareTo(left.CurrentPowerDelta);
            if (powerCompare != 0)
            {
                return powerCompare;
            }

            int zoneCompare = ZoneRank(left.Zone).CompareTo(ZoneRank(right.Zone));
            if (zoneCompare != 0)
            {
                return zoneCompare;
            }

            return left.ZoneIndex.CompareTo(right.ZoneIndex);
        }

        private static int ZoneRank(GameZone zone)
        {
            return zone == GameZone.Vanguard ? 0 : 1;
        }

        private sealed class VisibleTriggerUnit
        {
            private readonly GameCardInstance card;

            public GameZone Zone { get; private set; }
            public int ZoneIndex { get; private set; }
            public int CurrentPowerDelta
            {
                get { return card.power_delta; }
            }

            public VisibleTriggerUnit(GameCardInstance card, GameZone zone, int zoneIndex)
            {
                this.card = card;
                Zone = zone;
                ZoneIndex = zoneIndex;
            }

            public TriggerAllocationTarget ToTarget(int powerBonus, int criticalBonus)
            {
                return new TriggerAllocationTarget
                {
                    card_instance_id = card.instance_id,
                    card_id = card.card_id,
                    zone = Zone,
                    zone_index = ZoneIndex,
                    current_power_delta = card.power_delta,
                    power_bonus = powerBonus,
                    critical_bonus = criticalBonus
                };
            }
        }
    }
}
