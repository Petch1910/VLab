using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class CustomFormatProfileTests
    {
        [Test]
        public void ValidCatalogReferencesBaseRuleSetProfiles()
        {
            CustomFormatProfileCatalogDefinition catalog = CreateCatalog();

            CustomFormatProfileValidationResult result =
                CustomFormatProfileCatalog.Validate(catalog);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual("catalog", result.format_id);
        }

        [Test]
        public void StandardPresetUsesCoreStandardFlags()
        {
            CustomFormatProfile standard = CustomFormatProfileCatalog.CreateStandardPreset();
            CustomFormatProfileCatalogDefinition catalog =
                CustomFormatProfileCatalog.CreateStandardPresetCatalog();

            CustomFormatProfileValidationResult validation =
                CustomFormatProfileCatalog.Validate(catalog);
            RuleSetProfile profile =
                RuleSetProfileCatalog.Resolve(standard.base_rule_set_profile_id).profile;

            Assert.IsTrue(validation.accepted, validation.rejection_reason);
            Assert.AreEqual("standard", standard.format_id);
            Assert.AreEqual(RuleSetProfileIds.Standard, standard.base_rule_set_profile_id);
            Assert.IsTrue(profile.enable_ride_deck);
            Assert.IsTrue(profile.enable_persona_ride);
            Assert.IsTrue(profile.enable_over_trigger);
            Assert.IsTrue(profile.nation_fight);
            Assert.IsFalse(profile.enable_imaginary_gift);
            Assert.IsFalse(profile.enable_stride);
            Assert.IsFalse(profile.clan_fight);
        }

        [Test]
        public void VPremiumPresetUsesCoreVPremiumFlags()
        {
            CustomFormatProfile vPremium = CustomFormatProfileCatalog.CreateVPremiumPreset();
            CustomFormatProfileCatalogDefinition catalog =
                CustomFormatProfileCatalog.CreateVPremiumPresetCatalog();

            CustomFormatProfileValidationResult validation =
                CustomFormatProfileCatalog.Validate(catalog);
            RuleSetProfile profile =
                RuleSetProfileCatalog.Resolve(vPremium.base_rule_set_profile_id).profile;

            Assert.IsTrue(validation.accepted, validation.rejection_reason);
            Assert.AreEqual("v_premium", vPremium.format_id);
            Assert.AreEqual(RuleSetProfileIds.VPremium, vPremium.base_rule_set_profile_id);
            Assert.IsTrue(profile.enable_imaginary_gift);
            Assert.IsTrue(profile.enable_front_trigger);
            Assert.IsTrue(profile.clan_fight);
            Assert.IsFalse(profile.enable_ride_deck);
            Assert.IsFalse(profile.enable_persona_ride);
            Assert.IsFalse(profile.enable_over_trigger);
            Assert.IsFalse(profile.enable_stride);
            Assert.IsFalse(profile.enable_g_zone);
            Assert.IsFalse(profile.enable_energy_module);
        }

        [Test]
        public void PremiumPresetUsesCorePremiumFlags()
        {
            CustomFormatProfile premium = CustomFormatProfileCatalog.CreatePremiumPreset();
            CustomFormatProfileCatalogDefinition catalog =
                CustomFormatProfileCatalog.CreatePremiumPresetCatalog();

            CustomFormatProfileValidationResult validation =
                CustomFormatProfileCatalog.Validate(catalog);
            RuleSetProfile profile =
                RuleSetProfileCatalog.Resolve(premium.base_rule_set_profile_id).profile;

            Assert.IsTrue(validation.accepted, validation.rejection_reason);
            Assert.AreEqual("premium", premium.format_id);
            Assert.AreEqual(RuleSetProfileIds.Premium, premium.base_rule_set_profile_id);
            Assert.IsTrue(profile.enable_stride);
            Assert.IsTrue(profile.enable_g_guard);
            Assert.IsTrue(profile.enable_g_zone);
            Assert.IsTrue(profile.enable_stand_trigger);
            Assert.IsTrue(profile.enable_imaginary_gift);
            Assert.IsTrue(profile.clan_fight);
            Assert.IsFalse(profile.enable_ride_deck);
            Assert.IsFalse(profile.enable_energy_module);
            Assert.IsFalse(profile.extreme_fight);
        }

        [Test]
        public void ValidationRejectsUnknownBaseRuleSet()
        {
            CustomFormatProfileCatalogDefinition catalog = CreateCatalog();
            catalog.formats[0].base_rule_set_profile_id = "future";

            CustomFormatProfileValidationResult result =
                CustomFormatProfileCatalog.Validate(catalog);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(CustomFormatProfileRejectionReasons.BaseRuleSetUnknown, result.rejection_reason);
        }

        [Test]
        public void ValidationRejectsDuplicateFormatAliasAndPack()
        {
            CustomFormatProfileCatalogDefinition duplicateFormat = CreateCatalog();
            duplicateFormat.formats.Add(new CustomFormatProfile
            {
                format_id = "custom_standard",
                base_rule_set_profile_id = RuleSetProfileIds.Standard
            });
            CustomFormatProfileCatalogDefinition duplicateAlias = CreateCatalog();
            duplicateAlias.formats.Add(new CustomFormatProfile
            {
                format_id = "custom_second",
                base_rule_set_profile_id = RuleSetProfileIds.Standard,
                aliases = new List<string> { "cs" }
            });
            CustomFormatProfileCatalogDefinition duplicatePack = CreateCatalog();
            duplicatePack.formats[0].allowed_pack_ids.Add("vanguard_th");

            Assert.AreEqual(
                CustomFormatProfileRejectionReasons.DuplicateFormat,
                CustomFormatProfileCatalog.Validate(duplicateFormat).rejection_reason);
            Assert.AreEqual(
                CustomFormatProfileRejectionReasons.DuplicateAlias,
                CustomFormatProfileCatalog.Validate(duplicateAlias).rejection_reason);
            Assert.AreEqual(
                CustomFormatProfileRejectionReasons.DuplicatePack,
                CustomFormatProfileCatalog.Validate(duplicatePack).rejection_reason);
        }

        [Test]
        public void CatalogAndResultJsonRoundTrip()
        {
            CustomFormatProfileCatalogDefinition catalog = CreateCatalog();
            CustomFormatProfileCatalogDefinition roundTrip =
                CustomFormatProfileCatalogDefinition.FromJson(catalog.ToJson(false));
            CustomFormatProfileValidationResult result =
                CustomFormatProfileCatalog.Validate(catalog);
            CustomFormatProfileValidationResult resultRoundTrip =
                CustomFormatProfileValidationResult.FromJson(result.ToJson(false));

            Assert.AreEqual(catalog.formats.Count, roundTrip.formats.Count);
            Assert.AreEqual(catalog.formats[0].format_id, roundTrip.formats[0].format_id);
            Assert.AreEqual(catalog.formats[0].aliases.Count, roundTrip.formats[0].aliases.Count);
            Assert.AreEqual(result.accepted, resultRoundTrip.accepted);
            Assert.AreEqual(result.summary, resultRoundTrip.summary);
        }

        private static CustomFormatProfileCatalogDefinition CreateCatalog()
        {
            return new CustomFormatProfileCatalogDefinition
            {
                formats = new List<CustomFormatProfile>
                {
                    new CustomFormatProfile
                    {
                        format_id = "custom_standard",
                        display_name = "Custom Standard",
                        base_rule_set_profile_id = RuleSetProfileIds.Standard,
                        aliases = new List<string> { "cs" },
                        allowed_pack_ids = new List<string> { "vanguard_th" },
                        notes = "Data-only profile for custom Standard testing."
                    }
                }
            };
        }
    }
}
