using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class TriggerCheckResolutionBundleTests
    {
        [Test]
        public void CriticalTriggerBundleContainsResolverAllocationAndModifiers()
        {
            GameState state = CreateBoardState();

            TriggerCheckResolutionBundle bundle = TriggerCheckResolutionBundler.Build(
                state,
                0,
                TriggerCheckSource.Drive,
                0,
                "drive-card-1",
                "CRIT-001",
                TriggerType.Critical,
                CombatModifierExpiration.EndOfTurn);

            Assert.IsTrue(bundle.accepted);
            Assert.IsFalse(bundle.needs_manual_resolution);
            Assert.AreEqual(TriggerCheckSource.Drive, bundle.check_source);
            Assert.AreEqual(0, bundle.check_index);
            Assert.AreEqual("drive-card-1", bundle.checked_card_instance_id);
            Assert.AreEqual("CRIT-001", bundle.checked_card_id);
            Assert.AreEqual(TriggerType.Critical, bundle.trigger_result.trigger_type);
            Assert.AreEqual(10000, bundle.trigger_result.power_bonus);
            Assert.AreEqual(1, bundle.trigger_result.critical_bonus);
            Assert.AreEqual(1, bundle.allocation_plan.power_targets.Count);
            Assert.AreEqual(1, bundle.allocation_plan.critical_targets.Count);
            Assert.AreEqual(2, bundle.modifier_ledger.modifiers.Count);
            Assert.AreEqual(10000, bundle.modifier_ledger.Summarize("rg-high").total_power_delta);
            Assert.AreEqual(1, bundle.modifier_ledger.Summarize("vg").total_critical_delta);
        }

        [Test]
        public void NoTriggerBundleIsAcceptedWithoutModifiers()
        {
            TriggerCheckResolutionBundle bundle = TriggerCheckResolutionBundler.Build(
                CreateBoardState(),
                0,
                TriggerCheckSource.Damage,
                1,
                "damage-card-1",
                "NORMAL-001",
                TriggerType.None,
                CombatModifierExpiration.EndOfBattle);

            Assert.IsTrue(bundle.accepted);
            Assert.AreEqual(TriggerType.None, bundle.trigger_type);
            Assert.AreEqual(0, bundle.modifier_ledger.modifiers.Count);
            Assert.Greater(bundle.notes.Count, 0);
        }

        [Test]
        public void BundleJsonRoundTrips()
        {
            TriggerCheckResolutionBundle bundle = TriggerCheckResolutionBundler.Build(
                CreateBoardState(),
                0,
                TriggerCheckSource.Drive,
                0,
                "drive-card-1",
                "CRIT-001",
                TriggerType.Critical,
                CombatModifierExpiration.EndOfTurn);

            TriggerCheckResolutionBundle roundTrip =
                TriggerCheckResolutionBundle.FromJson(bundle.ToJson());

            Assert.AreEqual(bundle.accepted, roundTrip.accepted);
            Assert.AreEqual(bundle.check_source, roundTrip.check_source);
            Assert.AreEqual(bundle.checked_card_id, roundTrip.checked_card_id);
            Assert.AreEqual(bundle.trigger_result.power_bonus, roundTrip.trigger_result.power_bonus);
            Assert.AreEqual(bundle.allocation_plan.power_targets.Count, roundTrip.allocation_plan.power_targets.Count);
            Assert.AreEqual(bundle.modifier_ledger.modifiers.Count, roundTrip.modifier_ledger.modifiers.Count);
        }

        [Test]
        public void BundleIsDeterministic()
        {
            GameState state = CreateBoardState();

            TriggerCheckResolutionBundle first = TriggerCheckResolutionBundler.Build(
                state,
                0,
                TriggerCheckSource.Drive,
                0,
                "drive-card-1",
                "CRIT-001",
                TriggerType.Critical,
                CombatModifierExpiration.EndOfTurn);
            TriggerCheckResolutionBundle second = TriggerCheckResolutionBundler.Build(
                state,
                0,
                TriggerCheckSource.Drive,
                0,
                "drive-card-1",
                "CRIT-001",
                TriggerType.Critical,
                CombatModifierExpiration.EndOfTurn);

            Assert.AreEqual(first.ToJson(), second.ToJson());
        }

        [Test]
        public void BundleDoesNotMutateGameState()
        {
            GameState state = CreateBoardState();
            string before = state.ToJson();

            TriggerCheckResolutionBundler.Build(
                state,
                0,
                TriggerCheckSource.Drive,
                0,
                "drive-card-1",
                "CRIT-001",
                TriggerType.Critical,
                CombatModifierExpiration.EndOfTurn);

            Assert.AreEqual(before, state.ToJson());
        }

        private static GameState CreateBoardState()
        {
            var vanguard = new GameCardInstance("vg", "VG-001", 0);
            var highRearGuard = new GameCardInstance("rg-high", "RG-HIGH", 0);
            highRearGuard.power_delta = 5000;
            var lowRearGuard = new GameCardInstance("rg-low", "RG-LOW", 0);

            return new GameState
            {
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p1",
                        vanguard = new List<GameCardInstance> { vanguard },
                        rear_guard = new List<GameCardInstance> { highRearGuard, lowRearGuard }
                    }
                }
            };
        }
    }
}
