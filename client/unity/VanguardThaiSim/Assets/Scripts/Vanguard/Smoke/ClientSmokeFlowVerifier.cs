using System;
using System.Collections.Generic;
using System.IO;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.Multiplayer;
using VanguardThaiSim.Settings;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Smoke
{
    public static class ClientSmokeFlowVerifier
    {
        private const int RequiredMainCount = 50;
        private const int RequiredRideCount = 4;

        public static ClientSmokeFlowReport Run()
        {
            ClientSmokeFlowReport report = new ClientSmokeFlowReport();
            try
            {
                string packDirectory = CardPackFileSystem.DefaultPackDirectory;
                CardPackManifest manifest = CardPackFileSystem.LoadManifest(packDirectory);
                string databasePath = CardPackFileSystem.GetDatabasePath(packDirectory, manifest);
                string catalogPath = CardPackFileSystem.GetCatalogPath(packDirectory, manifest);
                if (!File.Exists(databasePath) && !File.Exists(catalogPath))
                {
                    report.AddBlocker("Card database/catalog is missing: " + databasePath + " | " + catalogPath);
                    return report;
                }

                CardRepositoryLoadResult loadResult = CardRepositoryFactory.LoadDefault(packDirectory, manifest);
                using (loadResult.Repository as IDisposable)
                {
                    IReadOnlyList<CardSummary> cards = VerifyCardBrowser(loadResult.Repository, report);
                    VanguardDeck deck = VerifyDeckBuilder(loadResult.Repository, cards, report);
                    DeckValidationResult validation = new DeckValidator(loadResult.Repository).Validate(deck);
                    VerifyHomeDashboard(manifest, deck, validation, report);
                    VerifyManual(report);
                    VerifySettings(report);
                    VerifyOnlineRoom(report);
                    VerifyPlayTableFlow(deck, report);
                }

                VerifyResponsiveLayout(report);
            }
            catch (Exception exception)
            {
                report.AddBlocker(exception.GetType().Name + ": " + exception.Message);
            }

            return report;
        }

        private static IReadOnlyList<CardSummary> VerifyCardBrowser(ICardRepository repository, ClientSmokeFlowReport report)
        {
            int cardCount = repository.CountCards();
            if (cardCount <= 0)
            {
                throw new InvalidOperationException("Repository has no cards.");
            }

            IReadOnlyList<CardSummary> cards = repository.QueryCards(new CardQueryOptions { Limit = 80 });
            if (cards.Count < RequiredMainCount + RequiredRideCount)
            {
                throw new InvalidOperationException("Repository query returned fewer than 54 cards.");
            }

            CardDetail first = repository.GetCard(cards[0].CardId);
            if (first == null || string.IsNullOrWhiteSpace(first.CardId))
            {
                throw new InvalidOperationException("Repository could not load the first queried card detail.");
            }

            report.AddStep("Card Browser smoke: loaded " + cardCount + " cards and queried " + cards.Count + " summaries.");
            return cards;
        }

        private static void VerifyHomeDashboard(
            CardPackManifest manifest,
            VanguardDeck deck,
            DeckValidationResult validation,
            ClientSmokeFlowReport report)
        {
            string packStatus = HomeLobbyStatusFormatter.FormatPackStatus(manifest, null);
            string deckStatus = HomeLobbyStatusFormatter.FormatDeckStatus(deck, validation);
            string modeStatus = HomeLobbyStatusFormatter.FormatModeStatus("Smoke route check.");
            SoloPlayEntryFlowStartResult soloResult = SoloPlayEntryFlow.CreateStartRequest(
                deck,
                validation,
                null,
                Array.Empty<VanguardDeck>(),
                new SoloPlayEntryFlowOptions());

            if (string.IsNullOrWhiteSpace(packStatus) ||
                !packStatus.Contains("Cards") ||
                string.IsNullOrWhiteSpace(deckStatus) ||
                !deckStatus.Contains("Main 50") ||
                string.IsNullOrWhiteSpace(modeStatus) ||
                !modeStatus.Contains("Local manual table") ||
                !soloResult.can_start)
            {
                throw new InvalidOperationException("Home Dashboard smoke failed status or Solo setup readiness.");
            }

            report.AddStep("Home smoke: pack, deck, mode status, and Solo setup readiness passed.");
        }

        private static VanguardDeck VerifyDeckBuilder(
            ICardRepository repository,
            IReadOnlyList<CardSummary> cards,
            ClientSmokeFlowReport report)
        {
            VanguardDeck deck = CreateSmokeDeck(cards, "m16-10-smoke");
            DeckValidationResult validation = new DeckValidator(repository).Validate(deck);
            if (!validation.IsPlayable)
            {
                throw new InvalidOperationException(
                    "Smoke deck is not playable. Errors=" + validation.ErrorCount +
                    " Warnings=" + validation.WarningCount);
            }

            string deckCode = DeckCodeCodec.Export(deck);
            VanguardDeck imported = DeckCodeCodec.Import(deckCode);
            if (imported.TotalCards(DeckZone.Main) != RequiredMainCount ||
                imported.TotalCards(DeckZone.Ride) != RequiredRideCount)
            {
                throw new InvalidOperationException("Deck code round-trip changed smoke deck counts.");
            }

            report.AddStep("Deck Builder smoke: playable 50+4 deck validated and deck code round-tripped.");
            return deck;
        }

        private static void VerifyManual(ClientSmokeFlowReport report)
        {
            ManualTutorialReadinessReport manual = ManualTutorialReadinessVerifier.Verify();
            if (manual == null || !manual.accepted)
            {
                string issue = manual == null || manual.issues.Count == 0
                    ? "unknown"
                    : manual.issues[0];
                throw new InvalidOperationException("Manual smoke failed: " + issue);
            }

            report.AddStep("Manual smoke: content, tips, and originality readiness passed.");
        }

        private static void VerifySettings(ClientSmokeFlowReport report)
        {
            PlayerSettings settings = PlayerSettings.CreateDefault();
            string formatted = HomeSettingsPanelFormatter.Format(settings);
            if (string.IsNullOrWhiteSpace(formatted) ||
                !formatted.Contains("Preferred format: D") ||
                !formatted.Contains("Image cache: Balanced") ||
                HomeSettingsPanelFormatter.NextPreferredFormat("D") != "V" ||
                HomeSettingsPanelFormatter.NextImageCacheMode(PlayerImageCacheMode.Balanced) != PlayerImageCacheMode.MemorySaver)
            {
                throw new InvalidOperationException("Settings smoke failed default formatter/cycle checks.");
            }

            report.AddStep("Settings smoke: default player settings and option cycling passed.");
        }

        private static void VerifyOnlineRoom(ClientSmokeFlowReport report)
        {
            WindowsOnlineRoomUsabilityCloseoutReport roomReport =
                WindowsOnlineRoomUsabilityCloseoutReportBuilder.CreateCurrent();
            WindowsOnlineRoomUsabilityCloseoutValidationResult validation =
                WindowsOnlineRoomUsabilityCloseoutReportBuilder.Validate(roomReport);
            if (validation == null || !validation.accepted)
            {
                string issue = validation == null || validation.errors.Count == 0
                    ? "unknown"
                    : validation.errors[0];
                throw new InvalidOperationException("Online Room smoke failed: " + issue);
            }

            report.AddStep("Online Room smoke: Photon trusted-client room usability guard passed.");
        }

        private static void VerifyPlayTableFlow(VanguardDeck deck, ClientSmokeFlowReport report)
        {
            WindowsGameplayCompletionReport gameplayReport =
                WindowsGameplayCompletionVerifier.Run(deck);
            if (gameplayReport == null || !gameplayReport.accepted)
            {
                string issue = gameplayReport == null || gameplayReport.blockers.Count == 0
                    ? "unknown"
                    : gameplayReport.blockers[0];
                throw new InvalidOperationException("Play Table gameplay completion smoke failed: " + issue);
            }

            report.AddStep("Play Table smoke: " + gameplayReport.summary);
        }

        private static void VerifyResponsiveLayout(ClientSmokeFlowReport report)
        {
            ResponsiveLayoutQaReport qaReport = ResponsiveLayoutQaVerifier.ValidateWindowsPlayTableBoardFirst();
            if (!qaReport.IsPass)
            {
                report.AddBlocker("Windows PlayTable board-first layout QA failed: " + qaReport.Issues[0].Code);
                return;
            }

            report.AddStep("Windows layout smoke: PlayTable board-first reference viewports passed.");
        }

        private static VanguardDeck CreateSmokeDeck(IReadOnlyList<CardSummary> cards, string owner)
        {
            VanguardDeck deck = VanguardDeck.Create("M16-10 Smoke " + owner, "D", "vanguard_th", "smoke");
            for (int i = 0; i < RequiredMainCount; i++)
            {
                deck.AddCard(DeckZone.Main, cards[i].CardId, 1);
            }

            for (int i = 0; i < RequiredRideCount; i++)
            {
                deck.AddCard(DeckZone.Ride, cards[RequiredMainCount + i].CardId, 1);
            }

            return deck;
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
    }
}
