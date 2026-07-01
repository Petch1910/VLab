using System;

namespace VanguardThaiSim.Multiplayer
{
    public static class DeckPrivacyGameplayPolicy
    {
        public const string CommitmentClientTrustPolicyRequired = "DECK_COMMITMENT_CLIENT_TRUST_POLICY_REQUIRED";
        public const string OwnerPrivateGameplayPathIncomplete = "OWNER_PRIVATE_GAMEPLAY_PATH_INCOMPLETE";
        public const string ServerHeldValidatorRequired = "SERVER_HELD_VALIDATOR_REQUIRED";
        public const string UnsupportedPrivacyMode = "DECK_PRIVACY_MODE_UNSUPPORTED_FOR_GAMEPLAY";

        public static DeckPrivacyGameplayDecision Evaluate(MultiplayerRoomState room)
        {
            return Evaluate(room, false);
        }

        public static DeckPrivacyGameplayDecision Evaluate(MultiplayerRoomState room, bool clientTrustWarningAcknowledged)
        {
            if (room == null)
            {
                return DeckPrivacyGameplayDecision.Block("ROOM_MISSING");
            }

            room.EnsureLists();
            string mode = string.IsNullOrWhiteSpace(room.deck_privacy_mode)
                ? DeckPrivacyModes.SharedDeckCode
                : room.deck_privacy_mode.Trim();

            if (string.Equals(mode, DeckPrivacyModes.SharedDeckCode, StringComparison.Ordinal))
            {
                return DeckPrivacyGameplayDecision.Allow();
            }

            if (string.Equals(mode, DeckPrivacyModes.DeckCommitment, StringComparison.Ordinal))
            {
                if (clientTrustWarningAcknowledged)
                {
                    return DeckPrivacyGameplayDecision.Block(
                        OwnerPrivateGameplayPathIncomplete,
                        false,
                        "Trusted-client warning acknowledged, but owner-private gameplay still needs public-event state application and reconnect recovery before normal start is allowed.",
                        true);
                }

                return DeckPrivacyGameplayDecision.Block(
                    CommitmentClientTrustPolicyRequired,
                    true,
                    "Commitment-only rooms hide deck lists but remain client-authoritative. This is casual trusted-client mode, not ranked secure server mode. Continue only if both players accept end-of-match reveal verification.");
            }

            if (string.Equals(mode, DeckPrivacyModes.ServerHeldDeck, StringComparison.Ordinal))
            {
                return DeckPrivacyGameplayDecision.Block(
                    ServerHeldValidatorRequired,
                    false,
                    "Server-held deck rooms require an authoritative validator before gameplay can start.");
            }

            return DeckPrivacyGameplayDecision.Block(UnsupportedPrivacyMode);
        }
    }
}
