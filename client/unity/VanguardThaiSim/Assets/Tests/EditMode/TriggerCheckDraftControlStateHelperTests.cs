using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckDraftControlStateHelperTests
    {
        [Test]
        public void LocalModeDisablesAllControls()
        {
            TriggerCheckDraftControlState state = TriggerCheckDraftControlStateHelper.Evaluate(
                false,
                true,
                true,
                "instance-1",
                "BT01-001TH");

            Assert.IsFalse(state.can_publish_trigger_log);
            Assert.IsFalse(state.can_publish_pending_auto_ability_queue);
            Assert.IsFalse(state.can_cycle_pending_auto_ability_selection);
            Assert.IsFalse(state.can_publish_pending_auto_ability_resolution_request);
            Assert.IsFalse(state.can_clear_pending_auto_ability_selection);
            Assert.IsFalse(state.can_publish_manual_draft);
            Assert.IsFalse(state.can_clear_selection);
        }

        [Test]
        public void OnlineNoSelectionCanOnlyPublishAvailableTriggerLog()
        {
            TriggerCheckDraftControlState state = TriggerCheckDraftControlStateHelper.Evaluate(
                true,
                true,
                false,
                null,
                null);

            Assert.IsTrue(state.can_publish_trigger_log);
            Assert.IsFalse(state.can_publish_pending_auto_ability_queue);
            Assert.IsFalse(state.can_cycle_pending_auto_ability_selection);
            Assert.IsFalse(state.can_publish_pending_auto_ability_resolution_request);
            Assert.IsFalse(state.can_clear_pending_auto_ability_selection);
            Assert.IsFalse(state.can_publish_manual_draft);
            Assert.IsFalse(state.can_clear_selection);
        }

        [Test]
        public void OnlineNoSelectionCanPublishAvailablePendingAutoAbilityQueue()
        {
            TriggerCheckDraftControlState state = TriggerCheckDraftControlStateHelper.Evaluate(
                true,
                false,
                true,
                null,
                null);

            Assert.IsFalse(state.can_publish_trigger_log);
            Assert.IsTrue(state.can_publish_pending_auto_ability_queue);
            Assert.IsTrue(state.can_cycle_pending_auto_ability_selection);
            Assert.IsFalse(state.can_publish_pending_auto_ability_resolution_request);
            Assert.IsFalse(state.can_clear_pending_auto_ability_selection);
            Assert.IsFalse(state.can_publish_manual_draft);
            Assert.IsFalse(state.can_clear_selection);
        }

        [Test]
        public void OnlinePendingAutoSelectionCanBeCleared()
        {
            TriggerCheckDraftControlState state = TriggerCheckDraftControlStateHelper.Evaluate(
                true,
                false,
                true,
                true,
                null,
                null);

            Assert.IsTrue(state.can_cycle_pending_auto_ability_selection);
            Assert.IsTrue(state.can_publish_pending_auto_ability_resolution_request);
            Assert.IsTrue(state.can_clear_pending_auto_ability_selection);
        }

        [Test]
        public void OnlineVisibleSelectionEnablesDraftAndClear()
        {
            TriggerCheckDraftControlState state = TriggerCheckDraftControlStateHelper.Evaluate(
                true,
                false,
                false,
                "instance-1",
                "BT01-001TH");

            Assert.IsFalse(state.can_publish_trigger_log);
            Assert.IsTrue(state.can_publish_manual_draft);
            Assert.IsTrue(state.can_clear_selection);
        }

        [Test]
        public void OnlineHiddenSelectionDisablesDraftButAllowsClear()
        {
            TriggerCheckDraftControlState state = TriggerCheckDraftControlStateHelper.Evaluate(
                true,
                false,
                false,
                "instance-1",
                GameStateViewFactory.HiddenCardId);

            Assert.IsFalse(state.can_publish_manual_draft);
            Assert.IsTrue(state.can_clear_selection);
        }

        [Test]
        public void OnlineMissingCardIdDisablesDraftButAllowsClear()
        {
            TriggerCheckDraftControlState state = TriggerCheckDraftControlStateHelper.Evaluate(
                true,
                false,
                false,
                "instance-1",
                string.Empty);

            Assert.IsFalse(state.can_publish_manual_draft);
            Assert.IsTrue(state.can_clear_selection);
        }
    }
}
