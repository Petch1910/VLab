using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class ComboDiscoveryRunnerTests
    {
        [Test]
        public void RunnerBuildsReportFromPlanningHelpers()
        {
            GameState playerView = GameStateViewFactory.CreatePlayerView(CreateState(), 0);
            ComboDiscoveryReport report = ComboDiscoveryRunner.Analyze(
                playerView,
                0,
                1,
                CreateRepository(),
                CreatePlaybookLibrary(),
                50,
                16,
                2);

            Assert.AreEqual("youth_line", report.playbook_id);
            Assert.AreEqual(BotProfileType.Aggro, report.preferred_profile);
            Assert.Greater(report.board_score, 0d);
            Assert.AreEqual(20000, report.opponent_expected_shield);
            Assert.AreEqual(30000, report.opponent_maximum_shield);
            Assert.AreEqual(664d / 1225d, report.probability_at_least_one_trigger, 0.0000001d);
            Assert.Greater(report.battle_candidate_ids.Count, 0);
            Assert.AreEqual("high_power_first", report.battle_candidate_ids[0]);
            Assert.Greater(report.combo_lines.Count, 0);
            Assert.AreEqual(1, report.combo_lines[0].rank);
            Assert.IsTrue(report.combo_lines[0].replay_reference.Contains("combo-test"));
            Assert.Greater(report.combo_lines[0].total_score, 0d);
        }

        [Test]
        public void ReportJsonRoundTrips()
        {
            GameState playerView = GameStateViewFactory.CreatePlayerView(CreateState(), 0);
            ComboDiscoveryReport report = ComboDiscoveryRunner.Analyze(
                playerView,
                0,
                1,
                CreateRepository(),
                CreatePlaybookLibrary(),
                50,
                16,
                2);

            ComboDiscoveryReport roundTrip = ComboDiscoveryReport.FromJson(report.ToJson());

            Assert.AreEqual(report.report_id, roundTrip.report_id);
            Assert.AreEqual(report.playbook_id, roundTrip.playbook_id);
            Assert.AreEqual(report.battle_candidate_ids.Count, roundTrip.battle_candidate_ids.Count);
            Assert.AreEqual(report.battle_candidate_ids[0], roundTrip.battle_candidate_ids[0]);
            Assert.AreEqual(report.combo_lines.Count, roundTrip.combo_lines.Count);
            Assert.AreEqual(report.combo_lines[0].line_id, roundTrip.combo_lines[0].line_id);
            Assert.AreEqual(report.combo_lines[0].replay_reference, roundTrip.combo_lines[0].replay_reference);
        }

        [Test]
        public void RunnerIsDeterministicAndDoesNotMutateState()
        {
            GameState playerView = GameStateViewFactory.CreatePlayerView(CreateState(), 0);
            string before = playerView.ToJson();

            ComboDiscoveryReport first = ComboDiscoveryRunner.Analyze(
                playerView,
                0,
                1,
                CreateRepository(),
                CreatePlaybookLibrary(),
                50,
                16,
                2);
            ComboDiscoveryReport second = ComboDiscoveryRunner.Analyze(
                playerView,
                0,
                1,
                CreateRepository(),
                CreatePlaybookLibrary(),
                50,
                16,
                2);
            string after = playerView.ToJson();

            Assert.AreEqual(before, after);
            Assert.AreEqual(first.report_id, second.report_id);
            Assert.AreEqual(first.board_score, second.board_score, 0.0000001d);
            Assert.AreEqual(first.battle_candidate_ids[0], second.battle_candidate_ids[0]);
            Assert.AreEqual(first.combo_lines.Count, second.combo_lines.Count);
            Assert.AreEqual(first.combo_lines[0].total_score, second.combo_lines[0].total_score, 0.0000001d);
            Assert.AreEqual(first.combo_lines[0].explanation, second.combo_lines[0].explanation);
        }

        private static GameState CreateState()
        {
            return new GameState
            {
                game_id = "combo-test",
                turn_number = 3,
                phase = GamePhase.Battle,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        vanguard = new List<GameCardInstance>
                        {
                            new GameCardInstance("vg", "RIDELINE-VG", 0)
                        },
                        rear_guard = new List<GameCardInstance>
                        {
                            new GameCardInstance("rg-high", "RG-HIGH", 0),
                            new GameCardInstance("rg-low", "RG-LOW", 0)
                        }
                    },
                    new PlayerGameState
                    {
                        player_id = "p2",
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("opp-h1", "SHIELD-15000", 1),
                            new GameCardInstance("opp-h2", "SHIELD-5000", 1)
                        }
                    }
                }
            };
        }

        private static BotPlaybookLibrary CreatePlaybookLibrary()
        {
            return new BotPlaybookLibrary
            {
                playbooks = new List<BotPlaybook>
                {
                    new BotPlaybook
                    {
                        playbook_id = "youth_line",
                        display_name = "Youth Line",
                        preferred_profile = BotProfileType.Aggro,
                        rideline_card_ids = new List<string> { "RIDELINE-VG" }
                    }
                }
            };
        }

        private static FakeCardRepository CreateRepository()
        {
            var repository = new FakeCardRepository();
            repository.Add("RIDELINE-VG", 13000, null);
            repository.Add("RG-HIGH", 18000, null);
            repository.Add("RG-LOW", 8000, null);
            repository.Add("SHIELD-15000", null, 15000);
            repository.Add("SHIELD-5000", null, 5000);
            return repository;
        }

        private sealed class FakeCardRepository : ICardRepository
        {
            private readonly Dictionary<string, CardDetail> cards = new Dictionary<string, CardDetail>();

            public void Add(string cardId, int? power, int? shield)
            {
                cards[cardId] = new CardDetail
                {
                    CardId = cardId,
                    Power = power,
                    Shield = shield
                };
            }

            public int CountCards()
            {
                return cards.Count;
            }

            public int CountSeries()
            {
                return 0;
            }

            public int CountClans()
            {
                return 0;
            }

            public CardDetail GetCard(string cardId)
            {
                CardDetail detail;
                return cards.TryGetValue(cardId, out detail) ? detail : null;
            }

            public IReadOnlyList<CardSummary> QueryCards(CardQueryOptions options)
            {
                return new List<CardSummary>();
            }

            public IReadOnlyList<SeriesOption> ListSeries()
            {
                return new List<SeriesOption>();
            }

            public IReadOnlyList<ClanOption> ListClans()
            {
                return new List<ClanOption>();
            }
        }
    }
}
