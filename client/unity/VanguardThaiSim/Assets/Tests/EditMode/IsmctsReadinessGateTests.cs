using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;

namespace VanguardThaiSim.Tests
{
    public sealed class IsmctsReadinessGateTests
    {
        [Test]
        public void DefaultGateAllowsAdvancedSearchPrototype()
        {
            IsmctsReadinessReport report = IsmctsReadinessGate.EvaluateDefault();

            Assert.IsTrue(report.advanced_search_allowed);
            Assert.Greater(report.ready_count, 0);
            Assert.AreEqual(0, report.blocked_count);
            Assert.AreEqual(report.ready_count, report.items.Count);
        }

        [Test]
        public void BlockedRequirementPreventsAdvancedSearch()
        {
            IsmctsReadinessReport report = IsmctsReadinessGate.Evaluate(new List<IsmctsReadinessItem>
            {
                new IsmctsReadinessItem
                {
                    requirement_id = "snapshot",
                    title = "Snapshot simulation",
                    ready = true,
                    evidence = "ready"
                },
                new IsmctsReadinessItem
                {
                    requirement_id = "hidden-state",
                    title = "Hidden state",
                    ready = false,
                    evidence = "blocked"
                }
            });

            Assert.IsFalse(report.advanced_search_allowed);
            Assert.AreEqual(1, report.ready_count);
            Assert.AreEqual(1, report.blocked_count);
        }

        [Test]
        public void ReportJsonRoundTrips()
        {
            IsmctsReadinessReport report = IsmctsReadinessGate.EvaluateDefault();

            IsmctsReadinessReport roundTrip = IsmctsReadinessReport.FromJson(report.ToJson(false));

            Assert.AreEqual(report.gate_id, roundTrip.gate_id);
            Assert.AreEqual(report.advanced_search_allowed, roundTrip.advanced_search_allowed);
            Assert.AreEqual(report.items.Count, roundTrip.items.Count);
            Assert.AreEqual(report.items[0].requirement_id, roundTrip.items[0].requirement_id);
        }

        [Test]
        public void GateIsDeterministic()
        {
            IsmctsReadinessReport first = IsmctsReadinessGate.EvaluateDefault();
            IsmctsReadinessReport second = IsmctsReadinessGate.EvaluateDefault();

            Assert.AreEqual(first.ToJson(false), second.ToJson(false));
        }
    }
}
