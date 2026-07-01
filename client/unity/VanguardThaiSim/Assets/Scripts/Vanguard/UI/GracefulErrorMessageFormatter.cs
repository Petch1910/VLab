using System;

namespace VanguardThaiSim.UI
{
    public static class GracefulErrorMessageFormatter
    {
        public const string RetryAction = "Retry: go back Home, then open this screen again.";
        public const string ReportAction = "If it keeps failing, check the local card pack files.";
        public const int DefaultMaxDetailLength = 180;

        public static string FormatRecoverableError(
            string title,
            string detail,
            string retryAction = RetryAction)
        {
            string safeTitle = string.IsNullOrWhiteSpace(title)
                ? "Something went wrong"
                : Compact(title);
            string safeDetail = string.IsNullOrWhiteSpace(detail)
                ? "No technical detail was provided."
                : Trim(Compact(detail), DefaultMaxDetailLength);
            string safeRetry = string.IsNullOrWhiteSpace(retryAction)
                ? RetryAction
                : Compact(retryAction);

            return safeTitle + "\n" +
                   safeDetail + "\n\n" +
                   safeRetry + "\n" +
                   ReportAction;
        }

        public static string FormatUnhandledException(Exception exception)
        {
            string detail = exception == null
                ? "Unknown exception"
                : exception.GetType().Name + ": " + exception.Message;
            return FormatRecoverableError("Unexpected error", detail);
        }

        private static string Compact(string value)
        {
            return string.Join(
                " ",
                value.Trim().Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static string Trim(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value ?? string.Empty;
            }

            return value.Substring(0, Math.Max(0, maxLength - 3)) + "...";
        }
    }
}
