namespace VanguardThaiSim.Cards
{
    public sealed class CardQueryOptions
    {
        public string SearchText { get; set; }
        public string Series { get; set; }
        public string Clan { get; set; }
        public string Nation { get; set; }
        public int? Grade { get; set; }
        public string Type1 { get; set; }
        public int Limit { get; set; } = 60;
        public int Offset { get; set; }
    }
}
