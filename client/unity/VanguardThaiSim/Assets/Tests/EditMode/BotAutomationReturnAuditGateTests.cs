using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;

namespace VanguardThaiSim.Tests
{
    public sealed class BotAutomationReturnAuditGateTests
    {
        [Test]
        public void DefaultAuditAllowsM26_02ButBlocksAdvancedExpansion()
        {
            BotAutomationReturnAuditReport report =
                BotAutomationReturnAuditGate.EvaluateDefault();

            Assert.IsTrue(report.m26_return_allowed);
            Assert.IsFalse(report.advanced_search_or_rl_allowed);
            Assert.AreEqual("M26-02", report.allowed_next_task);
            Assert.AreEqual(0, report.blocked_count);
            Assert.AreEqual(report.items.Count, report.ready_count);
            Assert.GreaterOrEqual(report.items.Count, 10);
        }

        [Test]
        public void BlockedPrerequisitePreventsBotAutomationReturn()
        {
            BotAutomationReturnAuditReport report =
                BotAutomationReturnAuditGate.Evaluate(new List<BotAutomationReturnAuditItem>
                {
                    new BotAutomationReturnAuditItem
                    {
                        requirement_id = "m25_online_room_closed",
                        title = "M25 closed",
                        ready = true,
                        evidence = "closeout"
                    },
                    new BotAutomationReturnAuditItem
                    {
                        requirement_id = "masked_state_boundary",
                        title = "Masked state",
                        ready = false,
                        evidence = "missing"
                    }
                });

            Assert.IsFalse(report.m26_return_allowed);
            Assert.IsFalse(report.advanced_search_or_rl_allowed);
            Assert.AreEqual(string.Empty, report.allowed_next_task);
            Assert.AreEqual(1, report.ready_count);
            Assert.AreEqual(1, report.blocked_count);
        }

        [Test]
        public void DefaultAuditIncludesWindowsMilestoneAndBotSafetyItems()
        {
            BotAutomationReturnAuditReport report =
                BotAutomationReturnAuditGate.EvaluateDefault();

            Assert.NotNull(Find(report, "m21_playtable_windows_ux_closed"));
            Assert.NotNull(Find(report, "m22_settings_accessories_closed"));
            Assert.NotNull(Find(report, "m23_manual_tutorial_closed"));
            Assert.NotNull(Find(report, "m24_deck_import_custom_pack_closed"));
            Assert.NotNull(Find(report, "m25_online_room_closed"));
            Assert.NotNull(Find(report, "legal_action_boundary"));
            Assert.NotNull(Find(report, "masked_state_boundary"));
            Assert.NotNull(Find(report, "simulation_no_live_mutation"));
            Assert.NotNull(Find(report, "probability_planning_only"));
            Assert.NotNull(Find(report, "no_live_text_or_llm_resolution"));
        }

        [Test]
        public void ReportJsonRoundTripsDeterministically()
        {
            BotAutomationReturnAuditReport first =
                BotAutomationReturnAuditGate.EvaluateDefault();
            BotAutomationReturnAuditReport second =
                BotAutomationReturnAuditGate.EvaluateDefault();

            string json = first.ToJson(false);
            BotAutomationReturnAuditReport roundTrip =
                BotAutomationReturnAuditReport.FromJson(json);

            Assert.AreEqual(first.ToJson(false), second.ToJson(false));
            Assert.AreEqual(first.gate_id, roundTrip.gate_id);
            Assert.AreEqual(first.m26_return_allowed, roundTrip.m26_return_allowed);
            Assert.AreEqual(first.advanced_search_or_rl_allowed, roundTrip.advanced_search_or_rl_allowed);
            Assert.AreEqual(first.items.Count, roundTrip.items.Count);
            Assert.IsFalse(json.Contains("deck_code"));
            Assert.IsFalse(json.Contains("FullGameState"));
        }

        private static BotAutomationReturnAuditItem Find(
            BotAutomationReturnAuditReport report,
            string requirementId)
        {
            foreach (BotAutomationReturnAuditItem item in report.items)
            {
                if (item.requirement_id == requirementId)
                {
                    return item;
                }
            }

            return null;
        }
    }
}
