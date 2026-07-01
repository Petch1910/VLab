using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class RuleSetProfileTests
    {
        [Test]
        public void StandardProfileSeparatesStandardOnlyFlags()
        {
            RuleSetProfile profile = Resolve("D");

            Assert.AreEqual(RuleSetProfileIds.Standard, profile.format_id);
            Assert.IsTrue(profile.HasFeature(RuleSetFeature.RideDeck));
            Assert.IsTrue(profile.HasFeature(RuleSetFeature.PersonaRide));
            Assert.IsTrue(profile.HasFeature(RuleSetFeature.OverTrigger));
            Assert.IsTrue(profile.HasFeature(RuleSetFeature.FrontTrigger));
            Assert.IsTrue(profile.HasFeature(RuleSetFeature.EnergyModule));
            Assert.IsTrue(profile.HasFeature(RuleSetFeature.NationFight));
            Assert.IsFalse(profile.HasFeature(RuleSetFeature.ImaginaryGift));
            Assert.IsFalse(profile.HasFeature(RuleSetFeature.Stride));
            Assert.IsFalse(profile.HasFeature(RuleSetFeature.GZone));
            Assert.IsFalse(profile.HasFeature(RuleSetFeature.ClanFight));
        }

        [Test]
        public void VPremiumProfileSeparatesGiftAndClanFightFlags()
        {
            RuleSetProfile profile = Resolve("v-premium");

            Assert.AreEqual(RuleSetProfileIds.VPremium, profile.format_id);
            Assert.IsTrue(profile.HasFeature(RuleSetFeature.ImaginaryGift));
            Assert.IsTrue(profile.HasFeature(RuleSetFeature.FrontTrigger));
            Assert.IsTrue(profile.HasFeature(RuleSetFeature.ClanFight));
            Assert.IsFalse(profile.HasFeature(RuleSetFeature.RideDeck));
            Assert.IsFalse(profile.HasFeature(RuleSetFeature.PersonaRide));
            Assert.IsFalse(profile.HasFeature(RuleSetFeature.OverTrigger));
            Assert.IsFalse(profile.HasFeature(RuleSetFeature.Stride));
            Assert.IsFalse(profile.HasFeature(RuleSetFeature.GZone));
            Assert.IsFalse(profile.HasFeature(RuleSetFeature.EnergyModule));
        }

        [Test]
        public void PremiumProfileSeparatesStrideGZoneAndLegacyTriggerFlags()
        {
            RuleSetProfile profile = Resolve("premium");

            Assert.AreEqual(RuleSetProfileIds.Premium, profile.format_id);
            Assert.IsTrue(profile.HasFeature(RuleSetFeature.Stride));
            Assert.IsTrue(profile.HasFeature(RuleSetFeature.GGuard));
            Assert.IsTrue(profile.HasFeature(RuleSetFeature.GZone));
            Assert.IsTrue(profile.HasFeature(RuleSetFeature.StandTrigger));
            Assert.IsTrue(profile.HasFeature(RuleSetFeature.ImaginaryGift));
            Assert.IsTrue(profile.HasFeature(RuleSetFeature.ClanFight));
            Assert.IsFalse(profile.HasFeature(RuleSetFeature.RideDeck));
            Assert.IsFalse(profile.HasFeature(RuleSetFeature.EnergyModule));
            Assert.IsFalse(profile.HasFeature(RuleSetFeature.ExtremeFight));
        }

        [Test]
        public void ResolverUsesGameStateFormatWithoutMutatingState()
        {
            GameState state = new GameState
            {
                format = "P",
                players = new List<PlayerGameState>(),
                event_log = new List<GameEvent>(),
                pending_auto_abilities = new PendingAutoAbilityQueue()
            };
            GameStateNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(state);

            RuleSetProfileResolutionResult result = RuleSetProfileCatalog.Resolve(state);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(RuleSetProfileIds.Premium, result.profile.format_id);
            Assert.IsTrue(snapshot.Matches(state));
        }

        [Test]
        public void CatalogValidationRejectsDuplicateProfileIdsAndAliases()
        {
            RuleSetProfileCatalogDefinition duplicateProfiles = new RuleSetProfileCatalogDefinition
            {
                profiles = new List<RuleSetProfile>
                {
                    new RuleSetProfile { format_id = "custom" },
                    new RuleSetProfile { format_id = "custom" }
                }
            };
            RuleSetProfileCatalogDefinition duplicateAliases = new RuleSetProfileCatalogDefinition
            {
                profiles = new List<RuleSetProfile>
                {
                    new RuleSetProfile { format_id = "format-a", aliases = new List<string> { "same" } },
                    new RuleSetProfile { format_id = "format-b", aliases = new List<string> { "same" } }
                }
            };

            RuleSetProfileResolutionResult profileResult =
                RuleSetProfileCatalog.ValidateCatalog(duplicateProfiles);
            RuleSetProfileResolutionResult aliasResult =
                RuleSetProfileCatalog.ValidateCatalog(duplicateAliases);

            Assert.IsFalse(profileResult.accepted);
            Assert.AreEqual(RuleSetProfileRejectionReasons.DuplicateProfile, profileResult.rejection_reason);
            Assert.IsFalse(aliasResult.accepted);
            Assert.AreEqual(RuleSetProfileRejectionReasons.DuplicateAlias, aliasResult.rejection_reason);
        }

        [Test]
        public void CatalogAndResolutionRoundTripJson()
        {
            RuleSetProfileCatalogDefinition catalog = RuleSetProfileCatalog.CreateCoreProfiles();
            RuleSetProfileCatalogDefinition roundTrip =
                RuleSetProfileCatalogDefinition.FromJson(catalog.ToJson(false));
            RuleSetProfileResolutionResult result = RuleSetProfileCatalog.Resolve("standard");
            RuleSetProfileResolutionResult resultRoundTrip =
                RuleSetProfileResolutionResult.FromJson(result.ToJson(false));

            Assert.AreEqual(catalog.profiles.Count, roundTrip.profiles.Count);
            Assert.AreEqual(catalog.profiles[0].format_id, roundTrip.profiles[0].format_id);
            Assert.AreEqual(catalog.profiles[0].aliases.Count, roundTrip.profiles[0].aliases.Count);
            Assert.AreEqual(result.accepted, resultRoundTrip.accepted);
            Assert.AreEqual(result.profile.format_id, resultRoundTrip.profile.format_id);
            Assert.AreEqual(result.profile.enable_ride_deck, resultRoundTrip.profile.enable_ride_deck);
        }

        [Test]
        public void ResolverRejectsMissingAndUnknownFormats()
        {
            RuleSetProfileResolutionResult missing = RuleSetProfileCatalog.Resolve("");
            RuleSetProfileResolutionResult unknown = RuleSetProfileCatalog.Resolve("future-format");

            Assert.IsFalse(missing.accepted);
            Assert.AreEqual(RuleSetProfileRejectionReasons.FormatMissing, missing.rejection_reason);
            Assert.IsFalse(unknown.accepted);
            Assert.AreEqual(RuleSetProfileRejectionReasons.FormatUnknown, unknown.rejection_reason);
        }

        private static RuleSetProfile Resolve(string format)
        {
            RuleSetProfileResolutionResult result = RuleSetProfileCatalog.Resolve(format);
            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.NotNull(result.profile);
            return result.profile;
        }
    }
}
