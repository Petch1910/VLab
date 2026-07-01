using System;
using System.IO;
using System.Text;
using VanguardThaiSim.Decks;

namespace VanguardThaiSim.UI
{
    public sealed class DeckImageExportPlan
    {
        public bool accepted;
        public string file_path;
        public string file_name;
        public string rejection_reason;
    }

    public static class DeckImageExportPlanner
    {
        public static DeckImageExportPlan CreatePlan(VanguardDeck deck, string exportRoot, DateTime timestamp)
        {
            if (deck == null)
            {
                return Reject("Deck is not ready.");
            }

            if (string.IsNullOrWhiteSpace(exportRoot))
            {
                return Reject("Export root is missing.");
            }

            string safeName = SafeFileName(deck.name);
            string fileName = "deck_" + safeName + "_" + timestamp.ToString("yyyyMMdd_HHmmss") + ".png";
            string root = Path.GetFullPath(exportRoot);
            string path = Path.GetFullPath(Path.Combine(root, fileName));
            if (!IsChildPath(path, root))
            {
                return Reject("Export path escaped the export root.");
            }

            return new DeckImageExportPlan
            {
                accepted = true,
                file_name = fileName,
                file_path = path,
                rejection_reason = string.Empty
            };
        }

        public static string FormatPlanStatus(DeckImageExportPlan plan)
        {
            if (plan == null)
            {
                return DeckToolsDialogFormatter.FormatOperationResult("Export Image", false, "Export plan is missing.");
            }

            if (!plan.accepted)
            {
                return DeckToolsDialogFormatter.FormatOperationResult("Export Image", false, plan.rejection_reason);
            }

            return DeckToolsDialogFormatter.FormatOperationResult(
                "Export Image",
                true,
                "PNG capture requested: " + plan.file_path);
        }

        public static string SafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "untitled_deck";
            }

            StringBuilder builder = new StringBuilder();
            string trimmed = value.Trim();
            for (int i = 0; i < trimmed.Length && builder.Length < 48; i++)
            {
                char c = trimmed[i];
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(char.ToLowerInvariant(c));
                }
                else if (c == ' ' || c == '-' || c == '_')
                {
                    AppendSeparator(builder);
                }
            }

            string result = builder.ToString().Trim('_');
            return string.IsNullOrEmpty(result) ? "untitled_deck" : result;
        }

        private static void AppendSeparator(StringBuilder builder)
        {
            if (builder.Length == 0 || builder[builder.Length - 1] == '_')
            {
                return;
            }

            builder.Append('_');
        }

        private static DeckImageExportPlan Reject(string reason)
        {
            return new DeckImageExportPlan
            {
                accepted = false,
                file_path = string.Empty,
                file_name = string.Empty,
                rejection_reason = string.IsNullOrWhiteSpace(reason) ? "Export rejected." : reason
            };
        }

        private static bool IsChildPath(string path, string root)
        {
            string normalizedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return path.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
        }
    }
}

