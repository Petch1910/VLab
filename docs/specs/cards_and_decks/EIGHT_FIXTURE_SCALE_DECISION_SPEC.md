# Eight-Fixture Scale Decision Spec

Milestone: `M66-04`

## Purpose

`M66-04` reviews the first eight offline runtime/test fixtures before allowing
any further slice work.

It consumes Nova Grappler, Oracle Think Tank, Bermuda Triangle, Royal Paladin
G-series, Gold Paladin, Shadow Paladin, Neo Nectar, and Kagero headless smoke
evidence plus the existing archetype priority ranking. It may open a ninth
offline pipeline, but it must not create runtime fixtures, saved decks, UI
entries, bot/playbook data, G Zone runtime, Stride runtime, Bloom/token
runtime, Lock/Unlock runtime, or Legion/Mate runtime by itself.

The implementation is scaffold-safe: tests may use in-memory sixth, seventh,
and eighth fixture smoke reports. Real CLI artifacts remain gated until the
M58-03, M62-03, and M66-03 real smoke reports plus Unity evidence exist.

## Inputs

- `outputs/target_slice/m39_03_headless_fixture_load_smoke.json`
- `outputs/target_slice/m42_03_second_fixture_load_smoke.json`
- `outputs/target_slice/m46_03_third_fixture_load_smoke.json`
- `outputs/target_slice/m50_03_fourth_fixture_load_smoke.json`
- `outputs/target_slice/m54_03_fifth_fixture_load_smoke.json`
- `outputs/target_slice/m58_03_sixth_fixture_load_smoke.json`
- `outputs/target_slice/m62_03_seventh_fixture_load_smoke.json`
- `outputs/target_slice/m66_03_eighth_fixture_load_smoke.json`
- `outputs/archetype_priority/archetype_priority_ranking.json`

## Outputs

- `outputs/target_slice/m66_04_eight_fixture_scale_decision.json`
- `outputs/target_slice/m66_04_eight_fixture_scale_decision.md`

These outputs are written only when the CLI is run against existing upstream
artifacts.

## Decision Rules

The decision allows the next offline pipeline only when:

- eight fixture evidence records exist
- all fixtures are offline-load ready
- all fixtures have generated deck code artifacts
- all Unity headless smokes passed with `deck_source = deck_code`
- all fixtures have `0` blocking issues
- at least one feasible candidate remains in the archetype priority ranking
  after excluding completed fixture groups
- G Zone, Stride, Bloom/token, Lock/Unlock, and Legion/Mate runtime remain
  disabled

## Boundary

This milestone must not:

- select a live runtime deck
- create a runtime fixture
- inject saved player decks
- publish UI deck lists
- enable bot playbooks
- enable G Zone runtime
- enable Stride runtime
- enable Bloom/token runtime
- enable Lock/Unlock runtime
- enable Legion/Mate runtime
- mutate `GameState`

## Verification

Scaffold-safe verification:

```powershell
python -m unittest tests.test_eight_fixture_scale_decision
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M58-03, M62-03, and M66-03 outputs exist:

```powershell
python tools\deck\build_eight_fixture_scale_decision.py
```

## Done Rule

`M66-04` scaffold work is ready when the scale decision passes with in-memory
sixth, seventh, and eighth fixture evidence, the candidate queue excludes
completed fixture groups, tests cover pass and fail behavior, docs are updated,
and real artifacts remain gated until M58-03/M62-03/M66-03 evidence files
exist.
