using System;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;

namespace VanguardThaiSim.Cards
{
    public sealed class SqliteCardRepository : ICardRepository, INationCardRepository, IDisposable
    {
        private readonly SqliteConnection connection;

        public SqliteCardRepository(string databasePath)
        {
            if (string.IsNullOrEmpty(databasePath))
            {
                throw new ArgumentException("Database path is required.", "databasePath");
            }

            connection = new SqliteConnection("URI=file:" + databasePath);
            connection.Open();
        }

        public void Dispose()
        {
            connection.Dispose();
        }

        public int CountCards()
        {
            return ExecuteScalarInt(CardSql.CountCards);
        }

        public int CountSeries()
        {
            return ExecuteScalarInt(CardSql.CountSeries);
        }

        public int CountClans()
        {
            return ExecuteScalarInt(CardSql.CountClans);
        }

        public CardDetail GetCard(string cardId)
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = CardSql.CardById;
                command.Parameters.Add(new SqliteParameter("@cardId", cardId));

                using (IDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    CardDetail detail = new CardDetail
                    {
                        CardId = ReadString(reader, "card_id"),
                        SourceId = ReadString(reader, "source_id"),
                        SourceKey = ReadString(reader, "source_key"),
                        NameTh = ReadString(reader, "name_th"),
                        TextTh = ReadString(reader, "text_th"),
                        Series = ReadString(reader, "series"),
                        SeriesCode = ReadString(reader, "series_code"),
                        Clan = ReadString(reader, "clan"),
                        Nation = ReadString(reader, "nation"),
                        Nation2 = ReadString(reader, "nation_2"),
                        Grade = ReadNullableInt(reader, "grade"),
                        Power = ReadNullableInt(reader, "power"),
                        Shield = ReadNullableInt(reader, "shield"),
                        Trigger = ReadString(reader, "trigger"),
                        DeckLimit = ReadInt(reader, "deck_limit"),
                        Type1 = ReadString(reader, "type_1"),
                        Type2 = ReadString(reader, "type_2"),
                        Race1 = ReadString(reader, "race_1"),
                        Race2 = ReadString(reader, "race_2"),
                        Warning = ReadString(reader, "warning"),
                        ImageUrl = ReadString(reader, "image_url"),
                        ImageRelativePath = ReadString(reader, "image_relative_path"),
                        ImageExists = ReadBool(reader, "image_exists")
                    };

                    LoadDetails(cardId, detail);
                    LoadFormats(cardId, detail);
                    return detail;
                }
            }
        }

        public IReadOnlyList<CardSummary> QueryCards(CardQueryOptions options)
        {
            CardQueryOptions safeOptions = options ?? new CardQueryOptions();
            List<string> where = new List<string>();
            List<SqliteParameter> parameters = new List<SqliteParameter>();

            if (!string.IsNullOrEmpty(safeOptions.SearchText))
            {
                where.Add("c.card_id IN (SELECT card_id FROM search_terms WHERE text LIKE @searchText)");
                parameters.Add(new SqliteParameter("@searchText", "%" + safeOptions.SearchText + "%"));
            }

            if (!string.IsNullOrEmpty(safeOptions.Series))
            {
                where.Add("c.series = @series");
                parameters.Add(new SqliteParameter("@series", safeOptions.Series));
            }

            if (!string.IsNullOrEmpty(safeOptions.Clan))
            {
                where.Add("c.clan = @clan");
                parameters.Add(new SqliteParameter("@clan", safeOptions.Clan));
            }

            if (!string.IsNullOrEmpty(safeOptions.Nation))
            {
                where.Add("TRIM(c.nation) = @nation");
                parameters.Add(new SqliteParameter("@nation", safeOptions.Nation.Trim()));
            }

            if (safeOptions.Grade.HasValue)
            {
                where.Add("c.grade = @grade");
                parameters.Add(new SqliteParameter("@grade", safeOptions.Grade.Value));
            }

            if (!string.IsNullOrEmpty(safeOptions.Type1))
            {
                where.Add("c.type_1 = @type1");
                parameters.Add(new SqliteParameter("@type1", safeOptions.Type1));
            }

            string sql = CardSql.CardSummaryBase;
            if (where.Count > 0)
            {
                sql += " WHERE " + string.Join(" AND ", where.ToArray());
            }

            sql += " ORDER BY c.card_id LIMIT @limit OFFSET @offset";
            parameters.Add(new SqliteParameter("@limit", Math.Max(1, safeOptions.Limit)));
            parameters.Add(new SqliteParameter("@offset", Math.Max(0, safeOptions.Offset)));

            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                foreach (SqliteParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }

                return ReadCardSummaries(command);
            }
        }

        public IReadOnlyList<SeriesOption> ListSeries()
        {
            List<SeriesOption> result = new List<SeriesOption>();
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = CardSql.SeriesOptions;
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new SeriesOption(
                            ReadString(reader, "series_code"),
                            ReadString(reader, "series"),
                            ReadInt(reader, "card_count")));
                    }
                }
            }

            return result;
        }

        public IReadOnlyList<ClanOption> ListClans()
        {
            List<ClanOption> result = new List<ClanOption>();
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = CardSql.ClanOptions;
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ClanOption(
                            ReadString(reader, "clan"),
                            ReadInt(reader, "card_count")));
                    }
                }
            }

            return result;
        }

        public IReadOnlyList<NationOption> ListNations()
        {
            List<NationOption> result = new List<NationOption>();
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = CardSql.NationOptions;
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new NationOption(
                            ReadString(reader, "nation"),
                            ReadInt(reader, "card_count")));
                    }
                }
            }

            return result;
        }

        private int ExecuteScalarInt(string sql)
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                object value = command.ExecuteScalar();
                return Convert.ToInt32(value);
            }
        }

        private static IReadOnlyList<CardSummary> ReadCardSummaries(SqliteCommand command)
        {
            List<CardSummary> result = new List<CardSummary>();
            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(new CardSummary(
                        ReadString(reader, "card_id"),
                        ReadString(reader, "name_th"),
                        ReadString(reader, "series"),
                        ReadString(reader, "series_code"),
                        ReadString(reader, "clan"),
                        ReadString(reader, "nation"),
                        ReadNullableInt(reader, "grade"),
                        ReadString(reader, "type_1"),
                        ReadString(reader, "trigger"),
                        ReadString(reader, "image_relative_path"),
                        ReadBool(reader, "image_exists")));
                }
            }

            return result;
        }

        private void LoadDetails(string cardId, CardDetail detail)
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = CardSql.DetailsByCardId;
                command.Parameters.Add(new SqliteParameter("@cardId", cardId));
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        detail.RawDetails.Add(new CardRawDetail(
                            ReadString(reader, "label"),
                            ReadString(reader, "value")));
                    }
                }
            }
        }

        private void LoadFormats(string cardId, CardDetail detail)
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = CardSql.FormatsByCardId;
                command.Parameters.Add(new SqliteParameter("@cardId", cardId));
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        detail.Formats.Add(new CardFormat(
                            ReadString(reader, "format_key"),
                            ReadString(reader, "format_value")));
                    }
                }
            }
        }

        private static string ReadString(IDataRecord reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        private static int ReadInt(IDataRecord reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            return Convert.ToInt32(reader.GetValue(ordinal));
        }

        private static int? ReadNullableInt(IDataRecord reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            return reader.IsDBNull(ordinal) ? (int?)null : Convert.ToInt32(reader.GetValue(ordinal));
        }

        private static bool ReadBool(IDataRecord reader, string name)
        {
            int ordinal = reader.GetOrdinal(name);
            return !reader.IsDBNull(ordinal) && Convert.ToInt32(reader.GetValue(ordinal)) != 0;
        }
    }
}
