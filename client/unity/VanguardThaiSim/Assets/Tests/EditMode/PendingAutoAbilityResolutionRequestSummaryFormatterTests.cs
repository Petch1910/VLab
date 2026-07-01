using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityResolutionRequestSummaryFormatterTests
    {
        [Test]
        public void NoPayloadsFormatsZeroMessage()
        {
            Assert.AreEqual(
                PendingAutoAbilityResolutionRequestSummaryFormatter.NoPayloadsMessage,
                PendingAutoAbilityResolutionRequestSummaryFormatter.FormatLatest(null));
            Assert.AreEqual(
                PendingAutoAbilityResolutionRequestSummaryFormatter.NoPayloadsMessage,
                PendingAutoAbilityResolutionRequestSummaryFormatter.FormatLatest(
                    new List<NetworkPendingAutoAbilityResolutionRequestPayload>()));
        }

        [Test]
        public void InvalidLatestPayloadFormatsInvalidMessage()
        {
            var payloads = new List<NetworkPendingAutoAbilityResolutionRequestPayload>
            {
                new NetworkPendingAutoAbilityResolutionRequestPayload
                {
                    protocol_version = MultiplayerProtocol.ProtocolVersion,
                    pending_auto_ability_resolution_request_json = string.Empty
                }
            };

            Assert.AreEqual(
                "Pending resolve requests: 1\nLatest: invalid (PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_PAYLOAD_INVALID)",
                PendingAutoAbilityResolutionRequestSummaryFormatter.FormatLatest(payloads));
        }

        [Test]
        public void LatestPayloadAndRequestAreShown()
        {
            NetworkPendingAutoAbilityResolutionRequestPayload payload = CreatePayload(CreateVisibleRequest());

            Assert.AreEqual(
                "Pending resolve requests: 1\nLatest: " + payload.payload_id +
                "\nPending resolve request: index=1 id=pending-2 player=0 timing=OnBattle source=CARD-2@src-2",
                PendingAutoAbilityResolutionRequestSummaryFormatter.FormatLatest(
                    new List<NetworkPendingAutoAbilityResolutionRequestPayload> { payload }));
        }

        [Test]
        public void HiddenRequestDoesNotLeakSource()
        {
            var request = new PendingAutoAbilityResolutionRequest
            {
                selected_index = 0,
                pending_id = "pending-auto-hidden|0|OnDraw|0000",
                player_index = 0,
                timing_event = "OnDraw",
                source_card_instance_id = "hidden-source",
                source_card_id = GameStateViewFactory.HiddenCardId,
                hides_source_card_identity = true
            };

            string formatted = PendingAutoAbilityResolutionRequestSummaryFormatter.FormatLatest(
                new List<NetworkPendingAutoAbilityResolutionRequestPayload> { CreatePayload(request) });

            Assert.IsTrue(formatted.Contains("source=hidden"));
            Assert.IsFalse(formatted.Contains("hidden-source"));
            Assert.IsFalse(formatted.Contains(GameStateViewFactory.HiddenCardId + "@"));
        }

        private static NetworkPendingAutoAbilityResolutionRequestPayload CreatePayload(
            PendingAutoAbilityResolutionRequest request)
        {
            return PendingAutoAbilityResolutionRequestPayloadCodec.Encode(
                "ROOM-PENDING",
                0,
                request,
                GameStateViewPerspective.Player,
                0);
        }

        private static PendingAutoAbilityResolutionRequest CreateVisibleRequest()
        {
            return new PendingAutoAbilityResolutionRequest
            {
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
