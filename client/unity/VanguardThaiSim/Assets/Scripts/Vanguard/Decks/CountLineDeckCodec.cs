using System;
using System.Text;

namespace VanguardThaiSim.Decks
{
    public static class CountLineDeckCodec
    {
        public const string Header = "# Vanguard Thai Sim Deck List";

        public static string Export(VanguardDeck deck)
        {
            return Export(deck, null);
        }

        public static string Export(VanguardDeck deck, string packDefinitionHash)
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(Header);
            builder.AppendLine("Name: " + SafeValue(deck.name, "New Deck"));
            builder.AppendLine("Format: " + SafeValue(deck.format, "D"));
            builder.AppendLine("PackId: " + SafeValue(deck.card_pack_id, "local"));
            builder.AppendLine("PackVersion: " + SafeValue(deck.card_pack_version, "unknown"));
            if (!string.IsNullOrWhiteSpace(packDefinitionHash))
            {
                builder.AppendLine("PackDefinitionHash: " + packDefinitionHash.Trim());
            }

            builder.AppendLine();
            AppendZone(builder, "Main", deck, DeckZone.Main);
            AppendZone(builder, "Ride", deck, DeckZone.Ride);
            AppendZone(builder, "G", deck, DeckZone.G);
            return builder.ToString().TrimEnd();
        }

        public static VanguardDeck Import(string text)
        {
            CountLineDeckImportResult result = ImportDetailed(text);
            return result.deck;
        }

        public static CountLineDeckImportResult ImportDetailed(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new FormatException("Deck text is empty.");
            }

            string name = "Imported Deck";
            string format = "D";
            string packId = "local";
            string packVersion = "unknown";
            string packDefinitionHash = string.Empty;
            VanguardDeck parsed = VanguardDeck.Create(name, format, packId, packVersion);
            DeckZone? currentZone = null;

            string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                if (TryReadMetadata(line, "Name:", out string metadataName))
                {
                    name = RequiredMetadata(metadataName, "Name", i);
                    continue;
                }

                if (TryReadMetadata(line, "Format:", out string metadataFormat))
                {
                    format = RequiredMetadata(metadataFormat, "Format", i);
                    continue;
                }

                if (TryReadMetadata(line, "PackId:", out string metadataPackId))
                {
                    packId = RequiredMetadata(metadataPackId, "PackId", i);
                    continue;
                }

                if (TryReadMetadata(line, "PackVersion:", out string metadataPackVersion))
                {
                    packVersion = RequiredMetadata(metadataPackVersion, "PackVersion", i);
                    continue;
                }

                if (TryReadMetadata(line, "PackDefinitionHash:", out string metadataPackDefinitionHash))
                {
                    packDefinitionHash = RequiredMetadata(metadataPackDefinitionHash, "PackDefinitionHash", i);
                    continue;
                }

                if (TryReadZone(line, out DeckZone zone))
                {
                    currentZone = zone;
                    continue;
                }

                if (!currentZone.HasValue)
                {
                    throw new FormatException("Card line before zone header at line " + (i + 1) + ".");
                }

                ParseCardLine(parsed, currentZone.Value, line, i);
            }

            VanguardDeck deck = VanguardDeck.Create(name, format, packId, packVersion);
            CopyZone(parsed, deck, DeckZone.Main);
            CopyZone(parsed, deck, DeckZone.Ride);
            CopyZone(parsed, deck, DeckZone.G);
            return new CountLineDeckImportResult
            {
                deck = deck,
                pack_definition_hash = packDefinitionHash
            };
        }

        private static void AppendZone(StringBuilder builder, string label, VanguardDeck deck, DeckZone zone)
        {
            builder.AppendLine("[" + label + "]");
            foreach (DeckCardEntry entry in deck.GetEntries(zone))
            {
                if (entry == null || entry.quantity <= 0 || string.IsNullOrWhiteSpace(entry.card_id))
                {
                    continue;
                }

                builder.Append(entry.quantity);
                builder.Append(' ');
                builder.AppendLine(entry.card_id.Trim());
            }

            builder.AppendLine();
        }

        private static bool TryReadMetadata(string line, string prefix, out string value)
        {
            if (line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                value = line.Substring(prefix.Length).Trim();
                return true;
            }

            value = string.Empty;
            return false;
        }

        private static string RequiredMetadata(string value, string field, int lineIndex)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new FormatException(field + " metadata is empty at line " + (lineIndex + 1) + ".");
            }

            return value.Trim();
        }

        private static bool TryReadZone(string line, out DeckZone zone)
        {
            if (string.Equals(line, "[Main]", StringComparison.OrdinalIgnoreCase))
            {
                zone = DeckZone.Main;
                return true;
            }

            if (string.Equals(line, "[Ride]", StringComparison.OrdinalIgnoreCase))
            {
                zone = DeckZone.Ride;
                return true;
            }

            if (string.Equals(line, "[G]", StringComparison.OrdinalIgnoreCase))
            {
                zone = DeckZone.G;
                return true;
            }

            zone = DeckZone.Main;
            return false;
        }

        private static void ParseCardLine(VanguardDeck deck, DeckZone zone, string line, int lineIndex)
        {
            string[] parts = line.Split(new[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new FormatException("Card line must be '<quantity> <card_id>' at line " + (lineIndex + 1) + ".");
            }

            if (!int.TryParse(parts[0], out int quantity) || quantity <= 0)
            {
                throw new FormatException("Card quantity must be positive at line " + (lineIndex + 1) + ".");
            }

            string cardId = parts[1].Trim();
            if (string.IsNullOrWhiteSpace(cardId))
            {
                throw new FormatException("Card id is empty at line " + (lineIndex + 1) + ".");
            }

            deck.AddCard(zone, cardId, quantity);
        }

        private static void CopyZone(VanguardDeck source, VanguardDeck target, DeckZone zone)
        {
            foreach (DeckCardEntry entry in source.GetEntries(zone))
            {
                if (entry != null && entry.quantity > 0 && !string.IsNullOrWhiteSpace(entry.card_id))
                {
                    target.AddCard(zone, entry.card_id, entry.quantity);
                }
            }
        }

        private static string SafeValue(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
