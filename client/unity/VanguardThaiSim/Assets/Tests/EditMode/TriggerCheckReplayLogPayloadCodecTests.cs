using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckReplayLogPayloadCodecTests
    {
        [Test]
        public void PayloadRoundTripsMaskedReplayLog()
        {
            TriggerCheckReplayLog maskedLog = CreateMaskedLog();

            NetworkTriggerCheckReplayLogPayload payload = TriggerCheckReplayLogPayloadCodec.Encode(
                "ROOM-1",
                "p1",
                maskedLog,
                GameStateViewPerspective.Spectator);
            NetworkTriggerCheckReplayLogPayload roundTripPayload =
                NetworkTriggerCheckReplayLogPayload.FromJson(payload.ToJson());

            TriggerCheckReplayLog decoded;
            string rejectionReason;
            Assert.IsTrue(
                TriggerCheckReplayLogPayloadCodec.TryDecode(roundTripPayload, out decoded, out rejectionReason),
                rejectionReason);
            Assert.AreEqual(MultiplayerProtocol.ProtocolVersion, roundTripPayload.protocol_version);
            Assert.AreEqual("ROOM-1", roundTripPayload.room_id);
            Assert.AreEqual(GameStateViewPerspective.Spectator.ToString(), roundTripPayload.perspective);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, decoded.entries[0].checked_card_id);
            Assert.AreEqual(maskedLog.entries[0].log_entry_id, decoded.entries[0].log_entry_id);
        }

        [Test]
        public void WrongProtocolVersionIsRejected()
        {
            NetworkTriggerCheckReplayLogPayload payload = TriggerCheckReplayLogPayloadCodec.Encode(
                "ROOM-1",
                "p1",
                CreateMaskedLog(),
                GameStateViewPerspective.Spectator);
            payload.protocol_version = MultiplayerProtocol.ProtocolVersion + 1;

            TriggerCheckReplayLog decoded;
            string rejectionReason;
            Assert.IsFalse(TriggerCheckReplayLogPayloadCodec.TryDecode(payload, out decoded, out rejectionReason));
            Assert.AreEqual("PROTOCOL_VERSION_MISMATCH", rejectionReason);
            Assert.IsNull(decoded);
        }

        [Test]
        public void EmptyLogJsonIsRejected()
        {
            var payload = new NetworkTriggerCheckReplayLogPayload
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                trigger_check_log_json = ""
            };

            TriggerCheckReplayLog decoded;
            string rejectionReason;
            Assert.IsFalse(TriggerCheckReplayLogPayloadCodec.TryDecode(payload, out decoded, out rejectionReason));
            Assert.AreEqual("TRIGGER_CHECK_REPLAY_LOG_PAYLOAD_INVALID", rejectionReason);
            Assert.IsNull(decoded);
        }

        [Test]
        public void EncodingIsDeterministic()
        {
            TriggerCheckReplayLog maskedLog = CreateMaskedLog();

            NetworkTriggerCheckReplayLogPayload first = TriggerCheckReplayLogPayloadCodec.Encode(
                "ROOM-1",
                "p1",
                maskedLog,
                GameStateViewPerspective.Spectator);
            NetworkTriggerCheckReplayLogPayload second = TriggerCheckReplayLogPayloadCodec.Encode(
                "ROOM-1",
                "p1",
                maskedLog,
                GameStateViewPerspective.Spectator);

            Assert.AreEqual(first.ToJson(), second.ToJson());
        }

        [Test]
        public void EncodingDoesNotMutateSourceLog()
        {
            TriggerCheckReplayLog maskedLog = CreateMaskedLog();
            string before = maskedLog.ToJson();

            TriggerCheckReplayLogPayloadCodec.Encode(
                "ROOM-1",
                "p1",
                maskedLog,
                GameStateViewPerspective.Spectator);

            Assert.AreEqual(before, maskedLog.ToJson());
        }

        private static TriggerCheckReplayLog CreateMaskedLog()
        {
            TriggerCheckReplayLog log = TriggerCheckReplayLogBuilder.Append(
                null,
                TriggerCheckLogEntryFactory.FromBundle(
                    TriggerCheckResolutionBundler.Build(
                        CreateState(),
                        0,
                        TriggerCheckSource.Drive,
                        0,
                        "drive-card-1",
                        "CRIT-001",
                        TriggerType.Critical,
                        CombatModifierExpiration.EndOfTurn)));

            return TriggerCheckReplayLogMasker.CreateView(log, GameStateViewPerspective.Spectator);
        }

        private static GameState CreateState()
        {
            return new GameState
            {
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        vanguard = new List<GameCardInstance>
                        {
                            new GameCardInstance("vg", "VG-001", 0)
                        },
                        rear_guard = new List<GameCardInstance>
                        {
                            new GameCardInstance("rg", "RG-001", 0)
                        }
                    }
                }
            };
        }
    }
}
