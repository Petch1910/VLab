using System;
using System.Collections.Generic;
using System.Text;

namespace VanguardThaiSim.UI
{
    public static class ManualContentFilter
    {
        public const string AllCategory = "All";
        public const string EmptyResultMessage = "No manual sections match the current filters.";

        public static IReadOnlyList<string> Categories()
        {
            List<string> categories = new List<string> { AllCategory };
            foreach (ManualSection section in ManualContentCatalog.Sections())
            {
                if (section == null || string.IsNullOrWhiteSpace(section.category))
                {
                    continue;
                }

                if (!Contains(categories, section.category))
                {
                    categories.Add(section.category);
                }
            }

            return categories;
        }

        public static IReadOnlyList<ManualSection> Filter(string query, string category)
        {
            return Filter(ManualContentCatalog.Sections(), query, category);
        }

        public static IReadOnlyList<ManualSection> Filter(
            IReadOnlyList<ManualSection> sections,
            string query,
            string category)
        {
            List<ManualSection> filtered = new List<ManualSection>();
            string normalizedQuery = Normalize(query);
            string normalizedCategory = Normalize(category);
            bool categoryIsAll = string.IsNullOrEmpty(normalizedCategory) ||
                                 string.Equals(normalizedCategory, Normalize(AllCategory), StringComparison.Ordinal);

            if (sections == null)
            {
                return filtered;
            }

            foreach (ManualSection section in sections)
            {
                if (section == null)
                {
                    continue;
                }

                if (!categoryIsAll &&
                    !string.Equals(Normalize(section.category), normalizedCategory, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(normalizedQuery) && !MatchesQuery(section, normalizedQuery))
                {
                    continue;
                }

                filtered.Add(section);
            }

            return filtered;
        }

        public static string FormatSections(IReadOnlyList<ManualSection> sections)
        {
            if (sections == null || sections.Count == 0)
            {
                return EmptyResultMessage;
            }

            StringBuilder builder = new StringBuilder();
            string lastCategory = null;
            foreach (ManualSection section in sections)
            {
                if (section == null)
                {
                    continue;
                }

                if (!string.Equals(lastCategory, section.category, StringComparison.Ordinal))
                {
                    if (builder.Length > 0)
                    {
                        builder.AppendLine();
                    }

                    builder.AppendLine(section.category);
                    lastCategory = section.category;
                }

                builder.AppendLine();
                builder.AppendLine(section.title);
                builder.AppendLine(section.body);
            }

            return builder.Length == 0 ? EmptyResultMessage : builder.ToString().TrimEnd();
        }

        public static string FormatCategoryButtonLabel(string category)
        {
            return "Category: " + (string.IsNullOrWhiteSpace(category) ? AllCategory : category.Trim());
        }

        private static bool MatchesQuery(ManualSection section, string normalizedQuery)
        {
            return ContainsNormalized(section.title, normalizedQuery) ||
                   ContainsNormalized(section.body, normalizedQuery) ||
                   ContainsNormalized(section.category, normalizedQuery) ||
                   ContainsNormalized(section.related_screen, normalizedQuery);
        }

        private static bool ContainsNormalized(string value, string normalizedQuery)
        {
            return Normalize(value).Contains(normalizedQuery);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
        }

        private static bool Contains(IReadOnlyList<string> values, string candidate)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i], candidate, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
