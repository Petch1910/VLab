# Multi-Fixture Scale Decision Spec

Milestone: `M42-04`

## Purpose

`M42-04` reviews the first two offline runtime fixtures before allowing any
third-slice work.

It consumes the Nova Grappler and Oracle Think Tank headless smoke evidence and
the existing archetype priority ranking. It may open a third-slice offline
pipeline, but it must not create runtime fixtures, saved decks, UI entries, or
bot/playbook data by itself.

## Inputs

- `outputs/target_slice/m39_03_headless_fixture_load_smoke.json`
- `outputs/target_slice/m42_03_second_fixture_load_smoke.json`
- `outputs/archetype_priority/archetype_priority_ranking.json`

## Outputs

- `outputs/target_slice/m42_04_multi_fixture_scale_decision.json`
- `outputs/target_slice/m42_04_multi_fixture_scale_decision.md`

## Decision Rules

The decision allows the next offline pipeline only when:

- two fixture evidence records exist
- both fixtures are offline-load ready
- both fixtures have generated deck code artifacts
- both Unity headless smokes passed with `deck_source=deck_code`
- both fixtures have `0` blocking issues
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
python tools\deck\build_multi_fixture_scale_decision.py
python -m unittest tests.test_multi_fixture_scale_decision
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M42-04` is done when the scale decision passes, the candidate queue excludes
completed fixture groups, tests cover pass and fail behavior, and the next
target is `M43-01`.
