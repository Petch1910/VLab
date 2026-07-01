using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class M19VisualSmokeVerifierTests
    {
        [Test]
        public void M19VisualSmokePassesWindowsAndAndroidReferenceChecks()
        {
            M19VisualSmokeReport report = M19VisualSmokeVerifier.Run();

            Assert.IsTrue(report.IsPass, string.Join("\n", report.issues));
            Assert.IsTrue(report.steps.Count >= 6);
        }

        [Test]
        public void M19ReferenceViewportsIncludeWindowsAndAndroid()
        {
            ResponsiveLayoutQaViewport[] viewports = ResponsiveLayoutQaVerifier.M19ReferenceViewports();

            Assert.GreaterOrEqual(viewports.Length, 10);
            Assert.IsTrue(viewports[0].Name.Contains("Windows"));
            Assert.IsTrue(viewports[viewports.Length - 1].Name.Contains("Android"));
        }
    }
}
