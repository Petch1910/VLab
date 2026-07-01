using NUnit.Framework;
using VanguardThaiSim.Cards;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class CardBrowserSearchPanelFormatterTests
    {
        [Test]
        public void NoFiltersFormatsClearDefaultMessage()
        {
            Assert.AreEqual(
                CardBrowserSearchPanelFormatter.NoActiveFiltersMessage,
                CardBrowserSearchPanelFormatter.FormatEmptyResult(null, null, null));
        }

        [Test]
        public void ThaiQueryIsPreservedInEmptyResultMessage()
        {
            string thaiQuery = "\u0E2D\u0E31\u0E25\u0E40\u0E1F\u0E23\u0E14";

            string formatted = CardBrowserSearchPanelFormatter.FormatEmptyResult(thaiQuery, null, null);

            Assert.IsTrue(formatted.Contains("Query: \"" + thaiQuery + "\""));
            Assert.IsTrue(formatted.Contains(CardBrowserSearchPanelFormatter.ClearFiltersHint));
        }

        [Test]
        public void EnglishQueryAndFiltersAreVisible()
        {
            string formatted = CardBrowserSearchPanelFormatter.FormatEmptyResult(
                "Blaster Blade",
                "[BT01] VG-BT01 : Descent of the King of Knights",
                "Royal Paladin");

            Assert.IsTrue(formatted.Contains("Query: \"Blaster Blade\""));
            Assert.IsTrue(formatted.Contains("Series: [BT01] VG-BT01 : Descent of the King of Knights"));
            Assert.IsTrue(formatted.Contains("Group: Royal Paladin"));
        }

        [Test]
        public void SearchWhitespaceIsNormalizedForDisplay()
        {
            string formatted = CardBrowserSearchPanelFormatter.FormatEmptyResult(
                "  Blaster\n\tBlade  ",
                "  Series\r\nName  ",
                "  Clan\tName  ");

            Assert.IsTrue(formatted.Contains("Query: \"Blaster Blade\""));
            Assert.IsTrue(formatted.Contains("Series: Series Name"));
            Assert.IsTrue(formatted.Contains("Group: Clan Name"));
        }

        [Test]
        public void BlankValuesAreOmitted()
        {
            string formatted = CardBrowserSearchPanelFormatter.FormatEmptyResult(
                "Dragon",
                " ",
                "\n");

            Assert.IsTrue(formatted.Contains("Query: \"Dragon\""));
            Assert.IsFalse(formatted.Contains("Series:"));
            Assert.IsFalse(formatted.Contains("Group:"));
        }

        [Test]
        public void FormattingDoesNotMutateOptions()
        {
            CardQueryOptions options = new CardQueryOptions
            {
                SearchText = "  Blaster\nBlade  ",
                Series = "Series",
                Clan = "Clan",
                Nation = "Nation",
                Limit = 24,
                Offset = 48
            };

            CardBrowserSearchPanelFormatter.FormatEmptyResult(options);

            Assert.AreEqual("  Blaster\nBlade  ", options.SearchText);
            Assert.AreEqual("Series", options.Series);
            Assert.AreEqual("Clan", options.Clan);
            Assert.AreEqual("Nation", options.Nation);
            Assert.AreEqual(24, options.Limit);
            Assert.AreEqual(48, options.Offset);
        }

        [Test]
        public void NationFilterIsVisibleFromOptions()
        {
            CardQueryOptions options = new CardQueryOptions
            {
                Nation = "Dragon Empire D"
            };

            string formatted = CardBrowserSearchPanelFormatter.FormatEmptyResult(options);

            Assert.IsTrue(formatted.Contains("Group: Nation Dragon Empire D"));
        }
    }
}
