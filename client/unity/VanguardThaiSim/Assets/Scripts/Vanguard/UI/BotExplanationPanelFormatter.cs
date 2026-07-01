using System;
using System.Collections.Generic;
using System.Text;
using VanguardThaiSim.Bots;

namespace VanguardThaiSim.UI
{
    public static class BotExplanationPanelFormatter
    {
        public const string EmptyMessage = "Bot Plan\nNo bot decision yet.";
        public const int DefaultMaxOptions = 3;

        public static string Format(BotDebugTrace trace, int maxOptions = DefaultMaxOptions)
        {
            if (trace == null || trace.candidate_count <= 0)
            {
                return EmptyMessage;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Bot Plan");
            builder.AppendLine("Choice: " + FormatAction(trace.selected_action_summary));
            builder.AppendLine("Why: compared " + Math.Max(0, trace.candidate_count) + " legal option(s).");
            if (!string.IsNullOrWhiteSpace(trace.playbook_id) && trace.playbook_id != "none")
            {
                builder.AppendLine("Style: " + FormatPlaybookId(trace.playbook_id));
            }

            int count = Math.Min(Math.Max(0, maxOptions), Count(trace.lines));
            if (count > 0)
            {
                builder.AppendLine("Options:");
                for (int i = 0; i < count; i++)
                {
                    BotDebugTraceLine line = trace.lines[i];
                    if (line == null)
                    {
                        continue;
                    }

                    builder.AppendLine(
                        (line.selected ? "* " : "- ") +
                        FormatAction(line.action_summary));
                }
            }

            return builder.ToString().TrimEnd();
        }

        private static int Count(IReadOnlyList<BotDebugTraceLine> lines)
        {
            return lines == null ? 0 : lines.Count;
        }

        private static string FormatAction(string rawSummary)
        {
            string summary = StripDeveloperDetails(rawSummary);
            if (string.IsNullOrWhiteSpace(summary))
            {
                return "No action selected.";
            }

            if (summary == "Draw")
            {
                return "Draw a card.";
            }

            if (summary.StartsWith("MoveCard ", StringComparison.Ordinal))
            {
                string route = summary.Substring("MoveCard ".Length);
                string[] zones = route.Split(new[] { "->" }, StringSplitOptions.None);
                if (zones.Length == 2)
                {
                    return "Move a card from " + FormatZone(zones[0]) + " to " + FormatZone(zones[1]) + ".";
                }

                return "Move a card.";
            }

            if (summary.StartsWith("SetPhase ", StringComparison.Ordinal))
            {
                return "Change to " + FormatZone(summary.Substring("SetPhase ".Length)) + " phase.";
            }

            if (summary.StartsWith("AddGiftMarker ", StringComparison.Ordinal))
            {
                return "Add a " + summary.Substring("AddGiftMarker ".Length) + " marker.";
            }

            if (summary.StartsWith("ResourceFlip ", StringComparison.Ordinal))
            {
                return "Use " + summary.Substring("ResourceFlip ".Length) + ".";
            }

            return summary + ".";
        }

        private static string StripDeveloperDetails(string rawSummary)
        {
            string summary = rawSummary ?? string.Empty;
            string[] markers =
            {
                " playbook=",
                " score=",
                " own=",
                " opp=",
                " base=",
                " bias=",
                " total="
            };

            int cut = summary.Length;
            foreach (string marker in markers)
            {
                int index = summary.IndexOf(marker, StringComparison.Ordinal);
                if (index >= 0 && index < cut)
                {
                    cut = index;
                }
            }

            return summary.Substring(0, cut).Trim();
        }

        private static string FormatPlaybookId(string playbookId)
        {
            return (playbookId ?? string.Empty).Replace('_', ' ').Trim();
        }

        private static string FormatZone(string value)
        {
            switch ((value ?? string.Empty).Trim())
            {
                case "Hand":
                    return "hand";
                case "Deck":
                    return "deck";
                case "RideDeck":
                    return "ride deck";
                case "Vanguard":
                    return "vanguard";
                case "RearGuard":
                    return "rear-guard";
                case "Drop":
                    return "drop";
                case "Damage":
                    return "damage";
                case "Bind":
                    return "bind";
                case "Soul":
                    return "soul";
                case "Battle":
                    return "battle";
                case "Main":
                    return "main";
                case "Ride":
                    return "ride";
                case "End":
                    return "end";
                default:
                    return (value ?? string.Empty).Trim().ToLowerInvariant();
            }
        }
    }
}
