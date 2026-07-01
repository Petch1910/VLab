# Ability Schema Validator Spec

## Status

Implemented in `M12-02`.

## Purpose

Validate `ability_schema_v1` JSON files before they are allowed into runtime
packs or Unity loading. This keeps structured ability data deterministic and
reviewable before effect templates mutate game state.

## Runtime Surface

Validator script:

```powershell
python tools\data\validate_ability_schema.py data\templates\ability_schema_v1\sample_abilities.json
```

Machine-readable report:

```powershell
python tools\data\validate_ability_schema.py data\templates\ability_schema_v1\sample_abilities.json --json
```

The report contains:

- `source_path`
- `ability_count`
- `validator_backend`
- `pydantic_version`
- `errors`
- `warnings`
- `all_ok`

## Pydantic Boundary

The script detects Pydantic when it is installed and reports the backend. The
current workspace does not pin or install Pydantic yet, so the validator also
has a built-in explicit validation path with the same checks. Pinning Pydantic
as a required dependency should happen in a later dependency/CI task.

## Validation Rules

The validator checks:

- `schema_version == ability_schema_v1`
- `abilities` is a list
- each ability has required sections
- non-empty `ability_id` and `card_id`
- duplicate `ability_id`
- valid ability kind
- trigger type, timing window, and source zone enums
- timing phase/window enums
- cost type and non-negative amount
- `once_per_turn` and `once_per_fight` costs require a key
- target id/type/owner/zone/count shape
- duplicate target id
- at least one effect
- effect type and optional zone enums
- duration type and cleanup timing enums

## Boundary

M12-02 does not:

- load ability data into Unity runtime
- compile ability data into runtime packs
- execute ability effects
- validate card ids against the Vanguard card database
- parse Thai free-text card text into structured abilities
- require Pydantic to be installed in the current workspace

## Verification

Python unit tests verify:

- sample ability file validates
- duplicate ability ids reject
- missing required sections reject
- invalid enums reject
- once-per-turn cost without key rejects

## Next Work

`M12-03` adds the Unity runtime ability registry.
