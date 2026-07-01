# Fourth-Slice Fixture/Format Readiness Spec

Milestone: `M47-02`

## Purpose

`M47-02` checks whether the selected fourth offline slice can proceed to the
semantic/compatibility probe.

The selected target is `รอยัล พาลาดิน` in the `g_series_first` era. The check
is source-backed from runtime SQLite only and must surface readiness gaps
instead of forcing a semantic probe.

## Inputs

- `outputs/target_slice/m47_01_fourth_target_slice_selection.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m47_02_fourth_slice_fixture_readiness.json`
- `outputs/target_slice/m47_02_fourth_slice_fixture_readiness.md`

## Checks

The readiness report must compute:

- era series scope for `g_series_first`
- source-backed card count
- series counts
- grade counts
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
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_fourth_slice_fixture_readiness.py
python -m unittest tests.test_fourth_slice_fixture_readiness
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M47-02` is done when the report records readiness truthfully, tests cover
ready blockers and invalid inputs, docs are updated, and the next target is
`M47-repair` when blockers exist.
