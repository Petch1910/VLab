using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityItemListFormatterTests
    {
        [Test]
        public void NoPayloadsFormatsZeroMessage()
        {
            Assert.AreEqual(
                PendingAutoAbilityItemListFormatter.NoPayloadsMessage,
                PendingAutoAbilityItemListFormatter.FormatLatest(null));
            Assert.AreEqual(
                PendingAutoAbilityItemListFormatter.NoPayloadsMessage,
                PendingAutoAbilityItemListFormatter.FormatLatest(new List<NetworkPendingAutoAbilityQueuePayload>()));
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
                "Pending ability items: invalid (PENDING_AUTO_ABILITY_QUEUE_PAYLOAD_INVALID)",
                PendingAutoAbilityItemListFormatter.FormatLatest(payloads));
        }

        [Test]
        public void EmptyDecodedQueueFormatsEmptyQueue()
        {
            Assert.AreEqual(
                "Pending ability items: 0\nQueue: queue-empty",
                PendingAutoAbilityItemListFormatter.FormatLatest(
                    new List<NetworkPendingAutoAbilityQueuePayload>
                    {
                        CreatePayload(new PendingAutoAbilityQueue { queue_id = "queue-empty" })
                    }));
        }

        [Test]
        public void ItemListIncludesPendingMetadataAndHiddenSource()
        {
            PendingAutoAbilityQueue queue = CreateQueue(2, true);

            string formatted = PendingAutoAbilityItemListFormatter.FormatLatest(
                new List<NetworkPendingAutoAbilityQueuePayload> { CreatePayload(queue) });

            Assert.IsTrue(formatted.Contains("Pending ability items: 2"));
            Assert.IsTrue(formatted.Contains("Queue: queue-1"));
            Assert.IsTrue(formatted.Contains("1. pending-1 player=0 timing=OnDraw source=hidden summary=summary-1"));
            Assert.IsTrue(formatted.Contains("2. pending-2 player=1 timing=OnBattle source=hidden summary=summary-2"));
            Assert.IsFalse(formatted.Contains("CARD-1@"));
        }

        [Test]
        public void VisibleSourceFormatsCardAndInstance()
        {
            PendingAutoAbilityQueue queue = CreateQueue(1, false);

            string formatted = PendingAutoAbilityItemListFormatter.FormatLatest(
                new List<NetworkPendingAutoAbilityQueuePayload> { CreatePayload(queue) });

            Assert.IsTrue(formatted.Contains("source=CARD-1@src-1"));
        }

        [Test]
        public void LongQueueIsCappedWithRemainingCount()
        {
            PendingAutoAbilityQueue queue = CreateQueue(7, true);

            string formatted = PendingAutoAbilityItemListFormatter.FormatLatest(
                new List<NetworkPendingAutoAbilityQueuePayload> { CreatePayload(queue) },
                3);

            Assert.IsTrue(formatted.Contains("Pending ability items: 7"));
            Assert.IsTrue(formatted.Contains("3. pending-3"));
            Assert.IsFalse(formatted.Contains("4. pending-4"));
            Assert.IsTrue(formatted.Contains("... +4 more"));
        }

        [Test]
        public void FormattingDoesNotMutatePayload()
        {
            NetworkPendingAutoAbilityQueuePayload payload = CreatePayload(CreateQueue(1, true));
            string before = payload.ToJson();

            PendingAutoAbilityItemListFormatter.FormatLatest(
                new List<NetworkPendingAutoAbilityQueuePayload> { payload });

            Assert.AreEqual(before, payload.ToJson());
        }

        private static NetworkPendingAutoAbilityQueuePayload CreatePayload(PendingAutoAbilityQueue queue)
        {
            return PendingAutoAbilityQueuePayloadCodec.Encode(
                "ROOM-PENDING",
                0,
                queue,
                GameStateViewPerspective.Spectator);
        }

        private static PendingAutoAbilityQueue CreateQueue(int count, bool hiddenSource)
        {
            var queue = new PendingAutoAbilityQueue
            {
                queue_id = "queue-1",
                pending = new List<PendingAutoAbility>()
            };

            for (int i = 1; i <= count; i++)
            {
                queue.pending.Add(new PendingAutoAbility
                {
                    pending_id = "pending-" + i,
                    source_card_instance_id = "src-" + i,
                    source_card_id = hiddenSource ? GameStateViewFactory.HiddenCardId : "CARD-" + i,
                    player_index = (i - 1) % 2,
                    timing_event = i % 2 == 0 ? "OnBattle" : "OnDraw",
                    summary = "summary-" + i,
                    hides_source_card_identity = hiddenSource
                });
            }

            return queue;
        }
    }
}
