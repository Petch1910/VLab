using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionDecisionPayloadCodecTests
    {
        [Test]
        public void VisibleDecisionPayloadRoundTrips()
        {
            PendingAutoAbilityManualResolutionDecision decision = CreateVisibleDecision();

            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                PendingAutoAbilityManualResolutionDecisionPayloadCodec.Encode(
                    "ROOM-DECISION",
                    1,
                    decision,
                    GameStateViewPerspective.Player,
                    1);
            PendingAutoAbilityManualResolutionDecision decoded;
            string rejectionReason;

            Assert.IsTrue(
                PendingAutoAbilityManualResolutionDecisionPayloadCodec.TryDecode(
                    payload,
                    out decoded,
                    out rejectionReason),
                rejectionReason);
            Assert.AreEqual(MultiplayerProtocol.ProtocolVersion, payload.protocol_version);
            Assert.AreEqual("ROOM-DECISION", payload.room_id);
            Assert.AreEqual(1, payload.sender_player_index);
            Assert.AreEqual(decision.decision_id, payload.decision_id);
            Assert.AreEqual(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, payload.decision_type);
            Assert.AreEqual(1, payload.selected_index);
            Assert.AreEqual("pending-2", payload.pending_id);
            Assert.AreEqual(GameStateViewPerspective.Player.ToString(), payload.perspective);
            Assert.AreEqual(1, payload.viewer_player_index);
            Assert.AreEqual(decision.decision_id, decoded.decision_id);
            Assert.AreEqual("CARD-2", decoded.source_card_id);
            Assert.AreEqual("src-2", decoded.source_card_instance_id);
        }

        [Test]
        public void ProtocolMismatchIsRejected()
        {
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                PendingAutoAbilityManualResolutionDecisionPayloadCodec.Encode(
                    "ROOM-DECISION",
                    0,
                    CreateVisibleDecision(),
                    GameStateViewPerspective.Player,
                    0);
            payload.protocol_version = -1;

            PendingAutoAbilityManualResolutionDecision decoded;
            string rejectionReason;
            bool accepted = PendingAutoAbilityManualResolutionDecisionPayloadCodec.TryDecode(
                payload,
                out decoded,
                out rejectionReason);

            Assert.IsFalse(accepted);
            Assert.IsNull(decoded);
            Assert.AreEqual("PROTOCOL_VERSION_MISMATCH", rejectionReason);
        }

        [Test]
        public void EmptyPayloadIsRejected()
        {
            PendingAutoAbilityManualResolutionDecision decoded;
            string rejectionReason;
            bool accepted = PendingAutoAbilityManualResolutionDecisionPayloadCodec.TryDecode(
                new NetworkPendingAutoAbilityManualResolutionDecisionPayload
                {
                    protocol_version = MultiplayerProtocol.ProtocolVersion,
                    pending_auto_ability_manual_resolution_decision_json = string.Empty
                },
                out decoded,
                out rejectionReason);

            Assert.IsFalse(accepted);
            Assert.IsNull(decoded);
            Assert.AreEqual(
                "PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_PAYLOAD_INVALID",
                rejectionReason);
        }

        [Test]
        public void EncodingIsDeterministic()
        {
            PendingAutoAbilityManualResolutionDecision decision = CreateVisibleDecision();

            NetworkPendingAutoAbilityManualResolutionDecisionPayload first =
                PendingAutoAbilityManualResolutionDecisionPayloadCodec.Encode(
                    "ROOM-DECISION",
                    0,
                    decision,
                    GameStateViewPerspective.Player,
                    0);
            NetworkPendingAutoAbilityManualResolutionDecisionPayload second =
                PendingAutoAbilityManualResolutionDecisionPayloadCodec.Encode(
                    "ROOM-DECISION",
                    0,
                    decision,
                    GameStateViewPerspective.Player,
                    0);

            Assert.AreEqual(first.ToJson(), second.ToJson());
        }

        [Test]
        public void HiddenSourceIsSanitized()
        {
            PendingAutoAbilityManualResolutionDecision decision = CreateVisibleDecision();
            decision.source_card_instance_id = "hidden-source";
            decision.source_card_id = GameStateViewFactory.HiddenCardId;

            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                PendingAutoAbilityManualResolutionDecisionPayloadCodec.Encode(
                    "ROOM-DECISION",
                    0,
                    decision,
                    GameStateViewPerspective.Spectator);
            PendingAutoAbilityManualResolutionDecision decoded;
            string rejectionReason;

            Assert.IsTrue(
                PendingAutoAbilityManualResolutionDecisionPayloadCodec.TryDecode(
                    payload,
                    out decoded,
                    out rejectionReason),
                rejectionReason);
            Assert.IsTrue(decoded.hides_source_card_identity);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, decoded.source_card_id);
            Assert.AreEqual(string.Empty, decoded.source_card_instance_id);
            Assert.IsFalse(payload.ToJson().Contains("hidden-source"));
            Assert.IsFalse(payload.ToJson().Contains(GameStateViewFactory.HiddenCardId + "@"));
        }

        [Test]
        public void EncodingDoesNotMutateSourceDecision()
        {
            PendingAutoAbilityManualResolutionDecision decision = CreateVisibleDecision();
            string before = decision.ToJson();

            PendingAutoAbilityManualResolutionDecisionPayloadCodec.Encode(
                "ROOM-DECISION",
                0,
                decision,
                GameStateViewPerspective.Player,
                0);

            Assert.AreEqual(before, decision.ToJson());
        }

        private static PendingAutoAbilityManualResolutionDecision CreateVisibleDecision()
        {
            PendingAutoAbilityManualResolutionDecisionResult result =
                PendingAutoAbilityManualResolutionDecisionFactory.Create(
                    new PendingAutoAbilityResolutionRequest
                    {
                        selected_index = 1,
                        pending_id = "pending-2",
                        player_index = 0,
                        timing_event = "OnBattle",
                        source_card_instance_id = "src-2",
                        source_card_id = "CARD-2",
                        summary = "Resolve CARD-2"
                    },
                    PendingAutoAbilityManualResolutionDecisionTypes.Resolve);

            return result.decision;
        }
    }
}
