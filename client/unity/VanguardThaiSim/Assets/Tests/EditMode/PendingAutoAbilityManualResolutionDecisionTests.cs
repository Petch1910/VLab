using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityManualResolutionDecisionTests
    {
        [Test]
        public void MissingRequestIsRejected()
        {
            PendingAutoAbilityManualResolutionDecisionResult result =
                PendingAutoAbilityManualResolutionDecisionFactory.Create(
                    null,
                    PendingAutoAbilityManualResolutionDecisionTypes.Resolve);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionFactory.RequestMissingReason,
                result.rejection_reason);
            Assert.IsNull(result.decision);
        }

        [Test]
        public void UnsupportedDecisionTypeIsRejected()
        {
            PendingAutoAbilityManualResolutionDecisionResult result =
                PendingAutoAbilityManualResolutionDecisionFactory.Create(
                    CreateVisibleRequest(),
                    "AutoResolveNow");

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionFactory.DecisionTypeInvalidReason,
                result.rejection_reason);
            Assert.IsNull(result.decision);
        }

        [Test]
        public void ResolveCreatesSerializableDecisionEnvelope()
        {
            PendingAutoAbilityManualResolutionDecisionResult result =
                PendingAutoAbilityManualResolutionDecisionFactory.Create(
                    CreateVisibleRequest(),
                    PendingAutoAbilityManualResolutionDecisionTypes.Resolve);
            PendingAutoAbilityManualResolutionDecision roundTrip =
                PendingAutoAbilityManualResolutionDecision.FromJson(result.decision.ToJson());

            Assert.IsTrue(result.accepted);
            Assert.AreEqual(string.Empty, result.rejection_reason);
            Assert.IsTrue(roundTrip.decision_id.Contains("Resolve"));
            Assert.AreEqual(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, roundTrip.decision_type);
            Assert.AreEqual(1, roundTrip.selected_index);
            Assert.AreEqual("pending-2", roundTrip.pending_id);
            Assert.AreEqual(0, roundTrip.player_index);
            Assert.AreEqual("OnBattle", roundTrip.timing_event);
            Assert.AreEqual("src-2", roundTrip.source_card_instance_id);
            Assert.AreEqual("CARD-2", roundTrip.source_card_id);
            Assert.IsFalse(roundTrip.hides_source_card_identity);
            Assert.AreEqual(string.Empty, roundTrip.reason);
            Assert.AreEqual("Resolve CARD-2", roundTrip.summary);
        }

        [Test]
        public void SkipAndDeferCarrySanitizedReason()
        {
            PendingAutoAbilityManualResolutionDecisionResult skip =
                PendingAutoAbilityManualResolutionDecisionFactory.Create(
                    CreateVisibleRequest(),
                    PendingAutoAbilityManualResolutionDecisionTypes.Skip,
                    "  optional\nskip  ");
            PendingAutoAbilityManualResolutionDecisionResult defer =
                PendingAutoAbilityManualResolutionDecisionFactory.Create(
                    CreateVisibleRequest(),
                    PendingAutoAbilityManualResolutionDecisionTypes.Defer,
                    "wait\r\nfor judge");

            Assert.IsTrue(skip.accepted);
            Assert.AreEqual(PendingAutoAbilityManualResolutionDecisionTypes.Skip, skip.decision.decision_type);
            Assert.AreEqual("optional skip", skip.decision.reason);
            Assert.IsTrue(defer.accepted);
            Assert.AreEqual(PendingAutoAbilityManualResolutionDecisionTypes.Defer, defer.decision.decision_type);
            Assert.AreEqual("wait for judge", defer.decision.reason);
        }

        [Test]
        public void HiddenSourceDoesNotLeakSourceIdentity()
        {
            PendingAutoAbilityManualResolutionDecisionResult result =
                PendingAutoAbilityManualResolutionDecisionFactory.Create(
                    CreateHiddenRequest(),
                    PendingAutoAbilityManualResolutionDecisionTypes.Resolve);

            Assert.IsTrue(result.accepted);
            Assert.IsTrue(result.decision.hides_source_card_identity);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, result.decision.source_card_id);
            Assert.AreEqual(string.Empty, result.decision.source_card_instance_id);
            Assert.IsFalse(result.decision.ToJson().Contains("hidden-source"));
            Assert.IsFalse(result.decision.ToJson().Contains(GameStateViewFactory.HiddenCardId + "@"));
        }

        [Test]
        public void DecisionCreationDoesNotMutateRequest()
        {
            PendingAutoAbilityResolutionRequest request = CreateVisibleRequest();
            string before = request.ToJson();

            PendingAutoAbilityManualResolutionDecisionFactory.Create(
                request,
                PendingAutoAbilityManualResolutionDecisionTypes.Resolve);

            Assert.AreEqual(before, request.ToJson());
        }

        [Test]
        public void DecisionCreationDoesNotMutateGameState()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateDeck("p1"),
                CreateDeck("p2"),
                1074);
            string before = state.ToJson();

            PendingAutoAbilityManualResolutionDecisionFactory.Create(
                CreateVisibleRequest(),
                PendingAutoAbilityManualResolutionDecisionTypes.Skip,
                "manual test");

            Assert.AreEqual(before, state.ToJson());
            Assert.AreEqual(0, state.event_log.Count);
        }

        private static PendingAutoAbilityResolutionRequest CreateVisibleRequest()
        {
            return new PendingAutoAbilityResolutionRequest
            {
                selected_index = 1,
                pending_id = "pending-2",
                player_index = 0,
                timing_event = "OnBattle",
                source_card_instance_id = "src-2",
                source_card_id = "CARD-2",
                summary = "Resolve CARD-2"
            };
        }

        private static PendingAutoAbilityResolutionRequest CreateHiddenRequest()
        {
            return new PendingAutoAbilityResolutionRequest
            {
                selected_index = 0,
                pending_id = "pending-auto-hidden|0|OnDraw|0000",
                player_index = 0,
                timing_event = "OnDraw",
                source_card_instance_id = "hidden-source",
                source_card_id = GameStateViewFactory.HiddenCardId,
                hides_source_card_identity = true,
                summary = "Pending auto ability <hidden-card> OnDraw player=0"
            };
        }

        private static VanguardDeck CreateDeck(string owner)
        {
            var deck = new VanguardDeck { name = owner + " deck" };
            for (int i = 0; i < 50; i++)
            {
                deck.AddCard(DeckZone.Main, "CARD-" + i);
            }

            return deck;
        }
    }
}
