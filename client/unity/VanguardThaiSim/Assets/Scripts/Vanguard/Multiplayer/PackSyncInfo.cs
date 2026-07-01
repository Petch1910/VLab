using System;
using VanguardThaiSim.Cards;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class PackSyncInfo
    {
        public string pack_id;
        public string source_version;
        public string definition_hash;
        public string image_manifest_hash;
        public string image_content_hash;

        public static PackSyncInfo FromManifest(CardPackManifest manifest)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException(nameof(manifest));
            }

            return new PackSyncInfo
            {
                pack_id = manifest.pack_id,
                source_version = manifest.source_version,
                definition_hash = manifest.definition_hash,
                image_manifest_hash = manifest.image_manifest_hash,
                image_content_hash = manifest.image_content_hash
            };
        }
    }
}
