# Pack Validation Status Spec

## Scope

`M15-03` adds a read-only runtime validation status surface for loaded card pack
manifests. It reports pack health to UI/tests but does not enable, disable,
import, delete, or mutate packs.

## Inputs

- `CardPackManifest`
- runtime SQLite file existence
- asset index file existence

## Output

`CardPackValidationStatus` includes:

- accepted flag
- pack id and source version
- runtime schema version
- source schema version
- card/image/ability counts
- capability summary
- issue list
- compact summary string

Issue severity values:

- `Info`
- `Warning`
- `Error`

The pack is accepted when there are no `Error` issues.

## Validation Rules

Common rules:

- manifest must exist
- runtime manifest `schema_version` must be `1`
- `pack_id` must be present
- `card_count` must be greater than zero
- `definition_hash` must be present
- missing `image_manifest_hash` is a warning
- missing SQLite database is an error
- missing asset index is a warning

Schema v2 source metadata rules:

- missing `source_capabilities` is a warning
- `source_capabilities.cards` must be true
- if ability capability is true, `source_abilities_file` must be present
- if ability capability is true, `source_ability_data_hash` must be present
- zero ability count with ability capability is a warning

## UI Surface

The card browser status text appends the compact validation status:

```text
Pack validation: OK | schema 2 | cards 2 | abilities 1 | caps cards,images,abilities | errors 0 | warnings 0
```

## Boundaries

M15-03 does not:

- run Python validators from Unity
- enable or disable packs
- import or delete packs
- change selected pack
- mutate `GameState`
- publish network payloads

## Verification

EditMode tests cover:

- default Vanguard TH manifest accepted
- schema v2 capability and ability metadata status
- missing definition hash blocks status
- status JSON round-trip
