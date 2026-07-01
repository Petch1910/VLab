using System;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class StructuredCostTemplateRejectionReasons
    {
        public const string AbilityMissing = "STRUCTURED_COST_ABILITY_MISSING";
        public const string CostsMissing = "STRUCTURED_COST_COSTS_MISSING";
        public const string NegativeAmount = "STRUCTURED_COST_NEGATIVE_AMOUNT";
        public const string UnsupportedCostType = "STRUCTURED_COST_UNSUPPORTED_TYPE";
        public const string MultipleOncePerTurn = "STRUCTURED_COST_MULTIPLE_ONCE_PER_TURN";
        public const string MultipleOncePerFight = "STRUCTURED_COST_MULTIPLE_ONCE_PER_FIGHT";
    }

    [Serializable]
    public sealed class StructuredCostTemplateResult
    {
        public bool accepted;
        public string rejection_reason;
        public bool requires_manual_resolution;
        public ResourceCostRequest request;
        public string summary;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static StructuredCostTemplateResult FromJson(string json)
        {
            StructuredCostTemplateResult result =
                JsonUtility.FromJson<StructuredCostTemplateResult>(json);
            if (result == null)
            {
                throw new ArgumentException("Structured cost template result JSON could not be parsed.", "json");
            }

            return result;
        }
    }

    public static class StructuredCostTemplate
    {
        public static StructuredCostTemplateResult BuildRequest(
            int playerIndex,
            int turnNumber,
            StructuredAbility ability)
        {
            if (ability == null)
            {
                return Reject(StructuredCostTemplateRejectionReasons.AbilityMissing, false, null);
            }

            ability.EnsureLists();
            if (ability.costs == null)
            {
                return Reject(StructuredCostTemplateRejectionReasons.CostsMissing, ability.manual_fallback, null);
            }

            var request = new ResourceCostRequest
            {
                player_index = playerIndex,
                ability_key = ability.ability_id ?? string.Empty
            };

            for (int i = 0; i < ability.costs.Count; i++)
            {
                StructuredAbilityCost cost = ability.costs[i];
                if (cost == null)
                {
                    continue;
                }

                if (cost.amount < 0)
                {
                    return Reject(StructuredCostTemplateRejectionReasons.NegativeAmount, ability.manual_fallback, request);
                }

                string costType = cost.type ?? string.Empty;
                switch (costType)
                {
                    case "":
                    case "none":
                        break;
                    case "counter_blast":
                        request.counter_blast += cost.amount;
                        break;
                    case "soul_blast":
                        request.soul_blast += cost.amount;
                        break;
                    case "energy_blast":
                        request.energy_blast += cost.amount;
                        break;
                    case "once_per_turn":
                        if (!string.IsNullOrEmpty(request.once_per_turn_key))
                        {
                            return Reject(StructuredCostTemplateRejectionReasons.MultipleOncePerTurn, ability.manual_fallback, request);
                        }

                        request.once_per_turn_key = string.IsNullOrEmpty(cost.key)
                            ? ResourceLedger.BuildOncePerTurnKey(ability.ability_id, turnNumber)
                            : cost.key;
                        break;
                    case "once_per_fight":
                        if (!string.IsNullOrEmpty(request.once_per_fight_key))
                        {
                            return Reject(StructuredCostTemplateRejectionReasons.MultipleOncePerFight, ability.manual_fallback, request);
                        }

                        request.once_per_fight_key = string.IsNullOrEmpty(cost.key)
                            ? ResourceLedger.BuildOncePerFightKey(ability.ability_id)
                            : cost.key;
                        break;
                    case "discard":
                        return Reject(StructuredCostTemplateRejectionReasons.UnsupportedCostType + ": discard", true, request);
                    default:
                        return Reject(StructuredCostTemplateRejectionReasons.UnsupportedCostType + ": " + costType, ability.manual_fallback, request);
                }
            }

            return new StructuredCostTemplateResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                requires_manual_resolution = false,
                request = CloneRequest(request),
                summary = "Structured cost template created resource request for " + (ability.ability_id ?? string.Empty) + "."
            };
        }

        public static ResourceLedgerValidationResult ValidateAgainstLedger(
            ResourceLedgerState ledger,
            int playerIndex,
            int turnNumber,
            StructuredAbility ability)
        {
            StructuredCostTemplateResult template = BuildRequest(playerIndex, turnNumber, ability);
            if (!template.accepted)
            {
                return new ResourceLedgerValidationResult
                {
                    accepted = false,
                    rejection_reason = template.rejection_reason,
                    before_state = ledger == null ? null : ResourceLedgerState.FromJson(ledger.ToJson(false)),
                    after_state = ledger == null ? null : ResourceLedgerState.FromJson(ledger.ToJson(false)),
                    request = template.request,
                    summary = "Structured cost template rejected before ledger validation: " + template.rejection_reason
                };
            }

            return ResourceLedger.ValidateCost(ledger, template.request);
        }

        private static StructuredCostTemplateResult Reject(
            string rejectionReason,
            bool requiresManualResolution,
            ResourceCostRequest request)
        {
            return new StructuredCostTemplateResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                requires_manual_resolution = requiresManualResolution,
                request = CloneRequest(request),
                summary = "Structured cost template rejected: " + (rejectionReason ?? string.Empty)
            };
        }

        private static ResourceCostRequest CloneRequest(ResourceCostRequest request)
        {
            if (request == null)
            {
                return null;
            }

            return JsonUtility.FromJson<ResourceCostRequest>(JsonUtility.ToJson(request, false));
        }
    }
}
