# Runtime Ability Registry Spec

## Status

Implemented in `M12-03`.

## Purpose

Load validated `ability_schema_v1` JSON into Unity runtime as read-only
structured ability data. The registry maps abilities by `ability_id` and
`card_id` without executing effects or mutating `GameState`.

## Runtime Surface

`RuntimeAbilityRegistry.LoadFromJson(json)` returns a
`RuntimeAbilityRegistryLoadResult` with:

- `accepted`
- `rejection_reason`
- `ability_count`
- `card_count`
- `summary`
- non-serialized `registry`

`RuntimeAbilityRegistry` provides:

- `FindAbility(abilityId)`
- `GetAbilitiesForCard(cardId)`
- `AbilityCount`
- `CardCount`

Returned abilities are clones, so UI, bot, tests, or future runtime code cannot
mutate the registry's source data accidentally.

## Loaded Data Model

The Unity model mirrors `ability_schema_v1`:

- `StructuredAbilityPack`
- `StructuredAbility`
- `StructuredAbilityTrigger`
- `StructuredAbilityTiming`
- `StructuredAbilityCost`
- `StructuredAbilityTarget`
- `StructuredAbilityEffect`
- `StructuredAbilityDuration`

The model keeps string enum values from JSON for now. Template execution and
typed enum conversion are later M12 tasks.

## Rejections

The registry rejects:

- missing JSON
- invalid JSON
- schema version mismatch
- missing ability id
- missing card id
- duplicate ability id

## Boundary

M12-03 does not:

- validate every schema rule again in Unity
- execute ability costs or effects
- append `GameEvent`
- auto-convert Thai card text
- replace existing `AbilityDefinition` scaffolds
- publish ability data over network

Runtime ability data must still be validated by `tools/data/validate_ability_schema.py`
before loading.

## Verification

EditMode coverage verifies:

- valid packs index by card id and ability id
- manual fallback is preserved
- returned ability data is cloned
- duplicate ability ids reject
- missing JSON and wrong schema reject
- load result JSON round-trip omits registry payload

## Next Work

`M12-04` adds Cost template v1.
