using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class StructuredAbilityManualFallbackBridgeTests
    {
        [Test]
        public void UnsupportedFixtureResultCreatesResolveDecision()
        {
            StructuredAbility ability = UnsupportedManualAbility(manualFallback: true);
            StructuredAbilityFixtureResult fixtureResult =
                StructuredAbilityFixtureRunner.Run(new StructuredAbilityFixture
                {
                    fixture_id = "manual-bridge-fixture",
                    player_index = 0,
                    before_state = CreateState(),
                    ability = ability
                });

            StructuredAbilityManualFallbackBridgeResult bridge =
                StructuredAbilityManualFallbackBridge.CreateResolveDecision(
                    ability,
                    fixtureResult,
                    0,
                    "PendingAutoResolution",
                    false);

            Assert.IsFalse(fixtureResult.accepted);
            Assert.IsTrue(fixtureResult.requires_manual_resolution);
            Assert.IsTrue(bridge.accepted, bridge.rejection_reason);
            Assert.AreEqual(PendingAutoAbilityManualResolutionDecisionTypes.Resolve, bridge.decision.decision_type);
            Assert.AreEqual(ability.card_id, bridge.request.source_card_id);
            Assert.AreEqual(fixtureResult.rejection_reason, bridge.decision.reason);
            Assert.IsTrue(bridge.pending_ability.summary.Contains(ability.ability_id));
        }

        [Test]
        public void HiddenFallbackDoesNotLeakSourceIdentity()
        {
            StructuredAbility ability = UnsupportedManualAbility(manualFallback: true);

            StructuredAbilityManualFallbackBridgeResult bridge =
                StructuredAbilityManualFallbackBridge.CreateResolveDecision(
                    ability,
                    "unsupported hidden source",
                    1,
                    "OnDraw",
                    true);

            Assert.IsTrue(bridge.accepted, bridge.rejection_reason);
            Assert.IsTrue(bridge.request.hides_source_card_identity);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, bridge.request.source_card_id);
            Assert.AreEqual(string.Empty, bridge.request.source_card_instance_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, bridge.decision.source_card_id);
            Assert.AreEqual(string.Empty, bridge.decision.source_card_instance_id);
            Assert.IsFalse(bridge.ToJson(false).Contains(ability.card_id));
        }

        [Test]
        public void DisabledManualFallbackRejects()
        {
            StructuredAbilityManualFallbackBridgeResult bridge =
                StructuredAbilityManualFallbackBridge.CreateResolveDecision(
                    UnsupportedManualAbility(manualFallback: false),
                    "unsupported",
                    0,
                    "PendingAutoResolution",
                    false);

            Assert.IsFalse(bridge.accepted);
            Assert.AreEqual(
                StructuredAbilityManualFallbackBridgeRejectionReasons.ManualFallbackDisabled,
                bridge.rejection_reason);
        }

        [Test]
        public void MissingReasonRejects()
        {
            StructuredAbilityManualFallbackBridgeResult bridge =
                StructuredAbilityManualFallbackBridge.CreateResolveDecision(
                    UnsupportedManualAbility(manualFallback: true),
                    "  \n ",
                    0,
                    "PendingAutoResolution",
                    false);

            Assert.IsFalse(bridge.accepted);
            Assert.AreEqual(
                StructuredAbilityManualFallbackBridgeRejectionReasons.RejectionReasonMissing,
                bridge.rejection_reason);
        }

        [Test]
        public void BridgeCreationDoesNotMutateAbilityOrGameState()
        {
            StructuredAbility ability = UnsupportedManualAbility(manualFallback: true);
            string abilityBefore = JsonUtility.ToJson(ability, false);
            GameState state = CreateState();
            GameStateNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(state);

            StructuredAbilityManualFallbackBridge.CreateResolveDecision(
                ability,
                "unsupported",
                0,
                "PendingAutoResolution",
                false);

            Assert.AreEqual(abilityBefore, JsonUtility.ToJson(ability, false));
            Assert.IsTrue(snapshot.Matches(state));
        }

        [Test]
        public void BridgeResultRoundTripsJson()
        {
            StructuredAbilityManualFallbackBridgeResult bridge =
                StructuredAbilityManualFallbackBridge.CreateResolveDecision(
                    UnsupportedManualAbility(manualFallback: true),
                    "unsupported",
                    0,
                    "PendingAutoResolution",
                    false);

            StructuredAbilityManualFallbackBridgeResult roundTrip =
                StructuredAbilityManualFallbackBridgeResult.FromJson(bridge.ToJson(false));

            Assert.AreEqual(bridge.accepted, roundTrip.accepted);
            Assert.AreEqual(bridge.request.pending_id, roundTrip.request.pending_id);
            Assert.AreEqual(bridge.decision.decision_id, roundTrip.decision.decision_id);
        }

        private static StructuredAbility UnsupportedManualAbility(bool manualFallback)
        {
            return new StructuredAbility
            {
                ability_id = "manual-fallback-unsupported-effect",
                card_id = "BT01-004TH",
                kind = "manual",
                trigger = new StructuredAbilityTrigger { type = "manual" },
                timing = new StructuredAbilityTiming
                {
                    phase = "Main",
                    window = "PendingAutoResolution",
                    optional = true
                },
                costs = new List<StructuredAbilityCost>(),
                targets = new List<StructuredAbilityTarget>(),
                effects = new List<StructuredAbilityEffect>
                {
                    new StructuredAbilityEffect { type = "manual", amount = 1 }
                },
                duration = new StructuredAbilityDuration { type = "instant", cleanup_timing = "none" },
                manual_fallback = manualFallback
            };
        }

        private static GameState CreateState()
        {
            return new GameState
            {
                game_id = "manual-fallback-bridge",
                format = "D",
                random_seed = 999,
                turn_number = 1,
                turn_player_index = 0,
                phase = GamePhase.Main,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState { player_id = "p0" },
                    new PlayerGameState { player_id = "p1" }
                },
                event_log = new List<GameEvent>(),
                pending_auto_abilities = new PendingAutoAbilityQueue()
            };
        }
    }
}
