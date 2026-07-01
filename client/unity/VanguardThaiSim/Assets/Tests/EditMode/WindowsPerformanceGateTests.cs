using NUnit.Framework;
using VanguardThaiSim.Smoke;

namespace VanguardThaiSim.Tests
{
    public sealed class WindowsPerformanceGateTests
    {
        [Test]
        public void GateAcceptsCurrentWindowsBaselineAndCacheLimits()
        {
            WindowsPerformanceGateReport report = WindowsPerformanceGate.Run();

            Assert.IsTrue(report.accepted, report.ToJson(true));
            Assert.AreEqual("M27-04", report.milestone);
            Assert.NotNull(report.baseline_report);
            Assert.IsTrue(report.baseline_report.accepted, report.baseline_report.ToJson(true));
            Assert.LessOrEqual(report.cache_observed_thumbnail_count, report.cache_max_thumbnails);
            Assert.LessOrEqual(report.cache_observed_full_image_count, report.cache_max_full_images);
            Assert.IsTrue(report.cache_clear_memory_passed);
            Assert.NotNull(report.headless_profile);
            Assert.IsTrue(report.headless_profile.accepted, report.headless_profile.ToJson(true));
            Assert.LessOrEqual(
                report.headless_profile.average_elapsed_ms,
                report.max_headless_average_elapsed_ms);
            Assert.AreEqual(30, report.playtable_target_fps);
            Assert.AreEqual(0, report.blockers.Count, report.ToJson(true));
        }

        [Test]
        public void ReportRoundTripsJson()
        {
            WindowsPerformanceGateReport report = WindowsPerformanceGate.Run();
            WindowsPerformanceGateReport roundTrip =
                WindowsPerformanceGateReport.FromJson(report.ToJson(false));

            Assert.AreEqual(report.schema_version, roundTrip.schema_version);
            Assert.AreEqual(report.milestone, roundTrip.milestone);
            Assert.AreEqual(report.accepted, roundTrip.accepted);
            Assert.AreEqual(report.cache_observed_thumbnail_count, roundTrip.cache_observed_thumbnail_count);
            Assert.AreEqual(report.cache_observed_full_image_count, roundTrip.cache_observed_full_image_count);
            Assert.AreEqual(report.playtable_target_fps, roundTrip.playtable_target_fps);
        }
    }
}
