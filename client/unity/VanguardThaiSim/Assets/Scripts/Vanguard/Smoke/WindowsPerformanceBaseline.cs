using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;

namespace VanguardThaiSim.Smoke
{
    [Serializable]
    public sealed class WindowsPerformanceBaselineReport
    {
        public int schema_version = 1;
        public string milestone = "M27-03";
        public bool accepted;
        public int card_count;
        public int queried_card_count;
        public int main_deck_count;
        public int ride_deck_count;
        public long repository_load_ms;
        public long card_query_ms;
        public long card_detail_ms;
        public long deck_validation_ms;
        public long deck_code_roundtrip_ms;
        public string summary;
        public List<string> blockers = new List<string>();

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static WindowsPerformanceBaselineReport FromJson(string json)
        {
            WindowsPerformanceBaselineReport report =
                JsonUtility.FromJson<WindowsPerformanceBaselineReport>(json);
            if (report == null)
            {
                throw new ArgumentException(
                    "Windows performance baseline report JSON could not be parsed.",
                    "json");
            }

            report.EnsureLists();
            return report;
        }

        public void EnsureLists()
        {
            if (blockers == null)
            {
                blockers = new List<string>();
            }
        }
    }

    public static class WindowsPerformanceBaseline
    {
        private const int QueryLimit = 120;
        private const int MainDeckSize = 50;
        private const int RideDeckSize = 4;

        public static WindowsPerformanceBaselineReport Run()
        {
            WindowsPerformanceBaselineReport report = new WindowsPerformanceBaselineReport();
            try
            {
                string packDirectory = CardPackFileSystem.DefaultPackDirectory;
                CardPackManifest manifest = CardPackFileSystem.LoadManifest(packDirectory);
                string databasePath = CardPackFileSystem.GetDatabasePath(packDirectory, manifest);
                string catalogPath = CardPackFileSystem.GetCatalogPath(packDirectory, manifest);
                if (!File.Exists(databasePath) && !File.Exists(catalogPath))
                {
                    report.blockers.Add("Card database/catalog is missing.");
                    Finish(report);
                    return report;
                }

                Stopwatch stopwatch = Stopwatch.StartNew();
                CardRepositoryLoadResult loadResult = CardRepositoryFactory.LoadDefault(packDirectory, manifest);
                stopwatch.Stop();
                report.repository_load_ms = stopwatch.ElapsedMilliseconds;

                using (loadResult.Repository as IDisposable)
                {
                    report.card_count = loadResult.Repository.CountCards();
                    stopwatch.Restart();
                    IReadOnlyList<CardSummary> cards =
                        loadResult.Repository.QueryCards(new CardQueryOptions { Limit = QueryLimit });
                    stopwatch.Stop();
                    report.card_query_ms = stopwatch.ElapsedMilliseconds;
                    report.queried_card_count = cards.Count;

                    if (cards.Count < MainDeckSize + RideDeckSize)
                    {
                        report.blockers.Add("Card query returned fewer than 54 cards.");
                        Finish(report);
                        return report;
                    }

                    stopwatch.Restart();
                    CardDetail first = loadResult.Repository.GetCard(cards[0].CardId);
                    stopwatch.Stop();
                    report.card_detail_ms = stopwatch.ElapsedMilliseconds;
                    if (first == null)
                    {
                        report.blockers.Add("First card detail could not be loaded.");
                        Finish(report);
                        return report;
                    }

                    VanguardDeck deck = CreateDeck(cards);
                    stopwatch.Restart();
                    DeckValidationResult validation = new DeckValidator(loadResult.Repository).Validate(deck);
                    stopwatch.Stop();
                    report.deck_validation_ms = stopwatch.ElapsedMilliseconds;
                    report.main_deck_count = validation.MainCount;
                    report.ride_deck_count = validation.RideCount;
                    if (!validation.IsPlayable)
                    {
                        report.blockers.Add("Smoke deck is not playable.");
                    }

                    stopwatch.Restart();
                    VanguardDeck imported = DeckCodeCodec.Import(DeckCodeCodec.Export(deck));
                    stopwatch.Stop();
                    report.deck_code_roundtrip_ms = stopwatch.ElapsedMilliseconds;
                    if (imported.TotalCards(DeckZone.Main) != MainDeckSize ||
                        imported.TotalCards(DeckZone.Ride) != RideDeckSize)
                    {
                        report.blockers.Add("Deck code round-trip changed deck counts.");
                    }
                }
            }
            catch (Exception exception)
            {
                report.blockers.Add(exception.GetType().Name + ": " + exception.Message);
            }

            Finish(report);
            return report;
        }

        private static void Finish(WindowsPerformanceBaselineReport report)
        {
            report.EnsureLists();
            report.accepted = report.blockers.Count == 0;
            report.summary = report.accepted
                ? "Windows card/deck performance baseline recorded."
                : "Windows card/deck performance baseline blocked.";
        }

        private static VanguardDeck CreateDeck(IReadOnlyList<CardSummary> cards)
        {
            VanguardDeck deck = VanguardDeck.Create("M27-03 Performance Baseline", "D", "vanguard_th", "baseline");
            for (int i = 0; i < MainDeckSize; i++)
            {
                deck.AddCard(DeckZone.Main, cards[i].CardId, 1);
            }

            for (int i = 0; i < RideDeckSize; i++)
            {
                deck.AddCard(DeckZone.Ride, cards[MainDeckSize + i].CardId, 1);
            }

            return deck;
        }
    }
}
