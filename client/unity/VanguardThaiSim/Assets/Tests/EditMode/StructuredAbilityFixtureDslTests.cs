using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class StructuredAbilityFixtureDslTests
    {
        [Test]
        public void DrawFixtureRunsBeforeActionAfterScenarioWithoutMutatingSource()
        {
            StructuredAbilityFixture fixture = CreateDrawFixture();
            GameStateNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(fixture.before_state);

            StructuredAbilityFixtureResult result = StructuredAbilityFixtureRunner.Run(fixture);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.expectation_met, string.Join(",", result.expectation_mismatches));
            Assert.AreEqual(1, result.event_count);
            Assert.AreEqual(2, result.player_hand_count);
            Assert.AreEqual(0, result.player_deck_count);
            Assert.IsTrue(snapshot.Matches(fixture.before_state));
            Assert.AreEqual(1, fixture.before_state.GetPlayer(0).deck.Count);
            Assert.AreEqual(1, fixture.before_state.GetPlayer(0).hand.Count);
        }

        [Test]
        public void ResourceFixtureCanAssertCounterBlastDamageFaceUpCount()
        {
            StructuredAbilityFixture fixture = CreateCounterBlastFixture();

            StructuredAbilityFixtureResult result = StructuredAbilityFixtureRunner.Run(fixture);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.expectation_met, string.Join(",", result.expectation_mismatches));
            Assert.AreEqual(1, result.event_count);
            Assert.AreEqual(0, result.damage_face_up_count);
            Assert.IsFalse(result.after_state.GetPlayer(0).damage[0].face_up);
            Assert.IsTrue(fixture.before_state.GetPlayer(0).damage[0].face_up);
        }

        [Test]
        public void ModifierFixtureWritesCombatLedgerOnly()
        {
            StructuredAbilityFixture fixture = CreatePowerPlusFixture();

            StructuredAbilityFixtureResult result = StructuredAbilityFixtureRunner.Run(fixture);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.expectation_met, string.Join(",", result.expectation_mismatches));
            Assert.AreEqual(0, result.event_count);
            Assert.AreEqual(1, result.modifier_count);
            Assert.AreEqual(10000, result.combat_modifier_ledger_after.modifiers[0].power_delta);
            Assert.AreEqual(0, fixture.combat_modifier_ledger.modifiers.Count);
            Assert.AreEqual(0, fixture.before_state.event_log.Count);
        }

        [Test]
        public void ExpectationMismatchIsReportedWithoutChangingAcceptedResult()
        {
            StructuredAbilityFixture fixture = CreateDrawFixture();
            fixture.expected.expected_event_count = 2;

            StructuredAbilityFixtureResult result = StructuredAbilityFixtureRunner.Run(fixture);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsFalse(result.expectation_met);
            Assert.AreEqual(1, result.expectation_mismatches.Count);
            Assert.IsTrue(result.expectation_mismatches[0].Contains("event_count"));
        }

        [Test]
        public void ManualFallbackEffectRejectsWithManualResolution()
        {
            StructuredAbilityFixture fixture = CreateManualFallbackFixture();

            StructuredAbilityFixtureResult result = StructuredAbilityFixtureRunner.Run(fixture);

            Assert.IsFalse(result.accepted);
            Assert.IsTrue(result.requires_manual_resolution);
            Assert.IsTrue(result.rejection_reason.StartsWith(
                StructuredAbilityTemplateAutomationGateRejectionReasons.ManualFallbackRequired));
            Assert.AreEqual(0, result.event_count);
        }

        [Test]
        public void FixtureAndResultRoundTripJson()
        {
            StructuredAbilityFixture fixture = CreatePowerPlusFixture();
            StructuredAbilityFixture roundTripFixture =
                StructuredAbilityFixture.FromJson(fixture.ToJson(false));

            StructuredAbilityFixtureResult result =
                StructuredAbilityFixtureRunner.Run(roundTripFixture);
            StructuredAbilityFixtureResult roundTripResult =
                StructuredAbilityFixtureResult.FromJson(result.ToJson(false));

            Assert.AreEqual(fixture.fixture_id, roundTripFixture.fixture_id);
            Assert.AreEqual(result.accepted, roundTripResult.accepted);
            Assert.AreEqual(result.modifier_count, roundTripResult.modifier_count);
            Assert.AreEqual(result.expectation_met, roundTripResult.expectation_met);
        }

        private static StructuredAbilityFixture CreateDrawFixture()
        {
            return new StructuredAbilityFixture
            {
                fixture_id = "fixture-draw",
                description = "Draw one card.",
                player_index = 0,
                before_state = CreateState(includeDamage: false),
                ability = Ability(
                    "fixture-draw-ability",
                    new List<StructuredAbilityEffect>
                    {
                        new StructuredAbilityEffect { type = "draw", amount = 1 }
                    },
                    new List<StructuredAbilityTarget>(),
                    Duration("instant", "none")),
                expected = new StructuredAbilityFixtureExpectation
                {
                    check_accepted = true,
                    expected_accepted = true,
                    check_event_count = true,
                    expected_event_count = 1,
                    check_player_hand_count = true,
                    expected_player_hand_count = 2,
                    check_player_deck_count = true,
                    expected_player_deck_count = 0
                }
            };
        }

        private static StructuredAbilityFixture CreateCounterBlastFixture()
        {
            return new StructuredAbilityFixture
            {
                fixture_id = "fixture-counter-blast",
                description = "Counter-Blast one damage.",
                player_index = 0,
                before_state = CreateState(includeDamage: true),
                ability = Ability(
                    "fixture-counter-blast-ability",
                    new List<StructuredAbilityEffect>
                    {
                        new StructuredAbilityEffect { type = "counter_blast", amount = 1 }
                    },
                    new List<StructuredAbilityTarget>(),
                    Duration("instant", "none")),
                expected = new StructuredAbilityFixtureExpectation
                {
                    check_accepted = true,
                    expected_accepted = true,
                    check_event_count = true,
                    expected_event_count = 1,
                    check_damage_face_up_count = true,
                    expected_damage_face_up_count = 0
                }
            };
        }

        private static StructuredAbilityFixture CreatePowerPlusFixture()
        {
            return new StructuredAbilityFixture
            {
                fixture_id = "fixture-power-plus",
                description = "Give a rear-guard +10000.",
                player_index = 0,
                before_state = CreateState(includeDamage: false),
                ability = Ability(
                    "fixture-power-plus-ability",
                    new List<StructuredAbilityEffect>
                    {
                        new StructuredAbilityEffect { type = "power_plus", amount = 10000 }
                    },
                    new List<StructuredAbilityTarget>
                    {
                        new StructuredAbilityTarget
                        {
                            id = "self_rg",
                            type = "unit",
                            owner = "self",
                            zone = "RearGuard",
                            count = 1,
                            optional = false
                        }
                    },
                    Duration("until_end_of_turn", "end_of_turn")),
                expected = new StructuredAbilityFixtureExpectation
                {
                    check_accepted = true,
                    expected_accepted = true,
                    check_event_count = true,
                    expected_event_count = 0,
                    check_modifier_count = true,
                    expected_modifier_count = 1
                }
            };
        }

        private static StructuredAbilityFixture CreateManualFallbackFixture()
        {
            return new StructuredAbilityFixture
            {
                fixture_id = "fixture-manual-fallback",
                description = "Unsupported manual effect falls back to manual resolution.",
                player_index = 0,
                before_state = CreateState(includeDamage: false),
                ability = Ability(
                    "fixture-manual-fallback-ability",
                    new List<StructuredAbilityEffect>
                    {
                        new StructuredAbilityEffect { type = "manual", amount = 1 }
                    },
                    new List<StructuredAbilityTarget>(),
                    Duration("instant", "none"))
            };
        }

        private static StructuredAbility Ability(
            string abilityId,
            List<StructuredAbilityEffect> effects,
            List<StructuredAbilityTarget> targets,
            StructuredAbilityDuration duration)
        {
            return new StructuredAbility
            {
                ability_id = abilityId,
                card_id = "FIXTURE-CARD",
                kind = "auto",
                trigger = new StructuredAbilityTrigger { type = "manual" },
                timing = new StructuredAbilityTiming { phase = "Main", window = "MainStep", optional = true },
                costs = new List<StructuredAbilityCost>(),
                targets = targets,
                effects = effects,
                duration = duration,
                manual_fallback = true
            };
        }

        private static StructuredAbilityDuration Duration(string type, string cleanupTiming)
        {
            return new StructuredAbilityDuration
            {
                type = type,
                cleanup_timing = cleanupTiming
            };
        }

        private static GameState CreateState(bool includeDamage)
        {
            var player = new PlayerGameState
            {
                player_id = "p0",
                deck = new List<GameCardInstance>
                {
                    new GameCardInstance("p0-deck-1", "DECK-1", 0, true)
                },
                hand = new List<GameCardInstance>
                {
                    new GameCardInstance("p0-hand-1", "HAND-1", 0, true)
                },
                rear_guard = new List<GameCardInstance>
                {
                    new GameCardInstance("p0-rg-1", "RG-1", 0, true)
                }
            };

            if (includeDamage)
            {
                player.damage.Add(new GameCardInstance("p0-damage-1", "DAMAGE-1", 0, true));
            }

            return new GameState
            {
                game_id = "structured-ability-fixture",
                format = "D",
                random_seed = 777,
                turn_number = 1,
                turn_player_index = 0,
                phase = GamePhase.Main,
                players = new List<PlayerGameState>
                {
                    player,
                    new PlayerGameState { player_id = "p1" }
                },
                event_log = new List<GameEvent>(),
                pending_auto_abilities = new PendingAutoAbilityQueue()
            };
        }
    }
}
