using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class CardImageFallbackStatusFormatterTests
    {
        [Test]
        public void TileLabelAppendsFallbackOnlyWhenNeeded()
        {
            Assert.AreEqual(
                "Alfred",
                CardImageFallbackStatusFormatter.FormatTileLabel("Alfred", false));
            Assert.AreEqual(
                "Alfred\n[image fallback]",
                CardImageFallbackStatusFormatter.FormatTileLabel("Alfred", true));
        }

        [Test]
        public void EmptyTileLabelUsesStableFallbackName()
        {
            Assert.AreEqual(
                "Unknown card\n[image fallback]",
                CardImageFallbackStatusFormatter.FormatTileLabel(" ", true));
        }

        [Test]
        public void DetailStatusIsEmptyForLoadedImages()
        {
            Assert.AreEqual(
                string.Empty,
                CardImageFallbackStatusFormatter.FormatDetailStatus(false, "cards/card.jpg"));
        }

        [Test]
        public void MissingPathUsesSpecificDetailStatus()
        {
            Assert.AreEqual(
                CardImageFallbackStatusFormatter.MissingPathDetailStatus,
                CardImageFallbackStatusFormatter.FormatDetailStatus(true, null));
            Assert.AreEqual(
                CardImageFallbackStatusFormatter.MissingPathDetailStatus,
                CardImageFallbackStatusFormatter.FormatDetailStatus(true, " "));
        }

        [Test]
        public void MissingFileUsesSpecificDetailStatus()
        {
            Assert.AreEqual(
                CardImageFallbackStatusFormatter.MissingFileDetailStatus,
                CardImageFallbackStatusFormatter.FormatDetailStatus(true, "missing/not-found.jpg"));
        }

        [Test]
        public void DetailStatusWithTipExplainsFallbackWithoutChangingLoadedImages()
        {
            Assert.AreEqual(
                string.Empty,
                CardImageFallbackStatusFormatter.FormatDetailStatusWithTip(false, "cards/card.jpg"));

            string formatted = CardImageFallbackStatusFormatter.FormatDetailStatusWithTip(
                true,
                "missing/not-found.jpg");

            StringAssert.Contains(CardImageFallbackStatusFormatter.MissingFileDetailStatus, formatted);
            StringAssert.Contains("Tip:", formatted);
            StringAssert.Contains("fallback", formatted);
        }
    }
}
