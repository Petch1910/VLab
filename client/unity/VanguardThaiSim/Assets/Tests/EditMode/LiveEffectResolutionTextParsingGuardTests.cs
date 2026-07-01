using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class LiveEffectResolutionTextParsingGuardTests
    {
        [Test]
        public void CurrentPolicyReportValidatesAndRoundTripsJson()
        {
            LiveEffectResolutionPolicyReport report =
                LiveEffectResolutionTextParsingGuard.CreateCurrentReport();

            LiveEffectResolutionPolicyValidationResult validation =
                LiveEffectResolutionTextParsingGuard.ValidateReport(report);
            LiveEffectResolutionPolicyReport roundTrip =
                LiveEffectResolutionPolicyReport.FromJson(report.ToJson(false));

            Assert.IsTrue(validation.accepted, validation.rejection_reason);
            Assert.Greater(report.entries.Count, 3);
            Assert.AreEqual(report.entries.Count, roundTrip.entries.Count);
            Assert.AreEqual(report.schema_version, roundTrip.schema_version);
        }

        [Test]
        public void PolicyReportRejectsLiveTextOrLlmResolution()
        {
            LiveEffectResolutionPolicyReport report =
                LiveEffectResolutionTextParsingGuard.CreateCurrentReport();
            report.entries[0].policy.allows_live_text_parsing = true;
            report.entries[1].policy.allows_llm_resolution = true;

            LiveEffectResolutionPolicyValidationResult validation =
                LiveEffectResolutionTextParsingGuard.ValidateReport(report);

            Assert.IsFalse(validation.accepted);
            Assert.IsTrue(validation.rejection_reason.StartsWith(
                LiveEffectResolutionTextParsingGuardRejectionReasons.ReportInvalid));
            Assert.AreEqual(2, validation.issues.Count);
        }

        [Test]
        public void CustomHandlerDefaultRegistrationUsesStructuredCommandPolicy()
        {
            const string EffectId = "m26_05.structured.handler";
            AbilityEffectRegistry.Register(
                EffectId,
                delegate(GameState state, int playerIndex, AbilityDefinition ability, AbilityEffectDefinition effect)
                {
                    return AbilityResolutionResult.NeedsManualResolution("test only");
                });

            AbilityEffectHandlerPolicy policy = AbilityEffectRegistry.GetPolicy(EffectId);
            AbilityEffectRegistry.Unregister(EffectId);

            Assert.NotNull(policy);
            Assert.IsFalse(policy.allows_live_text_parsing);
            Assert.IsFalse(policy.allows_llm_resolution);
            Assert.AreEqual("structured_command_only", policy.policy_id);
        }

        [Test]
        public void CustomHandlerRegistrationRejectsLiveTextPolicy()
        {
            var policy = new AbilityEffectHandlerPolicy
            {
                policy_id = "live_text_parser",
                allows_live_text_parsing = true,
                allows_llm_resolution = false,
                source = "test",
                notes = "should reject"
            };

            Assert.Throws<System.InvalidOperationException>(() =>
                AbilityEffectRegistry.Register(
                    "m26_05.live_text",
                    delegate(GameState state, int playerIndex, AbilityDefinition ability, AbilityEffectDefinition effect)
                    {
                        return AbilityResolutionResult.Accepted();
                    },
                    policy));
        }

        [Test]
        public void CustomHandlerRegistrationRejectsLlmPolicy()
        {
            var policy = new AbilityEffectHandlerPolicy
            {
                policy_id = "structured_command_only",
                allows_live_text_parsing = false,
                allows_llm_resolution = true,
                source = "test",
                notes = "should reject"
            };

            Assert.Throws<System.InvalidOperationException>(() =>
                AbilityEffectRegistry.Register(
                    "m26_05.llm",
                    delegate(GameState state, int playerIndex, AbilityDefinition ability, AbilityEffectDefinition effect)
                    {
                        return AbilityResolutionResult.Accepted();
                    },
                    policy));
        }

        [Test]
        public void AbilityCoreMissingCustomHandlerFallsBackWithoutMutatingState()
        {
            GameState state = CreateState();
            GameStateNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(state);
            AbilityDefinition ability = CustomAbility("m26_05.missing_handler", manualFallback: true);

            AbilityResolutionResult result = AbilityCore.Resolve(state, 0, ability);

            Assert.IsFalse(result.accepted);
            Assert.IsTrue(result.needs_manual_resolution);
            Assert.IsTrue(result.rejection_reason.StartsWith(
                LiveEffectResolutionTextParsingGuardRejectionReasons.CustomHandlerMissing));
            Assert.IsTrue(snapshot.Matches(state));
            Assert.AreEqual(0, state.event_log.Count);
        }

        [Test]
        public void RegisteredStructuredCustomHandlerCanStillResolveThroughRulesCore()
        {
            const string EffectId = "m26_05.protect_marker";
            AbilityEffectRegistry.Register(
                EffectId,
                delegate(GameState state, int playerIndex, AbilityDefinition ability, AbilityEffectDefinition effect)
                {
                    RulesCommandResult command = RulesCore.TryExecute(state, new LegalGameAction
                    {
                        action_type = GameActionType.AddGiftMarker,
                        actor_index = playerIndex,
                        gift_marker_type = GiftMarkerType.Protect,
                        marker_delta = 1
                    });
                    if (!command.accepted)
                    {
                        return AbilityResolutionResult.Rejected(command.rejection_reason);
                    }

                    AbilityResolutionResult resolved = AbilityResolutionResult.Accepted();
                    resolved.events.Add(command.game_event);
                    return resolved;
                });

            GameState state = CreateState();
            AbilityResolutionResult result = AbilityCore.Resolve(
                state,
                0,
                CustomAbility(EffectId, manualFallback: false));
            AbilityEffectRegistry.Unregister(EffectId);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(1, state.GetPlayer(0).GetGiftMarkerCount(GiftMarkerType.Protect));
            Assert.AreEqual(1, state.event_log.Count);
        }

        [Test]
        public void ValidationResultRoundTripsJson()
        {
            LiveEffectResolutionPolicyValidationResult result =
                LiveEffectResolutionTextParsingGuard.ValidateCustomEffectPolicy(
                    "m26_05.roundtrip",
                    AbilityEffectHandlerPolicy.StructuredCommandOnly("test"));

            LiveEffectResolutionPolicyValidationResult roundTrip =
                LiveEffectResolutionPolicyValidationResult.FromJson(result.ToJson(false));

            Assert.AreEqual(result.accepted, roundTrip.accepted);
            Assert.AreEqual(result.rejection_reason, roundTrip.rejection_reason);
            Assert.AreEqual(result.issues.Count, roundTrip.issues.Count);
        }

        private static AbilityDefinition CustomAbility(string effectId, bool manualFallback)
        {
            return new AbilityDefinition
            {
                ability_id = "m26-05-custom",
                label = "M26-05 custom",
                timing = AbilityTiming.Manual,
                manual_fallback = manualFallback,
                effects = new List<AbilityEffectDefinition>
                {
                    new AbilityEffectDefinition
                    {
                        effect_type = AbilityEffectType.Custom,
                        custom_effect_id = effectId
                    }
                }
            };
        }

        private static GameState CreateState()
        {
            return GameStateFactory.CreateTwoPlayerGame(
                CreateSampleDeck("p1"),
                CreateSampleDeck("p2"),
                2605);
        }

        private static VanguardDeck CreateSampleDeck(string prefix)
        {
            VanguardDeck deck = VanguardDeck.Create(prefix + " deck", "D", "vanguard_th", "test");
            for (int i = 0; i < 50; i++)
            {
                deck.AddCard(DeckZone.Main, prefix + "-MAIN-" + i, 1);
            }

            for (int i = 0; i < 4; i++)
            {
                deck.AddCard(DeckZone.Ride, prefix + "-RIDE-" + i, 1);
            }

            return deck;
        }
    }
}
