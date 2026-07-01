using System;
using System.Collections.Generic;
using System.Text;
using VanguardThaiSim.Cards;
using UnityEngine;

namespace VanguardThaiSim.UI
{
    [Serializable]
    public sealed class LocalCustomImportValidationReport
    {
        public string adapter;
        public bool accepted;
        public string pack_id;
        public string display_name;
        public string source_version;
        public string language;
        public string format;
        public string cards_file;
        public string images_zip;
        public string abilities_file;
        public int card_count;
        public int image_count;
        public int missing_image_count;
        public int unsupported_field_count;
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();

        public void EnsureLists()
        {
            if (errors == null)
            {
                errors = new List<string>();
            }

            if (warnings == null)
            {
                warnings = new List<string>();
            }
        }

        public static bool TryFromJson(string json, out LocalCustomImportValidationReport report)
        {
            report = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            string trimmed = json.Trim();
            if (!trimmed.StartsWith("{", StringComparison.Ordinal) ||
                trimmed.IndexOf("\"adapter\"", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            try
            {
                report = JsonUtility.FromJson<LocalCustomImportValidationReport>(trimmed);
            }
            catch (ArgumentException)
            {
                report = null;
                return false;
            }

            if (report == null || string.IsNullOrWhiteSpace(report.adapter))
            {
                report = null;
                return false;
            }

            report.EnsureLists();
            return true;
        }
    }

    public static class PackValidationUiFormatter
    {
        public const string SourceBoundaryNote =
            "Source note: local/user-provided data only; no third-party auto-download, asset copying, or runtime pack mutation.";

        public static string FormatRuntimePack(CardPackManifest manifest, CardPackValidationStatus status, int maxIssues = 4)
        {
            if (manifest == null)
            {
                return "Pack Check: runtime pack missing\n" + SourceBoundaryNote;
            }

            status = status ?? CardPackValidationStatusBuilder.FromManifest(manifest, false, false);
            status.EnsureLists();
            int missingImages = Math.Max(0, manifest.image_count - manifest.existing_image_count);
            StringBuilder builder = new StringBuilder();
            builder.Append("Pack Check: ");
            builder.Append(status.accepted ? "OK" : "BLOCKED");
            builder.Append(" | ");
            builder.Append(string.IsNullOrWhiteSpace(manifest.pack_id) ? "unknown pack" : manifest.pack_id);
            builder.Append(" | runtime schema ");
            builder.Append(manifest.schema_version);
            builder.Append(" / source schema ");
            builder.Append(status.source_schema_version);
            builder.AppendLine();
            builder.Append("Sets ");
            builder.Append(manifest.series_count);
            builder.Append(" | Clans ");
            builder.Append(manifest.clan_count);
            builder.Append(" | Cards ");
            builder.Append(status.card_count);
            builder.Append(" | Images ");
            builder.Append(manifest.existing_image_count);
            builder.Append("/");
            builder.Append(manifest.image_count);
            builder.Append(" | Missing images ");
            builder.Append(missingImages);
            builder.Append(" | Abilities ");
            builder.Append(status.source_ability_count);
            builder.AppendLine();
            AppendIssues(builder, status.issues, maxIssues);
            builder.Append(SourceBoundaryNote);
            return builder.ToString();
        }

        public static string FormatLocalImportReport(LocalCustomImportValidationReport report, int maxIssues = 4)
        {
            if (report == null)
            {
                return "Pack Check: no local import report\n" + SourceBoundaryNote;
            }

            report.EnsureLists();
            StringBuilder builder = new StringBuilder();
            builder.Append("Pack Check: ");
            builder.Append(report.accepted ? "OK" : "BLOCKED");
            builder.Append(" | ");
            builder.Append(string.IsNullOrWhiteSpace(report.pack_id) ? "unknown pack" : report.pack_id);
            builder.Append(" | adapter ");
            builder.Append(string.IsNullOrWhiteSpace(report.adapter) ? "unknown" : report.adapter);
            builder.AppendLine();
            builder.Append("Cards ");
            builder.Append(report.card_count);
            builder.Append(" | Images ");
            builder.Append(report.image_count);
            builder.Append(" | Missing images ");
            builder.Append(report.missing_image_count);
            builder.Append(" | Unsupported fields ");
            builder.Append(report.unsupported_field_count);
            builder.Append(" | Abilities file ");
            builder.Append(string.IsNullOrWhiteSpace(report.abilities_file) ? "none" : report.abilities_file);
            builder.AppendLine();
            AppendMessages(builder, "Error", report.errors, maxIssues);
            AppendMessages(builder, "Warning", report.warnings, maxIssues);
            builder.Append(SourceBoundaryNote);
            return builder.ToString();
        }

        public static string FormatFromDeckToolsInput(
            string input,
            CardPackManifest manifest,
            CardPackValidationStatus status)
        {
            LocalCustomImportValidationReport report;
            if (LocalCustomImportValidationReport.TryFromJson(input, out report))
            {
                return FormatLocalImportReport(report);
            }

            return FormatRuntimePack(manifest, status);
        }

        private static void AppendIssues(StringBuilder builder, List<CardPackValidationIssue> issues, int maxIssues)
        {
            if (issues == null || issues.Count == 0)
            {
                builder.AppendLine("Issues: none");
                return;
            }

            int limit = Math.Max(1, maxIssues);
            for (int i = 0; i < issues.Count && i < limit; i++)
            {
                CardPackValidationIssue issue = issues[i];
                builder.Append(issue.severity);
                builder.Append(": ");
                builder.Append(string.IsNullOrWhiteSpace(issue.code) ? "ISSUE" : issue.code);
                if (!string.IsNullOrWhiteSpace(issue.message))
                {
                    builder.Append(" - ");
                    builder.Append(Compact(issue.message));
                }

                builder.AppendLine();
            }

            if (issues.Count > limit)
            {
                builder.Append("+");
                builder.Append(issues.Count - limit);
                builder.AppendLine(" more issues");
            }
        }

        private static void AppendMessages(StringBuilder builder, string label, List<string> messages, int maxIssues)
        {
            if (messages == null || messages.Count == 0)
            {
                return;
            }

            int limit = Math.Max(1, maxIssues);
            for (int i = 0; i < messages.Count && i < limit; i++)
            {
                builder.Append(label);
                builder.Append(": ");
                builder.AppendLine(Compact(messages[i]));
            }

            if (messages.Count > limit)
            {
                builder.Append("+");
                builder.Append(messages.Count - limit);
                builder.Append(" more ");
                builder.Append(label.ToLowerInvariant());
                builder.AppendLine("s");
            }
        }

        private static string Compact(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return string.Join(" ", value.Trim().Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}

