using NUnit.Framework;
using VanguardThaiSim.Smoke;

namespace VanguardThaiSim.Tests
{
    public sealed class ClientSmokeFlowVerifierTests
    {
        [Test]
        public void ClientSmokeFlowPassesWindowsStabilityCoverage()
        {
            ClientSmokeFlowReport report = ClientSmokeFlowVerifier.Run();

            Assert.IsTrue(report.IsPass, report.ToJson(true));
            Assert.GreaterOrEqual(report.steps.Count, 8, report.ToJson(true));
            AssertHasStep(report, "Home smoke");
            AssertHasStep(report, "Deck Builder smoke");
            AssertHasStep(report, "Play Table smoke");
            AssertHasStep(report, "Manual smoke");
            AssertHasStep(report, "Settings smoke");
            AssertHasStep(report, "Online Room smoke");
            StringAssert.Contains("Windows layout smoke", report.steps[report.steps.Count - 1]);
        }

        [Test]
        public void PlayerSmokeFlowDetectsCommandLineFlag()
        {
            Assert.IsTrue(PlayerSmokeFlowBootstrap.IsSmokeRequested(new[]
            {
                "VanguardThaiSim.exe",
                "-vanguardPlayerSmoke"
            }));

            Assert.IsFalse(PlayerSmokeFlowBootstrap.IsSmokeRequested(new[]
            {
                "VanguardThaiSim.exe"
            }));
        }

        [Test]
        public void PlayerSmokeFlowResolvesExplicitOutputPath()
        {
            string output = PlayerSmokeFlowBootstrap.ResolveOutputPath(
                new[]
                {
                    "VanguardThaiSim.exe",
                    "-vanguardPlayerSmokeOutput",
                    "work/player-smoke.json"
                },
                "fallback.json");

            StringAssert.EndsWith("work\\player-smoke.json", output);
        }

        private static void AssertHasStep(ClientSmokeFlowReport report, string expected)
        {
            for (int i = 0; i < report.steps.Count; i++)
            {
                if (report.steps[i].Contains(expected))
                {
                    return;
                }
            }

            Assert.Fail("Missing smoke step: " + expected + "\n" + report.ToJson(true));
        }
    }
}
