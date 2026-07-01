using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class ManualContentFilterTests
    {
        [Test]
        public void CategoriesStartWithAllAndIncludeRuntimeCategories()
        {
            IReadOnlyList<string> categories = ManualContentFilter.Categories();

            Assert.AreEqual(ManualContentFilter.AllCategory, categories[0]);
            Assert.Contains("App Guide", (System.Collections.ICollection)categories);
            Assert.Contains("Vanguard Rules Basics", (System.Collections.ICollection)categories);
        }

        [Test]
        public void SearchMatchesManualText()
        {
            IReadOnlyList<ManualSection> results = ManualContentFilter.Filter("trigger", ManualContentFilter.AllCategory);

            Assert.Greater(results.Count, 0);
            string formatted = ManualContentFilter.FormatSections(results);
            StringAssert.Contains("Triggers", formatted);
            Assert.IsFalse(formatted.Contains(ManualContentFilter.EmptyResultMessage));
        }

        [Test]
        public void CategoryFilterNarrowsSections()
        {
            IReadOnlyList<ManualSection> results = ManualContentFilter.Filter(string.Empty, "Vanguard Rules Basics");
            string formatted = ManualContentFilter.FormatSections(results);

            StringAssert.Contains("Vanguard Rules Basics", formatted);
            Assert.IsFalse(formatted.Contains("App Guide"));
        }

        [Test]
        public void EmptyFilterShowsPlayerFacingFallback()
        {
            IReadOnlyList<ManualSection> results = ManualContentFilter.Filter("no-such-manual-topic", ManualContentFilter.AllCategory);

            Assert.AreEqual(0, results.Count);
            Assert.AreEqual(ManualContentFilter.EmptyResultMessage, ManualContentFilter.FormatSections(results));
        }
    }
}
