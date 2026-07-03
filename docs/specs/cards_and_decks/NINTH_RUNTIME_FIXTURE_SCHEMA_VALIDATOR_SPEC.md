# Ninth Runtime Fixture Schema Validator Spec

Milestone: `M70-01`

## Purpose

`M70-01` validates the ninth offline runtime/test fixture independently from
the M69 promotion gate and closeout generators.

The validator is scaffold-safe: unit tests may build the ninth fixture in
memory from the M69 chain, while the real CLI artifact remains gated until the
M69-06 runtime fixture file exists.

## Inputs

- `outputs/target_slice/runtime_fixtures/m68_recipe_001_aqua_force_m69_06.json`
- `data/packs/vanguard_th/cards.sqlite`

The default fixture path is expected only after the M69-06 real output is
generated. Before that, tests pass an in-memory fixture directly.

## Outputs

- `outputs/target_slice/m70_01_ninth_fixture_schema_validation.json`
- `outputs/target_slice/m70_01_ninth_fixture_schema_validation.md`

These outputs are written only when the CLI is run against an existing fixture
file.

## Checks

The validator must verify:

- `schema_version = deck_recipe_runtime_fixture_v1`
- `fixture_scope = offline_runtime_test_fixture`
- all required top-level fields exist, accepting M69-06 `source_artifacts`
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
  - `saved_deck_injection = false`
  - `bot_playbook_enabled = false`
  - `ui_deck_library_mutated = false`
  - `game_state_mutated = false`
  - `g_zone_runtime_enabled = false`
  - `stride_runtime_enabled = false`
  - `aqua_force_battle_order_runtime_enabled = false`
- system boundaries remain explicit:
  - `selected_g_zone_option_id = main_deck_only_review_no_runtime_promotion`
  - `selected_stride_option_id = main_deck_only_review_no_runtime_promotion`
  - `selected_aqua_force_option_id = manual_semantic_review_only_no_runtime_promotion`
  - `main_deck_only_validation_allowed = true`
  - `g_zone_boundary_applied = true`
  - `stride_boundary_applied = true`
  - `aqua_force_boundary_applied = true`
  - `g_zone_runtime_enabled = false`
  - `stride_runtime_enabled = false`
  - `aqua_force_battle_order_runtime_enabled = false`
  - `g_zone_slot_model_enabled = false`
  - `stride_deck_building_validation_enabled = false`
  - `generation_break_runtime_enabled = false`
  - `battle_count_tracker_enabled = false`
  - `attack_order_predicate_runtime_enabled = false`
  - `multi_attack_label_runtime_enabled = false`
  - `grade4_main_deck_count = 0`
  - `grade4_main_deck_allowed = false`
  - `g_units_allowed_in_main_deck = false`
  - `g_zone_cards_allowed_in_current_windows_fixture = false`

## Boundary

This milestone must not:

- mutate the fixture artifact
- inject saved player decks
- publish UI deck library entries
- enable bot playbook behavior
- parse live card text
- enable G Zone runtime
- enable Stride runtime
- enable Aqua Force battle-order runtime
- mutate `GameState`

## Verification

Scaffold-safe verification:

```powershell
python -m unittest tests.test_ninth_runtime_fixture_schema_validator
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M69-06 output exists:

```powershell
python tools\deck\validate_ninth_runtime_fixture_schema.py
```

## Done Rule

`M70-01` scaffold work is ready when the validator and tests accept the
in-memory M69-06 fixture, reject unsafe G Zone / Stride / Aqua Force
boundaries, reject saved deck injection, update docs, and keep the real CLI
artifact gated until the fixture file exists.
