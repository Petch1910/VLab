using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class StructuredAbilityTemplateAutomationGateTests
    {
        [Test]
        public void CurrentCoverageReportValidatesAndRoundTripsJson()
        {
            StructuredAbilityTemplateCoverageReport report =
                StructuredAbilityTemplateAutomationGate.CreateCurrentCoverageReport();

            StructuredAbilityTemplateCoverageValidationResult validation =
                StructuredAbilityTemplateAutomationGate.ValidateCoverageReport(report);
            StructuredAbilityTemplateCoverageReport roundTrip =
                StructuredAbilityTemplateCoverageReport.FromJson(report.ToJson(false));

            Assert.IsTrue(validation.accepted, validation.rejection_reason);
            Assert.Greater(report.templates.Count, 10);
            Assert.AreEqual(report.templates.Count, roundTrip.templates.Count);
            Assert.AreEqual(report.schema_version, roundTrip.schema_version);
        }

        [Test]
        public void AutomatedCoverageEntryWithoutTestsRejectsReport()
        {
            StructuredAbilityTemplateCoverageReport report =
                StructuredAbilityTemplateAutomationGate.CreateCurrentCoverageReport();
            StructuredAbilityTemplateCoverageEntry draw = Find(report, "effect", "draw");
            draw.test_names.Clear();
            draw.covered_by_tests = false;

            StructuredAbilityTemplateCoverageValidationResult validation =
                StructuredAbilityTemplateAutomationGate.ValidateCoverageReport(report);

            Assert.IsFalse(validation.accepted);
            Assert.AreEqual(
                StructuredAbilityTemplateAutomationGateRejectionReasons.CoverageReportInvalid,
                validation.rejection_reason);
            Assert.IsTrue(validation.issues[0].Contains("effect:draw"));
        }

        [Test]
        public void CoveredDrawAndPowerPlusAbilitiesPassGate()
        {
            StructuredAbilityTemplateAutomationGateResult draw =
                StructuredAbilityTemplateAutomationGate.Validate(DrawAbility());
            StructuredAbilityTemplateAutomationGateResult power =
                StructuredAbilityTemplateAutomationGate.Validate(PowerPlusAbility());

            Assert.IsTrue(draw.accepted, draw.rejection_reason);
            Assert.IsFalse(draw.requires_manual_resolution);
            Assert.IsTrue(power.accepted, power.rejection_reason);
            Assert.AreEqual(0, power.unsupported_templates.Count);
        }

        [Test]
        public void ManualEffectRequiresManualFallbackBeforeFixtureExecution()
        {
            StructuredAbility ability = ManualAbility(manualFallback: true);
            StructuredAbilityTemplateAutomationGateResult gate =
                StructuredAbilityTemplateAutomationGate.Validate(ability);
            StructuredAbilityFixtureResult fixture =
                StructuredAbilityFixtureRunner.Run(new StructuredAbilityFixture
                {
                    fixture_id = "m26-04-manual-effect",
                    player_index = 0,
                    before_state = CreateState(includeRearGuard: true),
                    ability = ability
                });

            Assert.IsFalse(gate.accepted);
            Assert.IsTrue(gate.requires_manual_resolution);
            Assert.Contains("effect:manual", gate.unsupported_templates);
            Assert.IsFalse(fixture.accepted);
            Assert.IsTrue(fixture.requires_manual_resolution);
            Assert.IsTrue(fixture.rejection_reason.StartsWith(
                StructuredAbilityTemplateAutomationGateRejectionReasons.ManualFallbackRequired));
            Assert.AreEqual(0, fixture.event_count);
        }

        [Test]
        public void UnsupportedTemplateWithoutManualFallbackRejectsWithoutFallback()
        {
            StructuredAbility ability = ManualAbility(manualFallback: false);

            StructuredAbilityTemplateAutomationGateResult gate =
                StructuredAbilityTemplateAutomationGate.Validate(ability);

            Assert.IsFalse(gate.accepted);
            Assert.IsFalse(gate.requires_manual_resolution);
            Assert.IsTrue(gate.rejection_reason.StartsWith(
                StructuredAbilityTemplateAutomationGateRejectionReasons.ManualFallbackDisabled));
        }

        [Test]
        public void UntestedModifierDurationRequiresManualFallback()
        {
            StructuredAbility ability = PowerPlusAbility();
            ability.duration = new StructuredAbilityDuration
            {
                type = "continuous",
                cleanup_timing = "manual"
            };

            StructuredAbilityTemplateAutomationGateResult gate =
                StructuredAbilityTemplateAutomationGate.Validate(ability);

            Assert.IsFalse(gate.accepted);
            Assert.IsTrue(gate.requires_manual_resolution);
            Assert.Contains("duration:continuous", gate.unsupported_templates);
        }

        [Test]
        public void TargetFiltersAndHiddenZonesRequireManualFallback()
        {
            StructuredAbility ability = PowerPlusAbility();
            ability.targets[0].zone = "Deck";
            ability.targets[0].filters = new List<string> { "grade:3" };

            StructuredAbilityTemplateAutomationGateResult gate =
                StructuredAbilityTemplateAutomationGate.Validate(ability);

            Assert.IsFalse(gate.accepted);
            Assert.IsTrue(gate.requires_manual_resolution);
            Assert.Contains("target:filters", gate.unsupported_templates);
            Assert.Contains("target:hidden_zone:Deck", gate.unsupported_templates);
        }

        [Test]
        public void M12SmokePackUsesOnlyCoveredTemplateSubset()
        {
            RuntimeAbilityRegistry registry =
                RuntimeAbilityRegistry.LoadFromJson(File.ReadAllText(PackPath())).registry;
            string[] abilityIds =
            {
                "m12_10_bt01_004_draw_01",
                "m12_10_bt01_014_power_plus_01",
                "m12_10_bt01_015_critical_plus_01"
            };

            for (int i = 0; i < abilityIds.Length; i++)
            {
                StructuredAbilityTemplateAutomationGateResult gate =
                    StructuredAbilityTemplateAutomationGate.Validate(registry.FindAbility(abilityIds[i]));
                Assert.IsTrue(gate.accepted, abilityIds[i] + ": " + gate.rejection_reason);
            }
        }

        [Test]
        public void GateResultRoundTripsJson()
        {
            StructuredAbilityTemplateAutomationGateResult result =
                StructuredAbilityTemplateAutomationGate.Validate(ManualAbility(manualFallback: true));

            StructuredAbilityTemplateAutomationGateResult roundTrip =
                StructuredAbilityTemplateAutomationGateResult.FromJson(result.ToJson(false));

            Assert.AreEqual(result.accepted, roundTrip.accepted);
            Assert.AreEqual(result.requires_manual_resolution, roundTrip.requires_manual_resolution);
            Assert.AreEqual(result.unsupported_templates.Count, roundTrip.unsupported_templates.Count);
            Assert.AreEqual(result.unsupported_templates[0], roundTrip.unsupported_templates[0]);
        }

        private static StructuredAbilityTemplateCoverageEntry Find(
            StructuredAbilityTemplateCoverageReport report,
            string category,
            string templateId)
        {
            for (int i = 0; i < report.templates.Count; i++)
            {
                StructuredAbilityTemplateCoverageEntry entry = report.templates[i];
                if (entry.category == category && entry.template_id == templateId)
                {
                    return entry;
                }
            }

            Assert.Fail("Missing coverage entry " + category + ":" + templateId);
            return null;
        }

        private static StructuredAbility DrawAbility()
        {
            return BaseAbility(
                "m26-04-draw",
                new List<StructuredAbilityEffect>
                {
                    new StructuredAbilityEffect { type = "draw", amount = 1 }
                },
                new List<StructuredAbilityTarget>(),
                new StructuredAbilityDuration { type = "instant", cleanup_timing = "none" },
                true);
        }

        private static StructuredAbility PowerPlusAbility()
        {
            return BaseAbility(
                "m26-04-power-plus",
                new List<StructuredAbilityEffect>
                {
                    new StructuredAbilityEffect { type = "power_plus", amount = 10000, target_ref = "self_rg" }
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
                        optional = false,
                        filters = new List<string>()
                    }
                },
                new StructuredAbilityDuration { type = "until_end_of_turn", cleanup_timing = "end_of_turn" },
                true);
        }

        private static StructuredAbility ManualAbility(bool manualFallback)
        {
            return BaseAbility(
                "m26-04-manual",
                new List<StructuredAbilityEffect>
                {
                    new StructuredAbilityEffect { type = "manual", amount = 1 }
                },
                new List<StructuredAbilityTarget>(),
                new StructuredAbilityDuration { type = "instant", cleanup_timing = "none" },
                manualFallback);
        }

        private static StructuredAbility BaseAbility(
            string abilityId,
            List<StructuredAbilityEffect> effects,
            List<StructuredAbilityTarget> targets,
            StructuredAbilityDuration duration,
            bool manualFallback)
        {
            return new StructuredAbility
            {
                ability_id = abilityId,
                card_id = "M26-04-CARD",
                kind = "manual",
                trigger = new StructuredAbilityTrigger { type = "manual" },
                timing = new StructuredAbilityTiming
                {
                    phase = "Main",
                    window = "PendingAutoResolution",
                    optional = true
                },
                costs = new List<StructuredAbilityCost>(),
                targets = targets,
                effects = effects,
                duration = duration,
                manual_fallback = manualFallback
            };
        }

        private static GameState CreateState(bool includeRearGuard)
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
                }
            };
            if (includeRearGuard)
            {
                player.rear_guard.Add(new GameCardInstance("p0-rg-1", "RG-1", 0, true));
            }

            return new GameState
            {
                game_id = "m26-04-template-gate-test",
                format = "D",
                random_seed = 2604,
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
