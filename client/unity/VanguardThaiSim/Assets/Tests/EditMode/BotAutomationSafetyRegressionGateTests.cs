using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;

namespace VanguardThaiSim.Tests
{
    public sealed class BotAutomationSafetyRegressionGateTests
    {
        [Test]
        public void DefaultGateAcceptsRequiredSafetyRegressions()
        {
            BotAutomationSafetyRegressionReport report =
                BotAutomationSafetyRegressionGate.RunDefault();

            Assert.IsTrue(report.accepted, report.summary);
            Assert.AreEqual("M26-07", report.milestone);
            Assert.AreEqual(3, report.check_count);
            Assert.AreEqual(3, report.passed_count);
            Assert.AreEqual(0, report.failed_count);
            Assert.NotNull(FindCheck(report, BotAutomationSafetyRegressionGate.HiddenStateNoLeakCheck));
            Assert.NotNull(FindCheck(report, BotAutomationSafetyRegressionGate.SnapshotSimulationNoLiveMutationCheck));
            Assert.NotNull(FindCheck(report, BotAutomationSafetyRegressionGate.ReplayDeterminismCheck));
        }

        [Test]
        public void BuildRejectsWhenAnyRequiredCheckFails()
        {
            BotAutomationSafetyRegressionReport report =
                BotAutomationSafetyRegressionGate.Build(new List<BotAutomationSafetyRegressionCheck>
                {
                    new BotAutomationSafetyRegressionCheck
                    {
                        check_id = BotAutomationSafetyRegressionGate.HiddenStateNoLeakCheck,
                        passed = true
                    },
                    new BotAutomationSafetyRegressionCheck
                    {
                        check_id = BotAutomationSafetyRegressionGate.SnapshotSimulationNoLiveMutationCheck,
                        passed = false
                    },
                    new BotAutomationSafetyRegressionCheck
                    {
                        check_id = BotAutomationSafetyRegressionGate.ReplayDeterminismCheck,
                        passed = true
                    }
                });

            Assert.IsFalse(report.accepted);
            Assert.AreEqual(1, report.failed_count);
            Assert.IsTrue(report.summary.Contains("failed"));
        }

        [Test]
        public void BuildRejectsWhenARequiredCheckIsMissing()
        {
            BotAutomationSafetyRegressionReport report =
                BotAutomationSafetyRegressionGate.Build(new List<BotAutomationSafetyRegressionCheck>
                {
                    new BotAutomationSafetyRegressionCheck
                    {
                        check_id = BotAutomationSafetyRegressionGate.HiddenStateNoLeakCheck,
                        passed = true
                    }
                });

            Assert.IsFalse(report.accepted);
            Assert.AreEqual(1, report.check_count);
        }

        [Test]
        public void ReportRoundTripsJson()
        {
            BotAutomationSafetyRegressionReport report =
                BotAutomationSafetyRegressionGate.RunDefault();

            BotAutomationSafetyRegressionReport roundTrip =
                BotAutomationSafetyRegressionReport.FromJson(report.ToJson(false));

            Assert.AreEqual(report.schema_version, roundTrip.schema_version);
            Assert.AreEqual(report.milestone, roundTrip.milestone);
            Assert.AreEqual(report.accepted, roundTrip.accepted);
            Assert.AreEqual(report.check_count, roundTrip.check_count);
            Assert.NotNull(FindCheck(roundTrip, BotAutomationSafetyRegressionGate.ReplayDeterminismCheck));
        }

        private static BotAutomationSafetyRegressionCheck FindCheck(
            BotAutomationSafetyRegressionReport report,
            string checkId)
        {
            for (int i = 0; i < report.checks.Count; i++)
            {
                BotAutomationSafetyRegressionCheck check = report.checks[i];
                if (check != null && check.check_id == checkId)
                {
                    return check;
                }
            }

            return null;
        }
    }
}
