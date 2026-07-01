using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class CustomFormatValidationCloseoutTests
    {
        [Test]
        public void CorePresetCatalogContainsDistinctStandardVPremiumAndPremiumProfiles()
        {
            CustomFormatProfileCatalogDefinition catalog =
                CustomFormatProfileCatalog.CreateCorePresetCatalog();

            CustomFormatSandboxResult standard =
                CustomFormatSandbox.Preview(catalog, "standard", "vanguard_th");
            CustomFormatSandboxResult vPremium =
                CustomFormatSandbox.Preview(catalog, "v-premium", "vanguard_th");
            CustomFormatSandboxResult premium =
                CustomFormatSandbox.Preview(catalog, "p", "vanguard_th");

            Assert.IsTrue(standard.accepted, standard.rejection_reason);
            Assert.IsTrue(vPremium.accepted, vPremium.rejection_reason);
            Assert.IsTrue(premium.accepted, premium.rejection_reason);

            Assert.AreEqual(RuleSetProfileIds.Standard, standard.base_rule_set_profile_id);
            Assert.AreEqual(RuleSetProfileIds.VPremium, vPremium.base_rule_set_profile_id);
            Assert.AreEqual(RuleSetProfileIds.Premium, premium.base_rule_set_profile_id);

            Assert.Contains("ride_deck", standard.enabled_features);
            Assert.Contains("nation_fight", standard.enabled_features);
            Assert.IsFalse(standard.enabled_features.Contains("imaginary_gift"));
            Assert.IsFalse(standard.enabled_features.Contains("stride"));

            Assert.Contains("imaginary_gift", vPremium.enabled_features);
            Assert.Contains("clan_fight", vPremium.enabled_features);
            Assert.IsFalse(vPremium.enabled_features.Contains("over_trigger"));
            Assert.IsFalse(vPremium.enabled_features.Contains("g_zone"));

            Assert.Contains("stride", premium.enabled_features);
            Assert.Contains("g_guard", premium.enabled_features);
            Assert.Contains("g_zone", premium.enabled_features);
            Assert.Contains("stand_trigger", premium.enabled_features);
            Assert.IsFalse(premium.enabled_features.Contains("ride_deck"));
        }

        [Test]
        public void CorePresetCatalogRoundTripsAndStaysValid()
        {
            CustomFormatProfileCatalogDefinition catalog =
                CustomFormatProfileCatalog.CreateCorePresetCatalog();

            CustomFormatProfileCatalogDefinition roundTrip =
                CustomFormatProfileCatalogDefinition.FromJson(catalog.ToJson(false));
            CustomFormatProfileValidationResult validation =
                CustomFormatProfileCatalog.Validate(roundTrip);

            Assert.AreEqual(3, roundTrip.formats.Count);
            Assert.IsTrue(validation.accepted, validation.rejection_reason);
            Assert.AreEqual("standard", roundTrip.formats[0].format_id);
            Assert.AreEqual("v_premium", roundTrip.formats[1].format_id);
            Assert.AreEqual("premium", roundTrip.formats[2].format_id);
        }

        [Test]
        public void SandboxPreviewDoesNotMutateFormatOrRuleSetCatalog()
        {
            CustomFormatProfileCatalogDefinition formatCatalog =
                CustomFormatProfileCatalog.CreateCorePresetCatalog();
            RuleSetProfileCatalogDefinition ruleSetCatalog =
                RuleSetProfileCatalog.CreateCoreProfiles();
            string formatBefore = JsonUtility.ToJson(formatCatalog, false);
            string ruleSetBefore = JsonUtility.ToJson(ruleSetCatalog, false);

            CustomFormatSandboxResult result =
                CustomFormatSandbox.Preview(formatCatalog, ruleSetCatalog, "premium", "vanguard_th");
            string formatAfter = JsonUtility.ToJson(formatCatalog, false);
            string ruleSetAfter = JsonUtility.ToJson(ruleSetCatalog, false);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(formatBefore, formatAfter);
            Assert.AreEqual(ruleSetBefore, ruleSetAfter);
        }

        [Test]
        public void CatalogAliasConflictRejectsBeforeSandboxResolution()
        {
            CustomFormatProfileCatalogDefinition catalog =
                CustomFormatProfileCatalog.CreateCorePresetCatalog();
            catalog.formats.Add(new CustomFormatProfile
            {
                format_id = "duplicate_alias_format",
                display_name = "Duplicate Alias Format",
                base_rule_set_profile_id = RuleSetProfileIds.Standard,
                aliases = new List<string> { "p" },
                allowed_pack_ids = new List<string> { "vanguard_th" }
            });

            CustomFormatSandboxResult result =
                CustomFormatSandbox.Preview(catalog, "p", "vanguard_th");

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(CustomFormatSandboxRejectionReasons.CatalogInvalid, result.rejection_reason);
            Assert.AreEqual(
                CustomFormatProfileRejectionReasons.DuplicateAlias,
                result.catalog_rejection_reason);
        }

        [Test]
        public void PackBoundaryRemainsPreviewOnlyAndUsesStableRejects()
        {
            CustomFormatProfileCatalogDefinition catalog = new CustomFormatProfileCatalogDefinition
            {
                formats = new List<CustomFormatProfile>
                {
                    new CustomFormatProfile
                    {
                        format_id = "team_standard",
                        display_name = "Team Standard",
                        base_rule_set_profile_id = RuleSetProfileIds.Standard,
                        aliases = new List<string> { "team" },
                        allowed_pack_ids = new List<string> { "team_pack" }
                    }
                }
            };

            CustomFormatSandboxResult accepted =
                CustomFormatSandbox.Preview(catalog, "team", "team_pack");
            CustomFormatSandboxResult rejected =
                CustomFormatSandbox.Preview(catalog, "team", "other_pack");

            Assert.IsTrue(accepted.accepted, accepted.rejection_reason);
            Assert.AreEqual("team_standard", accepted.resolved_format_id);
            Assert.IsTrue(accepted.pack_check_performed);
            Assert.IsTrue(accepted.pack_allowed);
            Assert.IsNotNull(accepted.rule_set_profile);

            Assert.IsFalse(rejected.accepted);
            Assert.AreEqual(CustomFormatSandboxRejectionReasons.PackNotAllowed, rejected.rejection_reason);
            Assert.IsTrue(rejected.pack_check_performed);
            Assert.IsFalse(rejected.pack_allowed);
            Assert.IsNull(rejected.rule_set_profile);
        }
    }
}
