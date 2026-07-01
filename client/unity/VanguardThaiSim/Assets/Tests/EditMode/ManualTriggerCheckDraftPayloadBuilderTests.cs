using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class ManualTriggerCheckDraftPayloadBuilderTests
    {
        [Test]
        public void DraftPayloadMasksSpectatorViewAndDoesNotMutateState()
        {
            GameState state = CreateState();
            string before = state.ToJson();

            ManualTriggerCheckDraftResult result = ManualTriggerCheckDraftPayloadBuilder.Build(
                state,
                CreateRequest(GameStateViewPerspective.Spectator, -1));

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.NotNull(result.payload);
            Assert.AreEqual("ROOM-DRAFT", result.payload.room_id);
            Assert.AreEqual(GameStateViewPerspective.Spectator.ToString(), result.payload.perspective);
            Assert.AreEqual(before, state.ToJson());
            Assert.AreEqual(0, state.event_log.Count);

            TriggerCheckReplayLog decoded;
            string rejectionReason;
            Assert.IsTrue(
                TriggerCheckReplayLogPayloadCodec.TryDecode(result.payload, out decoded, out rejectionReason),
                rejectionReason);
            Assert.AreEqual(1, decoded.entries.Count);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, decoded.entries[0].checked_card_id);
            Assert.IsTrue(decoded.entries[0].hides_checked_card_identity);
            Assert.AreEqual(TriggerType.Critical, decoded.entries[0].trigger_type);
        }

        [Test]
        public void DraftPayloadCanRevealOwnerPlayerView()
        {
            ManualTriggerCheckDraftResult result = ManualTriggerCheckDraftPayloadBuilder.Build(
                CreateState(),
                CreateRequest(GameStateViewPerspective.Player, 0));

            TriggerCheckReplayLog decoded;
            string rejectionReason;
            Assert.IsTrue(
                TriggerCheckReplayLogPayloadCodec.TryDecode(result.payload, out decoded, out rejectionReason),
                rejectionReason);
            Assert.AreEqual("CRIT-001", decoded.entries[0].checked_card_id);
            Assert.IsFalse(decoded.entries[0].hides_checked_card_identity);
        }

        [Test]
        public void DraftPayloadOutputIsDeterministic()
        {
            GameState state = CreateState();
            ManualTriggerCheckDraftRequest request = CreateRequest(GameStateViewPerspective.Spectator, -1);

            ManualTriggerCheckDraftResult first = ManualTriggerCheckDraftPayloadBuilder.Build(state, request);
            ManualTriggerCheckDraftResult second = ManualTriggerCheckDraftPayloadBuilder.Build(state, request);

            Assert.IsTrue(first.accepted);
            Assert.AreEqual(first.payload.ToJson(), second.payload.ToJson());
        }

        [Test]
        public void MissingCheckedCardIdIsRejected()
        {
            ManualTriggerCheckDraftRequest request = CreateRequest(GameStateViewPerspective.Spectator, -1);
            request.checked_card_id = "";

            ManualTriggerCheckDraftResult result = ManualTriggerCheckDraftPayloadBuilder.Build(CreateState(), request);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual("CHECKED_CARD_ID_MISSING", result.rejection_reason);
            Assert.IsNull(result.payload);
        }

        [Test]
        public void UnknownTriggerCreatesManualResolutionPayload()
        {
            ManualTriggerCheckDraftRequest request = CreateRequest(GameStateViewPerspective.Spectator, -1);
            request.trigger_type = TriggerType.Unknown;

            ManualTriggerCheckDraftResult result = ManualTriggerCheckDraftPayloadBuilder.Build(CreateState(), request);

            TriggerCheckReplayLog decoded;
            string rejectionReason;
            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(
                TriggerCheckReplayLogPayloadCodec.TryDecode(result.payload, out decoded, out rejectionReason),
                rejectionReason);
            Assert.IsTrue(decoded.entries[0].needs_manual_resolution);
            Assert.AreEqual(TriggerType.Unknown, decoded.entries[0].trigger_type);
        }

        private static ManualTriggerCheckDraftRequest CreateRequest(
            GameStateViewPerspective perspective,
            int viewerPlayerIndex)
        {
            return new ManualTriggerCheckDraftRequest
            {
                room_id = "ROOM-DRAFT",
                sender_player_id = "p1",
                player_index = 0,
                check_source = TriggerCheckSource.Drive,
                check_index = 0,
                checked_card_instance_id = "drive-card-1",
                checked_card_id = "CRIT-001",
                trigger_type = TriggerType.Critical,
                modifier_expiration = CombatModifierExpiration.EndOfTurn,
                perspective = perspective,
                viewer_player_index = viewerPlayerIndex
            };
        }

        private static GameState CreateState()
        {
            var vanguard = new GameCardInstance("vg", "VG-001", 0);
            var highRearGuard = new GameCardInstance("rg-high", "RG-HIGH", 0);
            highRearGuard.power_delta = 5000;

            return new GameState
            {
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        vanguard = new List<GameCardInstance> { vanguard },
                        rear_guard = new List<GameCardInstance> { highRearGuard }
                    }
                }
            };
        }
    }
}
