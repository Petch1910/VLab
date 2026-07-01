# Ability Schema v1 Spec

## Status

Implemented in `M12-01` as JSON Schema plus a sample ability pack.

## Purpose

Define the first structured card ability data contract before adding the
Python/Pydantic validator and Unity runtime registry. This keeps card ability
data deterministic, reviewable, and separate from runtime LLM decisions.

## Files

- Schema: `data/schemas/ability_schema_v1.json`
- Sample: `data/templates/ability_schema_v1/sample_abilities.json`
- Structural tests: `tests/test_ability_schema_v1.py`

## Top-Level Shape

```json
{
  "schema_version": "ability_schema_v1",
  "abilities": []
}
```

Each ability requires:

- `ability_id`
- `card_id`
- `kind`
- `trigger`
- `timing`
- `costs`
- `targets`
- `effects`
- `duration`
- `manual_fallback`

## Supported v1 Sections

Trigger:

- `manual`
- `on_timing`
- `on_event`
- `continuous`

Timing:

- `phase`
- `window`
- `optional`

Costs:

- `none`
- `counter_blast`
- `soul_blast`
- `energy_blast`
- `discard`
- `once_per_turn`
- `once_per_fight`

Targets:

- `none`
- `self`
- `unit`
- `circle`
- `card`

Effects:

- `manual`
- `draw`
- `move_zone`
- `counter_charge`
- `counter_blast`
- `soul_charge`
- `soul_blast`
- `power_plus`
- `critical_plus`

Duration:

- `instant`
- `until_end_of_battle`
- `until_end_of_turn`
- `continuous`
- `manual`

## Boundaries

M12-01 does not:

- validate ability files with Pydantic yet
- load ability files into Unity runtime
- execute the new schema
- parse Thai free-text card abilities automatically
- replace existing `AbilityDefinition` scaffolds

Unsupported or unconverted abilities must still use manual fallback until the
M12 validator, registry, templates, and fixture DSL are implemented.

## Verification

Python structural tests verify:

- schema top-level shape
- required ability sections
- enums needed by M12 cost/target/effect templates
- sample ability shape

## Next Work

`M12-02` adds the Python/Pydantic validator for ability schema v1.
