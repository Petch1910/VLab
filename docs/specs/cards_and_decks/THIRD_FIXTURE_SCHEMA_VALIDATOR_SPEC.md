# Third Fixture Schema Validator Spec

Milestone: `M46-01`

## Purpose

`M46-01` validates the third offline runtime/test fixture independently from
the M45 promotion generator.

The validator reads the third fixture and runtime SQLite card database, then
recomputes deck counts, trigger counts, grade counts, copy limits, selected
group membership, and runtime boundary flags.

## Inputs

- `outputs/target_slice/runtime_fixtures/m44_recipe_001_link_joker_legion_bermuda_triangle_m45_04.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m46_01_third_fixture_schema_validation.json`
- `outputs/target_slice/m46_01_third_fixture_schema_validation.md`

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
- publish UI deck library entries
- enable bot playbook behavior
- mutate `GameState`

## Verification

```powershell
python tools\deck\validate_third_runtime_fixture_schema.py
python -m unittest tests.test_third_runtime_fixture_schema_validator
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M46-01` is done when the validator reports the third fixture valid, tests
cover valid and invalid cases, docs are updated, and the next target is
`M46-02`.
