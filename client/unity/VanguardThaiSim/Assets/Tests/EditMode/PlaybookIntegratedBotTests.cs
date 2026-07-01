using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class PlaybookIntegratedBotTests
    {
        [Test]
        public void PriorityCallBiasPrefersPlaybookCard()
        {
            GameState state = CreateState("RIDELINE-VG", includePriorityCall: true);

            BotDecision decision = PlaybookIntegratedBot.DecideNext(
                state,
                0,
                new FakeCardRepository(),
                CreateLibrary());

            Assert.NotNull(decision.Action);
            Assert.AreEqual(GameActionType.MoveCard, decision.Action.action_type);
            Assert.AreEqual(GameZone.Hand, decision.Action.from_zone);
            Assert.AreEqual(GameZone.RearGuard, decision.Action.to_zone);
            Assert.AreEqual("call-a-inst", decision.Action.card_instance_id);
            Assert.IsTrue(decision.Reason.Contains("playbook=youth_line"));
        }

        [Test]
        public void NoMatchUsesDefaultBalancedWithoutPriorityBias()
        {
            GameState state = CreateState("UNKNOWN-VG", includePriorityCall: true);

            IReadOnlyList<PlaybookActionEvaluation> evaluations =
                PlaybookIntegratedBot.EvaluateActions(
                    state,
                    0,
                    new FakeCardRepository(),
                    CreateLibrary());
            PlaybookActionEvaluation priorityCall = FindMoveToRearGuard(evaluations, "call-a-inst");

            Assert.AreEqual("default_balanced", priorityCall.PlaybookId);
            Assert.AreEqual(0d, priorityCall.PlaybookBias, 0.0000001d);
        }

        [Test]
        public void EvaluationDoesNotMutateState()
        {
            GameState state = CreateState("RIDELINE-VG", includePriorityCall: true);
            string before = state.ToJson();

            PlaybookIntegratedBot.EvaluateActions(
                state,
                0,
                new FakeCardRepository(),
                CreateLibrary());
            string after = state.ToJson();

            Assert.AreEqual(before, after);
        }

        [Test]
        public void ReasonDoesNotLeakOpponentSecrets()
        {
            GameState state = CreateState("RIDELINE-VG", includePriorityCall: true);
            state.GetPlayer(1).hand.Add(new GameCardInstance("opp-secret", "OPP-HAND-SECRET", 1));

            BotDecision decision = PlaybookIntegratedBot.DecideNext(
                state,
                0,
                new FakeCardRepository(),
                CreateLibrary());

            Assert.IsFalse(decision.Reason.Contains("OPP-HAND-SECRET"));
            Assert.IsFalse(decision.Reason.Contains("CALL-A"));
        }

        [Test]
        public void PlaybookEvaluationIsDeterministic()
        {
            GameState state = CreateState("RIDELINE-VG", includePriorityCall: true);
            BotPlaybookLibrary library = CreateLibrary();

            IReadOnlyList<PlaybookActionEvaluation> first =
                PlaybookIntegratedBot.EvaluateActions(state, 0, new FakeCardRepository(), library);
            IReadOnlyList<PlaybookActionEvaluation> second =
                PlaybookIntegratedBot.EvaluateActions(state, 0, new FakeCardRepository(), library);

            Assert.AreEqual(first.Count, second.Count);
            for (int i = 0; i < first.Count; i++)
            {
                Assert.AreEqual(first[i].TotalScore, second[i].TotalScore, 0.0000001d);
                Assert.AreEqual(first[i].Summary, second[i].Summary);
            }
        }

        private static PlaybookActionEvaluation FindMoveToRearGuard(
            IReadOnlyList<PlaybookActionEvaluation> evaluations,
            string cardInstanceId)
        {
            for (int i = 0; i < evaluations.Count; i++)
            {
                LegalGameAction action = evaluations[i].Action;
                if (action != null &&
                    action.action_type == GameActionType.MoveCard &&
                    action.from_zone == GameZone.Hand &&
                    action.to_zone == GameZone.RearGuard &&
                    action.card_instance_id == cardInstanceId)
                {
                    return evaluations[i];
                }
            }

            Assert.Fail("Missing rear-guard call for " + cardInstanceId);
            return null;
        }

        private static GameState CreateState(string vanguardCardId, bool includePriorityCall)
        {
            var player = new PlayerGameState
            {
                player_id = "p1",
                vanguard = new List<GameCardInstance>
                {
                    new GameCardInstance("vg", vanguardCardId, 0)
                },
                hand = new List<GameCardInstance>
                {
                    new GameCardInstance("call-z-inst", "CALL-Z", 0)
                }
            };

            if (includePriorityCall)
            {
                player.hand.Insert(0, new GameCardInstance("call-a-inst", "CALL-A", 0));
            }

            return new GameState
            {
                phase = GamePhase.Main,
                players = new List<PlayerGameState>
                {
                    player,
                    new PlayerGameState
                    {
                        player_id = "p2"
                    }
                }
            };
        }

        private static BotPlaybookLibrary CreateLibrary()
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
                        rideline_card_ids = new List<string>
                        {
                            "RIDELINE-VG"
                        },
                        priority_call_card_ids = new List<string>
                        {
                            "CALL-A"
                        }
                    }
                }
            };
        }

        private sealed class FakeCardRepository : ICardRepository
        {
            public int CountCards()
            {
                return 0;
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
                return null;
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
