using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class CoreRegressionSuiteReportTests
    {
        [Test]
        public void CurrentReportValidatesRequiredCategories()
        {
            CoreRegressionSuiteReport report = CoreRegressionSuiteReportBuilder.CreateCurrent();

            CoreRegressionSuiteValidationResult result = CoreRegressionSuiteReportBuilder.Validate(report);

            Assert.IsTrue(result.accepted, string.Join(",", result.errors));
            Assert.AreEqual("M18-05", report.milestone);
            Assert.AreEqual(7, report.category_count);
            Assert.GreaterOrEqual(report.representative_test_count, 16);
            Assert.NotNull(FindCategory(report, "rulescore_command_facade"));
            Assert.NotNull(FindCategory(report, "hidden_state_masking"));
        }

        [Test]
        public void MissingRequiredCategoryRejectsReport()
        {
            CoreRegressionSuiteReport report = CoreRegressionSuiteReportBuilder.CreateCurrent();
            report.categories.Remove(FindCategory(report, "resource_ledger"));

            CoreRegressionSuiteValidationResult result = CoreRegressionSuiteReportBuilder.Validate(report);

            Assert.IsFalse(result.accepted);
            Assert.Contains("missing_category_resource_ledger", result.errors);
        }

        [Test]
        public void RequiredCategoryWithoutRepresentativeTestsRejectsReport()
        {
            CoreRegressionSuiteReport report = CoreRegressionSuiteReportBuilder.CreateCurrent();
            FindCategory(report, "ruleset_profiles").representative_tests = new List<string>();

            CoreRegressionSuiteValidationResult result = CoreRegressionSuiteReportBuilder.Validate(report);

            Assert.IsFalse(result.accepted);
            Assert.Contains("category_has_no_tests_ruleset_profiles", result.errors);
        }

        [Test]
        public void ReportJsonRoundTripKeepsMilestoneAndCategoriesVisible()
        {
            CoreRegressionSuiteReport report = CoreRegressionSuiteReportBuilder.CreateCurrent();

            CoreRegressionSuiteReport roundTrip = CoreRegressionSuiteReport.FromJson(report.ToJson(false));

            Assert.AreEqual(report.schema_version, roundTrip.schema_version);
            Assert.AreEqual("M18-05", roundTrip.milestone);
            Assert.AreEqual(report.category_count, roundTrip.category_count);
            Assert.AreEqual(report.representative_test_count, roundTrip.representative_test_count);
            Assert.NotNull(FindCategory(roundTrip, "rulescore_command_facade"));
            Assert.NotNull(FindCategory(roundTrip, "event_sourcing_replay"));
            Assert.NotNull(FindCategory(roundTrip, "hidden_state_masking"));
        }

        private static CoreRegressionSuiteCategory FindCategory(CoreRegressionSuiteReport report, string id)
        {
            for (int i = 0; i < report.categories.Count; i++)
            {
                CoreRegressionSuiteCategory category = report.categories[i];
                if (category != null && category.id == id)
                {
                    return category;
                }
            }

            return null;
        }
    }
}
