using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    [TestFixture]
    public class M21SoulAndBattleActionsTests
    {
        private GameState CreateInitialState()
        {
            GameState state = new GameState
            {
                game_id = "test-game",
                format = "D",
                random_seed = 42,
                turn_number = 1,
                turn_player_index = 0,
                phase = GamePhase.Battle
            };

            PlayerGameState player1 = new PlayerGameState { player_id = "player-1" };
            PlayerGameState player2 = new PlayerGameState { player_id = "player-2" };

            // Add standard cards to zones
            player1.vanguard.Add(new GameCardInstance("C001", "p1-vg", 0));
            player1.rear_guard.Add(new GameCardInstance("C002", "p1-rg", 0));
            player1.hand.Add(new GameCardInstance("C003", "p1-hand1", 0));
            player1.hand.Add(new GameCardInstance("C004", "p1-hand2", 0));
            player1.deck.Add(new GameCardInstance("C005", "p1-deck1", 0));

            player2.vanguard.Add(new GameCardInstance("C006", "p2-vg", 1));
            player2.rear_guard.Add(new GameCardInstance("C007", "p2-rg", 1));

            state.players.Add(player1);
            state.players.Add(player2);
            state.EnsureLists();

            return state;
        }

        [Test]
        public void TestDeclareAttack()
        {
            GameState state = CreateInitialState();
            string attackerId = state.players[0].vanguard[0].instance_id;
            string targetId = state.players[1].vanguard[0].instance_id;

            // Generate actions and verify DeclareAttack is generated
            var actions = LegalActionGenerator.Generate(state, 0);
            bool hasAttack = false;
            foreach (var act in actions)
            {
                if (act.action_type == GameActionType.DeclareAttack &&
                    act.card_instance_id == attackerId &&
                    act.target_card_instance_id == targetId)
                {
                    hasAttack = true;
                }
            }
            Assert.IsTrue(hasAttack, "DeclareAttack action should be generated in Battle phase");

            // Execute DeclareAttack
            GameEvent ev = GameActionService.DeclareAttack(state, 0, attackerId, targetId);
            Assert.AreEqual(attackerId, state.attacker_card_instance_id);
            Assert.AreEqual(targetId, state.target_card_instance_id);

            // Undo DeclareAttack
            GameActionService.UndoLast(state);
            Assert.IsNull(state.attacker_card_instance_id);
            Assert.IsNull(state.target_card_instance_id);
        }

        [Test]
        public void TestGuard()
        {
            GameState state = CreateInitialState();
            string guardCardId = state.players[0].hand[0].instance_id;

            // Generate actions and verify Guard is generated
            var actions = LegalActionGenerator.Generate(state, 0);
            bool hasGuard = false;
            foreach (var act in actions)
            {
                if (act.action_type == GameActionType.Guard && act.card_instance_id == guardCardId)
                {
                    hasGuard = true;
                }
            }
            Assert.IsTrue(hasGuard, "Guard action should be generated for hand cards in Battle phase");

            // Execute Guard
            GameEvent ev = GameActionService.Guard(state, 0, guardCardId);
            Assert.AreEqual(1, state.players[0].hand.Count);
            Assert.AreEqual(1, state.players[0].guardian.Count);
            Assert.Contains(guardCardId, state.guardian_card_instance_ids);

            // Undo Guard
            GameActionService.UndoLast(state);
            Assert.AreEqual(2, state.players[0].hand.Count);
            Assert.AreEqual(0, state.players[0].guardian.Count);
            Assert.IsFalse(state.guardian_card_instance_ids.Contains(guardCardId));
        }

        [Test]
        public void TestTriggerCheck()
        {
            GameState state = CreateInitialState();
            string deckCardId = state.players[0].deck[0].instance_id;

            // Execute TriggerCheck
            GameEvent ev = GameActionService.TriggerCheck(state, 0, GameZone.Deck, GameZone.Trigger);
            Assert.AreEqual(0, state.players[0].deck.Count);
            Assert.AreEqual(1, state.players[0].trigger.Count);
            Assert.AreEqual(deckCardId, state.players[0].trigger[0].instance_id);

            // Undo TriggerCheck
            GameActionService.UndoLast(state);
            Assert.AreEqual(1, state.players[0].deck.Count);
            Assert.AreEqual(0, state.players[0].trigger.Count);
            Assert.AreEqual(deckCardId, state.players[0].deck[0].instance_id);
        }

        [Test]
        public void TriggerCheckLegalActionsCarryDriveAndDamageSources()
        {
            GameState state = CreateInitialState();

            var actions = LegalActionGenerator.Generate(state, 0);

            Assert.IsTrue(HasTriggerCheckSource(actions, TriggerCheckSource.Drive));
            Assert.IsTrue(HasTriggerCheckSource(actions, TriggerCheckSource.Damage));
        }

        [Test]
        public void RulesCoreTriggerCheckPreservesSourceOnEvent()
        {
            GameState state = CreateInitialState();
            LegalGameAction damageCheck = null;
            foreach (LegalGameAction action in RulesCore.GetLegalActions(state, 0))
            {
                if (action.action_type == GameActionType.TriggerCheck &&
                    action.trigger_check_source == TriggerCheckSource.Damage)
                {
                    damageCheck = action;
                    break;
                }
            }

            RulesCommandResult result = RulesCore.TryExecute(state, damageCheck);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(TriggerCheckSource.Damage, result.game_event.trigger_check_source);
            Assert.AreEqual(TriggerCheckSource.Damage, state.event_log[0].trigger_check_source);
        }

        [Test]
        public void TestMulliganCards()
        {
            GameState state = CreateInitialState();
            state.phase = GamePhase.Mulligan;

            List<string> returnIds = new List<string> { state.players[0].hand[0].instance_id };

            // Execute MulliganCards
            GameEvent ev = GameActionService.MulliganCards(state, 0, returnIds);
            Assert.AreEqual(2, state.players[0].hand.Count);
            Assert.AreEqual(1, state.players[0].deck.Count);

            // Undo MulliganCards
            GameActionService.UndoLast(state);
            Assert.AreEqual(2, state.players[0].hand.Count);
            Assert.AreEqual(1, state.players[0].deck.Count);
            Assert.AreEqual("p1-hand1", state.players[0].hand[0].card_id);
        }

        [Test]
        public void TestHardeningVerifierIncludesNewZones()
        {
            GameState state = CreateInitialState();
            // Populate Soul, GZone, and Guardian
            state.players[0].soul.Add(new GameCardInstance("C999", "soul1", 0));
            state.players[0].g_zone.Add(new GameCardInstance("CG00", "gzone1", 0));
            state.players[0].guardian.Add(new GameCardInstance("CGD0", "guard1", 0));

            // Run verification - should succeed with no information leakage
            var res = HiddenStateViewHardeningVerifier.Verify(state);
            Assert.IsTrue(res.accepted, "Verifier should succeed for states containing Soul, GZone, and Guardian cards");
        }

        private static bool HasTriggerCheckSource(IEnumerable<LegalGameAction> actions, TriggerCheckSource source)
        {
            foreach (LegalGameAction action in actions)
            {
                if (action.action_type == GameActionType.TriggerCheck &&
                    action.trigger_check_source == source)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
