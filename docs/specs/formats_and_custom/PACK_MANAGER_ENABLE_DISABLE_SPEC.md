# Pack Manager Enable Disable Spec

## Scope

`M15-04` adds a local, pure pack registry state for installed card packs and
enable/disable mutations. It does not delete pack files and does not change the
active pack automatically.

## Model

`CardPackRegistryState` contains `CardPackRegistryEntry` rows:

- `pack_id`
- `display_name`
- `source_version`
- `pack_directory`
- `definition_hash`
- `enabled`
- `validation_summary`

## Mutation Helper

`CardPackRegistryManager.SetEnabled(...)`:

- clones the registry state
- finds the requested pack by `pack_id` and optional `source_version`
- toggles only the cloned entry `enabled` flag
- returns accepted/rejected metadata
- preserves source state and pack directory strings

Stable rejection reasons:

- `PACK_REGISTRY_STATE_MISSING`
- `PACK_REGISTRY_PACK_MISSING`
- `PACK_REGISTRY_PACK_ID_MISSING`

## Boundaries

M15-04 does not:

- delete or move pack files
- import packs
- change the active card repository
- change deck card ids
- mutate `GameState`
- publish network payloads

## Verification

EditMode tests cover:

- entry creation from manifest and validation status
- enable/disable result returns a cloned state
- missing pack rejection preserves source state
- registry JSON round-trip
