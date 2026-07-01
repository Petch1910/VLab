using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace VanguardThaiSim.Decks
{
    [Serializable]
    public sealed class UserDeckAssetValidationIssue
    {
        public string severity;
        public string code;
        public string slot;
        public string key;
        public string message;
    }

    [Serializable]
    public sealed class UserDeckAssetResolution
    {
        public string slot;
        public string key;
        public string file;
        public string full_path;
        public string sha256;
        public string fallback_key;
    }

    [Serializable]
    public sealed class UserDeckAssetValidationResult
    {
        public bool accepted;
        public string pack_id;
        public string display_name;
        public int declared_asset_count;
        public int accepted_asset_count;
        public int fallback_asset_count;
        public List<UserDeckAssetResolution> resolved_assets = new List<UserDeckAssetResolution>();
        public List<UserDeckAssetValidationIssue> issues = new List<UserDeckAssetValidationIssue>();

        public string ToJson(bool prettyPrint)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static UserDeckAssetValidationResult FromJson(string json)
        {
            UserDeckAssetValidationResult result = string.IsNullOrWhiteSpace(json)
                ? new UserDeckAssetValidationResult()
                : JsonUtility.FromJson<UserDeckAssetValidationResult>(json);
            if (result == null)
            {
                result = new UserDeckAssetValidationResult();
            }

            result.EnsureLists();
            return result;
        }

        public UserDeckAssetResolution FindResolvedAsset(string slot, string key)
        {
            if (string.IsNullOrWhiteSpace(slot) || string.IsNullOrWhiteSpace(key) || resolved_assets == null)
            {
                return null;
            }

            for (int i = 0; i < resolved_assets.Count; i++)
            {
                UserDeckAssetResolution asset = resolved_assets[i];
                if (asset != null &&
                    string.Equals(asset.slot, slot.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(asset.key, key.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return asset;
                }
            }

            return null;
        }

        private void EnsureLists()
        {
            if (resolved_assets == null)
            {
                resolved_assets = new List<UserDeckAssetResolution>();
            }

            if (issues == null)
            {
                issues = new List<UserDeckAssetValidationIssue>();
            }
        }
    }

    public static class UserDeckAssetValidator
    {
        private const string Error = "error";
        private const string Warning = "warning";

        public static UserDeckAssetValidationResult Validate(UserDeckAssetManifest manifest, string rootDirectory)
        {
            UserDeckAssetValidationResult result = new UserDeckAssetValidationResult();
            if (manifest == null)
            {
                AddIssue(result, Error, "MANIFEST_MISSING", string.Empty, string.Empty, "Deck asset manifest is missing.");
                Finish(result);
                return result;
            }

            if (manifest.assets == null)
            {
                manifest.assets = new List<UserDeckAssetManifestEntry>();
            }

            result.pack_id = manifest.pack_id ?? string.Empty;
            result.display_name = manifest.display_name ?? string.Empty;
            result.declared_asset_count = manifest.assets.Count;

            if (!string.Equals(manifest.schema, UserDeckAssetManifest.SupportedSchema, StringComparison.Ordinal))
            {
                AddIssue(result, Error, "SCHEMA_UNSUPPORTED", string.Empty, string.Empty, "Deck asset schema must be " + UserDeckAssetManifest.SupportedSchema + ".");
            }

            string fullRoot = NormalizeRoot(rootDirectory);
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < manifest.assets.Count; i++)
            {
                UserDeckAssetManifestEntry entry = manifest.assets[i];
                string slot = entry == null ? string.Empty : (entry.slot ?? string.Empty).Trim();
                string key = entry == null ? string.Empty : DeckAppearanceMetadata.Normalize(
                    new DeckAppearanceMetadata { sleeve_key = entry.key }).sleeve_key;
                string file = entry == null ? string.Empty : (entry.file ?? string.Empty).Trim();
                string expectedHash = entry == null ? string.Empty : (entry.sha256 ?? string.Empty).Trim().ToLowerInvariant();
                string fallbackKey = entry == null ? DeckAppearanceMetadata.DefaultKey : NormalizeFallback(entry.fallback_key);

                if (string.IsNullOrWhiteSpace(slot))
                {
                    AddIssue(result, Warning, "ASSET_SLOT_MISSING", string.Empty, key, "Asset slot is missing.");
                    continue;
                }

                if (!UserDeckAssetSlots.IsKnown(slot))
                {
                    AddIssue(result, Warning, "ASSET_SLOT_UNKNOWN", slot, key, "Asset slot is not supported.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(key) || key == DeckAppearanceMetadata.DefaultKey)
                {
                    AddIssue(result, Warning, "ASSET_KEY_MISSING_OR_DEFAULT", slot, key, "Custom asset key must be a non-default safe key.");
                    continue;
                }

                string dedupeKey = slot + ":" + key;
                if (!seen.Add(dedupeKey))
                {
                    AddIssue(result, Warning, "ASSET_KEY_DUPLICATE", slot, key, "Duplicate asset key is ignored.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(file))
                {
                    AddIssue(result, Warning, "ASSET_FILE_MISSING_NAME", slot, key, "Asset file name is missing; fallback will be used.");
                    continue;
                }

                if (!IsSupportedExtension(file))
                {
                    AddIssue(result, Warning, "ASSET_FILE_UNSUPPORTED_EXTENSION", slot, key, "Only png/jpg/jpeg/svg deck accessory assets are supported.");
                    continue;
                }

                if (!IsSha256Hex(expectedHash))
                {
                    AddIssue(result, Warning, "ASSET_HASH_MISSING_OR_INVALID", slot, key, "A valid SHA-256 hash is required before an asset can be used.");
                    continue;
                }

                string fullPath = ResolveInsideRoot(fullRoot, file);
                if (string.IsNullOrEmpty(fullPath))
                {
                    AddIssue(result, Error, "ASSET_FILE_OUTSIDE_ROOT", slot, key, "Asset file path must stay inside the user-provided asset folder.");
                    continue;
                }

                if (!File.Exists(fullPath))
                {
                    AddIssue(result, Warning, "ASSET_FILE_NOT_FOUND", slot, key, "Asset file was not found; fallback will be used.");
                    continue;
                }

                string actualHash = ComputeSha256(fullPath);
                if (!string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase))
                {
                    AddIssue(result, Warning, "ASSET_HASH_MISMATCH", slot, key, "Asset hash did not match; fallback will be used.");
                    continue;
                }

                result.resolved_assets.Add(new UserDeckAssetResolution
                {
                    slot = slot,
                    key = key,
                    file = file,
                    full_path = fullPath,
                    sha256 = actualHash,
                    fallback_key = fallbackKey
                });
            }

            Finish(result);
            return result;
        }

        private static void Finish(UserDeckAssetValidationResult result)
        {
            int errors = 0;
            for (int i = 0; i < result.issues.Count; i++)
            {
                if (result.issues[i] != null && result.issues[i].severity == Error)
                {
                    errors++;
                }
            }

            result.accepted_asset_count = result.resolved_assets.Count;
            result.fallback_asset_count = Math.Max(0, result.declared_asset_count - result.accepted_asset_count);
            result.accepted = errors == 0;
        }

        private static void AddIssue(
            UserDeckAssetValidationResult result,
            string severity,
            string code,
            string slot,
            string key,
            string message)
        {
            result.issues.Add(new UserDeckAssetValidationIssue
            {
                severity = severity,
                code = code,
                slot = slot ?? string.Empty,
                key = key ?? string.Empty,
                message = message ?? string.Empty
            });
        }

        private static string NormalizeRoot(string rootDirectory)
        {
            string root = string.IsNullOrWhiteSpace(rootDirectory) ? Directory.GetCurrentDirectory() : rootDirectory;
            return Path.GetFullPath(root);
        }

        private static string ResolveInsideRoot(string fullRoot, string relativeFile)
        {
            if (Path.IsPathRooted(relativeFile))
            {
                return string.Empty;
            }

            string fullPath = Path.GetFullPath(Path.Combine(fullRoot, relativeFile.Replace('/', Path.DirectorySeparatorChar)));
            string rootWithSeparator = fullRoot.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? fullRoot
                : fullRoot + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(fullPath, fullRoot, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return fullPath;
        }

        private static bool IsSupportedExtension(string file)
        {
            string extension = Path.GetExtension(file);
            return string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(extension, ".svg", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSha256Hex(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length != 64)
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                bool valid =
                    (c >= '0' && c <= '9') ||
                    (c >= 'a' && c <= 'f') ||
                    (c >= 'A' && c <= 'F');
                if (!valid)
                {
                    return false;
                }
            }

            return true;
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

        private static string NormalizeFallback(string fallback)
        {
            return DeckAppearanceMetadata.Normalize(new DeckAppearanceMetadata { sleeve_key = fallback }).sleeve_key;
        }
    }
}
