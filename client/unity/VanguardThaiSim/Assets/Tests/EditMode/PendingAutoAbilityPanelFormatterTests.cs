using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityPanelFormatterTests
    {
        [Test]
        public void NoPayloadsFormatsCompactPanel()
        {
            Assert.AreEqual(
                PendingAutoAbilityPanelFormatter.NoPayloadsMessage,
                PendingAutoAbilityPanelFormatter.Format(null, PendingAutoAbilitySelection.Clear()));
            Assert.AreEqual(
                PendingAutoAbilityPanelFormatter.NoPayloadsMessage,
                PendingAutoAbilityPanelFormatter.Format(
                    new List<NetworkPendingAutoAbilityQueuePayload>(),
                    PendingAutoAbilitySelection.Clear()));
        }

        [Test]
        public void ValidQueueFormatsPanelSummary()
        {
            NetworkPendingAutoAbilityQueuePayload payload = CreatePayload(CreateQueue(false));

            string formatted = PendingAutoAbilityPanelFormatter.Format(
                new List<NetworkPendingAutoAbilityQueuePayload> { payload },
                PendingAutoAbilitySelection.Clear());

            Assert.IsTrue(formatted.Contains("AUTO queue"));
            Assert.IsTrue(formatted.Contains("Payloads: 1"));
            Assert.IsTrue(formatted.Contains("Latest: " + payload.payload_id));
            Assert.IsTrue(formatted.Contains("Queue: queue-1"));
            Assert.IsTrue(formatted.Contains("Pending: 1"));
            Assert.IsTrue(formatted.Contains("Selection: none"));
        }

        [Test]
        public void SelectedHiddenSourceDoesNotLeak()
        {
            PendingAutoAbilityQueue queue = CreateQueue(true);
            PendingAutoAbilitySelectionState selection = PendingAutoAbilitySelection.Select(queue, 0);
            NetworkPendingAutoAbilityQueuePayload payload = CreatePayload(queue);

            string formatted = PendingAutoAbilityPanelFormatter.Format(
                new List<NetworkPendingAutoAbilityQueuePayload> { payload },
                selection);

            Assert.IsTrue(formatted.Contains("Selection: #1 pending-hidden"));
            Assert.IsTrue(formatted.Contains("source hidden"));
            Assert.IsFalse(formatted.Contains("src-1"));
            Assert.IsFalse(formatted.Contains(GameStateViewFactory.HiddenCardId + "@"));
        }

        [Test]
        public void InvalidPayloadFormatsStablePanelMessage()
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
                "AUTO queue\nPayloads: 1\nLatest: invalid (PENDING_AUTO_ABILITY_QUEUE_PAYLOAD_INVALID)\nSelection: none",
                PendingAutoAbilityPanelFormatter.Format(payloads, PendingAutoAbilitySelection.Clear()));
        }

        [Test]
        public void FormattingDoesNotMutatePayloadOrSelection()
        {
            PendingAutoAbilityQueue queue = CreateQueue(false);
            PendingAutoAbilitySelectionState selection = PendingAutoAbilitySelection.Select(queue, 0);
            NetworkPendingAutoAbilityQueuePayload payload = CreatePayload(queue);
            string payloadBefore = payload.ToJson();
            string abilityBefore = selection.selected_ability.ToJson();

            PendingAutoAbilityPanelFormatter.Format(
                new List<NetworkPendingAutoAbilityQueuePayload> { payload },
                selection);

            Assert.AreEqual(payloadBefore, payload.ToJson());
            Assert.AreEqual(abilityBefore, selection.selected_ability.ToJson());
        }

        private static NetworkPendingAutoAbilityQueuePayload CreatePayload(PendingAutoAbilityQueue queue)
        {
            return PendingAutoAbilityQueuePayloadCodec.Encode(
                "ROOM-PANEL",
                0,
                queue,
                GameStateViewPerspective.Spectator);
        }

        private static PendingAutoAbilityQueue CreateQueue(bool hiddenSource)
        {
            return new PendingAutoAbilityQueue
            {
                queue_id = "queue-1",
                pending = new List<PendingAutoAbility>
                {
                    new PendingAutoAbility
                    {
                        pending_id = hiddenSource ? "pending-hidden" : "pending-visible",
                        player_index = 0,
                        timing_event = "OnDraw",
                        source_card_id = hiddenSource ? GameStateViewFactory.HiddenCardId : "CARD-1",
                        source_card_instance_id = "src-1",
                        hides_source_card_identity = hiddenSource,
                        summary = "panel summary"
                    }
                }
            };
        }
    }
}
