using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class StructuredTargetTemplateTests
    {
        [Test]
        public void ResolvesSelfRearGuardVisibleUnit()
        {
            GameState state = CreateState();

            StructuredTargetTemplateResult result =
                StructuredTargetTemplate.Resolve(state, 0, Target("unit", "self", "RearGuard", 1));

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(1, result.candidates.Count);
            Assert.AreEqual("p0-rg-1", result.candidates[0].instance_id);
            Assert.AreEqual("P0-RG-1", result.candidates[0].card_id);
        }

        [Test]
        public void ResolvesAnyOwnerPublicZoneAndSkipsFaceDownCards()
        {
            GameState state = CreateState();

            StructuredTargetTemplateResult result =
                StructuredTargetTemplate.Resolve(state, 0, Target("unit", "any", "RearGuard", 3, optional: true));

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(2, result.candidates.Count);
            Assert.AreEqual("p0-rg-1", result.candidates[0].instance_id);
            Assert.AreEqual("p1-rg-1", result.candidates[1].instance_id);
        }

        [Test]
        public void OptionalTargetAcceptsFewerCandidatesButRequiredTargetRejects()
        {
            GameState state = CreateState();

            StructuredTargetTemplateResult optional =
                StructuredTargetTemplate.Resolve(state, 0, Target("unit", "self", "RearGuard", 2, optional: true));
            StructuredTargetTemplateResult required =
                StructuredTargetTemplate.Resolve(state, 0, Target("unit", "self", "RearGuard", 2, optional: false));

            Assert.IsTrue(optional.accepted, optional.rejection_reason);
            Assert.AreEqual(1, optional.candidates.Count);
            Assert.IsFalse(required.accepted);
            Assert.AreEqual(StructuredTargetTemplateRejectionReasons.TargetCountUnavailable, required.rejection_reason);
            Assert.AreEqual(1, required.candidates.Count);
        }

        [Test]
        public void HiddenOrUnsupportedZonesRequireManualResolution()
        {
            GameState state = CreateState();

            StructuredTargetTemplateResult deck =
                StructuredTargetTemplate.Resolve(state, 0, Target("card", "self", "Deck", 1));
            StructuredTargetTemplateResult soul =
                StructuredTargetTemplate.Resolve(state, 0, Target("card", "self", "Soul", 1));
            StructuredTargetTemplateResult opponentHand =
                StructuredTargetTemplate.Resolve(state, 0, Target("card", "opponent", "Hand", 1));

            Assert.IsFalse(deck.accepted);
            Assert.IsTrue(deck.requires_manual_resolution);
            Assert.IsTrue(deck.rejection_reason.StartsWith(StructuredTargetTemplateRejectionReasons.HiddenZone));
            Assert.IsFalse(soul.accepted);
            Assert.IsTrue(soul.requires_manual_resolution);
            Assert.IsFalse(opponentHand.accepted);
            Assert.IsTrue(opponentHand.requires_manual_resolution);
        }

        [Test]
        public void CircleTargetRequiresManualResolution()
        {
            StructuredTargetTemplateResult result =
                StructuredTargetTemplate.Resolve(CreateState(), 0, Target("circle", "self", "RearGuard", 1));

            Assert.IsFalse(result.accepted);
            Assert.IsTrue(result.requires_manual_resolution);
            Assert.IsTrue(result.rejection_reason.StartsWith(StructuredTargetTemplateRejectionReasons.UnsupportedTargetType));
        }

        [Test]
        public void ResolveDoesNotMutateStateAndResultRoundTripsJson()
        {
            GameState state = CreateState();
            GameStateNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(state);

            StructuredTargetTemplateResult result =
                StructuredTargetTemplate.Resolve(state, 0, Target("unit", "self", "RearGuard", 1));
            result.candidates[0].instance_id = "mutated";
            StructuredTargetTemplateResult roundTrip =
                StructuredTargetTemplateResult.FromJson(result.ToJson(false));

            Assert.IsTrue(snapshot.Matches(state));
            Assert.AreEqual("mutated", roundTrip.candidates[0].instance_id);
            Assert.AreEqual(result.accepted, roundTrip.accepted);
        }

        private static StructuredAbilityTarget Target(
            string type,
            string owner,
            string zone,
            int count,
            bool optional = false)
        {
            return new StructuredAbilityTarget
            {
                id = "target",
                type = type,
                owner = owner,
                zone = zone,
                count = count,
                optional = optional,
                filters = new List<string>()
            };
        }

        private static GameState CreateState()
        {
            return new GameState
            {
                game_id = "structured-target-template",
                format = "D",
                random_seed = 333,
                turn_number = 1,
                turn_player_index = 0,
                phase = GamePhase.Main,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p0",
                        rear_guard = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-rg-1", "P0-RG-1", 0, true),
                            new GameCardInstance("p0-rg-hidden", "P0-RG-HIDDEN", 0, false)
                        },
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-hand-1", "P0-HAND-1", 0, true)
                        },
                        deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-deck-1", "P0-DECK-1", 0, true)
                        }
                    },
                    new PlayerGameState
                    {
                        player_id = "p1",
                        rear_guard = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-rg-1", "P1-RG-1", 1, true)
                        },
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-hand-1", "P1-HAND-1", 1, true)
                        }
                    }
                },
                event_log = new List<GameEvent>(),
                pending_auto_abilities = new PendingAutoAbilityQueue()
            };
        }
    }
}
