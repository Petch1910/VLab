using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class CustomFormatSandboxTests
    {
        [Test]
        public void CorePresetCatalogResolvesAliasAndBaseRuleSet()
        {
            CustomFormatProfileCatalogDefinition catalog =
                CustomFormatProfileCatalog.CreateCorePresetCatalog();

            CustomFormatSandboxResult result =
                CustomFormatSandbox.Preview(catalog, "premium-clan", "vanguard_th");

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual("premium", result.resolved_format_id);
            Assert.AreEqual(RuleSetProfileIds.Premium, result.base_rule_set_profile_id);
            Assert.IsTrue(result.pack_check_performed);
            Assert.IsTrue(result.pack_allowed);
            Assert.IsNotNull(result.rule_set_profile);
            Assert.IsTrue(result.rule_set_profile.enable_stride);
            Assert.Contains("stride", result.enabled_features);
            Assert.Contains("g_guard", result.enabled_features);
            Assert.Contains("clan_fight", result.enabled_features);
        }

        [Test]
        public void UnknownFormatRejectsWithoutMutatingSourceCatalog()
        {
            CustomFormatProfileCatalogDefinition catalog =
                CustomFormatProfileCatalog.CreateCorePresetCatalog();
            string before = JsonUtility.ToJson(catalog, false);

            CustomFormatSandboxResult result =
                CustomFormatSandbox.Preview(catalog, "future", "vanguard_th");
            string after = JsonUtility.ToJson(catalog, false);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(CustomFormatSandboxRejectionReasons.FormatUnknown, result.rejection_reason);
            Assert.AreEqual(before, after);
        }

        [Test]
        public void DisallowedPackRejectsWithoutMutatingSourceCatalog()
        {
            CustomFormatProfileCatalogDefinition catalog =
                CustomFormatProfileCatalog.CreateCorePresetCatalog();
            string before = JsonUtility.ToJson(catalog, false);

            CustomFormatSandboxResult result =
                CustomFormatSandbox.Preview(catalog, "v", "fan_pack");
            string after = JsonUtility.ToJson(catalog, false);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(CustomFormatSandboxRejectionReasons.PackNotAllowed, result.rejection_reason);
            Assert.AreEqual("v_premium", result.resolved_format_id);
            Assert.AreEqual(RuleSetProfileIds.VPremium, result.base_rule_set_profile_id);
            Assert.IsTrue(result.pack_check_performed);
            Assert.IsFalse(result.pack_allowed);
            Assert.AreEqual(before, after);
        }

        [Test]
        public void InvalidCatalogRejectsWithCatalogReason()
        {
            CustomFormatProfileCatalogDefinition catalog =
                CustomFormatProfileCatalog.CreateCorePresetCatalog();
            catalog.formats[0].base_rule_set_profile_id = "future";

            CustomFormatSandboxResult result =
                CustomFormatSandbox.Preview(catalog, "standard", "vanguard_th");

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(CustomFormatSandboxRejectionReasons.CatalogInvalid, result.rejection_reason);
            Assert.AreEqual(
                CustomFormatProfileRejectionReasons.BaseRuleSetUnknown,
                result.catalog_rejection_reason);
        }

        [Test]
        public void EmptyAllowedPackListAcceptsAnyPack()
        {
            CustomFormatProfileCatalogDefinition catalog = new CustomFormatProfileCatalogDefinition
            {
                formats = new List<CustomFormatProfile>
                {
                    new CustomFormatProfile
                    {
                        format_id = "sandbox_standard",
                        display_name = "Sandbox Standard",
                        base_rule_set_profile_id = RuleSetProfileIds.Standard,
                        allowed_pack_ids = new List<string>()
                    }
                }
            };

            CustomFormatSandboxResult result =
                CustomFormatSandbox.Preview(catalog, "sandbox_standard", "fan_pack");

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.pack_check_performed);
            Assert.IsTrue(result.pack_allowed);
            Assert.AreEqual(0, result.allowed_pack_count);
            Assert.AreEqual(RuleSetProfileIds.Standard, result.base_rule_set_profile_id);
        }

        [Test]
        public void ResultJsonRoundTripPreservesFeatureSummary()
        {
            CustomFormatSandboxResult result = CustomFormatSandbox.Preview(
                CustomFormatProfileCatalog.CreateCorePresetCatalog(),
                "d",
                "vanguard_th");

            CustomFormatSandboxResult roundTrip =
                CustomFormatSandboxResult.FromJson(result.ToJson(false));

            Assert.AreEqual(result.accepted, roundTrip.accepted);
            Assert.AreEqual(result.resolved_format_id, roundTrip.resolved_format_id);
            Assert.AreEqual(result.enabled_features.Count, roundTrip.enabled_features.Count);
            Assert.IsNotNull(roundTrip.rule_set_profile);
            Assert.IsTrue(roundTrip.rule_set_profile.enable_ride_deck);
            Assert.Contains("ride_deck", roundTrip.enabled_features);
        }
    }
}
