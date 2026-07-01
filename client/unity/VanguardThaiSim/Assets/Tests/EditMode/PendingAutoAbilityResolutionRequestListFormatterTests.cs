using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityResolutionRequestListFormatterTests
    {
        [Test]
        public void NoPayloadsFormatsZeroMessage()
        {
            Assert.AreEqual(
                PendingAutoAbilityResolutionRequestListFormatter.NoPayloadsMessage,
                PendingAutoAbilityResolutionRequestListFormatter.Format(null));
            Assert.AreEqual(
                PendingAutoAbilityResolutionRequestListFormatter.NoPayloadsMessage,
                PendingAutoAbilityResolutionRequestListFormatter.Format(
                    new List<NetworkPendingAutoAbilityResolutionRequestPayload>()));
        }

        [Test]
        public void MultiplePayloadsFormatNewestFirstAndBounded()
        {
            var payloads = new List<NetworkPendingAutoAbilityResolutionRequestPayload>
            {
                CreatePayload("pending-1", 0),
                CreatePayload("pending-2", 1),
                CreatePayload("pending-3", 2)
            };

            string formatted = PendingAutoAbilityResolutionRequestListFormatter.Format(payloads, 2);

            Assert.IsTrue(formatted.StartsWith("Pending resolve request list: 3"));
            Assert.Less(formatted.IndexOf("id=pending-3"), formatted.IndexOf("id=pending-2"));
            Assert.IsFalse(formatted.Contains("id=pending-1"));
            Assert.IsTrue(formatted.Contains("... +1 older"));
        }

        [Test]
        public void InvalidPayloadFormatsSafeLine()
        {
            var payloads = new List<NetworkPendingAutoAbilityResolutionRequestPayload>
            {
                new NetworkPendingAutoAbilityResolutionRequestPayload
                {
                    protocol_version = MultiplayerProtocol.ProtocolVersion,
                    payload_id = "payload-invalid",
                    pending_auto_ability_resolution_request_json = string.Empty
                }
            };

            Assert.AreEqual(
                "Pending resolve request list: 1\n1. payload=payload-invalid invalid (PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_PAYLOAD_INVALID)",
                PendingAutoAbilityResolutionRequestListFormatter.Format(payloads));
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

            string formatted = PendingAutoAbilityResolutionRequestListFormatter.Format(
                new List<NetworkPendingAutoAbilityResolutionRequestPayload>
                {
                    PendingAutoAbilityResolutionRequestPayloadCodec.Encode(
                        "ROOM-PENDING",
                        0,
                        request,
                        GameStateViewPerspective.Spectator)
                });

            Assert.IsTrue(formatted.Contains("source=hidden"));
            Assert.IsFalse(formatted.Contains("hidden-source"));
            Assert.IsFalse(formatted.Contains(GameStateViewFactory.HiddenCardId + "@"));
        }

        [Test]
        public void FormattingDoesNotMutatePayload()
        {
            NetworkPendingAutoAbilityResolutionRequestPayload payload = CreatePayload("pending-1", 0);
            string before = payload.ToJson();

            PendingAutoAbilityResolutionRequestListFormatter.Format(
                new List<NetworkPendingAutoAbilityResolutionRequestPayload> { payload });

            Assert.AreEqual(before, payload.ToJson());
        }

        private static NetworkPendingAutoAbilityResolutionRequestPayload CreatePayload(
            string pendingId,
            int selectedIndex)
        {
            return PendingAutoAbilityResolutionRequestPayloadCodec.Encode(
                "ROOM-PENDING",
                0,
                new PendingAutoAbilityResolutionRequest
                {
                    selected_index = selectedIndex,
                    pending_id = pendingId,
                    player_index = 0,
                    timing_event = "OnBattle",
                    source_card_instance_id = "src-" + selectedIndex,
                    source_card_id = "CARD-" + selectedIndex,
                    summary = "Resolve " + pendingId
                },
                GameStateViewPerspective.Player,
                0);
        }
    }
}
