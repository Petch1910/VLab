using NUnit.Framework;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class WindowsOnlineRoomUsabilityCloseoutReportTests
    {
        [Test]
        public void CurrentReportValidatesM25Closeout()
        {
            WindowsOnlineRoomUsabilityCloseoutReport report =
                WindowsOnlineRoomUsabilityCloseoutReportBuilder.CreateCurrent();

            WindowsOnlineRoomUsabilityCloseoutValidationResult result =
                WindowsOnlineRoomUsabilityCloseoutReportBuilder.Validate(report);

            Assert.IsTrue(result.accepted, string.Join(",", result.errors));
            Assert.AreEqual("M25-08", report.milestone);
            Assert.AreEqual("M26-01", report.next_target);
            Assert.AreEqual(7, report.completed_tasks.Count);
            Assert.GreaterOrEqual(report.preserved_guardrails.Count, 6);
            Assert.GreaterOrEqual(report.verification_artifacts.Count, 4);
        }

        [Test]
        public void MissingRequiredTaskRejectsReport()
        {
            WindowsOnlineRoomUsabilityCloseoutReport report =
                WindowsOnlineRoomUsabilityCloseoutReportBuilder.CreateCurrent();
            report.completed_tasks.RemoveAt(3);

            WindowsOnlineRoomUsabilityCloseoutValidationResult result =
                WindowsOnlineRoomUsabilityCloseoutReportBuilder.Validate(report);

            Assert.IsFalse(result.accepted);
            Assert.Contains("missing_task_M25-04", result.errors);
        }

        [Test]
        public void MissingRequiredGuardrailRejectsReport()
        {
            WindowsOnlineRoomUsabilityCloseoutReport report =
                WindowsOnlineRoomUsabilityCloseoutReportBuilder.CreateCurrent();
            report.preserved_guardrails.Remove("hidden_state_masking_preserved");

            WindowsOnlineRoomUsabilityCloseoutValidationResult result =
                WindowsOnlineRoomUsabilityCloseoutReportBuilder.Validate(report);

            Assert.IsFalse(result.accepted);
            Assert.Contains("missing_guardrail_hidden_state_masking_preserved", result.errors);
        }

        [Test]
        public void JsonRoundTripPreservesCloseoutWithoutSensitivePayloadFieldNames()
        {
            WindowsOnlineRoomUsabilityCloseoutReport report =
                WindowsOnlineRoomUsabilityCloseoutReportBuilder.CreateCurrent();

            string json = report.ToJson(false);
            WindowsOnlineRoomUsabilityCloseoutReport roundTrip =
                WindowsOnlineRoomUsabilityCloseoutReport.FromJson(json);

            Assert.AreEqual(report.completed_tasks.Count, roundTrip.completed_tasks.Count);
            Assert.AreEqual("friend_room_on_windows_is_easier_to_use", roundTrip.player_outcome);
            Assert.IsFalse(json.Contains("deck_code"));
            Assert.IsFalse(json.Contains("revealed_deck_code"));
        }
    }
}
