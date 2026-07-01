# Three-Fixture Scale Decision Spec

Milestone: `M46-04`

## Purpose

`M46-04` reviews the first three offline runtime/test fixtures before allowing
any fourth-slice work.

It consumes the Nova Grappler, Oracle Think Tank, and Bermuda Triangle headless
smoke evidence plus the existing archetype priority ranking. It may open a
fourth-slice offline pipeline, but it must not create runtime fixtures, saved
decks, UI entries, or bot/playbook data by itself.

## Inputs

- `outputs/target_slice/m39_03_headless_fixture_load_smoke.json`
- `outputs/target_slice/m42_03_second_fixture_load_smoke.json`
- `outputs/target_slice/m46_03_third_fixture_load_smoke.json`
- `outputs/archetype_priority/archetype_priority_ranking.json`

## Outputs

- `outputs/target_slice/m46_04_three_fixture_scale_decision.json`
- `outputs/target_slice/m46_04_three_fixture_scale_decision.md`

## Decision Rules

The decision allows the next offline pipeline only when:

- three fixture evidence records exist
- all fixtures are offline-load ready
- all fixtures have generated deck code artifacts
- all Unity headless smokes passed with `deck_source=deck_code`
- all fixtures have `0` blocking issues
- at least one feasible candidate remains in the archetype priority ranking
  after excluding completed fixture groups

## Boundary

This milestone must not:

- select a live runtime deck
- create a runtime fixture
- inject saved player decks
- publish UI deck lists
- enable bot playbooks
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_three_fixture_scale_decision.py
python -m unittest tests.test_three_fixture_scale_decision
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M46-04` is done when the scale decision passes, the candidate queue excludes
completed fixture groups, tests cover pass and fail behavior, docs are updated,
and the next target is `M47-01`.
