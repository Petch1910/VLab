using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class EventSourcingCoverageReportTests
    {
        [Test]
        public void CurrentGameActionServiceMutationsAreGameEventSourced()
        {
            EventSourcingCoverageReport report = EventSourcingCoverageCatalog.CreateCurrentReport();

            AssertGameEventSourced(report, EventSourcingCoverageCatalog.DrawPathId);
            AssertGameEventSourced(report, EventSourcingCoverageCatalog.MoveCardPathId);
            AssertGameEventSourced(report, EventSourcingCoverageCatalog.SetPhasePathId);
            AssertGameEventSourced(report, EventSourcingCoverageCatalog.AddGiftMarkerPathId);
            AssertGameEventSourced(report, EventSourcingCoverageCatalog.ResourceFlipPathId);
        }

        [Test]
        public void EveryCurrentGameActionTypeHasCoveredEventSourcingEntry()
        {
            EventSourcingCoverageReport report = EventSourcingCoverageCatalog.CreateCurrentReport();
            var expected = new Dictionary<GameActionType, string>
            {
                { GameActionType.Draw, EventSourcingCoverageCatalog.DrawPathId },
                { GameActionType.MoveCard, EventSourcingCoverageCatalog.MoveCardPathId },
                { GameActionType.SetPhase, EventSourcingCoverageCatalog.SetPhasePathId },
                { GameActionType.AddGiftMarker, EventSourcingCoverageCatalog.AddGiftMarkerPathId },
                { GameActionType.ResourceFlip, EventSourcingCoverageCatalog.ResourceFlipPathId },
                { GameActionType.DeclareAttack, EventSourcingCoverageCatalog.DeclareAttackPathId },
                { GameActionType.Guard, EventSourcingCoverageCatalog.GuardPathId },
                { GameActionType.TriggerCheck, EventSourcingCoverageCatalog.TriggerCheckPathId },
                { GameActionType.MulliganCards, EventSourcingCoverageCatalog.MulliganCardsPathId }
            };

            foreach (GameActionType actionType in System.Enum.GetValues(typeof(GameActionType)))
            {
                Assert.IsTrue(expected.ContainsKey(actionType), "Missing event-sourcing path for " + actionType);
                Assert.IsTrue(
                    EventSourcingCoverageCatalog.IsGameEventSourced(report, expected[actionType]),
                    "GameActionType is not event-sourced: " + actionType);
            }
        }

        [Test]
        public void AbilityCoreStructuredEffectsRemainEventSourcedThroughRulesCore()
        {
            EventSourcingCoverageReport report = EventSourcingCoverageCatalog.CreateCurrentReport();
            EventSourcingCoverageEntry entry =
                EventSourcingCoverageCatalog.Find(
                    report,
                    EventSourcingCoverageCatalog.AbilityCoreStructuredEffectsPathId);

            Assert.NotNull(entry);
            Assert.IsTrue(entry.uses_rules_core);
            Assert.IsTrue(entry.creates_game_event);
            Assert.IsTrue(entry.appends_to_game_state_event_log);
            Assert.IsTrue(entry.replayable_with_game_event_reducer);
            Assert.IsFalse(entry.explicit_exception);
        }

        [Test]
        public void CurrentEventSourcingExceptionsStayExplicit()
        {
            EventSourcingCoverageReport report = EventSourcingCoverageCatalog.CreateCurrentReport();

            EventSourcingCoverageEntry undo =
                EventSourcingCoverageCatalog.Find(report, EventSourcingCoverageCatalog.UndoLastPathId);
            EventSourcingCoverageEntry pendingAuto =
                EventSourcingCoverageCatalog.Find(
                    report,
                    EventSourcingCoverageCatalog.PendingAutoTimingCommitPathId);

            Assert.NotNull(undo);
            Assert.IsTrue(undo.explicit_exception);
            Assert.IsFalse(undo.creates_game_event);
            Assert.IsTrue(undo.exception_reason.Contains("Undo"));
            Assert.IsTrue(undo.next_action.Contains("isolated"));

            Assert.NotNull(pendingAuto);
            Assert.IsTrue(pendingAuto.explicit_exception);
            Assert.IsFalse(pendingAuto.appends_to_game_state_event_log);
            Assert.IsTrue(pendingAuto.mutation_target.Contains("pending_auto_abilities"));
            Assert.IsTrue(pendingAuto.next_action.Contains("GameEvent"));

            Assert.AreEqual(2, EventSourcingCoverageCatalog.CountExceptions(report));
        }

        [Test]
        public void EventSourcingCoverageReportRoundTripsJson()
        {
            EventSourcingCoverageReport report = EventSourcingCoverageCatalog.CreateCurrentReport();

            EventSourcingCoverageReport roundTrip =
                EventSourcingCoverageReport.FromJson(report.ToJson(false));

            Assert.AreEqual(report.entries.Count, roundTrip.entries.Count);
            Assert.AreEqual(report.entries[0].path_id, roundTrip.entries[0].path_id);
            Assert.AreEqual(report.entries[0].mutation_target, roundTrip.entries[0].mutation_target);
            Assert.AreEqual(report.entries[0].creates_game_event, roundTrip.entries[0].creates_game_event);
        }

        [Test]
        public void EventSourcingCoverageReportHasNoDuplicatePathIds()
        {
            EventSourcingCoverageReport report = EventSourcingCoverageCatalog.CreateCurrentReport();
            var seen = new HashSet<string>();

            foreach (EventSourcingCoverageEntry entry in report.entries)
            {
                Assert.IsTrue(seen.Add(entry.path_id), "Duplicate event-sourcing entry: " + entry.path_id);
            }
        }

        private static void AssertGameEventSourced(EventSourcingCoverageReport report, string pathId)
        {
            EventSourcingCoverageEntry entry = EventSourcingCoverageCatalog.Find(report, pathId);

            Assert.NotNull(entry);
            Assert.IsTrue(entry.mutates_game_state);
            Assert.IsTrue(entry.creates_game_event);
            Assert.IsTrue(entry.appends_to_game_state_event_log);
            Assert.IsTrue(entry.replayable_with_game_event_reducer);
            Assert.IsFalse(entry.explicit_exception);
        }
    }
}
