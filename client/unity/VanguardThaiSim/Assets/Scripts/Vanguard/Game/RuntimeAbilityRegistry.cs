using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class RuntimeAbilityRegistryRejectionReasons
    {
        public const string JsonMissing = "RUNTIME_ABILITY_JSON_MISSING";
        public const string InvalidJson = "RUNTIME_ABILITY_JSON_INVALID";
        public const string SchemaVersionMismatch = "RUNTIME_ABILITY_SCHEMA_VERSION_MISMATCH";
        public const string AbilityMissingId = "RUNTIME_ABILITY_MISSING_ID";
        public const string AbilityMissingCardId = "RUNTIME_ABILITY_MISSING_CARD_ID";
        public const string DuplicateAbilityId = "RUNTIME_ABILITY_DUPLICATE_ID";
    }

    [Serializable]
    public sealed class StructuredAbilityPack
    {
        public string schema_version;
        public List<StructuredAbility> abilities = new List<StructuredAbility>();

        public void EnsureLists()
        {
            if (abilities == null)
            {
                abilities = new List<StructuredAbility>();
            }

            for (int i = 0; i < abilities.Count; i++)
            {
                abilities[i]?.EnsureLists();
            }
        }
    }

    [Serializable]
    public sealed class StructuredAbility
    {
        public string ability_id;
        public string card_id;
        public string label;
        public string kind;
        public StructuredAbilityTrigger trigger = new StructuredAbilityTrigger();
        public StructuredAbilityTiming timing = new StructuredAbilityTiming();
        public List<StructuredAbilityCost> costs = new List<StructuredAbilityCost>();
        public List<StructuredAbilityTarget> targets = new List<StructuredAbilityTarget>();
        public List<StructuredAbilityEffect> effects = new List<StructuredAbilityEffect>();
        public StructuredAbilityDuration duration = new StructuredAbilityDuration();
        public bool manual_fallback;
        public string notes;

        public void EnsureLists()
        {
            if (costs == null) costs = new List<StructuredAbilityCost>();
            if (targets == null) targets = new List<StructuredAbilityTarget>();
            if (effects == null) effects = new List<StructuredAbilityEffect>();
            if (trigger == null) trigger = new StructuredAbilityTrigger();
            if (timing == null) timing = new StructuredAbilityTiming();
            if (duration == null) duration = new StructuredAbilityDuration();
        }
    }

    [Serializable]
    public sealed class StructuredAbilityTrigger
    {
        public string type;
        public string timing_window;
        public string event_type;
        public string source_zone;
        public string condition;
    }

    [Serializable]
    public sealed class StructuredAbilityTiming
    {
        public string phase;
        public string window;
        public bool optional;
    }

    [Serializable]
    public sealed class StructuredAbilityCost
    {
        public string type;
        public int amount;
        public string target_ref;
        public string key;
        public string notes;
    }

    [Serializable]
    public sealed class StructuredAbilityTarget
    {
        public string id;
        public string type;
        public string owner;
        public string zone;
        public int count;
        public List<string> filters = new List<string>();
        public bool optional;

        public void EnsureLists()
        {
            if (filters == null)
            {
                filters = new List<string>();
            }
        }
    }

    [Serializable]
    public sealed class StructuredAbilityEffect
    {
        public string type;
        public int amount;
        public string target_ref;
        public string from_zone;
        public string to_zone;
        public string duration_ref;
        public string notes;
    }

    [Serializable]
    public sealed class StructuredAbilityDuration
    {
        public string type;
        public string cleanup_timing;
    }

    [Serializable]
    public sealed class RuntimeAbilityRegistryLoadResult
    {
        public bool accepted;
        public string rejection_reason;
        public int ability_count;
        public int card_count;
        public string summary;

        [NonSerialized]
        public RuntimeAbilityRegistry registry;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static RuntimeAbilityRegistryLoadResult FromJson(string json)
        {
            RuntimeAbilityRegistryLoadResult result =
                JsonUtility.FromJson<RuntimeAbilityRegistryLoadResult>(json);
            if (result == null)
            {
                throw new ArgumentException("Runtime ability registry load result JSON could not be parsed.", "json");
            }

            return result;
        }
    }

    public sealed class RuntimeAbilityRegistry
    {
        private readonly Dictionary<string, StructuredAbility> byAbilityId;
        private readonly Dictionary<string, List<StructuredAbility>> byCardId;

        private RuntimeAbilityRegistry(
            Dictionary<string, StructuredAbility> byAbilityId,
            Dictionary<string, List<StructuredAbility>> byCardId)
        {
            this.byAbilityId = byAbilityId ?? new Dictionary<string, StructuredAbility>();
            this.byCardId = byCardId ?? new Dictionary<string, List<StructuredAbility>>();
        }

        public int AbilityCount
        {
            get { return byAbilityId.Count; }
        }

        public int CardCount
        {
            get { return byCardId.Count; }
        }

        public static RuntimeAbilityRegistryLoadResult LoadFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return Reject(RuntimeAbilityRegistryRejectionReasons.JsonMissing, 0, 0);
            }

            StructuredAbilityPack pack;
            try
            {
                pack = JsonUtility.FromJson<StructuredAbilityPack>(json);
            }
            catch (Exception exception)
            {
                return Reject(RuntimeAbilityRegistryRejectionReasons.InvalidJson + ": " + exception.Message, 0, 0);
            }

            if (pack == null)
            {
                return Reject(RuntimeAbilityRegistryRejectionReasons.InvalidJson, 0, 0);
            }

            pack.EnsureLists();
            if (!string.Equals(pack.schema_version, "ability_schema_v1", StringComparison.Ordinal))
            {
                return Reject(RuntimeAbilityRegistryRejectionReasons.SchemaVersionMismatch, pack.abilities.Count, 0);
            }

            var byAbility = new Dictionary<string, StructuredAbility>();
            var byCard = new Dictionary<string, List<StructuredAbility>>();
            for (int i = 0; i < pack.abilities.Count; i++)
            {
                StructuredAbility ability = pack.abilities[i];
                if (ability == null)
                {
                    return Reject(RuntimeAbilityRegistryRejectionReasons.AbilityMissingId, pack.abilities.Count, byCard.Count);
                }

                ability.EnsureLists();
                if (string.IsNullOrWhiteSpace(ability.ability_id))
                {
                    return Reject(RuntimeAbilityRegistryRejectionReasons.AbilityMissingId, pack.abilities.Count, byCard.Count);
                }

                if (string.IsNullOrWhiteSpace(ability.card_id))
                {
                    return Reject(RuntimeAbilityRegistryRejectionReasons.AbilityMissingCardId, pack.abilities.Count, byCard.Count);
                }

                if (byAbility.ContainsKey(ability.ability_id))
                {
                    return Reject(RuntimeAbilityRegistryRejectionReasons.DuplicateAbilityId, pack.abilities.Count, byCard.Count);
                }

                StructuredAbility clone = CloneAbility(ability);
                byAbility.Add(clone.ability_id, clone);
                if (!byCard.TryGetValue(clone.card_id, out List<StructuredAbility> cardAbilities))
                {
                    cardAbilities = new List<StructuredAbility>();
                    byCard.Add(clone.card_id, cardAbilities);
                }

                cardAbilities.Add(CloneAbility(clone));
            }

            RuntimeAbilityRegistry registry = new RuntimeAbilityRegistry(byAbility, byCard);
            return new RuntimeAbilityRegistryLoadResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                ability_count = registry.AbilityCount,
                card_count = registry.CardCount,
                registry = registry,
                summary = "Loaded " + registry.AbilityCount + " structured ability definition(s)."
            };
        }

        public StructuredAbility FindAbility(string abilityId)
        {
            if (string.IsNullOrEmpty(abilityId))
            {
                return null;
            }

            return byAbilityId.TryGetValue(abilityId, out StructuredAbility ability)
                ? CloneAbility(ability)
                : null;
        }

        public List<StructuredAbility> GetAbilitiesForCard(string cardId)
        {
            var result = new List<StructuredAbility>();
            if (string.IsNullOrEmpty(cardId) || !byCardId.TryGetValue(cardId, out List<StructuredAbility> abilities))
            {
                return result;
            }

            for (int i = 0; i < abilities.Count; i++)
            {
                result.Add(CloneAbility(abilities[i]));
            }

            return result;
        }

        private static RuntimeAbilityRegistryLoadResult Reject(
            string rejectionReason,
            int abilityCount,
            int cardCount)
        {
            return new RuntimeAbilityRegistryLoadResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                ability_count = abilityCount,
                card_count = cardCount,
                summary = "Runtime ability registry rejected: " + (rejectionReason ?? string.Empty)
            };
        }

        private static StructuredAbility CloneAbility(StructuredAbility ability)
        {
            if (ability == null)
            {
                return null;
            }

            ability.EnsureLists();
            StructuredAbility clone =
                JsonUtility.FromJson<StructuredAbility>(JsonUtility.ToJson(ability, false));
            clone?.EnsureLists();
            return clone;
        }
    }
}
