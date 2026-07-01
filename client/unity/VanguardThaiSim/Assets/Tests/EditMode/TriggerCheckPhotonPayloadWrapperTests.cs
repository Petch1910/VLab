using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckPhotonPayloadWrapperTests
    {
        [Test]
        public void PhotonWrapperRoundTripsTriggerCheckReplayPayload()
        {
            TriggerCheckReplayLog maskedLog = CreateMaskedLog();
            NetworkTriggerCheckReplayLogPayload inner = TriggerCheckReplayLogPayloadCodec.Encode(
                "ROOM-1",
                "p1",
                maskedLog,
                GameStateViewPerspective.Spectator);

            PhotonRealtimePayload photonPayload = PhotonRealtimePayloadCodec.EncodeTriggerCheckReplayLog(inner);
            PhotonRealtimePayload roundTripPhotonPayload = PhotonRealtimePayload.FromJson(photonPayload.ToJson());

            NetworkTriggerCheckReplayLogPayload roundTripInner;
            string rejectionReason;
            Assert.IsTrue(
                PhotonRealtimePayloadCodec.TryDecodeTriggerCheckReplayLog(
                    roundTripPhotonPayload,
                    out roundTripInner,
                    out rejectionReason),
                rejectionReason);
            Assert.AreEqual(PhotonRealtimePayloadCodec.TriggerCheckReplayLogEventCode, roundTripPhotonPayload.event_code);
            Assert.AreEqual("p1", roundTripPhotonPayload.sender_player_id);
            Assert.AreEqual(inner.payload_id, roundTripInner.payload_id);
            Assert.AreEqual(inner.trigger_check_log_json, roundTripInner.trigger_check_log_json);
        }

        [Test]
        public void WrongPhotonEventCodeIsRejected()
        {
            PhotonRealtimePayload payload = PhotonRealtimePayloadCodec.EncodeTriggerCheckReplayLog(CreateInnerPayload());
            payload.event_code = PhotonRealtimePayloadCodec.PublicGameEventCode;

            NetworkTriggerCheckReplayLogPayload decoded;
            string rejectionReason;
            Assert.IsFalse(PhotonRealtimePayloadCodec.TryDecodeTriggerCheckReplayLog(payload, out decoded, out rejectionReason));
            Assert.AreEqual("TRIGGER_CHECK_REPLAY_LOG_PAYLOAD_INVALID", rejectionReason);
            Assert.IsNull(decoded);
        }

        [Test]
        public void InnerProtocolMismatchIsRejected()
        {
            NetworkTriggerCheckReplayLogPayload inner = CreateInnerPayload();
            inner.protocol_version = MultiplayerProtocol.ProtocolVersion + 1;
            PhotonRealtimePayload payload = PhotonRealtimePayloadCodec.EncodeTriggerCheckReplayLog(inner);

            NetworkTriggerCheckReplayLogPayload decoded;
            string rejectionReason;
            Assert.IsFalse(PhotonRealtimePayloadCodec.TryDecodeTriggerCheckReplayLog(payload, out decoded, out rejectionReason));
            Assert.AreEqual("PROTOCOL_VERSION_MISMATCH", rejectionReason);
            Assert.IsNull(decoded);
        }

        [Test]
        public void PhotonWrapperJsonIsDeterministic()
        {
            NetworkTriggerCheckReplayLogPayload inner = CreateInnerPayload();

            PhotonRealtimePayload first = PhotonRealtimePayloadCodec.EncodeTriggerCheckReplayLog(inner);
            PhotonRealtimePayload second = PhotonRealtimePayloadCodec.EncodeTriggerCheckReplayLog(inner);

            Assert.AreEqual(first.ToJson(), second.ToJson());
        }

        [Test]
        public void PhotonWrappingDoesNotMutateSourceLog()
        {
            TriggerCheckReplayLog maskedLog = CreateMaskedLog();
            string before = maskedLog.ToJson();
            NetworkTriggerCheckReplayLogPayload inner = TriggerCheckReplayLogPayloadCodec.Encode(
                "ROOM-1",
                "p1",
                maskedLog,
                GameStateViewPerspective.Spectator);

            PhotonRealtimePayloadCodec.EncodeTriggerCheckReplayLog(inner);

            Assert.AreEqual(before, maskedLog.ToJson());
        }

        private static NetworkTriggerCheckReplayLogPayload CreateInnerPayload()
        {
            return TriggerCheckReplayLogPayloadCodec.Encode(
                "ROOM-1",
                "p1",
                CreateMaskedLog(),
                GameStateViewPerspective.Spectator);
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
