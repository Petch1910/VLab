using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionApplyPreviewLogFormatterTests
    {
        [Test]
        public void AcceptedEntryFormatsDecisionTypeAndPendingId()
        {
            PendingAutoAbilityManualResolutionApplyPreviewLogEntry entry =
                PendingAutoAbilityManualResolutionApplyPreviewLogEntry.Accepted(
                    "log-1",
                    "queue-1",
                    "pending-1",
                    PendingAutoAbilityManualResolutionDecisionTypes.Skip,
                    "Applied Skip.");

            Assert.AreEqual(
                "Pending manual decision apply preview log: accepted type=Skip id=pending-1",
                PendingAutoAbilityManualResolutionApplyPreviewLogFormatter.Format(entry));
        }

        [Test]
        public void RejectedEntryFormatsReason()
        {
            PendingAutoAbilityManualResolutionApplyPreviewLogEntry entry =
                PendingAutoAbilityManualResolutionApplyPreviewLogEntry.Rejected(
                    "log-2",
                    PendingAutoAbilityManualResolutionApplyRejectionReasons.PendingIdMismatch);

            Assert.AreEqual(
                "Pending manual decision apply preview log: rejected " +
                PendingAutoAbilityManualResolutionApplyRejectionReasons.PendingIdMismatch,
                PendingAutoAbilityManualResolutionApplyPreviewLogFormatter.Format(entry));
        }

        [Test]
        public void NullEntryFormatsFallback()
        {
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionApplyPreviewLogFormatter.NullEntryMessage,
                PendingAutoAbilityManualResolutionApplyPreviewLogFormatter.Format(null));
        }

        [Test]
        public void ListFormatsNewestFirstAndBounded()
        {
            var entries = new List<PendingAutoAbilityManualResolutionApplyPreviewLogEntry>
            {
                CreateAccepted("log-1", "pending-1", PendingAutoAbilityManualResolutionDecisionTypes.Resolve),
                CreateAccepted("log-2", "pending-2", PendingAutoAbilityManualResolutionDecisionTypes.Skip),
                CreateAccepted("log-3", "pending-3", PendingAutoAbilityManualResolutionDecisionTypes.Defer)
            };

            string formatted =
                PendingAutoAbilityManualResolutionApplyPreviewLogFormatter.FormatList(entries, 2);

            Assert.IsTrue(formatted.Contains("Pending manual decision apply preview log: 3"));
            Assert.Less(formatted.IndexOf("id=pending-3"), formatted.IndexOf("id=pending-2"));
            Assert.IsFalse(formatted.Contains("id=pending-1"));
            Assert.IsTrue(formatted.Contains("... +1 older"));
        }

        [Test]
        public void EmptyListFormatsFallback()
        {
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionApplyPreviewLogFormatter.NoEntriesMessage,
                PendingAutoAbilityManualResolutionApplyPreviewLogFormatter.FormatList(null));
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionApplyPreviewLogFormatter.NoEntriesMessage,
                PendingAutoAbilityManualResolutionApplyPreviewLogFormatter.FormatList(
                    new List<PendingAutoAbilityManualResolutionApplyPreviewLogEntry>()));
        }

        [Test]
        public void FormatterDoesNotDisplayFreeTextSummary()
        {
            PendingAutoAbilityManualResolutionApplyPreviewLogEntry entry =
                PendingAutoAbilityManualResolutionApplyPreviewLogEntry.Accepted(
                    "log-hidden",
                    "queue-1",
                    "pending-hidden",
                    PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                    "Secret source CARD-SECRET source-instance-1");

            string formatted =
                PendingAutoAbilityManualResolutionApplyPreviewLogFormatter.Format(entry);

            Assert.IsTrue(formatted.Contains("id=pending-hidden"));
            Assert.IsFalse(formatted.Contains("CARD-SECRET"));
            Assert.IsFalse(formatted.Contains("source-instance-1"));
        }

        [Test]
        public void FormattingDoesNotMutateEntry()
        {
            PendingAutoAbilityManualResolutionApplyPreviewLogEntry entry =
                CreateAccepted("log-4", "pending-4", PendingAutoAbilityManualResolutionDecisionTypes.Skip);
            string before = entry.ToJson(false);

            PendingAutoAbilityManualResolutionApplyPreviewLogFormatter.Format(entry);
            PendingAutoAbilityManualResolutionApplyPreviewLogFormatter.FormatList(
                new List<PendingAutoAbilityManualResolutionApplyPreviewLogEntry> { entry });

            Assert.AreEqual(before, entry.ToJson(false));
        }

        private static PendingAutoAbilityManualResolutionApplyPreviewLogEntry CreateAccepted(
            string logEntryId,
            string pendingId,
            string decisionType)
        {
            return PendingAutoAbilityManualResolutionApplyPreviewLogEntry.Accepted(
                logEntryId,
                "queue-1",
                pendingId,
                decisionType,
                "Applied " + decisionType + ".");
        }
    }
}
