using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionDecisionPhotonPayloadWrapperTests
    {
        [Test]
        public void PhotonWrapperRoundTripsManualResolutionDecisionPayload()
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload inner = CreateInnerPayload();

            PhotonRealtimePayload photonPayload =
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityManualResolutionDecision(inner);
            PhotonRealtimePayload roundTripPhotonPayload =
                PhotonRealtimePayload.FromJson(photonPayload.ToJson());

            NetworkPendingAutoAbilityManualResolutionDecisionPayload roundTripInner;
            string rejectionReason;
            Assert.IsTrue(
                PhotonRealtimePayloadCodec.TryDecodePendingAutoAbilityManualResolutionDecision(
                    roundTripPhotonPayload,
                    out roundTripInner,
                    out rejectionReason),
                rejectionReason);
            Assert.AreEqual(
                PhotonRealtimePayloadCodec.PendingAutoAbilityManualResolutionDecisionEventCode,
                roundTripPhotonPayload.event_code);
            Assert.AreEqual("0", roundTripPhotonPayload.sender_player_id);
            Assert.AreEqual(inner.payload_id, roundTripInner.payload_id);
            Assert.AreEqual(
                inner.pending_auto_ability_manual_resolution_decision_json,
                roundTripInner.pending_auto_ability_manual_resolution_decision_json);
        }

        [Test]
        public void WrongPhotonEventCodeIsRejected()
        {
            PhotonRealtimePayload payload =
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityManualResolutionDecision(CreateInnerPayload());
            payload.event_code = PhotonRealtimePayloadCodec.PendingAutoAbilityResolutionRequestEventCode;

            NetworkPendingAutoAbilityManualResolutionDecisionPayload decoded;
            string rejectionReason;
            Assert.IsFalse(PhotonRealtimePayloadCodec.TryDecodePendingAutoAbilityManualResolutionDecision(
                payload,
                out decoded,
                out rejectionReason));
            Assert.AreEqual("PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_PAYLOAD_INVALID", rejectionReason);
            Assert.IsNull(decoded);
        }

        [Test]
        public void InnerProtocolMismatchIsRejected()
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload inner = CreateInnerPayload();
            inner.protocol_version = MultiplayerProtocol.ProtocolVersion + 1;
            PhotonRealtimePayload payload =
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityManualResolutionDecision(inner);

            NetworkPendingAutoAbilityManualResolutionDecisionPayload decoded;
            string rejectionReason;
            Assert.IsFalse(PhotonRealtimePayloadCodec.TryDecodePendingAutoAbilityManualResolutionDecision(
                payload,
                out decoded,
                out rejectionReason));
            Assert.AreEqual("PROTOCOL_VERSION_MISMATCH", rejectionReason);
            Assert.IsNull(decoded);
        }

        [Test]
        public void PhotonWrapperJsonIsDeterministic()
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload inner = CreateInnerPayload();

            PhotonRealtimePayload first =
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityManualResolutionDecision(inner);
            PhotonRealtimePayload second =
                PhotonRealtimePayloadCodec.EncodePendingAutoAbilityManualResolutionDecision(inner);

            Assert.AreEqual(first.ToJson(), second.ToJson());
        }

        [Test]
        public void PhotonWrappingDoesNotMutateSourcePayload()
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload inner = CreateInnerPayload();
            string before = inner.ToJson();

            PhotonRealtimePayloadCodec.EncodePendingAutoAbilityManualResolutionDecision(inner);

            Assert.AreEqual(before, inner.ToJson());
        }

        private static NetworkPendingAutoAbilityManualResolutionDecisionPayload CreateInnerPayload()
        {
            PendingAutoAbilityResolutionRequest request =
                new PendingAutoAbilityResolutionRequest
                {
                    selected_index = 0,
                    pending_id = "pending-1",
                    player_index = 0,
                    timing_event = "OnDraw",
                    source_card_instance_id = "src-1",
                    source_card_id = "CARD-1",
                    summary = "Resolve CARD-1"
                };

            PendingAutoAbilityManualResolutionDecisionResult result =
                PendingAutoAbilityManualResolutionDecisionFactory.Create(
                    request,
                    PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                    "manual test resolve");

            return PendingAutoAbilityManualResolutionDecisionPayloadCodec.Encode(
                "ROOM-1",
                0,
                result.decision,
                GameStateViewPerspective.Player,
                0);
        }
    }
}
