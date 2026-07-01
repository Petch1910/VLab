using System;
using VanguardThaiSim.Cards;

namespace VanguardThaiSim.Decks
{
    public static class DeckImportCompatibilityAnalyzer
    {
        public static DeckImportCompatibilityReport Analyze(
            VanguardDeck deck,
            DeckValidationResult validation,
            CardPackManifest manifest,
            string importedPackDefinitionHash)
        {
            DeckImportCompatibilityReport report = new DeckImportCompatibilityReport();
            if (deck == null)
            {
                Add(report, "DECK_MISSING", "error", "Imported deck is missing.", null, null);
                report.accepted = false;
                return report;
            }

            if (manifest != null)
            {
                if (!string.IsNullOrWhiteSpace(deck.card_pack_id) &&
                    !string.Equals(deck.card_pack_id, manifest.pack_id ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    Add(report, "PACK_ID_MISMATCH", "warning", "Deck pack id differs from the active pack.", null, null);
                }

                if (!string.IsNullOrWhiteSpace(deck.card_pack_version) &&
                    !string.Equals(deck.card_pack_version, manifest.source_version ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    Add(report, "PACK_VERSION_MISMATCH", "warning", "Deck pack version differs from the active pack.", null, null);
                }

                if (!string.IsNullOrWhiteSpace(importedPackDefinitionHash) &&
                    !string.Equals(importedPackDefinitionHash, manifest.definition_hash ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    Add(report, "PACK_HASH_MISMATCH", "warning", "Deck pack definition hash differs from the active pack.", null, null);
                }
            }

            if (validation != null)
            {
                foreach (DeckValidationIssue issue in validation.Issues)
                {
                    if (issue.Code == "UNKNOWN_CARD")
                    {
                        Add(
                            report,
                            "MISSING_CARD",
                            "error",
                            "Imported deck references a card missing from the active pack.",
                            issue.CardId,
                            issue.Zone);
                    }
                }
            }

            report.accepted = !HasError(report);
            return report;
        }

        private static void Add(
            DeckImportCompatibilityReport report,
            string code,
            string severity,
            string message,
            string cardId,
            DeckZone? zone)
        {
            report.issues.Add(new DeckImportCompatibilityIssue
            {
                code = code,
                severity = severity,
                message = message,
                card_id = cardId ?? string.Empty,
                zone = zone
            });
        }

        private static bool HasError(DeckImportCompatibilityReport report)
        {
            foreach (DeckImportCompatibilityIssue issue in report.issues)
            {
                if (issue != null &&
                    string.Equals(issue.severity, "error", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
