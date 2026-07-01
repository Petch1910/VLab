using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public enum RuleSetFeature
    {
        RideDeck,
        PersonaRide,
        OverTrigger,
        FrontTrigger,
        StandTrigger,
        ImaginaryGift,
        Stride,
        GGuard,
        GZone,
        EnergyModule,
        NationFight,
        ClanFight,
        ExtremeFight
    }

    public static class RuleSetProfileIds
    {
        public const string Standard = "standard";
        public const string VPremium = "v_premium";
        public const string Premium = "premium";
    }

    public static class RuleSetProfileRejectionReasons
    {
        public const string FormatMissing = "RULE_SET_FORMAT_MISSING";
        public const string FormatUnknown = "RULE_SET_FORMAT_UNKNOWN";
        public const string DuplicateProfile = "RULE_SET_DUPLICATE_PROFILE";
        public const string DuplicateAlias = "RULE_SET_DUPLICATE_ALIAS";
    }

    [Serializable]
    public sealed class RuleSetProfile
    {
        public string format_id;
        public string display_name;
        public List<string> aliases = new List<string>();
        public bool enable_ride_deck;
        public bool enable_persona_ride;
        public bool enable_over_trigger;
        public bool enable_front_trigger;
        public bool enable_stand_trigger;
        public bool enable_imaginary_gift;
        public bool enable_stride;
        public bool enable_g_guard;
        public bool enable_g_zone;
        public bool enable_energy_module;
        public bool nation_fight;
        public bool clan_fight;
        public bool extreme_fight;
        public string notes;

        public void EnsureLists()
        {
            if (aliases == null)
            {
                aliases = new List<string>();
            }
        }

        public bool HasFeature(RuleSetFeature feature)
        {
            switch (feature)
            {
                case RuleSetFeature.RideDeck:
                    return enable_ride_deck;
                case RuleSetFeature.PersonaRide:
                    return enable_persona_ride;
                case RuleSetFeature.OverTrigger:
                    return enable_over_trigger;
                case RuleSetFeature.FrontTrigger:
                    return enable_front_trigger;
                case RuleSetFeature.StandTrigger:
                    return enable_stand_trigger;
                case RuleSetFeature.ImaginaryGift:
                    return enable_imaginary_gift;
                case RuleSetFeature.Stride:
                    return enable_stride;
                case RuleSetFeature.GGuard:
                    return enable_g_guard;
                case RuleSetFeature.GZone:
                    return enable_g_zone;
                case RuleSetFeature.EnergyModule:
                    return enable_energy_module;
                case RuleSetFeature.NationFight:
                    return nation_fight;
                case RuleSetFeature.ClanFight:
                    return clan_fight;
                case RuleSetFeature.ExtremeFight:
                    return extreme_fight;
                default:
                    return false;
            }
        }
    }

    [Serializable]
    public sealed class RuleSetProfileCatalogDefinition
    {
        public List<RuleSetProfile> profiles = new List<RuleSetProfile>();

        public void EnsureLists()
        {
            if (profiles == null)
            {
                profiles = new List<RuleSetProfile>();
            }

            for (int i = 0; i < profiles.Count; i++)
            {
                profiles[i]?.EnsureLists();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static RuleSetProfileCatalogDefinition FromJson(string json)
        {
            RuleSetProfileCatalogDefinition catalog =
                JsonUtility.FromJson<RuleSetProfileCatalogDefinition>(json);
            if (catalog == null)
            {
                throw new ArgumentException("RuleSet profile catalog JSON could not be parsed.", "json");
            }

            catalog.EnsureLists();
            return catalog;
        }
    }

    [Serializable]
    public sealed class RuleSetProfileResolutionResult
    {
        public bool accepted;
        public string rejection_reason;
        public string requested_format;
        public RuleSetProfile profile;
        public string summary;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static RuleSetProfileResolutionResult FromJson(string json)
        {
            RuleSetProfileResolutionResult result =
                JsonUtility.FromJson<RuleSetProfileResolutionResult>(json);
            if (result == null)
            {
                throw new ArgumentException("RuleSet profile resolution result JSON could not be parsed.", "json");
            }

            result.profile?.EnsureLists();
            return result;
        }
    }

    public static class RuleSetProfileCatalog
    {
        public static RuleSetProfileCatalogDefinition CreateCoreProfiles()
        {
            return new RuleSetProfileCatalogDefinition
            {
                profiles = new List<RuleSetProfile>
                {
                    CreateStandardProfile(),
                    CreateVPremiumProfile(),
                    CreatePremiumProfile()
                }
            };
        }

        public static RuleSetProfileResolutionResult Resolve(string format)
        {
            return Resolve(CreateCoreProfiles(), format);
        }

        public static RuleSetProfileResolutionResult Resolve(GameState state)
        {
            return Resolve(state == null ? string.Empty : state.format);
        }

        public static RuleSetProfileResolutionResult Resolve(
            RuleSetProfileCatalogDefinition catalog,
            string format)
        {
            string normalized = Normalize(format);
            if (string.IsNullOrEmpty(normalized))
            {
                return Reject(RuleSetProfileRejectionReasons.FormatMissing, format, null);
            }

            if (catalog == null)
            {
                catalog = CreateCoreProfiles();
            }

            catalog.EnsureLists();
            for (int i = 0; i < catalog.profiles.Count; i++)
            {
                RuleSetProfile profile = catalog.profiles[i];
                if (profile == null)
                {
                    continue;
                }

                profile.EnsureLists();
                if (Normalize(profile.format_id) == normalized || ContainsAlias(profile.aliases, normalized))
                {
                    return new RuleSetProfileResolutionResult
                    {
                        accepted = true,
                        rejection_reason = string.Empty,
                        requested_format = format ?? string.Empty,
                        profile = CloneProfile(profile),
                        summary = "Resolved RuleSet profile " + profile.format_id + "."
                    };
                }
            }

            return Reject(RuleSetProfileRejectionReasons.FormatUnknown, format, null);
        }

        public static RuleSetProfileResolutionResult ValidateCatalog(
            RuleSetProfileCatalogDefinition catalog)
        {
            if (catalog == null)
            {
                catalog = CreateCoreProfiles();
            }

            catalog.EnsureLists();
            HashSet<string> profileIds = new HashSet<string>();
            HashSet<string> aliases = new HashSet<string>();
            for (int i = 0; i < catalog.profiles.Count; i++)
            {
                RuleSetProfile profile = catalog.profiles[i];
                if (profile == null)
                {
                    continue;
                }

                string profileId = Normalize(profile.format_id);
                if (!profileIds.Add(profileId))
                {
                    return Reject(RuleSetProfileRejectionReasons.DuplicateProfile, profile.format_id, profile);
                }

                profile.EnsureLists();
                for (int aliasIndex = 0; aliasIndex < profile.aliases.Count; aliasIndex++)
                {
                    string alias = Normalize(profile.aliases[aliasIndex]);
                    if (string.IsNullOrEmpty(alias))
                    {
                        continue;
                    }

                    if (!aliases.Add(alias) || profileIds.Contains(alias))
                    {
                        return Reject(RuleSetProfileRejectionReasons.DuplicateAlias, alias, profile);
                    }
                }
            }

            return new RuleSetProfileResolutionResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                requested_format = "catalog",
                profile = null,
                summary = "RuleSet profile catalog has unique ids and aliases."
            };
        }

        private static RuleSetProfile CreateStandardProfile()
        {
            return new RuleSetProfile
            {
                format_id = RuleSetProfileIds.Standard,
                display_name = "Standard",
                aliases = new List<string> { "d", "standard_d", "standard-nation" },
                enable_ride_deck = true,
                enable_persona_ride = true,
                enable_over_trigger = true,
                enable_front_trigger = true,
                enable_stand_trigger = false,
                enable_imaginary_gift = false,
                enable_stride = false,
                enable_g_guard = false,
                enable_g_zone = false,
                enable_energy_module = true,
                nation_fight = true,
                clan_fight = false,
                extreme_fight = false,
                notes = "Current Standard profile; energy is enabled as a module but paid only when an Energy Generator/crest exists."
            };
        }

        private static RuleSetProfile CreateVPremiumProfile()
        {
            return new RuleSetProfile
            {
                format_id = RuleSetProfileIds.VPremium,
                display_name = "V-Premium",
                aliases = new List<string> { "v", "v-premium", "vpremium" },
                enable_ride_deck = false,
                enable_persona_ride = false,
                enable_over_trigger = false,
                enable_front_trigger = true,
                enable_stand_trigger = false,
                enable_imaginary_gift = true,
                enable_stride = false,
                enable_g_guard = false,
                enable_g_zone = false,
                enable_energy_module = false,
                nation_fight = false,
                clan_fight = true,
                extreme_fight = false,
                notes = "V-Premium profile keeps Imaginary Gift enabled and excludes Standard ride deck/over trigger modules."
            };
        }

        private static RuleSetProfile CreatePremiumProfile()
        {
            return new RuleSetProfile
            {
                format_id = RuleSetProfileIds.Premium,
                display_name = "Premium",
                aliases = new List<string> { "p", "premium-clan" },
                enable_ride_deck = false,
                enable_persona_ride = false,
                enable_over_trigger = true,
                enable_front_trigger = true,
                enable_stand_trigger = true,
                enable_imaginary_gift = true,
                enable_stride = true,
                enable_g_guard = true,
                enable_g_zone = true,
                enable_energy_module = false,
                nation_fight = false,
                clan_fight = true,
                extreme_fight = false,
                notes = "Premium profile enables G zone, stride, G-Guard, legacy triggers, and gift modules when card effects grant them."
            };
        }

        private static RuleSetProfileResolutionResult Reject(
            string rejectionReason,
            string requestedFormat,
            RuleSetProfile profile)
        {
            return new RuleSetProfileResolutionResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                requested_format = requestedFormat ?? string.Empty,
                profile = profile == null ? null : CloneProfile(profile),
                summary = "RuleSet profile rejected: " + (rejectionReason ?? string.Empty)
            };
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

        private static RuleSetProfile CloneProfile(RuleSetProfile profile)
        {
            if (profile == null)
            {
                return null;
            }

            profile.EnsureLists();
            return JsonUtility.FromJson<RuleSetProfile>(JsonUtility.ToJson(profile, false));
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLowerInvariant().Replace(" ", "_");
        }
    }
}
