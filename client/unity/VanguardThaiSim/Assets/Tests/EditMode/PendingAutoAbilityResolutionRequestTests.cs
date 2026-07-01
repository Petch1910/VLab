using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityResolutionRequestTests
    {
        [Test]
        public void NullOrNoSelectionIsRejected()
        {
            PendingAutoAbilityResolutionRequestResult nullResult =
                PendingAutoAbilityResolutionRequestFactory.Create(null);
            PendingAutoAbilityResolutionRequestResult clearResult =
                PendingAutoAbilityResolutionRequestFactory.Create(PendingAutoAbilitySelection.Clear());

            Assert.IsFalse(nullResult.accepted);
            Assert.AreEqual(PendingAutoAbilityResolutionRequestFactory.SelectionMissingReason, nullResult.rejection_reason);
            Assert.IsNull(nullResult.request);
            Assert.IsFalse(clearResult.accepted);
            Assert.AreEqual(PendingAutoAbilityResolutionRequestFactory.SelectionMissingReason, clearResult.rejection_reason);
            Assert.IsNull(clearResult.request);
        }

        [Test]
        public void RejectedSelectionIsRejected()
        {
            PendingAutoAbilitySelectionState selection =
                PendingAutoAbilitySelection.Select(new PendingAutoAbilityQueue(), 0);

            PendingAutoAbilityResolutionRequestResult result =
                PendingAutoAbilityResolutionRequestFactory.Create(selection);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(PendingAutoAbilityResolutionRequestFactory.SelectionMissingReason, result.rejection_reason);
            Assert.IsNull(result.request);
        }

        [Test]
        public void VisibleSelectionCreatesSerializableRequest()
        {
            PendingAutoAbilitySelectionState selection = new PendingAutoAbilitySelectionState
            {
                accepted = true,
                has_selection = true,
                selected_index = 2,
                selected_ability = new PendingAutoAbility
                {
                    pending_id = "pending-1",
                    source_card_instance_id = "src-1",
                    source_card_id = "CARD-1",
                    player_index = 0,
                    timing_event = "OnDraw",
                    summary = "Resolve CARD-1"
                }
            };

            PendingAutoAbilityResolutionRequestResult result =
                PendingAutoAbilityResolutionRequestFactory.Create(selection);
            PendingAutoAbilityResolutionRequest roundTrip =
                PendingAutoAbilityResolutionRequest.FromJson(result.request.ToJson());

            Assert.IsTrue(result.accepted);
            Assert.AreEqual(string.Empty, result.rejection_reason);
            Assert.AreEqual(2, roundTrip.selected_index);
            Assert.AreEqual("pending-1", roundTrip.pending_id);
            Assert.AreEqual(0, roundTrip.player_index);
            Assert.AreEqual("OnDraw", roundTrip.timing_event);
            Assert.AreEqual("src-1", roundTrip.source_card_instance_id);
            Assert.AreEqual("CARD-1", roundTrip.source_card_id);
            Assert.IsFalse(roundTrip.hides_source_card_identity);
            Assert.AreEqual("Resolve CARD-1", roundTrip.summary);
        }

        [Test]
        public void HiddenSelectionPreservesVisibilityWithoutLeakingSource()
        {
            PendingAutoAbilitySelectionState selection = new PendingAutoAbilitySelectionState
            {
                accepted = true,
                has_selection = true,
                selected_index = 0,
                selected_ability = new PendingAutoAbility
                {
                    pending_id = "pending-auto-hidden|0|OnDraw|0000",
                    source_card_instance_id = "hidden-pending-auto-source-0000",
                    source_card_id = GameStateViewFactory.HiddenCardId,
                    player_index = 0,
                    timing_event = "OnDraw",
                    summary = "Pending auto ability <hidden-card> OnDraw player=0",
                    hides_source_card_identity = true
                }
            };

            PendingAutoAbilityResolutionRequestResult result =
                PendingAutoAbilityResolutionRequestFactory.Create(selection);

            Assert.IsTrue(result.accepted);
            Assert.IsTrue(result.request.hides_source_card_identity);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, result.request.source_card_id);
            Assert.AreEqual(string.Empty, result.request.source_card_instance_id);
            Assert.IsFalse(result.request.ToJson().Contains("hidden-pending-auto-source-0000"));
        }

        [Test]
        public void RequestCreationDoesNotMutateSelection()
        {
            PendingAutoAbility ability = new PendingAutoAbility
            {
                pending_id = "pending-1",
                source_card_instance_id = "src-1",
                source_card_id = "CARD-1",
                player_index = 0,
                timing_event = "OnDraw",
                summary = "Resolve CARD-1"
            };
            string before = ability.ToJson();
            PendingAutoAbilitySelectionState selection = new PendingAutoAbilitySelectionState
            {
                accepted = true,
                has_selection = true,
                selected_index = 0,
                selected_ability = ability
            };

            PendingAutoAbilityResolutionRequestFactory.Create(selection);

            Assert.AreEqual(before, ability.ToJson());
            Assert.AreSame(ability, selection.selected_ability);
        }
    }
}
