using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public static class LiveEffectResolutionTextParsingGuardRejectionReasons
    {
        public const string PolicyMissing = "LIVE_EFFECT_POLICY_MISSING";
        public const string PolicyAllowsLiveTextParsing = "LIVE_EFFECT_POLICY_ALLOWS_LIVE_TEXT_PARSING";
        public const string PolicyAllowsLlmResolution = "LIVE_EFFECT_POLICY_ALLOWS_LLM_RESOLUTION";
        public const string PolicyModeForbidden = "LIVE_EFFECT_POLICY_MODE_FORBIDDEN";
        public const string ReportMissing = "LIVE_EFFECT_POLICY_REPORT_MISSING";
        public const string ReportInvalid = "LIVE_EFFECT_POLICY_REPORT_INVALID";
        public const string AbilityMissing = "LIVE_EFFECT_ABILITY_MISSING";
        public const string CustomHandlerMissing = "LIVE_EFFECT_CUSTOM_HANDLER_MISSING";
    }

    [Serializable]
    public sealed class AbilityEffectHandlerPolicy
    {
        public string policy_id;
        public bool allows_live_text_parsing;
        public bool allows_llm_resolution;
        public string source;
        public string notes;

        public static AbilityEffectHandlerPolicy StructuredCommandOnly(string source)
        {
            return new AbilityEffectHandlerPolicy
            {
                policy_id = "structured_command_only",
                allows_live_text_parsing = false,
                allows_llm_resolution = false,
                source = source ?? string.Empty,
                notes = "Handler must use structured data, RulesCore commands, or manual fallback only."
            };
        }

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static AbilityEffectHandlerPolicy FromJson(string json)
        {
            AbilityEffectHandlerPolicy policy =
                JsonUtility.FromJson<AbilityEffectHandlerPolicy>(json);
            if (policy == null)
            {
                throw new ArgumentException(
                    "Ability effect handler policy JSON could not be parsed.",
                    "json");
            }

            return policy;
        }
    }

    [Serializable]
    public sealed class LiveEffectResolutionPolicyEntry
    {
        public string path_id;
        public string description;
        public AbilityEffectHandlerPolicy policy;
    }

    [Serializable]
    public sealed class LiveEffectResolutionPolicyReport
    {
        public string schema_version;
        public List<LiveEffectResolutionPolicyEntry> entries = new List<LiveEffectResolutionPolicyEntry>();
        public string summary;

        public void EnsureLists()
        {
            if (entries == null)
            {
                entries = new List<LiveEffectResolutionPolicyEntry>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static LiveEffectResolutionPolicyReport FromJson(string json)
        {
            LiveEffectResolutionPolicyReport report =
                JsonUtility.FromJson<LiveEffectResolutionPolicyReport>(json);
            if (report == null)
            {
                throw new ArgumentException(
                    "Live effect resolution policy report JSON could not be parsed.",
                    "json");
            }

            report.EnsureLists();
            return report;
        }
    }

    [Serializable]
    public sealed class LiveEffectResolutionPolicyValidationResult
    {
        public bool accepted;
        public string rejection_reason;
        public bool requires_manual_resolution;
        public List<string> issues = new List<string>();
        public string summary;

        public void EnsureLists()
        {
            if (issues == null)
            {
                issues = new List<string>();
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static LiveEffectResolutionPolicyValidationResult FromJson(string json)
        {
            LiveEffectResolutionPolicyValidationResult result =
                JsonUtility.FromJson<LiveEffectResolutionPolicyValidationResult>(json);
            if (result == null)
            {
                throw new ArgumentException(
                    "Live effect resolution policy validation result JSON could not be parsed.",
                    "json");
            }

            result.EnsureLists();
            return result;
        }
    }

    public static class LiveEffectResolutionTextParsingGuard
    {
        public const string PolicySchemaVersion = "live_effect_resolution_policy_v1";

        public static LiveEffectResolutionPolicyReport CreateCurrentReport()
        {
            var report = new LiveEffectResolutionPolicyReport
            {
                schema_version = PolicySchemaVersion,
                summary = "Live effect resolution must use structured data, registered command handlers, or manual fallback."
            };

            Add(
                report,
                "AbilityCore.EnumEffects",
                "Legacy enum-backed AbilityCore effects use structured effect types and RulesCore commands.",
                AbilityEffectHandlerPolicy.StructuredCommandOnly("AbilityCore"));
            Add(
                report,
                "AbilityEffectRegistry.CustomHandlers",
                "Custom handlers must register a policy that forbids live text parsing and LLM resolution.",
                AbilityEffectHandlerPolicy.StructuredCommandOnly("AbilityEffectRegistry"));
            Add(
                report,
                "RuntimeAbilityRegistry.StructuredData",
                "Runtime ability data is schema-driven; notes and labels are metadata only.",
                AbilityEffectHandlerPolicy.StructuredCommandOnly("RuntimeAbilityRegistry"));
            Add(
                report,
                "StructuredAbilityTemplateAutomationGate",
                "M26-04 allows automated structured templates only when test-covered.",
                AbilityEffectHandlerPolicy.StructuredCommandOnly("StructuredAbilityTemplateAutomationGate"));
            Add(
                report,
                "StructuredAbilityManualFallbackBridge",
                "Unsupported effects create manual Resolve decisions instead of auto-resolving card text.",
                AbilityEffectHandlerPolicy.StructuredCommandOnly("StructuredAbilityManualFallbackBridge"));

            return report;
        }

        public static LiveEffectResolutionPolicyValidationResult ValidateCurrentReport()
        {
            return ValidateReport(CreateCurrentReport());
        }

        public static LiveEffectResolutionPolicyValidationResult ValidateReport(
            LiveEffectResolutionPolicyReport report)
        {
            if (report == null)
            {
                return Reject(
                    LiveEffectResolutionTextParsingGuardRejectionReasons.ReportMissing,
                    false,
                    null);
            }

            report.EnsureLists();
            var issues = new List<string>();
            if (!string.Equals(report.schema_version, PolicySchemaVersion, StringComparison.Ordinal))
            {
                issues.Add("schema_version expected " + PolicySchemaVersion + " but was " + (report.schema_version ?? string.Empty));
            }

            for (int i = 0; i < report.entries.Count; i++)
            {
                LiveEffectResolutionPolicyEntry entry = report.entries[i];
                if (entry == null)
                {
                    issues.Add("entry " + i + " is null");
                    continue;
                }

                LiveEffectResolutionPolicyValidationResult policyResult =
                    ValidateCustomEffectPolicy(entry.path_id, entry.policy);
                if (!policyResult.accepted)
                {
                    issues.Add((entry.path_id ?? string.Empty) + ": " + policyResult.rejection_reason);
                }
            }

            if (issues.Count > 0)
            {
                return Reject(
                    LiveEffectResolutionTextParsingGuardRejectionReasons.ReportInvalid,
                    false,
                    issues);
            }

            return new LiveEffectResolutionPolicyValidationResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                requires_manual_resolution = false,
                issues = new List<string>(),
                summary = "Live effect resolution policy report accepted with " + report.entries.Count + " entries."
            };
        }

        public static LiveEffectResolutionPolicyValidationResult ValidateCustomEffectPolicy(
            string effectId,
            AbilityEffectHandlerPolicy policy)
        {
            if (policy == null)
            {
                return Reject(
                    LiveEffectResolutionTextParsingGuardRejectionReasons.PolicyMissing + ": " + (effectId ?? string.Empty),
                    false,
                    null);
            }

            if (policy.allows_live_text_parsing)
            {
                return Reject(
                    LiveEffectResolutionTextParsingGuardRejectionReasons.PolicyAllowsLiveTextParsing + ": " + (effectId ?? string.Empty),
                    false,
                    null);
            }

            if (policy.allows_llm_resolution)
            {
                return Reject(
                    LiveEffectResolutionTextParsingGuardRejectionReasons.PolicyAllowsLlmResolution + ": " + (effectId ?? string.Empty),
                    false,
                    null);
            }

            string policyId = policy.policy_id ?? string.Empty;
            if (ContainsForbiddenMode(policyId))
            {
                return Reject(
                    LiveEffectResolutionTextParsingGuardRejectionReasons.PolicyModeForbidden + ": " + policyId,
                    false,
                    null);
            }

            return new LiveEffectResolutionPolicyValidationResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                requires_manual_resolution = false,
                issues = new List<string>(),
                summary = "Live effect policy accepted for " + (effectId ?? string.Empty) + "."
            };
        }

        public static LiveEffectResolutionPolicyValidationResult ValidateAbilityDefinition(
            AbilityDefinition ability)
        {
            if (ability == null)
            {
                return Reject(
                    LiveEffectResolutionTextParsingGuardRejectionReasons.AbilityMissing,
                    false,
                    null);
            }

            ability.EnsureLists();
            var issues = new List<string>();
            bool missingCustomHandler = false;
            for (int i = 0; i < ability.effects.Count; i++)
            {
                AbilityEffectDefinition effect = ability.effects[i];
                if (effect == null || effect.effect_type != AbilityEffectType.Custom)
                {
                    continue;
                }

                AbilityEffectHandlerPolicy policy = AbilityEffectRegistry.GetPolicy(effect.custom_effect_id);
                if (policy == null)
                {
                    missingCustomHandler = true;
                    issues.Add(
                        LiveEffectResolutionTextParsingGuardRejectionReasons.CustomHandlerMissing +
                        ": " + (effect.custom_effect_id ?? string.Empty));
                    continue;
                }

                LiveEffectResolutionPolicyValidationResult policyResult =
                    ValidateCustomEffectPolicy(effect.custom_effect_id, policy);
                if (!policyResult.accepted)
                {
                    issues.Add(policyResult.rejection_reason);
                }
            }

            if (issues.Count > 0)
            {
                bool manual = ability.manual_fallback && missingCustomHandler;
                return Reject(
                    manual
                        ? LiveEffectResolutionTextParsingGuardRejectionReasons.CustomHandlerMissing
                        : LiveEffectResolutionTextParsingGuardRejectionReasons.ReportInvalid,
                    manual,
                    issues);
            }

            return new LiveEffectResolutionPolicyValidationResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                requires_manual_resolution = false,
                issues = new List<string>(),
                summary = "Ability definition live effect policy accepted for " + (ability.ability_id ?? string.Empty) + "."
            };
        }

        private static void Add(
            LiveEffectResolutionPolicyReport report,
            string pathId,
            string description,
            AbilityEffectHandlerPolicy policy)
        {
            report.entries.Add(new LiveEffectResolutionPolicyEntry
            {
                path_id = pathId ?? string.Empty,
                description = description ?? string.Empty,
                policy = ClonePolicy(policy)
            });
        }

        private static bool ContainsForbiddenMode(string value)
        {
            string safe = (value ?? string.Empty).ToLowerInvariant();
            return safe.Contains("llm") ||
                   safe.Contains("text_parser") ||
                   safe.Contains("free_text") ||
                   safe.Contains("runtime_text");
        }

        private static LiveEffectResolutionPolicyValidationResult Reject(
            string rejectionReason,
            bool requiresManualResolution,
            List<string> issues)
        {
            return new LiveEffectResolutionPolicyValidationResult
            {
                accepted = false,
                rejection_reason = BuildRejectionReason(rejectionReason, issues),
                requires_manual_resolution = requiresManualResolution,
                issues = CloneStrings(issues),
                summary = "Live effect resolution policy rejected: " + (rejectionReason ?? string.Empty)
            };
        }

        private static string BuildRejectionReason(string rejectionReason, List<string> issues)
        {
            string safeReason = rejectionReason ?? string.Empty;
            if (issues == null || issues.Count == 0)
            {
                return safeReason;
            }

            return safeReason + ": " + string.Join(", ", issues.ToArray());
        }

        internal static AbilityEffectHandlerPolicy ClonePolicy(AbilityEffectHandlerPolicy policy)
        {
            if (policy == null)
            {
                return null;
            }

            return AbilityEffectHandlerPolicy.FromJson(policy.ToJson(false));
        }

        private static List<string> CloneStrings(List<string> source)
        {
            var result = new List<string>();
            if (source == null)
            {
                return result;
            }

            for (int i = 0; i < source.Count; i++)
            {
                result.Add(source[i] ?? string.Empty);
            }

            return result;
        }
    }
}
