using System;
using System.Collections.Generic;

namespace VanguardThaiSim.Cards
{
    public enum CardTaxonomyFilterKind
    {
        None,
        Clan,
        Nation
    }

    public readonly struct CardTaxonomyFilterOption
    {
        public readonly CardTaxonomyFilterKind Kind;
        public readonly string Value;
        public readonly string DisplayLabel;
        public readonly string StatusLabel;
        public readonly int CardCount;

        public bool IsClan
        {
            get { return Kind == CardTaxonomyFilterKind.Clan; }
        }

        public bool IsNation
        {
            get { return Kind == CardTaxonomyFilterKind.Nation; }
        }

        public bool IsEmpty
        {
            get { return Kind == CardTaxonomyFilterKind.None || string.IsNullOrEmpty(Value); }
        }

        public static CardTaxonomyFilterOption Empty
        {
            get
            {
                return new CardTaxonomyFilterOption(
                    CardTaxonomyFilterKind.None,
                    null,
                    null,
                    null,
                    0);
            }
        }

        public CardTaxonomyFilterOption(
            CardTaxonomyFilterKind kind,
            string value,
            string displayLabel,
            string statusLabel,
            int cardCount)
        {
            Kind = kind;
            Value = value;
            DisplayLabel = displayLabel;
            StatusLabel = statusLabel;
            CardCount = cardCount;
        }
    }

    public static class VanguardAreaClanTaxonomy
    {
        public const string AllGroupsLabel = "All clans/groups";

