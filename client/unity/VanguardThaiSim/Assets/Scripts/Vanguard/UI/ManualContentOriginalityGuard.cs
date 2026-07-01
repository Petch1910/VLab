using System;
using System.Collections.Generic;

namespace VanguardThaiSim.UI
{
    public sealed class ManualContentOriginalityReport
    {
        public bool accepted;
        public readonly List<string> issues = new List<string>();
    }

    public static class ManualContentOriginalityGuard
    {
        private static readonly string[] RequiredCategories =
        {
            "App Guide",
            "Vanguard Rules Basics"
        };

        private static readonly string[] BannedTerms =
        {
            "VangPro",
            "Dear Days",
            "Cardfight Connect",
            "Card Game Simulator",
            "CGS",
            "Vanguard Area-style",
            "http://",
            "https://",
            "payload id",
            "private card id",
            "raw network payload",
            "hidden state",
            "C:\\",
            "D:\\"
        };

        public static ManualContentOriginalityReport Validate()
        {
            ManualContentOriginalityReport report = new ManualContentOriginalityReport();
            IReadOnlyList<ManualSection> sections = ManualContentCatalog.Sections();
            if (sections == null || sections.Count == 0)
            {
                report.issues.Add("Manual has no sections.");
            }
            else
            {
                ValidateRequiredCategories(sections, report);
                foreach (ManualSection section in sections)
                {
                    ValidateSection(section, report);
                }
            }

            ValidateText("manual", ManualContentCatalog.FormatAllSections(), report);
            foreach (string tip in LoadingTipCatalog.AllTips())
            {
                ValidateText("loading_tip", tip, report);
            }

            report.accepted = report.issues.Count == 0;
            return report;
        }

        private static void ValidateRequiredCategories(
            IReadOnlyList<ManualSection> sections,
            ManualContentOriginalityReport report)
        {
            foreach (string requiredCategory in RequiredCategories)
            {
                bool found = false;
                foreach (ManualSection section in sections)
                {
                    if (section != null &&
                        string.Equals(section.category, requiredCategory, StringComparison.Ordinal))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    report.issues.Add("Missing required manual category: " + requiredCategory);
                }
            }
        }

        private static void ValidateSection(ManualSection section, ManualContentOriginalityReport report)
        {
            if (section == null)
            {
                report.issues.Add("Manual contains a null section.");
                return;
            }

            if (string.IsNullOrWhiteSpace(section.section_id))
            {
                report.issues.Add("Manual section is missing section_id.");
            }

            if (string.IsNullOrWhiteSpace(section.category))
            {
                report.issues.Add("Manual section " + section.section_id + " is missing category.");
            }

            if (string.IsNullOrWhiteSpace(section.title))
            {
                report.issues.Add("Manual section " + section.section_id + " is missing title.");
            }

            if (string.IsNullOrWhiteSpace(section.body))
            {
                report.issues.Add("Manual section " + section.section_id + " is missing body.");
            }

            ValidateText(section.section_id, section.title, report);
            ValidateText(section.section_id, section.body, report);
            ValidateText(section.section_id, section.loading_tip, report);
        }

        private static void ValidateText(string label, string text, ManualContentOriginalityReport report)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            foreach (string bannedTerm in BannedTerms)
            {
                if (text.IndexOf(bannedTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    report.issues.Add(label + " contains banned player-facing term: " + bannedTerm);
                }
            }
        }
    }
}
