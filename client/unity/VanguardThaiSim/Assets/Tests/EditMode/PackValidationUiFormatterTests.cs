using System.Collections.Generic;
using NUnit.Framework;
using VanguardThaiSim.Cards;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class PackValidationUiFormatterTests
    {
        [Test]
        public void RuntimePackStatusShowsCountsAndBoundaryNote()
        {
            string formatted = PackValidationUiFormatter.FormatRuntimePack(
                Manifest(),
                AcceptedStatus());

            StringAssert.Contains("Pack Check: OK", formatted);
            StringAssert.Contains("vanguard_th", formatted);
            StringAssert.Contains("source schema 2", formatted);
            StringAssert.Contains("Sets 251", formatted);
            StringAssert.Contains("Cards 10836", formatted);
            StringAssert.Contains("Missing images 36", formatted);
            StringAssert.Contains("Abilities 20", formatted);
            StringAssert.Contains("no third-party auto-download", formatted);
        }

        [Test]
        public void LocalImportReportFormatsWarningsAndUnsupportedFields()
        {
            LocalCustomImportValidationReport report = new LocalCustomImportValidationReport
            {
                adapter = "vangpro_like_local_import_v1",
                accepted = false,
                pack_id = "local_custom_pack",
                card_count = 2,
                image_count = 1,
                missing_image_count = 1,
                unsupported_field_count = 3,
                abilities_file = "abilities.json",
                errors = new List<string> { "hash mismatch\nfor cards.csv" },
                warnings = new List<string> { "missing image" }
            };

            string formatted = PackValidationUiFormatter.FormatLocalImportReport(report);

            StringAssert.Contains("Pack Check: BLOCKED", formatted);
            StringAssert.Contains("local_custom_pack", formatted);
            StringAssert.Contains("Cards 2", formatted);
            StringAssert.Contains("Unsupported fields 3", formatted);
            StringAssert.Contains("Error: hash mismatch for cards.csv", formatted);
            StringAssert.Contains("Warning: missing image", formatted);
            StringAssert.Contains("runtime pack mutation", formatted);
        }

        [Test]
        public void DeckToolsInputParsesLocalValidatorJson()
        {
            string json = "{\"adapter\":\"vangpro_like_local_import_v1\",\"accepted\":true,\"pack_id\":\"local_custom_pack\",\"card_count\":2,\"image_count\":2,\"missing_image_count\":0,\"unsupported_field_count\":0,\"errors\":[],\"warnings\":[]}";

            string formatted = PackValidationUiFormatter.FormatFromDeckToolsInput(
                json,
                Manifest(),
                AcceptedStatus());

            StringAssert.Contains("local_custom_pack", formatted);
            StringAssert.Contains("adapter vangpro_like_local_import_v1", formatted);
            Assert.IsFalse(formatted.Contains("vanguard_th | runtime"));
        }

        [Test]
        public void InvalidDeckToolsInputFallsBackToRuntimePack()
        {
            string formatted = PackValidationUiFormatter.FormatFromDeckToolsInput(
                "not json",
                Manifest(),
                AcceptedStatus());

            StringAssert.Contains("vanguard_th", formatted);
            StringAssert.Contains("runtime schema", formatted);
        }

        [Test]
        public void LocalReportBoundsMessages()
        {
            LocalCustomImportValidationReport report = new LocalCustomImportValidationReport
            {
                adapter = "vangpro_like_local_import_v1",
                accepted = false,
                pack_id = "local_custom_pack",
                errors = new List<string> { "e1", "e2", "e3" },
                warnings = new List<string> { "w1", "w2", "w3" }
            };

            string formatted = PackValidationUiFormatter.FormatLocalImportReport(report, 2);

            StringAssert.Contains("Error: e1", formatted);
            StringAssert.Contains("Error: e2", formatted);
            StringAssert.Contains("+1 more errors", formatted);
            StringAssert.Contains("+1 more warnings", formatted);
            Assert.IsFalse(formatted.Contains("Error: e3"));
            Assert.IsFalse(formatted.Contains("Warning: w3"));
        }

        private static CardPackManifest Manifest()
        {
            return new CardPackManifest
            {
                pack_id = "vanguard_th",
                schema_version = 1,
                source_schema_version = 2,
                source_version = "2026.06",
                card_count = 10836,
                image_count = 10836,
                existing_image_count = 10800,
                series_count = 251,
                clan_count = 31,
                definition_hash = "hash",
                image_manifest_hash = "image-hash"
            };
        }

        private static CardPackValidationStatus AcceptedStatus()
        {
            return new CardPackValidationStatus
            {
                accepted = true,
                pack_id = "vanguard_th",
                schema_version = 1,
                source_schema_version = 2,
                card_count = 10836,
                existing_image_count = 10800,
                source_ability_count = 20,
                capabilities_summary = "cards,images,abilities",
                issues = new List<CardPackValidationIssue>()
            };
        }
    }
}

