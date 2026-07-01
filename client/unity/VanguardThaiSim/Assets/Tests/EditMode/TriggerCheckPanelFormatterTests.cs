using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckPanelFormatterTests
    {
        [Test]
        public void LocalNoLogsFormatsCompactPanel()
        {
            Assert.AreEqual(
                TriggerCheckPanelFormatter.LocalNoLogsMessage,
                TriggerCheckPanelFormatter.Format(
                    null,
                    false,
                    TriggerType.Unknown,
                    TriggerCheckSource.Manual,
                    0,
                    null,
                    null,
                    GameZone.Hand));
        }

        [Test]
        public void OnlineSummaryIncludesDraftAndSelectedCardWithoutInstanceId()
        {
            string formatted = TriggerCheckPanelFormatter.Format(
                null,
                true,
                TriggerType.Critical,
                TriggerCheckSource.Drive,
                1,
                "BT01-001TH",
                "instance-secret",
                GameZone.Hand);

            Assert.IsTrue(formatted.Contains("Trigger panel"));
            Assert.IsTrue(formatted.Contains("Logs: 0"));
            Assert.IsTrue(formatted.Contains("Draft: Critical / Drive / idx 1"));
            Assert.IsTrue(formatted.Contains("Selected: card BT01-001TH / zone Hand"));
            Assert.IsFalse(formatted.Contains("instance-secret"));
        }

        [Test]
        public void LatestLogSummaryAvoidsCheckedCardIdentity()
        {
            TriggerCheckReplayLog log = CreateLog(new TriggerCheckLogEntry
            {
                summary = "Drive check 0 SECRET-CARD Critical accepted; modifiers=1",
                checked_card_id = "SECRET-CARD",
                checked_card_instance_id = "SECRET-INSTANCE",
                check_source = TriggerCheckSource.Drive,
                check_index = 0,
                trigger_type = TriggerType.Critical,
                accepted = true,
                modifier_count = 1
            });

            string formatted = TriggerCheckPanelFormatter.Format(
                new List<NetworkTriggerCheckReplayLogPayload> { CreatePayload(log) },
                true,
                TriggerType.Unknown,
                TriggerCheckSource.Manual,
                0,
                null,
                null,
                GameZone.Hand);

            Assert.IsTrue(formatted.Contains("Logs: 1"));
            Assert.IsTrue(formatted.Contains("Latest: Drive #0 Critical accepted mods=1"));
            Assert.IsFalse(formatted.Contains("SECRET-CARD"));
            Assert.IsFalse(formatted.Contains("SECRET-INSTANCE"));
        }

        [Test]
        public void InvalidPayloadFormatsStablePanelMessage()
        {
            var payloads = new List<NetworkTriggerCheckReplayLogPayload>
            {
                new NetworkTriggerCheckReplayLogPayload
                {
                    protocol_version = MultiplayerProtocol.ProtocolVersion,
                    trigger_check_log_json = string.Empty
                }
            };

            string formatted = TriggerCheckPanelFormatter.Format(
                payloads,
                true,
                TriggerType.Unknown,
                TriggerCheckSource.Manual,
                0,
                null,
                null,
                GameZone.Hand);

            Assert.IsTrue(formatted.Contains("Logs: 1"));
            Assert.IsTrue(formatted.Contains("Latest: invalid (TRIGGER_CHECK_REPLAY_LOG_PAYLOAD_INVALID)"));
        }

        [Test]
        public void FormattingDoesNotMutatePayload()
        {
            NetworkTriggerCheckReplayLogPayload payload = CreatePayload(CreateLog(new TriggerCheckLogEntry
            {
                check_source = TriggerCheckSource.Damage,
                check_index = 2,
                trigger_type = TriggerType.Heal,
                accepted = true,
                modifier_count = 0
            }));
            string before = payload.ToJson();

            TriggerCheckPanelFormatter.Format(
                new List<NetworkTriggerCheckReplayLogPayload> { payload },
                true,
                TriggerType.Unknown,
                TriggerCheckSource.Manual,
                0,
                null,
                null,
                GameZone.Hand);

            Assert.AreEqual(before, payload.ToJson());
        }

        private static TriggerCheckReplayLog CreateLog(TriggerCheckLogEntry entry)
        {
            var log = new TriggerCheckReplayLog();
            log.entries.Add(entry);
            return log;
        }

        private static NetworkTriggerCheckReplayLogPayload CreatePayload(TriggerCheckReplayLog log)
        {
            return TriggerCheckReplayLogPayloadCodec.Encode(
                "ROOM-TRIGGER-PANEL",
                "p1",
                log,
                GameStateViewPerspective.Spectator);
        }
    }
}
