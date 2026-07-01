using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class CardWorkshopToolbarFormatterTests
    {
        [Test]
        public void CompactStatusShowsCountsAndNoFilters()
        {
            Assert.AreEqual(
                "Cards 10836 | Shown 24 | Filters none",
                CardWorkshopToolbarFormatter.FormatCompactStatus(10836, 24, false));
        }

        [Test]
        public void CompactStatusShowsActiveFilters()
        {
            Assert.AreEqual(
                "Cards 10836 | Shown 0 | Filters on",
                CardWorkshopToolbarFormatter.FormatCompactStatus(10836, 0, true));
        }

        [Test]
        public void CompactStatusClampsNegativeCounts()
        {
            Assert.AreEqual(
                "Cards 0 | Shown 0 | Filters none",
                CardWorkshopToolbarFormatter.FormatCompactStatus(-1, -2, false));
        }
    }
}
