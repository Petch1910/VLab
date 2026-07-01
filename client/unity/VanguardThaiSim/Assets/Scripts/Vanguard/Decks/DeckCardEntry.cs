using System;

namespace VanguardThaiSim.Decks
{
    [Serializable]
    public sealed class DeckCardEntry
    {
        public string card_id;
        public int quantity;

        public DeckCardEntry()
        {
        }

        public DeckCardEntry(string cardId, int quantity)
        {
            card_id = cardId;
            this.quantity = quantity;
        }
    }
}
