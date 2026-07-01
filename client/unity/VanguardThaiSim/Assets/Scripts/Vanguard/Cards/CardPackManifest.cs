using System;

namespace VanguardThaiSim.Cards
{
    [Serializable]
    public sealed class CardPackManifest
    {
        public string pack_id;
        public string display_name;
        public int schema_version;
        public int source_schema_version;
        public string source;
        public string source_version;
        public CardPackSourceCapabilities source_capabilities;
        public string source_ruleset_profile;
        public string source_abilities_file;
        public int source_ability_count;
        public string source_ability_data_hash;
        public string source_formats_file;
        public int card_count;
        public int image_count;
        public int existing_image_count;
        public int series_count;
        public int clan_count;
        public string definition_hash;
        public string image_manifest_hash;
        public string asset_index_file;
        public int asset_index_schema_version;
        public string image_content_hash;
        public string image_cache_strategy;
        public int cache_layout_version;
        public string user_data_policy;
        public string image_root;
        public string sqlite_file;
        public bool sqlite_fts_enabled;
        public string catalog_file;
        public int catalog_schema_version;
    }

    [Serializable]
    public sealed class CardPackSourceCapabilities
    {
        public bool cards;
        public bool images;
        public bool abilities;
        public bool custom_formats;
    }
}