        private static readonly Dictionary<string, TaxonomyEntry> ClanEntries =
            new Dictionary<string, TaxonomyEntry>
            {
                { "\u0e23\u0e2d\u0e22\u0e31\u0e25 \u0e1e\u0e32\u0e25\u0e32\u0e14\u0e34\u0e19", new TaxonomyEntry("US", 0, 0) },
                { "\u0e42\u0e2d\u0e23\u0e32\u0e40\u0e04\u0e34\u0e25 \u0e17\u0e34\u0e07\u0e04\u0e4c \u0e41\u0e17\u0e07\u0e04\u0e4c", new TaxonomyEntry("US", 0, 1) },
                { "\u0e41\u0e2d\u0e07\u0e40\u0e08\u0e34\u0e25 \u0e1f\u0e34\u0e17\u0e40\u0e18\u0e2d\u0e23\u0e4c", new TaxonomyEntry("US", 0, 2) },
                { "\u0e0a\u0e32\u0e42\u0e14\u0e27\u0e4c \u0e1e\u0e32\u0e25\u0e32\u0e14\u0e34\u0e19", new TaxonomyEntry("US", 0, 3) },
                { "\u0e42\u0e01\u0e25\u0e14\u0e4c \u0e1e\u0e32\u0e25\u0e32\u0e14\u0e34\u0e19", new TaxonomyEntry("US", 0, 4) },
                { "\u0e40\u0e08\u0e40\u0e19\u0e0b\u0e34\u0e2a", new TaxonomyEntry("US", 0, 5) },

                { "\u0e04\u0e32\u0e40\u0e07\u0e42\u0e23\u0e48", new TaxonomyEntry("DE", 1, 0) },
                { "\u0e19\u0e38\u0e1a\u0e32\u0e17\u0e32\u0e21\u0e30", new TaxonomyEntry("DE", 1, 1) },
                { "\u0e17\u0e32\u0e08\u0e34\u0e04\u0e32\u0e40\u0e2a\u0e30", new TaxonomyEntry("DE", 1, 2) },
                { "\u0e21\u0e38\u0e23\u0e32\u0e04\u0e38\u0e42\u0e21\u0e30", new TaxonomyEntry("DE", 1, 3) },
                { "\u0e19\u0e32\u0e23\u0e38\u0e04\u0e32\u0e21\u0e34", new TaxonomyEntry("DE", 1, 4) },

                { "\u0e42\u0e19\u0e27\u0e48\u0e32 \u0e40\u0e01\u0e23\u0e1b\u0e40\u0e1b\u0e2d\u0e23\u0e4c", new TaxonomyEntry("SG", 2, 0) },
                { "\u0e44\u0e14\u0e40\u0e21\u0e19\u0e0a\u0e31\u0e48\u0e19 \u0e42\u0e1e\u0e25\u0e34\u0e2a", new TaxonomyEntry("SG", 2, 1) },
                { "\u0e25\u0e34\u0e07\u0e04\u0e4c\u0e42\u0e08\u0e4a\u0e01\u0e40\u0e01\u0e2d\u0e23\u0e4c", new TaxonomyEntry("SG", 2, 2) },
                { "\u0e40\u0e2d\u0e17\u0e23\u0e32\u0e19\u0e40\u0e08\u0e2d\u0e23\u0e4c", new TaxonomyEntry("SG", 2, 3) },

                { "\u0e14\u0e32\u0e23\u0e4c\u0e04 \u0e2d\u0e34\u0e25\u0e40\u0e23\u0e01\u0e39\u0e25\u0e32\u0e23\u0e4c\u0e2a", new TaxonomyEntry("DZ", 3, 0) },
                { "\u0e40\u0e1e\u0e25\u0e21\u0e39\u0e19", new TaxonomyEntry("DZ", 3, 1) },
                { "\u0e2a\u0e44\u0e1b\u0e04\u0e4c \u0e1a\u0e23\u0e32\u0e40\u0e18\u0e2d\u0e23\u0e4c\u0e2a", new TaxonomyEntry("DZ", 3, 2) },
                { "\u0e40\u0e01\u0e35\u0e22\u0e23\u0e4c\u0e42\u0e04\u0e23\u0e19\u0e34\u0e40\u0e04\u0e34\u0e25", new TaxonomyEntry("DZ", 3, 3) },

                { "\u0e41\u0e01\u0e23\u0e19\u0e1a\u0e25\u0e39", new TaxonomyEntry("MG", 4, 0) },
                { "\u0e40\u0e1a\u0e2d\u0e23\u0e4c\u0e21\u0e34\u0e27\u0e14\u0e49\u0e32 \u0e44\u0e17\u0e23\u0e41\u0e2d\u0e07\u0e40\u0e01\u0e34\u0e25", new TaxonomyEntry("MG", 4, 1) },
                { "\u0e2d\u0e04\u0e27\u0e2d\u0e1f\u0e2d\u0e23\u0e4c\u0e0b", new TaxonomyEntry("MG", 4, 2) },

                { "\u0e40\u0e21\u0e01\u0e49\u0e32\u0e42\u0e04\u0e42\u0e25\u0e19\u0e35\u0e48", new TaxonomyEntry("Zoo", 5, 0) },
                { "\u0e40\u0e01\u0e23\u0e17 \u0e40\u0e19\u0e40\u0e08\u0e2d\u0e23\u0e4c", new TaxonomyEntry("Zoo", 5, 1) },
                { "\u0e40\u0e19\u0e42\u0e2d \u0e40\u0e19\u0e04\u0e15\u0e49\u0e32", new TaxonomyEntry("Zoo", 5, 2) },

                { "\u0e40\u0e04\u0e23\u0e22\u0e4c\u0e40\u0e2d\u0e40\u0e25\u0e40\u0e21\u0e19\u0e17\u0e31\u0e25", new TaxonomyEntry("SP", 7, 0) },
                { "BanG Dream!", new TaxonomyEntry("SP", 7, 1) },
                { "\u0e44\u0e2d\u0e04\u0e2d\u0e19\u0e34\u0e04", new TaxonomyEntry("SP", 7, 2) },
                { "\u0e40\u0e01\u0e21", new TaxonomyEntry("SP", 7, 3) },
                { "\u0e44\u0e25\u0e1f\u0e4c\u0e41\u0e2d\u0e04\u0e0a\u0e31\u0e48\u0e19", new TaxonomyEntry("SP", 7, 4) },
                { "\u0e41\u0e2d\u0e19\u0e34\u0e40\u0e21\u0e0a\u0e31\u0e48\u0e19", new TaxonomyEntry("SP", 7, 5) },
                { "\u0e40\u0e14\u0e2d\u0e30\u0e41\u0e21\u0e2a\u0e04\u0e4c\u0e04\u0e2d\u0e25\u0e40\u0e25\u0e04\u0e0a\u0e31\u0e48\u0e19", new TaxonomyEntry("SP", 7, 6) },
                { "N/A", new TaxonomyEntry("SP", 7, 99) }
            };

