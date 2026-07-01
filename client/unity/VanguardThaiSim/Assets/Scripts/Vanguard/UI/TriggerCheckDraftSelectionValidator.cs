using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public sealed class TriggerCheckDraftSelectionValidation
    {
        public bool accepted;
        public string rejection_reason;
    }

    public static class TriggerCheckDraftSelectionValidator
    {
        public const string OnlineOnlyMessage = "Trigger draft publish is only available online.";
        public const string SelectCardMessage = "Select a card before drafting a trigger check.";
        public const string HiddenCardMessage = "Selected card identity is hidden.";

        public static TriggerCheckDraftSelectionValidation Validate(
            bool isOnline,
            string selectedCardInstanceId,
            string selectedCardId)
        {
            if (!isOnline)
            {
                return Reject(OnlineOnlyMessage);
            }

            if (string.IsNullOrEmpty(selectedCardInstanceId) || string.IsNullOrEmpty(selectedCardId))
            {
                return Reject(SelectCardMessage);
            }

            if (selectedCardId == GameStateViewFactory.HiddenCardId)
            {
                return Reject(HiddenCardMessage);
            }

            return new TriggerCheckDraftSelectionValidation
            {
                accepted = true,
                rejection_reason = string.Empty
            };
        }

        private static TriggerCheckDraftSelectionValidation Reject(string reason)
        {
            return new TriggerCheckDraftSelectionValidation
            {
                accepted = false,
                rejection_reason = reason
            };
        }
    }
}
