using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class CombatStatProjection
    {
        public bool accepted;
        public string rejection_reason;
        public string target_card_instance_id;
        public string card_id;
        public GameZone zone;
        public int zone_index;
        public int current_power_delta;
        public int ledger_power_delta;
        public int ledger_critical_delta;
        public int projected_power_delta_total;
        public int projected_critical_delta_total;
        public int modifier_count;
        public string explanation;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static CombatStatProjection FromJson(string json)
        {
            CombatStatProjection projection = JsonUtility.FromJson<CombatStatProjection>(json);
            if (projection == null)
            {
                throw new ArgumentException("Combat stat projection JSON could not be parsed.", "json");
            }

            return projection;
        }

        internal static CombatStatProjection Rejected(
            string targetCardInstanceId,
            string reason)
        {
            return new CombatStatProjection
            {
                accepted = false,
                rejection_reason = reason ?? string.Empty,
                target_card_instance_id = targetCardInstanceId ?? string.Empty,
                explanation = reason ?? string.Empty
            };
        }
    }

    public static class CombatStatProjector
    {
        public static CombatStatProjection Project(
            GameState state,
            int playerIndex,
            string targetCardInstanceId,
            CombatModifierLedger ledger)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            if (string.IsNullOrEmpty(targetCardInstanceId))
            {
                return CombatStatProjection.Rejected(
                    targetCardInstanceId,
                    "Target card instance id is required.");
            }

            PlayerGameState player = state.GetPlayer(playerIndex);
            if (!TryFindVisibleUnit(player, targetCardInstanceId, out GameCardInstance card, out GameZone zone, out int zoneIndex))
            {
                return CombatStatProjection.Rejected(
                    targetCardInstanceId,
                    "Visible target unit was not found.");
            }

            CombatModifierSummary summary = (ledger ?? new CombatModifierLedger()).Summarize(targetCardInstanceId);
            int projectedPowerDelta = card.power_delta + summary.total_power_delta;
            int projectedCriticalDelta = summary.total_critical_delta;

            return new CombatStatProjection
            {
                accepted = true,
                rejection_reason = string.Empty,
                target_card_instance_id = card.instance_id,
                card_id = card.card_id,
                zone = zone,
                zone_index = zoneIndex,
                current_power_delta = card.power_delta,
                ledger_power_delta = summary.total_power_delta,
                ledger_critical_delta = summary.total_critical_delta,
                projected_power_delta_total = projectedPowerDelta,
                projected_critical_delta_total = projectedCriticalDelta,
                modifier_count = summary.modifier_count,
                explanation = "projected deltas only; printed base stats are not included yet"
            };
        }

        private static bool TryFindVisibleUnit(
            PlayerGameState player,
            string targetCardInstanceId,
            out GameCardInstance card,
            out GameZone zone,
            out int zoneIndex)
        {
            if (TryFindVisibleUnitInZone(player.vanguard, GameZone.Vanguard, targetCardInstanceId, out card, out zone, out zoneIndex))
            {
                return true;
            }

            return TryFindVisibleUnitInZone(player.rear_guard, GameZone.RearGuard, targetCardInstanceId, out card, out zone, out zoneIndex);
        }

        private static bool TryFindVisibleUnitInZone(
            IList<GameCardInstance> cards,
            GameZone candidateZone,
            string targetCardInstanceId,
            out GameCardInstance foundCard,
            out GameZone foundZone,
            out int foundIndex)
        {
            foundCard = null;
            foundZone = candidateZone;
            foundIndex = -1;
            if (cards == null)
            {
                return false;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                GameCardInstance card = cards[i];
                if (card == null || card.instance_id != targetCardInstanceId)
                {
                    continue;
                }

                if (!IsVisibleKnownCard(card))
                {
                    return false;
                }

                foundCard = card;
                foundZone = candidateZone;
                foundIndex = i;
                return true;
            }

            return false;
        }

        private static bool IsVisibleKnownCard(GameCardInstance card)
        {
            return card != null &&
                   card.face_up &&
                   !string.IsNullOrEmpty(card.card_id) &&
                   card.card_id != GameStateViewFactory.HiddenCardId;
        }
    }
}
