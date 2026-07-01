using System.Collections.Generic;

namespace VanguardThaiSim.Decks
{
    public sealed class DeckImportCompatibilityIssue
    {
        public string code;
        public string severity;
        public string message;
        public string card_id;
        public DeckZone? zone;
    }

    public sealed class DeckImportCompatibilityReport
    {
        public bool accepted;
        public readonly List<DeckImportCompatibilityIssue> issues = new List<DeckImportCompatibilityIssue>();
    }
}
