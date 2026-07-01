# Second Fixture Schema Validator Spec

Milestone: `M42-01`

## Purpose

`M42-01` validates the Oracle Think Tank runtime fixture independently from the
M41 generator.

The validator reads the second fixture and runtime SQLite card database, then
recomputes deck counts, trigger counts, grade counts, copy limits, selected
group membership, and runtime boundary flags.

## Inputs

- `outputs/target_slice/runtime_fixtures/m40_recipe_001_classic_core_oracle_think_tank_m41_04.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m42_01_second_fixture_schema_validation.json`
- `outputs/target_slice/m42_01_second_fixture_schema_validation.md`

## Checks

The validator must verify:

- `schema_version = deck_recipe_runtime_fixture_v1`
- `fixture_scope = offline_runtime_test_fixture`
- all required top-level fields exist
- main deck quantity total is `50`
- trigger profile is `Critical=4`, `Draw=4`, `Heal=4`, `Stand=4`
- grade profile is `G0=17`, `G1=14`, `G2=11`, `G3=8`
- all card ids exist in SQLite
- all quantities respect SQLite `deck_limit`
- all cards belong to the selected group/clan
- runtime boundaries remain safe:
  - `test_fixture_only = true`
  - `auto_injected_into_player_decks = false`
  - `bot_playbook_enabled = false`
  - `ui_deck_library_mutated = false`
  - `game_state_mutated = false`

## Boundary

This milestone must not:

- mutate the fixture artifact
- inject saved player decks
- enable UI deck library entries
- enable bot playbook behavior
- mutate `GameState`

## Verification

```powershell
python tools\deck\validate_second_runtime_fixture_schema.py
python -m unittest tests.test_second_runtime_fixture_schema_validator
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M42-01` is done when the validator reports the second fixture valid, tests
cover valid and invalid cases, and the next target is `M42-02`.
