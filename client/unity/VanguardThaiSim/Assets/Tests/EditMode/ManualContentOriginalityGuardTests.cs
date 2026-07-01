using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class ManualContentOriginalityGuardTests
    {
        [Test]
        public void CurrentManualAndLoadingTipsPassOriginalContentGate()
        {
            ManualContentOriginalityReport report = ManualContentOriginalityGuard.Validate();

            Assert.IsTrue(report.accepted, string.Join("; ", report.issues));
            Assert.AreEqual(0, report.issues.Count);
        }

        [Test]
        public void LoadingTipCatalogExposesAllRuntimeTips()
        {
            string[] tips = LoadingTipCatalog.AllTips();

            Assert.AreEqual(3, tips.Length);
            StringAssert.Contains("pack", tips[0]);
            StringAssert.Contains("fallback", tips[1]);
            StringAssert.Contains("legality", tips[2]);
        }
    }
}
