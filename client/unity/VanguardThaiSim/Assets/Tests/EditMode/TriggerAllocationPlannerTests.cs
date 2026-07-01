using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerAllocationPlannerTests
    {
        [Test]
        public void CriticalTriggerSplitsPowerToBestUnitAndCriticalToVanguard()
        {
            GameState state = CreateAllocationState(includeHiddenRearGuard: false);
            TriggerAllocationPlan plan = TriggerAllocationPlanner.Plan(
                state,
                0,
                TriggerResolver.Resolve(TriggerType.Critical));

            Assert.IsTrue(plan.accepted);
            Assert.AreEqual(1, plan.power_targets.Count);
            Assert.AreEqual("RG-HIGH", plan.power_targets[0].card_id);
            Assert.AreEqual(10000, plan.power_targets[0].power_bonus);
            Assert.AreEqual(1, plan.critical_targets.Count);
            Assert.AreEqual("VG", plan.critical_targets[0].card_id);
            Assert.AreEqual(1, plan.critical_targets[0].critical_bonus);
        }

        [Test]
        public void FrontTriggerTargetsVisibleUnitsAndSkipsHiddenUnits()
        {
            GameState state = CreateAllocationState(includeHiddenRearGuard: true);
            TriggerAllocationPlan plan = TriggerAllocationPlanner.Plan(
                state,
                0,
                TriggerResolver.Resolve(TriggerType.Front));

            Assert.IsTrue(plan.accepted);
            Assert.AreEqual(3, plan.power_targets.Count);
            foreach (TriggerAllocationTarget target in plan.power_targets)
            {
                Assert.AreNotEqual(GameStateViewFactory.HiddenCardId, target.card_id);
                Assert.AreNotEqual("hidden-rg", target.card_instance_id);
                Assert.AreEqual(10000, target.power_bonus);
            }
        }

        [Test]
        public void DrawHealAndOverTriggersEmitSideEffectNotes()
        {
            GameState state = CreateAllocationState(includeHiddenRearGuard: false);

            TriggerAllocationPlan draw = TriggerAllocationPlanner.Plan(
                state,
                0,
                TriggerResolver.Resolve(TriggerType.Draw));
            TriggerAllocationPlan heal = TriggerAllocationPlanner.Plan(
                state,
                0,
                TriggerResolver.Resolve(TriggerType.Heal));
            TriggerAllocationPlan over = TriggerAllocationPlanner.Plan(
                state,
                0,
                TriggerResolver.Resolve(TriggerType.Over));

            Assert.AreEqual(1, draw.side_effect_notes.Count);
            Assert.IsTrue(draw.side_effect_notes[0].Contains("Draw 1"));
            Assert.AreEqual(1, heal.side_effect_notes.Count);
            Assert.IsTrue(heal.side_effect_notes[0].Contains("Attempt heal"));
            Assert.AreEqual(1, over.marker_notes.Count);
            Assert.IsTrue(over.marker_notes[0].Contains("Over trigger"));
            Assert.AreEqual("VG", over.power_targets[0].card_id);
        }

        [Test]
        public void NoneAndUnknownTriggersUseExpectedPlanStatus()
        {
            GameState state = CreateAllocationState(includeHiddenRearGuard: false);
            TriggerAllocationPlan none = TriggerAllocationPlanner.Plan(
                state,
                0,
                TriggerResolver.Resolve(TriggerType.None));
            TriggerAllocationPlan unknown = TriggerAllocationPlanner.Plan(
                state,
                0,
                TriggerResolver.Resolve(TriggerType.Unknown));

            Assert.IsTrue(none.accepted);
            Assert.AreEqual(0, none.power_targets.Count);
            Assert.AreEqual(0, none.critical_targets.Count);
            Assert.AreEqual(1, none.side_effect_notes.Count);
            Assert.IsFalse(unknown.accepted);
            Assert.IsTrue(unknown.needs_manual_resolution);
        }

        [Test]
        public void AllocationPlanJsonRoundTrips()
        {
            TriggerAllocationPlan plan = TriggerAllocationPlanner.Plan(
                CreateAllocationState(includeHiddenRearGuard: false),
                0,
                TriggerResolver.Resolve(TriggerType.Critical));

            TriggerAllocationPlan roundTrip = TriggerAllocationPlan.FromJson(plan.ToJson());

            Assert.AreEqual(plan.trigger_type, roundTrip.trigger_type);
            Assert.AreEqual(plan.power_targets.Count, roundTrip.power_targets.Count);
            Assert.AreEqual(plan.power_targets[0].card_id, roundTrip.power_targets[0].card_id);
            Assert.AreEqual(plan.critical_targets[0].card_id, roundTrip.critical_targets[0].card_id);
        }

        [Test]
        public void PlannerIsDeterministicAndDoesNotMutateState()
        {
            GameState state = CreateAllocationState(includeHiddenRearGuard: true);
            string before = state.ToJson();

            TriggerAllocationPlan first = TriggerAllocationPlanner.Plan(
                state,
                0,
                TriggerResolver.Resolve(TriggerType.Front));
            TriggerAllocationPlan second = TriggerAllocationPlanner.Plan(
                state,
                0,
                TriggerResolver.Resolve(TriggerType.Front));
            string after = state.ToJson();

            Assert.AreEqual(before, after);
            Assert.AreEqual(first.power_targets.Count, second.power_targets.Count);
            for (int i = 0; i < first.power_targets.Count; i++)
            {
                Assert.AreEqual(first.power_targets[i].card_instance_id, second.power_targets[i].card_instance_id);
                Assert.AreEqual(first.power_targets[i].power_bonus, second.power_targets[i].power_bonus);
            }
        }

        private static GameState CreateAllocationState(bool includeHiddenRearGuard)
        {
            var vanguard = new GameCardInstance("vg", "VG", 0);
            vanguard.power_delta = 0;
            var highRearGuard = new GameCardInstance("rg-high", "RG-HIGH", 0);
            highRearGuard.power_delta = 5000;
            var lowRearGuard = new GameCardInstance("rg-low", "RG-LOW", 0);
            lowRearGuard.power_delta = 0;

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
    }
}
