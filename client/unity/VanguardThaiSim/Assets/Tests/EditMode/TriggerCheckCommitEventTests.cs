using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckCommitEventTests
    {
        [Test]
        public void BuildAcceptedTriggerCheckCommitEventRoundTripsOutcomeMetadata()
        {
            TriggerCheckLogEntry entry = CreateEntry();

            TriggerCheckCommitEventBuildResult result = TriggerCheckCommitEventBuilder.Build(entry);
            TriggerCheckCommitEvent roundTrip = TriggerCheckCommitEvent.FromJson(result.commit_event.ToJson(false));

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.NotNull(result.commit_event);
            Assert.AreEqual(TriggerCheckCommitEvent.EventType, roundTrip.event_type);
            Assert.AreEqual(entry.log_entry_id, roundTrip.source_log_entry_id);
            Assert.AreEqual(entry.check_source, roundTrip.check_source);
            Assert.AreEqual(entry.player_index, roundTrip.player_index);
            Assert.AreEqual(entry.check_index, roundTrip.check_index);
            Assert.AreEqual(entry.checked_card_instance_id, roundTrip.checked_card_instance_id);
            Assert.AreEqual(entry.checked_card_id, roundTrip.checked_card_id);
            Assert.AreEqual(entry.trigger_type, roundTrip.trigger_type);
            Assert.AreEqual(entry.accepted, roundTrip.accepted);
            Assert.AreEqual(entry.needs_manual_resolution, roundTrip.needs_manual_resolution);
            Assert.AreEqual(2, roundTrip.modifier_ids.Count);
            Assert.IsTrue(roundTrip.rng_outcome_id.Contains(entry.checked_card_id));
        }

        [Test]
        public void BuildPreservesMaskedTriggerCheckIdentity()
        {
            TriggerCheckReplayLog log = TriggerCheckReplayLogBuilder.Append(null, CreateEntry());
            TriggerCheckReplayLog masked = TriggerCheckReplayLogMasker.CreateView(
                log,
                GameStateViewPerspective.Spectator);
            TriggerCheckLogEntry maskedEntry = masked.entries[0];

            TriggerCheckCommitEventBuildResult result = TriggerCheckCommitEventBuilder.Build(maskedEntry);
            string json = result.commit_event.ToJson(false);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.commit_event.hides_checked_card_identity);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, result.commit_event.checked_card_id);
            Assert.IsTrue(result.commit_event.checked_card_instance_id.Contains("hidden-trigger-check"));
            Assert.IsFalse(json.Contains("CARD-TRIGGER-1"));
            Assert.IsFalse(json.Contains("trigger-card-instance-1"));
            Assert.IsFalse(json.Contains("modifier-secret"));
            Assert.IsTrue(json.Contains("hidden-trigger-modifier"));
        }

        [Test]
        public void BuildRejectsMissingOrInvalidTriggerCheckMetadata()
        {
            TriggerCheckCommitEventBuildResult missing = TriggerCheckCommitEventBuilder.Build(null);
            TriggerCheckCommitEventBuildResult invalidPlayer =
                TriggerCheckCommitEventBuilder.Build(
                    new TriggerCheckLogEntry
                    {
                        player_index = -1,
                        checked_card_id = "CARD-TRIGGER-1"
                    });
            TriggerCheckCommitEventBuildResult missingCard =
                TriggerCheckCommitEventBuilder.Build(
                    new TriggerCheckLogEntry
                    {
                        player_index = 0,
                        checked_card_id = string.Empty
                    });

            Assert.IsFalse(missing.accepted);
            Assert.AreEqual(TriggerCheckCommitEventBuildRejectionReasons.LogEntryMissing, missing.rejection_reason);
            Assert.IsNull(missing.commit_event);
            Assert.IsFalse(invalidPlayer.accepted);
            Assert.AreEqual(TriggerCheckCommitEventBuildRejectionReasons.PlayerIndexInvalid, invalidPlayer.rejection_reason);
            Assert.IsFalse(missingCard.accepted);
            Assert.AreEqual(TriggerCheckCommitEventBuildRejectionReasons.CheckedCardMissing, missingCard.rejection_reason);
        }

        [Test]
        public void BuildDoesNotMutateTriggerCheckLogEntry()
        {
            TriggerCheckLogEntry entry = CreateEntry();
            string before = entry.ToJson(false);

            TriggerCheckCommitEventBuildResult result = TriggerCheckCommitEventBuilder.Build(entry);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(before, entry.ToJson(false));
        }

        private static TriggerCheckLogEntry CreateEntry()
        {
            return new TriggerCheckLogEntry
            {
                log_entry_id = "trigger-log|Drive|0|0|trigger-card-instance-1|CARD-TRIGGER-1|Critical",
                check_source = TriggerCheckSource.Drive,
                player_index = 0,
                check_index = 0,
                checked_card_instance_id = "trigger-card-instance-1",
                checked_card_id = "CARD-TRIGGER-1",
                trigger_type = TriggerType.Critical,
                accepted = true,
                needs_manual_resolution = false,
                rejection_reason = string.Empty,
                modifier_count = 2,
                modifier_ids = new List<string>
                {
                    "modifier-secret-1",
                    "modifier-secret-2"
                },
                notes = new List<string>
                {
                    "test note"
                },
                summary = "Drive check 0 CARD-TRIGGER-1 Critical accepted; modifiers=2"
            };
        }
    }
}
