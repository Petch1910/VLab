using System;
using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class TimingWindowAuditTests
    {
        [Test]
        public void CurrentReportIncludesAllCurrentEnumValues()
        {
            TimingWindowAuditReport report = TimingWindowAuditCatalog.CreateCurrentReport();

            Assert.AreEqual(
                Enum.GetNames(typeof(GamePhase)).Length,
                TimingWindowAuditCatalog.CountCategory(report, TimingWindowAuditCategories.GamePhase));
            Assert.AreEqual(
                Enum.GetNames(typeof(GameActionType)).Length,
                TimingWindowAuditCatalog.CountCategory(report, TimingWindowAuditCategories.GameActionType));
            Assert.AreEqual(
                Enum.GetNames(typeof(AbilityTiming)).Length,
                TimingWindowAuditCatalog.CountCategory(report, TimingWindowAuditCategories.AbilityTiming));
            Assert.AreEqual(
                Enum.GetNames(typeof(TriggerCheckSource)).Length,
                TimingWindowAuditCatalog.CountCategory(report, TimingWindowAuditCategories.TriggerCheckSource));
            Assert.AreEqual(
                Enum.GetNames(typeof(CombatModifierExpiration)).Length,
                TimingWindowAuditCatalog.CountCategory(report, TimingWindowAuditCategories.CombatModifierExpiration));
        }

        [Test]
        public void PendingAutoTimingEntriesMatchCollectorMappings()
        {
            TimingWindowAuditReport report = TimingWindowAuditCatalog.CreateCurrentReport();

            AssertPendingTiming(report, GameActionType.Draw);
            AssertPendingTiming(report, GameActionType.MoveCard);
            AssertPendingTiming(report, GameActionType.SetPhase);
            AssertPendingTiming(report, GameActionType.AddGiftMarker);
            AssertPendingTiming(report, GameActionType.ResourceFlip);
        }

        [Test]
        public void ReportListsKnownM11Gaps()
        {
            TimingWindowAuditReport report = TimingWindowAuditCatalog.CreateCurrentReport();

            Assert.IsTrue(TimingWindowAuditCatalog.Contains(
                report,
                TimingWindowAuditCategories.Gap,
                "TypedTimingWindowEnumMissing"));
            Assert.IsTrue(TimingWindowAuditCatalog.Contains(
                report,
                TimingWindowAuditCategories.Gap,
                "PhaseTimingMatrixMissing"));
            Assert.IsTrue(TimingWindowAuditCatalog.Contains(
                report,
                TimingWindowAuditCategories.Gap,
                "BattleStepWindowsMissing"));
            Assert.IsTrue(TimingWindowAuditCatalog.Contains(
                report,
                TimingWindowAuditCategories.Gap,
                "CleanupNotPhaseIntegrated"));
            Assert.IsTrue(TimingWindowAuditCatalog.Contains(
                report,
                TimingWindowAuditCategories.Gap,
                "TriggerCheckSourceNotWindow"));

            foreach (TimingWindowAuditEntry entry in report.entries)
            {
                if (entry.category == TimingWindowAuditCategories.Gap)
                {
                    Assert.IsTrue(entry.gap);
                }
            }
        }

        [Test]
        public void ReportRoundTripsJson()
        {
            TimingWindowAuditReport report = TimingWindowAuditCatalog.CreateCurrentReport();

            TimingWindowAuditReport roundTrip = TimingWindowAuditReport.FromJson(report.ToJson(false));

            Assert.AreEqual(report.entries.Count, roundTrip.entries.Count);
            Assert.AreEqual(report.entries[0].category, roundTrip.entries[0].category);
            Assert.AreEqual(report.entries[0].identifier, roundTrip.entries[0].identifier);
        }

        [Test]
        public void ReportHasNoDuplicateCategoryIdentifierPairs()
        {
            TimingWindowAuditReport report = TimingWindowAuditCatalog.CreateCurrentReport();
            var seen = new HashSet<string>();

            foreach (TimingWindowAuditEntry entry in report.entries)
            {
                string key = entry.category + "|" + entry.identifier;
                Assert.IsTrue(seen.Add(key), "Duplicate timing audit entry: " + key);
            }
        }

        private static void AssertPendingTiming(TimingWindowAuditReport report, GameActionType actionType)
        {
            string timing = AbilityTriggerEventCollector.GetTimingEvent(new GameEvent { action_type = actionType });
            Assert.IsTrue(TimingWindowAuditCatalog.Contains(
                report,
                TimingWindowAuditCategories.PendingAutoTiming,
                timing));
        }
    }
}
