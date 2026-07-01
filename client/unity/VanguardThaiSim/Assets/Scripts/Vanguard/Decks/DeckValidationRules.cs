namespace VanguardThaiSim.Decks
{
    public sealed class DeckValidationRules
    {
        public int MainDeckSize { get; set; } = 50;
        public int RideDeckMax { get; set; } = 4;
        public int GDeckMax { get; set; } = 16;
        public int DefaultCopyLimit { get; set; } = 4;
    }
}
