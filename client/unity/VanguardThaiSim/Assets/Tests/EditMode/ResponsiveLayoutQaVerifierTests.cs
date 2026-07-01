using NUnit.Framework;
using UnityEngine;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class ResponsiveLayoutQaVerifierTests
    {
        [Test]
        public void AndroidReferenceViewportsPassLayoutQa()
        {
            ResponsiveLayoutQaReport report = ResponsiveLayoutQaVerifier.ValidateAndroidReferenceViewports();

            Assert.IsTrue(report.IsPass, Describe(report));
        }

        [Test]
        public void AndroidProfilesUseMinimumFortyEightTouchTarget()
        {
            ResponsiveLayoutQaViewport[] viewports = ResponsiveLayoutQaVerifier.AndroidReferenceViewports();
            for (int i = 0; i < viewports.Length; i++)
            {
                ResponsiveLayoutProfile profile = ResponsiveLayoutProfile.ForScreen(viewports[i].Width, viewports[i].Height);
                Assert.GreaterOrEqual(profile.TouchTargetHeight, 48f, viewports[i].Name);
            }
        }

        [Test]
        public void CollapsedTouchTargetProducesIssue()
        {
            ResponsiveLayoutProfile profile = new ResponsiveLayoutProfile(
                ResponsiveDeviceClass.PhonePortrait,
                new Vector2(720f, 1280f),
                0f,
                112f,
                124f,
                36f,
                0.50f,
                2,
                new Vector2(118f, 188f),
                new Vector2(96f, 134f),
                190f,
                210f,
                132f,
                220f,
                176f,
                148f,
                168f);

            ResponsiveLayoutQaReport report = ResponsiveLayoutQaVerifier.ValidateProfile("bad portrait", profile);

            Assert.IsFalse(report.IsPass);
            AssertIssueCode(report, "touch-target");
        }

        [Test]
        public void OverflowingToolbarProducesIssue()
        {
            ResponsiveLayoutProfile profile = new ResponsiveLayoutProfile(
                ResponsiveDeviceClass.PhonePortrait,
                new Vector2(360f, 640f),
                0f,
                112f,
                124f,
                48f,
                1.00f,
                2,
                new Vector2(118f, 188f),
                new Vector2(96f, 134f),
                190f,
                210f,
                132f,
                220f,
                176f,
                148f,
                168f);

            ResponsiveLayoutQaReport report = ResponsiveLayoutQaVerifier.ValidateProfile("bad toolbar", profile);

            Assert.IsFalse(report.IsPass);
            AssertIssueCode(report, "card-browser-toolbar-width");
        }

        [Test]
        public void OverflowingHomeHeaderProducesIssue()
        {
            ResponsiveLayoutProfile profile = new ResponsiveLayoutProfile(
                ResponsiveDeviceClass.PhonePortrait,
                new Vector2(360f, 640f),
                0f,
                112f,
                124f,
                48f,
                0.10f,
                2,
                new Vector2(118f, 188f),
                new Vector2(96f, 134f),
                190f,
                210f,
                132f,
                220f,
                176f,
                148f,
                168f);

            ResponsiveLayoutQaReport report = ResponsiveLayoutQaVerifier.ValidateProfile("bad home header", profile);

            Assert.IsFalse(report.IsPass);
            AssertIssueCode(report, "home-header-width");
        }

        [Test]
        public void ExistingProfileTestsStillClassifyTabletAsAndroidQaTarget()
        {
            ResponsiveLayoutProfile profile = ResponsiveLayoutProfile.ForScreen(800f, 1280f);

            Assert.AreEqual(ResponsiveDeviceClass.Tablet, profile.DeviceClass);
            Assert.GreaterOrEqual(profile.TouchTargetHeight, 48f);
            Assert.IsTrue(ResponsiveLayoutQaVerifier.ValidateProfile("tablet", profile).IsPass);
        }

        [Test]
        public void WindowsPlayTableBoardFirstProfilesPassLayoutQa()
        {
            ResponsiveLayoutQaReport report = ResponsiveLayoutQaVerifier.ValidateWindowsPlayTableBoardFirst();

            Assert.IsTrue(report.IsPass, Describe(report));
        }

        [Test]
        public void OversizedDesktopPlayTableToolbarProducesIssue()
        {
            ResponsiveLayoutProfile profile = new ResponsiveLayoutProfile(
                ResponsiveDeviceClass.Desktop,
                new Vector2(1280f, 720f),
                0.5f,
                74f,
                140f,
                42f,
                0.92f,
                4,
                new Vector2(132f, 204f),
                new Vector2(112f, 154f),
                340f,
                300f,
                250f,
                300f,
                238f,
                180f,
                230f);

            ResponsiveLayoutQaReport report = ResponsiveLayoutQaVerifier.ValidateProfile("bad desktop play table", profile);

            Assert.IsFalse(report.IsPass);
            AssertIssueCode(report, "play-board-toolbar-ratio");
        }

        [Test]
        public void OversizedDesktopInspectHudWidthProducesIssue()
        {
            ResponsiveLayoutProfile profile = new ResponsiveLayoutProfile(
                ResponsiveDeviceClass.Desktop,
                new Vector2(1280f, 720f),
                0.5f,
                74f,
                64f,
                42f,
                0.92f,
                4,
                new Vector2(132f, 204f),
                new Vector2(112f, 154f),
                340f,
                300f,
                250f,
                300f,
                330f,
                68f,
                140f);

            ResponsiveLayoutQaReport report = ResponsiveLayoutQaVerifier.ValidateProfile("bad desktop inspect", profile);

            Assert.IsFalse(report.IsPass);
            AssertIssueCode(report, "play-side-panel-board-budget");
        }

        private static void AssertIssueCode(ResponsiveLayoutQaReport report, string expectedCode)
        {
            for (int i = 0; i < report.Issues.Count; i++)
            {
                if (report.Issues[i].Code == expectedCode)
                {
                    return;
                }
            }

            Assert.Fail("Expected issue code not found: " + expectedCode + "\n" + Describe(report));
        }

        private static string Describe(ResponsiveLayoutQaReport report)
        {
            if (report.IsPass)
            {
                return "No layout QA issues.";
            }

            string text = string.Empty;
            for (int i = 0; i < report.Issues.Count; i++)
            {
                ResponsiveLayoutQaIssue issue = report.Issues[i];
                text += issue.ViewportName + ": " + issue.Code + " - " + issue.Message + "\n";
            }

            return text;
        }
    }
}
