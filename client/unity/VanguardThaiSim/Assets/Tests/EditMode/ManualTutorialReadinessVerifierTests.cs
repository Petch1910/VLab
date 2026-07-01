using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class ManualTutorialReadinessVerifierTests
    {
        [Test]
        public void CurrentManualPassesReadinessCloseout()
        {
            ManualTutorialReadinessReport report = ManualTutorialReadinessVerifier.Verify();

            Assert.IsTrue(report.accepted, string.Join("; ", report.issues));
            Assert.AreEqual(0, report.issues.Count);
        }
    }
}
