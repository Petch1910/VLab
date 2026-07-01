using System;
using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;

namespace VanguardThaiSim.Tests
{
    public sealed class MultiplayerGameSessionTests
    {
        [Test]
        public void LocalActionPublishesEnvelopeAndRemoteAppliesIt()
        {
            GameState initial = CreateState(1001);
            GameState hostState = GameState.FromJson(initial.ToJson(false));
            GameState guestState = GameState.FromJson(initial.ToJson(false));
            MultiplayerRoomState room = CreateRoom();
            FakeTransport hostTransport = new FakeTransport();
            FakeTransport guestTransport = new FakeTransport();
            MultiplayerGameSessionController host = new MultiplayerGameSessionController(hostTransport, room, hostState, "p1");
            MultiplayerGameSessionController guest = new MultiplayerGameSessionController(guestTransport, room, guestState, "p2");

            RulesCommandResult result = host.ExecuteLocalAction(FirstAction(RulesCore.GetLegalActions(hostState, 0), GameActionType.Draw));
            guestTransport.EmitGameEvent(hostTransport.sentEvents[0]);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(1, hostTransport.sentEvents.Count);
            Assert.AreEqual(hostState.GetPlayer(0).CountZone(GameZone.Deck), guest.State.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(hostState.GetPlayer(0).CountZone(GameZone.Hand), guest.State.GetPlayer(0).CountZone(GameZone.Hand));
            Assert.AreEqual(1, guest.State.event_log.Count);
        }

        [Test]
        public void ReconnectRequestSendsBatchFromRequestedCursor()
        {
            GameState initial = CreateState(1002);
            GameState hostState = GameState.FromJson(initial.ToJson(false));
            MultiplayerRoomState room = CreateRoom();
            FakeTransport hostTransport = new FakeTransport();
            MultiplayerGameSessionController host = new MultiplayerGameSessionController(hostTransport, room, hostState, "p1");

            host.ExecuteLocalAction(FirstAction(RulesCore.GetLegalActions(hostState, 0), GameActionType.Draw));
            host.ExecuteLocalAction(FirstPhase(RulesCore.GetLegalActions(hostState, 0), GamePhase.Main));
            hostTransport.EmitReconnectRequest(new NetworkReconnectRequest
            {
                protocol_version = MultiplayerProtocol.ProtocolVersion,
                room_id = room.room_id,
                player_id = "p2",
                from_event_index = 1
            });

            Assert.NotNull(hostTransport.sentReconnectBatch);
            Assert.AreEqual(1, hostTransport.sentReconnectBatch.from_event_index);
            Assert.AreEqual(1, hostTransport.sentReconnectBatch.events.Count);
            Assert.AreEqual(GameActionType.SetPhase, hostTransport.sentReconnectBatch.events[0].game_event.action_type);
        }

        [Test]
        public void ReconnectBatchAppliesMissingEventsToSessionState()
        {
            GameState initial = CreateState(1003);
            GameState hostState = GameState.FromJson(initial.ToJson(false));
            GameState guestState = GameState.FromJson(initial.ToJson(false));
            MultiplayerRoomState room = CreateRoom();
            FakeTransport hostTransport = new FakeTransport();
            FakeTransport guestTransport = new FakeTransport();
            MultiplayerGameSessionController host = new MultiplayerGameSessionController(hostTransport, room, hostState, "p1");
            MultiplayerGameSessionController guest = new MultiplayerGameSessionController(guestTransport, room, guestState, "p2");

            host.ExecuteLocalAction(FirstAction(RulesCore.GetLegalActions(hostState, 0), GameActionType.Draw));
            guestTransport.EmitGameEvent(hostTransport.sentEvents[0]);
            host.ExecuteLocalAction(FirstPhase(RulesCore.GetLegalActions(hostState, 0), GamePhase.Main));
            NetworkEventBatch batch = host.CreateReconnectBatch(1);

            int applied = guest.ApplyReconnectBatch(batch);

            Assert.AreEqual(1, applied);
            Assert.AreEqual(GamePhase.Main, guest.State.phase);
            Assert.AreEqual(host.State.event_log.Count, guest.State.event_log.Count);
        }

        [Test]
        public void NonSharedPrivacySessionPublishesPublicEventInsteadOfTrueEnvelope()
        {
            GameState initial = CreateState(1004);
            MultiplayerRoomState room = CreateRoom();
            room.deck_privacy_mode = DeckPrivacyModes.DeckCommitment;
            room.room_visibility = RoomVisibilityModes.Public;
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p1");

            RulesCommandResult result = session.ExecuteLocalAction(FirstAction(RulesCore.GetLegalActions(initial, 0), GameActionType.Draw));

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(0, transport.sentEvents.Count);
            Assert.AreEqual(1, transport.sentPublicEvents.Count);
            Assert.AreEqual(1, session.PublicEventLog.Count);
            Assert.IsTrue(transport.sentPublicEvents[0].hides_card_identity);
            Assert.AreEqual("", transport.sentPublicEvents[0].public_card_id);
            Assert.AreEqual(1, session.State.event_log.Count);
        }

        [Test]
        public void NonSharedPrivacySessionAddsRevealProofWhenCardBecomesPublic()
        {
            GameState initial = CreateState(1006);
            MultiplayerRoomState room = CreateRoom();
            room.deck_privacy_mode = DeckPrivacyModes.DeckCommitment;
            room.players.Add(new RoomPlayerInfo
            {
                player_id = "p1",
                display_name = "p1",
                deck_commitment = "commitment-p1",
                deck_commitment_algorithm = DeckCommitmentService.Algorithm,
                connected = true
            });
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p1");

            RulesCommandResult result = session.ExecuteLocalAction(FirstMove(RulesCore.GetLegalActions(initial, 0), GameZone.Hand, GameZone.RearGuard));

            string rejectionReason;
            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(1, transport.sentPublicEvents.Count);
            Assert.IsFalse(string.IsNullOrWhiteSpace(transport.sentPublicEvents[0].reveal_proof));
            Assert.IsTrue(
                DeckCommitmentService.TryVerifyRevealProof(room.players[0], room.room_id, room.pack.definition_hash, transport.sentPublicEvents[0], out rejectionReason),
                rejectionReason);
        }

        [Test]
        public void IncomingPublicEventIsStoredWithoutMutatingTrueState()
        {
            GameState initial = CreateState(1005);
            MultiplayerRoomState room = CreateRoom();
            room.deck_privacy_mode = DeckPrivacyModes.DeckCommitment;
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");
            int deckCount = session.State.GetPlayer(0).CountZone(GameZone.Deck);
            NetworkPublicGameEvent publicEvent = new NetworkPublicGameEvent
            {
                event_id = "public-remote",
                source_event_id = "event-remote",
                action_type = GameActionType.Draw,
                actor_index = 0,
                from_zone = GameZone.Deck,
                to_zone = GameZone.Hand,
                from_zone_count_delta = -1,
                to_zone_count_delta = 1,
                hides_card_identity = true
            };

            transport.EmitPublicGameEvent(publicEvent);

            Assert.AreEqual(1, session.PublicEventLog.Count);
            Assert.AreEqual("public-remote", session.PublicEventLog[0].event_id);
            Assert.AreEqual(0, session.State.event_log.Count);
            Assert.AreEqual(deckCount, session.State.GetPlayer(0).CountZone(GameZone.Deck));
        }

        [Test]
        public void IncomingTriggerCheckReplayPayloadIsStoredWithoutMutatingTrueState()
        {
            GameState initial = CreateState(1007);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");
            string before = session.State.ToJson();
            NetworkTriggerCheckReplayLogPayload payload = CreateTriggerCheckPayload(room, initial);

            transport.EmitTriggerCheckReplayLog(payload);

            Assert.AreEqual(1, session.TriggerCheckReplayLogPayloads.Count);
            Assert.AreEqual(payload.payload_id, session.TriggerCheckReplayLogPayloads[0].payload_id);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void NormalGameEventSyncStillWorksAfterTriggerCheckReplayReceipt()
        {
            GameState initial = CreateState(1008);
            GameState hostState = GameState.FromJson(initial.ToJson(false));
            GameState guestState = GameState.FromJson(initial.ToJson(false));
            MultiplayerRoomState room = CreateRoom();
            FakeTransport hostTransport = new FakeTransport();
            FakeTransport guestTransport = new FakeTransport();
            MultiplayerGameSessionController host = new MultiplayerGameSessionController(hostTransport, room, hostState, "p1");
            MultiplayerGameSessionController guest = new MultiplayerGameSessionController(guestTransport, room, guestState, "p2");

            guestTransport.EmitTriggerCheckReplayLog(CreateTriggerCheckPayload(room, initial));
            host.ExecuteLocalAction(FirstAction(RulesCore.GetLegalActions(hostState, 0), GameActionType.Draw));
            guestTransport.EmitGameEvent(hostTransport.sentEvents[0]);

            Assert.AreEqual(1, guest.TriggerCheckReplayLogPayloads.Count);
            Assert.AreEqual(1, guest.State.event_log.Count);
            Assert.AreEqual(host.State.GetPlayer(0).CountZone(GameZone.Hand), guest.State.GetPlayer(0).CountZone(GameZone.Hand));
        }

        [Test]
        public void NullTriggerCheckReplayPayloadIsIgnored()
        {
            GameState initial = CreateState(1009);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");

            transport.EmitTriggerCheckReplayLog(null);

            Assert.AreEqual(0, session.TriggerCheckReplayLogPayloads.Count);
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void IncomingPendingAutoAbilityQueuePayloadIsStoredWithoutMutatingTrueState()
        {
            GameState initial = CreateState(1014);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");
            string before = session.State.ToJson();
            NetworkPendingAutoAbilityQueuePayload payload = CreatePendingAutoAbilityQueuePayload(room);

            transport.EmitPendingAutoAbilityQueue(payload);

            Assert.AreEqual(1, session.PendingAutoAbilityQueuePayloads.Count);
            Assert.AreEqual(payload.payload_id, session.PendingAutoAbilityQueuePayloads[0].payload_id);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void NullPendingAutoAbilityQueuePayloadIsIgnored()
        {
            GameState initial = CreateState(1015);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");

            transport.EmitPendingAutoAbilityQueue(null);

            Assert.AreEqual(0, session.PendingAutoAbilityQueuePayloads.Count);
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void NormalGameEventSyncStillWorksAfterPendingAutoAbilityQueueReceipt()
        {
            GameState initial = CreateState(1016);
            GameState hostState = GameState.FromJson(initial.ToJson(false));
            GameState guestState = GameState.FromJson(initial.ToJson(false));
            MultiplayerRoomState room = CreateRoom();
            FakeTransport hostTransport = new FakeTransport();
            FakeTransport guestTransport = new FakeTransport();
            MultiplayerGameSessionController host = new MultiplayerGameSessionController(hostTransport, room, hostState, "p1");
            MultiplayerGameSessionController guest = new MultiplayerGameSessionController(guestTransport, room, guestState, "p2");

            guestTransport.EmitPendingAutoAbilityQueue(CreatePendingAutoAbilityQueuePayload(room));
            host.ExecuteLocalAction(FirstAction(RulesCore.GetLegalActions(hostState, 0), GameActionType.Draw));
            guestTransport.EmitGameEvent(hostTransport.sentEvents[0]);

            Assert.AreEqual(1, guest.PendingAutoAbilityQueuePayloads.Count);
            Assert.AreEqual(1, guest.State.event_log.Count);
            Assert.AreEqual(host.State.GetPlayer(0).CountZone(GameZone.Hand), guest.State.GetPlayer(0).CountZone(GameZone.Hand));
        }

        [Test]
        public void IncomingPendingAutoAbilityResolutionRequestPayloadIsStoredWithoutMutatingTrueState()
        {
            GameState initial = CreateState(1019);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");
            string before = session.State.ToJson();
            NetworkPendingAutoAbilityResolutionRequestPayload payload =
                CreatePendingAutoAbilityResolutionRequestPayload(room);

            transport.EmitPendingAutoAbilityResolutionRequest(payload);

            Assert.AreEqual(1, session.PendingAutoAbilityResolutionRequestPayloads.Count);
            Assert.AreEqual(payload.payload_id, session.PendingAutoAbilityResolutionRequestPayloads[0].payload_id);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void NullPendingAutoAbilityResolutionRequestPayloadIsIgnored()
        {
            GameState initial = CreateState(1020);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");

            transport.EmitPendingAutoAbilityResolutionRequest(null);

            Assert.AreEqual(0, session.PendingAutoAbilityResolutionRequestPayloads.Count);
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void NormalGameEventSyncStillWorksAfterPendingAutoAbilityResolutionRequestReceipt()
        {
            GameState initial = CreateState(1021);
            GameState hostState = GameState.FromJson(initial.ToJson(false));
            GameState guestState = GameState.FromJson(initial.ToJson(false));
            MultiplayerRoomState room = CreateRoom();
            FakeTransport hostTransport = new FakeTransport();
            FakeTransport guestTransport = new FakeTransport();
            MultiplayerGameSessionController host = new MultiplayerGameSessionController(hostTransport, room, hostState, "p1");
            MultiplayerGameSessionController guest = new MultiplayerGameSessionController(guestTransport, room, guestState, "p2");

            guestTransport.EmitPendingAutoAbilityResolutionRequest(
                CreatePendingAutoAbilityResolutionRequestPayload(room));
            host.ExecuteLocalAction(FirstAction(RulesCore.GetLegalActions(hostState, 0), GameActionType.Draw));
            guestTransport.EmitGameEvent(hostTransport.sentEvents[0]);

            Assert.AreEqual(1, guest.PendingAutoAbilityResolutionRequestPayloads.Count);
            Assert.AreEqual(1, guest.State.event_log.Count);
            Assert.AreEqual(host.State.GetPlayer(0).CountZone(GameZone.Hand), guest.State.GetPlayer(0).CountZone(GameZone.Hand));
        }

        [Test]
        public void IncomingPendingAutoAbilityManualResolutionDecisionPayloadIsStoredWithoutMutatingTrueState()
        {
            GameState initial = CreateState(1026);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");
            string before = session.State.ToJson();
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                CreatePendingAutoAbilityManualResolutionDecisionPayload(room);

            transport.EmitPendingAutoAbilityManualResolutionDecision(payload);

            Assert.AreEqual(1, session.PendingAutoAbilityManualResolutionDecisionPayloads.Count);
            Assert.AreEqual(payload.payload_id, session.PendingAutoAbilityManualResolutionDecisionPayloads[0].payload_id);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void NullPendingAutoAbilityManualResolutionDecisionPayloadIsIgnored()
        {
            GameState initial = CreateState(1027);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");

            transport.EmitPendingAutoAbilityManualResolutionDecision(null);

            Assert.AreEqual(0, session.PendingAutoAbilityManualResolutionDecisionPayloads.Count);
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void NormalGameEventSyncStillWorksAfterPendingAutoAbilityManualResolutionDecisionReceipt()
        {
            GameState initial = CreateState(1028);
            GameState hostState = GameState.FromJson(initial.ToJson(false));
            GameState guestState = GameState.FromJson(initial.ToJson(false));
            MultiplayerRoomState room = CreateRoom();
            FakeTransport hostTransport = new FakeTransport();
            FakeTransport guestTransport = new FakeTransport();
            MultiplayerGameSessionController host = new MultiplayerGameSessionController(hostTransport, room, hostState, "p1");
            MultiplayerGameSessionController guest = new MultiplayerGameSessionController(guestTransport, room, guestState, "p2");

            guestTransport.EmitPendingAutoAbilityManualResolutionDecision(
                CreatePendingAutoAbilityManualResolutionDecisionPayload(room));
            host.ExecuteLocalAction(FirstAction(RulesCore.GetLegalActions(hostState, 0), GameActionType.Draw));
            guestTransport.EmitGameEvent(hostTransport.sentEvents[0]);

            Assert.AreEqual(1, guest.PendingAutoAbilityManualResolutionDecisionPayloads.Count);
            Assert.AreEqual(1, guest.State.event_log.Count);
            Assert.AreEqual(host.State.GetPlayer(0).CountZone(GameZone.Hand), guest.State.GetPlayer(0).CountZone(GameZone.Hand));
        }

        [Test]
        public void PublishLatestPendingAutoAbilityManualResolutionDecisionRejectsWhenNoPayloadExists()
        {
            GameState initial = CreateState(1029);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");

            MultiplayerTransportResult result = session.PublishLatestPendingAutoAbilityManualResolutionDecision();

            Assert.IsFalse(result.ok);
            Assert.AreEqual("PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_MISSING", result.error_code);
            Assert.AreEqual(0, transport.sentPendingAutoAbilityManualResolutionDecisions.Count);
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PublishLatestPendingAutoAbilityManualResolutionDecisionSendsStoredPayloadWithoutMutatingState()
        {
            GameState initial = CreateState(1030);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                CreatePendingAutoAbilityManualResolutionDecisionPayload(room);
            string before = session.State.ToJson();

            transport.EmitPendingAutoAbilityManualResolutionDecision(payload);
            MultiplayerTransportResult result = session.PublishLatestPendingAutoAbilityManualResolutionDecision();

            Assert.IsTrue(result.ok);
            Assert.AreEqual(1, transport.sentPendingAutoAbilityManualResolutionDecisions.Count);
            Assert.AreEqual(payload.payload_id, transport.sentPendingAutoAbilityManualResolutionDecisions[0].payload_id);
            Assert.AreEqual(1, session.PendingAutoAbilityManualResolutionDecisionPayloads.Count);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void CreatePendingAutoAbilityManualResolutionDecisionDraftRejectsWhenNoRequestPayloadExists()
        {
            GameState initial = CreateState(1031);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p1");

            PendingAutoAbilityManualResolutionDecisionDraftResult result =
                session.CreatePendingAutoAbilityManualResolutionDecisionDraft(
                    PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                    "resolve");

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionFactory.RequestMissingReason,
                result.rejection_reason);
            Assert.AreEqual(0, transport.sentPendingAutoAbilityManualResolutionDecisions.Count);
            Assert.AreEqual(0, session.PendingAutoAbilityManualResolutionDecisionPayloads.Count);
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void CreatePendingAutoAbilityManualResolutionDecisionDraftStoresPayloadWithoutSendingOrMutatingState()
        {
            GameState initial = CreateState(1032);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p1");
            string before = session.State.ToJson();

            transport.EmitPendingAutoAbilityResolutionRequest(
                CreatePendingAutoAbilityResolutionRequestPayload(room));
            PendingAutoAbilityManualResolutionDecisionDraftResult result =
                session.CreatePendingAutoAbilityManualResolutionDecisionDraft(
                    PendingAutoAbilityManualResolutionDecisionTypes.Resolve,
                    "resolve");

            Assert.IsTrue(result.accepted);
            Assert.AreEqual(0, transport.sentPendingAutoAbilityManualResolutionDecisions.Count);
            Assert.AreEqual(1, session.PendingAutoAbilityManualResolutionDecisionPayloads.Count);
            Assert.AreEqual(result.payload.payload_id, session.PendingAutoAbilityManualResolutionDecisionPayloads[0].payload_id);
            Assert.AreEqual(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, result.payload.decision_type);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void CreatePendingAutoAbilityManualResolutionDecisionDraftKeepsHiddenSourceRedacted()
        {
            GameState initial = CreateState(1033);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p1");
            PendingAutoAbilityResolutionRequest hiddenRequest = new PendingAutoAbilityResolutionRequest
            {
                selected_index = 0,
                pending_id = "pending-hidden",
                player_index = 0,
                timing_event = "OnBattle",
                source_card_instance_id = "private-source",
                source_card_id = GameStateViewFactory.HiddenCardId,
                hides_source_card_identity = true,
                summary = "Hidden pending AUTO"
            };

            transport.EmitPendingAutoAbilityResolutionRequest(
                PendingAutoAbilityResolutionRequestPayloadCodec.Encode(
                    room.room_id,
                    0,
                    hiddenRequest,
                    GameStateViewPerspective.Player,
                    0));
            PendingAutoAbilityManualResolutionDecisionDraftResult result =
                session.CreatePendingAutoAbilityManualResolutionDecisionDraft(
                    PendingAutoAbilityManualResolutionDecisionTypes.Defer,
                    "wait");

            PendingAutoAbilityManualResolutionDecision decoded;
            string rejectionReason;
            Assert.IsTrue(
                PendingAutoAbilityManualResolutionDecisionPayloadCodec.TryDecode(
                    result.payload,
                    out decoded,
                    out rejectionReason));
            Assert.IsTrue(decoded.hides_source_card_identity);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, decoded.source_card_id);
            Assert.AreEqual(string.Empty, decoded.source_card_instance_id);
            Assert.IsFalse(result.payload.pending_auto_ability_manual_resolution_decision_json.Contains("private-source"));
            Assert.AreEqual(0, transport.sentPendingAutoAbilityManualResolutionDecisions.Count);
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PublishLatestPendingAutoAbilityResolutionRequestRejectsWhenNoPayloadExists()
        {
            GameState initial = CreateState(1022);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");

            MultiplayerTransportResult result = session.PublishLatestPendingAutoAbilityResolutionRequest();

            Assert.IsFalse(result.ok);
            Assert.AreEqual("PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_MISSING", result.error_code);
            Assert.AreEqual(0, transport.sentPendingAutoAbilityResolutionRequests.Count);
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PublishLatestPendingAutoAbilityResolutionRequestSendsStoredPayloadWithoutMutatingState()
        {
            GameState initial = CreateState(1023);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");
            NetworkPendingAutoAbilityResolutionRequestPayload payload =
                CreatePendingAutoAbilityResolutionRequestPayload(room);
            string before = session.State.ToJson();

            transport.EmitPendingAutoAbilityResolutionRequest(payload);
            MultiplayerTransportResult result = session.PublishLatestPendingAutoAbilityResolutionRequest();

            Assert.IsTrue(result.ok);
            Assert.AreEqual(1, transport.sentPendingAutoAbilityResolutionRequests.Count);
            Assert.AreEqual(payload.payload_id, transport.sentPendingAutoAbilityResolutionRequests[0].payload_id);
            Assert.AreEqual(1, session.PendingAutoAbilityResolutionRequestPayloads.Count);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PublishPendingAutoAbilityResolutionRequestRejectsMissingRequest()
        {
            GameState initial = CreateState(1024);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p1");

            MultiplayerTransportResult result = session.PublishPendingAutoAbilityResolutionRequest(null);

            Assert.IsFalse(result.ok);
            Assert.AreEqual("PENDING_AUTO_ABILITY_RESOLUTION_REQUEST_MISSING", result.error_code);
            Assert.AreEqual(0, transport.sentPendingAutoAbilityResolutionRequests.Count);
            Assert.AreEqual(0, session.PendingAutoAbilityResolutionRequestPayloads.Count);
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PublishPendingAutoAbilityResolutionRequestEncodesAndSendsWithoutMutatingState()
        {
            GameState initial = CreateState(1025);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p1");
            string before = session.State.ToJson();
            PendingAutoAbilityResolutionRequest request = new PendingAutoAbilityResolutionRequest
            {
                selected_index = 2,
                pending_id = "pending-3",
                player_index = 1,
                timing_event = "OnBattle",
                source_card_instance_id = "src-3",
                source_card_id = "CARD-3",
                summary = "Resolve CARD-3"
            };

            MultiplayerTransportResult result = session.PublishPendingAutoAbilityResolutionRequest(request);

            Assert.IsTrue(result.ok);
            Assert.AreEqual(1, transport.sentPendingAutoAbilityResolutionRequests.Count);
            Assert.AreEqual(1, session.PendingAutoAbilityResolutionRequestPayloads.Count);
            Assert.AreEqual(transport.sentPendingAutoAbilityResolutionRequests[0].payload_id, session.PendingAutoAbilityResolutionRequestPayloads[0].payload_id);
            Assert.AreEqual(room.room_id, transport.sentPendingAutoAbilityResolutionRequests[0].room_id);
            Assert.AreEqual(0, transport.sentPendingAutoAbilityResolutionRequests[0].sender_player_index);
            Assert.AreEqual(2, transport.sentPendingAutoAbilityResolutionRequests[0].selected_index);
            Assert.AreEqual("pending-3", transport.sentPendingAutoAbilityResolutionRequests[0].pending_id);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PublishLatestPendingAutoAbilityQueueRejectsWhenNoPayloadExists()
        {
            GameState initial = CreateState(1017);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");

            MultiplayerTransportResult result = session.PublishLatestPendingAutoAbilityQueue();

            Assert.IsFalse(result.ok);
            Assert.AreEqual("PENDING_AUTO_ABILITY_QUEUE_MISSING", result.error_code);
            Assert.AreEqual(0, transport.sentPendingAutoAbilityQueues.Count);
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PublishLatestPendingAutoAbilityQueueSendsStoredPayloadWithoutMutatingState()
        {
            GameState initial = CreateState(1018);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");
            NetworkPendingAutoAbilityQueuePayload payload = CreatePendingAutoAbilityQueuePayload(room);
            string before = session.State.ToJson();

            transport.EmitPendingAutoAbilityQueue(payload);
            MultiplayerTransportResult result = session.PublishLatestPendingAutoAbilityQueue();

            Assert.IsTrue(result.ok);
            Assert.AreEqual(1, transport.sentPendingAutoAbilityQueues.Count);
            Assert.AreEqual(payload.payload_id, transport.sentPendingAutoAbilityQueues[0].payload_id);
            Assert.AreEqual(1, session.PendingAutoAbilityQueuePayloads.Count);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PreviewApplyLatestPendingAutoAbilityManualResolutionDecisionRejectsMissingQueuePayload()
        {
            GameState initial = CreateState(1034);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p1");

            transport.EmitPendingAutoAbilityManualResolutionDecision(
                CreatePendingAutoAbilityManualResolutionDecisionPayload(room));
            PendingAutoAbilityManualResolutionApplyExecutorResult result =
                session.PreviewApplyLatestPendingAutoAbilityManualResolutionDecision();

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(PendingAutoAbilityManualResolutionApplyRejectionReasons.QueueMissing, result.rejection_reason);
            Assert.AreEqual(0, transport.sentPendingAutoAbilityQueues.Count);
            Assert.AreEqual(0, session.PendingAutoAbilityQueuePayloads.Count);
            Assert.AreEqual(1, session.PendingAutoAbilityManualResolutionApplyPreviewLogs.Count);
            Assert.IsFalse(session.LatestPendingAutoAbilityManualResolutionApplyPreviewLog.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionApplyRejectionReasons.QueueMissing,
                session.LatestPendingAutoAbilityManualResolutionApplyPreviewLog.rejection_reason);
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PreviewApplyLatestPendingAutoAbilityManualResolutionDecisionRejectsMissingDecisionPayload()
        {
            GameState initial = CreateState(1035);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p1");

            transport.EmitPendingAutoAbilityQueue(CreatePlayerPendingAutoAbilityQueuePayload(room));
            PendingAutoAbilityManualResolutionApplyExecutorResult result =
                session.PreviewApplyLatestPendingAutoAbilityManualResolutionDecision();

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(PendingAutoAbilityManualResolutionApplyRejectionReasons.DecisionMissing, result.rejection_reason);
            Assert.AreEqual(0, transport.sentPendingAutoAbilityQueues.Count);
            Assert.AreEqual(1, session.PendingAutoAbilityQueuePayloads.Count);
            Assert.AreEqual(1, session.PendingAutoAbilityManualResolutionApplyPreviewLogs.Count);
            Assert.IsFalse(session.LatestPendingAutoAbilityManualResolutionApplyPreviewLog.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionApplyRejectionReasons.DecisionMissing,
                session.LatestPendingAutoAbilityManualResolutionApplyPreviewLog.rejection_reason);
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PreviewApplyLatestPendingAutoAbilityManualResolutionDecisionStoresQueuePayloadWithoutSendingOrMutatingState()
        {
            GameState initial = CreateState(1036);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p1");
            string before = session.State.ToJson();

            transport.EmitPendingAutoAbilityQueue(CreatePlayerPendingAutoAbilityQueuePayload(room));
            transport.EmitPendingAutoAbilityManualResolutionDecision(
                CreatePendingAutoAbilityManualResolutionDecisionPayload(
                    room,
                    PendingAutoAbilityManualResolutionDecisionTypes.Skip));
            PendingAutoAbilityManualResolutionApplyExecutorResult result =
                session.PreviewApplyLatestPendingAutoAbilityManualResolutionDecision();

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(0, transport.sentPendingAutoAbilityQueues.Count);
            Assert.AreEqual(2, session.PendingAutoAbilityQueuePayloads.Count);
            Assert.AreEqual(1, session.PendingAutoAbilityManualResolutionApplyPreviewLogs.Count);
            Assert.IsTrue(session.LatestPendingAutoAbilityManualResolutionApplyPreviewLog.accepted);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionTypes.Skip,
                session.LatestPendingAutoAbilityManualResolutionApplyPreviewLog.decision_type);
            Assert.AreEqual("pending-1", session.LatestPendingAutoAbilityManualResolutionApplyPreviewLog.pending_id);
            PendingAutoAbilityQueue decoded;
            string rejectionReason;
            Assert.IsTrue(
                PendingAutoAbilityQueuePayloadCodec.TryDecode(
                    session.PendingAutoAbilityQueuePayloads[1],
                    out decoded,
                    out rejectionReason));
            Assert.AreEqual(0, decoded.pending.Count);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PreviewApplyLatestPendingAutoAbilityManualResolutionDecisionFlowAlignsValidationResultAndLog()
        {
            GameState initial = CreateState(1037);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p1");
            string before = session.State.ToJson();
            NetworkPendingAutoAbilityManualResolutionDecisionPayload decisionPayload =
                CreatePendingAutoAbilityManualResolutionDecisionPayload(
                    room,
                    PendingAutoAbilityManualResolutionDecisionTypes.Defer);
            PendingAutoAbilityManualResolutionDecisionValidationResult validation =
                PendingAutoAbilityManualResolutionDecisionValidator.Validate(decisionPayload);

            transport.EmitPendingAutoAbilityQueue(CreatePlayerPendingAutoAbilityQueuePayload(room));
            transport.EmitPendingAutoAbilityManualResolutionDecision(decisionPayload);
            PendingAutoAbilityManualResolutionApplyExecutorResult result =
                session.PreviewApplyLatestPendingAutoAbilityManualResolutionDecision();

            Assert.IsTrue(validation.accepted, validation.rejection_reason);
            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.NotNull(result.apply_result);
            Assert.IsTrue(result.apply_result.accepted, result.apply_result.rejection_reason);
            Assert.AreEqual(validation.decision.pending_id, result.apply_result.pending_id);
            Assert.AreEqual(validation.decision.decision_type, result.apply_result.decision_type);
            Assert.AreEqual(1, session.PendingAutoAbilityManualResolutionApplyPreviewLogs.Count);
            Assert.AreEqual(
                result.apply_result.pending_id,
                session.LatestPendingAutoAbilityManualResolutionApplyPreviewLog.pending_id);
            Assert.AreEqual(
                result.apply_result.decision_type,
                session.LatestPendingAutoAbilityManualResolutionApplyPreviewLog.decision_type);
            Assert.AreEqual(0, transport.sentPendingAutoAbilityQueues.Count);
            Assert.AreEqual(0, transport.sentPendingAutoAbilityManualResolutionDecisions.Count);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PreviewApplyLatestPendingAutoAbilityManualResolutionDecisionFlowLogsValidationRejection()
        {
            GameState initial = CreateState(1038);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p1");
            string before = session.State.ToJson();
            NetworkPendingAutoAbilityManualResolutionDecisionPayload decisionPayload =
                CreateRawPendingAutoAbilityManualResolutionDecisionPayload(
                    room,
                    "pending-1",
                    "UnsupportedDecision");
            PendingAutoAbilityManualResolutionDecisionValidationResult validation =
                PendingAutoAbilityManualResolutionDecisionValidator.Validate(decisionPayload);

            transport.EmitPendingAutoAbilityQueue(CreatePlayerPendingAutoAbilityQueuePayload(room));
            transport.EmitPendingAutoAbilityManualResolutionDecision(decisionPayload);
            PendingAutoAbilityManualResolutionApplyExecutorResult result =
                session.PreviewApplyLatestPendingAutoAbilityManualResolutionDecision();

            Assert.IsFalse(validation.accepted);
            Assert.AreEqual(PendingAutoAbilityManualResolutionDecisionFactory.DecisionTypeInvalidReason, validation.rejection_reason);
            Assert.IsFalse(result.accepted);
            Assert.AreEqual(validation.rejection_reason, result.rejection_reason);
            Assert.NotNull(result.apply_result);
            Assert.IsFalse(result.apply_result.accepted);
            Assert.AreEqual(validation.rejection_reason, result.apply_result.rejection_reason);
            Assert.AreEqual(1, session.PendingAutoAbilityQueuePayloads.Count);
            Assert.AreEqual(1, session.PendingAutoAbilityManualResolutionApplyPreviewLogs.Count);
            Assert.IsFalse(session.LatestPendingAutoAbilityManualResolutionApplyPreviewLog.accepted);
            Assert.AreEqual(
                validation.rejection_reason,
                session.LatestPendingAutoAbilityManualResolutionApplyPreviewLog.rejection_reason);
            Assert.AreEqual(0, transport.sentPendingAutoAbilityQueues.Count);
            Assert.AreEqual(0, transport.sentPendingAutoAbilityManualResolutionDecisions.Count);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PublishLatestTriggerCheckReplayLogRejectsWhenNoPayloadExists()
        {
            GameState initial = CreateState(1010);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");

            MultiplayerTransportResult result = session.PublishLatestTriggerCheckReplayLog();

            Assert.IsFalse(result.ok);
            Assert.AreEqual("TRIGGER_CHECK_REPLAY_LOG_MISSING", result.error_code);
            Assert.AreEqual(0, transport.sentTriggerCheckLogs.Count);
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PublishLatestTriggerCheckReplayLogSendsStoredPayloadWithoutMutatingState()
        {
            GameState initial = CreateState(1011);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p2");
            NetworkTriggerCheckReplayLogPayload payload = CreateTriggerCheckPayload(room, initial);
            string before = session.State.ToJson();

            transport.EmitTriggerCheckReplayLog(payload);
            MultiplayerTransportResult result = session.PublishLatestTriggerCheckReplayLog();

            Assert.IsTrue(result.ok);
            Assert.AreEqual(1, transport.sentTriggerCheckLogs.Count);
            Assert.AreEqual(payload.payload_id, transport.sentTriggerCheckLogs[0].payload_id);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PublishManualTriggerCheckDraftFillsSessionContextAndStoresSentPayload()
        {
            GameState initial = CreateState(1012);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p1");
            ManualTriggerCheckDraftRequest request = CreateDraftRequest();
            request.room_id = "CALLER-SHOULD-NOT-CONTROL-ROOM";
            request.sender_player_id = "CALLER-SHOULD-NOT-CONTROL-SENDER";
            string before = session.State.ToJson();

            ManualTriggerCheckDraftResult result = session.PublishManualTriggerCheckDraft(request);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.sent, result.transport_message);
            Assert.AreEqual(1, transport.sentTriggerCheckLogs.Count);
            Assert.AreEqual(1, session.TriggerCheckReplayLogPayloads.Count);
            Assert.AreEqual(room.room_id, transport.sentTriggerCheckLogs[0].room_id);
            Assert.AreEqual("p1", transport.sentTriggerCheckLogs[0].sender_player_id);
            Assert.AreEqual("CALLER-SHOULD-NOT-CONTROL-ROOM", request.room_id);
            Assert.AreEqual("CALLER-SHOULD-NOT-CONTROL-SENDER", request.sender_player_id);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PublishManualTriggerCheckDraftRejectsInvalidInputWithoutSending()
        {
            GameState initial = CreateState(1013);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p1");
            ManualTriggerCheckDraftRequest request = CreateDraftRequest();
            request.checked_card_id = "";

            ManualTriggerCheckDraftResult result = session.PublishManualTriggerCheckDraft(request);

            Assert.IsFalse(result.accepted);
            Assert.IsFalse(result.sent);
            Assert.AreEqual("CHECKED_CARD_ID_MISSING", result.rejection_reason);
            Assert.AreEqual(0, transport.sentTriggerCheckLogs.Count);
            Assert.AreEqual(0, session.TriggerCheckReplayLogPayloads.Count);
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        [Test]
        public void PublishManualTriggerCheckDraftSendsDeterministicPayloads()
        {
            GameState initial = CreateState(1014);
            MultiplayerRoomState room = CreateRoom();
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(transport, room, initial, "p1");
            ManualTriggerCheckDraftRequest request = CreateDraftRequest();

            ManualTriggerCheckDraftResult first = session.PublishManualTriggerCheckDraft(request);
            ManualTriggerCheckDraftResult second = session.PublishManualTriggerCheckDraft(request);

            Assert.IsTrue(first.sent);
            Assert.IsTrue(second.sent);
            Assert.AreEqual(2, transport.sentTriggerCheckLogs.Count);
            Assert.AreEqual(transport.sentTriggerCheckLogs[0].ToJson(), transport.sentTriggerCheckLogs[1].ToJson());
            Assert.AreEqual(2, session.TriggerCheckReplayLogPayloads.Count);
            Assert.AreEqual(0, session.State.event_log.Count);
        }

        private static GameState CreateState(int seed)
        {
            return GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), seed);
        }

        private static MultiplayerRoomState CreateRoom()
        {
            return MultiplayerProtocol.CreateRoom("ROOM-GAME", "D", "p1", 1001, new PackSyncInfo
            {
                pack_id = "vanguard_th",
                source_version = "test",
                definition_hash = "definition",
                image_manifest_hash = "image-manifest",
                image_content_hash = "image-content"
            });
        }

        private static VanguardDeck CreateSampleDeck(string prefix)
        {
            VanguardDeck deck = VanguardDeck.Create(prefix + " deck", "D", "vanguard_th", "test");
            for (int i = 0; i < 50; i++)
            {
                deck.AddCard(DeckZone.Main, prefix + "-MAIN-" + i, 1);
            }

            for (int i = 0; i < 4; i++)
            {
                deck.AddCard(DeckZone.Ride, prefix + "-RIDE-" + i, 1);
            }

            return deck;
        }

        private static LegalGameAction FirstAction(IReadOnlyList<LegalGameAction> actions, GameActionType actionType)
        {
            foreach (LegalGameAction action in actions)
            {
                if (action.action_type == actionType)
                {
                    return action;
                }
            }

            Assert.Fail("Missing action " + actionType);
            return null;
        }

        private static LegalGameAction FirstPhase(IReadOnlyList<LegalGameAction> actions, GamePhase phase)
        {
            foreach (LegalGameAction action in actions)
            {
                if (action.action_type == GameActionType.SetPhase && action.phase == phase)
                {
                    return action;
                }
            }

            Assert.Fail("Missing phase " + phase);
            return null;
        }

        private static LegalGameAction FirstMove(IReadOnlyList<LegalGameAction> actions, GameZone from, GameZone to)
        {
            foreach (LegalGameAction action in actions)
            {
                if (action.action_type == GameActionType.MoveCard && action.from_zone == from && action.to_zone == to)
                {
                    return action;
                }
            }

            Assert.Fail("Missing move " + from + " to " + to);
            return null;
        }

        private static NetworkTriggerCheckReplayLogPayload CreateTriggerCheckPayload(MultiplayerRoomState room, GameState state)
        {
            TriggerCheckReplayLog log = TriggerCheckReplayLogBuilder.Append(
                null,
                TriggerCheckLogEntryFactory.FromBundle(
                    TriggerCheckResolutionBundler.Build(
                        state,
                        0,
                        TriggerCheckSource.Drive,
                        0,
                        "drive-card-1",
                        "CRIT-001",
                        TriggerType.Critical,
                        CombatModifierExpiration.EndOfTurn)));
            TriggerCheckReplayLog maskedLog = TriggerCheckReplayLogMasker.CreateView(log, GameStateViewPerspective.Spectator);

            return TriggerCheckReplayLogPayloadCodec.Encode(
                room.room_id,
                "p1",
                maskedLog,
                GameStateViewPerspective.Spectator);
        }

        private static NetworkPendingAutoAbilityQueuePayload CreatePendingAutoAbilityQueuePayload(MultiplayerRoomState room)
        {
            var queue = new PendingAutoAbilityQueue
            {
                queue_id = "queue-1",
                pending = new List<PendingAutoAbility>
                {
                    new PendingAutoAbility
                    {
                        pending_id = "pending-1",
                        source_card_instance_id = "src-1",
                        source_card_id = "CARD-1",
                        player_index = 0,
                        timing_event = "OnDraw",
                        summary = "CARD-1 ability"
                    }
                }
            };

            PendingAutoAbilityQueue maskedQueue =
                PendingAutoAbilityQueueMasker.CreateView(queue, GameStateViewPerspective.Spectator);
            return PendingAutoAbilityQueuePayloadCodec.Encode(
                room.room_id,
                0,
                maskedQueue,
                GameStateViewPerspective.Spectator);
        }

        private static NetworkPendingAutoAbilityQueuePayload CreatePlayerPendingAutoAbilityQueuePayload(
            MultiplayerRoomState room)
        {
            var queue = new PendingAutoAbilityQueue
            {
                queue_id = "queue-1",
                pending = new List<PendingAutoAbility>
                {
                    new PendingAutoAbility
                    {
                        pending_id = "pending-1",
                        source_card_instance_id = "src-1",
                        source_card_id = "CARD-1",
                        player_index = 0,
                        timing_event = "OnDraw",
                        summary = "CARD-1 ability"
                    }
                }
            };

            PendingAutoAbilityQueue playerQueue =
                PendingAutoAbilityQueueMasker.CreateView(queue, GameStateViewPerspective.Player, 0);
            return PendingAutoAbilityQueuePayloadCodec.Encode(
                room.room_id,
                0,
                playerQueue,
                GameStateViewPerspective.Player,
                0);
        }

        private static NetworkPendingAutoAbilityResolutionRequestPayload CreatePendingAutoAbilityResolutionRequestPayload(
            MultiplayerRoomState room)
        {
            return PendingAutoAbilityResolutionRequestPayloadCodec.Encode(
                room.room_id,
                0,
                new PendingAutoAbilityResolutionRequest
                {
                    selected_index = 0,
                    pending_id = "pending-1",
                    player_index = 0,
                    timing_event = "OnDraw",
                    source_card_instance_id = "src-1",
                    source_card_id = "CARD-1",
                    summary = "Resolve CARD-1"
                },
                GameStateViewPerspective.Player,
                0);
        }

        private static NetworkPendingAutoAbilityManualResolutionDecisionPayload CreatePendingAutoAbilityManualResolutionDecisionPayload(
            MultiplayerRoomState room)
        {
            return CreatePendingAutoAbilityManualResolutionDecisionPayload(
                room,
                PendingAutoAbilityManualResolutionDecisionTypes.Resolve);
        }

        private static NetworkPendingAutoAbilityManualResolutionDecisionPayload CreatePendingAutoAbilityManualResolutionDecisionPayload(
            MultiplayerRoomState room,
            string decisionType)
        {
            PendingAutoAbilityManualResolutionDecisionResult result =
                PendingAutoAbilityManualResolutionDecisionFactory.Create(
                    new PendingAutoAbilityResolutionRequest
                    {
                        selected_index = 0,
                        pending_id = "pending-1",
                        player_index = 0,
                        timing_event = "OnDraw",
                        source_card_instance_id = "src-1",
                        source_card_id = "CARD-1",
                        summary = "Resolve CARD-1"
                    },
                    decisionType,
                    "manual " + decisionType);

            return PendingAutoAbilityManualResolutionDecisionPayloadCodec.Encode(
                room.room_id,
                0,
                result.decision,
                GameStateViewPerspective.Player,
                0);
        }

        private static NetworkPendingAutoAbilityManualResolutionDecisionPayload CreateRawPendingAutoAbilityManualResolutionDecisionPayload(
            MultiplayerRoomState room,
            string pendingId,
            string decisionType)
        {
            return PendingAutoAbilityManualResolutionDecisionPayloadCodec.Encode(
                room.room_id,
                0,
                new PendingAutoAbilityManualResolutionDecision
                {
                    decision_id = "manual-decision|" + pendingId + "|" + decisionType,
                    decision_type = decisionType,
                    selected_index = 0,
                    pending_id = pendingId,
                    player_index = 0,
                    timing_event = "OnDraw",
                    source_card_instance_id = "src-1",
                    source_card_id = "CARD-1",
                    reason = "raw " + decisionType,
                    summary = "Raw decision " + decisionType
                },
                GameStateViewPerspective.Player,
                0);
        }

        private static ManualTriggerCheckDraftRequest CreateDraftRequest()
        {
            return new ManualTriggerCheckDraftRequest
            {
                player_index = 0,
                check_source = TriggerCheckSource.Drive,
                check_index = 0,
                checked_card_instance_id = "drive-card-1",
                checked_card_id = "CRIT-001",
                trigger_type = TriggerType.Critical,
                modifier_expiration = CombatModifierExpiration.EndOfTurn,
                perspective = GameStateViewPerspective.Spectator,
                viewer_player_index = -1
            };
        }

        private sealed class FakeTransport : IMultiplayerTransport
        {
            public readonly List<NetworkEventEnvelope> sentEvents = new List<NetworkEventEnvelope>();
            public readonly List<NetworkPublicGameEvent> sentPublicEvents = new List<NetworkPublicGameEvent>();
            public readonly List<NetworkTriggerCheckReplayLogPayload> sentTriggerCheckLogs = new List<NetworkTriggerCheckReplayLogPayload>();
            public readonly List<NetworkPendingAutoAbilityQueuePayload> sentPendingAutoAbilityQueues = new List<NetworkPendingAutoAbilityQueuePayload>();
            public readonly List<NetworkPendingAutoAbilityResolutionRequestPayload> sentPendingAutoAbilityResolutionRequests =
                new List<NetworkPendingAutoAbilityResolutionRequestPayload>();
            public readonly List<NetworkPendingAutoAbilityManualResolutionDecisionPayload> sentPendingAutoAbilityManualResolutionDecisions =
                new List<NetworkPendingAutoAbilityManualResolutionDecisionPayload>();
            public NetworkEventBatch sentReconnectBatch;

            public string TransportName
            {
                get { return "Fake"; }
            }

            public MultiplayerTransportStatus Status
            {
                get { return MultiplayerTransportStatus.InRoom; }
            }

            public string LastError
            {
                get { return null; }
            }

            public event Action<MultiplayerRoomState> RoomStateReceived;
            public event Action<NetworkEventEnvelope> GameEventReceived;
            public event Action<NetworkPublicGameEvent> PublicGameEventReceived;
            public event Action<NetworkTriggerCheckReplayLogPayload> TriggerCheckReplayLogReceived;
            public event Action<NetworkPendingAutoAbilityQueuePayload> PendingAutoAbilityQueueReceived;
            public event Action<NetworkPendingAutoAbilityResolutionRequestPayload> PendingAutoAbilityResolutionRequestReceived;
            public event Action<NetworkPendingAutoAbilityManualResolutionDecisionPayload> PendingAutoAbilityManualResolutionDecisionReceived;
            public event Action<NetworkReconnectRequest> ReconnectRequestReceived;
            public event Action<NetworkEventBatch> ReconnectBatchReceived;
            public event Action<NetworkDeckRevealRequest> DeckRevealRequestReceived;
            public event Action<NetworkDeckRevealResponse> DeckRevealResponseReceived;

            public MultiplayerTransportResult Connect()
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult CreateRoom(MultiplayerRoomState room, RoomPlayerInfo localPlayer)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult JoinRoom(string roomId, RoomPlayerInfo localPlayer, PackSyncInfo localPack)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendRoomState(MultiplayerRoomState room)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendGameEvent(NetworkEventEnvelope envelope)
            {
                sentEvents.Add(envelope);
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPublicGameEvent(NetworkPublicGameEvent publicEvent)
            {
                sentPublicEvents.Add(publicEvent);
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendTriggerCheckReplayLog(NetworkTriggerCheckReplayLogPayload payload)
            {
                sentTriggerCheckLogs.Add(payload);
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPendingAutoAbilityQueue(NetworkPendingAutoAbilityQueuePayload payload)
            {
                sentPendingAutoAbilityQueues.Add(payload);
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPendingAutoAbilityResolutionRequest(
                NetworkPendingAutoAbilityResolutionRequestPayload payload)
            {
                sentPendingAutoAbilityResolutionRequests.Add(payload);
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendPendingAutoAbilityManualResolutionDecision(
                NetworkPendingAutoAbilityManualResolutionDecisionPayload payload)
            {
                sentPendingAutoAbilityManualResolutionDecisions.Add(payload);
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendReconnectRequest(NetworkReconnectRequest request)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendReconnectBatch(NetworkEventBatch batch)
            {
                sentReconnectBatch = batch;
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendDeckRevealRequest(NetworkDeckRevealRequest request)
            {
                return MultiplayerTransportResult.Ok();
            }

            public MultiplayerTransportResult SendDeckRevealResponse(NetworkDeckRevealResponse response)
            {
                return MultiplayerTransportResult.Ok();
            }

            public void Tick()
            {
            }

            public void Disconnect()
            {
            }

            public void EmitGameEvent(NetworkEventEnvelope envelope)
            {
                Action<NetworkEventEnvelope> handler = GameEventReceived;
                if (handler != null)
                {
                    handler(envelope);
                }
            }

            public void EmitPublicGameEvent(NetworkPublicGameEvent publicEvent)
            {
                Action<NetworkPublicGameEvent> handler = PublicGameEventReceived;
                if (handler != null)
                {
                    handler(publicEvent);
                }
            }

            public void EmitTriggerCheckReplayLog(NetworkTriggerCheckReplayLogPayload payload)
            {
                Action<NetworkTriggerCheckReplayLogPayload> handler = TriggerCheckReplayLogReceived;
                if (handler != null)
                {
                    handler(payload);
                }
            }

            public void EmitPendingAutoAbilityQueue(NetworkPendingAutoAbilityQueuePayload payload)
            {
                Action<NetworkPendingAutoAbilityQueuePayload> handler = PendingAutoAbilityQueueReceived;
                if (handler != null)
                {
                    handler(payload);
                }
            }

            public void EmitPendingAutoAbilityResolutionRequest(
                NetworkPendingAutoAbilityResolutionRequestPayload payload)
            {
                Action<NetworkPendingAutoAbilityResolutionRequestPayload> handler =
                    PendingAutoAbilityResolutionRequestReceived;
                if (handler != null)
                {
                    handler(payload);
                }
            }

            public void EmitPendingAutoAbilityManualResolutionDecision(
                NetworkPendingAutoAbilityManualResolutionDecisionPayload payload)
            {
                Action<NetworkPendingAutoAbilityManualResolutionDecisionPayload> handler =
                    PendingAutoAbilityManualResolutionDecisionReceived;
                if (handler != null)
                {
                    handler(payload);
                }
            }

            public void EmitReconnectRequest(NetworkReconnectRequest request)
            {
                Action<NetworkReconnectRequest> handler = ReconnectRequestReceived;
                if (handler != null)
                {
                    handler(request);
                }
            }

            public void EmitReconnectBatch(NetworkEventBatch batch)
            {
                Action<NetworkEventBatch> handler = ReconnectBatchReceived;
                if (handler != null)
                {
                    handler(batch);
                }
            }
        }
    }
}
