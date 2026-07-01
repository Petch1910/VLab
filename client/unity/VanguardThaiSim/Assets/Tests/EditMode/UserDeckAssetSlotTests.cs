using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using VanguardThaiSim.Decks;

namespace VanguardThaiSim.Tests
{
    public sealed class UserDeckAssetSlotTests
    {
        [Test]
        public void ManifestRoundTripsAssetEntries()
        {
            UserDeckAssetManifest manifest = UserDeckAssetManifest.FromJson(TemplateJson("abc"));

            Assert.AreEqual(UserDeckAssetManifest.SupportedSchema, manifest.schema);
            Assert.AreEqual("private_deck_assets", manifest.pack_id);
            Assert.AreEqual(1, manifest.assets.Count);
            Assert.AreEqual(UserDeckAssetSlots.Sleeve, manifest.assets[0].slot);
            Assert.AreEqual("blue", manifest.assets[0].key);
            Assert.AreEqual("sleeves/blue.png", manifest.assets[0].file);
        }

        [Test]
        public void ExistingFileWithMatchingHashResolvesAsset()
        {
            string root = CreateTempRoot();
            try
            {
                string file = Path.Combine(root, "sleeves", "blue.png");
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                File.WriteAllBytes(file, new byte[] { 1, 2, 3, 4 });
                string hash = ComputeSha256(file);

                UserDeckAssetValidationResult result =
                    UserDeckAssetValidator.Validate(UserDeckAssetManifest.FromJson(TemplateJson(hash)), root);

                Assert.IsTrue(result.accepted);
                Assert.AreEqual(1, result.accepted_asset_count);
                Assert.AreEqual(0, result.fallback_asset_count);
                UserDeckAssetResolution resolved = result.FindResolvedAsset(UserDeckAssetSlots.Sleeve, "blue");
                Assert.NotNull(resolved);
                Assert.AreEqual(Path.GetFullPath(file), resolved.full_path);
                Assert.AreEqual(hash, resolved.sha256);
                UserDeckAssetValidationResult roundTrip = UserDeckAssetValidationResult.FromJson(result.ToJson(false));
                Assert.AreEqual(1, roundTrip.accepted_asset_count);
                Assert.NotNull(roundTrip.FindResolvedAsset(UserDeckAssetSlots.Sleeve, "blue"));
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void MissingFileFallsBackWithoutRejectingManifest()
        {
            string root = CreateTempRoot();
            try
            {
                UserDeckAssetValidationResult result =
                    UserDeckAssetValidator.Validate(UserDeckAssetManifest.FromJson(TemplateJson(ShaOfText("missing"))), root);

                Assert.IsTrue(result.accepted);
                Assert.AreEqual(0, result.accepted_asset_count);
                Assert.AreEqual(1, result.fallback_asset_count);
                AssertIssue(result, "ASSET_FILE_NOT_FOUND");
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void HashMismatchFallsBackWithoutAcceptingAsset()
        {
            string root = CreateTempRoot();
            try
            {
                string file = Path.Combine(root, "sleeves", "blue.png");
                Directory.CreateDirectory(Path.GetDirectoryName(file));
                File.WriteAllBytes(file, new byte[] { 9, 9, 9 });

                UserDeckAssetValidationResult result =
                    UserDeckAssetValidator.Validate(UserDeckAssetManifest.FromJson(TemplateJson(ShaOfText("wrong"))), root);

                Assert.IsTrue(result.accepted);
                Assert.AreEqual(0, result.accepted_asset_count);
                Assert.AreEqual(1, result.fallback_asset_count);
                AssertIssue(result, "ASSET_HASH_MISMATCH");
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void PathTraversalRejectsManifest()
        {
            string root = CreateTempRoot();
            try
            {
                string json =
                    "{ \"schema\": \"vanguard-deck-assets-v1\", \"pack_id\": \"private\", \"assets\": [ { " +
                    "\"slot\": \"sleeve\", \"key\": \"blue\", \"file\": \"../outside.png\", " +
                    "\"sha256\": \"" + ShaOfText("x") + "\", \"fallback_key\": \"default\" } ] }";

                UserDeckAssetValidationResult result =
                    UserDeckAssetValidator.Validate(UserDeckAssetManifest.FromJson(json), root);

                Assert.IsFalse(result.accepted);
                AssertIssue(result, "ASSET_FILE_OUTSIDE_ROOT");
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void UnknownSlotIsWarningAndUsesFallback()
        {
            string root = CreateTempRoot();
            try
            {
                string json =
                    "{ \"schema\": \"vanguard-deck-assets-v1\", \"pack_id\": \"private\", \"assets\": [ { " +
                    "\"slot\": \"unknown\", \"key\": \"blue\", \"file\": \"blue.png\", " +
                    "\"sha256\": \"" + ShaOfText("x") + "\", \"fallback_key\": \"default\" } ] }";

                UserDeckAssetValidationResult result =
                    UserDeckAssetValidator.Validate(UserDeckAssetManifest.FromJson(json), root);

                Assert.IsTrue(result.accepted);
                Assert.AreEqual(0, result.accepted_asset_count);
                Assert.AreEqual(1, result.fallback_asset_count);
                AssertIssue(result, "ASSET_SLOT_UNKNOWN");
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        private static string TemplateJson(string hash)
        {
            return
                "{\n" +
                "  \"schema\": \"vanguard-deck-assets-v1\",\n" +
                "  \"pack_id\": \"private_deck_assets\",\n" +
                "  \"display_name\": \"Private Deck Assets\",\n" +
                "  \"assets\": [\n" +
                "    {\n" +
                "      \"slot\": \"sleeve\",\n" +
                "      \"key\": \"blue\",\n" +
                "      \"file\": \"sleeves/blue.png\",\n" +
                "      \"sha256\": \"" + hash + "\",\n" +
                "      \"fallback_key\": \"default\"\n" +
                "    }\n" +
                "  ]\n" +
                "}";
        }

        private static string CreateTempRoot()
        {
            string root = Path.Combine(Path.GetTempPath(), "vanguard-deck-asset-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return root;
        }

        private static void DeleteTempRoot(string root)
        {
            if (!string.IsNullOrWhiteSpace(root) && Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }

        private static void AssertIssue(UserDeckAssetValidationResult result, string code)
        {
            for (int i = 0; i < result.issues.Count; i++)
            {
                if (result.issues[i] != null && result.issues[i].code == code)
                {
                    return;
                }
            }

            Assert.Fail("Expected issue code not found: " + code);
        }

        private static string ShaOfText(string value)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
                StringBuilder builder = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private static string ComputeSha256(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                byte[] hash = sha.ComputeHash(stream);
                StringBuilder builder = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
