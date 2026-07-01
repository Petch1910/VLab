# Sixth-Slice G Zone / Stride Decision Artifact Spec

Milestone: `M57-04`

## Purpose

`M57-04` records the explicit G Zone / Stride boundary decision for the
accepted sixth-slice recipe from `M57-03`.

The current Windows-first recipe pipeline can validate the 50-card main deck,
but G Zone slots, Stride deck-building validation, G-unit visibility policy,
Stride timing, and Generation Break runtime support are not implemented.

This milestone records that boundary only. It does not validate the repaired
deck and does not promote runtime fixtures.

## Inputs

- `outputs/target_slice/m57_03_sixth_slice_human_accepted_repair_artifact.json`
- CLI/user input: `selected_option`

## Outputs

- `outputs/target_slice/m57_04_sixth_slice_g_zone_stride_decision_artifact.json`
- `outputs/target_slice/m57_04_sixth_slice_g_zone_stride_decision_artifact.md`

## Decision Options

### `main_deck_only_review_no_runtime_promotion`

Allow the accepted recipe to proceed to main-deck-only validation while keeping
G Zone / Stride runtime disabled.

Effects:

- `main_deck_only_validation_allowed=true`
- `boundary_resolves_g_zone_deferred_for_main_deck_validation=true`
- `g_zone_runtime_enabled=false`
- `stride_runtime_enabled=false`
- `runtime_promotion_allowed=false`
- next target is `M57-05`

### `defer_until_g_zone_runtime_exists`

Keep the recipe advisory until a real G Zone / Stride implementation exists.

Effects:

- `main_deck_only_validation_allowed=false`
- `runtime_promotion_allowed=false`
- no repaired validation gate is opened for current Windows fixture scope

## Runtime Boundary

This milestone must not:

- record human selection
- record human acceptance
- modify M56/M57 source artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- enable G Zone runtime
- enable Stride runtime
- allow Grade 4 / G units in the main deck
- mutate `GameState`

## M57-05 Validation Policy

When `main_deck_only_review_no_runtime_promotion` is selected, `M57-05` may
treat the deferred G Zone blocker as resolved for main-deck validation only.

`M57-05` must still enforce:

- main deck count
- missing card checks
- copy limits
- trigger count
- grade profile
- manual review overlap
- clan / format mismatch
- Grade 4 / G units excluded from the main deck

## Verification

```powershell
python -m unittest tests.test_sixth_slice_g_zone_stride_decision_artifact
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M57-03 output exists:

```powershell
python tools\deck\build_sixth_slice_g_zone_stride_decision_artifact.py `
  --selected-option main_deck_only_review_no_runtime_promotion
```
