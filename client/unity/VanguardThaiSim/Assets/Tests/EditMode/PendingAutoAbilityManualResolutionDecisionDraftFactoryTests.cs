using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionDecisionDraftFactoryTests
    {
        [TestCase(PendingAutoAbilityManualResolutionDecisionTypes.Resolve)]
        [TestCase(PendingAutoAbilityManualResolutionDecisionTypes.Skip)]
        [TestCase(PendingAutoAbilityManualResolutionDecisionTypes.Defer)]
        public void SupportedDecisionTypesCreateNetworkPayload(string decisionType)
        {
            PendingAutoAbilityResolutionRequest request = CreateRequest();

            PendingAutoAbilityManualResolutionDecisionDraftResult result =
                PendingAutoAbilityManualResolutionDecisionDraftFactory.Create(
                    "ROOM-MANUAL",
                    1,
                    request,
                    decisionType,
                    "manual choice",
                    GameStateViewPerspective.Player,
                    1);

            Assert.IsTrue(result.accepted);
            Assert.IsNotNull(result.decision);
            Assert.IsNotNull(result.payload);
            Assert.AreEqual("ROOM-MANUAL", result.payload.room_id);
            Assert.AreEqual(1, result.payload.sender_player_index);
            Assert.AreEqual(decisionType, result.payload.decision_type);
            Assert.AreEqual(request.selected_index, result.payload.selected_index);
            Assert.AreEqual(request.pending_id, result.payload.pending_id);
            Assert.AreEqual(GameStateViewPerspective.Player.ToString(), result.payload.perspective);
            Assert.AreEqual(1, result.payload.viewer_player_index);

            PendingAutoAbilityManualResolutionDecision decoded;
            string rejectionReason;
            Assert.IsTrue(
                PendingAutoAbilityManualResolutionDecisionPayloadCodec.TryDecode(
                    result.payload,
                    out decoded,
                    out rejectionReason));
            Assert.AreEqual(decisionType, decoded.decision_type);
            Assert.AreEqual("manual choice", decoded.reason);
        }

        [Test]
        public void MissingRequestIsRejected()
        {
            PendingAutoAbilityManualResolutionDecisionDraftResult result =
                PendingAutoAbilityManualResolutionDecisionDraftFactory.Create(
                    "ROOM-MANUAL",
                    0,
                    null,
                    PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                    "",
                    GameStateViewPerspective.Player,
                    0);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionFactory.RequestMissingReason,
                result.rejection_reason);
            Assert.IsNull(result.decision);
            Assert.IsNull(result.payload);
        }

        [Test]
        public void InvalidDecisionTypeIsRejected()
        {
            PendingAutoAbilityManualResolutionDecisionDraftResult result =
                PendingAutoAbilityManualResolutionDecisionDraftFactory.Create(
                    "ROOM-MANUAL",
                    0,
                    CreateRequest(),
                    "AutoResolve",
                    "",
                    GameStateViewPerspective.Player,
                    0);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionFactory.DecisionTypeInvalidReason,
                result.rejection_reason);
            Assert.IsNull(result.decision);
            Assert.IsNull(result.payload);
        }

        [Test]
        public void HiddenSourceRequestRemainsRedactedAfterDecode()
        {
            PendingAutoAbilityResolutionRequest request = CreateRequest();
            request.hides_source_card_identity = true;
            request.source_card_instance_id = "private-source";
            request.source_card_id = "CARD-SECRET";

            PendingAutoAbilityManualResolutionDecisionDraftResult result =
                PendingAutoAbilityManualResolutionDecisionDraftFactory.Create(
                    "ROOM-MANUAL",
                    0,
                    request,
                    PendingAutoAbilityManualResolutionDecisionTypes.Defer,
                    "wait",
                    GameStateViewPerspective.Player,
                    0);

            PendingAutoAbilityManualResolutionDecision decoded;
            string rejectionReason;
            Assert.IsTrue(
                PendingAutoAbilityManualResolutionDecisionPayloadCodec.TryDecode(
                    result.payload,
                    out decoded,
                    out rejectionReason));
            Assert.IsTrue(decoded.hides_source_card_identity);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, decoded.source_card_id);
            Assert.AreEqual(string.Empty, decoded.source_card_instance_id);
            Assert.IsFalse(result.payload.pending_auto_ability_manual_resolution_decision_json.Contains("private-source"));
            Assert.IsFalse(result.payload.pending_auto_ability_manual_resolution_decision_json.Contains("CARD-SECRET"));
        }

        [Test]
        public void FactoryDoesNotMutateSourceRequest()
        {
            PendingAutoAbilityResolutionRequest request = CreateRequest();
            string before = request.ToJson(false);

            PendingAutoAbilityManualResolutionDecisionDraftFactory.Create(
                "ROOM-MANUAL",
                0,
                request,
                PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                "resolve now",
                GameStateViewPerspective.Player,
                0);

            Assert.AreEqual(before, request.ToJson(false));
        }

        private static PendingAutoAbilityResolutionRequest CreateRequest()
        {
            return new PendingAutoAbilityResolutionRequest
            {
                selected_index = 2,
                pending_id = "pending-auto|2|OnBattle|abcd",
                player_index = 0,
                timing_event = "OnBattle",
                source_card_instance_id = "source-2",
                source_card_id = "CARD-2",
                hides_source_card_identity = false,
                summary = "Resolve pending AUTO"
            };
        }
    }
}
