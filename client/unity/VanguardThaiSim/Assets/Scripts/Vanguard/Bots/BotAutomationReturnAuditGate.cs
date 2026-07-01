using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Bots
{
    [Serializable]
    public sealed class BotAutomationReturnAuditItem
    {
        public string requirement_id;
        public string title;
        public bool ready;
        public string evidence;
    }

    [Serializable]
    public sealed class BotAutomationReturnAuditReport
    {
        public string gate_id;
        public bool m26_return_allowed;
        public bool advanced_search_or_rl_allowed;
        public string allowed_next_task;
        public int ready_count;
        public int blocked_count;
        public string summary;
        public List<BotAutomationReturnAuditItem> items =
            new List<BotAutomationReturnAuditItem>();

        public void EnsureLists()
        {
            if (items == null)
            {
                items = new List<BotAutomationReturnAuditItem>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static BotAutomationReturnAuditReport FromJson(string json)
        {
            BotAutomationReturnAuditReport report =
                JsonUtility.FromJson<BotAutomationReturnAuditReport>(json);
            if (report == null)
            {
                throw new ArgumentException(
                    "Bot automation return audit report JSON could not be parsed.",
                    nameof(json));
            }

            report.EnsureLists();
            return report;
        }
    }

    public static class BotAutomationReturnAuditGate
    {
        public static BotAutomationReturnAuditReport EvaluateDefault()
        {
            return Evaluate(new List<BotAutomationReturnAuditItem>
            {
                Ready(
                    "m21_playtable_windows_ux_closed",
                    "PlayTable Windows UX pass is closed.",
                    "docs/history/M21_09_PLAYTABLE_WINDOWS_UX_CLOSEOUT.md"),
                Ready(
                    "m22_settings_accessories_closed",
                    "Settings, deck type, and accessories pass is closed.",
                    "docs/history/M22_WINDOWS_SETTINGS_ACCESSORIES_CLOSEOUT.md"),
                Ready(
                    "m23_manual_tutorial_closed",
                    "Manual and tutorial pass is closed.",
                    "docs/history/M23_MANUAL_TUTORIAL_CLOSEOUT.md"),
                Ready(
                    "m24_deck_import_custom_pack_closed",
                    "Deck Builder, import, and custom pack UX pass is closed.",
                    "docs/history/M24_DECK_BUILDER_IMPORT_CUSTOM_PACK_UX_CLOSEOUT.md"),
                Ready(
                    "m25_online_room_closed",
                    "Windows Online Room usability pass is closed.",
                    "docs/history/M25_WINDOWS_ONLINE_ROOM_USABILITY_CLOSEOUT.md"),
                Ready(
                    "legal_action_boundary",
                    "Bot work resumes through RulesCore legal actions only.",
                    "docs/CORE_DEVELOPMENT_GUARDRAILS.md"),
                Ready(
                    "masked_state_boundary",
                    "Bot observations must use masked/player-legal state views.",
                    "docs/VANGUARD_AI_ENGINE_KNOWLEDGE_SUMMARY.md"),
                Ready(
                    "simulation_no_live_mutation",
                    "Snapshot simulation must clone branch state and never mutate live state.",
                    "docs/specs/ci_and_qa/SNAPSHOT_SIMULATION_PATH_SPEC.md"),
                Ready(
                    "probability_planning_only",
                    "Probability is planning input only and cannot replace actual RNG outcomes.",
                    "docs/CORE_DEVELOPMENT_GUARDRAILS.md"),
                Ready(
                    "no_live_text_or_llm_resolution",
                    "Runtime effects must use structured ability data/manual fallback, not LLM or live text parsing.",
                    "docs/specs/bot_and_headless/BOT_ENGINE_SPEC.md")
            });
        }

        public static BotAutomationReturnAuditReport Evaluate(
            IReadOnlyList<BotAutomationReturnAuditItem> items)
        {
            List<BotAutomationReturnAuditItem> safeItems = CopyItems(items);
            int readyCount = 0;
            int blockedCount = 0;
            foreach (BotAutomationReturnAuditItem item in safeItems)
            {
                if (item.ready)
                {
                    readyCount++;
                }
                else
                {
                    blockedCount++;
                }
            }

            bool allowed = safeItems.Count > 0 && blockedCount == 0;
            return new BotAutomationReturnAuditReport
            {
                gate_id = "M26-01-bot-automation-return-audit",
                m26_return_allowed = allowed,
                advanced_search_or_rl_allowed = false,
                allowed_next_task = allowed ? "M26-02" : string.Empty,
                ready_count = readyCount,
                blocked_count = blockedCount,
                summary = allowed
                    ? "M26-02 may proceed under legal-action and masked-state guardrails."
                    : "Bot/automation return remains blocked until prerequisites are fixed.",
                items = safeItems
            };
        }

        private static List<BotAutomationReturnAuditItem> CopyItems(
            IReadOnlyList<BotAutomationReturnAuditItem> items)
        {
            List<BotAutomationReturnAuditItem> safeItems =
                new List<BotAutomationReturnAuditItem>();
            if (items == null)
            {
                return safeItems;
            }

            for (int i = 0; i < items.Count; i++)
            {
                BotAutomationReturnAuditItem item = items[i];
                if (item == null)
                {
                    continue;
                }

                safeItems.Add(new BotAutomationReturnAuditItem
                {
                    requirement_id = item.requirement_id ?? string.Empty,
                    title = item.title ?? string.Empty,
                    ready = item.ready,
                    evidence = item.evidence ?? string.Empty
                });
            }

            return safeItems;
        }

        private static BotAutomationReturnAuditItem Ready(
            string requirementId,
            string title,
            string evidence)
        {
            return new BotAutomationReturnAuditItem
            {
                requirement_id = requirementId,
                title = title,
                ready = true,
                evidence = evidence
            };
        }
    }
}
