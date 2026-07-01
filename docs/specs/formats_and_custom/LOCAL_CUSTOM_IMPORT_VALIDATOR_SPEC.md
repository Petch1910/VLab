# Local Custom Import Validator Spec

Milestone: `M24-06`

## Purpose

Validate a user-provided local custom import package before any staging or
runtime pack import happens.

This validator implements the safe first step after
`VANGPRO_LIKE_CUSTOM_IMPORT_SPEC.md`:

- Read `manifest.json`.
- Validate declared relative paths.
- Verify SHA-256 hashes for declared files.
- Parse `cards.csv`.
- Inspect `images.zip` without extracting it.
- Report counts, warnings, and errors for Pack Validation UI.

## Inputs

Supported input shapes:

```text
custom_import/
  manifest.json
  cards.csv
  images.zip        optional
  abilities.json    optional

custom_import.zip
  manifest.json
  cards.csv
  images.zip        optional
  abilities.json    optional
```

The package manifest must use schema `vanguard-custom-import-v1`.

## Safety Boundaries

- Do not extract `images.zip`.
- Do not transform files into staging yet.
- Do not import into `data/packs`.
- Do not mutate active runtime packs.
- Do not execute scripts.
- Do not auto-download remote files.
- Reject absolute paths, drive-letter paths, and path traversal.
- Reject unsafe members inside `images.zip`.
- Reject `.xlsx` until parser support is implemented.

## Required Card Columns

The first supported card source is `cards.csv` with these columns:

- `card_id`
- `name_th`
- `series`
- `clan`
- `card_type`
- `grade`
- `power`
- `shield`
- `critical`
- `trigger_type`
- `text_th`
- `image_relative_path`

Missing image files are warnings, not errors, unless a later strict mode is
added.

## Report Contract

The validator returns a JSON-friendly report:

```json
{
  "adapter": "vangpro_like_local_import_v1",
  "accepted": true,
  "source": "path",
  "pack_id": "local_custom_pack",
  "cards_file": "cards.csv",
  "images_zip": "images.zip",
  "abilities_file": "abilities.json",
  "card_count": 2,
  "image_count": 2,
  "missing_image_count": 0,
  "unsupported_field_count": 0,
  "errors": [],
  "warnings": [],
  "validation_handoff": {
    "custom_pack_schema_validator": "tools/data/validate_custom_pack_schema.py",
    "custom_pack_importer": "tools/data/import_custom_pack.py",
    "direct_runtime_mutation": false
  }
}
```

## Verification

Unit tests must cover:

- valid local package
- missing manifest
- unsupported `.xlsx`
- missing declared file
- SHA-256 mismatch
- unsafe manifest path
- unsafe `images.zip` member
- invalid optional abilities JSON/schema
- failed validation does not extract or mutate files

