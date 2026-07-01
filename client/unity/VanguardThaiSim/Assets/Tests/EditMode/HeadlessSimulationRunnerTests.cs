using NUnit.Framework;
using System.Collections.Generic;
using VanguardThaiSim.Cards;
using VanguardThaiSim.Decks;
using VanguardThaiSim.Headless;

namespace VanguardThaiSim.Tests
{
    public sealed class HeadlessSimulationRunnerTests
    {
        [Test]
        public void DefaultHeadlessSimulationRunsDeterministicCoreActions()
        {
            HeadlessSimulationResult first = HeadlessSimulationRunner.RunDefault();
            HeadlessSimulationResult second = HeadlessSimulationRunner.RunDefault();

            Assert.IsTrue(first.accepted, first.ToJson(true));
            Assert.IsTrue(second.accepted, second.ToJson(true));
            Assert.AreEqual(HeadlessSimulationRunner.DefaultSeed, first.seed);
            Assert.AreEqual("D", first.ruleset);
            Assert.AreEqual("default", first.deck_source);
            Assert.AreEqual(4, first.actions_executed);
            Assert.AreEqual(4, first.event_count);
            Assert.AreEqual("Main", first.final_phase);
            Assert.AreEqual(2, first.player_count);
            Assert.AreEqual(44, first.player0_deck_count);
            Assert.AreEqual(5, first.player0_hand_count);
            Assert.AreEqual(1, first.player0_rear_guard_count);
            Assert.AreEqual(1, first.player0_protect_markers);
            Assert.AreEqual(first.ToJson(false), second.ToJson(false));
        }

        [Test]
        public void CustomSeedAndRulesetAreApplied()
        {
            HeadlessSimulationResult result = HeadlessSimulationRunner.Run(new HeadlessSimulationRequest
            {
                seed = 42,
                ruleset = "Premium"
            });

            Assert.IsTrue(result.accepted, result.ToJson(true));
            Assert.AreEqual(42, result.seed);
            Assert.AreEqual("Premium", result.ruleset);
            Assert.AreEqual("default", result.deck_source);
        }

        [Test]
        public void DeckCodeRequestUsesDeckCodeSource()
        {
            string deckCode = DeckCodeCodec.Export(CreatePlayableDeckCodeDeck());
            HeadlessSimulationResult result = HeadlessSimulationRunner.Run(new HeadlessSimulationRequest
            {
                seed = 99,
                ruleset = "standard",
                deck_code = deckCode
            });

            Assert.IsTrue(result.accepted, result.ToJson(true));
            Assert.AreEqual(99, result.seed);
            Assert.AreEqual("D", result.ruleset);
            Assert.AreEqual("deck_code", result.deck_source);
        }

        [Test]
        public void InvalidDeckCodeReturnsRejectedResult()
        {
            HeadlessSimulationResult result = HeadlessSimulationRunner.Run(new HeadlessSimulationRequest
            {
                deck_code = "not-a-valid-deck-code"
            });

            Assert.IsFalse(result.accepted);
            Assert.AreEqual("deck_code", result.deck_source);
            StringAssert.Contains("Deck code must start with VGTH1", result.failure_reason);
        }

        [Test]
        public void CliArgumentsParseSeedRulesetDeckCodeAndResultPath()
        {
            HeadlessSimulationCliInput input = HeadlessSimulationCliArguments.Parse(new[]
            {
                "-batchmode",
                HeadlessSimulationCliArguments.SeedArgument,
                "123",
                HeadlessSimulationCliArguments.RulesetArgument,
                "v-premium",
                HeadlessSimulationCliArguments.DeckCodeArgument,
                "VGTH1.example",
                HeadlessSimulationCliArguments.ResultPathArgument,
                "result.json",
                HeadlessSimulationCliArguments.ReplayPathArgument,
                "replay.json"
            });

            Assert.IsTrue(input.IsValid);
            Assert.AreEqual(123, input.request.seed);
            Assert.AreEqual("V", input.request.ruleset);
            Assert.AreEqual("VGTH1.example", input.request.deck_code);
            Assert.AreEqual("result.json", input.result_path);
            Assert.AreEqual("replay.json", input.replay_path);
        }

        [Test]
        public void CliArgumentsRejectInvalidSeed()
        {
            HeadlessSimulationCliInput input = HeadlessSimulationCliArguments.Parse(new[]
            {
                HeadlessSimulationCliArguments.SeedArgument,
                "abc"
            });

            Assert.IsFalse(input.IsValid);
            Assert.AreEqual(1, input.errors.Count);
        }

        [Test]
        public void RunWithReplayCreatesRedactedReplayArtifact()
        {
            HeadlessSimulationOutput output = HeadlessSimulationRunner.RunWithReplay(HeadlessSimulationRequest.Default());

            Assert.IsTrue(output.result.accepted, output.result.ToJson(true));
            Assert.NotNull(output.replay);
            Assert.IsTrue(output.replay.accepted);
            Assert.AreEqual(1, output.replay.schema_version);
            Assert.AreEqual("local_headless_trace_card_instance_ids_redacted", output.replay.hidden_state_policy);
            Assert.AreEqual(4, output.replay.event_count);
            Assert.AreEqual(4, output.replay.events.Count);
            Assert.AreEqual("Draw", output.replay.events[0].action_type);
            Assert.AreEqual("card_instance_id_redacted", output.replay.events[0].card_identity_policy);
            Assert.IsFalse(string.IsNullOrWhiteSpace(output.replay.events[0].event_id));
        }

        private static VanguardDeck CreatePlayableDeckCodeDeck()
        {
            string packDirectory = CardPackFileSystem.DefaultPackDirectory;
            CardPackManifest manifest = CardPackFileSystem.LoadManifest(packDirectory);
            using (SqliteCardRepository repository = new SqliteCardRepository(
                       CardPackFileSystem.GetDatabasePath(packDirectory, manifest)))
            {
                IReadOnlyList<CardSummary> cards = repository.QueryCards(new CardQueryOptions { Limit = 80 });
                VanguardDeck deck = VanguardDeck.Create("Headless Deck Code Test", "D", manifest.pack_id, manifest.source_version);
                for (int i = 0; i < 50; i++)
                {
                    deck.AddCard(DeckZone.Main, cards[i].CardId, 1);
                }

                for (int i = 0; i < 4; i++)
                {
                    deck.AddCard(DeckZone.Ride, cards[50 + i].CardId, 1);
                }

                return deck;
            }
        }
    }
}
