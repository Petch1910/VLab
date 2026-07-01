using System.IO;
using NUnit.Framework;
using VanguardThaiSim.Cards;
using UnityEngine;

namespace VanguardThaiSim.Tests
{
    public sealed class CardRepositoryTests
    {
        [Test]
        public void TestFrameworkIsRunning()
        {
            Assert.Pass("Unity EditMode test runner is active.");
        }

        [Test]
        public void LoadsManifestAndKnownCard()
        {
            string packDirectory = CardPackFileSystem.DefaultPackDirectory;
            CardPackManifest manifest = CardPackFileSystem.LoadManifest(packDirectory);

            Assert.AreEqual("vanguard_th", manifest.pack_id);
            Assert.AreEqual(10836, manifest.card_count);
            Assert.AreEqual(10836, manifest.existing_image_count);
            Assert.AreEqual(206, manifest.series_count);
            Assert.AreEqual(33, manifest.clan_count);
            Assert.AreEqual("asset_index.json", manifest.asset_index_file);
            Assert.IsFalse(string.IsNullOrEmpty(manifest.image_content_hash));
            Assert.AreEqual("content_addressed_external_source", manifest.image_cache_strategy);

            string databasePath = CardPackFileSystem.GetDatabasePath(packDirectory, manifest);
            Assert.IsTrue(File.Exists(databasePath), databasePath);
            Assert.IsTrue(File.Exists(CardPackFileSystem.GetAssetIndexPath(packDirectory, manifest)));
            Assert.IsFalse(CardPackFileSystem.GetPackCacheDirectory(manifest).StartsWith(packDirectory));
            Assert.IsFalse(CardPackFileSystem.GetUserDataRoot().StartsWith(packDirectory));

            using (SqliteCardRepository repository = new SqliteCardRepository(databasePath))
            {
                Assert.AreEqual(10836, repository.CountCards());
                Assert.AreEqual(206, repository.CountSeries());
                Assert.AreEqual(33, repository.CountClans());

                CardDetail alfred = repository.GetCard("BT01-001TH");
                Assert.NotNull(alfred);
                Assert.AreEqual("ราชาแห่งอัศวิน, อัลเฟรด", alfred.NameTh);
                Assert.AreEqual("BT01", alfred.SeriesCode);
                Assert.AreEqual("รอยัล พาลาดิน", alfred.Clan);
                Assert.AreEqual(3, alfred.Grade);
                Assert.AreEqual(10000, alfred.Power);
                Assert.IsTrue(alfred.ImageExists);

                string imagePath = Path.Combine(
                    CardPackFileSystem.GetImageRootPath(manifest),
                    alfred.ImageRelativePath.Replace('/', Path.DirectorySeparatorChar));
                Assert.IsTrue(File.Exists(imagePath), imagePath);
            }
        }

        [Test]
        public void RuntimePackResolverFindsAncestorPackDirectory()
        {
            string tempRoot = Path.Combine(Path.GetTempPath(), "VanguardPackResolverTests", System.Guid.NewGuid().ToString("N"));
            string nestedRoot = Path.Combine(tempRoot, "client", "unity", "build", "windows", "latest");
            string packDirectory = Path.Combine(
                tempRoot,
                CardPackFileSystem.DefaultPackRelativePath.Replace('/', Path.DirectorySeparatorChar));

            try
            {
                Directory.CreateDirectory(nestedRoot);
                Directory.CreateDirectory(packDirectory);
                File.WriteAllText(Path.Combine(packDirectory, "manifest.json"), "{}");

                string resolved = CardPackFileSystem.FindExistingDefaultPackDirectory(new[] { nestedRoot });

                Assert.AreEqual(Path.GetFullPath(packDirectory), Path.GetFullPath(resolved));
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                {
                    Directory.Delete(tempRoot, true);
                }
            }
        }

        [Test]
        public void PackValidationStatusAcceptsDefaultManifest()
        {
            string packDirectory = CardPackFileSystem.DefaultPackDirectory;
            CardPackManifest manifest = CardPackFileSystem.LoadManifest(packDirectory);
            CardPackValidationStatus status = CardPackValidationStatusBuilder.FromManifest(
                manifest,
                File.Exists(CardPackFileSystem.GetDatabasePath(packDirectory, manifest)),
                File.Exists(CardPackFileSystem.GetAssetIndexPath(packDirectory, manifest)));

            Assert.IsTrue(status.accepted, status.ToJson(false));
            Assert.AreEqual("vanguard_th", status.pack_id);
            Assert.AreEqual(1, status.source_schema_version);
            Assert.AreEqual(10836, status.card_count);
            StringAssert.Contains("Pack validation: OK", status.summary);
        }

