using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class StructuredModifierEffectTemplateTests
    {
        [Test]
        public void PreviewPowerPlusCreatesLedgerAndDoesNotMutateStateOrSourceLedger()
        {
            GameState state = CreateState();
            CombatModifierLedger sourceLedger = CreateExistingLedger();
            GameStateNoMutationSnapshot stateSnapshot = NoMutationSnapshot.Capture(state);
            string sourceLedgerBefore = sourceLedger.ToJson(false);

            StructuredModifierEffectTemplateResult result =
                StructuredModifierEffectTemplate.Preview(
                    state,
                    0,
                    sourceLedger,
                    new StructuredAbilityEffect { type = "power_plus", amount = 10000 },
                    Target("rg-high"),
                    Duration("until_end_of_turn", "end_of_turn"),
                    "ability-1");

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.preview_only);
            Assert.AreEqual(2, result.ledger_after.modifiers.Count);
            Assert.AreEqual(10000, result.modifier.power_delta);
            Assert.AreEqual(0, result.modifier.critical_delta);
            Assert.AreEqual(CombatModifierExpiration.EndOfTurn, result.modifier.expires_at);
            Assert.IsTrue(result.projection_after.accepted);
            Assert.AreEqual(13000, result.projection_after.projected_power_delta_total);
            Assert.IsTrue(stateSnapshot.Matches(state));
            Assert.AreEqual(sourceLedgerBefore, sourceLedger.ToJson(false));
        }

        [Test]
        public void ApplyCriticalPlusMutatesOnlyLedgerAndUsesEndOfBattleDuration()
        {
            GameState state = CreateState();
            CombatModifierLedger ledger = new CombatModifierLedger();
            GameStateNoMutationSnapshot stateSnapshot = NoMutationSnapshot.Capture(state);

            StructuredModifierEffectTemplateResult result =
                StructuredModifierEffectTemplate.ApplyToLedger(
                    state,
                    0,
                    ledger,
                    new StructuredAbilityEffect { type = "critical_plus", amount = 1 },
                    Target("vg"),
                    Duration("until_end_of_battle", "end_of_battle"),
                    "ability-critical");

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsFalse(result.preview_only);
            Assert.AreEqual(1, ledger.modifiers.Count);
            Assert.AreEqual(0, ledger.modifiers[0].power_delta);
            Assert.AreEqual(1, ledger.modifiers[0].critical_delta);
            Assert.AreEqual(CombatModifierExpiration.EndOfBattle, ledger.modifiers[0].expires_at);
            Assert.AreEqual(1, result.projection_after.projected_critical_delta_total);
            Assert.IsTrue(stateSnapshot.Matches(state));
            Assert.AreEqual(0, state.event_log.Count);
        }

        [Test]
        public void UnsupportedDurationRequiresManualResolutionWithoutLedgerMutation()
        {
            GameState state = CreateState();
            CombatModifierLedger ledger = new CombatModifierLedger();

            StructuredModifierEffectTemplateResult result =
                StructuredModifierEffectTemplate.ApplyToLedger(
                    state,
                    0,
                    ledger,
                    new StructuredAbilityEffect { type = "power_plus", amount = 5000 },
                    Target("rg-high"),
                    Duration("instant", "none"),
                    "ability-instant");

            Assert.IsFalse(result.accepted);
            Assert.IsTrue(result.requires_manual_resolution);
            Assert.IsTrue(result.rejection_reason.StartsWith(
                StructuredModifierEffectTemplateRejectionReasons.UnsupportedDuration));
            Assert.AreEqual(0, ledger.modifiers.Count);
        }

        [Test]
        public void MissingAmountRejectsWithoutManualFallback()
        {
            StructuredModifierEffectTemplateResult result =
                StructuredModifierEffectTemplate.Preview(
                    CreateState(),
                    0,
                    null,
                    new StructuredAbilityEffect { type = "power_plus", amount = 0 },
                    Target("rg-high"),
                    Duration("until_end_of_turn", "end_of_turn"),
                    "ability-missing-amount");

            Assert.IsFalse(result.accepted);
            Assert.IsFalse(result.requires_manual_resolution);
            Assert.AreEqual(
                StructuredModifierEffectTemplateRejectionReasons.AmountMissingOrZero,
                result.rejection_reason);
        }

        [Test]
        public void HiddenOrMissingTargetRejectsWithoutLedgerMutation()
        {
            GameState state = CreateState();
            CombatModifierLedger ledger = new CombatModifierLedger();

            StructuredModifierEffectTemplateResult result =
                StructuredModifierEffectTemplate.ApplyToLedger(
                    state,
                    0,
                    ledger,
                    new StructuredAbilityEffect { type = "critical_plus", amount = 1 },
                    Target("hidden-rg"),
                    Duration("until_end_of_turn", "end_of_turn"),
                    "ability-hidden");

            Assert.IsFalse(result.accepted);
            Assert.IsFalse(result.requires_manual_resolution);
            Assert.IsTrue(result.rejection_reason.StartsWith(
                StructuredModifierEffectTemplateRejectionReasons.TargetProjectionRejected));
            Assert.AreEqual(0, ledger.modifiers.Count);
        }

        [Test]
        public void ModifierTemplateResultRoundTripsJson()
        {
            StructuredModifierEffectTemplateResult result =
                StructuredModifierEffectTemplate.Preview(
                    CreateState(),
                    0,
                    null,
                    new StructuredAbilityEffect { type = "critical_plus", amount = 1 },
                    Target("vg"),
                    Duration("until_end_of_turn", "end_of_turn"),
                    "ability-round-trip");

            StructuredModifierEffectTemplateResult roundTrip =
                StructuredModifierEffectTemplateResult.FromJson(result.ToJson(false));

            Assert.AreEqual(result.accepted, roundTrip.accepted);
            Assert.AreEqual(result.preview_only, roundTrip.preview_only);
            Assert.AreEqual(result.modifier.modifier_id, roundTrip.modifier.modifier_id);
            Assert.AreEqual(result.ledger_after.modifiers.Count, roundTrip.ledger_after.modifiers.Count);
            Assert.AreEqual(
                result.projection_after.projected_critical_delta_total,
                roundTrip.projection_after.projected_critical_delta_total);
        }

        private static StructuredTargetCandidate Target(string instanceId)
        {
            return new StructuredTargetCandidate
            {
                player_index = 0,
                zone = "RearGuard",
                instance_id = instanceId,
                card_id = instanceId,
                face_up = true
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

        private static CombatModifierLedger CreateExistingLedger()
        {
            var ledger = new CombatModifierLedger();
            ledger.Add(new CombatModifier
            {
                modifier_id = "existing-rg-high",
                source_id = "existing",
                target_card_instance_id = "rg-high",
                power_delta = 3000,
                critical_delta = 0,
                expires_at = CombatModifierExpiration.EndOfTurn,
                note = "existing modifier"
            });
            return ledger;
        }

        private static GameState CreateState()
        {
            return new GameState
            {
                game_id = "structured-modifier-effect-template",
                format = "D",
                random_seed = 555,
                turn_number = 1,
                turn_player_index = 0,
                phase = GamePhase.Main,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p0",
                        vanguard = new List<GameCardInstance>
                        {
                            new GameCardInstance("vg", "VG", 0, true)
                        },
                        rear_guard = new List<GameCardInstance>
                        {
                            new GameCardInstance("rg-high", "RG-HIGH", 0, true),
                            new GameCardInstance("hidden-rg", GameStateViewFactory.HiddenCardId, 0, false)
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
    }
}
