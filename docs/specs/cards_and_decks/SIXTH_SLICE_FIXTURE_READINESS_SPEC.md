# Sixth-Slice Fixture/Format Readiness Spec

Milestone: `M55-02`

## Purpose

`M55-02` checks whether the selected sixth offline slice can proceed to the
semantic/compatibility probe.

The selected target is `ชาโดว์ พาลาดิน` in the `g_next_z` era. The check is
source-backed from runtime SQLite only and must surface readiness gaps instead
of forcing a semantic probe.

## Inputs

- `outputs/target_slice/m55_01_sixth_target_slice_selection.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m55_02_sixth_slice_fixture_readiness.json`
- `outputs/target_slice/m55_02_sixth_slice_fixture_readiness.md`

## Checks

The readiness report must compute:

- era series scope for `g_next_z`
- source-backed card count
- series counts
- grade counts including grade 4 / G-unit pool
- trigger counts
- trigger and non-trigger capacity
- trigger type gaps against `Critical`, `Draw`, `Heal`, `Stand`
- whether semantic probe is allowed

## Boundary

This milestone must not:

- create recipe drafts
- create runtime fixtures
- inject saved player decks
- publish UI deck lists
- enable bot playbooks
- mutate runtime packs
- enable G Zone or Stride runtime
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_sixth_slice_fixture_readiness.py
python -m unittest tests.test_sixth_slice_fixture_readiness
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M55-02` is done when the report records readiness truthfully, tests cover
ready and invalid inputs, docs are updated, and the next target is `M55-03`
when readiness passes.