        private static readonly Dictionary<string, TaxonomyEntry> NationEntries =
            new Dictionary<string, TaxonomyEntry>
            {
                { "\u0e14\u0e23\u0e32\u0e01\u0e49\u0e2d\u0e19\u0e40\u0e2d\u0e21\u0e44\u0e1e\u0e23\u0e4c D", new TaxonomyEntry("D", 6, 0) },
                { "\u0e14\u0e32\u0e23\u0e4c\u0e04\u0e2a\u0e40\u0e15\u0e17\u0e2a\u0e4c", new TaxonomyEntry("D", 6, 1) },
                { "\u0e1a\u0e23\u0e31\u0e19\u0e17\u0e4c\u0e40\u0e01\u0e15", new TaxonomyEntry("D", 6, 2) },
                { "\u0e40\u0e04\u0e40\u0e17\u0e2d\u0e23\u0e4c\u0e41\u0e0b\u0e07\u0e04\u0e4c\u0e17\u0e31\u0e27\u0e23\u0e35", new TaxonomyEntry("D", 6, 3) },
                { "\u0e2a\u0e42\u0e15\u0e22\u0e40\u0e04\u0e35\u0e22", new TaxonomyEntry("D", 6, 4) },
                { "\u0e25\u0e34\u0e23\u0e34\u0e04\u0e31\u0e25\u0e42\u0e21\u0e19\u0e32\u0e2a\u0e40\u0e17\u0e23\u0e34\u0e42\u0e2d\u0e49", new TaxonomyEntry("D", 6, 5) }
            };

        public static IReadOnlyList<CardTaxonomyFilterOption> BuildFilterOptions(
            IReadOnlyList<ClanOption> clans,
            IReadOnlyList<NationOption> nations)
        {
            List<SortableOption> sortable = new List<SortableOption>();

            if (clans != null)
            {
                foreach (ClanOption option in clans)
                {
                    string clan = Normalize(option.Clan);
                    if (string.IsNullOrEmpty(clan))
                    {
                        continue;
                    }

                    TaxonomyEntry entry = ResolveClan(clan);
                    sortable.Add(new SortableOption(
                        entry,
                        clan,
                        new CardTaxonomyFilterOption(
                            CardTaxonomyFilterKind.Clan,
                            clan,
                            FormatDisplay(entry, clan, option.CardCount),
                            FormatStatus(entry, clan),
                            Math.Max(0, option.CardCount))));
                }
            }

            if (nations != null)
            {
                foreach (NationOption option in nations)
                {
                    string nation = Normalize(option.Nation);
                    TaxonomyEntry entry;
                    if (!NationEntries.TryGetValue(nation, out entry))
                    {
                        continue;
                    }

                    sortable.Add(new SortableOption(
                        entry,
                        nation,
                        new CardTaxonomyFilterOption(
                            CardTaxonomyFilterKind.Nation,
                            nation,
                            FormatDisplay(entry, nation, option.CardCount),
                            FormatStatus(entry, nation),
                            Math.Max(0, option.CardCount))));
                }
            }

            sortable.Sort(Compare);

            List<CardTaxonomyFilterOption> result = new List<CardTaxonomyFilterOption>();
            foreach (SortableOption option in sortable)
            {
                result.Add(option.Option);
            }

            return result;
        }

        private static TaxonomyEntry ResolveClan(string clan)
        {
            TaxonomyEntry entry;
            if (ClanEntries.TryGetValue(clan, out entry))
            {
                return entry;
            }

            return new TaxonomyEntry("SP", 7, 50);
        }

        private static int Compare(SortableOption left, SortableOption right)
        {
            int group = left.Entry.GroupOrder.CompareTo(right.Entry.GroupOrder);
            if (group != 0)
            {
                return group;
            }

            int within = left.Entry.WithinOrder.CompareTo(right.Entry.WithinOrder);
            if (within != 0)
            {
                return within;
            }

            return string.Compare(left.SortName, right.SortName, StringComparison.Ordinal);
        }

        private static string FormatDisplay(TaxonomyEntry entry, string value, int cardCount)
        {
            return FormatStatus(entry, value) + " (" + Math.Max(0, cardCount) + ")";
        }

        private static string FormatStatus(TaxonomyEntry entry, string value)
        {
            return entry.GroupCode + " - " + value;
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return string.Join(" ", value.Trim().Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private readonly struct TaxonomyEntry
        {
            public readonly string GroupCode;
            public readonly int GroupOrder;
            public readonly int WithinOrder;

            public TaxonomyEntry(string groupCode, int groupOrder, int withinOrder)
            {
                GroupCode = groupCode;
                GroupOrder = groupOrder;
                WithinOrder = withinOrder;
            }
        }

        private readonly struct SortableOption
        {
            public readonly TaxonomyEntry Entry;
            public readonly string SortName;
            public readonly CardTaxonomyFilterOption Option;

            public SortableOption(TaxonomyEntry entry, string sortName, CardTaxonomyFilterOption option)
            {
                Entry = entry;
                SortName = sortName;
                Option = option;
            }
        }
    }
}
