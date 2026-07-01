# Custom Pack V2 Schema Spec

## Scope

`M15-01` adds a backward-compatible custom pack schema v2 envelope. `M15-02`
adds structured ability-file validation for that envelope. It does not change
card CSV parsing.

## Goals

- Keep schema v1 packs valid.
- Add explicit pack capabilities for cards, images, ability data, and custom
  formats.
- Preserve source schema metadata in runtime manifests.
- Prepare M15-02 ability data in custom packs without wiring ability parsing in
  this task.

## V2 Pack Metadata

`pack.json` may use:

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

## Validation Rules

- `schema_version` must be `1` or `2`.
- Schema v2 requires `capabilities`.
- `capabilities.cards` must be `true`.
- Supported capability keys are `cards`, `images`, `abilities`, and
  `custom_formats`.
- Capability values must be booleans.
- Unknown capability keys produce warnings, not errors.
- `ruleset_profile`, when present, must be `standard`, `v_premium`, `premium`,
  `mixed`, or `custom`.
- If `capabilities.abilities` is true, `abilities_file` is required, must be a
  safe relative `.json` path, must exist, and must pass `ability_schema_v1`.
- Ability entries may only reference `card_id` values present in the same
  custom pack.
- If `capabilities.custom_formats` is true, `formats_file` is required, must be
  a safe relative `.json` path, and must exist.
- `dependencies`, when present, must be an array.
- Dependency `pack_id` values use the same pattern as pack ids.
- Dependency hashes, when present, must be SHA-256 hex strings.

## Runtime Manifest Preservation

The importer keeps runtime manifest schema version at `1` for compatibility and
adds source metadata:

- `source_schema_version`
- `source_capabilities`
- `source_ruleset_profile`
- `source_abilities_file`
- `source_ability_count`
- `source_ability_data_hash`
- `source_formats_file`

## Non-Goals

- No Unity runtime execution of custom abilities.
- No custom format/ruleset execution. That starts in later M15 tasks.
- No SQLite schema migration.
- No Unity runtime pack manager changes.

## Verification

Python unit tests cover:

- v1 template still validates
- v2 template validates
- v2 rejects missing `capabilities.cards`
- v2 rejects unsafe `abilities_file`
- v2 rejects invalid ability schema
- v2 rejects ability `card_id` values missing from the pack
- v2 rejects invalid dependency pack ids
- v2 import preserves source metadata in the runtime manifest
