using System;
using System.IO;
using UnityEngine;

namespace VanguardThaiSim.Cards
{
    public static class CardRepositoryFactory
    {
        public static CardRepositoryLoadResult LoadDefault(string packDirectory, CardPackManifest manifest)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException("manifest");
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            bool preferCatalog = true;
#else
            bool preferCatalog = false;
#endif

            return preferCatalog
                ? LoadCatalogThenSqlite(packDirectory, manifest)
                : LoadSqliteThenCatalog(packDirectory, manifest);
        }

        private static CardRepositoryLoadResult LoadSqliteThenCatalog(string packDirectory, CardPackManifest manifest)
        {
            string databasePath = CardPackFileSystem.GetDatabasePath(packDirectory, manifest);
            try
            {
                return new CardRepositoryLoadResult(
                    new SqliteCardRepository(databasePath),
                    "sqlite",
                    databasePath,
                    string.Empty,
                    string.Empty);
            }
            catch (Exception sqliteException)
            {
                string catalogPath = CardPackFileSystem.GetCatalogPath(packDirectory, manifest);
                if (!File.Exists(catalogPath))
                {
                    throw;
                }

                Debug.LogWarning("SQLite card repository failed; using JSON catalog fallback. " + sqliteException.GetType().Name + ": " + sqliteException.Message);
                return new CardRepositoryLoadResult(
                    new JsonCardRepository(catalogPath),
                    "json_catalog_fallback",
                    databasePath,
                    catalogPath,
                    sqliteException.GetType().Name + ": " + sqliteException.Message);
            }
        }

        private static CardRepositoryLoadResult LoadCatalogThenSqlite(string packDirectory, CardPackManifest manifest)
        {
            string catalogPath = CardPackFileSystem.GetCatalogPath(packDirectory, manifest);
            if (File.Exists(catalogPath))
            {
                return new CardRepositoryLoadResult(
                    new JsonCardRepository(catalogPath),
                    "json_catalog",
                    CardPackFileSystem.GetDatabasePath(packDirectory, manifest),
                    catalogPath,
                    string.Empty);
            }

            string databasePath = CardPackFileSystem.GetDatabasePath(packDirectory, manifest);
            return new CardRepositoryLoadResult(
                new SqliteCardRepository(databasePath),
                "sqlite_no_catalog",
                databasePath,
                catalogPath,
                "Runtime card catalog was not found; SQLite fallback was used.");
        }
    }

    public sealed class CardRepositoryLoadResult
    {
        public readonly ICardRepository Repository;
        public readonly string Mode;
        public readonly string DatabasePath;
        public readonly string CatalogPath;
        public readonly string Warning;

        public CardRepositoryLoadResult(
            ICardRepository repository,
            string mode,
            string databasePath,
            string catalogPath,
            string warning)
        {
            Repository = repository ?? throw new ArgumentNullException("repository");
            Mode = mode ?? string.Empty;
            DatabasePath = databasePath ?? string.Empty;
            CatalogPath = catalogPath ?? string.Empty;
            Warning = warning ?? string.Empty;
        }
    }
}
