using System.Collections.Generic;

namespace VanguardThaiSim.UI
{
    public sealed class M19VisualSmokeReport
    {
        public readonly List<string> steps = new List<string>();
        public readonly List<string> issues = new List<string>();

        public bool IsPass
        {
            get { return issues.Count == 0; }
        }
    }

    public static class M19VisualSmokeVerifier
    {
        public static M19VisualSmokeReport Run()
        {
            M19VisualSmokeReport report = new M19VisualSmokeReport();

            ResponsiveLayoutQaReport layoutReport = ResponsiveLayoutQaVerifier.ValidateM19ReferenceViewports();
            if (!layoutReport.IsPass)
            {
                for (int i = 0; i < layoutReport.Issues.Count; i++)
                {
                    ResponsiveLayoutQaIssue issue = layoutReport.Issues[i];
                    report.issues.Add(issue.ViewportName + ": " + issue.Code + " - " + issue.Message);
                }
            }
            else
            {
                report.steps.Add("Windows and Android reference viewports pass responsive layout QA.");
            }

            AddRequiredText(report, "Home taxonomy", HomeLobbyStatusFormatter.TaxonomyBaseline, "local database");
            AddRequiredText(report, "Deck Builder layout", CardBrowserModeFormatter.FormatLayoutSummary(CardBrowserScreenMode.DeckBuilder), "left preview");
            AddRequiredText(report, "Card Browser layout", CardBrowserModeFormatter.FormatLayoutSummary(CardBrowserScreenMode.Browser), "read-only");
            AddRequiredText(report, "PlayTable layout", PlayTableZoneFirstLayoutFormatter.FormatSummary(), "compact command dock");
            AddRequiredText(report, "Advanced drawer", PlayTableAdvancedDrawerFormatter.FormatSummary(), "out of the primary PlayTable toolbar");

            if (UiStateMessageFormatter.CardPoolPreparing.Contains("Loading") ||
                UiStateMessageFormatter.FilterPreparing.Contains("Loading") ||
                UiStateMessageFormatter.ZoneEmpty == "Empty")
            {
                report.issues.Add("M19 state text still contains raw loading/empty labels.");
            }
            else
            {
                report.steps.Add("Loading, empty, and placeholder labels use player-facing M19 text.");
            }

            return report;
        }

        private static void AddRequiredText(M19VisualSmokeReport report, string label, string value, string expected)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains(expected))
            {
                report.issues.Add(label + " missing expected text: " + expected);
                return;
            }

            report.steps.Add(label + " check passed.");
        }
    }
}
