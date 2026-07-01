using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityQueuePhotonPayloadWrapperTests
    {
        [Test]
        public void PhotonWrapperRoundTripsPendingQueuePayload()
        {
            NetworkPendingAutoAbilityQueuePayload inner = CreateInnerPayload();

            PhotonRealtimePayload photonPayload =
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityQueue(inner);
            PhotonRealtimePayload roundTripPhotonPayload =
                PhotonRealtimePayload.FromJson(photonPayload.ToJson());

            NetworkPendingAutoAbilityQueuePayload roundTripInner;
            string rejectionReason;
            Assert.IsTrue(
                PhotonRealtimePayloadCodec.TryDecodePendingAutoAbilityQueue(
                    roundTripPhotonPayload,
                    out roundTripInner,
                    out rejectionReason),
                rejectionReason);
            Assert.AreEqual(PhotonRealtimePayloadCodec.PendingAutoAbilityQueueEventCode, roundTripPhotonPayload.event_code);
            Assert.AreEqual("0", roundTripPhotonPayload.sender_player_id);
            Assert.AreEqual(inner.payload_id, roundTripInner.payload_id);
            Assert.AreEqual(inner.pending_auto_ability_queue_json, roundTripInner.pending_auto_ability_queue_json);
        }

        [Test]
        public void WrongPhotonEventCodeIsRejected()
        {
            PhotonRealtimePayload payload =
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityQueue(CreateInnerPayload());
            payload.event_code = PhotonRealtimePayloadCodec.TriggerCheckReplayLogEventCode;

            NetworkPendingAutoAbilityQueuePayload decoded;
            string rejectionReason;
            Assert.IsFalse(PhotonRealtimePayloadCodec.TryDecodePendingAutoAbilityQueue(
                payload,
                out decoded,
                out rejectionReason));
            Assert.AreEqual("PENDING_AUTO_ABILITY_QUEUE_PAYLOAD_INVALID", rejectionReason);
            Assert.IsNull(decoded);
        }

        [Test]
        public void InnerProtocolMismatchIsRejected()
        {
            NetworkPendingAutoAbilityQueuePayload inner = CreateInnerPayload();
            inner.protocol_version = MultiplayerProtocol.ProtocolVersion + 1;
            PhotonRealtimePayload payload =
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityQueue(inner);

            NetworkPendingAutoAbilityQueuePayload decoded;
            string rejectionReason;
            Assert.IsFalse(PhotonRealtimePayloadCodec.TryDecodePendingAutoAbilityQueue(
                payload,
                out decoded,
                out rejectionReason));
            Assert.AreEqual("PROTOCOL_VERSION_MISMATCH", rejectionReason);
            Assert.IsNull(decoded);
        }

        [Test]
        public void PhotonWrapperJsonIsDeterministic()
        {
            NetworkPendingAutoAbilityQueuePayload inner = CreateInnerPayload();

            PhotonRealtimePayload first =
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityQueue(inner);
            PhotonRealtimePayload second =
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityQueue(inner);

            Assert.AreEqual(first.ToJson(), second.ToJson());
        }

        [Test]
        public void PhotonWrappingDoesNotMutateSourcePayload()
        {
            NetworkPendingAutoAbilityQueuePayload inner = CreateInnerPayload();
            string before = inner.ToJson();

            PhotonRealtimePayloadCodec.EncodePendingAutoAbilityQueue(inner);

            Assert.AreEqual(before, inner.ToJson());
        }

        private static NetworkPendingAutoAbilityQueuePayload CreateInnerPayload()
        {
            return PendingAutoAbilityQueuePayloadCodec.Encode(
                "ROOM-1",
                0,
                CreateMaskedQueue(),
                GameStateViewPerspective.Spectator);
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
