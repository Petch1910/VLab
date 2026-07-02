# Seventh Fixture Schema Validator Spec

Milestone: `M62-01`

## Purpose

`M62-01` validates the seventh offline runtime/test fixture independently from
the M61 promotion gate and closeout generators.

The validator is scaffold-safe: unit tests may build the seventh fixture in
memory from the M61-03 through M61-06 chain, while the real CLI artifact remains
gated until the M61-06 runtime fixture file exists.

## Inputs

- `outputs/target_slice/runtime_fixtures/m60_recipe_001_neo_nectar_m61_06.json`
- `data/packs/vanguard_th/cards.sqlite`

The default fixture path is expected only after the M61-06 real output is
generated. Before that, tests pass an in-memory fixture directly.

## Outputs

- `outputs/target_slice/m62_01_seventh_fixture_schema_validation.json`
- `outputs/target_slice/m62_01_seventh_fixture_schema_validation.md`

These outputs are written only when the CLI is run against an existing fixture
file.

## Checks

The validator must verify:

- `schema_version = deck_recipe_runtime_fixture_v1`
- `fixture_scope = offline_runtime_test_fixture`
- all required top-level fields exist, accepting M61-06 `source_artifacts`
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
  - `g_zone_runtime_enabled = false`
  - `stride_runtime_enabled = false`
  - `bloom_token_runtime_enabled = false`
- system boundaries remain explicit:
  - `selected_g_zone_option_id = main_deck_only_review_no_runtime_promotion`
  - `selected_bloom_token_option_id = manual_semantic_review_only_no_runtime_promotion`
  - `main_deck_only_validation_allowed = true`
  - `g_zone_boundary_applied = true`
  - `bloom_token_boundary_applied = true`
  - `grade4_main_deck_allowed = false`
  - `g_units_allowed_in_main_deck = false`
  - `tokens_allowed_in_main_deck = false`
  - `bloom_template_runtime_enabled = false`
  - `token_lifecycle_runtime_enabled = false`
  - `same_name_runtime_tracking_enabled = false`
  - `duration_cleanup_runtime_enabled = false`
  - `grade4_main_deck_count = 0`

## Boundary

This milestone must not:

- mutate the fixture artifact
- inject saved player decks
- publish UI deck library entries
- enable bot playbook behavior
- enable G Zone runtime
- enable Stride runtime
- enable Bloom/token runtime
- enable token lifecycle runtime
- enable same-name runtime tracking
- mutate `GameState`

## Verification

Scaffold-safe verification:

```powershell
python -m unittest tests.test_seventh_runtime_fixture_schema_validator
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M61-06 output exists:

```powershell
python tools\deck\validate_seventh_runtime_fixture_schema.py
```

## Done Rule

`M62-01` scaffold work is ready when the validator and tests accept the
in-memory M61-06 fixture, reject unsafe G Zone/Stride/Bloom/token boundaries,
update docs, and keep the real CLI artifact gated until the fixture file
exists.
