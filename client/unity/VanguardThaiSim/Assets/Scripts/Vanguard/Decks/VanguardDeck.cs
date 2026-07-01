using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Decks
{
    [Serializable]
    public sealed class VanguardDeck
    {
        public string deck_id;
        public string name;
        public string format;
        public string card_pack_id;
        public string card_pack_version;
        public List<DeckCardEntry> main = new List<DeckCardEntry>();
        public List<DeckCardEntry> ride = new List<DeckCardEntry>();
        public List<DeckCardEntry> g = new List<DeckCardEntry>();
        public DeckAppearanceMetadata appearance = DeckAppearanceMetadata.CreateDefault();
        public DeckCosmetics cosmetics = new DeckCosmetics();

        public static VanguardDeck Create(string name, string format, string cardPackId, string cardPackVersion)
        {
            VanguardDeck deck = new VanguardDeck
            {
                deck_id = Guid.NewGuid().ToString("N"),
                name = string.IsNullOrWhiteSpace(name) ? "New Deck" : name,
                format = string.IsNullOrWhiteSpace(format) ? "D" : format,
                card_pack_id = cardPackId,
                card_pack_version = cardPackVersion
            };
            deck.EnsureLists();
            return deck;
        }

        public static VanguardDeck FromJson(string json)
        {
            VanguardDeck deck = JsonUtility.FromJson<VanguardDeck>(json);
            if (deck == null)
            {
                throw new ArgumentException("Deck JSON could not be parsed.", nameof(json));
            }

            deck.EnsureLists();
            if (string.IsNullOrWhiteSpace(deck.deck_id))
            {
                deck.deck_id = Guid.NewGuid().ToString("N");
            }

            return deck;
        }

        public string ToJson(bool prettyPrint = false)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public IReadOnlyList<DeckCardEntry> GetEntries(DeckZone zone)
        {
            return GetMutableEntries(zone);
        }

        public int GetQuantity(DeckZone zone, string cardId)
        {
            DeckCardEntry entry = FindEntry(GetMutableEntries(zone), cardId);
            return entry == null ? 0 : entry.quantity;
        }

        public void AddCard(DeckZone zone, string cardId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                return;
            }

            SetQuantity(zone, cardId, GetQuantity(zone, cardId) + quantity);
        }

        public bool RemoveCard(DeckZone zone, string cardId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                return false;
            }

            int current = GetQuantity(zone, cardId);
            if (current <= 0)
            {
                return false;
            }

            SetQuantity(zone, cardId, Math.Max(0, current - quantity));
            return true;
        }

        public void SetQuantity(DeckZone zone, string cardId, int quantity)
        {
            if (string.IsNullOrWhiteSpace(cardId))
            {
                throw new ArgumentException("Card id is required.", nameof(cardId));
            }

            List<DeckCardEntry> entries = GetMutableEntries(zone);
            DeckCardEntry entry = FindEntry(entries, cardId);
            if (quantity <= 0)
            {
                if (entry != null)
                {
                    entries.Remove(entry);
                }

                return;
            }

            if (entry == null)
            {
                entries.Add(new DeckCardEntry(cardId, quantity));
            }
            else
            {
                entry.quantity = quantity;
            }
        }

        public int TotalCards(DeckZone zone)
        {
            int count = 0;
            foreach (DeckCardEntry entry in GetMutableEntries(zone))
            {
                count += Math.Max(0, entry.quantity);
            }

            return count;
        }

        public int TotalCards()
        {
            return TotalCards(DeckZone.Main) + TotalCards(DeckZone.Ride) + TotalCards(DeckZone.G);
        }

        public void Clear(DeckZone zone)
        {
            GetMutableEntries(zone).Clear();
        }

        public void ClearAll()
        {
            main.Clear();
            ride.Clear();
            g.Clear();
        }

        private void EnsureLists()
        {
            if (main == null)
            {
                main = new List<DeckCardEntry>();
            }

            if (ride == null)
            {
                ride = new List<DeckCardEntry>();
            }

            if (g == null)
            {
                g = new List<DeckCardEntry>();
            }

            if (cosmetics == null)
            {
                cosmetics = new DeckCosmetics();
            }

            if (appearance == null)
            {
                appearance = DeckAppearanceMetadata.FromLegacyCosmetics(cosmetics);
            }
            else
            {
                appearance = DeckAppearanceMetadata.Normalize(appearance);
            }
        }

        private List<DeckCardEntry> GetMutableEntries(DeckZone zone)
        {
            EnsureLists();
            switch (zone)
            {
                case DeckZone.Main:
                    return main;
                case DeckZone.Ride:
                    return ride;
                case DeckZone.G:
                    return g;
                default:
                    throw new ArgumentOutOfRangeException(nameof(zone), zone, null);
            }
        }

        private static DeckCardEntry FindEntry(List<DeckCardEntry> entries, string cardId)
        {
            foreach (DeckCardEntry entry in entries)
            {
                if (string.Equals(entry.card_id, cardId, StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
