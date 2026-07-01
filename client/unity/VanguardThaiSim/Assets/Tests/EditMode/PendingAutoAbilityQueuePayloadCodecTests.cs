using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityQueuePayloadCodecTests
    {
        [Test]
        public void PayloadRoundTripsMaskedPendingQueue()
        {
            PendingAutoAbilityQueue maskedQueue = CreateMaskedQueue();

            NetworkPendingAutoAbilityQueuePayload payload =
                PendingAutoAbilityQueuePayloadCodec.Encode(
                    "ROOM-1",
                    0,
                    maskedQueue,
                    GameStateViewPerspective.Spectator);
            NetworkPendingAutoAbilityQueuePayload roundTripPayload =
                NetworkPendingAutoAbilityQueuePayload.FromJson(payload.ToJson());

            PendingAutoAbilityQueue decoded;
            string rejectionReason;
            Assert.IsTrue(
                PendingAutoAbilityQueuePayloadCodec.TryDecode(
                    roundTripPayload,
                    out decoded,
                    out rejectionReason),
                rejectionReason);
            Assert.AreEqual(MultiplayerProtocol.ProtocolVersion, roundTripPayload.protocol_version);
            Assert.AreEqual("ROOM-1", roundTripPayload.room_id);
            Assert.AreEqual(0, roundTripPayload.sender_player_index);
            Assert.AreEqual("queue-1", roundTripPayload.source_queue_id);
            Assert.AreEqual(1, roundTripPayload.pending_count);
            Assert.AreEqual(GameStateViewPerspective.Spectator.ToString(), roundTripPayload.perspective);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, decoded.pending[0].source_card_id);
            Assert.IsTrue(decoded.pending[0].hides_source_card_identity);
        }

        [Test]
        public void WrongProtocolVersionIsRejected()
        {
            NetworkPendingAutoAbilityQueuePayload payload =
                PendingAutoAbilityQueuePayloadCodec.Encode(
                    "ROOM-1",
                    0,
                    CreateMaskedQueue(),
                    GameStateViewPerspective.Spectator);
            payload.protocol_version = MultiplayerProtocol.ProtocolVersion + 1;

            PendingAutoAbilityQueue decoded;
            string rejectionReason;
            Assert.IsFalse(PendingAutoAbilityQueuePayloadCodec.TryDecode(
                payload,
                out decoded,
                out rejectionReason));
            Assert.AreEqual("PROTOCOL_VERSION_MISMATCH", rejectionReason);
            Assert.IsNull(decoded);
        }

        [Test]
        public void EmptyQueueJsonIsRejected()
        {
            var payload = new NetworkPendingAutoAbilityQueuePayload
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                pending_auto_ability_queue_json = ""
            };

            PendingAutoAbilityQueue decoded;
            string rejectionReason;
            Assert.IsFalse(PendingAutoAbilityQueuePayloadCodec.TryDecode(
                payload,
                out decoded,
                out rejectionReason));
            Assert.AreEqual("PENDING_AUTO_ABILITY_QUEUE_PAYLOAD_INVALID", rejectionReason);
            Assert.IsNull(decoded);
        }

        [Test]
        public void InvalidQueueJsonIsRejected()
        {
            var payload = new NetworkPendingAutoAbilityQueuePayload
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                pending_auto_ability_queue_json = "{not-json"
            };

            PendingAutoAbilityQueue decoded;
            string rejectionReason;
            Assert.IsFalse(PendingAutoAbilityQueuePayloadCodec.TryDecode(
                payload,
                out decoded,
                out rejectionReason));
            Assert.IsTrue(rejectionReason.StartsWith("PENDING_AUTO_ABILITY_QUEUE_PARSE_FAILED"));
            Assert.IsNull(decoded);
        }

        [Test]
        public void EncodingIsDeterministic()
        {
            PendingAutoAbilityQueue maskedQueue = CreateMaskedQueue();

            NetworkPendingAutoAbilityQueuePayload first =
                PendingAutoAbilityQueuePayloadCodec.Encode(
                    "ROOM-1",
                    0,
                    maskedQueue,
                    GameStateViewPerspective.Spectator);
            NetworkPendingAutoAbilityQueuePayload second =
                PendingAutoAbilityQueuePayloadCodec.Encode(
                    "ROOM-1",
                    0,
                    maskedQueue,
                    GameStateViewPerspective.Spectator);

            Assert.AreEqual(first.ToJson(), second.ToJson());
        }

        [Test]
        public void EncodingDoesNotMutateSourceQueue()
        {
            PendingAutoAbilityQueue maskedQueue = CreateMaskedQueue();
            string before = maskedQueue.ToJson();

            PendingAutoAbilityQueuePayloadCodec.Encode(
                "ROOM-1",
                0,
                maskedQueue,
                GameStateViewPerspective.Spectator);

            Assert.AreEqual(before, maskedQueue.ToJson());
        }

        [Test]
        public void EncodingNullPendingListDoesNotMutateSourceQueue()
        {
            var queue = new PendingAutoAbilityQueue
            {
                queue_id = "queue-null",
                pending = null
            };

            NetworkPendingAutoAbilityQueuePayload payload =
                PendingAutoAbilityQueuePayloadCodec.Encode(
                    "ROOM-1",
                    0,
                    queue,
                    GameStateViewPerspective.Spectator);

            Assert.IsNull(queue.pending);
            Assert.AreEqual(0, payload.pending_count);
        }

        private static PendingAutoAbilityQueue CreateMaskedQueue()
        {
            var queue = new PendingAutoAbilityQueue
            {
                queue_id = "queue-1",
                pending = new List<PendingAutoAbility>
                {
                    new PendingAutoAbility
                    {
                        pending_id = "pending-1",
                        source_card_instance_id = "src-1",
                        source_card_id = "CARD-1",
                        player_index = 0,
                        timing_event = "OnDraw",
                        summary = "CARD-1 ability"
                    }
                }
            };

            return PendingAutoAbilityQueueMasker.CreateView(
                queue,
                GameStateViewPerspective.Spectator);
        }
    }
}
