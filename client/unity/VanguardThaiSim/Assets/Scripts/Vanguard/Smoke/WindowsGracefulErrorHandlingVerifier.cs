using System;
using System.Collections.Generic;
using UnityEngine;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Smoke
{
    [Serializable]
    public sealed class WindowsGracefulErrorHandlingReport
    {
        public int schema_version = 1;
        public string milestone = "M27-05";
        public bool accepted;
        public string card_pack_failure_message;
        public string missing_image_message;
        public string unhandled_exception_message;
        public string summary;
        public List<string> blockers = new List<string>();

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static WindowsGracefulErrorHandlingReport FromJson(string json)
        {
            WindowsGracefulErrorHandlingReport report =
                JsonUtility.FromJson<WindowsGracefulErrorHandlingReport>(json);
            if (report == null)
            {
                throw new ArgumentException(
                    "Windows graceful error handling report JSON could not be parsed.",
                    "json");
            }

            report.EnsureLists();
            return report;
        }

        public void EnsureLists()
        {
            if (blockers == null)
            {
                blockers = new List<string>();
            }
        }
    }

    public static class WindowsGracefulErrorHandlingVerifier
    {
        public static WindowsGracefulErrorHandlingReport Run()
        {
            WindowsGracefulErrorHandlingReport report = new WindowsGracefulErrorHandlingReport
            {
                card_pack_failure_message = UiStateMessageFormatter.FormatCardPackLoadFailure(
                    "FileNotFoundException: manifest.json was not found",
                    "data/packs/vanguard_th"),
                missing_image_message = CardImageFallbackStatusFormatter.FormatDetailStatusWithTip(
                    true,
                    "missing/not-found.jpg"),
                unhandled_exception_message = GracefulErrorMessageFormatter.FormatUnhandledException(
                    new InvalidOperationException("Smoke exception"))
            };

            RequireContains(report, "card_pack_failure_message", report.card_pack_failure_message, "could not be loaded");
            RequireContains(report, "card_pack_failure_message", report.card_pack_failure_message, "Expected pack:");
            RequireContains(report, "card_pack_failure_message", report.card_pack_failure_message, "Retry:");
            RequireContains(report, "missing_image_message", report.missing_image_message, "fallback");
            RequireContains(report, "missing_image_message", report.missing_image_message, "Tip:");
            RequireContains(report, "unhandled_exception_message", report.unhandled_exception_message, "Unexpected error");
            RequireContains(report, "unhandled_exception_message", report.unhandled_exception_message, "Retry:");
            RequireContains(report, "unhandled_exception_message", report.unhandled_exception_message, "InvalidOperationException");

            report.accepted = report.blockers.Count == 0;
            report.summary = report.accepted
                ? "Windows graceful error handling verifier passed."
                : "Windows graceful error handling verifier blocked.";
            return report;
        }

        private static void RequireContains(
            WindowsGracefulErrorHandlingReport report,
            string field,
            string value,
            string expected)
        {
            if (string.IsNullOrEmpty(value) || !value.Contains(expected))
            {
                report.blockers.Add(field + "_missing_" + expected);
            }
        }
    }
}
