# M15 Custom Packs / Custom Formats Closeout

## Status

Closed in `M15-10`.

## Completed Scope

M15 extended the custom pack pipeline and added data-only custom format
contracts:

- `M15-01` Custom pack v2 schema envelope
- `M15-02` Ability data in custom packs
- `M15-03` Pack validation UI/status
- `M15-04` Pack manager enable/disable registry
- `M15-05` RuleSet / custom format profile model
- `M15-06` Standard format preset flags
- `M15-07` V-Premium format preset flags
- `M15-08` Premium format preset flags
- `M15-09` Read-only custom format sandbox
- `M15-10` Custom format validation closeout tests

## Current Supported Custom Pack Contract

Supported now:

- v1 custom pack template remains valid
- v2 custom pack metadata supports `capabilities`, `dependencies`,
  `ruleset_profile`, `abilities_file`, and `formats_file`
- v2 custom pack ability files are validated through `ability_schema_v1`
- ability `card_id` values must exist in the same custom pack
- runtime manifests preserve source schema/capability/ability metadata
- pack validation status is read-only and UI-safe
- local pack registry can enable/disable packs without deleting pack data

## Current Supported Custom Format Contract

Supported now:

- custom format profiles reference a base `RuleSetProfile`
- profiles validate duplicate format ids, aliases, and allowed pack ids
- Standard, V-Premium, and Premium presets delegate to the core RuleSet
  profile catalog
- custom format sandbox resolves ids and aliases to cloned base RuleSet
  summaries
- optional pack id checks are preview-only and stable

## Boundaries Still In Force

M15 does not:

- enforce deck construction legality per format
- enforce Standard, V-Premium, or Premium gameplay behavior
- load custom format files from disk at runtime
- switch the active game format automatically
- allow packs or formats to mutate `GameState`
- unblock ranked/secure multiplayer

M16 UI work may display these statuses, but gameplay enforcement should remain
behind future explicit rules milestones.

## Verification

Closeout verification:

```powershell
python tools\data\validate_custom_pack_schema.py data\templates\custom_pack
python tools\data\validate_custom_pack_schema.py data\templates\custom_pack_v2
python tools\data\import_custom_pack.py data\templates\custom_pack_v2 --output-dir work\custom_pack_v2_import --overwrite
python -m unittest discover -s tests -p "test_*.py"
```

Unity closeout verification:

```powershell
Unity batchmode compile
Unity EditMode tests
```

Expected latest results:

- v1 custom pack validation: passed with expected fallback-image warnings
- v2 custom pack validation: passed with expected fallback-image warnings
- v2 custom pack import: passed
- Python tests: `31/31`
- Unity compile: no compiler errors
- Unity EditMode: `762/762`

## Next Target

Move to `M16-01` PlayTable pending AUTO panel polish.

Keep M16 focused on UI/mobile polish. Do not add new gameplay enforcement while
polishing custom pack/format surfaces.