        [Test]
        public void PackValidationStatusReportsV2CapabilitiesAndAbilityMetadata()
        {
            CardPackManifest manifest = new CardPackManifest
            {
                pack_id = "custom_starter_pack_v2",
                source_version = "1.0.0",
                schema_version = 1,
                source_schema_version = 2,
                card_count = 2,
                existing_image_count = 0,
                definition_hash = "definition-hash",
                image_manifest_hash = "image-hash",
                source_capabilities = new CardPackSourceCapabilities
                {
                    cards = true,
                    images = true,
                    abilities = true,
                    custom_formats = false
                },
                source_abilities_file = "abilities.json",
                source_ability_count = 1,
                source_ability_data_hash = "ability-hash"
            };

            CardPackValidationStatus status = CardPackValidationStatusBuilder.FromManifest(
                manifest,
                true,
                true);

            Assert.IsTrue(status.accepted, status.ToJson(false));
            Assert.AreEqual("cards,images,abilities", status.capabilities_summary);
            Assert.AreEqual(1, status.source_ability_count);
            StringAssert.Contains("schema 2", status.summary);
            StringAssert.Contains("abilities 1", status.summary);
        }

        [Test]
        public void PackValidationStatusBlocksMissingDefinitionHash()
        {
            CardPackManifest manifest = new CardPackManifest
            {
                pack_id = "broken_pack",
                source_version = "1.0.0",
                schema_version = 1,
                card_count = 2,
                image_manifest_hash = "image-hash"
            };

            CardPackValidationStatus status = CardPackValidationStatusBuilder.FromManifest(
                manifest,
                true,
                true);

            Assert.IsFalse(status.accepted);
            Assert.AreEqual("DEFINITION_HASH_MISSING", status.issues[0].code);
            StringAssert.Contains("BLOCKED", status.summary);
        }

        [Test]
        public void PackValidationStatusJsonRoundTrips()
        {
            CardPackValidationStatus status = CardPackValidationStatusBuilder.FromManifest(
                new CardPackManifest
                {
                    pack_id = "round_trip",
                    schema_version = 1,
                    card_count = 1,
                    definition_hash = "definition-hash",
                    image_manifest_hash = "image-hash"
                },
                true,
                true);

            CardPackValidationStatus roundTrip =
                CardPackValidationStatus.FromJson(status.ToJson(false));

            Assert.AreEqual(status.accepted, roundTrip.accepted);
            Assert.AreEqual(status.summary, roundTrip.summary);
            Assert.AreEqual(status.issues.Count, roundTrip.issues.Count);
        }

        [Test]
        public void QueryCardsSupportsSearchAndClanFilter()
        {
            string packDirectory = CardPackFileSystem.DefaultPackDirectory;
            CardPackManifest manifest = CardPackFileSystem.LoadManifest(packDirectory);
            string databasePath = CardPackFileSystem.GetDatabasePath(packDirectory, manifest);

            using (SqliteCardRepository repository = new SqliteCardRepository(databasePath))
            {
                CardQueryOptions options = new CardQueryOptions
                {
                    SearchText = "อัลเฟรด",
                    Clan = "รอยัล พาลาดิน",
                    Limit = 10
                };

                var cards = repository.QueryCards(options);
                Assert.Greater(cards.Count, 0);
                foreach (CardSummary card in cards)
                {
                    Assert.AreEqual("รอยัล พาลาดิน", card.Clan);
                    Assert.IsFalse(string.IsNullOrEmpty(card.CardId));
                    Assert.IsFalse(string.IsNullOrEmpty(card.ImageRelativePath));
                }
            }
        }

        [Test]
        public void QueryCardsSupportsNationFilter()
        {
            string packDirectory = CardPackFileSystem.DefaultPackDirectory;
            CardPackManifest manifest = CardPackFileSystem.LoadManifest(packDirectory);
            string databasePath = CardPackFileSystem.GetDatabasePath(packDirectory, manifest);

            using (SqliteCardRepository repository = new SqliteCardRepository(databasePath))
            {
                CardQueryOptions options = new CardQueryOptions
                {
                    Nation = "\u0e14\u0e23\u0e32\u0e01\u0e49\u0e2d\u0e19\u0e40\u0e2d\u0e21\u0e44\u0e1e\u0e23\u0e4c D",
                    Limit = 10
                };

                var cards = repository.QueryCards(options);
                Assert.Greater(cards.Count, 0);
                foreach (CardSummary card in cards)
                {
                    Assert.AreEqual(
                        "\u0e14\u0e23\u0e32\u0e01\u0e49\u0e2d\u0e19\u0e40\u0e2d\u0e21\u0e44\u0e1e\u0e23\u0e4c D",
                        card.Nation);
                    Assert.IsFalse(string.IsNullOrEmpty(card.CardId));
                }
            }
        }

