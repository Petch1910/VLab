using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;

namespace VanguardThaiSim.Tests
{
    public sealed class DeckAppearanceMetadataTests
    {
        private const string KnownCardId = "BT01-001TH";

        [Test]
        public void DefaultAppearanceUsesSafeKeys()
        {
            DeckAppearanceMetadata metadata = DeckAppearanceMetadata.CreateDefault();

            Assert.AreEqual(DeckAppearanceMetadata.DefaultKey, metadata.sleeve_key);
            Assert.AreEqual(DeckAppearanceMetadata.DefaultKey, metadata.card_back_key);
            Assert.AreEqual(DeckAppearanceMetadata.DefaultKey, metadata.playmat_key);
            Assert.AreEqual(DeckAppearanceMetadata.DefaultKey, metadata.crest_key);
            Assert.AreEqual(DeckAppearanceMetadata.DefaultKey, metadata.persona_shield_key);
            Assert.AreEqual(DeckAppearanceMetadata.DefaultKey, metadata.gift_marker_key);
            Assert.AreEqual(DeckAppearanceMetadata.DefaultKey, metadata.quick_shield_key);
        }

        [Test]
        public void JsonRoundTripPreservesValidAppearanceKeys()
        {
            var metadata = new DeckAppearanceMetadata
            {
                sleeve_key = "royal-blue",
                card_back_key = "classic.back",
                playmat_key = "area_style_01",
                crest_key = "royal-paladin",
                persona_shield_key = "persona.default",
                gift_marker_key = "gift-force",
                quick_shield_key = "quick-shield-01"
            };

            DeckAppearanceMetadata roundTrip = DeckAppearanceMetadata.FromJson(metadata.ToJson(false));

            Assert.AreEqual("royal-blue", roundTrip.sleeve_key);
            Assert.AreEqual("classic.back", roundTrip.card_back_key);
            Assert.AreEqual("area_style_01", roundTrip.playmat_key);
            Assert.AreEqual("royal-paladin", roundTrip.crest_key);
            Assert.AreEqual("persona.default", roundTrip.persona_shield_key);
            Assert.AreEqual("gift-force", roundTrip.gift_marker_key);
            Assert.AreEqual("quick-shield-01", roundTrip.quick_shield_key);
        }

        [Test]
        public void NormalizeTrimsSafeKeysAndFallbacksUnsafeKeys()
        {
            var metadata = new DeckAppearanceMetadata
            {
                sleeve_key = "  sleeve-01  ",
                card_back_key = "../private",
                playmat_key = "folder/playmat",
                crest_key = "bad key",
                persona_shield_key = "persona_01",
                gift_marker_key = "gift\\marker",
                quick_shield_key = ""
            };

            DeckAppearanceMetadata normalized = DeckAppearanceMetadata.Normalize(metadata);

            Assert.AreEqual("sleeve-01", normalized.sleeve_key);
            Assert.AreEqual(DeckAppearanceMetadata.DefaultKey, normalized.card_back_key);
            Assert.AreEqual(DeckAppearanceMetadata.DefaultKey, normalized.playmat_key);
            Assert.AreEqual(DeckAppearanceMetadata.DefaultKey, normalized.crest_key);
            Assert.AreEqual("persona_01", normalized.persona_shield_key);
            Assert.AreEqual(DeckAppearanceMetadata.DefaultKey, normalized.gift_marker_key);
            Assert.AreEqual(DeckAppearanceMetadata.DefaultKey, normalized.quick_shield_key);
        }

        [Test]
        public void NormalizeDoesNotMutateSource()
        {
            var metadata = new DeckAppearanceMetadata
            {
                sleeve_key = "  sleeve-01  ",
                card_back_key = "bad/key"
            };
            string before = JsonUtility.ToJson(metadata, false);

            DeckAppearanceMetadata.Normalize(metadata);

            Assert.AreEqual(before, JsonUtility.ToJson(metadata, false));
        }

        [Test]
        public void FromJsonFallsBackForEmptyOrInvalidJson()
        {
            DeckAppearanceMetadata empty = DeckAppearanceMetadata.FromJson("");
            DeckAppearanceMetadata invalid = DeckAppearanceMetadata.FromJson("{not valid json");

            Assert.AreEqual(DeckAppearanceMetadata.DefaultKey, empty.sleeve_key);
            Assert.AreEqual(DeckAppearanceMetadata.DefaultKey, invalid.playmat_key);
        }

