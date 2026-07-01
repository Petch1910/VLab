using System.Collections.Generic;

namespace VanguardThaiSim.Game
{
    public static class PendingAutoAbilityQueueMasker
    {
        public static PendingAutoAbilityQueue CreateView(
            PendingAutoAbilityQueue source,
            GameStateViewPerspective perspective,
            int viewerPlayerIndex = -1,
            bool revealOwnerAbilities = true)
        {
            PendingAutoAbilityQueue view = CloneQueue(source);
            if (perspective == GameStateViewPerspective.TrueState)
            {
                return view;
            }

            for (int i = 0; i < view.pending.Count; i++)
            {
                PendingAutoAbility ability = view.pending[i];
                if (ability == null)
                {
                    continue;
                }

                bool canSeeIdentity = perspective == GameStateViewPerspective.Player &&
                                      revealOwnerAbilities &&
                                      ability.player_index == viewerPlayerIndex;
                if (!canSeeIdentity)
                {
                    MaskAbility(ability, i);
                }
            }

            return view;
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
                    summary = ability.summary,
                    hides_source_card_identity = ability.hides_source_card_identity
                });
            }

            return clone;
        }

        private static void MaskAbility(PendingAutoAbility ability, int abilityIndex)
        {
            ability.hides_source_card_identity = true;
            ability.source_card_instance_id =
                "hidden-pending-auto-source-" + abilityIndex.ToString("D4");
            ability.source_card_id = GameStateViewFactory.HiddenCardId;
            ability.pending_id = BuildMaskedPendingId(ability, abilityIndex);
            ability.summary = BuildMaskedSummary(ability);
        }

        private static string BuildMaskedPendingId(PendingAutoAbility ability, int abilityIndex)
        {
            return "pending-auto-hidden|" +
                   ability.player_index +
                   "|" + (ability.timing_event ?? string.Empty) +
                   "|" + abilityIndex.ToString("D4");
        }

        private static string BuildMaskedSummary(PendingAutoAbility ability)
        {
            return "Pending auto ability " +
                   GameStateViewFactory.HiddenCardId +
                   " " +
                   (ability.timing_event ?? string.Empty) +
                   " player=" +
                   ability.player_index;
        }
    }
}
