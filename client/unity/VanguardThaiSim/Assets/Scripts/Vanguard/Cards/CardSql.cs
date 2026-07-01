namespace VanguardThaiSim.Cards
{
    public static class CardSql
    {
        public const string CountCards = "SELECT COUNT(*) FROM cards";
        public const string CountSeries = "SELECT COUNT(*) FROM series";
        public const string CountClans = "SELECT COUNT(*) FROM clans";

        public const string CardSummaryBase = @"
SELECT
  c.card_id,
  c.name_th,
  c.series,
  c.series_code,
  c.clan,
  c.nation,
  c.grade,
  c.type_1,
  c.trigger,
  i.image_relative_path,
  i.image_exists
FROM cards c
JOIN card_images i ON i.card_id = c.card_id";

        public const string CardById = @"
SELECT
  c.card_id,
  c.source_id,
  c.source_key,
  c.name_th,
  c.text_th,
  c.series,
  c.series_code,
  c.clan,
  c.nation,
  c.nation_2,
  c.grade,
  c.power,
  c.shield,
  c.trigger,
  c.deck_limit,
  c.type_1,
  c.type_2,
  c.race_1,
  c.race_2,
  c.warning,
  i.image_url,
  i.image_relative_path,
  i.image_exists
FROM cards c
JOIN card_images i ON i.card_id = c.card_id
WHERE c.card_id = @cardId";

        public const string DetailsByCardId = @"
SELECT label, value
FROM card_details
WHERE card_id = @cardId
ORDER BY sort_order";

        public const string FormatsByCardId = @"
SELECT format_key, format_value
FROM card_formats
WHERE card_id = @cardId
ORDER BY format_key";

        public const string SeriesOptions = @"
SELECT series_code, series, card_count
FROM series
ORDER BY series_code";

        public const string ClanOptions = @"
SELECT clan, card_count
FROM clans
ORDER BY card_count DESC, clan";

        public const string NationOptions = @"
SELECT TRIM(nation) AS nation, COUNT(*) AS card_count
FROM cards
WHERE nation IS NOT NULL AND TRIM(nation) <> ''
GROUP BY TRIM(nation)
ORDER BY card_count DESC, nation";
    }
}
