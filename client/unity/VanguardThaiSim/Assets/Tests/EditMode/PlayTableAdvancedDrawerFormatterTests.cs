using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableAdvancedDrawerFormatterTests
    {
        [Test]
        public void DrawerTitleIsStableForToolbarButton()
        {
            Assert.AreEqual("Advanced", PlayTableAdvancedDrawerFormatter.DrawerTitle);
        }

        [Test]
        public void SummaryListsHiddenDebugSurfaces()
        {
            string summary = PlayTableAdvancedDrawerFormatter.FormatSummary();
            Assert.IsTrue(summary.Contains("trigger draft"));
            Assert.IsTrue(summary.Contains("pending AUTO"));
            Assert.IsTrue(summary.Contains("manual resolution"));
            Assert.IsTrue(summary.Contains("online debug controls"));
        }
    }
}
