using NUnit.Framework;
using VanguardThaiSim.Smoke;

namespace VanguardThaiSim.Tests
{
    public sealed class WindowsPerformanceBaselineTests
    {
        [Test]
        public void BaselineRecordsCardAndDeckMetrics()
        {
            WindowsPerformanceBaselineReport report = WindowsPerformanceBaseline.Run();

            Assert.IsTrue(report.accepted, report.ToJson(true));
            Assert.AreEqual("M27-03", report.milestone);
            Assert.Greater(report.card_count, 0);
            Assert.GreaterOrEqual(report.queried_card_count, 54);
            Assert.AreEqual(50, report.main_deck_count);
            Assert.AreEqual(4, report.ride_deck_count);
            Assert.GreaterOrEqual(report.repository_load_ms, 0);
            Assert.GreaterOrEqual(report.card_query_ms, 0);
            Assert.GreaterOrEqual(report.card_detail_ms, 0);
            Assert.GreaterOrEqual(report.deck_validation_ms, 0);
            Assert.GreaterOrEqual(report.deck_code_roundtrip_ms, 0);
        }

        [Test]
        public void ReportRoundTripsJson()
        {
            WindowsPerformanceBaselineReport report = WindowsPerformanceBaseline.Run();
            WindowsPerformanceBaselineReport roundTrip =
                WindowsPerformanceBaselineReport.FromJson(report.ToJson(false));

            Assert.AreEqual(report.schema_version, roundTrip.schema_version);
            Assert.AreEqual(report.milestone, roundTrip.milestone);
            Assert.AreEqual(report.accepted, roundTrip.accepted);
            Assert.AreEqual(report.card_count, roundTrip.card_count);
            Assert.AreEqual(report.queried_card_count, roundTrip.queried_card_count);
        }

    }
}
