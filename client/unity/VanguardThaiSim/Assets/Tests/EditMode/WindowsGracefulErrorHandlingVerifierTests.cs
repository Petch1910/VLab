using NUnit.Framework;
using VanguardThaiSim.Smoke;

namespace VanguardThaiSim.Tests
{
    public sealed class WindowsGracefulErrorHandlingVerifierTests
    {
        [Test]
        public void VerifierAcceptsCurrentErrorSurfaces()
        {
            WindowsGracefulErrorHandlingReport report = WindowsGracefulErrorHandlingVerifier.Run();

            Assert.IsTrue(report.accepted, report.ToJson(true));
            Assert.AreEqual("M27-05", report.milestone);
            Assert.AreEqual(0, report.blockers.Count, report.ToJson(true));
            StringAssert.Contains("Retry:", report.card_pack_failure_message);
            StringAssert.Contains("fallback", report.missing_image_message);
            StringAssert.Contains("Unexpected error", report.unhandled_exception_message);
        }

        [Test]
        public void ReportRoundTripsJson()
        {
            WindowsGracefulErrorHandlingReport report = WindowsGracefulErrorHandlingVerifier.Run();
            WindowsGracefulErrorHandlingReport roundTrip =
                WindowsGracefulErrorHandlingReport.FromJson(report.ToJson(false));

            Assert.AreEqual(report.schema_version, roundTrip.schema_version);
            Assert.AreEqual(report.milestone, roundTrip.milestone);
            Assert.AreEqual(report.accepted, roundTrip.accepted);
            Assert.AreEqual(report.blockers.Count, roundTrip.blockers.Count);
        }
    }
}
