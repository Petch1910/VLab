# Third-Slice Repaired Recipe Validation Rerun Spec

Milestone: `M45-03`

## Purpose

`M45-03` validates the repaired quantity preview from the `M45-02`
human-accepted repair artifact.

This is still an offline validation rerun. It may report that the repaired
recipe is validator-passed and runtime-ready as data, but it must not create a
runtime fixture, saved deck, UI entry, or bot playbook. `M45-04` is the runtime
fixture promotion gate.

## Inputs

- `outputs/target_slice/m45_02_third_slice_human_accepted_repair_artifact.json`
- `data/packs/vanguard_th/cards.sqlite`

## Outputs

- `outputs/target_slice/m45_03_third_slice_repaired_recipe_validation_report.json`
- `outputs/target_slice/m45_03_third_slice_repaired_recipe_validation_report.md`

## Validation Rules

Reuse the third-slice recipe validator rules against the M45-02 repaired
quantity preview:

- Card ids must exist in runtime SQLite.
- Quantity must be positive.
- Quantity must not exceed SQLite `deck_limit`.
- Main deck explicit count must equal `50`.
- Trigger count must equal `16`.
- Heal trigger count must not exceed `4`.
- All cards must match the selected clan.
- Grade profile should equal the classic advisory target.
- Manual-review card overlap must be absent after the accepted repair.
- Human acceptance must already be recorded by M45-02.

## Runtime Boundary

This milestone must not:

- record human acceptance
- mutate M45-02 accepted repair files
- mutate M44-03 recipe draft files
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python tools\deck\validate_third_slice_repaired_recipe.py
python -m unittest tests.test_third_slice_repaired_recipe_validation
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M45-03` is done when:

- one repaired recipe is validated
- validator status is `validator_passed`
- runtime-ready recipe count is `1`
- missing-card/copy-limit/slot-gap/trigger-count/manual-overlap/grade-profile
  issue counts are `0`
- no fixture/saved deck/UI/bot/GameState mutation is performed
- `ready_for_m45_04=true`
- project status docs point the active queue to `M45-04`
