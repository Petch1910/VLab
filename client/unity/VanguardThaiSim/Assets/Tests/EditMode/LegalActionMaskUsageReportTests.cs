using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class LegalActionMaskUsageReportTests
    {
        [Test]
        public void UiBotSessionAndAbilityPathsAreReported()
        {
            LegalActionMaskUsageReport report = LegalActionMaskUsageCatalog.CreateCurrentReport();

            Assert.NotNull(LegalActionMaskUsageCatalog.Find(report, LegalActionMaskUsagePathIds.PlayTableDraw));
            Assert.NotNull(LegalActionMaskUsageCatalog.Find(report, LegalActionMaskUsagePathIds.PlayTableMoveSelected));
            Assert.NotNull(LegalActionMaskUsageCatalog.Find(report, LegalActionMaskUsagePathIds.PlayTableSetPhase));
            Assert.NotNull(LegalActionMaskUsageCatalog.Find(report, LegalActionMaskUsagePathIds.PlayTableAddGiftMarker));
            Assert.NotNull(LegalActionMaskUsageCatalog.Find(report, LegalActionMaskUsagePathIds.EasyBotDecision));
            Assert.NotNull(LegalActionMaskUsageCatalog.Find(report, LegalActionMaskUsagePathIds.ProfileBotDecision));
            Assert.NotNull(LegalActionMaskUsageCatalog.Find(report, LegalActionMaskUsagePathIds.MultiplayerSessionLocalAction));
            Assert.NotNull(LegalActionMaskUsageCatalog.Find(report, LegalActionMaskUsagePathIds.AbilityCoreCommandExecution));
        }

        [Test]
        public void HardenedPathsValidateThroughRulesCoreOrUseLegalActionGenerator()
        {
            LegalActionMaskUsageReport report = LegalActionMaskUsageCatalog.CreateCurrentReport();

            AssertHardened(report, LegalActionMaskUsagePathIds.PlayTableDraw, true, true);
            AssertHardened(report, LegalActionMaskUsagePathIds.PlayTableMoveSelected, false, true);
            AssertHardened(report, LegalActionMaskUsagePathIds.PlayTableSetPhase, true, true);
            AssertHardened(report, LegalActionMaskUsagePathIds.PlayTableAddGiftMarker, true, true);
            AssertHardened(report, LegalActionMaskUsagePathIds.EasyBotDecision, true, true);
            AssertHardened(report, LegalActionMaskUsagePathIds.ProfileBotDecision, true, true);
            AssertHardened(report, LegalActionMaskUsagePathIds.MultiplayerSessionLocalAction, false, true);
            AssertHardened(report, LegalActionMaskUsagePathIds.AbilityCoreCommandExecution, false, true);
        }

        [Test]
        public void UndoIsOnlyDirectMutationException()
        {
            LegalActionMaskUsageReport report = LegalActionMaskUsageCatalog.CreateCurrentReport();
            LegalActionMaskUsageEntry undo =
                LegalActionMaskUsageCatalog.Find(report, LegalActionMaskUsagePathIds.PlayTableUndo);

            Assert.NotNull(undo);
            Assert.IsFalse(undo.hardened);
            Assert.IsTrue(undo.mutates_directly);
            Assert.IsFalse(undo.validates_with_rules_core);
            Assert.IsTrue(undo.exception_reason.Contains("Undo"));
            Assert.AreEqual(1, LegalActionMaskUsageCatalog.CountDirectMutationExceptions(report));
            Assert.IsFalse(LegalActionMaskUsageCatalog.IsHardened(report, LegalActionMaskUsagePathIds.PlayTableUndo));
        }

        [Test]
        public void UsageReportRoundTripsJson()
        {
            LegalActionMaskUsageReport report = LegalActionMaskUsageCatalog.CreateCurrentReport();

            LegalActionMaskUsageReport roundTrip = LegalActionMaskUsageReport.FromJson(report.ToJson(false));

            Assert.AreEqual(report.entries.Count, roundTrip.entries.Count);
            Assert.AreEqual(report.entries[0].path_id, roundTrip.entries[0].path_id);
            Assert.AreEqual(report.entries[0].layer, roundTrip.entries[0].layer);
            Assert.AreEqual(report.entries[0].hardened, roundTrip.entries[0].hardened);
        }

        [Test]
        public void UsageReportHasNoDuplicatePathIds()
        {
            LegalActionMaskUsageReport report = LegalActionMaskUsageCatalog.CreateCurrentReport();
            var seen = new HashSet<string>();

            foreach (LegalActionMaskUsageEntry entry in report.entries)
            {
                Assert.IsTrue(seen.Add(entry.path_id), "Duplicate legal mask usage entry: " + entry.path_id);
            }
        }

        private static void AssertHardened(
            LegalActionMaskUsageReport report,
            string pathId,
            bool usesLegalActionGenerator,
            bool validatesWithRulesCore)
        {
            LegalActionMaskUsageEntry entry = LegalActionMaskUsageCatalog.Find(report, pathId);

            Assert.NotNull(entry);
            Assert.IsTrue(entry.hardened);
            Assert.IsFalse(entry.mutates_directly);
            Assert.AreEqual(usesLegalActionGenerator, entry.uses_legal_action_generator);
            Assert.AreEqual(validatesWithRulesCore, entry.validates_with_rules_core);
            Assert.IsTrue(LegalActionMaskUsageCatalog.IsHardened(report, pathId));
        }
    }
}
