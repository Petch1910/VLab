using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class CombatStatProjectionTests
    {
        [Test]
        public void ProjectionAppliesLedgerDeltasOnlyToRequestedTarget()
        {
            GameState state = CreateState(includeHiddenRearGuard: false);
            CombatModifierLedger ledger = CreateLedger();

            CombatStatProjection projection = CombatStatProjector.Project(state, 0, "rg-high", ledger);

            Assert.IsTrue(projection.accepted);
            Assert.AreEqual("RG-HIGH", projection.card_id);
            Assert.AreEqual(GameZone.RearGuard, projection.zone);
            Assert.AreEqual(0, projection.zone_index);
            Assert.AreEqual(5000, projection.current_power_delta);
            Assert.AreEqual(10000, projection.ledger_power_delta);
            Assert.AreEqual(1, projection.ledger_critical_delta);
            Assert.AreEqual(15000, projection.projected_power_delta_total);
            Assert.AreEqual(1, projection.projected_critical_delta_total);
            Assert.AreEqual(2, projection.modifier_count);

            CombatStatProjection otherProjection = CombatStatProjector.Project(state, 0, "rg-low", ledger);
            Assert.IsTrue(otherProjection.accepted);
            Assert.AreEqual(3000, otherProjection.ledger_power_delta);
            Assert.AreEqual(0, otherProjection.ledger_critical_delta);
        }

        [Test]
        public void MissingTargetReturnsClearRejection()
        {
            CombatStatProjection projection = CombatStatProjector.Project(
                CreateState(includeHiddenRearGuard: false),
                0,
                "missing-unit",
                CreateLedger());

            Assert.IsFalse(projection.accepted);
            Assert.AreEqual("missing-unit", projection.target_card_instance_id);
            Assert.IsTrue(projection.rejection_reason.Contains("not found"));
        }

        [Test]
        public void HiddenTargetIsRejected()
        {
            CombatStatProjection projection = CombatStatProjector.Project(
                CreateState(includeHiddenRearGuard: true),
                0,
                "hidden-rg",
                CreateLedger());

            Assert.IsFalse(projection.accepted);
            Assert.IsTrue(projection.rejection_reason.Contains("not found"));
        }

        [Test]
        public void ProjectionJsonRoundTrips()
        {
            CombatStatProjection projection = CombatStatProjector.Project(
                CreateState(includeHiddenRearGuard: false),
                0,
                "rg-high",
                CreateLedger());

            CombatStatProjection roundTrip = CombatStatProjection.FromJson(projection.ToJson());

            Assert.AreEqual(projection.accepted, roundTrip.accepted);
            Assert.AreEqual(projection.target_card_instance_id, roundTrip.target_card_instance_id);
            Assert.AreEqual(projection.card_id, roundTrip.card_id);
            Assert.AreEqual(projection.projected_power_delta_total, roundTrip.projected_power_delta_total);
            Assert.AreEqual(projection.projected_critical_delta_total, roundTrip.projected_critical_delta_total);
        }

        [Test]
        public void ProjectorIsDeterministicAndDoesNotMutateState()
        {
            GameState state = CreateState(includeHiddenRearGuard: true);
            string before = state.ToJson();
            CombatModifierLedger ledger = CreateLedger();

            CombatStatProjection first = CombatStatProjector.Project(state, 0, "rg-high", ledger);
            CombatStatProjection second = CombatStatProjector.Project(state, 0, "rg-high", ledger);
            string after = state.ToJson();

            Assert.AreEqual(before, after);
            Assert.AreEqual(first.projected_power_delta_total, second.projected_power_delta_total);
            Assert.AreEqual(first.projected_critical_delta_total, second.projected_critical_delta_total);
            Assert.AreEqual(first.modifier_count, second.modifier_count);
        }

        private static GameState CreateState(bool includeHiddenRearGuard)
        {
            var highRearGuard = new GameCardInstance("rg-high", "RG-HIGH", 0);
            highRearGuard.power_delta = 5000;
            var lowRearGuard = new GameCardInstance("rg-low", "RG-LOW", 0);

            var player = new PlayerGameState
            {
                player_id = "p1",
                vanguard = new List<GameCardInstance>
                {
                    new GameCardInstance("vg", "VG", 0)
                },
                rear_guard = new List<GameCardInstance>
                {
                    highRearGuard,
                    lowRearGuard
                }
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

        private static CombatModifierLedger CreateLedger()
        {
            var ledger = new CombatModifierLedger();
            ledger.Add(new CombatModifier
            {
                modifier_id = "rg-high-power",
                source_id = "trigger",
                target_card_instance_id = "rg-high",
                power_delta = 10000,
                critical_delta = 0,
                expires_at = CombatModifierExpiration.EndOfTurn
            });
            ledger.Add(new CombatModifier
            {
                modifier_id = "rg-high-critical",
                source_id = "trigger",
                target_card_instance_id = "rg-high",
                power_delta = 0,
                critical_delta = 1,
                expires_at = CombatModifierExpiration.EndOfTurn
            });
            ledger.Add(new CombatModifier
            {
                modifier_id = "rg-low-power",
                source_id = "ability",
                target_card_instance_id = "rg-low",
                power_delta = 3000,
                critical_delta = 0,
                expires_at = CombatModifierExpiration.EndOfBattle
            });
            return ledger;
        }
    }
}
