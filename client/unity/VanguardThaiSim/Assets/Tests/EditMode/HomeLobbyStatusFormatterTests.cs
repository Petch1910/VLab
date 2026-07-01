using NUnit.Framework;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class HomeLobbyStatusFormatterTests
    {
        [Test]
        public void PackStatusIncludesManifestCounts()
        {
            CardPackManifest manifest = new CardPackManifest
            {
                display_name = "Vanguard TH",
                pack_id = "vanguard_th",
                source_version = "2026.06",
                card_count = 10836,
                series_count = 128,
                clan_count = 32
            };

            string formatted = HomeLobbyStatusFormatter.FormatPackStatus(manifest, null);

            Assert.IsTrue(formatted.Contains("Vanguard TH"));
            Assert.IsTrue(formatted.Contains("2026.06"));
            Assert.IsTrue(formatted.Contains("Cards 10836"));
            Assert.IsTrue(formatted.Contains("Clans 32"));
        }

        [Test]
        public void PackStatusShowsLoadErrorWithoutNewlines()
        {
            string formatted = HomeLobbyStatusFormatter.FormatPackStatus(
                null,
                "FileNotFound\nmissing manifest");

            Assert.IsTrue(formatted.Contains("Pack: unavailable"));
            Assert.IsTrue(formatted.Contains("FileNotFound missing manifest"));
        }

        [Test]
        public void DeckStatusPromptsForDeckBuilderWhenNoDeck()
        {
            string formatted = HomeLobbyStatusFormatter.FormatDeckStatus(null, null);

            Assert.IsTrue(formatted.Contains("no saved deck"));
            Assert.IsTrue(formatted.Contains("Deck Builder"));
        }

        [Test]
        public void DeckStatusIncludesDeckCounts()
        {
            VanguardDeck deck = VanguardDeck.Create("Test Deck", "D", "pack", "version");
            deck.AddCard(DeckZone.Main, "CARD-001", 50);
            deck.AddCard(DeckZone.Ride, "CARD-002", 4);

            string formatted = HomeLobbyStatusFormatter.FormatDeckStatus(deck, null);

            Assert.IsTrue(formatted.Contains("Test Deck"));
            Assert.IsTrue(formatted.Contains("Main 50"));
            Assert.IsTrue(formatted.Contains("Ride 4"));
        }

        [Test]
        public void TaxonomyBaselineDocumentsVanguardAreaStyleWithoutCopyingFiles()
        {
            Assert.IsTrue(HomeLobbyStatusFormatter.TaxonomyBaseline.Contains("Vanguard Area-style clan/nation"));
            Assert.IsTrue(HomeLobbyStatusFormatter.TaxonomyBaseline.Contains("local database"));
        }

        [Test]
        public void IconPackStatusShowsUncheckedFallback()
        {
            string formatted = HomeLobbyStatusFormatter.FormatIconPackStatus(null);

            Assert.IsTrue(formatted.Contains("default text badges"));
            Assert.IsTrue(formatted.Contains("not checked"));
        }

        [Test]
        public void IconPackStatusShowsAcceptedOverrideCounts()
        {
            UserIconPackValidationResult result = new UserIconPackValidationResult
            {
                accepted = true,
                declared_icon_count = 3,
                accepted_icon_count = 1,
                fallback_icon_count = 2
            };

            string formatted = HomeLobbyStatusFormatter.FormatIconPackStatus(result);

            Assert.IsTrue(formatted.Contains("user overrides 1/3"));
            Assert.IsTrue(formatted.Contains("Fallbacks: 2 defaults"));
        }

        [Test]
        public void IconPackStatusShowsDefaultWhenNoOverridesLoaded()
        {
            UserIconPackValidationResult result = new UserIconPackValidationResult
            {
                accepted = true,
                declared_icon_count = 0,
                accepted_icon_count = 0,
                fallback_icon_count = 0
            };

            string formatted = HomeLobbyStatusFormatter.FormatIconPackStatus(result);

            Assert.IsTrue(formatted.Contains("default text badges"));
            Assert.IsTrue(formatted.Contains("none loaded"));
        }

        [Test]
        public void IconPackStatusShowsRejectedCodeWithoutPath()
        {
            UserIconPackValidationResult result = new UserIconPackValidationResult
            {
                accepted = false,
                declared_icon_count = 1
            };
            result.issues.Add(new UserIconPackValidationIssue
            {
                severity = "error",
                code = "ICON_FILE_OUTSIDE_ROOT",
                message = "C:\\Users\\Phet\\secret.png"
            });

            string formatted = HomeLobbyStatusFormatter.FormatIconPackStatus(result);

            Assert.IsTrue(formatted.Contains("rejected: ICON_FILE_OUTSIDE_ROOT"));
            Assert.IsFalse(formatted.Contains("C:\\Users"));
            Assert.IsFalse(formatted.Contains("secret.png"));
        }

        [Test]
        public void ModeStatusCanShowReloadTip()
        {
            string formatted = HomeLobbyStatusFormatter.FormatModeStatus(
                LoadingTipCatalog.AppendTip("Startup data reloaded.", LoadingTipCatalog.DataReload));

            StringAssert.Contains("Startup data reloaded.", formatted);
            StringAssert.Contains("Tip:", formatted);
            StringAssert.Contains("validation status", formatted);
        }
    }
}
