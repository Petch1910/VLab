using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckLogSummaryFormatterTests
    {
        [Test]
        public void NoLogsFormatsExistingText()
        {
            Assert.AreEqual(
                TriggerCheckLogSummaryFormatter.NoLogsMessage,
                TriggerCheckLogSummaryFormatter.FormatSummary(null));
            Assert.AreEqual(
                TriggerCheckLogSummaryFormatter.NoLogsMessage,
                TriggerCheckLogSummaryFormatter.FormatSummary(new List<NetworkTriggerCheckReplayLogPayload>()));
        }

        [Test]
        public void InvalidLatestPayloadFormatsExistingInvalidMessage()
        {
            var payloads = new List<NetworkTriggerCheckReplayLogPayload>
            {
                new NetworkTriggerCheckReplayLogPayload
                {
                    protocol_version = MultiplayerProtocol.ProtocolVersion,
                    trigger_check_log_json = string.Empty
                }
            };

            Assert.AreEqual(
                "Trigger logs: 1\nLatest: invalid (TRIGGER_CHECK_REPLAY_LOG_PAYLOAD_INVALID)",
                TriggerCheckLogSummaryFormatter.FormatSummary(payloads));
        }

        [Test]
        public void EmptyDecodedLogFormatsExistingEmptyMessage()
        {
            Assert.AreEqual(
                "Trigger logs: 1\nLatest: empty",
                TriggerCheckLogSummaryFormatter.FormatSummary(
                    new List<NetworkTriggerCheckReplayLogPayload> { CreatePayload(new TriggerCheckReplayLog()) }));
        }

        [Test]
        public void LatestEntrySummaryIsUsedWhenPresent()
        {
            TriggerCheckReplayLog log = CreateLog(new TriggerCheckLogEntry
            {
                summary = "Drive check 0 CRIT-001 Critical accepted; modifiers=1",
                check_source = TriggerCheckSource.Drive,
                check_index = 0,
                trigger_type = TriggerType.Critical
            });

            Assert.AreEqual(
                "Trigger logs: 1\nLatest: Drive check 0 CRIT-001 Critical accepted; modifiers=1",
                TriggerCheckLogSummaryFormatter.FormatSummary(
                    new List<NetworkTriggerCheckReplayLogPayload> { CreatePayload(log) }));
        }

        [Test]
        public void BlankSummaryFallsBackToCheckSourceIndexAndTriggerType()
        {
            TriggerCheckReplayLog log = CreateLog(new TriggerCheckLogEntry
            {
                summary = " ",
                check_source = TriggerCheckSource.Damage,
                check_index = 1,
                trigger_type = TriggerType.Over
            });

            Assert.AreEqual(
                "Trigger logs: 1\nLatest: Damage check 1 Over",
                TriggerCheckLogSummaryFormatter.FormatSummary(
                    new List<NetworkTriggerCheckReplayLogPayload> { CreatePayload(log) }));
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
                "ROOM-LOG",
                "p1",
                log,
                GameStateViewPerspective.Spectator);
        }
    }
}
