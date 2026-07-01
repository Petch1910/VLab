using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilitySummaryFormatterTests
    {
        [Test]
        public void NoPayloadsFormatsZeroMessage()
        {
            Assert.AreEqual(
                PendingAutoAbilitySummaryFormatter.NoPayloadsMessage,
                PendingAutoAbilitySummaryFormatter.FormatSummary(null));
            Assert.AreEqual(
                PendingAutoAbilitySummaryFormatter.NoPayloadsMessage,
                PendingAutoAbilitySummaryFormatter.FormatSummary(new List<NetworkPendingAutoAbilityQueuePayload>()));
        }

        [Test]
        public void InvalidLatestPayloadFormatsInvalidMessage()
        {
            var payloads = new List<NetworkPendingAutoAbilityQueuePayload>
            {
                new NetworkPendingAutoAbilityQueuePayload
                {
                    protocol_version = MultiplayerProtocol.ProtocolVersion,
                    pending_auto_ability_queue_json = string.Empty
                }
            };

            Assert.AreEqual(
                "Pending abilities: 1\nLatest: invalid (PENDING_AUTO_ABILITY_QUEUE_PAYLOAD_INVALID)",
                PendingAutoAbilitySummaryFormatter.FormatSummary(payloads));
        }

        [Test]
        public void EmptyDecodedQueueFormatsEmptyMessage()
        {
            Assert.AreEqual(
                "Pending abilities: 1\nLatest: empty",
                PendingAutoAbilitySummaryFormatter.FormatSummary(
                    new List<NetworkPendingAutoAbilityQueuePayload>
                    {
                        CreatePayload(new PendingAutoAbilityQueue { queue_id = "queue-empty" })
                    }));
        }

        [Test]
        public void LatestPayloadAndQueueIdsAreShown()
        {
            NetworkPendingAutoAbilityQueuePayload payload = CreatePayload(CreateQueue());

            Assert.AreEqual(
                "Pending abilities: 1\nLatest: " + payload.payload_id + "\nQueue: queue-1 pending=1",
                PendingAutoAbilitySummaryFormatter.FormatSummary(
                    new List<NetworkPendingAutoAbilityQueuePayload> { payload }));
        }

        private static NetworkPendingAutoAbilityQueuePayload CreatePayload(PendingAutoAbilityQueue queue)
        {
            return PendingAutoAbilityQueuePayloadCodec.Encode(
                "ROOM-PENDING",
                0,
                queue,
                GameStateViewPerspective.Spectator);
        }

        private static PendingAutoAbilityQueue CreateQueue()
        {
            return new PendingAutoAbilityQueue
            {
                queue_id = "queue-1",
                pending = new List<PendingAutoAbility>
                {
                    new PendingAutoAbility
                    {
                        pending_id = "pending-1",
                        source_card_instance_id = "hidden-source",
                        source_card_id = GameStateViewFactory.HiddenCardId,
                        player_index = 0,
                        timing_event = "OnDraw",
                        summary = "Pending"
                    }
                }
            };
        }
    }
}
