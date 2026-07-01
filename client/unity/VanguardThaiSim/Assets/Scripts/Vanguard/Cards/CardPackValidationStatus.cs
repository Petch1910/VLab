using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Cards
{
    public enum CardPackValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    [Serializable]
    public sealed class CardPackValidationIssue
    {
        public CardPackValidationSeverity severity;
        public string code;
        public string message;
    }

    [Serializable]
    public sealed class CardPackValidationStatus
    {
        public bool accepted;
        public string pack_id;
        public string source_version;
        public int schema_version;
        public int source_schema_version;
        public int card_count;
        public int existing_image_count;
        public int source_ability_count;
        public string capabilities_summary;
        public string summary;
        public List<CardPackValidationIssue> issues = new List<CardPackValidationIssue>();

        public void EnsureLists()
        {
            if (issues == null)
            {
                issues = new List<CardPackValidationIssue>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static CardPackValidationStatus FromJson(string json)
        {
            CardPackValidationStatus status = JsonUtility.FromJson<CardPackValidationStatus>(json);
            if (status == null)
            {
                throw new ArgumentException("Card pack validation status JSON could not be parsed.", "json");
            }

            status.EnsureLists();
            return status;
        }
    }

    public static class CardPackValidationStatusBuilder
    {
        public static CardPackValidationStatus FromManifest(
            CardPackManifest manifest,
            bool databaseExists,
            bool assetIndexExists)
        {
            CardPackValidationStatus status = new CardPackValidationStatus();
            if (manifest == null)
            {
                AddIssue(status, CardPackValidationSeverity.Error, "MANIFEST_MISSING", "Manifest is missing.");
                FinalizeStatus(status);
                return status;
            }

            status.pack_id = manifest.pack_id ?? string.Empty;
            status.source_version = manifest.source_version ?? string.Empty;
            status.schema_version = manifest.schema_version;
            status.source_schema_version = manifest.source_schema_version <= 0 ? 1 : manifest.source_schema_version;
            status.card_count = manifest.card_count;
            status.existing_image_count = manifest.existing_image_count;
            status.source_ability_count = manifest.source_ability_count;
            status.capabilities_summary = BuildCapabilitiesSummary(manifest.source_capabilities);

            if (string.IsNullOrEmpty(manifest.pack_id))
            {
                AddIssue(status, CardPackValidationSeverity.Error, "PACK_ID_MISSING", "Pack id is missing.");
            }

            if (manifest.schema_version != 1)
            {
                AddIssue(status, CardPackValidationSeverity.Error, "RUNTIME_SCHEMA_UNSUPPORTED", "Runtime manifest schema must be 1.");
            }

            if (manifest.card_count <= 0)
            {
                AddIssue(status, CardPackValidationSeverity.Error, "CARD_COUNT_EMPTY", "Pack contains no cards.");
            }

            if (string.IsNullOrEmpty(manifest.definition_hash))
            {
                AddIssue(status, CardPackValidationSeverity.Error, "DEFINITION_HASH_MISSING", "Definition hash is missing.");
            }

            if (string.IsNullOrEmpty(manifest.image_manifest_hash))
            {
                AddIssue(status, CardPackValidationSeverity.Warning, "IMAGE_MANIFEST_HASH_MISSING", "Image manifest hash is missing.");
            }

            if (!databaseExists)
            {
                AddIssue(status, CardPackValidationSeverity.Error, "DATABASE_MISSING", "Runtime SQLite database is missing.");
            }

            if (!assetIndexExists)
            {
                AddIssue(status, CardPackValidationSeverity.Warning, "ASSET_INDEX_MISSING", "Asset index is missing.");
            }

            if (status.source_schema_version >= 2)
            {
                ValidateSourceV2(manifest, status);
            }

            FinalizeStatus(status);
            return status;
        }

        private static void ValidateSourceV2(CardPackManifest manifest, CardPackValidationStatus status)
        {
            if (manifest.source_capabilities == null)
            {
                AddIssue(status, CardPackValidationSeverity.Warning, "SOURCE_CAPABILITIES_MISSING", "Source capabilities are missing.");
                return;
            }

            if (!manifest.source_capabilities.cards)
            {
                AddIssue(status, CardPackValidationSeverity.Error, "SOURCE_CAPABILITY_CARDS_DISABLED", "Source capabilities must include cards.");
            }

            if (manifest.source_capabilities.abilities)
            {
                if (string.IsNullOrEmpty(manifest.source_abilities_file))
                {
                    AddIssue(status, CardPackValidationSeverity.Error, "ABILITY_FILE_MISSING", "Ability capability is enabled but ability file is missing.");
                }

                if (manifest.source_ability_count <= 0)
                {
                    AddIssue(status, CardPackValidationSeverity.Warning, "ABILITY_COUNT_EMPTY", "Ability capability is enabled but ability count is zero.");
                }

                if (string.IsNullOrEmpty(manifest.source_ability_data_hash))
                {
                    AddIssue(status, CardPackValidationSeverity.Error, "ABILITY_HASH_MISSING", "Ability capability is enabled but ability data hash is missing.");
                }
            }
        }

        private static string BuildCapabilitiesSummary(CardPackSourceCapabilities capabilities)
        {
            if (capabilities == null)
            {
                return "v1";
            }

            List<string> enabled = new List<string>();
            if (capabilities.cards)
            {
                enabled.Add("cards");
            }
            if (capabilities.images)
            {
                enabled.Add("images");
            }
            if (capabilities.abilities)
            {
                enabled.Add("abilities");
            }
            if (capabilities.custom_formats)
            {
                enabled.Add("custom_formats");
            }

            return enabled.Count == 0 ? "none" : string.Join(",", enabled.ToArray());
        }

        private static void AddIssue(
            CardPackValidationStatus status,
            CardPackValidationSeverity severity,
            string code,
            string message)
        {
            status.EnsureLists();
            status.issues.Add(new CardPackValidationIssue
            {
                severity = severity,
                code = code ?? string.Empty,
                message = message ?? string.Empty
            });
        }

        private static void FinalizeStatus(CardPackValidationStatus status)
        {
            status.EnsureLists();
            bool hasErrors = false;
            for (int i = 0; i < status.issues.Count; i++)
            {
                if (status.issues[i].severity == CardPackValidationSeverity.Error)
                {
                    hasErrors = true;
                    break;
                }
            }

            status.accepted = !hasErrors;
            status.summary = CardPackValidationStatusFormatter.FormatCompact(status);
        }
    }

    public static class CardPackValidationStatusFormatter
    {
        public static string FormatCompact(CardPackValidationStatus status)
        {
            if (status == null)
            {
                return "Pack validation: missing";
            }

            status.EnsureLists();
            int errors = 0;
            int warnings = 0;
            for (int i = 0; i < status.issues.Count; i++)
            {
                if (status.issues[i].severity == CardPackValidationSeverity.Error)
                {
                    errors++;
                }
                else if (status.issues[i].severity == CardPackValidationSeverity.Warning)
                {
                    warnings++;
                }
            }

            string state = status.accepted ? "OK" : "BLOCKED";
            return "Pack validation: " + state +
                   " | schema " + status.source_schema_version +
                   " | cards " + status.card_count +
                   " | abilities " + status.source_ability_count +
                   " | caps " + (status.capabilities_summary ?? string.Empty) +
                   " | errors " + errors +
                   " | warnings " + warnings;
        }
    }
}
