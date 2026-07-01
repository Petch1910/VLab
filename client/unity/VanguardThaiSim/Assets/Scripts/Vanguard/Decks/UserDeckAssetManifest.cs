using System;
using System.Collections.Generic;
using UnityEngine;

namespace VanguardThaiSim.Decks
{
    [Serializable]
    public sealed class UserDeckAssetManifest
    {
        public const string SupportedSchema = "vanguard-deck-assets-v1";

        public string schema;
        public string pack_id;
        public string display_name;
        public List<UserDeckAssetManifestEntry> assets = new List<UserDeckAssetManifestEntry>();

        public string ToJson(bool prettyPrint)
        {
            EnsureLists();
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static UserDeckAssetManifest FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new UserDeckAssetManifest();
            }

            UserDeckAssetManifest manifest = JsonUtility.FromJson<UserDeckAssetManifest>(json) ?? new UserDeckAssetManifest();
            manifest.EnsureLists();
            return manifest;
        }

        private void EnsureLists()
        {
            if (assets == null)
            {
                assets = new List<UserDeckAssetManifestEntry>();
            }
        }
    }

    [Serializable]
    public sealed class UserDeckAssetManifestEntry
    {
        public string slot;
        public string key;
        public string file;
        public string sha256;
        public string fallback_key;
    }
}
