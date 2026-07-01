using VanguardThaiSim.Game;

namespace VanguardThaiSim.UI
{
    public static class PendingAutoAbilityResolutionRequestFormatter
    {
        public const string NullResultMessage = "Pending resolve request: no result";
        public const string NullRequestMessage = "Pending resolve request: none";

        public static string Format(PendingAutoAbilityResolutionRequestResult result)
        {
            if (result == null)
            {
                return NullResultMessage;
            }

            if (!result.accepted)
            {
                return "Pending resolve request rejected: " + (result.rejection_reason ?? string.Empty);
            }

            return Format(result.request);
        }

        public static string Format(PendingAutoAbilityResolutionRequest request)
        {
            if (request == null)
            {
                return NullRequestMessage;
            }

            string pendingId = string.IsNullOrWhiteSpace(request.pending_id) ? "none" : request.pending_id;
            string timingEvent = string.IsNullOrWhiteSpace(request.timing_event) ? "none" : request.timing_event;

            return "Pending resolve request: index=" + request.selected_index +
                   " id=" + pendingId +
                   " player=" + request.player_index +
                   " timing=" + timingEvent +
                   " source=" + FormatSource(request);
        }

        private static string FormatSource(PendingAutoAbilityResolutionRequest request)
        {
            if (request.hides_source_card_identity ||
                string.Equals(request.source_card_id, GameStateViewFactory.HiddenCardId, System.StringComparison.Ordinal))
            {
                return "hidden";
            }

            string cardId = string.IsNullOrWhiteSpace(request.source_card_id) ? "none" : request.source_card_id;
            string instanceId =
                string.IsNullOrWhiteSpace(request.source_card_instance_id) ? "none" : request.source_card_instance_id;
            return cardId + "@" + instanceId;
        }
    }
}
