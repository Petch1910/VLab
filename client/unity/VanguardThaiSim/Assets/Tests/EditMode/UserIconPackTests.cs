using System;
using System.IO;
using NUnit.Framework;
using VanguardThaiSim.UI;

namespace VanguardThaiSim.Tests
{
    public sealed class UserIconPackTests
    {
        [Test]
        public void RegistryDefaultsIncludeTriggerAndMarkerSemanticKeys()
        {
            UiGameSymbolResolution critical = UiGameSymbolRegistry.Resolve(UiGameSymbolKeys.TriggerCritical, null);
            UiGameSymbolResolution quickShield = UiGameSymbolRegistry.Resolve(UiGameSymbolKeys.MarkerQuickShield, null);

            Assert.AreEqual("CRITICAL", critical.label);
            Assert.AreEqual("default", critical.source);
            Assert.IsTrue(critical.icon_path.Contains("swords"));
            Assert.AreEqual("Q-SHIELD", quickShield.label);
            Assert.IsTrue(UiGameSymbolRegistry.IsKnownKey(UiGameSymbolKeys.ZoneDeck));
        }

        [Test]
        public void ManifestObjectParsesIconEntries()
        {
            UserIconPackManifest manifest = UserIconPackManifest.FromJson(TemplateJson());

            Assert.AreEqual(UserIconPackManifest.SupportedSchema, manifest.schema);
            Assert.AreEqual("private_user_icons", manifest.pack_id);
            Assert.AreEqual(3, manifest.icons.Count);
            Assert.AreEqual(UiGameSymbolKeys.TriggerCritical, manifest.icons[0].key);
            Assert.AreEqual("trigger_critical.png", manifest.icons[0].file);
        }

        [Test]
        public void MissingUserFilesAreWarningsAndDefaultResolutionFallsBack()
        {
            string root = CreateTempRoot();
            try
            {
                UserIconPackValidationResult result =
                    UserIconPackValidator.Validate(UserIconPackManifest.FromJson(TemplateJson()), root);
                UiGameSymbolResolution resolved = UiGameSymbolRegistry.Resolve(UiGameSymbolKeys.TriggerCritical, result);

                Assert.IsTrue(result.accepted);
                Assert.AreEqual(0, result.accepted_icon_count);
                Assert.AreEqual(3, result.fallback_icon_count);
                AssertIssue(result, "ICON_FILE_NOT_FOUND");
                Assert.AreEqual("default", resolved.source);
                Assert.AreEqual("CRITICAL", resolved.label);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void ExistingUserFileResolvesOverrideIcon()
        {
            string root = CreateTempRoot();
            try
            {
                string iconPath = Path.Combine(root, "trigger_critical.png");
                File.WriteAllBytes(iconPath, new byte[] { 137, 80, 78, 71 });

                UserIconPackValidationResult result =
                    UserIconPackValidator.Validate(UserIconPackManifest.FromJson(TemplateJson()), root);
                UiGameSymbolResolution resolved = UiGameSymbolRegistry.Resolve(UiGameSymbolKeys.TriggerCritical, result);

                Assert.IsTrue(result.accepted);
                Assert.AreEqual(1, result.accepted_icon_count);
                Assert.AreEqual("user", resolved.source);
                Assert.AreEqual(Path.GetFullPath(iconPath), resolved.icon_path);
                Assert.AreEqual("CRITICAL", resolved.label);
                UserIconPackValidationResult roundTrip = UserIconPackValidationResult.FromJson(result.ToJson(false));
                Assert.AreEqual(1, roundTrip.accepted_icon_count);
                Assert.AreEqual(Path.GetFullPath(iconPath), roundTrip.FindResolvedIcon(UiGameSymbolKeys.TriggerCritical).full_path);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void PathTraversalProducesErrorAndRejectsManifest()
        {
            string root = CreateTempRoot();
            try
            {
                string json =
                    "{ \"schema\": \"vanguard-icon-pack-v1\", \"pack_id\": \"private\", \"icons\": { " +
                    "\"trigger_critical\": \"../outside.png\" } }";

                UserIconPackValidationResult result =
                    UserIconPackValidator.Validate(UserIconPackManifest.FromJson(json), root);

                Assert.IsFalse(result.accepted);
                AssertIssue(result, "ICON_FILE_OUTSIDE_ROOT");
                Assert.AreEqual(0, result.accepted_icon_count);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void UnknownIconKeyIsIgnoredWithoutBlockingDefaults()
        {
            string root = CreateTempRoot();
            try
            {
                string json =
                    "{ \"schema\": \"vanguard-icon-pack-v1\", \"pack_id\": \"private\", \"icons\": { " +
                    "\"not_a_symbol\": \"unknown.png\" } }";

                UserIconPackValidationResult result =
                    UserIconPackValidator.Validate(UserIconPackManifest.FromJson(json), root);
                UiGameSymbolResolution resolved = UiGameSymbolRegistry.Resolve(UiGameSymbolKeys.TriggerDraw, result);

                Assert.IsTrue(result.accepted);
                AssertIssue(result, "ICON_KEY_UNKNOWN");
                Assert.AreEqual("default", resolved.source);
                Assert.AreEqual("DRAW", resolved.label);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void RuntimeResolverFindsPersistentManifestCandidate()
        {
            string root = CreateTempRoot();
            try
            {
                string manifestDirectory = Path.Combine(root, "Icons", "UserProvided");
                Directory.CreateDirectory(manifestDirectory);
                string manifestPath = Path.Combine(manifestDirectory, UserIconPackRuntime.ManifestFileName);
                File.WriteAllText(manifestPath, TemplateJson());

                string resolved = UserIconPackRuntime.ResolveManifestPath(new[] { root });

                Assert.AreEqual(Path.GetFullPath(manifestPath), resolved);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void RuntimeResolverFindsProjectAssetsManifestCandidate()
        {
            string root = CreateTempRoot();
            try
            {
                string manifestDirectory = Path.Combine(root, "Assets", "UI", "Icons", "UserProvided");
                Directory.CreateDirectory(manifestDirectory);
                string manifestPath = Path.Combine(manifestDirectory, UserIconPackRuntime.ManifestFileName);
                File.WriteAllText(manifestPath, TemplateJson());

                string resolved = UserIconPackRuntime.ResolveManifestPath(new[] { root });

                Assert.AreEqual(Path.GetFullPath(manifestPath), resolved);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        [Test]
        public void RuntimeResolverMissingManifestReturnsEmptyPath()
        {
            string root = CreateTempRoot();
            try
            {
                string resolved = UserIconPackRuntime.ResolveManifestPath(new[] { root });

                Assert.AreEqual(string.Empty, resolved);
            }
            finally
            {
                DeleteTempRoot(root);
            }
        }

        private static string TemplateJson()
        {
            return
                "{\n" +
                "  \"schema\": \"vanguard-icon-pack-v1\",\n" +
                "  \"pack_id\": \"private_user_icons\",\n" +
                "  \"display_name\": \"Private User Icons\",\n" +
                "  \"icons\": {\n" +
                "    \"trigger_critical\": \"trigger_critical.png\",\n" +
                "    \"trigger_draw\": \"trigger_draw.png\",\n" +
                "    \"marker_quick_shield\": \"marker_quick_shield.png\"\n" +
                "  }\n" +
                "}";
        }

        private static string CreateTempRoot()
        {
            string root = Path.Combine(Path.GetTempPath(), "vanguard-icon-pack-tests-" + Guid.NewGuid().ToString("N"));
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

        private static void AssertIssue(UserIconPackValidationResult result, string code)
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
    }
}
