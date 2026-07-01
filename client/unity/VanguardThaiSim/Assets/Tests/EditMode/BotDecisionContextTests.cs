using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Bots;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class BotDecisionContextTests
    {
        [Test]
        public void ContextMasksPrivateZonesWithoutMutatingSource()
        {
            GameState state = CreateDecisionState();
            string before = state.ToJson(false);

            BotDecisionContext context = BotDecisionContextFactory.Create(state, 0);
            string after = state.ToJson(false);

            Assert.AreEqual(before, after);
            Assert.AreEqual("player-masked-view", context.Source);
            Assert.AreEqual("OWN-HAND", context.MaskedState.GetPlayer(0).hand[0].card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, context.MaskedState.GetPlayer(0).deck[0].card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, context.MaskedState.GetPlayer(1).hand[0].card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, context.MaskedState.GetPlayer(1).deck[0].card_id);
        }

        [Test]
        public void MaskedLegalActionCanExecuteOnTrueState()
        {
            GameState state = CreateDecisionState();
            BotDecisionContext context = BotDecisionContextFactory.Create(state, 0);

            LegalGameAction call = FindAction(
                context.LegalActions,
                GameActionType.MoveCard,
                GameZone.Hand,
                GameZone.RearGuard);

            Assert.NotNull(call);
            Assert.AreEqual("own-hand", call.card_instance_id);
            Assert.IsTrue(RulesCore.CanExecute(state, call));
        }

        [Test]
        public void EasyBotDecisionUsesMaskedLegalActionBoundary()
        {
            GameState state = CreateDecisionState();
            string before = state.ToJson(false);

            BotDecision decision = EasyBotController.DecideNext(state, 0);
            string after = state.ToJson(false);

            Assert.AreEqual(before, after);
            Assert.NotNull(decision);
            Assert.NotNull(decision.Action);
            Assert.IsTrue(RulesCore.CanExecute(state, decision.Action));
            Assert.IsFalse(decision.Reason.Contains("OPP-HAND-SECRET"));
            Assert.IsFalse(decision.Reason.Contains("OWN-TOP-SECRET"));
        }

        [Test]
        public void HeuristicBotDecisionUsesContextWithoutSecretReasonLeak()
        {
            GameState state = CreateDecisionState();
            BotDecisionContext context = BotDecisionContextFactory.Create(state, 0);

            BotDecision decision = HeuristicBotV2.DecideNext(context, new FakeCardRepository());

            Assert.NotNull(decision);
            Assert.NotNull(decision.Action);
            Assert.IsTrue(RulesCore.CanExecute(state, decision.Action));
            Assert.IsFalse(decision.Reason.Contains("OPP-HAND-SECRET"));
            Assert.IsFalse(decision.Reason.Contains("OPP-DECK-SECRET"));
            Assert.IsFalse(decision.Reason.Contains("OWN-TOP-SECRET"));
        }

        private static GameState CreateDecisionState()
        {
            return new GameState
            {
                phase = GamePhase.Main,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("own-hand", "OWN-HAND", 0)
                        },
                        deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("own-top", "OWN-TOP-SECRET", 0, false)
                        }
                    },
                    new PlayerGameState
                    {
                        player_id = "p2",
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("opp-hand", "OPP-HAND-SECRET", 1)
                        },
                        deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("opp-top", "OPP-DECK-SECRET", 1, false)
                        },
                        vanguard = new List<GameCardInstance>
                        {
                            new GameCardInstance("opp-vg", "OPP-VG-PUBLIC", 1)
                        }
                    }
                }
            };
        }

        private static LegalGameAction FindAction(
            IReadOnlyList<LegalGameAction> actions,
            GameActionType actionType,
            GameZone from,
            GameZone to)
        {
            foreach (LegalGameAction action in actions)
            {
                if (action.action_type == actionType &&
                    action.from_zone == from &&
                    action.to_zone == to)
                {
                    return action;
                }
            }

            return null;
        }

        private sealed class FakeCardRepository : ICardRepository
        {
            public int CountCards() { return 0; }
            public int CountSeries() { return 0; }
            public int CountClans() { return 0; }
            public CardDetail GetCard(string cardId) { return null; }
            public IReadOnlyList<CardSummary> QueryCards(CardQueryOptions options) { return new List<CardSummary>(); }
            public IReadOnlyList<SeriesOption> ListSeries() { return new List<SeriesOption>(); }
            public IReadOnlyList<ClanOption> ListClans() { return new List<ClanOption>(); }
        }
    }
}
