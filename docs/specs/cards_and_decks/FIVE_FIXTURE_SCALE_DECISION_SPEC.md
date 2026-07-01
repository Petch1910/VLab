# Five-Fixture Scale Decision Spec

Milestone: `M54-04`

## Purpose

`M54-04` reviews the first five offline runtime/test fixtures before allowing
any sixth-slice work.

It consumes Nova Grappler, Oracle Think Tank, Bermuda Triangle, Royal Paladin
G-series, and Gold Paladin headless smoke evidence plus the existing archetype
priority ranking. It may open a sixth-slice offline pipeline, but it must not
create runtime fixtures, saved decks, UI entries, bot/playbook data, G Zone
runtime, or Stride runtime by itself.

## Inputs

- `outputs/target_slice/m39_03_headless_fixture_load_smoke.json`
- `outputs/target_slice/m42_03_second_fixture_load_smoke.json`
- `outputs/target_slice/m46_03_third_fixture_load_smoke.json`
- `outputs/target_slice/m50_03_fourth_fixture_load_smoke.json`
- `outputs/target_slice/m54_03_fifth_fixture_load_smoke.json`
- `outputs/archetype_priority/archetype_priority_ranking.json`

## Outputs

- `outputs/target_slice/m54_04_five_fixture_scale_decision.json`
- `outputs/target_slice/m54_04_five_fixture_scale_decision.md`

## Decision Rules

The decision allows the next offline pipeline only when:

- five fixture evidence records exist
- all fixtures are offline-load ready
- all fixtures have generated deck code artifacts
- all Unity headless smokes passed with `deck_source=deck_code`
- all fixtures have `0` blocking issues
- at least one feasible candidate remains in the archetype priority ranking
  after excluding completed fixture groups
- G Zone and Stride runtime remain disabled

## Boundary

This milestone must not:

- select a live runtime deck
- create a runtime fixture
- inject saved player decks
- publish UI deck lists
- enable bot playbooks
- enable G Zone or Stride runtime
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_five_fixture_scale_decision.py
python -m unittest tests.test_five_fixture_scale_decision
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M54-04` is done when the scale decision passes, the candidate queue excludes
completed fixture groups, tests cover pass and fail behavior, docs are updated,
and the next target is `M55-01`.