        [Test]
        public void JsonCardRepositorySupportsQueryDetailAndNationFilter()
        {
            string tempRoot = Path.Combine(Path.GetTempPath(), "JsonCardRepositoryTests", System.Guid.NewGuid().ToString("N"));
            string catalogPath = Path.Combine(tempRoot, "card_catalog.json");
            try
            {
                Directory.CreateDirectory(tempRoot);
                File.WriteAllText(
                    catalogPath,
                    "{" +
                    "\"schema_version\":1," +
                    "\"cards\":[" +
                    "{" +
                    "\"card_id\":\"BT01-001TH\",\"source_id\":\"source-1\",\"source_key\":\"BT01-001TH\"," +
                    "\"name_th\":\"Alfred\",\"text_th\":\"King of Knights\",\"series\":\"[BT01] Test\",\"series_code\":\"BT01\"," +
                    "\"clan\":\"Royal Paladin\",\"nation\":\"United Sanctuary\",\"nation_2\":\"\"," +
                    "\"grade_has_value\":true,\"grade\":3,\"power_has_value\":true,\"power\":10000," +
                    "\"shield_has_value\":false,\"shield\":0,\"trigger\":\"\",\"deck_limit\":4," +
                    "\"type_1\":\"Normal Unit\",\"type_2\":\"\",\"race_1\":\"Human\",\"race_2\":\"\",\"warning\":\"\"," +
                    "\"image_url\":\"url\",\"image_relative_path\":\"BT01/royal/BT01-001TH.jpg\",\"image_exists\":true," +
                    "\"formats\":[{\"format_key\":\"premium_standard_format\",\"format_value\":\"Premium Standard Format\"}]" +
                    "}," +
                    "{" +
                    "\"card_id\":\"BT01-002TH\",\"source_id\":\"source-2\",\"source_key\":\"BT01-002TH\"," +
                    "\"name_th\":\"Critical\",\"text_th\":\"Trigger text\",\"series\":\"[BT01] Test\",\"series_code\":\"BT01\"," +
                    "\"clan\":\"Royal Paladin\",\"nation\":\"United Sanctuary\",\"nation_2\":\"\"," +
                    "\"grade_has_value\":true,\"grade\":0,\"power_has_value\":true,\"power\":5000," +
                    "\"shield_has_value\":true,\"shield\":15000,\"trigger\":\"Critical\",\"deck_limit\":4," +
                    "\"type_1\":\"Trigger Unit\",\"type_2\":\"\",\"race_1\":\"Human\",\"race_2\":\"\",\"warning\":\"\"," +
                    "\"image_url\":\"url\",\"image_relative_path\":\"BT01/royal/BT01-002TH.jpg\",\"image_exists\":true," +
                    "\"formats\":[]" +
                    "}" +
                    "]" +
                    "}",
                    System.Text.Encoding.UTF8);

                using (JsonCardRepository repository = new JsonCardRepository(catalogPath))
                {
                    Assert.AreEqual(2, repository.CountCards());
                    Assert.AreEqual(1, repository.CountSeries());
                    Assert.AreEqual(1, repository.CountClans());
                    Assert.AreEqual(1, repository.ListNations().Count);

                    var cards = repository.QueryCards(new CardQueryOptions
                    {
                        SearchText = "king",
                        Clan = "Royal Paladin",
                        Nation = "United Sanctuary",
                        Limit = 10
                    });
                    Assert.AreEqual(1, cards.Count);
                    Assert.AreEqual("BT01-001TH", cards[0].CardId);

                    CardDetail detail = repository.GetCard("BT01-001TH");
                    Assert.NotNull(detail);
                    Assert.AreEqual("Alfred", detail.NameTh);
                    Assert.AreEqual(10000, detail.Power);
                    Assert.IsFalse(detail.Shield.HasValue);
                    Assert.AreEqual(1, detail.Formats.Count);
                }
            }
            finally
            {
                if (Directory.Exists(tempRoot))
                {
                    Directory.Delete(tempRoot, true);
                }
            }
        }

        [Test]
        public void ImageCacheLoadsFallbackAndClearsMemory()
        {
            string packDirectory = CardPackFileSystem.DefaultPackDirectory;
            CardPackManifest manifest = CardPackFileSystem.LoadManifest(packDirectory);
            string databasePath = CardPackFileSystem.GetDatabasePath(packDirectory, manifest);

            using (SqliteCardRepository repository = new SqliteCardRepository(databasePath))
            using (CardImageCache cache = new CardImageCache(CardPackFileSystem.GetImageRootPath(manifest), 1, 1))
            {
                CardDetail alfred = repository.GetCard("BT01-001TH");
                Assert.NotNull(alfred);

                Texture2D missing = cache.LoadThumbnail("missing/not-found.jpg");
                Assert.NotNull(missing);
                Assert.IsTrue(cache.IsFallbackTexture(missing));
                Assert.AreEqual(0, cache.ThumbnailCount);

                Texture2D thumbnail = cache.LoadThumbnail(alfred.ImageRelativePath);
                Assert.NotNull(thumbnail);
                Assert.IsFalse(cache.IsFallbackTexture(thumbnail));
                Assert.AreEqual(1, cache.ThumbnailCount);

                Texture2D fullImage = cache.LoadFullImage(alfred.ImageRelativePath);
                Assert.NotNull(fullImage);
                Assert.IsFalse(cache.IsFallbackTexture(fullImage));
                Assert.AreEqual(1, cache.FullImageCount);

                cache.ClearMemory();
                Assert.AreEqual(0, cache.ThumbnailCount);
                Assert.AreEqual(0, cache.FullImageCount);
            }
        }
    }
}
