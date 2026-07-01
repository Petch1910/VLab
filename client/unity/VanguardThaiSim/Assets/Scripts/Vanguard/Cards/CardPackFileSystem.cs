using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VanguardThaiSim.Cards
{
    public static class CardPackFileSystem
    {
        public const string DefaultPackRelativePath = "data/packs/vanguard_th";
        private const string ManifestFileName = "manifest.json";

        public static string ProjectRoot
        {
            get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
        }

        public static string WorkspaceRoot
        {
            get { return ResolveWorkspaceRoot(); }
        }

        public static string DefaultPackDirectory
        {
            get { return ResolveDefaultPackDirectory(); }
        }

        public static string ResolveDefaultPackDirectory()
        {
            string existing = FindExistingDefaultPackDirectory(GetDefaultPackSearchRoots());
            if (!string.IsNullOrEmpty(existing))
            {
                return existing;
            }

            return Path.Combine(WorkspaceRoot, DefaultPackRelativePath);
        }

        public static string FindExistingDefaultPackDirectory(IEnumerable<string> searchRoots)
        {
            if (searchRoots == null)
            {
                return string.Empty;
            }

            foreach (string root in searchRoots)
            {
                string candidate = FindExistingRelativePathInAncestors(root, DefaultPackRelativePath);
                if (!string.IsNullOrEmpty(candidate) &&
                    File.Exists(Path.Combine(candidate, ManifestFileName)))
                {
                    return candidate;
                }
            }

            return string.Empty;
        }

        public static string FindExistingRelativePathInAncestors(string startDirectory, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(startDirectory) || string.IsNullOrWhiteSpace(relativePath))
            {
                return string.Empty;
            }

            string current;
            try
            {
                current = Path.GetFullPath(startDirectory);
            }
            catch (Exception)
            {
                return string.Empty;
            }

            if (File.Exists(current))
            {
                current = Path.GetDirectoryName(current);
            }

            while (!string.IsNullOrEmpty(current))
            {
                string candidate = Path.Combine(
                    current,
                    relativePath.Replace('/', Path.DirectorySeparatorChar));
                if (Directory.Exists(candidate) || File.Exists(candidate))
                {
                    return Path.GetFullPath(candidate);
                }

                DirectoryInfo parent = Directory.GetParent(current);
                if (parent == null)
                {
                    break;
                }

                current = parent.FullName;
            }

            return string.Empty;
        }

        public static CardPackManifest LoadManifest(string packDirectory)
        {
            if (string.IsNullOrEmpty(packDirectory))
            {
                throw new ArgumentException("Pack directory is required.", "packDirectory");
            }

            string manifestPath = Path.Combine(packDirectory, ManifestFileName);
            if (!File.Exists(manifestPath))
            {
                throw new FileNotFoundException("Card pack manifest was not found.", manifestPath);
            }

            string json = File.ReadAllText(manifestPath);
            CardPackManifest manifest = JsonUtility.FromJson<CardPackManifest>(json);
            if (manifest == null)
            {
                throw new InvalidDataException("Card pack manifest could not be parsed.");
            }

            if (manifest.schema_version != 1)
            {
                throw new NotSupportedException("Unsupported card pack schema version: " + manifest.schema_version);
            }

            return manifest;
        }

        public static string GetDatabasePath(string packDirectory, CardPackManifest manifest)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException("manifest");
            }

            string sqliteFile = string.IsNullOrEmpty(manifest.sqlite_file) ? "cards.sqlite" : manifest.sqlite_file;
            return Path.Combine(packDirectory, sqliteFile);
        }

        public static string GetCatalogPath(string packDirectory, CardPackManifest manifest)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException("manifest");
            }

            string catalogFile = string.IsNullOrEmpty(manifest.catalog_file) ? "card_catalog.json" : manifest.catalog_file;
            return Path.Combine(packDirectory, catalogFile);
        }

        public static string GetImageRootPath(CardPackManifest manifest)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException("manifest");
            }

            if (string.IsNullOrEmpty(manifest.image_root))
            {
                throw new InvalidDataException("Card pack manifest is missing image_root.");
            }

            if (Path.IsPathRooted(manifest.image_root))
            {
                return Path.GetFullPath(manifest.image_root);
            }

            string relativePath = manifest.image_root.Replace('/', Path.DirectorySeparatorChar);
            string existing = FindExistingRelativePathInAncestors(ProjectRoot, relativePath);
            if (!string.IsNullOrEmpty(existing))
            {
                return existing;
            }

            existing = FindExistingRelativePathInAncestors(SafeGetCurrentDirectory(), relativePath);
            if (!string.IsNullOrEmpty(existing))
            {
                return existing;
            }

            return Path.Combine(WorkspaceRoot, relativePath);
        }

        public static string ResolveImagePath(CardPackManifest manifest, CardSummary summary)
        {
            return Path.Combine(GetImageRootPath(manifest), summary.ImageRelativePath.Replace('/', Path.DirectorySeparatorChar));
        }

        public static string GetAssetIndexPath(string packDirectory, CardPackManifest manifest)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException("manifest");
            }

            string assetIndexFile = string.IsNullOrEmpty(manifest.asset_index_file) ? "asset_index.json" : manifest.asset_index_file;
            return Path.Combine(packDirectory, assetIndexFile);
        }

        public static string GetRuntimeCacheRoot()
        {
            return Path.Combine(Application.persistentDataPath, "card_cache");
        }

        public static string GetPackCacheDirectory(CardPackManifest manifest)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException("manifest");
            }

            return Path.Combine(GetRuntimeCacheRoot(), manifest.pack_id);
        }

        public static string GetUserDataRoot()
        {
            return Path.Combine(Application.persistentDataPath, "user_data");
        }

        private static string ResolveWorkspaceRoot()
        {
            string packDirectory = FindExistingDefaultPackDirectory(GetDefaultPackSearchRoots());
            if (!string.IsNullOrEmpty(packDirectory))
            {
                DirectoryInfo packInfo = Directory.GetParent(packDirectory);
                DirectoryInfo packsInfo = packInfo == null ? null : packInfo.Parent;
                DirectoryInfo dataInfo = packsInfo == null ? null : packsInfo.Parent;
                if (dataInfo != null)
                {
                    return dataInfo.FullName;
                }
            }

            return Path.GetFullPath(Path.Combine(ProjectRoot, "..", "..", ".."));
        }

        private static IEnumerable<string> GetDefaultPackSearchRoots()
        {
            yield return ProjectRoot;
            yield return Application.persistentDataPath;
            yield return Application.streamingAssetsPath;
            yield return SafeGetCurrentDirectory();
        }

        private static string SafeGetCurrentDirectory()
        {
            try
            {
                return Directory.GetCurrentDirectory();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
