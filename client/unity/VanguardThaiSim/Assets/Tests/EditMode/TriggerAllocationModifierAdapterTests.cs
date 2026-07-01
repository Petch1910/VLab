using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerAllocationModifierAdapterTests
    {
        [Test]
        public void CriticalSplitProducesPowerAndCriticalModifiers()
        {
            TriggerAllocationPlan plan = TriggerAllocationPlanner.Plan(
                CreateAllocationState(includeHiddenRearGuard: false),
                0,
                TriggerResolver.Resolve(TriggerType.Critical));

            TriggerAllocationModifierResult result = TriggerAllocationModifierAdapter.ConvertToLedger(
                "drive-001",
                plan,
                CombatModifierExpiration.EndOfTurn);

            Assert.IsTrue(result.accepted);
            Assert.AreEqual(2, result.ledger.modifiers.Count);
            CombatModifierSummary powerTarget = result.ledger.Summarize("rg-high");
            CombatModifierSummary criticalTarget = result.ledger.Summarize("vg");
            Assert.AreEqual(10000, powerTarget.total_power_delta);
            Assert.AreEqual(0, powerTarget.total_critical_delta);
            Assert.AreEqual(0, criticalTarget.total_power_delta);
            Assert.AreEqual(1, criticalTarget.total_critical_delta);
        }

        [Test]
        public void FrontPlanProducesOnePowerModifierPerVisibleTarget()
        {
            TriggerAllocationPlan plan = TriggerAllocationPlanner.Plan(
                CreateAllocationState(includeHiddenRearGuard: true),
                0,
                TriggerResolver.Resolve(TriggerType.Front));

            TriggerAllocationModifierResult result = TriggerAllocationModifierAdapter.ConvertToLedger(
                "drive-front",
                plan,
                CombatModifierExpiration.EndOfBattle);

            Assert.IsTrue(result.accepted);
            Assert.AreEqual(3, result.ledger.modifiers.Count);
            Assert.AreEqual(10000, result.ledger.Summarize("vg").total_power_delta);
            Assert.AreEqual(10000, result.ledger.Summarize("rg-high").total_power_delta);
            Assert.AreEqual(10000, result.ledger.Summarize("rg-low").total_power_delta);
            Assert.AreEqual(0, result.ledger.Summarize("hidden-rg").modifier_count);
        }

        [Test]
        public void SideEffectOnlyNotesDoNotCreateFakeStatModifiers()
        {
            TriggerAllocationPlan draw = TriggerAllocationPlanner.Plan(
                CreateEmptyBoardState(),
                0,
                TriggerResolver.Resolve(TriggerType.Draw));
            TriggerAllocationPlan over = TriggerAllocationPlanner.Plan(
                CreateEmptyBoardState(),
                0,
                TriggerResolver.Resolve(TriggerType.Over));

            TriggerAllocationModifierResult drawResult = TriggerAllocationModifierAdapter.ConvertToLedger(
                "draw-trigger",
                draw,
                CombatModifierExpiration.EndOfTurn);
            TriggerAllocationModifierResult overResult = TriggerAllocationModifierAdapter.ConvertToLedger(
                "over-trigger",
                over,
                CombatModifierExpiration.EndOfTurn);

            Assert.IsTrue(drawResult.accepted);
            Assert.AreEqual(0, drawResult.ledger.modifiers.Count);
            Assert.Greater(drawResult.notes.Count, 0);
            Assert.IsTrue(overResult.accepted);
            Assert.AreEqual(0, overResult.ledger.modifiers.Count);
            Assert.Greater(overResult.notes.Count, 0);
        }

        [Test]
        public void ManualPlanReturnsManualStatusWithoutModifiers()
        {
            TriggerAllocationPlan manual = TriggerAllocationPlanner.Plan(
                CreateAllocationState(includeHiddenRearGuard: false),
                0,
                TriggerResolver.Resolve(TriggerType.Unknown));

            TriggerAllocationModifierResult result = TriggerAllocationModifierAdapter.ConvertToLedger(
                "unknown-trigger",
                manual,
                CombatModifierExpiration.EndOfTurn);

            Assert.IsFalse(result.accepted);
            Assert.IsTrue(result.needs_manual_resolution);
            Assert.AreEqual(0, result.ledger.modifiers.Count);
        }

        [Test]
        public void AdapterIsDeterministicAndDoesNotMutateState()
        {
            GameState state = CreateAllocationState(includeHiddenRearGuard: true);
            string before = state.ToJson();
            TriggerAllocationPlan plan = TriggerAllocationPlanner.Plan(
                state,
                0,
                TriggerResolver.Resolve(TriggerType.Front));

            TriggerAllocationModifierResult first = TriggerAllocationModifierAdapter.ConvertToLedger(
                "front-trigger",
                plan,
                CombatModifierExpiration.EndOfBattle);
            TriggerAllocationModifierResult second = TriggerAllocationModifierAdapter.ConvertToLedger(
                "front-trigger",
                plan,
                CombatModifierExpiration.EndOfBattle);
            string after = state.ToJson();

            Assert.AreEqual(before, after);
            Assert.AreEqual(first.ledger.modifiers.Count, second.ledger.modifiers.Count);
            for (int i = 0; i < first.ledger.modifiers.Count; i++)
            {
                Assert.AreEqual(first.ledger.modifiers[i].modifier_id, second.ledger.modifiers[i].modifier_id);
                Assert.AreEqual(first.ledger.modifiers[i].power_delta, second.ledger.modifiers[i].power_delta);
            }
        }

        private static GameState CreateAllocationState(bool includeHiddenRearGuard)
        {
            var vanguard = new GameCardInstance("vg", "VG", 0);
            var highRearGuard = new GameCardInstance("rg-high", "RG-HIGH", 0);
            highRearGuard.power_delta = 5000;
            var lowRearGuard = new GameCardInstance("rg-low", "RG-LOW", 0);

            var player = new PlayerGameState
            {
                player_id = "p1",
                vanguard = new List<GameCardInstance> { vanguard },
                rear_guard = new List<GameCardInstance> { highRearGuard, lowRearGuard }
            };

            if (includeHiddenRearGuard)
            {
                player.rear_guard.Add(new GameCardInstance(
                    "hidden-rg",
                    GameStateViewFactory.HiddenCardId,
                    0,
                    false));
            }

            return new GameState
            {
                players = new List<PlayerGameState> { player }
            };
        }

        private static GameState CreateEmptyBoardState()
        {
            return new GameState
            {
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1"
                    }
                }
            };
        }
    }
}
