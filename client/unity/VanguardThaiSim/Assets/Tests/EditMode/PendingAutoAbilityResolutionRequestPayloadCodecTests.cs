using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityResolutionRequestPayloadCodecTests
    {
        [Test]
        public void PayloadRoundTripsResolutionRequest()
        {
            PendingAutoAbilityResolutionRequest request = CreateVisibleRequest();

            NetworkPendingAutoAbilityResolutionRequestPayload payload =
                PendingAutoAbilityResolutionRequestPayloadCodec.Encode(
                    "ROOM-1",
                    0,
                    request,
                    GameStateViewPerspective.Player,
                    0);
            NetworkPendingAutoAbilityResolutionRequestPayload roundTripPayload =
                NetworkPendingAutoAbilityResolutionRequestPayload.FromJson(payload.ToJson());

            PendingAutoAbilityResolutionRequest decoded;
            string rejectionReason;
            Assert.IsTrue(
                PendingAutoAbilityResolutionRequestPayloadCodec.TryDecode(
                    roundTripPayload,
                    out decoded,
                    out rejectionReason),
                rejectionReason);
            Assert.AreEqual(MultiplayerProtocol.ProtocolVersion, roundTripPayload.protocol_version);
            Assert.AreEqual("ROOM-1", roundTripPayload.room_id);
            Assert.AreEqual(0, roundTripPayload.sender_player_index);
            Assert.AreEqual(2, roundTripPayload.selected_index);
            Assert.AreEqual("pending-1", roundTripPayload.pending_id);
            Assert.AreEqual(GameStateViewPerspective.Player.ToString(), roundTripPayload.perspective);
            Assert.AreEqual(0, roundTripPayload.viewer_player_index);
            Assert.AreEqual("pending-1", decoded.pending_id);
            Assert.AreEqual("CARD-1", decoded.source_card_id);
            Assert.AreEqual("src-1", decoded.source_card_instance_id);
        }

        [Test]
        public void WrongProtocolVersionIsRejected()
        {
            NetworkPendingAutoAbilityResolutionRequestPayload payload =
                PendingAutoAbilityResolutionRequestPayloadCodec.Encode(
                    "ROOM-1",
                    0,
                    CreateVisibleRequest(),
                    GameStateViewPerspective.Player,
                    0);
            payload.protocol_version = MultiplayerProtocol.ProtocolVersion + 1;

            PendingAutoAbilityResolutionRequest decoded;
            string rejectionReason;
            Assert.IsFalse(PendingAutoAbilityResolutionRequestPayloadCodec.TryDecode(
                payload,
                out decoded,
                out rejectionReason));
            Assert.AreEqual("PROTOCOL_VERSION_MISMATCH", rejectionReason);
            Assert.IsNull(decoded);
        }

        [Test]
        public void EmptyRequestJsonIsRejected()
        {
            var payload = new NetworkPendingAutoAbilityResolutionRequestPayload
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                pending_auto_ability_resolution_request_json = ""
            };

            PendingAutoAbilityResolutionRequest decoded;
            string rejectionReason;
            Assert.IsFalse(PendingAutoAbilityResolutionRequestPayloadCodec.TryDecode(
                payload,
                out decoded,
                out rejectionReason));
            Assert.AreEqual("PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_PAYLOAD_INVALID", rejectionReason);
            Assert.IsNull(decoded);
        }

        [Test]
        public void InvalidRequestJsonIsRejected()
        {
            var payload = new NetworkPendingAutoAbilityResolutionRequestPayload
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                pending_auto_ability_resolution_request_json = "{not-json"
            };

            PendingAutoAbilityResolutionRequest decoded;
            string rejectionReason;
            Assert.IsFalse(PendingAutoAbilityResolutionRequestPayloadCodec.TryDecode(
                payload,
                out decoded,
                out rejectionReason));
            Assert.IsTrue(rejectionReason.StartsWith("PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_PARSE_FAILED"));
            Assert.IsNull(decoded);
        }

        [Test]
        public void EncodingIsDeterministic()
        {
            PendingAutoAbilityResolutionRequest request = CreateVisibleRequest();

            NetworkPendingAutoAbilityResolutionRequestPayload first =
                PendingAutoAbilityResolutionRequestPayloadCodec.Encode(
                    "ROOM-1",
                    0,
                    request,
                    GameStateViewPerspective.Player,
                    0);
            NetworkPendingAutoAbilityResolutionRequestPayload second =
                PendingAutoAbilityResolutionRequestPayloadCodec.Encode(
                    "ROOM-1",
                    0,
                    request,
                    GameStateViewPerspective.Player,
                    0);

            Assert.AreEqual(first.ToJson(), second.ToJson());
        }

        [Test]
        public void HiddenRequestIsSanitizedWithoutMutatingSource()
        {
            PendingAutoAbilityResolutionRequest request = CreateHiddenRequestWithLeakyInstance();
            string before = request.ToJson();

            NetworkPendingAutoAbilityResolutionRequestPayload payload =
                PendingAutoAbilityResolutionRequestPayloadCodec.Encode(
                    "ROOM-1",
                    0,
                    request,
                    GameStateViewPerspective.Spectator);

            PendingAutoAbilityResolutionRequest decoded;
            string rejectionReason;
            Assert.IsTrue(
                PendingAutoAbilityResolutionRequestPayloadCodec.TryDecode(payload, out decoded, out rejectionReason),
                rejectionReason);
            Assert.IsTrue(decoded.hides_source_card_identity);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, decoded.source_card_id);
            Assert.AreEqual(string.Empty, decoded.source_card_instance_id);
            Assert.IsFalse(payload.ToJson().Contains("secret-source-instance"));
            Assert.AreEqual(before, request.ToJson());
        }

        private static PendingAutoAbilityResolutionRequest CreateVisibleRequest()
        {
            return new PendingAutoAbilityResolutionRequest
            {
                selected_index = 2,
                pending_id = "pending-1",
                player_index = 0,
                timing_event = "OnDraw",
                source_card_instance_id = "src-1",
                source_card_id = "CARD-1",
                summary = "Resolve CARD-1"
            };
        }

        private static PendingAutoAbilityResolutionRequest CreateHiddenRequestWithLeakyInstance()
        {
            return new PendingAutoAbilityResolutionRequest
            {
                selected_index = 0,
                pending_id = "pending-hidden",
                player_index = 0,
                timing_event = "OnDraw",
                source_card_instance_id = "secret-source-instance",
                source_card_id = GameStateViewFactory.HiddenCardId,
                hides_source_card_identity = true,
                summary = "Resolve hidden"
            };
        }
    }
}
