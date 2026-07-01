using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.UI;
using UnityEngine;
using UnityEngine.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class GameStateTests
    {
        [Test]
        public void FactoryCreatesTwoPlayerGameAndDrawsOpeningHands()
        {
            VanguardDeck playerOne = CreateSampleDeck("p1");
            VanguardDeck playerTwo = CreateSampleDeck("p2");

            GameState state = GameStateFactory.CreateTwoPlayerGame(playerOne, playerTwo, 1234);

            Assert.AreEqual(2, state.players.Count);
            Assert.AreEqual(GamePhase.Mulligan, state.phase);
            Assert.AreEqual(1, state.turn_number);
            Assert.AreEqual(0, state.turn_player_index);
            Assert.AreEqual(45, state.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(5, state.GetPlayer(0).CountZone(GameZone.Hand));
            Assert.AreEqual(4, state.GetPlayer(0).CountZone(GameZone.RideDeck));
            Assert.AreEqual(45, state.GetPlayer(1).CountZone(GameZone.Deck));
            Assert.AreEqual(5, state.GetPlayer(1).CountZone(GameZone.Hand));
        }

        [Test]
        public void FactoryShuffleIsDeterministicForSeed()
        {
            VanguardDeck playerOne = CreateSampleDeck("p1");
            VanguardDeck playerTwo = CreateSampleDeck("p2");

            GameState first = GameStateFactory.CreateTwoPlayerGame(playerOne, playerTwo, 777);
            GameState second = GameStateFactory.CreateTwoPlayerGame(playerOne, playerTwo, 777);

            for (int i = 0; i < GameStateFactory.OpeningHandSize; i++)
            {
                Assert.AreEqual(
                    first.GetPlayer(0).hand[i].card_id,
                    second.GetPlayer(0).hand[i].card_id);
                Assert.AreEqual(
                    first.GetPlayer(1).hand[i].card_id,
                    second.GetPlayer(1).hand[i].card_id);
            }
        }

        [Test]
        public void GameStateSerializesRoundTrip()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                42);

            string json = state.ToJson();
            GameState roundTrip = GameState.FromJson(json);

            Assert.AreEqual(state.random_seed, roundTrip.random_seed);
            Assert.AreEqual(state.phase, roundTrip.phase);
            Assert.AreEqual(2, roundTrip.players.Count);
            Assert.AreEqual(45, roundTrip.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(5, roundTrip.GetPlayer(0).CountZone(GameZone.Hand));
        }

        [Test]
        public void DrawCreatesEventAndUndoRestoresState()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                100);
            string nextCard = state.GetPlayer(0).deck[0].instance_id;

            GameEvent gameEvent = GameActionService.Draw(state, 0);

            Assert.AreEqual(GameActionType.Draw, gameEvent.action_type);
            Assert.AreEqual(nextCard, gameEvent.card_instance_id);
            Assert.AreEqual(44, state.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(6, state.GetPlayer(0).CountZone(GameZone.Hand));
            Assert.AreEqual(1, state.event_log.Count);

            Assert.IsTrue(GameActionService.UndoLast(state));
            Assert.AreEqual(45, state.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(5, state.GetPlayer(0).CountZone(GameZone.Hand));
            Assert.AreEqual(nextCard, state.GetPlayer(0).deck[0].instance_id);
            Assert.AreEqual(0, state.event_log.Count);
        }

        [Test]
        public void MoveCardCreatesEventAndUndoRestoresState()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                101);
            string cardId = state.GetPlayer(0).hand[0].instance_id;

            GameEvent gameEvent = GameActionService.MoveCard(
                state,
                0,
                cardId,
                GameZone.Hand,
                GameZone.Vanguard);

            Assert.AreEqual(GameActionType.MoveCard, gameEvent.action_type);
            Assert.AreEqual(4, state.GetPlayer(0).CountZone(GameZone.Hand));
            Assert.AreEqual(1, state.GetPlayer(0).CountZone(GameZone.Vanguard));
            Assert.AreEqual(cardId, state.GetPlayer(0).vanguard[0].instance_id);

            Assert.IsTrue(GameActionService.UndoLast(state));
            Assert.AreEqual(5, state.GetPlayer(0).CountZone(GameZone.Hand));
            Assert.AreEqual(0, state.GetPlayer(0).CountZone(GameZone.Vanguard));
            Assert.AreEqual(cardId, state.GetPlayer(0).hand[0].instance_id);
        }

        [Test]
        public void MoveCardToVanguardMovesOldVanguardToSoulAndUndoRestores()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                1011);
            string firstRide = state.GetPlayer(0).hand[0].instance_id;
            GameActionService.MoveCard(state, 0, firstRide, GameZone.Hand, GameZone.Vanguard);
            string secondRide = state.GetPlayer(0).hand[0].instance_id;

            GameEvent gameEvent = GameActionService.MoveCard(
                state,
                0,
                secondRide,
                GameZone.Hand,
                GameZone.Vanguard);

            Assert.AreEqual(GameActionType.MoveCard, gameEvent.action_type);
            Assert.AreEqual(1, gameEvent.card_instance_ids.Count);
            Assert.AreEqual(firstRide, gameEvent.card_instance_ids[0]);
            Assert.AreEqual(1, state.GetPlayer(0).CountZone(GameZone.Vanguard));
            Assert.AreEqual(secondRide, state.GetPlayer(0).vanguard[0].instance_id);
            Assert.AreEqual(1, state.GetPlayer(0).CountZone(GameZone.Soul));
            Assert.AreEqual(firstRide, state.GetPlayer(0).soul[0].instance_id);

            Assert.IsTrue(GameActionService.UndoLast(state));
            Assert.AreEqual(1, state.GetPlayer(0).CountZone(GameZone.Vanguard));
            Assert.AreEqual(firstRide, state.GetPlayer(0).vanguard[0].instance_id);
            Assert.AreEqual(0, state.GetPlayer(0).CountZone(GameZone.Soul));
            Assert.AreEqual(secondRide, state.GetPlayer(0).hand[0].instance_id);
        }

        [Test]
        public void SoulChargeLegalActionDoesNotExposeTopDeckCardId()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                1012);
            string topDeck = state.GetPlayer(0).deck[0].instance_id;

            LegalGameAction soulCharge = FirstMove(
                RulesCore.GetLegalActions(state, 0),
                GameZone.Deck,
                GameZone.Soul);

            Assert.AreEqual(string.Empty, soulCharge.card_instance_id);
            Assert.IsFalse(soulCharge.label.Contains(topDeck));

            GameEvent gameEvent = RulesCore.ExecuteOrThrow(state, soulCharge);
            Assert.AreEqual(topDeck, gameEvent.card_instance_id);
            Assert.AreEqual(1, state.GetPlayer(0).CountZone(GameZone.Soul));
            Assert.AreEqual(topDeck, state.GetPlayer(0).soul[0].instance_id);
        }

        [Test]
        public void SetPhaseCreatesEventAndUndoRestoresPhase()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                102);

            GameActionService.SetPhase(state, 0, GamePhase.Main);

            Assert.AreEqual(GamePhase.Main, state.phase);
            Assert.AreEqual(1, state.event_log.Count);
            Assert.IsTrue(GameActionService.UndoLast(state));
            Assert.AreEqual(GamePhase.Mulligan, state.phase);
            Assert.AreEqual(0, state.event_log.Count);
        }

        [Test]
        public void GiftMarkerCreatesEventAndUndoRestoresCount()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                1021);

            GameEvent giftEvent = GameActionService.AddGiftMarker(state, 0, GiftMarkerType.Force);

            Assert.AreEqual(GameActionType.AddGiftMarker, giftEvent.action_type);
            Assert.AreEqual(1, state.GetPlayer(0).GetGiftMarkerCount(GiftMarkerType.Force));
            Assert.AreEqual(1, state.event_log.Count);

            Assert.IsTrue(GameActionService.UndoLast(state));
            Assert.AreEqual(0, state.GetPlayer(0).GetGiftMarkerCount(GiftMarkerType.Force));
            Assert.AreEqual(0, state.event_log.Count);
        }

        [Test]
        public void MoveCardRejectsWrongSourceZone()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                103);
            string cardId = state.GetPlayer(0).hand[0].instance_id;

            Assert.Throws<System.InvalidOperationException>(() =>
                GameActionService.MoveCard(state, 0, cardId, GameZone.Deck, GameZone.Vanguard));
        }

        [Test]
        public void PlayTableBootstrapCreatesRuntimeUi()
        {
            PlayTableBootstrap.Show(CreateSampleDeck("p1"), CreateSampleDeck("p2"));
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject mulliganButtonObject = GameObject.Find("Mulligan Button");
            GameObject seatButtonObject = GameObject.Find("Seat P2 Button");
            GameObject setupStatusObject = GameObject.Find("PlayTable Setup Status");
            GameObject advancedButtonObject = GameObject.Find("Advanced Button");
            GameObject advancedDrawerObject = GameObject.Find("Advanced Drawer");
            GameObject triggerDraftObject = GameObject.Find("Trigger Draft Summary");
            GameObject pendingAbilityObject = GameObject.Find("Pending Ability Summary");
            GameObject exportReplayButtonObject = GameObject.Find("Export Replay Button");
            GameObject localVanguardSlotObject = GameObject.Find("Local Vanguard Slot");
            GameObject opponentVanguardSlotObject = GameObject.Find("Opponent Vanguard Slot");
            GameObject localBackCenterRearGuardSlotObject = GameObject.Find("Local Back Center RG Slot");
            GameObject localRearGuardContentObject = GameObject.Find("Local Rear-guard Card Content");
            GameObject opponentRearGuardContentObject = GameObject.Find("Opponent Rear-guard Card Content");
            GameObject handContentObject = GameObject.Find("Hand Content");

            Assert.NotNull(table);
            Assert.NotNull(mulliganButtonObject);
            Assert.NotNull(seatButtonObject);
            Assert.NotNull(setupStatusObject);
            Assert.NotNull(advancedButtonObject);
            Assert.NotNull(advancedDrawerObject);
            Assert.NotNull(triggerDraftObject);
            Assert.NotNull(pendingAbilityObject);
            Assert.NotNull(exportReplayButtonObject);
            Assert.NotNull(localVanguardSlotObject);
            Assert.NotNull(opponentVanguardSlotObject);
            Assert.NotNull(localBackCenterRearGuardSlotObject);
            Assert.NotNull(localRearGuardContentObject);
            Assert.NotNull(opponentRearGuardContentObject);
            Assert.NotNull(handContentObject);
            HorizontalLayoutGroup handLayout = handContentObject.GetComponent<HorizontalLayoutGroup>();
            Assert.NotNull(handLayout);
            Assert.AreEqual(TextAnchor.MiddleLeft, handLayout.childAlignment);
            Assert.IsFalse(handLayout.childControlHeight);
            Assert.IsFalse(handLayout.childForceExpandHeight);
            LayoutElement handContentLayout = handContentObject.GetComponent<LayoutElement>();
            Assert.NotNull(handContentLayout);
            Assert.GreaterOrEqual(handContentLayout.preferredHeight, PlayTableHandStripFormatter.CardButtonHeight);
            Button[] handButtons = handContentObject.GetComponentsInChildren<Button>(true);
            Assert.GreaterOrEqual(handButtons.Length, 1);
            LayoutElement handCardLayout = handButtons[0].GetComponent<LayoutElement>();
            Assert.NotNull(handCardLayout);
            Assert.GreaterOrEqual(handCardLayout.preferredHeight, PlayTableHandStripFormatter.CardButtonHeight);
            Assert.AreEqual(advancedDrawerObject.transform, triggerDraftObject.transform.parent);
            Assert.AreEqual(advancedDrawerObject.transform, pendingAbilityObject.transform.parent);
            Assert.IsTrue(IsDescendantOf(exportReplayButtonObject.transform, advancedDrawerObject.transform));
            Assert.AreEqual(0f, advancedDrawerObject.GetComponent<CanvasGroup>().alpha);
            Assert.IsTrue(advancedDrawerObject.GetComponent<LayoutElement>().ignoreLayout);
            Assert.IsFalse(mulliganButtonObject.GetComponent<Button>().interactable);
            GameState displayState = table.CreateDisplayStateView();
            Assert.NotNull(displayState);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, displayState.GetPlayer(0).deck[0].card_id);
            Assert.AreNotEqual(GameStateViewFactory.HiddenCardId, displayState.GetPlayer(0).hand[0].card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, displayState.GetPlayer(1).hand[0].card_id);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void LocalPlayTableSeatToggleSwitchesPerspectiveWithoutEvents()
        {
            PlayTableBootstrap.Show(CreateSampleDeck("p1"), CreateSampleDeck("p2"));
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject seatButtonObject = GameObject.Find("Seat P2 Button");

            Assert.NotNull(table);
            Assert.NotNull(seatButtonObject);
            Assert.AreEqual(0, table.CurrentPlayerIndex);

            Button seatButton = seatButtonObject.GetComponent<Button>();
            Assert.NotNull(seatButton);
            Assert.IsTrue(seatButton.interactable);

            seatButton.onClick.Invoke();

            Assert.AreEqual(1, table.CurrentPlayerIndex);
            Assert.AreEqual(0, table.CreateDisplayStateView().event_log.Count);
            Assert.IsTrue(table.CreateSelectionMessage().Contains("Manual seat: P2"));
            Assert.AreEqual("Seat P1", seatButton.GetComponentInChildren<Text>().text);

            seatButton.onClick.Invoke();

            Assert.AreEqual(0, table.CurrentPlayerIndex);
            Assert.AreEqual(0, table.CreateDisplayStateView().event_log.Count);
            Assert.IsTrue(table.CreateSelectionMessage().Contains("Manual seat: P1"));
            Assert.AreEqual("Seat P2", seatButton.GetComponentInChildren<Text>().text);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableSeatToggleIsLocked()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 880);
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                CreateRoom(),
                initialState,
                "p1");

            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject seatButtonObject = GameObject.Find("Seat P2 Button");

            Assert.NotNull(table);
            Assert.NotNull(seatButtonObject);
            Assert.AreEqual(0, table.CurrentPlayerIndex);
            Assert.AreEqual(0, session.State.event_log.Count);

            Button seatButton = seatButtonObject.GetComponent<Button>();
            Assert.NotNull(seatButton);
            Assert.IsFalse(seatButton.interactable);
            Assert.AreEqual("Seat Lock", seatButton.GetComponentInChildren<Text>().text);

            seatButton.onClick.Invoke();

            Assert.AreEqual(0, table.CurrentPlayerIndex);
            Assert.AreEqual(0, session.State.event_log.Count);
            Assert.IsTrue(table.CreateSelectionMessage().Contains("online mode"));

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTablePublishesDrawThroughSession()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 881);
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                CreateRoom(),
                initialState,
                "p1");

            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject drawButtonObject = GameObject.Find("Draw Button");

            Assert.NotNull(table);
            Assert.NotNull(drawButtonObject);

            Button drawButton = drawButtonObject.GetComponent<Button>();
            Assert.NotNull(drawButton);
            drawButton.onClick.Invoke();

            Assert.AreEqual(1, transport.sentEvents.Count);
            Assert.AreEqual(GameActionType.Draw, transport.sentEvents[0].game_event.action_type);
            Assert.AreEqual(1, session.State.event_log.Count);
            Assert.AreEqual(44, session.State.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(6, session.State.GetPlayer(0).CountZone(GameZone.Hand));

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableShowsTriggerCheckReplaySummaryWithoutMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 882);
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                CreateRoom(),
                initialState,
                "p1");
            string before = session.State.ToJson();
            transport.EmitTriggerCheckReplayLog(CreateTriggerCheckPayload(CreateRoom(), initialState));

            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject summaryObject = GameObject.Find("Trigger Check Summary");

            Assert.NotNull(table);
            Assert.NotNull(summaryObject);
            Assert.IsTrue(table.CreateTriggerCheckLogSummary().Contains("Trigger panel"));
            Assert.IsTrue(table.CreateTriggerCheckLogSummary().Contains("Logs: 1"));
            Assert.IsTrue(table.CreateTriggerCheckLogSummary().Contains("Critical"));
            Assert.AreEqual(before, session.State.ToJson());

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableDisablesTriggerCheckPublishWhenNoPayloadExists()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 883);
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                CreateRoom(),
                initialState,
                "p1");

            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject publishButtonObject = GameObject.Find("TrigLog Button");

            Assert.NotNull(table);
            Assert.NotNull(publishButtonObject);

            Button publishButton = publishButtonObject.GetComponent<Button>();
            Assert.NotNull(publishButton);
            Assert.IsFalse(publishButton.interactable);
            Assert.AreEqual(0, transport.sentTriggerCheckLogs.Count);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableDisablesManualTriggerDraftUntilCardSelected()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 885);
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                CreateRoom(),
                initialState,
                "p1");

            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject draftButtonObject = GameObject.Find("DraftTrig Button");

            Assert.NotNull(table);
            Assert.NotNull(draftButtonObject);

            Button draftButton = draftButtonObject.GetComponent<Button>();
            Assert.NotNull(draftButton);
            Assert.IsFalse(draftButton.interactable);
            draftButton.onClick.Invoke();
            Assert.AreEqual(0, transport.sentTriggerCheckLogs.Count);
            Assert.AreEqual(0, session.TriggerCheckReplayLogPayloads.Count);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTablePublishesLatestTriggerCheckReplayLogWithoutMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 884);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            NetworkTriggerCheckReplayLogPayload payload = CreateTriggerCheckPayload(room, initialState);
            string before = session.State.ToJson();

            transport.EmitTriggerCheckReplayLog(payload);
            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject publishButtonObject = GameObject.Find("TrigLog Button");

            Assert.NotNull(table);
            Assert.NotNull(publishButtonObject);

            Button publishButton = publishButtonObject.GetComponent<Button>();
            Assert.NotNull(publishButton);
            Assert.IsTrue(publishButton.interactable);

            publishButton.onClick.Invoke();

            Assert.AreEqual(1, transport.sentTriggerCheckLogs.Count);
            Assert.AreEqual(payload.payload_id, transport.sentTriggerCheckLogs[0].payload_id);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTablePublishesManualTriggerDraftFromSelectedCardWithoutMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 886);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string selectedCardId = initialState.GetPlayer(0).hand[0].card_id;
            string before = session.State.ToJson();

            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject selectedCardButtonObject = GameObject.Find(selectedCardId + " Button");
            GameObject draftButtonObject = GameObject.Find("DraftTrig Button");

            Assert.NotNull(table);
            Assert.NotNull(selectedCardButtonObject);
            Assert.NotNull(draftButtonObject);

            selectedCardButtonObject.GetComponent<Button>().onClick.Invoke();
            Button draftButton = draftButtonObject.GetComponent<Button>();
            Assert.IsTrue(draftButton.interactable);

            draftButton.onClick.Invoke();

            Assert.AreEqual(1, transport.sentTriggerCheckLogs.Count);
            Assert.AreEqual(1, session.TriggerCheckReplayLogPayloads.Count);
            Assert.AreEqual(room.room_id, transport.sentTriggerCheckLogs[0].room_id);
            Assert.AreEqual("p1", transport.sentTriggerCheckLogs[0].sender_player_id);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);

            TriggerCheckReplayLog decoded;
            string rejectionReason;
            Assert.IsTrue(
                TriggerCheckReplayLogPayloadCodec.TryDecode(transport.sentTriggerCheckLogs[0], out decoded, out rejectionReason),
                rejectionReason);
            Assert.AreEqual(1, decoded.entries.Count);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, decoded.entries[0].checked_card_id);
            Assert.AreEqual(TriggerType.Unknown, decoded.entries[0].trigger_type);
            Assert.IsTrue(decoded.entries[0].needs_manual_resolution);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableCyclesManualTriggerDraftTypeBeforePublishing()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 887);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string selectedCardId = initialState.GetPlayer(0).hand[0].card_id;
            string before = session.State.ToJson();

            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject selectedCardButtonObject = GameObject.Find(selectedCardId + " Button");
            GameObject draftButtonObject = GameObject.Find("DraftTrig Button");
            GameObject typeButtonObject = GameObject.Find("TrigType Button");

            Assert.NotNull(table);
            Assert.NotNull(selectedCardButtonObject);
            Assert.NotNull(draftButtonObject);
            Assert.NotNull(typeButtonObject);
            Assert.IsTrue(typeButtonObject.GetComponentInChildren<Text>().text.Contains("Unknown"));

            typeButtonObject.GetComponent<Button>().onClick.Invoke();
            Assert.IsTrue(typeButtonObject.GetComponentInChildren<Text>().text.Contains("Crit"));
            selectedCardButtonObject.GetComponent<Button>().onClick.Invoke();
            draftButtonObject.GetComponent<Button>().onClick.Invoke();

            Assert.AreEqual(1, transport.sentTriggerCheckLogs.Count);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);

            TriggerCheckReplayLog decoded;
            string rejectionReason;
            Assert.IsTrue(
                TriggerCheckReplayLogPayloadCodec.TryDecode(transport.sentTriggerCheckLogs[0], out decoded, out rejectionReason),
                rejectionReason);
            Assert.AreEqual(TriggerType.Critical, decoded.entries[0].trigger_type);
            Assert.IsFalse(decoded.entries[0].needs_manual_resolution);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableCyclesManualDraftCheckSourceBeforePublishing()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 888);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string selectedCardId = initialState.GetPlayer(0).hand[0].card_id;
            string before = session.State.ToJson();

            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject selectedCardButtonObject = GameObject.Find(selectedCardId + " Button");
            GameObject draftButtonObject = GameObject.Find("DraftTrig Button");
            GameObject sourceButtonObject = GameObject.Find("ChkSrc Button");

            Assert.NotNull(table);
            Assert.NotNull(selectedCardButtonObject);
            Assert.NotNull(draftButtonObject);
            Assert.NotNull(sourceButtonObject);
            Assert.IsTrue(sourceButtonObject.GetComponentInChildren<Text>().text.Contains("Manual"));

            sourceButtonObject.GetComponent<Button>().onClick.Invoke();
            Assert.IsTrue(sourceButtonObject.GetComponentInChildren<Text>().text.Contains("Drive"));
            selectedCardButtonObject.GetComponent<Button>().onClick.Invoke();
            draftButtonObject.GetComponent<Button>().onClick.Invoke();

            Assert.AreEqual(1, transport.sentTriggerCheckLogs.Count);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);

            TriggerCheckReplayLog decoded;
            string rejectionReason;
            Assert.IsTrue(
                TriggerCheckReplayLogPayloadCodec.TryDecode(transport.sentTriggerCheckLogs[0], out decoded, out rejectionReason),
                rejectionReason);
            Assert.AreEqual(TriggerCheckSource.Drive, decoded.entries[0].check_source);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableCyclesManualDraftCheckIndexBeforePublishing()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 889);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string selectedCardId = initialState.GetPlayer(0).hand[0].card_id;
            string before = session.State.ToJson();

            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject selectedCardButtonObject = GameObject.Find(selectedCardId + " Button");
            GameObject draftButtonObject = GameObject.Find("DraftTrig Button");
            GameObject indexButtonObject = GameObject.Find("ChkIdx Button");

            Assert.NotNull(table);
            Assert.NotNull(selectedCardButtonObject);
            Assert.NotNull(draftButtonObject);
            Assert.NotNull(indexButtonObject);
            Assert.IsTrue(indexButtonObject.GetComponentInChildren<Text>().text.Contains("0"));

            indexButtonObject.GetComponent<Button>().onClick.Invoke();
            Assert.IsTrue(indexButtonObject.GetComponentInChildren<Text>().text.Contains("1"));
            selectedCardButtonObject.GetComponent<Button>().onClick.Invoke();
            draftButtonObject.GetComponent<Button>().onClick.Invoke();

            Assert.AreEqual(1, transport.sentTriggerCheckLogs.Count);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);

            TriggerCheckReplayLog decoded;
            string rejectionReason;
            Assert.IsTrue(
                TriggerCheckReplayLogPayloadCodec.TryDecode(transport.sentTriggerCheckLogs[0], out decoded, out rejectionReason),
                rejectionReason);
            Assert.AreEqual(1, decoded.entries[0].check_index);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableShowsManualDraftSummaryAndUpdatesWithoutSending()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 890);
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                CreateRoom(),
                initialState,
                "p1");
            string before = session.State.ToJson();

            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject summaryObject = GameObject.Find("Trigger Draft Summary");
            GameObject typeButtonObject = GameObject.Find("TrigType Button");
            GameObject sourceButtonObject = GameObject.Find("ChkSrc Button");
            GameObject indexButtonObject = GameObject.Find("ChkIdx Button");

            Assert.NotNull(table);
            Assert.NotNull(summaryObject);
            Assert.NotNull(typeButtonObject);
            Assert.NotNull(sourceButtonObject);
            Assert.NotNull(indexButtonObject);
            Assert.AreEqual("Draft: Unknown / Manual / idx 0 / card none / zone none", table.CreateTriggerDraftSummary());

            typeButtonObject.GetComponent<Button>().onClick.Invoke();
            sourceButtonObject.GetComponent<Button>().onClick.Invoke();
            indexButtonObject.GetComponent<Button>().onClick.Invoke();

            Assert.AreEqual("Draft: Critical / Drive / idx 1 / card none / zone none", table.CreateTriggerDraftSummary());
            Assert.AreEqual(0, transport.sentTriggerCheckLogs.Count);
            Assert.AreEqual(0, session.TriggerCheckReplayLogPayloads.Count);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableDraftSummaryIncludesSelectedCardWithoutSending()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 891);
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                CreateRoom(),
                initialState,
                "p1");
            string selectedCardId = initialState.GetPlayer(0).hand[0].card_id;
            string before = session.State.ToJson();

            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject selectedCardButtonObject = GameObject.Find(selectedCardId + " Button");

            Assert.NotNull(table);
            Assert.NotNull(selectedCardButtonObject);

            selectedCardButtonObject.GetComponent<Button>().onClick.Invoke();

            Assert.IsTrue(table.CreateTriggerDraftSummary().Contains("card " + selectedCardId));
            Assert.IsTrue(table.CreateTriggerDraftSummary().Contains("zone Hand"));
            Assert.AreEqual(0, transport.sentTriggerCheckLogs.Count);
            Assert.AreEqual(0, session.TriggerCheckReplayLogPayloads.Count);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableClearDraftSelectionResetsSummaryAndDisablesDraft()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 892);
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                CreateRoom(),
                initialState,
                "p1");
            string selectedCardId = initialState.GetPlayer(0).hand[0].card_id;
            string before = session.State.ToJson();

            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject selectedCardButtonObject = GameObject.Find(selectedCardId + " Button");
            GameObject draftButtonObject = GameObject.Find("DraftTrig Button");
            GameObject clearButtonObject = GameObject.Find("ClrDraft Button");

            Assert.NotNull(table);
            Assert.NotNull(selectedCardButtonObject);
            Assert.NotNull(draftButtonObject);
            Assert.NotNull(clearButtonObject);

            Button draftButton = draftButtonObject.GetComponent<Button>();
            Button clearButton = clearButtonObject.GetComponent<Button>();
            Assert.IsFalse(draftButton.interactable);
            Assert.IsFalse(clearButton.interactable);

            selectedCardButtonObject.GetComponent<Button>().onClick.Invoke();
            Assert.IsTrue(draftButton.interactable);
            Assert.IsTrue(clearButton.interactable);
            Assert.IsTrue(table.CreateTriggerDraftSummary().Contains("card " + selectedCardId));
            Assert.IsTrue(table.CreateTriggerDraftSummary().Contains("zone Hand"));

            clearButton.onClick.Invoke();

            Assert.IsFalse(draftButton.interactable);
            Assert.IsFalse(clearButton.interactable);
            Assert.IsTrue(table.CreateTriggerDraftSummary().Contains("card none"));
            Assert.IsTrue(table.CreateTriggerDraftSummary().Contains("zone none"));
            Assert.AreEqual(0, transport.sentTriggerCheckLogs.Count);
            Assert.AreEqual(0, session.TriggerCheckReplayLogPayloads.Count);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void LocalPlayTableReportsNoTriggerCheckReplayLogs()
        {
            PlayTableBootstrap.Show(CreateSampleDeck("p1"), CreateSampleDeck("p2"));
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();

            Assert.NotNull(table);
            Assert.AreEqual(PlayTableEventReplayPanelFormatter.EmptyPanelMessage, table.CreateEventReplayPanel());
            Assert.AreEqual(TriggerCheckPanelFormatter.LocalNoLogsMessage, table.CreateTriggerCheckLogSummary());
            Assert.AreEqual("Draft: local mode", table.CreateTriggerDraftSummary());
            Assert.AreEqual(PendingAutoAbilityPanelFormatter.NoPayloadsMessage, table.CreatePendingAutoAbilitySummary());
            Assert.AreEqual("Pending ability items: 0", table.CreatePendingAutoAbilityItemList());
            Assert.AreEqual("Pending selection: none", table.CreatePendingAutoAbilitySelectionStatus());
            Assert.AreEqual(
                PendingAutoAbilityResolutionRequestFormatter.NullRequestMessage,
                table.CreatePendingAutoAbilityResolutionRequestPreview());
            Assert.AreEqual(
                PendingAutoAbilityResolutionRequestSummaryFormatter.NoPayloadsMessage,
                table.CreatePendingAutoAbilityResolutionRequestSummary());
            Assert.AreEqual(
                PendingAutoAbilityResolutionRequestListFormatter.NoPayloadsMessage,
                table.CreatePendingAutoAbilityResolutionRequestList());
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionPanelFormatter.NoDecisionsMessage,
                table.CreatePendingAutoAbilityManualResolutionDecisionSummary());
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionListFormatter.NoPayloadsMessage,
                table.CreatePendingAutoAbilityManualResolutionDecisionList());
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionApplyPreviewLogFormatter.NullEntryMessage,
                table.CreatePendingAutoAbilityManualResolutionApplyPreviewLogLatest());
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionApplyPreviewLogFormatter.NoEntriesMessage,
                table.CreatePendingAutoAbilityManualResolutionApplyPreviewLogList());

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableShowsPendingAutoAbilitySummaryWithoutMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 893);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string before = session.State.ToJson();
            NetworkPendingAutoAbilityQueuePayload payload = CreatePendingAutoAbilityQueuePayload(room);

            transport.EmitPendingAutoAbilityQueue(payload);
            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject summaryObject = GameObject.Find("Pending Ability Summary");
            GameObject itemListObject = GameObject.Find("Pending Ability Items");
            GameObject selectionStatusObject = GameObject.Find("Pending Ability Selection Status");
            GameObject requestPreviewObject = GameObject.Find("Pending Ability Resolution Request Preview");
            GameObject requestSummaryObject = GameObject.Find("Pending Ability Resolution Request Summary");
            GameObject requestListObject = GameObject.Find("Pending Ability Resolution Request List");

            Assert.NotNull(table);
            Assert.NotNull(summaryObject);
            Assert.NotNull(itemListObject);
            Assert.NotNull(selectionStatusObject);
            Assert.NotNull(requestPreviewObject);
            Assert.NotNull(requestSummaryObject);
            Assert.NotNull(requestListObject);
            Assert.IsTrue(table.CreatePendingAutoAbilitySummary().Contains("AUTO queue"));
            Assert.IsTrue(table.CreatePendingAutoAbilitySummary().Contains("Payloads: 1"));
            Assert.IsTrue(table.CreatePendingAutoAbilitySummary().Contains(payload.payload_id));
            Assert.IsTrue(table.CreatePendingAutoAbilitySummary().Contains("Queue: queue-1"));
            Assert.IsTrue(table.CreatePendingAutoAbilitySummary().Contains("Pending: 1"));
            Assert.IsTrue(table.CreatePendingAutoAbilityItemList().Contains("Pending ability items: 1"));
            Assert.IsTrue(table.CreatePendingAutoAbilityItemList().Contains("pending-auto-hidden"));
            Assert.IsFalse(table.CreatePendingAutoAbilityItemList().Contains("pending-1"));
            Assert.IsTrue(table.CreatePendingAutoAbilityItemList().Contains("player="));
            Assert.IsTrue(table.CreatePendingAutoAbilityItemList().Contains("timing=OnDraw"));
            Assert.IsTrue(table.CreatePendingAutoAbilityItemList().Contains("source=hidden"));
            Assert.IsFalse(table.CreatePendingAutoAbilityItemList().Contains("CARD-1@"));
            Assert.AreEqual("Pending selection: none", table.CreatePendingAutoAbilitySelectionStatus());
            Assert.AreEqual(
                "Pending resolve request rejected: " +
                PendingAutoAbilityResolutionRequestFactory.SelectionMissingReason,
                table.CreatePendingAutoAbilityResolutionRequestPreview());
            Assert.AreEqual(
                PendingAutoAbilityResolutionRequestSummaryFormatter.NoPayloadsMessage,
                table.CreatePendingAutoAbilityResolutionRequestSummary());
            Assert.AreEqual(
                PendingAutoAbilityResolutionRequestListFormatter.NoPayloadsMessage,
                table.CreatePendingAutoAbilityResolutionRequestList());
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, transport.sentPendingAutoAbilityQueues.Count);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableDisablesPendingAutoAbilityPublishWhenNoPayloadExists()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 894);
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                CreateRoom(),
                initialState,
                "p1");

            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject publishButtonObject = GameObject.Find("AutoQ Button");

            Assert.NotNull(table);
            Assert.NotNull(publishButtonObject);

            Button publishButton = publishButtonObject.GetComponent<Button>();
            Assert.NotNull(publishButton);
            Assert.IsFalse(publishButton.interactable);
            GameObject selectionButtonObject = GameObject.Find("SelAuto Button");
            Assert.NotNull(selectionButtonObject);
            Assert.IsFalse(selectionButtonObject.GetComponent<Button>().interactable);
            GameObject clearSelectionButtonObject = GameObject.Find("ClrAuto Button");
            Assert.NotNull(clearSelectionButtonObject);
            Assert.IsFalse(clearSelectionButtonObject.GetComponent<Button>().interactable);
            GameObject requestButtonObject = GameObject.Find("ReqAuto Button");
            Assert.NotNull(requestButtonObject);
            Assert.IsFalse(requestButtonObject.GetComponent<Button>().interactable);
            GameObject decisionButtonObject = GameObject.Find("DecAuto Button");
            Assert.NotNull(decisionButtonObject);
            Assert.IsFalse(decisionButtonObject.GetComponent<Button>().interactable);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableCyclesPendingAutoAbilitySelectionWithoutMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 896);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string before = session.State.ToJson();
            NetworkPendingAutoAbilityQueuePayload payload = CreatePendingAutoAbilityQueuePayload(room, 2);

            transport.EmitPendingAutoAbilityQueue(payload);
            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject selectionButtonObject = GameObject.Find("SelAuto Button");

            Assert.NotNull(table);
            Assert.NotNull(selectionButtonObject);

            Button selectionButton = selectionButtonObject.GetComponent<Button>();
            Assert.NotNull(selectionButton);
            Assert.IsTrue(selectionButton.interactable);

            selectionButton.onClick.Invoke();

            Assert.IsTrue(table.CreatePendingAutoAbilitySelectionStatus().Contains("index=0"));
            Assert.IsTrue(table.CreatePendingAutoAbilitySelectionStatus().Contains("pending-auto-hidden"));
            Assert.IsTrue(table.CreatePendingAutoAbilitySelectionStatus().Contains("source=hidden"));
            Assert.IsFalse(table.CreatePendingAutoAbilitySelectionStatus().Contains("CARD-1@"));
            Assert.IsTrue(table.CreatePendingAutoAbilityResolutionRequestPreview().Contains("index=0"));
            Assert.IsTrue(table.CreatePendingAutoAbilityResolutionRequestPreview().Contains("pending-auto-hidden"));
            Assert.IsTrue(table.CreatePendingAutoAbilityResolutionRequestPreview().Contains("source=hidden"));
            Assert.IsFalse(table.CreatePendingAutoAbilityResolutionRequestPreview().Contains("CARD-1@"));

            selectionButton.onClick.Invoke();

            Assert.IsTrue(table.CreatePendingAutoAbilitySelectionStatus().Contains("index=1"));
            Assert.IsTrue(table.CreatePendingAutoAbilitySelectionStatus().Contains("source=hidden"));
            Assert.IsTrue(table.CreatePendingAutoAbilityResolutionRequestPreview().Contains("index=1"));
            Assert.IsTrue(table.CreatePendingAutoAbilityResolutionRequestPreview().Contains("source=hidden"));
            Assert.AreEqual(0, transport.sentPendingAutoAbilityQueues.Count);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTablePublishesPendingAutoAbilityResolutionRequestWithoutMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 898);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string before = session.State.ToJson();
            NetworkPendingAutoAbilityQueuePayload payload = CreatePendingAutoAbilityQueuePayload(room, 2);

            transport.EmitPendingAutoAbilityQueue(payload);
            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject selectionButtonObject = GameObject.Find("SelAuto Button");
            GameObject requestButtonObject = GameObject.Find("ReqAuto Button");

            Assert.NotNull(table);
            Assert.NotNull(selectionButtonObject);
            Assert.NotNull(requestButtonObject);

            Button selectionButton = selectionButtonObject.GetComponent<Button>();
            Button requestButton = requestButtonObject.GetComponent<Button>();
            Assert.NotNull(selectionButton);
            Assert.NotNull(requestButton);
            Assert.IsTrue(selectionButton.interactable);
            Assert.IsFalse(requestButton.interactable);

            selectionButton.onClick.Invoke();

            Assert.IsTrue(requestButton.interactable);

            requestButton.onClick.Invoke();

            Assert.AreEqual(1, transport.sentPendingAutoAbilityResolutionRequests.Count);
            Assert.AreEqual(1, session.PendingAutoAbilityResolutionRequestPayloads.Count);
            Assert.AreEqual(0, transport.sentPendingAutoAbilityQueues.Count);
            Assert.AreEqual(0, session.State.event_log.Count);
            Assert.AreEqual(before, session.State.ToJson());

            NetworkPendingAutoAbilityResolutionRequestPayload requestPayload =
                transport.sentPendingAutoAbilityResolutionRequests[0];
            Assert.AreEqual(room.room_id, requestPayload.room_id);
            Assert.AreEqual(0, requestPayload.sender_player_index);
            Assert.AreEqual(0, requestPayload.selected_index);
            Assert.AreEqual("pending-auto-hidden|0|OnDraw|0000", requestPayload.pending_id);
            Assert.AreEqual(GameStateViewPerspective.Player.ToString(), requestPayload.perspective);

            PendingAutoAbilityResolutionRequest request;
            string rejectionReason;
            Assert.IsTrue(
                PendingAutoAbilityResolutionRequestPayloadCodec.TryDecode(
                    requestPayload,
                    out request,
                    out rejectionReason),
                rejectionReason);
            Assert.AreEqual(0, request.selected_index);
            Assert.AreEqual("pending-auto-hidden|0|OnDraw|0000", request.pending_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, request.source_card_id);
            Assert.IsTrue(request.hides_source_card_identity);
            Assert.IsFalse(table.CreatePendingAutoAbilityResolutionRequestPreview().Contains("CARD-1@"));
            Assert.IsTrue(table.CreatePendingAutoAbilityResolutionRequestSummary().Contains("Pending resolve requests: 1"));
            Assert.IsTrue(table.CreatePendingAutoAbilityResolutionRequestSummary().Contains("index=0"));
            Assert.IsTrue(table.CreatePendingAutoAbilityResolutionRequestSummary().Contains("source=hidden"));
            Assert.IsFalse(table.CreatePendingAutoAbilityResolutionRequestSummary().Contains("CARD-1@"));
            Assert.IsTrue(table.CreatePendingAutoAbilityResolutionRequestList().Contains("Pending resolve request list: 1"));
            Assert.IsTrue(table.CreatePendingAutoAbilityResolutionRequestList().Contains("source=hidden"));
            Assert.IsFalse(table.CreatePendingAutoAbilityResolutionRequestList().Contains("CARD-1@"));

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableShowsReceivedPendingAutoAbilityResolutionRequestSummaryWithoutMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 899);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p2");
            string before = session.State.ToJson();
            NetworkPendingAutoAbilityResolutionRequestPayload payload =
                PendingAutoAbilityResolutionRequestPayloadCodec.Encode(
                    room.room_id,
                    0,
                    new PendingAutoAbilityResolutionRequest
                    {
                        selected_index = 0,
                        pending_id = "pending-auto-hidden|0|OnDraw|0000",
                        player_index = 0,
                        timing_event = "OnDraw",
                        source_card_instance_id = "hidden-source",
                        source_card_id = GameStateViewFactory.HiddenCardId,
                        hides_source_card_identity = true
                    },
                    GameStateViewPerspective.Spectator);

            transport.EmitPendingAutoAbilityResolutionRequest(payload);
            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();

            Assert.NotNull(table);
            Assert.IsTrue(table.CreatePendingAutoAbilityResolutionRequestSummary().Contains("Pending resolve requests: 1"));
            Assert.IsTrue(table.CreatePendingAutoAbilityResolutionRequestSummary().Contains(payload.payload_id));
            Assert.IsTrue(table.CreatePendingAutoAbilityResolutionRequestSummary().Contains("source=hidden"));
            Assert.IsFalse(table.CreatePendingAutoAbilityResolutionRequestSummary().Contains("hidden-source"));
            Assert.IsFalse(table.CreatePendingAutoAbilityResolutionRequestSummary().Contains(GameStateViewFactory.HiddenCardId + "@"));
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableShowsPendingAutoAbilityResolutionRequestListNewestFirstWithoutMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 900);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p2");
            string before = session.State.ToJson();

            transport.EmitPendingAutoAbilityResolutionRequest(
                CreatePendingAutoAbilityResolutionRequestPayload(room, "pending-1", 0));
            transport.EmitPendingAutoAbilityResolutionRequest(
                CreatePendingAutoAbilityResolutionRequestPayload(room, "pending-2", 1));
            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();

            Assert.NotNull(table);
            Assert.IsTrue(table.CreatePendingAutoAbilityResolutionRequestList().Contains("Pending resolve request list: 2"));
            Assert.Less(
                table.CreatePendingAutoAbilityResolutionRequestList().IndexOf("id=pending-2"),
                table.CreatePendingAutoAbilityResolutionRequestList().IndexOf("id=pending-1"));
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTablePublishesPendingAutoAbilityManualResolutionDecisionWithoutMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 902);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string before = session.State.ToJson();
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                CreatePendingAutoAbilityManualResolutionDecisionPayload(room, "pending-1", 0);

            transport.EmitPendingAutoAbilityManualResolutionDecision(payload);
            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject decisionButtonObject = GameObject.Find("DecAuto Button");

            Assert.NotNull(table);
            Assert.NotNull(decisionButtonObject);

            Button decisionButton = decisionButtonObject.GetComponent<Button>();
            Assert.NotNull(decisionButton);
            Assert.IsTrue(decisionButton.interactable);

            decisionButton.onClick.Invoke();

            Assert.AreEqual(1, transport.sentPendingAutoAbilityManualResolutionDecisions.Count);
            Assert.AreEqual(payload.payload_id, transport.sentPendingAutoAbilityManualResolutionDecisions[0].payload_id);
            Assert.AreEqual(1, session.PendingAutoAbilityManualResolutionDecisionPayloads.Count);
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("Manual resolution"));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("Decisions: 1"));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("Decision: Resolve"));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("pending-1"));
            Assert.AreEqual(0, session.State.event_log.Count);
            Assert.AreEqual(before, session.State.ToJson());

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableCreatesPendingAutoAbilityManualResolutionDecisionDraftWithoutPublishingOrMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 907);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string before = session.State.ToJson();

            transport.EmitPendingAutoAbilityResolutionRequest(
                CreatePendingAutoAbilityResolutionRequestPayload(room, "pending-1", 0));
            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject draftButtonObject = GameObject.Find("DraftDec Button");
            GameObject decisionButtonObject = GameObject.Find("DecAuto Button");

            Assert.NotNull(table);
            Assert.NotNull(draftButtonObject);
            Assert.NotNull(decisionButtonObject);
            Assert.IsTrue(draftButtonObject.GetComponent<Button>().interactable);

            draftButtonObject.GetComponent<Button>().onClick.Invoke();

            Assert.AreEqual(0, transport.sentPendingAutoAbilityManualResolutionDecisions.Count);
            Assert.AreEqual(1, session.PendingAutoAbilityManualResolutionDecisionPayloads.Count);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionDraftResultFormatter.SuccessMessage,
                table.CreateSelectionMessage());
            Assert.IsTrue(decisionButtonObject.GetComponent<Button>().interactable);
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("Manual resolution"));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("Decisions: 1"));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("Decision: Resolve"));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("pending-1"));
            Assert.AreEqual(0, session.State.event_log.Count);
            Assert.AreEqual(before, session.State.ToJson());

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableManualResolutionDecisionTypeSelectorControlsDraftType()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 908);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string before = session.State.ToJson();

            transport.EmitPendingAutoAbilityResolutionRequest(
                CreatePendingAutoAbilityResolutionRequestPayload(room, "pending-1", 0));
            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject typeButtonObject = GameObject.Find("DecType Button");
            GameObject draftButtonObject = GameObject.Find("DraftDec Button");

            Assert.NotNull(table);
            Assert.NotNull(typeButtonObject);
            Assert.NotNull(draftButtonObject);

            typeButtonObject.GetComponent<Button>().onClick.Invoke();
            Assert.AreEqual(0, transport.sentPendingAutoAbilityManualResolutionDecisions.Count);
            Assert.AreEqual(0, session.PendingAutoAbilityManualResolutionDecisionPayloads.Count);
            draftButtonObject.GetComponent<Button>().onClick.Invoke();

            Assert.AreEqual(0, transport.sentPendingAutoAbilityManualResolutionDecisions.Count);
            Assert.AreEqual(1, session.PendingAutoAbilityManualResolutionDecisionPayloads.Count);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionTypes.Skip,
                session.PendingAutoAbilityManualResolutionDecisionPayloads[0].decision_type);
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("Decision: Skip"));
            Assert.AreEqual(0, session.State.event_log.Count);
            Assert.AreEqual(before, session.State.ToJson());

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableApplyPreviewStoresQueuePayloadWithoutPublishingOrMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 910);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string before = session.State.ToJson();

            transport.EmitPendingAutoAbilityQueue(CreatePlayerPendingAutoAbilityQueuePayload(room));
            transport.EmitPendingAutoAbilityManualResolutionDecision(
                CreatePendingAutoAbilityManualResolutionDecisionPayload(
                    room,
                    "pending-1",
                    0,
                    PendingAutoAbilityManualResolutionDecisionTypes.Skip));
            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject applyButtonObject = GameObject.Find("ApplyDec Button");
            GameObject applyLogLatestObject =
                GameObject.Find("Pending Ability Manual Resolution Apply Preview Log Latest");
            GameObject applyLogListObject =
                GameObject.Find("Pending Ability Manual Resolution Apply Preview Log List");

            Assert.NotNull(table);
            Assert.NotNull(applyButtonObject);
            Assert.NotNull(applyLogLatestObject);
            Assert.NotNull(applyLogListObject);
            applyButtonObject.GetComponent<Button>().onClick.Invoke();

            Assert.AreEqual(0, transport.sentPendingAutoAbilityQueues.Count);
            Assert.AreEqual(2, session.PendingAutoAbilityQueuePayloads.Count);
            Assert.IsTrue(table.CreateSelectionMessage().Contains("accepted type=Skip id=pending-1"));
            Assert.IsTrue(
                table.CreatePendingAutoAbilityManualResolutionApplyPreviewLogLatest()
                    .Contains("accepted type=Skip id=pending-1"));
            Assert.IsTrue(
                table.CreatePendingAutoAbilityManualResolutionApplyPreviewLogList()
                    .Contains("Pending manual decision apply preview log: 1"));
            Assert.IsTrue(
                table.CreatePendingAutoAbilityManualResolutionApplyPreviewLogList()
                    .Contains("accepted type=Skip id=pending-1"));
            Assert.IsFalse(table.CreatePendingAutoAbilityManualResolutionApplyPreviewLogLatest().Contains("CARD-0"));
            Assert.IsFalse(table.CreatePendingAutoAbilityManualResolutionApplyPreviewLogLatest().Contains("src-0"));
            Assert.IsFalse(table.CreatePendingAutoAbilityManualResolutionApplyPreviewLogList().Contains("CARD-0"));
            Assert.IsFalse(table.CreatePendingAutoAbilityManualResolutionApplyPreviewLogList().Contains("src-0"));
            Assert.AreEqual(0, session.State.event_log.Count);
            Assert.AreEqual(before, session.State.ToJson());

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableApplyPreviewMissingPayloadShowsRejectionWithoutMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 911);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string before = session.State.ToJson();

            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject applyButtonObject = GameObject.Find("ApplyDec Button");

            Assert.NotNull(table);
            Assert.NotNull(applyButtonObject);
            applyButtonObject.GetComponent<Button>().onClick.Invoke();

            Assert.IsTrue(table.CreateSelectionMessage().Contains(PendingAutoAbilityManualResolutionApplyRejectionReasons.QueueMissing));
            Assert.IsTrue(
                table.CreatePendingAutoAbilityManualResolutionApplyPreviewLogLatest()
                    .Contains("rejected " + PendingAutoAbilityManualResolutionApplyRejectionReasons.QueueMissing));
            Assert.IsTrue(
                table.CreatePendingAutoAbilityManualResolutionApplyPreviewLogList()
                    .Contains("Pending manual decision apply preview log: 1"));
            Assert.IsTrue(
                table.CreatePendingAutoAbilityManualResolutionApplyPreviewLogList()
                    .Contains("rejected " + PendingAutoAbilityManualResolutionApplyRejectionReasons.QueueMissing));
            Assert.AreEqual(0, transport.sentPendingAutoAbilityQueues.Count);
            Assert.AreEqual(0, session.State.event_log.Count);
            Assert.AreEqual(before, session.State.ToJson());

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableShowsReceivedPendingAutoAbilityManualResolutionDecisionSummaryWithoutMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 904);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string before = session.State.ToJson();
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                CreatePendingAutoAbilityManualResolutionDecisionPayload(room, "pending-1", 0);

            transport.EmitPendingAutoAbilityManualResolutionDecision(payload);
            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();

            Assert.NotNull(table);
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("Manual resolution"));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("Decisions: 1"));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains(payload.payload_id));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("Decision: Resolve"));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("source CARD-0"));
            Assert.IsFalse(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("src-0"));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionList().Contains("Pending manual decision list: 1"));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionList().Contains(payload.payload_id));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionValidationPreview().Contains("valid type=Resolve"));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionValidationPreview().Contains("id=pending-1"));
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableShowsPendingAutoAbilityManualResolutionDecisionListNewestFirstWithoutMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 906);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string before = session.State.ToJson();

            transport.EmitPendingAutoAbilityManualResolutionDecision(
                CreatePendingAutoAbilityManualResolutionDecisionPayload(room, "pending-1", 0));
            transport.EmitPendingAutoAbilityManualResolutionDecision(
                CreatePendingAutoAbilityManualResolutionDecisionPayload(room, "pending-2", 1));
            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();

            Assert.NotNull(table);
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionList().Contains("Pending manual decision list: 2"));
            Assert.Less(
                table.CreatePendingAutoAbilityManualResolutionDecisionList().IndexOf("id=pending-2"),
                table.CreatePendingAutoAbilityManualResolutionDecisionList().IndexOf("id=pending-1"));
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableManualResolutionDecisionSummaryKeepsHiddenSourceSafe()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 905);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            PendingAutoAbilityManualResolutionDecision hiddenDecision =
                new PendingAutoAbilityManualResolutionDecision
                {
                    decision_id = "decision-hidden",
                    decision_type = PendingAutoAbilityManualResolutionDecisionTypes.Defer,
                    selected_index = 0,
                    pending_id = "pending-auto-hidden|0|OnDraw|0000",
                    player_index = 0,
                    timing_event = "OnDraw",
                    source_card_instance_id = "hidden-source",
                    source_card_id = GameStateViewFactory.HiddenCardId,
                    hides_source_card_identity = true
                };
            NetworkPendingAutoAbilityManualResolutionDecisionPayload payload =
                PendingAutoAbilityManualResolutionDecisionPayloadCodec.Encode(
                    room.room_id,
                    0,
                    hiddenDecision,
                    GameStateViewPerspective.Player,
                    0);

            transport.EmitPendingAutoAbilityManualResolutionDecision(payload);
            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();

            Assert.NotNull(table);
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("Decision: Defer"));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("source hidden"));
            Assert.IsFalse(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains("hidden-source"));
            Assert.IsFalse(table.CreatePendingAutoAbilityManualResolutionDecisionSummary().Contains(GameStateViewFactory.HiddenCardId + "@"));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionList().Contains("type=Defer"));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionList().Contains("source=hidden"));
            Assert.IsFalse(table.CreatePendingAutoAbilityManualResolutionDecisionList().Contains("hidden-source"));
            Assert.IsFalse(table.CreatePendingAutoAbilityManualResolutionDecisionList().Contains(GameStateViewFactory.HiddenCardId + "@"));
            Assert.IsTrue(table.CreatePendingAutoAbilityManualResolutionDecisionValidationPreview().Contains("source=hidden"));
            Assert.IsFalse(table.CreatePendingAutoAbilityManualResolutionDecisionValidationPreview().Contains("hidden-source"));
            Assert.IsFalse(table.CreatePendingAutoAbilityManualResolutionDecisionValidationPreview().Contains(GameStateViewFactory.HiddenCardId + "@"));
            Assert.AreEqual(0, session.State.event_log.Count);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableShowsInvalidPendingAutoAbilityManualResolutionDecisionValidationWithoutMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 909);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string before = session.State.ToJson();

            transport.EmitPendingAutoAbilityManualResolutionDecision(
                new NetworkPendingAutoAbilityManualResolutionDecisionPayload
                {
                    protocol_version = MultiplayerProtocol.ProtocolVersion,
                    payload_id = "payload-invalid",
                    room_id = room.room_id,
                    pending_auto_ability_manual_resolution_decision_json = string.Empty
                });
            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();

            Assert.NotNull(table);
            Assert.IsTrue(
                table.CreatePendingAutoAbilityManualResolutionDecisionValidationPreview()
                    .Contains("rejected PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_PAYLOAD_INVALID"));
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableDisablesPendingAutoAbilityManualResolutionDecisionPublishWhenNoPayloadExists()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 903);
            FakeTransport transport = new FakeTransport();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                CreateRoom(),
                initialState,
                "p1");

            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject decisionButtonObject = GameObject.Find("DecAuto Button");

            Assert.NotNull(table);
            Assert.NotNull(decisionButtonObject);
            Assert.IsFalse(decisionButtonObject.GetComponent<Button>().interactable);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void LocalPlayTableKeepsPendingAutoAbilityManualResolutionDecisionDraftControlInHiddenAdvancedDrawer()
        {
            PlayTableBootstrap.Show(CreateSampleDeck("p1"), CreateSampleDeck("p2"));
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject advancedDrawerObject = GameObject.Find("Advanced Drawer");
            GameObject draftButtonObject = GameObject.Find("DraftDec Button");
            GameObject decisionTypeButtonObject = GameObject.Find("DecType Button");
            GameObject applyButtonObject = GameObject.Find("ApplyDec Button");

            Assert.NotNull(table);
            Assert.NotNull(advancedDrawerObject);
            Assert.NotNull(draftButtonObject);
            Assert.NotNull(decisionTypeButtonObject);
            Assert.NotNull(applyButtonObject);
            Assert.IsTrue(IsDescendantOf(draftButtonObject.transform, advancedDrawerObject.transform));
            Assert.IsTrue(IsDescendantOf(decisionTypeButtonObject.transform, advancedDrawerObject.transform));
            Assert.IsTrue(IsDescendantOf(applyButtonObject.transform, advancedDrawerObject.transform));
            Assert.AreEqual(0f, advancedDrawerObject.GetComponent<CanvasGroup>().alpha);
            Assert.IsTrue(advancedDrawerObject.GetComponent<LayoutElement>().ignoreLayout);
            Assert.AreEqual(
                PendingAutoAbilityManualResolutionDecisionValidationResultFormatter.NullResultMessage,
                table.CreatePendingAutoAbilityManualResolutionDecisionValidationPreview());

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTableClearsPendingAutoAbilitySelectionWithoutMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 897);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string before = session.State.ToJson();
            NetworkPendingAutoAbilityQueuePayload payload = CreatePendingAutoAbilityQueuePayload(room, 2);

            transport.EmitPendingAutoAbilityQueue(payload);
            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject selectionButtonObject = GameObject.Find("SelAuto Button");
            GameObject clearSelectionButtonObject = GameObject.Find("ClrAuto Button");

            Assert.NotNull(table);
            Assert.NotNull(selectionButtonObject);
            Assert.NotNull(clearSelectionButtonObject);

            Button selectionButton = selectionButtonObject.GetComponent<Button>();
            Button clearSelectionButton = clearSelectionButtonObject.GetComponent<Button>();
            Assert.NotNull(selectionButton);
            Assert.NotNull(clearSelectionButton);
            Assert.IsTrue(selectionButton.interactable);
            Assert.IsFalse(clearSelectionButton.interactable);

            selectionButton.onClick.Invoke();

            Assert.IsTrue(clearSelectionButton.interactable);
            Assert.IsTrue(table.CreatePendingAutoAbilitySelectionStatus().Contains("index=0"));

            clearSelectionButton.onClick.Invoke();

            Assert.AreEqual("Pending selection: none", table.CreatePendingAutoAbilitySelectionStatus());
            Assert.AreEqual(
                "Pending resolve request rejected: " +
                PendingAutoAbilityResolutionRequestFactory.SelectionMissingReason,
                table.CreatePendingAutoAbilityResolutionRequestPreview());
            Assert.IsFalse(clearSelectionButton.interactable);
            Assert.IsFalse(table.CreatePendingAutoAbilitySelectionStatus().Contains("CARD-1@"));
            Assert.AreEqual(0, transport.sentPendingAutoAbilityQueues.Count);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void OnlinePlayTablePublishesPendingAutoAbilityQueueWithoutMutatingState()
        {
            GameState initialState = GameStateFactory.CreateTwoPlayerGame(CreateSampleDeck("p1"), CreateSampleDeck("p2"), 895);
            FakeTransport transport = new FakeTransport();
            MultiplayerRoomState room = CreateRoom();
            MultiplayerGameSessionController session = new MultiplayerGameSessionController(
                transport,
                room,
                initialState,
                "p1");
            string before = session.State.ToJson();
            NetworkPendingAutoAbilityQueuePayload payload = CreatePendingAutoAbilityQueuePayload(room);

            transport.EmitPendingAutoAbilityQueue(payload);
            PlayTableBootstrap.ShowOnline(session);
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject publishButtonObject = GameObject.Find("AutoQ Button");

            Assert.NotNull(table);
            Assert.NotNull(publishButtonObject);

            Button publishButton = publishButtonObject.GetComponent<Button>();
            Assert.NotNull(publishButton);
            Assert.IsTrue(publishButton.interactable);

            publishButton.onClick.Invoke();

            Assert.AreEqual(1, transport.sentPendingAutoAbilityQueues.Count);
            Assert.AreEqual(payload.payload_id, transport.sentPendingAutoAbilityQueues[0].payload_id);
            Assert.AreEqual(1, session.PendingAutoAbilityQueuePayloads.Count);
            Assert.AreEqual(before, session.State.ToJson());
            Assert.AreEqual(0, session.State.event_log.Count);

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void LocalPlayTableKeepsTriggerCheckAndPendingAutoControlsInHiddenAdvancedDrawer()
        {
            PlayTableBootstrap.Show(CreateSampleDeck("p1"), CreateSampleDeck("p2"));
            PlayTableBootstrap table = Object.FindAnyObjectByType<PlayTableBootstrap>();
            GameObject advancedDrawerObject = GameObject.Find("Advanced Drawer");

            Assert.NotNull(table);
            Assert.NotNull(advancedDrawerObject);
            Assert.AreEqual(0f, advancedDrawerObject.GetComponent<CanvasGroup>().alpha);
            Assert.IsTrue(advancedDrawerObject.GetComponent<LayoutElement>().ignoreLayout);

            string[] hiddenButtonNames =
            {
                "TrigLog Button",
                "AutoQ Button",
                "SelAuto Button",
                "ReqAuto Button",
                "DecAuto Button",
                "ClrAuto Button",
                "DraftTrig Button",
                "ClrDraft Button",
                "TrigType Button",
                "ChkSrc Button",
                "ChkIdx Button",
                "Export Replay Button"
            };

            foreach (string buttonName in hiddenButtonNames)
            {
                GameObject buttonObject = GameObject.Find(buttonName);
                Assert.NotNull(buttonObject, buttonName);
                Assert.IsTrue(IsDescendantOf(buttonObject.transform, advancedDrawerObject.transform), buttonName);
            }

            Object.DestroyImmediate(table.gameObject);
        }

        [Test]
        public void ReplayPlayerStepsAndJumpsToEnd()
        {
            GameState initial = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                200);
            GameState live = GameState.FromJson(initial.ToJson(false));

            string handCard = live.GetPlayer(0).hand[0].instance_id;
            GameActionService.Draw(live, 0);
            GameActionService.MoveCard(live, 0, handCard, GameZone.Hand, GameZone.Vanguard);
            GameActionService.SetPhase(live, 0, GamePhase.Battle);
            GameActionService.AddGiftMarker(live, 0, GiftMarkerType.Accel);

            GameReplay replay = GameReplay.Create(initial, live.event_log);
            GameReplayPlayer player = new GameReplayPlayer(replay);

            Assert.AreEqual(0, player.CurrentIndex);
            Assert.AreEqual(45, player.CurrentState.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.IsTrue(player.StepForward());
            Assert.AreEqual(1, player.CurrentIndex);
            Assert.AreEqual(44, player.CurrentState.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(6, player.CurrentState.GetPlayer(0).CountZone(GameZone.Hand));

            player.JumpToEnd();
            Assert.IsTrue(player.IsAtEnd);
            Assert.AreEqual(GamePhase.Battle, player.CurrentState.phase);
            Assert.AreEqual(live.GetPlayer(0).CountZone(GameZone.Hand), player.CurrentState.GetPlayer(0).CountZone(GameZone.Hand));
            Assert.AreEqual(live.GetPlayer(0).CountZone(GameZone.Vanguard), player.CurrentState.GetPlayer(0).CountZone(GameZone.Vanguard));
            Assert.AreEqual(1, player.CurrentState.GetPlayer(0).GetGiftMarkerCount(GiftMarkerType.Accel));
            Assert.AreEqual(live.event_log.Count, player.CurrentState.event_log.Count);

            player.JumpToStart();
            Assert.AreEqual(0, player.CurrentIndex);
            Assert.AreEqual(GamePhase.Mulligan, player.CurrentState.phase);
            Assert.AreEqual(0, player.CurrentState.event_log.Count);
        }

        [Test]
        public void ReplaySerializesRoundTrip()
        {
            GameState initial = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                201);
            GameState live = GameState.FromJson(initial.ToJson(false));

            GameActionService.Draw(live, 0);
            GameActionService.SetPhase(live, 0, GamePhase.Main);

            GameReplay replay = GameReplay.Create(initial, live.event_log);
            string json = replay.ToJson();
            GameReplay roundTrip = GameReplay.FromJson(json);
            GameReplayPlayer player = new GameReplayPlayer(roundTrip);
            player.JumpToEnd();

            Assert.AreEqual(2, roundTrip.events.Count);
            Assert.AreEqual(GamePhase.Main, player.CurrentState.phase);
            Assert.AreEqual(2, player.CurrentState.event_log.Count);
        }

        [Test]
        public void LegalActionGeneratorReturnsDrawMoveAndPhaseActions()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                300);

            var actions = LegalActionGenerator.Generate(state, 0);

            Assert.IsTrue(HasAction(actions, GameActionType.Draw));
            Assert.IsTrue(HasMove(actions, GameZone.Hand, GameZone.Vanguard));
            Assert.IsTrue(HasMove(actions, GameZone.Hand, GameZone.RearGuard));
            Assert.IsTrue(HasAction(actions, GameActionType.SetPhase));
            Assert.NotNull(FirstPhase(actions, GamePhase.StandAndDraw));
            Assert.NotNull(FirstPhase(actions, GamePhase.Ride));
            Assert.NotNull(FirstPhase(actions, GamePhase.Main));
            Assert.IsTrue(HasGift(actions, GiftMarkerType.Force));
            Assert.IsTrue(HasGift(actions, GiftMarkerType.Accel));
            Assert.IsTrue(HasGift(actions, GiftMarkerType.Protect));
        }

        [Test]
        public void LegalActionGeneratorAllowsFirstVanguardFromRideDeckDuringMulligan()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                301);

            var actions = LegalActionGenerator.Generate(state, 0);
            LegalGameAction firstVanguard = FirstMove(actions, GameZone.RideDeck, GameZone.Vanguard);

            Assert.NotNull(firstVanguard);
            Assert.IsTrue(firstVanguard.label.Contains("Set first vanguard"));

            RulesCommandResult result = RulesCore.TryExecute(state, firstVanguard);

            Assert.IsTrue(result.accepted);
            Assert.AreEqual(3, state.GetPlayer(0).CountZone(GameZone.RideDeck));
            Assert.AreEqual(1, state.GetPlayer(0).CountZone(GameZone.Vanguard));
            Assert.AreEqual(GameActionType.MoveCard, state.event_log[0].action_type);
            Assert.AreEqual(GameZone.RideDeck, state.event_log[0].from_zone);
            Assert.AreEqual(GameZone.Vanguard, state.event_log[0].to_zone);
        }

        [Test]
        public void LegalActionGeneratorDoesNotOfferFirstVanguardAfterVanguardExists()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                302);
            LegalGameAction firstVanguard = FirstMove(
                LegalActionGenerator.Generate(state, 0),
                GameZone.RideDeck,
                GameZone.Vanguard);
            RulesCore.ExecuteOrThrow(state, firstVanguard);

            var actions = LegalActionGenerator.Generate(state, 0);

            Assert.IsFalse(HasMove(actions, GameZone.RideDeck, GameZone.Vanguard));
        }

        [Test]
        public void LegalActionGeneratorOffersSingleCardMulliganActionsDuringMulligan()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                303);
            string selectedHandCard = state.GetPlayer(0).hand[0].instance_id;

            LegalGameAction mulligan = FirstMulliganForCard(
                LegalActionGenerator.Generate(state, 0),
                selectedHandCard);

            Assert.NotNull(mulligan);
            Assert.AreEqual(1, mulligan.card_instance_ids.Count);
            Assert.AreEqual(selectedHandCard, mulligan.card_instance_ids[0]);

            RulesCommandResult result = RulesCore.TryExecute(state, mulligan);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(GameActionType.MulliganCards, state.event_log[0].action_type);
            Assert.AreEqual(5, state.GetPlayer(0).CountZone(GameZone.Hand));
            Assert.AreEqual(45, state.GetPlayer(0).CountZone(GameZone.Deck));
        }

        [Test]
        public void LegalActionExecutorUsesGameActionServiceAndEventLog()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                301);
            var actions = LegalActionGenerator.Generate(state, 0);
            LegalGameAction draw = FirstAction(actions, GameActionType.Draw);

            GameEvent drawEvent = LegalActionExecutor.Execute(state, draw);

            Assert.AreEqual(GameActionType.Draw, drawEvent.action_type);
            Assert.AreEqual(44, state.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(6, state.GetPlayer(0).CountZone(GameZone.Hand));
            Assert.AreEqual(1, state.event_log.Count);

            LegalGameAction move = FirstMove(LegalActionGenerator.Generate(state, 0), GameZone.Hand, GameZone.RearGuard);
            LegalActionExecutor.Execute(state, move);

            Assert.AreEqual(5, state.GetPlayer(0).CountZone(GameZone.Hand));
            Assert.AreEqual(1, state.GetPlayer(0).CountZone(GameZone.RearGuard));
            Assert.AreEqual(2, state.event_log.Count);
        }

        [Test]
        public void RulesCoreExecutesDrawMovePhaseAndGiftThroughEventLog()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                302);

            RulesCommandResult draw = RulesCore.TryExecute(
                state,
                FirstAction(RulesCore.GetLegalActions(state, 0), GameActionType.Draw));
            Assert.IsTrue(draw.accepted, draw.rejection_reason);
            Assert.AreEqual(GameActionType.Draw, draw.game_event.action_type);
            Assert.AreEqual(1, state.event_log.Count);

            RulesCommandResult move = RulesCore.TryExecute(
                state,
                FirstMove(RulesCore.GetLegalActions(state, 0), GameZone.Hand, GameZone.RearGuard));
            Assert.IsTrue(move.accepted, move.rejection_reason);
            Assert.AreEqual(1, state.GetPlayer(0).CountZone(GameZone.RearGuard));
            Assert.AreEqual(2, state.event_log.Count);

            RulesCommandResult phase = RulesCore.TryExecute(
                state,
                FirstPhase(RulesCore.GetLegalActions(state, 0), GamePhase.Main));
            Assert.IsTrue(phase.accepted, phase.rejection_reason);
            Assert.AreEqual(GamePhase.Main, state.phase);
            Assert.AreEqual(3, state.event_log.Count);

            RulesCommandResult gift = RulesCore.TryExecute(
                state,
                FirstGift(RulesCore.GetLegalActions(state, 0), GiftMarkerType.Protect));
            Assert.IsTrue(gift.accepted, gift.rejection_reason);
            Assert.AreEqual(1, state.GetPlayer(0).GetGiftMarkerCount(GiftMarkerType.Protect));
            Assert.AreEqual(4, state.event_log.Count);
        }

        [Test]
        public void RulesCoreRejectsInvalidActionWithoutMutatingState()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                303);
            string handCard = state.GetPlayer(0).hand[0].instance_id;
            int handCount = state.GetPlayer(0).CountZone(GameZone.Hand);
            int deckCount = state.GetPlayer(0).CountZone(GameZone.Deck);

            LegalGameAction invalid = new LegalGameAction
            {
                action_type = GameActionType.MoveCard,
                actor_index = 0,
                card_instance_id = handCard,
                from_zone = GameZone.Deck,
                to_zone = GameZone.Vanguard
            };

            RulesCommandResult result = RulesCore.TryExecute(state, invalid);

            Assert.IsFalse(result.accepted);
            Assert.IsFalse(RulesCore.CanExecute(state, invalid));
            Assert.AreEqual(handCount, state.GetPlayer(0).CountZone(GameZone.Hand));
            Assert.AreEqual(deckCount, state.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(0, state.GetPlayer(0).CountZone(GameZone.Vanguard));
            Assert.AreEqual(0, state.event_log.Count);
        }

        [Test]
        public void SnapshotRestoreReturnsStateAndSupportsIsolatedBranch()
        {
            GameState live = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                304);
            GameStateSnapshot snapshot = GameStateSnapshot.Capture(live);

            RulesCore.ExecuteOrThrow(live, FirstAction(RulesCore.GetLegalActions(live, 0), GameActionType.Draw));
            Assert.AreEqual(44, live.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(1, live.event_log.Count);

            GameState restored = snapshot.Restore();
            Assert.AreEqual(45, restored.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(0, restored.event_log.Count);

            GameState branch = snapshot.Clone();
            RulesCore.ExecuteOrThrow(branch, FirstAction(RulesCore.GetLegalActions(branch, 0), GameActionType.Draw));
            Assert.AreEqual(44, branch.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(45, restored.GetPlayer(0).CountZone(GameZone.Deck));
            Assert.AreEqual(44, live.GetPlayer(0).CountZone(GameZone.Deck));
        }

        [Test]
        public void SameStartingStateAndCommandsProduceMatchingEventLogs()
        {
            GameState initial = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                305);
            string initialJson = initial.ToJson(false);
            GameState first = GameState.FromJson(initialJson);
            GameState second = GameState.FromJson(initialJson);

            RunDeterministicCommandScript(first);
            RunDeterministicCommandScript(second);

            AssertEventsMatch(first, second);
        }

        [Test]
        public void AbilityCoreResolvesFiveSimpleFixturesThroughEventPath()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                306);

            AssertAbilityAccepted(state, Ability("draw", new AbilityEffectDefinition
            {
                effect_type = AbilityEffectType.Draw,
                amount = 1
            }));
            AssertAbilityAccepted(state, Ability("force", new AbilityEffectDefinition
            {
                effect_type = AbilityEffectType.AddGiftMarker,
                gift_marker_type = GiftMarkerType.Force,
                amount = 1
            }));
            AssertAbilityAccepted(state, Ability("accel", new AbilityEffectDefinition
            {
                effect_type = AbilityEffectType.AddGiftMarker,
                gift_marker_type = GiftMarkerType.Accel,
                amount = 1
            }));
            AssertAbilityAccepted(state, Ability("phase", new AbilityEffectDefinition
            {
                effect_type = AbilityEffectType.SetPhase,
                phase = GamePhase.Main
            }));
            AssertAbilityAccepted(state, Ability("call", new AbilityEffectDefinition
            {
                effect_type = AbilityEffectType.MoveFirstFromZoneToZone,
                from_zone = GameZone.Hand,
                to_zone = GameZone.RearGuard
            }));

            Assert.AreEqual(5, state.event_log.Count);
            Assert.AreEqual(1, state.GetPlayer(0).GetGiftMarkerCount(GiftMarkerType.Force));
            Assert.AreEqual(1, state.GetPlayer(0).GetGiftMarkerCount(GiftMarkerType.Accel));
            Assert.AreEqual(GamePhase.Main, state.phase);
            Assert.AreEqual(1, state.GetPlayer(0).CountZone(GameZone.RearGuard));
        }

        [Test]
        public void AbilityCoreUsesCustomEffectRegistry()
        {
            const string EffectId = "test.custom.protect";
            AbilityEffectRegistry.Register(EffectId, delegate(GameState state, int playerIndex, AbilityDefinition ability, AbilityEffectDefinition effect)
            {
                RulesCommandResult commandResult = RulesCore.TryExecute(state, new LegalGameAction
                {
                    action_type = GameActionType.AddGiftMarker,
                    actor_index = playerIndex,
                    gift_marker_type = GiftMarkerType.Protect,
                    marker_delta = 1
                });
                if (!commandResult.accepted)
                {
                    return AbilityResolutionResult.Rejected(commandResult.rejection_reason);
                }

                AbilityResolutionResult result = AbilityResolutionResult.Accepted();
                result.events.Add(commandResult.game_event);
                return result;
            });

            GameState gameState = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                307);
            AbilityDefinition custom = Ability("custom", new AbilityEffectDefinition
            {
                effect_type = AbilityEffectType.Custom,
                custom_effect_id = EffectId
            });

            AbilityResolutionResult resolution = AbilityCore.Resolve(gameState, 0, custom);

            AbilityEffectRegistry.Unregister(EffectId);
            Assert.IsTrue(resolution.accepted, resolution.rejection_reason);
            Assert.AreEqual(1, resolution.events.Count);
            Assert.AreEqual(1, gameState.GetPlayer(0).GetGiftMarkerCount(GiftMarkerType.Protect));
        }

        [Test]
        public void UnsupportedAbilityFallsBackToManualPlay()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                308);
            AbilityDefinition unsupported = Ability("unsupported", new AbilityEffectDefinition
            {
                effect_type = AbilityEffectType.Custom,
                custom_effect_id = "missing.handler"
            });
            unsupported.manual_fallback = true;

            AbilityResolutionResult resolution = AbilityCore.Resolve(state, 0, unsupported);

            Assert.IsFalse(resolution.accepted);
            Assert.IsTrue(resolution.needs_manual_resolution);
            Assert.AreEqual(0, state.event_log.Count);

            RulesCommandResult manual = RulesCore.TryExecute(
                state,
                FirstMove(RulesCore.GetLegalActions(state, 0), GameZone.Hand, GameZone.RearGuard));
            Assert.IsTrue(manual.accepted, manual.rejection_reason);
            Assert.AreEqual(1, state.event_log.Count);
            Assert.AreEqual(1, state.GetPlayer(0).CountZone(GameZone.RearGuard));
        }

        [Test]
        public void EasyBotPlaysSimpleTurnThroughEventLog()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                400);

            var events = EasyBotController.PlayTurn(state, 1);

            Assert.Greater(events.Count, 0);
            Assert.AreEqual(events.Count, state.event_log.Count);
            Assert.AreEqual(GamePhase.End, state.phase);
            Assert.AreEqual(1, state.GetPlayer(1).CountZone(GameZone.Vanguard));
            Assert.GreaterOrEqual(state.GetPlayer(1).CountZone(GameZone.RearGuard), 1);
        }

        [Test]
        public void EasyBotDecisionComesFromLegalActions()
        {
            GameState state = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                401);

            BotDecision decision = EasyBotController.DecideNext(state, 1);

            Assert.NotNull(decision);
            Assert.NotNull(decision.Action);
            Assert.AreEqual(GameActionType.Draw, decision.Action.action_type);

            RulesCommandResult result = RulesCore.TryExecute(state, decision.Action);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(GameActionType.Draw, result.game_event.action_type);
            Assert.AreEqual(1, state.event_log.Count);
        }

        [Test]
        public void BotProfilesChangeRearGuardCommitment()
        {
            GameState aggro = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                500);
            GameState defensive = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                500);

            ProfileBotController.PlayTurn(aggro, 1, BotProfile.Create(BotProfileType.Aggro), 9);
            ProfileBotController.PlayTurn(defensive, 1, BotProfile.Create(BotProfileType.Defensive), 9);

            Assert.Greater(
                aggro.GetPlayer(1).CountZone(GameZone.RearGuard),
                defensive.GetPlayer(1).CountZone(GameZone.RearGuard));
            Assert.AreEqual(GamePhase.End, aggro.phase);
            Assert.AreEqual(GamePhase.End, defensive.phase);
        }

        [Test]
        public void BotProfileSeedIsDeterministic()
        {
            GameState first = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                501);
            GameState second = GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                501);

            ProfileBotController.PlayTurn(first, 1, BotProfile.Create(BotProfileType.Balanced), 123);
            ProfileBotController.PlayTurn(second, 1, BotProfile.Create(BotProfileType.Balanced), 123);

            Assert.AreEqual(first.event_log.Count, second.event_log.Count);
            for (int i = 0; i < first.event_log.Count; i++)
            {
                Assert.AreEqual(first.event_log[i].action_type, second.event_log[i].action_type);
                Assert.AreEqual(first.event_log[i].card_instance_id, second.event_log[i].card_instance_id);
                Assert.AreEqual(first.event_log[i].from_zone, second.event_log[i].from_zone);
                Assert.AreEqual(first.event_log[i].to_zone, second.event_log[i].to_zone);
                Assert.AreEqual(first.event_log[i].new_phase, second.event_log[i].new_phase);
            }
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

        private static bool HasAction(System.Collections.Generic.IReadOnlyList<LegalGameAction> actions, GameActionType actionType)
        {
            foreach (LegalGameAction action in actions)
            {
                if (action.action_type == actionType)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDescendantOf(Transform child, Transform expectedParent)
        {
            Transform current = child;
            while (current != null)
            {
                if (current == expectedParent)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static bool HasMove(System.Collections.Generic.IReadOnlyList<LegalGameAction> actions, GameZone from, GameZone to)
        {
            foreach (LegalGameAction action in actions)
            {
                if (action.action_type == GameActionType.MoveCard && action.from_zone == from && action.to_zone == to)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasGift(System.Collections.Generic.IReadOnlyList<LegalGameAction> actions, GiftMarkerType markerType)
        {
            foreach (LegalGameAction action in actions)
            {
                if (action.action_type == GameActionType.AddGiftMarker && action.gift_marker_type == markerType)
                {
                    return true;
                }
            }

            return false;
        }

        private static LegalGameAction FirstAction(System.Collections.Generic.IReadOnlyList<LegalGameAction> actions, GameActionType actionType)
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

        private static LegalGameAction FirstMove(System.Collections.Generic.IReadOnlyList<LegalGameAction> actions, GameZone from, GameZone to)
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

        private static LegalGameAction FirstMulliganForCard(System.Collections.Generic.IReadOnlyList<LegalGameAction> actions, string cardInstanceId)
        {
            foreach (LegalGameAction action in actions)
            {
                if (action.action_type != GameActionType.MulliganCards ||
                    action.card_instance_ids == null ||
                    action.card_instance_ids.Count != 1)
                {
                    continue;
                }

                if (action.card_instance_ids[0] == cardInstanceId)
                {
                    return action;
                }
            }

            Assert.Fail("Missing mulligan action for " + cardInstanceId);
            return null;
        }

        private static LegalGameAction FirstPhase(System.Collections.Generic.IReadOnlyList<LegalGameAction> actions, GamePhase phase)
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

        private static LegalGameAction FirstGift(System.Collections.Generic.IReadOnlyList<LegalGameAction> actions, GiftMarkerType markerType)
        {
            foreach (LegalGameAction action in actions)
            {
                if (action.action_type == GameActionType.AddGiftMarker && action.gift_marker_type == markerType)
                {
                    return action;
                }
            }

            Assert.Fail("Missing gift marker " + markerType);
            return null;
        }

        private static void RunDeterministicCommandScript(GameState state)
        {
            RulesCore.ExecuteOrThrow(state, FirstAction(RulesCore.GetLegalActions(state, 0), GameActionType.Draw));
            RulesCore.ExecuteOrThrow(state, FirstMove(RulesCore.GetLegalActions(state, 0), GameZone.Hand, GameZone.Vanguard));
            RulesCore.ExecuteOrThrow(state, FirstPhase(RulesCore.GetLegalActions(state, 0), GamePhase.Main));
            RulesCore.ExecuteOrThrow(state, FirstGift(RulesCore.GetLegalActions(state, 0), GiftMarkerType.Force));
        }

        private static void AssertEventsMatch(GameState first, GameState second)
        {
            Assert.AreEqual(first.event_log.Count, second.event_log.Count);
            for (int i = 0; i < first.event_log.Count; i++)
            {
                GameEvent a = first.event_log[i];
                GameEvent b = second.event_log[i];
                Assert.AreEqual(a.event_id, b.event_id);
                Assert.AreEqual(a.action_type, b.action_type);
                Assert.AreEqual(a.actor_index, b.actor_index);
                Assert.AreEqual(a.card_instance_id, b.card_instance_id);
                Assert.AreEqual(a.from_zone, b.from_zone);
                Assert.AreEqual(a.to_zone, b.to_zone);
                Assert.AreEqual(a.from_index, b.from_index);
                Assert.AreEqual(a.to_index, b.to_index);
                Assert.AreEqual(a.previous_phase, b.previous_phase);
                Assert.AreEqual(a.new_phase, b.new_phase);
                Assert.AreEqual(a.gift_marker_type, b.gift_marker_type);
                Assert.AreEqual(a.marker_delta, b.marker_delta);
            }
        }

        private static AbilityDefinition Ability(string id, AbilityEffectDefinition effect)
        {
            AbilityDefinition ability = new AbilityDefinition
            {
                ability_id = id,
                label = id,
                timing = AbilityTiming.Manual
            };
            ability.effects.Add(effect);
            return ability;
        }

        private static void AssertAbilityAccepted(GameState state, AbilityDefinition ability)
        {
            AbilityResolutionResult result = AbilityCore.Resolve(state, 0, ability);
            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(1, result.events.Count);
        }

        private static MultiplayerRoomState CreateRoom()
        {
            return MultiplayerProtocol.CreateRoom("ROOM-PLAYTABLE", "D", "p1", 881, new PackSyncInfo
            {
                pack_id = "vanguard_th",
                source_version = "test",
                definition_hash = "definition",
                image_manifest_hash = "image-manifest",
                image_content_hash = "image-content"
            });
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
            return CreatePendingAutoAbilityQueuePayload(room, 1);
        }

        private static NetworkPendingAutoAbilityQueuePayload CreatePendingAutoAbilityQueuePayload(MultiplayerRoomState room, int pendingCount)
        {
            var queue = new PendingAutoAbilityQueue
            {
                queue_id = "queue-1",
                pending = new List<PendingAutoAbility>()
            };

            for (int i = 1; i <= pendingCount; i++)
            {
                queue.pending.Add(new PendingAutoAbility
                {
                    pending_id = "pending-" + i,
                    source_card_instance_id = "src-" + i,
                    source_card_id = "CARD-" + i,
                    player_index = (i - 1) % 2,
                    timing_event = i % 2 == 0 ? "OnBattle" : "OnDraw",
                    summary = "CARD-" + i + " ability"
                });
            }

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
            MultiplayerRoomState room,
            string pendingId,
            int selectedIndex)
        {
            return PendingAutoAbilityResolutionRequestPayloadCodec.Encode(
                room.room_id,
                0,
                new PendingAutoAbilityResolutionRequest
                {
                    selected_index = selectedIndex,
                    pending_id = pendingId,
                    player_index = 0,
                    timing_event = "OnBattle",
                    source_card_instance_id = "src-" + selectedIndex,
                    source_card_id = "CARD-" + selectedIndex,
                    summary = "Resolve " + pendingId
                },
                GameStateViewPerspective.Player,
                0);
        }

        private static NetworkPendingAutoAbilityManualResolutionDecisionPayload CreatePendingAutoAbilityManualResolutionDecisionPayload(
            MultiplayerRoomState room,
            string pendingId,
            int selectedIndex)
        {
            return CreatePendingAutoAbilityManualResolutionDecisionPayload(
                room,
                pendingId,
                selectedIndex,
                PendingAutoAbilityManualResolutionDecisionTypes.Resolve);
        }

        private static NetworkPendingAutoAbilityManualResolutionDecisionPayload CreatePendingAutoAbilityManualResolutionDecisionPayload(
            MultiplayerRoomState room,
            string pendingId,
            int selectedIndex,
            string decisionType)
        {
            PendingAutoAbilityManualResolutionDecisionResult result =
                PendingAutoAbilityManualResolutionDecisionFactory.Create(
                    new PendingAutoAbilityResolutionRequest
                    {
                        selected_index = selectedIndex,
                        pending_id = pendingId,
                        player_index = 0,
                        timing_event = "OnBattle",
                        source_card_instance_id = "src-" + selectedIndex,
                        source_card_id = "CARD-" + selectedIndex,
                        summary = "Resolve " + pendingId
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

        private sealed class FakeTransport : IMultiplayerTransport
        {
            public readonly System.Collections.Generic.List<NetworkEventEnvelope> sentEvents = new System.Collections.Generic.List<NetworkEventEnvelope>();
            public readonly System.Collections.Generic.List<NetworkTriggerCheckReplayLogPayload> sentTriggerCheckLogs =
                new System.Collections.Generic.List<NetworkTriggerCheckReplayLogPayload>();
            public readonly System.Collections.Generic.List<NetworkPendingAutoAbilityQueuePayload> sentPendingAutoAbilityQueues =
                new System.Collections.Generic.List<NetworkPendingAutoAbilityQueuePayload>();
            public readonly System.Collections.Generic.List<NetworkPendingAutoAbilityResolutionRequestPayload> sentPendingAutoAbilityResolutionRequests =
                new System.Collections.Generic.List<NetworkPendingAutoAbilityResolutionRequestPayload>();
            public readonly System.Collections.Generic.List<NetworkPendingAutoAbilityManualResolutionDecisionPayload> sentPendingAutoAbilityManualResolutionDecisions =
                new System.Collections.Generic.List<NetworkPendingAutoAbilityManualResolutionDecisionPayload>();

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

            public event System.Action<MultiplayerRoomState> RoomStateReceived;
            public event System.Action<NetworkEventEnvelope> GameEventReceived;
            public event System.Action<NetworkPublicGameEvent> PublicGameEventReceived;
            public event System.Action<NetworkTriggerCheckReplayLogPayload> TriggerCheckReplayLogReceived;
            public event System.Action<NetworkPendingAutoAbilityQueuePayload> PendingAutoAbilityQueueReceived;
            public event System.Action<NetworkPendingAutoAbilityResolutionRequestPayload> PendingAutoAbilityResolutionRequestReceived;
            public event System.Action<NetworkPendingAutoAbilityManualResolutionDecisionPayload> PendingAutoAbilityManualResolutionDecisionReceived;
            public event System.Action<NetworkReconnectRequest> ReconnectRequestReceived;
            public event System.Action<NetworkEventBatch> ReconnectBatchReceived;
            public event System.Action<NetworkDeckRevealRequest> DeckRevealRequestReceived;
            public event System.Action<NetworkDeckRevealResponse> DeckRevealResponseReceived;

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

            public void EmitTriggerCheckReplayLog(NetworkTriggerCheckReplayLogPayload payload)
            {
                System.Action<NetworkTriggerCheckReplayLogPayload> handler = TriggerCheckReplayLogReceived;
                if (handler != null)
                {
                    handler(payload);
                }
            }

            public void EmitPendingAutoAbilityQueue(NetworkPendingAutoAbilityQueuePayload payload)
            {
                System.Action<NetworkPendingAutoAbilityQueuePayload> handler = PendingAutoAbilityQueueReceived;
                if (handler != null)
                {
                    handler(payload);
                }
            }

            public void EmitPendingAutoAbilityResolutionRequest(
                NetworkPendingAutoAbilityResolutionRequestPayload payload)
            {
                System.Action<NetworkPendingAutoAbilityResolutionRequestPayload> handler =
                    PendingAutoAbilityResolutionRequestReceived;
                if (handler != null)
                {
                    handler(payload);
                }
            }

            public void EmitPendingAutoAbilityManualResolutionDecision(
                NetworkPendingAutoAbilityManualResolutionDecisionPayload payload)
            {
                System.Action<NetworkPendingAutoAbilityManualResolutionDecisionPayload> handler =
                    PendingAutoAbilityManualResolutionDecisionReceived;
                if (handler != null)
                {
                    handler(payload);
                }
            }

            public void Tick()
            {
            }

            public void Disconnect()
            {
            }
        }
    }
}
