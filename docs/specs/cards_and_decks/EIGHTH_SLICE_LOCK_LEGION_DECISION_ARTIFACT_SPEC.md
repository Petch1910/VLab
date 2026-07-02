# Eighth-Slice Lock / Legion Decision Artifact Spec

Milestone: `M65-04`

## Purpose

`M65-04` records explicit boundary decisions for the accepted eighth-slice
recipe from `M65-03`.

The current Windows-first recipe pipeline can validate the 50-card main deck,
but Lock/Unlock circle state, locked-card visibility, unlock timing,
Legion/Mate declaration, Mate identity checks, Legion state, and Legion attack
runtime timing are not implemented.

This milestone records that boundary only. It does not validate the repaired
deck and does not promote runtime fixtures.

## Inputs

- `outputs/target_slice/m65_03_eighth_slice_human_accepted_grade_repair_artifact.json`
- CLI/user input: `lock_option`
- CLI/user input: `legion_option`

Tests may pass an in-memory `M65-03` accepted artifact until real upstream
artifacts exist.

## Outputs

- `outputs/target_slice/m65_04_eighth_slice_lock_legion_decision_artifact.json`
- `outputs/target_slice/m65_04_eighth_slice_lock_legion_decision_artifact.md`

## Lock / Unlock Decision Options

### `main_deck_only_review_no_runtime_promotion`

Allow the accepted recipe to proceed to main-deck-only validation while keeping
Lock/Unlock runtime disabled.

Effects:

- `main_deck_only_validation_allowed=true` only if the Legion decision also
  allows main-deck validation
- `boundary_resolves_lock_deferred_for_main_deck_validation=true`
- `lock_runtime_enabled=false`
- `unlock_runtime_enabled=false`
- `runtime_promotion_allowed=false`

### `defer_until_lock_unlock_runtime_exists`

Keep the recipe advisory until a real Lock/Unlock implementation exists.

Effects:

- `main_deck_only_validation_allowed=false`
- `runtime_promotion_allowed=false`
- no repaired validation gate is opened for current Windows fixture scope

## Legion / Mate Decision Options

### `main_deck_only_review_no_runtime_promotion`

Allow the accepted recipe to proceed to main-deck-only validation while keeping
Legion/Mate runtime disabled.

Effects:

- `main_deck_only_validation_allowed=true` only if the Lock decision also
  allows main-deck validation
- `boundary_resolves_legion_deferred_for_main_deck_validation=true`
- `legion_runtime_enabled=false`
- `mate_identity_check_enabled=false`
- `runtime_promotion_allowed=false`

### `defer_until_legion_mate_runtime_exists`

Keep the recipe advisory until Legion declaration, Mate identity, Legion state,
and Legion attack timing exist.

Effects:

- `main_deck_only_validation_allowed=false`
- `runtime_promotion_allowed=false`
- no repaired validation gate is opened for current Windows fixture scope

## Runtime Boundary

This milestone must not:

- record human selection
- record human acceptance
- record grade repair acceptance
- modify M64/M65 source artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- enable Lock runtime
- enable Unlock runtime
- enable locked-card visibility or circle-state runtime
- enable Legion runtime
- enable Mate identity checks
- enable Legion deck-building validation
- mutate `GameState`

## M65-05 Validation Policy

When both Lock and Legion select `main_deck_only_review_no_runtime_promotion`,
`M65-05` may treat the deferred Lock and Legion checks as resolved for
main-deck validation scope only.

`M65-05` must still enforce:

- main deck count
- missing card checks
- copy limits
- trigger count
- heal trigger limit
- grade profile
- human selection presence
- clan / format mismatch
- no Lock/Unlock effect resolution
- no Legion/Mate effect resolution
- no runtime fixture before validation passes

## Verification

```powershell
python -m unittest tests.test_eighth_slice_lock_legion_decision_artifact
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M65-03 output exists:

```powershell
python tools\deck\build_eighth_slice_lock_legion_decision_artifact.py `
  --lock-option main_deck_only_review_no_runtime_promotion `
  --legion-option main_deck_only_review_no_runtime_promotion
```

## Done Rule

`M65-04` is done when:

- both Lock/Unlock and Legion/Mate decisions are recorded
- unsupported options are rejected
- options absent from the accepted artifact are rejected
- main-deck-only validation opens only when both decisions allow it
- all runtime flags remain disabled
- runtime/saved deck/UI/bot/GameState mutation remains disabled
- `ready_for_m65_05=true` only for the non-runtime main-deck validation path
