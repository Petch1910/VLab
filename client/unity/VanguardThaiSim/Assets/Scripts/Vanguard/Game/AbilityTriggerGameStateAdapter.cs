using System.Collections.Generic;

namespace VanguardThaiSim.Game
{
    public static class AbilityTriggerGameStateAdapterRejectionReasons
    {
        public const string MissingState = "STATE_MISSING";
        public const string MissingGameEvent = "GAME_EVENT_MISSING";
    }

    public sealed class AbilityTriggerGameStateAdapterResult
    {
        public bool accepted;
        public string rejection_reason;
        public string source_event_id;
        public string timing_event;
        public int before_count;
        public int after_count;
        public int added_count;
        public bool state_was_updated;
        public PendingAutoAbilityQueue pending_queue;

        private AbilityTriggerGameStateAdapterResult()
        {
        }

        public static AbilityTriggerGameStateAdapterResult Accepted(
            GameEvent gameEvent,
            PendingAutoAbilityQueue queue,
            int beforeCount,
            int afterCount,
            bool stateWasUpdated)
        {
            return new AbilityTriggerGameStateAdapterResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                source_event_id = gameEvent == null ? string.Empty : gameEvent.event_id ?? string.Empty,
                timing_event = AbilityTriggerEventCollector.GetTimingEvent(gameEvent),
                before_count = beforeCount,
                after_count = afterCount,
                added_count = afterCount > beforeCount ? afterCount - beforeCount : 0,
                state_was_updated = stateWasUpdated,
                pending_queue = queue
            };
        }

        public static AbilityTriggerGameStateAdapterResult Rejected(string rejectionReason)
        {
            return new AbilityTriggerGameStateAdapterResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                source_event_id = string.Empty,
                timing_event = string.Empty,
                before_count = 0,
                after_count = 0,
                added_count = 0,
                state_was_updated = false,
                pending_queue = null
            };
        }
    }

    public static class AbilityTriggerGameStateAdapter
    {
        public static PendingAutoAbilityQueue CollectPendingQueue(
            GameState state,
            GameEvent gameEvent,
            IReadOnlyList<AbilityTriggerRegistration> registrations)
        {
            PendingAutoAbilityQueue sourceQueue = state == null
                ? null
                : state.pending_auto_abilities;

            return AbilityTriggerEventCollector.Collect(
                gameEvent,
                registrations,
                sourceQueue);
        }

        public static AbilityTriggerGameStateAdapterResult CommitPendingQueueFromTimingEvent(
            GameState state,
            GameEvent gameEvent,
            IReadOnlyList<AbilityTriggerRegistration> registrations)
        {
            if (state == null)
            {
                return AbilityTriggerGameStateAdapterResult.Rejected(
                    AbilityTriggerGameStateAdapterRejectionReasons.MissingState);
            }

            if (gameEvent == null)
            {
                return AbilityTriggerGameStateAdapterResult.Rejected(
                    AbilityTriggerGameStateAdapterRejectionReasons.MissingGameEvent);
            }

            int beforeCount = CountPending(state.pending_auto_abilities);
            PendingAutoAbilityQueue collectedQueue = CollectPendingQueue(
                state,
                gameEvent,
                registrations);
            int afterCount = CountPending(collectedQueue);
            bool shouldUpdateState = afterCount > beforeCount;
            if (shouldUpdateState)
            {
                state.pending_auto_abilities = collectedQueue;
            }

            return AbilityTriggerGameStateAdapterResult.Accepted(
                gameEvent,
                collectedQueue,
                beforeCount,
                afterCount,
                shouldUpdateState);
        }

        private static int CountPending(PendingAutoAbilityQueue queue)
        {
            return queue == null || queue.pending == null ? 0 : queue.pending.Count;
        }
    }
}
