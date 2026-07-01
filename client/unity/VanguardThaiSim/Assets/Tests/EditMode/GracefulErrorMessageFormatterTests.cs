using System;
using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class GracefulErrorMessageFormatterTests
    {
        [Test]
        public void RecoverableErrorCompactsDetailAndIncludesRetry()
        {
            string formatted = GracefulErrorMessageFormatter.FormatRecoverableError(
                "Card data load failed",
                "FileNotFoundException:\nmissing manifest");

            StringAssert.Contains("Card data load failed", formatted);
            StringAssert.Contains("FileNotFoundException: missing manifest", formatted);
            StringAssert.Contains("Retry:", formatted);
            StringAssert.Contains("check the local card pack files", formatted);
        }

        [Test]
        public void UnhandledExceptionFormatsPlayerFacingMessage()
        {
            string formatted = GracefulErrorMessageFormatter.FormatUnhandledException(
                new InvalidOperationException("Smoke exception"));

            StringAssert.Contains("Unexpected error", formatted);
            StringAssert.Contains("InvalidOperationException: Smoke exception", formatted);
            StringAssert.Contains("Retry:", formatted);
        }
    }
}
