using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PendingAutoAbilityResolutionRequestFormatterTests
    {
        [Test]
        public void NullResultAndNullRequestFormatFallbacks()
        {
            Assert.AreEqual(
                PendingAutoAbilityResolutionRequestFormatter.NullResultMessage,
                PendingAutoAbilityResolutionRequestFormatter.Format((PendingAutoAbilityResolutionRequestResult)null));
            Assert.AreEqual(
                PendingAutoAbilityResolutionRequestFormatter.NullRequestMessage,
                PendingAutoAbilityResolutionRequestFormatter.Format((PendingAutoAbilityResolutionRequest)null));
        }

        [Test]
        public void RejectedResultFormatsReason()
        {
            PendingAutoAbilityResolutionRequestResult result =
                PendingAutoAbilityResolutionRequestFactory.Create(PendingAutoAbilitySelection.Clear());

            Assert.AreEqual(
                "Pending resolve request rejected: " +
                PendingAutoAbilityResolutionRequestFactory.SelectionMissingReason,
                PendingAutoAbilityResolutionRequestFormatter.Format(result));
        }

        [Test]
        public void VisibleRequestFormatsSource()
        {
            var request = new PendingAutoAbilityResolutionRequest
            {
                selected_index = 2,
                pending_id = "pending-1",
                player_index = 0,
                timing_event = "OnDraw",
                source_card_instance_id = "src-1",
                source_card_id = "CARD-1",
                summary = "Resolve CARD-1"
            };

            Assert.AreEqual(
                "Pending resolve request: index=2 id=pending-1 player=0 timing=OnDraw source=CARD-1@src-1",
                PendingAutoAbilityResolutionRequestFormatter.Format(request));
        }

        [Test]
        public void HiddenRequestDoesNotLeakSource()
        {
            var request = new PendingAutoAbilityResolutionRequest
            {
                selected_index = 0,
                pending_id = "pending-auto-hidden|0|OnDraw|0000",
                player_index = 0,
                timing_event = "OnDraw",
                source_card_instance_id = "hidden-source",
                source_card_id = GameStateViewFactory.HiddenCardId,
                hides_source_card_identity = true
            };

            string formatted = PendingAutoAbilityResolutionRequestFormatter.Format(request);

            Assert.IsTrue(formatted.Contains("source=hidden"));
            Assert.IsFalse(formatted.Contains("hidden-source"));
            Assert.IsFalse(formatted.Contains(GameStateViewFactory.HiddenCardId + "@"));
        }

        [Test]
        public void FormattingDoesNotMutateRequest()
        {
            var request = new PendingAutoAbilityResolutionRequest
            {
                selected_index = 0,
                pending_id = "pending-1",
                player_index = 0,
                timing_event = "OnDraw",
                source_card_instance_id = "src-1",
                source_card_id = "CARD-1",
                summary = "Resolve CARD-1"
            };
            string before = request.ToJson();

            PendingAutoAbilityResolutionRequestFormatter.Format(request);

            Assert.AreEqual(before, request.ToJson());
        }
    }
}
