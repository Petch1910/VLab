# Second Headless Fixture Load Smoke Spec

Milestone: `M42-03`

## Purpose

`M42-03` verifies that the Oracle Think Tank offline runtime fixture can be
loaded through non-UI headless preparation without entering the saved deck
library, PlayTable UI, or bot runtime.

The smoke starts from the `M42-02` count-line deck text, parses it, checks it
against the runtime fixture and SQLite pack, creates a compact `VGTH1.` deck
code artifact, and records optional Unity headless evidence.

## Inputs

- `outputs/target_slice/runtime_fixtures/m40_recipe_001_classic_core_oracle_think_tank_m41_04.json`
- `outputs/target_slice/m42_02_second_fixture_deck_text_export.json`
- `outputs/target_slice/m42_02_second_fixture_deck_text_export.txt`
- `data/packs/vanguard_th/cards.sqlite`

Unity evidence:

- `outputs/target_slice/m42_03_second_fixture_unity_result.json`
- `outputs/target_slice/m42_03_second_fixture_unity_replay.json`

## Outputs

- `outputs/target_slice/m42_03_second_fixture_deck_code.txt`
- `outputs/target_slice/m42_03_second_fixture_load_smoke.json`
- `outputs/target_slice/m42_03_second_fixture_load_smoke.md`

## Checks

The smoke must verify:

- `M42-02` export is ready
- count-line deck text parses with `[Main]`, `[Ride]`, and `[G]` sections
- deck text main entries exactly match the runtime fixture
- all card ids exist in SQLite
- generated `VGTH1.` deck code round-trips through the Python mirror of the
  Unity codec payload shape
- Unity headless result is accepted and uses `deck_source = deck_code`

## Boundary

This milestone must not:

- add a saved deck
- mutate the UI deck library
- enable bot playbooks
- mutate `GameState` from Python
- promote the fixture into live runtime deck selection

## Verification

```powershell
python tools\deck\build_second_headless_fixture_load_smoke.py
python tools\deck\build_second_headless_fixture_load_smoke.py `
  --unity-result outputs\target_slice\m42_03_second_fixture_unity_result.json `
  --unity-replay outputs\target_slice\m42_03_second_fixture_unity_replay.json
python -m unittest tests.test_second_headless_fixture_load_smoke
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M42-03` is done when offline load smoke passes, the deck code artifact is
created, Unity headless evidence is accepted, targeted/full Python tests pass,
and the next target is `M42-04`.
