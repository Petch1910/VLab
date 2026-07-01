using System.Collections.Generic;

namespace VanguardThaiSim.Cards
{
    public sealed class CardDetail
    {
        public string CardId { get; set; }
        public string SourceId { get; set; }
        public string SourceKey { get; set; }
        public string NameTh { get; set; }
        public string TextTh { get; set; }
        public string Series { get; set; }
        public string SeriesCode { get; set; }
        public string Clan { get; set; }
        public string Nation { get; set; }
        public string Nation2 { get; set; }
        public int? Grade { get; set; }
        public int? Power { get; set; }
        public int? Shield { get; set; }
        public string Trigger { get; set; }
        public int DeckLimit { get; set; }
        public string Type1 { get; set; }
        public string Type2 { get; set; }
        public string Race1 { get; set; }
        public string Race2 { get; set; }
        public string Warning { get; set; }
        public string ImageUrl { get; set; }
        public string ImageRelativePath { get; set; }
        public bool ImageExists { get; set; }
        public List<CardRawDetail> RawDetails { get; } = new List<CardRawDetail>();
        public List<CardFormat> Formats { get; } = new List<CardFormat>();
    }

    public readonly struct CardRawDetail
    {
        public readonly string Label;
        public readonly string Value;

        public CardRawDetail(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }

    public readonly struct CardFormat
    {
        public readonly string FormatKey;
        public readonly string FormatValue;

        public CardFormat(string formatKey, string formatValue)
        {
            FormatKey = formatKey;
            FormatValue = formatValue;
        }
    }
}

