using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class FirstStructuredCardPackTests
    {
        [Test]
        public void PackLoadsThroughRuntimeAbilityRegistry()
        {
            RuntimeAbilityRegistryLoadResult result =
                RuntimeAbilityRegistry.LoadFromJson(File.ReadAllText(PackPath()));

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(20, result.ability_count);
            Assert.AreEqual(20, result.card_count);
            Assert.NotNull(result.registry.FindAbility("m12_10_bt01_004_draw_01"));
            Assert.AreEqual(
                "BT01-004TH",
                result.registry.FindAbility("m12_10_bt01_004_draw_01").card_id);
        }

        [Test]
        public void PackDrawAbilityRunsThroughFixtureDsl()
        {
            RuntimeAbilityRegistry registry =
                RuntimeAbilityRegistry.LoadFromJson(File.ReadAllText(PackPath())).registry;
            StructuredAbility ability = registry.FindAbility("m12_10_bt01_004_draw_01");
            StructuredAbilityFixture fixture = BaseFixture("pack-draw-fixture", ability);
            fixture.expected = new StructuredAbilityFixtureExpectation
            {
                check_accepted = true,
                expected_accepted = true,
                check_event_count = true,
                expected_event_count = 1,
                check_player_hand_count = true,
                expected_player_hand_count = 2,
                check_player_deck_count = true,
                expected_player_deck_count = 0
            };

            StructuredAbilityFixtureResult result = StructuredAbilityFixtureRunner.Run(fixture);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.expectation_met, string.Join(",", result.expectation_mismatches));
        }

        [Test]
        public void PackPowerPlusAbilityRunsThroughFixtureDsl()
        {
            RuntimeAbilityRegistry registry =
                RuntimeAbilityRegistry.LoadFromJson(File.ReadAllText(PackPath())).registry;
            StructuredAbility ability = registry.FindAbility("m12_10_bt01_014_power_plus_01");
            StructuredAbilityFixture fixture = BaseFixture("pack-power-fixture", ability);
            fixture.expected = new StructuredAbilityFixtureExpectation
            {
                check_accepted = true,
                expected_accepted = true,
                check_event_count = true,
                expected_event_count = 0,
                check_modifier_count = true,
                expected_modifier_count = 1
            };

            StructuredAbilityFixtureResult result = StructuredAbilityFixtureRunner.Run(fixture);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.expectation_met, string.Join(",", result.expectation_mismatches));
            Assert.AreEqual(10000, result.combat_modifier_ledger_after.modifiers[0].power_delta);
        }

        private static StructuredAbilityFixture BaseFixture(string fixtureId, StructuredAbility ability)
        {
            return new StructuredAbilityFixture
            {
                fixture_id = fixtureId,
                player_index = 0,
                before_state = CreateState(),
                ability = ability,
                expected = new StructuredAbilityFixtureExpectation()
            };
        }

        private static GameState CreateState()
        {
            return new GameState
            {
                game_id = "first-structured-card-pack-test",
                format = "D",
                random_seed = 888,
                turn_number = 1,
                turn_player_index = 0,
                phase = GamePhase.Main,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
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
                        vanguard = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-vg-1", "VG-1", 0, true)
                        },
                        rear_guard = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-rg-1", "RG-1", 0, true)
                        },
                        damage = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-damage-face-up", "DAMAGE-UP", 0, true),
                            new GameCardInstance("p0-damage-face-down", "DAMAGE-DOWN", 0, false)
                        }
                    },
                    new PlayerGameState
                    {
                        player_id = "p1"
                    }
                },
                event_log = new List<GameEvent>(),
                pending_auto_abilities = new PendingAutoAbilityQueue()
            };
        }

        private static string PackPath()
        {
            return Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                "..",
                "..",
                "..",
                "data",
                "packs",
                "vanguard_th",
                "abilities",
                "structured_ability_pack_m12_10.json"));
        }
    }
}
