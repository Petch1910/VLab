# Seventh-Slice Human-Selected Recipe Artifact Spec

Milestone: `M61-02`

## Purpose

`M61-02` records exactly one explicitly selected seventh-slice review item from
the `M61-01` human repair review packet.

This artifact records selection only. It does not record acceptance, does not
record a G Zone / Stride decision, does not record a Bloom/token decision, and
does not promote runtime decks.

## Inputs

- `outputs/target_slice/m61_01_seventh_slice_human_repair_review_packet.json`
- CLI/user input: `review_item_id`
- CLI/user input: `selection_text`

Tests may pass an in-memory `M61-01` review packet until real upstream artifacts
exist.

## Outputs

- `outputs/target_slice/m61_02_seventh_slice_human_selected_recipe_artifact.json`
- `outputs/target_slice/m61_02_seventh_slice_human_selected_recipe_artifact.md`

## Selection Rules

- The selected `review_item_id` must be non-empty and must exist in `M61-01`.
- The selected id must match exactly one review item.
- `selection_text` must be non-empty.
- The artifact must carry pair context, manual-repair context, grade-profile
  repair context, deferred G Zone / Stride context, and deferred Bloom/token
  context from the selected review item.
- `ready_for_m61_03=true` only when the selected review item is ready for
  human repair review, has complete manual repair, has complete grade repair or
  explicitly needs no grade repair, and has no structural blockers.
- Deferred G Zone / Stride and Bloom/token work do not block `M61-03`; they
  must be recorded later by `M61-04` before fixture promotion can be considered.

## Runtime Boundary

This milestone must not:

- choose a recipe automatically
- record human acceptance
- record a G Zone / Stride decision
- record a Bloom/token decision
- modify M60/M61 recipe artifacts
- create runtime fixtures
- inject saved decks
- publish UI deck lists
- enable G Zone runtime
- enable Stride runtime
- enable Bloom/token runtime
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_seventh_slice_human_selected_recipe_artifact
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only with explicit selection input after the real
`M61-01` packet exists:

```powershell
python tools\deck\build_seventh_slice_human_selected_recipe_artifact.py `
  --review-item-id <M61-01 review_item_id> `
  --selection-text "<explicit user selection>"
```

## Done Rule

`M61-02` is done when:

- exactly one review item is recorded as selected
- the artifact carries the selected pair/manual/grade/G Zone/Bloom context
- selection is traceable to explicit `review_item_id` and `selection_text`
- no acceptance or system decision is recorded
- runtime/saved deck/UI/bot/GameState mutation remains disabled
- `ready_for_m61_03=true` for valid selected items
