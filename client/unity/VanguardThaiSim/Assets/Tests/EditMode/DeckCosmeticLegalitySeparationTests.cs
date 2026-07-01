using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;

namespace VanguardThaiSim.Tests
{
    public sealed class DeckCosmeticLegalitySeparationTests
    {
        private const string KnownCardId = "BT01-001TH";

        [Test]
        public void CosmeticMetadataDoesNotChangeDeckValidationResult()
        {
            var repository = new SingleCardRepository();
            var validator = new DeckValidator(repository, new DeckValidationRules { MainDeckSize = 4 });
            VanguardDeck baseline = CreatePlayableDeck();
            VanguardDeck styled = VanguardDeck.FromJson(baseline.ToJson(false));
            styled.appearance = new DeckAppearanceMetadata
            {
                sleeve_key = "blue",
                card_back_key = "standard",
                playmat_key = "table-grid",
                crest_key = "royal-paladin",
                persona_shield_key = "persona-standard",
                gift_marker_key = "force",
                quick_shield_key = "quick-shield"
            };
            styled.cosmetics = new DeckCosmetics
            {
                sleeve = "legacy-sleeve",
                playmat = "legacy-playmat",
                crest = "legacy-crest",
                persona_shield = "legacy-persona"
            };

            DeckValidationResult before = validator.Validate(baseline);
            DeckValidationResult after = validator.Validate(styled);

            Assert.AreEqual(before.MainCount, after.MainCount);
            Assert.AreEqual(before.RideCount, after.RideCount);
            Assert.AreEqual(before.GCount, after.GCount);
            Assert.AreEqual(before.ErrorCount, after.ErrorCount);
            Assert.AreEqual(before.WarningCount, after.WarningCount);
            Assert.AreEqual(before.IsLegal, after.IsLegal);
            Assert.AreEqual(before.IsComplete, after.IsComplete);
            Assert.AreEqual(before.IsPlayable, after.IsPlayable);
        }

        [Test]
        public void DeckValidatorDoesNotReferenceCosmeticModels()
        {
            string validatorPath = Path.Combine(
                Application.dataPath,
                "Scripts",
                "Vanguard",
                "Decks",
                "DeckValidator.cs");
            string source = File.ReadAllText(validatorPath);

            Assert.IsFalse(source.Contains("appearance"));
            Assert.IsFalse(source.Contains("cosmetics"));
            Assert.IsFalse(source.Contains("DeckAppearanceMetadata"));
            Assert.IsFalse(source.Contains("DeckCosmetics"));
        }

        private static VanguardDeck CreatePlayableDeck()
        {
            VanguardDeck deck = VanguardDeck.Create("Cosmetic Guard Deck", "D", "vanguard_th", "test");
            deck.AddCard(DeckZone.Main, KnownCardId, 4);
            return deck;
        }

        private sealed class SingleCardRepository : ICardRepository
        {
            public int CountCards()
            {
                return 1;
            }

            public int CountSeries()
            {
                return 1;
            }

            public int CountClans()
            {
                return 1;
            }

            public CardDetail GetCard(string cardId)
            {
                if (!string.Equals(cardId, KnownCardId, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return new CardDetail
                {
                    CardId = KnownCardId,
                    DeckLimit = 4
                };
            }

            public IReadOnlyList<CardSummary> QueryCards(CardQueryOptions options)
            {
                return Array.Empty<CardSummary>();
            }

            public IReadOnlyList<SeriesOption> ListSeries()
            {
                return Array.Empty<SeriesOption>();
            }

            public IReadOnlyList<ClanOption> ListClans()
            {
                return Array.Empty<ClanOption>();
            }
        }
    }
}
