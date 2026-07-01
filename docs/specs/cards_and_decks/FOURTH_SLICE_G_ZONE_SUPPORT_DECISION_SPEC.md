# Fourth-Slice G Zone Support Decision Spec

Milestone: `M49-02`

## Purpose

`M49-02` records the G Zone / Stride boundary decision for the fourth target
slice after `M49-01` exports repair review data.

The current project direction is Windows-first main-deck fixture validation.
G Zone slots, Stride timing, G-unit visibility, and Generation Break runtime
support are not implemented. Therefore the default decision is:

`main_deck_only_for_current_windows_fixture`

This means later milestones may validate repaired main-deck recipes, but must
keep G Zone and Stride runtime disabled.

## Inputs

- `outputs/target_slice/m49_01_fourth_slice_human_repair_review_packet.json`
- `outputs/target_slice/m48_closeout_fourth_slice_runtime_readiness.json`

## Outputs

- `outputs/target_slice/m49_02_fourth_slice_g_zone_support_decision.json`
- `outputs/target_slice/m49_02_fourth_slice_g_zone_support_decision.md`

## Decision Options

### `main_deck_only_for_current_windows_fixture`

Use repaired fourth-slice candidates only as main-deck validation candidates for
the current Windows fixture scope.

Effects:

- `main_deck_only_validation_allowed=true`
- `g_zone_runtime_enabled=false`
- `stride_runtime_enabled=false`
- `grade4_main_deck_allowed=false`
- `g_units_allowed_in_main_deck=false`
- `runtime_promotion_allowed=false`
- next target is `M49-03`

### `defer_recipe_until_g_zone_support`

Keep fourth-slice recipes advisory until G Zone / Stride support exists.

Effects:

- `main_deck_only_validation_allowed=false`
- `runtime_promotion_allowed=false`
- no human acceptance gate is opened for current Windows fixture scope

### `open_g_zone_implementation_queue`

Open a separate G Zone implementation queue.

Required future work:

- G Zone deck slot model
- Stride deck-building validation
- G-unit visibility and public-event masking policy
- Stride timing and Generation Break runtime support
- replay and validation coverage for G Zone events

## Runtime Boundary

This milestone may record the boundary decision only.

It must not:

- record human acceptance
- modify M48-03 recipe draft files
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- enable G Zone runtime
- enable Stride runtime
- allow Grade 4 / G units in the main deck
- mutate `GameState`

## M49-04 Validation Policy

When the default decision is selected, `M49-04` may treat
`g_zone_support_deferred` as resolved only for main-deck validation scope.

`M49-04` must still enforce:

- main deck count
- missing card checks
- copy limits
- trigger count
- grade profile
- manual review overlap
- clan / format mismatch

## Done Rule

`M49-02` is done when:

- all `25` review items receive a G Zone boundary decision
- default decision is recorded as `main_deck_only_for_current_windows_fixture`
- runtime promotion remains disabled
- G Zone and Stride runtime remain disabled
- the artifact points to `M49-03`
- targeted and full Python tests pass

## Verification

```powershell
python tools\deck\build_fourth_slice_g_zone_support_decision.py
python -m unittest tests.test_fourth_slice_g_zone_support_decision
python -m unittest discover -s tests -p "test_*.py"
```
