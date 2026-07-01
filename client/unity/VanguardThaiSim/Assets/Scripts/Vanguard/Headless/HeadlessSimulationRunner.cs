using System;
using System.Collections.Generic;
using System.IO;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Headless
{
    public static class HeadlessSimulationRunner
    {
        public const int DefaultSeed = 1701;
        public const string DefaultRuleset = "D";

        private const int MainDeckCount = 50;
        private const int RideDeckCount = 4;

        public static HeadlessSimulationResult RunDefault()
        {
            return Run(HeadlessSimulationRequest.Default());
        }

        public static HeadlessSimulationResult Run(int seed)
        {
            return Run(new HeadlessSimulationRequest { seed = seed });
        }

        public static HeadlessSimulationResult Run(HeadlessSimulationRequest request)
        {
            return RunWithReplay(request).result;
        }

        public static HeadlessSimulationOutput RunWithReplay(HeadlessSimulationRequest request)
        {
            HeadlessSimulationRequest safeRequest = (request ?? HeadlessSimulationRequest.Default()).CloneNormalized();
            List<GameEvent> eventLog = new List<GameEvent>();
            try
            {
                VanguardDeck deck = CreateDeckFromRequest(safeRequest, out string deckSource);
                GameState state = GameStateFactory.CreateTwoPlayerGame(deck, deck, safeRequest.seed, "headless-p1", "headless-p2");
                List<string> actionTypes = new List<string>();

                Execute(state, FirstAction(RulesCore.GetLegalActions(state, 0), GameActionType.Draw), actionTypes, eventLog);
                Execute(
                    state,
                    FirstMove(RulesCore.GetLegalActions(state, 0), GameZone.Hand, GameZone.RearGuard),
                    actionTypes,
                    eventLog);
                Execute(state, FirstPhase(RulesCore.GetLegalActions(state, 0), GamePhase.Main), actionTypes, eventLog);
                Execute(state, FirstGift(RulesCore.GetLegalActions(state, 0), GiftMarkerType.Protect), actionTypes, eventLog);

                PlayerGameState player = state.GetPlayer(0);
                HeadlessSimulationResult result = new HeadlessSimulationResult
                {
                    accepted = true,
                    seed = safeRequest.seed,
                    ruleset = safeRequest.ruleset,
                    deck_source = deckSource,
                    actions_executed = actionTypes.Count,
                    event_count = state.event_log.Count,
                    final_phase = state.phase.ToString(),
                    player_count = state.players.Count,
                    player0_deck_count = player.CountZone(GameZone.Deck),
                    player0_hand_count = player.CountZone(GameZone.Hand),
                    player0_rear_guard_count = player.CountZone(GameZone.RearGuard),
                    player0_protect_markers = player.GetGiftMarkerCount(GiftMarkerType.Protect),
                    action_types = actionTypes
                };

                return new HeadlessSimulationOutput
                {
                    result = result,
                    replay = HeadlessReplayArtifact.FromResult(result, eventLog)
                };
            }
            catch (Exception exception)
            {
                HeadlessSimulationResult result =
                    HeadlessSimulationResult.Failed(exception.GetType().Name + ": " + exception.Message, safeRequest.seed);
                result.ruleset = safeRequest.ruleset;
                result.deck_source = string.IsNullOrWhiteSpace(safeRequest.deck_code) ? "default" : "deck_code";
                return new HeadlessSimulationOutput
                {
                    result = result,
                    replay = HeadlessReplayArtifact.FromResult(result, eventLog)
                };
            }
        }

        private static VanguardDeck CreateDeckFromRequest(HeadlessSimulationRequest request, out string deckSource)
        {
            string packDirectory = CardPackFileSystem.DefaultPackDirectory;
            CardPackManifest manifest = CardPackFileSystem.LoadManifest(packDirectory);
            string databasePath = CardPackFileSystem.GetDatabasePath(packDirectory, manifest);
            if (!File.Exists(databasePath))
            {
                throw new FileNotFoundException("Default card database is missing.", databasePath);
            }

            using (SqliteCardRepository repository = new SqliteCardRepository(databasePath))
            {
                VanguardDeck deck;
                if (string.IsNullOrWhiteSpace(request.deck_code))
                {
                    deck = CreateGeneratedDeck(repository, manifest, request.ruleset);
                    deckSource = "default";
                }
                else
                {
                    deck = DeckCodeCodec.Import(request.deck_code);
                    deck.format = request.ruleset;
                    deckSource = "deck_code";
                }

                DeckValidationResult validation = new DeckValidator(repository).Validate(deck);
                if (!validation.IsPlayable)
                {
                    throw new InvalidOperationException(
                        "Headless default deck is not playable. Errors=" + validation.ErrorCount +
                        " Warnings=" + validation.WarningCount);
                }

                return deck;
            }
        }

        private static VanguardDeck CreateGeneratedDeck(
            SqliteCardRepository repository,
            CardPackManifest manifest,
            string ruleset)
        {
            IReadOnlyList<CardSummary> cards = repository.QueryCards(new CardQueryOptions { Limit = 80 });
            if (cards.Count < MainDeckCount + RideDeckCount)
            {
                throw new InvalidOperationException("Default pack does not have enough cards for headless smoke deck.");
            }

            VanguardDeck deck = VanguardDeck.Create(
                "M17 Headless Default",
                ruleset,
                manifest.pack_id,
                manifest.source_version);
            for (int i = 0; i < MainDeckCount; i++)
            {
                deck.AddCard(DeckZone.Main, cards[i].CardId, 1);
            }

            for (int i = 0; i < RideDeckCount; i++)
            {
                deck.AddCard(DeckZone.Ride, cards[MainDeckCount + i].CardId, 1);
            }

            return deck;
        }

        private static void Execute(
            GameState state,
            LegalGameAction action,
            List<string> actionTypes,
            List<GameEvent> eventLog)
        {
            GameEvent gameEvent = RulesCore.ExecuteOrThrow(state, action);
            actionTypes.Add(gameEvent.action_type.ToString());
            eventLog.Add(gameEvent);
        }

        private static LegalGameAction FirstAction(IReadOnlyList<LegalGameAction> actions, GameActionType type)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].action_type == type)
                {
                    return actions[i];
                }
            }

            throw new InvalidOperationException("Missing legal action: " + type);
        }

        private static LegalGameAction FirstMove(
            IReadOnlyList<LegalGameAction> actions,
            GameZone fromZone,
            GameZone toZone)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                LegalGameAction action = actions[i];
                if (action.action_type == GameActionType.MoveCard &&
                    action.from_zone == fromZone &&
                    action.to_zone == toZone)
                {
                    return action;
                }
            }

            throw new InvalidOperationException("Missing legal move: " + fromZone + " to " + toZone);
        }

        private static LegalGameAction FirstPhase(IReadOnlyList<LegalGameAction> actions, GamePhase phase)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                LegalGameAction action = actions[i];
                if (action.action_type == GameActionType.SetPhase && action.phase == phase)
                {
                    return action;
                }
            }

            throw new InvalidOperationException("Missing legal phase command: " + phase);
        }

        private static LegalGameAction FirstGift(IReadOnlyList<LegalGameAction> actions, GiftMarkerType markerType)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                LegalGameAction action = actions[i];
                if (action.action_type == GameActionType.AddGiftMarker &&
                    action.gift_marker_type == markerType)
                {
                    return action;
                }
            }

            throw new InvalidOperationException("Missing legal gift marker command: " + markerType);
        }
    }
}
