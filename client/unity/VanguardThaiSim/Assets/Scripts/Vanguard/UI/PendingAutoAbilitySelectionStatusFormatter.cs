using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class PendingAutoAbilitySelectionStatusFormatter
    {
        public const string NoSelectionMessage = "Pending selection: none";
        public const string NullStateMessage = "Pending selection: none";

        public static string Format(PendingAutoAbilitySelectionState state)
        {
            if (state == null)
            {
                return NullStateMessage;
            }

            if (!state.accepted)
            {
                return "Pending selection rejected: " + (state.rejection_reason ?? string.Empty);
            }

            if (!state.has_selection || state.selected_ability == null)
            {
                return NoSelectionMessage;
            }

            PendingAutoAbility ability = state.selected_ability;
            string pendingId = string.IsNullOrWhiteSpace(ability.pending_id) ? "none" : ability.pending_id;
            string timingEvent = string.IsNullOrWhiteSpace(ability.timing_event) ? "none" : ability.timing_event;

            return "Pending selection: index=" + state.selected_index +
                   " id=" + pendingId +
                   " player=" + ability.player_index +
                   " timing=" + timingEvent +
                   " source=" + FormatSource(ability);
        }

        private static string FormatSource(PendingAutoAbility ability)
        {
            if (ability.hides_source_card_identity ||
                string.Equals(ability.source_card_id, GameStateViewFactory.HiddenCardId, System.StringComparison.Ordinal))
            {
                return "hidden";
            }

            string cardId = string.IsNullOrWhiteSpace(ability.source_card_id) ? "none" : ability.source_card_id;
            string instanceId =
                string.IsNullOrWhiteSpace(ability.source_card_instance_id) ? "none" : ability.source_card_instance_id;
            return cardId + "@" + instanceId;
        }
    }
}
