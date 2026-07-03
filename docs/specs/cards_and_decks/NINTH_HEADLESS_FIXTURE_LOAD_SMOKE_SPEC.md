# Ninth Fixture Headless Load Smoke Spec

Milestone: `M70-03`

## Purpose

`M70-03` verifies that the ninth offline runtime/test fixture can be prepared
for headless loading from the M70-02 count-line deck text export.

The smoke is scaffold-safe. Unit tests build the fixture, M70-02 report, and
deck text in memory, then verify deck-code generation and round-trip behavior.
The real CLI artifacts remain gated until real M69-06, M70-01, and M70-02
files exist.

## Inputs

- `outputs/target_slice/runtime_fixtures/m68_recipe_001_aqua_force_m69_06.json`
- `outputs/target_slice/m70_02_ninth_fixture_deck_text_export.json`
- `outputs/target_slice/m70_02_ninth_fixture_deck_text_export.txt`
- `data/packs/vanguard_th/cards.sqlite`

Optional Unity/headless proof:

- `outputs/target_slice/m70_03_ninth_fixture_unity_result.json`
- `outputs/target_slice/m70_03_ninth_fixture_unity_replay.json`

## Outputs

- `outputs/target_slice/m70_03_ninth_fixture_deck_code.txt`
- `outputs/target_slice/m70_03_ninth_fixture_load_smoke.json`
- `outputs/target_slice/m70_03_ninth_fixture_load_smoke.md`

These outputs are written only when the CLI is run against existing real
artifacts.

## Checks

The smoke must verify:

- M70-02 export summary is ready
- count-line deck text parses without issues
- main deck quantities match the runtime fixture exactly
- all deck text card ids exist in SQLite
- generated deck code round-trips to the same main deck
- Ride section remains empty
- G section remains empty
- optional Unity result is accepted only when `accepted = true` and
  `deck_source = deck_code`

## Boundary

This milestone must not:

- mutate saved player decks
- publish the fixture to UI deck selection
- enable bot playbook behavior
- enable G Zone runtime
- enable Stride runtime
- enable Aqua Force battle-order runtime
- enable battle-count tracker runtime
- enable attack-order predicate runtime
- enable multi-attack label runtime
- parse live card text
- mutate `GameState` from Python

## Gate Semantics

`offline_load_ready = true` means the review-only deck text can be parsed and
converted into a deck code locally.

`ready_for_m70_04 = true` requires both `offline_load_ready = true` and a
provided Unity/headless result proving that the generated deck code was
accepted with `deck_source = deck_code`.

## Verification

Scaffold-safe verification:

```powershell
python -m unittest tests.test_ninth_headless_fixture_load_smoke
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M69-06, M70-01, and M70-02 outputs exist:

```powershell
python tools\deck\build_ninth_headless_fixture_load_smoke.py
```

## Done Rule

`M70-03` scaffold work is ready when offline load smoke passes, deck code can
round-trip, invalid M70-02 reports and non-empty G sections are rejected,
optional Unity result gates M70-04 correctly, docs are updated, and real
artifacts remain gated until required input files exist.
