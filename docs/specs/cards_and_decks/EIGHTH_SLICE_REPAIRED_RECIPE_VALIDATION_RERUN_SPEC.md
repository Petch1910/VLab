# Eighth-Slice Repaired Recipe Validation Rerun Spec

Milestone: `M65-05`

## Purpose

`M65-05` validates the repaired quantity preview from `M65-03` after the
explicit Lock / Legion boundary from `M65-04`.

This is still an offline validation rerun. It may report that the repaired
main-deck recipe is validator-passed and consistency-passed, but it must not
create a runtime fixture, saved deck, UI entry, or bot playbook. `M65-06` is
the runtime fixture promotion gate.

## Inputs

- `outputs/target_slice/m65_03_eighth_slice_human_accepted_grade_repair_artifact.json`
- `outputs/target_slice/m65_04_eighth_slice_lock_legion_decision_artifact.json`
- `data/packs/vanguard_th/cards.sqlite`

Tests may pass in-memory `M65-03` and `M65-04` artifacts until real upstream
artifacts exist.

## Outputs

- `outputs/target_slice/m65_05_eighth_slice_repaired_recipe_validation_report.json`
- `outputs/target_slice/m65_05_eighth_slice_repaired_recipe_validation_report.md`

## Validation Rules

Reuse the eighth-slice recipe validator and combo consistency rules against
the `M65-03` repaired quantity preview:

- Card ids must exist in runtime SQLite.
- Quantity must be positive.
- Quantity must not exceed SQLite `deck_limit`.
- Main deck explicit count must equal `50`.
- Trigger count must equal `16`.
- Heal trigger count must not exceed `4`.
- Grade 4 cards must not appear in the main deck.
- All cards must match the selected clan.
- Grade profile should equal the classic advisory target.
- Manual-review card overlap must be absent after the accepted repair.
- Human selection and human acceptance must already be recorded.
- Grade repair acceptance must already be recorded.
- The `M65-04` boundary must allow both Lock/Unlock and Legion/Mate
  main-deck-only validation before `M65-06` can open.

## Boundary Policy

`M65-05` validates only the main deck. The `M65-04` boundary may suppress
`lock_runtime_support_deferred` and `legion_runtime_support_deferred` for this
validation pass, but it must not enable:

- Lock runtime
- Unlock runtime
- locked-card visibility or circle-state runtime
- Legion runtime
- Mate identity checks
- Legion deck-building validation
- Legion attack timing

## Runtime Boundary

This milestone must not:

- record human acceptance
- record grade repair acceptance
- record a new Lock decision
- record a new Legion decision
- mutate M65-03 or M65-04 artifact files
- mutate M64 recipe draft files
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_eighth_slice_repaired_recipe_validation
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M65-03 and M65-04 outputs exist:

```powershell
python tools\deck\validate_eighth_slice_repaired_recipe.py
```

## Done Rule

`M65-05` is done when:

- repaired preview validates through the eighth-slice validator
- combo/recipe consistency passes for the selected pair
- deferred Lock and Legion review codes are suppressed only when the M65-04
  boundary allows main-deck validation
- deferred boundary decisions block `ready_for_m65_06`
- no runtime fixture, saved deck, UI deck list, bot playbook, or GameState
  mutation occurs
- `ready_for_m65_06=true` only when validation, consistency, acceptance,
  grade repair acceptance, and both M65-04 boundary decisions all pass
