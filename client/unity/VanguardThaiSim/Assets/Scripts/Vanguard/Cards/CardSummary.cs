namespace VanguardThaiSim.Cards
{
    public readonly struct CardSummary
    {
        public readonly string CardId;
        public readonly string NameTh;
        public readonly string Series;
        public readonly string SeriesCode;
        public readonly string Clan;
        public readonly string Nation;
        public readonly int? Grade;
        public readonly string Type1;
        public readonly string Trigger;
        public readonly string ImageRelativePath;
        public readonly bool ImageExists;

        public CardSummary(
            string cardId,
            string nameTh,
            string series,
            string seriesCode,
            string clan,
            string nation,
            int? grade,
            string type1,
            string trigger,
            string imageRelativePath,
            bool imageExists)
        {
            CardId = cardId;
            NameTh = nameTh;
            Series = series;
            SeriesCode = seriesCode;
            Clan = clan;
            Nation = nation;
            Grade = grade;
            Type1 = type1;
            Trigger = trigger;
            ImageRelativePath = imageRelativePath;
            ImageExists = imageExists;
        }
    }
}

