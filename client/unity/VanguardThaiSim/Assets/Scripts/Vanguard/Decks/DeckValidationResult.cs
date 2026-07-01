using System.Collections.Generic;

namespace VanguardThaiSim.Decks
{
    public sealed class DeckValidationResult
    {
        private readonly List<DeckValidationIssue> issues = new List<DeckValidationIssue>();

        public int MainCount { get; internal set; }
        public int RideCount { get; internal set; }
        public int GCount { get; internal set; }
        public IReadOnlyList<DeckValidationIssue> Issues => issues;
        public bool HasErrors => ErrorCount > 0;
        public bool HasWarnings => WarningCount > 0;
        public bool IsLegal => !HasErrors;
        public bool IsComplete { get; internal set; }
        public bool IsPlayable => IsLegal && IsComplete;

        public int ErrorCount
        {
            get
            {
                int count = 0;
                foreach (DeckValidationIssue issue in issues)
                {
                    if (issue.Severity == DeckValidationSeverity.Error)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public int WarningCount
        {
            get
            {
                int count = 0;
                foreach (DeckValidationIssue issue in issues)
                {
                    if (issue.Severity == DeckValidationSeverity.Warning)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        internal void Add(DeckValidationIssue issue)
        {
            issues.Add(issue);
        }
    }
}
