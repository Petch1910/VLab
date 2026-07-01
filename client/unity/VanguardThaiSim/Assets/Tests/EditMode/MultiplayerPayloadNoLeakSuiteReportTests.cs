using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class MultiplayerPayloadNoLeakSuiteReportTests
    {
        [Test]
        public void CurrentReportValidatesRequiredCategories()
        {
            MultiplayerPayloadNoLeakSuiteReport report =
                MultiplayerPayloadNoLeakSuiteReportBuilder.CreateCurrent();

            MultiplayerPayloadNoLeakSuiteValidationResult result =
                MultiplayerPayloadNoLeakSuiteReportBuilder.Validate(report);

            Assert.IsTrue(result.accepted, string.Join(",", result.errors));
            Assert.AreEqual("M18-07", report.milestone);
            Assert.AreEqual(10, report.category_count);
            Assert.GreaterOrEqual(report.representative_test_count, 45);
            Assert.NotNull(FindCategory(report, "public_event_masking"));
            Assert.NotNull(FindCategory(report, "trigger_check_payload"));
            Assert.NotNull(FindCategory(report, "manual_resolution_decision_payload"));
        }

        [Test]
        public void MissingRequiredCategoryRejectsReport()
        {
            MultiplayerPayloadNoLeakSuiteReport report =
                MultiplayerPayloadNoLeakSuiteReportBuilder.CreateCurrent();
            report.categories.Remove(FindCategory(report, "public_reconnect_recovery"));

            MultiplayerPayloadNoLeakSuiteValidationResult result =
                MultiplayerPayloadNoLeakSuiteReportBuilder.Validate(report);

            Assert.IsFalse(result.accepted);
            Assert.Contains("missing_category_public_reconnect_recovery", result.errors);
        }

        [Test]
        public void RequiredCategoryWithoutRepresentativeTestsRejectsReport()
        {
            MultiplayerPayloadNoLeakSuiteReport report =
                MultiplayerPayloadNoLeakSuiteReportBuilder.CreateCurrent();
            FindCategory(report, "pending_auto_queue_payload").representative_tests = new List<string>();

            MultiplayerPayloadNoLeakSuiteValidationResult result =
                MultiplayerPayloadNoLeakSuiteReportBuilder.Validate(report);

            Assert.IsFalse(result.accepted);
            Assert.Contains("category_has_no_tests_pending_auto_queue_payload", result.errors);
        }

        [Test]
        public void ReportJsonRoundTripKeepsMilestoneAndCategoriesVisible()
        {
            MultiplayerPayloadNoLeakSuiteReport report =
                MultiplayerPayloadNoLeakSuiteReportBuilder.CreateCurrent();

            MultiplayerPayloadNoLeakSuiteReport roundTrip =
                MultiplayerPayloadNoLeakSuiteReport.FromJson(report.ToJson(false));

            Assert.AreEqual(report.schema_version, roundTrip.schema_version);
            Assert.AreEqual("M18-07", roundTrip.milestone);
            Assert.AreEqual(report.category_count, roundTrip.category_count);
            Assert.AreEqual(report.representative_test_count, roundTrip.representative_test_count);
            Assert.NotNull(FindCategory(roundTrip, "command_envelope_cursor"));
            Assert.NotNull(FindCategory(roundTrip, "spectator_replay_sync"));
            Assert.NotNull(FindCategory(roundTrip, "session_storage_no_mutation"));
        }

        private static MultiplayerPayloadNoLeakSuiteCategory FindCategory(
            MultiplayerPayloadNoLeakSuiteReport report,
            string id)
        {
            for (int i = 0; i < report.categories.Count; i++)
            {
                MultiplayerPayloadNoLeakSuiteCategory category = report.categories[i];
                if (category != null && category.id == id)
                {
                    return category;
                }
            }

            return null;
        }
    }
}
