using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class PhaseTimingMatrixTests
    {
        [Test]
        public void MatrixMapsCurrentGameActionsToTimingWindows()
        {
            Assert.AreEqual(TimingWindow.OnDraw, PhaseTimingMatrix.ToTimingWindow(GameActionType.Draw));
            Assert.AreEqual(TimingWindow.OnMoveCard, PhaseTimingMatrix.ToTimingWindow(GameActionType.MoveCard));
            Assert.AreEqual(TimingWindow.OnSetPhase, PhaseTimingMatrix.ToTimingWindow(GameActionType.SetPhase));
            Assert.AreEqual(TimingWindow.OnAddGiftMarker, PhaseTimingMatrix.ToTimingWindow(GameActionType.AddGiftMarker));
        }

        [Test]
        public void MatrixContainsCurrentGameActionEntries()
        {
            PhaseTimingMatrixDefinition matrix = PhaseTimingMatrix.CreateCurrentMatrix();

            Assert.NotNull(PhaseTimingMatrix.Find(
                matrix,
                PhaseTimingMatrixCommandIds.Draw,
                GamePhase.StandAndDraw,
                TimingWindow.OnDraw));
            Assert.NotNull(PhaseTimingMatrix.Find(
                matrix,
                PhaseTimingMatrixCommandIds.MoveCard,
                GamePhase.Main,
                TimingWindow.OnMoveCard));
            Assert.NotNull(PhaseTimingMatrix.Find(
                matrix,
                PhaseTimingMatrixCommandIds.MoveCard,
                GamePhase.Ride,
                TimingWindow.OnMoveCard));
            Assert.NotNull(PhaseTimingMatrix.Find(
                matrix,
                PhaseTimingMatrixCommandIds.MoveCard,
                GamePhase.Mulligan,
                TimingWindow.OnMoveCard));
            Assert.NotNull(PhaseTimingMatrix.Find(
                matrix,
                PhaseTimingMatrixCommandIds.SetPhase,
                GamePhase.Battle,
                TimingWindow.OnSetPhase));
            Assert.NotNull(PhaseTimingMatrix.Find(
                matrix,
                PhaseTimingMatrixCommandIds.AddGiftMarker,
                GamePhase.Main,
                TimingWindow.OnAddGiftMarker));
        }

        [Test]
        public void MatrixAllowsAndRejectsRepresentativeGameActions()
        {
            PhaseTimingMatrixDefinition matrix = PhaseTimingMatrix.CreateCurrentMatrix();

            Assert.IsTrue(PhaseTimingMatrix.IsLegal(
                matrix,
                PhaseTimingMatrixCommandIds.Draw,
                GamePhase.StandAndDraw,
                TimingWindow.OnDraw));
            Assert.IsFalse(PhaseTimingMatrix.IsLegal(
                matrix,
                PhaseTimingMatrixCommandIds.Draw,
                GamePhase.Battle,
                TimingWindow.OnDraw));
            Assert.IsTrue(PhaseTimingMatrix.IsLegal(
                matrix,
                PhaseTimingMatrixCommandIds.MoveCard,
                GamePhase.Main,
                TimingWindow.OnMoveCard));
            Assert.IsTrue(PhaseTimingMatrix.IsLegal(
                matrix,
                PhaseTimingMatrixCommandIds.MoveCard,
                GamePhase.Ride,
                TimingWindow.OnMoveCard));
            Assert.IsTrue(PhaseTimingMatrix.IsLegal(
                matrix,
                PhaseTimingMatrixCommandIds.MoveCard,
                GamePhase.Mulligan,
                TimingWindow.OnMoveCard));
            Assert.IsFalse(PhaseTimingMatrix.IsLegal(
                matrix,
                PhaseTimingMatrixCommandIds.MoveCard,
                GamePhase.Battle,
                TimingWindow.OnMoveCard));
        }

        [Test]
        public void MatrixAllowsAndRejectsTriggerAndCleanupWindows()
        {
            PhaseTimingMatrixDefinition matrix = PhaseTimingMatrix.CreateCurrentMatrix();

            Assert.IsTrue(PhaseTimingMatrix.IsLegal(
                matrix,
                PhaseTimingMatrixCommandIds.TriggerCheckDrive,
                GamePhase.Battle,
                TimingWindow.DriveCheck));
            Assert.IsFalse(PhaseTimingMatrix.IsLegal(
                matrix,
                PhaseTimingMatrixCommandIds.TriggerCheckDrive,
                GamePhase.Main,
                TimingWindow.DriveCheck));
            Assert.IsTrue(PhaseTimingMatrix.IsLegal(
                matrix,
                PhaseTimingMatrixCommandIds.CleanupEndOfBattle,
                GamePhase.Battle,
                TimingWindow.CleanupEndOfBattle));
            Assert.IsFalse(PhaseTimingMatrix.IsLegal(
                matrix,
                PhaseTimingMatrixCommandIds.CleanupEndOfTurn,
                GamePhase.Battle,
                TimingWindow.CleanupEndOfTurn));
            Assert.IsTrue(PhaseTimingMatrix.IsLegal(
                matrix,
                PhaseTimingMatrixCommandIds.CleanupEndOfTurn,
                GamePhase.End,
                TimingWindow.CleanupEndOfTurn));
        }

        [Test]
        public void MatrixRoundTripsJson()
        {
            PhaseTimingMatrixDefinition matrix = PhaseTimingMatrix.CreateCurrentMatrix();

            PhaseTimingMatrixDefinition roundTrip = PhaseTimingMatrixDefinition.FromJson(matrix.ToJson(false));

            Assert.AreEqual(matrix.entries.Count, roundTrip.entries.Count);
            Assert.AreEqual(matrix.entries[0].command_id, roundTrip.entries[0].command_id);
            Assert.AreEqual(matrix.entries[0].phase, roundTrip.entries[0].phase);
            Assert.AreEqual(matrix.entries[0].timing_window, roundTrip.entries[0].timing_window);
        }

        [Test]
        public void MatrixHasNoDuplicateCommandPhaseWindowEntries()
        {
            PhaseTimingMatrixDefinition matrix = PhaseTimingMatrix.CreateCurrentMatrix();
            var seen = new HashSet<string>();

            foreach (PhaseTimingMatrixEntry entry in matrix.entries)
            {
                string key = entry.command_id + "|" + entry.phase + "|" + entry.timing_window;
                Assert.IsTrue(seen.Add(key), "Duplicate matrix entry: " + key);
            }
        }
    }
}
