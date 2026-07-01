using System.Collections.Generic;

namespace VanguardThaiSim.UI
{
    public sealed class ManualTutorialReadinessReport
    {
        public bool accepted;
        public readonly List<string> issues = new List<string>();
    }

    public static class ManualTutorialReadinessVerifier
    {
        private static readonly string[] RequiredSectionIds =
        {
            "home",
            "card_browser",
            "deck_builder",
            "play_table",
            "online_room",
            "replay",
            "custom_packs",
            "playing_field",
            "turn_flow",
            "deck_and_setup",
            "combat_basics",
            "triggers",
            "resources",
            "formats",
            "markers_and_tokens"
        };

        public static ManualTutorialReadinessReport Verify()
        {
            ManualTutorialReadinessReport report = new ManualTutorialReadinessReport();
            IReadOnlyList<ManualSection> sections = ManualContentCatalog.Sections();
            foreach (string requiredSectionId in RequiredSectionIds)
            {
                if (!HasSection(sections, requiredSectionId))
                {
                    report.issues.Add("Missing manual readiness section: " + requiredSectionId);
                }
            }

            if (LoadingTipCatalog.AllTips().Length < 3)
            {
                report.issues.Add("Manual readiness requires loading tips for data, images, and deck load.");
            }

            ManualContentOriginalityReport originality = ManualContentOriginalityGuard.Validate();
            if (originality == null || !originality.accepted)
            {
                report.issues.Add("Manual originality guard must pass before closeout.");
            }

            report.accepted = report.issues.Count == 0;
            return report;
        }

        private static bool HasSection(IReadOnlyList<ManualSection> sections, string sectionId)
        {
            if (sections == null)
            {
                return false;
            }

            foreach (ManualSection section in sections)
            {
                if (section != null && section.section_id == sectionId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
