using VanguardThaiSim.Game;

namespace VanguardThaiSim.Multiplayer
{
    public sealed class PendingAutoAbilityManualResolutionDecisionDraftResult
    {
        public bool accepted;
        public string rejection_reason;
        public PendingAutoAbilityManualResolutionDecision decision;
        public NetworkPendingAutoAbilityManualResolutionDecisionPayload payload;
    }

    public static class PendingAutoAbilityManualResolutionDecisionDraftFactory
    {
        public static PendingAutoAbilityManualResolutionDecisionDraftResult Create(
            string roomId,
            int senderPlayerIndex,
            PendingAutoAbilityResolutionRequest request,
            string decisionType,
            string reason,
            GameStateViewPerspective perspective,
            int viewerPlayerIndex = -1)
        {
            PendingAutoAbilityManualResolutionDecisionResult decisionResult =
                PendingAutoAbilityManualResolutionDecisionFactory.Create(
                    request,
                    decisionType,
                    reason);

            if (decisionResult == null || !decisionResult.accepted || decisionResult.decision == null)
            {
                return Reject(decisionResult == null
                    ? PendingAutoAbilityManualResolutionDecisionFactory.RequestMissingReason
                    : decisionResult.rejection_reason);
            }

            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                PendingAutoAbilityManualResolutionDecisionPayloadCodec.Encode(
                    roomId,
                    senderPlayerIndex,
                    decisionResult.decision,
                    perspective,
                    viewerPlayerIndex);

            return new PendingAutoAbilityManualResolutionDecisionDraftResult
            {
                accepted = true,
                rejection_reason = string.Empty,
                decision = decisionResult.decision,
                payload = payload
            };
        }

        private static PendingAutoAbilityManualResolutionDecisionDraftResult Reject(string rejectionReason)
        {
            return new PendingAutoAbilityManualResolutionDecisionDraftResult
            {
                accepted = false,
                rejection_reason = rejectionReason ?? string.Empty,
                decision = null,
                payload = null
            };
        }
    }
}
