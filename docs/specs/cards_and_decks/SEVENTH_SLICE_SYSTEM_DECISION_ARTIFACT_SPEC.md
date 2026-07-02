# Seventh-Slice G Zone / Stride / Bloom-Token Decision Artifact Spec

Milestone: `M61-04`

## Purpose

`M61-04` records explicit boundary decisions for the accepted seventh-slice
recipe from `M61-03`.

The current Windows-first recipe pipeline can validate the 50-card main deck,
but G Zone slots, Stride deck-building validation, G-unit visibility policy,
Stride timing, Generation Break runtime support, Bloom templates, same-name
tracking, token lifecycle, and duration cleanup are not implemented.

This milestone records that boundary only. It does not validate the repaired
deck and does not promote runtime fixtures.

## Inputs

- `outputs/target_slice/m61_03_seventh_slice_human_accepted_repair_artifact.json`
- CLI/user input: `g_zone_option`
- CLI/user input: `bloom_token_option`

Tests may pass an in-memory `M61-03` accepted artifact until real upstream
artifacts exist.

## Outputs

- `outputs/target_slice/m61_04_seventh_slice_system_decision_artifact.json`
- `outputs/target_slice/m61_04_seventh_slice_system_decision_artifact.md`

## G Zone / Stride Decision Options

### `main_deck_only_review_no_runtime_promotion`

Allow the accepted recipe to proceed to main-deck-only validation while keeping
G Zone / Stride runtime disabled.

Effects:

- `main_deck_only_validation_allowed=true` only if the Bloom/token decision
  also allows main-deck validation
- `boundary_resolves_g_zone_deferred_for_main_deck_validation=true`
- `g_zone_runtime_enabled=false`
- `stride_runtime_enabled=false`
- `runtime_promotion_allowed=false`

### `defer_until_g_zone_stride_runtime_exists`

Keep the recipe advisory until a real G Zone / Stride implementation exists.

Effects:

- `main_deck_only_validation_allowed=false`
- `runtime_promotion_allowed=false`
- no repaired validation gate is opened for current Windows fixture scope

## Bloom/Token Decision Options

### `manual_semantic_review_only_no_runtime_promotion`

Allow the accepted recipe to proceed to main-deck-only validation while keeping
Bloom/token runtime disabled.

Effects:

- `main_deck_only_validation_allowed=true` only if the G Zone decision also
  allows main-deck validation
- `boundary_resolves_bloom_token_deferred_for_main_deck_validation=true`
- `bloom_token_runtime_enabled=false`
- `runtime_promotion_allowed=false`

### `defer_until_bloom_token_runtime_exists`

Keep the recipe advisory until Bloom templates, same-name checks, token
lifecycle, and duration cleanup exist.

Effects:

- `main_deck_only_validation_allowed=false`
- `runtime_promotion_allowed=false`
- no repaired validation gate is opened for current Windows fixture scope

## Runtime Boundary

This milestone must not:

- record human selection
- record human acceptance
- modify M60/M61 source artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- enable G Zone runtime
- enable Stride runtime
- enable Bloom/token runtime
- allow Grade 4 / G units in the main deck
- allow tokens in the main deck
- mutate `GameState`

## M61-05 Validation Policy

When both `main_deck_only_review_no_runtime_promotion` and
`manual_semantic_review_only_no_runtime_promotion` are selected, `M61-05` may
treat the deferred G Zone and Bloom/token checks as resolved for main-deck
validation scope only.

`M61-05` must still enforce:

- main deck count
- missing card checks
- copy limits
- trigger count
- heal trigger limit
- grade profile
- manual review overlap
- clan / format mismatch
- Grade 4 / G units excluded from the main deck
- tokens excluded from the main deck

## Verification

```powershell
python -m unittest tests.test_seventh_slice_system_decision_artifact
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M61-03 output exists:

```powershell
python tools\deck\build_seventh_slice_system_decision_artifact.py `
  --g-zone-option main_deck_only_review_no_runtime_promotion `
  --bloom-token-option manual_semantic_review_only_no_runtime_promotion
```

## Done Rule

`M61-04` is done when:

- both G Zone / Stride and Bloom/token decisions are recorded
- unsupported options are rejected
- options absent from the accepted artifact are rejected
- main-deck-only validation opens only when both decisions allow it
- all runtime flags remain disabled
- runtime/saved deck/UI/bot/GameState mutation remains disabled
- `ready_for_m61_05=true` only for the non-runtime main-deck validation path
