# Custom Pack Ability Data Spec

## Scope

`M15-02` lets schema v2 custom packs include a structured ability data file and
validates it before import. This is data-contract work only.

## Pack Contract

A schema v2 `pack.json` can enable ability data with:

```json
{
  "schema_version": 2,
  "capabilities": {
    "cards": true,
    "images": true,
    "abilities": true,
    "custom_formats": false
  },
  "abilities_file": "abilities.json"
}
```

`abilities_file` must be:

- present when `capabilities.abilities` is true
- a safe relative path
- a `.json` file
- present on disk
- valid `ability_schema_v1`

Every ability `card_id` must reference a card row in the same custom pack.

## Runtime Manifest Fields

The importer preserves ability metadata:

- `source_abilities_file`
- `source_ability_count`
- `source_ability_data_hash`

`source_ability_data_hash` is the SHA-256 of the ability JSON file bytes. Use it
with `definition_hash` and `image_manifest_hash` for future multiplayer pack
matching.

## Boundaries

M15-02 does not:

- merge ability data into SQLite
- load custom abilities into Unity runtime registries
- execute custom abilities
- parse free-text ability text
- bypass manual fallback for unsupported effects

## Verification

Python tests cover:

- v2 template ability file validation
- invalid ability schema rejection
- ability `card_id` missing from pack rejection
- unsafe ability path rejection
- runtime manifest ability count/hash preservation
