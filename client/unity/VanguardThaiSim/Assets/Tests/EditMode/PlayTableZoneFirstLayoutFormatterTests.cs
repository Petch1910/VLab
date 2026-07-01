using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PlayTableZoneFirstLayoutFormatterTests
    {
        [Test]
        public void LabelsKeepCommonActionsNearZones()
        {
            Assert.AreEqual("Board Table", PlayTableZoneFirstLayoutFormatter.BoardPanelName);
            Assert.AreEqual("Turn actions", PlayTableZoneFirstLayoutFormatter.PrimaryActionsLabel);
            Assert.AreEqual("Move selected card", PlayTableZoneFirstLayoutFormatter.MoveActionsLabel);
        }

        [Test]
        public void SummaryDocumentsZoneFirstBoundary()
        {
            string summary = PlayTableZoneFirstLayoutFormatter.FormatSummary();
            Assert.IsTrue(summary.Contains("Vanguard Area-style"));
            Assert.IsTrue(summary.Contains("playmat field first"));
            Assert.IsTrue(summary.Contains("front/back circle slots"));
            Assert.IsTrue(summary.Contains("board zones first"));
            Assert.IsTrue(summary.Contains("trigger/order zones visible"));
            Assert.IsTrue(summary.Contains("compact command dock"));
            Assert.IsTrue(summary.Contains("overlay inspect HUD"));
            Assert.IsTrue(summary.Contains("de-dashboard"));
            Assert.IsTrue(summary.Contains("debug controls outside"));
        }

        [Test]
        public void PlaymatSkeletonUsesVanguardCircleStructure()
        {
            Assert.AreEqual(3, PlayTableZoneFirstLayoutFormatter.VanguardCircleColumnCount);
            Assert.AreEqual(2, PlayTableZoneFirstLayoutFormatter.CircleRowsPerPlayer);
            Assert.AreEqual(5, PlayTableZoneFirstLayoutFormatter.RearGuardVisualSlotCountPerPlayer);
        }

        [Test]
        public void CompactCommandRowsFitAreaStyleBudget()
        {
            Assert.LessOrEqual(
                PlayTableZoneFirstLayoutFormatter.EstimateCompactCommandRowWidth(false),
                PlayTableZoneFirstLayoutFormatter.MaximumAreaCommandDockRowWidth);
            Assert.LessOrEqual(
                PlayTableZoneFirstLayoutFormatter.EstimateCompactCommandRowWidth(true),
                PlayTableZoneFirstLayoutFormatter.MaximumAreaCommandDockRowWidth);
        }

        [Test]
        public void BoardFirstSummaryReportsToolbarRatio()
        {
            ResponsiveLayoutProfile profile = ResponsiveLayoutProfile.ForScreen(1280f, 720f);
            float ratio = PlayTableZoneFirstLayoutFormatter.EstimateBoardToToolbarHeightRatio(profile);
            string summary = PlayTableZoneFirstLayoutFormatter.FormatBoardFirstSummary(ratio);

            Assert.GreaterOrEqual(ratio, PlayTableZoneFirstLayoutFormatter.MinimumDesktopBoardToToolbarHeightRatio);
            Assert.IsTrue(summary.Contains("Board-first PlayTable"));
            Assert.IsTrue(summary.Contains("toolbar"));
        }

        [Test]
        public void DefaultInspectHudFitsDeDashboardBudget()
        {
            Assert.LessOrEqual(
                PlayTableZoneFirstLayoutFormatter.EstimateDefaultInspectHudHeight(),
                PlayTableZoneFirstLayoutFormatter.MaximumDefaultInspectHudHeight);
        }

        [Test]
        public void RightFieldZonesReserveInspectHudGutter()
        {
            Assert.LessOrEqual(PlayTableZoneFirstLayoutFormatter.MaximumDefaultRightFieldZoneEdge, 0.86f);
        }
    }
}
