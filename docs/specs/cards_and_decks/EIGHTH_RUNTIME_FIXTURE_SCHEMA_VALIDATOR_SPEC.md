# Eighth Runtime Fixture Schema Validator Spec

Milestone: `M66-01`

## Purpose

`M66-01` validates the eighth offline runtime/test fixture independently from
the M65 promotion gate and closeout generators.

The validator is scaffold-safe: unit tests may build the eighth fixture in
memory from the M65-03 through M65-06 chain, while the real CLI artifact remains
gated until the M65-06 runtime fixture file exists.

## Inputs

- `outputs/target_slice/runtime_fixtures/m64_recipe_001_kagero_m65_06.json`
- `data/packs/vanguard_th/cards.sqlite`

The default fixture path is expected only after the M65-06 real output is
generated. Before that, tests pass an in-memory fixture directly.

## Outputs

- `outputs/target_slice/m66_01_eighth_fixture_schema_validation.json`
- `outputs/target_slice/m66_01_eighth_fixture_schema_validation.md`

These outputs are written only when the CLI is run against an existing fixture
file.

## Checks

The validator must verify:

- `schema_version = deck_recipe_runtime_fixture_v1`
- `fixture_scope = offline_runtime_test_fixture`
- all required top-level fields exist, accepting M65-06 `source_artifacts`
  plural as a compatibility alias for the shared fixture schema validator
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
  - `lock_runtime_enabled = false`
  - `unlock_runtime_enabled = false`
  - `legion_runtime_enabled = false`
  - `mate_identity_check_enabled = false`
- system boundaries remain explicit:
  - `selected_lock_option_id = main_deck_only_review_no_runtime_promotion`
  - `selected_legion_option_id = main_deck_only_review_no_runtime_promotion`
  - `main_deck_only_validation_allowed = true`
  - `lock_boundary_applied = true`
  - `legion_boundary_applied = true`
  - `lock_runtime_enabled = false`
  - `unlock_runtime_enabled = false`
  - `locked_card_visibility_enabled = false`
  - `lock_circle_state_enabled = false`
  - `unlock_timing_runtime_enabled = false`
  - `legion_runtime_enabled = false`
  - `mate_identity_check_enabled = false`
  - `legion_state_enabled = false`
  - `legion_attack_timing_enabled = false`
  - `legion_deck_building_validation_enabled = false`
  - `grade4_main_deck_count = 0`

## Boundary

This milestone must not:

- mutate the fixture artifact
- inject saved player decks
- publish UI deck library entries
- enable bot playbook behavior
- enable Lock/Unlock runtime
- enable Legion/Mate runtime
- enable Mate identity checks
- mutate `GameState`

## Verification

Scaffold-safe verification:

```powershell
python -m unittest tests.test_eighth_runtime_fixture_schema_validator
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M65-06 output exists:

```powershell
python tools\deck\validate_eighth_runtime_fixture_schema.py
```

## Done Rule

`M66-01` scaffold work is ready when the validator and tests accept the
in-memory M65-06 fixture, reject unsafe Lock/Unlock/Legion/Mate boundaries,
update docs, and keep the real CLI artifact gated until the fixture file
exists.
