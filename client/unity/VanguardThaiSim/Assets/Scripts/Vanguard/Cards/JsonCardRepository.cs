using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VanguardThaiSim.Cards
{
    public sealed class JsonCardRepository : ICardRepository, INationCardRepository, IDisposable
    {
        private readonly List<JsonCardRecord> cards;
        private readonly Dictionary<string, JsonCardRecord> cardsById;

        public JsonCardRepository(string catalogPath)
        {
            if (string.IsNullOrEmpty(catalogPath))
            {
                throw new ArgumentException("Catalog path is required.", "catalogPath");
            }

            if (!File.Exists(catalogPath))
            {
                throw new FileNotFoundException("Runtime card catalog was not found.", catalogPath);
            }

            string json = File.ReadAllText(catalogPath);
            JsonCardCatalog catalog = JsonUtility.FromJson<JsonCardCatalog>(json);
            if (catalog == null || catalog.cards == null)
            {
                throw new InvalidDataException("Runtime card catalog could not be parsed.");
            }

            if (catalog.schema_version != 1)
            {
                throw new NotSupportedException("Unsupported card catalog schema version: " + catalog.schema_version);
            }

            cards = new List<JsonCardRecord>(catalog.cards);
            cards.Sort(CompareByCardId);
            cardsById = new Dictionary<string, JsonCardRecord>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < cards.Count; i++)
            {
                JsonCardRecord record = cards[i];
                if (record == null || string.IsNullOrWhiteSpace(record.card_id))
                {
                    continue;
                }

                cardsById[record.card_id] = record;
            }
        }

        public void Dispose()
        {
        }

        public int CountCards()
        {
            return cards.Count;
        }

        public int CountSeries()
        {
            return ListSeries().Count;
        }

        public int CountClans()
        {
            return ListClans().Count;
        }

        public CardDetail GetCard(string cardId)
        {
            if (string.IsNullOrWhiteSpace(cardId))
            {
                return null;
            }

            JsonCardRecord record;
            if (!cardsById.TryGetValue(cardId, out record))
            {
                return null;
            }

            CardDetail detail = new CardDetail
            {
                CardId = NullIfEmpty(record.card_id),
                SourceId = NullIfEmpty(record.source_id),
                SourceKey = NullIfEmpty(record.source_key),
                NameTh = NullIfEmpty(record.name_th),
                TextTh = NullIfEmpty(record.text_th),
                Series = NullIfEmpty(record.series),
                SeriesCode = NullIfEmpty(record.series_code),
                Clan = NullIfEmpty(record.clan),
                Nation = NullIfEmpty(record.nation),
                Nation2 = NullIfEmpty(record.nation_2),
                Grade = NullableInt(record.grade_has_value, record.grade),
                Power = NullableInt(record.power_has_value, record.power),
                Shield = NullableInt(record.shield_has_value, record.shield),
                Trigger = NullIfEmpty(record.trigger),
                DeckLimit = record.deck_limit,
                Type1 = NullIfEmpty(record.type_1),
                Type2 = NullIfEmpty(record.type_2),
                Race1 = NullIfEmpty(record.race_1),
                Race2 = NullIfEmpty(record.race_2),
                Warning = NullIfEmpty(record.warning),
                ImageUrl = NullIfEmpty(record.image_url),
                ImageRelativePath = NullIfEmpty(record.image_relative_path),
                ImageExists = record.image_exists
            };

            if (record.formats != null)
            {
                for (int i = 0; i < record.formats.Length; i++)
                {
                    JsonCardFormatRecord format = record.formats[i];
                    if (format == null)
                    {
                        continue;
                    }

                    detail.Formats.Add(new CardFormat(
                        NullIfEmpty(format.format_key),
                        NullIfEmpty(format.format_value)));
                }
            }

            return detail;
        }

        public IReadOnlyList<CardSummary> QueryCards(CardQueryOptions options)
        {
            CardQueryOptions safeOptions = options ?? new CardQueryOptions();
            int limit = Math.Max(1, safeOptions.Limit);
            int offset = Math.Max(0, safeOptions.Offset);
            List<CardSummary> result = new List<CardSummary>();
            int matched = 0;

            for (int i = 0; i < cards.Count; i++)
            {
                JsonCardRecord record = cards[i];
                if (!Matches(record, safeOptions))
                {
                    continue;
                }

                if (matched++ < offset)
                {
                    continue;
                }

                result.Add(ToSummary(record));
                if (result.Count >= limit)
                {
                    break;
                }
            }

            return result;
        }

        public IReadOnlyList<SeriesOption> ListSeries()
        {
            Dictionary<string, SeriesCount> counts = new Dictionary<string, SeriesCount>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < cards.Count; i++)
            {
                JsonCardRecord record = cards[i];
                string series = NullIfEmpty(record.series);
                if (series == null)
                {
                    continue;
                }

                SeriesCount current;
                if (!counts.TryGetValue(series, out current))
                {
                    current = new SeriesCount(NullIfEmpty(record.series_code), series, 0);
                }

                current.Count++;
                counts[series] = current;
            }

            List<SeriesCount> ordered = new List<SeriesCount>(counts.Values);
            ordered.Sort(CompareSeriesCount);
            List<SeriesOption> result = new List<SeriesOption>();
            for (int i = 0; i < ordered.Count; i++)
            {
                SeriesCount item = ordered[i];
                result.Add(new SeriesOption(item.SeriesCode, item.Series, item.Count));
            }

            return result;
        }

        public IReadOnlyList<ClanOption> ListClans()
        {
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < cards.Count; i++)
            {
                string clan = NullIfEmpty(cards[i].clan);
                if (clan == null)
                {
                    continue;
                }

                int current;
                counts.TryGetValue(clan, out current);
                counts[clan] = current + 1;
            }

            List<ClanOption> result = new List<ClanOption>();
            foreach (KeyValuePair<string, int> pair in counts)
            {
                result.Add(new ClanOption(pair.Key, pair.Value));
            }

            result.Sort(CompareClanOptions);
            return result;
        }

        public IReadOnlyList<NationOption> ListNations()
        {
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < cards.Count; i++)
            {
                string nation = NullIfEmpty(cards[i].nation);
                if (nation == null)
                {
                    continue;
                }

                nation = nation.Trim();
                if (nation.Length == 0)
                {
                    continue;
                }

                int current;
                counts.TryGetValue(nation, out current);
                counts[nation] = current + 1;
            }

            List<NationOption> result = new List<NationOption>();
            foreach (KeyValuePair<string, int> pair in counts)
            {
                result.Add(new NationOption(pair.Key, pair.Value));
            }

            result.Sort(CompareNationOptions);
            return result;
        }

        private static CardSummary ToSummary(JsonCardRecord record)
        {
            return new CardSummary(
                NullIfEmpty(record.card_id),
                NullIfEmpty(record.name_th),
                NullIfEmpty(record.series),
                NullIfEmpty(record.series_code),
                NullIfEmpty(record.clan),
                NullIfEmpty(record.nation),
                NullableInt(record.grade_has_value, record.grade),
                NullIfEmpty(record.type_1),
                NullIfEmpty(record.trigger),
                NullIfEmpty(record.image_relative_path),
                record.image_exists);
        }

        private static bool Matches(JsonCardRecord record, CardQueryOptions options)
        {
            if (record == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(options.SearchText) && !MatchesSearch(record, options.SearchText.Trim()))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(options.Series) &&
                !string.Equals(record.series, options.Series, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(options.Clan) &&
                !string.Equals(record.clan, options.Clan, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(options.Nation) &&
                !string.Equals((record.nation ?? string.Empty).Trim(), options.Nation.Trim(), StringComparison.Ordinal))
            {
                return false;
            }

            if (options.Grade.HasValue &&
                (!record.grade_has_value || record.grade != options.Grade.Value))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(options.Type1) &&
                !string.Equals(record.type_1, options.Type1, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        private static bool MatchesSearch(JsonCardRecord record, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return true;
            }

            return Contains(record.card_id, searchText) ||
                   Contains(record.source_key, searchText) ||
                   Contains(record.name_th, searchText) ||
                   Contains(record.text_th, searchText) ||
                   Contains(record.series, searchText) ||
                   Contains(record.series_code, searchText) ||
                   Contains(record.clan, searchText) ||
                   Contains(record.nation, searchText) ||
                   Contains(record.type_1, searchText) ||
                   Contains(record.trigger, searchText);
        }

        private static bool Contains(string source, string searchText)
        {
            return !string.IsNullOrEmpty(source) &&
                   source.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int CompareByCardId(JsonCardRecord left, JsonCardRecord right)
        {
            return string.Compare(left == null ? null : left.card_id, right == null ? null : right.card_id, StringComparison.OrdinalIgnoreCase);
        }

        private static int CompareSeriesCount(SeriesCount left, SeriesCount right)
        {
            int byCode = string.Compare(left.SeriesCode, right.SeriesCode, StringComparison.OrdinalIgnoreCase);
            if (byCode != 0)
            {
                return byCode;
            }

            return string.Compare(left.Series, right.Series, StringComparison.OrdinalIgnoreCase);
        }

        private static int CompareClanOptions(ClanOption left, ClanOption right)
        {
            int byCount = right.CardCount.CompareTo(left.CardCount);
            if (byCount != 0)
            {
                return byCount;
            }

            return string.Compare(left.Clan, right.Clan, StringComparison.OrdinalIgnoreCase);
        }

        private static int CompareNationOptions(NationOption left, NationOption right)
        {
            int byCount = right.CardCount.CompareTo(left.CardCount);
            if (byCount != 0)
            {
                return byCount;
            }

            return string.Compare(left.Nation, right.Nation, StringComparison.OrdinalIgnoreCase);
        }

        private static int? NullableInt(bool hasValue, int value)
        {
            return hasValue ? (int?)value : null;
        }

        private static string NullIfEmpty(string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }

        private struct SeriesCount
        {
            public readonly string SeriesCode;
            public readonly string Series;
            public int Count;

            public SeriesCount(string seriesCode, string series, int count)
            {
                SeriesCode = seriesCode;
                Series = series;
                Count = count;
            }
        }
    }

    [Serializable]
    internal sealed class JsonCardCatalog
    {
        public int schema_version;
        public JsonCardRecord[] cards;
    }

    [Serializable]
    internal sealed class JsonCardRecord
    {
        public string card_id;
        public string source_id;
        public string source_key;
        public string name_th;
        public string text_th;
        public string series;
        public string series_code;
        public string clan;
        public string nation;
        public string nation_2;
        public bool grade_has_value;
        public int grade;
        public bool power_has_value;
        public int power;
        public bool shield_has_value;
        public int shield;
        public string trigger;
        public int deck_limit;
        public string type_1;
        public string type_2;
        public string race_1;
        public string race_2;
        public string warning;
        public string image_url;
        public string image_relative_path;
        public bool image_exists;
        public JsonCardFormatRecord[] formats;
    }

    [Serializable]
    internal sealed class JsonCardFormatRecord
    {
        public string format_key;
        public string format_value;
    }
}
