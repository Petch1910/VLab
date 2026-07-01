using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Cards
{
    public static class CardPackRegistryRejectionReasons
    {
        public const string StateMissing = "PACK_REGISTRY_STATE_MISSING";
        public const string PackMissing = "PACK_REGISTRY_PACK_MISSING";
        public const string PackIdMissing = "PACK_REGISTRY_PACK_ID_MISSING";
    }

    [Serializable]
    public sealed class CardPackRegistryEntry
    {
        public string pack_id;
        public string display_name;
        public string source_version;
        public string pack_directory;
        public string definition_hash;
        public bool enabled;
        public string validation_summary;
    }

    [Serializable]
    public sealed class CardPackRegistryState
    {
        public List<CardPackRegistryEntry> packs = new List<CardPackRegistryEntry>();

        public void EnsureLists()
        {
            if (packs == null)
            {
                packs = new List<CardPackRegistryEntry>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static CardPackRegistryState FromJson(string json)
        {
            CardPackRegistryState state = JsonUtility.FromJson<CardPackRegistryState>(json);
            if (state == null)
            {
                throw new ArgumentException("Card pack registry JSON could not be parsed.", "json");
            }

            state.EnsureLists();
            return state;
        }
    }

    [Serializable]
    public sealed class CardPackRegistryMutationResult
    {
        public bool accepted;
        public string rejection_reason;
        public CardPackRegistryState state;
    }

    public static class CardPackRegistryManager
    {
        public static CardPackRegistryEntry CreateEntry(
            CardPackManifest manifest,
            string packDirectory,
            CardPackValidationStatus validationStatus,
            bool enabled)
        {
            if (manifest == null)
            {
                return null;
            }

            return new CardPackRegistryEntry
            {
                pack_id = manifest.pack_id ?? string.Empty,
                display_name = string.IsNullOrEmpty(manifest.display_name) ? manifest.pack_id ?? string.Empty : manifest.display_name,
                source_version = manifest.source_version ?? string.Empty,
                pack_directory = packDirectory ?? string.Empty,
                definition_hash = manifest.definition_hash ?? string.Empty,
                enabled = enabled,
                validation_summary = CardPackValidationStatusFormatter.FormatCompact(validationStatus)
            };
        }

        public static CardPackRegistryMutationResult SetEnabled(
            CardPackRegistryState source,
            string packId,
            string sourceVersion,
            bool enabled)
        {
            if (source == null)
            {
                return Reject(CardPackRegistryRejectionReasons.StateMissing, null);
            }

            if (string.IsNullOrEmpty(packId))
            {
                return Reject(CardPackRegistryRejectionReasons.PackIdMissing, CloneState(source));
            }

            CardPackRegistryState clone = CloneState(source);
            for (int i = 0; i < clone.packs.Count; i++)
            {
                CardPackRegistryEntry entry = clone.packs[i];
                if (entry == null)
                {
                    continue;
                }

                bool samePack = string.Equals(entry.pack_id ?? string.Empty, packId, StringComparison.Ordinal);
                bool sameVersion = string.IsNullOrEmpty(sourceVersion) ||
                                   string.Equals(entry.source_version ?? string.Empty, sourceVersion, StringComparison.Ordinal);
                if (!samePack || !sameVersion)
                {
                    continue;
                }

                entry.enabled = enabled;
                return new CardPackRegistryMutationResult
                {
                    accepted = true,
                    rejection_reason = string.Empty,
                    state = clone
                };
            }

            return Reject(CardPackRegistryRejectionReasons.PackMissing, clone);
        }

        public static CardPackRegistryState CloneState(CardPackRegistryState source)
        {
            CardPackRegistryState clone = new CardPackRegistryState();
            if (source == null)
            {
                return clone;
            }

            source.EnsureLists();
            for (int i = 0; i < source.packs.Count; i++)
            {
                clone.packs.Add(CloneEntry(source.packs[i]));
            }

            return clone;
        }

        private static CardPackRegistryEntry CloneEntry(CardPackRegistryEntry source)
        {
            if (source == null)
            {
                return null;
            }

            return new CardPackRegistryEntry
            {
                pack_id = source.pack_id ?? string.Empty,
                display_name = source.display_name ?? string.Empty,
                source_version = source.source_version ?? string.Empty,
                pack_directory = source.pack_directory ?? string.Empty,
                definition_hash = source.definition_hash ?? string.Empty,
                enabled = source.enabled,
                validation_summary = source.validation_summary ?? string.Empty
            };
        }

        private static CardPackRegistryMutationResult Reject(string reason, CardPackRegistryState state)
        {
            return new CardPackRegistryMutationResult
            {
                accepted = false,
                rejection_reason = reason ?? string.Empty,
                state = state
            };
        }
    }
}
