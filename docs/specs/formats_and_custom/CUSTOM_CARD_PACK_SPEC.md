# Custom Card Pack Spec

Status: source schema v1 plus backward-compatible schema v2 envelope and
ability-file validation for M15-02.

This spec defines the input contract for user-made card packs. It is inspired by
VangPro-style custom packs, but it uses our own format and must not copy code,
assets, or private data from other apps.

## Goals

- Let users create new card packs without editing the core Vanguard TH pack.
- Keep card definitions deterministic so two clients can verify they are using
  the same custom pack before multiplayer.
- Keep images outside the source definition hash so card text and image drift can
  be detected separately.
- Give M7-02 a clear importer contract for building `cards.sqlite`,
  `manifest.json`, and optional image cache/index files.

## Source Directory Layout

```text
custom_pack/
  pack.json
  cards.csv
  images/
    <relative image files>
```

Transport may use a zip file with the same internal layout:

```text
custom_pack.zip
  pack.json
  cards.csv
  images/
    BT-CUSTOM-001.png
    subfolder/BT-CUSTOM-002.webp
```

`cards.xlsx` is allowed by the spec, but the first importer target is CSV. An
XLSX importer must expose the same columns as `cards.csv` and read from a sheet
named `cards`.

## `pack.json` v1

Required fields:

```json
{
  "schema_version": 1,
  "pack_id": "custom_starter_pack",
  "display_name": "Custom Starter Pack",
  "source_version": "1.0.0",
  "format": "custom",
  "language": "th",
  "cards_file": "cards.csv",
  "image_root": "images"
}
```

Optional fields:

```json
{
  "author": "Creator name",
  "description": "Short pack description",
  "license": "personal-use",
  "homepage_url": "https://example.invalid"
}
```

Rules:

- `schema_version` must be `1`.
- `pack_id` must match `^[a-z0-9][a-z0-9_-]{2,63}$`.
- `format` should be one of `standard`, `v_premium`, `premium`, `mixed`, or
  `custom`.
- `cards_file` must be a relative path to `cards.csv` or `cards.xlsx`.
- `image_root` must be a relative directory path.
- Absolute paths and paths containing `..` are invalid.

## `pack.json` v2 Envelope

Schema v2 keeps all v1 card CSV rules and adds an explicit feature envelope for
future custom abilities and custom formats.

Required fields are still the v1 fields, except `schema_version` is `2` and
`capabilities` is required:

```json
{
  "schema_version": 2,
  "pack_id": "custom_starter_pack_v2",
  "display_name": "Custom Starter Pack V2",
  "source_version": "1.0.0",
  "format": "custom",
  "language": "th",
  "cards_file": "cards.csv",
  "image_root": "images",
  "ruleset_profile": "custom",
  "capabilities": {
    "cards": true,
    "images": true,
  "abilities": true,
  "custom_formats": false
  },
  "abilities_file": "abilities.json",
  "dependencies": []
}
```

Rules:

- v1 packs remain valid; do not migrate existing packs unless needed.
- v2 `capabilities.cards` must be `true`.
- v2 `capabilities.images`, `capabilities.abilities`, and
  `capabilities.custom_formats` must be booleans.
- `ruleset_profile`, when present, must be one of `standard`, `v_premium`,
  `premium`, `mixed`, or `custom`.
- `abilities_file`, when `capabilities.abilities` is `true`, must be a safe
  relative `.json` path, must exist, must pass `ability_schema_v1`, and every
  ability `card_id` must exist in the same custom pack.
- `formats_file`, when `capabilities.custom_formats` is `true`, must be a safe
  relative `.json` path and must exist. M15-05+ will validate its contents.
- `dependencies`, when present, must be an array of objects with a valid
  `pack_id` and optional SHA-256 `definition_hash` /
  `image_manifest_hash`.

The v2 template lives at:

```text
data/templates/custom_pack_v2/
```

Generated runtime manifest fields, created by the M7-02 importer:

