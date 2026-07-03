# Nine-Fixture Scale Decision Spec

Milestone: `M70-04`

## Purpose

`M70-04` reviews the first nine offline runtime/test fixtures before any
further slice selection or queue expansion.

It consumes Nova Grappler, Oracle Think Tank, Bermuda Triangle, Royal Paladin
G-series, Gold Paladin, Shadow Paladin, Neo Nectar, Kagero, and Aqua Force
headless smoke evidence plus the existing archetype priority ranking.

This milestone does not select a tenth slice. If all nine fixture evidence
records pass, it opens only `M71-01` post-nine fixture queue planning so the
next queue can be defined explicitly.

The implementation is scaffold-safe: tests may use in-memory sixth, seventh,
eighth, and ninth fixture smoke reports. Real CLI artifacts remain gated until
the M58-03, M62-03, M66-03, and M70-03 real smoke reports plus Unity evidence
exist.

## Inputs

- `outputs/target_slice/m39_03_headless_fixture_load_smoke.json`
- `outputs/target_slice/m42_03_second_fixture_load_smoke.json`
- `outputs/target_slice/m46_03_third_fixture_load_smoke.json`
- `outputs/target_slice/m50_03_fourth_fixture_load_smoke.json`
- `outputs/target_slice/m54_03_fifth_fixture_load_smoke.json`
- `outputs/target_slice/m58_03_sixth_fixture_load_smoke.json`
- `outputs/target_slice/m62_03_seventh_fixture_load_smoke.json`
- `outputs/target_slice/m66_03_eighth_fixture_load_smoke.json`
- `outputs/target_slice/m70_03_ninth_fixture_load_smoke.json`
- `outputs/archetype_priority/archetype_priority_ranking.json`

## Outputs

- `outputs/target_slice/m70_04_nine_fixture_scale_decision.json`
- `outputs/target_slice/m70_04_nine_fixture_scale_decision.md`

These outputs are written only when the CLI is run against existing upstream
artifacts.

## Decision Rules

The decision is ready for post-M70 queue planning only when:

- nine fixture evidence records exist
- all fixtures are offline-load ready
- all fixtures have generated deck code artifacts
- all Unity headless smokes passed with `deck_source = deck_code`
- all fixtures have `0` blocking issues
- G Zone, Stride, Aqua Force battle-order, Bloom/token, Lock/Unlock, and
  Legion/Mate runtime remain disabled

Candidate queue output is advisory only. It must exclude completed fixture
groups, but `M70-04` must not select the next slice.

## Boundary

This milestone must not:

- select a live runtime deck
- select a tenth slice
- create a runtime fixture
- inject saved player decks
- publish UI deck lists
- enable bot playbooks
- enable G Zone runtime
- enable Stride runtime
- enable Aqua Force battle-order runtime
- enable Bloom/token runtime
- enable Lock/Unlock runtime
- enable Legion/Mate runtime
- mutate `GameState`

## Verification

Scaffold-safe verification:

```powershell
python -m unittest tests.test_nine_fixture_scale_decision
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M58-03, M62-03, M66-03, and M70-03 outputs
exist:

```powershell
python tools\deck\build_nine_fixture_scale_decision.py
```

## Done Rule

`M70-04` scaffold work is ready when the scale decision passes with in-memory
sixth, seventh, eighth, and ninth fixture evidence, the candidate queue
excludes completed fixture groups, tests cover pass and fail behavior, docs are
updated, and real artifacts remain gated until M58-03/M62-03/M66-03/M70-03
evidence files exist.
