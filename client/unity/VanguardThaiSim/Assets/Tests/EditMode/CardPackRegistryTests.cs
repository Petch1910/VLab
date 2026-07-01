using NUnit.Framework;
using VanguardThaiSim.Cards;

namespace VanguardThaiSim.Tests
{
    public sealed class CardPackRegistryTests
    {
        [Test]
        public void CreateEntryCopiesManifestAndValidationMetadata()
        {
            CardPackManifest manifest = Manifest("pack_a", "1.0.0");
            CardPackValidationStatus status = CardPackValidationStatusBuilder.FromManifest(
                manifest,
                true,
                true);

            CardPackRegistryEntry entry = CardPackRegistryManager.CreateEntry(
                manifest,
                "data/packs/pack_a",
                status,
                true);

            Assert.AreEqual("pack_a", entry.pack_id);
            Assert.AreEqual("1.0.0", entry.source_version);
            Assert.AreEqual("data/packs/pack_a", entry.pack_directory);
            Assert.IsTrue(entry.enabled);
            StringAssert.Contains("Pack validation: OK", entry.validation_summary);
        }

        [Test]
        public void SetEnabledReturnsClonedStateWithoutMutatingSource()
        {
            CardPackRegistryState state = StateWithPack(true);

            CardPackRegistryMutationResult result =
                CardPackRegistryManager.SetEnabled(state, "pack_a", "1.0.0", false);

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.IsTrue(state.packs[0].enabled);
            Assert.IsFalse(result.state.packs[0].enabled);
            Assert.AreEqual("data/packs/pack_a", result.state.packs[0].pack_directory);
        }

        [Test]
        public void SetEnabledRejectsMissingPackWithoutMutatingSource()
        {
            CardPackRegistryState state = StateWithPack(true);

            CardPackRegistryMutationResult result =
                CardPackRegistryManager.SetEnabled(state, "missing", "1.0.0", false);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(CardPackRegistryRejectionReasons.PackMissing, result.rejection_reason);
            Assert.IsTrue(state.packs[0].enabled);
            Assert.IsTrue(result.state.packs[0].enabled);
        }

        [Test]
        public void RegistryJsonRoundTrips()
        {
            CardPackRegistryState state = StateWithPack(false);

            CardPackRegistryState roundTrip = CardPackRegistryState.FromJson(state.ToJson(false));

            Assert.AreEqual(1, roundTrip.packs.Count);
            Assert.AreEqual("pack_a", roundTrip.packs[0].pack_id);
            Assert.IsFalse(roundTrip.packs[0].enabled);
        }

        private static CardPackRegistryState StateWithPack(bool enabled)
        {
            CardPackRegistryState state = new CardPackRegistryState();
            state.packs.Add(CardPackRegistryManager.CreateEntry(
                Manifest("pack_a", "1.0.0"),
                "data/packs/pack_a",
                CardPackValidationStatusBuilder.FromManifest(Manifest("pack_a", "1.0.0"), true, true),
                enabled));
            return state;
        }

        private static CardPackManifest Manifest(string packId, string sourceVersion)
        {
            return new CardPackManifest
            {
                pack_id = packId,
                display_name = "Pack A",
                source_version = sourceVersion,
                schema_version = 1,
                card_count = 1,
                definition_hash = "definition-hash",
                image_manifest_hash = "image-hash"
            };
        }
    }
}
