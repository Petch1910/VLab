using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionPanelFormatterTests
    {
        [Test]
        public void NoDecisionFormatsCompactPanel()
        {
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionPanelFormatter.NoDecisionsMessage,
                PendingAutoAbilityManualResolutionPanelFormatter.Format(null, null));
        }

        [Test]
        public void ValidDecisionFormatsPanelWithoutSourceInstance()
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                CreatePayload(CreateDecision(false, PendingAutoAbilityManualResolutionDecisionTypes.Resolve));

            string formatted = PendingAutoAbilityManualResolutionPanelFormatter.Format(
                new List<NetworkPendingAutoAbilityManualResolutionDecisionPayload> { payload },
                null);

            Assert.IsTrue(formatted.Contains("Manual resolution"));
            Assert.IsTrue(formatted.Contains("Decisions: 1"));
            Assert.IsTrue(formatted.Contains("Latest: " + payload.payload_id));
            Assert.IsTrue(formatted.Contains("Decision: Resolve #1 pending-1 P0 OnDraw source CARD-1"));
            Assert.IsTrue(formatted.Contains("Validation: valid"));
            Assert.IsTrue(formatted.Contains("Apply preview: none"));
            Assert.IsFalse(formatted.Contains("src-1"));
        }

        [Test]
        public void HiddenDecisionDoesNotLeakSource()
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                CreatePayload(CreateDecision(true, PendingAutoAbilityManualResolutionDecisionTypes.Defer));

            string formatted = PendingAutoAbilityManualResolutionPanelFormatter.Format(
                new List<NetworkPendingAutoAbilityManualResolutionDecisionPayload> { payload },
                null);

            Assert.IsTrue(formatted.Contains("Decision: Defer #1 pending-1 P0 OnDraw source hidden"));
            Assert.IsFalse(formatted.Contains("hidden-source"));
            Assert.IsFalse(formatted.Contains(GameStateViewFactory.HiddenCardId + "@"));
        }

        [Test]
        public void InvalidDecisionPayloadFormatsStableRejection()
        {
            var payloads = new List<NetworkPendingAutoAbilityManualResolutionDecisionPayload>
            {
                new NetworkPendingAutoAbilityManualResolutionDecisionPayload
                {
                    protocol_version = MultiplayerProtocol.ProtocolVersion,
                    pending_auto_ability_manual_resolution_decision_json = string.Empty
                }
            };

            string formatted = PendingAutoAbilityManualResolutionPanelFormatter.Format(payloads, null);

            Assert.IsTrue(formatted.Contains("Decisions: 1"));
            Assert.IsTrue(formatted.Contains(
                "Latest: invalid (PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_PAYLOAD_INVALID)"));
            Assert.IsTrue(formatted.Contains(
                "Validation: rejected PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_PAYLOAD_INVALID"));
        }

        [Test]
        public void ApplyPreviewFormatsWithoutFreeTextSummary()
        {
            PendingAutoAbilityManualResolutionApplyPreviewLogEntry entry =
                PendingAutoAbilityManualResolutionApplyPreviewLogEntry.Accepted(
                    "log-1",
                    "queue-1",
                    "pending-1",
                    PendingAutoAbilityManualResolutionDecisionTypes.Skip,
                    "Secret CARD-SECRET source-instance");

            string formatted = PendingAutoAbilityManualResolutionPanelFormatter.Format(
                new List<NetworkPendingAutoAbilityManualResolutionDecisionPayload>
                {
                    CreatePayload(CreateDecision(false, PendingAutoAbilityManualResolutionDecisionTypes.Skip))
                },
                entry);

            Assert.IsTrue(formatted.Contains("Apply preview: accepted Skip pending-1"));
            Assert.IsFalse(formatted.Contains("CARD-SECRET"));
            Assert.IsFalse(formatted.Contains("source-instance"));
        }

        [Test]
        public void FormattingDoesNotMutatePayloadOrPreviewLog()
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                CreatePayload(CreateDecision(false, PendingAutoAbilityManualResolutionDecisionTypes.Resolve));
            PendingAutoAbilityManualResolutionApplyPreviewLogEntry entry =
                PendingAutoAbilityManualResolutionApplyPreviewLogEntry.Rejected("log-2", "REJECTED");
            string payloadBefore = payload.ToJson();
            string entryBefore = entry.ToJson(false);

            PendingAutoAbilityManualResolutionPanelFormatter.Format(
                new List<NetworkPendingAutoAbilityManualResolutionDecisionPayload> { payload },
                entry);

            Assert.AreEqual(payloadBefore, payload.ToJson());
            Assert.AreEqual(entryBefore, entry.ToJson(false));
        }

        private static NetworkPendingAutoAbilityManualResolutionDecisionPayload CreatePayload(
            PendingAutoAbilityManualResolutionDecision decision)
        {
            return PendingAutoAbilityManualResolutionDecisionPayloadCodec.Encode(
                "ROOM-MANUAL-PANEL",
                0,
                decision,
                GameStateViewPerspective.Player,
                0);
        }

        private static PendingAutoAbilityManualResolutionDecision CreateDecision(
            bool hiddenSource,
            string decisionType)
        {
            return new PendingAutoAbilityManualResolutionDecision
            {
                decision_id = "decision-1",
                decision_type = decisionType,
                selected_index = 0,
                pending_id = "pending-1",
                player_index = 0,
                timing_event = "OnDraw",
                source_card_id = hiddenSource ? GameStateViewFactory.HiddenCardId : "CARD-1",
                source_card_instance_id = hiddenSource ? "hidden-source" : "src-1",
                hides_source_card_identity = hiddenSource,
                summary = "manual panel"
            };
        }
    }
}
