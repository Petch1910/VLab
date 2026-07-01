namespace VanguardThaiSim.UI
{
    public static class PlayTableZoneFirstLayoutFormatter
    {
        public const string BoardPanelName = "Board Table";
        public const string PrimaryActionsLabel = "Turn actions";
        public const string MoveActionsLabel = "Move selected card";
        public const float MinimumDesktopBoardToToolbarHeightRatio = 10f;
        public const float MaximumDesktopSidePanelWidth = 180f;
        public const float MaximumAreaCommandDockRowWidth = 760f;
        public const float MaximumDefaultInspectHudHeight = 260f;
        public const float MaximumDefaultRightFieldZoneEdge = 0.86f;
        public const int VanguardCircleColumnCount = 3;
        public const int CircleRowsPerPlayer = 2;
        public const int RearGuardVisualSlotCountPerPlayer = 5;

        public static string FormatSummary()
        {
            return "Vanguard Area-style PlayTable: playmat field first, front/back circle slots, board zones first, trigger/order zones visible, compact command dock, overlay inspect HUD, de-dashboard default view, debug controls outside the main board flow.";
        }

        public static string FormatBoardFirstSummary(float boardToToolbarRatio)
        {
            return "Board-first PlayTable: board/table area ratio " +
                   boardToToolbarRatio.ToString("0.0") +
                   "x toolbar, with debug controls kept outside the main board flow.";
        }

        public static float EstimateDefaultInspectHudHeight()
        {
            return 18f + 30f + 16f + 78f + 16f + 46f + 16f + (4f * 5f);
        }

        public static float EstimateCompactCommandRowWidth(bool moveRow)
        {
            if (moveRow)
            {
                return 56f + 70f + 54f + 56f + 56f + 64f +
                       (4f * 5f) +
                       8f;
            }

            return 44f + 44f + 54f + 48f + 56f + 52f + 60f + 60f + 50f +
                   (4f * 8f) +
                   8f;
        }

        public static float EstimateBoardToToolbarHeightRatio(ResponsiveLayoutProfile profile)
        {
            if (profile.PlayToolbarHeight <= 0f)
            {
                return 0f;
            }

            float boardHeight = profile.ReferenceResolution.y - profile.PlayToolbarHeight;
            return boardHeight / profile.PlayToolbarHeight;
        }
    }
}
