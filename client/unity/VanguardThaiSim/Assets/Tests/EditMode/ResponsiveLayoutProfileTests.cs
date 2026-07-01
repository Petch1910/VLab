using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class ResponsiveLayoutProfileTests
    {
        [Test]
        public void PhonePortraitFitsCardBrowserToolbarAndUsesTwoColumns()
        {
            ResponsiveLayoutProfile profile = ResponsiveLayoutProfile.ForScreen(390f, 844f);

            Assert.AreEqual(ResponsiveDeviceClass.PhonePortrait, profile.DeviceClass);
            Assert.AreEqual(2, profile.CardGridColumns);
            Assert.GreaterOrEqual(profile.TouchTargetHeight, 48f);
            Assert.LessOrEqual(EstimateToolbarWidth(profile, CardBrowserToolbarWidths()), profile.ReferenceResolution.x);
        }

        [Test]
        public void PhoneLandscapeKeepsPlayToolbarWithinReferenceWidth()
        {
            ResponsiveLayoutProfile profile = ResponsiveLayoutProfile.ForScreen(844f, 390f);

            Assert.AreEqual(ResponsiveDeviceClass.PhoneLandscape, profile.DeviceClass);
            Assert.AreEqual(3, profile.CardGridColumns);
            Assert.LessOrEqual(profile.PlaySidePanelWidth, 250f);
            Assert.LessOrEqual(EstimateToolbarWidth(profile, PlayTableToolbarWidths()), profile.ReferenceResolution.x);
        }

        [Test]
        public void DesktopKeepsFourColumnBrowserAndLargerPanels()
        {
            ResponsiveLayoutProfile profile = ResponsiveLayoutProfile.ForScreen(1440f, 900f);

            Assert.AreEqual(ResponsiveDeviceClass.Desktop, profile.DeviceClass);
            Assert.AreEqual(4, profile.CardGridColumns);
            Assert.AreEqual(340f, profile.DetailPanelWidth);
            Assert.AreEqual(168f, profile.PlaySidePanelWidth);
            Assert.LessOrEqual(profile.PlaySidePanelWidth, PlayTableZoneFirstLayoutFormatter.MaximumDesktopSidePanelWidth);
            Assert.Less(profile.PlayToolbarHeight, profile.ToolbarHeight);
        }

        [Test]
        public void InvalidScreenSizeFallsBackToDesktopProfile()
        {
            ResponsiveLayoutProfile profile = ResponsiveLayoutProfile.ForScreen(0f, 0f);

            Assert.AreEqual(ResponsiveDeviceClass.Desktop, profile.DeviceClass);
            Assert.AreEqual(4, profile.CardGridColumns);
        }

        private static float EstimateToolbarWidth(ResponsiveLayoutProfile profile, float[] baseWidths)
        {
            float spacing = profile.IsCompact ? 4f : 8f;
            float padding = profile.IsCompact ? 16f : 24f;
            float total = padding + spacing * (baseWidths.Length - 1);
            for (int i = 0; i < baseWidths.Length; i++)
            {
                total += profile.ScaleControlWidth(baseWidths[i]);
            }

            return total;
        }

        private static float[] CardBrowserToolbarWidths()
        {
            return new[] { 220f, 250f, 190f, 75f, 65f, 76f, 36f, 36f, 70f, 220f };
        }

        private static float[] PlayTableToolbarWidths()
        {
            return new[] { 72f, 58f, 68f, 68f, 82f, 66f, 72f, 58f, 68f, 68f, 78f, 68f, 70f, 280f };
        }
    }
}
