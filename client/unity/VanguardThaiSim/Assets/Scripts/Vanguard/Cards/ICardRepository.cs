using System.Collections.Generic;

namespace VanguardThaiSim.Cards
{
    public interface ICardRepository
    {
        int CountCards();
        int CountSeries();
        int CountClans();
        CardDetail GetCard(string cardId);
        IReadOnlyList<CardSummary> QueryCards(CardQueryOptions options);
        IReadOnlyList<SeriesOption> ListSeries();
        IReadOnlyList<ClanOption> ListClans();
    }

    public interface INationCardRepository
    {
        IReadOnlyList<NationOption> ListNations();
    }

    public readonly struct SeriesOption
    {
        public readonly string SeriesCode;
        public readonly string Series;
        public readonly int CardCount;

        public SeriesOption(string seriesCode, string series, int cardCount)
        {
            SeriesCode = seriesCode;
            Series = series;
            CardCount = cardCount;
        }
    }

    public readonly struct ClanOption
    {
        public readonly string Clan;
        public readonly int CardCount;

        public ClanOption(string clan, int cardCount)
        {
            Clan = clan;
            CardCount = cardCount;
        }
    }

    public readonly struct NationOption
    {
        public readonly string Nation;
        public readonly int CardCount;

        public NationOption(string nation, int cardCount)
        {
            Nation = nation;
            CardCount = cardCount;
        }
    }
}
