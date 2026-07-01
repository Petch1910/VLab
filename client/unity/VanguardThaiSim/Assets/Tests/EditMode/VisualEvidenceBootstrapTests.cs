using NUnit.Framework;
using VanguardThaiSim.Smoke;

namespace VanguardThaiSim.Tests
{
    public sealed class VisualEvidenceBootstrapTests
    {
        [Test]
        public void DetectsVisualEvidenceFlag()
        {
            Assert.IsTrue(VisualEvidenceBootstrap.IsVisualEvidenceRequested(new[]
            {
                "VanguardThaiSim.exe",
                "-vanguardVisualEvidence"
            }));
            Assert.IsFalse(VisualEvidenceBootstrap.IsVisualEvidenceRequested(new[]
            {
                "VanguardThaiSim.exe"
            }));
        }

        [Test]
        public void ResolvesExplicitOutputAndDirectory()
        {
            string output = VisualEvidenceBootstrap.ResolveOutputPath(
                new[] { "-vanguardVisualEvidenceOutput", "work\\visual\\report.json" },
                "fallback.json");
            string directory = VisualEvidenceBootstrap.ResolveScreenshotDirectory(
                new[] { "-vanguardVisualEvidenceDir", "work\\visual\\screens" },
                "fallback");

            StringAssert.EndsWith("work\\visual\\report.json", output);
            StringAssert.EndsWith("work\\visual\\screens", directory);
        }
    }
}
