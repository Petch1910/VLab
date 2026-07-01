using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class CustomFormatSandboxRejectionReasons
    {
        public const string CatalogInvalid = "CUSTOM_FORMAT_SANDBOX_CATALOG_INVALID";
        public const string FormatMissing = "CUSTOM_FORMAT_SANDBOX_FORMAT_MISSING";
        public const string FormatUnknown = "CUSTOM_FORMAT_SANDBOX_FORMAT_UNKNOWN";
        public const string BaseRuleSetRejected = "CUSTOM_FORMAT_SANDBOX_BASE_RULE_SET_REJECTED";
        public const string PackNotAllowed = "CUSTOM_FORMAT_SANDBOX_PACK_NOT_ALLOWED";
    }

    [Serializable]
    public sealed class CustomFormatSandboxResult
    {
        public bool accepted;
        public string rejection_reason;
        public string catalog_rejection_reason;
        public string requested_format;
        public string resolved_format_id;
        public string display_name;
        public string base_rule_set_profile_id;
        public string requested_pack_id;
        public bool pack_check_performed;
        public bool pack_allowed;
        public int allowed_pack_count;
        public List<string> enabled_features = new List<string>();
        public RuleSetProfile rule_set_profile;
        public string summary;

        public void EnsureLists()
        {
            if (enabled_features == null)
            {
                enabled_features = new List<string>();
            }

            rule_set_profile?.EnsureLists();
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static CustomFormatSandboxResult FromJson(string json)
        {
            CustomFormatSandboxResult result =
                JsonUtility.FromJson<CustomFormatSandboxResult>(json);
            if (result == null)
            {
                throw new ArgumentException("Custom format sandbox result JSON could not be parsed.", "json");
            }

            result.EnsureLists();
            return result;
        }
    }

    public static class CustomFormatSandbox
    {
        public static CustomFormatSandboxResult Preview(
            CustomFormatProfileCatalogDefinition catalog,
            string requestedFormat,
            string requestedPackId = "")
        {
            return Preview(
                catalog,
                RuleSetProfileCatalog.CreateCoreProfiles(),
                requestedFormat,
                requestedPackId);
        }

        public static CustomFormatSandboxResult Preview(
            CustomFormatProfileCatalogDefinition catalog,
            RuleSetProfileCatalogDefinition ruleSetCatalog,
            string requestedFormat,
            string requestedPackId = "")
        {
            string normalizedFormat = Normalize(requestedFormat);
            if (string.IsNullOrEmpty(normalizedFormat))
            {
                return Reject(
                    CustomFormatSandboxRejectionReasons.FormatMissing,
                    string.Empty,
                    string.Empty,
                    requestedFormat,
                    requestedPackId,
                    string.Empty);
            }

            CustomFormatProfileCatalogDefinition catalogClone = CloneCatalog(catalog);
            CustomFormatProfileValidationResult validation =
                CustomFormatProfileCatalog.Validate(catalogClone, ruleSetCatalog);
            if (!validation.accepted)
            {
                return Reject(
                    CustomFormatSandboxRejectionReasons.CatalogInvalid,
                    string.Empty,
                    string.Empty,
                    requestedFormat,
                    requestedPackId,
                    validation.rejection_reason);
            }

            CustomFormatProfile profile = ResolveFormat(catalogClone, normalizedFormat);
            if (profile == null)
            {
                return Reject(
                    CustomFormatSandboxRejectionReasons.FormatUnknown,
                    string.Empty,
                    string.Empty,
                    requestedFormat,
                    requestedPackId,
                    string.Empty);
            }

            profile.EnsureLists();
            string normalizedPack = Normalize(requestedPackId);
            bool checkPack = !string.IsNullOrEmpty(normalizedPack);
            bool packAllowed = !checkPack || AllowsPack(profile, normalizedPack);
            if (checkPack && !packAllowed)
            {
                return Reject(
                    CustomFormatSandboxRejectionReasons.PackNotAllowed,
                    profile.format_id,
                    profile.base_rule_set_profile_id,
                    requestedFormat,
                    requestedPackId,
                    string.Empty,
                    true,
                    false,
                    profile.allowed_pack_ids.Count);
            }

            RuleSetProfileResolutionResult baseProfile =
                RuleSetProfileCatalog.Resolve(ruleSetCatalog, profile.base_rule_set_profile_id);
            if (!baseProfile.accepted || baseProfile.profile == null)
            {
                return Reject(
                    CustomFormatSandboxRejectionReasons.BaseRuleSetRejected,
                    profile.format_id,
                    profile.base_rule_set_profile_id,
                    requestedFormat,
                    requestedPackId,
                    baseProfile.rejection_reason,
                    checkPack,
                    packAllowed,
                    profile.allowed_pack_ids.Count);
            }

            List<string> features = BuildEnabledFeatures(baseProfile.profile);
            return new CustomFormatSandboxResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                catalog_rejection_reason = string.Empty,
                requested_format = requestedFormat ?? string.Empty,
                resolved_format_id = profile.format_id ?? string.Empty,
                display_name = profile.display_name ?? string.Empty,
                base_rule_set_profile_id = profile.base_rule_set_profile_id ?? string.Empty,
                requested_pack_id = requestedPackId ?? string.Empty,
                pack_check_performed = checkPack,
                pack_allowed = packAllowed,
                allowed_pack_count = profile.allowed_pack_ids.Count,
                enabled_features = features,
                rule_set_profile = CloneRuleSetProfile(baseProfile.profile),
                summary = "Custom format sandbox resolved " + (profile.format_id ?? string.Empty) + "."
            };
        }

        private static CustomFormatSandboxResult Reject(
            string reason,
            string resolvedFormatId,
            string baseRuleSetProfileId,
            string requestedFormat,
            string requestedPackId,
            string catalogRejectionReason,
            bool packCheckPerformed = false,
            bool packAllowed = false,
            int allowedPackCount = 0)
        {
            return new CustomFormatSandboxResult
            {
                accepted = false,
                rejection_reason = reason ?? string.Empty,
                catalog_rejection_reason = catalogRejectionReason ?? string.Empty,
                requested_format = requestedFormat ?? string.Empty,
                resolved_format_id = resolvedFormatId ?? string.Empty,
                display_name = string.Empty,
                base_rule_set_profile_id = baseRuleSetProfileId ?? string.Empty,
                requested_pack_id = requestedPackId ?? string.Empty,
                pack_check_performed = packCheckPerformed,
                pack_allowed = packAllowed,
                allowed_pack_count = allowedPackCount,
                enabled_features = new List<string>(),
                rule_set_profile = null,
                summary = "Custom format sandbox rejected: " + (reason ?? string.Empty)
            };
        }

        private static CustomFormatProfileCatalogDefinition CloneCatalog(
            CustomFormatProfileCatalogDefinition catalog)
        {
            if (catalog == null)
            {
                return null;
            }

            CustomFormatProfileCatalogDefinition clone =
                JsonUtility.FromJson<CustomFormatProfileCatalogDefinition>(
                    JsonUtility.ToJson(catalog, false));
            clone?.EnsureLists();
            return clone;
        }

        private static CustomFormatProfile ResolveFormat(
            CustomFormatProfileCatalogDefinition catalog,
            string normalizedFormat)
        {
            if (catalog == null || string.IsNullOrEmpty(normalizedFormat))
            {
                return null;
            }

            catalog.EnsureLists();
            for (int i = 0; i < catalog.formats.Count; i++)
            {
                CustomFormatProfile profile = catalog.formats[i];
                if (profile == null)
                {
                    continue;
                }

                profile.EnsureLists();
                if (Normalize(profile.format_id) == normalizedFormat ||
                    ContainsAlias(profile.aliases, normalizedFormat))
                {
                    return profile;
                }
            }

            return null;
        }

        private static bool AllowsPack(CustomFormatProfile profile, string normalizedPackId)
        {
            if (profile == null || string.IsNullOrEmpty(normalizedPackId))
            {
                return false;
            }

            profile.EnsureLists();
            if (profile.allowed_pack_ids.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < profile.allowed_pack_ids.Count; i++)
            {
                if (Normalize(profile.allowed_pack_ids[i]) == normalizedPackId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsAlias(List<string> aliases, string normalized)
        {
            if (aliases == null || string.IsNullOrEmpty(normalized))
            {
                return false;
            }

            for (int i = 0; i < aliases.Count; i++)
            {
                if (Normalize(aliases[i]) == normalized)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<string> BuildEnabledFeatures(RuleSetProfile profile)
        {
            List<string> features = new List<string>();
            if (profile == null)
            {
                return features;
            }

            AddFeature(features, profile.enable_ride_deck, "ride_deck");
            AddFeature(features, profile.enable_persona_ride, "persona_ride");
            AddFeature(features, profile.enable_over_trigger, "over_trigger");
            AddFeature(features, profile.enable_front_trigger, "front_trigger");
            AddFeature(features, profile.enable_stand_trigger, "stand_trigger");
            AddFeature(features, profile.enable_imaginary_gift, "imaginary_gift");
            AddFeature(features, profile.enable_stride, "stride");
            AddFeature(features, profile.enable_g_guard, "g_guard");
            AddFeature(features, profile.enable_g_zone, "g_zone");
            AddFeature(features, profile.enable_energy_module, "energy_module");
            AddFeature(features, profile.nation_fight, "nation_fight");
            AddFeature(features, profile.clan_fight, "clan_fight");
            AddFeature(features, profile.extreme_fight, "extreme_fight");
            return features;
        }

        private static void AddFeature(List<string> features, bool enabled, string feature)
        {
            if (enabled)
            {
                features.Add(feature);
            }
        }

        private static RuleSetProfile CloneRuleSetProfile(RuleSetProfile profile)
        {
            if (profile == null)
            {
                return null;
            }

            RuleSetProfile clone =
                JsonUtility.FromJson<RuleSetProfile>(JsonUtility.ToJson(profile, false));
            clone?.EnsureLists();
            return clone;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLowerInvariant().Replace(" ", "_");
        }
    }
}
