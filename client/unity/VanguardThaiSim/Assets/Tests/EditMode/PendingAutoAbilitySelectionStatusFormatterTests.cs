using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilitySelectionStatusFormatterTests
    {
        [Test]
        public void NullAndNoSelectionStateFormatNone()
        {
            Assert.AreEqual(
                PendingAutoAbilitySelectionStatusFormatter.NullStateMessage,
                PendingAutoAbilitySelectionStatusFormatter.Format(null));
            Assert.AreEqual(
                PendingAutoAbilitySelectionStatusFormatter.NoSelectionMessage,
                PendingAutoAbilitySelectionStatusFormatter.Format(PendingAutoAbilitySelection.Clear()));
        }

        [Test]
        public void RejectedStateFormatsReason()
        {
            PendingAutoAbilitySelectionState state =
                PendingAutoAbilitySelection.Select(new PendingAutoAbilityQueue(), 0);

            Assert.AreEqual(
                "Pending selection rejected: " + PendingAutoAbilitySelection.EmptyQueueReason,
                PendingAutoAbilitySelectionStatusFormatter.Format(state));
        }

        [Test]
        public void AcceptedVisibleSelectionFormatsSource()
        {
            var state = new PendingAutoAbilitySelectionState
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
                    timing_event = "OnDraw"
                }
            };

            Assert.AreEqual(
                "Pending selection: index=2 id=pending-1 player=0 timing=OnDraw source=CARD-1@src-1",
                PendingAutoAbilitySelectionStatusFormatter.Format(state));
        }

        [Test]
        public void AcceptedHiddenSelectionDoesNotLeakSource()
        {
            var state = new PendingAutoAbilitySelectionState
            {
                accepted = true,
                has_selection = true,
                selected_index = 0,
                selected_ability = new PendingAutoAbility
                {
                    pending_id = "pending-auto-hidden|0|OnDraw|0000",
                    source_card_instance_id = "hidden-source",
                    source_card_id = GameStateViewFactory.HiddenCardId,
                    player_index = 0,
                    timing_event = "OnDraw",
                    hides_source_card_identity = true
                }
            };

            string formatted = PendingAutoAbilitySelectionStatusFormatter.Format(state);

            Assert.IsTrue(formatted.Contains("source=hidden"));
            Assert.IsFalse(formatted.Contains("hidden-source"));
            Assert.IsFalse(formatted.Contains(GameStateViewFactory.HiddenCardId + "@"));
        }

        [Test]
        public void FormattingDoesNotMutateSelectedAbility()
        {
            var ability = new PendingAutoAbility
            {
                pending_id = "pending-1",
                source_card_instance_id = "src-1",
                source_card_id = "CARD-1",
                player_index = 0,
                timing_event = "OnDraw"
            };
            string before = ability.ToJson();
            var state = new PendingAutoAbilitySelectionState
            {
                accepted = true,
                has_selection = true,
                selected_index = 0,
                selected_ability = ability
            };

            PendingAutoAbilitySelectionStatusFormatter.Format(state);

            Assert.AreEqual(before, ability.ToJson());
        }
    }
}
