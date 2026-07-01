using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class ResourceLedgerRejectionReasons
    {
        public const string LedgerMissing = "RESOURCE_LEDGER_MISSING";
        public const string RequestMissing = "RESOURCE_LEDGER_REQUEST_MISSING";
        public const string PlayerMismatch = "RESOURCE_LEDGER_PLAYER_MISMATCH";
        public const string NegativeCost = "RESOURCE_LEDGER_NEGATIVE_COST";
        public const string CounterBlastUnavailable = "RESOURCE_LEDGER_COUNTER_BLAST_UNAVAILABLE";
        public const string SoulBlastUnavailable = "RESOURCE_LEDGER_SOUL_BLAST_UNAVAILABLE";
        public const string EnergyBlastUnavailable = "RESOURCE_LEDGER_ENERGY_BLAST_UNAVAILABLE";
        public const string OncePerTurnUsed = "RESOURCE_LEDGER_ONCE_PER_TURN_USED";
        public const string OncePerFightUsed = "RESOURCE_LEDGER_ONCE_PER_FIGHT_USED";
    }

    [Serializable]
    public sealed class ResourceLedgerState
    {
        public int player_index;
        public int available_counter_blast;
        public int available_soul;
        public int available_energy;
        public List<string> used_once_per_turn_keys = new List<string>();
        public List<string> used_once_per_fight_keys = new List<string>();

        public static ResourceLedgerState FromGameState(
            GameState state,
            int playerIndex,
            int availableSoul = 0,
            int availableEnergy = 0,
            IEnumerable<string> usedOncePerTurnKeys = null,
            IEnumerable<string> usedOncePerFightKeys = null)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            PlayerGameState player = state.GetPlayer(playerIndex);
            ResourceLedgerState ledger = new ResourceLedgerState
            {
                player_index = playerIndex,
                available_counter_blast = CountFaceUp(player.damage),
                available_soul = Math.Max(CountCards(player.soul), Math.Max(0, availableSoul)),
                available_energy = Math.Max(0, availableEnergy)
            };

            AddRange(ledger.used_once_per_turn_keys, usedOncePerTurnKeys);
            AddRange(ledger.used_once_per_fight_keys, usedOncePerFightKeys);
            return ledger;
        }

        public void EnsureLists()
        {
            if (used_once_per_turn_keys == null)
            {
                used_once_per_turn_keys = new List<string>();
            }

            if (used_once_per_fight_keys == null)
            {
                used_once_per_fight_keys = new List<string>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static ResourceLedgerState FromJson(string json)
        {
            ResourceLedgerState state = JsonUtility.FromJson<ResourceLedgerState>(json);
            if (state == null)
            {
                throw new ArgumentException("Resource ledger state JSON could not be parsed.", "json");
            }

            state.EnsureLists();
            return state;
        }

        private static int CountFaceUp(IList<GameCardInstance> cards)
        {
            if (cards == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] != null && cards[i].face_up)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountCards(IList<GameCardInstance> cards)
        {
            return cards == null ? 0 : cards.Count;
        }

        private static void AddRange(List<string> target, IEnumerable<string> values)
        {
            if (values == null)
            {
                return;
            }

            foreach (string value in values)
            {
                if (!string.IsNullOrEmpty(value) && !target.Contains(value))
                {
                    target.Add(value);
                }
            }
        }
    }

    [Serializable]
    public sealed class ResourceCostRequest
    {
        public int player_index;
        public string ability_key;
        public int counter_blast;
        public int soul_blast;
        public int energy_blast;
        public string once_per_turn_key;
        public string once_per_fight_key;
    }

    [Serializable]
    public sealed class ResourceLedgerValidationResult
    {
        public bool accepted;
        public string rejection_reason;
        public ResourceLedgerState before_state;
        public ResourceLedgerState after_state;
        public ResourceCostRequest request;
        public string summary;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static ResourceLedgerValidationResult FromJson(string json)
        {
            ResourceLedgerValidationResult result =
                JsonUtility.FromJson<ResourceLedgerValidationResult>(json);
            if (result == null)
            {
                throw new ArgumentException(
                    "Resource ledger validation result JSON could not be parsed.",
                    "json");
            }

            result.before_state?.EnsureLists();
            result.after_state?.EnsureLists();
            return result;
        }
    }

    public static class ResourceLedger
    {
        public static ResourceLedgerValidationResult ValidateCost(
            ResourceLedgerState ledger,
            ResourceCostRequest request)
        {
            if (ledger == null)
            {
                return Reject(ResourceLedgerRejectionReasons.LedgerMissing, null, request);
            }

            if (request == null)
            {
                return Reject(ResourceLedgerRejectionReasons.RequestMissing, ledger, null);
            }

            ResourceLedgerState before = CloneLedger(ledger);
            ResourceCostRequest safeRequest = CloneRequest(request);

            if (request.player_index != ledger.player_index)
            {
                return Reject(ResourceLedgerRejectionReasons.PlayerMismatch, before, safeRequest);
            }

            if (request.counter_blast < 0 || request.soul_blast < 0 || request.energy_blast < 0)
            {
                return Reject(ResourceLedgerRejectionReasons.NegativeCost, before, safeRequest);
            }

            if (request.counter_blast > before.available_counter_blast)
            {
                return Reject(ResourceLedgerRejectionReasons.CounterBlastUnavailable, before, safeRequest);
            }

            if (request.soul_blast > before.available_soul)
            {
                return Reject(ResourceLedgerRejectionReasons.SoulBlastUnavailable, before, safeRequest);
            }

            if (request.energy_blast > before.available_energy)
            {
                return Reject(ResourceLedgerRejectionReasons.EnergyBlastUnavailable, before, safeRequest);
            }

            if (!string.IsNullOrEmpty(request.once_per_turn_key) &&
                ContainsKey(before.used_once_per_turn_keys, request.once_per_turn_key))
            {
                return Reject(ResourceLedgerRejectionReasons.OncePerTurnUsed, before, safeRequest);
            }

            if (!string.IsNullOrEmpty(request.once_per_fight_key) &&
                ContainsKey(before.used_once_per_fight_keys, request.once_per_fight_key))
            {
                return Reject(ResourceLedgerRejectionReasons.OncePerFightUsed, before, safeRequest);
            }

            ResourceLedgerState after = CloneLedger(before);
            after.available_counter_blast -= request.counter_blast;
            after.available_soul -= request.soul_blast;
            after.available_energy -= request.energy_blast;
            AddOnceKey(after.used_once_per_turn_keys, request.once_per_turn_key);
            AddOnceKey(after.used_once_per_fight_keys, request.once_per_fight_key);

            return new ResourceLedgerValidationResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                before_state = before,
                after_state = after,
                request = safeRequest,
                summary = "Resource ledger accepted cost request for player " + request.player_index + "."
            };
        }

        public static string BuildOncePerTurnKey(string abilityKey, int turnNumber)
        {
            return NormalizeKey(abilityKey) + "|turn|" + turnNumber;
        }

        public static string BuildOncePerFightKey(string abilityKey)
        {
            return NormalizeKey(abilityKey) + "|fight";
        }

        private static ResourceLedgerValidationResult Reject(
            string rejectionReason,
            ResourceLedgerState ledger,
            ResourceCostRequest request)
        {
            ResourceLedgerState before = CloneLedger(ledger);
            return new ResourceLedgerValidationResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                before_state = before,
                after_state = CloneLedger(before),
                request = CloneRequest(request),
                summary = "Resource ledger rejected cost request: " + (rejectionReason ?? string.Empty)
            };
        }

        private static ResourceLedgerState CloneLedger(ResourceLedgerState ledger)
        {
            if (ledger == null)
            {
                return null;
            }

            ledger.EnsureLists();
            return ResourceLedgerState.FromJson(ledger.ToJson(false));
        }

        private static ResourceCostRequest CloneRequest(ResourceCostRequest request)
        {
            if (request == null)
            {
                return null;
            }

            return JsonUtility.FromJson<ResourceCostRequest>(JsonUtility.ToJson(request, false));
        }

        private static bool ContainsKey(List<string> keys, string key)
        {
            if (keys == null || string.IsNullOrEmpty(key))
            {
                return false;
            }

            for (int i = 0; i < keys.Count; i++)
            {
                if (string.Equals(keys[i], key, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AddOnceKey(List<string> keys, string key)
        {
            if (keys == null || string.IsNullOrEmpty(key) || ContainsKey(keys, key))
            {
                return;
            }

            keys.Add(key);
        }

        private static string NormalizeKey(string abilityKey)
        {
            return string.IsNullOrEmpty(abilityKey) ? "unknown" : abilityKey;
        }
    }
}
