using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class CustomFormatProfileRejectionReasons
    {
        public const string CatalogMissing = "CUSTOM_FORMAT_CATALOG_MISSING";
        public const string FormatIdMissing = "CUSTOM_FORMAT_ID_MISSING";
        public const string BaseRuleSetMissing = "CUSTOM_FORMAT_BASE_RULE_SET_MISSING";
        public const string BaseRuleSetUnknown = "CUSTOM_FORMAT_BASE_RULE_SET_UNKNOWN";
        public const string DuplicateFormat = "CUSTOM_FORMAT_DUPLICATE_FORMAT";
        public const string DuplicateAlias = "CUSTOM_FORMAT_DUPLICATE_ALIAS";
        public const string DuplicatePack = "CUSTOM_FORMAT_DUPLICATE_PACK";
    }

    [Serializable]
    public sealed class CustomFormatProfile
    {
        public string format_id;
        public string display_name;
        public string base_rule_set_profile_id;
        public List<string> aliases = new List<string>();
        public List<string> allowed_pack_ids = new List<string>();
        public string notes;

        public void EnsureLists()
        {
            if (aliases == null)
            {
                aliases = new List<string>();
            }

            if (allowed_pack_ids == null)
            {
                allowed_pack_ids = new List<string>();
            }
        }
    }

    [Serializable]
    public sealed class CustomFormatProfileCatalogDefinition
    {
        public List<CustomFormatProfile> formats = new List<CustomFormatProfile>();

        public void EnsureLists()
        {
            if (formats == null)
            {
                formats = new List<CustomFormatProfile>();
            }

            for (int i = 0; i < formats.Count; i++)
            {
                formats[i]?.EnsureLists();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static CustomFormatProfileCatalogDefinition FromJson(string json)
        {
            CustomFormatProfileCatalogDefinition catalog =
                JsonUtility.FromJson<CustomFormatProfileCatalogDefinition>(json);
            if (catalog == null)
            {
                throw new ArgumentException("Custom format profile catalog JSON could not be parsed.", "json");
            }

            catalog.EnsureLists();
            return catalog;
        }
    }

    [Serializable]
    public sealed class CustomFormatProfileValidationResult
    {
        public bool accepted;
        public string rejection_reason;
        public string format_id;
        public string base_rule_set_profile_id;
        public string summary;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static CustomFormatProfileValidationResult FromJson(string json)
        {
            CustomFormatProfileValidationResult result =
                JsonUtility.FromJson<CustomFormatProfileValidationResult>(json);
            if (result == null)
            {
                throw new ArgumentException("Custom format profile validation result JSON could not be parsed.", "json");
            }

            return result;
        }
    }

    public static class CustomFormatProfileCatalog
    {
        public static CustomFormatProfile CreateStandardPreset()
        {
            return new CustomFormatProfile
            {
                format_id = "standard",
                display_name = "Standard",
                base_rule_set_profile_id = RuleSetProfileIds.Standard,
                aliases = new List<string> { "d", "standard_d" },
                allowed_pack_ids = new List<string> { "vanguard_th" },
                notes = "Standard custom-format preset delegates flags to the core standard RuleSetProfile."
            };
        }

        public static CustomFormatProfileCatalogDefinition CreateStandardPresetCatalog()
        {
            return new CustomFormatProfileCatalogDefinition
            {
                formats = new List<CustomFormatProfile> { CreateStandardPreset() }
            };
        }

        public static CustomFormatProfile CreateVPremiumPreset()
        {
            return new CustomFormatProfile
            {
                format_id = "v_premium",
                display_name = "V-Premium",
                base_rule_set_profile_id = RuleSetProfileIds.VPremium,
                aliases = new List<string> { "v", "v-premium", "vpremium" },
                allowed_pack_ids = new List<string> { "vanguard_th" },
                notes = "V-Premium custom-format preset delegates flags to the core v_premium RuleSetProfile."
            };
        }

        public static CustomFormatProfileCatalogDefinition CreateVPremiumPresetCatalog()
        {
            return new CustomFormatProfileCatalogDefinition
            {
                formats = new List<CustomFormatProfile> { CreateVPremiumPreset() }
            };
        }

        public static CustomFormatProfile CreatePremiumPreset()
        {
            return new CustomFormatProfile
            {
                format_id = "premium",
                display_name = "Premium",
                base_rule_set_profile_id = RuleSetProfileIds.Premium,
                aliases = new List<string> { "p", "premium-clan" },
                allowed_pack_ids = new List<string> { "vanguard_th" },
                notes = "Premium custom-format preset delegates flags to the core premium RuleSetProfile."
            };
        }

        public static CustomFormatProfileCatalogDefinition CreatePremiumPresetCatalog()
        {
            return new CustomFormatProfileCatalogDefinition
            {
                formats = new List<CustomFormatProfile> { CreatePremiumPreset() }
            };
        }

        public static CustomFormatProfileCatalogDefinition CreateCorePresetCatalog()
        {
            return new CustomFormatProfileCatalogDefinition
            {
                formats = new List<CustomFormatProfile>
                {
                    CreateStandardPreset(),
                    CreateVPremiumPreset(),
                    CreatePremiumPreset()
                }
            };
        }

        public static CustomFormatProfileValidationResult Validate(
            CustomFormatProfileCatalogDefinition catalog)
        {
            return Validate(catalog, RuleSetProfileCatalog.CreateCoreProfiles());
        }

        public static CustomFormatProfileValidationResult Validate(
            CustomFormatProfileCatalogDefinition catalog,
            RuleSetProfileCatalogDefinition ruleSetCatalog)
        {
            if (catalog == null)
            {
                return Reject(CustomFormatProfileRejectionReasons.CatalogMissing, string.Empty, string.Empty);
            }

            catalog.EnsureLists();
            HashSet<string> formatIds = new HashSet<string>();
            HashSet<string> aliases = new HashSet<string>();
            for (int i = 0; i < catalog.formats.Count; i++)
            {
                CustomFormatProfile profile = catalog.formats[i];
                if (profile == null)
                {
                    continue;
                }

                profile.EnsureLists();
                string formatId = Normalize(profile.format_id);
                if (string.IsNullOrEmpty(formatId))
                {
                    return Reject(CustomFormatProfileRejectionReasons.FormatIdMissing, profile.format_id, profile.base_rule_set_profile_id);
                }

                if (!formatIds.Add(formatId))
                {
                    return Reject(CustomFormatProfileRejectionReasons.DuplicateFormat, profile.format_id, profile.base_rule_set_profile_id);
                }

                string baseRuleSet = Normalize(profile.base_rule_set_profile_id);
                if (string.IsNullOrEmpty(baseRuleSet))
                {
                    return Reject(CustomFormatProfileRejectionReasons.BaseRuleSetMissing, profile.format_id, profile.base_rule_set_profile_id);
                }

                RuleSetProfileResolutionResult baseResolution =
                    RuleSetProfileCatalog.Resolve(ruleSetCatalog, baseRuleSet);
                if (!baseResolution.accepted)
                {
                    return Reject(CustomFormatProfileRejectionReasons.BaseRuleSetUnknown, profile.format_id, profile.base_rule_set_profile_id);
                }

                for (int aliasIndex = 0; aliasIndex < profile.aliases.Count; aliasIndex++)
                {
                    string alias = Normalize(profile.aliases[aliasIndex]);
                    if (string.IsNullOrEmpty(alias))
                    {
                        continue;
                    }

                    if (!aliases.Add(alias) || formatIds.Contains(alias))
                    {
                        return Reject(CustomFormatProfileRejectionReasons.DuplicateAlias, alias, profile.base_rule_set_profile_id);
                    }
                }

                HashSet<string> packIds = new HashSet<string>();
                for (int packIndex = 0; packIndex < profile.allowed_pack_ids.Count; packIndex++)
                {
                    string packId = Normalize(profile.allowed_pack_ids[packIndex]);
                    if (string.IsNullOrEmpty(packId))
                    {
                        continue;
                    }

                    if (!packIds.Add(packId))
                    {
                        return Reject(CustomFormatProfileRejectionReasons.DuplicatePack, profile.format_id, profile.base_rule_set_profile_id);
                    }
                }
            }

            return new CustomFormatProfileValidationResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                format_id = "catalog",
                base_rule_set_profile_id = string.Empty,
                summary = "Custom format profile catalog is valid."
            };
        }

        private static CustomFormatProfileValidationResult Reject(
            string reason,
            string formatId,
            string baseRuleSetProfileId)
        {
            return new CustomFormatProfileValidationResult
            {
                accepted = false,
                rejection_reason = reason ?? string.Empty,
                format_id = formatId ?? string.Empty,
                base_rule_set_profile_id = baseRuleSetProfileId ?? string.Empty,
                summary = "Custom format profile rejected: " + (reason ?? string.Empty)
            };
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLowerInvariant().Replace(" ", "_");
        }
    }
}
