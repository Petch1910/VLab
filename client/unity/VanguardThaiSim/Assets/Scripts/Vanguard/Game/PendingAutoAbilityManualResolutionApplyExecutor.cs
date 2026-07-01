namespace VanguardThaiSim.Game
{
    public sealed class PendingAutoAbilityManualResolutionApplyExecutorResult
    {
        public bool accepted;
        public string rejection_reason;
        public PendingAutoAbilityQueue queue;
        public PendingAutoAbilityManualResolutionApplyResult apply_result;
    }

    public static class PendingAutoAbilityManualResolutionApplyExecutor
    {
        public static PendingAutoAbilityManualResolutionApplyExecutorResult Apply(
            PendingAutoAbilityQueue queue,
            PendingAutoAbilityManualResolutionDecision decision)
        {
            PendingAutoAbilityManualResolutionApplyResult validation =
                PendingAutoAbilityManualResolutionApplyCommandValidator.Validate(queue, decision);
            if (!validation.accepted)
            {
                return new PendingAutoAbilityManualResolutionApplyExecutorResult
                {
                    accepted = false,
                    rejection_reason = validation.rejection_reason ?? string.Empty,
                    queue = CloneQueue(queue),
                    apply_result = validation
                };
            }

            PendingAutoAbilityQueue nextQueue = CloneQueue(queue);
            nextQueue.EnsureLists();
            if (decision.decision_type == PendingAutoAbilityManualResolutionDecisionTypes.Skip)
            {
                nextQueue.pending.RemoveAt(0);
            }
            else if (decision.decision_type == PendingAutoAbilityManualResolutionDecisionTypes.Defer)
            {
                PendingAutoAbility deferred = nextQueue.pending[0];
                nextQueue.pending.RemoveAt(0);
                nextQueue.pending.Add(deferred);
            }

            PendingAutoAbilityManualResolutionApplyResult applyResult =
                PendingAutoAbilityManualResolutionApplyResult.Accepted(
                    validation.queue_id,
                    validation.pending_id,
                    validation.decision_type,
                    BuildSummary(decision));

            return new PendingAutoAbilityManualResolutionApplyExecutorResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                queue = nextQueue,
                apply_result = applyResult
            };
        }

        private static string BuildSummary(PendingAutoAbilityManualResolutionDecision decision)
        {
            if (decision.decision_type == PendingAutoAbilityManualResolutionDecisionTypes.Resolve)
            {
                return "Validated Resolve for pending AUTO. Structured ability execution remains manual.";
            }

            return "Applied " + decision.decision_type + " to pending AUTO " + (decision.pending_id ?? string.Empty) + ".";
        }

        private static PendingAutoAbilityQueue CloneQueue(PendingAutoAbilityQueue source)
        {
            if (source == null)
            {
                return null;
            }

            PendingAutoAbilityQueue clone = PendingAutoAbilityQueue.FromJson(source.ToJson(false));
            clone.EnsureLists();
            return clone;
        }
    }
}