        [Test]
        public void LegacyCosmeticsMapToAppearanceMetadata()
        {
            var cosmetics = new DeckCosmetics
            {
                sleeve = "legacy-sleeve",
                playmat = "legacy-playmat",
                crest = "legacy-crest",
                persona_shield = "legacy-persona"
            };

            DeckAppearanceMetadata metadata = DeckAppearanceMetadata.FromLegacyCosmetics(cosmetics);

            Assert.AreEqual("legacy-sleeve", metadata.sleeve_key);
            Assert.AreEqual("legacy-sleeve", metadata.card_back_key);
            Assert.AreEqual("legacy-playmat", metadata.playmat_key);
            Assert.AreEqual("legacy-crest", metadata.crest_key);
            Assert.AreEqual("legacy-persona", metadata.persona_shield_key);
            Assert.AreEqual(DeckAppearanceMetadata.DefaultKey, metadata.gift_marker_key);
            Assert.AreEqual(DeckAppearanceMetadata.DefaultKey, metadata.quick_shield_key);
        }

        [Test]
        public void VanguardDeckRoundTripPreservesAppearanceMetadata()
        {
            VanguardDeck deck = CreatePlayableDeck();
            deck.appearance = new DeckAppearanceMetadata
            {
                sleeve_key = "sleeve-01",
                card_back_key = "back-01",
                playmat_key = "playmat-01",
                gift_marker_key = "gift-force"
            };

            VanguardDeck roundTrip = VanguardDeck.FromJson(deck.ToJson(false));

            Assert.NotNull(roundTrip.appearance);
            Assert.AreEqual("sleeve-01", roundTrip.appearance.sleeve_key);
            Assert.AreEqual("back-01", roundTrip.appearance.card_back_key);
            Assert.AreEqual("playmat-01", roundTrip.appearance.playmat_key);
            Assert.AreEqual("gift-force", roundTrip.appearance.gift_marker_key);
        }

        [Test]
        public void DeckCodeRoundTripPreservesAppearanceMetadata()
        {
            VanguardDeck deck = CreatePlayableDeck();
            deck.appearance = new DeckAppearanceMetadata
            {
                sleeve_key = "sleeve-01",
                card_back_key = "back-01",
                playmat_key = "playmat-01",
                quick_shield_key = "quick-shield-01"
            };

            VanguardDeck roundTrip = DeckCodeCodec.Import(DeckCodeCodec.Export(deck));

            Assert.NotNull(roundTrip.appearance);
            Assert.AreEqual("sleeve-01", roundTrip.appearance.sleeve_key);
            Assert.AreEqual("back-01", roundTrip.appearance.card_back_key);
            Assert.AreEqual("playmat-01", roundTrip.appearance.playmat_key);
            Assert.AreEqual("quick-shield-01", roundTrip.appearance.quick_shield_key);
        }

        [Test]
        public void AppearanceMetadataDoesNotAffectDeckLegality()
        {
            var repository = new SingleCardRepository();
            var validator = new DeckValidator(repository, new DeckValidationRules { MainDeckSize = 4 });
            VanguardDeck baseline = CreatePlayableDeck();
            VanguardDeck styled = VanguardDeck.FromJson(baseline.ToJson(false));
            styled.appearance = new DeckAppearanceMetadata
            {
                sleeve_key = "sleeve-01",
                card_back_key = "back-01",
                playmat_key = "playmat-01",
                crest_key = "royal-paladin",
                persona_shield_key = "persona-01",
                gift_marker_key = "gift-force",
                quick_shield_key = "quick-shield-01"
            };

            DeckValidationResult before = validator.Validate(baseline);
            DeckValidationResult after = validator.Validate(styled);

            Assert.AreEqual(before.MainCount, after.MainCount);
            Assert.AreEqual(before.RideCount, after.RideCount);
            Assert.AreEqual(before.GCount, after.GCount);
            Assert.AreEqual(before.ErrorCount, after.ErrorCount);
            Assert.AreEqual(before.WarningCount, after.WarningCount);
            Assert.AreEqual(before.IsPlayable, after.IsPlayable);
        }

        private static VanguardDeck CreatePlayableDeck()
        {
            VanguardDeck deck = VanguardDeck.Create("Appearance Deck", "D", "vanguard_th", "test");
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
