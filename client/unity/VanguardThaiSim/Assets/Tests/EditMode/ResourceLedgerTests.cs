using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class ResourceLedgerTests
    {
        [Test]
        public void LedgerDerivesCounterBlastFromFaceUpDamageAndAcceptsAvailableCosts()
        {
            GameState state = CreateState();
            GameStateNoMutationSnapshot stateSnapshot = NoMutationSnapshot.Capture(state);
            ResourceLedgerState ledger = ResourceLedgerState.FromGameState(state, 0, availableSoul: 2, availableEnergy: 5);
            string ledgerBefore = ledger.ToJson(false);

            ResourceCostRequest request = new ResourceCostRequest
            {
                player_index = 0,
                ability_key = "BT01-001TH:auto",
                counter_blast = 2,
                soul_blast = 1,
                energy_blast = 3,
                once_per_turn_key = ResourceLedger.BuildOncePerTurnKey("BT01-001TH:auto", 3),
                once_per_fight_key = ResourceLedger.BuildOncePerFightKey("BT01-001TH:divine")
            };

            ResourceLedgerValidationResult result = ResourceLedger.ValidateCost(ledger, request);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(2, result.before_state.available_counter_blast);
            Assert.AreEqual(0, result.after_state.available_counter_blast);
            Assert.AreEqual(1, result.after_state.available_soul);
            Assert.AreEqual(2, result.after_state.available_energy);
            Assert.Contains(request.once_per_turn_key, result.after_state.used_once_per_turn_keys);
            Assert.Contains(request.once_per_fight_key, result.after_state.used_once_per_fight_keys);
            Assert.AreEqual(ledgerBefore, ledger.ToJson(false));
            Assert.IsTrue(stateSnapshot.Matches(state));
        }

        [Test]
        public void LedgerRejectsUnavailableCostsWithoutMutatingLedger()
        {
            ResourceLedgerState ledger = new ResourceLedgerState
            {
                player_index = 0,
                available_counter_blast = 1,
                available_soul = 0,
                available_energy = 2
            };
            string before = ledger.ToJson(false);

            ResourceLedgerValidationResult counterBlast =
                ResourceLedger.ValidateCost(ledger, Request(counterBlast: 2, soulBlast: 0, energyBlast: 0));
            ResourceLedgerValidationResult soulBlast =
                ResourceLedger.ValidateCost(ledger, Request(counterBlast: 0, soulBlast: 1, energyBlast: 0));
            ResourceLedgerValidationResult energyBlast =
                ResourceLedger.ValidateCost(ledger, Request(counterBlast: 0, soulBlast: 0, energyBlast: 3));

            AssertRejected(counterBlast, ResourceLedgerRejectionReasons.CounterBlastUnavailable);
            AssertRejected(soulBlast, ResourceLedgerRejectionReasons.SoulBlastUnavailable);
            AssertRejected(energyBlast, ResourceLedgerRejectionReasons.EnergyBlastUnavailable);
            Assert.AreEqual(before, ledger.ToJson(false));
            Assert.AreEqual(before, counterBlast.after_state.ToJson(false));
        }

        [Test]
        public void LedgerDerivesSoulFromGameStateWhenAvailableSoulIsNotProvided()
        {
            GameState state = CreateState();
            state.GetPlayer(0).soul.Add(new GameCardInstance("p0-soul-1", "SOUL-1", 0, true));
            state.GetPlayer(0).soul.Add(new GameCardInstance("p0-soul-2", "SOUL-2", 0, false));

            ResourceLedgerState ledger = ResourceLedgerState.FromGameState(state, 0, availableEnergy: 5);

            Assert.AreEqual(2, ledger.available_soul);
        }

        [Test]
        public void ExplicitAvailableSoulCanOverrideGameStateSoulForOfflinePreviews()
        {
            GameState state = CreateState();
            state.GetPlayer(0).soul.Add(new GameCardInstance("p0-soul-1", "SOUL-1", 0, true));

            ResourceLedgerState ledger = ResourceLedgerState.FromGameState(state, 0, availableSoul: 4);

            Assert.AreEqual(4, ledger.available_soul);
        }

        [Test]
        public void GameStateSoulCanPaySoulBlastCost()
        {
            GameState state = CreateState();
            state.GetPlayer(0).soul.Add(new GameCardInstance("p0-soul-1", "SOUL-1", 0, true));
            ResourceLedgerState ledger = ResourceLedgerState.FromGameState(state, 0);

            ResourceLedgerValidationResult result =
                ResourceLedger.ValidateCost(ledger, Request(counterBlast: 0, soulBlast: 1, energyBlast: 0));

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(0, result.after_state.available_soul);
        }


        [Test]
        public void LedgerRejectsDuplicateOnceFlags()
        {
            string turnKey = ResourceLedger.BuildOncePerTurnKey("ability-a", 9);
            string fightKey = ResourceLedger.BuildOncePerFightKey("ability-b");
            ResourceLedgerState ledger = new ResourceLedgerState
            {
                player_index = 0,
                available_counter_blast = 3,
                available_soul = 3,
                available_energy = 3,
                used_once_per_turn_keys = new List<string> { turnKey },
                used_once_per_fight_keys = new List<string> { fightKey }
            };

            ResourceLedgerValidationResult oncePerTurn =
                ResourceLedger.ValidateCost(ledger, new ResourceCostRequest
                {
                    player_index = 0,
                    once_per_turn_key = turnKey
                });
            ResourceLedgerValidationResult oncePerFight =
                ResourceLedger.ValidateCost(ledger, new ResourceCostRequest
                {
                    player_index = 0,
                    once_per_fight_key = fightKey
                });

            AssertRejected(oncePerTurn, ResourceLedgerRejectionReasons.OncePerTurnUsed);
            AssertRejected(oncePerFight, ResourceLedgerRejectionReasons.OncePerFightUsed);
        }

        [Test]
        public void LedgerRejectsNegativeCostAndPlayerMismatch()
        {
            ResourceLedgerState ledger = new ResourceLedgerState
            {
                player_index = 0,
                available_counter_blast = 2,
                available_soul = 2,
                available_energy = 2
            };

            ResourceLedgerValidationResult negative =
                ResourceLedger.ValidateCost(ledger, Request(counterBlast: -1, soulBlast: 0, energyBlast: 0));
            ResourceLedgerValidationResult mismatch =
                ResourceLedger.ValidateCost(ledger, new ResourceCostRequest { player_index = 1 });

            AssertRejected(negative, ResourceLedgerRejectionReasons.NegativeCost);
            AssertRejected(mismatch, ResourceLedgerRejectionReasons.PlayerMismatch);
        }

        [Test]
        public void LedgerRejectsMissingInputs()
        {
            ResourceLedgerState ledger = new ResourceLedgerState { player_index = 0 };

            ResourceLedgerValidationResult missingLedger =
                ResourceLedger.ValidateCost(null, new ResourceCostRequest { player_index = 0 });
            ResourceLedgerValidationResult missingRequest =
                ResourceLedger.ValidateCost(ledger, null);

            AssertRejected(missingLedger, ResourceLedgerRejectionReasons.LedgerMissing);
            AssertRejected(missingRequest, ResourceLedgerRejectionReasons.RequestMissing);
        }

        [Test]
        public void LedgerStateAndResultRoundTripJson()
        {
            ResourceLedgerState ledger = ResourceLedgerState.FromGameState(
                CreateState(),
                0,
                availableSoul: 4,
                availableEnergy: 6,
                usedOncePerTurnKeys: new[] { "ability-x|turn|1" },
                usedOncePerFightKeys: new[] { "ability-y|fight" });

            ResourceLedgerState ledgerRoundTrip = ResourceLedgerState.FromJson(ledger.ToJson(false));
            ResourceLedgerValidationResult result =
                ResourceLedger.ValidateCost(ledger, Request(counterBlast: 1, soulBlast: 1, energyBlast: 1));
            ResourceLedgerValidationResult resultRoundTrip =
                ResourceLedgerValidationResult.FromJson(result.ToJson(false));

            Assert.AreEqual(ledger.available_counter_blast, ledgerRoundTrip.available_counter_blast);
            Assert.AreEqual(ledger.available_soul, ledgerRoundTrip.available_soul);
            Assert.AreEqual(ledger.used_once_per_turn_keys.Count, ledgerRoundTrip.used_once_per_turn_keys.Count);
            Assert.AreEqual(result.accepted, resultRoundTrip.accepted);
            Assert.AreEqual(result.after_state.available_counter_blast, resultRoundTrip.after_state.available_counter_blast);
            Assert.AreEqual(result.request.counter_blast, resultRoundTrip.request.counter_blast);
        }

        private static ResourceCostRequest Request(int counterBlast, int soulBlast, int energyBlast)
        {
            return new ResourceCostRequest
            {
                player_index = 0,
                counter_blast = counterBlast,
                soul_blast = soulBlast,
                energy_blast = energyBlast
            };
        }

        private static void AssertRejected(ResourceLedgerValidationResult result, string expectedReason)
        {
            Assert.IsFalse(result.accepted);
            Assert.AreEqual(expectedReason, result.rejection_reason);
        }

        private static GameState CreateState()
        {
            return new GameState
            {
                game_id = "resource-ledger-game",
                format = "D",
                random_seed = 111,
                turn_number = 3,
                turn_player_index = 0,
                phase = GamePhase.Main,
                players = new List<PlayerGameState>
                {
                    new PlayerGameState
                    {
                        player_id = "p0",
                        damage = new List<GameCardInstance>
                        {
                            new GameCardInstance("p0-damage-1", "DAMAGE-1", 0, true),
                            new GameCardInstance("p0-damage-2", "DAMAGE-2", 0, true),
                            new GameCardInstance("p0-damage-3", "DAMAGE-3", 0, false)
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