- `definition_hash`
- `image_manifest_hash`
- `image_content_hash`
- `sqlite_file`
- `card_count`
- `image_count`
- `existing_image_count`
- `series_count`
- `clan_count`

## Card Columns

The CSV/XLSX header is the contract. Columns are lower snake case.

Required columns:

```text
card_id,name,text,series,clan,grade,deck_limit,image_file
```

Full recommended columns:

```text
card_id,name,language,format,series,series_code,nation,clan,race,type_1,type_2,grade,power,shield,critical,trigger,deck_limit,ability_timing,ability_tags,text,flavor_text,image_file,artist,rarity,notes
```

Column mapping to the current runtime database:

| Source column | Runtime target |
| --- | --- |
| `card_id` | `cards.card_id` |
| `name` | `cards.name_th` for Thai packs, display name for other languages |
| `text` | `cards.text_th` for Thai packs, effect text for other languages |
| `series` | `cards.series` |
| `series_code` | `cards.series_code` |
| `clan` | `cards.clan` |
| `nation` | `cards.nation` |
| `grade` | `cards.grade` |
| `power` | `cards.power` |
| `shield` | `cards.shield` |
| `trigger` | `cards.trigger` |
| `deck_limit` | `cards.deck_limit` |
| `type_1` | `cards.type_1` |
| `type_2` | `cards.type_2` |
| `race` | `cards.race_1` |
| `image_file` | `card_images.image_relative_path` |

## Card Field Rules

- `card_id` must be unique within the pack.
- `card_id` must match `^[A-Za-z0-9][A-Za-z0-9_.:-]{1,63}$`.
- `name`, `text`, `series`, and `clan` must not be empty.
- `grade` must be an integer from `0` to `13`.
- `power`, `shield`, and `critical` may be empty or non-negative integers.
- `deck_limit` must be an integer from `0` to `4`.
- `trigger` may be empty or one of:
  `critical`, `draw`, `front`, `heal`, `stand`, `over`, `none`.
- `ability_timing` is free text for M7, but recommended values are:
  `auto`, `act`, `cont`, `trigger`, `manual`, separated by `;` when multiple.
- `ability_tags` is free text for M7, separated by `;`.
- `notes` is author-only metadata and is not part of the definition hash.

## Image Rules

- `image_file` may be empty during draft/template work. Runtime will use the
  missing-image fallback until an image is supplied.
- Non-empty `image_file` must be relative to `image_root`.
- Use `/` as the separator.
- Absolute paths, drive-letter paths, and `..` are invalid.
- Allowed extensions: `.png`, `.jpg`, `.jpeg`, `.webp`.
- Image paths should be unique within the pack.
- A strict importer may reject missing files; the schema validator reports them
  as warnings unless `--strict-images` is used.

## Hashing Contract

The importer and validator compute two source-level hashes:

- `definition_hash`: SHA-256 of normalized card definitions sorted by `card_id`.
  This excludes `image_file` and `notes`.
- `image_manifest_hash`: SHA-256 of normalized image entries sorted by card id,
  including image path and image file SHA-256 when the file exists.

Multiplayer setup must compare:

- same `pack_id`
- same `source_version`
- same `definition_hash`
- same `image_manifest_hash` when image parity matters

## Template

The template lives at:

```text
data/templates/custom_pack/
```

Validate it with:

```powershell
python tools\data\validate_custom_pack_schema.py data\templates\custom_pack
```

Import it into a runtime pack with:

```powershell
python tools\data\import_custom_pack.py data\templates\custom_pack --output-dir work\custom_pack_import --overwrite
```

## Importer Output

The importer should produce:

```text
data/packs/<pack_id>/
  manifest.json
  cards.sqlite
  asset_index.json
```

The SQLite schema should remain compatible with `SqliteCardRepository` unless a
future migration explicitly updates the Unity data access contract.

For schema v2 sources, the runtime manifest additionally preserves:

- `source_schema_version`
- `source_capabilities`
- `source_ruleset_profile`
- `source_abilities_file`
- `source_ability_count`
- `source_ability_data_hash`
- `source_formats_file`
