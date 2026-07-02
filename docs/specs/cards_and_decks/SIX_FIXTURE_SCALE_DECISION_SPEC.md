# Six-Fixture Scale Decision Spec

Milestone: `M58-04`

## Purpose

`M58-04` reviews the first six offline runtime/test fixtures before allowing
any seventh-slice work.

It consumes Nova Grappler, Oracle Think Tank, Bermuda Triangle, Royal Paladin
G-series, Gold Paladin, and Shadow Paladin headless smoke evidence plus the
existing archetype priority ranking. It may open a seventh-slice offline
pipeline, but it must not create runtime fixtures, saved decks, UI entries,
bot/playbook data, G Zone runtime, or Stride runtime by itself.

The implementation is scaffold-safe: tests may use an in-memory sixth fixture
and in-memory M58-03 smoke report. Real CLI artifacts remain gated until the
M58-03 real smoke report and Unity evidence exist.

## Inputs

- `outputs/target_slice/m39_03_headless_fixture_load_smoke.json`
- `outputs/target_slice/m42_03_second_fixture_load_smoke.json`
- `outputs/target_slice/m46_03_third_fixture_load_smoke.json`
- `outputs/target_slice/m50_03_fourth_fixture_load_smoke.json`
- `outputs/target_slice/m54_03_fifth_fixture_load_smoke.json`
- `outputs/target_slice/m58_03_sixth_fixture_load_smoke.json`
- `outputs/archetype_priority/archetype_priority_ranking.json`

## Outputs

- `outputs/target_slice/m58_04_six_fixture_scale_decision.json`
- `outputs/target_slice/m58_04_six_fixture_scale_decision.md`

These outputs are written only when the CLI is run against existing upstream
artifacts.

## Decision Rules

The decision allows the next offline pipeline only when:

- six fixture evidence records exist
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
- enable G Zone runtime
- enable Stride runtime
- mutate `GameState`

## Verification

Scaffold-safe verification:

```powershell
python -m unittest tests.test_six_fixture_scale_decision
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M58-03 output exists:

```powershell
python tools\deck\build_six_fixture_scale_decision.py
```

## Done Rule

`M58-04` scaffold work is ready when the scale decision passes with in-memory
sixth fixture evidence, the candidate queue excludes completed fixture groups,
tests cover pass and fail behavior, docs are updated, and real artifacts remain
gated until M58-03 evidence files exist.
