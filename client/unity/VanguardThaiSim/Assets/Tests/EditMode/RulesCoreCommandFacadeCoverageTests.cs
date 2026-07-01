using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class RulesCoreCommandFacadeCoverageTests
    {
        [Test]
        public void CurrentGameActionsAreFacadeCovered()
        {
            RulesCoreCommandFacadeCoverageReport report =
                RulesCoreCommandFacadeCoverage.CreateCurrentReport();

            AssertCovered(report, PhaseTimingMatrixCommandIds.Draw, GameActionType.Draw, "GameActionService.Draw");
            AssertCovered(report, PhaseTimingMatrixCommandIds.MoveCard, GameActionType.MoveCard, "GameActionService.MoveCard");
            AssertCovered(report, PhaseTimingMatrixCommandIds.SetPhase, GameActionType.SetPhase, "GameActionService.SetPhase");
            AssertCovered(report, PhaseTimingMatrixCommandIds.AddGiftMarker, GameActionType.AddGiftMarker, "GameActionService.AddGiftMarker");
            AssertCovered(report, PhaseTimingMatrixCommandIds.ResourceFlip, GameActionType.ResourceFlip, "GameActionService.ResourceFlip");
        }

        [Test]
        public void UndoLastIsExplicitFacadeException()
        {
            RulesCoreCommandFacadeCoverageReport report =
                RulesCoreCommandFacadeCoverage.CreateCurrentReport();

            RulesCoreCommandFacadeCoverageEntry entry =
                RulesCoreCommandFacadeCoverage.Find(report, RulesCoreCommandFacadeCoverage.UndoLastCommandId);

            Assert.NotNull(entry);
            Assert.IsFalse(entry.facade_covered);
            Assert.IsFalse(entry.legal_action_generator_covered);
            Assert.IsFalse(entry.legal_action_executor_covered);
            Assert.IsFalse(entry.rules_core_matcher_covered);
            Assert.AreEqual("GameActionService.UndoLast", entry.mutation_service_method);
            Assert.IsTrue(entry.exception_reason.Contains("event-sourcing"));
            Assert.AreEqual(1, RulesCoreCommandFacadeCoverage.CountExceptions(report));
        }

        [Test]
        public void FacadeCoveredLookupReturnsFalseForUnknownOrException()
        {
            RulesCoreCommandFacadeCoverageReport report =
                RulesCoreCommandFacadeCoverage.CreateCurrentReport();

            Assert.IsTrue(RulesCoreCommandFacadeCoverage.IsFacadeCovered(report, PhaseTimingMatrixCommandIds.Draw));
            Assert.IsFalse(RulesCoreCommandFacadeCoverage.IsFacadeCovered(report, RulesCoreCommandFacadeCoverage.UndoLastCommandId));
            Assert.IsFalse(RulesCoreCommandFacadeCoverage.IsFacadeCovered(report, "missing"));
            Assert.IsFalse(RulesCoreCommandFacadeCoverage.IsFacadeCovered(null, PhaseTimingMatrixCommandIds.Draw));
        }

        [Test]
        public void CoverageReportRoundTripsJson()
        {
            RulesCoreCommandFacadeCoverageReport report =
                RulesCoreCommandFacadeCoverage.CreateCurrentReport();

            RulesCoreCommandFacadeCoverageReport roundTrip =
                RulesCoreCommandFacadeCoverageReport.FromJson(report.ToJson(false));

            Assert.AreEqual(report.entries.Count, roundTrip.entries.Count);
            Assert.AreEqual(report.entries[0].command_id, roundTrip.entries[0].command_id);
            Assert.AreEqual(report.entries[0].mutation_service_method, roundTrip.entries[0].mutation_service_method);
            Assert.AreEqual(report.entries[0].facade_covered, roundTrip.entries[0].facade_covered);
        }

        [Test]
        public void CoverageReportHasNoDuplicateCommandIds()
        {
            RulesCoreCommandFacadeCoverageReport report =
                RulesCoreCommandFacadeCoverage.CreateCurrentReport();
            var seen = new HashSet<string>();

            foreach (RulesCoreCommandFacadeCoverageEntry entry in report.entries)
            {
                Assert.IsTrue(seen.Add(entry.command_id), "Duplicate facade coverage entry: " + entry.command_id);
            }
        }

        private static void AssertCovered(
            RulesCoreCommandFacadeCoverageReport report,
            string commandId,
            GameActionType actionType,
            string serviceMethod)
        {
            RulesCoreCommandFacadeCoverageEntry entry =
                RulesCoreCommandFacadeCoverage.Find(report, commandId);

            Assert.NotNull(entry);
            Assert.AreEqual(actionType.ToString(), entry.action_type);
            Assert.IsTrue(entry.facade_covered);
            Assert.IsTrue(entry.legal_action_generator_covered);
            Assert.IsTrue(entry.legal_action_executor_covered);
            Assert.IsTrue(entry.rules_core_matcher_covered);
            Assert.AreEqual(serviceMethod, entry.mutation_service_method);
            Assert.AreEqual(string.Empty, entry.exception_reason);
        }
    }
}
