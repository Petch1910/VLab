using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionDecisionValidatorTests
    {
        [Test]
        public void ValidPayloadPassesAndReturnsDecodedDecision()
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                CreatePayload(CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, "pending-1"));

            PendingAutoAbilityManualResolutionDecisionValidationResult result =
                PendingAutoAbilityManualResolutionDecisionValidator.Validate(payload);

            Assert.IsTrue(result.accepted);
            Assert.IsNotNull(result.decision);
            Assert.AreEqual(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, result.decision.decision_type);
            Assert.AreEqual("pending-1", result.decision.pending_id);
        }

        [Test]
        public void InvalidPayloadIsRejected()
        {
            PendingAutoAbilityManualResolutionDecisionValidationResult result =
                PendingAutoAbilityManualResolutionDecisionValidator.Validate(
                    new NetworkPendingAutoAbilityManualResolutionDecisionPayload
                    {
                        protocol_version = MultiplayerProtocol.ProtocolVersion,
                        pending_auto_ability_manual_resolution_decision_json = string.Empty
                    });

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(
                "PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_PAYLOAD_INVALID",
                result.rejection_reason);
            Assert.IsNull(result.decision);
        }

        [Test]
        public void UnsupportedDecisionTypeIsRejected()
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                CreatePayload(CreateDecision("AutoResolve", "pending-1"));

            PendingAutoAbilityManualResolutionDecisionValidationResult result =
                PendingAutoAbilityManualResolutionDecisionValidator.Validate(payload);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionFactory.DecisionTypeInvalidReason,
                result.rejection_reason);
        }

        [Test]
        public void MissingPendingIdIsRejected()
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                CreatePayload(CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Skip, ""));

            PendingAutoAbilityManualResolutionDecisionValidationResult result =
                PendingAutoAbilityManualResolutionDecisionValidator.Validate(payload);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionValidator.PendingIdMissingReason,
                result.rejection_reason);
        }

        [Test]
        public void HiddenSourceOutputIsRedacted()
        {
            PendingAutoAbilityManualResolutionDecision decision =
                CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Defer, "pending-hidden");
            decision.hides_source_card_identity = true;
            decision.source_card_instance_id = "private-source";
            decision.source_card_id = "CARD-SECRET";
            var payload = new NetworkPendingAutoAbilityManualResolutionDecisionPayload
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                payload_id = "payload-hidden",
                room_id = "ROOM",
                sender_player_index = 0,
                decision_id = decision.decision_id,
                decision_type = decision.decision_type,
                selected_index = decision.selected_index,
                pending_id = decision.pending_id,
                pending_auto_ability_manual_resolution_decision_json = decision.ToJson(false)
            };

            PendingAutoAbilityManualResolutionDecisionValidationResult result =
                PendingAutoAbilityManualResolutionDecisionValidator.Validate(payload);

            Assert.IsTrue(result.accepted);
            Assert.IsTrue(result.decision.hides_source_card_identity);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, result.decision.source_card_id);
            Assert.AreEqual(string.Empty, result.decision.source_card_instance_id);
            Assert.IsTrue(payload.pending_auto_ability_manual_resolution_decision_json.Contains("private-source"));
        }

        [Test]
        public void ValidationDoesNotMutateSourcePayload()
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                CreatePayload(CreateDecision(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, "pending-1"));
            string before = payload.ToJson(false);

            PendingAutoAbilityManualResolutionDecisionValidator.Validate(payload);

            Assert.AreEqual(before, payload.ToJson(false));
        }

        private static NetworkPendingAutoAbilityManualResolutionDecisionPayload CreatePayload(
            PendingAutoAbilityManualResolutionDecision decision)
        {
            return PendingAutoAbilityManualResolutionDecisionPayloadCodec.Encode(
                "ROOM",
                0,
                decision,
                GameStateViewPerspective.Player,
                0);
        }

        private static PendingAutoAbilityManualResolutionDecision CreateDecision(
            string decisionType,
            string pendingId)
        {
            return new PendingAutoAbilityManualResolutionDecision
            {
                decision_id = "decision-" + pendingId + "-" + decisionType,
                decision_type = decisionType,
                selected_index = 0,
                pending_id = pendingId,
                player_index = 0,
                timing_event = "OnBattle",
                source_card_instance_id = "source-0",
                source_card_id = "CARD-0",
                hides_source_card_identity = false,
                reason = "test",
                summary = "Manual decision"
            };
        }
    }
}
