using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class CoreRegressionSuiteReport
    {
        public int schema_version = 1;
        public string milestone = "M18-05";
        public string suite_status = "inventory_ready";
        public int category_count;
        public int representative_test_count;
        public List<CoreRegressionSuiteCategory> categories = new List<CoreRegressionSuiteCategory>();

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static CoreRegressionSuiteReport FromJson(string json)
        {
            CoreRegressionSuiteReport report = JsonUtility.FromJson<CoreRegressionSuiteReport>(json);
            report?.EnsureLists();
            return report;
        }

        public void EnsureLists()
        {
            if (categories == null)
            {
                categories = new List<CoreRegressionSuiteCategory>();
            }

            foreach (CoreRegressionSuiteCategory category in categories)
            {
                category?.EnsureLists();
            }
        }
    }

    [Serializable]
    public sealed class CoreRegressionSuiteCategory
    {
        public string id;
        public string description;
        public List<string> representative_tests = new List<string>();

        public void EnsureLists()
        {
            if (representative_tests == null)
            {
                representative_tests = new List<string>();
            }
        }
    }

    [Serializable]
    public sealed class CoreRegressionSuiteValidationResult
    {
        public bool accepted;
        public List<string> errors = new List<string>();

        public string ToJson(bool prettyPrint = false)
        {
            if (errors == null)
            {
                errors = new List<string>();
            }

            return JsonUtility.ToJson(this, prettyPrint);
        }
    }

    public static class CoreRegressionSuiteReportBuilder
    {
        private static readonly string[] RequiredCategories =
        {
            "rulescore_command_facade",
            "legal_action_mask",
            "event_sourcing_replay",
            "snapshot_rollback",
            "hidden_state_masking",
            "resource_ledger",
            "ruleset_profiles"
        };

        public static CoreRegressionSuiteReport CreateCurrent()
        {
            CoreRegressionSuiteReport report = new CoreRegressionSuiteReport();
            report.categories.Add(Category(
                "rulescore_command_facade",
                "RulesCore command/query facade and accepted/rejected command paths.",
                "GameStateTests.RulesCoreExecutesDrawMovePhaseAndGiftThroughEventLog",
                "RejectNoMutationGuaranteeTests.RulesCoreRejectsIllegalActionWithoutMutatingState"));
            report.categories.Add(Category(
                "legal_action_mask",
                "Legal action mask and UI/bot usage boundaries.",
                "GameStateTests.LegalActionGeneratorReturnsDrawMoveAndPhaseActions",
                "LegalActionMaskUsageReportTests.HardenedPathsValidateThroughRulesCoreOrUseLegalActionGenerator",
                "GameStateTests.EasyBotDecisionComesFromLegalActions"));
            report.categories.Add(Category(
                "event_sourcing_replay",
                "GameEvent sourcing and replay determinism.",
                "GameStateTests.LegalActionExecutorUsesGameActionServiceAndEventLog",
                "ReplayDeterminismVerifierTests.VerifierAcceptsSupportedRulesCoreCommandScript"));
            report.categories.Add(Category(
                "snapshot_rollback",
                "Snapshot, clone, rollback, and no live-state mutation checks.",
                "GameStateTests.SnapshotRestoreReturnsStateAndSupportsIsolatedBranch",
                "SnapshotRollbackVerifierTests.VerifierAcceptsBranchActionWithoutMutatingLiveState"));
            report.categories.Add(Category(
                "hidden_state_masking",
                "Player/spectator/bot hidden-state view hardening.",
                "GameStateViewTests.PlayerViewKeepsOwnHandButMasksOpponentHandAndAllDecks",
                "GameStateViewTests.SpectatorViewMasksBothHandsAndDecksButKeepsFaceUpPublicCards",
                "HiddenStateViewHardeningVerifierTests.VerifierAcceptsCurrentPlayerAndSpectatorMaskingPolicy"));
            report.categories.Add(Category(
                "resource_ledger",
                "CounterBlast, SoulBlast, Energy, and once-flag validation.",
                "ResourceLedgerTests.LedgerDerivesCounterBlastFromFaceUpDamageAndAcceptsAvailableCosts",
                "ResourceLedgerTests.LedgerRejectsUnavailableCostsWithoutMutatingLedger",
                "ResourceLedgerTests.LedgerRejectsDuplicateOnceFlags"));
            report.categories.Add(Category(
                "ruleset_profiles",
                "Standard, V-Premium, Premium, and custom-format RuleSet flags.",
                "RuleSetProfileTests.StandardProfileSeparatesStandardOnlyFlags",
                "RuleSetProfileTests.VPremiumProfileSeparatesGiftAndClanFightFlags",
                "RuleSetProfileTests.PremiumProfileSeparatesStrideGZoneAndLegacyTriggerFlags"));

            RefreshCounts(report);
            return report;
        }

        public static CoreRegressionSuiteValidationResult Validate(CoreRegressionSuiteReport report)
        {
            CoreRegressionSuiteValidationResult result = new CoreRegressionSuiteValidationResult();
            if (report == null)
            {
                result.errors.Add("report_missing");
                return result;
            }

            report.EnsureLists();
            if (report.schema_version != 1)
            {
                result.errors.Add("schema_version_must_be_1");
            }

            if (report.milestone != "M18-05")
            {
                result.errors.Add("milestone_must_be_M18-05");
            }

            foreach (string required in RequiredCategories)
            {
                CoreRegressionSuiteCategory category = FindCategory(report, required);
                if (category == null)
                {
                    result.errors.Add("missing_category_" + required);
                    continue;
                }

                if (category.representative_tests.Count == 0)
                {
                    result.errors.Add("category_has_no_tests_" + required);
                }
            }

            result.accepted = result.errors.Count == 0;
            return result;
        }

        private static CoreRegressionSuiteCategory Category(
            string id,
            string description,
            params string[] representativeTests)
        {
            return new CoreRegressionSuiteCategory
            {
                id = id,
                description = description,
                representative_tests = new List<string>(representativeTests)
            };
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

        private static void RefreshCounts(CoreRegressionSuiteReport report)
        {
            report.EnsureLists();
            report.category_count = report.categories.Count;
            report.representative_test_count = 0;
            foreach (CoreRegressionSuiteCategory category in report.categories)
            {
                if (category != null)
                {
                    report.representative_test_count += category.representative_tests.Count;
                }
            }
        }
    }
}
