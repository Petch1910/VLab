using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionDecisionSummaryFormatterTests
    {
        [Test]
        public void NoPayloadsFormatsZeroMessage()
        {
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionSummaryFormatter.NoPayloadsMessage,
                PendingAutoAbilityManualResolutionDecisionSummaryFormatter.FormatLatest(null));
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionSummaryFormatter.NoPayloadsMessage,
                PendingAutoAbilityManualResolutionDecisionSummaryFormatter.FormatLatest(
                    new List<NetworkPendingAutoAbilityManualResolutionDecisionPayload>()));
        }

        [Test]
        public void InvalidLatestPayloadFormatsInvalidMessage()
        {
            var payloads = new List<NetworkPendingAutoAbilityManualResolutionDecisionPayload>
            {
                new NetworkPendingAutoAbilityManualResolutionDecisionPayload
                {
                    protocol_version = MultiplayerProtocol.ProtocolVersion,
                    pending_auto_ability_manual_resolution_decision_json = string.Empty
                }
            };

            Assert.AreEqual(
                "Pending manual decisions: 1\nLatest: invalid (PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_PAYLOAD_INVALID)",
                PendingAutoAbilityManualResolutionDecisionSummaryFormatter.FormatLatest(payloads));
        }

        [Test]
        public void LatestPayloadAndDecisionAreShown()
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                CreatePayload(CreateVisibleDecision());

            Assert.AreEqual(
                "Pending manual decisions: 1\nLatest: " + payload.payload_id +
                "\nPending manual decision: type=Resolve index=1 id=pending-2 player=0 timing=OnBattle source=CARD-2@src-2",
                PendingAutoAbilityManualResolutionDecisionSummaryFormatter.FormatLatest(
                    new List<NetworkPendingAutoAbilityManualResolutionDecisionPayload> { payload }));
        }

        [Test]
        public void HiddenDecisionDoesNotLeakSource()
        {
            var decision = new PendingAutoAbilityManualResolutionDecision
            {
                decision_id = "decision-hidden",
                decision_type = PendingAutoAbilityManualResolutionDecisionTypes.Skip,
                selected_index = 0,
                pending_id = "pending-auto-hidden|0|OnDraw|0000",
                player_index = 0,
                timing_event = "OnDraw",
                source_card_instance_id = "hidden-source",
                source_card_id = GameStateViewFactory.HiddenCardId,
                hides_source_card_identity = true
            };

            string formatted = PendingAutoAbilityManualResolutionDecisionSummaryFormatter.FormatLatest(
                new List<NetworkPendingAutoAbilityManualResolutionDecisionPayload> { CreatePayload(decision) });

            Assert.IsTrue(formatted.Contains("type=Skip"));
            Assert.IsTrue(formatted.Contains("source=hidden"));
            Assert.IsFalse(formatted.Contains("hidden-source"));
            Assert.IsFalse(formatted.Contains(GameStateViewFactory.HiddenCardId + "@"));
        }

        [Test]
        public void FormattingDoesNotMutatePayload()
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                CreatePayload(CreateVisibleDecision());
            string before = payload.ToJson();

            PendingAutoAbilityManualResolutionDecisionSummaryFormatter.FormatLatest(
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

        private static PendingAutoAbilityManualResolutionDecision CreateVisibleDecision()
        {
            return new PendingAutoAbilityManualResolutionDecision
            {
                decision_id = "decision-2",
                decision_type = PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                selected_index = 1,
                pending_id = "pending-2",
                player_index = 0,
                timing_event = "OnBattle",
                source_card_instance_id = "src-2",
                source_card_id = "CARD-2",
                summary = "Resolve CARD-2"
            };
        }
    }
}
