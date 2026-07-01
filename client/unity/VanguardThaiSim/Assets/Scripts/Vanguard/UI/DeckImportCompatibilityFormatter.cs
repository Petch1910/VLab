using System.Text;
using VanguardThaiSim.Decks;

namespace VanguardThaiSim.UI
{
    public static class DeckImportCompatibilityFormatter
    {
        public static string Format(DeckImportCompatibilityReport report, int maxIssues = 5)
        {
            if (report == null)
            {
                return "Compatibility: not checked.";
            }

            if (report.issues.Count == 0)
            {
                return "Compatibility: active pack match.";
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(report.accepted ? "Compatibility: warnings" : "Compatibility: missing data");
            int limit = System.Math.Max(0, System.Math.Min(maxIssues, report.issues.Count));
            for (int i = 0; i < limit; i++)
            {
                DeckImportCompatibilityIssue issue = report.issues[i];
                builder.AppendLine();
                builder.Append(issue.severity == "error" ? "Error: " : "Warn: ");
                builder.Append(issue.code);
                if (!string.IsNullOrWhiteSpace(issue.card_id))
                {
                    builder.Append(" ");
                    builder.Append(issue.card_id);
                }

                if (issue.zone.HasValue)
                {
                    builder.Append(" (");
                    builder.Append(issue.zone.Value);
                    builder.Append(")");
                }
            }

            if (report.issues.Count > limit)
            {
                builder.AppendLine();
                builder.Append("+");
                builder.Append(report.issues.Count - limit);
                builder.Append(" more compatibility issues");
            }

            return builder.ToString();
        }
    }
}
