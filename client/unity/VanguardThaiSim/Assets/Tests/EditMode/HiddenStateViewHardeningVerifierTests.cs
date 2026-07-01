using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class HiddenStateViewHardeningVerifierTests
    {
        [Test]
        public void VerifierAcceptsCurrentPlayerAndSpectatorMaskingPolicy()
        {
            GameState state = CreateState();

            HiddenStateViewHardeningVerificationResult result =
                HiddenStateViewHardeningVerifier.Verify(state);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(string.Empty, result.rejection_reason);
            Assert.Greater(result.checked_rule_count, 0);
            Assert.AreEqual(result.checked_rule_count, result.passed_rule_count);
            Assert.AreEqual(0, result.failed_rule_count);
            AssertNoFailedChecks(result);
        }

        [Test]
        public void VerifierDoesNotMutateTrueState()
        {
            GameState state = CreateState();
            GameStateNoMutationSnapshot snapshot = NoMutationSnapshot.Capture(state);

            HiddenStateViewHardeningVerificationResult result =
                HiddenStateViewHardeningVerifier.Verify(state);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(snapshot.Matches(state));
            Assert.AreEqual("p1-hand-open", state.GetPlayer(1).hand[0].instance_id);
            Assert.AreEqual("P1-HAND", state.GetPlayer(1).hand[0].card_id);
        }

        [Test]
        public void DirectViewsMaskPrivateZonesAndEventsAsVerifierRequires()
        {
            GameState state = CreateState();

            GameState playerZero = GameStateViewFactory.CreatePlayerView(state, 0);
            GameState spectator = GameStateViewFactory.CreateSpectatorView(state);

            Assert.AreEqual("P0-HAND", playerZero.GetPlayer(0).hand[0].card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, playerZero.GetPlayer(1).hand[0].card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, playerZero.GetPlayer(0).deck[0].card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, playerZero.GetPlayer(1).deck[0].card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, playerZero.GetPlayer(1).bind[0].card_id);
            Assert.AreEqual("P0-VG", playerZero.GetPlayer(0).vanguard[0].card_id);

            Assert.AreEqual(GameStateViewFactory.HiddenCardId, spectator.GetPlayer(0).hand[0].card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, spectator.GetPlayer(1).hand[0].card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, spectator.GetPlayer(0).ride_deck[0].card_id);
            Assert.AreEqual(GameStateViewFactory.HiddenCardId, spectator.GetPlayer(1).ride_deck[0].card_id);

            Assert.AreNotEqual("p1-hand-open", playerZero.event_log[0].card_instance_id);
            Assert.IsTrue(playerZero.event_log[0].card_instance_id.StartsWith("hidden-"));
            Assert.AreEqual("p0-hand-open", playerZero.event_log[2].card_instance_id);
            Assert.AreNotEqual("p0-hand-open", spectator.event_log[2].card_instance_id);
            Assert.IsTrue(spectator.event_log[2].card_instance_id.StartsWith("hidden-"));
        }

        [Test]
        public void VerifierRejectsMissingStateOrPlayers()
        {
            HiddenStateViewHardeningVerificationResult missingState =
                HiddenStateViewHardeningVerifier.Verify(null);
            HiddenStateViewHardeningVerificationResult missingPlayers =
                HiddenStateViewHardeningVerifier.Verify(new GameState { players = new List<PlayerGameState>() });

            Assert.IsFalse(missingState.accepted);
            Assert.AreEqual(
                HiddenStateViewHardeningRejectionReasons.StateMissing,
                missingState.rejection_reason);
            Assert.IsFalse(missingPlayers.accepted);
            Assert.AreEqual(
                HiddenStateViewHardeningRejectionReasons.PlayersMissing,
                missingPlayers.rejection_reason);
        }

        [Test]
        public void VerificationResultRoundTripsJson()
        {
            HiddenStateViewHardeningVerificationResult result =
                HiddenStateViewHardeningVerifier.Verify(CreateState());

            HiddenStateViewHardeningVerificationResult roundTrip =
                HiddenStateViewHardeningVerificationResult.FromJson(result.ToJson(false));

            Assert.AreEqual(result.accepted, roundTrip.accepted);
            Assert.AreEqual(result.checked_rule_count, roundTrip.checked_rule_count);
            Assert.AreEqual(result.passed_rule_count, roundTrip.passed_rule_count);
            Assert.AreEqual(result.failed_rule_count, roundTrip.failed_rule_count);
            Assert.AreEqual(result.checks.Count, roundTrip.checks.Count);
        }

        private static void AssertNoFailedChecks(HiddenStateViewHardeningVerificationResult result)
        {
            for (int i = 0; i < result.checks.Count; i++)
            {
                Assert.IsTrue(result.checks[i].passed, result.checks[i].check_id + ": " + result.checks[i].message);
            }
        }

        private static GameState CreateState()
        {
            return new GameState
            {
                game_id = "hidden-state-hardening",
                format = "D",
                random_seed = 909,
                turn_number = 3,
                turn_player_index = 0,
                phase = GamePhase.Main,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p0",
                        deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-deck-secret", "P0-DECK", 0)
                        },
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-hand-open", "P0-HAND", 0)
                        },
                        ride_deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-ride-open", "P0-RIDE", 0)
                        },
                        vanguard = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-vg-open", "P0-VG", 0)
                        },
                        bind = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-bind-hidden", "P0-BIND", 0, false)
                        }
                    },
                    new PlayerGameState
                    {
                        player_id = "p1",
                        deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-deck-secret", "P1-DECK", 1)
                        },
                        hand = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-hand-open", "P1-HAND", 1)
                        },
                        ride_deck = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-ride-open", "P1-RIDE", 1)
                        },
                        vanguard = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-vg-open", "P1-VG", 1)
                        },
                        bind = new List<GameCardInstance>
                        {
                            new GameCardInstance("p1-bind-hidden", "P1-BIND", 1, false)
                        }
                    }
                },
                event_log = new List<GameEvent>
                {
                    new GameEvent
                    {
                        event_id = "event-opponent-private-hand",
                        actor_index = 1,
                        card_instance_id = "p1-hand-open",
                        from_zone = GameZone.Hand,
                        to_zone = GameZone.Vanguard
                    },
                    new GameEvent
                    {
                        event_id = "event-opponent-private-deck-missing",
                        actor_index = 1,
                        card_instance_id = "p1-drawn-not-in-current-zone",
                        from_zone = GameZone.Deck,
                        to_zone = GameZone.Hand
                    },
                    new GameEvent
                    {
                        event_id = "event-own-private-hand",
                        actor_index = 0,
                        card_instance_id = "p0-hand-open",
                        from_zone = GameZone.Hand,
                        to_zone = GameZone.Vanguard
                    },
                    new GameEvent
                    {
                        event_id = "event-public-zone",
                        actor_index = 1,
                        card_instance_id = "p1-vg-open",
                        from_zone = GameZone.Vanguard,
                        to_zone = GameZone.RearGuard
                    }
                },
                pending_auto_abilities = new PendingAutoAbilityQueue()
            };
        }
    }
}
