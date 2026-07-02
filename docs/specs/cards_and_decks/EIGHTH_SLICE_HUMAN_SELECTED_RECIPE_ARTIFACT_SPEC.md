# Eighth-Slice Human-Selected Recipe Artifact Spec

Milestone: `M65-02`

## Purpose

`M65-02` records exactly one explicitly selected eighth-slice review item from
the `M65-01` human repair review packet.

This artifact records recipe selection only. It does not record grade repair
acceptance, does not record a Lock decision, does not record a Legion decision,
and does not promote runtime decks.

## Inputs

- `outputs/target_slice/m65_01_eighth_slice_human_repair_review_packet.json`
- CLI/user input: `review_item_id`
- CLI/user input: `selection_text`

Tests may pass an in-memory `M65-01` review packet until real upstream
artifacts exist.

## Outputs

- `outputs/target_slice/m65_02_eighth_slice_human_selected_recipe_artifact.json`
- `outputs/target_slice/m65_02_eighth_slice_human_selected_recipe_artifact.md`

## Selection Rules

- The selected `review_item_id` must be non-empty and must exist in `M65-01`.
- The selected id must match exactly one review item.
- `selection_text` must be non-empty.
- The artifact must carry pair context, human-selection context, grade-profile
  repair context, deferred Lock context, and deferred Legion context from the
  selected review item.
- `ready_for_m65_03=true` only when the `M65-01` packet is ready for selection,
  the selected review item is ready for human repair review, has human
  selection context, has its candidate pair present, has complete grade repair
  or explicitly needs no grade repair, and has no structural blockers.
- Deferred Lock and Legion work do not block `M65-03`; they must be recorded
  later by `M65-04` before fixture promotion can be considered.

## Runtime Boundary

This milestone must not:

- choose a recipe automatically
- record grade repair acceptance
- record a Lock decision
- record a Legion decision
- modify M64/M65 recipe artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable Lock runtime
- enable Legion runtime
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_eighth_slice_human_selected_recipe_artifact
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only with explicit selection input after the real
`M65-01` packet exists:

```powershell
python tools\deck\build_eighth_slice_human_selected_recipe_artifact.py `
  --review-item-id <M65-01 review_item_id> `
  --selection-text "<explicit user selection>"
```

## Done Rule

`M65-02` is done when:

- exactly one review item is recorded as selected
- the artifact carries the selected pair/human-selection/grade/Lock/Legion
  context
- selection is traceable to explicit `review_item_id` and `selection_text`
- no acceptance or system decision is recorded
- runtime/saved deck/UI/bot/GameState mutation remains disabled
- `ready_for_m65_03=true` for valid selected items
