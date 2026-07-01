using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public sealed class TriggerCheckDraftControlState
    {
        public bool can_publish_trigger_log;
        public bool can_publish_pending_auto_ability_queue;
        public bool can_cycle_pending_auto_ability_selection;
        public bool can_publish_pending_auto_ability_resolution_request;
        public bool can_clear_pending_auto_ability_selection;
        public bool can_publish_manual_draft;
        public bool can_clear_selection;
    }

    public static class TriggerCheckDraftControlStateHelper
    {
        public static TriggerCheckDraftControlState Evaluate(
            bool isOnline,
            bool canPublishTriggerCheckReplayLog,
            bool canPublishPendingAutoAbilityQueue,
            string selectedCardInstanceId,
            string selectedCardId)
        {
            bool hasSelectedInstance = !string.IsNullOrEmpty(selectedCardInstanceId);
            bool hasSelectedCard = !string.IsNullOrEmpty(selectedCardId);
            bool hasPendingAutoAbilitySelection = false;

            return new TriggerCheckDraftControlState
            {
                can_publish_trigger_log = isOnline && canPublishTriggerCheckReplayLog,
                can_publish_pending_auto_ability_queue = isOnline && canPublishPendingAutoAbilityQueue,
                can_cycle_pending_auto_ability_selection = isOnline && canPublishPendingAutoAbilityQueue,
                can_publish_pending_auto_ability_resolution_request = false,
                can_clear_pending_auto_ability_selection = isOnline && hasPendingAutoAbilitySelection,
                can_publish_manual_draft =
                    isOnline &&
                    hasSelectedInstance &&
                    hasSelectedCard &&
                    selectedCardId != GameStateViewFactory.HiddenCardId,
                can_clear_selection = isOnline && hasSelectedInstance
            };
        }

        public static TriggerCheckDraftControlState Evaluate(
            bool isOnline,
            bool canPublishTriggerCheckReplayLog,
            bool canPublishPendingAutoAbilityQueue,
            bool hasPendingAutoAbilitySelection,
            string selectedCardInstanceId,
            string selectedCardId)
        {
            TriggerCheckDraftControlState state = Evaluate(
                isOnline,
                canPublishTriggerCheckReplayLog,
                canPublishPendingAutoAbilityQueue,
                selectedCardInstanceId,
                selectedCardId);
            state.can_publish_pending_auto_ability_resolution_request =
                isOnline && hasPendingAutoAbilitySelection;
            state.can_clear_pending_auto_ability_selection = isOnline && hasPendingAutoAbilitySelection;
            return state;
        }
    }
}
