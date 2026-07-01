using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Smoke;

namespace VanguardThaiSim.Tests
{
    public sealed class WindowsGameplayCompletionVerifierTests
    {
        [Test]
        public void VerifierPassesManualMatchLoopReadiness()
        {
            WindowsGameplayCompletionReport report = WindowsGameplayCompletionVerifier.Run();

            Assert.IsTrue(report.accepted, report.ToJson(true));
            Assert.AreEqual(0, report.blockers.Count, report.ToJson(true));
            Assert.GreaterOrEqual(report.event_count, 16);
            Assert.AreEqual(1, report.player_vanguard_count);
            Assert.GreaterOrEqual(report.player_soul_count, 1);
            Assert.GreaterOrEqual(report.player_rear_guard_count, 1);
            Assert.GreaterOrEqual(report.player_trigger_count, 1);
            Assert.AreEqual(1, report.opponent_vanguard_count);
            Assert.GreaterOrEqual(report.opponent_guardian_count, 1);
            Assert.GreaterOrEqual(report.opponent_trigger_count, 1);
            Assert.AreEqual(GamePhase.End, report.final_phase);
            Assert.NotNull(report.replay_determinism);
            Assert.IsTrue(report.replay_determinism.accepted, report.replay_determinism.rejection_reason);
        }

        [Test]
        public void ReportRoundTripsJson()
        {
            WindowsGameplayCompletionReport report = WindowsGameplayCompletionVerifier.Run();
            WindowsGameplayCompletionReport roundTrip =
                WindowsGameplayCompletionReport.FromJson(report.ToJson(false));

            Assert.AreEqual(report.accepted, roundTrip.accepted);
            Assert.AreEqual(report.event_count, roundTrip.event_count);
            Assert.AreEqual(report.final_phase, roundTrip.final_phase);
            Assert.AreEqual(report.steps.Count, roundTrip.steps.Count);
            Assert.AreEqual(report.blockers.Count, roundTrip.blockers.Count);
            Assert.NotNull(roundTrip.replay_determinism);
            Assert.AreEqual(
                report.replay_determinism.accepted,
                roundTrip.replay_determinism.accepted);
        }
    }
}
