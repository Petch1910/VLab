using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class AbilityRegressionSuiteReportTests
    {
        [Test]
        public void CurrentReportValidatesRequiredCategories()
        {
            AbilityRegressionSuiteReport report = AbilityRegressionSuiteReportBuilder.CreateCurrent();

            AbilityRegressionSuiteValidationResult result = AbilityRegressionSuiteReportBuilder.Validate(report);

            Assert.IsTrue(result.accepted, string.Join(",", result.errors));
            Assert.AreEqual("M18-06", report.milestone);
            Assert.AreEqual(9, report.category_count);
            Assert.GreaterOrEqual(report.representative_test_count, 40);
            Assert.NotNull(FindCategory(report, "schema_contract_python"));
            Assert.NotNull(FindCategory(report, "runtime_registry"));
            Assert.NotNull(FindCategory(report, "manual_fallback_bridge"));
        }

        [Test]
        public void MissingRequiredCategoryRejectsReport()
        {
            AbilityRegressionSuiteReport report = AbilityRegressionSuiteReportBuilder.CreateCurrent();
            report.categories.Remove(FindCategory(report, "modifier_templates"));

            AbilityRegressionSuiteValidationResult result = AbilityRegressionSuiteReportBuilder.Validate(report);

            Assert.IsFalse(result.accepted);
            Assert.Contains("missing_category_modifier_templates", result.errors);
        }

        [Test]
        public void RequiredCategoryWithoutRepresentativeTestsRejectsReport()
        {
            AbilityRegressionSuiteReport report = AbilityRegressionSuiteReportBuilder.CreateCurrent();
            FindCategory(report, "fixture_dsl_pack_smoke").representative_tests = new List<string>();

            AbilityRegressionSuiteValidationResult result = AbilityRegressionSuiteReportBuilder.Validate(report);

            Assert.IsFalse(result.accepted);
            Assert.Contains("category_has_no_tests_fixture_dsl_pack_smoke", result.errors);
        }

        [Test]
        public void ReportJsonRoundTripKeepsMilestoneAndCategoriesVisible()
        {
            AbilityRegressionSuiteReport report = AbilityRegressionSuiteReportBuilder.CreateCurrent();

            AbilityRegressionSuiteReport roundTrip = AbilityRegressionSuiteReport.FromJson(report.ToJson(false));

            Assert.AreEqual(report.schema_version, roundTrip.schema_version);
            Assert.AreEqual("M18-06", roundTrip.milestone);
            Assert.AreEqual(report.category_count, roundTrip.category_count);
            Assert.AreEqual(report.representative_test_count, roundTrip.representative_test_count);
            Assert.NotNull(FindCategory(roundTrip, "schema_validator_python"));
            Assert.NotNull(FindCategory(roundTrip, "effect_resource_templates"));
            Assert.NotNull(FindCategory(roundTrip, "custom_pack_ability_metadata"));
        }

        private static AbilityRegressionSuiteCategory FindCategory(AbilityRegressionSuiteReport report, string id)
        {
            for (int i = 0; i < report.categories.Count; i++)
            {
                AbilityRegressionSuiteCategory category = report.categories[i];
                if (category != null && category.id == id)
                {
                    return category;
                }
            }

            return null;
        }
    }
}
