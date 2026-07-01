using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    public sealed class GameStateNoMutationSnapshot
    {
        public readonly string state_json;
        public readonly int event_log_count;
        public readonly int pending_auto_count;

        internal GameStateNoMutationSnapshot(string stateJson, int eventLogCount, int pendingAutoCount)
        {
            state_json = stateJson ?? string.Empty;
            event_log_count = eventLogCount;
            pending_auto_count = pendingAutoCount;
        }

        public bool Matches(GameState state)
        {
            return state_json == NoMutationSnapshot.SerializeGameState(state) &&
                   event_log_count == NoMutationSnapshot.CountEvents(state) &&
                   pending_auto_count == NoMutationSnapshot.CountPendingAutoAbilities(state);
        }
    }

    public sealed class PendingAutoAbilityQueueNoMutationSnapshot
    {
        public readonly string queue_json;
        public readonly int pending_count;

        internal PendingAutoAbilityQueueNoMutationSnapshot(string queueJson, int pendingCount)
        {
            queue_json = queueJson ?? string.Empty;
            pending_count = pendingCount;
        }

        public bool Matches(PendingAutoAbilityQueue queue)
        {
            return queue_json == NoMutationSnapshot.SerializePendingAutoAbilityQueue(queue) &&
                   pending_count == NoMutationSnapshot.CountPendingAutoAbilities(queue);
        }
    }

    public sealed class CollectionCountNoMutationSnapshot
    {
        public readonly int count;

        internal CollectionCountNoMutationSnapshot(int count)
        {
            this.count = count;
        }

        public bool Matches<T>(IReadOnlyCollection<T> collection)
        {
            return count == (collection == null ? -1 : collection.Count);
        }
    }

    public static class NoMutationSnapshot
    {
        public static GameStateNoMutationSnapshot Capture(GameState state)
        {
            return new GameStateNoMutationSnapshot(
                SerializeGameState(state),
                CountEvents(state),
                CountPendingAutoAbilities(state));
        }

        public static PendingAutoAbilityQueueNoMutationSnapshot Capture(PendingAutoAbilityQueue queue)
        {
            return new PendingAutoAbilityQueueNoMutationSnapshot(
                SerializePendingAutoAbilityQueue(queue),
                CountPendingAutoAbilities(queue));
        }

        public static CollectionCountNoMutationSnapshot CaptureCount<T>(IReadOnlyCollection<T> collection)
        {
            return new CollectionCountNoMutationSnapshot(collection == null ? -1 : collection.Count);
        }

        public static string SerializeGameState(GameState state)
        {
            return state == null ? "<null>" : JsonUtility.ToJson(state, false);
        }

        public static string SerializePendingAutoAbilityQueue(PendingAutoAbilityQueue queue)
        {
            return queue == null ? "<null>" : JsonUtility.ToJson(queue, false);
        }

        public static int CountEvents(GameState state)
        {
            return state == null || state.event_log == null ? -1 : state.event_log.Count;
        }

        public static int CountPendingAutoAbilities(GameState state)
        {
            return state == null || state.pending_auto_abilities == null
                ? -1
                : CountPendingAutoAbilities(state.pending_auto_abilities);
        }

        public static int CountPendingAutoAbilities(PendingAutoAbilityQueue queue)
        {
            return queue == null || queue.pending == null ? -1 : queue.pending.Count;
        }
    }
}
