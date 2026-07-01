# First Structured Card Pack Spec

## Status

Implemented in `M12-10`.

## Purpose

Create the first small structured ability data pack that exercises the supported
M12 templates end to end:

- Python ability schema validation
- Unity `RuntimeAbilityRegistry` loading
- structured ability fixture execution

## Pack File

`data/packs/vanguard_th/abilities/structured_ability_pack_m12_10.json`

The pack contains 20 ability entries mapped to real Vanguard TH card ids from
the local card export. These entries are template smoke data only and are not
real card-effect transcriptions.

Every ability includes this note:

```text
M12-10 template smoke pack only; not a real card transcription.
```

## Supported Template Coverage

The pack covers:

- `draw`
- `counter_blast`
- `counter_charge`
- `power_plus`
- `critical_plus`

All entries keep `manual_fallback = true` so unsupported future execution paths
can still route to manual resolution.

## Verification

Python coverage verifies:

- the pack validates through `tools/data/validate_ability_schema.py`
- the pack has exactly 20 abilities
- ability ids and card ids are unique
- the effect set is limited to supported M12 templates
- every entry is marked as template smoke data

Unity EditMode coverage verifies:

- the actual pack file loads through `RuntimeAbilityRegistry`
- registry indexes 20 abilities and 20 card ids
- a draw ability from the pack runs through `StructuredAbilityFixtureRunner`
- a PowerPlus ability from the pack runs through
  `StructuredAbilityFixtureRunner`

## Boundary

M12-10 does not:

- claim these are official card effects
- auto-convert Thai card text
- attach the pack to production UI
- enable arbitrary ability automation
- remove manual fallback

## Next Work

`M12-11` should connect unsupported structured ability paths to the existing
manual resolution bridge explicitly.
