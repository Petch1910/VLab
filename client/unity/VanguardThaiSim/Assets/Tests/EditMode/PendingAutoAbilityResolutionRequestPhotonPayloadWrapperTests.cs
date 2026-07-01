using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityResolutionRequestPhotonPayloadWrapperTests
    {
        [Test]
        public void PhotonWrapperRoundTripsResolutionRequestPayload()
        {
            NetworkPendingAutoAbilityResolutionRequestPayload inner = CreateInnerPayload();

            PhotonRealtimePayload photonPayload =
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityResolutionRequest(inner);
            PhotonRealtimePayload roundTripPhotonPayload =
                PhotonRealtimePayload.FromJson(photonPayload.ToJson());

            NetworkPendingAutoAbilityResolutionRequestPayload roundTripInner;
            string rejectionReason;
            Assert.IsTrue(
                PhotonRealtimePayloadCodec.TryDecodePendingAutoAbilityResolutionRequest(
                    roundTripPhotonPayload,
                    out roundTripInner,
                    out rejectionReason),
                rejectionReason);
            Assert.AreEqual(
                PhotonRealtimePayloadCodec.PendingAutoAbilityResolutionRequestEventCode,
                roundTripPhotonPayload.event_code);
            Assert.AreEqual("0", roundTripPhotonPayload.sender_player_id);
            Assert.AreEqual(inner.payload_id, roundTripInner.payload_id);
            Assert.AreEqual(
                inner.pending_auto_ability_resolution_request_json,
                roundTripInner.pending_auto_ability_resolution_request_json);
        }

        [Test]
        public void WrongPhotonEventCodeIsRejected()
        {
            PhotonRealtimePayload payload =
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityResolutionRequest(CreateInnerPayload());
            payload.event_code = PhotonRealtimePayloadCodec.PendingAutoAbilityQueueEventCode;

            NetworkPendingAutoAbilityResolutionRequestPayload decoded;
            string rejectionReason;
            Assert.IsFalse(PhotonRealtimePayloadCodec.TryDecodePendingAutoAbilityResolutionRequest(
                payload,
                out decoded,
                out rejectionReason));
            Assert.AreEqual("PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_PAYLOAD_INVALID", rejectionReason);
            Assert.IsNull(decoded);
        }

        [Test]
        public void InnerProtocolMismatchIsRejected()
        {
            NetworkPendingAutoAbilityResolutionRequestPayload inner = CreateInnerPayload();
            inner.protocol_version = MultiplayerProtocol.ProtocolVersion + 1;
            PhotonRealtimePayload payload =
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityResolutionRequest(inner);

            NetworkPendingAutoAbilityResolutionRequestPayload decoded;
            string rejectionReason;
            Assert.IsFalse(PhotonRealtimePayloadCodec.TryDecodePendingAutoAbilityResolutionRequest(
                payload,
                out decoded,
                out rejectionReason));
            Assert.AreEqual("PROTOCOL_VERSION_MISMATCH", rejectionReason);
            Assert.IsNull(decoded);
        }

        [Test]
        public void PhotonWrapperJsonIsDeterministic()
        {
            NetworkPendingAutoAbilityResolutionRequestPayload inner = CreateInnerPayload();

            PhotonRealtimePayload first =
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityResolutionRequest(inner);
            PhotonRealtimePayload second =
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityResolutionRequest(inner);

            Assert.AreEqual(first.ToJson(), second.ToJson());
        }

        [Test]
        public void PhotonWrappingDoesNotMutateSourcePayload()
        {
            NetworkPendingAutoAbilityResolutionRequestPayload inner = CreateInnerPayload();
            string before = inner.ToJson();

            PhotonRealtimePayloadCodec.EncodePendingAutoAbilityResolutionRequest(inner);

            Assert.AreEqual(before, inner.ToJson());
        }

        private static NetworkPendingAutoAbilityResolutionRequestPayload CreateInnerPayload()
        {
            return PendingAutoAbilityResolutionRequestPayloadCodec.Encode(
                "ROOM-1",
                0,
                new PendingAutoAbilityResolutionRequest
                {
                    selected_index = 0,
                    pending_id = "pending-1",
                    player_index = 0,
                    timing_event = "OnDraw",
                    source_card_instance_id = "src-1",
                    source_card_id = "CARD-1",
                    summary = "Resolve CARD-1"
                },
                GameStateViewPerspective.Player,
                0);
        }
    }
}
