using System;
using System.Collections.Generic;

namespace VanguardThaiSim.Game
{
    [Serializable]
    public sealed class AbilityTriggerRegistration
    {
        public string registration_id;
        public string source_card_instance_id;
        public string source_card_id;
        public int player_index;
        public string timing_event;
        public string summary;
    }

    public static class AbilityTriggerEventCollector
    {
        public static PendingAutoAbilityQueue Collect(
            GameEvent gameEvent,
            IReadOnlyList<AbilityTriggerRegistration> registrations,
            PendingAutoAbilityQueue sourceQueue = null)
        {
            PendingAutoAbilityQueue result = CloneQueue(sourceQueue);
            if (gameEvent == null || registrations == null)
            {
                return result;
            }

            string timingEvent = GetTimingEvent(gameEvent);
            for (int i = 0; i < registrations.Count; i++)
            {
                AbilityTriggerRegistration registration = registrations[i];
                if (registration == null)
                {
                    continue;
                }

                if (!string.Equals(registration.timing_event, timingEvent, StringComparison.Ordinal))
                {
                    continue;
                }

                result.pending.Add(CreatePendingAbility(gameEvent, registration, timingEvent, i));
            }

            return result;
        }

        public static string GetTimingEvent(GameEvent gameEvent)
        {
            if (gameEvent == null)
            {
                return string.Empty;
            }

            switch (gameEvent.action_type)
            {
                case GameActionType.Draw:
                    return "OnDraw";
                case GameActionType.MoveCard:
                    return "OnMoveCard";
                case GameActionType.SetPhase:
                    return "OnSetPhase";
                case GameActionType.AddGiftMarker:
                    return "OnAddGiftMarker";
                case GameActionType.ResourceFlip:
                    return "OnResourceFlip";
                default:
                    return "On" + gameEvent.action_type;
            }
        }

        private static PendingAutoAbility CreatePendingAbility(
            GameEvent gameEvent,
            AbilityTriggerRegistration registration,
            string timingEvent,
            int registrationIndex)
        {
            return new PendingAutoAbility
            {
                pending_id = BuildPendingId(gameEvent, registration, timingEvent, registrationIndex),
                source_card_instance_id = registration.source_card_instance_id ?? string.Empty,
                source_card_id = registration.source_card_id ?? string.Empty,
                player_index = registration.player_index,
                timing_event = timingEvent,
                summary = registration.summary ?? string.Empty
            };
        }

        private static string BuildPendingId(
            GameEvent gameEvent,
            AbilityTriggerRegistration registration,
            string timingEvent,
            int registrationIndex)
        {
            return string.Join("|", new[]
            {
                "pending-auto",
                Sanitize(registration.registration_id),
                Sanitize(timingEvent),
                Sanitize(gameEvent.event_id),
                Sanitize(gameEvent.action_type.ToString()),
                Sanitize(gameEvent.card_instance_id),
                registration.player_index.ToString(),
                registrationIndex.ToString()
            });
        }

        private static PendingAutoAbilityQueue CloneQueue(PendingAutoAbilityQueue source)
        {
            var clone = new PendingAutoAbilityQueue
            {
                queue_id = source == null || string.IsNullOrEmpty(source.queue_id)
                    ? "pending-auto-ability-queue"
                    : source.queue_id,
                pending = new List<PendingAutoAbility>()
            };

            if (source == null || source.pending == null)
            {
                return clone;
            }

            for (int i = 0; i < source.pending.Count; i++)
            {
                PendingAutoAbility ability = source.pending[i];
                if (ability == null)
                {
                    continue;
                }

                clone.pending.Add(new PendingAutoAbility
                {
                    pending_id = ability.pending_id,
                    source_card_instance_id = ability.source_card_instance_id,
                    source_card_id = ability.source_card_id,
                    player_index = ability.player_index,
                    timing_event = ability.timing_event,
                    summary = ability.summary
                });
            }

            return clone;
        }

        private static string Sanitize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "none";
            }

            return value.Replace("|", "_");
        }
    }
}
