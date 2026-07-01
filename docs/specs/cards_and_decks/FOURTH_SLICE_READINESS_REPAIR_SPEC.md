# Fourth-Slice Readiness Repair Spec

Milestone: `M47-repair`

## Purpose

`M47-repair` analyzes the blocker from `M47-02` without mutating card data or
relaxing fixture policy.

The known blocker is that `รอยัล พาลาดิน` in the selected `g_series_first`
scope has no `Heal` trigger. The repair report must check whether same-group
Heal triggers exist elsewhere in the runtime SQLite pool before recommending
reselection.

## Inputs

- `outputs/target_slice/m47_02_fourth_slice_fixture_readiness.json`
- `outputs/target_slice/m46_04_three_fixture_scale_decision.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m47_repair_fourth_slice_readiness_blockers.json`
- `outputs/target_slice/m47_repair_fourth_slice_readiness_blockers.md`

## Decision Rules

The repair report must:

- confirm whether the selected group has any same-group `Heal` trigger anywhere
  in runtime SQLite
- reject silent relaxation of trigger-profile requirements
- avoid card data mutation
- recommend same-group source expansion review when the selected group has
  Heal triggers outside the selected scope
- recommend next-candidate reselection only when the selected group cannot be
  repaired from current source data

## Boundary

This milestone must not:

- edit card data
- create recipe drafts
- create runtime fixtures
- inject saved player decks
- publish UI deck lists
- enable bot playbooks
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_fourth_slice_readiness_repair.py
python -m unittest tests.test_fourth_slice_readiness_repair
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M47-repair` is done when the report identifies whether the blocker can be
repaired from existing source data, tests cover the chosen path, docs are
updated, and the next target is `M47-repair-expand-scope` when same-group
source expansion is safer.
