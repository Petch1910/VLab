using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Game;

namespace VanguardThaiSim.Tests
{
    public sealed class RuntimeAbilityRegistryTests
    {
        [Test]
        public void LoadValidPackIndexesByCardAndAbilityId()
        {
            RuntimeAbilityRegistryLoadResult result =
                RuntimeAbilityRegistry.LoadFromJson(CreatePackJson());

            Assert.IsTrue(result.accepted, result.rejection_reason);
            Assert.AreEqual(2, result.ability_count);
            Assert.AreEqual(1, result.card_count);
            Assert.NotNull(result.registry);

            StructuredAbility first = result.registry.FindAbility("ability-a");
            List<StructuredAbility> cardAbilities = result.registry.GetAbilitiesForCard("BT01-001TH");

            Assert.NotNull(first);
            Assert.AreEqual("BT01-001TH", first.card_id);
            Assert.IsTrue(first.manual_fallback);
            Assert.AreEqual("counter_blast", first.costs[0].type);
            Assert.AreEqual(2, cardAbilities.Count);
            Assert.AreEqual("ability-a", cardAbilities[0].ability_id);
            Assert.AreEqual("ability-b", cardAbilities[1].ability_id);
        }

        [Test]
        public void ReturnedAbilitiesAreClones()
        {
            RuntimeAbilityRegistryLoadResult result =
                RuntimeAbilityRegistry.LoadFromJson(CreatePackJson());

            StructuredAbility first = result.registry.FindAbility("ability-a");
            first.label = "mutated";
            first.costs[0].amount = 99;
            List<StructuredAbility> cardAbilities = result.registry.GetAbilitiesForCard("BT01-001TH");
            cardAbilities[0].label = "mutated-card-list";

            StructuredAbility again = result.registry.FindAbility("ability-a");
            List<StructuredAbility> cardAbilitiesAgain = result.registry.GetAbilitiesForCard("BT01-001TH");

            Assert.AreEqual("Ability A", again.label);
            Assert.AreEqual(1, again.costs[0].amount);
            Assert.AreEqual("Ability A", cardAbilitiesAgain[0].label);
        }

        [Test]
        public void DuplicateAbilityIdIsRejected()
        {
            string json = CreatePackJson().Replace("\"ability-b\"", "\"ability-a\"");

            RuntimeAbilityRegistryLoadResult result = RuntimeAbilityRegistry.LoadFromJson(json);

            Assert.IsFalse(result.accepted);
            Assert.AreEqual(RuntimeAbilityRegistryRejectionReasons.DuplicateAbilityId, result.rejection_reason);
        }

        [Test]
        public void MissingJsonOrWrongSchemaRejects()
        {
            RuntimeAbilityRegistryLoadResult missing = RuntimeAbilityRegistry.LoadFromJson("");
            RuntimeAbilityRegistryLoadResult wrongSchema =
                RuntimeAbilityRegistry.LoadFromJson(CreatePackJson().Replace("ability_schema_v1", "wrong"));

            Assert.IsFalse(missing.accepted);
            Assert.AreEqual(RuntimeAbilityRegistryRejectionReasons.JsonMissing, missing.rejection_reason);
            Assert.IsFalse(wrongSchema.accepted);
            Assert.AreEqual(RuntimeAbilityRegistryRejectionReasons.SchemaVersionMismatch, wrongSchema.rejection_reason);
        }

        [Test]
        public void LoadResultRoundTripsJsonWithoutRegistryPayload()
        {
            RuntimeAbilityRegistryLoadResult result =
                RuntimeAbilityRegistry.LoadFromJson(CreatePackJson());

            RuntimeAbilityRegistryLoadResult roundTrip =
                RuntimeAbilityRegistryLoadResult.FromJson(result.ToJson(false));

            Assert.AreEqual(result.accepted, roundTrip.accepted);
            Assert.AreEqual(result.ability_count, roundTrip.ability_count);
            Assert.AreEqual(result.card_count, roundTrip.card_count);
            Assert.IsNull(roundTrip.registry);
        }

        private static string CreatePackJson()
        {
            return @"{
  ""schema_version"": ""ability_schema_v1"",
  ""abilities"": [
    {
      ""ability_id"": ""ability-a"",
      ""card_id"": ""BT01-001TH"",
      ""label"": ""Ability A"",
      ""kind"": ""auto"",
      ""trigger"": {
        ""type"": ""on_timing"",
        ""timing_window"": ""OnMoveCard"",
        ""event_type"": ""placed"",
        ""source_zone"": ""RearGuard"",
        ""condition"": ""sample""
      },
      ""timing"": {
        ""phase"": ""Main"",
        ""window"": ""OnMoveCard"",
        ""optional"": true
      },
      ""costs"": [
        {
          ""type"": ""counter_blast"",
          ""amount"": 1
        }
      ],
      ""targets"": [
        {
          ""id"": ""self_unit"",
          ""type"": ""self"",
          ""owner"": ""self"",
          ""zone"": ""RearGuard"",
          ""count"": 1,
          ""filters"": [],
          ""optional"": false
        }
      ],
      ""effects"": [
        {
          ""type"": ""draw"",
          ""amount"": 1,
          ""target_ref"": ""self_player""
        }
      ],
      ""duration"": {
        ""type"": ""instant"",
        ""cleanup_timing"": ""none""
      },
      ""manual_fallback"": true,
      ""notes"": ""sample""
    },
    {
      ""ability_id"": ""ability-b"",
      ""card_id"": ""BT01-001TH"",
      ""label"": ""Ability B"",
      ""kind"": ""manual"",
      ""trigger"": {
        ""type"": ""manual""
      },
      ""timing"": {
        ""phase"": ""Main"",
        ""window"": ""PendingAutoResolution"",
        ""optional"": true
      },
      ""costs"": [],
      ""targets"": [],
      ""effects"": [
        {
          ""type"": ""manual""
        }
      ],
      ""duration"": {
        ""type"": ""manual"",
        ""cleanup_timing"": ""manual""
      },
      ""manual_fallback"": true
    }
  ]
}";
        }
    }
}
