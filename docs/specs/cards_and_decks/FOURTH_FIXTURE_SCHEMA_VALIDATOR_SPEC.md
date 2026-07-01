# Fourth Fixture Schema Validator Spec

Milestone: `M50-01`

## Purpose

`M50-01` validates the fourth offline runtime/test fixture independently from
the M49 promotion and closeout generators.

The validator reads the Royal Paladin G-series fixture and runtime SQLite card
database, then recomputes deck counts, trigger counts, grade counts, copy
limits, selected group membership, runtime boundary flags, and the accepted
main-deck-only G Zone boundary.

## Inputs

- `outputs/target_slice/runtime_fixtures/m48_recipe_001_g_series_first_royal_paladin_m49_05.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m50_01_fourth_fixture_schema_validation.json`
- `outputs/target_slice/m50_01_fourth_fixture_schema_validation.md`

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
- G Zone and Stride boundaries remain disabled:
  - `format_policy.g_zone_boundary = main_deck_only_for_current_windows_fixture`
  - `g_zone_boundary.selected_option_id = main_deck_only_for_current_windows_fixture`
  - `g_zone_boundary.main_deck_only_validation_allowed = true`
  - `g_zone_boundary.g_zone_runtime_enabled = false`
  - `g_zone_boundary.stride_runtime_enabled = false`
  - `g_zone_boundary.grade4_main_deck_allowed = false`
  - `g_zone_boundary.g_units_allowed_in_main_deck = false`
  - `runtime_boundaries.g_zone_runtime_enabled = false`
  - `runtime_boundaries.stride_runtime_enabled = false`
  - `count_summary.grade4_main_deck_count = 0`

## Boundary

This milestone must not:

- mutate the fixture artifact
- inject saved player decks
- publish UI deck library entries
- enable bot playbook behavior
- enable G Zone or Stride runtime
- mutate `GameState`

## Verification

```powershell
python tools\deck\validate_fourth_runtime_fixture_schema.py
python -m unittest tests.test_fourth_runtime_fixture_schema_validator
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M50-01` is done when the validator reports the fourth fixture valid, tests
cover valid and invalid cases, docs are updated, and the next target is
`M50-02`.
