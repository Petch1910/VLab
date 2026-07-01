using NUnit.Framework;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Game;
using VanguardThaiSim.Headless;

namespace VanguardThaiSim.Tests
{
    public sealed class HeadlessObservationActionRewardApiTests
    {
        [Test]
        public void ObservationUsesPlayerViewAndSanitizedActionMask()
        {
            GameState state = CreateGameState();
            string before = state.ToJson(false);

            HeadlessObservation observation =
                HeadlessObservationActionRewardApi.CreateObservation(state, 0, "D", 1706);

            Assert.AreEqual(before, state.ToJson(false));
            Assert.AreEqual(1, observation.schema_version);
            Assert.AreEqual("obs-s1706-e0000-p0", observation.observation_id);
            Assert.AreEqual(0, observation.player_index);
            Assert.AreEqual("Mulligan", observation.phase);
            Assert.AreEqual(5, observation.player_hand_count);
            Assert.AreEqual(5, observation.opponent_hand_count);
            Assert.Greater(observation.legal_action_count, 0);
            Assert.AreEqual(observation.legal_action_count, observation.legal_actions.Count);

            string json = observation.ToJson(false);
            StringAssert.DoesNotContain("card_instance", json);
            StringAssert.DoesNotContain("P1-CARD", json);
            StringAssert.DoesNotContain("P2-CARD", json);
            StringAssert.Contains("\"requires_card_selection\":true", json);
        }

        [Test]
        public void ObservationIsDeterministicForSameState()
        {
            GameState state = CreateGameState();

            HeadlessObservation first =
                HeadlessObservationActionRewardApi.CreateObservation(state, 0, "D", 1706);
            HeadlessObservation second =
                HeadlessObservationActionRewardApi.CreateObservation(state, 0, "D", 1706);

            Assert.AreEqual(first.ToJson(false), second.ToJson(false));
            Assert.AreEqual(first.legal_actions[0].action_id, second.legal_actions[0].action_id);
        }

        [Test]
        public void RewardUsesSmokeAcceptanceModelOnly()
        {
            HeadlessRewardSignal accepted = HeadlessObservationActionRewardApi.CreateReward(new HeadlessSimulationResult
            {
                accepted = true
            });
            HeadlessRewardSignal rejected = HeadlessObservationActionRewardApi.CreateReward(new HeadlessSimulationResult
            {
                accepted = false,
                failure_reason = "blocked"
            });

            Assert.AreEqual("m17_06_smoke_acceptance_v1", accepted.reward_model);
            Assert.IsFalse(accepted.terminal);
            Assert.AreEqual(0f, accepted.reward);
            Assert.AreEqual("non_terminal_no_match_result", accepted.reason);

            Assert.IsTrue(rejected.terminal);
            Assert.AreEqual(-1f, rejected.reward);
            Assert.AreEqual("blocked", rejected.reason);
        }

        [Test]
        public void SampleCombinesObservationAndRewardWithoutSourceMutation()
        {
            GameState state = CreateGameState();
            string before = state.ToJson(false);

            HeadlessObservationActionRewardSample sample =
                HeadlessObservationActionRewardApi.CreateSample(
                    state,
                    0,
                    "D",
                    1706,
                    new HeadlessSimulationResult { accepted = true });

            Assert.AreEqual(before, state.ToJson(false));
            Assert.AreEqual(1, sample.schema_version);
            Assert.NotNull(sample.observation);
            Assert.NotNull(sample.reward);
            StringAssert.DoesNotContain("card_instance", sample.ToJson(false));
            StringAssert.DoesNotContain("P2-CARD", sample.ToJson(false));
        }

        private static GameState CreateGameState()
        {
            VanguardDeck p1 = CreateDeck("P1");
            VanguardDeck p2 = CreateDeck("P2");
            return GameStateFactory.CreateTwoPlayerGame(p1, p2, 1706, "p1", "p2");
        }

        private static VanguardDeck CreateDeck(string prefix)
        {
            VanguardDeck deck = VanguardDeck.Create(prefix + " Deck", "D", "test-pack", "test-source");
            for (int i = 0; i < 50; i++)
            {
                deck.AddCard(DeckZone.Main, prefix + "-CARD-" + i.ToString("D3"), 1);
            }

            for (int i = 0; i < 4; i++)
            {
                deck.AddCard(DeckZone.Ride, prefix + "-RIDE-" + i.ToString("D3"), 1);
            }

            return deck;
        }
    }
}
