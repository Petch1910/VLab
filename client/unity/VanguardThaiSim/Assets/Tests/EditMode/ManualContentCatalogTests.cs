using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class ManualContentCatalogTests
    {
        [Test]
        public void SectionsCoverAppGuideAndRulesBasics()
        {
            IReadOnlyList<ManualSection> sections = ManualContentCatalog.Sections();

            Assert.GreaterOrEqual(sections.Count, 15);
            Assert.IsTrue(ContainsSection(sections, "home"));
            Assert.IsTrue(ContainsSection(sections, "play_table"));
            Assert.IsTrue(ContainsSection(sections, "playing_field"));
            Assert.IsTrue(ContainsSection(sections, "triggers"));
            Assert.IsTrue(ContainsCategory(sections, "App Guide"));
            Assert.IsTrue(ContainsCategory(sections, "Vanguard Rules Basics"));
        }

        [Test]
        public void FormatAllSectionsIsPlayerFacing()
        {
            string manual = ManualContentCatalog.FormatAllSections();

            StringAssert.Contains("Home", manual);
            StringAssert.Contains("PlayTable", manual);
            StringAssert.Contains("trusted-client", manual);
            StringAssert.Contains("Triggers", manual);
            Assert.IsFalse(manual.Contains("payload id"));
            Assert.IsFalse(manual.Contains("private card id"));
        }

        [Test]
        public void LoadingTipsComeFromSections()
        {
            IReadOnlyList<string> tips = ManualContentCatalog.LoadingTips();

            Assert.GreaterOrEqual(tips.Count, 8);
            StringAssert.Contains("playable deck", tips[0]);
        }

        private static bool ContainsSection(IReadOnlyList<ManualSection> sections, string sectionId)
        {
            foreach (ManualSection section in sections)
            {
                if (section.section_id == sectionId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsCategory(IReadOnlyList<ManualSection> sections, string category)
        {
            foreach (ManualSection section in sections)
            {
                if (section.category == category)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
