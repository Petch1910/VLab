using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.UI
{
    public readonly struct ResponsiveLayoutQaViewport
    {
        public ResponsiveLayoutQaViewport(string name, float width, float height)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Unnamed" : name.Trim();
            Width = width;
            Height = height;
        }

        public string Name { get; }

        public float Width { get; }

        public float Height { get; }
    }

    public readonly struct ResponsiveLayoutQaIssue
    {
        public ResponsiveLayoutQaIssue(string viewportName, string code, string message)
        {
            ViewportName = string.IsNullOrWhiteSpace(viewportName) ? "Unnamed" : viewportName.Trim();
            Code = string.IsNullOrWhiteSpace(code) ? "unknown" : code.Trim();
            Message = string.IsNullOrWhiteSpace(message) ? "Layout QA issue." : message.Trim();
        }

        public string ViewportName { get; }

        public string Code { get; }

        public string Message { get; }
    }

    public sealed class ResponsiveLayoutQaReport
    {
        public ResponsiveLayoutQaReport(IReadOnlyList<ResponsiveLayoutQaIssue> issues)
        {
            Issues = issues ?? new List<ResponsiveLayoutQaIssue>();
        }

        public IReadOnlyList<ResponsiveLayoutQaIssue> Issues { get; }

        public bool IsPass
        {
            get { return Issues.Count == 0; }
        }
    }

    public static class ResponsiveLayoutQaVerifier
    {
        private const float MinAndroidTouchTargetHeight = 48f;
        private const float CompactMargin = 16f;
        private const float RegularMargin = 24f;

        private static readonly float[] CardBrowserToolbarBaseWidths =
        {
            190f,
            210f,
            160f,
            110f,
            75f,
            65f,
            76f,
            36f,
            36f,
            70f,
            180f
        };

        private static readonly float[] PlayTableToolbarBaseWidths =
        {
            58f,
            78f,
            96f,
            66f,
            64f,
            360f
        };

        public static ResponsiveLayoutQaViewport[] WindowsReferenceViewports()
        {
            return new[]
            {
                new ResponsiveLayoutQaViewport("Windows desktop 1280x720", 1280f, 720f),
                new ResponsiveLayoutQaViewport("Windows desktop 1600x900", 1600f, 900f),
                new ResponsiveLayoutQaViewport("Windows desktop 1920x1080", 1920f, 1080f)
            };
        }

        public static ResponsiveLayoutQaViewport[] AndroidReferenceViewports()
        {
            return new[]
            {
                new ResponsiveLayoutQaViewport("Android compact portrait 360x640", 360f, 640f),
                new ResponsiveLayoutQaViewport("Android compact portrait 390x844", 390f, 844f),
                new ResponsiveLayoutQaViewport("Android compact portrait 412x915", 412f, 915f),
                new ResponsiveLayoutQaViewport("Android compact landscape 844x390", 844f, 390f),
                new ResponsiveLayoutQaViewport("Android compact landscape 915x412", 915f, 412f),
                new ResponsiveLayoutQaViewport("Android tablet portrait 800x1280", 800f, 1280f),
                new ResponsiveLayoutQaViewport("Android tablet landscape 1100x820", 1100f, 820f)
            };
        }

        public static ResponsiveLayoutQaViewport[] M19ReferenceViewports()
        {
            List<ResponsiveLayoutQaViewport> viewports = new List<ResponsiveLayoutQaViewport>();
            viewports.AddRange(WindowsReferenceViewports());
            viewports.AddRange(AndroidReferenceViewports());
            return viewports.ToArray();
        }

        public static ResponsiveLayoutQaReport ValidateM19ReferenceViewports()
        {
            List<ResponsiveLayoutQaIssue> issues = new List<ResponsiveLayoutQaIssue>();
            ResponsiveLayoutQaViewport[] viewports = M19ReferenceViewports();
            for (int i = 0; i < viewports.Length; i++)
            {
                AppendIssues(issues, ValidateViewport(viewports[i]));
            }

            return new ResponsiveLayoutQaReport(issues);
        }

        public static ResponsiveLayoutQaReport ValidateAndroidReferenceViewports()
        {
            List<ResponsiveLayoutQaIssue> issues = new List<ResponsiveLayoutQaIssue>();
            ResponsiveLayoutQaViewport[] viewports = AndroidReferenceViewports();
            for (int i = 0; i < viewports.Length; i++)
            {
                AppendIssues(issues, ValidateViewport(viewports[i]));
            }

            return new ResponsiveLayoutQaReport(issues);
        }

        public static ResponsiveLayoutQaReport ValidateWindowsPlayTableBoardFirst()
        {
            List<ResponsiveLayoutQaIssue> issues = new List<ResponsiveLayoutQaIssue>();
            ResponsiveLayoutQaViewport[] viewports = WindowsReferenceViewports();
            for (int i = 0; i < viewports.Length; i++)
            {
                AppendIssues(issues, ValidateViewport(viewports[i]));
            }

            return new ResponsiveLayoutQaReport(issues);
        }

        public static ResponsiveLayoutQaReport ValidateViewport(ResponsiveLayoutQaViewport viewport)
        {
            ResponsiveLayoutProfile profile = ResponsiveLayoutProfile.ForScreen(viewport.Width, viewport.Height);
            return ValidateProfile(viewport.Name, profile);
        }

        public static ResponsiveLayoutQaReport ValidateProfile(string viewportName, ResponsiveLayoutProfile profile)
        {
            List<ResponsiveLayoutQaIssue> issues = new List<ResponsiveLayoutQaIssue>();
            string name = string.IsNullOrWhiteSpace(viewportName) ? profile.DeviceClass.ToString() : viewportName.Trim();

            if (profile.DeviceClass != ResponsiveDeviceClass.Desktop && profile.TouchTargetHeight < MinAndroidTouchTargetHeight)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "touch-target",
                    "Android phone/tablet profile touch target height must be at least 48."));
            }

            if (profile.ToolbarHeight < profile.TouchTargetHeight)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "card-browser-toolbar-height",
                    "Card Browser toolbar height must not be smaller than the touch target height."));
            }

            if (profile.PlayToolbarHeight < profile.TouchTargetHeight)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "play-table-toolbar-height",
                    "Play Table toolbar height must not be smaller than the touch target height."));
            }

            int minColumns = profile.DeviceClass == ResponsiveDeviceClass.PhonePortrait ? 2 : 3;
            if (profile.CardGridColumns < minColumns)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "card-grid-columns",
                    "Card grid column count is below the minimum for this profile."));
            }

            if (profile.CardTileImageSize.x + CompactMargin > profile.CardGridCellSize.x ||
                profile.CardTileImageSize.y + 36f > profile.CardGridCellSize.y)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "card-tile-cell",
                    "Card tile image plus label space does not fit inside the grid cell."));
            }

            if (profile.DetailImageWidth + (CompactMargin * 2f) > profile.DetailPanelWidth)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "detail-image-width",
                    "Detail image width must fit inside the detail panel with compact margins."));
            }

            float panelBudget = profile.DetailPanelWidth + profile.DeckPanelWidth + (RegularMargin * 2f);
            if (panelBudget > profile.ReferenceResolution.x)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "deck-detail-panel-budget",
                    "Deck and detail panels exceed the profile reference width budget."));
            }

            if (profile.PlaySidePanelWidth + (RegularMargin * 2f) > profile.ReferenceResolution.x)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "play-side-panel-budget",
                    "Play side panel exceeds the profile reference width budget."));
            }

            if (profile.DeviceClass == ResponsiveDeviceClass.Desktop &&
                profile.PlaySidePanelWidth > PlayTableZoneFirstLayoutFormatter.MaximumDesktopSidePanelWidth)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "play-side-panel-board-budget",
                    "Desktop Play Table side panel is too wide for the board-first Windows pass."));
            }

            if (profile.DeviceClass == ResponsiveDeviceClass.Desktop &&
                PlayTableZoneFirstLayoutFormatter.EstimateDefaultInspectHudHeight() >
                PlayTableZoneFirstLayoutFormatter.MaximumDefaultInspectHudHeight)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "play-inspect-hud-height",
                    "Desktop Play Table default inspect HUD is too tall for the de-dashboard pass."));
            }

            if (profile.DeviceClass == ResponsiveDeviceClass.Desktop &&
                PlayTableZoneFirstLayoutFormatter.EstimateBoardToToolbarHeightRatio(profile) <
                PlayTableZoneFirstLayoutFormatter.MinimumDesktopBoardToToolbarHeightRatio)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "play-board-toolbar-ratio",
                    "Desktop Play Table board area must be visually more prominent than the toolbar."));
            }

            float playHeightBudget = profile.PlayToolbarHeight +
                                     22f +
                                     Mathf.Max(300f, profile.PlayFieldRowHeight) +
                                     Mathf.Max(64f, profile.PlayResourceRowHeight) +
                                     Mathf.Max(128f, profile.PlayHandHeight) +
                                     (RegularMargin * 2f);
            if (playHeightBudget > profile.ReferenceResolution.y)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "play-height-budget",
                    "Play table toolbar, rows, and hand exceed the profile reference height budget."));
            }

            if (profile.DeviceClass == ResponsiveDeviceClass.Desktop &&
                PlayTableZoneFirstLayoutFormatter.EstimateCompactCommandRowWidth(false) >
                PlayTableZoneFirstLayoutFormatter.MaximumAreaCommandDockRowWidth)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "play-command-dock-primary-width",
                    "Primary command dock row exceeds the Area-style row width budget."));
            }

            if (profile.DeviceClass == ResponsiveDeviceClass.Desktop &&
                PlayTableZoneFirstLayoutFormatter.EstimateCompactCommandRowWidth(true) >
                PlayTableZoneFirstLayoutFormatter.MaximumAreaCommandDockRowWidth)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "play-command-dock-move-width",
                    "Move command dock row exceeds the Area-style row width budget."));
            }

            float browserToolbarWidth = EstimateToolbarWidth(profile, CardBrowserToolbarBaseWidths);
            if (browserToolbarWidth > profile.ReferenceResolution.x)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "card-browser-toolbar-width",
                    "Card Browser toolbar controls exceed the profile reference width."));
            }

            float playToolbarWidth = EstimateToolbarWidth(profile, PlayTableToolbarBaseWidths);
            if (playToolbarWidth > profile.ReferenceResolution.x)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "play-table-toolbar-width",
                    "Play Table toolbar controls exceed the profile reference width."));
            }

            float homeHeaderWidth = EstimateHomeHeaderWidth(profile);
            if (homeHeaderWidth > profile.ReferenceResolution.x)
            {
                issues.Add(new ResponsiveLayoutQaIssue(
                    name,
                    "home-header-width",
                    "Home header title, mode pills, and local database label exceed the profile reference width."));
            }

            return new ResponsiveLayoutQaReport(issues);
        }

        private static float EstimateToolbarWidth(ResponsiveLayoutProfile profile, float[] baseWidths)
        {
            float spacing = profile.IsCompact ? 4f : 8f;
            float padding = profile.IsCompact ? CompactMargin : RegularMargin;
            float total = padding + spacing * Mathf.Max(0, baseWidths.Length - 1);
            for (int i = 0; i < baseWidths.Length; i++)
            {
                total += profile.ScaleControlWidth(baseWidths[i]);
            }

            return total;
        }

        private static float EstimateHomeHeaderWidth(ResponsiveLayoutProfile profile)
        {
            if (profile.IsCompact)
            {
                return 36f + (10f * 5f) + 176f + 70f + 56f + 116f + 90f;
            }

            return 44f + (12f * 5f) + 330f + 88f + 128f + 170f + 210f;
        }

        private static void AppendIssues(List<ResponsiveLayoutQaIssue> target, ResponsiveLayoutQaReport report)
        {
            for (int i = 0; i < report.Issues.Count; i++)
            {
                target.Add(report.Issues[i]);
            }
        }
    }
}
