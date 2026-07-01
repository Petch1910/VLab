using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class StructuredEffectTemplateTests
    {
        [Test]
        public void PreviewDrawDoesNotMutateLiveState()
        {
            GameState state = CreateState();
            GameStateNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(state);

            StructuredEffectTemplateResult result =
                StructuredEffectTemplate.Preview(state, 0, new StructuredAbilityEffect { type = "draw", amount = 1 });

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.preview_only);
            Assert.AreEqual(1, result.events.Count);
            Assert.IsTrue(snapshot.Matches(state));
            Assert.AreEqual(1, state.GetPlayer(0).deck.Count);
            Assert.AreEqual(1, state.GetPlayer(0).hand.Count);
        }

        [Test]
        public void ApplyDrawMutatesThroughRulesCoreEventPath()
        {
            GameState state = CreateState();

            StructuredEffectTemplateResult result =
                StructuredEffectTemplate.Apply(state, 0, new StructuredAbilityEffect { type = "draw", amount = 1 });

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(1, result.events.Count);
            Assert.AreEqual(GameActionType.Draw, result.events[0].action_type);
            Assert.AreEqual(0, state.GetPlayer(0).deck.Count);
            Assert.AreEqual(2, state.GetPlayer(0).hand.Count);
            Assert.AreEqual(1, state.event_log.Count);
        }

        [Test]
        public void ApplyMoveZoneUsesRulesCoreMoveCommand()
        {
            GameState state = CreateState();

            StructuredEffectTemplateResult result =
                StructuredEffectTemplate.Apply(
                    state,
                    0,
                    new StructuredAbilityEffect
                    {
                        type = "move_zone",
                        amount = 1,
                        from_zone = "Hand",
                        to_zone = "RearGuard"
                    });

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(GameActionType.MoveCard, result.events[0].action_type);
            Assert.AreEqual(0, state.GetPlayer(0).hand.Count);
            Assert.AreEqual(1, state.GetPlayer(0).rear_guard.Count);
            Assert.AreEqual("p0-hand-1", state.GetPlayer(0).rear_guard[0].instance_id);
        }

        [Test]
        public void ApplyCounterBlastFlipsFaceDownThroughRulesCoreEventPath()
        {
            GameState state = CreateState();

            StructuredEffectTemplateResult result =
                StructuredEffectTemplate.Apply(
                    state,
                    0,
                    new StructuredAbilityEffect { type = "counter_blast", amount = 1 });

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(1, result.events.Count);
            Assert.AreEqual(GameActionType.ResourceFlip, result.events[0].action_type);
            Assert.AreEqual(GameResourceOperationType.CounterBlast, result.events[0].resource_operation_type);
            Assert.IsTrue(result.events[0].previous_face_up);
            Assert.IsFalse(result.events[0].new_face_up);
            Assert.IsFalse(state.GetPlayer(0).damage[0].face_up);
            Assert.AreEqual(1, state.event_log.Count);
        }

        [Test]
        public void PreviewCounterChargeDoesNotMutateLiveState()
        {
            GameState state = CreateState();
            state.GetPlayer(0).damage[0].face_up = false;
            GameStateNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(state);

            StructuredEffectTemplateResult result =
                StructuredEffectTemplate.Preview(
                    state,
                    0,
                    new StructuredAbilityEffect { type = "counter_charge", amount = 1 });

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.preview_only);
            Assert.AreEqual(GameActionType.ResourceFlip, result.events[0].action_type);
            Assert.AreEqual(GameResourceOperationType.CounterCharge, result.events[0].resource_operation_type);
            Assert.IsTrue(snapshot.Matches(state));
            Assert.IsFalse(state.GetPlayer(0).damage[0].face_up);
            Assert.AreEqual(0, state.event_log.Count);
        }

        [Test]
        public void CounterBlastRejectsWithoutMutationWhenNoFaceUpDamageExists()
        {
            GameState state = CreateState();
            state.GetPlayer(0).damage[0].face_up = false;
            GameStateNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(state);

            StructuredEffectTemplateResult result =
                StructuredEffectTemplate.Apply(
                    state,
                    0,
                    new StructuredAbilityEffect { type = "counter_blast", amount = 1 });

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(StructuredEffectTemplateRejectionReasons.NoLegalCommand, result.rejection_reason);
            Assert.IsFalse(result.requires_manual_resolution);
            Assert.IsTrue(snapshot.Matches(state));
        }

        [Test]
        public void SoulChargeMovesTopDeckToSoulThroughRulesCoreEventPath()
        {
            GameState state = CreateState();

            StructuredEffectTemplateResult result =
                StructuredEffectTemplate.Apply(
                    state,
                    0,
                    new StructuredAbilityEffect { type = "soul_charge", amount = 1 });

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(1, result.events.Count);
            Assert.AreEqual(GameActionType.MoveCard, result.events[0].action_type);
            Assert.AreEqual(GameZone.Deck, result.events[0].from_zone);
            Assert.AreEqual(GameZone.Soul, result.events[0].to_zone);
            Assert.AreEqual("p0-deck-1", state.GetPlayer(0).soul[0].instance_id);
            Assert.AreEqual(1, state.event_log.Count);
        }

        [Test]
        public void PreviewSoulChargeDoesNotMutateLiveState()
        {
            GameState state = CreateState();
            GameStateNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(state);

            StructuredEffectTemplateResult result =
                StructuredEffectTemplate.Preview(
                    state,
                    0,
                    new StructuredAbilityEffect { type = "soul_charge", amount = 1 });

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(result.preview_only);
            Assert.AreEqual(GameZone.Soul, result.events[0].to_zone);
            Assert.IsTrue(snapshot.Matches(state));
            Assert.AreEqual(1, state.GetPlayer(0).deck.Count);
            Assert.AreEqual(0, state.GetPlayer(0).soul.Count);
        }

        [Test]
        public void SoulBlastMovesSoulToDropThroughRulesCoreEventPath()
        {
            GameState state = CreateState();
            state.GetPlayer(0).soul.Add(new GameCardInstance("p0-soul-1", "P0-SOUL-1", 0, true));

            StructuredEffectTemplateResult result =
                StructuredEffectTemplate.Apply(
                    state,
                    0,
                    new StructuredAbilityEffect { type = "soul_blast", amount = 1 });

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(1, result.events.Count);
            Assert.AreEqual(GameActionType.MoveCard, result.events[0].action_type);
            Assert.AreEqual(GameZone.Soul, result.events[0].from_zone);
            Assert.AreEqual(GameZone.Drop, result.events[0].to_zone);
            Assert.AreEqual(0, state.GetPlayer(0).soul.Count);
            Assert.AreEqual("p0-soul-1", state.GetPlayer(0).drop[0].instance_id);
        }

        [Test]
        public void SoulBlastRejectsWithoutMutationWhenSoulIsEmpty()
        {
            GameState state = CreateState();
            GameStateNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(state);

            StructuredEffectTemplateResult result =
                StructuredEffectTemplate.Apply(
                    state,
                    0,
                    new StructuredAbilityEffect { type = "soul_blast", amount = 1 });

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(StructuredEffectTemplateRejectionReasons.NoLegalCommand, result.rejection_reason);
            Assert.IsFalse(result.requires_manual_resolution);
            Assert.IsTrue(snapshot.Matches(state));
        }

        [Test]
        public void UnsupportedEffectRequiresManualResolution()
        {
            StructuredEffectTemplateResult manual =
                StructuredEffectTemplate.Apply(CreateState(), 0, new StructuredAbilityEffect { type = "manual" });
            StructuredEffectTemplateResult unknown =
                StructuredEffectTemplate.Apply(CreateState(), 0, new StructuredAbilityEffect { type = "power_plus" });

            Assert.IsFalse(manual.accepted);
            Assert.IsTrue(manual.requires_manual_resolution);
            Assert.IsFalse(unknown.accepted);
            Assert.IsTrue(unknown.requires_manual_resolution);
            Assert.IsTrue(unknown.rejection_reason.StartsWith(StructuredEffectTemplateRejectionReasons.UnsupportedEffectType));
        }

        [Test]
        public void InvalidMoveRejectsWithoutMutation()
        {
            GameState state = CreateState();
            GameStateNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(state);

            StructuredEffectTemplateResult result =
                StructuredEffectTemplate.Apply(
                    state,
                    0,
                    new StructuredAbilityEffect
                    {
                        type = "move_zone",
                        amount = 1,
                        from_zone = "Damage",
                        to_zone = "Vanguard"
                    });

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(StructuredEffectTemplateRejectionReasons.NoLegalCommand, result.rejection_reason);
            Assert.IsTrue(snapshot.Matches(state));
        }

        [Test]
        public void EffectResultRoundTripsJson()
        {
            StructuredEffectTemplateResult result =
                StructuredEffectTemplate.Preview(CreateState(), 0, new StructuredAbilityEffect { type = "draw", amount = 1 });

            StructuredEffectTemplateResult roundTrip =
                StructuredEffectTemplateResult.FromJson(result.ToJson(false));

            Assert.AreEqual(result.accepted, roundTrip.accepted);
            Assert.AreEqual(result.preview_only, roundTrip.preview_only);
            Assert.AreEqual(result.events.Count, roundTrip.events.Count);
            Assert.AreEqual(result.events[0].action_type, roundTrip.events[0].action_type);
        }

        private static GameState CreateState()
        {
            return new GameState
            {
                game_id = "structured-effect-template",
                format = "D",
                random_seed = 444,
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
                            new GameCardInstance("p0-deck-1", "P0-DECK-1", 0, true)
                        },
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-hand-1", "P0-HAND-1", 0, true)
                        },
                        damage = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-damage-1", "P0-DAMAGE-1", 0, true)
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
