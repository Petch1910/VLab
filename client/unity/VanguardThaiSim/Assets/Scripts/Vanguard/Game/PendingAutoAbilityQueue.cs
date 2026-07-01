using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class PendingAutoAbility
    {
        public string pending_id;
        public string source_card_instance_id;
        public string source_card_id;
        public int player_index;
        public string timing_event;
        public string summary;
        public bool hides_source_card_identity;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static PendingAutoAbility FromJson(string json)
        {
            PendingAutoAbility ability = JsonUtility.FromJson<PendingAutoAbility>(json);
            if (ability == null)
            {
                throw new ArgumentException("Pending auto ability JSON could not be parsed.", "json");
            }

            return ability;
        }
    }

    [Serializable]
    public sealed class PendingAutoAbilityQueue
    {
        public string queue_id = "pending-auto-ability-queue";
        public List<PendingAutoAbility> pending = new List<PendingAutoAbility>();

        public void EnsureLists()
        {
            if (pending == null) pending = new List<PendingAutoAbility>();
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static PendingAutoAbilityQueue FromJson(string json)
        {
            PendingAutoAbilityQueue queue = JsonUtility.FromJson<PendingAutoAbilityQueue>(json);
            if (queue == null)
            {
                throw new ArgumentException("Pending auto ability queue JSON could not be parsed.", "json");
            }

            queue.EnsureLists();
            return queue;
        }
    }

    public static class PendingAutoAbilityQueueBuilder
    {
        public static PendingAutoAbilityQueue Enqueue(
            PendingAutoAbilityQueue source,
            PendingAutoAbility ability)
        {
            if (ability == null)
            {
                throw new ArgumentNullException("ability");
            }

            PendingAutoAbilityQueue result = CloneQueue(source);
            result.pending.Add(CloneAbility(ability));
            return result;
        }

        public static PendingAutoAbility Peek(PendingAutoAbilityQueue source)
        {
            PendingAutoAbilityQueue safeSource = source ?? new PendingAutoAbilityQueue();
            safeSource.EnsureLists();
            if (safeSource.pending.Count == 0)
            {
                return null;
            }

            return CloneAbility(safeSource.pending[0]);
        }

        public static PendingAutoAbilityQueue Dequeue(
            PendingAutoAbilityQueue source,
            out PendingAutoAbility ability)
        {
            PendingAutoAbilityQueue result = CloneQueue(source);
            if (result.pending.Count == 0)
            {
                ability = null;
                return result;
            }

            ability = CloneAbility(result.pending[0]);
            result.pending.RemoveAt(0);
            return result;
        }

        public static PendingAutoAbilityQueue Clear(PendingAutoAbilityQueue source)
        {
            PendingAutoAbilityQueue result = CloneQueue(source);
            result.pending.Clear();
            return result;
        }

        private static PendingAutoAbilityQueue CloneQueue(PendingAutoAbilityQueue source)
        {
            PendingAutoAbilityQueue safeSource = source ?? new PendingAutoAbilityQueue();
            safeSource.EnsureLists();
            var clone = new PendingAutoAbilityQueue
            {
                queue_id = safeSource.queue_id ?? "pending-auto-ability-queue"
            };

            for (int i = 0; i < safeSource.pending.Count; i++)
            {
                PendingAutoAbility ability = safeSource.pending[i];
                if (ability != null)
                {
                    clone.pending.Add(CloneAbility(ability));
                }
            }

            return clone;
        }

        private static PendingAutoAbility CloneAbility(PendingAutoAbility ability)
        {
            if (ability == null)
            {
                return null;
            }

            return PendingAutoAbility.FromJson(ability.ToJson());
        }
    }
}
