# VangPro-Like Custom Import Spec

Milestone: `M24-05`

## Purpose

Define a local custom import package workflow inspired by convenient
deck/card-management UX, without copying VangPro files, assets, data, code, or
exact formats.

The target user-provided package:

```text
custom_import.zip
  manifest.json
  cards.csv or cards.xlsx
  images.zip
  abilities.json        optional
```

## Hard Boundaries

- Do not copy VangPro data, assets, card images, icons, code, UI files, or
  proprietary package contents.
- Do not auto-download remote files.
- Do not execute scripts from the package.
- Do not import directly into the active runtime pack.
- Do not bypass custom pack schema validation.
- Do not treat `.xlsx` as supported until parser support is implemented.

## Manifest Shape

`manifest.json` uses this project's own local import manifest:

```json
{
  "schema": "vanguard-custom-import-v1",
  "pack_id": "local_custom_pack",
  "display_name": "Local Custom Pack",
  "source_version": "1.0.0",
  "language": "th",
  "format": "custom",
  "cards_file": "cards.csv",
  "images_zip": "images.zip",
  "abilities_file": "abilities.json",
  "sha256": {
    "cards.csv": "64-char-hex",
    "images.zip": "64-char-hex",
    "abilities.json": "64-char-hex"
  }
}
```

## Card Source Rules

Supported first:

- `cards.csv`

Reserved for a later parser:

- `cards.xlsx`

Required card columns should align with existing custom pack source columns:

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

## Image Package Rules

- `images.zip` is optional but must be declared if used.
- All extracted image paths must stay inside the staging directory.
- Supported image extensions follow existing custom pack importer behavior.
- Missing images become validation warnings unless strict mode is introduced
  later.
- Hash mismatch blocks import into staging.

## Staging Output

The validator/importer should stage into the existing custom pack v2 source
shape:

```text
work/vangpro_like_import/<pack_id>/
  pack.json
  cards.csv
  images/
  abilities.json
  import_report.json
```

## Validation Pipeline

1. Validate `manifest.json` shape.
2. Verify declared file paths are safe relative paths.
3. Verify declared SHA-256 hashes.
4. Extract `images.zip` into staging only after hash validation passes.
5. Transform source into custom pack v2 `pack.json`.
6. Run `validate_custom_pack_schema.py`.
7. Run `import_custom_pack.py` only after validation passes.
8. Show results in Pack Validation UI.

## Report Shape

```json
{
  "adapter": "vangpro_like_local_import_v1",
  "accepted": true,
  "pack_id": "local_custom_pack",
  "cards_file": "cards.csv",
  "images_zip": "images.zip",
  "card_count": 0,
  "image_count": 0,
  "warnings": [],
  "errors": []
}
```

## Non-Goals

- No VangPro file reverse engineering.
- No remote repository mirroring.
- No `.xlsx` parser in this spec milestone.
- No direct runtime pack mutation.
- No automatic structured ability conversion from free text.

## Acceptance For Future Implementation

- Local import validator exists.
- Unit tests cover valid package, missing manifest, unsupported `.xlsx`,
  missing declared file, hash mismatch, unsafe paths, image zip traversal, and
  validation handoff.
- Failed imports do not mutate active packs.
- Pack Validation UI shows manifest/hash/image/card-count results.
