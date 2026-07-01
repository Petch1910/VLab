using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionDecisionListFormatterTests
    {
        [Test]
        public void NoPayloadsFormatsZeroMessage()
        {
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionListFormatter.NoPayloadsMessage,
                PendingAutoAbilityManualResolutionDecisionListFormatter.Format(null));
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionListFormatter.NoPayloadsMessage,
                PendingAutoAbilityManualResolutionDecisionListFormatter.Format(
                    new List<NetworkPendingAutoAbilityManualResolutionDecisionPayload>()));
        }

        [Test]
        public void NewestPayloadsAreShownFirstAndBounded()
        {
            var payloads = new List<NetworkPendingAutoAbilityManualResolutionDecisionPayload>
            {
                CreatePayload(CreateDecision("pending-1", 0, PendingAutoAbilityManualResolutionDecisionTypes.Resolve)),
                CreatePayload(CreateDecision("pending-2", 1, PendingAutoAbilityManualResolutionDecisionTypes.Skip)),
                CreatePayload(CreateDecision("pending-3", 2, PendingAutoAbilityManualResolutionDecisionTypes.Defer))
            };

            string formatted = PendingAutoAbilityManualResolutionDecisionListFormatter.Format(payloads, 2);

            Assert.IsTrue(formatted.Contains("Pending manual decision list: 3"));
            Assert.Less(formatted.IndexOf("id=pending-3"), formatted.IndexOf("id=pending-2"));
            Assert.IsFalse(formatted.Contains("id=pending-1"));
            Assert.IsTrue(formatted.Contains("... +1 older"));
        }

        [Test]
        public void InvalidPayloadIsReported()
        {
            var payloads = new List<NetworkPendingAutoAbilityManualResolutionDecisionPayload>
            {
                new NetworkPendingAutoAbilityManualResolutionDecisionPayload
                {
                    payload_id = "payload-invalid",
                    protocol_version = MultiplayerProtocol.ProtocolVersion,
                    pending_auto_ability_manual_resolution_decision_json = string.Empty
                }
            };

            Assert.AreEqual(
                "Pending manual decision list: 1\n1. payload=payload-invalid invalid (PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_PAYLOAD_INVALID)",
                PendingAutoAbilityManualResolutionDecisionListFormatter.Format(payloads));
        }

        [Test]
        public void HiddenDecisionDoesNotLeakSource()
        {
            var hiddenDecision = new PendingAutoAbilityManualResolutionDecision
            {
                decision_id = "decision-hidden",
                decision_type = PendingAutoAbilityManualResolutionDecisionTypes.Defer,
                selected_index = 0,
                pending_id = "pending-auto-hidden|0|OnDraw|0000",
                player_index = 0,
                timing_event = "OnDraw",
                source_card_instance_id = "hidden-source",
                source_card_id = GameStateViewFactory.HiddenCardId,
                hides_source_card_identity = true
            };

            string formatted = PendingAutoAbilityManualResolutionDecisionListFormatter.Format(
                new List<NetworkPendingAutoAbilityManualResolutionDecisionPayload>
                {
                    CreatePayload(hiddenDecision)
                });

            Assert.IsTrue(formatted.Contains("type=Defer"));
            Assert.IsTrue(formatted.Contains("source=hidden"));
            Assert.IsFalse(formatted.Contains("hidden-source"));
            Assert.IsFalse(formatted.Contains(GameStateViewFactory.HiddenCardId + "@"));
        }

        [Test]
        public void FormattingDoesNotMutatePayload()
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                CreatePayload(CreateDecision("pending-1", 0, PendingAutoAbilityManualResolutionDecisionTypes.Resolve));
            string before = payload.ToJson();

            PendingAutoAbilityManualResolutionDecisionListFormatter.Format(
                new List<NetworkPendingAutoAbilityManualResolutionDecisionPayload> { payload });

            Assert.AreEqual(before, payload.ToJson());
        }

        private static NetworkPendingAutoAbilityManualResolutionDecisionPayload CreatePayload(
            PendingAutoAbilityManualResolutionDecision decision)
        {
            return PendingAutoAbilityManualResolutionDecisionPayloadCodec.Encode(
                "ROOM-PENDING",
                0,
                decision,
                GameStateViewPerspective.Player,
                0);
        }

        private static PendingAutoAbilityManualResolutionDecision CreateDecision(
            string pendingId,
            int selectedIndex,
            string decisionType)
        {
            return new PendingAutoAbilityManualResolutionDecision
            {
                decision_id = "decision-" + pendingId,
                decision_type = decisionType,
                selected_index = selectedIndex,
                pending_id = pendingId,
                player_index = 0,
                timing_event = "OnBattle",
                source_card_instance_id = "src-" + selectedIndex,
                source_card_id = "CARD-" + selectedIndex,
                summary = "Resolve " + pendingId
            };
        }
    }
}
