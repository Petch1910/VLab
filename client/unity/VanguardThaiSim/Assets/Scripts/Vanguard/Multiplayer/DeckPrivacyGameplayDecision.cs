using System;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class DeckPrivacyGameplayDecision
    {
        public bool can_start_gameplay;
        public string rejection_reason;
        public bool requires_client_trust_warning;
        public bool client_trust_warning_acknowledged;
        public string warning_message;

        public static DeckPrivacyGameplayDecision Allow()
        {
            return new DeckPrivacyGameplayDecision
            {
                can_start_gameplay = true
            };
        }

        public static DeckPrivacyGameplayDecision Block(
            string reason,
            bool requiresWarning = false,
            string warningMessage = "",
            bool clientTrustWarningAcknowledged = false)
        {
            return new DeckPrivacyGameplayDecision
            {
                can_start_gameplay = false,
                rejection_reason = string.IsNullOrWhiteSpace(reason) ? "DECK_PRIVACY_GAMEPLAY_BLOCKED" : reason,
                requires_client_trust_warning = requiresWarning,
                client_trust_warning_acknowledged = clientTrustWarningAcknowledged,
                warning_message = warningMessage ?? ""
            };
        }
    }
}
